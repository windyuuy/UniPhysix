
using System.Collections.Generic;

namespace fsync
{
	using TActorId = System.String;
	using TRoleId = System.String;
	using TPlayerId = System.String;
	using number = System.Double;
	using boolean = System.Boolean;

	/// <summary>
	/// 网络帧指令
	/// </summary>
	public class FrameCmd
	{
		/// <summary>
		/// 命令类型信息
		/// </summary>
		public enum TCmdType
		{
			/// <summary>
			/// 角色指令
			/// </summary>
			RoleCmd = 1,
			/// <summary>
			/// 略过指令
			/// </summary>
			Pass = 2,
			/// <summary>
			/// 同步玩家当前角色信息
			/// </summary>
			SyncRoleInfo = 3,

			/// <summary>
			/// 匹配房间超时
			/// </summary>
			MatchRoomTimeout = 4,
		};

		/// <summary>
		/// 命令路由信息
		/// </summary>
		public enum TRoute
		{
			/// <summary>
			/// 网络指令
			/// </summary>
			Net = 1,
			/// <summary>
			/// 本地指令
			/// </summary>
			Local = 2,
		}

		/// <summary>
		/// 角色移动信息
		/// </summary>
		public class TMove
		{
			/// <summary>
			/// 移动方向 (包含标量信息)
			/// </summary>
			public float Angle = 0;

			/// <summary>
			/// 是否正在移动
			/// </summary>
			public bool IsMoving = false;
		}
		/// <summary>
		/// 角色技能信息
		/// - 此处简化封装, 所有技能公用一个模型, 自取字段
		/// </summary>
		public class TSkill
		{
			/// <summary>
			/// 技能类型
			/// - 用于唯一确定角色当前释放那个技能
			/// </summary>
			public SkillType SkillType = SkillType.Invalid;
			/// <summary>
			/// 攻击角度
			/// </summary>
			public float FireAngle = 0;
			/// <summary>
			/// 攻击距离
			/// </summary>
			public float FireAngleLen = 0;
			/// <summary>
			/// 瞄准时长
			/// </summary>
			public float ChargeTime = 0;
		}

		/// <summary>
		/// 需要同步的角色信息
		/// </summary>
		public class TRoleInfo
		{
			/// <summary>
			/// 角色等级
			/// </summary>
			public int Level = 0;
			/// <summary>
			/// 角色ID(暂时无用)
			/// </summary>
			public TRoleId RoleId = "";
			/// <summary>
			/// 角色名称
			/// </summary>
			public string Name = "";
			/// <summary>
			/// 角色配置ID
			/// </summary>
			public int RoleConfigId = 0;
			/// <summary>
			/// 角色是否就绪
			/// </summary>
			public bool IsReady = false;
			/// <summary>
			/// 准备就绪的帧序
			/// </summary>
			public long ReadyFrameCount = -1;
			/// <summary>
			/// 选择的召唤师技能
			/// </summary>
			public int CommonSkillId = 0;
		}


		/// <summary>
		/// 声明命令类型
		/// </summary>
		public TCmdType CmdType;

		/// <summary>
		/// 命令id
		/// - 保证全局唯一
		/// </summary>
		public int CmdId;

		/// <summary>
		/// 命令索引
		/// - 当前player的指令按顺序编号
		/// </summary>
		public int CmdIndex;

		/// <summary>
		/// 是否需要触发同步
		/// </summary>
		public boolean NeedSync;

		/// <summary>
		/// 命令路由
		/// - 来自网络或本地等
		/// </summary>
		public TRoute Route;

		/// <summary>
		/// 角色ID
		/// - 如果每个玩家只能同时操作一名角色, 并且此字段不为空, 那么当前指令全部应用此roleid
		/// </summary>
		public TPlayerId PlayerId = "";

		/// <summary>
		/// 操作的对象ID
		/// </summary>
		public string ActorId = "";

		/// <summary>
		/// 命令创建时间(本地时间)
		/// </summary>
		public long CreateTime;

		/// <summary>
		/// 命令创建帧序(本地帧序)
		/// </summary>
		public int CreateFrameCount;

		/// <summary>
		/// 命令网络帧序(以服务器接收时标记的为准)
		/// </summary>
		public int FrameCount;

		/// <summary>
		/// 命令接收时间(本地时间)
		/// </summary>
		public long ReceivedTime;

		/// <summary>
		/// 服务器接收时间点
		/// </summary>
		public long ServerTime = 0;

		/// <summary>
		/// 角色移动信息
		/// </summary>
		/// <returns></returns>
		public TMove Move = new TMove();

		/// <summary>
		/// 指定角色释放的技能列表
		/// </summary>
		public List<TSkill> Skills = new List<TSkill>();

		public TRoleInfo RoleInfo = new TRoleInfo();

		public FrameCmd Init()
		{
			{
				var fireSkill = new TSkill();
				this.Skills.Add(fireSkill);
			}
			{
				var ultiSkill = new TSkill();
				this.Skills.Add(ultiSkill);
			}
			{
				var cskillSkill = new TSkill();
				this.Skills.Add(cskillSkill);
			}
			return this;
		}
	}
}
