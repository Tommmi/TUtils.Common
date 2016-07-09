using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUtils.Common.Async;

namespace TUtils.Common.Test
{
	[TestClass]
	public class ThreadTest
	{
		[TestMethod]
		public async Task TestThread1()
		{
			CancellationTokenSource cancellationSource;
			var thread = CreateThread(out cancellationSource);

			await thread.WaitForStart();
			var res = await thread.WaitForTermination();
			Assert.IsTrue(!res.Canceled);
			Assert.IsTrue(res.Exception == null);
			Assert.IsTrue(res.Result == 123);
		}

		private static AsyncThreadStarter.Thread<int> CreateThread(out CancellationTokenSource cancellationSource)
		{
			cancellationSource = new CancellationTokenSource();
			var cancellationToken = cancellationSource.Token;
			var thread = AsyncThreadStarter.Start(
				threadName: "test thread",
				cancellationToken: cancellationToken,
				threadPriority:ThreadPriority.Normal, 
				logger: null,
				synchronousThreadMethod: cancelToken =>
				{
					Thread.Sleep(1000);
					return 123;
				});
			return thread;
		}

		[TestMethod]
		public async Task TestThread2()
		{
			CancellationTokenSource cancellationSource;
			var thread = CreateThread(out cancellationSource);

			cancellationSource.Cancel();
			var res = await thread.WaitForTermination();
			Assert.IsTrue(res.Canceled);
			Assert.IsTrue(!res.Crashed);
			Assert.IsTrue(res.Exception != null);
			Assert.IsTrue(res.Result == 0);
		}
	}
}
