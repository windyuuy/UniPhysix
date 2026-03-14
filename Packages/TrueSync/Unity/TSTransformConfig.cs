
using UnityEngine;

namespace TrueSync
{
	[AddComponentMenu("TrueSync/ViewLerp/TSTransformConfig")]
	public class TSTransformConfig : MonoBehaviour
	{
		// TODO: 优化画面拟合参数
		public const float DELTA_TIME_FACTOR = 22;
		public static Vector3 _ConstLerpFactor = new Vector3(DELTA_TIME_FACTOR, DELTA_TIME_FACTOR, DELTA_TIME_FACTOR);

		[Header("自定义拟合系数")]
		[SerializeField]
		[AddTracking]
		public bool OverwriteLerpFactor = false;
		[Header("拟合系数")]
		[SerializeField]
		[AddTracking]
		public Vector3 LerpFactor = _ConstLerpFactor;

		public Vector3 GetLerpFactor()
		{
			Vector3 lerpFactor;
			if (OverwriteLerpFactor)
			{
				lerpFactor = LerpFactor;
			}
			else
			{
				lerpFactor = _ConstLerpFactor;
			}
			return lerpFactor;
		}
	}
}
