# Unity 敌人AI技术Demo — 项目计划

> 来源：ChatGPT 分享对话《Unity敌人AI设计方案》
> 测试题：Full Metal Bagel Games 程序测试题 · 测试方向一：敌人AI（敌人行为系统）
> 技术栈：Unity (Tuanjie 2022.3.61t13) / C#

## 文档索引

| 文档 | 内容 |
|---|---|
| [01_设计方案.md](./01_设计方案.md) | 完整设计方案整理（玩法、AI、战斗、配置、技能、架构） |
| [02_子任务拆分.md](./02_子任务拆分.md) | 按模块拆分的子任务清单（含依赖与验收标准） |
| [03_测试样例.md](./03_测试样例.md) | 验收场景 + 自动化测试样例 |
| [04_开发进度.md](./04_开发进度.md) | 按阶段/里程碑拆分的进度跟踪表 |

## 项目现状（已导入资源）

- 已导入：[xNode-master](`Assets/Script/xNode-master`)（节点图框架）
- 已放置未导入：Slate Cinematic Sequencer v2.2.3（`.unitypackage`，位于 `Assets/Script/Slate Cinematic Sequencer v2.2.3/`，需导入后才能使用 Slate API）
- `Assets/Resource`：空目录（预留资源目录）

## 设计核心结论（一句话版）

> **xNode 决定敌人"何时做什么"，Slate 描述技能"在什么时间发生什么"，ScriptableObject 保存"敌人/武器/技能静态配置"；玩家与敌人共用同一套战斗与技能系统。**

## 三层职责边界

| 层 | 负责内容 | 对应系统 |
|---|---|---|
| xNode | 敌人状态机：状态与转换关系 | `EnemyStateGraph` + `EnemyStateMachineRunner` |
| Slate | 技能时序：动画、伤害帧、位移、子弹、特效、音效 | `SlateSkillPlayer` + 自定义 ActionClip |
| ScriptableObject | 静态配置：敌人参数、武器参数、技能规则 | `EnemyConfig` / `WeaponConfig` / `SkillConfig` |
