
namespace fsync
{
	/// <summary>
	/// 帧同步配置
	/// </summary>
	public class FrameSyncConfig
	{
		public static FrameSyncConfig Inst = new FrameSyncConfig();

		/// <summary>
		/// 网络帧率
		/// </summary>
		public readonly int NetFps = 15;
		public readonly float NetFDT = 0.067f;
		public readonly float NetFDT_MIN = 0.066f;
	}
}
