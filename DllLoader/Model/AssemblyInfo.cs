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
using Oxide.Core.Plugins;
using Oxide.Ext.DllLoader.Helper;

#endregion

namespace Oxide.Ext.DllLoader.Model
{
    public class AssemblyInfo(AssemblyDefinition assemblyDefinition, string filePath, IAssemblyResolver assemblyResolver) : IDisposable, IEquatable<AssemblyInfo>
    {
        public readonly string AssemblyFile = filePath;
        public readonly string AssemblyFileNoExt = Path.GetFileNameWithoutExtension(filePath);
        public readonly string OriginalName = assemblyDefinition.Name.Name;

        private Assembly? _assembly;
        private AssemblyDefinition? _assemblyDefinition;
        private Dictionary<string, PluginInfo>? _namePluginInfo;

        public bool IsAssemblyLoaded => _assembly != null;
        private Dictionary<string, PluginInfo> NamePluginInfo => _namePluginInfo ??= GetPluginsInAssembly();
        public IReadOnlyCollection<string> PluginsName => NamePluginInfo.Keys;
        public IReadOnlyCollection<PluginInfo> PluginsInfo => NamePluginInfo.Values;

        public Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                {
                    using var stream = new MemoryStream();
                    AssemblyDefinition.Write(stream);
                    _assembly = Assembly.Load(stream.ToArray());
                }
               
                return _assembly;
            }
        }

        public AssemblyDefinition AssemblyDefinition
        {
            get
            {
                if (_assemblyDefinition == null)
                {
                    _assemblyDefinition = AssemblyDefinition.ReadAssembly(AssemblyFile, new ReaderParameters { AssemblyResolver = assemblyResolver });
                    ApplyPatches();
                }

                return _assemblyDefinition;
            }
        }


        public bool ContainsPlugin(string pluginName)
        {
            return PluginsName.Contains(pluginName);
        }

        public PluginInfo? GetPluginInfo(string pluginName)
        {
            return NamePluginInfo.TryGetValue(pluginName, out var pluginInfo) ? pluginInfo : null;
        }

        private Dictionary<string, PluginInfo> GetPluginsInAssembly()
        {
            var pluginsType = Assembly.GetDefinedTypes().GetAssignedTypes(typeof(Plugin));
            return pluginsType.Select(p => new PluginInfo(this, p)).ToDictionary(p => p.PluginName);
        }

        public bool IsFile(string fileName)
        {
            return AssemblyFile.Equals(fileName, StringComparison.OrdinalIgnoreCase) || AssemblyFileNoExt.Equals(fileName, StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyPatches()
        {
            _assemblyDefinition!.Name.Name = $"{_assemblyDefinition.Name.Name}-{DateTime.UtcNow.Ticks}";
            var pluginTypes = _assemblyDefinition.GetPluginTypes();
            foreach (var pluginType in pluginTypes)
                _ = new DirectCallMethod(pluginType.Module, pluginType, new ReaderParameters { AssemblyResolver = assemblyResolver });
        }

        #region BasicImplementations

        public void Dispose()
        {
            _namePluginInfo?.Values.Do(p => p.Dispose());
            _namePluginInfo?.Clear();
            _namePluginInfo = null;
            _assemblyDefinition = null;
            _assembly = null;
        }

        public bool Equals(AssemblyInfo other)
        {
            if (other is null)
                return false;

            return ReferenceEquals(this, other) || OriginalName.Equals(other.OriginalName);
        }

        public override bool Equals(object obj)
        {
            if (obj is not AssemblyInfo assemblyInfo)
                return false;

            return Equals(assemblyInfo);
        }

        public override int GetHashCode()
        {
            return OriginalName.GetHashCode();
        }

        #endregion
    }
}