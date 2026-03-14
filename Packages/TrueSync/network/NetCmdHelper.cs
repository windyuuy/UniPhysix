using System;
using System.IO;
using System.Collections.Generic;
using Google.Protobuf;

namespace fsync
{
	public class NetCmdHelper
	{
		public const uint FloatPrecision = 10000;
		public static long ConvFloatToInt64(float x)
		{
			return (long)(x * FloatPrecision);
		}
		public static float ConvInt64ToFloat(long x)
		{
			return (float)(((double)x) / FloatPrecision);
		}

		public static FrameCmd GenNopCmd()
		{
			return new FrameCmd();
		}

		/// <summary>
		/// 用于同步玩家信息的指令
		/// </summary>
		/// <param name="roleInfo"></param>
		/// <returns></returns>
		public static FrameCmd GenSyncUserInfoCmd(FrameCmd.TRoleInfo roleInfo)
		{
			var cmd = new FrameCmd();
			cmd.RoleInfo.Level = roleInfo.Level;
			cmd.RoleInfo.RoleId = roleInfo.RoleId;
			cmd.RoleInfo.Name = roleInfo.Name;
			cmd.RoleInfo.RoleConfigId = roleInfo.RoleConfigId;
			cmd.RoleInfo.CommonSkillId = roleInfo.CommonSkillId;
			cmd.CmdType = FrameCmd.TCmdType.SyncRoleInfo;
			return cmd;
		}

		/// <summary>
		/// 用于同步玩家信息的指令
		/// </summary>
		/// <param name="roleInfo"></param>
		/// <returns></returns>
		public static FrameCmd GenSyncUserReadyCmd(FrameCmd.TRoleInfo roleInfo)
		{
			var cmd = new FrameCmd();
			cmd.RoleInfo.Level = roleInfo.Level;
			cmd.RoleInfo.RoleId = roleInfo.RoleId;
			cmd.RoleInfo.Name = roleInfo.Name;
			cmd.RoleInfo.RoleConfigId = roleInfo.RoleConfigId;
			cmd.RoleInfo.CommonSkillId = roleInfo.CommonSkillId;
			cmd.RoleInfo.IsReady = true;
			cmd.CmdType = FrameCmd.TCmdType.SyncRoleInfo;
			return cmd;
		}

		public static FrameCmd GenMatchRoomTimeoutCmd()
		{
			var cmd = new FrameCmd();
			cmd.CmdType = FrameCmd.TCmdType.MatchRoomTimeout;
			return cmd;
		}

		/// <summary>
		/// 压缩帧命令
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static string EncodeFrameCmd(FrameCmd cmd)
		{
			var netCmd = new RoomProto.TRPGPlayerCmd()
			{
				ActorId = cmd.ActorId,
				PlayerId = cmd.PlayerId,
				CmdId = cmd.CmdId,
				CmdIndex = cmd.CmdIndex,
				CreateFrameCount = cmd.CreateFrameCount,
				CreateTime = cmd.CreateTime,
				FrameCount = cmd.FrameCount,
				CmdType = Convert.ToInt32(cmd.CmdType),
				Move = new RoomProto.TActorMoveInfo()
				{
					Angle = ConvFloatToInt64(cmd.Move.Angle),
					IsMoving = cmd.Move.IsMoving,
				},
				RoleInfo = new RoomProto.TRoleInfo()
				{
					Level = cmd.RoleInfo.Level,
					Name = cmd.RoleInfo.Name,
					RoleConfigId = cmd.RoleInfo.RoleConfigId,
					RoleId = cmd.RoleInfo.RoleId,
					IsReady = cmd.RoleInfo.IsReady,
					CommonSkillId = cmd.RoleInfo.CommonSkillId,
				},
			};

			var skills = new List<RoomProto.TSkill>();
			foreach (var skill in cmd.Skills)
			{
				netCmd.Skills.Add(new RoomProto.TSkill()
				{
					SkillType = Convert.ToInt64(skill.SkillType),
					FireAngleLen = ConvFloatToInt64(skill.FireAngleLen),
					FireAngle = ConvFloatToInt64(skill.FireAngle),
					ChargeTime = ConvFloatToInt64(skill.ChargeTime),
				});
			}

			byte[] cmdData = null;// = new byte[netCmd.CalculateSize()];
			//UnityEngine.Debug.LogWarning($"begin seri cmdData, cmd.Move.Angle:{cmd.Move.Angle}");
			//try
			//{
				using (MemoryStream stream = new MemoryStream())
				{
					//UnityEngine.Debug.LogWarning($"begin write cmdData");
					netCmd.WriteTo(stream);
					cmdData = stream.ToArray();
				}
			//}
			//catch (System.Exception e)
			//{
			//	UnityEngine.Debug.LogError("Serialize msg failed");
			//	UnityEngine.Debug.LogError(e);
			//}

			//if (cmdData == null)
			//{
			//	throw new Exception("Serialize msg failed");
			//}

			//UnityEngine.Debug.LogWarning($"begin base64 cmdData");
			var cmdStr = Convert.ToBase64String(cmdData);
			return cmdStr;
		}

		/// <summary>
		/// 帧命令解码
		/// </summary>
		/// <param name="cmdStr"></param>
		/// <returns></returns>
		public static FrameCmd DecodeFrameCmd(string cmdStr, com.unity.mgobe.Frame frameData, long receivedTime)
		{
			//UnityEngine.Debug.Log($"servertime: Id: {frameData.Id}, Time: {TimeUtil.ConvFrameIdToServerTime(frameData.Id)}");
			var cmdData = Convert.FromBase64String(cmdStr);
			var netCmd = RoomProto.TRPGPlayerCmd.Parser.ParseFrom(cmdData);
			var cmd = new FrameCmd()
			{
				ActorId = netCmd.ActorId,
				PlayerId = netCmd.PlayerId,
				CmdId = netCmd.CmdId,
				CmdIndex = netCmd.CmdIndex,
				CreateFrameCount = netCmd.CreateFrameCount,
				CreateTime = netCmd.CreateTime,
				FrameCount = (int)frameData.Id,
				CmdType = (FrameCmd.TCmdType)netCmd.CmdType,
				ServerTime = TimeUtil.ConvFrameIdToServerTime(frameData.Id),
				ReceivedTime = receivedTime,
				Move = new FrameCmd.TMove()
				{
					Angle = ConvInt64ToFloat(netCmd.Move.Angle),
					IsMoving = netCmd.Move.IsMoving,
				},
				RoleInfo = new FrameCmd.TRoleInfo()
				{
					Level = netCmd.RoleInfo.Level,
					Name = netCmd.RoleInfo.Name,
					RoleConfigId = netCmd.RoleInfo.RoleConfigId,
					RoleId = netCmd.RoleInfo.RoleId,
					IsReady = netCmd.RoleInfo.IsReady,
					CommonSkillId = netCmd.RoleInfo.CommonSkillId,
				},
			};

			var skills = new List<FrameCmd.TSkill>();
			foreach (var skill in netCmd.Skills)
			{
				skills.Add(new FrameCmd.TSkill()
				{
					SkillType = (SkillType)skill.SkillType,
					FireAngleLen = ConvInt64ToFloat(skill.FireAngleLen),
					FireAngle = ConvInt64ToFloat(skill.FireAngle),
					ChargeTime = ConvInt64ToFloat(skill.ChargeTime),
				});
			}
			cmd.Skills = skills;

			return cmd;
		}
	}

}