"""Duplicate the hand-placed Level 02 mushroom into the two climbing gaps."""

from pathlib import Path
import re

scene = Path(__file__).resolve().parents[1] / "Assets/_Game/Scenes/Level02_Alpha.unity"
raw = scene.read_text(encoding="utf-8-sig")
if "Recovery mushroom between columns 1 and 2" in raw:
    raise SystemExit("Recovery mushrooms already exist")

blocks = re.split(r"(?=^--- !u!\d+ &\d+)", raw, flags=re.M)
header, blocks = blocks[0], blocks[1:]
docs = {}
types = {}
for block in blocks:
    match = re.match(r"--- !u!(\d+) &(\d+)", block)
    if match:
        types[int(match[2])] = int(match[1])
        docs[int(match[2])] = block

original_go = 1746927256
original_renderers = (1183403905, 1836387527, 1077215737)
stage_go = []
for renderer in original_renderers:
    stage_go.append(int(re.search(r"m_GameObject: \{fileID: (\d+)\}", docs[renderer])[1]))
source_objects = (original_go, *stage_go)
source_docs = []
for go in source_objects:
    source_docs.append(go)
    for doc_id, block in docs.items():
        if re.search(rf"(?m)^  m_GameObject: \{{fileID: {go}\}}$", block):
            source_docs.append(doc_id)
source_docs = list(dict.fromkeys(source_docs))
assert len(source_docs) == 13, source_docs
original_transform = next(i for i in source_docs if types[i] == 4
                          and re.search(rf"(?m)^  m_GameObject: \{{fileID: {original_go}\}}$", docs[i]))
original_x = float(re.search(r"m_LocalPosition: \{x: ([-\d.]+),",
                             docs[original_transform])[1])

root_transforms = []
new_blocks = []
for suffix, x in (("1 and 2", 34.6), ("2 and 3", 42.18)):
    new_ids = {old: 3200005000 + 100 * len(root_transforms) // 4 + n
               for n, old in enumerate(source_docs)}
    # Four GameObjects per mushroom are scene roots, like the original art.
    for go in source_objects:
        old_transform = next(i for i in source_docs if types[i] == 4
                             and re.search(rf"(?m)^  m_GameObject: \{{fileID: {go}\}}$", docs[i]))
        root_transforms.append(new_ids[old_transform])
    delta = x - original_x
    for old in source_docs:
        block = docs[old]
        block = re.sub(rf"(?<=&)({old})(?=\n)", str(new_ids[old]), block, count=1)
        block = re.sub(r"fileID: (\d+)", lambda m: "fileID: " +
                       str(new_ids.get(int(m[1]), int(m[1]))), block)
        if types[old] == 1:
            if old == original_go:
                block = block.replace("m_Name: Bounce mushroom",
                                      f"m_Name: Recovery mushroom between columns {suffix}")
            else:
                block = re.sub(r"(?m)^(  m_Name: .+)$", rf"\1 recovery {suffix}", block, count=1)
        elif types[old] == 4:
            def move(match):
                return f"m_LocalPosition: {{x: {float(match[1]) + delta:.5f}, y: {match[2]}, z: {match[3]}}}"
            block = re.sub(r"m_LocalPosition: \{x: ([-\d.]+), y: ([-\d.]+), z: ([-\d.]+)\}",
                           move, block, count=1)
        new_blocks.append(block)

roots_id = next(i for i, t in types.items() if t == 1660057539)
roots = docs[roots_id]
roots += "".join(f"  - {{fileID: {i}}}\n" for i in root_transforms)
docs[roots_id] = roots
result = header + "".join(docs[int(re.match(r"--- !u!\d+ &(\d+)", b)[1])]
                          for b in blocks) + "".join(new_blocks)
scene.write_text(result, encoding="utf-8", newline="\n")
print(f"Added two recovery mushrooms in the climbing gaps at x=29.3 and 34.8 ({len(new_blocks)} scene documents)")
