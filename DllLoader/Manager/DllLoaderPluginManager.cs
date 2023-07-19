#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Plugins;

#endregion

namespace Oxide.Ext.DllLoader.Manager
{
    public sealed class DllLoaderPluginManager : PluginLoader
    {
        public override string FileExtension => ".dll";
        internal List<Plugin> OnFramePlugins = new List<Plugin>();

        private DllLoaderMapper _mapper;
        private DllLoaderExtension _extension;


        public DllLoaderPluginManager(DllLoaderExtension extension)
        {
            _extension = extension;
            _mapper = new DllLoaderMapper();
        }
        ~DllLoaderPluginManager()
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

        protected override Plugin GetPlugin(string filename)
        {
            var name = Path.GetFileNameWithoutExtension(filename);
            if (LoadedPlugins.TryGetValue(name, out var plugin))
                return plugin;

            var pluginType = _mapper.GetPluginType(name);
            if (pluginType == null)
                return null;

            try
            {
                plugin = Activator.CreateInstance(pluginType) as Plugin;
            }
            catch (MissingMethodException ex)
            {
                Interface.Oxide.LogException($"Fail to found constructor to plugin({name})", ex);
                return null;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException tex)
                    ex = tex.InnerException;

                Interface.Oxide.LogException($"Fail to load plugin({name})", ex);
                return null;
            }

            if (plugin == null)
            {
                Interface.Oxide.LogWarning("Plugin({0}) failed to load.", name);
                return null;
            }

            foreach (var method in pluginType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var hooks = method.GetCustomAttributes(typeof(HookMethodAttribute), true);
                if (hooks.Length > 0)
                    continue;

                if (method.Name.Equals("OnFrame"))
                    OnFramePlugins.Add(plugin);
            }

            plugin.Loader = this;
            if (!(plugin is CSharpPlugin csharpPlugin))
                return plugin;

            var pluginFilePath = _mapper.GetPluginFilePath(name);
            if (!csharpPlugin.SetPluginInfo(name, pluginFilePath))
            {
                Interface.Oxide.LogError($"Fail to set plugin info for plugin({name})");
                return null;
            }

            csharpPlugin.Watcher = _extension.Watcher;
            return csharpPlugin;
        }

        public override void Unloading(Plugin plugin)
        {
            //check dependencies
            LoadedPlugins.Remove(plugin.Name);
        }
    }
}