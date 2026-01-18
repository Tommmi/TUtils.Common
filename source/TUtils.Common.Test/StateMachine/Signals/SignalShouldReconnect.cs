using System.Threading.Tasks;
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
        public override async Task Trigger(ITestConnectorSignalExecuter signalExecuter)
        {
            await signalExecuter.OnSignalReconnect(this);
        }
    }
}
