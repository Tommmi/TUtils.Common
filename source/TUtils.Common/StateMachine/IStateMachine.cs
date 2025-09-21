namespace TUtils.Common.StateMachine;

/// <summary>
/// Represents a state machine that can process signals and transition between states.
/// This interface provides the main functionality for managing state transitions and signal processing.
/// </summary>
/// <typeparam name="TStateBase">The base type for all states in this state machine. Must implement IState and TISignalExecuter.</typeparam>
/// <typeparam name="TSignalBase">The base type for all signals that can be processed by this state machine.</typeparam>
/// <typeparam name="TISignalExecuter">The interface that defines how signals are executed on states.</typeparam>
/// <example>
/// // Example usage with a connection state machine:
/// // Define your state base class that implements both IState and ISignalExecuter
/// public class ConnectionState : State&lt;ConnectionState, ConnectionSignal, ConnectionContext, IConnectionSignalExecuter&gt;, IConnectionSignalExecuter
/// {
///     // Implementation details...
/// }
/// 
/// // Create and use the state machine
/// var stateMachine = new StateMachine&lt;ConnectionState, ConnectionSignal, ConnectionContext, IConnectionSignalExecuter&gt;("ConnectionManager");
/// var initialState = new DisconnectedState(context, stateMachine);
/// stateMachine.Initialize(initialState);
/// 
/// // Trigger a signal to change state
/// var connectSignal = new ConnectSignal();
/// stateMachine.Trigger(connectSignal);
/// </example>
public interface IStateMachine<TStateBase,TSignalBase, TISignalExecuter> 
    where TStateBase : IState<TStateBase, TSignalBase, TISignalExecuter>, TISignalExecuter
    where TSignalBase : ISignal<TSignalBase, TISignalExecuter>
{
    /// <summary>
    /// Gets the name of this state machine instance.
    /// This is useful for logging and debugging purposes when multiple state machines are used.
    /// </summary>
    /// <value>The name assigned to this state machine when it was created.</value>
    string StateMachineName { get; }

    /// <summary>
    /// Triggers a signal to be processed by the current state.
    /// The signal will be executed by the current state, which may result in a state transition.
    /// This operation is thread-safe.
    /// </summary>
    /// <param name="signal">The signal to be processed by the current state.</param>
    /// <example>
    /// // Example of triggering a signal:
    /// var connectSignal = new ConnectSignal();
    /// stateMachine.Trigger(connectSignal);
    /// 
    /// // The current state will handle this signal and may transition to a new state
    /// Console.WriteLine($"Current state: {stateMachine.CurrentState.StateName}");
    /// </example>
    void Trigger(TSignalBase signal);

    /// <summary>
    /// Gets the current active state of the state machine.
    /// This property provides thread-safe access to the current state.
    /// </summary>
    /// <value>The current state that is active in the state machine.</value>
    /// <example>
    /// // Check the current state:
    /// if (stateMachine.CurrentState is ConnectedState)
    /// {
    ///     // Handle connected state specific logic
    ///     Console.WriteLine("Connection is active");
    /// }
    /// 
    /// // Get state name for logging:
    /// Console.WriteLine($"State machine '{stateMachine.StateMachineName}' is in state: {stateMachine.CurrentState.StateName}");
    /// </example>
    TStateBase CurrentState { get; }
}