namespace TUtils.Common.StateMachine;

/// <summary>
/// A thread-safe implementation of a state machine that manages state transitions and signal processing.
/// This class provides a complete state machine implementation that can be used for modeling
/// any system that has discrete states and transitions between those states based on signals/events.
/// </summary>
/// <typeparam name="TStateBase">The base class for all states in this state machine. Must derive from State and implement TISignalExecuter.</typeparam>
/// <typeparam name="TSignalBase">The base class for all signals that can be processed by this state machine. Must derive from Signal.</typeparam>
/// <typeparam name="TContext">The context type that provides shared data and services to all states in the state machine.
/// </typeparam>
/// <typeparam name="TISignalExecuter">The interface that defines signal handling methods. States implement this interface to handle different signal types.</typeparam>
/// <remarks>
/// This state machine implementation provides:
/// - Thread safety through internal locking mechanisms
/// - Proper state lifecycle management (OnEntered/OnLeaving callbacks)
/// - Signal-driven state transitions using the Command pattern
/// - Context sharing between states for shared data and services
/// - Clean separation of concerns between state logic and transition logic
/// 
/// The state machine follows these principles:
/// - States are responsible for their own behavior and transitions
/// - Signals encapsulate events and trigger appropriate state actions
/// - The context provides shared data and services to all states
/// - All operations are thread-safe and atomic
/// </remarks>
/// <example>
/// // Example: Creating and using a connection state machine
/// 
/// // 1. Define your context with shared data/services
/// public class ConnectionContext
/// {
///     public IConnectionService ConnectionService { get; set; }
///     public string ServerAddress { get; set; }
///     public TimeSpan Timeout { get; set; }
/// }
/// 
/// // 2. Create and initialize the state machine
/// var context = new ConnectionContext
/// {
///     ConnectionService = new TcpConnectionService(),
///     ServerAddress = "192.168.1.1",
///     Timeout = TimeSpan.FromSeconds(30)
/// };
/// 
/// var stateMachine = new StateMachine&lt;ConnectionState, ConnectionSignal, ConnectionContext, IConnectionSignalExecuter&gt;("TcpConnector");
/// var initialState = new DisconnectedState(context, stateMachine);
/// stateMachine.Initialize(initialState);
/// 
/// // 3. Trigger signals to drive state transitions
/// var connectSignal = new ConnectSignal();
/// stateMachine.Trigger(connectSignal);
/// 
/// // 4. Monitor current state
/// Console.WriteLine($"Current state: {stateMachine.CurrentState.StateName}");
/// 
/// // 5. Handle state-specific logic based on current state
/// if (stateMachine.CurrentState is ConnectedState connectedState)
/// {
///     // Connected-specific operations
///     connectedState.SendHeartbeat();
/// }
/// </example>
public class StateMachine<TStateBase, TSignalBase,TContext, TISignalExecuter> : IStateMachine<TStateBase, TSignalBase, TISignalExecuter>,
                                                                                IStateMachine4State<TStateBase, TSignalBase, TISignalExecuter>
    where TStateBase : State<TStateBase, TSignalBase, TContext, TISignalExecuter>, TISignalExecuter
    where TSignalBase : Signal<TSignalBase, TISignalExecuter>
{
    /// <summary>
    /// Synchronization object used to ensure thread-safe access to the state machine's internal state.
    /// All state transitions and signal processing are protected by this lock.
    /// </summary>
    private object _sync = new object();

    /// <summary>
    /// Gets the name of this state machine instance.
    /// This is useful for logging and debugging when multiple state machines are used in an application.
    /// </summary>
    public string StateMachineName { get; }

    /// <summary>
    /// The internal storage for the current state. Access should be through the CurrentState property
    /// to ensure proper thread safety.
    /// </summary>
    private TStateBase _currentState = null;
    
    /// <summary>
    /// Gets the current active state of the state machine.
    /// This property provides thread-safe access to the current state and is the primary way
    /// to query the state machine's current condition.
    /// </summary>
    /// <value>The currently active state, or null if the state machine has not been initialized.</value>
    /// <example>
    /// // Check current state for logging:
    /// Console.WriteLine($"State machine '{stateMachine.StateMachineName}' is in state: {stateMachine.CurrentState?.StateName ?? "Uninitialized"}");
    /// 
    /// // Use pattern matching for state-specific logic:
    /// switch (stateMachine.CurrentState)
    /// {
    ///     case ConnectedState connected:
    ///         connected.SendData(data);
    ///         break;
    ///     case ConnectingState connecting:
    ///         Console.WriteLine($"Still connecting, {connecting.AttemptsRemaining} attempts left");
    ///         break;
    ///     case DisconnectedState:
    ///         Console.WriteLine("Not connected");
    ///         break;
    /// }
    /// </example>
    public TStateBase CurrentState 
    {
        get
        {
            lock (_sync)
            {
                return _currentState;
            }
        }

        private set => _currentState = value;
    }

    /// <summary>
    /// Initializes a new instance of the StateMachine class with the specified name.
    /// </summary>
    /// <param name="stateMachineName">A descriptive name for this state machine instance. Used for logging and debugging.</param>
    /// <example>
    /// // Create a state machine for managing database connections:
    /// var dbStateMachine = new StateMachine&lt;DbState, DbSignal, DbContext, IDbSignalExecuter&gt;("DatabaseConnectionManager");
    /// 
    /// // Create a state machine for managing user sessions:
    /// var sessionStateMachine = new StateMachine&lt;SessionState, SessionSignal, SessionContext, ISessionSignalExecuter&gt;("UserSessionManager");
    /// </example>
    public StateMachine(string stateMachineName)
    {
        StateMachineName = stateMachineName;
    }

    /// <summary>
    /// Initializes the state machine with its initial state.
    /// This method must be called before the state machine can process any signals.
    /// The initialization process sets the current state and calls OnEntered() on the initial state.
    /// </summary>
    /// <param name="initialState">The initial state for the state machine. This state's OnEntered() method will be called.</param>
    /// <remarks>
    /// This method is thread-safe and should only be called once during the state machine's lifetime.
    /// After initialization, state changes should only occur through signal processing or explicit state transitions.
    /// </remarks>
    /// <example>
    /// // Initialize a connection state machine:
    /// var context = new ConnectionContext { ServerAddress = "localhost" };
    /// var stateMachine = new StateMachine&lt;ConnectionState, ConnectionSignal, ConnectionContext, IConnectionSignalExecuter&gt;("Connector");
    /// var initialState = new DisconnectedState(context, stateMachine);
    /// 
    /// stateMachine.Initialize(initialState);  // This will call initialState.OnEntered()
    /// 
    /// Console.WriteLine($"State machine initialized in state: {stateMachine.CurrentState.StateName}");
    /// </example>
    public void Initialize(TStateBase initialState)
    {
        lock (_sync)
        {
            CurrentState = initialState;
            CurrentState.OnEntered();
        }
    }

    /// <summary>
    /// Triggers a signal to be processed by the current state.
    /// The signal will execute its logic using the current state as the signal executer,
    /// which may result in state transitions or other actions.
    /// This operation is thread-safe and atomic.
    /// </summary>
    /// <param name="signal">The signal to be processed by the current state.</param>
    /// <remarks>
    /// The signal processing follows this sequence:
    /// 1. Acquire lock for thread safety
    /// 2. Call signal.Trigger(CurrentState, signal)
    /// 3. The signal calls appropriate methods on the current state
    /// 4. The state may perform actions or trigger state transitions
    /// 5. Release lock
    /// 
    /// All of this happens atomically, ensuring consistent state during signal processing.
    /// </remarks>
    /// <example>
    /// // Example of triggering different types of signals:
    /// 
    /// // Basic signal triggering:
    /// var connectSignal = new ConnectSignal();
    /// stateMachine.Trigger(connectSignal);
    /// 
    /// // Signal with parameters:
    /// var reconnectSignal = new ReconnectSignal(maxRetries: 3, delay: TimeSpan.FromSeconds(5));
    /// stateMachine.Trigger(reconnectSignal);
    /// 
    /// // Check the result:
    /// if (stateMachine.CurrentState is ConnectingState)
    /// {
    ///     Console.WriteLine("Connection attempt started");
    /// }
    /// 
    /// // Chaining multiple signals:
    /// stateMachine.Trigger(new ConnectSignal());
    /// Thread.Sleep(1000);  // Allow connection to establish
    /// stateMachine.Trigger(new SendDataSignal(data));
    /// </example>
    public void Trigger(TSignalBase signal)
    {
        lock (_sync)
        {
            signal.Trigger(CurrentState);
        }
    }

    /// <summary>
    /// Switches the state machine to a new state.
    /// This method is called by states to transition to other states and handles the complete
    /// state transition lifecycle including calling OnLeaving() and OnEntered() methods.
    /// This operation is thread-safe and atomic.
    /// </summary>
    /// <param name="state">The new state to transition to.</param>
    /// <remarks>
    /// This method implements the IStateMachine4State interface and is typically called by states
    /// rather than external code. The state transition follows this sequence:
    /// 1. Call OnLeaving() on the current state
    /// 2. Set the new state as current
    /// 3. Call OnEntered() on the new state
    /// All of this happens atomically within a lock.
    /// </remarks>
    /// <example>
    /// // This method is typically called from within a state:
    /// public class ConnectingState : ConnectionState
    /// {
    ///     public void OnConnectionEstablished()
    ///     {
    ///         var connectedState = new ConnectedState(Context, StateMachine);
    ///         Switch2State(connectedState);  // This calls StateMachine.Switch2State internally
    ///     }
    /// }
    /// 
    /// // The transition will:
    /// // 1. Call this.OnLeaving() on ConnectingState
    /// // 2. Set ConnectedState as current
    /// // 3. Call OnEntered() on ConnectedState
    /// </example>
    public void Switch2State(TStateBase state)
    {
        lock (_sync)
        {
            CurrentState.OnLeaving();
            CurrentState = state;
            CurrentState.OnEntered();
        }
    }
}