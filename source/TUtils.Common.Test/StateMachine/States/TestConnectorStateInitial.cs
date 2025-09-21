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
        public override void OnSignalReconnect(SignalShouldReconnect signal)
        {
            Switch2State(Context.TestConnectorStateConnecting);
        }

        /// <inheritdoc />
        public override void OnSignalConnect(SignalShouldConnect signal)
        {
            Switch2State(Context.TestConnectorStateConnecting);
        }
    }
}
