using TUtils.Common.Logging;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
	public static class TaskExtension
	{
		/// <summary>
		/// Logs exception but doesn't catch exceptions
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static async Task LogExceptions(this Task task)
		{
			await task.ContinueWith(tsk => typeof(TaskExtension).Log().LogError(e:tsk.Exception), TaskContinuationOptions.OnlyOnFaulted);
			await task;
		}

		/// <summary>
		/// Logs exception but doesn't catch exceptions
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="task"></param>
		/// <returns></returns>
		public static async Task<T> LogExceptions<T>(this Task<T> task)
		{
			await task.ContinueWith(tsk => typeof(TaskExtension).Log().LogError(e: tsk.Exception), TaskContinuationOptions.OnlyOnFaulted);
			return await task;
		}

		public static T WaitAndGetResult<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			task.Wait(cancellationToken);
			return task.Result;
		}
	}
}
