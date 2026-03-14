using UnityEngine;

namespace TrueSync
{
	public class MyUECollider : ICollider3D
	{
		Collider _collider;

		public MyUECollider(Collider collider)
		{
			this._collider = collider;
		}

		public Transform uetransform => _collider.transform;

		// public TSTransform TSTransform => _collider.GetComponent<TSTransform>();
		public TSTransform TSTransform => _collider.ReferTransform();

		public bool Raycast(ref TSRay ray, out TSRaycastHit hitInfo, FP maxDistance)
		{
			return this.Raycast(ref ray, out hitInfo, maxDistance);
		}

		public bool TryGetComponent<T>(out T component)
		{
			return _collider.TryGetComponent<T>(out component);
		}

	}

}