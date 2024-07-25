#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Oxide.Core;
using Oxide.Core.Plugins;

#endregion

namespace Oxide.Ext.DllLoader.Helper
{
    public static class AssemblyHelper
    {
        #region MonoCecilHelpers

        public static IEnumerable<TypeDefinition> GetPluginTypes(this AssemblyDefinition assemblyDefinition)
        {
            var modulePluginType = assemblyDefinition.MainModule.Import(typeof(Plugin)).Resolve();
            return assemblyDefinition.GetDefinedTypes().GetAssignedTypes(modulePluginType);
        }

        public static IEnumerable<TypeDefinition> GetDefinedTypes(this AssemblyDefinition assemblyDefinition)
        {
            return assemblyDefinition.Modules.SelectMany(m => m.Types);
        }

        public static IEnumerable<TypeDefinition> GetAssignedTypes(this IEnumerable<TypeDefinition> typesToCheck, TypeDefinition? typeWanted)
        {
            return typesToCheck.Where(tp => tp.IsClass && !tp.IsAbstract && tp.IsAssignableTo(typeWanted));
        }

        public static bool IsAssignableTo(this TypeDefinition? typeToCheck, TypeDefinition? typeWanted)
        {
            if (typeToCheck == null || typeWanted == null)
                return false;

            if (typeToCheck.IsTypeDefEqual(typeWanted))
                return true;

            if (typeToCheck.IsSubclassOf(typeWanted))
                return true;

            if (typeToCheck.IsInterface)
                return typeToCheck.ImplementInterface(typeWanted);

            return false;
        }

        public static bool IsTypeDefEqual(this TypeDefinition? typeToCheck, TypeDefinition? typeWanted)
        {
            //some people report MetadataToken could be diff for the same type
            return typeToCheck == typeWanted || typeToCheck?.MetadataToken == typeWanted?.MetadataToken;
        }

        public static bool IsSubclassOf(this TypeDefinition? typeToCheck, TypeDefinition? typeWanted)
        {
            if (typeToCheck == null || typeWanted == null || IsTypeDefEqual(typeToCheck, typeWanted))
                return false;

            return typeToCheck.GetAllParents().Any(tp => tp.IsTypeDefEqual(typeWanted));
        }

        public static IEnumerable<TypeDefinition> GetAllParents(this TypeDefinition? typeToCheck)
        {
            for (var typeDefinition = typeToCheck;
                 typeDefinition != null;
                 typeDefinition = typeDefinition.BaseType?.Resolve())
                yield return typeDefinition;
        }

        public static bool ImplementInterface(this TypeDefinition? typeToCheck, TypeDefinition? typeWanted)
        {
            if (typeToCheck == null || typeWanted == null)
                return false;

            if (IsTypeDefEqual(typeToCheck, typeWanted))
                return true;

            if (typeToCheck.Interfaces.Select(tp => tp.Resolve()).Any(tp => tp.IsTypeDefEqual(typeWanted) || tp.ImplementInterface(typeWanted)))
                return true;

            return typeToCheck.GetAllParents().Any(tp => tp.ImplementInterface(typeWanted));
        }

        public static bool IsAssignableTo(this IEnumerable<TypeDefinition> typeToCheck, TypeDefinition? typeWanted)
        {
            return typeToCheck.Any(tp => tp.IsAssignableTo(typeWanted));
        }

        #endregion

        public static Type[] GetDefinedTypes(this Assembly assembly)
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

        public static IEnumerable<Type> GetAssignedTypes(this IEnumerable<Type> typesToCheck, Type typeWanted)
        {
            return typesToCheck.Where(tp => tp.IsClass && !tp.IsAbstract && typeWanted.IsAssignableFrom(tp));
        }
    }
}