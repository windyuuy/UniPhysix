
using System.Collections;
using System.Collections.Generic;
using TPlayerId = System.String;
using Debug = UnityEngine.Debug;
using Exception = System.Exception;
using IEnumerator = System.Collections.IEnumerator;
using System.Linq;
using SdkUtil = com.unity.mgobe.src.Util.SdkUtil;

namespace fsync
{
	public interface IFrameSyncUpdate
	{
		/// <summary>
		/// 基于帧同步网络的定时器
		/// </summary>
		/// <value></value>
		INetTimer NetTimer { get; set; }

		/// <summary>
		/// 对象ID
		/// </summary>
		/// <value></value>
		TPlayerId OID { get; }

		/// <summary>
		/// 本地对象ID
		/// </summary>
		/// <value></value>
		int LocalOID { get; }

		/// <summary>
		/// 帧同步一帧调度
		/// - 在物理引擎update之前调度
		/// </summary>
		void FrameSyncUpdate();

		/// <summary>
		/// 帧同步一帧调度
		/// - 在物理引擎update之后调度
		/// </summary>
		void FrameSyncLateUpdate();

		/// <summary>
		/// 处理一帧中收到的各个命令
		/// </summary>
		/// <param name="cmd"></param>
		void HandleFrameCmd(FrameCmd cmd);

	}

	/// <summary>
	/// 持续处理网络指令
	/// - 收到一帧网络指令之后, 直接简单执行一帧中的所有指令
	/// - 处理完指令之后, 统一调度各个对象的主动 Update 方法
	/// </summary>
	public class NetCmdHandler
	{
		public void HandleFrameCmd(FrameCmd cmd)
		{
			var gameObject = GameObjectManager.Inst.GetOnlineTarget(cmd.ActorId);
			if (gameObject != null)
			{
				try
				{
					gameObject.HandleFrameCmd(cmd);
				}
				catch (Exception e)
				{
					Debug.LogError("对象执行帧指令异常:");
					Debug.LogError(e);
				}
			}
		}

		public int simulateOdd = 0;

		protected NetDelay netDelay = new NetDelay();

		public long GetNetDelay()
		{
			return netDelay.GetNetDelayAve();
		}
		public NetDelay GetNetDelayProxy()
		{
			return netDelay;
		}

		/// <summary>
		/// 处理网络帧刷新事件
		/// </summary>
		/// <param name="cmds"></param>
		public void HandleOneFrame(com.unity.mgobe.Frame frameData)
		{
			var netTimer = GameObjectManager.Inst.NetTimer;
			// 执行一帧指令
			var cmds = this.GetFrameCmds(frameData);

			if (MyPlayer.IsOnlineMode)
			{
				// 解析网络延迟
				var myPlayerId = MyPlayer.GetPlayerID();
				var myItem = frameData.Items.FirstOrDefault((item) => item.PlayerId == myPlayerId);
				var myCmd = cmds.FirstOrDefault((cmd) => cmd.ActorId == myPlayerId);
				if (myItem != null)
				{
					var receiveTime1 = SdkUtil.GetCurrentTimeMilliseconds();
					var sendTime1 = myCmd.CreateTime;
					// 处理延迟
					var handleDt = receiveTime1 - sendTime1;
					var receiveTime = frameData.Time;
					var sendTime = (long)myItem.Timestamp;
					// 网络延迟
					var netDt = receiveTime - sendTime;
					// Debug.Log($"net delay: {netDt}, {handleDt}");
					netDelay.Put(netDt);
					netDelay.PutLocal(handleDt);
				}
			}

			// TODO: 如果存在画面拟合导致的不同步问题, 尝试开启下面代码
			{
				// TrueSync.TSPhysics.SyncView();
			}

			// 遍历处理帧指令
			foreach (var cmd in cmds)
			{
				this.HandleFrameCmd(cmd);
			}
			// 遍历所有本地注册对象处理网络帧刷新事件
			GameObjectManager.Inst.ForeachAll(gameObject =>
			{
				try
				{
					gameObject.FrameSyncUpdate();
				}
				catch (Exception e)
				{
					Debug.LogError("对象执行帧刷新异常:");
					Debug.LogError(e);
				}
			});
			if (netTimer.deltaTime != 0)
			{
				// simulateOdd++;
				// simulateOdd = simulateOdd % 2;
				// if (simulateOdd == 1)
				// {
				// UnityEngine.Physics.Simulate(netTimer.deltaTime);
				// TrueSync.TrueSyncManager.instance.scheduler.UpdateAllCoroutines();
				TrueSync.TSPhysics.Simulate(netTimer.deltaTime);
				// }

				// 细分更新
				// {
				// 	var div = 2;
				// 	var ddt = TrueSync.TSMath.Floor(netTimer.deltaTime / div * 1000) / 1000;
				// 	var ddt2 = netTimer.deltaTime - ddt * (div - 1);
				// 	for (var i = 0; i < (div - 1); i++)
				// 	{
				// 		if (ddt > 0)
				// 		{
				// 			TrueSync.TSPhysics.Simulate(ddt);
				// 		}
				// 	}
				// 	if (ddt2 > 0)
				// 	{
				// 		TrueSync.TSPhysics.Simulate(ddt2);
				// 	}
				// }
			}
			// 遍历所有本地注册对象处理网络帧刷新事件
			GameObjectManager.Inst.ForeachAll(gameObject =>
			{
				try
				{
					gameObject.FrameSyncLateUpdate();
				}
				catch (Exception e)
				{
					Debug.LogError("对象执行帧刷新异常:");
					Debug.LogError(e);
				}
			});
		}

