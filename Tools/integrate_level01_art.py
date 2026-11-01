"""Bake Level01 art and its matching gameplay geometry into the saved Unity scene."""

from pathlib import Path
import re
import struct


PROJECT = Path(__file__).resolve().parents[1]
ART = PROJECT / "Assets/_Game/Resources/Level01"
SCENE = PROJECT / "Assets/_Game/Scenes/Level01_Alpha.unity"
PARALLAX_GUID = "3cfcb4b9829f4c1a852c715804c26af1"
WHITE = "7a00bfbd1e9b99b46bb8e6e789f3a8a4"


def png_size(name):
    data = (ART / name).read_bytes()[:24]
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(name)
    return struct.unpack(">II", data[16:24])


def guid(name):
    return re.search(r"(?m)^guid: ([0-9a-f]{32})$", (ART / (name + ".meta")).read_text(encoding="utf-8")).group(1)


def field(doc, name, value):
    pattern = rf"(?m)^(  {re.escape(name)}:).*$"
    if re.search(pattern, doc):
        return re.sub(pattern, lambda m: m.group(1) + " " + value, doc, count=1)
    return doc.rstrip("\n") + f"\n  {name}: {value}\n"


def append_array(doc, name, sprites):
    block = f"  {name}:\n" + "".join(
        f"  - {{fileID: 21300000, guid: {guid(path)}, type: 3}}\n" for path in sprites
    )
    return doc.rstrip("\n") + "\n" + block


raw = SCENE.read_text(encoding="utf-8-sig")
if "Level01 Art - Sky" in raw:
    print("Level01 artwork is already baked into the scene.")
    raise SystemExit(0)

parts = re.split(r"(?=^--- !u!\d+ &\d+)", raw, flags=re.M)
preamble = parts[0]
docs = {}
order = []
for doc in parts[1:]:
    match = re.match(r"--- !u!(\d+) &(\d+)", doc)
    if not match:
        continue
    fid = int(match.group(2))
    docs[fid] = doc
    order.append(fid)


def klass(doc):
    return int(re.match(r"--- !u!(\d+)", doc).group(1))


def go_name(doc):
    match = re.search(r"(?m)^  m_Name: (.*)$", doc)
    return match.group(1) if match else ""


def go_ids(name):
    return [fid for fid, doc in docs.items() if klass(doc) == 1 and go_name(doc) == name]


def component(go, class_id):
    target = f"m_GameObject: {{fileID: {go}}}"
    for fid, doc in docs.items():
        if klass(doc) == class_id and target in doc:
            return fid
    raise KeyError((go, class_id))


def position(go):
    doc = docs[component(go, 4)]
    m = re.search(r"m_LocalPosition: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}", doc)
    return float(m.group(1)), float(m.group(2))


group = go_ids("First level alpha - editable objects")[0]
group_transform = component(group, 4)
camera_transform = component(go_ids("Main Camera")[0], 4)
go_template_id = go_ids("Middle pale garden")[0]
go_template = docs[go_template_id]
transform_template_id = component(go_template_id, 4)
renderer_template_id = component(go_template_id, 212)
transform_template = docs[transform_template_id]
renderer_template = docs[renderer_template_id]
next_id = max(fid for fid in docs if fid < 2_147_000_000) + 1000


def fresh():
    global next_id
    next_id += 1
    return next_id


def child(parent_transform, child_transform):
    doc = docs[parent_transform]
    if "  m_Children: []" in doc:
        doc = doc.replace("  m_Children: []", f"  m_Children:\n  - {{fileID: {child_transform}}}", 1)
    else:
        doc = doc.replace("  m_Father:", f"  - {{fileID: {child_transform}}}\n  m_Father:", 1)
    docs[parent_transform] = doc


