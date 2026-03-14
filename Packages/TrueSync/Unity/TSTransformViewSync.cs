
using UnityEngine;

namespace TrueSync
{
	public class TSTransformViewSync : MonoBehaviour
	{
		public TSTransform tsTransform;

		/// <summary>
		/// 更新view
		/// - 不直接使用update, 避免时序问题
		/// </summary>
		public virtual void UpdateView()
		{
			tsTransform.SyncViewWithLerp();
		}
	}
}
