# Unity 敌人AI技术Demo

> Full Metal Bagel Games 程序测试题 · 测试方向一：敌人AI（敌人行为系统）
> 基于 xNode 可视化状态机 + Slate 技能时间轴 + ScriptableObject 配置
> 引擎：Tuanjie (Unity 2022.3.61t13) / C#

## 快速开始

1. 用 Tuanjie/Unity 打开 `New Tuanjie Project`
2. 打开场景 `Assets/Game/Scenes/Main.unity`
3. 点击 Play：WASD 移动、鼠标左键近战、鼠标右键远程、R 重开

## 操作

| 操作 | 功能 |
|---|---|
| WASD | 移动 |
| 鼠标位置 | 控制角色朝向（脚下红箭头指示） |
| **鼠标左键** | **近战攻击**（挥剑斩，带挥砍动画） |
| **鼠标右键** | **远程攻击**（手枪射击，带后坐动画） |
| R | 重新开始 |

**UI 信息**：
- 角色头顶：血条 + AI 状态（敌人）
- 敌人头顶：警戒值（百分比）
- 右下角：近战/远程技能冷却（就绪 / 剩余秒数）
- 敌人警戒范围：青色圆环 + 橙色视野扇形（运行时可见）

## 系统架构

```
xNode ──► 敌人状态机（Idle/Patrol/Chase/Attack/Investigate/Return/Dead）
Slate ──► 技能时间轴（攻击动画/伤害窗口/弹丸/特效/音效）
ScriptableObject ──► 敌人/武器/技能静态配置（只读模板）
```

### 敌人 AI（xNode）

- `EnemyStateGraph`（xNode 节点图）+ `EnemyStateMachineRunner`（运行时）
- 状态节点只保存配置，运行时数据放 `EnemyBlackboard`（多敌人可共享一张图）
- **切换条件可视化连线**：状态转换通过独立条件节点（`TransitionConditionNode`）连线——`状态 → 条件节点 → 目标状态`。条件类型可配置（目标可见/丢失、进入/超出攻击范围、计时器、到达目的地、生命阈值、总是），在图编辑器中连线/改参数，无需改代码
- 感知：距离 + 视野角 + 遮挡检测 + 警觉值（归零立即停止追击）
- 移动：NavMeshAgent 封装（`EnemyMotor`）
- 图校验器：`Game > AI > Validate Enemy State Graph`

### 统一战斗系统

玩家与敌人共用 `CharacterCombat`，武器统一三阶段（Windup/Active/Recovery）：

- `WeaponConfig`（SO）：`MeleeWeaponConfig` / `RangedWeaponConfig`
- 伤害：`IDamageable` / `DamageInfo` / `Health` / `FactionMember`（阵营判定）
- **玩家双武器**：左键近战（剑）/ 右键远程（手枪），通过 `CharacterSkillController` 释放
- 死亡判定：玩家死亡后禁止控制、敌人不再攻击尸体

### Slate 技能

- `SkillConfig`（SO）→ `SlateSkillPlayer` → 自定义 ActionClip
- `CharacterSkillController`：冷却 / 范围 / 打断 / 移动锁定 / 指定武器结算
- 自定义 Clip：`AttackAnimationClip`、`FaceTargetClip`、`LockMovementClip`、`MeleeHitWindowClip`、`SpawnProjectileClip`、`SpawnEffectClip`、`PlaySkillAudioClip`、`DashClip`、`SkillSignalClip`

### 角色/武器美术（程序化）

- 低多边形小人（身体/头/四肢）：玩家蓝色、近战敌人红色、远程敌人紫色
- 武器可见模型：剑（刃/护手/柄）、手枪（身/管/握把）
- 朝向指示：脚下红色箭头
- 动画：移动摆动 + 技能攻击动画（手臂挥砍 + 身体前倾）

## 配置资源

```
Assets/Game/Configs
├── Enemies/Enemy_MeleeGrunt.asset      近战杂兵（剑，红色）
├── Enemies/Enemy_RangedGuard.asset     远程守卫（手枪，紫色）
├── Weapons/Weapon_Sword.asset          近战（剑）
├── Weapons/Weapon_Pistol.asset         远程（手枪）
├── Skills/Skill_SwordSlash.asset       挥剑斩（Slate 时间轴+攻击动画）
├── Skills/Skill_PistolShot.asset       手枪射击（Slate 时间轴+攻击动画）
└── Graphs/Enemy_DefaultGraph.asset     xNode 状态图
```

重新生成资产：菜单 `Game > Tools > Create Demo Assets`

## 编辑器工具菜单

| 菜单 | 功能 |
|---|---|
| Game > Tools > Create Demo Assets | 生成所有配置/技能/状态图资源 |
| Game > Tools > Build Character Visual | 为选中角色构建程序化小人视觉 |
| Game > Tools > Build Weapon Visuals | 给武器 prefab 添加可见模型 |
| Game > Tools > Build macOS | 构建 macOS 可执行版 |
| Game > AI > Validate Enemy State Graph | 校验状态图 |

## 场景

- `Main.unity`：地面 + 墙体 + 玩家 + 3 敌人（近战x2/远程x1）+ 巡逻点 + NavMesh + `@DirectorGUI`

## 已实现验收点

- [x] 敌人 Idle/Patrol 巡逻（连续巡逻）
- [x] 视野检测（角度+遮挡+警觉值）→ Chase
- [x] 进入射程 → Attack（通过 Slate 技能）
- [x] 丢失目标 → Investigate → Return → Patrol（警戒归零立即停止）
- [x] 生命归零 → Dead（延迟销毁）
- [x] 玩家/敌人共用战斗与技能系统
- [x] 玩家双武器：左键近战 / 右键远程 + 冷却 UI
- [x] 俯视角平滑跟随相机（鼠标偏移）
- [x] 运行时警戒范围可视化
- [x] 角色/武器美术 + 攻击动画
- [x] 多敌人独立运行（3 敌人共享状态图）
- [x] 胜负判定 + R 重开
- [x] EditMode 自动化测试（10 项通过）
- [x] macOS 可执行版构建
