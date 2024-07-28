#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using HarmonyLib;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Plugins;

#endregion

namespace Oxide.Ext.DllLoader.Model
{
    public class PluginInfo(AssemblyInfo assemblyInfo, Type type) : IDisposable, IEquatable<PluginInfo>
    {
        public readonly AssemblyInfo AssemblyInfo = assemblyInfo;
        public readonly string AssemblyName = assemblyInfo.OriginalName;
        public readonly string PluginFile = assemblyInfo.AssemblyFile;
        public readonly string PluginName = type.Name;
        public readonly Type PluginType = type;

        private List<string>? _pluginReferences;
        private Plugin? _plugin;

        public IReadOnlyCollection<string> PluginReferences => _pluginReferences ??= GetReferencesInPlugin();
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

        private List<string> GetReferencesInPlugin()
        {
            var pluginRefMembers = AccessTools.GetDeclaredFields(PluginType).OfType<MemberInfo>().Union(AccessTools.GetDeclaredProperties(PluginType));
            return pluginRefMembers
                .AsParallel()
                .SelectMany<MemberInfo, string?>(m =>
                {
                    var attribute = m.GetCustomAttribute<PluginReferenceAttribute>(true);
                    if (attribute == null)
                        return [];

                    var memberType = m.GetUnderlyingType();
                    if (!memberType!.IsSubclassOf(typeof(Plugin)))
                        return [];

                    //attribute.Name ?? m.Name
                    return [memberType.Name];
                })
                .Where(n => n != null)
                .Cast<string>()
                .ToList();
        }


        #region BasicImplementations

        public void Dispose()
        {
            _pluginReferences?.Clear();
            _pluginReferences = null;
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