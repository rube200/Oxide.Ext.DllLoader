#region

using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Plugins.Watchers;
using Oxide.Ext.DllLoader.Controller;

#endregion

namespace Oxide.Ext.DllLoader
{
    // ReSharper disable once UnusedMember.Global
    //todo implement last write check
    public sealed class DllLoaderExtension : Extension
    {
        private DllPluginLoaderController _pluginLoader;


        public DllLoaderExtension(ExtensionManager manager) : base(manager)
        {
            _pluginLoader = new DllPluginLoaderController(this);
        }

        public override string Name => "DllLoader";
        public override string Author => "Rube200";
        public override VersionNumber Version => new VersionNumber(1, 1, 2);
        public FSWatcher Watcher { get; private set; }

        ~DllLoaderExtension()
        {
            _pluginLoader = null;
        }


        public override void Load()
        {
            Manager.RegisterPluginLoader(_pluginLoader);

            Interface.Oxide.OnFrame(OnFrame);
        }

        public override void LoadPluginWatchers(string pluginDirectory)
        {
            Watcher = new FSWatcher(pluginDirectory, "*.dll");
            Manager.RegisterPluginChangeWatcher(Watcher);

            /*
            var dllWatcher = new DllLoaderWatcher(pluginDirectory);
            _pluginsLoader.SetWatcher(dllWatcher);


            Manager.RegisterPluginChangeWatcher(dllWatcher.Watcher);*/
        }


        public override void OnModLoad()
        {
            _pluginLoader.OnModLoad();
        }

        public override void OnShutdown()
        {
            _pluginLoader.OnShutdown();
        }

        private void OnFrame(float delta)
        {
            foreach (var plugin in _pluginLoader.OnFramePlugins)
                plugin.CallHook("OnFrame", new[] { delta });
        }
    }
}