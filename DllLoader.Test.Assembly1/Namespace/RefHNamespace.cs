using Oxide.Plugins;

namespace Test.Namespace
{
    [Info("RefHNamespace", "Rube200", "1.0.0")]
    [Description("RefHNamespace is for testing")]
    public class RefHNamespace : RustPlugin
    {
        void Init()
        {
            var selfName = GetType().Name;
            Puts($"I am alive {selfName}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}