
using UnityEngine;
using UnityEngine.UI;

namespace fsync
{

	public class UINetDelay
	{
		public static readonly UINetDelay Inst = new UINetDelay();

		public void CreateUI(NetDelay netDelay)
		{
			var ui = GameObject.Find("UIRoot(Clone)/UILayer/DebugLayer/SharedUINetDelay");
			if (ui == null)
			{
				var root = GameObject.Find("UIRoot(Clone)/UILayer/DebugLayer");
				ui = new GameObject("SharedUINetDelay");
				ui.transform.parent = root.transform;

				var rectTransform = ui.AddComponent<RectTransform>();
				rectTransform.localPosition = new Vector3(325, 291);

				ui.AddComponent<CanvasRenderer>();

				var text = ui.AddComponent<Text>();
				text.text = "网络延迟: --";
				text.fontSize = 40;

				ui.AddComponent<UINetDelayComp>();
			}
			var comp = ui.GetComponent<UINetDelayComp>();
			comp.SetNetDelayProxy(netDelay);
		}

	}

}
