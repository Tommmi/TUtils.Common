using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming

namespace TUtils.Common.Async
{
	/// <summary>
	/// An event for which a listener can wait asynchronously.
	/// </summary>
	/// <remarks>
	/// <code>
	/// 	[TestClass]
	/// 	public class AsyncEventTest
	/// 	{
	/// 		[TestMethod]
	/// 		public async Task TestAsyncEvent1()
	/// 		{
	/// 			var cancellationSource = new CancellationTokenSource();
	/// 			var cancellationToken = cancellationSource.Token;
	/// 			var myEvent = new AsyncEvent(cancellationToken);
	/// 			var waitOnEvent = myEvent.RegisterForEvent();
	/// 			var executed = false;
	/// 			var sleep = Task.Delay(1000);
	/// 			sleep.ContinueWith(task =>
	/// 			{
	/// 				executed = true;
	/// 				myEvent.Rise();
	/// 			});
	/// 
	/// 			await waitOnEvent;
	/// 
	/// 			Assert.IsTrue(executed);
	/// 		}
	/// 	}
	/// </code>
	/// </remarks>
	public class AsyncEvent : AsyncEventBase<bool>
	{
		public AsyncEvent(CancellationToken? cancellationToken) : base(cancellationToken)
		{
		}

		/// <summary>
		/// Registers task on event - no deregistration is neccessary.
		/// Task completes, when method Rise() is called.
		/// </summary>
		/// <returns></returns>
		public Task RegisterForEvent()
		{
			var tcs = new TaskCompletionSource<bool>();

			lock (_sync)
			{
				_waitingTasks.Add(tcs);
			}

			return tcs.Task;
		}

		public void Rise()
		{
			DoForAllTasks(tcs => tcs.TrySetResult(true));
		}
	}

	public class AsyncEvent<TResult> : AsyncEventBase<TResult>
	{
		public AsyncEvent(CancellationToken cancellationToken) : base(cancellationToken)
		{
		}

		/// <summary>
		/// Register task on event - no deregistration is neccessary.
		/// Task completes, when method Rise() is called.
		/// </summary>
		/// <returns></returns>
		public Task<TResult> RegisterForEvent()
		{
			if (_cancellationToken.HasValue)
				_cancellationToken.Value.ThrowIfCancellationRequested();
			var tcs = new TaskCompletionSource<TResult>();

			lock (_sync)
			{
				_waitingTasks.Add(tcs);
			}

			return tcs.Task;
		}

		// ReSharper disable once UnusedMember.Global
		public void Rise(TResult arg)
		{
			DoForAllTasks(tcs=>tcs.TrySetResult(arg));
		}
	}

	public class AsyncEventBase<TResult> : IDisposable
	{
		protected readonly CancellationToken? _cancellationToken;
		protected readonly List<TaskCompletionSource<TResult>> _waitingTasks;
		protected readonly object _sync = new object();
		private Nullable<CancellationTokenRegistration> _cancelRegistration;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cancellationToken">may be null</param>
		protected AsyncEventBase(CancellationToken? cancellationToken)
		{
			_cancellationToken = cancellationToken;
			_waitingTasks = new List<TaskCompletionSource<TResult>>();
			_cancelRegistration = cancellationToken.HasValue ? cancellationToken.Value.Register(OnCanceled) : default(CancellationTokenRegistration);
		}

		private void OnCanceled()
		{
			_cancelRegistration?.Dispose();
			_cancelRegistration = null;
			DoForAllTasks(tcs=>tcs.TrySetCanceled());
		}

		protected void DoForAllTasks(Action<TaskCompletionSource<TResult>> action)
		{
			List<TaskCompletionSource<TResult>> waitingTasksCopy;

			lock (_sync)
			{
				if (!_waitingTasks.Any())
					return;
				waitingTasksCopy = _waitingTasks.ToList();
				_waitingTasks.Clear();
			}

			foreach (var tcs in waitingTasksCopy)
			{
				var task = tcs.Task;
				if (!task.IsCanceled
					&& !task.IsCompleted
					&& !task.IsFaulted)
				{
					action(tcs); // do it outside lock() !
				}
			}
		}

		public void Dispose()
		{
			_cancelRegistration?.Dispose();
			_cancelRegistration = null;
			lock (_sync)
			{
				_waitingTasks.Clear();
			}
		}
	}

	// ReSharper disable once UnusedMember.Global
}
