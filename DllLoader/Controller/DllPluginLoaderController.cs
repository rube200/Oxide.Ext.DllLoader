#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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


        public void OnModLoad()
        {
            _mapper.OnModLoad();
        }

        public void OnShutdown()
        {
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
            Console.WriteLine("RRRRRRRRRRRRRRRRRRRRRRRRRReload " + directory + " " + name);
            base.Reload(directory, name);
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

        private void LoadPlugin(PluginInfo pluginInfo)
        {
            var pluginName = pluginInfo.PluginName;
            if (!pluginInfo.PluginReferences.All(LoadedPlugins.ContainsKey))
            {
                var referencesLoading = pluginInfo.PluginReferences.Where(LoadingPlugins.Contains);
                if (referencesLoading.Any())
                {
#if DEBUG
                    Interface.Oxide.LogDebug("Plugin({0}) is waiting for dependencies to load: {1}", pluginInfo.PluginName, referencesLoading.ToSentence());
#endif
                    return;
                }
            }

            if (pluginInfo.IsPluginLoaded)
            {
#if DEBUG
                Interface.Oxide.LogDebug("Plugin({0}) is already loaded.", pluginName);
#endif
                LoadedPlugins[pluginName] = pluginInfo.Plugin;
                PluginLoaded(pluginName);
                return;
            }

#if DEBUG
            Interface.Oxide.LogDebug("Loading plugin({0}).", pluginName);
#endif
            Interface.Oxide.UnloadPlugin(pluginName);
            LoadedPlugins[pluginName] = InstantiatePlugin(pluginInfo);

            Interface.Oxide.LogInfo("Plugin({0}) loadded successfully.", pluginName);
            PluginLoaded(pluginName);
        }

        private void PluginLoaded(string pluginName)
        {
            LoadingPlugins.Remove(pluginName);

#if DEBUG
            Interface.Oxide.LogDebug("Checking dependents to load...");
#endif
            foreach (var loadingPluginName in LoadingPlugins.ToArray())
            {
                var loadingAssemblyInfo = Mapper.GetAssemblyInfoByPlugin(loadingPluginName);
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
                    LoadPlugin(loadingPluginInfo);
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