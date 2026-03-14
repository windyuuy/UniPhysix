
using System.Collections.Generic;

namespace fsync
{

	using TActorId = System.String;
	using TRoleId = System.String;
	using TPlayerId = System.String;

	/// <summary>
	/// 单个玩家命令缓冲
	/// </summary>
	public class PlayerCmdBuffer
	{
		/// <summary>
		/// 命令缓存列表
		/// </summary>
		/// <typeparam name="FrameCmd"></typeparam>
		/// <returns></returns>
		protected List<FrameCmd> cmds = new List<FrameCmd>();

		/// <summary>
		/// 名称(调试用)
		/// </summary>
		public string Name = "";

		/// <summary>
		/// 玩家ID
		/// </summary>
		public TPlayerId PlayerId = "";

		public PlayerCmdBuffer Init(TPlayerId playerId)
		{
			this.PlayerId = playerId;
			return this;
		}

		/// <summary>
		/// 当前帧序
		/// </summary>
		protected ulong curFrameCount = 0;
		/// <summary>
		/// 当前已执行命令序号
		/// </summary>
		protected ulong curOutdateCmdIndex = 0;

		/// <summary>
		/// 放入已排序指令
		/// </summary>
		/// <param name="cmd"></param>
		public void PutCmd(FrameCmd cmd)
		{
			this.cmds.Add(cmd);
		}

		/// <summary>
		/// 弹出一帧所有指令
		/// </summary>
		/// <param name="frameCount"></param>
		/// <returns></returns>
		public List<FrameCmd> PopFrameCmds(int frameCount)
		{
			var frameCmds = this.cmds.FindAll(cmd => cmd.FrameCount == frameCount);
			return frameCmds;
		}
	}
}
