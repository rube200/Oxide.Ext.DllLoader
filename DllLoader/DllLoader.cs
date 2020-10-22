#region

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Plugins;

#endregion

namespace Oxide.Ext.DllLoader
{
    [UsedImplicitly]
    public sealed class DllLoader : PluginLoader
    {
        private readonly DllLoaderMapper _dllLoaderMapper;
        private DllLoaderWatcher _watcher;

        public DllLoader()
        {
            _dllLoaderMapper = new DllLoaderMapper(this);
        }

        public override string FileExtension => ".dll";

        internal void OnModLoaded()
        {
            _dllLoaderMapper.OnModLoaded();
        }

        internal void OnShutdown()
        {
            _dllLoaderMapper.OnShutdown();
        }

        public override IEnumerable<string> ScanDirectory(string directory)
        {
            try
            {
                LogDebug($"ScanDirectory({directory})");
                var plugins = _dllLoaderMapper.ScanAndMapPluginsDir(directory);
                LogDebug($"Return {plugins}; Count: {plugins.Count}");
                return plugins;
            }
            catch (Exception ex)
            {
                Interface.Oxide.LogException($"Fail to scan directory: {directory}", ex);
                return new List<string>();
            }
        }

        public override Plugin Load(string directory, string name)
        {
            try
            {
                LogDebug($"Load({directory}, {name})");
                if (LoadingPlugins.Contains(name))
                {
                    LogDebug("Return null; (plugin already loading)");
                    return null;
                }

                var plugin = GetPlugin(name);
                if (plugin == null)
                {
                    LogDebug("Return null; (plugin not found)");
                    return null;
                }

                LoadingPlugins.Add(plugin.Name);
                Interface.Oxide.NextTick(() => LoadPlugin(plugin));
                LogDebug("Return null; (load on next tick)");
                return null;
            }
            catch (Exception ex)
            {
                Interface.Oxide.LogException($"Fail to load plugin: {name}", ex);
                return null;
            }
        }

        protected override Plugin GetPlugin(string name)
        {
            try
            {
                LogDebug($"GetPlugin({name})");
                var pluginType = _dllLoaderMapper.GetPluginTypeByName(name);
                if (pluginType is null)
                {
                    LogDebug("Return null; (type not found)");
                    return null;
                }

                if (!(Activator.CreateInstance(pluginType) is Plugin plugin))
                {
                    LogDebug("Return null; (invalid plugin type)");
                    return null;
                }

                if (!(plugin is CSharpPlugin csharpPlugin))
                {
                    LogDebug($"Return {plugin}; (not CSharpPlugin)");
                    return plugin;
                }

                var pluginPath = _dllLoaderMapper.GetPluginPathByName(name);
                csharpPlugin.SetPluginInfo(name, pluginPath);
                csharpPlugin.Watcher = _watcher.Watcher;

                LogDebug($"Return {plugin}; (CSharpPlugin)");
                return plugin;
            }
            catch (Exception ex)
            {
                Interface.Oxide.LogException($"Fail to get plugin: {name}", ex);
                return null;
            }
        }

        #region Debug

        public static readonly bool Debug = false;

        internal static void LogDebug(string message)
        {
            if (!Debug)
                return;

            Interface.Oxide.LogDebug(message);
        }

        #endregion

        #region Watcher

        public void SetWatcher(DllLoaderWatcher dllWatcher)
        {
            LogDebug($"SetWatcher({dllWatcher})");
            if (_watcher != null)
                _watcher.OnGetPluginsInFile -= OnGetPluginsInFile;

            _watcher = dllWatcher;
            _watcher.OnGetPluginsInFile += OnGetPluginsInFile;
        }

        private void OnGetPluginsInFile(string filepath, HashSet<string> plugins)
        {
            LogDebug($"OnGetPluginsInFile({filepath}, {plugins}) Count: {plugins.Count}");
            plugins.UnionWith(_dllLoaderMapper.ScanAndMapPluginsFile(filepath));
        }

        #endregion
    }
}