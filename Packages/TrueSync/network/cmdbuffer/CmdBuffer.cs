
using System.Collections.Generic;

namespace fsync
{
	using TPlayerId = System.String;

	/// <summary>
	/// 命令缓冲
	/// </summary>
	public class CmdBuffer
	{
		/// <summary>
		/// 所有玩家命令缓冲
		/// </summary>
		/// <typeparam name="PlayerCmdBuffer"></typeparam>
		/// <returns></returns>
		List<PlayerCmdBuffer> cmdBuffers = new List<PlayerCmdBuffer>();

		/// <summary>
		/// 获取或创建玩家命令缓冲
		/// </summary>
		/// <param name="playerId"></param>
		/// <returns></returns>
		public PlayerCmdBuffer GetOrCreatePlayerCmdBuffer(TPlayerId playerId)
		{
			var cmdBuffer = this.cmdBuffers.Find(cmdBuffer1 => cmdBuffer1.PlayerId == playerId);
			if (cmdBuffer == null)
			{
				cmdBuffer = new PlayerCmdBuffer().Init(playerId);
				this.cmdBuffers.Add(cmdBuffer);
			}
			return cmdBuffer;
		}

		/// <summary>
		/// 放入已排序指令
		/// </summary>
		/// <param name="cmd"></param>
		public void PutCmd(FrameCmd cmd)
		{
			var cmdBuffer = this.GetOrCreatePlayerCmdBuffer(cmd.PlayerId);
			cmdBuffer.PutCmd(cmd);
		}

		/// <summary>
		/// 弹出一帧所有指令
		/// </summary>
		/// <param name="frameCount"></param>
		/// <returns></returns>
		public List<FrameCmd> PopFrameCmds(int frameCount)
		{
			var cmds = new List<FrameCmd>();
			foreach (var cmdBuffer in this.cmdBuffers)
			{
				cmds.AddRange(cmdBuffer.PopFrameCmds(frameCount));
			}

			return cmds;
		}
	}
}
