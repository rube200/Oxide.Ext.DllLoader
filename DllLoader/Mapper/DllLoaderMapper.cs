#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Oxide.Core;
using Oxide.Ext.DllLoader.API;
using Oxide.Ext.DllLoader.Controller;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.Mapper
{
    public sealed class DllLoaderMapper : DefaultAssemblyResolver, IDllLoaderMapper
    {
        private readonly ISet<string> _searchDirs = new HashSet<string>();
        private readonly IDictionary<string, AssemblyInfo> _assembliesInfo =
            new Dictionary<string, AssemblyInfo>(StringComparer.OrdinalIgnoreCase);

        public void RemoveAssemblyInfo(string assemblyName)
        {
            _assembliesInfo.Remove(assemblyName);
        }


        #region AssemblyResolver

        public void OnModLoad()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            AddSearchDirectory(Interface.Oxide.ExtensionDirectory);
        }

        public void OnShutdown()
        {
            RemoveSearchDirectory(Interface.Oxide.ExtensionDirectory);
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
            _assembliesInfo.Clear();
        }

        private Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyInfo = GetAssemblyInfoByName(args.Name);
            if (assemblyInfo == null)
                return null;

            if (assemblyInfo.IsAssemblyLoaded || AssemblyController.LoadAssembly(assemblyInfo, this))
                return assemblyInfo.Assembly;

            RemoveAssemblyInfo(args.Name);
            return null;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            if (!name.Name.StartsWith("Oxide.") && !name.FullName.StartsWith("Oxide."))
            {
                var assemblyInfo = GetAssemblyInfoByName(name.Name) ?? GetAssemblyInfoByName(name.FullName);
                if (assemblyInfo != null)
                    return assemblyInfo.AssemblyDefinition;
            }

            return base.Resolve(name);
        }

        #endregion


        #region ScanDiretory

        public IEnumerable<string> ScanDirectoryPlugins(string directory)
        {
            if (!_searchDirs.Contains(directory))
            {
                _searchDirs.Add(directory);
                AddSearchDirectory(directory);
            }

            ScanAndRegisterAssemblies(directory);
#if DEBUG
            Interface.Oxide.LogDebug("Total assemblies registered({0}).", _assembliesInfo.Count);
#endif

            var plugins = _assembliesInfo.Values.SelectMany(ai => ai.PluginsName).ToArray();
#if DEBUG
            Interface.Oxide.LogDebug("Total plugins registered({0}).", plugins.Length);
#endif
            return plugins;
        }

        private void ScanAndRegisterAssemblies(string directory)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Scanning directory({0}) for assemblies...", directory);
#endif
            if (!Directory.Exists(directory))
            {
                Interface.Oxide.LogWarning("Fail to scan directory({0}), directory not found.", directory);
                return;
            }

            var dirFiles = new DirectoryInfo(directory).GetFiles("*.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in dirFiles)
            {
#if DEBUG
                Interface.Oxide.LogDebug("Found file({0}) in directory({1}).", file.Name, directory);
#endif
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    Interface.Oxide.LogInfo("Ignoring file({0}), marked as hidden.", file.Name);
                    continue;
                }

                var assemblyName = AssemblyController.GetAssemblyNameFromFile(file.FullName);
                var assemblyInfo = GetAssemblyInfoByName(assemblyName.FullName) ?? GetAssemblyInfoByName(assemblyName.Name);
                if (assemblyInfo != null)
                {
                    if (assemblyInfo.LastWriteTimeUtc >= file.LastWriteTimeUtc)
                    {
#if DEBUG
                        Interface.Oxide.LogDebug("Assembly({0}) already registered.", file.FullName);
#endif
                        continue;
                    }

#if DEBUG
                    Interface.Oxide.LogDebug("Assembly({0}) already registered, but need to be reloaded.",
                        file.FullName);
#endif
                }

                assemblyInfo = AssemblyController.LoadAssemblyInfo(file.FullName, file.LastWriteTimeUtc, this);
                if (assemblyInfo == null)
                {
                    Interface.Oxide.LogError("Fail to load assembly({0})", file.FullName);
                    continue;
                }

#if DEBUG
                Interface.Oxide.LogDebug("Assembly({0}) loaded from file({1}) in directory({2}).",
                    assemblyInfo.OriginalName, file.Name, directory);
#endif

                _assembliesInfo[assemblyName.Name] = assemblyInfo;
                _assembliesInfo[assemblyName.FullName] = assemblyInfo;
                RegisterAssembly(assemblyInfo.AssemblyDefinition);
            }

            foreach (var assembly in _assembliesInfo.Values)
                AssemblyController.RegisterAssemblyPlugins(assembly);
        }

        #endregion


        #region Getters

        public AssemblyInfo? GetAssemblyInfoByName(string name)
        {
            if (_assembliesInfo.TryGetValue(name, out var assemblyInfo))
                return assemblyInfo;

            return null;
        }

        public AssemblyInfo GetAssemblyInfoByFile(string filename)
        {
            return _assembliesInfo.Values.FirstOrDefault(assemblyInfo => assemblyInfo.IsFile(filename));
        }

        public AssemblyInfo GetAssemblyInfoByPluginName(string name)
        {
            return _assembliesInfo.Values.FirstOrDefault(assemblyInfo => assemblyInfo.ContainsPlugin(name));
        }

        public PluginInfo GetPluginInfoByName(string pluginName)
        {
            return _assembliesInfo.Values.Select(assemblyInfo => assemblyInfo.GetPluginInfo(pluginName))
                .FirstOrDefault(pluginInfo => pluginInfo != null);
        }

        #endregion
    }
}