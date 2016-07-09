using System;
using System.Threading;
using System.Threading.Tasks;
using TUtils.Common.Logging;
using TUtils.Common.Logging.LogMocs;

namespace TUtils.Common.Async
{
	/// <summary>
	/// Creates Threads, for which you can wait asynchronously and which you can 
	/// cancel via cancellation token
	/// </summary>
	/// <remarks>
	/// <code><![CDATA[
	/// [TestMethod]
	/// public async Task TestThread1()
	/// {
	///		var cancellationSource = new CancellationTokenSource();
	/// 	var cancellationToken = cancellationSource.Token;
	/// 	var thread = AsyncThreadStarter.Start(
	/// 		threadName: "test thread",
	/// 		cancellationToken: cancellationToken,
	///			threadPriority:ThreadPriority.Normal,
	/// 		logger: null,
	/// 		threadMethod: cancelToken =>
	/// 		{
	/// 			Thread.Sleep(1000);
	/// 			return 123;
	/// 		});
	/// 
	/// 	await thread.WaitForStart();
	/// 	var res = await thread.WaitForTermination();
	/// 	Assert.IsTrue(!res.Canceled);
	/// 	Assert.IsTrue(res.Exception == null);
	/// 	Assert.IsTrue(res.Result == 123);
	/// }
	/// ]]></code>
	/// </remarks>
	public static class AsyncThreadStarter
	{
		#region types

		public class Thread<TReturn> 
		{
			#region types

			public class ResultValue
			{
				public TReturn Result { get; }

				public bool Canceled => Exception is OperationCanceledException || Exception is ThreadAbortException;
				public bool Crashed => Exception != null && !(Exception is OperationCanceledException || Exception is ThreadAbortException);

				public Exception Exception { get; }

				public ResultValue(TReturn result, Exception exception)
				{
					Result = result;
					Exception = exception;
				}
			}

			#endregion

			#region fields

			private readonly Func<CancellationToken, TReturn> _threadMethod;
			private readonly CancellationToken _cancellationToken;
			private readonly ITLog _logger;
			private AsyncEvent _startedEv;
			private AsyncEvent _terminatedEv;
			private readonly Task _waitForStart;
			private readonly Task _waitForTerminated;
			private ResultValue _result;

			#endregion

			#region private

			/// <summary>
			/// 
			/// </summary>
			/// <param name="threadName"></param>
			/// <param name="cancellationToken"></param>
			/// <param name="threadPriority"></param>
			/// <param name="logger">may be null</param>
			/// <param name="threadMethod"></param>
			internal Thread(
				string threadName,
				CancellationToken cancellationToken,
				ThreadPriority threadPriority,
				ITLog logger,
				Func<CancellationToken, TReturn> threadMethod)
			{
				ThreadName = threadName;
				_threadMethod = threadMethod;
				_cancellationToken = cancellationToken;
				_logger = logger??new TLog(new LogMocWriter(),false);
				// waiting tasks for the events _startedEv and _terminatedEv may not be canceled 
				_startedEv = new AsyncEvent(null);
				_waitForStart = _startedEv.RegisterForEvent();
				_terminatedEv = new AsyncEvent(null);
				_waitForTerminated = _terminatedEv.RegisterForEvent();
				var thread = new Thread(ThreadMethod);
				thread.Name = threadName;
				thread.Priority = threadPriority;
				thread.Start();
			}

			private void ThreadMethod()
			{
				_startedEv.Rise();
				_logger.LogInfo(this,"thread name={0} id={1} started", ()=>ThreadName, ()=>Thread.CurrentThread.ManagedThreadId);
				try
				{
					var res = _threadMethod(_cancellationToken);
					_cancellationToken.ThrowIfCancellationRequested();
					_result = new ResultValue(res, null);
				}
				catch (Exception e)
				{
					_result = new ResultValue(default(TReturn), e);
#					if DEBUG_EXCEPTIONS
					if (!(e is OperationCanceledException || e is ThreadAbortException))
						throw;
#					endif
				}
				finally
				{
					_terminatedEv.Rise();
				}
			}

			#endregion

			public string ThreadName { get; }

			public async Task<ResultValue> WaitForTermination()
			{
				await _waitForTerminated;
				return _result;
			}

			public async Task WaitForStart()
			{
				await _waitForStart;
			}
		}

		#endregion

		public static Thread<TReturn> Start<TReturn>(
			string threadName,
			CancellationToken cancellationToken,
			ThreadPriority threadPriority,
			ITLog logger,
			Func<CancellationToken, TReturn> synchronousThreadMethod)
		{
			return new Thread<TReturn>(threadName,cancellationToken, threadPriority, logger, synchronousThreadMethod);
		}
	}
}
