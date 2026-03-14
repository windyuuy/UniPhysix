using System;
using UnityEngine;
using UnityEngine.Serialization;
using TrueSync.Physics3D;

namespace TrueSync {

	//
	// 摘要:
	//     CollisionFlags is a bitmask returned by CharacterController.Move.
	public enum CollisionFlags
	{
		//
		// 摘要:
		//     CollisionFlags is a bitmask returned by CharacterController.Move.
		None = 0,
		//
		// 摘要:
		//     CollisionFlags is a bitmask returned by CharacterController.Move.
		Sides = 1,
		CollidedSides = 1,
		//
		// 摘要:
		//     CollisionFlags is a bitmask returned by CharacterController.Move.
		Above = 2,
		CollidedAbove = 2,
		//
		// 摘要:
		//     CollisionFlags is a bitmask returned by CharacterController.Move.
		Below = 4,
		CollidedBelow = 4
	}

	/**
     *  @brief Abstract collider for 3D shapes. 
     **/
	[RequireComponent(typeof(TSTransform))]
    [Serializable]
    [ExecuteInEditMode]
	public abstract partial class TSCollider : MonoBehaviour, ICollider, ICollider3D
	{
		public UnityEngine.Transform uetransform => base.transform;
		public new TSTransform transform => this.tsTransform;

		public void SetEnabled(bool b)
		{
			base.enabled = b;
			OnHandleEnabled(b);
		}

		protected virtual void OnHandleEnabled(bool b)
		{
			if (this._body != null)
			{
				this._body.IsActive = b;
			}
		}

		protected virtual void OnEnable()
		{
			OnHandleEnabled(true);
		}
		protected virtual void OnDisable()
		{
			OnHandleEnabled(false);
		}

		private Shape shape;

        /**
         *  @brief Shape used by a collider.
         **/
        public Shape Shape {
            get {
				if (shape == null)
				{
                    shape = CreateShape();
				}
                return shape;
            }
            protected set { shape = value; }
        }

		public virtual void SetScale(TSVector scale)
		{
			lossyScale = scale;
			this._body.SetScale(scale);
		}


		[FormerlySerializedAs("isTrigger")]
        [SerializeField]
        private bool _isTrigger;

        /**
         *  @brief If it is only a trigger and doesn't interfere on collisions. 
         **/
        public bool isTrigger {
            get {
                if (_body != null) {
                    return _body.IsColliderOnly;
                }

                return _isTrigger;
            }
            set {
                _isTrigger = value;

                if (_body != null) {
                    _body.IsColliderOnly = _isTrigger;
                }
            }
        }

        /**
         *  @brief Simulated material. 
         **/
        public TSMaterial tsMaterial;

		public TSMaterial TSMaterial
		{
			get
			{
				return tsMaterial;
			}
			set
			{
				tsMaterial = value;
				UpdateTSMaterial();
			}
		}

		[SerializeField]
        private TSVector center;

		private TSVector scaledCenter;

        internal RigidBody _body;

        /**
         *  @brief Center of the collider shape.
         **/
        public TSVector Center {
            get {
                return center;
            }
            set {
                center = value;

				TSVector.Scale(ref center, ref lossyScale, out scaledCenter);
            }
        }

		// TODO: 需要验证 ScaledCenter
		/**
         *  @brief Returns a version of collider's center scaled by parent's transform.
         */
		public ref TSVector ScaledCenter
		{
			get {
				// return TSVector.Scale (Center, lossyScale);
				return ref scaledCenter;
			}
		}

		public void GetScaledCenter(out TSVector center)
		{
			center = scaledCenter;
		}

		/**
         *  @brief Creates the shape related to a concrete implementation of TSCollider.
         **/
		public abstract Shape CreateShape();

        private TSRigidBody tsRigidBody;

        /**
         *  @brief Returns the {@link TSRigidBody} attached.
         */
        public TSRigidBody attachedRigidbody {
            get {
                return tsRigidBody;
            }
        }

        /**
         *  @brief Returns body's boundind box.
         */
        public TSBBox bounds {
            get {
                return this._body.BoundingBox;
            }
        }

