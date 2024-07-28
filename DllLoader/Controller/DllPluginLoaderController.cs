#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Ext.DllLoader.API;
using Oxide.Ext.DllLoader.Mapper;
using Oxide.Ext.DllLoader.Model;
using CSharpPlugin = Oxide.Plugins.CSharpPlugin;

#endregion

namespace Oxide.Ext.DllLoader.Controller
{
    public sealed class DllPluginLoaderController(DllLoaderExtension extension) : PluginLoader
    {
        private readonly IDllLoaderMapperLoadable _mapper = new DllLoaderMapper();
        private readonly List<Plugin> _onFramePlugins = [];
        private readonly Dictionary<string, PluginInfo> _pluginsWaitingDep = [];

        public IDllLoaderMapper Mapper => _mapper;
        public IReadOnlyCollection<Plugin> OnFramePlugins => _onFramePlugins;


        public void OnModLoad()
        {
            _mapper.OnModLoad();
        }

        public void OnShutdown()
        {
            _pluginsWaitingDep.Clear();
            _onFramePlugins.Clear();
            _mapper.OnShutdown();
        }


        #region Override

        public override string FileExtension => ".dll";

        public override IEnumerable<string> ScanDirectory(string directory)
        {
            return Mapper.ScanDirectoryPlugins(directory);
        }

        public override Plugin? Load(string _, string pluginName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Loading requested to plugin({0}).", pluginName);
#endif
            if (!IsPluginLoadingOrLoaded(pluginName))
                return null;

            LoadingPlugins.Add(pluginName);

#if DEBUG
            Interface.Oxide.LogDebug("Getting assembly info for plugin({0}).", pluginName);
#endif
            var assemblyInfo = Mapper.GetAssemblyInfoByPlugin(pluginName);
            if (assemblyInfo == null)
            {
                LoadingPlugins.Remove(pluginName);
                Interface.Oxide.LogError("Fail to find assembly info for plugin({0}).", pluginName);
                PluginErrors[pluginName] = $"Fail to find assembly info";
                return null;
            }

#if DEBUG
            Interface.Oxide.LogDebug("Plugin({0}) assembly({1}) info found.", pluginName, assemblyInfo.OriginalName);
#endif

            var pluginInfo = assemblyInfo.GetPluginInfo(pluginName);
            if (pluginInfo == null)
            {
                LoadingPlugins.Remove(pluginName);
                Interface.Oxide.LogError("Fail to find plugin({0}) in assembly({1}).", pluginName, assemblyInfo.OriginalName);
                PluginErrors[pluginName] = $"Fail to find plugin info.";
                return null;
            }

#if DEBUG
            Interface.Oxide.LogDebug("Checking plugin({0}) in dependencies in the next tick.", pluginName);
#endif
            Interface.Oxide.NextTick(() => LoadPlugin(pluginInfo));
            return null;
        }

        public override void Unloading(Plugin plugin)
        {
            LoadedPlugins.Remove(plugin.Name);

            var assemblyInfo = Mapper.GetAssemblyInfoByPlugin(plugin.Name);
            var pluginInfo = assemblyInfo?.GetPluginInfo(plugin.Name);
            if (pluginInfo == null)
            {
                Interface.Oxide.LogError("Plugin({0}) could not be unloaded correctly.", plugin.Name);
                return;
            }

            pluginInfo.Dispose();
            foreach (var depPlugin in _mapper.GetAllPlugins().Where(p => p.IsPluginLoaded && p.PluginReferences.Contains(plugin.Name)))
                Interface.Oxide.UnloadPlugin(depPlugin.PluginName);
        }

        #endregion


        private bool IsPluginLoadingOrLoaded(string pluginName)
        {
            if (LoadingPlugins.Contains(pluginName))
            {
#if DEBUG
                Interface.Oxide.LogDebug("Load requested failed, plugin({0}) is already loading.", pluginName);
#endif
                return false;
            }

            if (LoadedPlugins.ContainsKey(pluginName))
            {
#if DEBUG
                Interface.Oxide.LogDebug("Load requested failed, plugin({0}) is already loaded.", pluginName);
#endif
                return false;
            }

            return true;
        }

