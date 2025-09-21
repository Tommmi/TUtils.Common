namespace TUtils.Common.StateMachine;

/// <summary>
/// Defines the contract for signals in a state machine.
/// Signals represent events or messages that can trigger state transitions or actions within states.
/// They encapsulate both the event data and the logic for how that event should be processed.
/// </summary>
/// <typeparam name="TSignalBase">The base type for all signals in the state machine. This enables signals to reference other signals of the same type.</typeparam>
/// <typeparam name="TISignalExecuter">The interface that defines the methods available for signal execution. This is typically implemented by states.</typeparam>
/// <remarks>
/// Signals in this framework follow the Command pattern - they encapsulate both the request (what happened) 
/// and the action to take (how to handle it). This allows for:
/// - Decoupling of event generation from event handling
/// - Easy testing of signal logic in isolation
/// - Clear separation of concerns between different types of events
/// - Consistent handling of similar events across different states
/// 
/// Signals are typically triggered by external events (user input, network events, timers, etc.)
/// and then processed by the current state of the state machine.
/// </remarks>
/// <example>
/// // Example signal implementation for a connection state machine:
/// public class ConnectSignal : TestConnectorSignal
/// {
///     public ConnectSignal() : base(nameof(ConnectSignal))
///     {
///     }
/// 
///     public override void Trigger(ITestConnectorSignalExecuter signalExecuter, TestConnectorSignal signal)
///     {
///         // Execute the connect action on the current state
///         signalExecuter.OnShouldConnect();
///     }
/// }
/// 
/// // Usage example:
/// var connectSignal = new ConnectSignal();
/// stateMachine.Trigger(connectSignal);  // This will call OnShouldConnect() on the current state
/// </example>
public interface ISignal<TSignalBase,TISignalExecuter>
    where TSignalBase : ISignal<TSignalBase, TISignalExecuter>
{
    /// <summary>
    /// Gets the name of this signal.
    /// The signal name is used for identification, logging, debugging, and display purposes.
    /// It should be descriptive and unique within the context of the signal type hierarchy.
    /// </summary>
    /// <value>A string that identifies this signal type.</value>
    /// <example>
    /// // Example of using signal names for logging:
    /// Console.WriteLine($"Processing signal: {signal.SignalName}");
    /// 
    /// // Example of conditional logic based on signal name:
    /// if (signal.SignalName == "ConnectSignal")
    /// {
    ///     // Handle connect-specific preprocessing
    /// }
    /// 
    /// // Typical naming convention using nameof in constructor:
    /// public ConnectSignal() : base(nameof(ConnectSignal))
    /// {
    /// }
    /// </example>
    string SignalName { get; }

    /// <summary>
    /// Executes this signal's action using the provided signal executer.
    /// This method contains the core logic of what should happen when this signal is processed.
    /// It acts as a bridge between the signal (what happened) and the state (how to handle it).
    /// </summary>
    /// <param name="signalExecuter">The object that will execute the signal's action. This is typically the current state of the state machine.</param>
    /// <remarks>
    /// This method implements the double dispatch pattern:
    /// 1. The state machine calls Trigger() on the signal
    /// 2. The signal calls the appropriate method on the signal executer (typically a state)
    /// 3. The state executes the appropriate action, possibly transitioning to a new state
    /// 
    /// This pattern ensures that the right action is taken based on both the signal type and the current state,
    /// without requiring complex conditional logic in either the signal or the state.
    /// </remarks>
    /// <example>
    /// // Example implementation of Trigger method:
    /// public override void Trigger(ITestConnectorSignalExecuter signalExecuter)
    /// {
    ///     signalExecuter.OnSinalA(this);
    /// }
    /// </example>
    void Trigger(TISignalExecuter signalExecuter);
}

