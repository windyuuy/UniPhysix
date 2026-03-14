
namespace fsync
{
	using TTimeStamp = System.Int64;
	using TSFloat = TrueSync.FP;

	public interface INetTimer
	{
		/// <summary>
		/// 当前帧开始时间
		/// - 为游戏时长计时
		/// - 相当于 Time.time
		/// </summary>
		/// <returns></returns>
		TSFloat time { get; }

		TSFloat deltaTime { get; }

		TSFloat timeSinceLevelLoad { get; }
		TSFloat realtimeSinceStartup { get; }

		TSFloat timeScale { get; set; }

		void setStartTime(ulong time);
		void updateTime(ulong time);

		void setStartFrameCount(long frameCount);

		/// <summary>
		/// 刷新逻辑帧率
		/// </summary>
		void updateFrameCount(long frameCount);

		/// <summary>
		/// 逻辑帧率
		/// </summary>
		long frameCount { get; }

		void clear();
	}

	/// <summary>
	/// 网络时间管理
	/// - 使用前需要设置 setStartTime 和 updateTime
	/// </summary>
	public class TNetTimer : INetTimer
	{

		/// <summary>
		/// 外界实际当前时间点
		/// </summary>
		protected TTimeStamp _curTimeRecord = 0;

		/// <summary>
		/// 游戏内部当前时间点
		/// - 从一局开始
		/// </summary>
		protected TTimeStamp _curTime = 0;
		
		/// <summary>
		/// 游戏真实经过时长
		/// </summary>
		protected TTimeStamp _curRealTime = 0;

		/// <summary>
		/// 游戏当前帧开始时间
		/// - 从一局开始
		/// </summary>
		protected TTimeStamp _lastTime = 0;

		/// <summary>
		/// 当前帧间间隔
		/// </summary>
		protected TTimeStamp _deltaTime = 0;

		/// <summary>
		/// 最大帧间隔,用于提升断点调试体验
		/// </summary>
		protected TTimeStamp _maxDeltaTime = TTimeStamp.MaxValue;

		/// <summary>
		/// 游戏开始时间点
		/// </summary>
		protected TTimeStamp _startTime = 0;

		/// <summary>
		/// 获取当前游戏时间戳
		/// - 和 getGameTime() 的区别在于, getGameTime 的起始时间点为 0, getTime 的起始时间点和游戏开始时的 Date.now() 基本一致
		/// </summary>
		/// <returns></returns>
		public TTimeStamp getTime()
		{
			return this._curTimeRecord;
		}


		/// <summary>
		/// 用于逐步更新游戏时间点进度
		/// </summary>
		/// <param name="time"></param>
		public void updateTime(TTimeStamp time)
		{
			// 记录迭代源时刻
			var dt = time - this._curTimeRecord;
			this._curTimeRecord = time;

			// 计算时差
			// this._deltaTime = Math.Min((TSFloat)dt, (TSFloat)this._maxDeltaTime);
			this._deltaTime = dt <= this._maxDeltaTime ? dt : this._maxDeltaTime;
			this._curRealTime = this._curRealTime + this._deltaTime;
			// 缩放时差
			this._deltaTime = this._deltaTime * this._timeScale / medium;
			// 迭代计时器对外状态
			this._lastTime = this._curTime;
			this._curTime = this._curTime + this._deltaTime;
		}

		public void updateTime(ulong time)
		{
			this.updateTime((long)time);
		}

		/// <summary>
		/// 重设游戏开始时间点
		/// </summary>
		/// <param name="time"></param>
		public void setStartTime(TTimeStamp time)
		{
			this._startTime = time;
			this._curTimeRecord = time;
			this._curTime = 0;
			this._lastTime = 0;
			this._deltaTime = 0;
			this._frameCount = 0;
			this._curRealTime=0;
		}
		public void setStartTime(ulong time)
		{
			this.setStartTime((long)time);
		}

		/// <summary>
		/// 游戏已进行时长
		/// </summary>
		/// <returns></returns>
		public TTimeStamp getGameTime()
		{
			return this._curTime;
		}
		/// <summary>
		/// 当前帧开始时间
		/// - 为游戏时长计时
		/// - 相当于 Time.time
		/// </summary>
		/// <returns></returns>
		public TSFloat time => (TSFloat)(this.getGameTime()) / 1000;

		/// <summary>
		/// 游戏经过真实时长
		/// </summary>
		/// <returns></returns>
		public TSFloat realtimeSinceStartup =>(TSFloat)(this._curRealTime) / 1000;

		/// <summary>
		/// 间隔
		/// - 受 timeScale 影响
		/// </summary>
		/// <value></value>
		public TSFloat deltaTime
		{
			get
			{
				return (TSFloat)(this._deltaTime) / 1000;
			}
		}

		public TSFloat timeSinceLevelLoad => (TSFloat)(this.getGameTime()) / 1000;

		public const long medium = 1024 * 16;
		public long _timeScale = medium;
		public TSFloat timeScale
		{
			get
			{
				return (float)_timeScale / medium;
			}
			set
			{
				UnityEngine.Debug.LogWarning($"set TimeScale: {this.timeScale} -> {value}");
				this._timeScale = (long)(value * medium);

				UnityEngine.Time.timeScale = value.AsFloat();
			}
		}

		public bool isWorking => this._timeScale > 0;
		public bool isPaused => this._timeScale == 0;

		public void setMaxDeltaTime(TTimeStamp dt)
		{
			this._maxDeltaTime = dt;
		}

		/// <summary>
		/// 逻辑帧率
		/// </summary>
		public long frameCount => this._frameCount - _startFrameCount;
		protected long _startFrameCount = 0;
		protected long _frameCount = 0;

		public void setStartFrameCount(long frameCount)
		{
			this._startFrameCount = frameCount;
		}

		/// <summary>
		/// 刷新逻辑帧率
		/// </summary>
		public void updateFrameCount(long frameCount)
		{
			this._frameCount = frameCount;
		}

		public void clear()
        {
			this.setStartTime(0);
			this.setStartFrameCount(0);
			this.updateTime(0);
			this.updateFrameCount(0);
        }
	}
}
