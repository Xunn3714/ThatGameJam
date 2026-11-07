"""Import the supplied paintings and assemble the fixed third scene as Unity YAML.

This is deliberately one-shot: it refuses to overwrite later manual placement.
Unity batch mode is unavailable on this machine because its licensing client
rejects headless connections, so this writes the same serialized scene data.
"""

from __future__ import annotations

import re
import shutil
import uuid
from pathlib import Path


PROJECT = Path(__file__).resolve().parents[1]
SOURCE = PROJECT.parent / "美术资源" / "第三关"
ART = PROJECT / "Assets/_Game/Resources/Level03"
SCENE = PROJECT / "Assets/_Game/Scenes/Level03_Alpha.unity"
SECOND = PROJECT / "Assets/_Game/Scenes/Level02_Alpha.unity"
ORIGINAL = SECOND.read_text(encoding="utf-8-sig")
HEADER, *DOCUMENTS = re.split(r"(?=^--- !u!)", ORIGINAL, flags=re.M)
DOCS = {int(match.group(1)): document for document in DOCUMENTS
        if (match := re.search(r"^--- !u!\d+ &(-?\d+)", document))}


ART_FILES = {
    "Faded/Sky": "褪色场景全部素材/天空.png",
    "Faded/Cloud": "褪色场景全部素材/云.png",
    "Faded/Hill": "褪色场景全部素材/远景山丘.png",
    "Faded/Mist": "褪色场景全部素材/雾气（橙）.png",
    "Faded/MidGround": "褪色场景全部素材/中景地面.png",
    "Faded/Tree": "褪色场景全部素材/母树（褪色）.png",
    "Faded/BetweenFlowers": "褪色场景全部素材/两层地板中间的碎花（褪色.png",
    "Faded/Ground": "褪色场景全部素材/近景地面.png",
    "Faded/NearPlants": "褪色场景全部素材/近景草木（褪色）.png",
    "Reborn/Sky": "复生场景全部素材/天空.png",
    "Reborn/Cloud": "复生场景全部素材/云.png",
    "Reborn/Hill": "复生场景全部素材/远景山丘.png",
    "Reborn/Mist": "复生场景全部素材/雾气（橙）.png",
    "Reborn/MidGround": "复生场景全部素材/中景地面.png",
    "Reborn/Tree": "复生场景全部素材/母树.png",
    "Reborn/BetweenFlowers": "复生场景全部素材/两层地面之间的碎花.png",
    "Reborn/Ground": "复生场景全部素材/地面.png",
    "Reborn/NearPlants": "复生场景全部素材/近景草木.png",
    "Reborn/Glow": "复生场景全部素材/辉光（发光属性层）.png",
    "Reborn/Fafa": "复生场景全部素材/fafa.png",
    "Leaves/Crown": "树叶/树叶.png",
    "Leaves/Shadow": "树叶/树冠阴影（图层属性).png",
    "Botanical/Tree": "母树、花草/母树.png",
    "Botanical/NearGrass": "母树、花草/草丛（近.png",
    "Botanical/BigFlowers": "母树、花草/花丛大.png",
    "Botanical/MediumFlowers": "母树、花草/花丛中.png",
    "Botanical/SmallFlowers": "母树、花草/花丛小.png",
    "Botanical/SingleFlower1": "母树、花草/单朵花1.png",
    "Botanical/SingleFlower2": "母树、花草/单朵花2.png",
}


def new_guid() -> str:
    return uuid.uuid4().hex


