using System.Data;
using TUtils.Common.Logging;
using TUtils.Common.StateMachine;
using TUtils.Common.Test.StateMachine.Common;
using TUtils.Common.Test.StateMachine.Signals;

namespace TUtils.Common.Test.StateMachine.States;

public class TestConnectorStateConnected : TestConnectorState
{
    /// <inheritdoc />
    public TestConnectorStateConnected(TestConnectorContext context, IStateMachine4State<TestConnectorState, TestConnectorSignal, ITestConnectorSignalExecuter> stateMachine)
        : base(stateName:nameof(TestConnectorStateConnected), context, stateMachine)
    {
    }

    /// <inheritdoc />
    public override void OnSignalReconnect(SignalShouldReconnect signal)
    {
        Switch2State(Context.TestConnectorStateReconnecting);
    }

    /// <inheritdoc />
    public override void OnSignalConnect(SignalShouldConnect signal)
    {
        
    }

    /// <inheritdoc />
    public override void OnEntered()
    {
        this.Log().LogInfo(() => new { newState = GetType().Name });
    }
}
