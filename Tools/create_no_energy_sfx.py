"""Rebuild the short descending no-energy cue using only Python's stdlib."""

import math
import struct
import wave
from pathlib import Path


OUTPUT = Path(__file__).resolve().parents[1] / "Assets/_Game/Resources/Audio/NoEnergy.wav"
SAMPLE_RATE = 44100
DURATION = 0.52


def descending_tone(t: float, delay: float, start: float, end: float) -> float:
    if t < delay:
        return 0.0
    elapsed = t - delay
    portion = min(1.0, elapsed / (DURATION - delay))
    # Integrate an exponential pitch sweep so the phase remains continuous.
    ratio = math.log(end / start) / (DURATION - delay)
    phase = 2.0 * math.pi * start * math.expm1(ratio * elapsed) / ratio
    attack = min(1.0, elapsed / 0.009)
    release = max(0.0, 1.0 - portion) ** 1.8
    return attack * release * (
        math.sin(phase) + 0.18 * math.sin(2 * phase) + 0.07 * math.sin(3 * phase)
    )


samples = bytearray()
for index in range(round(SAMPLE_RATE * DURATION)):
    t = index / SAMPLE_RATE
    upper = descending_tone(t, 0.0, 660.0, 270.0)
    lower = descending_tone(t, 0.055, 370.0, 165.0)
    value = max(-1.0, min(1.0, 0.19 * upper + 0.12 * lower))
    samples.extend(struct.pack("<h", round(value * 32767)))

OUTPUT.parent.mkdir(parents=True, exist_ok=True)
with wave.open(str(OUTPUT), "wb") as output:
    output.setnchannels(1)
    output.setsampwidth(2)
    output.setframerate(SAMPLE_RATE)
    output.writeframes(samples)

print(OUTPUT)
