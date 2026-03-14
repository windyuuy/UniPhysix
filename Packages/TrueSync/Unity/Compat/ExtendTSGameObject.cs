using UnityEngine;

namespace TrueSync
{
	public static class ExtendTSGameObject
	{

		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : UnityEngine.Component
		{
			var comp = gameObject.GetComponent<T>();
			if (comp == null)
			{
				comp = gameObject.AddComponent<T>();
			}
			return comp;
		}

		public static T GetOrAddComponent<T>(this Component component) where T : UnityEngine.Component
		{
			var comp = component.GetComponent<T>();
			if (comp == null)
			{
				comp = component.gameObject.AddComponent<T>();
			}
			return comp;
		}

		public static GameObject AttachTSTransform(this GameObject gameObject)
		{
			{
				var rawTransform = gameObject.transform;

				// var tscomp = gameObject.GetComponent<TSTransform>();
				var tscomp = gameObject.ReGetTransform();
				if (tscomp == null)
				{
					if (rawTransform.parent != null)
					{
						rawTransform.parent.gameObject.AttachTSTransform();
					}

					tscomp = gameObject.AddComponent<TSTransform>();
					TSTransform.markNewTSTransform(tscomp);
					if (rawTransform.parent != null)
					{
						tscomp.TSParent = rawTransform.parent.gameObject.GetTransform();
					}

					tscomp.localScale = rawTransform.localScale.ToTSVector();
					tscomp.localRotation = rawTransform.localRotation.ToTSQuaternion();
					tscomp.localPosition = rawTransform.localPosition.ToTSVector();
				}
				else
				{
					if (rawTransform.parent != null)
					{
						rawTransform.parent.gameObject.AttachTSTransform();
						tscomp.TSParent = rawTransform.parent.gameObject.GetTransform();
					}
				}
			}

			return gameObject;
		}
		public static void CopyColliderAttrs(TSCollider tscollider, Collider collider)
		{
			tscollider.isTrigger = collider.isTrigger;

			if (collider.material != null && tscollider.TSMaterial == null)
			{
				tscollider.UpdateTSMaterial();
			}
		}
		public static GameObject ForeachAttachMissingPhysicsComps(this GameObject gameObject)
		{
			if (gameObject == null)
			{
				return gameObject;
			}

			// {
			// 	var comps = gameObject.GetComponentsInChildren<CharacterController>(true);
			// 	foreach (var comp in comps)
			// 	{
			// 		comp.gameObject.AttachMissingPhysicsComps();
			// 	}
			// }
			{
				var comps = gameObject.GetComponentsInChildren<Collider>(true);
				foreach (var comp in comps)
				{
					comp.gameObject.AttachMissingPhysicsComps();
				}
			}
			{
				var comps = gameObject.GetComponentsInChildren<TSTransform>(true);
				foreach (var comp in comps)
				{
					comp.gameObject.AttachMissingPhysicsComps();
				}
			}
			return gameObject;
		}

		private static void AttachColliderMaterial<T>(GameObject gameObject, T collider) where T : Collider
		{
			if (collider.material != null)
			{
				var tsmaterial = gameObject.GetComponent<TSMaterial>();
				if (tsmaterial == null)
				{
					tsmaterial = gameObject.AddComponent<TSMaterial>();
					var material = collider.material;
					tsmaterial.friction = material.dynamicFriction;
					tsmaterial.restitution = material.bounciness;
				}
			}
		}

		public static GameObject AttachTSAnimPlayer(this GameObject gameObject)
		{
			{
				var comp = gameObject.GetComponent<UnityEngine.Playables.PlayableDirector>();
				if (comp != null)
				{
					gameObject.GetOrAddComponent<TrueSync.Anim.TSPlayableDirector>();
					gameObject.GetOrAddComponent<TrueSync.Anim.TSAnimPlayer>();
				}
			}
			return gameObject;
		}

		public static GameObject ForeachAttachTSAnimPlayer(this GameObject gameObject)
		{
			var comps = gameObject.GetComponentsInChildren<UnityEngine.Playables.PlayableDirector>(true);
			foreach (var comp in comps)
			{
				comp.gameObject.AttachTSAnimPlayer();
			}
			return gameObject;
		}

