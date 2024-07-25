#region

using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Plugins.Watchers;
using Oxide.Ext.DllLoader.Controller;
using Oxide.Ext.DllLoader.Watcher;

#endregion

namespace Oxide.Ext.DllLoader
{
    // ReSharper disable once UnusedMember.Global
    public sealed class DllLoaderExtension : Extension
    {
        private readonly DllPluginLoaderController _pluginLoader;

        public DllLoaderExtension(ExtensionManager manager) : base(manager)
        {
            _pluginLoader = new DllPluginLoaderController(this);
        }

        public override string Name => "DllLoader";
        public override string Author => "Rube200";
        public override VersionNumber Version => new(1, 3, 1);
        public FSWatcher? Watcher { get; private set; }

        ~DllLoaderExtension()
        {
            Watcher!.OnPluginSourceChanged -= OnPluginSourceChanged;
            Watcher.OnPluginRemoved -= OnPluginRemoved;
            Watcher.OnPluginAdded -= OnPluginAdded;
            DllLoaderWatcherFix.RemoveWatcher(Watcher);
        }


        public override void Load()
        {
            Manager.RegisterPluginLoader(_pluginLoader);
            Interface.Oxide.OnFrame(OnFrame);
        }

        public override void LoadPluginWatchers(string pluginDirectory)
        {
            Watcher = new FSWatcher(pluginDirectory, "*.dll");
            DllLoaderWatcherFix.AddWatcher(Watcher, _pluginLoader!._mapper);

            Watcher.OnPluginAdded += OnPluginAdded;
            Watcher.OnPluginRemoved += OnPluginRemoved;
            Watcher.OnPluginSourceChanged += OnPluginSourceChanged;
        }

        public override void OnModLoad()
        {
            _pluginLoader!.OnModLoad();
        }

        public override void OnShutdown()
        {
            _pluginLoader!.OnShutdown();
        }

        private void OnFrame(float delta)
        {
            if (_pluginLoader == null)
                return;

            foreach (var plugin in _pluginLoader.OnFramePlugins)
                plugin.CallHook(nameof(OnFrame), delta);
        }

        private void OnPluginAdded(string assemblyName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Assembly({0}) added, changed! Loading plugins...", assemblyName);
#endif
            _pluginLoader.AddAssembly(assemblyName);
        }

        private void OnPluginRemoved(string assemblyName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Assembly({0}) removed. Unloading plugins...", assemblyName);
#endif
            _pluginLoader.RemoveAssembly(assemblyName);
        }

        private void OnPluginSourceChanged(string assemblyName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Assembly({0}) changed! Reloading...", assemblyName);
#endif
            _pluginLoader.ReloadAssembly(assemblyName);
        }
    }
}