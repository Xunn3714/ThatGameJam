"""Static integrity check for the baked Unity scene and imported Level01 sprites."""

from pathlib import Path
import hashlib
import re
import struct

project = Path(__file__).resolve().parents[1]
source = project.parent / "美术资源/第一关零件-assets"
copied = project / "Assets/_Game/Resources/Level01"
scene = (project / "Assets/_Game/Scenes/Level01_Alpha.unity").read_text(encoding="utf-8")
assert scene.rstrip().endswith("- {fileID: 1954418791}"), "SceneRoots should be the final document"

for original in source.rglob("*.png"):
    target = copied / original.relative_to(source)
    assert target.exists(), original
    assert hashlib.sha256(original.read_bytes()).digest() == hashlib.sha256(target.read_bytes()).digest(), original
    width, height = struct.unpack(">II", target.read_bytes()[16:24])
    meta = Path(str(target) + ".meta").read_text(encoding="utf-8")
    assert re.search(rf"(?m)^        width: {width}$", meta), target
    assert re.search(rf"(?m)^        height: {height}$", meta), target

guids = {}
for meta in (project / "Assets").rglob("*.meta"):
    match = re.search(r"(?m)^guid: ([0-9a-f]{32})$", meta.read_text(encoding="utf-8", errors="replace"))
    if match:
        assert match.group(1) not in guids, (meta, guids.get(match.group(1)))
        guids[match.group(1)] = meta
for guid in re.findall(r"m_Sprite: \{fileID: 21300000, guid: ([0-9a-f]{32}), type: 3\}", scene):
    assert guid in guids, guid
for guid in re.findall(r"\{fileID: 21300000, guid: ([0-9a-f]{32}), type: 3\}", scene):
    assert guid in guids, guid

blocks = re.split(r"(?=^--- !u!\d+ &\d+)", scene, flags=re.M)[1:]
docs = {int(re.match(r"--- !u!\d+ &(\d+)", block).group(1)): block for block in blocks}

def world_collider_size(name):
    go = next(int(re.match(r"--- !u!1 &(\d+)", block).group(1)) for block in blocks
              if block.startswith("--- !u!1 ") and f"m_Name: {name}\n" in block)
    transform = next(block for block in blocks if block.startswith("--- !u!4 ")
                     and f"m_GameObject: {{fileID: {go}}}" in block)
    collider = next(block for block in blocks if block.startswith("--- !u!61 ")
                    and f"m_GameObject: {{fileID: {go}}}" in block)
    scale = re.search(r"m_LocalScale: \{x: ([^,]+), y: ([^,]+)", transform)
    size = re.search(r"^  m_Size: \{x: ([^,]+), y: ([^}]+)\}", collider, flags=re.M)
    return float(scale.group(1)) * float(size.group(1)), float(scale.group(2)) * float(size.group(2))

def collider_top(name):
    go = next(int(re.match(r"--- !u!1 &(\d+)", block).group(1)) for block in blocks
              if block.startswith("--- !u!1 ") and f"m_Name: {name}\n" in block)
    transform = next(block for block in blocks if block.startswith("--- !u!4 ")
                     and f"m_GameObject: {{fileID: {go}}}" in block)
    collider = next(block for block in blocks if block.startswith("--- !u!61 ")
                    and f"m_GameObject: {{fileID: {go}}}" in block)
    position = re.search(r"m_LocalPosition: \{x: [^,]+, y: ([^,]+)", transform)
    offset = re.search(r"m_Offset: \{x: [^,]+, y: ([^}]+)\}", collider)
    size = re.search(r"^  m_Size: \{x: [^,]+, y: ([^}]+)\}", collider, flags=re.M)
    return float(position.group(1)) + float(offset.group(1)) + float(size.group(1)) / 2

def object_x(name):
    go = next(int(re.match(r"--- !u!1 &(\d+)", block).group(1)) for block in blocks
              if block.startswith("--- !u!1 ") and f"m_Name: {name}\n" in block)
    transform = next(block for block in blocks if block.startswith("--- !u!4 ")
                     and f"m_GameObject: {{fileID: {go}}}" in block)
    return float(re.search(r"m_LocalPosition: \{x: ([^,]+)", transform).group(1))

