namespace NeatMapper.Core.Internal {
	internal static class TaskUtils {
		public static async Task<TResult> AwaitTask<TResult>(Task task) {
			await task;
			if (task.IsFaulted) {
				if(task.Exception != null){
					if(task.Exception.InnerExceptions.Count > 1)
						throw task.Exception;
					else
						throw task.Exception.InnerExceptions.Single();
				}
				else
					throw new Exception("Task is faulted");
			}
			else if (task.IsCanceled) 
				throw new TaskCanceledException();
			else {
				try {
					return (TResult)task.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(task)!;
				}
				catch {
					throw new Exception("Task contains no result");
				}
			}
		}
	}
}
