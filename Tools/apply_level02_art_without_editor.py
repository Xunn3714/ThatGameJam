"""Apply the requested Level 1 door and Level 2 art edits to saved Unity scenes.

This edits serialized scene objects so Unity's desktop window is not required.
It is idempotent and preserves unrelated scene objects and user edits.
"""

from pathlib import Path
import re
import struct


PROJECT = Path(__file__).resolve().parents[1]
SCENES = PROJECT / "Assets/_Game/Scenes"
ART = PROJECT / "Assets/_Game/Resources/Level02"
MARKER = PROJECT / "Assets/_Game/Scripts/ManualArtPlacement.cs.meta"
HEADER = re.compile(r"--- !u!(\d+) &(\d+)")


def asset_guid(path):
    return re.search(r"(?m)^guid: ([0-9a-f]{32})$", path.read_text(encoding="utf-8")).group(1)


def sprite_size(path):
    data = path.read_bytes()[:24]
    assert data[:8] == b"\x89PNG\r\n\x1a\n", path
    width, height = struct.unpack(">II", data[16:24])
    return width / 200, height / 200


def field(doc, name, value):
    pattern = rf"(?m)^(  {re.escape(name)}:).*$"
    result, count = re.subn(pattern, lambda m: m.group(1) + " " + value, doc, count=1)
    assert count == 1, name
    return result


class Scene:
    def __init__(self, path):
        self.path = path
        raw = path.read_text(encoding="utf-8-sig")
        parts = re.split(r"(?=^--- !u!\d+ &\d+)", raw, flags=re.M)
        self.preamble = parts[0]
        self.docs = {}
        self.order = []
        for doc in parts[1:]:
            match = HEADER.match(doc)
            assert match, doc[:50]
            key = int(match.group(2))
            assert key not in self.docs
            self.docs[key] = doc
            self.order.append(key)
        self.next_id = max(key for key in self.docs if key < 2_147_000_000) + 1000

    def fresh(self):
        self.next_id += 1
        assert self.next_id not in self.docs
        return self.next_id

    def go(self, name):
        found = [key for key, doc in self.docs.items()
                 if doc.startswith("--- !u!1 ") and f"  m_Name: {name}\n" in doc]
        assert len(found) == 1, (name, found)
        return found[0]

    def components(self, go):
        return [(key, doc) for key, doc in self.docs.items()
                if f"  m_GameObject: {{fileID: {go}}}\n" in doc]

    def component(self, go, kind):
        found = [(key, doc) for key, doc in self.components(go)
                 if doc.startswith(f"--- !u!{kind} ")]
        assert len(found) == 1, (go, kind, len(found))
        return found[0][0]

    def position(self, go, x, y):
        key = self.component(go, 4)
        self.docs[key] = field(self.docs[key], "m_LocalPosition", f"{{x: {x}, y: {y}, z: 0}}")

    def scale(self, go, x, y=None, z=1):
        key = self.component(go, 4)
        self.docs[key] = field(self.docs[key], "m_LocalScale", f"{{x: {x}, y: {x if y is None else y}, z: {z}}}")

    def renderer(self, go, sprite=None, order=None, color=None):
        key = self.component(go, 212)
        doc = self.docs[key]
        if sprite:
            path = ART / (sprite + ".png")
            doc = field(doc, "m_Sprite", f"{{fileID: 21300000, guid: {asset_guid(Path(str(path) + '.meta'))}, type: 3}}")
            w, h = sprite_size(path)
            doc = field(doc, "m_Size", f"{{x: {w:g}, y: {h:g}}}")
        if order is not None:
            doc = field(doc, "m_SortingOrder", str(order))
        if color:
            doc = field(doc, "m_Color", color)
        self.docs[key] = doc

    def rename(self, old, new):
        key = self.go(old)
        self.docs[key] = field(self.docs[key], "m_Name", new)
        return key

    def remove_object(self, name):
        go = self.go(name)
        transform = self.component(go, 4)
        for key, _ in self.components(go):
            del self.docs[key]
        del self.docs[go]
        # The old first-level placeholders are root objects.
        roots = next(key for key, doc in self.docs.items() if doc.startswith("--- !u!1660057539 "))
        line = f"  - {{fileID: {transform}}}\n"
        assert line in self.docs[roots]
        self.docs[roots] = self.docs[roots].replace(line, "", 1)

    def mark_manual_placement(self, go, script_guid):
        if any("ManualArtPlacement" in doc for _, doc in self.components(go)):
            return
        transform = self.docs[self.component(go, 4)]
        pos = re.search(r"(?m)^  m_LocalPosition: (\{[^\n]+\})$", transform).group(1)
        scale = re.search(r"(?m)^  m_LocalScale: (\{[^\n]+\})$", transform).group(1)
        rotation = re.search(r"(?m)^  m_LocalRotation: (\{[^\n]+\})$", transform).group(1)
        marker = self.fresh()
        self.docs[go] = self.docs[go].replace("  m_Layer:", f"  - component: {{fileID: {marker}}}\n  m_Layer:", 1)
        self.docs[marker] = f"""--- !u!114 &{marker}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name:
  m_EditorClassIdentifier: Assembly-CSharp::SeedGame.ManualArtPlacement
  generatedPosition: {pos}
  generatedScale: {scale}
  generatedRotation: {rotation}
"""
        self.order.append(marker)

    def save(self):
        normal = [key for key in self.order if key in self.docs and not self.docs[key].startswith("--- !u!1660057539 ")]
        roots = [key for key in self.order if key in self.docs and self.docs[key].startswith("--- !u!1660057539 ")]
        assert len(roots) == 1
        result = self.preamble + "".join(self.docs[key] for key in normal + roots)
        result = re.sub(r"(?m)^([ \t]*[^\n:]+:)[ \t]+$", r"\1", result)
        self.path.write_text(result, encoding="utf-8", newline="\n")


