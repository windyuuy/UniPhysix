using UnityEngine;
using System.Collections.Generic;
using System;
using TrueSync.Physics3D;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace TrueSync {

    /**
     *  @brief Manages the 3D physics simulation.
     **/
    public class PhysicsWorldManager : IPhysicsManager {

        public static PhysicsWorldManager instance;

        private World world;

        Dictionary<IBody, GameObject> gameObjectMap;

        Dictionary<RigidBody, Dictionary<RigidBody, TSCollision>> collisionInfo;

        /**
         *  @brief Property access to simulated gravity.
         **/
        public TSVector Gravity {
            get;
            set;
        }

        /**
         *  @brief Property access to speculative contacts.
         **/
        public bool SpeculativeContacts {
            get;
            set;
        }

        public FP LockedTimeStep {
            get;
            set;
        }

        // Use this for initialization
        public void Init() {
            ChecksumExtractor.Init(this);

            gameObjectMap = new Dictionary<IBody, GameObject>();
            collisionInfo = new Dictionary<RigidBody, Dictionary<RigidBody, TSCollision>>();

            CollisionSystemPersistentSAP collisionSystem = new CollisionSystemPersistentSAP();
            collisionSystem.EnableSpeculativeContacts = SpeculativeContacts;

            world = new World(collisionSystem);
            collisionSystem.world = world;

            world.physicsManager = this;
            world.Gravity = Gravity;

            world.Events.BodiesBeginCollide += CollisionEnter;
            world.Events.BodiesStayCollide += CollisionStay;
            world.Events.BodiesEndCollide += CollisionExit;

            world.Events.TriggerBeginCollide += TriggerEnter;
            world.Events.TriggerStayCollide += TriggerStay;
            world.Events.TriggerEndCollide += TriggerExit;

            world.Events.RemovedRigidBody += OnRemovedRigidBody;

            instance = this;

            AddRigidBodies();
        }

        public static bool autoSimulation = true;

        /**
         *  @brief Goes one step further on the physics simulation.
         **/
        public void UpdateStep() {
            if (autoSimulation)
            {
                world.Step(LockedTimeStep);
            }
        }

        public void UpdateStepForce(FP dt)
        {
			Profiler.BeginSample("UpdateStepForce");
			world.Step(dt);
			Profiler.EndSample();
        }

        /**
         *  @brief Instance of the current simulated world.
         **/
        public IWorld GetWorld() {
            return world;
        }

        void AddRigidBodies() {
            TSCollider[] bodies = GameObject.FindObjectsOfType<TSCollider>();
            List<TSCollider> sortedBodies = new List<TSCollider>(bodies);
            sortedBodies.Sort(UnityUtils.bodyComparer);

            for (int i = 0; i < sortedBodies.Count; i++) {
                AddBody(sortedBodies[i]);
            }
        }

        /**
         *  @brief Add a new RigidBody to the world.
         *  
         *  @param jRigidBody Instance of a {@link TSRigidBody}.
         **/
        public void AddBody(ICollider iCollider) {
            if (!(iCollider is TSCollider)) {
                Debug.LogError("You have a 2D object but your Physics 2D is disabled.");
                return;
            }

            TSCollider tsCollider = (TSCollider) iCollider;

            if (tsCollider._body != null) {
                //already added
                return;
            }

            TSRigidBody tsRB = tsCollider.GetComponent<TSRigidBody>();
            TSRigidBodyConstraints constraints = tsRB != null ? tsRB.constraints : TSRigidBodyConstraints.None;

            tsCollider.Initialize();
            world.AddBody(tsCollider._body);
            gameObjectMap[tsCollider._body] = tsCollider.gameObject;
            // transfer layer
            tsCollider._body.layer = tsCollider.gameObject.layer;

            if (tsCollider.gameObject.transform.parent != null && tsCollider.gameObject.transform.parent.GetComponentInParent<TSCollider>() != null) {
                TSCollider parentCollider = tsCollider.gameObject.transform.parent.GetComponentInParent<TSCollider>();
				world.AddConstraint(new ConstraintHierarchy(parentCollider.Body, tsCollider._body,
					// (tsCollider.GetComponent<TSTransform>().position + tsCollider.ScaledCenter)
					(tsCollider.ReGetTransform().position + tsCollider.ScaledCenter)
						// - (parentCollider.GetComponent<TSTransform>().position + parentCollider.ScaledCenter)
						- (parentCollider.ReGetTransform().position + parentCollider.ScaledCenter)
					));
            }

            tsCollider._body.FreezeConstraints = constraints;
        }

        public void RemoveBody(IBody iBody) {
            world.RemoveBody((RigidBody) iBody);
            // reset layer
            iBody.layer = -1;
        }

        public void OnRemoveBody(System.Action<IBody> OnRemoveBody){
            world.Events.RemovedRigidBody += delegate (RigidBody rb) {
                OnRemoveBody(rb);
            };
        }

        public bool Raycast(Physics3D.RigidBody body, ref TSRay ray, out TSRaycastHit hitInfo, FP maxDistance)
		{
			ref TSVector origin = ref ray.origin;

            TSVector.Multiply(ref ray.direction, maxDistance, out __cached_direction);

			if (Raycast(body, ref origin, ref __cached_direction, out TSVector hitNormal, out FP hitFraction))
			{
				UnityEngine.GameObject other = PhysicsManager.instance.GetGameObject(body);
				// TSRigidBody bodyComponent = other.GetComponent<TSRigidBody>();
				// TSCollider colliderComponent = other.GetComponent<TSCollider>();
				// TSTransform transformComponent = other.GetComponent<TSTransform>();
				TSTransform transformComponent = other.ReferTransform();
				TSCollider colliderComponent = transformComponent.tsCollider;
				TSRigidBody bodyComponent = transformComponent.tsRigidBody;
				TSRaycastHit.Init(out hitInfo,bodyComponent, colliderComponent, transformComponent, ref hitNormal, ref origin, ref __cached_direction, hitFraction);
				return true;
			}

            TSRaycastHit.Reset(out hitInfo);
            return false;
		}

		public bool Raycast(Physics3D.RigidBody body, ref TSVector rayOrigin, ref TSVector rayDirection, out TSVector normal, out FP fraction)
		{
			bool result = world.CollisionSystem.Raycast(body, ref rayOrigin, ref rayDirection, out normal, out fraction);
			return result;
		}

		public bool Raycast(ref TSVector rayOrigin, ref TSVector rayDirection, RaycastCallback raycast, out IBody body, out TSVector normal, out FP fraction)
		{
            RigidBody rb;
			bool result = world.CollisionSystem.Raycast(ref rayOrigin, ref rayDirection, raycast, out rb, out normal, out fraction);
            body = rb;

            return result;
        }

        public bool Raycast(ref TSVector rayOrigin, ref TSVector rayDirection, RaycastCallback raycast, int layerMask, out IBody body, out TSVector normal, out FP fraction)
        {
            RigidBody rb;
            bool result = world.CollisionSystem.Raycast(ref rayOrigin, ref rayDirection, raycast, layerMask, out rb, out normal, out fraction);
            body = rb;
            return result;
        }

        public bool Raycast(ref TSRay ray, out TSRaycastHit hit, FP maxDistance, RaycastCallback callback = null) {
            IBody hitBody;
            TSVector hitNormal;
            FP hitFraction;

            TSVector origin = ray.origin;
            TSVector direction = ray.direction;

            if (Raycast(ref origin, ref direction, callback, out hitBody, out hitNormal, out hitFraction)) {
                if (hitFraction <= maxDistance) {
                    GameObject other = PhysicsManager.instance.GetGameObject(hitBody);
					// TSRigidBody bodyComponent = other.GetComponent<TSRigidBody>();
					// TSCollider colliderComponent = other.GetComponent<TSCollider>();
					// TSTransform transformComponent = other.GetComponent<TSTransform>();

					TSTransform transformComponent = other.ReferTransform();
					TSCollider colliderComponent = transformComponent.tsCollider;
					TSRigidBody bodyComponent = transformComponent.tsRigidBody;
                    TSRaycastHit.Init(out hit, bodyComponent, colliderComponent, transformComponent, ref hitNormal, ref ray.origin, ref ray.direction, hitFraction);
                    return true;
                }
            } else {
                direction *= maxDistance;
                if (Raycast(ref origin, ref direction, callback, out hitBody, out hitNormal, out hitFraction)) {
                    GameObject other = PhysicsManager.instance.GetGameObject(hitBody);
					// TSRigidBody bodyComponent = other.GetComponent<TSRigidBody>();
					// TSCollider colliderComponent = other.GetComponent<TSCollider>();
					// TSTransform transformComponent = other.GetComponent<TSTransform>();

					TSTransform transformComponent = other.ReferTransform();
					TSCollider colliderComponent = transformComponent.tsCollider;
					TSRigidBody bodyComponent = transformComponent.tsRigidBody;
                    TSRaycastHit.Init(out hit, bodyComponent, colliderComponent, transformComponent, ref hitNormal, ref ray.origin, ref direction, hitFraction);
                    return true;
                }
            }

            TSRaycastHit.Reset(out hit);
            return false;
        }

        private TSVector __cached_direction;
        private TSVector __cached_hitNormal;
        private FP __cached_hitFraction;

        public bool Raycast(ref TSVector rayOrigin, ref TSVector rayDirection,out TSRaycastHit hit, FP maxDistance, int layerMask, RaycastCallback callback = null)
        {
            TSVector.Multiply(ref rayDirection, maxDistance, out __cached_direction);
            if (Raycast(ref rayOrigin, ref __cached_direction, callback, layerMask, out var hitBody, out __cached_hitNormal, out __cached_hitFraction))
            {
				Profiler.BeginSample("getcomps");
                GameObject other = PhysicsManager.instance.GetGameObject(hitBody);
				// TSTransform transformComponent = other.GetComponent<TSTransform>();
				// TSRigidBody bodyComponent = other.GetComponent<TSRigidBody>();
				// TSCollider colliderComponent = other.GetComponent<TSCollider>();

				TSTransform transformComponent = other.ReferTransform();
				TSCollider colliderComponent = transformComponent.tsCollider;
				TSRigidBody bodyComponent = transformComponent.tsRigidBody;
				TSRaycastHit.Init(out hit, bodyComponent, colliderComponent, transformComponent, ref __cached_hitNormal, ref rayOrigin, ref __cached_direction, __cached_hitFraction);
				Profiler.EndSample();
				return true;
            }

            TSRaycastHit.Reset(out hit);
            return false;
        }

		public void ClearCollisionCache()
		{
			this.world.CollisionSystem.ClearCollisionCache();
		}

		public bool Raycast(ref TSRay ray, out TSRaycastHit hit, FP maxDistance, int layerMask, RaycastCallback callback = null)
        {
            IBody hitBody;
            TSVector hitNormal;
            FP hitFraction;

            TSVector origin = ray.origin;
            TSVector direction = ray.direction;

            direction *= maxDistance;
            if (Raycast(ref origin, ref direction, callback, layerMask, out hitBody, out hitNormal, out hitFraction))
            {
                GameObject other = PhysicsManager.instance.GetGameObject(hitBody);
				// TSRigidBody bodyComponent = other.GetComponent<TSRigidBody>();
				// TSCollider colliderComponent = other.GetComponent<TSCollider>();
				// TSTransform transformComponent = other.GetComponent<TSTransform>();

				TSTransform transformComponent = other.ReferTransform();
				TSCollider colliderComponent = transformComponent.tsCollider;
				TSRigidBody bodyComponent = transformComponent.tsRigidBody;
                TSRaycastHit.Init(out hit, bodyComponent, colliderComponent, transformComponent, ref hitNormal, ref ray.origin, ref direction, hitFraction);
                return true;
            }

            TSRaycastHit.Reset(out hit);
            return false;
        }

		public virtual bool CheckRigidBody(RigidBody body, int layerMask, TrueSync.Physics3D.QueryTriggerInteraction queryTriggerInteraction, TrueSync.Physics3D.CollisionDetectedHandler handler = null)
		{
			return this.world.CollisionSystem.CheckRigidBody(body, layerMask, queryTriggerInteraction, handler);
		}

		public virtual bool CheckCapsule(ref TSVector start, ref TSVector end, FP radius, int layerMask, TrueSync.Physics3D.QueryTriggerInteraction queryTriggerInteraction, bool useCache = false)
		{
			return this.world.CollisionSystem.CheckCapsule(ref start, ref end, radius, layerMask, queryTriggerInteraction, useCache);
		}

		public virtual bool CheckSphere(ref TSVector position, FP radius, int layerMask, TrueSync.Physics3D.QueryTriggerInteraction queryTriggerInteraction, bool useCache = false)
		{
			return this.world.CollisionSystem.CheckSphere(ref position, radius, layerMask, queryTriggerInteraction, useCache);
		}

		private void OnRemovedRigidBody(RigidBody body) {
            GameObject go = gameObjectMap[body];

            if (go != null) {
                GameObject.Destroy(go);
            }
        }

        private void CollisionEnter(Contact c) {
            CollisionDetected(c.body1, c.body2, c, "OnSyncedCollisionEnter");
        }

        private void CollisionStay(Contact c) {
            CollisionDetected(c.body1, c.body2, c, "OnSyncedCollisionStay");
        }

        private void CollisionExit(RigidBody body1, RigidBody body2) {
            CollisionDetected(body1, body2, null, "OnSyncedCollisionExit");
        }

        private void TriggerEnter(Contact c) {
            CollisionDetected(c.body1, c.body2, c, "OnSyncedTriggerEnter");
        }

        private void TriggerStay(Contact c) {
            CollisionDetected(c.body1, c.body2, c, "OnSyncedTriggerStay");
        }

        private void TriggerExit(RigidBody body1, RigidBody body2) {
            CollisionDetected(body1, body2, null, "OnSyncedTriggerExit");
        }

        private void CollisionDetected(RigidBody body1, RigidBody body2, Contact c, string callbackName) {
			// 是否本托管游戏对象
			if (!gameObjectMap.ContainsKey(body1) || !gameObjectMap.ContainsKey(body2)) {
                return;
            }

            GameObject b1 = gameObjectMap[body1];
            GameObject b2 = gameObjectMap[body2];

			// 是否有效托管游戏对象
			if (b1 == null || b2 == null) {
                return;
            }

			// TODO: 判断collision是否需要 syncDirtyPhysicsTransform

			b1.SendMessage(callbackName, GetCollisionInfo(body1, body2, c), SendMessageOptions.DontRequireReceiver);
            b2.SendMessage(callbackName, GetCollisionInfo(body2, body1, c), SendMessageOptions.DontRequireReceiver);

			TrueSyncManager.UpdateCoroutines ();
            TSPhysics.syncDirtyDelayedTransform();
        }

        private TSCollision GetCollisionInfo(RigidBody body1, RigidBody body2, Contact c) {
            if (!collisionInfo.ContainsKey(body1)) {
                collisionInfo.Add(body1, new Dictionary<RigidBody, TSCollision>());
            }

            Dictionary<RigidBody, TSCollision> collisionInfoBody1 = collisionInfo[body1];

            TSCollision result = null;

            if (collisionInfoBody1.ContainsKey(body2)) {
                result = collisionInfoBody1[body2];
            } else {
                result = new TSCollision();
				// 缓存碰撞结果
				collisionInfoBody1.Add(body2, result);
            }


            result.Update(gameObjectMap[body2], c);

            return result;
        }

        /**
         *  @brief Get the GameObject related to a specific RigidBody.
         *  
         *  @param rigidBody Instance of a {@link RigidBody}
         **/
        public GameObject GetGameObject(IBody rigidBody) {
            if (!gameObjectMap.ContainsKey(rigidBody)) {
                return null;
            }

            return gameObjectMap[rigidBody];
        }

        public int GetBodyLayer(IBody body) {
            // GameObject go = GetGameObject(body);
            // if (go == null) {
            //     return -1;
            // }

            // return go.layer;
            return body.layer;
        }

        /**
         *  @brief Check if the collision between two RigidBodies is enabled.
         *  
         *  @param rigidBody1 First {@link RigidBody}
         *  @param rigidBody2 Second {@link RigidBody}
         **/
        public bool IsCollisionEnabled(IBody rigidBody1, IBody rigidBody2) {
            return LayerCollisionMatrix.CollisionEnabled(gameObjectMap[rigidBody1], gameObjectMap[rigidBody2]);
        }

        public IWorldClone GetWorldClone() {
            return new WorldClone();
        }

    }

}