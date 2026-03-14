using TrueSync.Physics3D;

namespace TrueSync
{

	public abstract partial class TSCollider
	{
		internal void SetRigidBody(TSRigidBody attachedRigidbody)
		{
			if (this.tsRigidBody != attachedRigidbody)
			{
				this.tsRigidBody = attachedRigidbody;

				updateTSRigidBody();
			}
		}

		protected void updateTSRigidBody()
		{
			if (_body != null)
			{
				updateBody(_body);
			}

		}

		protected void updateBody(RigidBody newBody)
		{
			if (tsMaterial == null)
			{
				tsMaterial = GetComponent<TSMaterial>();
			}

			if (tsMaterial != null)
			{
				newBody.TSFriction = tsMaterial.friction;
				newBody.TSRestitution = tsMaterial.restitution;
			}

			newBody.IsColliderOnly = isTrigger;
			newBody.IsKinematic = tsRigidBody != null && tsRigidBody.isKinematic;

			bool isStatic = tsRigidBody == null || tsRigidBody.isKinematic;

			if (tsRigidBody != null)
			{
				newBody.AffectedByGravity = tsRigidBody.useGravity;

				if (tsRigidBody.mass <= 0)
				{
					tsRigidBody.mass = 1;
				}

				newBody.Mass = tsRigidBody.mass;
				newBody.TSLinearDrag = tsRigidBody.drag;
				newBody.TSAngularDrag = tsRigidBody.angularDrag;
			}
			else
			{
				newBody.SetMassProperties();
			}

			if (isStatic)
			{
				newBody.AffectedByGravity = false;
				newBody.IsStatic = true;
			}
		}
	}
}
