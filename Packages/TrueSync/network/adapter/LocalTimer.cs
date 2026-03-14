

namespace fsync
{
	using TTimeStamp = System.Int64;
	using TSFloat = TrueSync.FP;

	/// <summary>
	/// 网络时间管理
	/// - 使用前需要设置 setStartTime 和 updateTime
	/// </summary>
	public class TLocalTimer : INetTimer
	{

		/// <summary>
		/// 当前帧开始时间
		/// - 为游戏时长计时
		/// - 相当于 Time.time
		/// </summary>
		/// <returns></returns>
		public TSFloat time => UnityEngine.Time.time;

		public TSFloat deltaTime
		{
			get
			{
				return UnityEngine.Time.deltaTime;
			}
		}

		public TSFloat timeSinceLevelLoad => UnityEngine.Time.timeSinceLevelLoad;

		public TSFloat timeScale
		{
			get => UnityEngine.Time.timeScale;
			set => UnityEngine.Time.timeScale = value.AsFloat();
		}

		/// <summary>
		/// 逻辑帧率
		/// </summary>
		public long frameCount => UnityEngine.Time.frameCount;

        public TSFloat realtimeSinceStartup => UnityEngine.Time.realtimeSinceStartup;

        public void setStartFrameCount(long frameCount)
		{
		}

		public void setStartTime(ulong time)
		{
		}

		public void updateFrameCount(long frameCount)
		{
		}

		public void updateTime(ulong time)
		{
		}

		public void clear()
        {

        }
	}
}
