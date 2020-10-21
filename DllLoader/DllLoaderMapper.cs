using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Oxide.Core;
using Oxide.Core.CSharp;
using Oxide.Core.Plugins;

namespace Oxide.Ext.DllLoader
{
    internal sealed class DllLoaderMapper
    {
        private readonly DllLoader _loader;

        internal DllLoaderMapper(DllLoader loader)
        {
            _loader = loader;
        }

        internal void OnModLoaded()
        {
            _assembliesHash = new Dictionary<string, byte[]>();
            _registeredAssemblies = new Dictionary<string, Assembly>();

            _fileTypes = new Dictionary<string, HashSet<string>>();
            _pluginsType = new Dictionary<string, Type>();
        }

        internal void OnShutdown()
        {
            _registeredAssemblies.Clear();
            _registeredAssemblies = null;

            _assembliesHash.Clear();
            _assembliesHash = null;

            _pluginsType.Clear();
            _pluginsType = null;

            foreach (var value in _fileTypes.Values)
                value.Clear();
            _fileTypes.Clear();
            _fileTypes = null;
        }

        public List<string> ScanAndMapPluginsDir(string directory)
        {
            DllLoader.LogDebug($"ScanAndMapPluginsDir({directory})");
            if (!directory.EndsWith("plugins", StringComparison.OrdinalIgnoreCase) || !Directory.Exists(directory))
            {
                DllLoader.LogDebug("Return new List<string>(); (invalid directory)");
                return new List<string>();
            }

            var registeredPlugins = new List<string>();
            foreach (var file in Directory.GetFiles(directory, $"*{_loader.FileExtension}"))
            {
                var assembly = GetPluginAssembly(file);
                var plugins = GetOrRegisterPlugins(file, assembly);
                registeredPlugins.AddRange(plugins);
            }

            DllLoader.LogDebug($"Return {registeredPlugins}; Count: {registeredPlugins.Count} (from plugins directory)");
            return registeredPlugins;
        }

        public List<string> ScanAndMapPluginsFile(string filePath)
        {
            DllLoader.LogDebug($"ScanAndMapPluginsDir({filePath})");
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                DllLoader.LogDebug("Return new List<string>(); (invalid file)");
                return new List<string>();
            }

            var assembly = GetPluginAssembly(filePath);
            var plugins = GetOrRegisterPlugins(filePath, assembly).ToList();
            DllLoader.LogDebug($"Return {plugins}; Count: {plugins.Count} (from file)");
            return plugins;
        }

        #region Assemblies

        private Dictionary<string, byte[]> _assembliesHash;
        private Dictionary<string, Assembly> _registeredAssemblies;

        private Assembly GetPluginAssembly(string filePath)
        {
            DllLoader.LogDebug($"GetPluginAssembly({filePath})");
            AssemblyDefinition assemblyDefinition;
            using (var memory = new MemoryStream(File.ReadAllBytes(filePath)))
            {
                byte[] hash;
                using (var sha1 = SHA1.Create())
                    hash = sha1.ComputeHash(memory.ToArray());

                if (VerifyAssemblyHash(filePath, hash) && _registeredAssemblies.TryGetValue(filePath, out var assembly))
                {
                    DllLoader.LogDebug($"Return {assembly}; (from cache)");
                    return assembly;
                }

                assemblyDefinition = AssemblyDefinition.ReadAssembly(memory);
            }

            PatchAssembly(assemblyDefinition);
            
            Assembly pluginAssembly;
            using (var memory = new MemoryStream())
            {
                assemblyDefinition.Write(memory);
                pluginAssembly = Assembly.Load(memory.ToArray());
            }

            ClearPluginsCache(filePath);
            _registeredAssemblies[filePath] = pluginAssembly;
            DllLoader.LogDebug($"Return {pluginAssembly}; (from file)");
            return pluginAssembly;
        }

        private bool VerifyAssemblyHash(string filePath, byte[] expectedHash)
        {
            DllLoader.LogDebug($"VerifyAssemblyHash({filePath}, {BitConverter.ToString(expectedHash)})");
            if (_assembliesHash.TryGetValue(filePath, out var assemblyHash))
            {
                DllLoader.LogDebug($"Hash found: {BitConverter.ToString(assemblyHash)}");
                if (assemblyHash.SequenceEqual(expectedHash))
                {
                    DllLoader.LogDebug("Return true; (hash match)");
                    return true;
                }
            }

            _assembliesHash[filePath] = expectedHash;
            DllLoader.LogDebug("Return false; (hash updated)");
            return false;
        } 

