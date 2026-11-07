"""Static checks for the saved second-level art and first-level entrance."""

from pathlib import Path
import re


project = Path(__file__).resolve().parents[1]
art = project / "Assets/_Game/Resources/Level02"
scene_dir = project / "Assets/_Game/Scenes"


def docs(path):
    text = path.read_text(encoding="utf-8-sig")
    blocks = re.split(r"(?=^--- !u!\d+ &\d+)", text, flags=re.M)[1:]
    result = {int(re.match(r"--- !u!\d+ &(\d+)", block).group(1)): block for block in blocks}
    assert len(result) == len(blocks), "duplicate scene fileID"
    for block in blocks:
        if block.startswith("--- !u!1 "):
            for ref in re.findall(r"^  - component: \{fileID: (\d+)\}", block, flags=re.M):
                assert int(ref) in result, (path, "component", ref)
        if block.startswith("--- !u!4 "):
            parent = re.search(r"^  m_Father: \{fileID: (\d+)\}", block, flags=re.M)
            assert parent and (parent.group(1) == "0" or int(parent.group(1)) in result)
            for ref in re.findall(r"^  - \{fileID: (\d+)\}", block, flags=re.M):
                assert int(ref) in result, (path, "child", ref)
    roots = next(block for block in blocks if block.startswith("--- !u!1660057539 "))
    for ref in re.findall(r"^  - \{fileID: (\d+)\}", roots, flags=re.M):
        assert int(ref) in result, (path, "root", ref)
    return result


def go(objects, name):
    found = [key for key, block in objects.items()
             if block.startswith("--- !u!1 ") and f"  m_Name: {name}\n" in block]
    assert len(found) == 1, (name, found)
    return found[0]


def component(objects, game_object, kind):
    found = [(key, block) for key, block in objects.items()
             if block.startswith(f"--- !u!{kind} ")
             and f"  m_GameObject: {{fileID: {game_object}}}\n" in block]
    assert len(found) == 1, (game_object, kind, len(found))
    return found[0]


first = docs(scene_dir / "Level01_Alpha.unity")
second = docs(scene_dir / "Level02_Alpha.unity")
for name in ("Ruins entrance rear half", "Ruins entrance front half"):
    component(first, go(first, name), 212)
for name in ("Ruins lintel", "Ruins doorway shadow", "Ruins left jamb", "Ruins right jamb"):
    assert not any(f"  m_Name: {name}\n" in block for block in first.values())
assert not any("  m_Name: Entrance arch rear\n" in block for block in second.values())
go(second, "Water corridor vines")
go(second, "Moss on water corridor")
front = go(second, "Entrance arch front")
collider = component(second, front, 61)[1]
assert "  m_IsTrigger: 0\n" in collider
assert "  m_Size: {x: 0.9, y: 5.6}\n" in collider
camera = component(second, go(second, "Main Camera"), 114)[1]
assert "  minX: 2.01\n" in camera
assert "  playSize: 4\n" in camera
entrance_floor = component(second, go(second, "Ruins floor before shallow water"), 4)[1]
assert "  m_LocalPosition: {x: 3.3, y: -1.5, z: 0}\n" in entrance_floor
assert "  m_LocalScale: {x: 6.6, y: 2.4, z: 1}\n" in entrance_floor

algae = go(second, "Blue algae resonance")
algae_script = next(block for block in second.values()
                    if f"  m_GameObject: {{fileID: {algae}}}\n" in block
                    and "Assembly-CSharp::SeedGame.Level02Algae" in block)
marks = re.findall(r"^  - \{fileID: (\d+)\}", algae_script, flags=re.M)
assert len(marks) == 5, marks
for index in range(1, 5):
    component(second, go(second, f"Algae glow {index}"), 212)
bank = component(second, go(second, "Water plants at far bank"), 212)[0]
assert str(bank) in marks
for mark in marks:
    assert "  m_Color: {r: 0.035, g: 0.09, b: 0.13, a: 1}\n" in second[int(mark)]
