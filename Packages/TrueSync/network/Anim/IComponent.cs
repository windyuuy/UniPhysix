#region 程序集 UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// UnityEngine.CoreModule.dll
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using UnityEngine.Internal;
using UnityEngineInternal;

namespace UnityEngine
{
	public interface IOptionalComponent
    {

		//
		// 摘要:
		//     The ParticleSystem attached to this GameObject. (Null if there is none attached).
		Component particleSystem { get; }
		//
		// 摘要:
		//     The Rigidbody attached to this GameObject. (Null if there is none attached).
		Component rigidbody { get; }
		//
		// 摘要:
		//     The HingeJoint attached to this GameObject. (Null if there is none attached).
		Component hingeJoint { get; }
		//
		// 摘要:
		//     The Camera attached to this GameObject. (Null if there is none attached).
		Component camera { get; }
		//
		// 摘要:
		//     The Rigidbody2D that is attached to the Component's GameObject.
		Component rigidbody2D { get; }
		//
		// 摘要:
		//     The Animation attached to this GameObject. (Null if there is none attached).
		Component animation { get; }
		//
		// 摘要:
		//     The ConstantForce attached to this GameObject. (Null if there is none attached).
		Component constantForce { get; }
		//
		// 摘要:
		//     The Renderer attached to this GameObject. (Null if there is none attached).
		Component renderer { get; }
		//
		// 摘要:
		//     The AudioSource attached to this GameObject. (Null if there is none attached).
		Component audio { get; }
		//
		// 摘要:
		//     The NetworkView attached to this GameObject (Read Only). (null if there is none
		//     attached).
		Component networkView { get; }
		//
		// 摘要:
		//     The Collider attached to this GameObject. (Null if there is none attached).
		Component collider { get; }
		//
		// 摘要:
		//     The Collider2D component attached to the object.
		Component collider2D { get; }
		//
		// 摘要:
		//     The Light attached to this GameObject. (Null if there is none attached).
		Component light { get; }
	}

