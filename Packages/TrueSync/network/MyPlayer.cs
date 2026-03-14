
namespace fsync
{
	public class MyPlayer
	{
		public static bool IsMyPlayer(string playerId, UnityEngine.MonoBehaviour comp)
		{
			if (FrameCmdHandlerManager.IsOnlineMode)
			{
				return playerId == fsync.GameRoomManager.SharedGameRoom.MyPlayerId;
			}
			else
			{
				return comp.gameObject.CompareTag("Player");
			}
		}

		public static string GetOID(string playerId, UnityEngine.MonoBehaviour comp)
		{
			if (FrameCmdHandlerManager.IsOnlineMode)
			{
				return playerId;
			}
			else
			{
				return comp.GetInstanceID().ToString();
			}
		}

		public static string GetPlayerID()
		{
			return fsync.GameRoomManager.SharedGameRoom.MyPlayerId;
		}

		/// <summary>
		/// 是否在线战斗(在线战斗/纯本地战斗)
		/// </summary>
		public static bool IsOnlineMode => FrameCmdHandlerManager.IsOnlineMode;

		/// <summary>
		/// 游戏进程正在进行
		/// </summary>
		public static bool IsRunning = false;

	}
}
