
namespace fsync
{
	public class NetDelay
	{
		/// <summary>
		/// 延迟样本
		/// </summary>
		protected long[] samples = new long[FrameSyncConfig.Inst.NetFps];

		protected int insertIndex = 0;

		public void Put(long sample)
		{
			this.samples[insertIndex % samples.Length] = sample;
			insertIndex++;
		}

		public long GetNetDelayAve()
		{
			long dt = 0;
			foreach (var sample in samples)
			{
				dt += sample;
			}

			return dt / samples.Length;
		}

		protected long[] localSamples = new long[FrameSyncConfig.Inst.NetFps];
		protected int localIndex = 0;

		public void PutLocal(long sample)
		{
			this.localSamples[localIndex % localSamples.Length] = sample;
			localIndex++;
		}

		public long GetLocalDelayAve()
		{
			long dt = 0;
			foreach (var sample in localSamples)
			{
				dt += sample;
			}

			return dt / localSamples.Length;
		}

	}

}
