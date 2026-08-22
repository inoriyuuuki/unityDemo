# Unity 敌人AI技术 Demo

> 3D 俯视角射击/近战技术 Demo · 敌人 AI 行为系统演示
> Full Metal Bagel Games 程序测试题 · 测试方向一：敌人AI（敌人行为系统）
> 引擎：Tuanjie / Unity **2022.3.61t13** · 语言：C# · 命名空间：`FMBG.*`

一个"小而完整"的俯视角战斗 Demo：玩家在封闭竞技场中与 3 个各具特色的 AI 敌人周旋，敌人拥有完整的「感知 → 追击 → 攻击 → 搜索 → 返回」行为闭环；玩家与敌人共用同一套战斗与技能系统。

---

## 一、玩法概览

### 玩法目标

- **胜利**：击败场景中全部敌人（近战杂兵 ×2 + 远程守卫 ×1）。
- **失败**：玩家生命归零即失败。
- 胜负判定后弹出结果界面，按 **R** 可重新开始。

### 核心玩法

- **移动与瞄准**：WASD 移动，鼠标控制角色朝向（脚下红色箭头指示）。
- **双武器攻击**：左键挥剑近战、右键手枪远程，均带冷却与技能表现（挥砍/射击动画、弹丸、命中特效）。
- **与 AI 周旋**：敌人会巡逻、发现玩家后追击、进入射程后攻击；被墙体遮挡视线或拉开距离会丢失目标、前往最后发现位置搜索，最终返回巡逻。
- **实时反馈**：HUD 显示玩家/敌人血条、敌人 AI 状态与警戒值、技能冷却；运行时可见敌人警戒范围（青色圆环 + 橙色视野扇形）。

### 操作一览

| 操作 | 功能 |
|---|---|
| `WASD` | 移动 |
| `鼠标位置` | 控制角色朝向 |
| `鼠标左键` | 近战攻击（挥剑斩） |
| `鼠标右键` | 远程攻击（手枪射击） |
| `R` | 重新开始 |
| `F1` | 切换玩家无敌模式（调试用） |

### 快速开始

1. 用 Tuanjie / Unity 2022.3.61t13 打开工程 `New Tuanjie Project/`。
2. 打开场景 `Assets/Game/Scenes/Main.unity`。
3. 点击 Play 即可游玩。
4. 构建可执行版：菜单 `Game > Tools > Build macOS`。

---

## 二、相关功能文档引用

| 文档 | 内容 |
|---|---|
| [docs/00_README.md](./docs/00_README.md) | 项目计划总览：现状、文档索引、三层职责边界 |
| [docs/01_设计方案.md](./docs/01_设计方案.md) | 完整设计方案：玩法、敌人 AI、感知、战斗、技能时间轴、架构 |
| [docs/02_子任务拆分.md](./docs/02_子任务拆分.md) | 按模块拆分的子任务清单（依赖与验收标准） |
| [docs/03_测试样例.md](./docs/03_测试样例.md) | 手动验收场景 + 自动化测试样例（EditMode 20 项 / PlayMode 8 项） |
| [docs/04_开发进度.md](./docs/04_开发进度.md) | 按阶段/里程碑的进度跟踪表（阶段一~七完成） |
| [docs/05_代码评审与结构优化.md](./docs/05_代码评审与结构优化.md) | 代码评审结论与结构优化清单（2026-08-22） |
| [New Tuanjie Project/Assets/Game/README.md](./New%20Tuanjie%20Project/Assets/Game/README.md) | 工程内实现说明：快速开始、系统架构、配置资源、编辑器工具 |

> 仓库根目录另有 [AGENTS.md](./AGENTS.md) 记录开发/构建/测试/提交规范。

---

## 三、实现方案简介

整体采用 **「三层分离」** 架构：**GraphView 状态图** 决定敌人"何时做什么"，**SkillTimeline 技能时间轴** 描述技能"在什么时间发生什么"，**ScriptableObject 配置** 保存"敌人/武器/技能的静态参数"。

