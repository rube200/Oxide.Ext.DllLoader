#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Oxide.Core;
using Oxide.Ext.DllLoader.API;
using Oxide.Ext.DllLoader.Model;
using static HarmonyLib.AccessTools;

#endregion

namespace Oxide.Ext.DllLoader.Mapper
{
    public sealed class DllLoaderMapper : DefaultAssemblyResolver, IDllLoaderMapperLoadable
    {
        private readonly IDictionary<string, AssemblyInfo> _assembliesInfoByName =
            new Dictionary<string, AssemblyInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, IReadOnlyCollection<PluginInfo>> _pluginsInfoByAssemblyName = 
            new Dictionary<string, IReadOnlyCollection<PluginInfo>>(StringComparer.OrdinalIgnoreCase);

        #region AssemblyResolver

        private readonly ISet<string> _searchResolverDirs = new HashSet<string>();

        public void OnModLoad()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            AddSearchDirectory(Interface.Oxide.ExtensionDirectory);
        }

        public void OnShutdown()
        {
            RemoveSearchDirectory(Interface.Oxide.ExtensionDirectory);
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
            _assembliesInfoByName.Values.Do(a => a.Dispose());
            _assembliesInfoByName.Clear();
        }

        private Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = AssemblyNameReference.Parse(args.Name);
            var assemblyInfo = GetAssemblyInfoFromNameReference(assemblyName);
            if (assemblyInfo == null)
                return null;

            return assemblyInfo.Assembly;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            if (!name.FullName.StartsWith("Oxide.") && !name.Name.StartsWith("Oxide."))
            {
                var assemblyInfo = GetAssemblyInfoFromNameReference(name);
                if (assemblyInfo != null)
                    return assemblyInfo.AssemblyDefinition;
            }

            return base.Resolve(name);
        }

        private void AddDirectoryToResolver(string directory)
        {
            if (!_searchResolverDirs.Contains(directory))
            {
                _searchResolverDirs.Add(directory);
                AddSearchDirectory(directory);
            }
        }

        #endregion

        #region ScanDiretory

        public IEnumerable<string> ScanDirectoryPlugins(string directory)
        {
            AddDirectoryToResolver(directory);
            ScanAndRegisterAssemblies(directory);

#if DEBUG
            Interface.Oxide.LogDebug("Total assemblies registered({0}).", _assembliesInfoByName.Count);
#endif

            var plugins = _assembliesInfoByName.Values.SelectMany(ai => ai.PluginsName).ToArray();
#if DEBUG
            Interface.Oxide.LogDebug("Total plugins registered({0}).", plugins.Length);
#endif
            return plugins;
        }

        public void ScanAndRegisterAssemblies(string directory)
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
                RegisterAssemblyFromFile(file);

            foreach (var assemblyInfo in _assembliesInfoByName.Values.Where(a => !_pluginsInfoByAssemblyName.ContainsKey(a.OriginalName)))
            {
                try
                {
                    _pluginsInfoByAssemblyName.Add(assemblyInfo.OriginalName, assemblyInfo.PluginsInfo);
                }
                catch (Exception ex)
                {
                    Interface.Oxide.LogException($"Fail to patch assembly({assemblyInfo.OriginalName})", ex);
                }
            }
        }

        private static readonly FieldRef<object, IDictionary<string, AssemblyDefinition>> Cache = FieldRefAccess<IDictionary<string, AssemblyDefinition>>(typeof(DllLoaderMapper), "cache");
        private void RegisterAssemblyFromFile(FileInfo file)
        {
            if (!file.Exists)
            {
                Interface.Oxide.LogError("Fail to load assembly({0}), file does not exist.", file.FullName);
                return;
            }

#if DEBUG
            Interface.Oxide.LogDebug("Found file({0}) in directory({1}).", file.Name, file.DirectoryName);
#endif
            if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                Interface.Oxide.LogInfo("Ignoring file({0}), marked as hidden.", file.Name);
                return;
            }

            var assemblyDefinition = GetAssemblyDefinitionFromFile(file.FullName);
            var assemblyDefinitionName = assemblyDefinition.Name;
            var assemblyInfo = GetAssemblyInfoFromNameReference(assemblyDefinitionName);
            if (assemblyInfo != null)
            {
#if DEBUG
                Interface.Oxide.LogDebug("Assembly({0}) already registered.", file.FullName);
#endif
                return;
            }

            assemblyInfo = new AssemblyInfo(assemblyDefinition, file.FullName, this);

            _assembliesInfoByName.Add(assemblyInfo.OriginalName, assemblyInfo);
            Cache(this)[assemblyInfo.OriginalName] = assemblyInfo.AssemblyDefinition;
        }

        #endregion

        #region Getters

        public IReadOnlyCollection<PluginInfo> GetAllPlugins()
        {
            return _pluginsInfoByAssemblyName.Values.SelectMany(p => p).ToList();
        }

        public AssemblyDefinition GetAssemblyDefinitionFromFile(string filepath)
        {
            return AssemblyDefinition.ReadAssembly(filepath, new ReaderParameters { AssemblyResolver = this });
        }

        public AssemblyInfo? GetAssemblyInfoFromNameReference(AssemblyNameReference assemblyNameReference)
        {
            if (_assembliesInfoByName.TryGetValue(assemblyNameReference.Name, out var assemblyInfo))
                return assemblyInfo;
            return null;
        }

        public AssemblyInfo? GetAssemblyInfoByPlugin(string pluginName)
        {
            return _assembliesInfoByName.Values.FirstOrDefault(assemblyInfo => assemblyInfo.ContainsPlugin(pluginName));
        }

        public AssemblyInfo? GetAssemblyInfoByFilename(string fileName)
        {
            return _assembliesInfoByName.Values.FirstOrDefault(assemblyInfo => assemblyInfo.IsFile(fileName));
        }

        public void RemoveAssembly(AssemblyInfo assemblyInfo)
        {
            _assembliesInfoByName.Remove(assemblyInfo.OriginalName);
            _pluginsInfoByAssemblyName.Remove(assemblyInfo.OriginalName);
            assemblyInfo.Dispose();
        }

        #endregion
    }
}