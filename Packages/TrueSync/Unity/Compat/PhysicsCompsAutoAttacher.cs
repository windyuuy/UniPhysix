using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrueSync
{
	public class PhysicsCompsAutoAttacher : MonoBehaviour
	{
		void Awake()
		{
			this.gameObject.ForeachAttachMissingPhysicsComps();
		}
	}

}
