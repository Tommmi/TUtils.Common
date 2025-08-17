using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.Async;

namespace TUtils.Common.Test
{
	/// <summary>
	/// Test suite for AsyncThreadStarter functionality.
	/// Tests thread creation, execution, cancellation, and result handling capabilities.
	/// </summary>
	[TestClass]
	public class ThreadTest
	{
		/// <summary>
		/// Tests successful thread execution without cancellation.
		/// Verifies that a thread can be started, executed completely, and return the expected result.
		/// </summary>
		/// <returns>A task representing the asynchronous test operation.</returns>
		[TestMethod]
		public async Task TestThread1()
		{
			// Arrange: Create a test thread
			CancellationTokenSource cancellationSource;
			var thread = CreateThread(out cancellationSource);

			// Act: Wait for thread to start and complete
			await thread.WaitForStart();
			var res = await thread.WaitForTermination();

			// Assert: Verify successful execution
			Assert.IsTrue(!res.Canceled, "Thread should not be marked as canceled");
			Assert.IsTrue(res.Exception == null, "Thread should complete without exceptions");
			Assert.IsTrue(res.Result == 123, "Thread should return the expected result value");
		}

		/// <summary>
		/// Helper method to create a test thread that sleeps for 1 second and returns 123.
		/// </summary>
		/// <param name="cancellationSource">Output parameter containing the cancellation source for the created thread.</param>
		/// <returns>An AsyncThreadStarter.Thread instance configured for testing.</returns>
		private static AsyncThreadStarter.Thread<int> CreateThread(out CancellationTokenSource cancellationSource)
		{
			cancellationSource = new CancellationTokenSource();
			var cancellationToken = cancellationSource.Token;
			var thread = AsyncThreadStarter.Start(
				threadName: "test thread",
				cancellationToken: cancellationToken,
				threadPriority: ThreadPriority.Normal, 
				synchronousThreadMethod: cancelToken =>
				{
					// Simulate work with a sleep operation
					Thread.Sleep(1000);
					return 123; // Return test value
				});
			return thread;
		}

		/// <summary>
		/// Tests thread cancellation functionality.
		/// Verifies that a thread can be properly canceled and reports the correct cancellation state.
		/// </summary>
		/// <returns>A task representing the asynchronous test operation.</returns>
		[TestMethod]
		public async Task TestThread2()
		{
			// Arrange: Create a test thread
			CancellationTokenSource cancellationSource;
			var thread = CreateThread(out cancellationSource);

			// Act: Cancel the thread before it completes
			cancellationSource.Cancel();
			var res = await thread.WaitForTermination();

			// Assert: Verify cancellation behavior
			Assert.IsTrue(res.Canceled, "Thread should be marked as canceled");
			Assert.IsTrue(!res.Crashed, "Thread should not be marked as crashed when canceled");
			Assert.IsTrue(res.Exception != null, "Thread should have an exception when canceled");
			Assert.IsTrue(res.Result == 0, "Canceled thread should return default result value");
		}
	}
}
