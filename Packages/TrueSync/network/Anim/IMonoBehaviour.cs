#region 程序集 UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// UnityEngine.CoreModule.dll
#endregion

using System;
using System.Collections;
using UnityEngine.Internal;

namespace UnityEngine
{
	public interface IOptionalMonoBehaviour : IBehaviour
    {

		//
		// 摘要:
		//     Allow a specific instance of a MonoBehaviour to run in edit mode (only available
		//     in the editor).
		bool runInEditMode { get; set; }

	}

	//
	// 摘要:
	//     MonoBehaviour is the base class from which every Unity script derives.
	public interface IMonoBehaviour : IBehaviour
	{
		//
		// 摘要:
		//     Disabling this lets you skip the GUI layout phase.
		bool useGUILayout { get; set; }
		// 摘要:
		//     Cancels all Invoke calls with name methodName on this behaviour.
		//
		// 参数:
		//   methodName:
		void CancelInvoke(string methodName);
		//
		// 摘要:
		//     Cancels all Invoke calls on this MonoBehaviour.
		void CancelInvoke();
		//
		// 摘要:
		//     Invokes the method methodName in time seconds.
		//
		// 参数:
		//   methodName:
		//
		//   time:
		void Invoke(string methodName, float time);
		//
		// 摘要:
		//     Invokes the method methodName in time seconds, then repeatedly every repeatRate
		//     seconds.
		//
		// 参数:
		//   methodName:
		//
		//   time:
		//
		//   repeatRate:
		void InvokeRepeating(string methodName, float time, float repeatRate);
		//
		// 摘要:
		//     Is any invoke on methodName pending?
		//
		// 参数:
		//   methodName:
		bool IsInvoking(string methodName);
		//
		// 摘要:
		//     Is any invoke pending on this MonoBehaviour?
		bool IsInvoking();
		//
		// 摘要:
		//     Starts a coroutine named methodName.
		//
		// 参数:
		//   methodName:
		//
		//   value:
		[ExcludeFromDocs]
		Coroutine StartCoroutine(string methodName);
		//
		// 摘要:
		//     Starts a Coroutine.
		//
		// 参数:
		//   routine:
		Coroutine StartCoroutine(IEnumerator routine);
		//
		// 摘要:
		//     Starts a coroutine named methodName.
		//
		// 参数:
		//   methodName:
		//
		//   value:
		Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value);
		Coroutine StartCoroutine_Auto(IEnumerator routine);
		//
		// 摘要:
		//     Stops all coroutines running on this behaviour.
		void StopAllCoroutines();
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
		void StopCoroutine(IEnumerator routine);
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
		void StopCoroutine(Coroutine routine);
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
		void StopCoroutine(string methodName);
	}
}