		/// <summary>
		/// 从帧数据中获取帧指令
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		List<FrameCmd> GetFrameCmds(com.unity.mgobe.Frame data)
		{
			var cmds = new List<FrameCmd>();
			foreach (var item in data.Items)
			{
				try
				{
					// TODO: 需要处理 roleID冗余
					var cmd = NetCmdHelper.DecodeFrameCmd(item.Data, data, 0);
					cmds.Add(cmd);
				}
				catch (Exception e)
				{
					Debug.LogError("DecodeFrameCmd failed:");
					Debug.LogError(e);
				}
			}
			//Debug.Log($"framecmds: {cmds.Count}");

			// 合并一帧内重叠的指令
			//var roleCmds = new Dictionary<string, FrameCmd>();
			//foreach (var cmd in cmds)
			//{
			//	roleCmds[cmd.ActorId] = cmd;
			//}
			//foreach (var p in roleCmds)
			//{
			//	var cmd = p.Value;
			//	var roleCmd = roleCmds[cmd.ActorId];
			//	if (cmd.Move.IsMoving)
			//	{
			//		roleCmd.Move = cmd.Move;
			//	}
			//	for (var i = 0; i < roleCmd.Skills.Count; i++)
			//	{
			//		if (cmd.Skills[i].SkillType != SkillType.Invalid)
			//		{
			//			roleCmd.Skills[i] = cmd.Skills[i];
			//		}
			//	}
			//}

			//cmds.Clear();
			//foreach (var p in roleCmds)
			//         {
			//	cmds.Add(p.Value);
			//}

			return cmds;
		}

		/// <summary>
		/// 缓存的帧数据
		/// - 推迟到 Unity 的 Update 中执行
		/// </summary>
		/// <typeparam name="com.unity.mgobe.RecvFrameBst"></typeparam>
		/// <returns></returns>
		protected List<com.unity.mgobe.RecvFrameBst> frameDatas = new List<com.unity.mgobe.RecvFrameBst>();
		protected List<com.unity.mgobe.RecvFrameBst> frameDatasBuffer = new List<com.unity.mgobe.RecvFrameBst>();
		protected readonly object _lock = new object();

		/// <summary>
		/// 帧序转换为指令到达服务器的大致时间点
		/// </summary>
		/// <param name="frameCount"></param>
		/// <returns></returns>
		protected ulong toFrameServerTime(ulong frameCount)
		{
			return (1000 * frameCount) / (ulong)FrameSyncConfig.Inst.NetFps;
		}

