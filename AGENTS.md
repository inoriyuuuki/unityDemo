# Repository Guidelines

## Project Structure & Module Organization

The repository contains a Tuanjie/Unity project in `New Tuanjie Project/` and design/progress notes in `docs/`.

- `Assets/Game/AI/`: xNode enemy state graph, runtime states, perception, and movement.
- `Assets/Game/Combat/`: health, factions, weapons, projectiles, skills, and Slate action clips.
- `Assets/Game/Characters/`, `Camera/`, `UI/`, `Visual/`: player/enemy behavior and presentation.
- `Assets/Game/Configs/`: ScriptableObject enemy, weapon, skill, and graph configuration.
- `Assets/Game/Prefabs/` and `Scenes/Main.unity`: runtime content and the primary demo scene.
- `Assets/Game/Editor/`: asset-generation, validation, and build menu tools.
- `Assets/Game/Tests/EditMode/` and `PlayMode/`: Unity Test Framework suites.

Do not commit generated `Library/`, `Temp/`, `Builds/`, IDE files, or local MCP configuration.

## Build, Test, and Development Commands

Open `New Tuanjie Project` with Tuanjie `2022.3.61t13`, load `Assets/Game/Scenes/Main.unity`, and press Play for local development.

Useful editor actions:

- `Game > Tools > Build macOS`: creates `Builds/EnemyAIDemo_mac`.
- `Game > Tools > Create Demo Assets`: regenerates configured demo assets; review resulting asset changes carefully.
- `Game > AI > Validate Enemy State Graph`: validates required graph nodes and connections.
- `Window > General > Test Runner`: runs EditMode or PlayMode tests.

Batch test example:

```bash
/Applications/Tuanjie/Tuanjie.app/Contents/MacOS/Tuanjie \
  -batchmode -quit -projectPath "$PWD/New Tuanjie Project" \
  -runTests -testPlatform editmode -testResults /tmp/editmode.xml
```

## Coding Style & Naming Conventions

Use four-space indentation and standard C# conventions: `PascalCase` for types/methods/properties, `camelCase` for locals and private fields, and namespaces under `FMBG.*`. Prefer `[SerializeField] private` fields over public mutable state. Keep ScriptableObjects as static configuration; place per-enemy runtime state in contexts or blackboards. Add concise XML summaries to public systems and non-obvious gameplay logic.

## Testing Guidelines

Tests use NUnit with the Unity Test Framework. Name tests by behavior, such as `PlayerRangedSkill_SpawnsProjectileAndDealsDamage`. Add EditMode tests for deterministic logic/configuration and PlayMode tests for timelines, physics, scene wiring, and AI behavior. Run the relevant suite plus existing regression tests before submitting.

## Commit & Pull Request Guidelines

Use descriptive Conventional Commit-style prefixes seen in history: `feat:`, `fix:`, `test:`, `docs:`, and `build:`. Avoid placeholder messages. Pull requests should summarize behavior changes, list tests run, identify modified scenes/assets, and include screenshots or short captures for camera, UI, animation, or visual changes. Link related issues and call out regenerated assets explicitly.
