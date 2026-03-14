using System.Text.RegularExpressions;

using UnityEngine;
using com.unity.mgobe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TPlayerId = System.String;
using System.Linq;

namespace fsync
{

	public class PlayerInfoPara
	{
		public string OpenId { get; set; }
		public string Name { get; set; }

		public int PlayerId { get; set; }
		public ulong CustomPlayerStatus { get; set; }

		public string CustomProfile { get; set; }
		public List<MatchAttribute> MatchAttributes { get; set; }
	}

	public class MatchRoomPara
	{
		public PlayerInfoPara PlayerInfo { get; set; }

		public fsync.FrameCmd.TRoleInfo RoleInfo { get; set; }

		public ulong MaxPlayers { get; set; }

		public string RoomType { get; set; }
		public string MatchCode { get; set; }
	}


	public enum GameMatchMode
    {
		MatchPlayers=0,
		MatchGroup=1,
		MatchRoom=2,
	}

	/// <summary>
	/// 房间基础配置
	/// </summary>
	public class GameRoomConfig
	{
		public int MaxPlayers = 1;
		public string RoomType = "1V1";
		public GameMatchMode MatchMode = GameMatchMode.MatchPlayers;
	}

	/// <summary>
	/// 房间信息
	/// </summary>
	public class RoomInfo
	{
		/// <summary>
		/// 随机种子
		/// </summary>
		public ulong RandomSeed;
		/// <summary>
		/// 在线玩家数量
		/// </summary>
		public int OnlinePlayersCount=0;
		/// <summary>
		/// AI玩家数量
		/// </summary>
		public int AIPlayerCount = 0;
	}

	/// <summary>
	/// 房间成员信息
	/// </summary>
	public class PlayerInfo
	{
		/// <summary>
		/// 是否有效的角色信息
		/// </summary>
		/// <value></value>
		public bool IsValid { get { return this.Id != "UnKnown"; } }
		/// <summary>
		/// 角色昵称
		/// </summary>
		public string NickName = "UnKnown";
		/// <summary>
		/// 服务器分配的角色ID
		/// </summary>
		public string Id = "UnKnown";
		/// <summary>
		/// 自定义的玩家ID
		/// </summary>
		public string PlayerId = "";
		/// <summary>
		/// 头像
		/// </summary>
		public string CustomProfile = "";
		/// <summary>
		/// 是否自身
		/// </summary>
		public bool IsSelf = false;

		/// <summary>
		/// 队伍ID
		/// </summary>
		public string TeamId = "";
		public int TeamIdInt = -1;

		/// <summary>
		/// 是否机器人
		/// </summary>
		public bool IsRobot = false;

		/// <summary>
		/// 角色信息
		/// </summary>
		/// <returns></returns>
		public FrameCmd.TRoleInfo RoleInfo = new FrameCmd.TRoleInfo();

		/// <summary>
		/// 该玩家是否已开启帧同步就绪
		/// </summary>
		public bool IsFrameSyncOpened = false;

		/// <summary>
		/// team string -> team int
		/// </summary>
		/// <typeparam name="TPlayerId"></typeparam>
		/// <typeparam name="int"></typeparam>
		/// <returns></returns>
		protected static Dictionary<string, int> teamIndexConvMap = new Dictionary<TPlayerId, int>();
		protected static int teamIndexAcc = 0;
		public static void ClearTeamIndexConvMap()
		{
			teamIndexConvMap.Clear();
			teamIndexAcc = 0;
		}
		public static int ConvertTeamIndex(string teamId)
		{
			if (!teamIndexConvMap.ContainsKey(teamId))
			{
				teamIndexConvMap.Add(teamId, teamIndexAcc++);
			}
			return teamIndexConvMap[teamId];
		}

