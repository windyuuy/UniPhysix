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

	#region private class SweepPoint
	public class SweepPoint
	{
		public IBroadphaseEntity Body;
		public bool Begin;
		public int Axis;

		public SweepPoint(IBroadphaseEntity body, bool begin, int axis)
		{
			this.Body = body;
			this.Begin = begin;
			this.Axis = axis;
		}

		public FP Value
		{
			get
			{
				if (Begin)
				{
					if (Axis == 0) return Body.BoundingBox.min.x;
					else if (Axis == 1) return Body.BoundingBox.min.y;
					else return Body.BoundingBox.min.z;
				}
				else
				{
					if (Axis == 0) return Body.BoundingBox.max.x;
					else if (Axis == 1) return Body.BoundingBox.max.y;
					else return Body.BoundingBox.max.z;
				}
			}
		}


	}
	#endregion

	#region private struct OverlapPair
	public class OverlapPair : IComparable
	{
		// internal values for faster access within the engine
		public IBroadphaseEntity Entity1, Entity2;

		/// <summary>
		/// Initializes a new instance of the BodyPair class.
		/// </summary>
		/// <param name="entity1"></param>
		/// <param name="entity2"></param>
		public OverlapPair(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			this.Entity1 = entity1;
			this.Entity2 = entity2;
		}

		/// <summary>
		/// Don't call this, while the key is used in the arbitermap.
		/// It changes the hashcode of this object.
		/// </summary>
		/// <param name="entity1">The first body.</param>
		/// <param name="entity2">The second body.</param>
		internal void SetBodies(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			this.Entity1 = entity1;
			this.Entity2 = entity2;
		}

		/// <summary>
		/// Checks if two objects are equal.
		/// </summary>
		/// <param name="obj">The object to check against.</param>
		/// <returns>Returns true if they are equal, otherwise false.</returns>
		public override bool Equals(object obj)
		{
			OverlapPair other = (OverlapPair)obj;
			return (other.Entity1.Equals(Entity1) && other.Entity2.Equals(Entity2) ||
				other.Entity1.Equals(Entity2) && other.Entity2.Equals(Entity1));
		}

		/// <summary>
		/// Returns the hashcode of the BodyPair.
		/// The hashcode is the same if an BodyPair contains the same bodies.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return Entity1.GetHashCode() + Entity2.GetHashCode();
		}

		public int CompareTo(object obj)
		{
			if (obj is OverlapPair)
			{
				long a = ((OverlapPair)obj).GetHashCode();
				long b = GetHashCode();

				//int diff = ((OverlapPair)obj).GetHashCode () - GetHashCode ();
				long diff = a - b;
				if (diff < 0)
				{
					return 1;
				}
				else if (diff > 0)
				{
					return -1;
				}
			}

			return 0;
		}


	}
	#endregion

	public class OverlapPairContact : IComparable
	{

		// internal values for faster access within the engine
		public IBroadphaseEntity Entity1, Entity2;

		public Contact contact;

		/// <summary>
		/// Initializes a new instance of the BodyPair class.
		/// </summary>
		/// <param name="entity1"></param>
		/// <param name="entity2"></param>
		public OverlapPairContact(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			this.Entity1 = entity1;
			this.Entity2 = entity2;
		}

		/// <summary>
		/// Don't call this, while the key is used in the arbitermap.
		/// It changes the hashcode of this object.
		/// </summary>
		/// <param name="entity1">The first body.</param>
		/// <param name="entity2">The second body.</param>
		internal void SetBodies(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			this.Entity1 = entity1;
			this.Entity2 = entity2;
		}

		/// <summary>
		/// Checks if two objects are equal.
		/// </summary>
		/// <param name="obj">The object to check against.</param>
		/// <returns>Returns true if they are equal, otherwise false.</returns>
		public override bool Equals(object obj)
		{
			OverlapPairContact other = (OverlapPairContact)obj;
			return (other.Entity1.Equals(Entity1) && other.Entity2.Equals(Entity2) ||
				other.Entity1.Equals(Entity2) && other.Entity2.Equals(Entity1));
		}

		/// <summary>
		/// Returns the hashcode of the BodyPair.
		/// The hashcode is the same if an BodyPair contains the same bodies.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return Entity1.GetHashCode() + Entity2.GetHashCode();
		}

		public int CompareTo(object obj)
		{
			if (obj is OverlapPairContact)
			{
				long a = ((OverlapPairContact)obj).GetHashCode();
				long b = GetHashCode();

				long diff = a - b;
				if (diff < 0)
				{
					return 1;
				}
				else if (diff > 0)
				{
					return -1;
				}
			}

			return 0;
		}


	}

	/// <summary>
	/// Full 3-Axis SweepAndPrune using persistent updates.
	/// </summary>
	public class CollisionSystemPersistentSAP : CollisionSystem
	{
		private const int AddedObjectsBruteForceIsUsed = 250;

		// not needed anymore
		public List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();

		public List<SweepPoint> axis1 = new List<SweepPoint>();
		public List<SweepPoint> axis2 = new List<SweepPoint>();
		public List<SweepPoint> axis3 = new List<SweepPoint>();

		public HashList<OverlapPair> fullOverlaps = new HashList<OverlapPair>();

		Action<object> sortCallback;

		public CollisionSystemPersistentSAP()
		{
			sortCallback = new Action<object>(SortCallback);
		}

		#region Incoherent Update - Quicksort

		private int QuickSort(SweepPoint sweepPoint1, SweepPoint sweepPoint2)
		{
			FP val1 = sweepPoint1.Value;
			FP val2 = sweepPoint2.Value;

			if (val1 > val2) return 1;
			else if (val2 > val1) return -1;
			else return 0;
		}

		public List<IBroadphaseEntity> activeList = new List<IBroadphaseEntity>();

		private void DirtySortAxis(List<SweepPoint> axis)
		{
			axis.Sort(QuickSort);
			activeList.Clear();

			for (int i = 0; i < axis.Count; i++)
			{
				SweepPoint keyelement = axis[i];

				if (keyelement.Begin)
				{
					foreach (IBroadphaseEntity body in activeList)
					{
						if (
							// TODO: 优化效率
							// body.IsActive && keyelement.Body.IsActive &&
							CheckBoundingBoxes(body, keyelement.Body))
						{
							fullOverlaps.Add(new OverlapPair(body, keyelement.Body));
						}
					}

					activeList.Add(keyelement.Body);
				}
				else
				{
					activeList.Remove(keyelement.Body);
				}
			}
		}
		#endregion

		#region Coherent Update - Insertionsort

		private void SortAxis(List<SweepPoint> axis)
		{
			for (int j = 1; j < axis.Count; j++)
			{
				SweepPoint keyelement = axis[j];
				FP key = keyelement.Value;

				int i = j - 1;

				var isActive = keyelement.Body.IsActive;
				//if (keyelement.Body.IsActive)
				{
					while (i >= 0 && axis[i].Value > key)
					{
						SweepPoint swapper = axis[i];

						if (keyelement.Begin && !swapper.Begin)
						{
							if (
								isActive &&
								swapper.Body.IsActive &&
								CheckBoundingBoxes(swapper.Body, keyelement.Body))
							{
								lock (fullOverlaps) fullOverlaps.Add(new OverlapPair(swapper.Body, keyelement.Body));
							}
						}

						if (!keyelement.Begin && swapper.Begin)
						{
							lock (fullOverlaps) fullOverlaps.Remove(new OverlapPair(swapper.Body, keyelement.Body));
						}

						axis[i + 1] = swapper;
						i = i - 1;
					}
				}
				axis[i + 1] = keyelement;
			}
		}
		#endregion

		public int addCounter = 0;
		public override void AddEntity(IBroadphaseEntity body)
		{
			bodyList.Add(body);

			axis1.Add(new SweepPoint(body, true, 0)); axis1.Add(new SweepPoint(body, false, 0));
			axis2.Add(new SweepPoint(body, true, 1)); axis2.Add(new SweepPoint(body, false, 1));
			axis3.Add(new SweepPoint(body, true, 2)); axis3.Add(new SweepPoint(body, false, 2));

			addCounter++;
		}

		public Stack<OverlapPair> depricated = new Stack<OverlapPair>();
		public override bool RemoveEntity(IBroadphaseEntity body)
		{
			int count;

			count = 0;
			for (int i = 0; i < axis1.Count; i++)
			{ if (axis1[i].Body == body) { count++; axis1.RemoveAt(i); if (count == 2) break; i--; } }

			count = 0;
			for (int i = 0; i < axis2.Count; i++)
			{ if (axis2[i].Body == body) { count++; axis2.RemoveAt(i); if (count == 2) break; i--; } }

			count = 0;
			for (int i = 0; i < axis3.Count; i++)
			{ if (axis3[i].Body == body) { count++; axis3.RemoveAt(i); if (count == 2) break; i--; } }

			foreach (var pair in fullOverlaps) if (pair.Entity1 == body || pair.Entity2 == body) depricated.Push(pair);
			while (depricated.Count > 0) fullOverlaps.Remove(depricated.Pop());

			bodyList.Remove(body);

			return true;
		}

		public bool swapOrder = false;

		/// <summary>
		/// Tells the collisionsystem to check all bodies for collisions. Hook into the
		/// <see cref="CollisionSystem.PassedBroadphase"/>
		/// and <see cref="CollisionSystem.CollisionDetected"/> events to get the results.
		/// </summary>
		public override void Detect()
		{

			// TODO: 需要进一步排查根源
			//foreach (var bb in bodyList)
			//{
			//	var body = bb as RigidBody;
			//	if (body != null && body.IsActive && !body.gameObject.activeInHierarchy)
			//	{
			//		body.IsActive = false;
			//	}
			//}

			Profiler.BeginSample("Detect_Physics");

			if (addCounter > AddedObjectsBruteForceIsUsed)
			{
				fullOverlaps.Clear();

				DirtySortAxis(axis1);
				DirtySortAxis(axis2);
				DirtySortAxis(axis3);
			}
			else
			{
				sortCallback(axis1);
				sortCallback(axis2);
				sortCallback(axis3);
			}

			addCounter = 0;
			foreach (OverlapPair key in fullOverlaps)
			{
				if (this.CheckBothStaticNonKinematic(key.Entity1, key.Entity2))
				{
					continue;
				}

				// var b1 = key.Entity1 as IBody3D;
				// if (b1 != null && !b1.IsEnabled)
				// {
				// 	continue;
				// }
				// var b2 = key.Entity2 as IBody3D;
				// if (b2 != null && !b2.IsEnabled)
				// {
				// 	continue;
				// }

				// if (
				// 	((key.Entity1 as RigidBody).gameObject.name.Contains("footBall") &&
				// 	(key.Entity2 as RigidBody).gameObject.name.Contains("shuiqiu")) ||

				// 	((key.Entity2 as RigidBody).gameObject.name.Contains("footBall") &&
				// 	(key.Entity1 as RigidBody).gameObject.name.Contains("shuiqiu"))
				// 	)
				// {
				// 	UnityEngine.Debug.Log("lkwje");
				// }

				if (base.RaisePassedBroadphase(key.Entity1, key.Entity2))
				{
					// var ddsf = $"DetectPSAP:{(key.Entity1 as RigidBody).gameObject.name},{(key.Entity2 as RigidBody).gameObject.name}";
					Profiler.BeginSample($"DetectPSAP:{(key.Entity1 as RigidBody).gameObject.name},{(key.Entity2 as RigidBody).gameObject.name}");
					// if (swapOrder)
					// {
					// 	Detect(key.Entity1, key.Entity2);
					// }
					// else
					{
						Detect(key.Entity2, key.Entity1);
					}
					Profiler.EndSample();

					// swapOrder = !swapOrder;
				}
			}

			Profiler.EndSample();
		}

		private void SortCallback(object obj)
		{
			SortAxis(obj as List<SweepPoint>);
		}

		private void DetectCallback(object obj)
		{
			BroadphasePair pair = obj as BroadphasePair;
			base.Detect(pair.Entity1, pair.Entity2);
			BroadphasePair.Pool.GiveBack(pair);
		}

		// okay, people often say raycasting can be made faster using the sweep
		// and prune data. (sorted axis lists). That's only partly correct,
		// the method commented out below uses the non-uniform voxelgrid
		// approach (best approach known so far). 
		// but this is O(n) for long rays too and it even takes
		// more time. Just for short rays it has a small benefit.
		// it also gives the hits in order but the startposition problem
		// is unsolved - so it starts from outside the broadphase.

		#region Depricated
		//public void QueryRay(HashSet<IBroadphaseEntity> entities,JVector rayOrigin, JVector rayDirection)
		//{
		//    rayDirection.Normalize();

		//    int index1 = 0,index2 = 0,index3 = 0;

		//    if (rayDirection.X < FP.Zero) index1 = axis1.Count - 1;
		//    if (rayDirection.Y < FP.Zero) index2 = axis2.Count - 1;
		//    if (rayDirection.Z < FP.Zero) index3 = axis3.Count - 1;

		//    int steps = 0;

		//    while (true)
		//    {
		//        steps++;
		//        FP distance1 = (axis1[index1].Value - rayOrigin.X) / rayDirection.X;
		//        FP distance2 = (axis2[index2].Value - rayOrigin.Y) / rayDirection.Y;
		//        FP distance3 = (axis3[index3].Value - rayOrigin.Z) / rayDirection.Z;


		//        FP dist1 = Math.Abs(distance1);
		//        FP dist2 = Math.Abs(distance2);
		//        FP dist3 = Math.Abs(distance3);

		//        if (dist1 < dist2)
		//        {
		//            if (dist3 < dist1)
		//            {

		//                if (axis3[index3].Begin)
		//                {
		//                    if (axis3[index3].Body.BoundingBox.RayIntersect(rayOrigin, rayDirection)) entities.Add(axis3[index3].Body);
		//                }

		//                rayOrigin = rayOrigin + distance3 * rayDirection;

		//                index3 += (rayDirection.Z > FP.Zero) ? 1 : -1;
		//                if (index3 >= axis3.Count || index3 < 0) break;

		//            }
		//            else
		//            {

		//                if (axis1[index1].Begin)
		//                {
		//                    if (axis1[index1].Body.BoundingBox.RayIntersect(rayOrigin, rayDirection)) entities.Add(axis1[index1].Body);
		//                }

		//                rayOrigin = rayOrigin + distance1 * rayDirection;

		//                index1 += (rayDirection.X > FP.Zero) ? 1 : -1;
		//                if (index1 >= axis1.Count || index1 < 0) break;
		//            }
		//        }
		//        else
		//        {
		//            if (dist2 < dist3)
		//            {
		//                if (axis2[index2].Begin)
		//                {
		//                    if (axis2[index2].Body.BoundingBox.RayIntersect(rayOrigin, rayDirection)) entities.Add(axis2[index2].Body);
		//                }

		//                rayOrigin = rayOrigin + distance2 * rayDirection;

		//                index2 += (rayDirection.Y > FP.Zero) ? 1 : -1;
		//                if (index2 >= axis2.Count || index2 < 0) break;

		//            }
		//            else
		//            {

		//                if (axis3[index3].Begin)
		//                {
		//                    if (axis3[index3].Body.BoundingBox.RayIntersect(rayOrigin, rayDirection)) entities.Add(axis3[index3].Body);
		//                }

		//                rayOrigin = rayOrigin + distance3 * rayDirection;

		//                index3 += (rayDirection.Z > FP.Zero) ? 1 : -1;
		//                if (index3 >= axis3.Count || index3 < 0) break;
		//            }
		//        }
		//    }

		//    System.Diagnostics.Debug.WriteLine(steps);
		//}
		#endregion


		/// <summary>
		/// Sends a ray (definied by start and direction) through the scene (all bodies added).
		/// NOTE: For performance reasons terrain and trianglemeshshape aren't checked
		/// against rays (rays are of infinite length). They are checked against segments
		/// which start at rayOrigin and end in rayOrigin + rayDirection.
		/// </summary>
		#region public override bool Raycast(JVector rayOrigin, JVector rayDirection, out JVector normal,out FP fraction)
		public override bool Raycast(ref TSVector rayOrigin, ref TSVector rayDirection, RaycastCallback raycast, out RigidBody body, out TSVector normal, out FP fraction)
		{
			body = null; normal = TSVector.zero; fraction = FP.MaxValue;

			TSVector tempNormal; FP tempFraction;
			bool result = false;

			// TODO: This can be done better in CollisionSystemPersistenSAP
			foreach (IBroadphaseEntity e in bodyList)
			{
				if (e is SoftBody)
				{
					SoftBody softBody = e as SoftBody;
					foreach (RigidBody b in softBody.VertexBodies)
					{
						if (this.Raycast(b, ref rayOrigin, ref rayDirection, out tempNormal, out tempFraction))
						{
							if (tempFraction < fraction && (raycast == null || raycast(b, tempNormal, tempFraction)))
							{
								body = b;
								normal = tempNormal;
								fraction = tempFraction;
								result = true;
							}
						}
					}
				}
				else
				{
					RigidBody b = e as RigidBody;

					if (this.Raycast(b, ref rayOrigin, ref rayDirection, out tempNormal, out tempFraction))
					{
						if (tempFraction < fraction && (raycast == null || raycast(b, tempNormal, tempFraction)))
						{
							body = b;
							normal = tempNormal;
							fraction = tempFraction;
							result = true;
						}
					}
				}
			}

			return result;
		}
		#endregion

		/// <summary>
		/// Raycasts a single body. NOTE: For performance reasons terrain and trianglemeshshape aren't checked
		/// against rays (rays are of infinite length). They are checked against segments
		/// which start at rayOrigin and end in rayOrigin + rayDirection.
		/// </summary>
		#region public override bool Raycast(RigidBody body, JVector rayOrigin, JVector rayDirection, out JVector normal, out FP fraction)
		public override bool Raycast(RigidBody body, ref TSVector rayOrigin, ref TSVector rayDirection, out TSVector normal, out FP fraction)
		{

			// if (!body.IsActive)
			// {
			// 	fraction = FP.MaxValue; normal = TSVector.zero;
			// 	return false;
			// }

			if (!body.BoundingBox.RayIntersect(ref rayOrigin, ref rayDirection))
			{
				fraction = FP.MaxValue; normal = TSVector.zero;
				return false;
			}

			if (body.Shape is Multishape)
			{
				fraction = FP.MaxValue; normal = TSVector.zero;
				Multishape ms = (body.Shape as Multishape).RequestWorkingClone();

				TSVector tempNormal; FP tempFraction;
				bool multiShapeCollides = false;

				TSVector transformedOrigin; TSVector.Subtract(ref rayOrigin, ref body.position, out transformedOrigin);
				TSVector.Transform(ref transformedOrigin, ref body.invOrientation, out transformedOrigin);
				TSVector transformedDirection; TSVector.Transform(ref rayDirection, ref body.invOrientation, out transformedDirection);

				int msLength = ms.Prepare(ref transformedOrigin, ref transformedDirection);

				for (int i = 0; i < msLength; i++)
				{
					ms.SetCurrentShape(i);

					if (GJKCollide.Raycast(ms, ref body.orientation, ref body.invOrientation, ref body.position,
						ref rayOrigin, ref rayDirection, out tempFraction, out tempNormal))
					{
						if (tempFraction < fraction)
						{
							if (useTerrainNormal && ms is TerrainShape)
							{
								(ms as TerrainShape).CollisionNormal(out tempNormal);
								TSVector.Transform(ref tempNormal, ref body.orientation, out tempNormal);
								tempNormal.Negate();
							}
							else if (useTriangleMeshNormal && ms is TriangleMeshShape)
							{
								(ms as TriangleMeshShape).CollisionNormal(out tempNormal);
								TSVector.Transform(ref tempNormal, ref body.orientation, out tempNormal);
								tempNormal.Negate();
							}

							normal = tempNormal;
							fraction = tempFraction;
							multiShapeCollides = true;
						}
					}
				}

				ms.ReturnWorkingClone();
				return multiShapeCollides;
			}
			else
			{
				return (SpecificShapeCollision.Raycast(body, ref rayOrigin, ref rayDirection, out fraction, out normal));
			}


		}
		#endregion

		/// <summary>
		/// Sends a ray (definied by start and direction) through the scene (all bodies added).
		/// NOTE: For performance reasons terrain and trianglemeshshape aren't checked
		/// against rays (rays are of infinite length). They are checked against segments
		/// which start at rayOrigin and end in rayOrigin + rayDirection.
		/// </summary>
		public override bool Raycast(ref TSVector rayOrigin, ref TSVector rayDirection, RaycastCallback raycast, int layerMask, out RigidBody body, out TSVector normal, out FP fraction)
		{
			body = null;
			normal = TSVector.zero;
			fraction = FP.MaxValue;

			TSVector tempNormal; FP tempFraction;
			bool result = false;

			// TODO: This can be done better in CollisionSystemPersistenSAP
			foreach (IBroadphaseEntity e in bodyList)
			{
				if (e is SoftBody)
				{
					SoftBody softBody = e as SoftBody;
					foreach (RigidBody b in softBody.VertexBodies)
					{
						int bodyLayerMask = 1 << PhysicsManager.instance.GetBodyLayer(b);
						if ((layerMask & bodyLayerMask) != bodyLayerMask)
							continue;

						if (this.Raycast(b, ref rayOrigin, ref rayDirection, out tempNormal, out tempFraction))
						{
							if (tempFraction < fraction && (raycast == null || raycast(b, tempNormal, tempFraction)))
							{
								body = b;
								normal = tempNormal;
								fraction = tempFraction;
								result = true;
							}
						}
					}
				}
				else
				{
					RigidBody b = e as RigidBody;
					int bodyLayerMask = 1 << PhysicsManager.instance.GetBodyLayer(b);
					if ((layerMask & bodyLayerMask) != bodyLayerMask)
						continue;

					if (this.Raycast(b, ref rayOrigin, ref rayDirection, out tempNormal, out tempFraction))
					{
						if (tempFraction < fraction && (raycast == null || raycast(b, tempNormal, tempFraction)))
						{
							body = b;
							normal = tempNormal;
							fraction = tempFraction;
							result = true;
						}
					}
				}
			}

			return result;
		}

		private class IBroadphaseEntityXCompare : IComparer<IBroadphaseEntity>
		{
			public int Compare(IBroadphaseEntity body1, IBroadphaseEntity body2)
			{
				FP f = body1.BoundingBox.min.x - body2.BoundingBox.min.x;
				return (f < 0) ? -1 : (f > 0) ? 1 : 0;
			}
		}

		/// <summary>
		/// CheckRigidBody
		/// - body1 为传入的body      
		/// - body2 为碰撞到的body
		/// </summary>
		/// <param name="body"></param>
		/// <param name="layerMask"></param>
		/// <param name="queryTriggerInteraction"></param>
		/// <param name="handler"></param>
		/// <returns></returns>
		public override bool CheckRigidBody(RigidBody body, int layerMask, QueryTriggerInteraction queryTriggerInteraction, CollisionDetectedHandler handler = null)
		{
			return CheckRigidBody(body, layerMask, queryTriggerInteraction, handler, false);
		}
		// protected Dictionary<string, bool> cacheResult = new Dictionary<string, bool>();
		protected Dictionary<int, Dictionary<int, bool>> colKeyMap = new Dictionary<int, Dictionary<int, bool>>();
		protected bool checkFromCache(RigidBody body, IBroadphaseEntity ac, QueryTriggerInteraction queryTriggerInteraction, bool useCache)
		{
			var checkOnly = true;

			if (useCache)
			{
				var b2 = ac as RigidBody;
				if (b2 == null)
				{
					var detected = Detect(body, ac, queryTriggerInteraction, checkOnly);
					return detected;
				}
				else
				{
					// Serialize 0.003 ms
					// Profiler.BeginSample("Serialize");
					// var shape = body.Shape as CapsuleShape;
					// var key = $"{shape.ScaledRadius._serializedValue}_{shape.ScaledLength._serializedValue}_{body.position.x._serializedValue},{body.position.y._serializedValue},{body.position.z._serializedValue}_{b2.GetInstance()}_{b2.position.x._serializedValue},{b2.position.y._serializedValue},{b2.position.z._serializedValue}";
					// var key = $"{body.colKey}_{b2.colKey}";
					var k1 = body.colKey.GetHashCode();
					var k2 = b2.colKey.GetHashCode();
					var keyMap = colKeyMap[k1];
					if (keyMap.ContainsKey(k2))
					{
						return keyMap[k2];
					}
					else
					{
						var detected = Detect(body, ac, queryTriggerInteraction, checkOnly);
						keyMap.Add(k2, detected);
						return detected;
					}
				}
			}
			else
			{
				var detected = Detect(body, ac, queryTriggerInteraction, checkOnly);
				return detected;
			}
		}
		public virtual bool CheckRigidBody(RigidBody body, int layerMask, QueryTriggerInteraction queryTriggerInteraction, CollisionDetectedHandler handler = null,
			bool useCache = false, bool needDelayUpdate = false)
		{

			bool thisInactive = body.IsStaticOrInactive;

			if (handler != null)
			{
				this.checkCollisionDetected += handler;
			}

			bool checkOnly = true;

			bool isCollision = false;

			//Profiler.BeginSample("Sort");
			//var xComparer = new IBroadphaseEntityXCompare();
			//bodyList.Sort(xComparer);
			//Profiler.EndSample();

			var bodyBox = body.BoundingBox;
			FP xmin = bodyBox.min.x;
			FP xmax = bodyBox.max.x;

			FP ymin = bodyBox.min.y;
			FP ymax = bodyBox.max.y;

			FP zmin = bodyBox.min.z;
			FP zmax = bodyBox.max.z;

			{
				foreach (IBroadphaseEntity ac in bodyList)
				{
					// 过滤自身
					if (body == ac)
					{
						continue;
					}

					var acb = ac as IBody3D;
					if (!acb.IsActive)
					{
						continue;
					}

					// if (thisInactive && ac.IsStaticOrInactive)
					if (thisInactive && acb.TSIsStatic)
					{
						continue;
					}


					ref TSBBox acBox = ref ac.CachedBoundingBox;
					if (
						acBox.max.y < ymin || acBox.min.y > ymax ||
						acBox.max.z < zmin || acBox.min.z > zmax ||
						acBox.max.x < xmin || acBox.min.x > xmax
						)
					{
						continue;
					}

					// 检查层
					if (!this.IsOnLayer(ac as IBody, layerMask))
					{
						continue;
					}

					// 碰撞检测
					{
						// if (this.CheckBothStaticNonKinematic(body, ac))
						// {
						// 	continue;
						// }

						//if (base.RaisePassedBroadphase(body, ac))
						{
#if UNITY_EDITOR
							Profiler.BeginSample("Detect");
#endif
							var detected = checkFromCache(body, ac, queryTriggerInteraction, useCache);
#if UNITY_EDITOR
							Profiler.EndSample();
#endif
							if (detected)
							{
								isCollision = true;

								// 屏蔽重复检测
								if (checkOnly)
								{
									break;
								}
							}
						}
					}
				}
			}

			if (handler != null)
			{
				this.checkCollisionDetected -= handler;
			}

			return isCollision;
		}

		private Dictionary<FP, RigidBody> _bodyMap = new Dictionary<FP, RigidBody>();
		private TSVector _cacheforward = new TSVector();
		private TSVector _cachecenter = new TSVector();
		private TSMatrix TSMatrix_Identity = TSMatrix.Identity;

		/// <summary>
		/// CheckCapsule
		/// </summary>
		/// <param name="start">起始球心位置</param>
		/// <param name="end">终止球心位置</param>
		/// <param name="radius">半径</param>
		/// <param name="layerMask"></param>
		/// <param name="queryTriggerInteraction"></param>
		/// <returns></returns>
		public override bool CheckCapsule(ref TSVector start, ref TSVector end, FP radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction, bool useCache = false)
		{
			// CreateShapeParams 0.001ms
			// Profiler.BeginSample("CreateShapeParams");
			TSVector.Subtract(ref end, ref start, out _cacheforward);
			var key = radius;
			// Profiler.EndSample();

			// TODO: 此处只假定end==start
			if (!_bodyMap.ContainsKey(key))
			{
				Profiler.BeginSample("CreateRigidBody");
				// 创建自定义形体
				CapsuleShape shape = new CapsuleShape(_cacheforward.magnitude, radius);
				RigidBody body = new RigidBody(shape);
				body.colKey = $"{shape.ScaledRadius}_{shape.ScaledLength}";
				colKeyMap.Add(body.colKey.GetHashCode(), new Dictionary<int, bool>());
				_bodyMap.Add(key, body);

				body.SetOrientation(ref TSMatrix_Identity);
				body.SetPosition(ref start);
				body.Update(false);
				Profiler.EndSample();
			}

			// SetRigidBodyParams 0.002ms
			// Profiler.BeginSample("SetRigidBodyParams");
			var body1 = _bodyMap[key];

			if (end != start)
			{
				UnityEngine.Debug.LogError("invalid capsule params");
				TSMatrix orien;
				if (_cacheforward.IsZero())
				{
					orien = TSMatrix.Identity;
				}
				else
				{
					orien = TSMatrix.AngleAxis(FP.Zero, _cacheforward);
				}
				body1.SetOrientation(ref orien);

				TSVector.Add(ref end, ref start, out _cachecenter);
				TSVector.Divide(ref _cachecenter, 2, out _cachecenter);
				body1.SetPosition(ref _cachecenter);
			}
			else
			{
				body1.SetOrientation(ref TSMatrix_Identity);
				body1.SetPosition(ref start);
			}

			// Profiler.EndSample();

#if UNITY_EDITOR
			Profiler.BeginSample("UpdateRigidBody");
#endif
			body1.UpdateDiff(false);
#if UNITY_EDITOR
			Profiler.EndSample();
#endif

			var needDelayUpdate = true;

#if UNITY_EDITOR
			Profiler.BeginSample("CheckRigidBody");
#endif
			var ret = CheckRigidBody(body1, layerMask, queryTriggerInteraction, null, useCache, needDelayUpdate);
#if UNITY_EDITOR
			Profiler.EndSample();
#endif

			return ret;
		}

		/// <summary>
		/// CheckSphere
		/// </summary>
		public override bool CheckSphere(ref TSVector position, FP radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction, bool useCache = false)
		{

			// 创建自定义形体
			Shape shape = new SphereShape(radius);
			RigidBody body = new RigidBody(shape);
			body.Position = position;

			var ret = CheckRigidBody(body, layerMask, queryTriggerInteraction, null, useCache);

			return ret;
		}

		public override void ClearCollisionCache()
		{
			_bodyMap.Clear();
			colKeyMap.Clear();
		}
	}
}
