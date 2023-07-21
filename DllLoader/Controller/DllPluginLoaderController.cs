#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Ext.DllLoader.Mapper;
using Oxide.Ext.DllLoader.Model;
using Oxide.Plugins;
using ExtensionMethods = Oxide.Core.ExtensionMethods;

#endregion

namespace Oxide.Ext.DllLoader.Controller
{
    public sealed class DllPluginLoaderController : PluginLoader
    {
        private DllLoaderExtension _extension;

        private DllLoaderMapper _mapper;
        internal List<Plugin> OnFramePlugins = new List<Plugin>();


        public DllPluginLoaderController(DllLoaderExtension extension)
        {
            _extension = extension;
            _mapper = new DllLoaderMapper();
        }

        public override string FileExtension => ".dll";

        ~DllPluginLoaderController()
        {
            _mapper = null;
            _extension = null;
        }


        public void OnModLoad()
        {
            _mapper.OnModLoad();
        }

        public void OnShutdown()
        {
            _mapper.OnShutdown();
        }


        public override IEnumerable<string> ScanDirectory(string directory)
        {
            return _mapper.ScanDirectoryPlugins(directory);
        }

        public override Plugin Load(string directory, string name)
        {
            Interface.Oxide.LogDebug("Loading requested to plugin({0}).", name);
            if (LoadingPlugins.Contains(name))
            {
                Interface.Oxide.LogDebug("Load requested failed, plugin({0}) is already loading.", name);
                return null;
            }

            if (LoadedPlugins.ContainsKey(name))
            {
                Interface.Oxide.LogDebug("Load requested failed, plugin({0}) is already loaded.", name);
                return null;
            }

            try
            {
                Interface.Oxide.LogDebug("Getting assembly info for plugin({0}).", name);
                var assemblyInfo = _mapper.GetAssemblyInfoByPluginName(name);
                if (assemblyInfo == null)
                {
                    Interface.Oxide.LogDebug("Fail to find assembly info for plugin({0}).", name);
                    return null;
                }

                Interface.Oxide.LogDebug("Plugin({0}) assembly({1}) info found.", name, assemblyInfo.OriginalName);
                LoadingPlugins.Add(name);

                if (!assemblyInfo.IsAssemblyLoaded && !AssemblyController.LoadAssembly(assemblyInfo))
                {
                    Interface.Oxide.LogDebug(
                        "Fail to load plugin({0}), assembly({1}) and assembly file({2}) not found.", name,
                        assemblyInfo.OriginalName, assemblyInfo.AssemblyFile);
                    LoadingPlugins.Remove(name);
                    _mapper.RemoveAssemblyInfo(assemblyInfo);
                    return null;
                }

                var pluginInfo = assemblyInfo.GetPluginInfo(name);
                if (pluginInfo == null)
                {
                    Interface.Oxide.LogDebug("Fail to find plugin({0}) in assembly({1}).", name,
                        assemblyInfo.OriginalName);
                    LoadingPlugins.Remove(name);
                    return null;
                }

                if (pluginInfo.IsPluginLoaded)
                {
                    LoadingPlugins.Remove(name);
                    LoadedPlugins.Add(name, pluginInfo.Plugin);
                    return pluginInfo.Plugin;
                }

                Interface.Oxide.NextTick(() => Load(pluginInfo));
                return null;
            }
            catch (Exception ex)
            {
                LoadingPlugins.Remove(name);
                PluginErrors[name] = $"Fail to load plugin({name}): {ex}";
                return null;
            }
        }

        private void Load(PluginInfo pluginInfo)
        {
            Interface.Oxide.LogDebug("Loading plugin({0}).", pluginInfo.PluginName);
            var loadedLoadingRef =
                pluginInfo.ReferencedPlugins.Where(pr => LoadedPlugins.ContainsKey(pr) && LoadingPlugins.Contains(pr));
            foreach (var pluginRef in loadedLoadingRef)
                Interface.Oxide.UnloadPlugin(pluginRef);

            var missingRef = pluginInfo.ReferencedPlugins.Where(rf => !LoadedPlugins.ContainsKey(rf)).ToArray();
            if (missingRef.Any())
            {
                var waitingRef = pluginInfo.ReferencedPlugins.Where(rf => LoadingPlugins.Contains(rf)).ToArray();
                if (waitingRef.Any())
                {
                    Interface.Oxide.LogDebug("Plugin({0}) is waiting for references to be loaded: {1}",
                        pluginInfo.PluginName, ExtensionMethods.ToSentence(waitingRef));
                    return;
                }

                var missingReqString = ExtensionMethods.ToSentence(missingRef);
                Interface.Oxide.LogError("Plugin({0}) references are missing: {1}", pluginInfo.PluginName,
                    missingReqString);
                PluginErrors[pluginInfo.PluginName] = $"Missing dependencies: {missingReqString}";
            }
            else
            {
                Interface.Oxide.UnloadPlugin(pluginInfo.PluginName);
                pluginInfo.Plugin = InstantiatePlugin(pluginInfo);

                if (pluginInfo.IsPluginLoaded)
                    LoadedPlugins[pluginInfo.PluginName] = pluginInfo.Plugin;
            }

            LoadingPlugins.Remove(pluginInfo.PluginName);
            foreach (var loadingName in LoadingPlugins.ToArray())
            {
                var plInfo = _mapper.GetPluginInfoByName(loadingName);
                if (plInfo == null || plInfo.IsPluginLoaded ||
                    !plInfo.ReferencedPlugins.Contains(pluginInfo.PluginName))
                    continue;

                Load(plInfo);
            }
        }

        private Plugin InstantiatePlugin(PluginInfo pluginInfo)
        {
            Plugin plugin;
            try
            {
                plugin = Activator.CreateInstance(pluginInfo.PluginType) as Plugin;
            }
            catch (MissingMethodException ex)
            {
                Interface.Oxide.LogException($"Fail to found constructor to plugin({pluginInfo.PluginName})", ex);
                return null;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException tex)
                    ex = tex.InnerException;

                Interface.Oxide.LogException($"Fail to load plugin({pluginInfo.PluginName})", ex);
                return null;
            }

            if (plugin == null)
            {
                Interface.Oxide.LogWarning("Plugin({0}) failed to load.", pluginInfo.PluginName);
                return null;
            }

            foreach (var method in pluginInfo.PluginType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var hooks = method.GetCustomAttributes(typeof(HookMethodAttribute), true);
                if (hooks.Length > 0)
                    continue;

                if (method.Name.Equals("OnFrame"))
                    OnFramePlugins.Add(plugin);
            }

            if (plugin is CSharpPlugin csharpPlugin)
            {
                if (!csharpPlugin.SetPluginInfo(pluginInfo.PluginName, pluginInfo.PluginFile))
                {
                    Interface.Oxide.LogError("Fail to set plugin info for plugin({0})", pluginInfo.PluginName);
                    return null;
                }

                csharpPlugin.Watcher = _extension.Watcher;
            }

            plugin.Loader = this;
            if (!Interface.Oxide.PluginLoaded(plugin))
            {
                Interface.Oxide.LogError("Plugin({0}) failed to load(oxide).", pluginInfo.PluginName);
                return null;
            }

            return plugin;
        }

        public override void Unloading(Plugin plugin)
        {
            //check dependencies
            //todo add debug
            LoadedPlugins.Remove(plugin.Name);
        }
    }
}