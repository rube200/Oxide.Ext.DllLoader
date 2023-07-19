using System;
using System.Collections.Generic;
using System.Reflection;
using Oxide.Core;
using System.Linq;
using Mono.Cecil;
using Oxide.Core.Plugins;

namespace Oxide.Ext.DllLoader.Helpers
{
    public static class AssemblyHelper
    {
        public static ISet<Type> GetPlugins(this Assembly assembly)
        {
            Type[] assemblyTypes;
            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Interface.Oxide.LogException($"Fail to get types in assembly: {assembly.FullName}", ex);
                foreach (var loaderException in ex.LoaderExceptions)
                    Interface.Oxide.LogException("->", loaderException);

                assemblyTypes = ex.Types.Where(tp => tp != null).ToArray();
            }

            var pluginsType = new HashSet<Type>();
            foreach (var pluginType in assemblyTypes.Where(tp => !tp.IsAbstract && typeof(Plugin).IsAssignableFrom(tp)))
            {
                Interface.Oxide.LogDebug("Plugin found({0}) at {1}", pluginType.Name, assembly.FullName);
                pluginsType.Add(pluginType);
            }

            return pluginsType;
        }

        //returns the patched assembly(unless it fails) and also returns original name
        public static void PatchAssembly(this AssemblyDefinition assemblyDefinition, bool patchName = true, bool patchOxide = true)
        {
            Interface.Oxide.LogDebug("Patching assembly name({0}), oxide({1})...", patchName, patchOxide);
            try
            {
                var originalName = assemblyDefinition.FullName;
                if (patchName)
                    assemblyDefinition.Name.Name = $"{originalName}-{DateTime.UtcNow.Ticks}";

                /*
                var stringsStream = metadata.GetStream<StringsStream>();
                var stringsStreamIndex = metadata.Streams.IndexOf(stringsStream);

                var tablesStream = metadata.GetStream<TablesStream>();
                ref var assemblyRow = ref tablesStream.GetTable<AssemblyDefinitionRow>().GetRowRef(1);

                var (name, originalFullName, newName) = GetNameFromAssemblyDefinitionRow(assemblyRow, stringsStream);

                using (var stream = new MemoryStream())
                {
                    var streamWriter = new BinaryStreamWriter(stream);

                    //Writes StringStream to streamWriter
                    stringsStream.Write(streamWriter);

                    //Change assembly name index
                    assemblyRow.Name = streamWriter.Length;

                    if (streamWriter.Length != stringsStream.GetPhysicalSize()) //todo remove
                    {
                        Interface.Oxide.LogWarning("StreamWriter Length({0}) != StringStream PhysicalSize({1})", streamWriter.Length, stringsStream.GetPhysicalSize());
                        return (assemblyData, originalFullName);
                    }

                    streamWriter.WriteBytes(Encoding.UTF8.GetBytes(newName));
                    streamWriter.WriteByte(0);
                    streamWriter.Align(4);

                    stringsStream = new SerializedStringsStream(stream.ToArray());
                }

                tablesStream.StringIndexSize = stringsStream.IndexSize;
                metadata.Streams[stringsStreamIndex] = stringsStream;

                var builder = new ManagedPEFileBuilder();
                using (var outputData = new MemoryStream())
                {
                    builder.CreateFile(peImage).Write(outputData);
                    assemblyData = outputData.ToArray();
                }

                Interface.Oxide.LogDebug("Assembly name patched.{2}Old name:{0}{2}New name:{1}", name, newName, Environment.NewLine);
                return (assemblyData, originalFullName);*/
            }
            catch (Exception ex)
            {
                Interface.Oxide.LogException("Fail to patch assembly name.", ex);
            }
        }
    }
}