def add_object(name, parent, x, y, sx, sy, sprite_path=None, sorting=0, parallax=None):
    go, tr, comp = fresh(), fresh(), fresh()
    game = go_template.replace(f"&{go_template_id}", f"&{go}", 1)
    game = game.replace(f"fileID: {transform_template_id}", f"fileID: {tr}")
    game = game.replace(f"fileID: {renderer_template_id}", f"fileID: {comp}")
    game = field(game, "m_Name", name)
    docs[go] = game

    trans = transform_template.replace(f"&{transform_template_id}", f"&{tr}", 1)
    trans = field(trans, "m_GameObject", f"{{fileID: {go}}}")
    trans = field(trans, "m_LocalPosition", f"{{x: {x:.6g}, y: {y:.6g}, z: 0}}")
    trans = field(trans, "m_LocalScale", f"{{x: {sx:.8g}, y: {sy:.8g}, z: 1}}")
    trans = field(trans, "m_Father", f"{{fileID: {parent}}}")
    docs[tr] = trans

    if sprite_path is not None:
        render = renderer_template.replace(f"&{renderer_template_id}", f"&{comp}", 1)
        render = field(render, "m_GameObject", f"{{fileID: {go}}}")
        render = field(render, "m_SortingOrder", str(sorting))
        render = field(render, "m_Sprite", f"{{fileID: 21300000, guid: {guid(sprite_path)}, type: 3}}")
        render = field(render, "m_Color", "{r: 1, g: 1, b: 1, a: 1}")
        docs[comp] = render
    else:
        game = game.replace("  m_Layer: 0", "  m_Layer: 0")
        mono = f"""--- !u!114 &{comp}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {PARALLAX_GUID}, type: 3}}
  m_Name:
  m_EditorClassIdentifier: Assembly-CSharp::SeedGame.ParallaxLayer
  cameraTarget: {{fileID: {camera_transform}}}
  horizontalScreenSpeed: {parallax[0]}
  verticalScreenSpeed: {parallax[1]}
"""
        docs[comp] = mono

    order.extend((go, tr, comp))
    child(parent, tr)
    return go, tr


def layer(name, x_speed, y_speed):
    return add_object(name, group_transform, 0, 0, 1, 1, parallax=(x_speed, y_speed))[1]


def art(name, parent, path, x, y, width, sorting, height=None):
    px, py = png_size(path)
    if height is None:
        height = width * py / px
    return add_object(name, parent, x, y, width / (px / 100), height / (py / 100), path, sorting)


def remove_object(go):
    tr = component(go, 4)
    descendants = []
    for other, doc in list(docs.items()):
        if klass(doc) == 4 and f"m_Father: {{fileID: {tr}}}" in doc:
            m = re.search(r"m_GameObject: \{fileID: (\d+)\}", doc)
            descendants.append(int(m.group(1)))
    for descendant in descendants:
        remove_object(descendant)
    for fid, doc in list(docs.items()):
        if klass(doc) == 4:
            line = f"  - {{fileID: {tr}}}\n"
            if line in doc:
                doc = doc.replace(line, "")
                doc = doc.replace("  m_Children:\n  m_Father:", "  m_Children: []\n  m_Father:")
                docs[fid] = doc
    for fid, doc in list(docs.items()):
        if fid == go or f"m_GameObject: {{fileID: {go}}}" in doc:
            docs.pop(fid)


for name in [
    "Distant violet hills", "Middle pale garden", "Distant mother tree",
    "Starting earth top rim", "After gap earth top rim", "Grown platforms and leaves",
]:
    for go in go_ids(name):
        remove_object(go)
for go in list(docs):
    if go in docs and klass(docs[go]) == 1 and go_name(docs[go]).startswith("Simple midground tree "):
        remove_object(go)
for go in go_ids("Vine top highlight"):
    remove_object(go)


def set_sprite(go, path, enabled=True):
    rid = component(go, 212)
    doc = docs[rid]
    doc = field(doc, "m_Sprite", f"{{fileID: 21300000, guid: {guid(path)}, type: 3}}")
    doc = field(doc, "m_Color", "{r: 1, g: 1, b: 1, a: 1}")
    doc = field(doc, "m_Enabled", "1" if enabled else "0")
    docs[rid] = doc


