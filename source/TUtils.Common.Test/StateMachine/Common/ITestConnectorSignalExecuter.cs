using TUtils.Common.Test.StateMachine.Signals;

namespace TUtils.Common.Test.StateMachine.Common;

public interface ITestConnectorSignalExecuter
{
    void OnSignalReconnect(SignalShouldReconnect signal);
    void OnSignalConnect(SignalShouldConnect signal);
}
