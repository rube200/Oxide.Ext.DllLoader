#region

using System.Linq;
using JetBrains.Annotations;
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Plugins;

#endregion

namespace Oxide.Ext.DllLoader
{
    [UsedImplicitly]
    public sealed class DllLoaderExtension : Extension
    {
        private DllLoader _pluginsLoader;

        public DllLoaderExtension(ExtensionManager manager) : base(manager)
        {
        }

        public override string Name => "DllLoader";
        public override string Author => "Rube200";
        public override VersionNumber Version => new VersionNumber(1, 0, 3);

        public override void Load()
        {
            _pluginsLoader = new DllLoader();
            Manager.RegisterPluginLoader(_pluginsLoader);
            Interface.Oxide.OnFrame(OnFrame);
        }

        public override void OnModLoad()
        {
            _pluginsLoader.OnModLoaded();
        }

        public override void OnShutdown()
        {
            _pluginsLoader.OnShutdown();
        }

        public override void LoadPluginWatchers(string pluginDirectory)
        {
            var dllWatcher = new DllLoaderWatcher(pluginDirectory);
            _pluginsLoader.SetWatcher(dllWatcher);
            Manager.RegisterPluginChangeWatcher(dllWatcher.Watcher);
        }

        private void OnFrame(float delta)
        {
            var args = new object[] {delta};
            foreach (var plugin in _pluginsLoader.LoadedPlugins.Values.OfType<CSharpPlugin>()
                .Where(pl => pl.HookedOnFrame)) plugin.CallHook("OnFrame", args);
        }
    }
}