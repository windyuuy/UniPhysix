
using System;
using Debug = UnityEngine.Debug;
using TPlayerId = System.String;
using TSFloat = TrueSync.FP;
using TrueSync;

namespace fsync
{
	// using Timer = System.Timers.Timer;

	/// <summary>
	/// 本地指令处理器
	/// - 转发指令到服务器
	/// </summary>
	public class LocalCmdHandler
	{
		/// <summary>
		/// 帧命令采样缓存
		/// </summary>
		/// <returns></returns>
		protected FrameCmd localFrameCmd = new FrameCmd().Init();
		/// <summary>
		/// 是否存在有效的网络命令需要发送
		/// </summary>
		public bool IsCmdDirty = false;
		protected bool isLastMoving = false;

		/// <summary>
		/// 发送角色移动命令
		/// </summary>
		/// <param name="roleId"></param>
		/// <param name="isMoving"></param>
		/// <param name="angle"></param>
		public void SendRoleMoveCmd(TPlayerId playerId, bool isMoving, TSFloat angle)
		{
			if (isMoving != isLastMoving || isMoving)
			{
				isLastMoving = isMoving;
				IsCmdDirty = true;
			}

			localFrameCmd.Move.Angle = angle.LimitAccurate().AsFloat();
			localFrameCmd.Move.IsMoving = isMoving;
			localFrameCmd.ActorId = playerId;
			localFrameCmd.PlayerId = playerId.ToString();
		}

		public void SendFireCmd(TPlayerId playerId, SkillType skillType, TSFloat fireAngle, TSFloat fireAngleLen, TSFloat chargeTime)
		{
			IsCmdDirty = true;

			var fireSkill = localFrameCmd.Skills[0];
			fireSkill.SkillType = skillType;
			fireSkill.FireAngle = fireAngle.LimitAccurate().AsFloat();
			fireSkill.FireAngleLen = fireAngleLen.LimitAccurate().AsFloat();
			fireSkill.ChargeTime = chargeTime.LimitAccurate().AsFloat();
			localFrameCmd.ActorId = playerId;
			localFrameCmd.PlayerId = playerId.ToString();
		}

		public void SendUltiCmd(TPlayerId playerId, SkillType skillType, TSFloat fireAngle, TSFloat fireAngleLen, TSFloat chargeTime)
		{
			IsCmdDirty = true;

			var fireSkill = localFrameCmd.Skills[1];
			fireSkill.SkillType = skillType;
			fireSkill.FireAngle = fireAngle.LimitAccurate().AsFloat();
			fireSkill.FireAngleLen = fireAngleLen.LimitAccurate().AsFloat();
			fireSkill.ChargeTime = chargeTime.LimitAccurate().AsFloat();
			localFrameCmd.ActorId = playerId;
			localFrameCmd.PlayerId = playerId.ToString();
		}

		public void SendCSkillCmd(TPlayerId playerId, SkillType skillType, TSFloat fireAngle, TSFloat fireAngleLen, TSFloat chargeTime)
		{
			IsCmdDirty = true;

			var fireSkill = localFrameCmd.Skills[2];
			fireSkill.SkillType = skillType;
			fireSkill.FireAngle = fireAngle.LimitAccurate().AsFloat();
			fireSkill.FireAngleLen = fireAngleLen.LimitAccurate().AsFloat();
			fireSkill.ChargeTime = chargeTime.LimitAccurate().AsFloat();
			localFrameCmd.ActorId = playerId;
			localFrameCmd.PlayerId = playerId.ToString();
		}

		/// <summary>
		/// 清理帧命令采样缓存
		/// </summary>
		public void ResetCmd()
		{
			IsCmdDirty = false;

			var fireSkill = localFrameCmd.Skills[0];
			fireSkill.SkillType = SkillType.Invalid;
			fireSkill.FireAngle = 0;
			fireSkill.FireAngleLen = 0;
			fireSkill.ChargeTime = 0;

			var ultiSkill = localFrameCmd.Skills[1];
			ultiSkill.SkillType = SkillType.Invalid;
			ultiSkill.FireAngle = 0;
			ultiSkill.FireAngleLen = 0;
			ultiSkill.ChargeTime = 0;

			var cskillSkill = localFrameCmd.Skills[2];
			cskillSkill.SkillType = SkillType.Invalid;
			cskillSkill.FireAngle = 0;
			cskillSkill.FireAngleLen = 0;
			cskillSkill.ChargeTime = 0;
		}

		/// <summary>
		/// 发送一帧命令
		/// - 无论是否有命令, 每个逻辑帧都会发送帧命令
		/// </summary>
		public void SendFrameCmd()
		{
			if (!IsCmdDirty)
			{
				// 没有有效操作指令, 跳过
				return;
			}

			{
				var cmd = this.localFrameCmd;
				try
				{
					// 记录发送时间
					cmd.CreateTime = com.unity.mgobe.src.Util.SdkUtil.GetCurrentTimeMilliseconds();
					var cmdStr = NetCmdHelper.EncodeFrameCmd(cmd);
					GameRoomManager.SharedGameRoom.SendFrameCmd(cmdStr);
				}
				catch (Exception e)
				{
					Debug.LogError("序列化帧命令失败");
					Debug.LogError(e);
				}
				// 发送完命令需要清理帧命令缓存
				this.ResetCmd();
			}
		}

