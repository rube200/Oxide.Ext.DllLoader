#region

using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Oxide.Core;
using Oxide.Core.Plugins.Watchers;
using Oxide.Ext.DllLoader.API;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.Watcher
{
    [HarmonyPatch(typeof(FSWatcher))]
    internal static class DllLoaderWatcherFix
    {
        private static readonly Dictionary<PluginChangeWatcher, IDllLoaderMapper> _fsWatcherMappers = [];
        private static readonly Dictionary<string, byte> _mappingCount = [];

        public static void AddWatcher(PluginChangeWatcher watcher, IDllLoaderMapper mapper)
        {
            _fsWatcherMappers[watcher] = mapper;
        }

        public static void RemoveWatcher(PluginChangeWatcher watcher)
        {
            _fsWatcherMappers.Remove(watcher);
        }

        #region AddMappingPatch

        [HarmonyPatch(nameof(FSWatcher.AddMapping))]
        [HarmonyPrefix]
        private static bool WatcherAddMapping(FSWatcher __instance, string name, ICollection<string> ___watchedPlugins)
        {
            var assemblyInfo = GetAssemblyFromPluginName(__instance, name);
            if (assemblyInfo == null)
                return true;

            var filename = Path.GetFileNameWithoutExtension(assemblyInfo.AssemblyFile);
            _mappingCount[filename] = _mappingCount.TryGetValue(filename, out var count) ? ++count : (byte)1;
            ___watchedPlugins.Add(filename);
            return false;
        }

        #endregion

        #region RemoveMappingPatch

        [HarmonyPatch(nameof(FSWatcher.RemoveMapping))]
        [HarmonyPrefix]
        private static bool WatcherRemoveMapping(FSWatcher __instance, string name, ICollection<string> ___watchedPlugins)
        {
            var assemblyInfo = GetAssemblyFromPluginName(__instance, name);
            if (assemblyInfo == null)
                return true;

            var filename = Path.GetFileNameWithoutExtension(assemblyInfo.AssemblyFile);
            var count = _mappingCount[filename];
            if (count == 1)
            {
                _mappingCount[filename] = (byte)(count - 1);
                ___watchedPlugins.Remove(filename);
            }

            return false;
        }

        #endregion

        #region CommonMappingPatch

        private static AssemblyInfo? GetAssemblyFromPluginName(PluginChangeWatcher watcher, string pluginName)
        {
            if (!_fsWatcherMappers.TryGetValue(watcher, out var mapper))
            {
#if DEBUG
                Interface.Oxide.LogDebug("No corresponding watcher found({0}), ignoring...", pluginName);
#endif
                return null;
            }

            var assemblyInfo = mapper.GetAssemblyInfoByPlugin(pluginName);
            if (assemblyInfo == null)
            {
                Interface.Oxide.LogError("Fail to find assembly info for plugin({0}).", pluginName);
                return null;
            }

            return assemblyInfo;
        }

        #endregion
    }
}