using System;
using System.Collections.Generic;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace TrueSync
{

	/**
    *  @brief Helpers for 3D physics.
    **/
	public class TSPhysics
	{

		public static bool Raycast(ref TSVector rayOrigin, ref TSVector rayDirection, out TSRaycastHit hit, FP maxDistance,
			int layerMask = UnityEngine.Physics.DefaultRaycastLayers,
			TrueSync.Physics3D.QueryTriggerInteraction queryTriggerInteraction = Physics3D.QueryTriggerInteraction.UseGlobal
			)
		{
			InitForce();
			return PhysicsWorldManager.instance.Raycast(ref rayOrigin, ref rayDirection, out hit, maxDistance, layerMask);
		}

		public static bool Raycast(Physics3D.RigidBody body, ref TSRay ray, out TSRaycastHit hitInfo, FP maxDistance)
		{
			return PhysicsWorldManager.instance.Raycast(body, ref ray, out hitInfo, maxDistance);
		}

		public static bool Linecast(ref TSVector start, ref TSVector end, int layerMask)
		{
			var direction = end - start;
			var maxDistance=direction.NormalizeR();
			var ray = new TSRay(ref start,ref direction);
			if(PhysicsWorldManager.instance.Raycast(ref ray, out var hit, maxDistance, layerMask))
			{
				if (hit.distance <= maxDistance)
					return true;
			}
			return false;
		}

		public static void Clear()
		{
            // 清除碰撞缓存
            if (PhysicsWorldManager.instance != null)
            {
				PhysicsWorldManager.instance.ClearCollisionCache();
			}
			// 清除组件缓存
			TSTransform.Clear();
		}

		// static int frameAcc = 0;
		/// <summary>
		/// 手动调度物理模拟
		/// </summary>
		/// <param name="step"></param>
		public static void Simulate(FP step)
		{
			Profiler.BeginSample("TSPhysic.Simulate");

			syncDirtyDelayedTransform();

			PhysicsWorldManager.instance.UpdateStepForce(step);

			syncDirtyPhysicsTransform();
			syncDirtyDelayedTransform();

			Profiler.EndSample();

			// frameAcc++;
			// UnityEngine.Debug.Log($"simulateframe: sf:{frameAcc}, uif:{UnityEngine.Time.frameCount}, time:{UnityEngine.Time.time}");
		}

		/// <summary>
		/// 同步ui 中的方位数据, 避免 ui系统中的方位数值反哺时造成ts系统中的方位数值错误.
		/// - 例如画面拟合后
		/// </summary>
		public static void SyncView()
		{
			TSTransform.ForeachLeavesFromRoot((trans) =>
			{
				trans.SyncView();
			});
		}

		/// <summary>
		/// 同步TSTransform中基于物理模拟而失去同步的局部方位状态
		/// </summary>
		private static void syncDirtyPhysicsTransform()
		{
			Profiler.BeginSample("syncDirtyPhysicsTransform");
			TSTransform.ForeachLeavesFromRoot((trans) =>
			{
#if UNITY_EDITOR
				Profiler.BeginSample("trans.SyncDirtyPhysicsTransform");
#endif
				trans.SyncDirtyPhysicsTransform();
#if UNITY_EDITOR
				Profiler.EndSample();
#endif
			});
			Profiler.EndSample();
		}

		/// <summary>
		/// 同步TSTransform中基于物理模拟而失去同步的局部方位状态
		/// </summary>
		internal static void syncDirtyDelayedTransform()
		{
			Profiler.BeginSample("syncDirtyDelayedTransform");
			TSTransform.ForeachLeavesFromRoot((trans) =>
			{
#if UNITY_EDITOR
				Profiler.BeginSample($"UpdateDirtyTransform_{trans.name}");
#endif
				trans.UpdateDirtyTransform();
#if UNITY_EDITOR
				Profiler.EndSample();
#endif
			});
			Profiler.EndSample();
		}

		/// <summary>
		/// 自动调度物理模拟
		/// </summary>
		public static bool autoSimulation
		{
			get => PhysicsWorldManager.autoSimulation;
			set => PhysicsWorldManager.autoSimulation = value;
		}

		public static void InitForce()
		{
			if (PhysicsWorldManager.instance == null)
			{
				var tsmanager = UnityEngine.GameObject.Find("TrueSyncManager").GetComponent<TrueSyncManager>();
				tsmanager._Awake();
			}
		}

		public const int DefaultLayerMask = -5;

		public static bool CheckRigidBody(TrueSync.Physics3D.RigidBody body, int layerMask = DefaultLayerMask, TrueSync.Physics3D.QueryTriggerInteraction queryTriggerInteraction = Physics3D.QueryTriggerInteraction.UseGlobal, TrueSync.Physics3D.CollisionDetectedHandler handler = null)
		{
			InitForce();
			return PhysicsWorldManager.instance.CheckRigidBody(body, layerMask, queryTriggerInteraction, handler);
		}

		public static bool CheckCapsule(ref TSVector start, ref TSVector end, FP radius,
		 int layerMask = DefaultLayerMask, Physics3D.QueryTriggerInteraction queryTriggerInteraction = Physics3D.QueryTriggerInteraction.UseGlobal, bool useCache = false)
		{
			InitForce();

			return PhysicsWorldManager.instance.CheckCapsule(ref start, ref end, radius, layerMask, queryTriggerInteraction, useCache);
		}


		public static bool CheckSphere(ref TSVector position, FP radius,
		 int layerMask = DefaultLayerMask, Physics3D.QueryTriggerInteraction queryTriggerInteraction = Physics3D.QueryTriggerInteraction.UseGlobal, bool useCache = false)
		{
			InitForce();
			return PhysicsWorldManager.instance.CheckSphere(ref position, radius, layerMask, queryTriggerInteraction, useCache);
		}
	}

}