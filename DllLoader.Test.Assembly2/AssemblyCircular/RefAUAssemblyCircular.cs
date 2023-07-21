using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAUAssemblyCircular", "Rube200", "1.0.0")]
    [Description("RefAUAssemblyCircular is for testing")]
    public class RefAUAssemblyCircular : RustPlugin
    {
        [PluginReference]
        Plugin RefATAssemblyCircular;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefATAssemblyCircular != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefATAssemblyCircular != null}");
            RefATAssemblyCircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}