		internal void InitPlayerInfo(com.unity.mgobe.PlayerInfo player)
		{
			this.Id = player.Id;
			this.CustomProfile = player.CustomProfile;
			this.TeamId = player.TeamId;
			this.TeamIdInt = ConvertTeamIndex(player.TeamId);
			this.IsRobot = player.IsRobot;
			if (!player.IsRobot)
			{
				var match = Regex.Match(player.Name, @"([0-9]+)_(.*)");
				if (match == null)
				{
					throw new Exception($"无效的玩家ID信息: {player.Name}");
				}
				var playerIdStr = match.Groups[1].Value;
				var playerId = System.Int64.Parse(playerIdStr);
				var roleName = match.Groups[2].Value;
				if (playerId > 0)
				{
					this.PlayerId = $"{playerId}";
				}
				else
				{
					this.PlayerId = this.Id;
				}
				this.NickName = roleName;
			}
			else
			{
				this.PlayerId = player.Id;
				this.NickName = player.Name;
			}
		}
	}

	public class WaitAllMembersEnterRoomResult
	{
		public bool IsOk = false;
	}

	public class MatchGameRoomResult
	{
		public bool IsOk = false;
	}

	public class StartFightResult
	{
		public bool IsOk = false;
	}

	public class WaitReadyResult
	{
		public bool IsOk = false;
		public long FrameCount = 0;
	}

	/// <summary>
	/// 游戏房间实例
	/// </summary>
	public class GameRoom
	{
		/// <summary>
		/// 房间基础配置
		/// </summary>
		/// <returns></returns>
		GameRoomConfig roomConfig = new GameRoomConfig();
		/// <summary>
		/// 房间服务器
		/// </summary>
		/// <returns></returns>
		RoomServer roomServer = new RoomServer();

		/// <summary>
		/// 房间信息
		/// </summary>
		/// <returns></returns>
		RoomInfo roomInfo = new RoomInfo();
		/// <summary>
		/// 房间信息
		/// </summary>
		/// <returns></returns>
		public RoomInfo RoomInfo => roomInfo;

		/// <summary>
		/// 房间内所有成员
		/// </summary>
		/// <typeparam name="PlayerInfo"></typeparam>
		/// <returns></returns>
		List<PlayerInfo> playerInfos = new List<PlayerInfo>();
		public List<PlayerInfo> PlayerInfos
		{
			get
			{
				playerInfos.Sort((p1, p2) =>
				{
					return p1.Id.CompareTo(p2.Id) > 0 ? 1 : -1;
				});
				return playerInfos;
			}
		}

		public List<PlayerInfo> AIPlayerInfos
		{
			get
			{
				playerInfos.Sort((p1, p2) =>
				{
					return p1.Id.CompareTo(p2.Id) > 0 ? 1 : -1;
				});
				var aiPlayerInfos = (from playerInfo in playerInfos where playerInfo.IsRobot select playerInfo).ToList();
				return aiPlayerInfos;
			}
		}

		public List<PlayerInfo> OnlinePlayerInfos
		{
			get
			{
				playerInfos.Sort((p1, p2) =>
				{
					return p1.Id.CompareTo(p2.Id) > 0 ? 1 : -1;
				});
				var aiPlayerInfos = (from playerInfo in playerInfos where (!playerInfo.IsRobot) select playerInfo).ToList();
				return aiPlayerInfos;
			}
		}

		public TPlayerId MyPlayerId = "";

		Action onAllPlayersEnterRoom = null;

		/// <summary>
		/// 清理状态
		/// </summary>
		public void Clear()
		{
			PlayerInfo.ClearTeamIndexConvMap();

			if (this.roomServer.IsConnected)
			{
				this.roomServer.DisableAutoRequestFrame();
				this.roomServer.OffEnterRoom(null);
				this.roomServer.OffLeaveRoom(null);
				this.roomServer.OffRecvFrame(null);
				this.roomServer.OffRecvFromClient(null);
			}
		}

