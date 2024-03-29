#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefATAssemblyCircular", "Rube200", "1.0.0")]
    [Description("RefATAssemblyCircular is for testing")]
    public class RefATAssemblyCircular : RustPlugin
    {
        [PluginReference] private Plugin RefAUAssemblyCircular;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAUAssemblyCircular != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefAUAssemblyCircular != null}");
                RefAUAssemblyCircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}