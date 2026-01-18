using System;
using System.Threading;
using System.Threading.Tasks;

namespace TUtils.Common.Async
{
	public class TSemaphore : IDisposable
	{
		private readonly CancellationToken? _cancellationToken;
		private readonly SemaphoreSlim _concurrencySemaphore = new SemaphoreSlim(1, 1);

		public TSemaphore(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
		}

		public TSemaphore()
		{
		}


		public async Task DoSynchronized(Func<Task> action)
		{
			await WaitEndless();

			try
			{
				await action();
			}
			finally
			{
				_concurrencySemaphore.Release();
			}
		}

		public async Task<bool> TryDoSynchronizedWithTimeout(TimeSpan timeout, Func<Task> action)
		{
			if(!await WaitWithTimeout(timeout))
			{
				return false;
			}

			try
			{
				await action();
				return true;
			}
			finally
			{
				_concurrencySemaphore.Release();
			}
		}

		private async Task WaitEndless()
		{
			if(_cancellationToken.HasValue)
			{
				await _concurrencySemaphore.WaitAsync(cancellationToken: _cancellationToken.Value);
			}
			else
			{
				await _concurrencySemaphore.WaitAsync();
			}
		}

		private async Task<bool> WaitWithTimeout(TimeSpan timeout)
		{
			bool entered = false;
			if (_cancellationToken.HasValue)
			{
				entered = await _concurrencySemaphore.WaitAsync(timeout: timeout, cancellationToken: _cancellationToken.Value);
			}
			else
			{
				entered = await _concurrencySemaphore.WaitAsync(timeout: timeout);
			}

			if (!entered)
			{
				return true;
			}

			return false;
		}

		public async Task<TResult> DoSynchronized<TResult>(Func<Task<TResult>> action)
		{
			await WaitEndless();

			try
			{
				return await action();
			}
			finally
			{
				_concurrencySemaphore.Release();
			}
		}

		public async Task<(bool succeeded, TResult result)> TryDoSynchronized<TResult>(TimeSpan timeout, Func<Task<TResult>> action)
		{
			if(!await WaitWithTimeout(timeout))
			{
				return (succeeded: false, result: default);
			}

			try
			{
				var result = await action();
				return (succeeded: true, result: result);
			}
			finally
			{
				_concurrencySemaphore.Release();
			}
		}

		public void Dispose()
		{
			_concurrencySemaphore?.Dispose();
		}
	}
}
