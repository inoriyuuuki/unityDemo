# Unity 敌人AI技术Demo — 项目计划

> 来源：ChatGPT 分享对话《Unity敌人AI设计方案》
> 测试题：Full Metal Bagel Games 程序测试题 · 测试方向一：敌人AI（敌人行为系统）
> 技术栈：Unity (Tuanjie 2022.3.61t13) / C#

## 文档索引

| 文档 | 内容 |
|---|---|
| [01_设计方案.md](./01_设计方案.md) | 完整设计方案整理（玩法、AI、战斗、配置、技能、架构） |
| [02_子任务拆分.md](./02_子任务拆分.md) | 按模块拆分的子任务清单（含依赖与验收标准） |
| [03_测试样例.md](./03_测试样例.md) | 验收场景 + 自动化测试样例（EditMode 10 项已通过） |
| [04_开发进度.md](./04_开发进度.md) | 按阶段/里程碑拆分的进度跟踪表（阶段一~五完成） |
| [05_代码评审与结构优化.md](./05_代码评审与结构优化.md) | 代码审查结论与结构优化清单（2026-08-22） |

## 项目现状（2026-08-22）

### 已完成
- ✅ **核心闭环**：玩家移动/朝向、俯视跟随相机、战斗系统（伤害/阵营/武器/投射物）
- ✅ **敌人 AI**：GraphView 可视化状态机（Idle/Patrol/Chase/Attack/Investigate/Return/Dead），切换条件可视化连线（已由 xNode 迁移至 Unity GraphView）
- ✅ **配置化**：`EnemyConfig` / `WeaponConfig` / `SkillConfig`（ScriptableObject 只读模板）
- ✅ **技能时间轴（UI Toolkit 编辑器）**：`SkillTimeline` 资源 + 自定义 Clip + 攻击动画（替代原 Slate，Slate 已删除）
- ✅ **玩家双武器**：左键近战（剑）/ 右键远程（手枪）+ 冷却 UI
- ✅ **表现**：程序化角色/武器模型、攻击动画、运行时警戒范围可视化、HUD（血条/状态/警戒值）
- ✅ **多敌人**：近战 x2 + 远程 x1 独立运行
- ✅ **测试**：EditMode 自动化测试 10 项通过
- ✅ **构建**：macOS 可执行版（`Game > Tools > Build macOS`）
- ✅ **代码评审**：全量代码/文档审查 + 结构优化（详见 [05_代码评审与结构优化.md](./05_代码评审与结构优化.md)）

### 待办/可扩展
- ~~PlayMode 集成测试（T7/T8/T11）~~ → 已通过（8/8，2026-08-22）
- 演示视频（30~60s）
- Windows 构建
- 更多敌人类型（精英、远程保持距离 + Retreat 状态）
- 更丰富技能（冲刺斩、Buff、无敌帧等）

## 设计核心结论（一句话版）

> **GraphView 状态图决定敌人"何时做什么"，SkillTimeline 描述技能"在什么时间发生什么"，ScriptableObject 保存"敌人/武器/技能静态配置"；玩家与敌人共用同一套战斗与技能系统。**

## 三层职责边界

| 层 | 负责内容 | 对应系统 |
|---|---|---|
| GraphView | 敌人状态机：状态与转换关系（运行时为纯数据模型） | `EnemyStateGraph` + `EnemyStateMachineRunner` + `EnemyStateMachineCompiler` |
| SkillTimeline | 技能时序：攻击动画、伤害帧、位移、子弹、特效、音效 | `SkillTimelinePlayer` + 自定义 Clip（UI Toolkit 编辑器） |
| ScriptableObject | 静态配置：敌人参数、武器参数、技能规则 | `EnemyConfig` / `WeaponConfig` / `SkillConfig` |
