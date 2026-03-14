
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
	public class CapsuleMeshDetect
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="support1"></param>
		/// <param name="support2"></param>
		/// <param name="orientation1"></param>
		/// <param name="orientation2"></param>
		/// <param name="position1"></param>
		/// <param name="position2"></param>
		/// <param name="point"></param>
		/// <param name="normal"></param>
		/// <param name="penetration"></param>
		/// <returns></returns>
		// public static bool DetectCapsuleMesh(ISupportMappable meshshape, ISupportMappable capsule1, ref TSMatrix orientation1,
		// 	 ref TSMatrix orientation2, ref TSVector position1, ref TSVector position2,
		// 	 out TSVector point, out TSVector normal, out FP penetration)
		// {
		// 	var triangle = meshshape as TriangleMeshShape;
		// 	var capsule = capsule1 as CapsuleShape;
		// 	var verts = triangle.GetTrianglePoints();
		// 	var v0 = verts[0];
		// 	var v1 = verts[1];
		// 	var e1 = v0 - v1;
		// 	var e2 = verts[2] - v1;
		// 	normal = TSVector.Cross(e1, e2);
		// 	capsule.SupportMapping(ref normal, out var near);

		// 	var sd = -normal;
		// 	capsule.SupportMapping(ref sd, out var near2);

		// 	var distance = (near - v0) * normal;
		// 	// 投影点
		// 	var shadow = v0 - distance * normal;
		// 	var isInCapsule = capsule.BoundingBox.IsContains(shadow);

		// }

		// public static bool DetectSphereMesh(ISupportMappable meshshape, ISupportMappable capsule1, ref TSMatrix orientation1,
		// 	 ref TSMatrix orientation2, ref TSVector position1, ref TSVector position2,
		// 	 out TSVector point, out TSVector normal, out FP penetration)
		// {
		// 	var triangle = meshshape as TriangleMeshShape;
		// 	var sphere = capsule1 as SphereShape;

		// 	var verts = triangle.GetTrianglePoints();
		// 	var v0 = verts[0];
		// 	var v1 = verts[1];
		// 	var e1 = v0 - v1;
		// 	var e2 = verts[2] - v1;
		// 	normal = TSVector.Cross(e1, e2);
		// 	capsule.SupportMapping(ref normal, out var near);
		// }
	}

	public abstract partial class CollisionSystem
	{

		private bool DetectCapsuleMesh(RigidBody b2, RigidBody b1, QueryTriggerInteraction queryTriggerInteraction, bool checkOnly = false)
		{
			var ms = b1.Shape as Multishape;
			bool isCollision = false;
			ms = ms.RequestWorkingClone();

			TSBBox transformedBoundingBox = b2.boundingBox;
			transformedBoundingBox.InverseTransform(ref b1.position, ref b1.orientation);

			int msLength = ms.Prepare(ref transformedBoundingBox);

			if (msLength == 0)
			{
				ms.ReturnWorkingClone();
				return false;
			}

			TSVector point, normal;
			FP penetration;

			for (int i = 0; i < msLength; i++)
			{
				ms.SetCurrentShape(i);

				var xx = XenoCollide.Detect(ms, b2.Shape, ref b1.orientation,
					ref b2.orientation, ref b1.position, ref b2.position,
					out point, out normal, out penetration, checkOnly);
				if (xx)
				{
					TSVector point1, point2;
					FindSupportPoints(b1, b2, ms, b2.Shape, ref point, ref normal, out point1, out point2);

					if (useTerrainNormal && ms is TerrainShape)
					{
						(ms as TerrainShape).CollisionNormal(out normal);
						TSVector.Transform(ref normal, ref b1.orientation, out normal);
					}
					else if (useTriangleMeshNormal && ms is TriangleMeshShape)
					{
						(ms as TriangleMeshShape).CollisionNormal(out normal);
						TSVector.Transform(ref normal, ref b1.orientation, out normal);
					}

					isCollision = true;
					RaiseCollisionDetected(b1, b2, ref point1, ref point2, ref normal, penetration, queryTriggerInteraction);
				}
			}

			ms.ReturnWorkingClone();

			return isCollision;
		}

		private bool DetectSphereMesh(RigidBody b2, RigidBody b1, QueryTriggerInteraction queryTriggerInteraction, bool checkOnly = false)
		{
			var ms = b1.Shape as Multishape;
			bool isCollision = false;
			ms = ms.RequestWorkingClone();

			TSBBox transformedBoundingBox = b2.boundingBox;
			transformedBoundingBox.InverseTransform(ref b1.position, ref b1.orientation);

			int msLength = ms.Prepare(ref transformedBoundingBox);

			if (msLength == 0)
			{
				ms.ReturnWorkingClone();
				return false;
			}

			TSVector point, normal;
			FP penetration;

			for (int i = 0; i < msLength; i++)
			{
				ms.SetCurrentShape(i);

				var xx = XenoCollide.Detect(ms, b2.Shape, ref b1.orientation,
					ref b2.orientation, ref b1.position, ref b2.position,
					out point, out normal, out penetration, checkOnly);
				if (xx)
				{
					TSVector point1, point2;
					FindSupportPoints(b1, b2, ms, b2.Shape, ref point, ref normal, out point1, out point2);

					if (useTerrainNormal && ms is TerrainShape)
					{
						(ms as TerrainShape).CollisionNormal(out normal);
						TSVector.Transform(ref normal, ref b1.orientation, out normal);
					}
					else if (useTriangleMeshNormal && ms is TriangleMeshShape)
					{
						(ms as TriangleMeshShape).CollisionNormal(out normal);
						TSVector.Transform(ref normal, ref b1.orientation, out normal);
					}

					isCollision = true;
					RaiseCollisionDetected(b1, b2, ref point1, ref point2, ref normal, penetration, queryTriggerInteraction);
				}
			}

			ms.ReturnWorkingClone();

			return isCollision;
		}

		private bool BoxContain(ref TSVector hsize,ref TSVector vert)
        {
			if(
				hsize.x>vert.x && vert.x>-hsize.x &&
				hsize.z>vert.z && vert.z > -hsize.z
                )
            {
				return true;
            }
			return false;
        }
		private bool CompareBoxVerts(
			ref TSVector wCenter0, ref TSMatrix rot0, ref TSVector hsize0, ref TSVector lastPos0,
			ref TSVector wCenter1, ref TSMatrix rot1, ref TSVector hsize1, ref TSVector lastPos1,
			ref TSMatrix rot1x0T,
			out TSVector lcenter1,
			out TSVector vert1_1, out TSVector vert1_2, out TSVector vert1_3, out TSVector vert1_4,
			out TSVector localNormal0, out TSVector point0, out bool isSide, out FP penetration, out bool isPort
		)
		{
			lcenter1 = wCenter1;

			TSVector.Subtract(ref lcenter1, ref wCenter0, out lcenter1);
			TSVector.TransposedTransform(ref lcenter1, ref rot0, out lcenter1);

			var diag1_1 = new TSVector(hsize1.x, 0, hsize1.z);
			var diag1_2 = new TSVector(hsize1.x, 0, -hsize1.z);
			TSVector.Transform(ref diag1_1, ref rot1x0T, out diag1_1);
			TSVector.Transform(ref diag1_2, ref rot1x0T, out diag1_2);

			// TSVector.Transform(ref axis1_1, ref rot1, out axis1_1);
			// TSVector.TransposedTransform(ref axis1_1, ref rot0, out axis1_1);
			// TSVector.Transform(ref axis1_2, ref rot1, out axis1_2);
			// TSVector.TransposedTransform(ref axis1_2, ref rot0, out axis1_2);

			// TSVector axis3;
			// TSVector.Cross(ref axis1, ref axis2, out axis3);

			TSVector.Add(ref lcenter1, ref diag1_1, out vert1_1);
			TSVector.Add(ref lcenter1, ref diag1_2, out vert1_2);
			TSVector.Subtract(ref lcenter1, ref diag1_1, out vert1_3);
			TSVector.Subtract(ref lcenter1, ref diag1_2, out vert1_4);

			var hx = hsize0.x;
			var hz = hsize0.z;
			var nhx = -hx;
			var nhz = -hz;
			isPort = false;

			if (
				(nhx > vert1_1.x && nhx > vert1_2.x && nhx > vert1_3.x && nhx > vert1_4.x) ||
				(hx < vert1_1.x && hx < vert1_2.x && hx < vert1_3.x && hx < vert1_4.x) ||
				(nhz > vert1_1.z && nhz > vert1_2.z && nhz > vert1_3.z && nhz > vert1_4.z) ||
				(hz < vert1_1.z && hz < vert1_2.z && hz < vert1_3.z && hz < vert1_4.z)
				)
			{
				localNormal0 = lcenter1;
				point0 = lcenter1;
				isSide = false;
				penetration = 0;
				return false;
			}
			else
			{
				TSVector lLastPos1;
				TSVector.Subtract(ref lastPos1, ref wCenter0, out lLastPos1);
				TSVector.TransposedTransform(ref lLastPos1, ref rot0, out lLastPos1);
				// 无需考虑相对运动, 相对center0
				TSVector lMoveOffset;
				TSVector.Subtract(ref lLastPos1, ref lcenter1, out lMoveOffset);
				var len = lMoveOffset.magnitude;
				if (len > hsize0.x || len > hsize0.z || len>TSMath.Max(hsize1.x,hsize1.z))
				{
					FP penetrationX, penetrationZ;
                    // 越过中心
                    if (lLastPos1.z * lcenter1.z <= 0)
                    {
                        penetrationZ = FP.Abs(lcenter1.z) + hsize0.z;
                    }
                    else
                    {
						penetrationZ = hsize0.z - FP.Sign(lMoveOffset.z)*lcenter1.z;
						if (penetrationZ > FP.Abs(lMoveOffset.z))
						{
							penetrationZ = lMoveOffset.z;
						}
					}
					var penZ = TSMath.Max(diag1_1.z, diag1_2.z);
					penetrationZ += FP.Abs(penZ);

					// 越过中心
					if (lLastPos1.x * lcenter1.x <= 0)
                    {
                        penetrationX = FP.Abs(lcenter1.x) + hsize0.x;
                    }
                    else
                    {
						penetrationX = hsize0.x - FP.Sign(lMoveOffset.x)* lcenter1.x;
						if (penetrationX > FP.Abs(lMoveOffset.x))
						{
							penetrationX = lMoveOffset.x;
						}
					}
					var penX = TSMath.Max(diag1_1.x, diag1_2.x);
					penetrationX += FP.Abs(penX);
                    if (penetrationX < 0)
                    {
						penetrationX = 0;
					}
                    if (penetrationZ < 0)
                    {
						penetrationZ = 0;
					}

					//penetration = TSMath.Sqrt(penetrationX* penetrationX+ penetrationZ* penetrationZ);
					localNormal0 = new TSVector(FP.Sign(lMoveOffset.x)*penetrationX,lMoveOffset.y,FP.Sign(lMoveOffset.z)*penetrationZ);
					penetration = localNormal0.magnitude;
					isSide = true;
					isPort = true;
					point0 = lcenter1;
					return true;
					//if (
					//	// 在z朝向
					//	FP.Abs(lLastPos1.x) < hsize0.x && FP.Abs(lcenter1.x) < hsize0.x
					//	)
					//{
					//	isSide = true;
					//	isPort = true;
					//	penetration = FP.Sign(lMoveOffset.z) * hsize0.z - lcenter1.z;
					//	//localNormal0 = new TSVector(0, 0, lMoveOffset.z);
					//	localNormal0 = lMoveOffset;
					//	// 越过中心
					//	if (lLastPos1.z * lcenter1.z <= 0)
					//                   {
					//		penetration = FP.Abs(lcenter1.z) + hsize0.z;
					//                   }
					//                   else
					//                   {
					//		penetration = FP.Abs(lcenter1.z) - hsize0.z;
					//	}
					//	var penZ = TSMath.Max(diag1_1.z, diag1_2.z);
					//	penetration += FP.Abs(penZ);
					//	point0 = lcenter1;
					//	//TSVector.ClampBox(ref point0, ref hsize0, out point0);
					//	return true;
					//}
					//if (
					//	// 在x朝向
					//	FP.Abs(lLastPos1.z) < hsize0.z && FP.Abs(lcenter1.z) < hsize0.z
					//                   )
					//{
					//	isSide = true;
					//	isPort = true;
					//	penetration = FP.Sign(lMoveOffset.x) * hsize0.x - lcenter1.x;
					//	//localNormal0 = new TSVector(lMoveOffset.x, 0, 0);
					//	localNormal0 = lMoveOffset;
					//	// 越过中心
					//	if (lLastPos1.x * lcenter1.x <= 0)
					//                   {
					//		penetration = FP.Abs(lcenter1.x) + hsize0.x;
					//                   }
					//                   else
					//                   {
					//		penetration = FP.Abs(lcenter1.x) - hsize0.x;
					//	}
					//	var penX = TSMath.Max(diag1_1.x, diag1_2.x);
					//	penetration += FP.Abs(penX);
					//	point0 = lcenter1;
					//	//TSVector.ClampBox(ref point0, ref hsize0, out point0);
					//	return true;
					//}
				}

				localNormal0 = TSVector.zero;
				var vCount = 0;
				if(BoxContain(ref hsize0,ref vert1_1))
                {
					vCount++;
					TSVector.AddSelf(ref localNormal0, ref vert1_1);
                }
				if(BoxContain(ref hsize0,ref vert1_2))
                {
					vCount++;
					TSVector.AddSelf(ref localNormal0, ref vert1_2);
				}
				if (BoxContain(ref hsize0, ref vert1_3))
				{
					vCount++;
					TSVector.AddSelf(ref localNormal0, ref vert1_3);
				}
				if (BoxContain(ref hsize0, ref vert1_4))
				{
					vCount++;
					TSVector.AddSelf(ref localNormal0, ref vert1_4);
				}
                if (vCount > 0)
                {
					TSVector.Divide(ref localNormal0, vCount, out localNormal0);

					TSVector localNormal0C;
					TSVector.Multiply(ref lcenter1, 4, out localNormal0C);
					TSVector.Subtract(ref localNormal0C, ref localNormal0, out localNormal0C);
					TSVector.Divide(ref localNormal0C, 4 - vCount, out localNormal0C);

					if (FP.Abs(lcenter1.x) * hsize0.z > FP.Abs(lcenter1.z) * hsize0.x)
                    {
						// 取 x 向
						penetration = hsize0.x - FP.Abs(localNormal0.x);
						localNormal0 = localNormal0C;
						localNormal0.z = 0;
					}
                    else
                    {
						// 取 z 向
						penetration = hsize0.z - FP.Abs(localNormal0.z);
						localNormal0 = localNormal0C;
						localNormal0.x = 0;
					}
					localNormal0.y = 0;
					isSide = true;
				}
				else
                {
					TSVector.ClampBox(ref lcenter1, ref hsize0, out localNormal0);
					TSVector.Subtract(ref localNormal0, ref hsize0, out localNormal0);
					penetration = localNormal0.magnitude;
					//localNormal0.x = FP.Sign(localNormal0.x) * hsize0.x;
					//localNormal0.z = FP.Sign(localNormal0.z) * hsize0.z;
					//localNormal0.y = 0;
					localNormal0 = lcenter1;

					isSide = false;
				}

				// TSVector.ClampBoxXZWithRate(ref localNormal0, ref hsize0.x, ref hsize0.z, out point0);
				TSVector.ClampBox(ref localNormal0, ref hsize0, out point0);
				return true;
			}
		}
		// TODO: 提高效率, 暂时只考虑y轴旋转, 需要扩展到支持3轴旋转
		private bool DetectBoxBox(RigidBody b1, RigidBody b2, QueryTriggerInteraction queryTriggerInteraction, bool checkOnly = false)
		{
			//if (b1.gameObject.name.StartsWith("plan2_0") && b2.gameObject.name.Length == 8)
			//{
			//    UnityEngine.Debug.Log("lkwje");
			//}

			// TSVector point1x=new TSVector(), point2x=new TSVector();
			// if (XenoCollide.Detect(b1.Shape, b2.Shape, ref b1.orientation,
			//     ref b2.orientation, ref b1.position, ref b2.position,
			//     out var pointx, out var normalx, out var penetrationx, false))
			// {
			//     //normal = JVector.Up;
			//     //UnityEngine.Debug.Log("FINAL  --- >>> normal: " + normal);
			//     FindSupportPoints(b1, b2, b1.Shape, b2.Shape, ref pointx, ref normalx, out point1x, out point2x);
			//     //RaiseCollisionDetected(b1, b2, ref point1x, ref point2x, ref normalx, penetrationx, queryTriggerInteraction);
			//     //return true ;
			// }
			// else
			// {
			//     //return false;
			// }

			// if (b1.gameObject.name.StartsWith("obstacleCollider") || b2.gameObject.name.StartsWith("obstacleCollider"))
			// {
			//     UnityEngine.Debug.Log("lkjef");
			// }

			var box1 = b1.Shape as BoxShape;
			var box2 = b2.Shape as BoxShape;

			var pos1 = b1.position;
			ref var rot1 = ref b1.orientation;
			var pos2 = b2.position;
			ref var rot2 = ref b2.orientation;

			ref var wCenter1 = ref pos1;
			// box1.geomCen;
			// TSVector.Add(ref wCenter1, ref pos1, out wCenter1);

			ref var wCenter2 = ref pos2;
			// box2.geomCen;
			// TSVector.Add(ref wCenter2, ref pos2, out wCenter2);

			ref var hsize1 = ref box1.GetCachedScaledHalfSize();
			ref var hsize2 = ref box2.GetCachedScaledHalfSize();

			FP dY = FP.Abs(wCenter1.y - wCenter2.y);
			FP sY = hsize1.y + hsize2.y;
			if (dY > sY)
			{
				// 高度差过大
				return false;
			}

			TSMatrix rot2x1T;
			TSMatrix rot1x2T;
			{
				TSMatrix.Transpose(ref rot1, out rot1x2T);
				TSMatrix.Multiply(ref rot2, ref rot1x2T, out rot2x1T);
			}
			TSMatrix.Transpose(ref rot2x1T,out rot1x2T);

			if (
				CompareBoxVerts(ref wCenter1, ref rot1, ref hsize1, ref b1.lastPosition,
					ref wCenter2, ref rot2, ref hsize2, ref b2.lastPosition,
					ref rot2x1T,
					out var lCenter2,
					out var vert1_1, out var vert1_2, out var vert1_3, out var vert1_4,
					out var localNormal1, out var point1,
					out var isSide1, out var penetration1, out var isPort1
					) &&
				CompareBoxVerts(ref wCenter2, ref rot2, ref hsize2, ref b2.lastPosition,
					ref wCenter1, ref rot1, ref hsize1, ref b1.lastPosition,
					ref rot1x2T,
					out var lCenter1,
					out var vert2_1, out var vert2_2, out var vert2_3, out var vert2_4,
					out var localNormal2, out var point2,
					out var isSide2, out var penetration2, out var isPort2
					)
				)
			{
				TSVector normal;
				FP penetration;
				var lcenter2y = lCenter2.y;
				var lcenter1y = lCenter1.y;
				// 法线是2的挤压受力方向
				if (lcenter2y > hsize2.y || lcenter2y > hsize1.y * 3)
				{
					// center2 在上方
					normal = new TSVector(0, lCenter2.y, 0);
					penetration = (sY - dY);
					point1 = wCenter1;
					point2 = wCenter2;
					point1.y += hsize1.y;
					point2.y -= hsize2.y;
					point1.x = point2.x;
					point1.z = point2.z;
					//point1.x = point1x.x;
					//point1.z = point1x.z;
					//point2.x = point2x.x;
					//point2.z = point2x.z;
				}
				else if (lcenter1y > hsize1.y ||lcenter1y>hsize2.y*3)
				{
					// center1 在上方
					normal = new TSVector(0, -lCenter1.y, 0);
					penetration = (sY - dY);
					point1 = wCenter1;
					point2 = wCenter2;
					point1.y -= hsize1.y;
					point2.y += hsize2.y;
					point2.x = point1.x;
					point2.z = point1.z;
				}
				else
				{
					TSVector.Subtract(ref wCenter2, ref wCenter1, out normal);
					normal.y = 0;
					var dsize = hsize1 + hsize2;
					dsize.y = 0;
					var normalLen=normal.NormalizeR();
					penetration = dsize.magnitude - normalLen;

                    if (isSide1)
                    {
						normal = localNormal1;
						TSVector.Transform(ref normal, ref rot1, out normal);
						penetration = penetration1;
					}
					else if (isSide2)
                    {
						TSVector.Negate(ref localNormal2, out normal);
						TSVector.Transform(ref normal, ref rot2, out normal);
						penetration = penetration2;
					}
                    else
                    {
						normal = TSVector.Average(TSVector.Transform(localNormal1,rot1), TSVector.Transform(-localNormal2,rot2));
						penetration = 0.5f*(penetration1+penetration2);
					}
					normal.Normalize();

					point1 = wCenter1;
					point2 = wCenter2;
				}

				if (penetration < 0)
				{
					penetration = 0;
				}


				// if (b1.gameObject.name.StartsWith("obstacleCollider") || b2.gameObject.name.StartsWith("obstacleCollider"))
				// {
				//     if (penetration > 0.3f)
				//     {
				// 		UnityEngine.Debug.Log($"penetration: {penetration}");
				// 	}
				// }

				RaiseCollisionDetected(b1, b2, ref point1, ref point2, ref normal, penetration, queryTriggerInteraction);
				return true;
			}

			return false;
		}

		private bool CompareBoxSphere(
			ref TSVector pos1, ref TSMatrix rot1, ref TSVector hsize1,
			ref TSVector pos2, ref FP radius2,
			out TSVector lnearby1,out bool isInside
			)
		{
			TSVector lpos2;
			TSVector.Subtract(ref pos2, ref pos1, out lpos2);

			// TODO: 此处可提前粗略检查距离, 排除未相撞的情况
			// if( lpos2-hsize1 > radius^2 ){ return false; }

			TSVector.TransposedTransform(ref lpos2, ref rot1, out lpos2);
			// 得到最近矢量
			// 也即最近点, 箱子在原点, 所以不需要加箱子坐标
			// 如果碰撞, 则作为方块的碰撞点
			var isIntersect = !TSVector.ClampBox(ref lpos2, ref hsize1, out lnearby1);
			if (!isIntersect)
			{
				isInside = false;
				// 进一步判断和圆心距离
				TSVector.DistanceSQ(ref lnearby1, ref lpos2, out var distanceSQ);
				isIntersect = radius2 * radius2 >= distanceSQ;
            }
            else
            {
				// 圆心在方块内部
				isInside = true;
			}

			return isIntersect;
		}

		private bool DetectBoxSphere(RigidBody bb1, RigidBody bs2, QueryTriggerInteraction queryTriggerInteraction, bool checkOnly = false)
		{
			var box1 = bb1.Shape as BoxShape;
			var sphere2 = bs2.Shape as SphereShape;

			var pos1 = bb1.position;
			ref var rot1 = ref bb1.orientation;
			var pos2 = bs2.position;

			ref var hsize1 = ref box1.GetCachedScaledHalfSize();
			var radius2 = sphere2.ScaledRadius;

			// TSVector lpos2;
			// TSVector.Subtract(ref pos2, ref pos1, out lpos2);

			// // TODO: 此处可提前粗略检查距离, 排除未相撞的情况
			// // if( lpos2-hsize1 > radius^2 ){ return false; }

			// TSVector.TransposedTransform(ref lpos2, ref rot1, out lpos2);
			// TSVector nearby;
			// // 得到最近矢量
			// // 也即最近点, 箱子在原点, 所以不需要加箱子坐标
			// // 如果碰撞, 则作为方块的碰撞点
			// var isIntersect = !TSVector.ClampBox(ref lpos2, ref hsize1, out nearby);
			// if (!isIntersect)
			// {
			// 	// 进一步判断和圆心距离
			// 	TSVector.DistanceSQ(ref nearby, ref lpos2, out var distanceSQ);
			// 	isIntersect = radius2 * radius2 >= distanceSQ;
			// }
			TSVector lnearby1;
			var isIntersect = CompareBoxSphere(ref pos1, ref rot1, ref hsize1, ref pos2, ref radius2, out lnearby1,out var isInside);
			if (isIntersect)
			{
				TSVector point1, point2, normal;
				FP penetration;

				// 最近点作为双方碰撞点
				TSVector.Transform(ref lnearby1, ref rot1, out point1);
				TSVector.Add(ref point1, ref pos1, out point1);
				point2 = point1;
				if (isInside)
				{
					// 圆心在方块内部, (在穿透低的情况下)使用最近面法线.
					var isX = (hsize1.x - FP.Abs(lnearby1.x)) <= (hsize1.z - FP.Abs(lnearby1.z));
					bool isY;
					if (isX)
					{
						isY = (hsize1.y - FP.Abs(lnearby1.y)) < (hsize1.x - FP.Abs(lnearby1.x));
					}
                    else
                    {
						isY= (hsize1.x - FP.Abs(lnearby1.x)) < (hsize1.y - FP.Abs(lnearby1.y));
					}

					if (isY)
                    {
						normal = TSVector.zero;
						normal.y = -lnearby1.y;
                    }
					else if (isX)
                    {
						normal = TSVector.zero;
						normal.x = -lnearby1.x;
					}
                    else
                    {
						normal = TSVector.zero;
						normal.z = -lnearby1.z;
					}
                }
                else
                {
					// 最近点->圆心 矢量作为法线
					TSVector.Subtract(ref pos2, ref point1, out normal);
				}
				var normalLen = normal.NormalizeR();
				penetration = radius2 - normalLen;

				RaiseCollisionDetected(bb1, bs2, ref point1, ref point2, ref normal, penetration, queryTriggerInteraction);
			}

			return false;
		}

		private bool CompareBoxCapsuleHalfSphere(
			ref TSVector pos1, ref TSMatrix rot1, ref TSVector hsize1,
			ref TSVector pos2, ref FP radius2,
			ref TSVector pos2_s1, ref TSVector wrPolar_1,
			out TSVector lnearby1,
			out TSVector point1, out TSVector point2, out TSVector normal, out FP penetration
			)
		{
			var isIntersect = CompareBoxSphere(ref pos1, ref rot1, ref hsize1, ref pos2_s1, ref radius2, out lnearby1, out var isInside);
			if (isIntersect)
			{
				TSVector.Transform(ref lnearby1, ref rot1, out point1);
				TSVector.Add(ref point1, ref pos1, out point1);

				// 判断是极点方向半球, 还是极点反方向半球
				TSVector port;
				TSVector.Subtract(ref pos2_s1, ref point1, out port);
				var th = TSVector.Dot(ref port, ref wrPolar_1);
				if (th >= 0)
				{
					// 如果是极点方向半球, 那么返回半球碰撞
					// 最近点作为双方碰撞点
					point2 = point1;
					// 最近点->圆心 矢量作为法线
					TSVector.Subtract(ref pos2, ref point1, out normal);
					var normalLen=normal.NormalizeR();
					penetration = radius2 - normalLen;
				}
				else
				{
					// 如果是极点反方向半球, 那么返回圆柱碰撞
					// normal 和 port 同指向柱心
					TSVector.Cross(ref wrPolar_1, ref port, out normal);
					TSVector.Cross(ref normal, ref wrPolar_1, out normal);
					point2 = point1;
					var normalLen=normal.NormalizeR();
					penetration = radius2 - normalLen;
				}
				return true;
			}
			else
			{
				lnearby1 = normal = point2 = point1 = TSVector.zero;
				penetration = 0;
			}

			return false;
		}

		private bool DetectCapsuleBox(RigidBody bb1, RigidBody bs2, QueryTriggerInteraction queryTriggerInteraction, bool checkOnly = false)
		{

			var box1 = bb1.Shape as BoxShape;
			var sphere2 = bs2.Shape as CapsuleShape;

			var pos1 = bb1.position;
			ref var rot1 = ref bb1.orientation;
			var pos2 = bs2.position;
			ref var rot2 = ref bs2.orientation;

			ref var hsize1 = ref box1.GetCachedScaledHalfSize();
			var radius2 = sphere2.ScaledRadius;
			var length2 = sphere2.ScaledLength;
			var hlength2 = length2 * FP.myhalf;

			var lPolar_1 = new TSVector(0, -hlength2, 0);
			// 世界坐标下, 极点朝向
			TSVector wrPolar_1;
			TSVector.Transform(ref lPolar_1, ref rot2, out wrPolar_1);
			var wrPolar_2 = -wrPolar_1;

			// 先检查两端半球
			{
				// 上方球心世界坐标
				TSVector pos2_s1;
				TSVector.Add(ref pos2, ref wrPolar_1, out pos2_s1);

				TSVector point1, point2, normal;
				FP penetration;
				bool isIntersect;

				TSVector lnearby1;
				// 下方球心
				isIntersect = CompareBoxCapsuleHalfSphere(
					ref pos1, ref rot1, ref hsize1,
					ref pos2, ref radius2, ref pos2_s1, ref wrPolar_1,
					out lnearby1,
					out point1, out point2, out normal, out penetration);
				if (isIntersect)
				{
					RaiseCollisionDetected(bb1, bs2, ref point1, ref point2, ref normal, penetration, queryTriggerInteraction);
					return true;
				}

				// 上方球心
				TSVector pos2_s2;
				TSVector.Add(ref pos2, ref wrPolar_2, out pos2_s2);
				isIntersect = CompareBoxCapsuleHalfSphere(
					ref pos1, ref rot1, ref hsize1,
					ref pos2, ref radius2, ref pos2_s1, ref wrPolar_1,
					out lnearby1,
					out point1, out point2, out normal, out penetration);
				if (isIntersect)
				{
					RaiseCollisionDetected(bb1, bs2, ref point1, ref point2, ref normal, penetration, queryTriggerInteraction);
					return true;
				}
			}

			// 再检查圆柱
			{
				// 方块为原点
				{
					TSVector lpos2;
					TSVector.Subtract(ref pos2, ref pos1, out lpos2);
					TSVector.TransposedTransform(ref lpos2, ref rot1, out lpos2);

					TSVector lpolar2 = new TSVector(0, length2, 0);
					TSVector.Transform(ref lpolar2, ref rot2, out lpolar2);
					TSVector.TransposedTransform(ref lpolar2, ref rot1, out lpolar2);

					TSVector lpole1;
					TSVector.Add(ref lpos2, ref lpolar2, out lpole1);

					TSVector lpole2;
					TSVector.Subtract(ref lpos2, ref lpolar2, out lpole2);


					var hx = hsize1.x;
					var hy = hsize1.y;
					var hz = hsize1.z;
					var nhx = -hx;
					var nhy = -hy;
					var nhz = -hz;

					if (
						(nhx > lpole1.x + radius2 && nhx > lpole2.x + radius2) ||
						(hx < lpole1.x - radius2 && hx < lpole2.x - radius2) ||
						(nhy > lpole1.y + radius2 && nhy > lpole2.y + radius2) ||
						(hy < lpole1.y - radius2 && hy < lpole2.y - radius2) ||
						(nhz > lpole1.z + radius2 && nhz > lpole2.z + radius2) ||
						(hz < lpole1.z - radius2 && hz < lpole2.z - radius2)
						)
					{
						return false;
					}
				}
				// 柱心为原点
				{
					var axis1_1 = new TSVector(hsize1.x, 0, 0);
					var axis1_2 = new TSVector(0, 0, hsize1.z);
					TSVector axis1_3;
					TSVector lpos1;
					TSVector.Subtract(ref pos1, ref pos2, out lpos1);
					TSVector.TransposedTransform(ref lpos1, ref rot2, out lpos1);
					TSVector.Transform(ref axis1_1, ref rot1, out axis1_1);
					TSVector.TransposedTransform(ref axis1_1, ref rot2, out axis1_1);
					TSVector.Transform(ref axis1_2, ref rot1, out axis1_2);
					TSVector.TransposedTransform(ref axis1_2, ref rot2, out axis1_2);
					TSVector.Cross(ref axis1_1, ref axis1_2, out axis1_3);
					TSVector.Divide(ref axis1_3, (hsize1.x * hsize1.z) / hsize1.y, out axis1_3);

					var nhlength2 = -hlength2;
					var lpos1x = lpos1.x;
					var lpos1y = lpos1.y;
					var lpos1z = lpos1.z;
					var radius2pow2 = radius2 * radius2;
					if (
						(
							radius2pow2 < TSVector2.ModSqrt(lpos1x + axis1_1.x, lpos1z + axis1_1.z) &&
							radius2pow2 < TSVector2.ModSqrt(lpos1x - axis1_1.x, lpos1z - axis1_1.z) &&
							radius2pow2 < TSVector2.ModSqrt(lpos1x + axis1_2.x, lpos1z + axis1_2.z) &&
							radius2pow2 < TSVector2.ModSqrt(lpos1x - axis1_2.x, lpos1z - axis1_2.z) &&
							radius2pow2 < TSVector2.ModSqrt(lpos1x - axis1_3.x, lpos1z - axis1_3.z) &&
							radius2pow2 < TSVector2.ModSqrt(lpos1x - axis1_3.x, lpos1z - axis1_3.z)
							) ||
						(
							hlength2 < lpos1y + axis1_1.y && hlength2 < lpos1y - axis1_1.y &&
							hlength2 < lpos1y + axis1_2.y && hlength2 < lpos1y - axis1_2.y &&
							hlength2 < lpos1y + axis1_3.y && hlength2 < lpos1y - axis1_3.y
							)
						)
					{
						return false;
					}
				}

				{
					TSVector wnear1;
					TSVector.Subtract(ref pos2, ref pos1, out wnear1);
					TSVector.ClampBox(ref wnear1, ref hsize1, out wnear1);
					TSVector.Add(ref wnear1, ref pos1, out wnear1);
					TSVector normal;
					TSVector.Subtract(ref pos2, ref wnear1, out normal);
					var len = TSVector.Dot(ref wrPolar_2, ref normal) / hlength2;
					TSVector.Multiply(wrPolar_2, len / hlength2);
					TSVector.Subtract(ref normal, ref wrPolar_2, out normal);
					var normalLen = normal.NormalizeR();
					FP penetration = radius2 - normalLen;
					var point1 = wnear1;
					var point2 = point1;
					RaiseCollisionDetected(bb1, bs2, ref point1, ref point2, ref normal, penetration, queryTriggerInteraction);
				}
			}

			return false;
		}
	}
}
