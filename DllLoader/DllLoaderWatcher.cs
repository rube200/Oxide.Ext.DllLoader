#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins.Watchers;

#endregion

namespace Oxide.Ext.DllLoader
{
    public sealed class DllLoaderWatcher
    {
        public delegate void PluginsInFile(string filePath, ref HashSet<string> plugins);

        private readonly Dictionary<string, QueuedChange> _changeQueue;
        private readonly Timer _timers;
        internal FSWatcher Watcher;

        public DllLoaderWatcher(string pluginDirectory)
        {
            try
            {
                Watcher = new FSWatcher(pluginDirectory, "*.dll");
                var fileWatcher = GetFileWatcher.GetValue(Watcher) as FileSystemWatcher;
                var originalWatcherChanged =
                    (FileSystemEventHandler) Delegate.CreateDelegate(typeof(FileSystemEventHandler), Watcher,
                        GetWatcherChange);
                // ReSharper disable once PossibleNullReferenceException
                fileWatcher.Changed -= originalWatcherChanged;
                fileWatcher.Created -= originalWatcherChanged;
                fileWatcher.Deleted -= originalWatcherChanged;
                fileWatcher.Changed += OnFileChange;
                fileWatcher.Created += OnFileChange;
                fileWatcher.Deleted += OnFileChange;
            }
            catch (Exception ex)
            {
                Interface.Oxide.LogException("DllLoader fail to patch watcher", ex);
            }

            _changeQueue = new Dictionary<string, QueuedChange>();
            _timers = Interface.Oxide.GetLibrary<Timer>();
        }

        public event PluginsInFile OnGetPluginsInFile;

        private IEnumerable<string> GetPluginsInFile(string fileName)
        {
            var plugins = new HashSet<string>();

            OnGetPluginsInFile?.Invoke(fileName, ref plugins);

            return plugins;
        }

        private void OnFileChange(object sender, FileSystemEventArgs e)
        {
            var fileSystemWatcher = (FileSystemWatcher) sender;
            var length = e.FullPath.Length - fileSystemWatcher.Path.Length - Path.GetExtension(e.Name).Length - 1;
            var subPath = e.FullPath.Substring(fileSystemWatcher.Path.Length + 1, length);

            if (!_changeQueue.TryGetValue(subPath, out var change))
            {
                change = new QueuedChange();
                _changeQueue[subPath] = change;
            }

            change.OxideTimer?.Destroy();
            change.OxideTimer = null;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    change.ChangeType = change.ChangeType != WatcherChangeTypes.Deleted
                        ? WatcherChangeTypes.Created
                        : WatcherChangeTypes.Changed;
                    break;

                case WatcherChangeTypes.Deleted:
                    if (change.ChangeType == WatcherChangeTypes.Created)
                    {
                        _changeQueue.Remove(subPath);
                        return;
                    }

                    change.ChangeType = WatcherChangeTypes.Deleted;
                    break;

                case WatcherChangeTypes.Changed:
                    if (change.ChangeType != WatcherChangeTypes.Created) change.ChangeType = WatcherChangeTypes.Changed;
                    break;
            }

            Interface.Oxide.NextTick(() =>
            {
                change.OxideTimer?.Destroy();
                change.OxideTimer = _timers.Once(0.2f, () =>
                {
                    change.OxideTimer = null;
                    _changeQueue.Remove(subPath);

                    var pluginsName = GetPluginsInFile(subPath);
                    if (Regex.Match(subPath, "include\\\\", RegexOptions.IgnoreCase).Success)
                    {
                        if (change.ChangeType != WatcherChangeTypes.Created &&
                            change.ChangeType != WatcherChangeTypes.Changed)
                            return;

                        foreach (var plugin in pluginsName)
                            FirePluginSourceChangedMethod.Invoke(Watcher, new object[] {plugin});
                        return;
                    }

                    var watchedPlugins = GetWatchedPlugins.GetValue(Watcher) as ICollection<string>;
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (change.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                            foreach (var plugin in pluginsName)
                                FirePluginAddedMethod.Invoke(Watcher, new object[] {plugin});
                            break;

                        case WatcherChangeTypes.Deleted:
                            foreach (var plugin in pluginsName)
                            {
                                // ReSharper disable once PossibleNullReferenceException
                                if (!watchedPlugins.Contains(plugin))
                                    continue;

                                FirePluginRemovedMethod.Invoke(Watcher, new object[] {plugin});
                            }

                            break;

                        case WatcherChangeTypes.Changed:
                            foreach (var plugin in pluginsName)
                                // ReSharper disable once PossibleNullReferenceException
                                if (watchedPlugins.Contains(plugin))
                                    FirePluginSourceChangedMethod.Invoke(Watcher, new object[] {plugin});
                                else
                                    FirePluginAddedMethod.Invoke(Watcher, new object[] {plugin});
                            break;
                    }
                });
            });
        }

        #region Reflection

        public static readonly FieldInfo GetFileWatcher =
            typeof(FSWatcher).GetField("watcher",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly MethodInfo GetWatcherChange =
            typeof(FSWatcher).GetMethod("watcher_Changed",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly MethodInfo FirePluginAddedMethod =
            typeof(PluginChangeWatcher).GetMethod("FirePluginAdded",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly MethodInfo FirePluginRemovedMethod =
            typeof(PluginChangeWatcher).GetMethod("FirePluginRemoved",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly MethodInfo FirePluginSourceChangedMethod =
            typeof(PluginChangeWatcher).GetMethod("FirePluginSourceChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly FieldInfo GetWatchedPlugins =
            typeof(FSWatcher).GetField("watchedPlugins",
                BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion
    }
}