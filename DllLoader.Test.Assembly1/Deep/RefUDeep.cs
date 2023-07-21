namespace Oxide.Plugins
{
    [Info("RefUDeep", "Rube200", "1.0.0")]
    [Description("RefUDeep is for testing")]
    public class RefUDeep : RustPlugin
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