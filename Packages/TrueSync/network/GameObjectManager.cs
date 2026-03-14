using System;
using System.Collections.Generic;
using TPlayerId = System.String;
using System.Linq;

namespace fsync
{
	public interface IWithGenId
	{
		int genIndex { get; set; }
	}
	
	public class NameSpaceManager
	{
		public static Dictionary<string, int> myNamespace = new Dictionary<TPlayerId, int>();
		public static int namespaceAcc = 0;
		public static int GetStrIndex(string name)
		{
			if (!myNamespace.ContainsKey(name))
			{
				myNamespace.Add(name, namespaceAcc++);
			}
			return myNamespace[name];
		}

		public static int GetUID(IWithGenId comp)
		{
			if (comp.genIndex == 0)
			{
				comp.genIndex = GenIndex(comp);
			}

			var name = comp.GetType().Name;
			var nameIndex = GetStrIndex(name);
			var id = nameIndex * 1000 + comp.genIndex;
			return id;
		}

		protected static Dictionary<string, int> uidAccMap = new Dictionary<TPlayerId, int>();
		public static int GenIndex(Object target)
		{
			var key = target.GetType().Name;
			if (!uidAccMap.ContainsKey(key))
			{
				uidAccMap[key] = 1;
			}

			var id = uidAccMap[key]++;
			return id;
		}
	}

	/// <summary>
	/// 通过ID管理游戏对象
	/// </summary>
	public class GameObjectManager
	{
		public static readonly GameObjectManager Inst = new GameObjectManager();

		/// <summary>
		/// 对象列表
		/// - key: LocalOID
		/// - value: wrf(gameObject)
		/// </summary>
		/// <returns></returns>
		protected Dictionary<int, IFrameSyncUpdate> gameObjects = new Dictionary<int, IFrameSyncUpdate>();

		public TNetTimer NetTimer = new TNetTimer();
		public TNetTimer ServerTimer = new TNetTimer();

		/// <summary>
		/// 注册游戏对象
		/// </summary>
		/// <param name="gameObject"></param>
		public void Add(IFrameSyncUpdate gameObject)
		{
			gameObject.NetTimer = this.NetTimer;

			var rf = gameObject;
			this.gameObjects.Add(gameObject.LocalOID, rf);
		}

		/// <summary>
		/// 移除对象注册
		/// </summary>
		/// <param name="gameObject"></param>
		public void Remove(IFrameSyncUpdate gameObject)
		{
			this.gameObjects.Remove(gameObject.LocalOID);
		}

		public void Clear()
		{
			this.gameObjects.Clear();
		}

		/// <summary>
		/// 按id获取对象
		/// </summary>
		/// <param name="oid"></param>
		/// <returns></returns>
		public IFrameSyncUpdate GetOnlineTarget(TPlayerId oid)
		{
            if (oid == "")
            {
				return null;
            }
			foreach (var p in this.gameObjects)
			{
				var rf = p.Value;
				// rf.TryGetTarget(out var gameObject);
				var gameObject = rf;
				if (gameObject != null)
				{
					if (gameObject.OID == oid)
					{
						return gameObject;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// 遍历所有托管对象
		/// </summary>
		/// <param name="action"></param>
		public void ForeachAll(Action<IFrameSyncUpdate> action)
		{
			var objs = (from ele in this.gameObjects.Values select ele).ToList();
			// 			objs.Sort((o1, o2) =>
			// 			{
			// 				if (o1 == null)
			// 				{
			// 					return 1;
			// 				}
			// 				if (o2 == null)
			// 				{
			// 					return -1;
			// 				}
			// 				if (o1.LocalOID == o2.LocalOID)
			// 				{
			// 					return 1;
			// 				}
			// #if UNITY_EDITOR
			// 				try
			// 				{
			// #endif
			// 					return o1.LocalOID > o2.LocalOID ? 1 : -1;
			// #if UNITY_EDITOR
			// 				}
			// 				catch (Exception e)
			// 				{
			// 					throw e;
			// 				}
			// #endif
			// 			});
			foreach (var p in objs)
			{
				// p.Value.TryGetTarget(out var gameObject);
				var gameObject = p;
				if (gameObject != null)
				{
					action(gameObject);
				}
			}
		}

	}

}
