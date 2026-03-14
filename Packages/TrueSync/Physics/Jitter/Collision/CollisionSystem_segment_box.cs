

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
		static void face(uint i0, uint i1, uint i2, ref PxVec3 rkPnt, /*const*/ref PxVec3 rkDir, /*const*/ref PxVec3 extents, /*const*/ref PxVec3 rkPmE,
			 out PxReal pfLParam, ref PxReal rfSqrDistance)
		{
			PxVec3 kPpE = new PxVec3();
			PxReal fLSqr, fInv, fTmp, fParam, fT, fDelta;

			kPpE[i1] = rkPnt[i1] + extents[i1];
			kPpE[i2] = rkPnt[i2] + extents[i2];
			if (rkDir[i0] * kPpE[i1] >= rkDir[i1] * rkPmE[i0])
			{
				if (rkDir[i0] * kPpE[i2] >= rkDir[i2] * rkPmE[i0])
				{
					// v[i1] >= -e[i1], v[i2] >= -e[i2] (distance = 0)
					// if (pfLParam)
					{
						rkPnt[i0] = extents[i0];
						fInv = 1.0f / rkDir[i0];
						rkPnt[i1] -= rkDir[i1] * rkPmE[i0] * fInv;
						rkPnt[i2] -= rkDir[i2] * rkPmE[i0] * fInv;
						pfLParam = -rkPmE[i0] * fInv;
					}
				}
				else
				{
					// v[i1] >= -e[i1], v[i2] < -e[i2]
					fLSqr = rkDir[i0] * rkDir[i0] + rkDir[i2] * rkDir[i2];
					fTmp = fLSqr * kPpE[i1] - rkDir[i1] * (rkDir[i0] * rkPmE[i0] + rkDir[i2] * kPpE[i2]);
					if (fTmp <= 2.0f * fLSqr * extents[i1])
					{
						fT = fTmp / fLSqr;
						fLSqr += rkDir[i1] * rkDir[i1];
						fTmp = kPpE[i1] - fT;
						fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * fTmp + rkDir[i2] * kPpE[i2];
						fParam = -fDelta / fLSqr;
						rfSqrDistance += rkPmE[i0] * rkPmE[i0] + fTmp * fTmp + kPpE[i2] * kPpE[i2] + fDelta * fParam;

						// if (pfLParam)
						{
							pfLParam = fParam;
							rkPnt[i0] = extents[i0];
							rkPnt[i1] = fT - extents[i1];
							rkPnt[i2] = -extents[i2];
						}
					}
					else
					{
						fLSqr += rkDir[i1] * rkDir[i1];
						fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * rkPmE[i1] + rkDir[i2] * kPpE[i2];
						fParam = -fDelta / fLSqr;
						rfSqrDistance += rkPmE[i0] * rkPmE[i0] + rkPmE[i1] * rkPmE[i1] + kPpE[i2] * kPpE[i2] + fDelta * fParam;

						// if (pfLParam)
						{
							pfLParam = fParam;
							rkPnt[i0] = extents[i0];
							rkPnt[i1] = extents[i1];
							rkPnt[i2] = -extents[i2];
						}
					}
				}
			}
			else
			{
				if (rkDir[i0] * kPpE[i2] >= rkDir[i2] * rkPmE[i0])
				{
					// v[i1] < -e[i1], v[i2] >= -e[i2]
					fLSqr = rkDir[i0] * rkDir[i0] + rkDir[i1] * rkDir[i1];
					fTmp = fLSqr * kPpE[i2] - rkDir[i2] * (rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1]);
					if (fTmp <= 2.0f * fLSqr * extents[i2])
					{
						fT = fTmp / fLSqr;
						fLSqr += rkDir[i2] * rkDir[i2];
						fTmp = kPpE[i2] - fT;
						fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * fTmp;
						fParam = -fDelta / fLSqr;
						rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + fTmp * fTmp + fDelta * fParam;

						// if (pfLParam)
						{
							pfLParam = fParam;
							rkPnt[i0] = extents[i0];
							rkPnt[i1] = -extents[i1];
							rkPnt[i2] = fT - extents[i2];
						}
					}
					else
					{
						fLSqr += rkDir[i2] * rkDir[i2];
						fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * rkPmE[i2];
						fParam = -fDelta / fLSqr;
						rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + rkPmE[i2] * rkPmE[i2] + fDelta * fParam;

						// if (pfLParam)
						{
							pfLParam = fParam;
							rkPnt[i0] = extents[i0];
							rkPnt[i1] = -extents[i1];
							rkPnt[i2] = extents[i2];
						}
					}
				}
				else
				{
					// v[i1] < -e[i1], v[i2] < -e[i2]
					fLSqr = rkDir[i0] * rkDir[i0] + rkDir[i2] * rkDir[i2];
					fTmp = fLSqr * kPpE[i1] - rkDir[i1] * (rkDir[i0] * rkPmE[i0] + rkDir[i2] * kPpE[i2]);
					if (fTmp >= 0.0f)
					{
						// v[i1]-edge is closest
						if (fTmp <= 2.0f * fLSqr * extents[i1])
						{
							fT = fTmp / fLSqr;
							fLSqr += rkDir[i1] * rkDir[i1];
							fTmp = kPpE[i1] - fT;
							fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * fTmp + rkDir[i2] * kPpE[i2];
							fParam = -fDelta / fLSqr;
							rfSqrDistance += rkPmE[i0] * rkPmE[i0] + fTmp * fTmp + kPpE[i2] * kPpE[i2] + fDelta * fParam;

							// if (pfLParam)
							{
								pfLParam = fParam;
								rkPnt[i0] = extents[i0];
								rkPnt[i1] = fT - extents[i1];
								rkPnt[i2] = -extents[i2];
							}
						}
						else
						{
							fLSqr += rkDir[i1] * rkDir[i1];
							fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * rkPmE[i1] + rkDir[i2] * kPpE[i2];
							fParam = -fDelta / fLSqr;
							rfSqrDistance += rkPmE[i0] * rkPmE[i0] + rkPmE[i1] * rkPmE[i1] + kPpE[i2] * kPpE[i2] + fDelta * fParam;

							// if (pfLParam)
							{
								pfLParam = fParam;
								rkPnt[i0] = extents[i0];
								rkPnt[i1] = extents[i1];
								rkPnt[i2] = -extents[i2];
							}
						}
						return;
					}

					fLSqr = rkDir[i0] * rkDir[i0] + rkDir[i1] * rkDir[i1];
					fTmp = fLSqr * kPpE[i2] - rkDir[i2] * (rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1]);
					if (fTmp >= 0.0f)
					{
						// v[i2]-edge is closest
						if (fTmp <= 2.0f * fLSqr * extents[i2])
						{
							fT = fTmp / fLSqr;
							fLSqr += rkDir[i2] * rkDir[i2];
							fTmp = kPpE[i2] - fT;
							fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * fTmp;
							fParam = -fDelta / fLSqr;
							rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + fTmp * fTmp + fDelta * fParam;

							// if (pfLParam)
							{
								pfLParam = fParam;
								rkPnt[i0] = extents[i0];
								rkPnt[i1] = -extents[i1];
								rkPnt[i2] = fT - extents[i2];
							}
						}
						else
						{
							fLSqr += rkDir[i2] * rkDir[i2];
							fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * rkPmE[i2];
							fParam = -fDelta / fLSqr;
							rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + rkPmE[i2] * rkPmE[i2] + fDelta * fParam;

							// if (pfLParam)
							{
								pfLParam = fParam;
								rkPnt[i0] = extents[i0];
								rkPnt[i1] = -extents[i1];
								rkPnt[i2] = extents[i2];
							}
						}
						return;
					}

					// (v[i1],v[i2])-corner is closest
					fLSqr += rkDir[i2] * rkDir[i2];
					fDelta = rkDir[i0] * rkPmE[i0] + rkDir[i1] * kPpE[i1] + rkDir[i2] * kPpE[i2];
					fParam = -fDelta / fLSqr;
					rfSqrDistance += rkPmE[i0] * rkPmE[i0] + kPpE[i1] * kPpE[i1] + kPpE[i2] * kPpE[i2] + fDelta * fParam;

					// if (pfLParam)
					{
						pfLParam = fParam;
						rkPnt[i0] = extents[i0];
						rkPnt[i1] = -extents[i1];
						rkPnt[i2] = -extents[i2];
					}
				}
			}
		}

		/**
		 * 线段和box距离: 线段方向三轴全正的情况
		 * @param rkPnt 线段起点
		 * @param rkDir 线段方向
		 * @param extends 方块半径
		 */
		static void caseNoZeros(ref PxVec3 rkPnt, /*const*/ref PxVec3 rkDir, /*const*/ref PxVec3 extents, out PxReal pfLParam, ref PxReal rfSqrDistance)
		{
			PxVec3 kPmE = new PxVec3(rkPnt.x - extents.x, rkPnt.y - extents.y, rkPnt.z - extents.z);

			PxReal fProdDxPy, fProdDyPx, fProdDzPx, fProdDxPz, fProdDzPy, fProdDyPz;

			fProdDxPy = rkDir.x * kPmE.y;
			fProdDyPx = rkDir.y * kPmE.x;
			if (fProdDyPx >= fProdDxPy)
			{
				fProdDzPx = rkDir.z * kPmE.x;
				fProdDxPz = rkDir.x * kPmE.z;
				if (fProdDzPx >= fProdDxPz)
				{
					// line intersects x = e0
					face(0, 1, 2, ref rkPnt, ref rkDir, ref extents, ref kPmE, out pfLParam, ref rfSqrDistance);
				}
				else
				{
					// line intersects z = e2
					face(2, 0, 1, ref rkPnt, ref rkDir, ref extents, ref kPmE, out pfLParam, ref rfSqrDistance);
				}
			}
			else
			{
				fProdDzPy = rkDir.z * kPmE.y;
				fProdDyPz = rkDir.y * kPmE.z;
				if (fProdDzPy >= fProdDyPz)
				{
					// line intersects y = e1
					face(1, 2, 0, ref rkPnt, ref rkDir, ref extents, ref kPmE, out pfLParam, ref rfSqrDistance);
				}
				else
				{
					// line intersects z = e2
					face(2, 0, 1, ref rkPnt, ref rkDir, ref extents, ref kPmE, out pfLParam, ref rfSqrDistance);
				}
			}
		}

		static void case0(uint i0, uint i1, uint i2, ref PxVec3 rkPnt, /*const*/ref PxVec3 rkDir, /*const*/ref PxVec3 extents, out PxReal pfLParam, ref PxReal rfSqrDistance)
		{
			PxReal fPmE0 = rkPnt[i0] - extents[i0];
			PxReal fPmE1 = rkPnt[i1] - extents[i1];
			PxReal fProd0 = rkDir[i1] * fPmE0;
			PxReal fProd1 = rkDir[i0] * fPmE1;
			PxReal fDelta, fInvLSqr, fInv;

			if (fProd0 >= fProd1)
			{
				// line intersects P[i0] = e[i0]
				rkPnt[i0] = extents[i0];

				PxReal fPpE1 = rkPnt[i1] + extents[i1];
				fDelta = fProd0 - rkDir[i0] * fPpE1;
				if (fDelta >= 0.0f)
				{
					fInvLSqr = 1.0f / (rkDir[i0] * rkDir[i0] + rkDir[i1] * rkDir[i1]);
					rfSqrDistance += fDelta * fDelta * fInvLSqr;
					// if (pfLParam)
					{
						rkPnt[i1] = -extents[i1];
						pfLParam = -(rkDir[i0] * fPmE0 + rkDir[i1] * fPpE1) * fInvLSqr;
					}
				}
				else
				{
					// if (pfLParam)
					{
						fInv = 1.0f / rkDir[i0];
						rkPnt[i1] -= fProd0 * fInv;
						pfLParam = -fPmE0 * fInv;
					}
				}
			}
			else
			{
				// line intersects P[i1] = e[i1]
				rkPnt[i1] = extents[i1];

				PxReal fPpE0 = rkPnt[i0] + extents[i0];
				fDelta = fProd1 - rkDir[i1] * fPpE0;
				if (fDelta >= 0.0f)
				{
					fInvLSqr = 1.0f / (rkDir[i0] * rkDir[i0] + rkDir[i1] * rkDir[i1]);
					rfSqrDistance += fDelta * fDelta * fInvLSqr;
					// if (pfLParam)
					{
						rkPnt[i0] = -extents[i0];
						pfLParam = -(rkDir[i0] * fPpE0 + rkDir[i1] * fPmE1) * fInvLSqr;
					}
				}
				else
				{
					// if (pfLParam)
					{
						fInv = 1.0f / rkDir[i1];
						rkPnt[i0] -= fProd1 * fInv;
						pfLParam = -fPmE1 * fInv;
					}
				}
			}

			if (rkPnt[i2] < -extents[i2])
			{
				fDelta = rkPnt[i2] + extents[i2];
				rfSqrDistance += fDelta * fDelta;
				rkPnt[i2] = -extents[i2];
			}
			else if (rkPnt[i2] > extents[i2])
			{
				fDelta = rkPnt[i2] - extents[i2];
				rfSqrDistance += fDelta * fDelta;
				rkPnt[i2] = extents[i2];
			}
		}

		static void case00(uint i0, uint i1, uint i2, ref PxVec3 rkPnt, /*const*/ref PxVec3 rkDir, /*const*/ref PxVec3 extents, out PxReal pfLParam, ref PxReal rfSqrDistance)
		{
			PxReal fDelta;

			// if (pfLParam)
			{
				pfLParam = (extents[i0] - rkPnt[i0]) / rkDir[i0];
			}

			rkPnt[i0] = extents[i0];

			if (rkPnt[i1] < -extents[i1])
			{
				fDelta = rkPnt[i1] + extents[i1];
				rfSqrDistance += fDelta * fDelta;
				rkPnt[i1] = -extents[i1];
			}
			else if (rkPnt[i1] > extents[i1])
			{
				fDelta = rkPnt[i1] - extents[i1];
				rfSqrDistance += fDelta * fDelta;
				rkPnt[i1] = extents[i1];
			}

			if (rkPnt[i2] < -extents[i2])
			{
				fDelta = rkPnt[i2] + extents[i2];
				rfSqrDistance += fDelta * fDelta;
				rkPnt[i2] = -extents[i2];
			}
			else if (rkPnt[i2] > extents[i2])
			{
				fDelta = rkPnt[i2] - extents[i2];
				rfSqrDistance += fDelta * fDelta;
				rkPnt[i2] = extents[i2];
			}
		}

		static void case000(ref PxVec3 rkPnt, /*const*/ref PxVec3 extents, ref PxReal rfSqrDistance)
		{
			PxReal fDelta;

			if (rkPnt.x < -extents.x)
			{
				fDelta = rkPnt.x + extents.x;
				rfSqrDistance += fDelta * fDelta;
				rkPnt.x = -extents.x;
			}
			else if (rkPnt.x > extents.x)
			{
				fDelta = rkPnt.x - extents.x;
				rfSqrDistance += fDelta * fDelta;
				rkPnt.x = extents.x;
			}

			if (rkPnt.y < -extents.y)
			{
				fDelta = rkPnt.y + extents.y;
				rfSqrDistance += fDelta * fDelta;
				rkPnt.y = -extents.y;
			}
			else if (rkPnt.y > extents.y)
			{
				fDelta = rkPnt.y - extents.y;
				rfSqrDistance += fDelta * fDelta;
				rkPnt.y = extents.y;
			}

			if (rkPnt.z < -extents.z)
			{
				fDelta = rkPnt.z + extents.z;
				rfSqrDistance += fDelta * fDelta;
				rkPnt.z = -extents.z;
			}
			else if (rkPnt.z > extents.z)
			{
				fDelta = rkPnt.z - extents.z;
				rfSqrDistance += fDelta * fDelta;
				rkPnt.z = extents.z;
			}
		}

		//! Compute the smallest distance from the (infinite) line to the box.
		static PxReal distanceLineBoxSquared(ref PxVec3 lineOrigin, /*const*/ref PxVec3 lineDirection,
												 /*const*/ref PxVec3 boxOrigin, /*const*/ref PxVec3 boxExtent, /*const*/ref PxMat33 boxBase,
											 out PxReal lineParam,
											 out PxVec3 boxParam)
		{
			PxVec3 axis0 = boxBase.column0;
			PxVec3 axis1 = boxBase.column1;
			PxVec3 axis2 = boxBase.column2;

			// 转换坐标系
			// compute coordinates of line in box coordinate system
			PxVec3 diff = lineOrigin - boxOrigin;
			// 计算矢量基投影, 转换为方块局部坐标: 计算局部坐标, 线段端点0坐标
			PxVec3 pnt = new PxVec3(diff.Dot(ref axis0), diff.Dot(ref axis1), diff.Dot(ref axis2));
			// 计算矢量基投影, 转换为方块局部坐标: 计算局部坐标下, 线段方向
			PxVec3 dir = new PxVec3(lineDirection.Dot(ref axis0), lineDirection.Dot(ref axis1), lineDirection.Dot(ref axis2));

			// Apply reflections so that direction vector has nonnegative components.
			// 线段方向镜像变换到非负
			var reflect = new bool[3];
			for (uint i = 0; i < 3; i++)
			{
				if (dir[i] < 0.0f)
				{
					pnt[i] = -pnt[i];
					dir[i] = -dir[i];
					reflect[i] = true;
				}
				else
				{
					reflect[i] = false;
				}
			}

			PxReal sqrDistance = 0.0f;

			/**
			 * 按照轴值为0的轴数, 分为四个情况
			 */

			if (dir.x > 0.0f)
			{
				if (dir.y > 0.0f)
				{
					if (dir.z > 0.0f)
						caseNoZeros(ref pnt, ref dir, ref boxExtent, out lineParam, ref sqrDistance); // (+,+,+)
					else
						case0(0, 1, 2, ref pnt, ref dir, ref boxExtent, out lineParam, ref sqrDistance); // (+,+,0)
				}
				else
				{
					if (dir.z > 0.0f)
						case0(0, 2, 1, ref pnt, ref dir, ref boxExtent, out lineParam, ref sqrDistance); // (+,0,+)
					else
						case00(0, 1, 2, ref pnt, ref dir, ref boxExtent, out lineParam, ref sqrDistance); // (+,0,0)
				}
			}
			else
			{
				if (dir.y > 0.0f)
				{
					if (dir.z > 0.0f)
						case0(1, 2, 0, ref pnt, ref dir, ref boxExtent, out lineParam, ref sqrDistance); // (0,+,+)
					else
						case00(1, 0, 2, ref pnt, ref dir, ref boxExtent, out lineParam, ref sqrDistance); // (0,+,0)
				}
				else
				{
					if (dir.z > 0.0f)
						case00(2, 0, 1, ref pnt, ref dir, ref boxExtent, out lineParam, ref sqrDistance); // (0,0,+)
					else
					{
						case000(ref pnt, ref boxExtent, ref sqrDistance); // (0,0,0)
																		  // if (lineParam)
						{
							lineParam = 0.0f;
						}
					}
				}
			}

			// if (boxParam)
			{
				// undo reflections
				// 还原反射变换
				for (uint i = 0; i < 3; i++)
				{
					if (reflect[i])
						pnt[i] = -pnt[i];
				}

				// 传出线段原点
				boxParam = pnt;
			}

			return sqrDistance;
		}

		//! Compute the smallest distance from the (finite) line segment to the box.
		PxReal distanceSegmentBoxSquared(ref PxVec3 segmentPoint0, /*const*/ref PxVec3 segmentPoint1,
												 /*const*/ref PxVec3 boxOrigin, /*const*/ref PxVec3 boxExtent, /*const*/ref PxMat33 boxBase,
											 out PxReal segmentParam,
											 out PxVec3 boxParam)
		{
			// 转换到方块的局部坐标系
			// compute coordinates of line in box coordinate system

			PxReal lp;
			PxVec3 bp;

			var segDist = segmentPoint1 - segmentPoint0;
			PxReal sqrDistance = distanceLineBoxSquared(ref segmentPoint0, ref segDist, ref boxOrigin, ref boxExtent, ref boxBase, out lp, out bp);
			if (lp >= 0.0f)
			{
				if (lp <= 1.0f)
				{
					// if (segmentParam)
					{
						segmentParam = lp;
					}
					// if (boxParam)
					{
						boxParam = bp;
					}
					return sqrDistance;
				}
				else
				{
					// if (segmentParam)
					{
						segmentParam = 1.0f;
					}
					return distancePointBoxSquared(ref segmentPoint1, ref boxOrigin, ref boxExtent, ref boxBase, out boxParam);
				}
			}
			else
			{
				// if (segmentParam)
				{
					segmentParam = 0.0f;
				}
				return distancePointBoxSquared(ref segmentPoint0, ref boxOrigin, ref boxExtent, ref boxBase, out boxParam);
			}
		}

	}
}