        /**
         *  @brief Returns the body linked to this collider.
         */
        public IBody3D Body {
            get {
                if (_body == null) {
                    CheckPhysics();
                }

                return _body;
            }
        }

		public void CheckPhysicsForce()
		{
			CheckPhysics();
		}

		/**
         *  @brief Holds an first value of the GameObject's lossy scale.
         **/
		[SerializeField]
        [HideInInspector]
        protected TSVector lossyScale = TSVector.one;

        [HideInInspector]
		protected TSTransform tsTransform;
		public TSTransform TSTransform => tsTransform ?? (tsTransform = this.ReGetTransform());

        /**
         *  @brief Creates a new {@link TSRigidBody} when there is no one attached to this GameObject.
         **/
        public void Awake() {
			//tsTransform = this.GetComponent<TSTransform>();
			tsTransform = this.ReGetTransform();
            tsRigidBody = this.GetComponent<TSRigidBody>();

            if (lossyScale == TSVector.one) {
				// lossyScale = TSVector.Abs(transform.localScale.ToTSVector());
				lossyScale = TSVector.Abs(tsTransform.lossyScale);
			}
        }

        public void Update() {
            if (!Application.isPlaying) {
				lossyScale = TSVector.Abs(uetransform.lossyScale.ToTSVector());
            }
        }

		public void UpdateTSMaterial()
		{
			if (tsMaterial == null)
			{
				tsMaterial = GetComponent<TSMaterial>();
			}

			if (_body != null && tsMaterial != null)
			{

				_body.TSFriction = tsMaterial.friction;
				_body.TSRestitution = tsMaterial.restitution;
			}

		}

		private void CreateBody() {
			RigidBody newBody = new RigidBody(Shape);
			updateBody(newBody);

			// if (tsMaterial == null) {
			//     tsMaterial = GetComponent<TSMaterial>();
			// }

			// if (tsMaterial != null) {
			//     newBody.TSFriction = tsMaterial.friction;
			//     newBody.TSRestitution = tsMaterial.restitution;
			// }

			// newBody.IsColliderOnly = isTrigger;
			// newBody.IsKinematic = tsRigidBody != null && tsRigidBody.isKinematic;

			// bool isStatic = tsRigidBody == null || tsRigidBody.isKinematic;

			// if (tsRigidBody != null) {
			//     newBody.AffectedByGravity = tsRigidBody.useGravity;

			//     if (tsRigidBody.mass <= 0) {
			//         tsRigidBody.mass = 1;
			//     }

			//     newBody.Mass = tsRigidBody.mass;
			//     newBody.TSLinearDrag = tsRigidBody.drag;
			//     newBody.TSAngularDrag = tsRigidBody.angularDrag;
			// } else {
			//     newBody.SetMassProperties();
			// }

			// if (isStatic) {
			//     newBody.AffectedByGravity = false;
			//     newBody.IsStatic = true;
			// }

			_body = newBody;
        }

        /**
         *  @brief Initializes Shape and RigidBody and sets initial values to position and orientation based on Unity's transform.
         **/
        public void Initialize() {
            CreateBody();
        }

        private void CheckPhysics() {
            if (_body == null && PhysicsManager.instance != null) {
                PhysicsManager.instance.AddBody(this);
            }
        }

        /**
         *  @brief Do a base matrix transformation to draw correctly all collider gizmos.
         **/
        public virtual void OnDrawGizmos() {
            if (!this.enabled) {
                return;
            }

			Vector3 position = _body != null ? _body.Position.ToVector() : (uetransform.position + ScaledCenter.ToVector());
			Quaternion rotation = _body != null ? _body.Orientation.ToQuaternion() : uetransform.rotation;

            Gizmos.color = Color.yellow;

			Matrix4x4 cubeTransform;
			try
            {
				cubeTransform = Matrix4x4.TRS(position, rotation.normalized, GetGizmosSize());
			}catch(Exception e)
            {
				Debug.LogError(e);
				cubeTransform = Matrix4x4.TRS(position, Quaternion.identity, GetGizmosSize());
            }
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

            Gizmos.matrix *= cubeTransform;

            DrawGizmos();

            Gizmos.matrix = oldGizmosMatrix;
        }

