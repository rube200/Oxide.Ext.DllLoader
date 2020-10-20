#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil;
using Oxide.Core;
using Oxide.Core.CSharp;
using Oxide.Core.Plugins;
using Oxide.Plugins;

#endregion

namespace Oxide.Ext.DllLoader
{
    [UsedImplicitly]
    public sealed class DllLoader : PluginLoader
    {
        private Dictionary<string, HashSet<string>> _filePlugins;
        private Dictionary<string, string> _pluginsPath;
        private Dictionary<string, Type> _pluginsType;
        private DllLoaderWatcher _watcher;

        public override string FileExtension => ".dll";

        internal void OnModLoaded()
        {
            _filePlugins = new Dictionary<string, HashSet<string>>();
            _pluginsPath = new Dictionary<string, string>();
            _pluginsType = new Dictionary<string, Type>();
        }

        internal void OnShutdown()
        {
            foreach (var value in _filePlugins.Values)
                value.Clear();
            _filePlugins.Clear();
            _filePlugins = null;

            _pluginsPath.Clear();
            _pluginsPath = null;

            _pluginsType.Clear();
            _pluginsType = null;
        }

        private string GetPath(string directory, string name)
        {
            return Path.Combine(directory, name + FileExtension);
        }

        private IEnumerable<Type> GetPluginsTypes(string directory, string fileName)
        {
            var filePath = GetPath(directory, fileName);
            var assemblyBytes = File.ReadAllBytes(filePath);
            PatchAssembly(ref assemblyBytes);

            var assemblyPlugin = Assembly.Load(assemblyBytes);

            Type[] types;
            try
            {
                types = assemblyPlugin.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Interface.Oxide.LogException($"[{fileName}]", ex);

                if (ex.LoaderExceptions != null && ex.LoaderExceptions.Length > 0)
                    foreach (var loaderException in ex.LoaderExceptions)
                        Interface.Oxide.LogException($"[{fileName}]", loaderException);

                types = ex.Types.Where(tp => tp != null).ToArray();
            }

            return types.Where(tp => !tp.IsAbstract && typeof(Plugin).IsAssignableFrom(tp));
        }

        public override IEnumerable<string> ScanDirectory(string directory)
        {
            if (!directory.EndsWith("plugins", StringComparison.OrdinalIgnoreCase) ||
                !Directory.Exists(directory)) yield break;

            foreach (var file in Directory.GetFiles(directory, $"*{FileExtension}"))
            {
                var fileName = Utility.GetFileNameWithoutExtension(file);
                _filePlugins[fileName] = new HashSet<string>();

                foreach (var pluginType in GetPluginsTypes(directory, fileName))
                {
                    _filePlugins[fileName].Add(pluginType.Name);
                    _pluginsPath[pluginType.Name] = file;
                    _pluginsType[pluginType.Name] = pluginType;
                    yield return pluginType.Name;
                }
            }
        }

        public override Plugin Load(string directory, string name)
        {
            if (LoadingPlugins.Contains(name))
            {
                Interface.Oxide.LogDebug($"Load requested for plugin which is already loading: {name}");
                return null;
            }

            var plugin = GetPlugin(Path.Combine(directory, name + FileExtension));
            if (plugin == null)
                return null;

            LoadingPlugins.Add(plugin.Name);
            Interface.Oxide.NextTick(() => LoadPlugin(plugin));
            return null;
        }

        protected override Plugin GetPlugin(string filename)
        {
            var pluginName = Utility.GetFileNameWithoutExtension(filename);
            if (!_pluginsType.TryGetValue(pluginName, out var pluginType))
            {
                Interface.Oxide.LogWarning($"Plugin '{pluginName}' is not registered on DllLoader");
                return null;
            }

            if (!(Activator.CreateInstance(pluginType) is Plugin plugin))
                return null;

            if (!(plugin is CSharpPlugin csharpPlugin))
                return plugin;

            csharpPlugin.Watcher = _watcher.Watcher;
            csharpPlugin.SetPluginInfo(pluginName, _pluginsPath[pluginName]);

            return plugin;
        }

        private void PatchAssembly(ref byte[] assemblyBytes)
        {
            AssemblyDefinition definition;
            using (var stream = new MemoryStream(assemblyBytes)) 
                definition = AssemblyDefinition.ReadAssembly(stream);

            foreach (var type in definition.MainModule.Types)
            {
                var found = false;
                var typeDefinition = type;

                while (true)
                {
                    if (typeDefinition.FullName == "Oxide.Plugins.CSharpPlugin")
                    {
                        found = true;
                        break;
                    }

                    if (typeDefinition.BaseType is null)
                        break;

                    typeDefinition = typeDefinition.BaseType.Resolve();
                }

                if (!found)
                    continue;

                _ = new DirectCallMethod(definition.MainModule, type);
            }

            using (var stream = new MemoryStream())
            {
                definition.Write(stream);
                assemblyBytes = stream.ToArray();
            }
        }

        private void OnGetPluginsInFile(string filepath, HashSet<string> plugins)
        {
            var fileName = Utility.GetFileNameWithoutExtension(filepath);
            if (!_filePlugins.TryGetValue(fileName, out var pluginsName))
                return;

            plugins.UnionWith(pluginsName);
        }

        public void SetWatcher(DllLoaderWatcher dllWatcher)
        {
            if (_watcher != null)
                _watcher.OnGetPluginsInFile -= OnGetPluginsInFile;

            _watcher = dllWatcher;
            _watcher.OnGetPluginsInFile += OnGetPluginsInFile;
        }
    }
}