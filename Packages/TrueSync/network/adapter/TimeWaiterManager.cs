using System.Collections.Generic;

#region 帧同步
#endregion

namespace fsync
{
	public class TimeWaiterManager
	{
		public static readonly TimeWaiterManager Inst = new TimeWaiterManager();
		protected List<ITimerWaiter> waiters = new List<ITimerWaiter>();

		protected static MonoScheduler schedulerObject = null;
		public void Init()
		{
			if (schedulerObject == null)
			{
				//schedulerObject = MonoScheduler.Create();
				//schedulerObject.ScheduleLate(() =>
				//{
				//	isAllDone = _isAllDone();
				//});
			}
		}

		public void Add(ITimerWaiter waiter)
		{
			this.waiters.Add(waiter);
		}
		public void Remove(ITimerWaiter waiter)
		{
			this.waiters.RemoveAll(p => p.getOID() == waiter.getOID());
		}

		public bool isAllDone = false;

		public void reset()
		{
			isAllDone = false;
		}
		protected bool _isAllDone()
		{
			var running = this.waiters.Find(p => p.IsComplete());
			return running == null;
		}

	}

}