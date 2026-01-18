using TUtils.Common.StateMachine;
using TUtils.Common.Test.StateMachine.Common;

namespace TUtils.Common.Test.StateMachine;

public class TestConnectorStateMachine : StateMachine<TestConnectorState, TestConnectorSignal, TestConnectorContext,ITestConnectorSignalExecuter>
{
    /// <inheritdoc />
    public TestConnectorStateMachine(string stateMachineName)
        : base(stateMachineName)
    {
    }
}