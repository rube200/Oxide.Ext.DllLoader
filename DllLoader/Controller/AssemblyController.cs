#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Ext.DllLoader.Helper;
using Oxide.Ext.DllLoader.Model;

#endregion

namespace Oxide.Ext.DllLoader.Controller
{
    public static class AssemblyController
    {
        public static AssemblyNameDefinition GetAssemblyNameFromFile(string filepath)
        {
            return AssemblyDefinition.ReadAssembly(filepath).Name;
        }

        public static AssemblyInfo LoadAssemblyInfo(string filepath, DateTime lastWriteUtc, IAssemblyResolver? assemblyResolver = null)
        {
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(filepath, new ReaderParameters
            {
                AssemblyResolver = assemblyResolver,
            });
            return new AssemblyInfo(assemblyDefinition, filepath, lastWriteUtc);
        }

        public static IEnumerable<TypeDefinition> GetPluginTypes(AssemblyDefinition assemblyDefinition)
        {
            var modulePluginType = assemblyDefinition.MainModule.Import(typeof(Plugin)).Resolve();
            return assemblyDefinition.GetDefinedTypes().GetAssignedTypes(modulePluginType);
        }

        public static void RegisterAssemblyPlugins(AssemblyInfo assemblyInfo)
        {
            var pluginTypes = GetPluginTypes(assemblyInfo.AssemblyDefinition);
            foreach (var pluginType in pluginTypes)
                assemblyInfo.RegisterPluginName(pluginType.Name);
        }

        public static bool LoadAssembly(AssemblyInfo assemblyInfo, IAssemblyResolver? assemblyResolver = null)
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

            assemblyInfo.AssemblyDefinition.ApplyPatches(true, assemblyInfo.PluginsName.Count > 0, assemblyResolver);

            using var stream = new MemoryStream();
            assemblyInfo.AssemblyDefinition.Write(stream);
            assemblyInfo.Assembly = Assembly.Load(stream.ToArray());

            return true;
        }
    }
}