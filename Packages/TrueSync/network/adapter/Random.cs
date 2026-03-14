
#region 帧同步
using TrueSync;
using TSFloat = TrueSync.FP;
#endregion

namespace fsync
{
	public sealed class Random
	{
		public static ulong Seed
		{
			get => seed;
			set
			{
				UnityEngine.Debug.Log($"genRandom: set random seed: {value}");
				seed = value;
				inc = value;
			}
		}
		private static ulong seed = 0;
		private static ulong inc = 0;

		public static ulong genIndex = 0;
		public static TSFloat value
		{
			get
			{
				var inc0 = inc;

				inc = (inc * 9301 + 49297) % 233280;
				var rd = (TSFloat)(int)inc / (TSFloat)233280.0f;

#if false
				genIndex++;
				UnityEngine.Debug.LogWarning(
					$"genRandom: genIndex:{genIndex}, FrameCount:{GameObjectManager.inst.NetTimer.frameCount}, inc0:{inc0}, inc:{inc} -> {rd}");
#endif
				return rd;
			}
		}

		//
		// 摘要:
		//     Return a random integer number between min [inclusive] and max [exclusive] (Read
		//     Only).
		//
		// 参数:
		//   min:
		//
		//   max:
		public static int Range(int min, int max)
		{
			var n = min + (max - min) * value;
			return TSMath.FloorToInt(n);
		}

		//
		// 摘要:
		//     Return a random float number between min [inclusive] and max [inclusive] (Read
		//     Only).
		//
		// 参数:
		//   min:
		//
		//   max:
		public static TSFloat Range(TSFloat min, TSFloat max)
		{
			var n = min + (max - min) * value;
			return n;
		}

	}

}
