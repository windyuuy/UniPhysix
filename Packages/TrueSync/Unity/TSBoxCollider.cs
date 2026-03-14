
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TrueSync.Physics3D;

namespace TrueSync {
    /**
     *  @brief Collider with a box shape. 
     **/
    [AddComponentMenu("TrueSync/Physics/BoxCollider", 0)]
    public class TSBoxCollider : TSCollider {

		public BoxShape ShapeSpecific => Shape as BoxShape;
		protected void updateShape()
		{
            if(ShapeSpecific.Scale!= this.tsTransform.lossyScale)
            {
                ShapeSpecific.Scale = this.tsTransform.lossyScale;
            }
            if (ShapeSpecific.Size != size)
            {
                ShapeSpecific.Size = size;
            }
		}

		[FormerlySerializedAs("size")]
        [SerializeField]
        private Vector3 _size = Vector3.one;

        /**
         *  @brief Size of the box. 
         **/
        public TSVector size {
            get {
                if (_body != null) {
                    TSVector boxSize = ((BoxShape)_body.Shape).Size;
					// boxSize.x /= lossyScale.x;
					// boxSize.y /= lossyScale.y;
					// boxSize.z /= lossyScale.z;

					return boxSize;
                }

                return _size.ToTSVector();
            }

            set {
                _size = value.ToVector();

                if (_body != null) {
					// ((BoxShape)_body.Shape).Size = TSVector.Scale(value, lossyScale);
					((BoxShape)_body.Shape).Size = value;
                }
            }
        }


        /**
         *  @brief Sets initial values to {@link #size} based on a pre-existing BoxCollider or BoxCollider2D.
         **/
        public void Reset() {
            if (GetComponent<BoxCollider2D>() != null) {
                BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();

                size = new TSVector(boxCollider2D.size.x, boxCollider2D.size.y, 1);
                Center = new TSVector(boxCollider2D.offset.x, boxCollider2D.offset.y, 0);
                isTrigger = boxCollider2D.isTrigger;
            } else if (GetComponent<BoxCollider>() != null) {
                BoxCollider boxCollider = GetComponent<BoxCollider>();

                size = new TSVector(boxCollider.size.x, boxCollider.size.y, 1);
                Center = boxCollider.center.ToTSVector();
                isTrigger = boxCollider.isTrigger;
            }
        }

        /**
         *  @brief Create the internal shape used to represent a TSBoxCollider.
         **/
        public override Shape CreateShape() {
			return new BoxShape(size);
        }

        protected override void DrawGizmos() {
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        protected override Vector3 GetGizmosSize() {
			updateShape();
			var size = this.ShapeSpecific.ScaledSize;
			// return TSVector.Scale(size, lossyScale).ToVector();
			return size.ToVector();
		}

		// TODO: 需要完善
		/// <summary>
		/// brief Moves the body to a new position.
		/// </summary>
		/// <param name="position"></param>
		/// <returns>CollisionFlags</returns>
		public override CollisionFlags MoveTo(TSVector position)
		{
			// TODO: 需要实现 TSBoxCollider.Move
			CollisionFlags flags = CollisionFlags.None;

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
				var shape = body.shape as BoxShape;
				var centerY = body.Position.y;
				var shapeSize = shape.ScaledSize;
				var hlen = (shapeSize.y - (shapeSize.x + shapeSize.z) * FP.myhalf) * FP.myhalf;
				// TODO: 假定始终直立, 暂时和　CharactorController.Move 保持一致
				var pHighY = centerY + hlen;
				var pLowY = centerY - hlen;
				if (pt.y >= pHighY)
				{
					flags |= CollisionFlags.Above;
				}
				else if (pt.y <= pLowY)
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