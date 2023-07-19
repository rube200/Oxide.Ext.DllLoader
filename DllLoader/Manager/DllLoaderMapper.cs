#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Oxide.Core;
using Oxide.Ext.DllLoader.Helpers;
using FileAttributes = System.IO.FileAttributes;

#endregion

namespace Oxide.Ext.DllLoader.Manager
{
    internal sealed class DllLoaderMapper
    {
        private readonly IDictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, IDictionary<string, Type>> _assemblyPluginsType = new Dictionary<string, IDictionary<string, Type>>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, string> _pluginTypePath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


        public void OnModLoad()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        }
        public void OnShutdown()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
            _pluginTypePath.Clear();
            _assemblyPluginsType.Clear();
            _loadedAssemblies.Clear();
        }


        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (_loadedAssemblies.TryGetValue(args.Name, out var assembly))
                return assembly;

            return null;
        }


        public ISet<string> ScanDirectoryPlugins(string directory)
        {
            var assemblies = ScanDirectoryForAssemblies(directory);
            var dirPlugins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (assemblyName, filepath) in assemblies)
            {
                if (!_loadedAssemblies.TryGetValue(assemblyName, out var assembly))
                    continue;

                var nameType = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                var plugins = assembly.GetPlugins();

                foreach (var plugin in plugins)
                {
                    var pluginName = plugin.FullName ?? plugin.Name;

                    dirPlugins.Add(pluginName);
                    nameType[pluginName] = plugin;

                    _pluginTypePath.Add(pluginName, filepath);
                }
                
                _assemblyPluginsType[assemblyName] = nameType;
            }

            return dirPlugins;
        }


        private ISet<(string, string)> ScanDirectoryForAssemblies(string directory)
        {
            Interface.Oxide.LogDebug("Scanning directory({0}).", directory);
            if (!Directory.Exists(directory))
            {
                Interface.Oxide.LogDebug("Fail to scan directory({0}), directory not found.", directory);
                return new HashSet<(string, string)>();
            }

            var assembliesNameAndPath = new HashSet<(string, string)>();
            var dirFiles = new DirectoryInfo(directory).GetFiles("*.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in dirFiles)
            {
                Interface.Oxide.LogDebug("Found file({0}) in directory({1}).", file.Name, directory);
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    Interface.Oxide.LogDebug("File ({0}) is marked as hidden, ignoring.", file.Name);
                    continue;
                }

                var assembly = LoadAssembly(file.FullName);
                if (assembly == null)
                {
                    Interface.Oxide.LogWarning("Fail to load assembly({0})", file.FullName);
                    continue;
                }

                Interface.Oxide.LogDebug("Assembly({0}) loaded from file({1}) in directory({2}).", assembly.FullName, file.Name, directory);
                assembliesNameAndPath.Add((assembly.FullName, file.FullName));
            }

            return assembliesNameAndPath;
        }


        private Assembly LoadAssembly(string assemblyPath)
        {
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);

            var originalFullName = assemblyDefinition.FullName;
            assemblyDefinition.PatchAssembly(patchName:true, patchOxide:true);

            byte[] symbolsData = null;
            var symbolsPath = Path.ChangeExtension(assemblyPath, ".pdb");
            if (File.Exists(symbolsPath))
            {
                using (var fileStream = new FileStream(symbolsPath, FileMode.Open, FileAccess.Read))
                {
                    symbolsData = new byte[fileStream.Length];
                    var count = fileStream.Read(symbolsData, 0, symbolsData.Length);
                    if (count != symbolsData.Length)
                        Interface.Oxide.LogWarning("Fail to load symbols({0}) {1}bytes of {2}bytes.", symbolsPath, count, symbolsData.Length);
                }
            }

            Assembly assembly;
            using (var stream = new MemoryStream())
            {
                assemblyDefinition.Write(stream);
                assembly = Assembly.Load(stream.ToArray(), symbolsData);
            }

            if (originalFullName == assembly.FullName)
                Interface.Oxide.LogDebug("Assembly name patch failed.{2}Old name:{0}{2}New name:{1}", originalFullName, assembly.FullName, Environment.NewLine);

            _loadedAssemblies[originalFullName ?? assembly.FullName] = assembly;
            return assembly;
        }


        public Type GetPluginType(string name)
        {
            foreach (var nameType in _assemblyPluginsType.Values)
            {
                if (nameType.TryGetValue(name, out var pluginType))
                    return pluginType;
            }

            return null;
        }

        public string GetPluginFilePath(string name)
        {
            if (_pluginTypePath.TryGetValue(name, out var filepath))
                return filepath;

            return null;
        }
    }
}