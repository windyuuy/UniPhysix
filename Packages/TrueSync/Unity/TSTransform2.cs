using UnityEngine;
using System.Collections.Generic;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace TrueSync
{
	public partial class TSTransform
	{

		// 添加转换矩阵缓存, 提高坐标更新效率
		// 由子节点触发父节点更新
		protected TSMatrix4x4 _cachedLocalToWorldMatrix;
		protected TSMatrix4x4 _cachedWorldToLocalMatrix;
		protected TSMatrix4x4 _cachedMyMatrix;

		//通过标志位避免每次设置方位都要递归遍历更新子节点

		protected bool isPosDirty = false;
		protected bool isRotateDirty = false;
		protected bool isScaleDirty = false;
		protected bool isTransformMatrixDirty = false;
		protected bool isReverseTransformMatrixDirty = false;
		protected bool isSelfTransformMatrixDirty = false;
		protected bool isSelfTransformDirty = false;
		protected bool isMyTransformDirty()
		{
			return this.isSelfTransformDirty;
		}
		protected void markMatrixDirty()
		{
			// TODO: 合并过多标志位
			isSelfTransformMatrixDirty = true;
			isTransformMatrixDirty = true;
			isReverseTransformMatrixDirty = true;
		}
		protected void markAllMainFactorDirty()
		{
			isPosDirty = true;
			isRotateDirty = true;
			isScaleDirty = true;
			isSelfTransformDirty = true;

			markMatrixDirty();
		}
		protected void markPosDirty()
		{
			isPosDirty = true;
			isSelfTransformDirty = true;

			markMatrixDirty();

			foreach (var child in tsChildren)
			{
				child.markPosDirty();
			}
		}
		protected void markRotateDirty()
		{
			isRotateDirty = true;
			isSelfTransformDirty = true;

			markMatrixDirty();

			foreach (var child in tsChildren)
			{
				child.markPosDirty();
				child.markRotateDirty();
			}
		}
		protected void markPosAndRotateDirty()
		{
			isPosDirty = true;
			markRotateDirty();
		}
		protected void markScaleDirty()
		{
			isScaleDirty = true;
			isSelfTransformDirty = true;

			markMatrixDirty();

			foreach (var child in tsChildren)
			{
				child.markPosDirty();
				child.markScaleDirty();
			}
		}

		public void UpdateDirtyTransform()
		{
			if (isSelfTransformDirty)
			{
				if (isScaleDirty)
				{
					this._updateLossyScale(false);
					isScaleDirty = false;
				}
				if (isRotateDirty)
				{
					this._updateRotation(false);
					isRotateDirty = false;
				}
				if (isPosDirty)
				{
					this._updatePosition(false);
					isPosDirty = false;
				}
				isSelfTransformDirty = false;
			}
		}


		[SerializeField]
		[HideInInspector]
		[AddTracking]
		private TSVector _localPosition;

		/// <summary>
		/// 由全局坐标换算得到局部坐标
		/// - 非物理模拟阶段, 局部坐标为主体, 向全局坐标换算.
		/// </summary>
		/// <value></value>
		public TSVector localPosition
		{
			get
			{
				return _localPosition;
			}
			set
			{
				if (_localPosition != value)
				{
					_localPosition = value;
					// _updatePosition(true);
					markPosDirty();
				}
				syncViewPosition();
			}
		}

		[SerializeField]
		[HideInInspector]
		[AddTracking]
		private TSVector _position;

		/**
        *  @brief Property access to position. 
        *  
        *  It works as proxy to a Body's position when there is a collider attached.
        **/
		public TSVector position
		{
			get
			{
				// 碰撞会影响位置
				//if (tsCollider != null && tsCollider.Body != null)
				//{
				//	// position = tsCollider.Body.TSPosition - scaledCenter;
				//	return tsCollider.Body.TSPosition - scaledCenter;
				//}
				return GetPosition();
			}
			set
			{
				SetPosition(ref value);
				syncViewPosition();
			}
		}
		
		public ref TSVector GetPosition(){

			UpdateDirtyTransform();
			return ref _position;
		}

		public void SetPosition(TSVector value)
		{
			SetPosition(ref value, true);
		}
		public void SetPosition(ref TSVector value, bool recurse = true)
		{
			if (isMyTransformDirty())
			{
				toLocalPosition(ref value, out var localPos);
				if (_localPosition != localPos)
				{
					_localPosition = localPos;
					// _updatePosition(recurse);
					markPosDirty();
				}
			}
			else
			{
				if (_position != value)
				{
					toLocalPosition(ref value, out _localPosition);
					markPosDirty();
				}
			}
		}

		public void MoveBy(TSVector value, bool needLerp = true)
		{
			if (needLerp)
			{
				TSVector.Add(ref GetPosition(), ref value, out __sharedPos);
				this.SetPosition(ref __sharedPos);
			}
			else
			{
				// TODO: 可以优化
				this.position += value;
			}
		}

		public void ScaleTo(TSVector value, bool needLerp = true)
		{
			if (needLerp)
			{
				this.SetLocalScale(ref value);
			}
			else
			{
				this.localScale = value;
			}
		}

		public void RotateTo(TSQuaternion value, bool needLerp = true)
        {
			RotateTo(ref value, needLerp);
        }
		public void RotateTo(ref TSQuaternion value, bool needLerp = true)
		{
			if (needLerp)
			{
				this.SetRotation(ref value);
			}
			else
			{
				this.rotation = value;
			}
		}

		public void RotateToEuler(TSVector value, bool needLerp = true)
		{
			if (needLerp)
			{
				this.SetRotation(TSQuaternion.Euler(value));
			}
			else
			{
				this.rotation = TSQuaternion.Euler(value);
			}
		}

		protected void _getRigidBodyPosition(out TSVector pos)
		{
			// if (tsCollider != null && tsCollider.Body != null)
			// return tsCollider.Body.TSPosition - scaledCenter;
			TSVector.Subtract(ref tsCollider.Body.ReferTSPosition, ref scaledCenter, out pos);
		}

		static private TSVector __sharedCenter;

		protected void _updatePosition(bool recurse)
		{
			toWorldPosition(ref _localPosition, out _position);

			if (tsCollider != null && tsCollider._body != null)
			{
				TSVector.Add(ref _position, ref tsCollider.ScaledCenter, out __sharedPos);
				tsCollider.Body.SetTSPosition(ref __sharedPos);
				// tsCollider.Body.TSPosition = _position + scaledCenter;
			}

			if (recurse)
			{
				updateChildPosition();
			}
		}

		public void toLocalPosition(ref TSVector value, out TSVector lPos)
		{
			if (this.tsParent)
			{
				tsParent.InverseTransformPoint(ref value, out lPos);
			}
			else
			{
				// return value;
				lPos = value;
			}
		}

		public void toWorldPosition(ref TSVector value, out TSVector wPos)
		{
			if (tsParent)
			{
				tsParent.TransformPoint(ref value, out wPos);
			}
			else
			{
				wPos = value;
			}
		}

		[SerializeField]
		[HideInInspector]
		[AddTracking]
		private TSQuaternion _localRotation;

		/**
         *  由全局旋转换算为局部旋转
         **/
		public TSQuaternion localRotation
		{
			get
			{
				return _localRotation;
			}
			set
			{
				if (_localRotation != value)
				{
					_localRotation = value;

					// _updateRotation(true);
					markRotateDirty();
				}
				syncViewRotation();
			}
		}

		[SerializeField]
		[HideInInspector]
		[AddTracking]
		private TSQuaternion _rotation;

		/**
        *  @brief Property access to rotation. 
        *  
        *  It works as proxy to a Body's rotation when there is a collider attached.
        **/
		public TSQuaternion rotation
		{
			get
			{
				// 碰撞会影响旋转
				//if (tsCollider != null && tsCollider.Body != null)
				//{
				//	// rotation = TSQuaternion.CreateFromMatrix(tsCollider.Body.TSOrientation);
				//	return TSQuaternion.CreateFromMatrix(tsCollider.Body.TSOrientation);
				//}
				return GetRotation();
			}
			set
			{
				SetRotation(ref value);
				syncViewRotation();
			}
		}

		public ref TSQuaternion GetRotation(){
			UpdateDirtyTransform();
			return ref _rotation;
		}

		public void SetRotation(TSQuaternion value, bool recurse = true)
		{
			SetRotation(ref value, recurse);
		}
		public void SetRotation(ref TSQuaternion value, bool recurse = true)
		{
			if (isMyTransformDirty())
			{
				toLocalRotation(ref value, out var localRotate);
				if (this._localRotation != localRotate)
				{
					this._localRotation = localRotate;
					markRotateDirty();
				}
			}
			else
			{
				if (this._rotation != value)
				{
					toLocalRotation(ref value, out this._localRotation);
					this.markRotateDirty();
				}
			}
			// _updateRotation(recurse);
		}

		protected void _getRigidBodyRotation(out TSQuaternion rot)
		{
			// if (tsCollider != null && tsCollider.Body != null)
			var orien = tsCollider.Body.TSOrientation;
			TSQuaternion.CreateFromMatrix(ref orien, out rot);
		}

		protected void _updateRotation(bool recurse)
		{
			toWorldRotation(ref _localRotation, out _rotation);

			if (tsCollider != null && tsCollider.Body != null)
			{
				tsCollider.Body.TSOrientation = TSMatrix.CreateFromQuaternion(_rotation);
			}

			if (recurse)
			{
				updateChildRotation();
			}
		}

		protected void toWorldRotation(ref TSQuaternion value, out TSQuaternion worldRot)
		{
			if (this.tsParent == null)
			{
				worldRot = value;
			}
			else
			{
				// 此顺序和unity保持一致
				// worldRot = this.tsParent.rotation * value;
				TSQuaternion.Multiply(ref this.tsParent.GetRotation(),ref value,out worldRot);
			}
		}

		protected void toLocalRotation(ref TSQuaternion value, out TSQuaternion localRot)
		{
			if (this.tsParent == null)
			{
				localRot = value;
			}
			else
			{
				var parentRot = this.tsParent.rotation;
				// TODO: 优化除法调用
				// localRot = value / parentRot;
				TSQuaternion.Divide(ref value, ref parentRot, out localRot);
			}
		}

		static private TSVector __sharedPos;
		static private TSQuaternion __sharedRot;
		/// <summary>
		/// 同步TSTransform中基于物理模拟而失去同步的局部方位状态
		/// </summary>
		public void SyncDirtyPhysicsTransform()
		{
			if (tsCollider == null || tsCollider._body == null)
			{
				return;
			}

			var isPosDirty = false;
			var isRotateDirty = false;

			// this._position = this._getRigidBodyPosition();
			// this._rotation = this._getRigidBodyRotation();
			this._getRigidBodyPosition(out __sharedPos);
			if (this._position != __sharedPos)
			{
				isPosDirty = true;
				toLocalPosition(ref __sharedPos, out this._localPosition);
				_updatePosition(false);
				// this.markPosDirty();
			}

			this._getRigidBodyRotation(out __sharedRot);
			if (this._rotation != __sharedRot)
			{
				isRotateDirty = true;
				toLocalRotation(ref __sharedRot, out this._localRotation);
				_updateRotation(false);
				// this.markRotateDirty();
			}
			// TODO: 需要优化性能, 对于update过的物理节点重新更新dirty状态, 只针对非物理节点标记继承性的dirty
			if (isPosDirty)
			{
				if (isRotateDirty)
				{
					markPosAndRotateDirty();
				}
				else
				{
					markPosDirty();
				}
			}
			else if (isRotateDirty)
			{
				markRotateDirty();
			}

			this.recordRigidBodyTransform();
		}

		/// <summary>
		/// 记录rigidbody的方位信息, 用途提高碰撞检测精度
		/// </summary>
		protected void recordRigidBodyTransform()
		{
			this.tsCollider.RecordRigidBodyTransform();
		}

		[SerializeField]
		[HideInInspector]
		[AddTracking]
		private TSVector _lossyScale = new TSVector(1, 1, 1);

		/// <summary>
		/// 全局缩放比例
		/// - 只读, 由局部缩放比例换算为全局缩放比例
		/// </summary>
		/// <value></value>
		public TSVector lossyScale
		{
			get
			{
				return GetLossyScale();
			}
		}

		public ref TSVector GetLossyScale(){
			UpdateDirtyTransform();
			return ref _lossyScale;
		}

		[SerializeField]
		[HideInInspector]
		[AddTracking]
		private TSVector _localScale = new TSVector(1, 1, 1);

		/// <summary>
		/// 局部缩放比例
		/// </summary>
		/// <value></value>
		public TSVector localScale
		{
			get
			{
				return _localScale;
			}
			set
			{
				SetLocalScale(ref value);
				this.syncViewScale();
			}
		}

		public void SetLocalScale(ref TSVector value)
		{
			if (_localScale != value)
			{
				_localScale = value;
				// updateLossyScale(true);
				markScaleDirty();
			}
		}

		protected void _updateLossyScale(bool recurse = true)
		{
			TSVector lossyScale;
			if (this.tsParent == null)
			{
				lossyScale = _localScale;
			}
			else
			{
				TSVector.MultiplyPair(ref _localScale,ref tsParent.GetLossyScale(),out lossyScale);
			}

			if (this._lossyScale != lossyScale)
			{
				this._lossyScale = lossyScale;

				if (tsCollider != null && tsCollider.Body != null)
				{
					// position = tsCollider.Body.TSPosition - scaledCenter;
					tsCollider.Body.TSScale = this._lossyScale;
				}

				if (recurse)
				{
					updateChildScale();
				}
			}
		}

		protected void updateChildScale()
		{
			foreach (TSTransform child in tsChildren)
			{
				child._updatePosition(false);
				child._updateLossyScale();
			}
		}

		protected TSTransform tsParent;

		[HideInInspector]
		public TSTransform TSParent
		{
			get => tsParent;
			set
			{
				SetParent(value);
			}
		}

		public void SetParent(TSTransform parent, bool worldPositionStays = true)
		{
			_setParent(parent, worldPositionStays);
			this.transform.SetParent(parent.transform, worldPositionStays);
		}

		protected void _setParent(TSTransform value, bool worldPositionStays = true)
		{
			if (tsParent != value)
			{
				tsParent = value;

				// 无parent, 非递归, 更新速度块, 可以忽略重复自身update造成的效率降低
				_localScale = _lossyScale;
				_updateLossyScale(false);
				_updatePosition(false);
				// _updateRotation(true);
				_updateRotation(false);

				markScaleDirty();
				markPosDirty();
				markRotateDirty();
			}

			if (value == null)
			{
				if (value.tsChildren.Contains(this))
				{
					value.tsChildren.Remove(this);
				}
			}
			else
			{
				if (!value.tsChildren.Contains(this))
				{
					value.tsChildren.Add(this);
				}
			}
		}

		[HideInInspector]
		public List<TSTransform> tsChildren = new List<TSTransform>();

		public void Update()
		{
			if (Application.isPlaying)
			{
				if (initialized)
				{
					UpdatePlayMode();
				}
			}
			else
			{
				UpdateEditMode();
			}
		}

		private void UpdateEditMode()
		{
			if (transform.hasChanged)
			{
				_position = transform.position.ToTSVector();
				_rotation = transform.rotation.ToTSQuaternion();
				_lossyScale = transform.lossyScale.ToTSVector();

				_localScale = transform.localScale.ToTSVector();

				_serialized = true;
			}
		}

		private void UpdatePlayMode()
		{

			if (tsViewSync != null)
			{
				tsViewSync.UpdateView();
				return;
			}

			// update view transform
			SyncViewWithLerp();
		}

		/// <summary>
		/// 配置
		/// </summary>
		protected TSTransformConfig tsTransformConfig;

		public Vector3 GetLerpFactor()
		{
			if(tsTransformConfig!=null){
				return tsTransformConfig.GetLerpFactor();
			}else{
				return TSTransformConfig._ConstLerpFactor;
			}
		}

		public bool OverwriteLerpFactor
		{
			get
			{
				if (tsTransformConfig != null)
				{
					return tsTransformConfig.OverwriteLerpFactor;
				}
				else
				{
					return false;
				}
			}
		}

		public void SyncViewWithLerp()
		{
			// UpdateDirtyTransform();

			// update view only
			if (rb != null)
			{
				/*
				var speed = rb.tsCollider.Body.TSLinearVelocity.magnitude;
				var distance = (localPosition - transform.localPosition.ToTSVector()).magnitude;
				var scale = 1.0f;
				if (transform.parent != null)
				{
					scale = transform.parent.lossyScale.magnitude;
				}
				// 以local变换为准, 避免父子坐标平滑关系失效脱节
				// 考虑伸缩带来的距离变换影响
				var needLerp = 2 * speed * Time.deltaTime * DELTA_TIME_FACTOR > distance * scale;
				*/

				if (true)
				{
					var lerpFactor = GetLerpFactor();

					if (rb.interpolation == TSRigidBody.InterpolateMode.Interpolate || rb.interpolation == TSRigidBody.InterpolateMode.None)
					{
						transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition.ToVector(), Time.deltaTime * lerpFactor.x);
						try
                        {
							transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotation.ToQuaternion(), Time.deltaTime * lerpFactor.y);
						}catch(System.Exception e)
                        {
							Debug.LogError(e);
                        }
						transform.localScale = Vector3.Lerp(transform.localScale, localScale.ToVector(), Time.deltaTime * lerpFactor.z);
						return;
					}
					else if (rb.interpolation == TSRigidBody.InterpolateMode.Extrapolate)
					{
						transform.localPosition = (localPosition + rb.tsCollider.Body.TSLinearVelocity * Time.deltaTime * lerpFactor.x).ToVector();
						transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotation.ToQuaternion(), Time.deltaTime * lerpFactor.y);
						transform.localScale = Vector3.Lerp(transform.localScale, localScale.ToVector(), Time.deltaTime * lerpFactor.z);
						return;
					}
				}
			}

			_syncView();
		}

		private static UnityEngine.Vector3 __cachedUEVector;
		private static UnityEngine.Quaternion __cachedUEQuat;
		/// <summary>
		/// 简单同步当前节点视图
		/// </summary>
		private void _syncView()
		{
			_localPosition.Overwrite(ref __cachedUEVector);
			_localRotation.Overwrite(ref __cachedUEQuat);
			transform.localPosition = __cachedUEVector;
			transform.localRotation = __cachedUEQuat;
			_localScale.Overwrite(ref __cachedUEVector);
			if (transform.localScale != __cachedUEVector)
			{
				transform.localScale = __cachedUEVector;
			}
		}

		private void _syncViewPosition()
		{
			_localPosition.Overwrite(ref __cachedUEVector);
			transform.localPosition = __cachedUEVector;
		}

		public void syncViewPosition()
		{
			// UpdateDirtyTransform();
			this._syncViewPosition();
		}

		private void _syncViewRotation()
		{
			_localRotation.Overwrite(ref __cachedUEQuat);
			transform.localRotation = __cachedUEQuat;
		}

		public void syncViewRotation()
		{
			// UpdateDirtyTransform();
			this._syncViewRotation();
		}

		private void _syncViewScale()
		{
			_localScale.Overwrite(ref __cachedUEVector);
			transform.localScale = __cachedUEVector;
		}

		public void syncViewScale()
		{
			// UpdateDirtyTransform();
			this._syncViewScale();
		}

		private void _syncViewPositionAndRotation()
		{
			_localPosition.Overwrite(ref __cachedUEVector);
			_localRotation.Overwrite(ref __cachedUEQuat);
			// transform.SetPositionAndRotation(__cachedUEVector, __cachedUEQuat);
			transform.localPosition = __cachedUEVector;
			transform.localRotation = __cachedUEQuat;
		}

		public void syncViewPositionAndRotation()
		{
			// UpdateDirtyTransform();
			_syncViewPositionAndRotation();
		}

		/// <summary>
		/// 简单同步当前节点视图
		/// </summary>
		public void SyncView()
		{
			// this.UpdateDirtyTransform();
			this._syncView();
		}

		private void updateChildPosition()
		{
			foreach (TSTransform child in tsChildren)
			{
				child._updatePosition(true);
			}
		}

		private void updateChildRotation()
		{
			foreach (TSTransform child in tsChildren)
			{
				child._updatePosition(false);
				child._updateRotation(true);
			}
		}

		protected void updateSelfMatrix()
		{
			if (isSelfTransformMatrixDirty)
			{
				isSelfTransformMatrixDirty = false;

				TSTransform thisTransform = this;
				// _cachedMyMatrix = TSMatrix4x4.TransformToMatrix(ref thisTransform);
				TSMatrix4x4.TransformToMatrix(ref thisTransform, out _cachedMyMatrix);
			}
		}
		public ref TSMatrix4x4 GetSelfMatrix()
		{
			updateSelfMatrix();
			return ref _cachedMyMatrix;
		}

		/// <summary>
		/// 当前节点作为原点, 考虑了 pos, rot, scale 的转换
		/// </summary>
		/// <value></value>
		public ref TSMatrix4x4 localToWorldMatrix
		{
			get
			{
				return ref GetLocalToWorldMatrix();
			}
		}
		protected virtual ref TSMatrix4x4 GetLocalToWorldMatrix()
		{
			updateLocalToWorldMatrix();
			return ref _cachedLocalToWorldMatrix;
		}
		protected void updateLocalToWorldMatrix()
		{
			if (this.isTransformMatrixDirty)
			{
				isTransformMatrixDirty = false;

				TSTransform parent = tsParent;
				if (parent != null)
				{
					TSMatrix4x4.Multiply(
						ref parent.GetLocalToWorldMatrix(),
						ref GetSelfMatrix(),
						out _cachedLocalToWorldMatrix
						);
				}
				else
				{
					_cachedLocalToWorldMatrix = GetSelfMatrix();
				}
			}
		}

		/// <summary>
		/// 当前节点作为原点, 考虑了 pos, rot, scale 的转换
		/// </summary>
		/// <value></value>
		public ref TSMatrix4x4 worldToLocalMatrix
		{
			get
			{
				if (isReverseTransformMatrixDirty)
				{
					isReverseTransformMatrixDirty = false;

					TSMatrix4x4.Inverse(ref GetLocalToWorldMatrix(), out _cachedWorldToLocalMatrix);
				}
				return ref _cachedWorldToLocalMatrix;
			}
		}

		//
		// 摘要:
		//     Sets the world space position and rotation of the Transform component.
		//
		// 参数:
		//   position:
		//
		//   rotation:
		public void SetPositionAndRotation(TSVector pos, TSQuaternion rot, bool updateView = true)
		{
			// TODO: 优化性能 SetPositionAndRotation
			if (updateView)
			{
				this.position = pos;
				this.rotation = rot;
			}
			else
			{
				this.SetPosition(ref pos);
				this.SetRotation(ref rot);
			}

			// if (updateView)
			// {
			// 	this.syncViewPositionAndRotation();
			// }
		}

	}
}
