using System;
using System.Collections.Generic;
using System.Reflection;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Oxide.Ext.DllLoader.Model
{
    public class PluginInfo : IEquatable<PluginInfo>
    {
        public Plugin Plugin;
        public readonly string PluginFile;
        public readonly Type PluginType;
        private readonly HashSet<string> _referencedPlugins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


        public string PluginName => PluginType.Name;
        public bool IsPluginLoaded => Plugin != null;
        public IReadOnlyCollection<string> ReferencedPlugins => _referencedPlugins;


        public PluginInfo(Type pluginType, string filePath)
        {
            PluginFile = filePath;
            PluginType = pluginType;
            ScanForReferencedPlugins();
        }
        ~PluginInfo()
        {
            _referencedPlugins.Clear();
        }


        private void ScanForReferencedPlugins()
        {
            var pluginRefFields = PluginType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in pluginRefFields)
            {
                var pluginRefAttributes = field.GetCustomAttributes<PluginReferenceAttribute>(true);
                foreach (var attribute in pluginRefAttributes)
                    _referencedPlugins.Add(attribute.Name ?? field.Name);
            }
        }

        public bool Equals(PluginInfo other)
        {
            if (other is null)
                return false;

            return ReferenceEquals(this, other) || PluginName.Equals(other.PluginName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((PluginInfo)obj);
        }

        public override int GetHashCode()
        {
            return PluginName != null ? PluginName.GetHashCode() : 0;
        }
    }
}