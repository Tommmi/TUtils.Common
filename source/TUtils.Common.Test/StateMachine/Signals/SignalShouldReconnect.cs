using TUtils.Common.Test.StateMachine.Common;

namespace TUtils.Common.Test.StateMachine.Signals
{
    public class SignalShouldReconnect : TestConnectorSignal
    {
        /// <inheritdoc />
        public SignalShouldReconnect()
            : base(nameof(SignalShouldReconnect))
        {
        }

        /// <inheritdoc />
        public override void Trigger(ITestConnectorSignalExecuter signalExecuter)
        {
            signalExecuter.OnSignalReconnect(this);
        }
    }
}
