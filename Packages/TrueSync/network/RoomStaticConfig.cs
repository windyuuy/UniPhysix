
using System.Collections.Generic;
namespace fsync
{
	/// <summary>
	/// 房间匹配模式
	/// </summary>
	public class RoomMatchMode
	{
		public ulong MaxPlayers;
		public string RoomType;
	}

	/// <summary>
	/// 房间服务器配置
	/// </summary>
	public class RoomStaticConfig
	{
		Dictionary<int, RoomMatchMode> matchModes = new Dictionary<int, RoomMatchMode>();

		public RoomStaticConfig Init()
		{
			this.matchModes.Add(1, new RoomMatchMode()
			{
				MaxPlayers = 1,
				RoomType = "1V1",
			});
			this.matchModes.Add(2, new RoomMatchMode()
			{
				MaxPlayers = 2,
				RoomType = "2V2",
			});
            for (int i = 3; i < 10; i++)
            {
                this.matchModes.Add(i, new RoomMatchMode()
                {
                    MaxPlayers = (ulong)i,
                    RoomType = $"{i}V{i}",
                });
            }
            return this;
		}

		public static RoomStaticConfig Inst = new RoomStaticConfig().Init();

		public RoomMatchMode GetMatchMode(int count)
		{
            if (!this.matchModes.ContainsKey(count))
            {
				UnityEngine.Debug.LogError($"unsupport match count: {count}");
            }
			return this.matchModes[count];
		}
	}
}