# Asset Usage Declaration

This declaration covers the Windows and WebGL releases of **The Winding Journey**. Unity embeds the images and audio in the game's data files. The filenames below identify their sources; they do not need to be copied separately into the release folder.

## Other Visual Assets

The game's character art, artwork for all three levels, original plant keyframes, UI, and opening and ending comics were supplied by the project owner and imported into Unity. The available asset records do not include individual artist credits for each item. Images created or assisted by AI are listed separately below.

## AI-Generated Images Used in the Game

Level 1 uses **10 plant-growth in-between frames** made with OpenAI image generation. Each frame was generated with the adjacent artist-supplied keyframes as references. The original keyframes were not overwritten.

| Plant | AI-generated in-between frames used in the release |
| --- | --- |
| a | `a1_5_ai.png`, `a2_5_ai.png` |
| b | `b1_5_ai.png`, `b2_5_ai.png` |
| c | `c1_5_ai.png`, `c2_5_ai.png` |
| d | `d1_5_ai.png`, `d2_5_ai.png` |
| f | `f1_5_ai.png`, `f2_5_ai.png` |

In the Unity project, these files are in their respective plant folders under `Assets/_Game/Resources/Level01/植物a` through `植物f`. The original generation record is in `文件资料/AI素材登记.md`.

## AI-Assisted Images Used in the Game

The Level 2 gate images `卡齿石板_裂开.png` and `卡齿石板_破开.png` were developed with image-generated composition drafts for the cracks and breaks. The two images in the current release were produced by `Tools/BuildSlabBreakFrames.ps1`, which modifies pixels and creates transparent breaks in the artist-supplied original `卡住齿轮的石板.png`. The earlier AI-redrawn frames were replaced. The process is documented in `AI素材记录.md` at the project root.

## Typeface

The ending instruction uses Chewy by Font Diner, Inc. DBA Sideshow, from [Google Fonts](https://github.com/google/fonts/tree/main/apache/chewy). It is licensed under Apache License 2.0; the license text is included at `ThirdPartyLicenses/Chewy-LICENSE.txt`.

## Sound Effects and Music

| Use | Release file | Source |
| --- | --- | --- |
| Resonance | `Resonance.mp3` | [BenKirb / Benjamin Adams — Shine 5, Pixabay](https://pixabay.com/sound-effects/film-special-effects-shine-5-268908/) |
| Gear pickup | `GearPickup.mp3` | [BenKirb / Benjamin Adams — Shine 7, Pixabay](https://pixabay.com/sound-effects/film-special-effects-shine-7-268909/) |
| Plant growth | `PlantsMovement.mp3` | [Roman_Sol / Roman Sol — Plants Movement SFX, Pixabay](https://pixabay.com/sound-effects/plants-movement-sfx-449035/); short excerpts are selected at runtime |
| Wind | `SoftWind.mp3` | [storegraphic — Soft Wind, Pixabay](https://pixabay.com/sound-effects/soft-wind-477404/) |
| No-energy cue | `NoEnergy.wav` | Synthesized by the project script `Tools/create_no_energy_sfx.py`; not AI-generated |

The four Pixabay creator credits and work links come from the attribution supplied by the project owner and were checked against the work pages.

