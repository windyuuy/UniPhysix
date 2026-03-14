
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace fsync
{
	public class AsyncTimer
	{
		/// <summary>
		/// 延迟(ms)
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static Task Delay(float dt)
		{
			return AsyncTask.Run((resolve, reject) =>
			{
				var timer = new System.Timers.Timer();
				timer.Elapsed += (s, e) =>
				{
					timer.Dispose();
					resolve();
				};
				timer.Interval = dt;
				timer.AutoReset = false;
				timer.Enabled = true;
			});
		}
	}
}
