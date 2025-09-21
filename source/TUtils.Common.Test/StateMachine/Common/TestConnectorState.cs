using System.Data;
using TUtils.Common.StateMachine;
using TUtils.Common.Test.StateMachine.Signals;

namespace TUtils.Common.Test.StateMachine.Common
{
    public abstract class TestConnectorState : State<TestConnectorState, TestConnectorSignal, TestConnectorContext,ITestConnectorSignalExecuter> 
        ,ITestConnectorSignalExecuter
    {
        /// <inheritdoc />
        protected TestConnectorState(string stateName, TestConnectorContext context, IStateMachine4State<TestConnectorState, TestConnectorSignal, ITestConnectorSignalExecuter> stateMachine)
            : base(stateName, context, stateMachine)
        {
        }

        /// <inheritdoc />
        public abstract void OnSignalReconnect(SignalShouldReconnect signal);

        /// <inheritdoc />
        public abstract void OnSignalConnect(SignalShouldConnect signal);
    }
}
