using System;
using System.Threading.Tasks;
using TUtils.Common.Logging;
using TUtils.Common.StateMachine;
using TUtils.Common.Test.StateMachine.Common;
using TUtils.Common.Test.StateMachine.Signals;

namespace TUtils.Common.Test.StateMachine.States;

public class TestConnectorStateReconnecting : TestConnectorState, IDisposable
{
    private bool _disposed;
    private TimeSpan _connectingTrialInterval;

    /// <inheritdoc />
    public TestConnectorStateReconnecting(TestConnectorContext context, 
                                       IStateMachine4State<TestConnectorState, TestConnectorSignal, ITestConnectorSignalExecuter> stateMachine,
                                       TimeSpan connectingTrialInterval)
        : base(stateName: nameof(TestConnectorStateReconnecting), context, stateMachine)
    {
        _connectingTrialInterval = connectingTrialInterval;
    }

    public override async Task OnEntered()
    {
        this.Log().LogInfo(() => new { newState = GetType().Name });
        StartReconnectTask();
    }

    private void StartReconnectTask()
    {
        _ = Task.Run(ReconnectLongRunningTask);
    }

    private async Task ReconnectLongRunningTask()
    {
        while (!await Context.ConnectableService.TryReconnect())
        {
            for (int i = 0; i < _connectingTrialInterval.TotalMilliseconds / 100; i++)
            {
                if (_disposed)
                {
                    return;
                }

                await Task.Delay(100);
            }
        }

        await Switch2State(Context.TestConnectorStateConnected);
    }

    /// <inheritdoc />
    public override async Task OnSignalReconnect(SignalShouldReconnect signal)
    {
        
    }

    /// <inheritdoc />
    public override async Task OnSignalConnect(SignalShouldConnect signal)
    {
        
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposed = true;
    }
}
