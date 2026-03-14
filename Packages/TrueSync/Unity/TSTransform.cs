using UnityEngine;
using System.Collections.Generic;

namespace TrueSync
{
	/**
    *  @brief A deterministic version of Unity's Transform component for 3D physics. 
    **/
	[ExecuteInEditMode]
	[AddComponentMenu("TrueSync/TSTransform", 0)]
	public partial class TSTransform : MonoBehaviour
	{
		protected void Awake()
		{
			markNewTSTransform(this);
		}
		protected void OnDestroy()
		{
			markDeleteTSTransform(this);
		}

		[SerializeField]
		[HideInInspector]
		private bool _serialized;

		private static TSVector zero = TSVector.zero;
		private ref TSVector scaledCenter
		{
			get
			{
				if (tsCollider != null)
				{
					return ref tsCollider.ScaledCenter;
				}

				return ref zero;
			}
		}

		private void GetScaledCenter(out TSVector result)
		{
			tsCollider.GetScaledCenter(out result);
		}

		/**
        *  @brief Rotates game object to point forward vector to a target position. 
        *  
        *  @param other TSTrasform used to get target position.
        **/
		public void LookAt(TSTransform other)
		{
			LookAt(other.position);
		}

		/**
        *  @brief Rotates game object to point forward vector to a target position. 
        *  
        *  @param target Target position.
        **/
		public void LookAt(TSVector target)
		{
			this.rotation = TSQuaternion.CreateFromMatrix(TSMatrix.CreateFromLookAt(position, target));
		}

		/**
        *  @brief Moves game object based on provided axis values. 
        **/
		public void Translate(FP x, FP y, FP z)
		{
			Translate(x, y, z, Space.Self);
		}

		/**
        *  @brief Moves game object based on provided axis values and a relative space.
        *  
        *  If relative space is SELF then the game object will move based on its forward vector.
        **/
		public void Translate(FP x, FP y, FP z, Space relativeTo)
		{
			Translate(new TSVector(x, y, z), relativeTo);
		}

		/**
        *  @brief Moves game object based on provided axis values and a relative {@link TSTransform}.
        *  
        *  The game object will move based on TSTransform's forward vector.
        **/
		public void Translate(FP x, FP y, FP z, TSTransform relativeTo)
		{
			Translate(new TSVector(x, y, z), relativeTo);
		}

		/**
        *  @brief Moves game object based on provided translation vector.
        **/
		public void Translate(TSVector translation)
		{
			Translate(translation, Space.Self);
		}

		/**
        *  @brief Moves game object based on provided translation vector and a relative space.
        *  
        *  If relative space is SELF then the game object will move based on its forward vector.
        **/
		public void Translate(TSVector translation, Space relativeTo)
		{
			if (relativeTo == Space.Self)
			{
				Translate(translation, this);
			}
			else
			{
				this.position += translation;
			}
		}

		/**
        *  @brief Moves game object based on provided translation vector and a relative {@link TSTransform}.
        *  
        *  The game object will move based on TSTransform's forward vector.
        **/
		public void Translate(TSVector translation, TSTransform relativeTo)
		{
			this.position += TSVector.Transform(translation, TSMatrix.CreateFromQuaternion(relativeTo.rotation));
		}

		/**
        *  @brief Rotates game object based on provided axis, point and angle of rotation.
        **/
		public void RotateAround(TSVector point, TSVector axis, FP angle)
		{
			TSVector vector = this.position;
			TSVector vector2 = vector - point;
			vector2 = TSVector.Transform(vector2, TSMatrix.AngleAxis(angle * FP.Deg2Rad, axis));
			vector = point + vector2;
			this.position = vector;

			Rotate(axis, angle);
		}

		/**
        *  @brief Rotates game object based on provided axis and angle of rotation.
        **/
		public void RotateAround(TSVector axis, FP angle)
		{
			Rotate(axis, angle);
		}

		/**
        *  @brief Rotates game object based on provided axis angles of rotation.
        **/
		public void Rotate(FP xAngle, FP yAngle, FP zAngle)
		{
			Rotate(new TSVector(xAngle, yAngle, zAngle), Space.Self);
		}

