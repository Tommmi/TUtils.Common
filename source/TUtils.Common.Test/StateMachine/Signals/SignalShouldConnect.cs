using TUtils.Common.Test.StateMachine.Common;

namespace TUtils.Common.Test.StateMachine.Signals;

public class SignalShouldConnect : TestConnectorSignal
{
    /// <inheritdoc />
    public SignalShouldConnect()
        : base(signalName: nameof(SignalShouldConnect))
    {
    }

    /// <inheritdoc />
    public override void Trigger(ITestConnectorSignalExecuter signalExecuter)
    {
        signalExecuter.OnSignalConnect(this);
    }
}
