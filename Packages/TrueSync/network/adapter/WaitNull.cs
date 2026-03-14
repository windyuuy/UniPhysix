#region 帧同步
#endregion

namespace fsync
{
	/// <summary>
	/// 需要用 `yield return new WaitNull()` 代替 `yield return null`, 否则会出现调度不确定性.
	/// </summary>
	public class WaitNull : WaitForSeconds
	{
		public WaitNull() : base(0)
		{
		}
	}

}