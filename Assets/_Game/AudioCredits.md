# 游戏音频来源与使用

| 游戏内用途 | 项目音频 | 来源 | 处理 |
| --- | --- | --- | --- |
| 共鸣 | `Resonance.mp3` | [Benjamin Adams，Pixabay，268908](https://pixabay.com/sound-effects//?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=268908) | 每次共鸣从头播放，约 2.1 秒后停止 |
| 获得齿轮 | `GearPickup.mp3` | [Benjamin Adams，Pixabay，268909](https://pixabay.com/sound-effects//?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=268909) | 齿轮转为能量时播放 |
| 植物生长 | `PlantsMovement.mp3` | [Roman Sol，Pixabay，449035](https://pixabay.com/?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=449035) | 运行时按音量选出四段相隔至少 1.5 秒的 0.7 秒片段，随机播放且避免连续重复 |
| 轻风 | `SoftWind.mp3` | [storegraphic，Pixabay，477404](https://pixabay.com/?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=477404) | 开场漫画期间低音量播放一次、黑屏时淡出；第三关偶尔低音量播放 |
| 第一、三关脚步 | `Grass1.wav` 至 `Grass4.wav` | 用户提供的 `zc_fs_grass1–4.wav`；创作者信息未提供 | 随走路动画循环随机播放，避免连续重复 |
| 第二关脚步 | `Concrete1.wav` 至 `Concrete3.wav` | 用户提供的 `concrete1–3.wav`；创作者信息未提供 | 随走路动画循环随机播放，避免连续重复 |
| 全关卡背景音乐 | `GameMusic.mp3` | 用户提供的 `游戏.mp3`；创作者信息未提供 | 跨关卡持续循环播放 |
| 无能量提示 | `NoEnergy.wav` | 项目内用 `Tools/create_no_energy_sfx.py` 合成 | 下降音；程序生成，非 AI 素材 |
| 共鸣能量回复 | `EnergyRecharge.wav` | 用户自行制作的 `Additive1.wav` | 每回复一格能量时播放一次 |

`EnergyRecharge.wav` 来自用户的 IMGD2030 作业文件夹，`NoEnergy.wav` 由项目脚本生成；其他音频来自用户指定的 `美术资源/音乐音效` 文件夹。植物短片段由原音频在运行时生成，没有使用 AI 生成音频。