first = Scene(SCENES / "Level01_Alpha.unity")
if "Ruins entrance rear half" not in "".join(first.docs.values()):
    rear = first.rename("Ruins left jamb", "Ruins entrance rear half")
    front = first.rename("Ruins right jamb", "Ruins entrance front half")
    first.position(rear, 46.4, 1.3)
    first.position(front, 47.2, 1.35)
    first.scale(rear, 3 / sprite_size(ART / "石门-后.png")[0])
    first.scale(front, 1.4 / sprite_size(ART / "石门-前.png")[0])
    first.renderer(rear, "石门-后", 2, "{r: 1, g: 1, b: 1, a: 1}")
    first.renderer(front, "石门-前", 9, "{r: 1, g: 1, b: 1, a: 1}")
    first.remove_object("Ruins lintel")
    first.remove_object("Ruins doorway shadow")
    first.save()


second = Scene(SCENES / "Level02_Alpha.unity")
if any("  m_Name: Water corridor vines\n" in doc for doc in second.docs.values()):
    print("Requested scene art is already saved; no changes needed.")
    raise SystemExit(0)
guids = {asset_guid(meta) for meta in ART.glob("*.png.meta")}
native_count = 0
marker_guid = asset_guid(MARKER)
mark_later = []
for key, doc in list(second.docs.items()):
    if not doc.startswith("--- !u!212 "):
        continue
    match = re.search(r"m_Sprite: \{fileID: \d+, guid: ([0-9a-f]{32}), type: 3\}", doc)
    if not match or match.group(1) not in guids:
        continue
    go = int(re.search(r"m_GameObject: \{fileID: (\d+)\}", doc).group(1))
    name = re.search(r"(?m)^  m_Name: (.*)$", second.docs[go]).group(1)
    if not name.startswith("Gate pushing vine "):
        second.scale(go, 1)
    if name != "Growing painted leaf":
        mark_later.append(go)
    native_count += 1

second.position(second.go("Moss on water corridor"), 11.4, 1.7)
vine = second.rename("Entrance arch rear", "Water corridor vines")
second.position(vine, 11.4, 3.3)
second.scale(vine, 1)
second.renderer(vine, "藤蔓", 4)
second.scale(second.go("Entrance arch front"), 1)

# Make the right door jamb a solid stop, while leaving the player's spawn to its right.
front = second.go("Entrance arch front")
if not any(doc.startswith("--- !u!61 ") for _, doc in second.components(front)):
    source = next(doc for doc in second.docs.values() if doc.startswith("--- !u!61 "))
    source_id = int(HEADER.match(source).group(2))
    collider_id = second.fresh()
    collider = source.replace(f"&{source_id}\n", f"&{collider_id}\n", 1)
    collider = field(collider, "m_GameObject", f"{{fileID: {front}}}")
    collider = field(collider, "m_Size", "{x: 0.65, y: 3.7}")
    collider = field(collider, "m_Offset", "{x: 0, y: 0}")
    collider = field(collider, "m_IsTrigger", "0")
    second.docs[front] = second.docs[front].replace("  m_Layer:", f"  - component: {{fileID: {collider_id}}}\n  m_Layer:", 1)
    second.docs[collider_id] = collider
    second.order.append(collider_id)

unlit = "{r: 0.2, g: 0.34, b: 0.37, a: 1}"
for index in range(1, 5):
    second.renderer(second.go(f"Algae glow {index}"), color=unlit)
second.renderer(second.go("Water plants at far bank"), color=unlit)
algae = second.go("Blue algae resonance")
algae_script = next(key for key, doc in second.components(algae)
                    if "Assembly-CSharp::SeedGame.Level02Algae" in doc)
doc = second.docs[algae_script]
bank_renderer = second.component(second.go("Water plants at far bank"), 212)
if f"  - {{fileID: {bank_renderer}}}\n" not in doc:
    doc = doc.replace("--- !u!4", f"  - {{fileID: {bank_renderer}}}\n--- !u!4", 1) if "--- !u!4" in doc else doc.rstrip("\n") + f"\n  - {{fileID: {bank_renderer}}}\n"
second.docs[algae_script] = doc
for go in mark_later:
    second.mark_manual_placement(go, marker_guid)
second.save()
print(f"Saved first-level doorway and {native_count} second-level sprite scales; scene files are ready to open.")
