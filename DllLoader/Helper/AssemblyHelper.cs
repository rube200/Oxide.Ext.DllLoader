using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oxide.Core;
using Mono.Cecil;
using Oxide.Core.CSharp;
using Oxide.Core.Plugins;

namespace Oxide.Ext.DllLoader.Helper
{
    public static class AssemblyHelper
    {
        public static void ApplyPatches(this AssemblyDefinition assemblyDefinition, bool name = true, bool oxide = true)
        {
            var originalName = assemblyDefinition.Name.Name;
            Interface.Oxide.LogDebug("Patching assembly({0})...", originalName);

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (!name && !oxide)
            {
                Interface.Oxide.LogDebug("Not patches to apply to assembly({0})", originalName);
                return;
            }

            if (name)
            {
                Interface.Oxide.LogDebug("Patching assembly name...");
                assemblyDefinition.Name.Name = $"{assemblyDefinition.Name.Name}-{DateTime.UtcNow.Ticks}";
                Interface.Oxide.LogDebug("Patch name complete. New name({0}) | old name({1}).", originalName, assemblyDefinition.Name.Name);
            }

            if (oxide)
            {
                Interface.Oxide.LogDebug("Patching assembly oxide...");

                var modulePluginType = assemblyDefinition.MainModule.Import(typeof(Plugin)).Resolve();
                var pluginTypes = assemblyDefinition.GetDefinedTypes().GetAssignedTypes(modulePluginType);

                foreach (var pluginType in pluginTypes)
                    _ = new DirectCallMethod(assemblyDefinition.MainModule, pluginType/*, new ReaderParameters()*/);

                Interface.Oxide.LogDebug("Patch oxide complete.");
            }

            Interface.Oxide.LogDebug("All patches to assembly({0}) are completed.", originalName);
        }

        public static IEnumerable<Type> GetDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Interface.Oxide.LogException($"Fail to get types in assembly: {assembly.FullName}", ex);
                foreach (var loaderException in ex.LoaderExceptions)
                    Interface.Oxide.LogException("->", loaderException);

                return ex.Types.Where(tp => tp != null).ToArray();
            }
        }

        public static IEnumerable<Type> GetAssignedTypes(this IEnumerable<Type> types, Type target)
        {
            return types.Where(tp => tp.IsClass && !tp.IsAbstract && target.IsAssignableFrom(tp));
        }

        public static IEnumerable<TypeDefinition> GetDefinedTypes(this AssemblyDefinition assemblyDefinition)
        {
            return assemblyDefinition.MainModule.Types;
        }

        public static IEnumerable<TypeDefinition> GetAssignedTypes(this IEnumerable<TypeDefinition> types, TypeDefinition target)
        {
            return types.Where(tp => tp.IsClass && !tp.IsAbstract && target.IsAssignableFrom(tp));
        }

        #region IsAssignableFromForTypeDefinition
        public static bool IsTypeDefEqual(this TypeDefinition self, TypeDefinition other)
        {
            //some people report MetadataToken could be diff for the same type
            return self == other || self?.MetadataToken == other?.MetadataToken;
        }
        public static IEnumerable<TypeDefinition> EnumerateBaseClasses(this TypeDefinition classType)
        {
            for (var typeDefinition = classType; typeDefinition != null; typeDefinition = typeDefinition.BaseType?.Resolve())
                yield return typeDefinition;
        }
        public static IEnumerable<TypeDefinition> EnumerateBaseInterfaces(this TypeDefinition classType)
        {
            return classType.EnumerateBaseClasses().SelectMany(b => b.Interfaces.Select(i => i.Resolve()));
        }
        public static bool IsSubclassOf(this TypeDefinition child, TypeDefinition parent)
        {
            if (child == null || parent == null || child.IsTypeDefEqual(parent))
                return false;

            return child.EnumerateBaseClasses().Any(c => c.IsTypeDefEqual(parent));
        }
        public static bool ImplementInterface(this TypeDefinition child, TypeDefinition parentInterface)
        {
            return child.EnumerateBaseInterfaces().Any(t => t.IsTypeDefEqual(parentInterface) || child.ImplementInterface(parentInterface));
        }
        public static bool IsAssignableFrom(this TypeDefinition target, TypeDefinition source)
        {
            if (target == null || source == null)
                return false;

            if (target.IsTypeDefEqual(source))
                return true;

            if (source.IsSubclassOf(target))
                return true;

            if (target.IsInterface)
                return source.ImplementInterface(target);

            return false;
        }
        #endregion
    }
}
