#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Ext.DllLoader.API;
using Oxide.Ext.DllLoader.Mapper;
using Oxide.Ext.DllLoader.Model;
using Oxide.Plugins;

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

        public override Plugin? Load(string directory, string pluginName)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Loading requested to plugin({0}).", pluginName);
#endif
            if (!CanPluginLoad(pluginName))
                return null;

#if DEBUG
            Interface.Oxide.LogDebug("Getting assembly info for plugin({0}).", pluginName);
#endif
            var assemblyInfo = Mapper.GetAssemblyInfoByPlugin(pluginName);
            if (assemblyInfo == null)
            {
                Interface.Oxide.LogError("Fail to find assembly info for plugin({0}).", pluginName);
                return null;
            }

#if DEBUG
            Interface.Oxide.LogDebug("Plugin({0}) assembly({1}) info found.", pluginName, assemblyInfo.OriginalName);
#endif

            LoadingPlugins.Add(pluginName);
            try
            {
                if (!assemblyInfo.IsAssemblyLoaded && !Mapper.LoadAssembly(assemblyInfo))
                {
                    Interface.Oxide.LogError("Fail to load plugin({0}), assembly({1}) and assembly file({2}) not found.", pluginName, assemblyInfo.OriginalName, assemblyInfo.AssemblyFile);

                    var assemblyNameDefinition = assemblyInfo.AssemblyDefinition.Name;
                    Mapper.RemoveAssemblyInfo(assemblyNameDefinition);
                    return null;
                }

                var pluginInfo = assemblyInfo.GetPluginInfo(pluginName);
                if (pluginInfo == null)
                {
                    Interface.Oxide.LogError("Fail to find plugin({0}) in assembly({1}).", pluginName, assemblyInfo.OriginalName);
                    return null;
                }

                if (pluginInfo.IsPluginLoaded)
                {
                    LoadedPlugins.Add(pluginName, pluginInfo.Plugin);
                    return pluginInfo.Plugin;
                }

                //Interface.Oxide.NextTick(() => );//add catch back very similar to this one
#if DEBUG
                Interface.Oxide.LogDebug("Loading plugin({0}).", pluginInfo.PluginName);
#endif
                Interface.Oxide.UnloadPlugin(pluginInfo.PluginName);
                InstantiatePlugin(pluginInfo);

                if (pluginInfo.IsPluginLoaded)
                    LoadedPlugins[pluginInfo.PluginName] = pluginInfo.Plugin;

                return pluginInfo.Plugin;
            }
            catch (Exception ex)
            {
                PluginErrors[pluginName] = $"Fail to load plugin({pluginName}): {ex}";
#if DEBUG
                Interface.Oxide.LogDebug("Exception while loading plugin({0}): {1}", pluginName, ex);
#endif
                return null;
            }
            finally
            {
                LoadingPlugins.Remove(pluginName);
            }
        }

        private void InstantiatePlugin(PluginInfo pluginInfo)
        {
            Plugin plugin;
            try
            {
                plugin = pluginInfo.Plugin;
            }
            catch (MissingMethodException ex)
            {
                PluginErrors[pluginInfo.PluginName] = $"Fail to found constructor to plugin({pluginInfo.PluginName}): {ex}";
                Interface.Oxide.LogException($"Fail to found constructor to plugin({pluginInfo.PluginName})", ex);
                return;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException tex)
                    ex = tex.InnerException;

                PluginErrors[pluginInfo.PluginName] = $"Fail to load plugin({pluginInfo.PluginName}): {ex}";
                Interface.Oxide.LogException($"Fail to load plugin({pluginInfo.PluginName})", ex);
                return;
            }

            if (plugin == null)
            {
                Interface.Oxide.LogError("Plugin({0}) failed to load.", pluginInfo.PluginName);
                return;
            }

            if (plugin is CSharpPlugin csharpPlugin)
            {
                if (!csharpPlugin.SetPluginInfo(pluginInfo.PluginName, pluginInfo.PluginFile))
                {
                    Interface.Oxide.LogError("Fail to set plugin info for plugin({0})", pluginInfo.PluginName);
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
                Interface.Oxide.LogError("Plugin({0}) failed to load(oxide).", pluginInfo.PluginName);
                return;
            }
        }


        public override void Unloading(Plugin plugin)
        {
            LoadedPlugins.Remove(plugin.Name);
        }


        internal void AddAssembly(string assemblyName)
        {
            //todo assemblyName is file name without ext instead of path
            var fileInfo = new FileInfo(assemblyName);
            if (!fileInfo.Exists || !Mapper.RegisterAssemblyFromFile(fileInfo))
            {
                Interface.Oxide.LogDebug(fileInfo.Exists.ToString());
                Interface.Oxide.LogError("Fail to load assembly in file({0})", assemblyName);
                return;
            }

            Interface.Oxide.LogDebug("super");
        }

        internal void RemoveAssembly(string assemblyName)
        {
            throw new NotImplementedException("RemoveAssembly");
        }

        internal void ReloadAssembly(string assemblyName)
        {
            throw new NotImplementedException("ReloadAssembly");
        }
    }
}