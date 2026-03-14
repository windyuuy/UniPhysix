using System.Threading.Tasks;
using System.Collections;

namespace fsync
{
	/// <summary>
	/// 游戏房间实例管理
	/// </summary>
	public class GameRoomManager
	{
		public static GameRoom SharedGameRoom = null;

		/// <summary>
		/// 创建房间
		/// </summary>
		public static void RecreateGameRoom()
		{
			SharedGameRoom = new GameRoom();
		}

		/// <summary>
		/// 销毁房间
		/// </summary>
		public static void DestroyGameRoom()
		{
			SharedGameRoom.Dispose();
			SharedGameRoom = null;
		}

		/// <summary>
		/// 禁用物理自动模拟
		/// </summary>
		public static void DisablePhysicsAutoSimulation()
		{
			// 禁用物理自动模拟
			UnityEngine.Physics.autoSimulation = false;
			TrueSync.TSPhysics.autoSimulation = false;
		}

		public static void ExitGame()
		{
			// 禁用物理自动模拟
			UnityEngine.Physics.autoSimulation = true;
			// TrueSync.TSPhysics.autoSimulation = true;
			FrameCmdHandlerManager.Inst.Clear();
            if (SharedGameRoom != null)
            {
				SharedGameRoom.Clear();
			}
			GameObjectManager.Inst.Clear();
		}

		/// <summary>
		/// 开始游戏
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public static async Task StartGame(MatchRoomPara info)
		{
			DisablePhysicsAutoSimulation();
			GameObjectManager.Inst.Clear();
			TrueSync.TSPhysics.Clear();

			// 新建游戏房间客户端
			fsync.GameRoomManager.RecreateGameRoom();
			// 匹配游戏
			fsync.MatchGameRoomResult result = null;
			for (var i = 0; i < 5000; i++)
			{
				if (i > 0)
				{
					UnityEngine.Debug.Log($"匹配失败, 尝试重新匹配, 第{i}次.");
				}
				try
				{
					fsync.GameRoomManager.SharedGameRoom.Clear();
					result = await fsync.GameRoomManager.SharedGameRoom.MatchGameRoom(info);
					if (result.IsOk)
					{
						break;
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError("匹配异常:");
					UnityEngine.Debug.LogError(e);
				}
				await AsyncTimer.Delay(1000);
			}
			if (!result.IsOk)
			{
				fsync.GameRoomManager.SharedGameRoom.Clear();
				throw new System.Exception("匹配房间失败");
			}

			FrameCmdHandlerManager.MarkOnlineMode();

			// 等待开始游戏
			await fsync.GameRoomManager.SharedGameRoom.StartFight();

		}

		/// <summary>
		/// 监听所有玩家就绪
		/// </summary>
		/// <param name="call"></param>
		public static void ListenAllReady(System.Action<WaitReadyResult> call)
		{
			fsync.GameRoomManager.SharedGameRoom.WaitAllMembersReadyToFight().ContinueWith((result) =>
			{
				call(result.Result);
			});
		}

		public static IEnumerator _NotifyAndWaitAllMembersReady(float delayS)
		{
			fsync.GameRoomManager.SharedGameRoom.NotifyReady();

			WaitReadyResult readyResult = null;
			yield return AsyncTask.ToEnumerable(fsync.GameRoomManager.SharedGameRoom.WaitAllMembersReadyToFight()
				.ContinueWith((result) =>
				{
					readyResult = result.Result;
				}));

			{
				fsync.FrameCmdHandlerManager.Inst.SetStartFrameCount((ulong)readyResult.FrameCount + (ulong)(delayS * FrameSyncConfig.Inst.NetFps));
				// 准备帧同步指令处理器
				fsync.FrameCmdHandlerManager.Inst.Prepare();
				// 开启帧事件处理
				fsync.GameRoomManager.StartFrameSyncHandler();
			}
		}

		/// <summary>
		/// 等待所有玩家准备就绪
		/// </summary>
		/// <param name="delayS">延迟开始时长(秒)</param>
		public static void NotifyAndWaitAllMembersReady(float delayS)
		{
			fsync.MonoScheduler.GetShared().StartCoroutine(fsync.GameRoomManager._NotifyAndWaitAllMembersReady(delayS));
		}

		/// <summary>
		/// 开始处理帧同步事件
		/// - 如果因为手机性能问题, 出现有的玩家较早进房间, 可以添加界面遮挡, 并延迟调用此函数, 屏蔽用户联机操作
		/// </summary>
		public static void StartFrameSyncHandler()
		{
			FrameCmdHandlerManager.SharedNetCmdHandler.StartFrameSyncHandler();
		}

		/// <summary>
		/// 清理游戏房间状态, 退出游戏时需要调用
		/// </summary>
		public static void ClearGameRoom()
		{
			fsync.GameRoomManager.SharedGameRoom.Clear();
		}
	}
}