		public void Stop()
		{
			if (this.roomServer.IsConnected)
			{
				Debug.Log("退出游戏, 离开房间");
				this.roomServer.LeaveRoom().ContinueWith((s) =>
				{
					if (s.Result.IsOk)
					{
						Debug.Log("离开房间成功");
					}
					else
					{
						Debug.Log($"离开房间失败: Code:{s.Result.Error.Code}, Msg:{s.Result.Error.Msg}");
					}
				});
				// this.roomServer.StopFrameSync();
			}
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		public void Dispose()
		{

		}

		public int MaxWaitPlayersCount
        {
            get
            {
                if (this.roomConfig.MatchMode == GameMatchMode.MatchPlayers)
                {
					return this.roomInfo.OnlinePlayersCount;
                }
                else
                {
					return this.roomConfig.MaxPlayers;
                }
            }
        }

		/// <summary>
		/// 当所有人进房间时触发 onAllPlayersEnterRoom
		/// </summary>
		void tryTriggerAllPlayersEnterRoom()
		{
			if (this.playerInfos.Count >= this.roomConfig.MaxPlayers)
			{
				if (this.onAllPlayersEnterRoom != null)
				{
					onAllPlayersEnterRoom();
				}
			}
		}
		void ListenPlayersEnterRoom()
		{
			roomServer.OnEnterRoom((resp) =>
			{
				var data = resp.Data;
				foreach (var player in data.RoomInfo.PlayerList)
				{
					Debug.Log($"玩家进入房间: {player.Id}");
					this.TryAddPlayerInfo(player);
				}
				this.tryTriggerAllPlayersEnterRoom();
			});
		}

		Action OnAllPlayersSendFrame = null;
		/// <summary>
		/// 当所有人开启帧同步并就绪时触发 OnAllPlayersSendFrame
		/// </summary>
		void tryTriggerAllPlayersSendFrame()
		{
			var openedCount = this.playerInfos.FindAll(playerInfo => playerInfo.IsFrameSyncOpened).Count;
			if (openedCount >= this.MaxWaitPlayersCount)
			{
				if (this.OnAllPlayersSendFrame != null)
				{
					OnAllPlayersSendFrame();
				}
			}
		}

		public Task WaitAllMembersSendFrame()
		{
			return AsyncTask.Run((resolve, reject) =>
			{
				this.OnAllPlayersSendFrame = () =>
				{
					// timer.Stop();
					this.OnAllPlayersSendFrame = null;
					resolve();
				};
				this.tryTriggerAllPlayersSendFrame();
			});
		}

		void ListenPlayersSendFrame()
		{
			roomServer.OnRecvFrame((resp) =>
			{
				var receivedTime = 0;//com.unity.mgobe.src.Util.SdkUtil.GetCurrentTimeMilliseconds();
				var frameData = resp.Data.Frame;

				// 随机种子
				var RandomSeed = frameData.Ext.Seed;
				this.roomInfo.RandomSeed = RandomSeed;

				if (Random.Seed != RandomSeed)
				{
					Random.Seed = RandomSeed;
				}
				else
				{
					// UnityEngine.Debug.LogError($"cannot to set randomseed twice: {RandomSeed}");
				}

				// 标记玩家开启帧同步状态
				foreach (var item in frameData.Items)
				{
					var playerInfo = this.playerInfos.Find(playerInfo1 => playerInfo1.Id == item.PlayerId);
					playerInfo.IsFrameSyncOpened = true;

					// 同步角色数据
					var cmd = NetCmdHelper.DecodeFrameCmd(item.Data, frameData, receivedTime);
					if (cmd.CmdType == FrameCmd.TCmdType.SyncRoleInfo)
					{
						playerInfo.RoleInfo.Level = cmd.RoleInfo.Level;
						playerInfo.RoleInfo.Name = cmd.RoleInfo.Name;
						playerInfo.RoleInfo.RoleId = cmd.RoleInfo.RoleId;
						playerInfo.RoleInfo.RoleConfigId = cmd.RoleInfo.RoleConfigId;
						playerInfo.RoleInfo.IsReady = cmd.RoleInfo.IsReady;
						playerInfo.RoleInfo.CommonSkillId = cmd.RoleInfo.CommonSkillId;
						if (playerInfo.RoleInfo.IsReady)
						{
							if (playerInfo.RoleInfo.ReadyFrameCount < 0)
							{
								playerInfo.RoleInfo.ReadyFrameCount = (long)frameData.Id;
							}
						}
						// playerInfo.NickName = cmd.RoleInfo.Name;
						// playerInfo.PlayerId = cmd.RoleInfo.RoleId;
					}
				}

				this.tryTriggerAllPlayersSendFrame();
				this.tryTriggerAllPlayersReady();
			});
		}

		/// <summary>
		/// 服务器时间
		/// </summary>
		protected long ServerTime = 0;

		/// <summary>
		/// 监听成功的帧数据
		/// </summary>
		/// <param name="action"></param>
		public void ListenRecvFrame(Action<RecvFrameBst> action)
		{
			this.roomServer.OnRecvFrame((resp) =>
			{
				if (resp.IsOk)
				{
					// 更新服务器时间
					// this.ServerTime = resp.Data.Frame.Time;
					this.ServerTime = TimeUtil.ConvFrameIdToServerTime(resp.Data.Frame.Id);
					action(resp.Data);
				}
			});
		}

		public void OffRecvFrame()
		{
			this.roomServer.OffRecvFrame();
		}

		PlayerInfo getOrCreatePlayerInfo(string id)
		{
			var playerInfo = this.playerInfos.Find(info => info.Id == id);
			if (playerInfo == null)
			{
				playerInfo = new PlayerInfo()
				{
					Id = id,
				};
				this.playerInfos.Add(playerInfo);
			}
			return playerInfo;
		}

		public PlayerInfo TryAddPlayerInfo(com.unity.mgobe.PlayerInfo player)
		{
			var playerInfo = this.getOrCreatePlayerInfo(player.Id);
			playerInfo.InitPlayerInfo(player);
			if (player.Id == Player.Id)
			{
				playerInfo.IsSelf = true;
				this.MyPlayerId = playerInfo.PlayerId;
			}
			this.roomInfo.OnlinePlayersCount = (from p in playerInfos where !p.IsRobot select p).Count();
			this.roomInfo.AIPlayerCount = playerInfos.Count - this.roomInfo.OnlinePlayersCount;
			return playerInfo;
		}

		async Task<NetResponse<MatchRoomSimpleRsp>> matchRoom(MatchRoomPara myinfo)
		{
			var info = new com.unity.mgobe.MatchRoomPara()
			{
				PlayerInfo = new com.unity.mgobe.PlayerInfoPara()
				{
					// 在name上做文章, 方便绑定自定义角色ID, 账号名和房间服分配的角色ID
					// name=roleid+rawname
					Name = $"{myinfo.PlayerInfo.PlayerId}_{myinfo.PlayerInfo.Name}",
					CustomPlayerStatus = myinfo.PlayerInfo.CustomPlayerStatus,
					CustomProfile = myinfo.PlayerInfo.CustomProfile,
				},
				MaxPlayers = myinfo.MaxPlayers,
				RoomType = myinfo.RoomType,
			};
			var matchResult = await roomServer.MatchRoom(info);
			if (matchResult.IsOk)
			{
				var roomInfo = matchResult.Data.RoomInfo;
				foreach (var player in roomInfo.PlayerList)
				{
					var playerInfo = this.TryAddPlayerInfo(player);
				}
			}
			return matchResult;
		}

		async Task<NetResponse<MatchPlayersBst>> matchPlayers(MatchRoomPara myinfo)
		{
			var info = new com.unity.mgobe.MatchPlayersPara()
			{
				MatchCode = myinfo.MatchCode,
				PlayerInfoPara = new MatchPlayerInfoPara()
				{
					Name = $"{myinfo.PlayerInfo.PlayerId}_{myinfo.PlayerInfo.Name}",
					CustomPlayerStatus = myinfo.PlayerInfo.CustomPlayerStatus,
					CustomProfile = myinfo.PlayerInfo.CustomProfile,
					MatchAttributes = myinfo.PlayerInfo.MatchAttributes,
				},
			};
			var matchResult = await roomServer.MatchPlayers(info);
			if (matchResult.IsOk)
			{
				var roomInfo = matchResult.Data.RoomInfo;
				foreach (var player in roomInfo.PlayerList)
				{
					var playerInfo = this.TryAddPlayerInfo(player);
				}
			}
			return matchResult;
		}

		async Task<NetResponse<MatchGroupRsp>> matchGroup(MatchRoomPara myinfo)
		{
			var info = new com.unity.mgobe.MatchGroupPara()
			{
				MatchCode = myinfo.MatchCode,
				PlayerInfoList = new List<MatchGroupPlayerInfoPara>()
				{
					new MatchGroupPlayerInfoPara(){
						Id= myinfo.PlayerInfo.OpenId,
						Name = $"{myinfo.PlayerInfo.PlayerId}_{myinfo.PlayerInfo.Name}",
						CustomPlayerStatus= myinfo.PlayerInfo.CustomPlayerStatus,
						CustomProfile= myinfo.PlayerInfo.CustomProfile,
						MatchAttributes=myinfo.PlayerInfo.MatchAttributes,
					},
				},
			};
			var matchResult = await roomServer.MatchGroup(info);
			return matchResult;
		}

		MatchRoomPara matchInfo = null;
		/// <summary>
		/// 匹配游戏服务
		/// </summary>
		/// <returns></returns>
		public async Task<MatchGameRoomResult> MatchGameRoom(MatchRoomPara info)
		{
			this.matchInfo = info;

			this.roomConfig.MaxPlayers = (int)info.MaxPlayers;
			this.roomConfig.RoomType = info.RoomType;


			GameInfoPara gameInfo = new GameInfoPara
			{

				// 替换 为控制台上的“游戏ID”
				GameId = "obg-ggeirzp7",
				// 玩家 openId
				OpenId = info.PlayerInfo.OpenId,
				//替换 为控制台上的“游戏Key”
				SecretKey = "43bb9373c381f43170efd6c9dd15bcf1ce1efa8f",
			};
			roomServer.Init(gameInfo);

			var connectResult = await roomServer.ConnectAsync();
			if (connectResult.IsOk)
			{
				Debug.Log("连接房间服务器成功");
				roomServer.EnableAutoRequestFrame();
				Debug.Log("已开启自动补帧");

				this.ListenPlayersEnterRoom();
				this.ListenPlayersSendFrame();
				Debug.Log("设置服务器响应监听");

				var queryRoomResult = await roomServer.GetMyRoom();
				if (queryRoomResult.IsOk)
				{
					Debug.LogFormat("检测到玩家已在房间中, roomId:{0}", roomServer.RoomId);
					await roomServer.StopFrameSync();
					Debug.LogFormat("关闭房间帧同步成功, roomId:{0}", roomServer.RoomId);
				}
				else
				{
					Debug.LogFormat("角色未曾加入房间, {0}", queryRoomResult.Error.Code);
				}
				
				// 先尝试取消匹配
				var cancelMatchResult=await roomServer.CancelPlayerMatch(new CancelPlayerMatchPara(){
					MatchType=com.unity.mgobe.MatchType.PlayerComplex,
				});
				if(cancelMatchResult.IsOk){
					Debug.Log($"取消匹配成功");
				}else{
					Debug.Log($"取消匹配失败");
				}

				//不支持断线续玩, 先离开房间
				var leaveResult = await roomServer.LeaveRoom();
				if (leaveResult.IsOk)
				{
					Debug.Log("离开房间成功");
				}
				else
				{
					Debug.Log("离开房间失败:" + leaveResult.ErrMsg);
				}

				// 匹配房间并进入
				// var matchResult = await this.matchRoom(info);
				// var matchResult = await this.matchGroup(info);
				var matchResult = await this.matchPlayers(info);
				if (matchResult.IsOk)
				{
					Debug.LogFormat("匹配房间成功, roomId:{0}", roomServer.RoomId);
					// 等待其他玩家全部进房间
					var waitAllResult = await WaitAllMembersEnterRoom();
					if (waitAllResult.IsOk)
					{
						Debug.Log("所有成员已进入房间, 直接开始游戏");

						return new MatchGameRoomResult()
						{
							IsOk = true,
						};
					}
					else
					{
						Debug.LogFormat("匹配房间超时");
					}
				}
				else
				{
					Debug.LogFormat("匹配房间失败, code: {1}, msg: {0}", matchResult.Error.Code, matchResult.ErrMsg);
					System.Action unmatch = () =>
					{
						roomServer.CancelPlayerMatch(new CancelPlayerMatchPara()
						{
							MatchType = com.unity.mgobe.MatchType.PlayerComplex,
						});
						roomServer.CancelPlayerMatch(new CancelPlayerMatchPara()
						{
							MatchType = com.unity.mgobe.MatchType.RoomSimple,
						});
					};
					unmatch();
				}

			}
			else
			{
				Debug.Log("连接房间服务器失败:" + connectResult.ErrMsg);
			}

			return new MatchGameRoomResult()
			{
				IsOk = false,
			};

		}

		public async Task<StartFightResult> StartFight()
		{
			// 开启帧同步
			var startFrameSyncResult = await roomServer.StartFrameSync();
			if (startFrameSyncResult.IsOk)
			{
				Debug.Log("开启帧同步成功");

				// 定时发送帧同步消息, 直到收到所有玩家的帧同步消息
				var syncRoleInfoCmd = NetCmdHelper.GenSyncUserInfoCmd(matchInfo.RoleInfo);
				var cmdStr = NetCmdHelper.EncodeFrameCmd(syncRoleInfoCmd);
				var timer = new System.Timers.Timer(500);
				timer.Elapsed += async (s, e) =>
				{
					await this.roomServer.SendFrame(new SendFramePara()
					{
						Data = cmdStr,
					});
				};
				timer.AutoReset = true;
				timer.Enabled = true;

				await WaitAllMembersSendFrame();
				timer.Stop();
				// roomServer.OffRecvFrame();

				Debug.Log("所有玩家已开启帧同步");

				// 连接服务器完成, 异步返回
				Debug.Log("连接对战服务完成");
				return new StartFightResult()
				{
					IsOk = true,
				};
			}
			else
			{
				Debug.Log("开启帧同步失败:" + startFrameSyncResult.ErrMsg);
			}

			return new StartFightResult()
			{
				IsOk = false,
			};
		}

		protected System.Timers.Timer matchRoomTimer = new System.Timers.Timer(5 * 1000);
		/// <summary>
		/// 监听匹配超时
		/// </summary>
		protected void setWaitAllMembersTimer()
		{
			var timer = matchRoomTimer;
			timer.Elapsed += (s, e) =>
			{
				if (this.OnWaitAllMembersTimeout != null)
				{
					this.OnWaitAllMembersTimeout();
				}
			};
			timer.AutoReset = false;
			timer.Enabled = true;
		}
		protected Action OnWaitAllMembersTimeout = null;

		/// <summary>
		/// 重新整理玩家组队ID
		/// </summary>
		public void ReSortPlayerTeamId()
		{
			var teams = new List<string>();
			foreach (var playerInfo in this.playerInfos)
			{
				if (!teams.Contains(playerInfo.TeamId))
				{
					teams.Add(playerInfo.TeamId);
				}
			}
			teams.Sort();
			PlayerInfo.ClearTeamIndexConvMap();
			foreach (var team in teams)
			{
				PlayerInfo.ConvertTeamIndex(team);
			}
			foreach (var playerInfo in this.playerInfos)
			{
				playerInfo.TeamIdInt = PlayerInfo.ConvertTeamIndex(playerInfo.TeamId);
			}
		}

		/// <summary>
		/// 等待所有玩家进房间
		/// </summary>
		/// <returns></returns>
		public Task<WaitAllMembersEnterRoomResult> WaitAllMembersEnterRoom()
		{
			// this.setWaitAllMembersTimer();
			return AsyncTask.Run<WaitAllMembersEnterRoomResult>((resolve, reject) =>
			{
				this.OnWaitAllMembersTimeout = () =>
				{
					this.OnWaitAllMembersTimeout = null;
					this.onAllPlayersEnterRoom = null;
					resolve(new WaitAllMembersEnterRoomResult()
					{
						IsOk = false,
					});
				};
				this.onAllPlayersEnterRoom = () =>
				{
					ReSortPlayerTeamId();

					this.OnWaitAllMembersTimeout = null;
					this.onAllPlayersEnterRoom = null;
					matchRoomTimer.Enabled = false;
					resolve(new WaitAllMembersEnterRoomResult()
					{
						IsOk = true,
					});
				};
				this.tryTriggerAllPlayersEnterRoom();
			});
		}

		/// <summary>
		/// 发送帧数据
		/// </summary>
		public void SendFrameCmd(string cmdStr)
		{
			this.roomServer.SendFrameRaw(new SendFramePara()
			{
				Data = cmdStr,
			}, null);
		}

		Action OnAllPlayersReady = null;
		/// <summary>
		/// 当所有人开启帧同步并就绪时触发 OnAllPlayersReady
		/// </summary>
		bool tryTriggerAllPlayersReady()
		{
			var openedCount = this.playerInfos.FindAll(playerInfo => playerInfo.RoleInfo.IsReady).Count;
			if (openedCount >= this.MaxWaitPlayersCount)
			{
				if (this.OnAllPlayersReady != null)
				{
					OnAllPlayersReady();
				}
				return true;
			}
			return false;
		}

		protected Task<bool> WaitAllMembersReady()
		{
			return AsyncTask.Run<bool>((resolve, reject) =>
			{

				var timer = new System.Timers.Timer(5000);
				timer.Elapsed += (s, e) =>
				{
					// if (this.ServerTime - (long)this.roomServer.RoomInfo.StartGameTime >= 5 * 1000)
					if (this.OnAllPlayersReady != null)
					{
						this.OnAllPlayersReady = null;
						resolve(this.tryTriggerAllPlayersReady());
					}
				};
				timer.AutoReset = true;
				timer.Enabled = true;

				this.OnAllPlayersReady = () =>
				{
					timer.Stop();
					this.OnAllPlayersReady = null;
					resolve(true);
				};
				this.tryTriggerAllPlayersReady();
			});
		}

		/// <summary>
		/// 等待所有玩家准备就绪, 可以开始战斗
		/// </summary>
		public async Task<WaitReadyResult> WaitAllMembersReadyToFight()
		{
			// var syncRoleInfoCmd = NetCmdHelper.GenSyncUserReadyCmd(matchInfo.RoleInfo);
			// var cmdStr = NetCmdHelper.EncodeFrameCmd(syncRoleInfoCmd);
			// var timer = new System.Timers.Timer(500);
			// timer.Elapsed += async (s, e) =>
			// {
			// 	await this.roomServer.SendFrame(new SendFramePara()
			// 	{
			// 		Data = cmdStr,
			// 	});
			// };
			// timer.AutoReset = true;
			// timer.Enabled = true;

			Debug.Log("等待所有玩家就绪");
			bool isAllReady = await WaitAllMembersReady();
			roomServer.OffRecvFrame();
			// timer.Stop();

			var maxFrame = playerInfos.Max((p) => p.RoleInfo.ReadyFrameCount);
			var result = new WaitReadyResult()
			{
				IsOk = isAllReady,
				FrameCount = maxFrame,
			};

			if (result.IsOk)
			{
				Debug.Log("所有玩家已就绪");
			}
			else
			{
				Debug.LogWarning("等待所有玩家就绪超时");
			}

			return result;
		}

		/// <summary>
		/// 通知其他玩家自己就绪
		/// </summary>
		public void NotifyReady()
		{
			var syncRoleInfoCmd = NetCmdHelper.GenSyncUserReadyCmd(matchInfo.RoleInfo);
			var cmdStr = NetCmdHelper.EncodeFrameCmd(syncRoleInfoCmd);
			this.roomServer.SendFrame(new SendFramePara()
			{
				Data = cmdStr,
			});
		}

	}
}
