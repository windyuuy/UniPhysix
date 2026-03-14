
using System.Collections.Generic;
using fsync;
using UnityEngine;
using System.Reflection;
using System.Linq;

using TPlayerId = System.String;

namespace TrueSync
{

	public partial class TSMonoBehaviour : MonoBehaviour, IWithGenId
	{
		/// <summary>
		/// 尝试按照已添加的原生物理组件添加ts版的物理组件
		/// </summary>
		public virtual void AttachMissingPhysicsComps()
		{
			ExtendTSGameObject.AttachMissingPhysicsComps(this.gameObject);
		}

		public virtual void ForeachAttachMissingPhysicsComps()
		{
			ExtendTSGameObject.ForeachAttachMissingPhysicsComps(this.gameObject);
		}

		public static TSTransform GetTransform(GameObject gameObject)
		{
			// return gameObject.GetComponent<TSTransform>();
			return gameObject.ReferTransform();
		}

		public virtual TSTransform GetTransform()
		{
			// return this.GetComponent<TSTransform>();
			return this.ReferTransform();
		}

		public new TSTransform transform
		{
			get => this.GetTransform();
		}

		public Transform uetransform => base.transform;

	}

	public partial class TSMonoBehaviour : MonoBehaviour, IFrameSyncUpdate
	{

		#region 帧同步
		protected TSMonoBehaviour2 tsHelper;

		// TODO: 通过额外添加代理组件, 提供 awake 事件
		protected virtual void Awake()
		{
			tsHelper = this.GetOrAddComponent<TSMonoBehaviour2>();

			GenIndex();

			this.ForeachAttachMissingPhysicsComps();

			// 注册帧同步接口
			fsync.GameObjectManager.Inst.Add(this);

			// Awake2();
			this.SendCompMsg("Awake2");
		}

		public virtual void GenIndex()
		{
			fsync.NameSpaceManager.GenIndex(this);
		}

		protected virtual void OnDestroy()
		{
			this.SendCompMsg("OnDestroy2");

			// 反注册帧同步接口
			fsync.GameObjectManager.Inst.Remove(this);
		}

		#region localid
		public int LocalOID => fsync.NameSpaceManager.GetUID(this);
		public int genIndex { get; set; } = 0;
		#endregion

		protected INetTimer Time = null;

		public INetTimer NetTimer
		{
			get => Time;
			set => Time = value;
		}

		/// <summary>
		/// 对象ID
		/// - 用于帧同步唯一索引对象
		/// </summary>
		/// <value></value>
		public TPlayerId OID => GetOID();

		public virtual TPlayerId GetOID()
		{
			return "";
		}

		/// <summary>
		/// 对象ID
		/// - 仅用于本地唯一索引对象
		/// </summary>
		/// <value></value>

		public virtual void FrameSyncUpdate()
		{
			// TODO: 需要确认是否存在 active 延迟生效问题, 会存在逻辑不同步问题
			if (!this.gameObject.activeInHierarchy)
			{
				return;
			}

			if (!this.enabled)
			{
				return;
			}

			// this.UpdateFrame();
			this.SendCompMsg("UpdateTS");
		}

		// protected virtual void UpdateTS()
		// {

		// }

		public virtual void FrameSyncLateUpdate()
		{
			// TODO: 需要确认是否存在 active 延迟生效问题, 会存在逻辑不同步问题
			if (!this.gameObject.activeInHierarchy)
			{
				return;
			}

			if (!this.enabled)
			{
				return;
			}

			// this.UpdateFrame();
			this.SendCompMsg("LateUpdateTS");
		}

		public virtual void HandleFrameCmd(FrameCmd cmd)
		{
			// throw new System.NotImplementedException();
			this.SendCompMsg("HandleTSCmd", cmd, SendMessageOptions.RequireReceiver);
		}

		#endregion
	}

