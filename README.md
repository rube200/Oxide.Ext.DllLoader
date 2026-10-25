# Oxide.Ext.DllLoader

An [Oxide/uMod](https://umod.org) extension that loads **precompiled plugin assemblies (`.dll`)** from the `oxide/plugins` folder, alongside the usual `.cs` plugins.

Instead of shipping C# source for Oxide to compile at runtime, you can build your plugins in your own IDE and project (with NuGet packages, shared libraries, multiple files, etc.) and drop the resulting `.dll` in the plugins folder.

## Features

- **Load `.dll` plugins**: every non-abstract class deriving from `Plugin` (e.g. `RustPlugin`, `CovalencePlugin`) in an assembly is registered as a plugin.
- **Multiple plugins per assembly**: one `.dll` can contain any number of plugins.
- **Hot reload**: adding, removing or replacing a `.dll` in the plugins folder loads, unloads or reloads every plugin in that assembly.
- **Dependency ordering**: plugins wait for the plugins they reference with `[PluginReference]` to load first. When a dependency unloads, its dependents unload too. Circular references don't deadlock.
- **Cross-assembly references**: a plugin assembly can reference another plugin assembly in the same folder, or a library in `oxide/extensions`.
- **Oxide hook patching**: assemblies get the same `DirectCallMethod` hook-dispatch patch that Oxide applies to plugins it compiles.
- **Normal Oxide integration**: `oxide.load`, `oxide.unload`, `oxide.reload`, `OnFrame` hooks and the plugin watcher all work as usual.

## Installation

1. Download the latest zip from [Releases](https://github.com/rube200/Oxide.Ext.DllLoader/releases). Pick the one that matches your server:
   - `DllLoader-Release-win-x64.zip` / `DllLoader-Release-linux-x64.zip`
   - The `Debug` builds log every step of the loader and are only useful for troubleshooting.
2. Extract `Oxide.Ext.DllLoader.dll` into your server's `RustDedicated_Data/Managed` folder (the folder that holds `Oxide.Core.dll`). Zips for v1.2.1 and earlier also contain copies of the Oxide and Harmony DLLs. Don't copy those, because they would overwrite your server's own files.
3. Restart the server.
4. Put your plugin `.dll` files in `oxide/plugins`.

## Writing a plugin

Create a `net48` class library that references the Oxide assemblies from your server (`Oxide.Core`, `Oxide.CSharp`, `Oxide.Rust`, ...), and write plugins the same way you would in a `.cs` file:

```csharp
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("MyPlugin", "You", "1.0.0")]
    [Description("A precompiled plugin")]
    public class MyPlugin : RustPlugin
    {
        // Loaded before MyPlugin, because the field is typed with the plugin's class
        [PluginReference]
        private OtherPlugin? OtherPlugin;

        private void Init()
        {
            Puts("Hello from a DLL!");
        }
    }
}
```

The plugin name is the class name, so it must be unique across all loaded plugins.

### Dependencies

The loader reads `[PluginReference]` fields and properties to decide load order. Only references **typed with the plugin's class** (e.g. `OtherPlugin? OtherPlugin`) count as dependencies. A reference declared as plain `Plugin` is still filled in by Oxide, but the loader doesn't wait for it.

Give the member the same name as the plugin class. The loader works out the dependency from the member's type, but Oxide fills the member by its name (or by `[PluginReference("Name")]`), so the two need to match.

If a dependency isn't found, the plugin fails to load and Oxide shows a `missing dependencies` error.

### Notes

- Files marked as hidden are ignored.
- Only the top level of `oxide/plugins` is scanned, not subfolders.
- Non-plugin libraries your plugins need can go in `oxide/plugins` or in the server's extensions folder; the loader resolves them from both.

## Building from source

Requirements: .NET SDK 8 (see [global.json](global.json)), which can build the `net48` target.

```bash
dotnet build DllLoader --configuration Release
```

The Oxide assemblies the project compiles against are in [Oxide.Libs](Oxide.Libs). Replace them with the ones from your server if you need to target a different Oxide version.

Releases are built by the [Github Release](.github/workflows/Release_Deploy.yml) workflow, which takes a version number and publishes Release and Debug zips for `win-x64` and `linux-x64`.

## Testing

The solution contains test projects that exercise load order, circular references, deep dependency chains and cross-assembly references:

| Project | Contents |
| --- | --- |
| `DllLoader.Test.Assembly1` | Compiled test plugins, referencing each other and `Assembly2` |
| `DllLoader.Test.Assembly2` | Shared base classes and plugins that `Assembly1` depends on |
| `DllLoader.Test.Uncompiled` | `.cs` plugins that reference the compiled ones (and the other way round) |
| `DllLoader.Tester` | `TesterPlugin`, which runs the automated test |

To run it, copy the built test `.dll` files, the `DllLoader.Test.Uncompiled` `.cs` files and the `DllLoader.Tester` `.dll` into `oxide/plugins`, then run this in the server console:

```
dllloader.test
```

The tester checks the plugin count, then randomly unloads, loads and reloads plugins three times, checking that every plugin's references are still valid.

## License

[GPL-3.0](LICENSE)
