
using System;

namespace TrueSync
{
	public partial class TSTransform
	{

		#region compatitable

		public TSTransform GetChild(int index)
		{
			return this.tsChildren[index];
		}
		[System.Obsolete("warning use Transform.childCount instead (UnityUpgradable) -> Transform.childCount", false)]
		public int GetChildCount()
		{
			return tsChildren.Count;
		}
		public int childCount => tsChildren.Count;

		public TSTransform Find(string n)
		{
			var trans = this.transform.Find(n);
			if (trans != null)
			{
				var tstrans = trans.gameObject.GetTransform();
				if (tstrans == null)
				{
					trans.gameObject.AttachTSTransform();
					tstrans = trans.gameObject.GetTransform();
				}
				return tstrans;
			}
			return null;

			//return this.transform.Find(n).gameObject.GetTransform();
		}
		[System.Obsolete("FindChild has been deprecated. Use Find instead (UnityUpgradable) -> Find([mscorlib] System.String)", false)]
		public TSTransform FindChild(string n)
		{
			return Find(n);
		}

		public int GetSiblingIndex()
		{
			return this.tsParent.tsChildren.IndexOf(this);
		}

		//
		// 摘要:
		//     Unparents all children.
		public void DetachChildren()
		{
			while (tsChildren.Count > 0)
			{
				tsChildren[0].tsParent = null;
			}
		}

		public bool IsChildOf(TSTransform parent)
		{
			return parent.tsChildren.Contains(this);
		}

		public TSTransform[] children => this.tsChildren.ToArray();

		#endregion
	}

}
