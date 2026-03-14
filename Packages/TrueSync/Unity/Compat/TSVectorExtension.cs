namespace TrueSync
{
	public static class TSVectorExtension
	{
		public const int DefaultAccurate = 1024;
		public static FP LimitAccurate(this FP value)
		{
			// return TSMath.Round((FP)value * 1024) / 1024;
			value._serializedValue >>= 20;
			value._serializedValue <<= 20;
			return value;
		}
		public static TSVector LimitAccurate(this UnityEngine.Vector3 vec, int accurate = DefaultAccurate)
		{
			TSVector result;
			result.x = LimitAccurate(vec.x);
			result.y = LimitAccurate(vec.y);
			result.z = LimitAccurate(vec.z);
			return result;
		}
		public static TSQuaternion LimitAccurate(this UnityEngine.Quaternion vec, int accurate = DefaultAccurate)
		{
			TSQuaternion result;
			result.x = LimitAccurate(vec.x);
			result.y = LimitAccurate(vec.y);
			result.z = LimitAccurate(vec.z);
			result.w = LimitAccurate(vec.w);
			return result;
		}
	}

}