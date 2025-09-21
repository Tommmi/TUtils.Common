namespace TUtils.Common.StateMachine
{
    /// <summary>
    /// Abstract base class for all signals in a state machine.
    /// This class provides the foundation for implementing signals that can trigger state transitions
    /// and actions within a state machine. It follows the Command pattern to encapsulate
    /// both the event information and the action to be taken.
    /// </summary>
    /// <typeparam name="TSignalBase">The base type for all signals in the state machine. This should be the same as the derived class to enable proper type safety.</typeparam>
    /// <typeparam name="TISignalExecuter">The interface that defines the signal handling methods. States implement this interface to handle different signal types.</typeparam>
    /// <remarks>
    /// This abstract base class provides:
    /// - Signal name management for identification and debugging
    /// - Abstract Trigger method that derived classes must implement to define their behavior
    /// - Type safety through generic constraints
    /// - Consistent interface for all signals in the state machine
    /// 
    /// Derived signal classes should:
    /// - Implement the abstract Trigger method to define what happens when the signal is processed
    /// - Include any signal-specific data as properties or constructor parameters
    /// - Use the double dispatch pattern by calling the appropriate method on the signal executer
    /// - Follow naming conventions (typically ending with "Signal")
    /// 
    /// The signal processing flow:
    /// 1. External code triggers a signal on the state machine
    /// 2. State machine calls Trigger() on the signal with the current state
    /// 3. Signal calls the appropriate method on the state (signal executer)
    /// 4. State processes the signal and may transition to a new state
    /// </remarks>
    /// <example>
    /// // Example of a concrete signal implementation:
    /// public class ConnectSignal : TestConnectorSignal
    /// {
    ///     public string ServerAddress { get; }
    ///     public int Port { get; }
    ///     
    ///     public ConnectSignal(string serverAddress, int port) 
    ///         : base(nameof(ConnectSignal))
    ///     {
    ///         ServerAddress = serverAddress;
    ///         Port = port;
    ///     }
    /// 
    ///     public override void Trigger(ITestConnectorSignalExecuter signalExecuter, TestConnectorSignal signal)
    ///     {
    ///         // Cast to access signal-specific data
    ///         if (signal is ConnectSignal connectSignal)
    ///         {
    ///             signalExecuter.OnShouldConnect(connectSignal.ServerAddress, connectSignal.Port);
    ///         }
    ///         else
    ///         {
    ///             // Fallback for type safety
    ///             signalExecuter.OnShouldConnect("localhost", 80);
    ///         }
    ///     }
    /// }
    /// 
    /// // Usage example:
    /// var connectSignal = new ConnectSignal("192.168.1.100", 8080);
    /// stateMachine.Trigger(connectSignal);  // This will call OnShouldConnect on the current state
    /// </example>
    public abstract class Signal<TSignalBase, TISignalExecuter> : ISignal<TSignalBase, TISignalExecuter>
        where TSignalBase : Signal<TSignalBase, TISignalExecuter>
    {
        /// <summary>
        /// Gets the name of this signal.
        /// The signal name is used for identification, logging, debugging, and display purposes.
        /// It should be descriptive and unique within the context of the signal type hierarchy.
        /// </summary>
        /// <value>A string that identifies this signal type.</value>
        /// <example>
        /// // Example of accessing signal name for logging:
        /// Console.WriteLine($"Processing signal: {signal.SignalName}");
        /// 
        /// // Example of using signal name in conditional logic:
        /// if (signal.SignalName.EndsWith("TimeoutSignal"))
        /// {
        ///     // Handle timeout-related preprocessing
        ///     Context.TimeoutCount++;
        /// }
        /// </example>
        public string SignalName { get; }

        /// <summary>
        /// Executes this signal's action using the provided signal executer.
        /// This abstract method must be implemented by derived classes to define
        /// what specific action should be taken when this signal is processed.
        /// </summary>
        /// <param name="signalExecuter">The object that will execute the signal's action. This is typically the current state of the state machine.</param>
        /// <remarks>
        /// This method implements the double dispatch pattern:
        /// 1. The state machine calls Trigger() on the signal
        /// 2. The signal calls the appropriate method on the signal executer (typically a state)
        /// 3. The state executes the appropriate action, possibly transitioning to a new state
        /// 
        /// Implementation guidelines:
        /// - Cast the signal parameter to access signal-specific data
        /// - Call the appropriate method on the signalExecuter based on the signal type
        /// - Provide fallback behavior for type safety
        /// - Keep the method focused on dispatching - complex logic should be in the state
        /// </remarks>
        /// <example>
        /// // Example implementation for a timeout signal:
        /// public override void Trigger(ITestConnectorSignalExecuter signalExecuter, TestConnectorSignal signal)
        /// {
        ///     if (signal is TimeoutSignal timeoutSignal)
        ///     {
        ///         signalExecuter.OnTimeout(timeoutSignal.TimeoutType, timeoutSignal.Duration);
        ///     }
        ///     else
        ///     {
        ///         // Fallback - call without parameters
        ///         signalExecuter.OnTimeout(TimeoutType.General, TimeSpan.Zero);
        ///     }
        /// }
        /// 
        /// // Example implementation for a parameterless signal:
        /// public override void Trigger(ITestConnectorSignalExecuter signalExecuter, TestConnectorSignal signal)
        /// {
        ///     signalExecuter.OnShouldDisconnect();
        /// }
        /// </example>
        public abstract void Trigger(TISignalExecuter signalExecuter);

        /// <summary>
        /// Initializes a new instance of the Signal class with the specified signal name.
        /// </summary>
        /// <param name="signalName">The name that identifies this signal. Should be descriptive and unique within the signal hierarchy.</param>
        /// <example>
        /// // Example constructor usage in derived classes:
        /// public ConnectSignal() : base(nameof(ConnectSignal))
        /// {
        /// }
        /// 
        /// // Example with custom name:
        /// public CustomSignal() : base("CustomConnectionAttempt")
        /// {
        /// }
        /// 
        /// // Example with parameters:
        /// public RetrySignal(int attemptNumber) : base($"RetrySignal_{attemptNumber}")
        /// {
        ///     AttemptNumber = attemptNumber;
        /// }
        /// </example>
        public Signal(string signalName)
        {
            SignalName = signalName;
        }
    }
}