def import_art() -> dict[str, str]:
    result = {}
    template = (PROJECT / "Assets/_Game/Resources/UI/ResonanceKey.png.meta").read_text()
    for name, original in ART_FILES.items():
        source = SOURCE / original
        if not source.is_file():
            raise FileNotFoundError(source)
        target = ART / (name + ".png")
        target.parent.mkdir(parents=True, exist_ok=True)
        if not target.exists() or target.read_bytes() != source.read_bytes():
            shutil.copy2(source, target)
        meta_path = Path(str(target) + ".meta")
        if not meta_path.exists():
            meta = template.replace(
                re.search(r"^guid: (\w+)$", template, re.M).group(1), new_guid())
            meta = meta.replace("maxTextureSize: 1024", "maxTextureSize: 4096")
            meta = meta.replace("maxTextureSize: 2048", "maxTextureSize: 4096")
            meta = meta.replace("ResonanceKey_0", name.split("/")[-1] + "_0")
            meta_path.write_text(meta, encoding="utf-8")
        result[name] = re.search(r"^guid: (\w+)$", meta_path.read_text(), re.M).group(1)
    for folder in [ART, *ART.iterdir()]:
        if folder.is_dir():
            meta_path = Path(str(folder) + ".meta")
            if not meta_path.exists():
                meta_path.write_text(
                    "fileFormatVersion: 2\nguid: " + new_guid() +
                    "\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
                    "  userData: \n  assetBundleName: \n  assetBundleVariant: \n",
                    encoding="utf-8")
    return result


def script_guid(filename: str) -> str:
    path = PROJECT / "Assets/_Game/Scripts" / (filename + ".cs.meta")
    if not path.exists():
        path.write_text("fileFormatVersion: 2\nguid: " + new_guid() + "\n",
                        encoding="utf-8")
    return re.search(r"^guid: (\w+)$", path.read_text(), re.M).group(1)


def esc(value: str) -> str:
    return '"' + value.replace('"', '\\"') + '"'


next_id = 3000000000
scene_docs = []
roots = []


def alloc() -> int:
    global next_id
    next_id += 1
    return next_id


def add_root(name: str, components: list[int], position=(0, 0, 0), layer=0):
    go, transform = alloc(), alloc()
    all_components = [transform, *components]
    block = "\n".join(f"  - component: {{fileID: {item}}}" for item in all_components)
    scene_docs.append(f"""--- !u!1 &{go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{block}
  m_Layer: {layer}
  m_Name: {esc(name)}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
""")
    x, y, z = position
    scene_docs.append(f"""--- !u!4 &{transform}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {x}, y: {y}, z: {z}}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
""")
    roots.append(transform)
    return go


def mono(go: int, component: int, classname: str, guid: str, fields: str):
    scene_docs.append(f"""--- !u!114 &{component}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::SeedGame.{classname}
{fields}""")


def art_sprite(name: str, key: str, guids: dict[str, str], order: int,
               x=0.0, y=0.0, alpha=1.0):
    go = add_root(name, [next_id + 3], (x, y, 0))
    renderer = alloc()
    # The renderer matches the existing hand-placed scene's Unity 6 format.
    template = DOCS[453742871]
    template = template.replace("&453742871", f"&{renderer}")
    template = template.replace("{fileID: 453742868}", f"{{fileID: {go}}}")
    template = re.sub(r"m_SortingOrder: -?\d+", f"m_SortingOrder: {order}", template)
    template = re.sub(r"m_Sprite: \{fileID: 21300000, guid: \w+, type: 3\}",
                      f"m_Sprite: {{fileID: 21300000, guid: {guids[key]}, type: 3}}", template)
    template = re.sub(r"m_Color: \{r: [^}]+\}",
                      f"m_Color: {{r: 1, g: 1, b: 1, a: {alpha}}}", template)
    scene_docs.append(template)


def collider(name: str, position, size):
    component = next_id + 3
    go = add_root(name, [component], position, layer=6)
    actual = alloc()
    assert actual == component
    template = DOCS[799953928]
    template = template.replace("&799953928", f"&{actual}")
    template = template.replace("{fileID: 799953925}", f"{{fileID: {go}}}")
    template = template.replace("m_Size: {x: 0.58, y: 1.1}",
                                f"m_Size: {{x: {size[0]}, y: {size[1]}}}")
    scene_docs.append(template)


