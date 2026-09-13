# Party Marker Party List

[中文](#中文) | [English](#english)

## 中文

这是一个 Dalamud 插件，用于把队伍成员头顶的原生目标标记同步显示在小队列表中：

```text
[头顶标记] [职业图标] [玩家名]
```

### 功能

- 从 `MarkingController.Markers` 读取当前角色头顶的原生目标标记及目标实体 ID。
- 使用游戏原生图标显示攻击、止步/锁链、禁止和几何标记。
- 通过实体 ID 将标记与小队列表中的实际队伍成员行对应起来。
- 在小队列表职业图标左侧绘制标记，不遮挡职业图标和队伍点击区域。
- 覆盖层不接收输入，不改写原生 UI 节点。
- 使用 `/partymarkers` 切换显示。

### 使用

将发布包中的 `PartyMarkerPartyList.dll` 和 `PartyMarkerPartyList.json` 放入 Dalamud 的插件目录，
或把构建输出目录加入 Dalamud 的开发插件目录，然后在游戏中加载插件。

### 本地构建

插件通过 DLL 引用使用本机的 Hooks 开发运行时，不把 Dalamud 或
FFXIVClientStructs 源码项目作为编译依赖。默认目录是：

```text
%APPDATA%\XIVLauncher\addon\Hooks\dev
```

目录中需要有 `Dalamud.dll`、`FFXIVClientStructs.dll`、
`Dalamud.Bindings.ImGui.dll` 和 `InteropGenerator.Runtime.dll`。如果 DLL 在其他目录，可以通过
`-p:HooksDevPath="..."` 覆盖：

```powershell
dotnet build .\PartyMarkerPartyList.csproj -c Debug -p:HooksDevPath="$env:APPDATA\XIVLauncher\addon\Hooks\dev"
```

Dalamud 与 FFXIVClientStructs 源码 checkout 仅用于查阅 API 和结构布局。

当前支持角色头顶的原生目标标记：攻击 1～8、止步/锁链 1～3、禁止 1～2、方块、圆圈、十字、三角；
不包含场地标点或机制标记。

### 修复与回归检查

`0.1.1` 修复了跨服小队在副本外，远端成员被误显示为“攻击 1”的问题。
无效实体 ID（`0`、`0xE0000000`）不会匹配标记；没有有效本地实体 ID 的成员不显示标记。

使用同一套 Hooks dev DLL 运行匹配逻辑回归检查：

```powershell
dotnet run --project .\tests\MarkerMatcher.Tests.csproj -c Release -p:HooksDevPath="$env:APPDATA\XIVLauncher\addon\Hooks\dev"
```

## English

Party Marker Party List is a Dalamud plugin that mirrors native target markers above party members' heads
in the party list:

```text
[target marker] [job icon] [player name]
```

### Features

- Reads native target-marker slots and target entity IDs from `MarkingController.Markers`.
- Uses the game's native icons for attack, bind/stop, ignore, and shape markers.
- Matches markers to the correct party-list row by entity ID.
- Draws each marker to the left of the party member's job icon.
- Uses a non-interactive overlay and does not modify native UI nodes or party-list click areas.
- Use `/partymarkers` to toggle the overlay.

### Usage

Place `PartyMarkerPartyList.dll` and `PartyMarkerPartyList.json` from the release package in your Dalamud plugin
directory, or add the build output directory as a Dalamud dev-plugin directory and load the plugin in game.

### Local build

The project references the locally installed Hooks development runtime DLLs directly; it does not use the
Dalamud or FFXIVClientStructs source repositories as compile-time project dependencies. The default directory is:

```text
%APPDATA%\XIVLauncher\addon\Hooks\dev
```

The directory must contain `Dalamud.dll`, `FFXIVClientStructs.dll`, `Dalamud.Bindings.ImGui.dll`, and
`InteropGenerator.Runtime.dll`. Override the location with `-p:HooksDevPath="..."` when necessary:

```powershell
dotnet build .\PartyMarkerPartyList.csproj -c Debug -p:HooksDevPath="$env:APPDATA\XIVLauncher\addon\Hooks\dev"
```

The Dalamud and FFXIVClientStructs source checkouts are used only for API and structure-layout reference.

The plugin currently supports native target markers above characters' heads: Attack 1–8, Bind/Stop 1–3,
Ignore 1–2, Square, Circle, Plus, and Triangle. Field markers and mechanic-specific markers are not included.

### Fixes and regression checks

`0.1.1` fixes remote cross-world party members incorrectly displaying Attack 1 outside duties.
Invalid entity IDs (`0`, `0xE0000000`) cannot match markers. Members without a valid local entity ID show no marker.

Run the matching regression checks using the same Hooks dev DLLs:

```powershell
dotnet run --project .\tests\MarkerMatcher.Tests.csproj -c Release -p:HooksDevPath="$env:APPDATA\XIVLauncher\addon\Hooks\dev"
```