	public partial class TSMonoBehaviour : MonoBehaviour
	{
		/// <summary>
		/// 对象数量不多, 则直接存储在对象上, 加快访问速度
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="value"></param>
		/// <param name="options"></param>
		private static Dictionary<System.Type, Dictionary<string, MethodInfo>> staticTSCompMethodsMap = new Dictionary<System.Type, Dictionary<string, MethodInfo>>();
		private static object[] EmtpyParams = new object[] { };
		private static object[] OneParams = new object[] { null };
		private const BindingFlags getMethodFlag = (BindingFlags)(((int)BindingFlags.OptionalParamBinding) * 2 - 1);
		private Dictionary<string, MethodInfo> getStaticMethodMap()
		{
			var clsType = this.GetType();
			if (!staticTSCompMethodsMap.ContainsKey(clsType))
			{
				var methodMap = new Dictionary<string, MethodInfo>();
				staticTSCompMethodsMap.Add(clsType, methodMap);
				return methodMap;
			}
			else
			{
				return staticTSCompMethodsMap[clsType];
			}
		}
		private Dictionary<string, MethodInfo> tsCompMethodsMap = new Dictionary<string, MethodInfo>();
		private MethodInfo getMethod(string methodName, out bool exist)
		{
			if (tsCompMethodsMap.ContainsKey(methodName))
			{
				var methodInfo = tsCompMethodsMap[methodName];
				exist = true;
				return methodInfo;
			}

			var methodMap = this.getStaticMethodMap();
			if (methodMap.ContainsKey(methodName))
			{
				var methodInfo = methodMap[methodName];
				tsCompMethodsMap[methodName] = methodInfo;
				exist = true;
				return methodInfo;
			}

			exist = false;
			return null;
		}
		private void addMethod(string methodName, MethodInfo method)
		{
			var methodMap = this.getStaticMethodMap();
			methodMap[methodName] = method;
			tsCompMethodsMap[methodName] = method;
		}
		private bool rawInvokeMethodCompat(ref System.Type t, ref string methodName, ref object value)
		{
			foreach (var method1 in t.GetMethods(getMethodFlag))
			{
				if (method1.GetParameters().Length == 1 && value.GetType().IsAssignableFrom(method1.GetParameters().First().ParameterType))
				{
					addMethod(methodName, method1);
					method1.Invoke(this, new object[] { value });
					return true;
				}
			}
			return false;
		}
		private bool rawInvokeMethod(ref System.Type t, ref string methodName, ref object value)
		{
			if (value != null)
			{
				{
					var method1 = t.GetMethod(methodName, getMethodFlag, null, new System.Type[] { value.GetType() }, null);
					if (method1 != null)
					{
						addMethod(methodName, method1);
						method1.Invoke(this, new object[] { value });
						return true;
					}
				}

				if (rawInvokeMethodCompat(ref t, ref methodName, ref value))
				{
					return true;
				}

				{
					var method2 = t.GetMethod(methodName, getMethodFlag, null, new System.Type[] { }, null);
					if (method2 != null)
					{
						addMethod(methodName, method2);
						method2.Invoke(this, new object[] { value });
						return true;
					}
				}

				{
					var method3 = t.GetMethod(methodName);
					if (method3 != null)
					{
						addMethod(methodName, method3);
						if (method3.GetParameters().Length == 0)
						{
							method3.Invoke(this, EmtpyParams);
						}
						else
						{
							method3.Invoke(this, new object[] { value });
						}
						return true;
					}
				}

			}
			else
			{
				{
					var method4 = t.GetMethod(methodName, getMethodFlag, null, new System.Type[] { }, null);
					if (method4 != null)
					{
						addMethod(methodName, method4);
						method4.Invoke(this, EmtpyParams);
						return true;
					}
				}

				{
					var method5 = t.GetMethod(methodName);
					if (method5 != null)
					{
						addMethod(methodName, method5);
						if (method5.GetParameters().Length == 0)
						{
							method5.Invoke(this, EmtpyParams);
						}
						else
						{
							method5.Invoke(this, new object[] { value });
						}
						return true;
					}
				}

			}

			var baseType = t.BaseType;
			if (baseType != typeof(TSMonoBehaviour))
			{
				return rawInvokeMethod(ref baseType, ref methodName, ref value);
			}

			addMethod(methodName, null);
			return false;
		}
		private bool rawInvokeMethod(ref System.Type t, ref string methodName)
		{
			{
				var method0 = t.GetMethod(methodName, getMethodFlag, null, new System.Type[] { }, null);
				if (method0 != null)
				{
					addMethod(methodName, method0);
					method0.Invoke(this, EmtpyParams);
					return true;
				}
			}

			{
				var method3 = t.GetMethod(methodName);
				if (method3 != null)
				{
					if (method3.GetParameters().Length == 0)
					{
						addMethod(methodName, method3);
						method3.Invoke(this, EmtpyParams);
						return true;
					}
				}
			}

			var baseType = t.BaseType;
			if (baseType != typeof(TSMonoBehaviour))
			{
				return rawInvokeMethod(ref baseType, ref methodName);
			}

			addMethod(methodName, null);
			return false;
		}
		public void SendCompMsg(string methodName, object value, SendMessageOptions options = SendMessageOptions.DontRequireReceiver)
		{
			var method2 = getMethod(methodName, out var exist);
			if (exist)
			{
				if (method2 != null)
				{
#if UNITY_EDITOR
					try
					{
#endif
						method2.Invoke(this, new object[] { value });
#if UNITY_EDITOR
					}
					catch (System.Exception e)
					{
						throw e;
					}
#endif
					return;
				}
			}
			else
			{
				var t = this.GetType();
				if (rawInvokeMethod(ref t, ref methodName, ref value))
				{
					return;
				}
			}

			if (options == SendMessageOptions.RequireReceiver)
			{
				throw new System.Exception($"invalid method <{methodName}> to call");
			}

			return;
		}
		public void SendCompMsg(string methodName, SendMessageOptions options = SendMessageOptions.DontRequireReceiver)
		{
			var method2 = getMethod(methodName, out var exist);
			if (exist)
			{
				if (method2 != null)
				{
					method2.Invoke(this, EmtpyParams);
					return;
				}
			}
			else
			{
				var t = this.GetType();
				if (rawInvokeMethod(ref t, ref methodName))
				{
					return;
				}
			}

			if (options == SendMessageOptions.RequireReceiver)
			{
				throw new System.Exception($"invalid method <{methodName}> to call");
			}

			return;
		}
	}
	public partial class TSMonoBehaviour : MonoBehaviour
	{

