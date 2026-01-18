using System;
using System.Threading;
using System.Threading.Tasks;
using TUtils.Common.Common;
using TUtils.Common.Tasks;
// ReSharper disable InconsistentNaming

namespace TUtils.Common.Async
{
	public class TSemaphore : IDisposable
	{
		#region fields

		private readonly CancellationToken? _cancellationToken;
		private readonly bool _reentranceable;
		private readonly SemaphoreSlim _concurrencySemaphore = new SemaphoreSlim(1, 1);
		private readonly static Guid _taskStorageId = Guid.NewGuid();
		private TaskStorage<BoxedObject<int>> _taskStorage;

		#endregion

		#region constructor

		public TSemaphore(CancellationToken cancellationToken, bool reentranceable)
		{
			_cancellationToken = cancellationToken;
			_reentranceable = reentranceable;

			EnsureTaskStorage();
		}

		public TSemaphore(bool reentranceable)
		{
			_reentranceable = reentranceable;
			EnsureTaskStorage();
		}

		#endregion


		#region IDisposable

		public void Dispose()
		{
			_concurrencySemaphore?.Dispose();
		}

		#endregion

		#region public

		public async Task DoSynchronized(Func<Task> action)
		{
			bool mustCallRelease = false;

			try
			{
				var waitResult = await WaitEndless();
				mustCallRelease = waitResult.mustCallRelease;

				if (!waitResult.succeeded)
				{
					return;
				}

				await action();
			}
			finally
			{
				DoFinally(mustCallRelease: mustCallRelease);
			}
		}

		public async Task<TResult> DoSynchronized<TResult>(Func<Task<TResult>> action)
		{
			bool mustCallRelease = false;

			try
			{
				var waitResult = await WaitEndless();
				mustCallRelease = waitResult.mustCallRelease;

				if (!waitResult.succeeded)
				{
					return default;
				}

				return await action();
			}
			finally
			{
				DoFinally(mustCallRelease: mustCallRelease);
			}
		}

		public async Task<bool> TryDoSynchronizedWithTimeout(TimeSpan timeout, Func<Task> action)
		{
			bool mustCallRelease = false;
			try
			{
				var waitResult = await WaitWithTimeout(timeout);
				mustCallRelease = waitResult.mustCallRelease;

				if (!waitResult.succeeded)
				{
					return false;
				}

				await action();
				return true;
			}
			finally
			{
				DoFinally(mustCallRelease:mustCallRelease);
			}
		}

		public async Task<(bool succeeded, TResult result)> TryDoSynchronizedWithTimeout<TResult>(TimeSpan timeout, Func<Task<TResult>> action)
		{
			bool mustCallRelease = false;

			try
			{
				var waitResult = await WaitWithTimeout(timeout);
				mustCallRelease = waitResult.mustCallRelease;

				if (!waitResult.succeeded)
				{
					return (succeeded:false, result:default);
				}

				var res = await action();
				return (succeeded: true, result: res);
			}
			finally
			{
				DoFinally(mustCallRelease: mustCallRelease);
			}
		}

		#endregion

		#region private

		private void DoFinally(bool mustCallRelease)
		{
			if(mustCallRelease)
			{
				_concurrencySemaphore.Release();
			}

			if(_reentranceable)
			{
				_taskStorage.Value.Value--;
			}
		}

		private void EnsureTaskStorage()
		{
			if(_reentranceable)
			{
				_taskStorage = new TaskStorage<BoxedObject<int>>(id: _taskStorageId.ToString());
			}
		}

		private async Task<(bool succeeded, bool mustCallRelease)> WaitEndless()
		{
			if (HandleReentrance())
			{
				return (succeeded: true, mustCallRelease: false);
			}

			if(_cancellationToken.HasValue)
			{
				_cancellationToken.Value.ThrowIfCancellationRequested();
				await _concurrencySemaphore.WaitAsync(cancellationToken: _cancellationToken.Value);
			}
			else
			{
				await _concurrencySemaphore.WaitAsync();
			}
			return (succeeded: true, mustCallRelease: true);
		}

		private bool HandleReentrance()
		{
			if(_reentranceable)
			{
				bool isReentrance = _taskStorage.Value.Value > 0;
				_taskStorage.Value.Value++;
				return isReentrance;
			}

			return false;
		}

		private async Task<(bool succeeded, bool mustCallRelease)> WaitWithTimeout(TimeSpan timeout)
		{
			if (HandleReentrance())
			{
				return (succeeded:true, mustCallRelease: false);
			}

			bool entered;

			if (_cancellationToken.HasValue)
			{
				_cancellationToken.Value.ThrowIfCancellationRequested();
				entered = await _concurrencySemaphore.WaitAsync(timeout: timeout, cancellationToken: _cancellationToken.Value);
			}
			else
			{
				entered = await _concurrencySemaphore.WaitAsync(timeout: timeout);
			}

			if (!entered)
			{
				return (succeeded: false, mustCallRelease: false);
			}

			return (succeeded: true, mustCallRelease: true);
		}

		#endregion
	}
}