for name, expected in (
    ("Starting earth", (25, 2.4)), ("After gap earth", (17, 2.4)),
    ("Jump tutorial stump", (1.15, .8)),
    ("Continuous vine crossing", (1.2, .18)), ("Continuous vine completion", (4.1, .18)),
):
    actual = world_collider_size(name)
    assert all(abs(a - e) < .001 for a, e in zip(actual, expected)), (name, actual, expected)
for name in ("Starting earth", "After gap earth"):
    assert abs(collider_top(name)) < .001, (name, collider_top(name))

wall_go = next(int(re.match(r"--- !u!1 &(\d+)", block).group(1)) for block in blocks
               if block.startswith("--- !u!1 ") and "m_Name: Practice wall\n" in block)
wall_polygon = next(block for block in blocks if block.startswith("--- !u!60 ")
                    and f"m_GameObject: {{fileID: {wall_go}}}" in block)
assert "m_Points:\n    m_Paths:" in wall_polygon
assert len(re.findall(r"\{x: [-\d.]+, y: [-\d.]+\}", wall_polygon)) >= 7
assert not any(block.startswith("--- !u!61 ") and f"m_GameObject: {{fileID: {wall_go}}}" in block
               for block in blocks)

for block in blocks:
    if block.startswith("--- !u!1 "):
        for ref in re.findall(r"^  - component: \{fileID: (\d+)\}", block, flags=re.M):
            assert int(ref) in docs, ("component", ref)
    if block.startswith("--- !u!4 "):
        parent = re.search(r"^  m_Father: \{fileID: (\d+)\}", block, flags=re.M)
        assert parent and (parent.group(1) == "0" or int(parent.group(1)) in docs), parent
        for ref in re.findall(r"^  - \{fileID: (\d+)\}", block, flags=re.M):
            assert int(ref) in docs, ("child", ref)

for removed in ("Distant violet hills", "Middle pale garden", "Simple midground tree", "Distant mother tree", "Vine top highlight", "Solid leaf platform"):
    assert f"m_Name: {removed}" not in scene, removed
for present in ("Level01 Art - Sky", "Level01 Art - Far mountains and mother tree", "Level01 Art - Middle garden", "Level01 Art - Middle trees", "Level01 Art - Foreground", "Stump artwork", "Wall artwork"):
    assert f"m_Name: {present}" in scene, present
assert scene.count("bridgeFrames:") == 2
assert scene.count("stepFramesA:") == 1
assert scene.count("stepFramesB:") == 1
assert scene.count("stepFramesC:") == 1
assert scene.count("stepArtRenderers:") == 1
assert "stepColliderWidths:\n  - 1.1\n  - 1.2\n  - 1.1" in scene
for name in ("Sleeping plant A", "Sleeping plant B", "Sleeping plant C",
             "Near-bank vine root", "Far-bank vine root"):
    assert scene.count(f"m_Name: {name}") == 1, name
assert "- {x: 18.6, y: 1.75, z: 1.75, w: 0.2}" in scene
assert "stepArtGroundY: -0.08" in scene
assert scene.count("keepDormantAsRoot: 1") == 2
shore_edge = object_x("Starting earth") + world_collider_size("Starting earth")[0] / 2
first_bridge_edge = object_x("Continuous vine crossing") + world_collider_size("Continuous vine crossing")[0] / 2
far_root_x = object_x("03 - far-bank vine")
radius = float(re.search(r"(?m)^  resonanceRadius: ([^\n]+)", scene).group(1))
assert far_root_x - shore_edge > radius, "Far-bank vine must be unreachable from shore"
assert far_root_x - first_bridge_edge < radius, "Far-bank vine must be reachable from the first bridge"
assert "minY: -1.6" in scene
assert scene.count("verticalScreenSpeed: 1\n") == 5
assert scene.count("horizontalScreenSpeed:") == 5
print(f"Validated {len(list(source.rglob('*.png')))} unchanged PNGs, {len(docs)} scene documents and 5 parallax layers.")
