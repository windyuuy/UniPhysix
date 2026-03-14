using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace TrueSync
{
	public static class CoroutineManager
	{
		private static CoroutineScheduler sharedScheduler;
		public static CoroutineScheduler SharedScheduler
		{
			get
			{
				if (sharedScheduler == null)
				{
					TSPhysics.InitForce();
				}
				return sharedScheduler;
			}
            set
            {
				sharedScheduler = value;
            }
		}
	}
}
