# Party Marker Party List

[中文](#中文) | [English](#english)

## 中文

这是一个 Dalamud 插件，用于把队伍成员头顶的原生目标标记同步显示在小队列表中：

```text
[头顶标记] [职业图标] [玩家名]
```

### 功能

- 从 `MarkingController.Markers` 读取当前角色头顶的原生目标标记及目标实体 ID。
- 从游戏 `Marker` 表读取原生图标，显示攻击、止步/锁链、禁止和几何标记。
- 通过实体 ID 将标记与小队列表中的实际队伍成员行对应起来，支持亲信战友 / 剧情辅助器小队。
- 默认在职业图标左侧绘制标记，尺寸跟随 HUD 布局缩放，透明度跟随小队列表。
- 覆盖层不接收输入，不改写原生 UI 节点，也不会盖在其它插件窗口上面。
- 设置窗口可以调整显示位置（左侧/右侧）、大小和偏移，设置会持久保存。

### 命令

| 命令 | 作用 |
| --- | --- |
| `/partymarkers` | 切换显示 |
| `/partymarkers on` / `off` | 开启 / 关闭显示 |
| `/partymarkers config` | 打开设置窗口（也可以在插件安装器中点设置按钮） |

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

`0.2.0` 修复了调整 HUD 布局中小队列表大小后，标记尺寸与职业图标不一致的问题。

`0.1.1` 修复了跨服小队在副本外，远端成员被误显示为“攻击 1”的问题。
无效实体 ID（`0`、`0xE0000000`）不会匹配标记；没有有效本地实体 ID 的成员不显示标记。

使用同一套 Hooks dev DLL 运行匹配逻辑回归检查：

```powershell
dotnet test .\tests\MarkerMatcher.Tests.csproj -c Release -p:HooksDevPath="$env:APPDATA\XIVLauncher\addon\Hooks\dev"
```

## English

Party Marker Party List is a Dalamud plugin that mirrors native target markers above party members' heads
in the party list:

```text
[target marker] [job icon] [player name]
```

### Features

- Reads native target-marker slots and target entity IDs from `MarkingController.Markers`.
- Reads native icons from the game's `Marker` sheet for attack, bind/stop, ignore, and shape markers.
- Matches markers to the correct party-list row by entity ID, including trust / duty-support parties.
- Draws each marker left of the job icon by default, following the HUD layout scale and the party list's opacity.
- Uses a non-interactive overlay that does not modify native UI nodes and stays below other plugin windows.
- A settings window adjusts side (left/right), size, and offset; settings are persisted.

### Commands

| Command | Action |
| --- | --- |
| `/partymarkers` | Toggle the overlay |
| `/partymarkers on` / `off` | Enable / disable the overlay |
| `/partymarkers config` | Open settings (also available from the plugin installer) |

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
`InteropGenerator.Runtime.dll`, `Lumina.dll`, and `Lumina.Excel.dll`. Override the location with `-p:HooksDevPath="..."` when necessary:

```powershell
dotnet build .\PartyMarkerPartyList.csproj -c Debug -p:HooksDevPath="$env:APPDATA\XIVLauncher\addon\Hooks\dev"
```

The Dalamud and FFXIVClientStructs source checkouts are used only for API and structure-layout reference.

The plugin currently supports native target markers above characters' heads: Attack 1–8, Bind/Stop 1–3,
Ignore 1–2, Square, Circle, Plus, and Triangle. Field markers and mechanic-specific markers are not included.

### Fixes and regression checks

`0.2.0` fixes marker size not following the party list's HUD layout scale.

`0.1.1` fixes remote cross-world party members incorrectly displaying Attack 1 outside duties.
Invalid entity IDs (`0`, `0xE0000000`) cannot match markers. Members without a valid local entity ID show no marker.

Run the matching regression checks using the same Hooks dev DLLs:

```powershell
dotnet test .\tests\MarkerMatcher.Tests.csproj -c Release -p:HooksDevPath="$env:APPDATA\XIVLauncher\addon\Hooks\dev"
```
