#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using HarmonyLib;
using Oxide.Core.Plugins;
using Oxide.Plugins;

#endregion

namespace Oxide.Ext.DllLoader.Model
{
    public class PluginInfo(Type type, string filePath) : IDisposable, IEquatable<PluginInfo>
    {
        public readonly string PluginFile = filePath;
        public readonly string PluginName = type.Name;
        public readonly Type PluginType = type;

        private Dictionary<string, Type>? _namePluginReferences;
        private Plugin? _plugin;

        private Dictionary<string, Type> NamePluginReferences => _namePluginReferences ??= GetReferencesInPlugin();
        public bool IsPluginLoaded => _plugin != null;

        public Plugin Plugin
        {
            get
            {
                if (_plugin != null)
                    return _plugin;


                return _plugin ??= (Plugin)Activator.CreateInstance(PluginType);
            }
        }


        private Dictionary<string, Type> GetReferencesInPlugin()
        {
            var pluginRefFields = AccessTools.GetDeclaredFields(PluginType);
            return pluginRefFields
                .AsParallel()
                .Select<FieldInfo, (string?, Type)> (f =>
                {
                    var attribute = f.GetCustomAttribute<PluginReferenceAttribute>(true);
                    var name = attribute != null ? attribute?.Name ?? f.Name : null;
                    return (name, f.FieldType);
                })
                .Where(f => f.Item1 != null)
                .ToDictionary(n => n.Item1!, t => t.Item2);
        }


        #region BasicImplementations

        public void Dispose()
        {
            _namePluginReferences?.Clear();
            _namePluginReferences = null;
            _plugin = null;
        }

        public bool Equals(PluginInfo other)
        {
            if (other is null)
                return false;

            return ReferenceEquals(this, other) || PluginName.Equals(other.PluginName);
        }

        public override bool Equals(object obj)
        {
            if (obj is not PluginInfo pluginInfo)
                return false;

            return Equals(pluginInfo);
        }

        public override int GetHashCode()
        {
            return PluginName.GetHashCode();
        }

        #endregion
    }
}