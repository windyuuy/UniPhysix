

namespace fsync
{
	public static class TimeUtil
	{
		public static long ConvFrameIdToServerTime(ulong frameCount)
		{
			return (long)(1382300 + (1000 * frameCount) / (ulong)FrameSyncConfig.Inst.NetFps);
		}
	}
}