for name, width in [("Starting earth", 25), ("After gap earth", 17)]:
    go = go_ids(name)[0]
    set_sprite(go, "地板.png")
    tid = component(go, 4)
    docs[tid] = field(docs[tid], "m_LocalScale", "{x: 1, y: 1, z: 1}")
    docs[tid] = field(docs[tid], "m_LocalPosition", f"{{x: {12.5 if width == 25 else 38.5}, y: -0.8, z: 0}}")
    rid = component(go, 212)
    docs[rid] = field(field(docs[rid], "m_DrawMode", "1"), "m_Size", f"{{x: {width}, y: 2.4}}")
    cid = component(go, 61)
    docs[cid] = field(docs[cid], "m_Offset", "{x: 0, y: -0.4}")
    docs[cid] = field(docs[cid], "m_Size", f"{{x: {width}, y: 2.4}}")

for name in ("Jump tutorial stump", "Practice wall"):
    go = go_ids(name)[0]
    rid = component(go, 212)
    docs[go] = docs[go].replace(f"  - component: {{fileID: {rid}}}\n", "")
    docs.pop(rid)

stump = go_ids("Jump tutorial stump")[0]
stump_tr = component(stump, 4)
docs[stump_tr] = field(field(docs[stump_tr], "m_LocalPosition", "{x: 8, y: 0.4, z: 0}"),
                       "m_LocalScale", "{x: 1.15, y: 0.8, z: 1}")

wall = go_ids("Practice wall")[0]
wall_tr = component(wall, 4)
docs[wall_tr] = field(field(docs[wall_tr], "m_LocalPosition", "{x: 20.65, y: 0, z: 0}"),
                      "m_LocalScale", "{x: 1, y: 1, z: 1}")
wall_collider = component(wall, 61)
polygon = docs[wall_collider].replace(f"--- !u!61 &{wall_collider}\nBoxCollider2D:",
                                      f"--- !u!60 &{wall_collider}\nPolygonCollider2D:", 1)
polygon = re.sub(r"(?m)^  m_Size: .*\n  m_EdgeRadius: .*\n",
                 "  m_Points:\n"
                 "    m_Paths:\n"
                 "    - - {x: -1.1, y: 0}\n"
                 "      - {x: -1.1, y: 1.5}\n"
                 "      - {x: -0.8, y: 1.7}\n"
                 "      - {x: -0.62, y: 1.65}\n"
                 "      - {x: 0.75, y: 0.36}\n"
                 "      - {x: 1.1, y: 0.1}\n"
                 "      - {x: 1.1, y: 0}\n", polygon, count=1)
docs[wall_collider] = polygon

for name, asset, width, height, x, y in [
    ("Stump artwork", "跳跃树桩.png", 1.55, .9, 8, .32),
    ("Wall artwork", "墙.png", 2.2, 1.8, 20.65, .8),
]:
    art(name, group_transform, asset, x, y, width, 1, height)

buds = sorted(go_ids("Curled sleeping bud"), key=lambda go: position(go)[0])
a_scale = 1.6 * 1.12 / (png_size("植物a/a3.png")[0] / 100)
for go, path, scale in zip(buds, ("植物a/a1.png", "植物d/d1.png", "植物f/f1.png"), (a_scale, .42, .34)):
    set_sprite(go, path)
    tid = component(go, 4)
    docs[tid] = field(docs[tid], "m_LocalScale", f"{{x: {scale}, y: {scale}, z: 1}}")
for go, x, y in zip(buds, (15.1, 24.5, 30.6), (.20, .1069, .1342)):
    tid = component(go, 4)
    docs[tid] = field(docs[tid], "m_LocalPosition", f"{{x: {x}, y: {y}, z: 0}}")
for go, name in zip(buds, ("Sleeping plant A", "Near-bank vine root", "Far-bank vine root")):
    docs[go] = field(docs[go], "m_Name", name)

