"""Read-only overview of Level 02 scene colliders and their world boxes."""
from pathlib import Path
import re
import sys

scene = Path(__file__).resolve().parents[1] / 'Assets/_Game/Scenes/Level02_Alpha.unity'
raw = scene.read_text(encoding='utf-8-sig')
docs = {}
types = {}
for block in re.split(r'(?=^--- !u!\d+ &\d+)', raw, flags=re.M)[1:]:
    m = re.match(r'--- !u!(\d+) &(\d+)', block)
    types[int(m[2])] = int(m[1])
    docs[int(m[2])] = block

def field(block, key):
    m = re.search(rf'(?m)^  {re.escape(key)}: (.+)$', block)
    return m[1] if m else ''

def vec(block, key):
    s = field(block, key)
    return tuple(float(x) for x in re.findall(r'(?<=[xyz]: )-?[\d.]+', s))

gos = {i: b for i, b in docs.items() if types[i] == 1}
transforms = {int(field(b, 'm_GameObject').split('fileID: ')[1].split('}')[0]): (i, b)
              for i, b in docs.items() if types[i] == 4}

def world(go):
    _, t = transforms[go]
    x, y, _ = vec(t, 'm_LocalPosition')
    sx, sy, _ = vec(t, 'm_LocalScale')
    parent = int(field(t, 'm_Father').split('fileID: ')[1].split('}')[0])
    if parent:
        parent_go = int(field(docs[parent], 'm_GameObject').split('fileID: ')[1].split('}')[0])
        px, py, psx, psy = world(parent_go)
        return px + x*psx, py + y*psy, sx*psx, sy*psy
    return x, y, sx, sy

if '--art' in sys.argv:
    for i, b in docs.items():
        if types[i] != 212 or field(b, 'm_Enabled') != '1':
            continue
        go = int(field(b, 'm_GameObject').split('fileID: ')[1].split('}')[0])
        x,y,sx,sy = world(go)
        if not ('--all' in sys.argv or 6 <= x <= 90):
            continue
        name = field(gos[go], 'm_Name')
        size = vec(b, 'm_Size')
        if len(size) != 2:
            continue
        print(f'{i:10} x={x:6.2f} y={y:6.2f} worldSize=({abs(sx*size[0]):6.2f},{abs(sy*size[1]):6.2f}) order={field(b,"m_SortingOrder"):>3} {name}')
    sys.exit(0)

for i, b in docs.items():
    if types[i] not in (58, 60, 61, 68, 70) or field(b, 'm_Enabled') != '1':
        continue
    go = int(field(b, 'm_GameObject').split('fileID: ')[1].split('}')[0])
    name = field(gos[go], 'm_Name')
    x,y,sx,sy = world(go)
    size = vec(b, 'm_Size')
    offset = vec(b, 'm_Offset')
    label = {58:'circle',60:'polygon',61:'box',68:'edge',70:'capsule'}[types[i]]
    active = field(gos[go], 'm_IsActive')
    print(f'{i:10} {label:7} active={active} layer={field(gos[go], "m_Layer"):>2} '
          f'world=({x:6.2f},{y:6.2f}) scale=({sx:5.2f},{sy:5.2f}) '
          f'size={str(size):18} offset={str(offset):15} {name}')
