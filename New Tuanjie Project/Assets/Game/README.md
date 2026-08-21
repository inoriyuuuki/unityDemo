# Unity 敌人AI技术Demo

> Full Metal Bagel Games 程序测试题 · 测试方向一：敌人AI（敌人行为系统）
> 基于 xNode 可视化状态机 + Slate 技能时间轴 + ScriptableObject 配置

## 快速开始

1. 用 Tuanjie/Unity 打开 `New Tuanjie Project`
2. 打开场景 `Assets/Game/Scenes/Main.unity`
3. 点击 Play：WASD 移动、鼠标左键攻击、F1 开关 AI 调试、R 重开

## 操作

| 操作 | 功能 |
|---|---|
| WASD | 移动 |
| 鼠标位置 | 控制角色朝向 |
| 鼠标左键 | 攻击（射击 / 挥砍） |
| F1 | 显示/隐藏 AI 调试信息 |
| R | 重新开始 |

## 系统架构

```
xNode ──► 敌人状态机（Idle/Patrol/Chase/Attack/Investigate/Return/Dead）
Slate ──► 技能时间轴（伤害窗口/弹丸/特效/音效/位移）
ScriptableObject ──► 敌人/武器/技能静态配置（只读模板）
```

### 敌人 AI（xNode）

- `EnemyStateGraph`（xNode 节点图）+ `EnemyStateMachineRunner`（运行时）
- 状态节点只保存配置，运行时数据放 `EnemyBlackboard`（多敌人可共享一张图）
- 感知：距离 + 视野角 + 遮挡检测 + 警觉值
- 移动：NavMeshAgent 封装（`EnemyMotor`）
- 图校验器：`Game > AI > Validate Enemy State Graph`

### 统一战斗系统

玩家与敌人共用 `CharacterCombat`，武器统一三阶段（Windup/Active/Recovery）：

- `WeaponConfig`（SO）：`MeleeWeaponConfig` / `RangedWeaponConfig`
- 伤害：`IDamageable` / `DamageInfo` / `Health` / `FactionMember`（阵营判定）

### Slate 技能

- `SkillConfig`（SO）→ `SlateSkillPlayer` → 自定义 ActionClip
- `CharacterSkillController`：冷却 / 范围 / 打断 / 移动锁定
- 自定义 Clip：`FaceTargetClip`、`LockMovementClip`、`MeleeHitWindowClip`、
  `SpawnProjectileClip`、`SpawnEffectClip`、`PlaySkillAudioClip`、`DashClip`、`SkillSignalClip`

## 配置资源

```
Assets/Game/Configs
├── Enemies/Enemy_MeleeGrunt.asset      敌人参数（生命/移动/感知/行为/战斗）
├── Weapons/Weapon_Sword.asset          近战（剑）
├── Weapons/Weapon_Pistol.asset         远程（手枪）
├── Skills/Skill_SwordSlash.asset       挥剑斩（Slate 时间轴）
├── Skills/Skill_PistolShot.asset       手枪射击（Slate 时间轴）
└── Graphs/Enemy_DefaultGraph.asset     xNode 状态图
```

重新生成资产：菜单 `Game > Tools > Create Demo Assets`

## 场景

- `Main.unity`：地面 + 墙体 + 玩家 + 敌人 + 巡逻点 + NavMesh + `@DirectorGUI`（Slate 演出 GUI）

## 已实现验收点

- [x] 敌人 Idle/Patrol 巡逻
- [x] 视野检测（角度+遮挡+警觉值）→ Chase
- [x] 进入射程 → Attack（通过 Slate 技能）
- [x] 丢失目标 → Investigate → Return → Patrol
- [x] 生命归零 → Dead（延迟销毁）
- [x] 玩家/敌人共用战斗与技能系统
- [x] 俯视角跟随相机
- [x] AI 调试可视化（F1）
- [x] 胜负判定 + R 重开
