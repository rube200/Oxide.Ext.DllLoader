namespace Oxide.Plugins
{
    [Info("RefVDeep", "Rube200", "1.0.0")]
    [Description("RefVDeep is for testing")]
    public class RefVDeep : RustPlugin
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