def build_scene(guids: dict[str, str]):
    if SCENE.exists():
        raise SystemExit(f"Refusing to overwrite hand placement: {SCENE}")
    scene_docs.extend(DOCUMENTS[:4])

    # Retain the working player, feet probe, animation and flow references.
    for root, children in [(799953925, [799953927, 799953929, 799953928, 799953926]),
                           (453742868, [453742869, 453742871, 453742870]),
                           (1666568692, [1666568693]),
                           (483829532, [483829534, 483829533])]:
        for identifier in [root, *children]:
            document = DOCS[identifier]
            if identifier == 799953927:
                document = re.sub(r"m_LocalPosition: \{x: [^}]+\}",
                                  "m_LocalPosition: {x: -7.4, y: -2.70, z: 0}", document)
            elif identifier == 799953926:
                document = document.replace("fallResetY: -8", "fallResetY: -7")
            elif identifier == 483829532:
                document = document.replace("Level 2 completion", "Level 3 completion")
            scene_docs.append(document)
        roots.append({799953925: 799953927, 483829532: 483829534}.get(root, 0))
    roots[:] = [item for item in roots if item]

    # Reuse the camera and listener, but remove SeedCamera's follow component.
    camera_go = DOCS[691012173].replace("  - component: {fileID: 691012174}\n", "")
    fixed_camera = alloc()
    camera_go = camera_go.replace("  m_Layer: 0", f"  - component: {{fileID: {fixed_camera}}}\n  m_Layer: 0")
    scene_docs.append(camera_go)
    transform = re.sub(r"m_LocalPosition: \{x: [^}]+\}",
                       "m_LocalPosition: {x: 0, y: 0, z: -10}", DOCS[691012177])
    scene_docs.append(transform)
    camera = re.sub(r"orthographic size: [^\n]+", "orthographic size: 4.285", DOCS[691012176])
    scene_docs.append(camera)
    scene_docs.append(DOCS[691012175])
    mono(691012173, fixed_camera, "Level03FixedCamera",
         script_guid("Level03FixedCamera"), "")
    roots.append(691012177)

    # Positions are derived from the painting's 4096 x 1714 canvas at 200 PPU.
    # The cropped tree is right/top anchored; all full-canvas layers share (0,0).
    pairs = [
        ("Sky", -100, 0, 0, 1),
        ("Mist", -90, -4.00, -2.15, .08),
        ("MidGround", -80, 0, -2.425, 1),
        ("Hill", -60, -4.375, -2.775, 1),
        ("Tree", -40, 6.7775, .0975, 1),
        ("BetweenFlowers", -30, 0, -2.025, 1),
        ("Ground", -20, 0, -2.81, 1),
        ("NearPlants", 18, 0, -2.85, 1),
    ]
    for key, order, x, y, alpha in pairs:
        art_sprite("L03 Faded " + key, "Faded/" + key, guids, order, x, y, alpha)
        # The reborn foreground was exported cropped 352 px from the left of
        # the 4096 px painting. Keep that offset so it reaches the right edge.
        reborn_x, reborn_y = (0, 0) if key == "Tree" else (x, y)
        if key == "NearPlants":
            reborn_x = .8875
        art_sprite("L03 Reborn " + key, "Reborn/" + key, guids,
                   order + 1, reborn_x, reborn_y, alpha)
    # The artist's glow layer is a gentle finishing tint over the new scene.
    art_sprite("L03 Reborn Glow", "Reborn/Glow", guids, 2, 0, 0, .24)
    # The revived painting exports its leftmost flower separately from the
    # cropped 3747 px NearPlants sprite (matched at canvas offset 0, 1440).
    art_sprite("L03 Reborn Fafa", "Reborn/Fafa", guids,
               20, -9.1425, -3.6)
    for i in range(3):
        x = (i - 1) * 20.405
        faded_x, faded_y = (-20.25, .14) if i == 0 else (x, -.27)
        reborn_x, reborn_y = (-20.19, .1) if i == 0 else (x, -.27)
        # Cloud bottoms sit behind the midground. Drawing them above it exposes
        # the lavender brush tail as a false pink strip across the walkable art.
        art_sprite("L03 Faded Cloud " + str(i + 1), "Faded/Cloud", guids,
                   -85, faded_x, faded_y)
        art_sprite("L03 Reborn Cloud " + str(i + 1), "Reborn/Cloud", guids,
                   -84, reborn_x, reborn_y)
    art_sprite("L03 Leaf Shadow", "Leaves/Shadow", guids,
               -37, 7.6075, .3275, .30)
    art_sprite("L03 Leaf Crown", "Leaves/Crown", guids,
               -36, 2.725, 1.485)

    # A single walkable ground edge and broad boundaries avoid invisible gaps.
    collider("L03 Ground collision", (0, -3.50, 0), (20.4, .50))
    collider("L03 Left boundary", (-10.49, 0, 0), (.5, 10.5))
    collider("L03 Right boundary", (10.49, 0, 0), (.5, 10.5))
    collider("L03 Mother tree trunk", (8.70, .05, 0), (2.6, 7.2))

    sequence_id = next_id + 3
    sequence_go = add_root("L03 Rebirth sequence", [sequence_id])
    assert alloc() == sequence_id
    mono(sequence_go, sequence_id, "Level03RebirthSequence",
         script_guid("Level03RebirthSequence"),
         "  player: {fileID: 799953926}\n"
         "  landscapeSeconds: 5.5\n  leavesSeconds: 4.5\n")
    drift_id = next_id + 3
    drift_go = add_root("L03 Cloud drift", [drift_id])
    assert alloc() == drift_id
    mono(drift_go, drift_id, "Level03CloudDrift", script_guid("Level03CloudDrift"),
         "  speed: 0.045\n  repeatWidth: 20.405\n")

    scene_docs.append("--- !u!1660057539 &9223372036854775807\nSceneRoots:\n"
                      "  m_ObjectHideFlags: 0\n  m_Roots:\n" +
                      "".join(f"  - {{fileID: {root}}}\n" for root in roots))
    SCENE.write_text(HEADER + "".join(scene_docs), encoding="utf-8")
    scene_meta = Path(str(SCENE) + ".meta")
    if not scene_meta.exists():
        scene_meta.write_text(
            "fileFormatVersion: 2\nguid: " + new_guid() +
            "\nDefaultImporter:\n  externalObjects: {}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")
    return re.search(r"^guid: (\w+)$", scene_meta.read_text(), re.M).group(1)


def connect_scene(scene_guid: str):
    # Keep the player's existing doorway animation, then load the third scene.
    second = SECOND.read_text(encoding="utf-8-sig")
    marker = "  m_EditorClassIdentifier: Assembly-CSharp::SeedGame.LevelExit\n  destinationScene: \n"
    if marker in second:
        SECOND.write_text(second.replace(marker, marker.replace("destinationScene: ",
            "destinationScene: Level03_Alpha"), 1), encoding="utf-8")
    elif "destinationScene: Level03_Alpha" not in second:
        raise RuntimeError("Second-level exit format changed; did not replace it")
    settings = PROJECT / "ProjectSettings/EditorBuildSettings.asset"
    content = settings.read_text(encoding="utf-8-sig")
    path = "Assets/_Game/Scenes/Level03_Alpha.unity"
    if path not in content:
        content = content.replace("  m_configObjects:\n",
            f"  - enabled: 1\n    path: {path}\n    guid: {scene_guid}\n"
            "  m_configObjects:\n")
        settings.write_text(content, encoding="utf-8")


if __name__ == "__main__":
    guids = import_art()
    guid = build_scene(guids)
    connect_scene(guid)
    print(f"Imported {len(guids)} level-three sprites and built {SCENE}")
