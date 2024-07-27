#region

using HarmonyLib;
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Plugins.Watchers;
using Oxide.Ext.DllLoader.Controller;
using Oxide.Ext.DllLoader.Model;
using Oxide.Ext.DllLoader.Watcher;
using System;

#endregion

namespace Oxide.Ext.DllLoader
{
    // ReSharper disable once UnusedMember.Global
    public sealed class DllLoaderExtension : Extension
    {
        public const string ProjectName = "DllLoader";
        private static readonly Harmony harmony = new(ProjectName);

        private readonly DllPluginLoaderController _pluginLoader;

        public DllLoaderExtension(ExtensionManager manager) : base(manager)
        {
            _pluginLoader = new DllPluginLoaderController(this);
        }

        public override string Name => ProjectName;
        public override string Author => "Rube200";
        public override VersionNumber Version => new(1, 3, 1);
        public FSWatcher? Watcher { get; private set; }

        public override void Load()
        {
            Manager.RegisterPluginLoader(_pluginLoader);
            Interface.Oxide.OnFrame(OnFrame);
        }

        public override void LoadPluginWatchers(string pluginDirectory)
        {
            Watcher = new FSWatcher(pluginDirectory, "*.dll");
            DllLoaderWatcherFix.AddWatcher(Watcher, _pluginLoader!.Mapper);

            Watcher.OnPluginAdded += OnPluginAdded;
            Watcher.OnPluginRemoved += OnPluginRemoved;
            Watcher.OnPluginSourceChanged += OnPluginSourceChanged;
        }

        public override void OnModLoad()
        {
            harmony.PatchAll();
            _pluginLoader!.OnModLoad();
        }

        public override void OnShutdown()
        {
            Watcher!.OnPluginSourceChanged -= OnPluginSourceChanged;
            Watcher.OnPluginRemoved -= OnPluginRemoved;
            Watcher.OnPluginAdded -= OnPluginAdded;

            DllLoaderWatcherFix.RemoveWatcher(Watcher);
            Watcher = null;

            _pluginLoader!.OnShutdown();
            harmony.UnpatchAll(harmony.Id);
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
            OnPluginAdded(assemblyName, "load");
        }

        private void OnPluginAdded(string assemblyName, string eventName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Assembly({0}) file added. Loading plugins...", assemblyName);
#endif
            _pluginLoader.Mapper.ScanAndRegisterAssemblies(Interface.Oxide.PluginDirectory);
            ExecuteAssemblyEvent(eventName, assemblyName, Interface.Oxide.LoadPlugin);
        }

        private void OnPluginRemoved(string assemblyName)
        {
            OnPluginRemoved(assemblyName, "unload");
        }

        private void OnPluginRemoved(string assemblyName, string eventName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Assembly({0}) file removed. Unloading plugins...", assemblyName);
#endif
            ExecuteAssemblyEvent(eventName, assemblyName, Interface.Oxide.UnloadPlugin, assemblyInfo => assemblyInfo.MarkDirty());
        }

        private void OnPluginSourceChanged(string assemblyName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Assembly({0}) fille changed! Reloading plugins...", assemblyName);
#endif
            OnPluginRemoved(assemblyName, "unload");
            OnPluginAdded(assemblyName, "load");
        }

        private void ExecuteAssemblyEvent(string eventName, string assemblyName, Func<string, bool> eventAction, Action<AssemblyInfo>? afterAction = null)
        {
            var assemblyInfo = _pluginLoader.Mapper.GetAssemblyInfoByFilename(assemblyName);
            if (assemblyInfo == null)
            {
                Interface.Oxide.LogError("Fail to {0} assembly from file({1}.dll)", eventName, assemblyName);
                return;
            }

            foreach (var pluginName in assemblyInfo.PluginsName)
                eventAction(pluginName);

            afterAction?.Invoke(assemblyInfo);
        }
    }
}