		#region 触发器
		public void OnSyncedTriggerEnter(TSCollision other)
		{
			OnTSTriggerEnter(other);
			SendCompMsg("OnTSTriggerEnter", other.collider, SendMessageOptions.DontRequireReceiver);
		}

		protected virtual void OnTSTriggerEnter(TSCollision other)
		{
		}

		public void OnSyncedTriggerStay(TSCollision other)
		{
			OnTSTriggerStay(other);
			SendCompMsg("OnTSTriggerStay", other.collider, SendMessageOptions.DontRequireReceiver);
		}

		protected virtual void OnTSTriggerStay(TSCollision other)
		{
		}


		public void OnSyncedTriggerExit(TSCollision other)
		{
			OnTSTriggerExit(other);
			SendCompMsg("OnTSTriggerExit", other.collider, SendMessageOptions.DontRequireReceiver);
		}

		protected virtual void OnTSTriggerExit(TSCollision other)
		{
		}

		#endregion


		#region 碰撞器
		public void OnSyncedCollisionEnter(TSCollision other)
		{
			OnTSCollisionEnter(other);
			SendCompMsg("OnTSCollisionEnter", other.collider, SendMessageOptions.DontRequireReceiver);
		}

		protected virtual void OnTSCollisionEnter(TSCollision other)
		{
		}

		public void OnSyncedCollisionStay(TSCollision other)
		{
			OnTSCollisionStay(other);
			SendCompMsg("OnTSCollisionStay", other.collider, SendMessageOptions.DontRequireReceiver);
		}

		protected virtual void OnTSCollisionStay(TSCollision other)
		{
		}

		public void OnSyncedCollisionExit(TSCollision other)
		{
			OnTSCollisionExit(other);
			SendCompMsg("OnTSCollisionExit", other.collider, SendMessageOptions.DontRequireReceiver);
		}

		protected virtual void OnTSCollisionExit(TSCollision other)
		{
		}

		#endregion
	}

}