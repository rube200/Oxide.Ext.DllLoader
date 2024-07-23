#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Oxide.Core.Plugins;
using Oxide.Ext.DllLoader.Helper;

#endregion

namespace Oxide.Ext.DllLoader.Model
{
    public class AssemblyInfo(AssemblyDefinition assemblyDefinition, string filePath, DateTime lastWriteTimeUtc) : IEquatable<AssemblyInfo>
    {
        private readonly HashSet<string> _expectedPluginsName = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<PluginInfo> _pluginsInfo = [];

        public AssemblyDefinition AssemblyDefinition => assemblyDefinition;
        public string AssemblyFile => filePath;
        public DateTime LastWriteTimeUtc => lastWriteTimeUtc;


        private Assembly? _assembly;

        public string OriginalName => AssemblyDefinition.FullName;

        public Assembly Assembly
        {
            get => _assembly!;
            set
            {
                _assembly = value;
                RegisterPluginTypesFromAssembly();
            }
        }

        public bool IsAssemblyLoaded => Assembly != null;
        public IReadOnlyCollection<PluginInfo> PluginsInfo => _pluginsInfo;

        public IReadOnlyCollection<string> PluginsName => IsAssemblyLoaded
            ? PluginsInfo.Select(pl => pl.PluginName).ToHashSet()
            : _expectedPluginsName;


        public bool Equals(AssemblyInfo other)
        {
            if (other is null)
                return false;

            return ReferenceEquals(this, other) ||
                   OriginalName.Equals(other.OriginalName, StringComparison.OrdinalIgnoreCase);
        }

        ~AssemblyInfo()
        {
            _expectedPluginsName.Clear();
            _pluginsInfo.Clear();
        }


        private void RegisterPluginTypesFromAssembly()
        {
            var pluginsType = _assembly!.GetDefinedTypes().GetAssignedTypes(typeof(Plugin));
            pluginsType.Do(p => _pluginsInfo.Add(new PluginInfo(p, filePath)));
        }

        internal void RegisterPluginName(string pluginName)
        {
            _expectedPluginsName.Add(pluginName);
        }

        public bool ContainsPlugin(string name)
        {
            return PluginsName.Any(pl => pl.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public PluginInfo GetPluginInfo(string name)
        {
            return _pluginsInfo.FirstOrDefault(pl => pl.PluginName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((AssemblyInfo)obj);
        }

        public override int GetHashCode()
        {
            return OriginalName != null ? OriginalName.GetHashCode() : 0;
        }

        public bool IsFile(string filename)
        {
            if (filePath.Equals(filename, StringComparison.OrdinalIgnoreCase))
                return true;

            var file = Path.GetFileNameWithoutExtension(filePath);
            if (file.Equals(filename, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}