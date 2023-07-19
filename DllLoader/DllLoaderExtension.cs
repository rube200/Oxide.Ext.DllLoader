#region

using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Plugins.Watchers;
using Oxide.Ext.DllLoader.Manager;

#endregion

namespace Oxide.Ext.DllLoader
{
    // ReSharper disable once UnusedMember.Global
    public sealed class DllLoaderExtension : Extension
    {
        public override string Name => "DllLoader";
        public override string Author => "Rube200";
        public override VersionNumber Version => new VersionNumber(1, 1, 1);
        public FSWatcher Watcher { get; private set; }


        private DllLoaderPluginManager _pluginsLoader;


        public DllLoaderExtension(ExtensionManager manager) : base(manager)
        {
            _pluginsLoader = new DllLoaderPluginManager(this);
        }
        ~DllLoaderExtension()
        {
            _pluginsLoader = null;
        }


        public override void Load()
        {
            Manager.RegisterPluginLoader(_pluginsLoader);

            Interface.Oxide.OnFrame(OnFrame);
        }

        public override void LoadPluginWatchers(string pluginDirectory)
        {
            Watcher = new FSWatcher(pluginDirectory, "*.dll");
            Manager.RegisterPluginChangeWatcher(Watcher);
        }


        public override void OnModLoad()
        {
            _pluginsLoader.OnModLoad();
        }

        public override void OnShutdown()
        {
            _pluginsLoader.OnShutdown();
        }

        private void OnFrame(float delta)
        {
            foreach (var plugin in _pluginsLoader.OnFramePlugins)
                plugin.CallHook("OnFrame", new [] { delta });
        }
    }
}