        private void PatchAssembly(AssemblyDefinition assemblyDefinition)
        {
            DllLoader.LogDebug($"PatchAssembly({assemblyDefinition})");
            foreach (var type in assemblyDefinition.MainModule.Types)
            {
                var pluginType = false;
                var typeDefinition = type.BaseType?.Resolve();

                while (typeDefinition != null)
                {
                    if (typeDefinition.FullName == "Oxide.Plugins.CSharpPlugin")
                    {
                        pluginType = true;
                        break;
                    }

                    typeDefinition = typeDefinition.BaseType?.Resolve();
                }

                if (!pluginType)
                    continue;

                _ = new DirectCallMethod(assemblyDefinition.MainModule, type);
            }
            DllLoader.LogDebug("Patch Completed!");
        }

        #endregion

        #region ResolvePlugins

        private Dictionary<string, HashSet<string>> _fileTypes;
        private Dictionary<string, Type> _pluginsType;

        private void ClearPluginsCache(string filePath)
        {
            DllLoader.LogDebug($"ClearPluginsCache({filePath})");
            if (!_fileTypes.TryGetValue(filePath, out var pluginNames))
                return;

            _fileTypes.Remove(filePath);
            foreach (var pluginName in pluginNames)
                _pluginsType.Remove(pluginName);
        }

        private IEnumerable<string> GetOrRegisterPlugins(string filePath, Assembly assembly)
        {
            DllLoader.LogDebug($"GetOrRegisterPlugins({filePath}, {assembly})");
            if (_fileTypes.TryGetValue(filePath, out var plugins))
            {
                DllLoader.LogDebug($"Return {plugins}; Count: {plugins.Count} (from cache)");
                return plugins;
            }

            Type[] pluginTypes;
            try
            {
                pluginTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Interface.Oxide.LogException($"[{assembly.GetName().Name}]", ex);
                if (ex.LoaderExceptions != null && ex.LoaderExceptions.Length > 0)
                {
                    foreach (var loaderException in ex.LoaderExceptions)
                        Interface.Oxide.LogException($"[{assembly.GetName().Name}]", loaderException);
                }

                pluginTypes = ex.Types.Where(tp => tp != null).ToArray();
            }

            var pluginNames = new HashSet<string>();
            foreach (var pluginType in pluginTypes.Where(tp => !tp.IsAbstract && typeof(Plugin).IsAssignableFrom(tp)))
            {
                DllLoader.LogDebug($"Plugin: '{pluginType.Name}'");
                pluginNames.Add(pluginType.Name);
                _pluginsType[pluginType.Name] = pluginType;
            }

            _fileTypes[filePath] = pluginNames;
            DllLoader.LogDebug($"Return {pluginNames}; Count: {pluginNames.Count} (from assembly)");
            return pluginNames;
        }

        internal Type GetPluginTypeByName(string pluginName)
        {
            DllLoader.LogDebug($"GetPluginTypeByName({pluginName})");
            if (_pluginsType.TryGetValue(pluginName, out var pluginType))
            {
                DllLoader.LogDebug($"Return {pluginType};");
                return pluginType;
            }
            
            var filePath = GetPluginPathByName(pluginName);
            if (!string.IsNullOrEmpty(filePath))
            {
                ScanAndMapPluginsFile(filePath);
                if (_pluginsType.TryGetValue(pluginName, out pluginType))
                {
                    DllLoader.LogDebug($"Return {pluginType};");
                    return pluginType;
                }
            }

            Interface.Oxide.LogWarning($"Plugin '{pluginName}' is not registered on DllLoader");
            DllLoader.LogDebug("Return null;");
            return null;
        }

        internal string GetPluginPathByName(string pluginName)
        {
            DllLoader.LogDebug($"GetPluginPathByName({pluginName})");
            foreach (var fileTypes in _fileTypes.Where(fileTypes => fileTypes.Value.Contains(pluginName)))
            {
                DllLoader.LogDebug($"Return {fileTypes.Key};");
                return fileTypes.Key;
            }

            return string.Empty;
        }

        #endregion
    }
}
