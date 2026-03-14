
using com.unity.mgobe;
using System;
using System.Threading.Tasks;

namespace fsync
{

	using Debug = UnityEngine.Debug;
	using InitListenerResult = System.Object;

	/// <summary>
	/// 服务器返回错误
	/// </summary>
	public class NetError
	{
		public NetError Init(ResponseEvent resp)
		{
			this.Code = resp.Code;
			this.Msg = resp.Msg;
			this.Seq = resp.Seq;
			this.Data = resp.Data;
			return this;
		}

		public NetError Init(Exception e)
		{
			this.Code = -1;
			this.Msg = e.Message;
			return this;
		}

		public int Code { get; set; }

		public string Msg { get; set; }

		public string Seq { get; set; }

		public object Data { get; set; }

		public string GetString()
		{
			string str = "{\"Code\": " + this.Code +
						 ", \"Seq\": \"" + this.Seq +
						 "\", \"Msg\": \"" + this.Msg +
						 "\", \"Data\": " + this.Data?.ToString() +
						 "}";
			return str;
		}
	}


	/// <summary>
	/// 网络请求返回
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NetResponse<T>
	{
		/// <summary>
		/// 是否返回成功
		/// </summary>
		public bool IsOk;
		/// <summary>
		/// 错误信息
		/// </summary>
		public NetError Error;
		/// <summary>
		/// 返回数据
		/// </summary>
		public T Data;

		public NetResponse<T> Init(ResponseEvent resp)
		{
			try
			{
				this.Data = (T)resp.Data;
				this.Error = new NetError().Init(resp);
				this.IsOk = resp.Code == ErrCode.EcOk;
			}
			catch (Exception e)
			{
				Debug.LogError("格式化数据失败:");
				Debug.LogError(e);
				this.IsOk = false;
				this.Error = new NetError().Init(e);
			}
			return this;
		}

		public NetResponse<T> Init(BroadcastEvent resp)
		{
			this.Data = (T)resp.Data;
			this.Error = new NetError();
			this.IsOk = true;
			return this;
		}

		public NetResponse<T> Init(T data)
		{
			this.Data = data;
			this.Error = new NetError();
			this.IsOk = true;
			return this;
		}

		/// <summary>
		/// 获取错误信息
		/// </summary>
		/// <returns></returns>
		public string ErrMsg { get { return this.Error.GetString(); } }
	}

	public class RoomServer
	{
		public Room room;

		public com.unity.mgobe.RoomInfo RoomInfo => this.room.RoomInfo;

		public string RoomId => this.RoomInfo.Id;

		NetResponse<Object> convResp(ResponseEvent resp)
		{
			return convResp<Object>(resp);
		}
		NetResponse<T> convResp<T>(ResponseEvent resp)
		{
			return new NetResponse<T>().Init(resp);
		}
		NetResponse<Object> convResp(BroadcastEvent resp)
		{
			return convResp<Object>(resp);
		}
		NetResponse<T> convResp<T>(BroadcastEvent resp)
		{
			return new NetResponse<T>().Init(resp);
		}


		/// <summary>
		/// 判断listen是否初始化过一次,防止重复初始化
		/// </summary>
		protected InitListenerResult listenerInitResult = null;

		protected GameInfoPara gameInfo;
		public void Init(GameInfoPara gameInfo)
		{
			this.gameInfo = gameInfo;
		}

		public async Task<NetResponse<InitListenerResult>> InitListener()
		{
			if (listenerInitResult != null)
			{
				return new NetResponse<InitListenerResult>().Init(listenerInitResult);
			}

			// GameInfoPara gameInfo = new GameInfoPara
			// {

			// 	// 替换 为控制台上的“游戏ID”
			// 	GameId = "obg-cvmuteb5",
			// 	// 玩家 openId
			// 	OpenId = "rrrrrrrrr1",
			// 	//替换 为控制台上的“游戏Key”
			// 	SecretKey = "01f6a30599a384ae6f5a20bb766015e9a4799762",
			// };

			ConfigPara config = new ConfigPara
			{

				// 替换 为控制台上的“域名”
				// CacertNativeUrl,
				IsAutoRequestFrame = true,
				// ReconnectInterval= 1000,
				// ReconnectMaxTimes= 5,
				// ResendInterval= 1000,
				// ResendTimeout= 5000,
				Url = "cvmuteb5.wxlagame.com",
				ReconnectMaxTimes = 5,
				ReconnectInterval = 1000,
				ResendInterval = 1000,
				ResendTimeout = 10000,
			};

			var initListenerTask = AsyncTask.Run<NetResponse<InitListenerResult>>((resolve, reject) =>
			{
				if (Listener.IsInited())
				{
					resolve(new NetResponse<InitListenerResult>()
					{
						IsOk = true,
						Data = new InitListenerResult(),
						Error = new NetError()
						{
							Code = 0,
							Data = null,
							Msg = "SDK.Listener.IsInited",
							Seq = null,
						},
					});
				}
				else
				{
					// 初始化监听器 Listener
					Listener.Init(gameInfo, config, (ResponseEvent eve) =>
						{
							resolve(convResp<InitListenerResult>(eve));
						});
				}
			});

			var result = await initListenerTask;
			// 缓存结果
			listenerInitResult = result;
			return result;
		}