		public void SendSimulateFrame(ulong SimulateFrameId)
		{
			{
				var cmd = this.localFrameCmd;
				try
				{
					var cmdStr = NetCmdHelper.EncodeFrameCmd(cmd);
					//GameRoomManager.SharedGameRoom.SendFrameCmd(cmdStr);
					var frame = new com.unity.mgobe.RecvFrameBst()
					{
						Frame = new com.unity.mgobe.Frame()
						{
							Ext = new com.unity.mgobe.FrameExtInfo()
							{
								Seed = (ulong)(100000 * UnityEngine.Random.value),
							},
							Id = SimulateFrameId++,
							IsReplay = false,
							RoomId = "",
							Time = 0,
							Items =
							{
								new com.unity.mgobe.FrameItem()
								{
									PlayerId="",
									Timestamp=0,
									Data=cmdStr,
								}
							},
						},
					};
					FrameCmdHandlerManager.SharedNetCmdHandler.PostLocalFrame(frame);
				}
				catch (Exception e)
				{
					Debug.LogError("序列化帧命令失败");
					Debug.LogError(e);
				}
				// 发送完命令需要清理帧命令缓存
				this.ResetCmd();
			}
		}
	}

	public class FrameCmdHandlerManager
	{
		/// <summary>
		/// 共享命令处理器
		/// </summary>
		/// <returns></returns>
		public static readonly LocalCmdHandler SharedLocalCmdHandler = new LocalCmdHandler();
		public static readonly NetCmdHandler SharedNetCmdHandler = new NetCmdHandler();

		public static FrameCmdHandlerManager Inst = new FrameCmdHandlerManager();

		//protected Timer timer;
		FrameCmdHandlerManager()
		{
			//timer = new Timer();
			//timer.Elapsed += (s, e) =>
			//{
			//	if (!IsOnlineMode)
			//	{
			//		return;
			//	}

			//	SharedLocalCmdHandler.SendFrameCmd();
			//};
			//timer.AutoReset = true;
		}

		// TODO: 临时实现, 需要重构
		public static bool IsOnlineMode = false;

		public static void MarkOnlineMode()
        {
			IsOnlineMode = true;
		}

		/// <summary>
		/// 每帧发送用户输入, 并转发
		/// </summary>
		public void ScheduleSendUserInput(double interval = -1)
		{
			MonoScheduler.GetShared().UnScheduleFixed(NetDriver);
			MonoScheduler.GetShared().ScheduleFixed(NetDriver);
			IsOnlineMode = true;

			//if (interval <= 0)
			//{
			//	interval = (double)((ulong)1000 / (ulong)FrameSyncConfig.inst.NetFps);
			//}
			//timer.Interval = interval;
			//timer.Enabled = true;
		}

		protected float lastSendTime = 0;

		public void NetDriver()
		{
			if (!IsOnlineMode)
			{
				return;
			}

			var dt = UnityEngine.Time.time - lastSendTime;
			if (dt >= FrameSyncConfig.Inst.NetFDT)
			{
				lastSendTime = UnityEngine.Time.time;
				SharedLocalCmdHandler.SendFrameCmd();
			}
		}

		public static ulong SimulateFrameId = 0;
		/// <summary>
		/// 每帧发送用户输入, 并转发本地
		/// </summary>
		public void ScheduleLocalDriver(double interval = -1)
		{
			IsOnlineMode = false;

			SimulateFrameId = 0;
			SharedNetCmdHandler.ResetTimer();
			SharedNetCmdHandler.ClearFrame();
			MonoScheduler.GetShared().UnSchedule(LocalDriver);
			MonoScheduler.GetShared().Schedule(LocalDriver);
		}

		protected void LocalDriver()
		{
			if (IsOnlineMode)
			{
				return;
			}

			if (UnityEngine.Time.frameCount % 4 == 0)
			{
				SimulateFrameId++;
				SharedLocalCmdHandler.SendSimulateFrame(SimulateFrameId);
				SharedNetCmdHandler.HandleFramesData();
			}
		}

		/// <summary>
		/// 停止发送用户输入
		/// </summary>
		public void UnscheduleSendUserInput()
		{
			//timer.Enabled = false;
			MonoScheduler.GetShared().UnScheduleFixed(NetDriver);
			MonoScheduler.GetShared().UnSchedule(LocalDriver);
		}

		/// <summary>
		/// 准备帧同步指令处理器
		/// </summary>
		public void Prepare()
		{
			UnscheduleSendUserInput();
			// 准备网络处理器
			SharedNetCmdHandler.Prepare();
			// 收集玩家输入并转发
			this.ScheduleSendUserInput();
		}

		public void SetStartFrameCount(ulong frameCount)
		{
			SharedNetCmdHandler.SetStartFrameCount(frameCount);
		}

		public void Clear()
		{
			UnscheduleSendUserInput();
			SharedNetCmdHandler.Clear();
		}
	}
}