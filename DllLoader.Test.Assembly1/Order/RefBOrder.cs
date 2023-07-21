namespace Oxide.Plugins
{
    [Info("RefBOrder", "Rube200", "1.0.0")]
    [Description("RefBOrder is for testing")]
    public class RefBOrder : RustPlugin
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