		/// <summary>
		/// 连接房间服务器
		/// </summary>
		/// <returns></returns>
		public async Task<NetResponse<InitListenerResult>> ConnectAsync()
		{
			var result = await this.InitListener();
			if (result.IsOk)
			{
				room = new Room(null);
				Listener.Add(room);
			}
			return result;
		}

		/// <summary>
		/// 启用自动补帧流程
		/// </summary>
		public void EnableAutoRequestFrame()
		{
			room.OnAutoRequestFrameError = (evt) =>
			{
				Debug.LogError($"自动补帧失败: code: {evt.Data}, 尝试自动请求补帧.");
				// 重试
				room.RetryAutoRequestFrame();
				Debug.Log("已经自动请求补帧.");
			};
		}

		public void DisableAutoRequestFrame()
        {
			room.OnAutoRequestFrameError = null;
        }

		/// <summary>
		/// 房间服务器是否已连接
		/// </summary>
		public bool IsConnected => this.room != null;

		/// <summary>
		/// 获取玩家已加入的房间
		/// </summary>
		/// <returns></returns>
		public Task<NetResponse<GetRoomByRoomIdRsp>> GetMyRoom()
		{
			return AsyncTask.Run<NetResponse<GetRoomByRoomIdRsp>>((resolve, reject) =>
			{
				Room.GetMyRoom(resp =>
				{
					if (resp.Code == 0)
					{
						var data = (GetRoomByRoomIdRsp)resp.Data;
						// 设置房间信息到 room 实例
						room.InitRoom(data.RoomInfo);
					}
					resolve(convResp<GetRoomByRoomIdRsp>(resp));
				});
			});
		}

		/// <summary>
		/// 离开房间
		/// </summary>
		/// <returns></returns>
		public Task<NetResponse<LeaveRoomRsp>> LeaveRoom()
		{
			return AsyncTask.Run<NetResponse<LeaveRoomRsp>>((resolve, reject) =>
			{
				room.LeaveRoom((resp) =>
				{
					resolve(convResp<LeaveRoomRsp>(resp));
				});
			});
		}

		/// <summary>
		/// 解散房间
		/// </summary>
		/// <returns></returns>
		public Task<NetResponse<DismissRoomRsp>> DismissRoom()
		{
			return AsyncTask.Run<NetResponse<DismissRoomRsp>>((resolve, reject) =>
			{
				room.DismissRoom((resp) =>
				{
					resolve(convResp<DismissRoomRsp>(resp));
				});
			});
		}

		/// <summary>
		/// 监听成员离开房间
		/// </summary>
		/// <param name="call"></param>
		public void OnLeaveRoom(Action<NetResponse<LeaveRoomBst>> call)
		{
			room.OnLeaveRoom = (eve) =>
			{
				call.Invoke(convResp<LeaveRoomBst>(eve));
			};
		}

		/// <summary>
		/// 取消成员监听离开房间
		/// </summary>
		/// <param name="call"></param>
		public void OffLeaveRoom(Action<NetResponse<LeaveRoomBst>> call)
		{
			room.OnLeaveRoom = null;
		}

		/// <summary>
		/// 监听进入房间
		/// </summary>
		/// <param name="call"></param>
		public void OnEnterRoom(Action<NetResponse<JoinRoomBst>> call)
		{
			room.OnJoinRoom = (eve) =>
			{
				call.Invoke(convResp<JoinRoomBst>(eve));
			};
		}

		/// <summary>
		/// 取消监听进入房间
		/// </summary>
		/// <param name="call"></param>
		public void OffEnterRoom(Action<NetResponse<JoinRoomBst>> call)
		{
			room.OnJoinRoom = null;
		}

		/// <summary>
		/// 匹配房间
		/// </summary>
		/// <returns></returns>
		public Task<NetResponse<MatchRoomSimpleRsp>> MatchRoom(com.unity.mgobe.MatchRoomPara info)
		{
			return AsyncTask.Run<NetResponse<MatchRoomSimpleRsp>>((resolve, reject) =>
			{
				room.MatchRoom(info, (resp) =>
				 {
					 resolve(convResp<MatchRoomSimpleRsp>(resp));
				 });
			});
		}

