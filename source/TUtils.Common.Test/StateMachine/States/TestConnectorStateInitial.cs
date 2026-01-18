using System.Threading.Tasks;
using TUtils.Common.Test.StateMachine.Common;
using TUtils.Common.Test.StateMachine.Signals;

namespace TUtils.Common.Test.StateMachine.States
{
    public class TestConnectorStateInitial : TestConnectorState
	{
        /// <inheritdoc />
        public TestConnectorStateInitial(TestConnectorContext context, TestConnectorStateMachine stateMachine)
            : base(stateName:nameof(TestConnectorStateInitial), context, stateMachine)
        {
        }

        /// <inheritdoc />
        public override async Task OnSignalReconnect(SignalShouldReconnect signal)
        {
            await Switch2State(Context.TestConnectorStateConnecting);
        }

        /// <inheritdoc />
        public override async Task OnSignalConnect(SignalShouldConnect signal)
        {
            await Switch2State(Context.TestConnectorStateConnecting);
        }
    }
}
