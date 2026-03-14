
using UnityEngine;

public static class VectorExtension
{
	public static string ToHString(this Vector3 vec)
	{
		return $"({vec.x},{vec.y},{vec.z})";
	}
}
