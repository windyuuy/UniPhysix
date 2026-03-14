
using UnityEngine;
using UnityEngine.UI;

namespace fsync
{
	public class UINetDelayComp : MonoBehaviour
	{
		protected NetDelay netDelay = null;
		public void SetNetDelayProxy(NetDelay netDelay)
		{
			this.netDelay = netDelay;
		}

		Text label = null;
		void Start()
		{
			label = this.GetComponent<Text>();
		}

		public float UpdateDelay = 0.5f;
		protected float lastUpdateTime = 0f;

		void Update()
		{
			if (UnityEngine.Time.time - lastUpdateTime > UpdateDelay)
			{
				lastUpdateTime = UnityEngine.Time.time;

				if (this.netDelay != null)
				{
					label.text = $"网络延迟: {this.netDelay.GetNetDelayAve()}, {this.netDelay.GetLocalDelayAve()}";
				}
			}
		}
	}

}
