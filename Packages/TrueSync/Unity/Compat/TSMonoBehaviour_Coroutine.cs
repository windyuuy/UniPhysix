
using System.Collections.Generic;
using System.Collections;
using fsync;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace TrueSync
{
	public partial class TSMonoBehaviour2 : MonoBehaviour
	{
		protected List<TSCoroutine> coroutines = new List<TSCoroutine>();
		private void OnEnable()
		{

		}
		private void OnDisable()
		{
			if (!this.gameObject.activeInHierarchy)
			{
				// component.enable 不影响协程
				// gameObject.activeInHierarchy->false 会导致对象上协程全部立即清除, 协程后续不会执行
				// gameObject.activeInHierarchy 为false, 那么 StartCoroutine 会报错: "Coroutine couldn't be started because the the game object 'GameObject' is inactive!"

				this.StopAllCoroutinesA();
			}
		}

		protected void OnCoroutineFinished(TSCoroutine coroutine)
		{
			this.coroutines.Remove(coroutine);
		}

		//
		// 摘要:
		//     Starts a Coroutine.
		//
		// 参数:
		//   routine:
		public TSCoroutine StartCoroutineA(IEnumerator routine)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				Debug.LogError("Coroutine couldn't be started because the the game object 'GameObject' is inactive!");
				return null;
			}

			var coroutine = CoroutineManager.SharedScheduler.StartCoroutine(routine);
			// 注册移除事件
			coroutine.OnFinished = this.OnCoroutineFinished;
			this.coroutines.Add(coroutine);
			return coroutine;
		}
		//
		// 摘要:
		//     Stops all coroutines running on this behaviour.
		public void StopAllCoroutinesA()
		{
			foreach (var coroutine in coroutines)
			{
				CoroutineManager.SharedScheduler.RemoveCoroutine(coroutine);
			}
			this.coroutines.Clear();
		}
		//
		// 摘要:
		//     Stops the first coroutine named methodName, or the coroutine stored in routine
		//     running on this behaviour.
		//
		// 参数:
		//   methodName:
		//     Name of coroutine.
		//
		//   routine:
		//     Name of the function in code, including coroutines.
		public void StopCoroutineA(IEnumerator routine)
		{
			if (routine == null)
			{
				return;
			}

			var coroutine = coroutines.FirstOrDefault(c => c.fiber == routine);
			if (coroutine != null)
			{
				CoroutineManager.SharedScheduler.RemoveCoroutine(coroutine);
				this.coroutines.Remove(coroutine);
			}
		}
		//
		// 摘要:
		//     Stops the first coroutine named methodName, or the coroutine stored in routine
		//     running on this behaviour.
		//
		// 参数:
		//   methodName:
		//     Name of coroutine.
		//
		//   routine:
		//     Name of the function in code, including coroutines.
		public void StopCoroutineA(TSCoroutine coroutine)
		{
			if (coroutine != null)
			{
				CoroutineManager.SharedScheduler.RemoveCoroutine(coroutine);
				this.coroutines.Remove(coroutine);
			}
		}
	}

	public partial class TSMonoBehaviour : MonoBehaviour
	{
#if false
		//
		// 摘要:
		//     Starts a Coroutine.
		//
		// 参数:
		//   routine:
		public new TSCoroutine StartCoroutine(IEnumerator routine)
		{
			return tsHelper.StartCoroutineA(routine);
		}
		//
		// 摘要:
		//     Stops all coroutines running on this behaviour.
		public new void StopAllCoroutines()
		{
			tsHelper.StopAllCoroutinesA();
		}
		//
		// 摘要:
		//     Stops the first coroutine named methodName, or the coroutine stored in routine
		//     running on this behaviour.
		//
		// 参数:
		//   methodName:
		//     Name of coroutine.
		//
		//   routine:
		//     Name of the function in code, including coroutines.
		public new void StopCoroutine(IEnumerator routine)
		{
			tsHelper.StopCoroutineA(routine);
		}
		//
		// 摘要:
		//     Stops the first coroutine named methodName, or the coroutine stored in routine
		//     running on this behaviour.
		//
		// 参数:
		//   methodName:
		//     Name of coroutine.
		//
		//   routine:
		//     Name of the function in code, including coroutines.
		public void StopCoroutine(TSCoroutine coroutine)
		{
			tsHelper.StopCoroutineA(coroutine);
		}
#endif
	}
}
