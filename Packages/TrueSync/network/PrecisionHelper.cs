
using UnityEngine;

namespace fsync
{
	public class PrecisionHelper
	{
		public static int precision = 10000000;
		public static Vector3 Round(Vector3 vec, ref Vector3 outVec)
		{
			outVec.x = Mathf.Round(vec.x * precision) / precision;
			outVec.y = Mathf.Round(vec.y * precision) / precision;
			outVec.z = Mathf.Round(vec.z * precision) / precision;
			return outVec;
		}
		public static Quaternion Round(Quaternion vec, ref Quaternion outVec)
		{
			outVec.x = Mathf.Round(vec.x * precision) / precision;
			outVec.y = Mathf.Round(vec.y * precision) / precision;
			outVec.z = Mathf.Round(vec.z * precision) / precision;
			outVec.w = Mathf.Round(vec.w * precision) / precision;
			return outVec;
		}

		protected static Vector3 sharedVec = new Vector3();
		protected static Quaternion sharedQuat = new Quaternion();
		public static void RoundGameObjectPrecision(GameObject target)
		{
			var transform = target.transform;
			transform.position = Round(transform.position, ref sharedVec);
		}
		public static void RoundGameObjectPrecision(MonoBehaviour target)
		{
            var transform = target.transform;
			//Round(transform.position, ref sharedVec);
			//transform.position = sharedVec;

			var vec = transform.position;
			var outVec = new Vector3();
			outVec.x = Mathf.Round(vec.x * precision) / precision;
			outVec.y = vec.y;
			outVec.z = Mathf.Round(vec.z * precision) / precision;

			if(outVec.x!=vec.x || outVec.z != vec.z)
            {
                Debug.Log($"wefwef: ({transform.position.x}, {transform.position.z}), ({outVec.x},{outVec.z})");
            }

			transform.position = outVec;
		}
	}
}