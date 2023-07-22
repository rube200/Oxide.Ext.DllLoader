#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oxide.Core.Plugins;
using Oxide.Ext.DllLoader.Helper;

#endregion

namespace Oxide.Ext.DllLoader.Model
{
    public class AssemblyInfo : IEquatable<AssemblyInfo>
    {
        private readonly HashSet<string> _expectedPluginsName = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<PluginInfo> _pluginsInfo = new HashSet<PluginInfo>();
        public readonly string AssemblyFile;
        public readonly string OriginalName;
        public readonly DateTime LastWriteTimeUtc;
        private Assembly _assembly;


        public AssemblyInfo(string originalName, string filePath, DateTime lastWriteTimeUtc)
        {
            AssemblyFile = filePath;
            OriginalName = originalName;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }


        public Assembly Assembly
        {
            get => _assembly;
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
            var pluginsType = _assembly.GetDefinedTypes().GetAssignedTypes(typeof(Plugin));
            foreach (var pluginType in pluginsType)
            {
                var pluginInfo = new PluginInfo(pluginType, AssemblyFile);
                _pluginsInfo.Add(pluginInfo);
            }
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
    }
}