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

        public IDllLoaderMapper Mapper => _mapper;
        public IReadOnlyCollection<Plugin> OnFramePlugins => _onFramePlugins;

        public override string FileExtension => ".dll";


        public void OnModLoad()
        {
            _mapper.OnModLoad();
        }

        public void OnShutdown()
        {
            _onFramePlugins.Clear();
            _mapper.OnShutdown();
        }


        public override IEnumerable<string> ScanDirectory(string directory)
        {
            return Mapper.ScanDirectoryPlugins(directory);
        }


        private bool CanPluginLoad(string pluginName)
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

        private AssemblyInfo? GetAssemblyInfoByName(string pluginName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Getting assembly info for plugin({0}).", pluginName);
#endif
            return Mapper.GetAssemblyInfoByPlugin(pluginName);
        }

        public override Plugin? Load(string _, string pluginName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Loading requested to plugin({0}).", pluginName);
#endif
            if (!CanPluginLoad(pluginName))
                return null;

            LoadPlugin(pluginName);
            return null;
        }

        private void LoadPlugin(string pluginName)
        {
            LoadingPlugins.Add(pluginName);

            try
            {
                var assemblyInfo = GetAssemblyInfoByName(pluginName);
                if (assemblyInfo == null)
                {
                    LoadingPlugins.Remove(pluginName);
                    Interface.Oxide.LogError("Fail to find assembly info for plugin({0}).", pluginName);
                    PluginErrors[pluginName] = $"Fail to find assembly info";
                    return;
                }

#if DEBUG
                Interface.Oxide.LogDebug("Plugin({0}) assembly({1}) info found.", pluginName, assemblyInfo.OriginalName);
#endif
                if (!assemblyInfo.IsAssemblyLoaded && !Mapper.LoadAssembly(assemblyInfo))
                {
                    LoadingPlugins.Remove(pluginName);
                    Interface.Oxide.LogError("Fail to load plugin({0}), it was not possible to load assembly({1}).", pluginName, assemblyInfo.OriginalName);
                    PluginErrors[pluginName] = $"Fail to load because assembly did not load.";

                    var assemblyNameDefinition = assemblyInfo.AssemblyDefinition.Name;
                    Mapper.RemoveAssemblyInfo(assemblyNameDefinition);
                    return;
                }

                var pluginInfo = assemblyInfo.GetPluginInfo(pluginName);
                if (pluginInfo == null)
                {
                    LoadingPlugins.Remove(pluginName);
                    Interface.Oxide.LogError("Fail to find plugin({0}) in assembly({1}).", pluginName, assemblyInfo.OriginalName);
                    PluginErrors[pluginName] = $"Fail to find plugin info.";
                    return;
                }

                LoadPlugin(pluginInfo, assemblyInfo.OriginalName);
            }
            catch (Exception ex)
            {
                LoadingPlugins.Remove(pluginName);

                Interface.Oxide.LogError("Exception while loading plugin({0}): {1}", pluginName, ex);
                PluginErrors[pluginName] = $"Exception while loading plugin({pluginName}): {ex}";
            }
        }

        private void LoadPlugin(PluginInfo pluginInfo, string assemblyName)
        {
            var pluginName = pluginInfo.PluginName;
            try
            {
                if (!pluginInfo.PluginReferences.All(LoadedPlugins.ContainsKey))
                {
                    var referencesLoading = pluginInfo.PluginReferences.Where(LoadingPlugins.Contains);
                    if (!referencesLoading.Any())
                    {
                        LoadingPlugins.Remove(pluginName);

                        var referencesMissing = pluginInfo.PluginReferences.Where(p => !LoadingPlugins.Contains(p) && !LoadedPlugins.ContainsKey(p));
                        Interface.Oxide.LogError("Fail to load plugin({0}) in assembly({1}), some dependencies are missing: {2}", pluginName, assemblyName, referencesMissing.ToSentence());
                        PluginErrors[pluginName] = $"Fail to load plugin, some dependencies are missing: {referencesMissing.ToSentence()}";
                        return;
                    }
#if DEBUG
                    else
                    {
                        Interface.Oxide.LogDebug("Plugin({0}) is waiting for dependencies to load: {1}", pluginName, referencesLoading.ToSentence());
                        return;
                    }
#endif
                }

                if (pluginInfo.IsPluginLoaded)
                {
                    LoadingPlugins.Remove(pluginName);

#if DEBUG
                    Interface.Oxide.LogDebug("Plugin({0}) is already loaded.", pluginName);
#endif
                    LoadedPlugins.Add(pluginName, pluginInfo.Plugin);
                    return;
                }

#if DEBUG
                Interface.Oxide.LogDebug("Loading plugin({0}) in the next tick.", pluginName);
#endif
                Interface.Oxide.NextTick(() => LoadPlugin(pluginInfo));
            }
            catch (Exception ex)
            {
                LoadingPlugins.Remove(pluginName);

                Interface.Oxide.LogError("Exception while loading plugin({0}): {1}", pluginName, ex);
                PluginErrors[pluginName] = $"Exception while loading plugin({pluginName}): {ex}";
            }
        }

        private void LoadPlugin(PluginInfo pluginInfo)
        {
            var pluginName = pluginInfo.PluginName;
            try
            {
#if DEBUG
                Interface.Oxide.LogDebug("Loading plugin({0}).", pluginName);
#endif
                Interface.Oxide.UnloadPlugin(pluginName);
                InstantiatePlugin(pluginInfo);

                if (pluginInfo.IsPluginLoaded)
                    LoadedPlugins[pluginName] = pluginInfo.Plugin;

                Interface.Oxide.LogInfo("Plugin({0}) loadded successfully.", pluginName);
            }
            catch (Exception ex)
            {
                LoadingPlugins.Remove(pluginName);

                Interface.Oxide.LogError("Exception while loading plugin({0}): {1}", pluginName, ex);
                PluginErrors[pluginName] = $"Exception while loading plugin({pluginName}): {ex}";
            }
            finally
            {
                LoadingPlugins.Remove(pluginName);
            }

#if DEBUG
            Interface.Oxide.LogDebug("Checking dependents to load...");
#endif
            foreach (var loadingPluginName in LoadingPlugins.ToArray())
            {
                var loadingAssemblyInfo = GetAssemblyInfoByName(loadingPluginName);
                var loadingPluginInfo = loadingAssemblyInfo?.GetPluginInfo(loadingPluginName);
                if (loadingPluginInfo == null)
                {
                    LoadingPlugins.Remove(pluginName);

                    Interface.Oxide.LogError("Fail to find plugin({0}) in assembly({1}).", pluginName, loadingAssemblyInfo?.OriginalName ?? "???");
                    PluginErrors[pluginName] = $"Fail to find plugin info.";
                    continue;
                }

                if (LoadingPlugins.Contains(loadingPluginName) && loadingPluginInfo.PluginReferences.Contains(pluginName))
                {
#if DEBUG
                    Interface.Oxide.LogDebug("Found dependant plugin({0}) from assembly({1}). Loading it...", pluginName, loadingAssemblyInfo!.OriginalName);
#endif
                    LoadPlugin(loadingPluginInfo, loadingAssemblyInfo.OriginalName);
                }
            }
        }

        private void InstantiatePlugin(PluginInfo pluginInfo)
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
                return;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException tex)
                    ex = tex.InnerException;

                PluginErrors[pluginName] = $"Fail to load plugin({pluginName}): {ex}";
                Interface.Oxide.LogException($"Fail to load plugin({pluginName})", ex);
                return;
            }

            if (plugin == null)
            {
                Interface.Oxide.LogError("Plugin({0}) failed to load.", pluginName);
                return;
            }

            if (plugin is CSharpPlugin csharpPlugin)
            {
                if (!csharpPlugin.SetPluginInfo(pluginName, pluginInfo.PluginFile))
                {
                    Interface.Oxide.LogError("Fail to set plugin info for plugin({0})", pluginName);
                    csharpPlugin.SetFailState("Fail to set plugin info");
                    return;
                }

                if (csharpPlugin.HookedOnFrame)
                    _onFramePlugins.Add(csharpPlugin);
                csharpPlugin.Watcher = extension.Watcher;
            }

            plugin.Loader = this;
            if (!Interface.Oxide.PluginLoaded(plugin))
            {
                Interface.Oxide.LogError("Plugin({0}) failed to load(oxide).", pluginName);
                return;
            }
        }

        public override void Unloading(Plugin plugin)
        {
            LoadedPlugins.Remove(plugin.Name);

            var pluginsInfo = _mapper.GetRegisteredPlugins().Where(p => p.PluginReferences.Contains(plugin.Name));
            foreach (var pluginInfo in pluginsInfo)
            {
                if (!pluginInfo.PluginReferences.Contains(plugin.Name))
                    continue;

                Interface.Oxide.UnloadPlugin(pluginInfo.PluginName);
            }
        }

        public override void Reload(string directory, string name)
        {
            /*            CompilablePlugin compilablePlugin = GetCompilablePlugin(directory, name);
            if (compilablePlugin.IsLoading)
            {
                Interface.Oxide.RootLogger.WriteDebug(LogType.Warning, LogEvent.Compile, "CSharp", $"Reload requested for plugin which is already loading: {compilablePlugin.Name}");
                return;
            }

            // Attempt to compile the plugin before unloading the old version
            Load(compilablePlugin);*/
            //todo
            //a bit more of work need to be done where
            //we need to invalidate old data first
            base.Reload(directory, name);
        }
    }
}