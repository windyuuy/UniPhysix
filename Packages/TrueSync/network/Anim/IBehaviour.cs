#region 程序集 UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// UnityEngine.CoreModule.dll
#endregion


namespace UnityEngine
{
	//
	// 摘要:
	//     Behaviours are Components that can be enabled or disabled.
	public interface IBehaviour : IComponent
	{
		//
		// 摘要:
		//     Enabled Behaviours are Updated, disabled Behaviours are not.
		bool enabled { get; set; }
		//
		// 摘要:
		//     Has the Behaviour had active and enabled called?
		bool isActiveAndEnabled { get; }
	}
}