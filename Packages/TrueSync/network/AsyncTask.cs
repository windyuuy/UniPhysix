using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System;

public class TaskEnumerator : IEnumerator
{
	Task task;
	protected bool isTaskDone = false;
	public TaskEnumerator(Task task)
	{
		this.task = task;
	}
	public object Current
	{
		get
		{
			return null;
		}
	}

	public bool MoveNext()
	{
        if (task.IsFaulted)
        {
			throw task.Exception;
        }

		return !task.IsCompleted;
	}

	public void Reset()
	{
		if (task.Status != TaskStatus.Running)
		{
			task.Start();
		}
		else
		{
			throw new Exception("one task cannot be start twice");
		}
	}
}

public class TaskEnumerator<T> : IEnumerator
{
	Task<T> task;
	protected bool isTaskDone = false;
	public TaskEnumerator(Task<T> task)
	{
		this.task = task;
	}
	public object Current
	{
		get
		{
			if (task.IsCompleted)
			{
				return task.Result;
			}
			else
			{
				return null;
			}
		}
	}

	public bool MoveNext()
	{
		return !task.IsCompleted;
	}

	public void Reset()
	{
		if (task.Status != TaskStatus.Running)
		{
			task.Start();
		}
		else
		{
			throw new Exception("one task cannot be start twice");
		}
	}
}

delegate void Resolve(object p);
delegate void Reject(Exception e);

public class AsyncTask
{
	/// <summary>
	/// 生成异步任务
	/// </summary>
	/// <param name="action"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static Task<T> Run<T>(Action<Action<T>, Action<Exception>> action)
	{
		var task = New(action);
		task.Start();
		return task;
	}
	public static Task<T> New<T>(Action<Action<T>, Action<Exception>> action)
	{
		return new Task<T>(() =>
		{
			var signal = new ManualResetEvent(false);
			var isOk = false;
			/// <summary>
			/// 判断是否执行过回调, 执行过则略过回调
			/// </summary>
			var isDone = false;
			T result = default(T);
			System.Action<T> resolve = (T p) =>
		   {
			   if (isDone) { return; }
			   isDone = true;
			   isOk = true;
			   result = p;
			   signal.Set();
		   };
			Exception exception = null;
			Action<Exception> reject = (e) =>
			{
				if (isDone) { return; }
				isDone = true;
				exception = e;
				signal.Set();
			};
			action.Invoke(resolve, reject);
			signal.WaitOne();
			signal.Dispose();
			if (isOk)
			{
				return result;
			}
			else
			{
				if (exception == null)
				{
					exception = new Exception();
				}

				throw exception;
			}
		});
	}
	public static Task Run(Action<Action, Action<Exception>> action)
	{
		var task = New(action);
		task.Start();
		return task;
	}
	public static Task New(Action<Action, Action<Exception>> action)
	{
		return new Task(() =>
		{
			var signal = new ManualResetEvent(false);
			var isOk = false;
			/// <summary>
			/// 判断是否执行过回调, 执行过则略过回调
			/// </summary>
			var isDone = false;
			System.Action resolve = () =>
		   {
			   if (isDone) { return; }
			   isDone = true;
			   isOk = true;
			   signal.Set();
		   };
			Exception exception = null;
			Action<Exception> reject = (e) =>
			{
				if (isDone) { return; }
				isDone = true;
				exception = e;
				signal.Set();
			};
			action.Invoke(resolve, reject);
			signal.WaitOne();
			signal.Dispose();
			if (isOk)
			{
				return;
			}
			else
			{
				if (exception == null)
				{
					exception = new Exception();
				}

				throw exception;
			}
		});
	}

	/// <summary>
	/// 支持异步转协程
	/// </summary>
	/// <param name="task"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static TaskEnumerator<T> ToEnumerable<T>(Task<T> task)
	{
		return new TaskEnumerator<T>(task);
	}
	/// <summary>
	/// 支持异步转协程
	/// </summary>
	/// <param name="task"></param>
	/// <returns></returns>
	public static TaskEnumerator ToEnumerable(Task task)
	{
		return new TaskEnumerator(task);
	}
}
