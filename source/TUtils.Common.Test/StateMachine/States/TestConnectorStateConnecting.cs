using System;
using System.Threading.Tasks;
using TUtils.Common.Logging;
using TUtils.Common.StateMachine;
using TUtils.Common.Test.StateMachine.Common;
using TUtils.Common.Test.StateMachine.Signals;

namespace TUtils.Common.Test.StateMachine.States
{
    public class TestConnectorStateConnecting : TestConnectorState, IDisposable
    {
        private bool _disposed;
        private TimeSpan _connectingTrialInterval;

        /// <inheritdoc />
        public TestConnectorStateConnecting(TestConnectorContext context, 
                                         IStateMachine4State<TestConnectorState, TestConnectorSignal, ITestConnectorSignalExecuter> stateMachine,
                                         TimeSpan connectingTrialInterval)
            : base(stateName:nameof(TestConnectorStateConnecting), context, stateMachine)
        {
            _connectingTrialInterval = connectingTrialInterval;
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
        public override async Task OnEntered()
        {
            this.Log().LogInfo(()=>new { newState = GetType().Name });
            StartConnecting();
        }

        private void StartConnecting()
        {
            _ = Task.Run(ConnectingLongRunningTask);
        }

        private async Task ConnectingLongRunningTask()
        {
            while (!await Context.ConnectableService.TryConnectOneTime())
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
        public void Dispose()
        {
            _disposed = true;
        }
    }
}