b_scale = 1.6 * 1.12 / (png_size("植物b/b3.png")[0] / 100)
c_scale = 1.75 * 1.12 / (png_size("植物c/c3.png")[0] / 100)
bw, bh = png_size("植物b/b1.png")
cw, ch = png_size("植物c/c1.png")
b_art, _ = art("Sleeping plant B", group_transform, "植物b/b1.png", 16.7, .22,
               bw / 100 * b_scale, 3, bh / 100 * b_scale)
c_art, _ = art("Sleeping plant C", group_transform, "植物c/c1.png", 18.6, .30,
               cw / 100 * c_scale, 3, ch / 100 * c_scale)

bridges = sorted(go_ids("Continuous vine crossing") + go_ids("Continuous vine completion"), key=lambda go: position(go)[0])
for go, path, width, x, contact_top in zip(
    bridges, ("植物d/d3.png", "植物f/f3.png"), (1.2, 4.1), (25.4, 28.05), (.27, .64)
):
    set_sprite(go, path)
    px, _ = png_size(path)
    scale = width / (px / 100)
    tid = component(go, 4)
    docs[tid] = field(docs[tid], "m_LocalScale", f"{{x: {scale:.8g}, y: {scale:.8g}, z: 1}}")
    art_center_y = png_size(path)[1] / 100 * scale / 2 - .08
    docs[tid] = field(docs[tid], "m_LocalPosition", f"{{x: {x}, y: {art_center_y:.8g}, z: 0}}")
    cid = component(go, 61)
    docs[cid] = field(docs[cid], "m_Size", f"{{x: {width / scale:.8g}, y: {.18 / scale:.8g}}}")
    docs[cid] = field(docs[cid], "m_Offset", f"{{x: 0, y: {(contact_top - .09 - art_center_y) / scale:.8g}}}")

plants = sorted(
    go_ids("01 - climb vine") + go_ids("02 - near-bank vine") + go_ids("03 - far-bank vine"),
    key=lambda go: position(go)[0],
)
for go, x in zip(plants[1:], (24.5, 30.6)):
    tid = component(go, 4)
    docs[tid] = field(docs[tid], "m_LocalPosition", f"{{x: {x}, y: 0.2, z: 0}}")
    root = int(re.search(r"grownRoot: \{fileID: (\d+)\}", docs[component(go, 114)]).group(1))
    root_tid = component(root, 4)
    docs[root_tid] = field(docs[root_tid], "m_LocalPosition", f"{{x: {-x}, y: -0.2, z: 0}}")
first_doc_id = component(plants[0], 114)
doc = docs[first_doc_id]
doc = field(doc, "grownRoot", "{fileID: 0}")
doc = re.sub(r"(?m)^  placeholderSteps:\n(?:  - [^\n]+\n)+",
             "  placeholderSteps:\n"
             "  - {x: 15.1, y: 0.5, z: 1.6, w: 0.2}\n"
             "  - {x: 16.7, y: 1.35, z: 1.6, w: 0.2}\n"
             "  - {x: 18.6, y: 1.75, z: 1.75, w: 0.2}\n", doc, count=1)
doc = doc.rstrip("\n") + "\n  stepColliderWidths:\n  - 1.1\n  - 1.2\n  - 1.1\n"
doc = field(doc, "plantFrames", "[]")
doc = append_array(doc, "stepFramesA", [f"植物a/a{i}.png" for i in (1, 2, 3)])
doc = append_array(doc, "stepFramesB", [f"植物b/b{i}.png" for i in (1, 2, 3)])
doc = append_array(doc, "stepFramesC", [f"植物c/c{i}.png" for i in (1, 2, 3)])
doc = doc.rstrip("\n") + "\n  stepArtRenderers:\n" + "".join(
    f"  - {{fileID: {component(go, 212)}}}\n" for go in (buds[0], b_art, c_art)
)
doc = field(doc, "growthFrameSeconds", "0.14")
doc = field(doc, "stepArtGroundY", "-0.08")
docs[first_doc_id] = doc
for go, bridge_go, kind, from_right in zip(plants[1:], bridges, ("d", "f"), (0, 1)):
    fid = component(go, 114)
    doc = docs[fid]
    doc = append_array(doc, "bridgeFrames", [f"植物{kind}/{kind}{i}.png" for i in (1, 2, 3)])
    doc = field(doc, "bridgeRenderer", f"{{fileID: {component(bridge_go, 212)}}}")
    doc = field(doc, "bridgeGrowsFromRight", str(from_right))
    doc = field(doc, "growthFrameSeconds", "0.14")
    doc = field(doc, "keepDormantAsRoot", "1")
    docs[fid] = doc

