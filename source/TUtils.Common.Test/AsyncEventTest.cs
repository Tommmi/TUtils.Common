using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.Async;

namespace TUtils.Common.Test
{
	[TestClass]
	public class AsyncEventTest
	{
		[TestMethod]
		public async Task TestAsyncEvent1()
		{
			var cancellationSource = new CancellationTokenSource();
			var cancellationToken = cancellationSource.Token;
			var myEvent = new AsyncEvent(cancellationToken);
			var waitOnEvent = myEvent.RegisterForEvent();
			var executed = false;
			var sleep = Task.Delay(1000);
			sleep.ContinueWith(task =>
			{
				executed = true;
				myEvent.Rise();
			});

			await waitOnEvent;

			Assert.IsTrue(executed);
		}
	}
}
