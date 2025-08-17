using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.Async;

namespace TUtils.Common.Test
{
	/// <summary>
	/// Test suite for AsyncEvent functionality.
	/// AsyncEvent provides an event-based mechanism for asynchronous waiting patterns.
	/// </summary>
	[TestClass]
	public class AsyncEventTest
	{
		/// <summary>
		/// Tests the basic functionality of AsyncEvent.
		/// Verifies that an event can be registered, raised after a delay, and successfully awaited.
		/// </summary>
		/// <returns>A task representing the asynchronous test operation.</returns>
		[TestMethod]
		public async Task TestAsyncEvent1()
		{
			// Arrange: Set up cancellation token and async event
			var cancellationSource = new CancellationTokenSource();
			var cancellationToken = cancellationSource.Token;
			var myEvent = new AsyncEvent(cancellationToken);
			var waitOnEvent = myEvent.RegisterForEvent();
			var executed = false;

			// Act: Schedule event to be raised after 1 second delay
			var sleep = Task.Delay(1000);
			await sleep.ContinueWith(task =>
			{
				executed = true;
				myEvent.Rise(); // Trigger the event
			});

			// Wait for the event to be raised
			await waitOnEvent;

			// Assert: Verify that the event was properly triggered
			Assert.IsTrue(executed, "The event should have been executed after the delay");
		}
	}
}
