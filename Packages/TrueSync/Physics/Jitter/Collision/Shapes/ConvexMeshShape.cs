
//windy

#region Using Statements
using System;
using System.Collections.Generic;
#endregion
using System.Linq;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace TrueSync.Physics3D
{

	/// <summary>
	/// ConvexMeshShape class.
	/// </summary>
	public class ConvexMeshShape : Shape
	{
		List<TSVector> vertices = null;

		TSVector shifted;

		/// <summary>
		/// Constructor of ConvexMeshShape class.
		/// </summary>
		/// <param name="vertices">A list containing all vertices defining
		/// the convex hull.</param>
		public ConvexMeshShape(List<TSVector> vertices)
		{
			this.vertices = vertices;
			UpdateShape();
		}

		public TSVector Shift { get { return -1 * this.shifted; } }

		public override void CalculateMassInertia()
		{
			this.mass = Shape.CalculateMassInertia(this, out shifted, out inertia);
		}

		/// <summary>
		/// SupportMapping. Finds the point in the shape furthest away from the given direction.
		/// Imagine a plane with a normal in the search direction. Now move the plane along the normal
		/// until the plane does not intersect the shape. The last intersection point is the result.
		/// </summary>
		/// <param name="direction">The direction.</param>
		/// <param name="result">The result.</param>
		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			FP maxDotProduct = FP.MinValue;
			int maxIndex = 0;
			FP dotProduct;

			for (int i = 0; i < vertices.Count; i++)
			{
				dotProduct = TSVector.Dot(vertices[i], direction);
				if (dotProduct > maxDotProduct)
				{
					maxDotProduct = dotProduct;
					maxIndex = i;
				}
			}

			result = vertices[maxIndex] - this.shifted;
		}
	}
}

