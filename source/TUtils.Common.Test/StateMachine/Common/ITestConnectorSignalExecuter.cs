using System.Threading.Tasks;
using TUtils.Common.Test.StateMachine.Signals;

namespace TUtils.Common.Test.StateMachine.Common;

public interface ITestConnectorSignalExecuter
{
    Task OnSignalReconnect(SignalShouldReconnect signal);
    Task OnSignalConnect(SignalShouldConnect signal);
}
