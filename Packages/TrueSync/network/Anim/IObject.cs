#region 程序集 UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// UnityEngine.CoreModule.dll
#endregion

using System;
using System.Security;
using UnityEngine.Internal;
using UnityEngineInternal;

namespace UnityEngine
{
	//
	// 摘要:
	//     Base class for all objects Unity can reference.
	public interface IObject
	{
		//
		// 摘要:
		//     Should the object be hidden, saved with the Scene or modifiable by the user?
		HideFlags hideFlags { get; set; }
		//
		// 摘要:
		//     The name of the object.
		string name { get; set; }

		bool Equals(object other);
		int GetHashCode();
		//
		// 摘要:
		//     Returns the instance id of the object.
		int GetInstanceID();
		//
		// 摘要:
		//     Returns the name of the object.
		//
		// 返回结果:
		//     The name returned by ToString.
		string ToString();


	}
}