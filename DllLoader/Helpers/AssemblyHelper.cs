using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Oxide.Core;
using System.Linq;
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

        public static (string, string, string) GetNameFromAssemblyDefinitionRow(AssemblyDefinitionRow assemblyRow, StringsStream stringsStream)
        {
            var name = stringsStream.GetStringByIndex(assemblyRow.Name);
            var version = new Version(assemblyRow.MajorVersion, assemblyRow.MinorVersion, assemblyRow.BuildNumber, assemblyRow.RevisionNumber);
            var culture = stringsStream.GetStringByIndex(assemblyRow.Culture);

            var originalFullName = $"{name}, Version={version}, Culture={culture}, PublicKeyToken=null";
            var newName = $"{name}-{DateTime.UtcNow.Ticks}";

            return (name, originalFullName, newName);
        }

        //returns the patched assembly(unless it fails) and also returns original name
        public static (byte[], string) PatchAssemblyName(byte[] assemblyData)
        {
            Interface.Oxide.LogDebug("Patching assembly name...");
            try
            {
                var peImage = PEImage.FromBytes(assemblyData);

                var metadata = peImage.DotNetDirectory?.Metadata;
                if (metadata == null)
                    return (assemblyData, null);

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
                return (assemblyData, originalFullName);
            }
            catch (Exception ex)
            {
                Interface.Oxide.LogException("Fail to patch assembly name.", ex);
                return (assemblyData, null);
            }
        }
    }
}
