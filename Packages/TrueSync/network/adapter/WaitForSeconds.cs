
using System.Diagnostics;
using System.Collections;

#region 帧同步
using WaitForSeconds = fsync.WaitForSeconds;
using Random = fsync.Random;
using TrueSync;
using Vector2 = TrueSync.TSVector2;
using Vector3 = TrueSync.TSVector;
using Mathf = TrueSync.TSMath;
using Quaternion = TrueSync.TSQuaternion;
using TSFloat = TrueSync.FP;
#endregion

namespace fsync
{

	/// <summary>
	/// 游戏内代替UnityEngine.WaitForSeconds, 否则协程会出现不确定性
	/// - 暂时只能保证分布式一致性和跨帧时序, 无法保证严格的帧内时序
	/// </summary>
	public class WaitForSeconds : IEnumerator, ITimerWaiter
	{
		protected INetTimer sharedTimer => GameObjectManager.Inst.NetTimer;

		public object Current => null;

		/// <summary>
		/// 等待时长
		/// </summary>
		protected TSFloat duration;
		protected TSFloat startTime;
		/// <summary>
		/// 开始计时的帧序
		/// - 保证至少要下一帧才执行
		/// </summary>
		protected long startFrame = 0;

		protected long oidAcc = 1;
		public long oid = 0;

		/// <summary>
		/// 等待若干秒
		/// </summary>
		/// <param name="duration">等待时长, 单位秒</param>
		public WaitForSeconds(TSFloat duration)
		{
			this.startTime = sharedTimer.time;
			this.duration = duration;

			this.startFrame = sharedTimer.frameCount;

			this.oid = oidAcc++;
			// TimeWaiterManager.Inst.Add(this);
		}

		/// <summary>
		/// 等待结束的那帧
		/// - 使用帧序去判断避免基于渲染帧的协程调度导致不确定性.
		/// - TODO: 可以优化为更细碎的确定性调度, 优化性能消耗分配
		/// </summary>
		/// <value></value>
		public long WaitFrameCount
		{
			get
			{
				var finishTime = this.startTime + this.duration;
				long frameCount = Mathf.CeilToInt(finishTime * FrameSyncConfig.Inst.NetFps);
				if (frameCount <= startFrame)
				{
					frameCount = startFrame + 1;
				}
				return frameCount;
			}
		}

		public TSFloat WaitTime
		{
			get
			{
				TSFloat finishTime;
				if (this.duration <= 0)
				{
					finishTime = this.startTime;
				}
				else
				{
					finishTime = this.startTime + this.duration;
				}
				return finishTime;
			}
		}

		public bool IsComplete()
		{
			// return sharedTimer.frameCount >= this.WaitFrameCount;
			var waitTime = this.WaitTime;
			if (waitTime < sharedTimer.time)
			{
				return true;
			}
			else if (waitTime == sharedTimer.time && this.duration != 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public virtual bool MoveNext()
		{
			// if (sharedTimer.frameCount >= this.WaitFrameCount)
			if (IsComplete())
			{
				// TimeWaiterManager.Inst.Remove(this);
				// 条件满足则返回false
				return false;
			}
			else
			{
				return true;
			}
		}

		public void Reset()
		{
			this.startTime = sharedTimer.time;
		}

		public long getOID()
		{
			return this.oid;
		}
	}

}