### 1. 敌人 AI：GraphView 可视化状态机

- **数据资产 `EnemyStateGraph`**：纯序列化数据模型（状态 + 转换 + 条件节点），不依赖任何编辑器框架；由 **Unity GraphView** 编辑器（`Game > AI > Enemy State Graph (GraphView)`）可视化编辑与校验。
- **运行时 `EnemyStateMachineCompiler` + `EnemyStateMachineRunner`**：把资产编译为运行时状态机，每帧「先全局转换，再当前状态转换，最后 Tick」。
- **状态闭环**：`Idle → Patrol → Chase → Attack → Investigate → Return`，任意状态生命归零 → `Dead`。
- **感知系统**：距离 + 视野角度 + 障碍物遮挡 + 警觉值四层判定，避免"背后秒发现"。
- **节点无运行时状态**：运行时数据全部放在 `EnemyBlackboard`，多个敌人可安全共享同一张图。

### 2. 技能系统：SkillTimeline 时间轴（自研，已替代 Slate）

- **`SkillTimeline`（ScriptableObject）**：轨道 + Clip（`SerializeReference` 多态），内置 9 种 Clip（攻击动画 / 面向目标 / 锁定移动 / 近战伤害窗口 / 生成弹丸 / 特效 / 音效 / 位移 / 信号）。
- **UI Toolkit 时间轴编辑器**：`Game > Tools > Skill Timeline Editor`，支持 Clip 拖拽/缩放/增删、播放头预览、参数面板。
- **`CharacterSkillController`**：统一处理冷却、施法距离、移动锁定、打断；玩家与敌人共用。
- 时间轴只做时序编排，伤害结算统一交给战斗系统（编辑器预览不结算）。

### 3. 统一战斗与配置系统

- **战斗系统**：`Health` / `IDamageable` / `DamageInfo` / `FactionMember`（阵营判定）+ `CharacterCombat` + 武器三阶段（Windup/Active/Recovery），玩家与敌人共用。
- **ScriptableObject 配置**：`EnemyConfig` / `WeaponConfig` / `SkillConfig`，改配置即可改变敌人/武器/技能表现，无需改代码（`OnValidate` 自动钳制非法参数）。
- **程序化表现**：低多边形角色/武器模型、攻击动画、朝向箭头、运行时警戒范围可视化，全部代码生成。

### 4. 测试与交付现状

- ✅ EditMode 自动化测试 **20 项**（伤害/阵营/范围/配置校验/图校验/时间轴运行时）
- ✅ PlayMode 集成测试 **8 项**（AI 状态切换、玩家技能、相机、无敌、胜负判定）
- ✅ macOS 可执行版构建（`Game > Tools > Build macOS`）
- ⏳ 待办：演示视频、Windows 构建、更多敌人类型与技能（详见 [docs/04](./docs/04_开发进度.md)）

### 目录速览

```text
New Tuanjie Project/Assets/Game
├── AI/          GraphView 编辑器 + 状态图数据模型 + 运行时状态机 + 感知/移动
├── Characters/  玩家控制 / 敌人 Actor
├── Combat/      伤害/阵营/武器/技能/时间轴/弹丸（玩家与敌人共用）
├── Camera/      俯视角平滑跟随相机
├── UI/          血条 / 技能冷却 / 胜负结果界面
├── Configs/     敌人/武器/技能/状态图/时间轴配置资源
├── Prefabs/     运行时预制体
├── Scenes/      主场景 Main.unity（含 NavMesh）
├── Editor/      资源生成 / 构建 / 时间轴编辑器
└── Tests/       EditMode + PlayMode 自动化测试
```

---

> 详细设计、进度与评审结论见上文「相关功能文档引用」；行为/画面变更请同步更新 `docs/` 与两处 README。