		/**
        *  @brief Rotates game object based on provided axis angles of rotation and a relative space.
        *  
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
		public void Rotate(FP xAngle, FP yAngle, FP zAngle, Space relativeTo)
		{
			Rotate(new TSVector(xAngle, yAngle, zAngle), relativeTo);
		}

		/**
        *  @brief Rotates game object based on provided axis angles of rotation.
        **/
		public void Rotate(TSVector eulerAngles)
		{
			Rotate(eulerAngles, Space.Self);
		}

		/**
        *  @brief Rotates game object based on provided axis and angle of rotation.
        **/
		public void Rotate(TSVector axis, FP angle)
		{
			Rotate(axis, angle, Space.Self);
		}

		/**
        *  @brief Rotates game object based on provided axis, angle of rotation and relative space.
        *  
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
		public void Rotate(TSVector axis, FP angle, Space relativeTo)
		{
			TSQuaternion result = TSQuaternion.identity;

			if (relativeTo == Space.Self)
			{
				result = this.rotation * TSQuaternion.AngleAxis(angle, axis);
			}
			else
			{
				result = TSQuaternion.AngleAxis(angle, axis) * this.rotation;
			}

			result.Normalize();
			this.rotation = result;
		}

		/**
        *  @brief Rotates game object based on provided axis angles and relative space.
        *  
        *  If relative space is SELF then the game object will rotate based on its forward vector.
        **/
		public void Rotate(TSVector eulerAngles, Space relativeTo)
		{
			TSQuaternion result = TSQuaternion.identity;

			if (relativeTo == Space.Self)
			{
				result = this.rotation * TSQuaternion.Euler(eulerAngles);
			}
			else
			{
				result = TSQuaternion.Euler(eulerAngles) * this.rotation;
			}

			result.Normalize();
			this.rotation = result;
		}

		/**
        *  @brief Current self forward vector.
        **/
		public TSVector forward
		{
			get
			{
				return TSVector.Transform(TSVector.forward, TSMatrix.CreateFromQuaternion(rotation));
			}
			set
			{
				this.rotation = TSQuaternion.LookRotation(value);
			}
		}

		/**
        *  @brief Current self right vector.
        **/
		public TSVector right
		{
			get
			{
				return TSVector.Transform(TSVector.right, TSMatrix.CreateFromQuaternion(rotation));
			}
		}

		/**
        *  @brief Current self up vector.
        **/
		public TSVector up
		{
			get
			{
				return TSVector.Transform(TSVector.up, TSMatrix.CreateFromQuaternion(rotation));
			}
		}

		/**
        *  @brief Returns Euler angles in degrees.
        **/
		public TSVector eulerAngles
		{
			get
			{
				return rotation.eulerAngles;
			}
			set
			{
				this.rotation = TSQuaternion.Euler(value);
			}
		}

		/**
         *  @brief Transform a point from local space to world space.
         **/
		public TSVector4 TransformPoint(TSVector4 point)
		{
			Debug.Assert(point.w == FP.One);
			return TSVector4.Transform(point, localToWorldMatrix);
		}

		public TSVector TransformPoint(TSVector point)
		{
			return TSVector4.Transform(point, localToWorldMatrix).ToTSVector3();
		}

		public void TransformPoint(ref TSVector point, out TSVector wPoint)
		{
			// return TSVector4.Transform(point, localToWorldMatrix).ToTSVector3();
			TSVector4.Transform(ref point, ref localToWorldMatrix, out wPoint);
		}

		/**
         *  @brief Transform a point from world space to local space.
         **/
		public TSVector4 InverseTransformPoint(TSVector4 point)
		{
			Debug.Assert(point.w == FP.One);
			return TSVector4.Transform(point, worldToLocalMatrix);
		}

		public TSVector InverseTransformPoint(TSVector point)
		{
			return TSVector4.Transform(point, worldToLocalMatrix).ToTSVector3();
		}

		public void InverseTransformPoint(ref TSVector point, out TSVector lPoint)
		{
			// return TSVector4.Transform(point, worldToLocalMatrix).ToTSVector3();
			TSVector4.Transform(ref point, ref worldToLocalMatrix, out lPoint);
		}

		/**
         *  @brief Transform a direction from local space to world space.
         **/
		public TSVector4 TransformDirection(TSVector4 direction)
		{
			Debug.Assert(direction.w == FP.Zero);
			TSMatrix4x4 matrix = TSMatrix4x4.Translate(position) * TSMatrix4x4.Rotate(rotation);
			return TSVector4.Transform(direction, matrix);
		}

