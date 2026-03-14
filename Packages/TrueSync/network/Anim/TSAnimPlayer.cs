using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using System.Collections.Generic;

#region 帧同步
using fsync;
using BoxCollider = TrueSync.TSBoxCollider;
using Bounds = TrueSync.TSBBox;
using WaitForSeconds = fsync.WaitForSeconds;
using Random = fsync.Random;
using TrueSync;
using Vector2 = TrueSync.TSVector2;
using Vector3 = TrueSync.TSVector;
using Mathf = TrueSync.TSMath;
using Quaternion = TrueSync.TSQuaternion;
using TSFloat = TrueSync.FP;
#endregion

namespace TrueSync.Anim
{
	public class AnimContext
	{
		PlayableDirector director = null;
		PlayableAsset anim = null;

		/// <summary>
		/// 需要监控的对象列表
		/// </summary>
		public List<GameObject> WatchTargets = new List<GameObject>();
		public TSFloat Duration = 0;
		public TSFloat StartTime = 0;
		public TSFloat LastTime = 0;

		public bool IsPlaying = false;
		public bool IsResumed = false;

		public void Load(PlayableDirector director, PlayableAsset anim)
		{
			WatchTargets.Clear();

			this.Duration = director.duration;

			foreach (var o in anim.outputs)
			{
				if (o.sourceObject != null)
				{
					var bind = director.GetGenericBinding(o.sourceObject);
					GameObject target;
					if (bind is GameObject)
					{
						target = bind as GameObject;
					}
					else if (bind is Component)
					{
						target = (bind as Component).gameObject;
					}
					else
					{
						throw new System.Exception("unsupport media type");
					}

					if (!WatchTargets.Contains(target))
					{
						WatchTargets.Add(target);
					}
				}
			}
		}
	}

	public class TSAnimPlayer : TSMonoBehaviour, fsync.IFrameSyncUpdate
	{
		protected PlayableDirector director;
		TSPlayableDirector _proxy;
		TSPlayableDirector proxy => _proxy != null ? _proxy : _proxy = this.GetComponent<TSPlayableDirector>();

		protected Dictionary<PlayableAsset, AnimContext> animContextMap
			= new Dictionary<PlayableAsset, AnimContext>();

		/// <summary>
		/// 当前播放的动画
		/// </summary>
		protected PlayableAsset curAnim;

		public AnimContext CurAnimContext
		{
			get
			{
				if (curAnim == null)
				{
					return null;
				}
				// animContextMap.TryGetValue(curAnim, out var result);
				if (animContextMap.ContainsKey(curAnim))
				{
					return animContextMap[curAnim];
				}
				return null;
			}
		}

		protected AnimContext curAnimContext;

		public event System.Action<IPlayableDirector> stopped;
		public event System.Action<IPlayableDirector> played;
		public event System.Action<IPlayableDirector> paused;

		/// <summary>
		/// 加载动画
		/// - 传入动画组件和动画对象
		/// - 预播放动画组件和动画对象, 采集帧动画样本并存储.
		/// </summary>
		public void Load()
		{
			director = this.GetComponent<PlayableDirector>();
			director.played -= Director_played;
			director.played += Director_played;

			director.paused -= Director_paused;
			director.paused += Director_paused;

		}

		void Awake2()
		{
			Load();
		}

		private void Director_played(PlayableDirector director)
		{
			// Debug.Log($"playanim: {director.playableAsset.name}");
			this.PlayAnim(director.playableAsset);
		}

		// TODO: 需要改进为通过pause和resume函数调用
		private void Director_paused(PlayableDirector director)
		{
			var curAnimContext = this.CurAnimContext;
			if (curAnimContext != null)
			{
				if (curAnimContext.IsResumed != (director.state == PlayState.Playing))
				{
					curAnimContext.IsResumed = director.state == PlayState.Playing;

					if (this.paused != null)
					{
						this.paused(this.proxy);
					}
				}
			}
		}

		public void LoadAnims(List<PlayableAsset> anims)
		{
			foreach (var anim in anims)
			{
				LoadAnim(anim);
			}
		}

		public void LoadAnim(PlayableAsset anim)
		{
			if (this.animContextMap.ContainsKey(anim))
			{
				return;
			}

			var context = new AnimContext();
			context.Load(director, anim);
			this.animContextMap.Add(anim, context);
		}

		/// <summary>
		/// 播放逻辑动画
		/// - 获取预先采集的动画样本, 在每次逻辑帧调度时, 设置动画对象属性, 通过帧同步组件来驱动动画播放, 控制逻辑对象实时属性.
		/// </summary>
		public void PlayAnim(PlayableAsset anim)
		{
			director.Pause();
			this.LoadAnim(anim);
			this.curAnim = anim;
			var curAnimContext = this.CurAnimContext;
			curAnimContext.StartTime = Time.time;
			curAnimContext.LastTime = curAnimContext.StartTime;
			curAnimContext.IsPlaying = true;
			curAnimContext.IsResumed = true;

			if (this.played != null)
			{
				this.played(this.proxy);
			}

			Evaluate();
		}

		public void Evaluate()
		{
			if (curAnim == null)
			{
				return;
			}
			// if (director.time >= director.duration)
			// {
			// 	return;
			// }
			curAnimContext = this.CurAnimContext;
			if (!curAnimContext.IsPlaying)
			{
				return;
			}
			if (director.time >= curAnimContext.Duration)
			{
				return;
			}

			director.time = ((TSFloat)director.time + Time.time - curAnimContext.LastTime).AsFloat();
			director.Evaluate();
			curAnimContext.LastTime = Time.time;

			// limit accurate
			foreach (var target in curAnimContext.WatchTargets)
			{
				var transform = target.transform;
				var comp = target.ReferTransform();
				if (comp != null)
				{
					// TODO: 优化性能
					// comp.position = transform.position.LimitAccurate();
					// comp.rotation = transform.rotation.LimitAccurate();
					comp.SetPositionAndRotation(
						transform.position.LimitAccurate()
						, transform.rotation.LimitAccurate()
						);
					comp.localScale = transform.localScale.LimitAccurate();
					comp.Update();
				}
			}
		}

		/// <summary>
		/// 按逻辑帧进度播放动画
		/// </summary>
		void UpdateTS()
		{
			Evaluate();

			if (curAnimContext != null)
			{
				if (director.time >= curAnimContext.Duration)
				{
					Stop();
				}
			}
		}

		protected void TryEmitStop()
		{
			if (this.stopped != null)
			{
				this.stopped(this.proxy);
			}
		}

		public void Stop()
		{
			var curAnimContext = this.CurAnimContext;
			if (curAnimContext == null)
			{
				return;
			}

			if (curAnimContext.IsPlaying)
			{
				curAnimContext.IsResumed = false;
				curAnimContext.IsPlaying = false;

				TryEmitStop();
			}
		}

		#region 帧同步
		#endregion

	}
}
