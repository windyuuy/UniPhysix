
namespace TrueSync
{
	public interface ICollider
	{
	}

	public interface ICollider3D
	{
		bool Raycast(ref TSRay ray, out TSRaycastHit hitInfo, FP maxDistance);
		UnityEngine.Transform uetransform { get; }

		TSTransform TSTransform { get; }

		[System.Security.SecuritySafeCritical]
		bool TryGetComponent<T>(out T component);
		// [UnityEngineInternal.TypeInferenceRule(UnityEngineInternal.TypeInferenceRules.TypeReferencedByFirstArgument)]
		// public bool TryGetComponent(System.Type type, out UnityEngine.Component component);
	}
}