		public TSVector TransformDirection(TSVector direction)
		{
			return TransformDirection(new TSVector4(direction.x, direction.y, direction.z, FP.Zero)).ToTSVector3();
		}

		/**
         *  @brief Transform a direction from world space to local space.
         **/
		public TSVector4 InverseTransformDirection(TSVector4 direction)
		{
			Debug.Assert(direction.w == FP.Zero);
			TSMatrix4x4 matrix = TSMatrix4x4.Translate(position) * TSMatrix4x4.Rotate(rotation);
			return TSVector4.Transform(direction, TSMatrix4x4.Inverse(matrix));
		}

		public TSVector InverseTransformDirection(TSVector direction)
		{
			return InverseTransformDirection(new TSVector4(direction.x, direction.y, direction.z, FP.Zero)).ToTSVector3();
		}

		/**
         *  @brief Transform a vector from local space to world space.
         **/
		public TSVector4 TransformVector(TSVector4 vector)
		{
			Debug.Assert(vector.w == FP.Zero);
			return TSVector4.Transform(vector, localToWorldMatrix);
		}

		public TSVector TransformVector(TSVector vector)
		{
			return TransformVector(new TSVector4(vector.x, vector.y, vector.z, FP.Zero)).ToTSVector3();
		}

		/**
         *  @brief Transform a vector from world space to local space.
         **/
		public TSVector4 InverseTransformVector(TSVector4 vector)
		{
			Debug.Assert(vector.w == FP.Zero);
			return TSVector4.Transform(vector, worldToLocalMatrix);
		}

		public TSVector InverseTransformVector(TSVector vector)
		{
			return InverseTransformVector(new TSVector4(vector.x, vector.y, vector.z, FP.Zero)).ToTSVector3();
		}

		[HideInInspector]
		public TSCollider tsCollider;

		private bool initialized = false;

		private TSRigidBody rb = null;
		public TSRigidBody tsRigidBody
		{
			get
			{
				if (rb == null)
				{
					if (this.tsCollider != null)
					{
						rb = this.tsCollider.attachedRigidbody;
						return rb;
					}
				}
				return rb;
			}
		}

		// TODO: 解除对ui事件的依赖
		public void Start()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			Initialize();
		}

		/**
        *  @brief Initializes internal properties based on whether there is a {@link TSCollider} attached.
        **/
		public void Initialize()
		{
			if (initialized)
			{
				return;
			}

			markMatrixDirty();

			rb = GetComponent<TSRigidBody>();
			tsTransformConfig = GetComponent<TSTransformConfig>();

			tsCollider = GetComponent<TSCollider>();
			if (transform.parent != null)
			{
				// tsParent = transform.parent.GetComponent<TSTransform>();
				tsParent = transform.parent.ReGetTransform();
			}

			foreach (Transform child in transform)
			{
				// TSTransform tsChild = child.GetComponent<TSTransform>();
				TSTransform tsChild = child.ReGetTransform();
				if (tsChild != null)
				{
					if (!tsChildren.Contains(tsChild))
					{
						tsChildren.Add(tsChild);
					}
				}

			}

			if (!_serialized)
			{
				UpdateEditMode();
			}

			if (tsCollider != null)
			{
				//if (tsCollider.IsBodyInitialized)
				tsCollider.CheckPhysicsForce();
				if (tsCollider._body != null)
				{
					tsCollider.Body.TSScale = _lossyScale;
					// tsCollider.Body.TSPosition = _position + scaledCenter;
					TSVector.Add(ref _position, ref scaledCenter, out __sharedPos);
					tsCollider.Body.SetTSPosition(ref __sharedPos);
					tsCollider.Body.TSOrientation = TSMatrix.CreateFromQuaternion(_rotation);
                }
			}
			else
			{
				StateTracker.AddTracking(this);
			}

			// var viewSync = this.GetOrAddComponent<TSTransformViewSync>();
			var viewSync = this.GetComponent<TSTransformViewSync>();
			if (viewSync != null)
			{
				viewSync.tsTransform = this;
				tsViewSync = viewSync;
			}

			initialized = true;
		}

		TSTransformViewSync tsViewSync = null;
		/// <summary>
		/// 如果 TSViewSync 存在, 那么将覆盖自带的视图更新方式
		/// </summary>
		TSTransformViewSync TSViewSync
		{
			get => this.tsViewSync;
			set => this.tsViewSync = value;
		}

	}

}