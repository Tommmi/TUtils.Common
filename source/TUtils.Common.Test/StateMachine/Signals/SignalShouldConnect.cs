using System.Threading.Tasks;
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
    public override async Task Trigger(ITestConnectorSignalExecuter signalExecuter)
    {
        await signalExecuter.OnSignalConnect(this);
    }
}