		public static GameObject AttachMissingPhysicsComps(this GameObject gameObject)
		{
			if (gameObject == null)
			{
				return gameObject;
			}
			{
				gameObject.AttachTSTransform();
				gameObject.AttachTSAnimPlayer();
			}

			{
				var comp = gameObject.GetComponent<Collider>();
				if (comp != null)
				{
					AttachColliderMaterial(gameObject, comp);
				}
			}

			{
				var comp = gameObject.GetComponent<SphereCollider>();
				if (comp != null)
				{
					var tscomp = gameObject.GetComponent<TSSphereCollider>();
					if (tscomp == null)
					{
						tscomp = gameObject.AddComponent<TSSphereCollider>();

						CopyColliderAttrs(tscomp, comp);
						tscomp.radius = comp.radius;
						tscomp.Center = comp.center.ToTSVector();
					}
				}
			}

			{
				var comp = gameObject.GetComponent<CharacterController>();
				if (comp != null)
				{
					{
						var tscomp = gameObject.GetComponent<TSBoxCollider>();
						if (tscomp == null)
						{
							tscomp = gameObject.AddComponent<TSBoxCollider>();

							CopyColliderAttrs(tscomp, comp);
							tscomp.Center = comp.center.ToTSVector();
							// tscomp.radius = comp.radius;
							// tscomp.length = comp.height - comp.radius * 2;
							tscomp.size = new TSVector(comp.radius * 2, comp.height, comp.radius * 2);
						}
					}
					{
						var tscomp = gameObject.GetComponent<TSRigidBody>();
						if (tscomp == null)
						{
							tscomp = gameObject.AddComponent<TSRigidBody>();

							tscomp.mass = 1;
							tscomp.drag = 0;
							tscomp.angularDrag = 0.05f;
							tscomp.useGravity = true;
							tscomp.isKinematic = false;
							tscomp.interpolation = TSRigidBody.InterpolateMode.Interpolate;
							tscomp.constraints = Physics3D.TSRigidBodyConstraints.FreezeRotationX
								| Physics3D.TSRigidBodyConstraints.FreezeRotationY
								| Physics3D.TSRigidBodyConstraints.FreezeRotationZ;
						}
					}
					comp.enabled = false;
				}
			}

			{
				var comp = gameObject.GetComponent<CapsuleCollider>();
				if (comp != null)
				{
					var tscomp = gameObject.GetComponent<TSCapsuleCollider>();
					if (tscomp == null)
					{
						tscomp = gameObject.AddComponent<TSCapsuleCollider>();

						CopyColliderAttrs(tscomp, comp);
						tscomp.radius = comp.radius;
						tscomp.Center = comp.center.ToTSVector();
						tscomp.length = comp.height - comp.radius * 2;
					}
				}
			}

			{
				var comp = gameObject.GetComponent<BoxCollider>();
				if (comp != null)
				{
					var tscomp = gameObject.GetComponent<TSBoxCollider>();
					if (tscomp == null)
					{
						tscomp = gameObject.AddComponent<TSBoxCollider>();

						CopyColliderAttrs(tscomp, comp);
						tscomp.Center = comp.center.ToTSVector();
						tscomp.size = comp.size.ToTSVector();
					}
				}
			}

			{
				var comp = gameObject.GetComponent<MeshCollider>();
				if (comp != null)
				{
					var tscomp = gameObject.GetComponent<TSMeshCollider>();
					if (tscomp == null)
					{
						tscomp = gameObject.AddComponent<TSMeshCollider>();

						CopyColliderAttrs(tscomp, comp);
						// Debug.Log($"setmesxh: {comp.gameObject.name}");
						tscomp.Mesh = comp.sharedMesh;
					}
				}
			}

			{
				var comp = gameObject.GetComponent<TerrainCollider>();
				if (comp != null)
				{
					var tscomp = gameObject.GetComponent<TSTerrainCollider>();
					if (tscomp == null)
					{
						tscomp = gameObject.AddComponent<TSTerrainCollider>();

						CopyColliderAttrs(tscomp, comp);
					}
				}
			}

			{
				var comp = gameObject.GetComponent<Rigidbody>();
				if (comp != null)
				{
					var tscomp = gameObject.GetComponent<TSRigidBody>();
					if (tscomp == null)
					{
						tscomp = gameObject.AddComponent<TSRigidBody>();

						tscomp.mass = comp.mass;
						tscomp.drag = comp.drag;
						tscomp.angularDrag = comp.angularDrag;
						tscomp.useGravity = comp.useGravity;
						tscomp.isKinematic = comp.isKinematic;
						tscomp.interpolation = (TSRigidBody.InterpolateMode)(int)comp.interpolation;
						tscomp.constraints = (Physics3D.TSRigidBodyConstraints)(int)comp.constraints;

					}
				}
			}

			return gameObject;
		}

		public static TSTransform ReferTransform(this GameObject gameObject)
		{
			return TSTransform.GetTSTransform(gameObject);
		}

		public static TSTransform ReferTransform(this Component comp)
		{
			return TSTransform.GetTSTransform(comp.gameObject);
		}

		public static TSTransform GetTransform(this GameObject gameObject)
		{
			// if (gameObject.GetComponent<Collider>() == null)
			// {
			//     if (gameObject.GetComponent<TSTransform>() == null)
			//     {
			// 		gameObject.AddComponent<TSTransform>();
			// 	}
			// }
			gameObject.AttachMissingPhysicsComps();
			// var trans = gameObject.GetComponent<TSTransform>();
			var trans = gameObject.ReferTransform();
			return trans;
		}

		public static TSTransform ReGetTransform(this GameObject gameObject)
		{
			var trans = TSTransform.ReGetTSTransform(gameObject);
			return trans;
		}

		public static TSTransform ReGetTransform(this Component comp)
		{
			var trans = TSTransform.ReGetTSTransform(comp.gameObject);
			return trans;
		}

		public static ICollider3D AsTSCollider(this Collider collider)
		{
			return new MyUECollider(collider);
		}

		public static TrueSync.Anim.TSAnimPlayer TSAnimPlayer(this UnityEngine.Playables.PlayableDirector director)
		{
			var tsAnimPlayer = director.GetComponent<TrueSync.Anim.TSAnimPlayer>();
			return tsAnimPlayer;
		}

		public static TrueSync.Anim.TSPlayableDirector TSProxy(this UnityEngine.Playables.PlayableDirector director)
		{
			var tsAnimPlayer = director.GetComponent<TrueSync.Anim.TSPlayableDirector>();
			return tsAnimPlayer;
		}

		public static TrueSync.Anim.UEPlayableDirector UEProxy(this UnityEngine.Playables.PlayableDirector director)
		{
			var tsAnimPlayer = director.GetOrAddComponent<TrueSync.Anim.UEPlayableDirector>();
			return tsAnimPlayer;
		}
		public static TrueSync.Anim.IPlayableDirector AnyTSProxy(this UnityEngine.Playables.PlayableDirector director)
		{
			TrueSync.Anim.IPlayableDirector director1 = director.TSProxy();
			if (director1 != null)
			{
				return director1;
			}
			director1 = director.UEProxy();
			return director1;
		}

	}

}