
using System;
using System.Collections.Generic;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif


namespace TrueSync
{
	public partial class TSTransform
	{
		static int lastUpdateFrame = -2;
		// static TSTransform[] _cachedTSTransforms;
		static List<TSTransform> _cachedTSTransforms = new List<TSTransform>();
		static Dictionary<UnityEngine.GameObject, TSTransform> gameObjectTSMap = new Dictionary<UnityEngine.GameObject, TSTransform>();
		public static void markNewTSTransform(TSTransform trans)
		{
			if (!_cachedTSTransforms.Contains(trans))
			{
				_cachedTSTransforms.Add(trans);
			}
			gameObjectTSMap[trans.gameObject] = trans;
		}
		private static void markDeleteTSTransform(TSTransform trans)
		{
			_cachedTSTransforms.Remove(trans);
			gameObjectTSMap.Remove(trans.gameObject);
		}
		public static void Clear()
		{
			_cachedTSTransforms.Clear();
			gameObjectTSMap.Clear();
		}
		public static TSTransform GetTSTransform(UnityEngine.GameObject gameObject)
		{
			gameObjectTSMap.TryGetValue(gameObject, out var trans);
			return trans;
		}
		public static TSTransform ReGetTSTransform(UnityEngine.GameObject gameObject)
		{
			gameObjectTSMap.TryGetValue(gameObject, out var trans);
			if (trans == null)
			{
				trans = gameObject.GetComponent<TSTransform>();
				if (trans != null)
				{
					gameObjectTSMap[trans.gameObject] = trans;
				}
			}
			return trans;
		}
		private static void _foreachLeavesFromRoot(TSTransform trans, ref System.Action<TSTransform> handler)
		{
			if (trans == null || !trans.isActiveAndEnabled)
			{
				return;
			}

			// 优先遍历父节点
			handler(trans);

			if (trans.tsChildren.Count > 0)
			{
				foreach (var child in trans.tsChildren)
				{
					_foreachLeavesFromRoot(child, ref handler);
				}
			}
		}

		public static void ForeachLeavesFromRoot(System.Action<TSTransform> handler)
		{
			foreach (var trans in _cachedTSTransforms)
			{
				if (trans != null && trans.tsParent == null && trans.isActiveAndEnabled)
				{
					_foreachLeavesFromRoot(trans, ref handler);
				}
			}
		}
	}
}
