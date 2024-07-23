#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Oxide.Core;
using Oxide.Core.Plugins.Watchers;
using Oxide.Ext.DllLoader.API;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.Watcher
{
    internal class DllLoaderWatcherFix
    {
        private static readonly IDictionary<PluginChangeWatcher, IDllLoaderMapper> _fsWatcherMappers =
            new Dictionary<PluginChangeWatcher, IDllLoaderMapper>();

        private Harmony? _harmonyInstance;
        private FSWatcher? _watcher;

        public DllLoaderWatcherFix(string name, FSWatcher fsWatcher, IDllLoaderMapper dllMapper)
        {
            _harmonyInstance = new Harmony(name);

            //Plugin add/remove to/from watcher
            _harmonyInstance.Patch(WatcherAddMappingInfo, WatcherAddMappingPatchInfo);
            _harmonyInstance.Patch(WatcherRemoveMappingInfo, WatcherRemoveMappingPatchInfo);

            //Watcher fire events
            _harmonyInstance.Patch(FirePluginAddedInfo, FirePluginAddedPatchInfo);
            _harmonyInstance.Patch(FirePluginRemovedInfo, FirePluginRemovedPatchInfo);
            _harmonyInstance.Patch(FirePluginSourceChangedInfo, FirePluginSourceChangedPatchInfo);

            _watcher = fsWatcher;
            _fsWatcherMappers[_watcher] = dllMapper;
        }

        ~DllLoaderWatcherFix()
        {
            _fsWatcherMappers.Remove(_watcher!);
            _watcher = null;

            _harmonyInstance!.UnpatchAll(_harmonyInstance.Id);
            _harmonyInstance = null;
        }

        #region MethodsInfo

        //Plugin add/remove to/from watcher
        private static readonly MethodInfo WatcherAddMappingInfo;
        private static readonly MethodInfo WatcherRemoveMappingInfo;
        private static readonly HarmonyMethod WatcherAddMappingPatchInfo;
        private static readonly HarmonyMethod WatcherRemoveMappingPatchInfo;

        //Watcher fire events
        private static readonly MethodInfo FirePluginAddedInfo;
        private static readonly MethodInfo FirePluginRemovedInfo;
        private static readonly MethodInfo FirePluginSourceChangedInfo;

        private static readonly HarmonyMethod FirePluginAddedPatchInfo;
        private static readonly HarmonyMethod FirePluginRemovedPatchInfo;
        private static readonly HarmonyMethod FirePluginSourceChangedPatchInfo;

        private static readonly FieldInfo OnPluginAddedEvent;
        private static readonly FieldInfo OnPluginRemovedEvent;
        private static readonly FieldInfo OnPluginSourceChangedEvent;


        static DllLoaderWatcherFix()
        {
            var fsWatcherType = typeof(FSWatcher);
            WatcherAddMappingInfo = fsWatcherType.GetMethod(nameof(FSWatcher.AddMapping));
            WatcherRemoveMappingInfo = fsWatcherType.GetMethod(nameof(FSWatcher.RemoveMapping));

            var dllWatcherFixType = typeof(DllLoaderWatcherFix);
            const BindingFlags staticPrivateBinds = BindingFlags.Static | BindingFlags.NonPublic;
            WatcherAddMappingPatchInfo =
                new HarmonyMethod(dllWatcherFixType.GetMethod(nameof(WatcherAddMapping), staticPrivateBinds));
            WatcherRemoveMappingPatchInfo =
                new HarmonyMethod(dllWatcherFixType.GetMethod(nameof(WatcherRemoveMapping), staticPrivateBinds));


            //Watcher fire events
            const BindingFlags instancePrivateBinds = BindingFlags.Instance | BindingFlags.NonPublic;
            var pluginChangeWatcherType = typeof(PluginChangeWatcher);
            FirePluginAddedInfo = pluginChangeWatcherType.GetMethod("FirePluginAdded", instancePrivateBinds);
            FirePluginRemovedInfo = pluginChangeWatcherType.GetMethod("FirePluginRemoved", instancePrivateBinds);
            FirePluginSourceChangedInfo =
                pluginChangeWatcherType.GetMethod("FirePluginSourceChanged", instancePrivateBinds);

            FirePluginAddedPatchInfo =
                new HarmonyMethod(dllWatcherFixType.GetMethod(nameof(FirePluginAdded), staticPrivateBinds));
            FirePluginRemovedPatchInfo =
                new HarmonyMethod(dllWatcherFixType.GetMethod(nameof(FirePluginRemoved), staticPrivateBinds));
            FirePluginSourceChangedPatchInfo =
                new HarmonyMethod(dllWatcherFixType.GetMethod(nameof(FirePluginSourceChanged), staticPrivateBinds));

            OnPluginAddedEvent =
                pluginChangeWatcherType.GetField(nameof(PluginChangeWatcher.OnPluginAdded), instancePrivateBinds);
            OnPluginRemovedEvent =
                pluginChangeWatcherType.GetField(nameof(PluginChangeWatcher.OnPluginRemoved), instancePrivateBinds);
            OnPluginSourceChangedEvent =
                pluginChangeWatcherType.GetField(nameof(PluginChangeWatcher.OnPluginSourceChanged),
                    instancePrivateBinds);
        }

        #endregion

        #region PluginWatcherColletion

        private static bool MapPluginNameToAssembly(PluginChangeWatcher __instance, string pluginName,
            Action<AssemblyInfo> action)
        {
            if (!_fsWatcherMappers.TryGetValue(__instance, out var mapper))
            {
#if DEBUG
                Interface.Oxide.LogDebug("No corresponding watcher found({0}), ignoring...", pluginName);
#endif
                return true;
            }

            var assemblyInfo = mapper.GetAssemblyInfoByPluginName(pluginName);
            if (assemblyInfo == null)
            {
                Interface.Oxide.LogError("Fail to find assembly info for plugin({0}).", pluginName);
                return true;
            }

            action(assemblyInfo);
            return false;
        }

        //Since ___watchedPlugins is HashSet(FSWatcher source code) we do not need to check if is in collection
        // ReSharper disable SuggestBaseTypeForParameter
        private static bool WatcherAddMapping(FSWatcher __instance, string name, ICollection<string> ___watchedPlugins)
        {
            return MapPluginNameToAssembly(__instance, name, assemblyInfo =>
            {
                var filename = Path.GetFileNameWithoutExtension(assemblyInfo.AssemblyFile);
                ___watchedPlugins.Add(filename);
            });
        }

        private static bool WatcherRemoveMapping(FSWatcher __instance, string name,
            ICollection<string> ___watchedPlugins)
        {
            return MapPluginNameToAssembly(__instance, name, assemblyInfo =>
            {
                var filename = Path.GetFileNameWithoutExtension(assemblyInfo.AssemblyFile);
                ___watchedPlugins.Remove(filename);
            });
        }
        // ReSharper restore SuggestBaseTypeForParameter

        #endregion

        #region WatcherFireEvents

        private static bool FirePluginAdded(PluginChangeWatcher __instance, string name)
        {
            return MapAssemblyNameToEvent(__instance, name, OnPluginAddedEvent);
        }

        private static bool FirePluginRemoved(PluginChangeWatcher __instance, string name)
        {
            return MapAssemblyNameToEvent(__instance, name, OnPluginRemovedEvent);
        }

        private static bool FirePluginSourceChanged(PluginChangeWatcher __instance, string name)
        {
            return MapAssemblyNameToEvent(__instance, name, OnPluginSourceChangedEvent);
        }

        private static bool MapAssemblyNameToEvent(PluginChangeWatcher __instance, string assemblyName,
            FieldInfo fieldInfo)
        {
            if (!_fsWatcherMappers.TryGetValue(__instance, out var mapper))
            {
                Interface.Oxide.LogDebug("No corresponding watcher found, ignoring...");
                return true;
            }

            mapper.ScanDirectoryPlugins(Interface.Oxide.PluginDirectory);
            var assemblyInfo = mapper.GetAssemblyInfoByFile(assemblyName);
            if (assemblyInfo == null)
            {
                Interface.Oxide.LogError("Fail to find assembly info for ({0}).", assemblyName);
                return true;
            }

            var eventDelegate = (Delegate)fieldInfo.GetValue(__instance);
            foreach (var pluginName in assemblyInfo.PluginsName)
                Raise(pluginName, eventDelegate);

            return false;
        }

        private static void Raise(string pluginName, Delegate eventDelegate)
        {
            foreach (var handler in eventDelegate.GetInvocationList())
                handler.Method.Invoke(handler.Target, new object[] { pluginName });
        }

        #endregion
    }
}