		protected bool isHandlingFrame = false;
		/// <summary>
		/// 批量处理缓存的帧数据
		/// </summary>
		public void HandleFramesData()
		{
			isHandlingFrame = true;
			var netTimer = GameObjectManager.Inst.NetTimer;
			var serverTimer = GameObjectManager.Inst.ServerTimer;

			// 通过剪切尽量避开多线程的影响
			lock (_lock)
			{
				frameDatasBuffer.AddRange(this.frameDatas);
				this.frameDatas.Clear();
			}

			if (needResetTimer && frameDatasBuffer.Count >= 1)
			{
				needResetTimer = false;
				var data = frameDatasBuffer[0];
				var frameCount = data.Frame.Id;
				var startTime = toFrameServerTime(frameCount);
				netTimer.setStartTime(startTime);
				netTimer.setStartFrameCount((long)frameCount);

				// init server timer
				{
					serverTimer.setStartTime(startTime);
					serverTimer.setStartFrameCount((long)frameCount);
				}

				Debug.Log("disable Physics Auto Simulation");
				UnityEngine.Physics.autoSimulation = false;
				TrueSync.TSPhysics.autoSimulation = false;
			}

			// while (frameDatasBuffer.Count > 0)
			if (frameDatasBuffer.Count > 0)
			{

				// update server timer
				{
					var latestFrame = frameDatasBuffer[frameDatasBuffer.Count - 1].Frame;
					var latestServerTime = toFrameServerTime(latestFrame.Id);
					serverTimer.updateTime(latestServerTime);
					serverTimer.updateFrameCount((long)latestFrame.Id);
				}

				var data = frameDatasBuffer[0];
				frameDatasBuffer.RemoveAt(0);

				var frameData = data.Frame;

				// 更新定时器
				var serverTime = toFrameServerTime(frameData.Id);
				netTimer.updateTime(serverTime);
				// 更新当前帧率
				netTimer.updateFrameCount((long)frameData.Id);
                if (netTimer.isWorking)
				{
					// 同步测试
#if true
					Debug.Log($"handle frame-start: FrameCount: {netTimer.frameCount}, UIFC: {UnityEngine.Time.frameCount}");
#endif
					// {
					// 	Debug.Log($"==== playerspos start FrameCount: {GameObjectManager.inst.NetTimer.frameCount}=============================================");
					// 	var players = UnityEngine.GameObject.FindObjectsOfType<BaseRole>();
					// 	var players2 = new List<BaseRole>(players);
					// 	players2.Sort((a, b) =>
					// 	{
					// 		return a.transform.position.x > b.transform.position.x ? 1 : -1;
					// 	});
					// 	foreach (var role in players)
					// 	{
					// 		var pos = role.transform.position;
					// 		var rot = role.transform.rotation.eulerAngles;
					// 		Debug.Log($"playerspos: id:{role.GetInstanceID()}, prefab={role.config.roleId}, pos=({pos.x},{pos.y},{pos.z}), rot=({rot.x},{rot.y},{rot.z})");
					// 	}
					// 	Debug.Log("==== playerspos end =============================================");
					// }

					// 执行帧调度
					this.HandleOneFrame(data.Frame);
				}

			}

			isHandlingFrame = false;
		}

		/// <summary>
		/// 清理帧缓存数据
		/// </summary>
		public void ClearFrame()
		{
			this.frameDatas.Clear();
			this.frameDatasBuffer.Clear();
		}

		protected static MonoScheduler schedulerObject = null;

		protected System.Action OnHandleFramesData = null;