		/// <summary>
		/// 组队匹配。
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public Task<NetResponse<MatchGroupRsp>> MatchGroup(com.unity.mgobe.MatchGroupPara info)
		{
			return AsyncTask.Run<NetResponse<MatchGroupRsp>>((resolve, reject) =>
			{
				room.MatchGroup(info, (resp) =>
				 {
					 resolve(convResp<MatchGroupRsp>(resp));
				 });
			});
		}

		/// <summary>
		/// 玩家在线匹配。
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public Task<NetResponse<MatchPlayersBst>> MatchPlayers(com.unity.mgobe.MatchPlayersPara info)
		{
			return AsyncTask.Run<NetResponse<MatchPlayersBst>>((resolve, reject) =>
			{
				room.MatchPlayers(info, (resp) =>
				 {
					 Debug.Log("MatchPlayers resped");
					 resolve(convResp<MatchPlayersBst>(resp));
				 });
			});
		}

		/// <summary>
		/// 玩家在线匹配。
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public Task<NetResponse<CancelPlayerMatchRsp>> CancelPlayerMatch(com.unity.mgobe.CancelPlayerMatchPara info)
		{
			return AsyncTask.Run<NetResponse<CancelPlayerMatchRsp>>((resolve, reject) =>
			{
				room.CancelPlayerMatch(info, (resp) =>
				 {
					 resolve(convResp<CancelPlayerMatchRsp>(resp));
				 });
			});
		}

		/// <summary>
		/// 开启帧同步
		/// </summary>
		/// <returns></returns>
		public Task<NetResponse<StartFrameSyncRsp>> StartFrameSync()
		{
			return AsyncTask.Run<NetResponse<StartFrameSyncRsp>>((resolve, reject) =>
			{
				room.StartFrameSync((resp) =>
				{
					resolve(convResp<StartFrameSyncRsp>(resp));
				});
			});
		}

		/// <summary>
		/// 关闭帧同步
		/// </summary>
		/// <returns></returns>
		public Task<NetResponse<StopFrameSyncRsp>> StopFrameSync()
		{
			return AsyncTask.Run<NetResponse<StopFrameSyncRsp>>((resolve, reject) =>
			{
				room.StopFrameSync((resp) =>
				{
					resolve(convResp<StopFrameSyncRsp>(resp));
				});
			});
		}

		/// <summary>
		/// 发送帧消息
		/// </summary>
		/// <param name="para"></param>
		/// <returns></returns>
		public Task<NetResponse<SendFrameRsp>> SendFrame(SendFramePara para)
		{
			return AsyncTask.Run<NetResponse<SendFrameRsp>>((resolve, reject) =>
			{
				room.SendFrame(para, (resp) =>
				 {
					 resolve(convResp<SendFrameRsp>(resp));
				 });
			});
		}

		public void SendFrameRaw(SendFramePara para, Action<ResponseEvent> callback)
		{
			room.SendFrame(para, callback);
		}

		/// <summary>
		/// 监听帧同步消息
		/// </summary>
		/// <param name="call"></param>
		public void OnRecvFrame(Action<NetResponse<RecvFrameBst>> call)
		{
            room.OnRecvFrame = (eve) =>
            {
                call.Invoke(convResp<RecvFrameBst>(eve));
            };
        }

		/// <summary>
		/// 取消监听帧同步消息
		/// </summary>
		public void OffRecvFrame(Action<NetResponse<RecvFrameBst>> call = null)
		{
			room.OnRecvFrame = null;
		}

		/// <summary>
		/// 发送消息给房间内玩家
		/// </summary>
		/// <param name="para"></param>
		/// <returns></returns>
		public Task<NetResponse<SendToClientRsp>> SendToClient(SendToClientPara para)
		{
			return AsyncTask.Run<NetResponse<SendToClientRsp>>((resolve, reject) =>
			{
				room.SendToClient(para, (resp) =>
				 {
					 resolve(convResp<SendToClientRsp>(resp));
				 });
			});
		}

		/// <summary>
		/// 收到房间内其他玩家消息广播回调接口
		/// </summary>
		/// <param name="call"></param>
		public void OnRecvFromClient(Action<NetResponse<RecvFromClientBst>> call)
		{
			room.OnRecvFromClient = (eve) =>
			{
				call.Invoke(convResp<RecvFromClientBst>(eve));
			};
		}

		/// <summary>
		/// 取消收到房间内其他玩家消息广播回调接口
		/// </summary>
		public void OffRecvFromClient(Action<NetResponse<RecvFromClientBst>> call = null)
		{
			room.OnRecvFrame = null;
		}

	}
}
