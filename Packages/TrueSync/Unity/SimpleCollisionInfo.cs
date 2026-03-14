using TrueSync.Physics3D;

namespace TrueSync
{
	internal class SimpleCollisionInfo
	{
		public RigidBody body2;
		public TSVector point1;
		public TSVector normal;
		public FP penetration;
	}
}