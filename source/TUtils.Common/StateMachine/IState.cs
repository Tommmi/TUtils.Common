namespace TUtils.Common.StateMachine;

/// <summary>
/// Defines the basic contract for all states in a state machine.
/// States represent the different conditions or modes that a system can be in,
/// and they define how the system behaves while in that particular state.
/// </summary>
/// <typeparam name="TStateBase">The base type for all states in the state machine. This enables states to reference other states of the same type.</typeparam>
/// <typeparam name="TSignalBase">The base type for all signals that can be processed by states in this state machine.</typeparam>
/// <typeparam name="TISignalExecuter">The interface that defines how signals are executed on states. States must implement this interface to handle signals.</typeparam>
/// <remarks>
/// This interface is typically implemented by deriving from the abstract State&lt;TStateBase, TSignalBase, TContext, TISignalExecuter&gt; class
/// rather than implementing it directly. The State base class provides common functionality like state transitions and lifecycle management.
/// 
/// States in this framework are responsible for:
/// - Defining their name for identification and debugging
/// - Implementing signal handlers through the TISignalExecuter interface
/// - Managing transitions to other states when appropriate conditions are met
/// - Handling entry and exit logic through OnEntered() and OnLeaving() virtual methods
/// </remarks>
/// <example>
/// // Example of a concrete state implementation:
/// public class ConnectedState : TestConnectorState, ITestConnectorSignalExecuter
/// {
///     public ConnectedState(TestConnectorContext context, IStateMachine4State&lt;TestConnectorState, TestConnectorSignal, ITestConnectorSignalExecuter&gt; stateMachine)
///         : base(nameof(ConnectedState), context, stateMachine)
///     {
///     }
/// 
///     public override void OnEntered()
///     {
///         // State entry logic - called when transitioning TO this state
///         Console.WriteLine($"Entered {StateName} state");
///         Context.ConnectionService.StartHeartbeat();
///     }
/// 
///     public override void OnLeaving()
///     {
///         // State exit logic - called when transitioning FROM this state  
///         Console.WriteLine($"Leaving {StateName} state");
///         Context.ConnectionService.StopHeartbeat();
///     }
/// 
///     // Signal handler implementation from ITestConnectorSignalExecuter
///     public void OnShouldDisconnect()
///     {
///         var disconnectedState = new DisconnectedState(Context, StateMachine);
///         Switch2State(disconnectedState);
///     }
/// }
/// </example>
public interface IState<TStateBase,TSignalBase, TISignalExecuter> 
    where TStateBase : IState<TStateBase,TSignalBase, TISignalExecuter>, TISignalExecuter
    where TSignalBase : ISignal<TSignalBase, TISignalExecuter>
{
    /// <summary>
    /// Gets the name of this state.
    /// The state name is used for identification, logging, debugging, and display purposes.
    /// It should be unique within the context of the state machine to avoid confusion.
    /// </summary>
    /// <value>A string that uniquely identifies this state within its state machine context.</value>
    /// <example>
    /// // Example of using state names for logging:
    /// Console.WriteLine($"State machine transitioned to: {currentState.StateName}");
    /// 
    /// // Example of using state names for conditional logic:
    /// if (stateMachine.CurrentState.StateName == "Connected")
    /// {
    ///     // Perform connected-state-specific operations
    /// }
    /// 
    /// // Typical naming convention using nameof:
    /// public ConnectedState(...) : base(nameof(ConnectedState), ...)
    /// {
    /// }
    /// </example>
    string StateName { get; }
}