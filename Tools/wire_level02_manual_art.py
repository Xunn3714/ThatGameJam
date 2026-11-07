"""Bind the hand-placed Level 02 sprites without changing their transforms."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
SCENE = ROOT / 'Assets/_Game/Scenes/Level02_Alpha.unity'
ART = ROOT / 'Assets/_Game/Resources/Level02'

sprite_guids = {
    '卡齿石板_裂开.png': ('cd58b411d8af44ce8a806bc7f4268311', '-1824833963447752830', '387e0aabd23eca6e0800000000000001'),
    '卡齿石板_破开.png': ('ad5ed7e9638544ca955d1bb99712eac2', '-1824833963447752831', '387e0aabd23eca6e0800000000000002'),
}
template = (ART / '卡住齿轮的石板.png.meta').read_text(encoding='utf-8')
for filename, (guid, internal_id, sprite_id) in sprite_guids.items():
    meta = template.replace('8df590af7664ef74f96800e2b242b403', guid)
    meta = meta.replace('-1824833963447752829', internal_id)
    meta = meta.replace('387e0aabd23eca6e0800000000000000', sprite_id)
    meta = meta.replace('卡住齿轮的石板', filename[:-4])
    (ART / (filename + '.meta')).write_text(meta, encoding='utf-8')

raw = SCENE.read_text(encoding='utf-8-sig')
parts = re.split(r'(?=^--- !u!\d+ &\d+\s*$)', raw, flags=re.M)
headers = {re.search(r'^--- !u!\d+ &(\d+)', part).group(1): i
           for i, part in enumerate(parts) if part.startswith('--- !u!')}

def doc(identity):
    return parts[headers[str(identity)]]

def put(identity, value):
    parts[headers[str(identity)]] = value

def replace_once(identity, before, after):
    value = doc(identity)
    assert before in value, (identity, before)
    put(identity, value.replace(before, after, 1))

def place_stages(identity, renderers):
    value = doc(identity)
    assert 'placedStages:' not in value
    lines = '  placedStages:\n' + ''.join(f'  - {{fileID: {ref}}}\n' for ref in renderers)
    put(identity, value + lines)

def move_logic(identity, x, y):
    value = doc(identity)
    put(identity, re.sub(r'  m_LocalPosition: \{x: [^}]+\}',
                         f'  m_LocalPosition: {{x: {x}, y: {y}, z: 0}}', value, count=1))

def sorting(identity, order):
    replace_once(identity, '  m_SortingOrder: 0\n', f'  m_SortingOrder: {order}\n')

# The artist placed each frame as a distinct object. Only one renderer is visible at a time.
place_stages(742475990, [221565333, 2033585525, 522873548])
place_stages(158663740, [1099002172, 1301387802, 221331045])
move_logic(742475991, 27.8188, .2867)
move_logic(158663741, 32.63, 5.02)

# A third climb plant has no old gameplay target, so it receives its own target.
replace_once(34098450, '  - component: {fileID: 34098451}\n',
             '  - component: {fileID: 34098451}\n  - component: {fileID: 2116237800}\n')
extra = '''--- !u!114 &2116237800
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 34098450}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 698d5c67cadb46d3a2710d5f12129325, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::SeedGame.Level02GrowthVine
  platforms: []
  bud: {fileID: 0}
  artSteps: []
  placedStages:
  - {fileID: 34098451}
  - {fileID: 2018089809}
  - {fileID: 587771477}
'''
assert '2116237800' not in raw
parts.append(extra)

# Hand-placed mushroom and oxalis frame sets.
place_stages(1746927257, [1183403905, 1836387527, 1077215737])
place_stages(2104538852, [1422210752, 1098614929, 736766506])
move_logic(1746927259, 57.22853, .39235115)
move_logic(2104538853, 52.5, 10.06)

flowers = [
    (1635984341, 1635984342, (68.81, 3.83), [640573656, 1800929762, 1454340161]),
    (793017372, 793017373, (73.3024, 3.455), [381552755, 1426261771, 457494990]),
    (1606560266, 1606560267, (77.88, 4.34), [953095111, 2095571748, 472532084]),
    (1883016016, 1883016017, (80.1138, 3.7713), [1057199488, 1517536235, 2145642973]),
]
for component, transform, (x, y), stages in flowers:
    place_stages(component, stages)
    move_logic(transform, x, y)

# Bind the new mechanism pieces; the former invisible collider becomes the door's collider.
gate = doc(1634145350)
gate = gate.replace('  gateVisual: {fileID: 0}\n', '  gateVisual: {fileID: 312413321}\n')
gate = re.sub(r'  rubble:\n(?:  - \{fileID: \d+\}\n)+',
              '  rubble:\n  - {fileID: 0}\n  - {fileID: 839504776}\n'
              '  - {fileID: 2074496539}\n  - {fileID: 1055944345}\n', gate)
gate += ('  jammingSlab: {fileID: 213823325}\n'
         '  slabBreakFrames:\n'
         f'  - {{fileID: -1824833963447752830, guid: {sprite_guids["卡齿石板_裂开.png"][0]}, type: 3}}\n'
         f'  - {{fileID: -1824833963447752831, guid: {sprite_guids["卡齿石板_破开.png"][0]}, type: 3}}\n'
         '  gears:\n  - {fileID: 1299094753}\n  - {fileID: 82959584}\n'
         '  - {fileID: 1884804917}\n  - {fileID: 143608575}\n'
         '  - {fileID: 1907544026}\n  doorRise: 6\n')
put(1634145350, gate)
move_logic(1634145353, 65.2993, 7.5)
replace_once(1634145353, '  m_LocalScale: {x: 0.7, y: 4.9, z: 1}',
             '  m_LocalScale: {x: 1, y: 1, z: 1}')
replace_once(1634145351, '  m_Size: {x: 1, y: 1}', '  m_Size: {x: 1.15, y: 12}')

# Seal the entrance arch, including the space above its visible stonework.
replace_once(2116237434, '  m_Size: {x: 0.65, y: 3.7}',
             '  m_Size: {x: 0.9, y: 5.6}')
replace_once(2116237434, '  m_Offset: {x: 0, y: 0}',
             '  m_Offset: {x: 0, y: 0.3}')

# Make the corridor overlay darker without moving its hand-set transform.
replace_once(948705560, 'm_Color: {r: 0.02, g: 0.08, b: 0.11, a: 0.42}',
             'm_Color: {r: 0.01, g: 0.045, b: 0.065, a: 0.62}')

for renderer, order in {
    34098451: 4, 312413321: 5, 334804831: 6,
    82959583: 4, 1299094752: 4, 143608574: 4,
    1907544025: 4, 839504775: 6, 1055944344: 6,
    2074496538: 6, 1055625029: 2, 1794190282: 8,
}.items():
    sorting(renderer, order)

SCENE.write_text(''.join(parts), encoding='utf-8', newline='\n')
print('Wired Level 02 hand-placed art, gate, entrance collider, and sorting.')
