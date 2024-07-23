#region

using System;
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Logging;
using Oxide.Core.Plugins.Watchers;
using Oxide.Ext.DllLoader.Controller;
using Oxide.Ext.DllLoader.Watcher;

#endregion

namespace Oxide.Ext.DllLoader
{
    // ReSharper disable once UnusedMember.Global
    public sealed class DllLoaderExtension : Extension
    {
        private DllPluginLoaderController? _pluginLoader;
#pragma warning disable IDE0052
        // ReSharper disable once NotAccessedField.Local
        private DllLoaderWatcherFix? _watcherFix; //Need to make GC not collect it when it shouldn't
#pragma warning restore IDE0052

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
            _watcherFix = null;
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
            try
            {
                _watcherFix = new DllLoaderWatcherFix(Name, Watcher, _pluginLoader!._mapper);
            }
            catch (Exception ex)
            {
                Interface.Oxide.LogException("Fail to file watcher, it will not work properly", ex);
                return;
            }

            Manager.RegisterPluginChangeWatcher(Watcher);
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
    }
}