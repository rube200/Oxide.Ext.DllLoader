#region

using System;
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
        public static string GetAssemblyNameFromFile(string filepath)
        {
            return AssemblyDefinition.ReadAssembly(filepath).FullName;
        }

        public static AssemblyInfo LoadAssemblyInfo(string filepath, DateTime lastWriteUtc)
        {
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(filepath);
            var assemblyInfo = new AssemblyInfo(assemblyDefinition.FullName, filepath, lastWriteUtc);

            var modulePluginType = assemblyDefinition.MainModule.Import(typeof(Plugin)).Resolve();
            var pluginTypes = assemblyDefinition.GetDefinedTypes().GetAssignedTypes(modulePluginType);

            foreach (var pluginType in pluginTypes)
                assemblyInfo.RegisterPluginName(pluginType.Name);

            return assemblyInfo;
        }

        public static bool LoadAssembly(AssemblyInfo assemblyInfo)
        {
            Interface.Oxide.LogDebug("Loading assembly({0}) from assembly info.", assemblyInfo.OriginalName);
            if (!File.Exists(assemblyInfo.AssemblyFile))
            {
                Interface.Oxide.LogError("Fail to load assembly({0}), file({1}) does not exist.",
                    assemblyInfo.OriginalName, assemblyInfo.AssemblyFile);
                return false;
            }

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyInfo.AssemblyFile);
            var symbols = GetAssemblySymbols(assemblyInfo.AssemblyFile);

            assemblyDefinition.ApplyPatches(true, assemblyInfo.PluginsName.Count > 0);

            using (var stream = new MemoryStream())
            {
                assemblyDefinition.Write(stream);
                assemblyInfo.Assembly = Assembly.Load(stream.ToArray(), symbols);
            }

            return true;
        }

        private static byte[] GetAssemblySymbols(string filepath)
        {
            filepath = Path.ChangeExtension(filepath, ".pdb");
            Interface.Oxide.LogDebug("Getting debug symbols from file({0})", filepath);

            if (!File.Exists(filepath))
            {
                Interface.Oxide.LogDebug("Fail to get debug symbols, file({0}) do not exist.", filepath);
                return null;
            }

            using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var symbolsData = new byte[fileStream.Length];
                var count = fileStream.Read(symbolsData, 0, symbolsData.Length);
                if (count != symbolsData.Length)
                {
                    Interface.Oxide.LogDebug("Fail to load symbols({0}) {1}bytes of {2}bytes.", symbolsData, count,
                        symbolsData.Length);
                    return null;
                }

                return symbolsData;
            }
        }
    }
}