	//
	// 摘要:
	//     Base class for everything attached to GameObjects.
	public interface IComponent : IObject
	{
		//
		// 摘要:
		//     The Transform attached to this GameObject.
		Transform transform { get; }
		//
		// 摘要:
		//     The game object this component is attached to. A component is always attached
		//     to a game object.
		GameObject gameObject { get; }
		//
		// 摘要:
		//     The tag of this game object.
		string tag { get; set; }

		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object
		//     or any of its children.
		//
		// 参数:
		//   methodName:
		//     Name of the method to call.
		//
		//   parameter:
		//     Optional parameter to pass to the method (can be any value).
		//
		//   options:
		//     Should an error be raised if the method does not exist for a given target object?
		void BroadcastMessage(string methodName, SendMessageOptions options);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object
		//     or any of its children.
		//
		// 参数:
		//   methodName:
		//     Name of the method to call.
		//
		//   parameter:
		//     Optional parameter to pass to the method (can be any value).
		//
		//   options:
		//     Should an error be raised if the method does not exist for a given target object?
		[ExcludeFromDocs]
		void BroadcastMessage(string methodName);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object
		//     or any of its children.
		//
		// 参数:
		//   methodName:
		//     Name of the method to call.
		//
		//   parameter:
		//     Optional parameter to pass to the method (can be any value).
		//
		//   options:
		//     Should an error be raised if the method does not exist for a given target object?
		[ExcludeFromDocs]
		void BroadcastMessage(string methodName, object parameter);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object
		//     or any of its children.
		//
		// 参数:
		//   methodName:
		//     Name of the method to call.
		//
		//   parameter:
		//     Optional parameter to pass to the method (can be any value).
		//
		//   options:
		//     Should an error be raised if the method does not exist for a given target object?
		void BroadcastMessage(string methodName, [Internal.DefaultValue("null")] object parameter, [Internal.DefaultValue("SendMessageOptions.RequireReceiver")] SendMessageOptions options);
		//
		// 摘要:
		//     Is this game object tagged with tag ?
		//
		// 参数:
		//   tag:
		//     The tag to compare.
		bool CompareTag(string tag);
		//
		// 摘要:
		//     Returns the component of Type type if the game object has one attached, null
		//     if it doesn't.
		//
		// 参数:
		//   type:
		//     The type of Component to retrieve.
		Component GetComponent(Type type);
		T GetComponent<T>();
		//
		// 摘要:
		//     Returns the component with name type if the game object has one attached, null
		//     if it doesn't.
		//
		// 参数:
		//   type:
		Component GetComponent(string type);
		Component GetComponentInChildren(Type t, bool includeInactive);
		//
		// 摘要:
		//     Returns the component of Type type in the GameObject or any of its children using
		//     depth first search.
		//
		// 参数:
		//   t:
		//     The type of Component to retrieve.
		//
		// 返回结果:
		//     A component of the matching type, if found.
		Component GetComponentInChildren(Type t);
		T GetComponentInChildren<T>([Internal.DefaultValue("false")] bool includeInactive);
		[ExcludeFromDocs]
		T GetComponentInChildren<T>();
		//
		// 摘要:
		//     Returns the component of Type type in the GameObject or any of its parents.
		//
		// 参数:
		//   t:
		//     The type of Component to retrieve.
		//
		// 返回结果:
		//     A component of the matching type, if found.
		Component GetComponentInParent(Type t);
		T GetComponentInParent<T>();
		//
		// 摘要:
		//     Returns all components of Type type in the GameObject.
		//
		// 参数:
		//   type:
		//     The type of Component to retrieve.
		Component[] GetComponents(Type type);
		void GetComponents(Type type, List<Component> results);
		void GetComponents<T>(List<T> results);
		T[] GetComponents<T>();
		[ExcludeFromDocs]
		Component[] GetComponentsInChildren(Type t);
		void GetComponentsInChildren<T>(List<T> results);
		T[] GetComponentsInChildren<T>();
		void GetComponentsInChildren<T>(bool includeInactive, List<T> result);
		T[] GetComponentsInChildren<T>(bool includeInactive);
		//
		// 摘要:
		//     Returns all components of Type type in the GameObject or any of its children.
		//
		// 参数:
		//   t:
		//     The type of Component to retrieve.
		//
		//   includeInactive:
		//     Should Components on inactive GameObjects be included in the found set? includeInactive
		//     decides which children of the GameObject will be searched. The GameObject that
		//     you call GetComponentsInChildren on is always searched regardless.
		Component[] GetComponentsInChildren(Type t, bool includeInactive);
		T[] GetComponentsInParent<T>(bool includeInactive);
		[ExcludeFromDocs]
		Component[] GetComponentsInParent(Type t);
		//
		// 摘要:
		//     Returns all components of Type type in the GameObject or any of its parents.
		//
		// 参数:
		//   t:
		//     The type of Component to retrieve.
		//
		//   includeInactive:
		//     Should inactive Components be included in the found set?
		Component[] GetComponentsInParent(Type t, [Internal.DefaultValue("false")] bool includeInactive);
		T[] GetComponentsInParent<T>();
		void GetComponentsInParent<T>(bool includeInactive, List<T> results);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object.
		//
		// 参数:
		//   methodName:
		//     Name of the method to call.
		//
		//   value:
		//     Optional parameter for the method.
		//
		//   options:
		//     Should an error be raised if the target object doesn't implement the method for
		//     the message?
		void SendMessage(string methodName, object value, SendMessageOptions options);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object.
		//
		// 参数:
		//   methodName:
		//     Name of the method to call.
		//
		//   value:
		//     Optional parameter for the method.
		//
		//   options:
		//     Should an error be raised if the target object doesn't implement the method for
		//     the message?
		void SendMessage(string methodName);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object.
		//
		// 参数:
		//   methodName:
		//     Name of the method to call.
		//
		//   value:
		//     Optional parameter for the method.
		//
		//   options:
		//     Should an error be raised if the target object doesn't implement the method for
		//     the message?
		void SendMessage(string methodName, object value);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object.
		//
		// 参数:
		//   methodName:
		//     Name of the method to call.
		//
		//   value:
		//     Optional parameter for the method.
		//
		//   options:
		//     Should an error be raised if the target object doesn't implement the method for
		//     the message?
		void SendMessage(string methodName, SendMessageOptions options);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object
		//     and on every ancestor of the behaviour.
		//
		// 参数:
		//   methodName:
		//     Name of method to call.
		//
		//   value:
		//     Optional parameter value for the method.
		//
		//   options:
		//     Should an error be raised if the method does not exist on the target object?
		[ExcludeFromDocs]
		void SendMessageUpwards(string methodName, object value);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object
		//     and on every ancestor of the behaviour.
		//
		// 参数:
		//   methodName:
		//     Name of method to call.
		//
		//   value:
		//     Optional parameter value for the method.
		//
		//   options:
		//     Should an error be raised if the method does not exist on the target object?
		void SendMessageUpwards(string methodName, SendMessageOptions options);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object
		//     and on every ancestor of the behaviour.
		//
		// 参数:
		//   methodName:
		//     Name of method to call.
		//
		//   value:
		//     Optional parameter value for the method.
		//
		//   options:
		//     Should an error be raised if the method does not exist on the target object?
		[ExcludeFromDocs]
		void SendMessageUpwards(string methodName);
		//
		// 摘要:
		//     Calls the method named methodName on every MonoBehaviour in this game object
		//     and on every ancestor of the behaviour.
		//
		// 参数:
		//   methodName:
		//     Name of method to call.
		//
		//   value:
		//     Optional parameter value for the method.
		//
		//   options:
		//     Should an error be raised if the method does not exist on the target object?
		void SendMessageUpwards(string methodName, [Internal.DefaultValue("null")] object value, [Internal.DefaultValue("SendMessageOptions.RequireReceiver")] SendMessageOptions options);
		bool TryGetComponent<T>(out T component);
		bool TryGetComponent(Type type, out Component component);
	}
}