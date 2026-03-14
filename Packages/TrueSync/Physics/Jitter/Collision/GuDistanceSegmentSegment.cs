

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

	using Px = FP;
	using PxReal = FP;
	using PxU32 = UInt32;
	using PxVec3 = TSVector;
	using PxMat33 = TSMatrix;

	public abstract partial class CollisionSystem
	{
		const float ZERO_TOLERANCE = 1e-06f;

		// S0 = origin + extent * dir;
		// S1 = origin - extent * dir;
		PxReal distanceSegmentSegmentSquared(   /*const*/ ref PxVec3 origin0, /*const*/ ref PxVec3 dir0, PxReal extent0,
												/*const*/ ref PxVec3 origin1, /*const*/ ref PxVec3 dir1, PxReal extent1,
												out PxReal param0, out PxReal param1)
		{
			PxVec3 kDiff = origin0 - origin1;
			PxReal fA01 = -dir0.Dot(dir1);
			PxReal fB0 = kDiff.Dot(dir0);
			PxReal fB1 = -kDiff.Dot(dir1);
			PxReal fC = kDiff.sqrMagnitude;
			PxReal fDet = Px.Abs(1.0f - fA01 * fA01);
			PxReal fS0, fS1, fSqrDist, fExtDet0, fExtDet1, fTmpS0, fTmpS1;

			if (fDet >= ZERO_TOLERANCE)
			{
				// segments are not parallel
				fS0 = fA01 * fB1 - fB0;
				fS1 = fA01 * fB0 - fB1;
				fExtDet0 = extent0 * fDet;
				fExtDet1 = extent1 * fDet;

				if (fS0 >= -fExtDet0)
				{
					if (fS0 <= fExtDet0)
					{
						if (fS1 >= -fExtDet1)
						{
							if (fS1 <= fExtDet1)  // region 0 (interior)
							{
								// minimum at two interior points of 3D lines
								PxReal fInvDet = 1.0f / fDet;
								fS0 *= fInvDet;
								fS1 *= fInvDet;
								fSqrDist = fS0 * (fS0 + fA01 * fS1 + 2.0f * fB0) + fS1 * (fA01 * fS0 + fS1 + 2.0f * fB1) + fC;
							}
							else  // region 3 (side)
							{
								fS1 = extent1;
								fTmpS0 = -(fA01 * fS1 + fB0);
								if (fTmpS0 < -extent0)
								{
									fS0 = -extent0;
									fSqrDist = fS0 * (fS0 - 2.0f * fTmpS0) + fS1 * (fS1 + 2.0f * fB1) + fC;
								}
								else if (fTmpS0 <= extent0)
								{
									fS0 = fTmpS0;
									fSqrDist = -fS0 * fS0 + fS1 * (fS1 + 2.0f * fB1) + fC;
								}
								else
								{
									fS0 = extent0;
									fSqrDist = fS0 * (fS0 - 2.0f * fTmpS0) + fS1 * (fS1 + 2.0f * fB1) + fC;
								}
							}
						}
						else  // region 7 (side)
						{
							fS1 = -extent1;
							fTmpS0 = -(fA01 * fS1 + fB0);
							if (fTmpS0 < -extent0)
							{
								fS0 = -extent0;
								fSqrDist = fS0 * (fS0 - 2.0f * fTmpS0) + fS1 * (fS1 + 2.0f * fB1) + fC;
							}
							else if (fTmpS0 <= extent0)
							{
								fS0 = fTmpS0;
								fSqrDist = -fS0 * fS0 + fS1 * (fS1 + 2.0f * fB1) + fC;
							}
							else
							{
								fS0 = extent0;
								fSqrDist = fS0 * (fS0 - 2.0f * fTmpS0) + fS1 * (fS1 + 2.0f * fB1) + fC;
							}
						}
					}
					else
					{
						if (fS1 >= -fExtDet1)
						{
							if (fS1 <= fExtDet1)  // region 1 (side)
							{
								fS0 = extent0;
								fTmpS1 = -(fA01 * fS0 + fB1);
								if (fTmpS1 < -extent1)
								{
									fS1 = -extent1;
									fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
								else if (fTmpS1 <= extent1)
								{
									fS1 = fTmpS1;
									fSqrDist = -fS1 * fS1 + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
								else
								{
									fS1 = extent1;
									fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
							}
							else  // region 2 (corner)
							{
								fS1 = extent1;
								fTmpS0 = -(fA01 * fS1 + fB0);
								if (fTmpS0 < -extent0)
								{
									fS0 = -extent0;
									fSqrDist = fS0 * (fS0 - 2.0f * fTmpS0) + fS1 * (fS1 + 2.0f * fB1) + fC;
								}
								else if (fTmpS0 <= extent0)
								{
									fS0 = fTmpS0;
									fSqrDist = -fS0 * fS0 + fS1 * (fS1 + 2.0f * fB1) + fC;
								}
								else
								{
									fS0 = extent0;
									fTmpS1 = -(fA01 * fS0 + fB1);
									if (fTmpS1 < -extent1)
									{
										fS1 = -extent1;
										fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
									}
									else if (fTmpS1 <= extent1)
									{
										fS1 = fTmpS1;
										fSqrDist = -fS1 * fS1 + fS0 * (fS0 + 2.0f * fB0) + fC;
									}
									else
									{
										fS1 = extent1;
										fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
									}
								}
							}
						}
						else  // region 8 (corner)
						{
							fS1 = -extent1;
							fTmpS0 = -(fA01 * fS1 + fB0);
							if (fTmpS0 < -extent0)
							{
								fS0 = -extent0;
								fSqrDist = fS0 * (fS0 - 2.0f * fTmpS0) + fS1 * (fS1 + 2.0f * fB1) + fC;
							}
							else if (fTmpS0 <= extent0)
							{
								fS0 = fTmpS0;
								fSqrDist = -fS0 * fS0 + fS1 * (fS1 + 2.0f * fB1) + fC;
							}
							else
							{
								fS0 = extent0;
								fTmpS1 = -(fA01 * fS0 + fB1);
								if (fTmpS1 > extent1)
								{
									fS1 = extent1;
									fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
								else if (fTmpS1 >= -extent1)
								{
									fS1 = fTmpS1;
									fSqrDist = -fS1 * fS1 + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
								else
								{
									fS1 = -extent1;
									fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
							}
						}
					}
				}
				else
				{
					if (fS1 >= -fExtDet1)
					{
						if (fS1 <= fExtDet1)  // region 5 (side)
						{
							fS0 = -extent0;
							fTmpS1 = -(fA01 * fS0 + fB1);
							if (fTmpS1 < -extent1)
							{
								fS1 = -extent1;
								fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
							}
							else if (fTmpS1 <= extent1)
							{
								fS1 = fTmpS1;
								fSqrDist = -fS1 * fS1 + fS0 * (fS0 + 2.0f * fB0) + fC;
							}
							else
							{
								fS1 = extent1;
								fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
							}
						}
						else  // region 4 (corner)
						{
							fS1 = extent1;
							fTmpS0 = -(fA01 * fS1 + fB0);
							if (fTmpS0 > extent0)
							{
								fS0 = extent0;
								fSqrDist = fS0 * (fS0 - 2.0f * fTmpS0) + fS1 * (fS1 + 2.0f * fB1) + fC;
							}
							else if (fTmpS0 >= -extent0)
							{
								fS0 = fTmpS0;
								fSqrDist = -fS0 * fS0 + fS1 * (fS1 + 2.0f * fB1) + fC;
							}
							else
							{
								fS0 = -extent0;
								fTmpS1 = -(fA01 * fS0 + fB1);
								if (fTmpS1 < -extent1)
								{
									fS1 = -extent1;
									fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
								else if (fTmpS1 <= extent1)
								{
									fS1 = fTmpS1;
									fSqrDist = -fS1 * fS1 + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
								else
								{
									fS1 = extent1;
									fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
								}
							}
						}
					}
					else   // region 6 (corner)
					{
						fS1 = -extent1;
						fTmpS0 = -(fA01 * fS1 + fB0);
						if (fTmpS0 > extent0)
						{
							fS0 = extent0;
							fSqrDist = fS0 * (fS0 - 2.0f * fTmpS0) + fS1 * (fS1 + 2.0f * fB1) + fC;
						}
						else if (fTmpS0 >= -extent0)
						{
							fS0 = fTmpS0;
							fSqrDist = -fS0 * fS0 + fS1 * (fS1 + 2.0f * fB1) + fC;
						}
						else
						{
							fS0 = -extent0;
							fTmpS1 = -(fA01 * fS0 + fB1);
							if (fTmpS1 < -extent1)
							{
								fS1 = -extent1;
								fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
							}
							else if (fTmpS1 <= extent1)
							{
								fS1 = fTmpS1;
								fSqrDist = -fS1 * fS1 + fS0 * (fS0 + 2.0f * fB0) + fC;
							}
							else
							{
								fS1 = extent1;
								fSqrDist = fS1 * (fS1 - 2.0f * fTmpS1) + fS0 * (fS0 + 2.0f * fB0) + fC;
							}
						}
					}
				}
			}
			else
			{
				// The segments are parallel.
				PxReal fE0pE1 = extent0 + extent1;
				PxReal fSign = (fA01 > 0.0f ? -1.0f : 1.0f);
				PxReal b0Avr = 0.5f * (fB0 - fSign * fB1);
				PxReal fLambda = -b0Avr;
				if (fLambda < -fE0pE1)
				{
					fLambda = -fE0pE1;
				}
				else if (fLambda > fE0pE1)
				{
					fLambda = fE0pE1;
				}

				fS1 = -fSign * fLambda * extent1 / fE0pE1;
				fS0 = fLambda + fSign * fS1;
				fSqrDist = fLambda * (fLambda + 2.0f * b0Avr) + fC;
			}

			// if (param0)
			{
				param0 = fS0;
			}
			// if (param1)
			{
				param1 = fS1;
			}

			// account for numerical round-off error
			return TSMath.Max(0.0f, fSqrDist);
		}

		PxReal distanceSegmentSegmentSquared(   /*const*/ ref PxVec3 origin0, /*const*/ ref PxVec3 extent0,
												/*const*/ ref PxVec3 origin1, /*const*/ ref PxVec3 extent1,
												out PxReal param0,
												out PxReal param1)
		{
			// Some conversion is needed between the old & new code
			// Old:
			// segment (s0, s1)
			// origin = s0
			// extent = s1 - s0
			//
			// New:
			// s0 = origin + extent * dir;
			// s1 = origin - extent * dir;

			// dsequeira: is this really sensible? We use a highly optimized Wild Magic routine, 
			// then use a segment representation that requires an expensive conversion to/from...

			PxVec3 dir0 = extent0;
			PxVec3 center0 = origin0 + extent0 * 0.5f;
			PxReal length0 = extent0.magnitude;   //AM: change to make it work for degenerate (zero length) segments.
			bool b0 = length0 != 0.0f;
			PxReal oneOverLength0 = 0.0f;
			if (b0)
			{
				oneOverLength0 = 1.0f / length0;
				dir0 *= oneOverLength0;
				length0 *= 0.5f;
			}

			PxVec3 dir1 = extent1;
			PxVec3 center1 = origin1 + extent1 * 0.5f;
			PxReal length1 = extent1.magnitude;
			bool b1 = length1 != 0.0f;
			PxReal oneOverLength1 = 0.0f;
			if (b1)
			{
				oneOverLength1 = 1.0f / length1;
				dir1 *= oneOverLength1;
				length1 *= 0.5f;
			}

			// the return param vals have -extent = s0, extent = s1

			PxReal d2 = distanceSegmentSegmentSquared(ref center0, ref dir0, length0,
															ref center1, ref dir1, length1,
															out param0, out param1);

			//ML : This is wrong for some reason, I guess it has precision issue
			//// renormalize into the 0 = s0, 1 = s1 range
			//if (param0)
			//	*param0 = b0 ? ((*param0) * oneOverLength0 * 0.5f + 0.5f) : 0.0f;
			//if (param1)
			//	*param1 = b1 ? ((*param1) * oneOverLength1 * 0.5f + 0.5f) : 0.0f;

			// if (param0)
			{
				param0 = b0 ? ((length0 + (param0)) * oneOverLength0) : 0.0f;
			}
			// if (param1)
			{
				param1 = b1 ? ((length1 + (param1)) * oneOverLength1) : 0.0f;
			}

			return d2;
		}

	}
}
