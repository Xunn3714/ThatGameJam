# Seed 第一关 Alpha

项目使用 Unity 6000.5.9f1 的 2D 模板。打开 `Assets/_Game/Scenes/Level01_Alpha.unity` 并按 Play，即可从主角醒来开始试玩。**Seed > Build first level alpha** 会按代码重建同一布局和美术图层；它会覆盖对场景的手工修改，使用前请先保存备份。

操作：A/D 或左右方向键移动，Space 跳跃，E 共鸣，抵达终点后 R 重开。开始试玩时请在 Unity 的 Game 视图内点击一次，让它接收键盘输入。

流程：镜头聚焦并拉远；第一题在树桩学习跳跃；第二题在墙前按一次 E，使 A、B、C 三株植物从左到右依次长成叶台阶梯并翻墙；第三题在沟壑近岸共鸣，只长出一截藤蔓。玩家站上这截藤蔓再次共鸣，才能唤醒对岸的植物并补全桥。过桥后到达暖光终点。失足后返回最近的检查点。主角仍使用原始静态草稿。

当前 alpha 调整：角色移速 3.8，共鸣半径 5。第二题的 A、B、C 三片叶台从左到右逐级升高，墙前只需共鸣一次；墙位于 C 后方，使用贴合倾斜外形的 PolygonCollider2D。第三题第一截藤蔓从近岸地面延伸至 x=26.0，无法直接跳到 x=30 的对岸；站上藤蔓后，半径 5 的共鸣才够到 x=30.6 的对岸植物，补出余下桥面。玩家对三株可共鸣目标的引用保存在场景中，并在引用为空时重新查找。共鸣光圈在角色身上复用；主角贴图导入上限降为 1024，以降低占用。角色物理移动启用插值，使镜头跟随更平滑。持续掉帧仍需要在 Unity Profiler 中确认具体瓶颈。

第二题的 A、B、C 由同一个共鸣目标驱动，分别使用植物 a/b/c 的三阶段图片顺序生长，并保留单向平台碰撞。叶台美术的茎根埋入地面约 0.08 单位，碰撞面对应叶片中央可站立区域，碰撞宽度比整张图片窄。第三题近岸和对岸藤蔓分别使用植物 d/f 的三阶段图片；两端根部位于地面上，桥面碰撞在生长完成后启用。共鸣光圈、终点发光标记仍是占位效果。按 E 后，Console 会记录共鸣到的植物；若没有碰到任何植物，会记录当前位置。这个记录用于 alpha 调试，不是游戏内提示。

第一关美术原图完整复制到 `Assets/_Game/Resources/Level01/`。场景内有五个独立视差图层：天空水平速度 0、远景山丘与母树 0.25、中景花园 0.55、中景树木 0.7、前景 1.18；各层垂直速度均为 1，使树木和地面在镜头缩放后保持接地。地面、墙、树桩、植物和桥保持地图坐标不变。母树位于开场画面中央附近，中景零件重叠构成花园，两种石堆与四种叶片沿地面铺成连续前景。树木与树桩的根部略埋入地面。地板采用拉伸绘制，消除了重复贴图接缝；美术图上移，但碰撞体仍以 y=0 为表面。原来的山丘、花园、细树、地面顶边和藤蔓顶边色块已移除；树桩和墙保留碰撞体，画面由新图片显示。镜头与视差逻辑由独立组件承担，后续关卡可以复用。

共用逻辑位于 `Assets/_Game/Scripts/`：`SeedPlayer` 管移动、跳跃与共鸣；`SeedCamera` 管跟随滞后、自由区和地图边界；`ResonancePlant` 管植物状态；`Checkpoint`、`LevelExit` 与 `LevelFlow` 管关卡流程。第二、三关可复用这些组件，并在新场景中设置不同的位置、半径、镜头边界和植物平台。

`FirstLevelTutorial` 只放在第一关，负责开始时的 A/D、Space、E 提示。第二题的 A/B/C 被唤醒后提示消失；第三题的两端藤蔓用于让玩家自己应用刚学会的规则。第二、三关不要添加此组件，也不会出现教学提示。第一关布局由 `AlphaSceneFactory` 定义；它目前是原型布置工具，正式关卡可将布局放在场景或关卡配置中。

## 在 Unity 中微调

打开 `Level01_Alpha.unity`，在 Hierarchy 中选择对象，使用 Inspector 调整；保存场景后 Play 会使用场景中保存的位置。**Seed > Build first level alpha** 会重新创建场景并覆盖手工调整。若要同时修改重建时的默认值，请改 `Assets/_Game/Scripts/AlphaSceneFactory.cs`（碰撞、谜题坐标）和 `Assets/_Game/Scripts/Level01ArtFactory.cs`（美术坐标）。

- 树与树桩：`Level01 Art - Middle trees/Middle tree 1…12`、`Stump artwork` 的 Transform Y 调画面；`Jump tutorial stump` 的 BoxCollider2D 调实际可站立表面。
- 墙：`Wall artwork` 的 Transform 调画面；`Practice wall` 的 PolygonCollider2D 使用 **Edit Collider** 拖动斜面顶点。墙的碰撞对象和图片对象分开，改一边时检查另一边是否对齐。
- 第二题：`Sleeping plant A/B/C` 调未共鸣时的位置；`01 - climb vine` 上的 ResonancePlant `placeholderSteps` 分别设置三片叶台的中心 X/Y、图片宽度 Z、碰撞厚度 W；`stepColliderWidths` 设置可站立宽度，`stepArtGroundY` 设置三株根部的埋入高度。叶台碰撞体会在 Play 时生成。
- 第三题：`Near-bank vine root`、`Far-bank vine root` 调两端根部；`Continuous vine crossing`、`Continuous vine completion` 调桥面图片和 BoxCollider2D。改桥长时检查从近岸地面共鸣不到对岸，而站上第一段藤蔓后可以共鸣到。

本机 Unity 批处理卡在授权初始化，未能执行 Play 模式。场景已离线写入并通过引用、碰撞与资源完整性检查；打开编辑器后应先确认 Console 无编译错误，再运行场景；如有差异，以实际试玩调整跳高、呼唤半径、平台距离和镜头边界。
