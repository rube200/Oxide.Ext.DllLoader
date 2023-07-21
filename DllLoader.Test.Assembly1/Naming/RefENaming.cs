using Oxide.Plugins;

namespace Test.Naming
{
    [Info("RefENaming", "Rube200", "1.0.0")]
    [Description("RefENaming is for testing")]
    public class RefENaming : RustPlugin
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