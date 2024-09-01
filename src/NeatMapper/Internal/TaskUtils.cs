#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// Helper class to await Tasks and return the result via reflection.
	/// </summary>
	internal static class TaskUtils {
		/// <summary>
		/// Awaits a task and casts the result.
		/// </summary>
		public static async Task<TResult> AwaitTask<TResult>(Task<object> task) {
			return (TResult)await task;
		}

		// https://stackoverflow.com/a/60482164/2672235
		public static Task<TResult[]> WhenAllFailFast<TResult>(params Task<TResult>[] tasks) {
			if (tasks is null)
				throw new ArgumentNullException(nameof(tasks));
			if (tasks.Length == 0)
				return Task.FromResult(Array.Empty<TResult>());

			var results = new TResult[tasks.Length];
			var remaining = tasks.Length;
			var tcs = new TaskCompletionSource<TResult[]>(TaskCreationOptions.RunContinuationsAsynchronously);

			for (int i = 0; i < tasks.Length; i++) {
				var task = tasks[i]
					?? throw new ArgumentException($"The {nameof(tasks)} argument included a null value.", nameof(tasks));
				HandleCompletion(task, i);
			}
			return tcs.Task;


			async void HandleCompletion(Task<TResult> task, int index) {
				try {
					var result = await task.ConfigureAwait(false);
					results[index] = result;
					if (Interlocked.Decrement(ref remaining) == 0) 
						tcs.TrySetResult(results);
				}
				catch (OperationCanceledException) {
					tcs.TrySetCanceled();
				}
				catch (Exception ex) {
					tcs.TrySetException(ex);
				}
			}
		}
	}
}