		long uiFrameCount = 0;
		/// <summary>
		/// 监听帧同步数据
		/// </summary>
		public void ListenRecvFrame()
		{
			if (schedulerObject == null)
			{
				TimeWaiterManager.Inst.Init();

				// 编辑器模式下, 用户操作编辑器导致的暂停事件会导致后续的协程事件返回延后,
				// 导致协程回调会错误得延后到下一逻辑帧执行,
				Debug.LogWarning("编辑器模式下, 需要限制加速倍率, 避免协程调度受干扰");
				schedulerObject = MonoScheduler.Create();
				var isPausedEver = false;
				schedulerObject.OnPause(() =>
				{
					uiFrameCount = 0;
					isPausedEver = true;
				});
				OnHandleFramesData=schedulerObject.Schedule(() =>
				{
					uiFrameCount++;

					if (!this.isStartedFrameHandler)
					{
						return;
					}

#if UNITY_EDITOR
					if (uiFrameCount <= 0)
					{
						return;
					}
#else
					if (uiFrameCount <= DebugConfig.uiFrameCountForMobile)
					{
						return;
					}
#endif
					if (isPausedEver)
					{
						if (uiFrameCount <= 2)
						{
							return;
						}
					}
					isPausedEver = false;
					uiFrameCount = 0;

					// 在UI线程中处理帧事件, 否则在其他线程中很多Unity API无法使用
					this.HandleFramesData();
				});
			}

			// TODO: 完善掉帧检测
			var startFrameId = startFrameCount;
			ulong thisFrameId = startFrameCount - 1;
			GameRoomManager.SharedGameRoom.ListenRecvFrame((data) =>
			{
				// 延迟一小段时间, 避免在UI回调中初始设置的协程干扰调度, 同时降低不同设备差距
				if (data.Frame.Id < startFrameId)
				{
					// Debug.Log($"netframe, uiframeCount:{UnityEngine.Time.frameCount}");
					// discard
					return;
				}

				if (data.Frame.Id - thisFrameId != 1)
				{
					Debug.LogError($"frameId missing: {thisFrameId}");
				}
				thisFrameId = data.Frame.Id;

				lock (_lock)
				{
					this.frameDatas.Add(data);
				}
			});
		}

		public void OffListenRecvFrame()
		{
            if (GameRoomManager.SharedGameRoom != null)
            {
				GameRoomManager.SharedGameRoom.OffRecvFrame();
			}
			//schedulerObject.UnSchedule(OnHandleFramesData);
			//OnHandleFramesData = null;
		}

		public void PostLocalFrame(com.unity.mgobe.RecvFrameBst data)
		{

			lock (_lock)
			{
				this.frameDatas.Add(data);
			}
		}

		/// <summary>
		/// 重置计时器开始时间点
		/// </summary>
		bool needResetTimer = false;
		/// <summary>
		/// 重置计时器开始时间点
		/// </summary>
		public void ResetTimer()
		{
			GameObjectManager.Inst.NetTimer.clear();
			needResetTimer = true;
		}

		/// <summary>
		/// 开始处理帧同步事件
		/// </summary>
		protected bool isStartedFrameHandler = false;

		/// <summary>
		/// 开始处理帧同步事件
		/// </summary>
		public void StartFrameSyncHandler()
		{
			this.isStartedFrameHandler = true;
			// 延迟一点点
			this.uiFrameCount = -2;
		}
		/// <summary>
		/// 停止帧同步事件处理
		/// </summary>
		public void StopFrameSyncHandler()
		{
			this.isStartedFrameHandler = false;
		}

		/// <summary>
		/// 清理状态
		/// </summary>
		public void Prepare()
		{
			// 初始化在线计时器
			this.ResetTimer();
			// 清除帧缓存
			this.ClearFrame();
			// 监听帧同步指令
			this.ListenRecvFrame();
			// 停止帧同步事件处理
			this.StopFrameSyncHandler();
			MyPlayer.IsRunning = true;
		}

		/// <summary>
		/// 停止帧同步
		/// </summary>
		public void Stop()
		{
			GameRoomManager.SharedGameRoom.Stop();
		}

		/// <summary>
		/// 清理状态
		/// </summary>
		public void Clear()
		{
			// 初始化在线计时器
			this.ResetTimer();
			// TODO: 在回调里清除帧缓存
			this.Stop();
			MyPlayer.IsRunning = false;
			// 清除帧缓存
			this.ClearFrame();
			// 取消监听帧同步指令
			this.OffListenRecvFrame();
			// 停止帧同步事件处理
			this.StopFrameSyncHandler();
		}

		protected ulong startFrameCount = 0;
		public void SetStartFrameCount(ulong frameCount)
		{
			startFrameCount = frameCount;
			UnityEngine.Debug.Log($"设置开始帧率: {frameCount}");
		}
	}

}