darkness = component(second, go(second, "Dim corridor overlay"), 212)[1]
assert "  m_Color: {r: 0.01, g: 0.045, b: 0.065, a: 0.78}\n" in darkness
darkness_transform = component(second, go(second, "Dim corridor overlay"), 4)[1]
assert "  m_LocalPosition: {x: 12, y: 3.4, z: 0}\n" in darkness_transform
assert "  m_LocalScale: {x: 20.2, y: 16, z: 1}\n" in darkness_transform

asset_guids = {}
for meta in art.glob("*.png.meta"):
    guid = re.search(r"(?m)^guid: ([0-9a-f]{32})$", meta.read_text(encoding="utf-8")).group(1)
    asset_guids[guid] = meta
native = 0
for block in second.values():
    if not block.startswith("--- !u!212 "):
        continue
    match = re.search(r"m_Sprite: \{fileID: \d+, guid: ([0-9a-f]{32}), type: 3\}", block)
    if not match or match.group(1) not in asset_guids:
        continue
    game_object = int(re.search(r"m_GameObject: \{fileID: (\d+)\}", block).group(1))
    name = re.search(r"(?m)^  m_Name: (.+)$", second[game_object]).group(1)
    native += 1
assert native >= 70, native

unlayered = []
missing_sprites = []
for block in second.values():
    if not block.startswith("--- !u!212 ") or "  m_Enabled: 0\n" in block:
        continue
    game_object = int(re.search(r"m_GameObject: \{fileID: (\d+)\}", block).group(1))
    name = re.search(r"(?m)^  m_Name: (.+)$", second[game_object]).group(1)
    if "  m_SortingOrder: 0\n" in block:
        unlayered.append(name)
    if "  m_Sprite: {fileID: 0}\n" in block:
        missing_sprites.append(name)
assert not missing_sprites, missing_sprites
assert not unlayered, unlayered

placed = [block for block in second.values() if "  placedStages:\n" in block]
assert len(placed) == 9, len(placed)  # three climb, mushroom, oxalis, four gate plants
project_sprite_guids = {
    match.group(1)
    for meta in (project / "Assets").rglob("*.png.meta")
    if (match := re.search(r"(?m)^guid: ([0-9a-f]{32})$", meta.read_text(encoding="utf-8")))
}
for block in placed:
    frames = re.search(r"  placedStages:\n((?:  - \{fileID: \d+\}\n)+)", block)
    assert frames and len(re.findall(r"fileID: (\d+)", frames.group(1))) == 3
    for ref in re.findall(r"fileID: (\d+)", frames.group(1)):
        assert second[int(ref)].startswith("--- !u!212 "), ref
        sprite = re.search(r"  m_Sprite: \{fileID: -?\d+, guid: ([0-9a-f]{32}), type: 3\}", second[int(ref)])
        assert sprite and sprite.group(1) in project_sprite_guids, ref
gate = next(block for block in second.values() if "Assembly-CSharp::SeedGame.Level02Gate\n" in block)
assert "  gateVisual: {fileID: 312413321}\n" in gate
assert "  jammingSlab: {fileID: 213823325}\n" in gate
assert "  doorRise: 6\n" in gate
assert "  maxX: 88.4\n" in camera
floor = go(second, '"\\u5730\\u677F_0"')
assert "  m_Layer: 6\n" in second[floor]
assert "  m_Size: {x: 38.28, y: 0.3}\n" in component(second, floor, 61)[1]
assert ((80.1138 - 64.4) ** 2 + (3.7713 - 4) ** 2) ** .5 < 8.6 * 1.9
for key in ("卡齿石板_裂开.png", "卡齿石板_破开.png"):
    assert (art / key).is_file() and (art / (key + ".meta")).is_file()

# The saved collision route must follow the artist-positioned sprites.
for index, (x, y) in enumerate(((8.39, -0.04), (12.66, -0.11),
                                (16.99, -0.11), (20.95, -0.08)), 1):
    stone = go(second, f"Stepping stone {index}")
    transform = component(second, stone, 4)[1]
    assert f"  m_LocalPosition: {{x: {x}, y: {y}, z: 0}}\n" in transform
    assert "  m_LocalScale: {x: 2.4, y: 0.12, z: 1}\n" in transform
    assert "  m_UsedByEffector: 0\n" in component(second, stone, 61)[1]
    if index == 1:
        first_collider = component(second, stone, 61)[1]
        assert "  m_Offset: {x: -0.025, y: 0}\n" in first_collider
        assert "  m_Size: {x: 1.1, y: 1}\n" in first_collider
    assert not any(f"  m_GameObject: {{fileID: {stone}}}\n" in block and
                   block.startswith("--- !u!251 ") for block in second.values())