        /**
         *  @brief Returns the gizmos size.
         **/
        protected abstract Vector3 GetGizmosSize();

        /**
         *  @brief Draws the specific gizmos of concrete collider (for example "Gizmos.DrawWireCube" for a {@link TSBoxCollider}).
         **/
        protected abstract void DrawGizmos();

        /**
         *  @brief Returns true if the body was already initialized.
         **/
        public bool IsBodyInitialized {
            get {
                return _body != null;
            }
        }

		public bool Raycast(ref TSRay ray, out TSRaycastHit hitInfo, FP maxDistance)
		{
			var ret = TSPhysics.Raycast(this._body, ref ray, out hitInfo, maxDistance);
			return ret;
		}

		/// <summary>
		/// 寻找当前碰撞体上距离目标点最近的点
		/// - 基于局部坐标
		/// </summary>
		/// <param name="localPos">局部坐标系下坐标</param>
		/// <param name="nearPos">局部坐标系下最近点坐标</param>
		protected void getClosestPointLocal(ref TSVector localPos, out TSVector nearPos)
		{
			// 求局部向量
			var localOffset = localPos - ScaledCenter;
			var localForward = localOffset.normalized;
			// 求直球方向最近点
			this.shape.SupportMapping(ref localForward, out var localNearPos1);

			// 修正路径
			{
				var localForward2 = localPos - localNearPos1;
				localForward2.Normalize();
				// localForward2 = (localForward2 + localForward) * FP.myhalf;
				TSVector.Average(ref localForward2, ref localForward, out localForward2);
				this.shape.SupportMapping(ref localForward2, out var localNearPos2);
				if (localPos.Distance(localNearPos2) < localPos.Distance(localNearPos1))
				{
					localNearPos1 = localForward2;
				}
			}

			nearPos = localNearPos1;

		}

		// TODO: 需要优化精度
		/// <summary>
		/// 寻找当前碰撞体上距离目标点最近的点
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public TSVector ClosestPoint(TSVector position)
		{
			// TODO: 暂时不支持软体检测

			var orientation = this._body.orientation;
			var bodyPos = this._body.position;

			// 去几何中心局部坐标
			shape.SupportCenter(out var center);

			// 转换为局部坐标, 方便后续优化算法
			TSVector.ReverseTransform(ref position, ref bodyPos, ref orientation, out var localPos);

			TSVector localNearPos;
			if (shape is Multishape)
			{
				getClosestPointLocal(ref localPos, out localNearPos);

				Multishape ms1 = (shape as Multishape);
				ms1 = ms1.RequestWorkingClone();
				int ms1Length = ms1.Prepare(ref shape.boundingBox);
				for (int i = 0; i < ms1Length; i++)
				{
					ms1.SetCurrentShape(i);
					getClosestPointLocal(ref localPos, out var localNearPos1);
					if (localPos.Distance(localNearPos1) < localPos.Distance(localNearPos))
					{
						localNearPos = localNearPos1;
					}
				}
				ms1.ReturnWorkingClone();
			}
			else
			{
				getClosestPointLocal(ref localPos, out localNearPos);
			}

			// 将最近点转换为世界坐标
			TSVector.Transform(ref localNearPos, ref bodyPos, ref orientation, out var nearPos);
			return nearPos;
		}

		/// <summary>
		/// 按距离位移
		/// </summary>
		/// <param name="deltaMove"></param>
		/// <returns></returns>
		public virtual CollisionFlags Move(TSVector deltaMove)
		{
			return MoveTo(this.attachedRigidbody.position + deltaMove);
		}

		// TODO: 需要完善
		/// <summary>
		/// brief Moves the body to a new position.
		/// </summary>
		/// <param name="position"></param>
		/// <returns>CollisionFlags</returns>
		public virtual CollisionFlags MoveTo(TSVector position)
		{
#if UNITY_EDITOR
			throw new System.Exception("unsupport collider shape");
#else
            return CollisionFlags.None;
#endif
		}

		public virtual void MoveDelta(TSVector deltaMove)
		{
			this._body.Move(deltaMove);
		}

		internal virtual void RecordRigidBodyTransform()
		{
			this._body.RecordRigidBodyTransform();
		}

	}

}