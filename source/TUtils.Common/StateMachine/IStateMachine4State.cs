using System.Threading.Tasks;

namespace TUtils.Common.StateMachine;

/// <summary>
/// Internal interface used by states to request state transitions from the state machine.
/// This interface is specifically designed to be used by state implementations to trigger
/// state changes without exposing the full state machine interface to external consumers.
/// </summary>
/// <typeparam name="TStateBase">The base type for all states in this state machine.</typeparam>
/// <typeparam name="TSignalBase">The base type for all signals that can be processed by this state machine.</typeparam>
/// <typeparam name="TISignalExecuter">The interface that defines how signals are executed on states.</typeparam>
/// <remarks>
/// This interface follows the principle of minimal interface exposure - states only need 
/// the ability to switch states, not the full state machine functionality like triggering signals.
/// This helps maintain proper encapsulation and prevents states from accessing functionality
/// they shouldn't have direct access to.
/// </remarks>
/// <example>
/// // Example usage within a state implementation:
/// public class ConnectingState : TestConnectorState
/// {
///     public override void OnConnected()
///     {
///         var connectedState = new ConnectedState(Context, _stateMachine);
///         // Use the interface to switch to the connected state
///         Switch2State(connectedState);
///     }
/// }
/// </example>
public interface IStateMachine4State<TStateBase, TSignalBase, TISignalExecuter> where TStateBase : IState<TStateBase, TSignalBase, TISignalExecuter>, TISignalExecuter
                                                                                 where TSignalBase : ISignal<TSignalBase, TISignalExecuter>
{
    /// <summary>
    /// Switches the state machine to a new state.
    /// This method handles the complete state transition process, including calling
    /// OnLeaving() on the current state and OnEntered() on the new state.
    /// This operation is thread-safe and atomic.
    /// </summary>
    /// <param name="newState">The new state to transition to. Cannot be null.</param>
    /// <remarks>
    /// The state transition follows this sequence:
    /// 1. Call OnLeaving() on the current state
    /// 2. Set the new state as current
    /// 3. Call OnEntered() on the new state
    /// All of this happens atomically within a lock to ensure thread safety.
    /// </remarks>
    /// <example>
    /// // Example of switching states from within a state:
    /// protected void HandleConnectionSuccess()
    /// {
    ///     var connectedState = new ConnectedState(Context, StateMachine);
    ///     Switch2State(connectedState);  // This will trigger OnLeaving() on current state and OnEntered() on new state
    /// }
    /// </example>
    Task Switch2State(TStateBase newState);
}