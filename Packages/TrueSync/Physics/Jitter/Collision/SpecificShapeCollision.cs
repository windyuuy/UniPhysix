/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace TrueSync.Physics3D
{
	public static class SpecificShapeCollision
	{
		public static bool Raycast(RigidBody body, ref TSVector rayOrigin, ref TSVector rayDirection, out FP fraction, out TSVector normal)
		{
			if (body.Shape is BoxShape)
			{
				return SpecificShapeCollision.RaycastBox(body.Shape as BoxShape, ref body.orientation, ref body.invOrientation, ref body.position,
				ref rayOrigin, ref rayDirection, out fraction, out normal);
			}

			return (GJKCollide.Raycast(body.Shape, ref body.orientation, ref body.invOrientation, ref body.position,
				ref rayOrigin, ref rayDirection, out fraction, out normal));

		}

		public static TSVector lrpos;
		public static TSVector lrend;
		public static bool RaycastBox(BoxShape shape, ref TSMatrix orientation, ref TSMatrix invOrientation,
			ref TSVector position, ref TSVector rayOrigin, ref TSVector rayDirection, out FP fraction, out TSVector normal)
		{
			if (orientation == TSMatrix.Identity && rayDirection.x == 0 && rayDirection.z == 0)
			{
				// 针对方块未旋转, 上下方向的射线执行特殊检测

				// var ret11 = GJKCollide.Raycast(shape, ref orientation, ref invOrientation, ref position,
				//    ref rayOrigin, ref rayDirection, out fraction, out normal);

				ref var hsize = ref shape.GetCachedScaledHalfSize();
				TSVector.Subtract(ref rayOrigin, ref position, out lrpos);
				// TSVector.Add(ref lrpos, ref rayDirection, out lrend);
				var ray_y = rayDirection.y;

				// if (
				// 	// 排除两点都在外部同侧
				// 	(lrpos.y > hsize.y && lrend_y > hsize.y) ||
				// 	(lrpos.y < -hsize.y && lrend_y < -hsize.y) ||
				// 	// 排除投射点在外部
				// 	(lrpos.x > hsize.x) || (lrpos.x < -hsize.x) ||
				// 	(lrpos.z > hsize.z) || (lrpos.z < -hsize.z) ||
				// 	// 排除起点在内部的情况, 在内部也视为未碰撞
				// 	(-hsize.y < lrpos.y && lrpos.y < hsize.y)
				// 	)
				var hsize_y = hsize.y;
				var lrpos_y = lrpos.y;
				var lrend_y = lrpos_y + ray_y;
				var neg_hsize_y = -hsize_y;
				if (
					// 外面已经判断投射点是否在外部, 可忽略
					(hsize.x >= lrpos.x && lrpos.x >= -hsize.x) && (hsize.z >= lrpos.z && lrpos.z >= -hsize.z) &&
					(
						(ray_y <= 0 && lrpos_y >= hsize_y && lrend_y <= hsize_y) ||
						(ray_y > 0 && lrpos_y <= neg_hsize_y && lrend_y >= neg_hsize_y)
					)
				)
				{
					normal = TSVector.up;
					if (ray_y <= 0)
					{
						normal.y = 1;
						var dirLen = -ray_y;
						fraction = (lrpos_y - hsize_y) / dirLen;
					}
					else
					{
						normal.y = -1;
						var dirLen = ray_y;
						fraction = -(lrpos_y + hsize_y) / dirLen;
					}
					return true;
				}
				else
				{
					fraction = FP.Zero;
					normal = TSVector.zero;
					return false;
				}
			}
			else
			{
				return (GJKCollide.Raycast(shape, ref orientation, ref invOrientation, ref position,
				   ref rayOrigin, ref rayDirection, out fraction, out normal));
			}
		}

	}
}