for name in ("First climbing column", "Tall climbing column", "Stone bridge before gate"):
    item = go(second, name)
    assert "  m_Layer: 6\n" in second[item]
    assert any(f"  m_GameObject: {{fileID: {item}}}\n" in block and
               "guid: 66e18529d7644c94ab78c5941c83977c" in block
               for block in second.values())
for name in ("First broken column", "Second broken column", "High gear platform",
             "Exit raised platform before gate", "Exit raised platform after gate"):
    assert "  m_Enabled: 0\n" in component(second, go(second, name), 61)[1]
bank_floor = component(second, go(second, "Ruins floor after shallow water"), 4)[1]
assert "  m_LocalPosition: {x: 43.4, y: -1.44, z: 0}\n" in bank_floor
assert "  m_LocalScale: {x: 42.6, y: 2.4, z: 1}\n" in bank_floor
exit_transform = component(second, go(second, "Level 2 exit trigger"), 4)[1]
assert "  m_LocalPosition: {x: 87, y: 4.15, z: 0}\n" in exit_transform
mushroom_collision = component(second, go(second, "Bounce mushroom"), 61)[1]
assert "  m_Offset: {x: 0.14, y: 0.91}\n" in mushroom_collision
assert "  m_Size: {x: 1.3, y: 0.18}\n" in mushroom_collision
assert "  m_Enabled: 0\n" in mushroom_collision
gate_collision = component(second, go(second, "Final stone gate"), 61)[1]
assert "  m_Size: {x: 1.1, y: 10.05}\n" in gate_collision
assert "  m_Offset: {x: 0, y: 0.95}\n" in gate_collision
camera_zone = go(second, "Gate puzzle camera zone")
assert "  m_IsTrigger: 1\n" in component(second, camera_zone, 61)[1]
assert "  m_LocalPosition: {x: 48.5, y: 10.25, z: 0}\n" in component(
    second, camera_zone, 4)[1]
assert "  m_Size: {x: 6, y: 3.5}\n" in component(second, camera_zone, 61)[1]
assert any(f"  m_GameObject: {{fileID: {camera_zone}}}\n" in block and
           "Assembly-CSharp::SeedGame.Level02GateCameraZone" in block and
           "  cameraController: {fileID: 691012174}\n" in block and
           "  gate: {fileID: 1634145350}\n" in block and
           "  viewMin: {x: 55, y: -0.9}\n" in block and
           "  viewMax: {x: 86.2, y: 16.4}\n" in block and
           "  padding: 0.05\n" in block
           for block in second.values())

# The saved exit art brackets the player and carries the right-hand world wall.
front_door = go(second, '"\\u77F3\\u95E8-\\u524D"')
rear_door = go(second, '"\\u77F3\\u95E8-\\u540E"')
player_art = go(second, "Original character draft")
def order(item):
    return int(re.search(r"^  m_SortingOrder: (-?\d+)$",
                         component(second, item, 212)[1], flags=re.M).group(1))
assert order(rear_door) < order(player_art) < order(front_door)
assert "  m_Layer: 6\n" in second[front_door]
exit_wall = component(second, front_door, 61)[1]
assert "  m_IsTrigger: 0\n" in exit_wall
assert "  m_Offset: {x: 1.115, y: 0}\n" in exit_wall
assert "  m_Size: {x: 0.4, y: 16}\n" in exit_wall
assert order(go(second, '"\\u690D\\u7269b-3_0"')) > order(player_art)
assert order(go(second, '"\\u5C0F\\u77F3\\u5934_0"')) > order(player_art)

vine_meta = (art / "藤蔓.png.meta").read_text(encoding="utf-8")
assert "  spriteMode: 1\n" in vine_meta and "  spritePixelsToUnits: 200\n" in vine_meta
print(f"Validated two scenes, 9 three-frame plants, gate wiring, entrance collider, and {native} imported art sprites.")
