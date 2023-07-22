#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefAUAssemblyCircular", "Rube200", "1.0.0")]
    [Description("RefAUAssemblyCircular is for testing")]
    public class RefAUAssemblyCircular : RustPlugin
    {
        [PluginReference] private Plugin RefATAssemblyCircular;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefATAssemblyCircular != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefATAssemblyCircular != null}");
                RefATAssemblyCircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}