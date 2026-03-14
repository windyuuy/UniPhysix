

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace TrueSync.Physics3D
{
	using PxReal = FP;
	using PxU32 = UInt32;
	using PxVec3 = TSVector;
	using PxMat33 = TSMatrix;

	public abstract partial class CollisionSystem
	{
		PxReal distancePointBoxSquared( /*const*/ref PxVec3 point,
									/*const*/ref PxVec3 boxOrigin, /*const*/ref PxVec3 boxExtent, /*const*/ref PxMat33 boxBase,
									out PxVec3 boxParam)
		{
			// Compute coordinates of point in box coordinate system
			PxVec3 diff = point - boxOrigin;

			PxVec3 closest = new PxVec3(boxBase.column0.Dot(diff),
							boxBase.column1.Dot(diff),
							boxBase.column2.Dot(diff));

			// Project test point onto box
			PxReal sqrDistance = 0.0f;
			for (PxU32 ax = 0; ax < 3; ax++)
			{
				if (closest[ax] < -boxExtent[ax])
				{
					PxReal delta = closest[ax] + boxExtent[ax];
					sqrDistance += delta * delta;
					closest[ax] = -boxExtent[ax];
				}
				else if (closest[ax] > boxExtent[ax])
				{
					PxReal delta = closest[ax] - boxExtent[ax];
					sqrDistance += delta * delta;
					closest[ax] = boxExtent[ax];
				}
			}

			boxParam = closest;

			return sqrDistance;
		}
	}
}