sky = layer("Level01 Art - Sky", 0, 1)
art("Sky gradient", sky, "天空.png", 2, 4.25, 21.1, -40, 8.5)
far = layer("Level01 Art - Far mountains and mother tree", .25, 1)
for i, center in enumerate((4.5, 22.3, 40.1), 1):
    path = f"远景山丘{i}.png"
    width = 18
    height = width * png_size(path)[1] / png_size(path)[0]
    art(f"Far hill {i}", far, path, center, -.2 + height * .5, width, -30)
art("Mother tree", far, "母树（远景）.png", 3.7, 4.32, 8, -27)

mid = layer("Level01 Art - Middle garden", .55, 1)
middle_layout = ((3, 9.6), (9.5, 7.5), (13.5, 6), (16, 4.5),
                 (27, 8.5), (33.5, 7), (38.5, 6), (42, 5))
for i, (center, width) in enumerate(middle_layout, 1):
    path = f"中景{((i - 1) % 4) + 1}.png"
    height = width * png_size(path)[1] / png_size(path)[0]
    art(f"Middle garden {i}", mid, path, center, -.1 + height * .5, width, -20 - ((i - 1) % 2))

trees = layer("Level01 Art - Middle trees", .7, 1)
for i in range(12):
    path = f"树木{(i % 8) + 1}.png"
    height = (3.6, 4.5, 2.8, 3.2, 4, 2.5, 3.6, 4.5)[i % 8]
    width = height * png_size(path)[0] / png_size(path)[1]
    art(f"Middle tree {i + 1}", trees, path, 2 + i * 4.1, height * .5 - .12, width, -17)

front = layer("Level01 Art - Foreground", 1.18, 1)
for i, x in enumerate(range(-2, 58, 4)):
    path = "前景石堆a.png" if i % 2 == 0 else "前景石堆b.png"
    width = 4.6
    height = width * png_size(path)[1] / png_size(path)[0]
    art(f"Foreground rocks {i + 1}", front, path, x, -1.6 + height * .5, width, 11)
leaf_paths = [
    "前景叶片a（浅色）.png", "前景叶片c（深色）.png",
    "前景叶片b（浅色）.png", "前景叶片d（深色）.png",
]
for i, x in enumerate((-1, 4, 9, 14, 19, 24, 29, 34, 39, 44, 49, 54)):
    path = leaf_paths[i % len(leaf_paths)]
    width = 1.4
    height = width * png_size(path)[1] / png_size(path)[0]
    art(f"Foreground leaves {i + 1}", front, path, x, -1.15 + height * .5, width, 12)

camera_go = go_ids("Main Camera")[0]
camera_doc_id = component(camera_go, 20)
docs[camera_doc_id] = field(
    docs[camera_doc_id], "m_BackGroundColor",
    "{r: 0.9059, g: 0.8941, b: 0.8588, a: 1}",
)
follow_doc_id = component(camera_go, 114)
docs[follow_doc_id] = field(docs[follow_doc_id], "minY", "-1.6")

ordinary = [fid for fid in order if fid in docs and klass(docs[fid]) != 1660057539]
scene_roots = [fid for fid in order if fid in docs and klass(docs[fid]) == 1660057539]
result = preamble + "".join(docs[fid] for fid in ordinary + scene_roots)
result = re.sub(r"(?m)^  m_Name: +$", "  m_Name:", result)
SCENE.write_text(result, encoding="utf-8", newline="\n")
print(f"Baked Level01 artwork: {len(docs)} scene documents, {len(list(ART.rglob('*.png')))} PNG assets.")
