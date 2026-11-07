# Seed Alpha — Windows 64-bit

This is the three-level Game Jam build. Extract the entire ZIP, then run `SeedAlpha.exe`. Keep `SeedAlpha_Data` and the other files beside the executable.

## Controls

| Key | Action |
| --- | --- |
| A / D | Move |
| Space | Jump |
| E | Resonance; press again during an active pulse to extend its range |
| Hold Space | Glide after unlocking the leaf |

The opening and ending comics play automatically. Resonance becomes available after collecting the first gear.

## AI-generated art used in the game

Ten Level 1 plant transition frames were generated with OpenAI image generation from the artist's original key frames. The original key frames remain separate. These are the AI-generated images included in this build:

| Plant | AI-generated frames |
| --- | --- |
| a | `a1_5_ai.png`, `a2_5_ai.png` |
| b | `b1_5_ai.png`, `b2_5_ai.png` |
| c | `c1_5_ai.png`, `c2_5_ai.png` |
| d | `d1_5_ai.png`, `d2_5_ai.png` |
| f | `f1_5_ai.png`, `f2_5_ai.png` |

The Level 2 gear-blocking slab frames (`卡齿石板_裂开.png` and `卡齿石板_破开.png`) used AI images as early visual drafts. The final in-game frames were made by editing pixels of the artist's original slab with a project script, so they are disclosed as AI-assisted design rather than directly generated images. AI concept pictures and rejected drafts were not included in the build. See `Asset_Usage_Declaration.md` for the complete distinction.

## Audio credits

| In-game use | File | Source / creator |
| --- | --- | --- |
| Resonance | `Resonance.mp3` | [Shine 5 — BenKirb (Benjamin Adams), Pixabay](https://pixabay.com/sound-effects/film-special-effects-shine-5-268908/) |
| Gear pickup | `GearPickup.mp3` | [Shine 7 — BenKirb (Benjamin Adams), Pixabay](https://pixabay.com/sound-effects/film-special-effects-shine-7-268909/) |
| Plant growth | `PlantsMovement.mp3` | [Plants Movement SFX — Roman_Sol (Roman Sol), Pixabay](https://pixabay.com/sound-effects/plants-movement-sfx-449035/); short segments are selected in the game |
| Wind | `SoftWind.mp3` | [Soft Wind — storegraphic, Pixabay](https://pixabay.com/sound-effects/soft-wind-477404/) |
| Level 1 and 3 footsteps | `Grass1.wav`–`Grass4.wav` | User-provided `zc_fs_grass1–4.wav`; creator and license information were not provided |
| Level 2 footsteps | `Concrete1.wav`–`Concrete3.wav` | User-provided `concrete1–3.wav`; creator and license information were not provided |
| Background music | `GameMusic.mp3` | User-provided `游戏.mp3`; creator and license information were not provided |
| Energy recharge | `EnergyRecharge.wav` | User-created `Additive1.wav` |
| Empty energy | `NoEnergy.wav` | Synthesized by the project script `Tools/create_no_energy_sfx.py`; not AI-generated |

The four linked sound effects are credited using the source pages supplied by the user and checked against Pixabay. The game contains no AI-generated audio recorded in the project asset log.

## Build

- Platform: Windows 64-bit
- Unity editor version: 6000.5.9f1
- Scenes: Level 1 → Level 2 → Level 3

