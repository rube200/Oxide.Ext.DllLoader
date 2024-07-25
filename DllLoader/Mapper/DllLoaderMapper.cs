#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Oxide.Core;
using Oxide.Core.CSharp;
using Oxide.Ext.DllLoader.API;
using Oxide.Ext.DllLoader.Helper;
using Oxide.Ext.DllLoader.Model;
using static HarmonyLib.AccessTools;

#endregion

namespace Oxide.Ext.DllLoader.Mapper
{
    public sealed class DllLoaderMapper : DefaultAssemblyResolver, IDllLoaderMapperLoadable
    {
        private readonly IDictionary<string, AssemblyInfo> _assembliesInfoByFullName =
            new Dictionary<string, AssemblyInfo>(StringComparer.OrdinalIgnoreCase);

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
            _assembliesInfoByFullName.Clear();
        }

        private Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assemblyName = AssemblyNameReference.Parse(args.Name);
                var assemblyInfo = GetAssemblyInfoFromNameReference(assemblyName);
                if (assemblyInfo == null)
                    return null;

                if (assemblyInfo.IsAssemblyLoaded || LoadAssembly(assemblyInfo))
                    return assemblyInfo.Assembly;
            }
            catch (Exception)
            { }
            
            return null;
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
            Interface.Oxide.LogDebug("Total assemblies registered({0}).", _assembliesInfoByFullName.Count);
#endif

            var plugins = _assembliesInfoByFullName.Values.SelectMany(ai => ai.PluginsName).ToArray();
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
            dirFiles.Do(file => RegisterAssemblyFromFile(file));
        }

        private static readonly FieldRef<object, IDictionary<string, AssemblyDefinition>> Cache = FieldRefAccess<IDictionary<string, AssemblyDefinition>>(typeof(DllLoaderMapper), "cache");
        public bool RegisterAssemblyFromFile(FileInfo file)
        {
            if (!file.Exists)
            {
                Interface.Oxide.LogError("Fail to load assembly({0}), file does not exist.", file.FullName);
                return false;
            }

#if DEBUG
            Interface.Oxide.LogDebug("Found file({0}) in directory({1}).", file.Name, file.DirectoryName);
#endif
            if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                Interface.Oxide.LogInfo("Ignoring file({0}), marked as hidden.", file.Name);
                return false;
            }

            var assemblyDefinition = GetAssemblyDefinitionFromFile(file.FullName);
            var assemblyDefinitionName = assemblyDefinition.Name;

            var assemblyInfo = GetAssemblyInfoFromNameReference(assemblyDefinitionName);
            if (assemblyInfo != null)
            {
#if DEBUG
                Interface.Oxide.LogDebug("Assembly({0}) already registered.", file.FullName);
#endif
                return false;
            }

            assemblyInfo = new AssemblyInfo(assemblyDefinition, file.FullName);
            if (assemblyInfo == null)
            {
                Interface.Oxide.LogError("Fail to load assembly({0})", file.FullName);
                return false;
            }

#if DEBUG
            Interface.Oxide.LogDebug("Assembly({0}) loaded from file({1}) in directory({2}).", assemblyInfo.OriginalName, file.Name, file.Directory);
#endif

            _assembliesInfoByFullName[assemblyDefinition.FullName] = assemblyInfo;
            Cache(this)[assemblyDefinitionName.FullName] = assemblyInfo.AssemblyDefinition;
            return true;
        }

        #endregion

        #region Getters

        public static AssemblyDefinition GetAssemblyDefinitionFromFile(string filepath, IAssemblyResolver? assemblyResolver = null)
        {
            return AssemblyDefinition.ReadAssembly(filepath, new ReaderParameters { AssemblyResolver = assemblyResolver });
        }

        public AssemblyInfo? GetAssemblyInfoFromNameReference(AssemblyNameReference assemblyNameReference)
        {
            if (_assembliesInfoByFullName.TryGetValue(assemblyNameReference.FullName, out var assemblyInfo))
                return assemblyInfo;

            return null;
        }

        public AssemblyInfo GetAssemblyInfoByPlugin(string pluginName)
        {
            return _assembliesInfoByFullName.Values.FirstOrDefault(assemblyInfo => assemblyInfo.ContainsPlugin(pluginName));
        }




        /*


        public AssemblyInfo? GetAssemblyInfoByName(string name)
        {
            if (_assembliesInfoByName.TryGetValue(name, out var assemblyInfo))
                return assemblyInfo;

            return null;
        }

        public AssemblyInfo GetAssemblyInfoByFile(string filename)
        {
            return _assembliesInfoByName.Values.FirstOrDefault(assemblyInfo => assemblyInfo.IsFile(filename));
        }



        public PluginInfo GetPluginInfoByName(string pluginName)
        {
            return _assembliesInfoByName.Values.Select(assemblyInfo => assemblyInfo.GetPluginInfo(pluginName))
                .FirstOrDefault(pluginInfo => pluginInfo != null);
        }*/

        #endregion

        public bool LoadAssembly(AssemblyInfo assemblyInfo)
        {
#if DEBUG
            Interface.Oxide.LogDebug("Loading assembly({0}) from assembly info.", assemblyInfo.OriginalName);
#endif
            if (!File.Exists(assemblyInfo.AssemblyFile))
            {
                Interface.Oxide.LogError("Fail to load assembly({0}), file({1}) does not exist.",
                    assemblyInfo.OriginalName, assemblyInfo.AssemblyFile);
                return false;
            }

            ApplyPatches(assemblyInfo);
            return true;
        }

        public void ApplyPatches(AssemblyInfo assemblyInfo)
        {
            var assemblyDefinition = assemblyInfo.AssemblyDefinition;
            var originalName = assemblyDefinition.Name.Name;
#if DEBUG
            Interface.Oxide.LogDebug("Patching assembly({0})...", originalName);
            Interface.Oxide.LogDebug("Patching assembly name...");
#endif
            assemblyDefinition.Name.Name = $"{assemblyDefinition.Name.Name}-{DateTime.UtcNow.Ticks}";
#if DEBUG
            Interface.Oxide.LogDebug("Patch name complete. New name({0}) | Old name({1}).", assemblyDefinition.Name.Name, originalName);
#endif

            if (assemblyInfo.PluginsName.Count > 0)
            {
#if DEBUG
                Interface.Oxide.LogDebug("Patching assembly oxide...");
#endif
                var pluginTypes = assemblyDefinition.GetPluginTypes();
                foreach (var pluginType in pluginTypes)
                    _ = new DirectCallMethod(pluginType.Module, pluginType, new ReaderParameters { AssemblyResolver = this });

#if DEBUG
                Interface.Oxide.LogDebug("Patch oxide complete.");
#endif
            }

#if DEBUG
            Interface.Oxide.LogDebug("All patches to assembly({0}) are completed.", originalName);
#endif
        }

        public bool RemoveAssemblyInfo(AssemblyNameReference assemblyNameReference)
        {
            return _assembliesInfoByFullName.Remove(assemblyNameReference.FullName);
        }
    }
}