using System;

namespace TrueSync
{

    /**
    *  @brief Represents few information about a raycast hit. 
    **/
    public struct TSRaycastHit
	{
		public TSRigidBody rigidbody;
		public TSCollider collider;
		public TSTransform transform;
		public TSVector point;
		public TSVector normal;
		public FP distance;

		public static readonly TSRaycastHit none =new TSRaycastHit();

		public TSRaycastHit(TSRigidBody rigidbody, TSCollider collider, TSTransform transform, TSVector normal, TSVector origin, TSVector direction, FP fraction):this(rigidbody, collider, transform, ref normal, ref origin, ref direction, fraction)
		{
		}

		public TSRaycastHit(TSRigidBody rigidbody, TSCollider collider, TSTransform transform, ref TSVector normal, ref TSVector origin, ref TSVector direction, FP fraction)
		{
			this.rigidbody = rigidbody;
			this.collider = collider;
			this.transform = transform;
			this.normal = normal;
			this.point = origin + direction * fraction;
			this.distance = fraction * direction.magnitude;
		}

		public void SetHitPoint(ref TSVector origin, ref TSVector direction, FP fraction)
        {
			//this.point = origin + direction * fraction;
			TSVector pt;
			TSVector.Multiply(ref direction, fraction, out pt);
			TSVector.Add(ref pt, ref origin,out pt);
			this.point = pt;
		}

		public static void Init(out TSRaycastHit hit, TSRigidBody rigidbody, TSCollider collider, TSTransform transform, ref TSVector normal, ref TSVector origin, ref TSVector direction, FP fraction)
		{
			hit.rigidbody = rigidbody;
			hit.collider = collider;
			hit.transform = transform;
			hit.normal = normal;
			// hit.point = origin + direction * fraction;
			TSVector.Multiply(ref direction, fraction, out hit.point);
			TSVector.Add(ref hit.point, ref origin, out hit.point);

			hit.distance = fraction * direction.magnitude;
		}

		public static void Reset(out TSRaycastHit hit)
        {
			hit = none;
		}

		public void PartlySetRaycastHit(ref UnityEngine.RaycastHit hit)
		{
			hit.normal = this.normal.ToUEVector();
			hit.point = this.point.ToUEVector();
			hit.distance = this.distance.AsFloat();
		}

		public static UnityEngine.RaycastHit ueNone = new UnityEngine.RaycastHit();
		public void ToRaycastHit(out UnityEngine.RaycastHit hit)
		{
			hit = ueNone;
			hit.normal = this.normal.ToUEVector();
			hit.point = this.point.ToUEVector();
			hit.distance = this.distance.AsFloat();
		}

		public UnityEngine.RaycastHit ToRaycastHit()
		{
			var hit = new UnityEngine.RaycastHit();
			PartlySetRaycastHit(ref hit);
			return hit;
		}
	}
}

