using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TrueSync.Physics3D;

#if UNITY_EDITOR
using Handles = UnityEditor.Handles;
#endif

namespace TrueSync
{

	/**
     *  @brief Collider with a capsule shape. 
     **/
	[AddComponentMenu("TrueSync/Physics/CapsuleCollider", 0)]
	public partial class TSCapsuleCollider : TSCollider
	{

		public CapsuleShape ShapeSpecific => Shape as CapsuleShape;
		protected void updateShape()
		{
			ShapeSpecific.Scale = this.tsTransform.lossyScale;
			ShapeSpecific.Length = _length;
			ShapeSpecific.Radius = _radius;
		}

		[FormerlySerializedAs("radius")]
        [SerializeField]
        private FP _radius;

        /**
         *  @brief Radius of the capsule. 
         **/
        public FP radius {
            get {
                if (_body != null) {
                    return ((CapsuleShape)_body.Shape).Radius;
                }

                return _radius;
            }
            set {
                _radius = value;

                if (_body != null) {
                    ((CapsuleShape)_body.Shape).Radius = _radius;
				}
            }
        }

        [FormerlySerializedAs("length")]
        [SerializeField]
		[Tooltip("长度, 不包含半球范围")]
        private FP _length;

		/**
         *  @brief Length of the capsule. 不包含半球范围.
         **/
		public FP length {
            get {
                if (_body != null) {
                    return ((CapsuleShape)_body.Shape).Length;
                }

                return _length;
            }
            set {
                _length = value;

                if (_body != null) {
                    ((CapsuleShape)_body.Shape).Length = _length;
				}
            }
		}

		/**
         *  @brief Create the internal shape used to represent a TSCapsuleCollider.
         **/
		public override Shape CreateShape() {
            return new CapsuleShape(length, radius);
        }

        protected override void DrawGizmos() {
#if UNITY_EDITOR
			updateShape();
			var color = Color.yellow;

			Vector3 pos = _body != null ? _body.Position.ToVector() : (uetransform.position + ScaledCenter.ToVector());
			Quaternion rot = _body != null ? _body.Orientation.ToQuaternion() : uetransform.rotation;

			// var pos = this.transform.position + this.Center.ToVector();
			// var rot = this.transform.rotation;

			var length = ShapeSpecific.ScaledLength.AsFloat();
			var radius = ShapeSpecific.ScaledRadius.AsFloat();
			var height = length + radius * 2;
			DrawWireCapsule(pos, rot, radius, height, color);
#else
             Gizmos.DrawWireSphere(Vector3.zero, 1);
             Gizmos.DrawWireSphere(new TSVector(0, (length / 2) / radius, 0).ToVector(), 1);
             Gizmos.DrawWireSphere(new TSVector(0, (-length / 2) / radius, 0).ToVector(), 1);
#endif
        }

#if UNITY_EDITOR
        public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color))
		{
			if (_color != default(Color))
				Handles.color = _color;
			Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
			using (new Handles.DrawingScope(angleMatrix))
			{
				var pointOffset = (_height - (_radius * 2)) / 2;

				//draw sideways
				Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
				Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
				Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
				Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
				//draw frontways
				Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
				Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
				Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
				Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
				//draw center
				Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
				Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);

			}
		}
#endif

        protected override Vector3 GetGizmosSize() {
			// unused
			return Vector3.one * radius.AsFloat();
        }

		// TODO: 需要完善
		/// <summary>
		/// brief Moves the body to a new position.
		/// </summary>
		/// <param name="position"></param>
		/// <returns>CollisionFlags</returns>
		public override CollisionFlags MoveTo(TSVector position)
		{
			CollisionFlags flags = CollisionFlags.None;

			// var curCollisionMap = new Dictionary<RigidBody, SimpleCollisionInfo>();

			// SimpleCollisionInfo oldCollision = null;
			// SimpleCollisionInfo newCollision = null;

			// 	TSPhysics.CheckRigidBody(this.tsCollider._body, handler: (RigidBody body1, RigidBody body2,
			// 	   TSVector point1, TSVector point2, TSVector normal, FP penetration) =>
			//    {
			// 	   oldCollision = new SimpleCollisionInfo()
			// 	   {
			// 		   body2 = body2,
			// 		   point1 = point1,
			// 		   normal = normal,
			// 		   penetration = penetration,
			// 	   };
			// 	   curCollisionMap[body2] = oldCollision;
			//    });

			this.attachedRigidbody.MovePosition(position);

			var collisions = new List<SimpleCollisionInfo>();
			TSPhysics.CheckRigidBody(this._body, handler: (RigidBody body1, RigidBody body2,
			   TSVector point1, TSVector point2, TSVector normal, FP penetration) =>
		   {
			   collisions.Add(new SimpleCollisionInfo()
			   {
				   body2 = body2,
				   point1 = point1,
				   normal = normal,
				   penetration = penetration,
			   });
		   });

			// 判断方位
			foreach (var collisioin in collisions)
			{
				var pt = collisioin.point1;

				var body = this._body;
				var shape = body.shape as CapsuleShape;
				var centerY = body.Position.y;
				var hlen = shape.ScaledLength * FP.myhalf;
				// TODO: 假定始终直立, 暂时和　CharactorController.Move 保持一致
				var pHighY = centerY + hlen;
				var pLowY = centerY - hlen;
				if (pt.y > pHighY)
				{
					flags |= CollisionFlags.Above;
				}
				else if (pt.y < pLowY)
				{
					flags |= CollisionFlags.Below;
				}
				else
				{
					flags |= CollisionFlags.Sides;
				}
			}

			return flags;
		}

	}
}