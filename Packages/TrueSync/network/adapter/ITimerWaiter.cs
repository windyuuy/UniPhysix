#region 帧同步
#endregion

namespace fsync
{
	public interface ITimerWaiter
	{
		long getOID();
		bool IsComplete();
	}

}