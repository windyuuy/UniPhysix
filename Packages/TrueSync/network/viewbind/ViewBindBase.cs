
using System;

namespace fsync.viewbind
{
	public class ViewData
	{
		public bool IsDirty = false;

		/// <summary>
		/// 更新脏数据
		/// </summary>
		public virtual void UpdateDirty()
		{

		}

		public virtual void Update()
		{
			this.UpdateDirty();
		}
	}
	public class TValue<T> : ViewData
	{
		// User-defined conversion from TValue<T> to T
		public static implicit operator T(TValue<T> v)
		{
			return v.Value;
		}

		protected T value = default(T);
		public virtual TValue<T> Set(T v)
		{
			if ((object)this.value != (object)v)
			{
				this.value = v;
				this.IsDirty = true;
			}
			return this;
		}
		public T Value => this.value;
	}
	public class TFloat : TValue<float>
	{
		public override TValue<float> Set(float v)
		{
			if (this.value != v)
			{
				this.value = v;
				this.IsDirty = true;
			}
			return this;
		}
	}
	public class TBool : TValue<bool>
	{
		public override TValue<bool> Set(bool v)
		{
			if (this.value != v)
			{
				this.value = v;
				this.IsDirty = true;
			}
			return this;
		}
	}

	public class Quaternion : ViewData
	{
		//
		// 摘要:
		//     X component of the Quaternion. Don't modify this directly unless you know quaternions
		//     inside out.
		public TFloat x = new TFloat();
		//
		// 摘要:
		//     Y component of the Quaternion. Don't modify this directly unless you know quaternions
		//     inside out.
		public TFloat y = new TFloat();
		//
		// 摘要:
		//     Z component of the Quaternion. Don't modify this directly unless you know quaternions
		//     inside out.
		public TFloat z = new TFloat();
		//
		// 摘要:
		//     W component of the Quaternion. Do not directly modify quaternions.
		public TFloat w = new TFloat();

		UnityEngine.Quaternion value = new UnityEngine.Quaternion();

		public Quaternion Set(UnityEngine.Quaternion v)
		{
			this.x.Set(v.x);
			this.y.Set(v.y);
			this.z.Set(v.z);
			this.w.Set(v.w);
			this.IsDirty = this.x.IsDirty || this.y.IsDirty || this.z.IsDirty || this.w.IsDirty;

			if (this.IsDirty)
			{
				this.value.Set(v.x, v.y, v.z, v.w);
			}

			return this;
		}

		public UnityEngine.Quaternion Value => this.value;
	}

	public class Vector3 : ViewData
	{
		//
		// 摘要:
		//     X component of the vector.
		public TFloat x = new TFloat();
		//
		// 摘要:
		//     Y component of the vector.
		public TFloat y = new TFloat();
		//
		// 摘要:
		//     Z component of the vector.
		public TFloat z = new TFloat();

		public UnityEngine.Vector3 value = new UnityEngine.Vector3();

		public Vector3 Set(UnityEngine.Vector3 v)
		{
			this.x.Set(v.x);
			this.y.Set(v.y);
			this.z.Set(v.z);
			this.IsDirty = this.x.IsDirty || this.y.IsDirty || this.z.IsDirty;

			if (this.IsDirty)
			{
				this.value.Set(v.x, v.y, v.z);
			}

			return this;
		}

		public UnityEngine.Vector3 Value => this.value;
	}

	public class Transform : ViewData
	{
		//
		// 摘要:
		//     A Quaternion that stores the rotation of the Transform in world space.
		public Quaternion _rotation = new Quaternion();
		public UnityEngine.Quaternion rotation
		{
			get => this._rotation.Value;
			set => this._rotation.Set(value);
		}
		//
		// 摘要:
		//     The world space position of the Transform.
		public Vector3 _position = new Vector3();
		public UnityEngine.Vector3 position
		{
			get => this._position.Value;
			set => this._position.Set(value);
		}

		public Vector3 _eulerAngles = new Vector3();
		//
		// 摘要:
		//     The rotation as Euler angles in degrees.
		public UnityEngine.Vector3 eulerAngles
		{
			get => this._eulerAngles.Value;
			set => this._eulerAngles.Set(value);
		}


		public Vector3 _forward = new Vector3();
		//
		// 摘要:
		//     Returns a normalized vector representing the blue axis of the transform in world
		//     space.
		public UnityEngine.Vector3 forward
		{
			get => this._forward.Value;
			set => this._forward.Set(value);
		}

		public Transform Set(UnityEngine.Transform v)
		{
			this._rotation.Set(v.rotation);
			this._position.Set(v.position);
			this._eulerAngles.Set(v.eulerAngles);
			this._forward.Set(v.forward);
			this.IsDirty = this._position.IsDirty
				|| this._rotation.IsDirty
				|| this._eulerAngles.IsDirty
				|| this._forward.IsDirty
				|| false;
			return this;
		}
	}

	/// <summary>
	/// 视图绑定数据
	/// </summary>
	public class GameObject : ViewData
	{
		//
		// 摘要:
		//     The Transform attached to this GameObject.
		public Transform transform = new Transform();

		/// <summary>
		/// 绑定的游戏对象
		/// </summary>
		protected UnityEngine.GameObject value = null;

		public GameObject Set(UnityEngine.GameObject value)
		{
			this.transform.Set(value.transform);
			return this;
		}

		/// <summary>
		/// 绑定视图对象
		/// </summary>
		/// <param name="v"></param>
		public virtual void Bind(UnityEngine.GameObject v)
		{
			this.value = v;
			this.Set(v);
		}

		/// <summary>
		/// 更新脏数据
		/// </summary>
		public override void UpdateDirty()
		{
			if (this.transform.IsDirty)
			{
				if (this.transform._rotation.IsDirty)
				{
					this.value.transform.rotation = this.transform.rotation;
				}
				if (this.transform._position.IsDirty)
				{
					this.value.transform.position = this.transform.position;
				}
			}
			if (this.active.IsDirty)
			{
				this.value.SetActive(this.active.Value);
			}
		}

		protected TBool active = new TBool();

		public void SetActive(bool v)
		{
			this.active.Set(v);
		}
	}

	public class MonoBehaviour : ViewData
	{

		// TODO: 需要抽离
		protected GameObject gameObject = new GameObject();

		/// <summary>
		/// 绑定的游戏组件
		/// </summary>
		protected UnityEngine.MonoBehaviour value = null;

		public Transform transform => this.gameObject.transform;

		public MonoBehaviour Set(UnityEngine.MonoBehaviour value)
		{
			this.gameObject.Set(value.gameObject);
			return this;
		}

		/// <summary>
		/// 绑定视图对象
		/// </summary>
		/// <param name="v"></param>
		public virtual void Bind(UnityEngine.MonoBehaviour v)
		{
			this.value = v;
			this.gameObject.Bind(this.value.gameObject);
		}

		public override void UpdateDirty()
		{
			this.gameObject.UpdateDirty();
		}
	}
}
