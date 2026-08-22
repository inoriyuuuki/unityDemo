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
| [04_开发进度.md](./04_开发进度.md) | 按阶段/里程碑拆分的进度跟踪表（阶段一~四完成） |

## 项目现状（2026-08-22）

### 已完成
- ✅ **核心闭环**：玩家移动/朝向、俯视跟随相机、战斗系统（伤害/阵营/武器/投射物）
- ✅ **敌人 AI**：xNode 可视化状态机（Idle/Patrol/Chase/Attack/Investigate/Return/Dead），切换条件可视化连线
- ✅ **配置化**：`EnemyConfig` / `WeaponConfig` / `SkillConfig`（ScriptableObject 只读模板）
- ✅ **Slate 技能**：技能时间轴 + 自定义 Clip + 攻击动画
- ✅ **玩家双武器**：左键近战（剑）/ 右键远程（手枪）+ 冷却 UI
- ✅ **表现**：程序化角色/武器模型、攻击动画、运行时警戒范围可视化、HUD（血条/状态/警戒值）
- ✅ **多敌人**：近战 x2 + 远程 x1 独立运行
- ✅ **测试**：EditMode 自动化测试 10 项通过
- ✅ **构建**：macOS 可执行版（`Game > Tools > Build macOS`）

### 待办/可扩展
- PlayMode 集成测试（T7/T8/T11）本地运行验证
- 演示视频（30~60s）
- Windows 构建
- 更多敌人类型（精英、远程保持距离 + Retreat 状态）
- 更丰富技能（冲刺斩、Buff、无敌帧等）

## 设计核心结论（一句话版）

> **xNode 决定敌人"何时做什么"，Slate 描述技能"在什么时间发生什么"，ScriptableObject 保存"敌人/武器/技能静态配置"；玩家与敌人共用同一套战斗与技能系统。**

## 三层职责边界

| 层 | 负责内容 | 对应系统 |
|---|---|---|
| xNode | 敌人状态机：状态与转换关系 | `EnemyStateGraph` + `EnemyStateMachineRunner` + `TransitionConditionNode` |
| Slate | 技能时序：攻击动画、伤害帧、位移、子弹、特效、音效 | `SlateSkillPlayer` + 自定义 ActionClip |
| ScriptableObject | 静态配置：敌人参数、武器参数、技能规则 | `EnemyConfig` / `WeaponConfig` / `SkillConfig` |
