using TUtils.Common.StateMachine;

namespace TUtils.Common.Test.StateMachine.Common;

public abstract class TestConnectorSignal : Signal<TestConnectorSignal, ITestConnectorSignalExecuter>
{
    /// <inheritdoc />
    protected TestConnectorSignal(string signalName)
        : base(signalName)
    {
    }
}