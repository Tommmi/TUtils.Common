// ReSharper disable InconsistentNaming

using System.Threading.Tasks;

namespace TUtils.Common.StateMachine;

/// <summary>
/// Abstract base class for all states in a state machine.
/// This class provides common functionality for state lifecycle management, context access,
/// and state transitions while allowing derived classes to implement state-specific behavior.
/// </summary>
/// <typeparam name="TStateBase">The base type for all states in the state machine. This should be the same as the derived class to enable proper type safety.</typeparam>
/// <typeparam name="TSignalBase">The base type for all signals that can be processed by states in this state machine.</typeparam>
/// <typeparam name="TContext">The context type that provides shared data and services to all states in the state machine.
/// </typeparam>
/// <typeparam name="TISignalExecuter">The interface that defines signal handling methods. Derived state classes must implement this interface.</typeparam>
/// <remarks>
/// This abstract base class provides:
/// - State name management for identification and debugging
/// - Context access for shared data and services between states
/// - State transition capabilities through the Switch2State method
/// - Lifecycle management through OnEntered and OnLeaving virtual methods
/// - Thread-safe state transitions through the state machine interface
/// 
/// Derived classes should:
/// - Implement the TISignalExecuter interface to handle signals
/// - Override OnEntered() and OnLeaving() for state-specific initialization/cleanup
/// - Use Switch2State() to transition to other states when appropriate conditions are met
/// - Use the Context property to access shared data and services
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
///     // State lifecycle methods
///     public override void OnEntered()
///     {
///         Console.WriteLine($"Connected to {Context.ServerAddress}");
///         Context.ConnectionService.StartHeartbeat();
///         Context.IsConnected = true;
///     }
/// 
///     public override void OnLeaving()
///     {
///         Console.WriteLine("Disconnecting...");
///         Context.ConnectionService.StopHeartbeat();
///         Context.IsConnected = false;
///     }
/// 
///     // Signal handler implementations
///     public void OnShouldDisconnect()
///     {
///         var disconnectedState = new DisconnectedState(Context, _stateMachine);
///         Switch2State(disconnectedState);
///     }
/// 
///     public void OnShouldReconnect()
///     {
///         var reconnectingState = new ReconnectingState(Context, _stateMachine);
///         Switch2State(reconnectingState);
///     }
/// 
///     // State-specific methods
///     public void SendData(byte[] data)
///     {
///         if (Context.IsConnected)
///         {
///             Context.ConnectionService.Send(data);
///         }
///     }
/// }
/// </example>
public abstract class State<TStateBase, TSignalBase,TContext, TISignalExecuter> : IState<TStateBase, TSignalBase, TISignalExecuter> 
    where TStateBase : State<TStateBase, TSignalBase, TContext, TISignalExecuter>, TISignalExecuter
    where TSignalBase : Signal<TSignalBase, TISignalExecuter>
{
    /// <summary>
    /// Reference to the state machine interface that allows this state to request state transitions.
    /// This interface provides only the minimal functionality needed by states (switching states)
    /// without exposing the full state machine interface.
    /// </summary>
    protected IStateMachine4State<TStateBase, TSignalBase, TISignalExecuter> StateMachine { get; }

    /// <summary>
    /// Initializes a new instance of the State class with the specified parameters.
    /// This constructor sets up the basic state infrastructure including name, context, and state machine reference.
    /// </summary>
    /// <param name="stateName">The name that identifies this state. Should be unique within the state machine context for clarity.</param>
    /// <param name="context">The shared context object that provides data and services to all states in the state machine.</param>
    /// <param name="stateMachine">The state machine interface that this state can use to request state transitions.</param>
    /// <example>
    /// // Example constructor call from a derived state:
    /// public ConnectedState(ConnectionContext context, IStateMachine4State&lt;ConnectionState, ConnectionSignal, IConnectionSignalExecuter&gt; stateMachine)
    ///     : base(nameof(ConnectedState), context, stateMachine)
    /// {
    ///     // Additional initialization can be done here
    /// }
    /// </example>
    public State(string stateName, TContext context, IStateMachine4State<TStateBase, TSignalBase, TISignalExecuter> stateMachine)
    {
        StateMachine = stateMachine;
        StateName = stateName;
        Context = context;
    }

    /// <summary>
    /// Gets the name of this state.
    /// The state name is used for identification, logging, debugging, and display purposes.
    /// </summary>
    /// <value>A string that uniquely identifies this state within its state machine context.</value>
    public string StateName { get; }

    /// <summary>
    /// Gets the shared context object that provides data and services to all states in the state machine.
    /// This context allows states to access shared resources, configuration, and services
    /// without requiring complex parameter passing or dependency injection at the state level.
    /// </summary>
    /// <value>The context instance that was provided when this state was created.</value>
    /// <example>
    /// // Example of using the context in state logic:
    /// public void AttemptConnection()
    /// {
    ///     try
    ///     {
    ///         Context.ConnectionService.ConnectTo(Context.ServerAddress, Context.Port);
    ///         var connectedState = new ConnectedState(Context, _stateMachine);
    ///         Switch2State(connectedState);
    ///     }
    ///     catch (ConnectionException ex)
    ///     {
    ///         Context.Logger.LogError($"Connection failed: {ex.Message}");
    ///         var disconnectedState = new DisconnectedState(Context, _stateMachine);
    ///         Switch2State(disconnectedState);
    ///     }
    /// }
    /// </example>
    protected TContext Context { get; private set; }

    /// <summary>
    /// Requests a transition to a new state.
    /// This method provides a convenient way for states to initiate state transitions
    /// by delegating to the state machine's Switch2State method.
    /// The transition is handled atomically and includes proper lifecycle management.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    /// <remarks>
    /// This method should be called by state logic when conditions are met for transitioning to a different state.
    /// The actual state transition is handled by the state machine, which will:
    /// 1. Call OnLeaving() on the current state (this state)
    /// 2. Set the new state as current
    /// 3. Call OnEntered() on the new state
    /// This ensures proper cleanup and initialization during state transitions.
    /// </remarks>
    /// <example>
    /// // Example of using Switch2State in signal handlers:
    /// public void OnConnectionEstablished()
    /// {
    ///     Context.Logger.LogInfo("Connection established successfully");
    ///     var connectedState = new ConnectedState(Context, _stateMachine);
    ///     Switch2State(connectedState);  // This will trigger OnLeaving() on this state and OnEntered() on connectedState
    /// }
    /// 
    /// public void OnConnectionFailed()
    /// {
    ///     Context.Logger.LogError("Connection attempt failed");
    ///     Context.RetryCount++;
    ///     
    ///     if (Context.RetryCount &lt; Context.MaxRetries)
    ///     {
    ///         var retryState = new RetryingState(Context, _stateMachine);
    ///         Switch2State(retryState);
    ///     }
    ///     else
    ///     {
    ///         var failedState = new ConnectionFailedState(Context, _stateMachine);
    ///         Switch2State(failedState);
    ///     }
    /// }
    /// </example>
    protected async Task Switch2State(TStateBase newState)
    {
        await StateMachine.Switch2State(newState);
    }

    /// <summary>
    /// Called when the state machine transitions to this state.
    /// Override this method in derived classes to implement state-specific initialization logic
    /// such as starting timers, opening resources, or updating the context.
    /// </summary>
    /// <remarks>
    /// This method is called as part of the state transition process after the new state
    /// has been set as the current state but as part of the same atomic operation.
    /// Any exceptions thrown in this method will propagate and may leave the state machine
    /// in an inconsistent state, so proper error handling is recommended.
    /// </remarks>
    /// <example>
    /// // Example OnEntered implementation:
    /// public override void OnEntered()
    /// {
    ///     Context.Logger.LogInfo($"Entered {StateName} state");
    ///     
    ///     // Start state-specific operations
    ///     Context.ConnectionService.StartMonitoring();
    ///     
    ///     // Update context state
    ///     Context.CurrentStateStartTime = DateTime.UtcNow;
    ///     Context.StateTransitionCount++;
    ///     
    ///     // Schedule state-specific timers or tasks
    ///     ScheduleHeartbeat();
    /// }
    /// </example>
    public virtual async Task OnEntered()
    {
    }

    /// <summary>
    /// Called when the state machine transitions away from this state.
    /// Override this method in derived classes to implement state-specific cleanup logic
    /// such as stopping timers, closing resources, or saving state information.
    /// </summary>
    /// <remarks>
    /// This method is called as part of the state transition process before the new state
    /// is set as the current state but as part of the same atomic operation.
    /// Any exceptions thrown in this method will propagate and may prevent the state transition,
    /// so proper error handling and cleanup is essential.
    /// </remarks>
    /// <example>
    /// // Example OnLeaving implementation:
    /// public override void OnLeaving()
    /// {
    ///     Context.Logger.LogInfo($"Leaving {StateName} state");
    ///     
    ///     // Clean up state-specific resources
    ///     Context.ConnectionService.StopMonitoring();
    ///     
    ///     // Save state information
    ///     var stateInfo = new StateInfo
    ///     {
    ///         StateName = StateName,
    ///         TimeInState = DateTime.UtcNow - Context.CurrentStateStartTime,
    ///         ExitReason = Context.LastSignalName
    ///     };
    ///     Context.StateHistory.Add(stateInfo);
    ///     
    ///     // Cancel any pending operations
    ///     CancelHeartbeat();
    /// }
    /// </example>
    public virtual async Task OnLeaving()
    {
    }
}