        private bool LoadPlugin(PluginInfo pluginInfo)
        {
            var pluginName = pluginInfo.PluginName;
            var referencesNotLoaded = pluginInfo.PluginReferences.Where(p => !LoadedPlugins.ContainsKey(p));
            if (referencesNotLoaded.Any())
            {
                //var referencesLoading = pluginInfo.PluginReferences.Where(p => LoadingPlugins.Contains(p) && !_pluginsWaitingDep.Contains(p));
                var missingReferences = referencesNotLoaded.Where(p => !LoadingPlugins.Contains(p));
                if (missingReferences.Any())
                {
                    PluginErrors[pluginName] = $"Fail to load plugin({pluginName}) missing dependencies: {missingReferences.ToSentence()}";
                    Interface.Oxide.LogError("Fail to load plugin({0}) missing dependencies: {1}", pluginName, missingReferences.ToSentence());
                    return false;
                }
                
                if (referencesNotLoaded.Any(p => !_pluginsWaitingDep.ContainsKey(p)))
                {
#if DEBUG
                    Interface.Oxide.LogDebug("Plugin({0}) is waiting for dependencies to load: {1}", pluginName, referencesNotLoaded.ToSentence());
#endif
                    _pluginsWaitingDep.Add(pluginName, pluginInfo);
                    return false;
                }
            }

            if (pluginInfo.IsPluginLoaded)
            {
#if DEBUG
                Interface.Oxide.LogDebug("Plugin({0}) is already loaded.", pluginName);
#endif
                LoadedPlugins[pluginName] = pluginInfo.Plugin;
                PluginLoaded(pluginName);
                return true;
            }

#if DEBUG
            Interface.Oxide.LogDebug("Loading plugin({0}).", pluginName);
#endif
            Interface.Oxide.UnloadPlugin(pluginName);
            LoadedPlugins[pluginName] = InstantiatePlugin(pluginInfo);

            Interface.Oxide.LogInfo("Plugin({0}) loaded successfully.", pluginName);
            PluginLoaded(pluginName);
            return true;
        }

        private void PluginLoaded(string loadedPluginName)
        {
            LoadingPlugins.Remove(loadedPluginName);

#if DEBUG
            Interface.Oxide.LogDebug("Checking dependents to load...");
#endif
            foreach (var toLoadPluginName in _pluginsWaitingDep.Keys.ToArray())
            {
                //we need this check since we copied the list
                if (_pluginsWaitingDep.TryGetValue(toLoadPluginName, out var toLoadPluginInfo) && toLoadPluginInfo!.PluginReferences.Contains(loadedPluginName))
                {
#if DEBUG
                    Interface.Oxide.LogDebug("Found dependent plugin({0}) from assembly({1}). Loading it...", toLoadPluginName, toLoadPluginInfo!.AssemblyName);
#endif
                    if (LoadPlugin(toLoadPluginInfo))
                        _pluginsWaitingDep.Remove(toLoadPluginName);
                }
            }
        }

        private Plugin? InstantiatePlugin(PluginInfo pluginInfo)
        {
            var pluginName = pluginInfo.PluginName;
            Plugin plugin;
            try
            {
                plugin = pluginInfo.Plugin;
            }
            catch (MissingMethodException ex)
            {
                PluginErrors[pluginName] = $"Fail to found constructor to plugin({pluginName}): {ex}";
                Interface.Oxide.LogException($"Fail to found constructor to plugin({pluginName})", ex);
                return null;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException tex)
                    ex = tex.InnerException;

                PluginErrors[pluginName] = $"Fail to load plugin({pluginName}): {ex}";
                Interface.Oxide.LogException($"Fail to load plugin({pluginName})", ex);
                return null;
            }

            if (plugin == null)
            {
                Interface.Oxide.LogError("Plugin({0}) failed to load.", pluginName);
                return null;
            }

            if (plugin is CSharpPlugin csharpPlugin)
            {
                if (!csharpPlugin.SetPluginInfo(pluginName, pluginInfo.PluginFile))
                {
                    Interface.Oxide.LogError("Fail to set plugin info for plugin({0})", pluginName);
                    csharpPlugin.SetFailState("Fail to set plugin info");
                    return null;
                }

                if (csharpPlugin.HookedOnFrame)
                    _onFramePlugins.Add(csharpPlugin);
                csharpPlugin.Watcher = extension.Watcher;
            }

            plugin.Loader = this;
            if (!Interface.Oxide.PluginLoaded(plugin))
            {
                Interface.Oxide.LogError("Plugin({0}) failed to load(oxide).", pluginName);
                return null;
            }

            return plugin;
        }
    }
}