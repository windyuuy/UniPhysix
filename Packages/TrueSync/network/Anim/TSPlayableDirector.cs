
using UnityEngine;

namespace TrueSync.Anim
{

	public class TSPlayableDirector : MonoBehaviour, IPlayableDirector, IExposedPropertyTable
	{

		protected UnityEngine.Playables.PlayableDirector _director;
		public UnityEngine.Playables.PlayableDirector director => _director;

		protected TSAnimPlayer tsAnimPlayer;

		void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this._director = this.GetComponent<UnityEngine.Playables.PlayableDirector>();
			this.tsAnimPlayer = this.GetOrAddComponent<TSAnimPlayer>();
		}

		//
		// 摘要:
		//     The duration of the Playable in seconds.
		public double duration
		{
			get
			{
				return (double)(FP)_director.duration;
			}
		}
		//
		// 摘要:
		//     The time at which the Playable should start when first played.
		public double initialTime
		{
			get
			{
				return (double)(FP)_director.initialTime;
			}
			set
			{
				_director.initialTime = (double)(FP)value;
			}
		}
		//
		// 摘要:
		//     The component's current time. This value is incremented according to the PlayableDirector.timeUpdateMode
		//     when it is playing. You can also change this value manually.
		public double time
		{
			get
			{
				return (double)(FP)_director.time;
			}
			set
			{
				_director.time = (double)(FP)value;
			}
		}
		//
		// 摘要:
		//     Controls how time is incremented when playing back.
		public UnityEngine.Playables.DirectorUpdateMode timeUpdateMode
		{
			get
			{
				return _director.timeUpdateMode;
			}
			set
			{
				_director.timeUpdateMode = value;
			}
		}
		//
		// 摘要:
		//     Whether the playable asset will start playing back as soon as the component awakes.
		public bool playOnAwake
		{
			get
			{
				return _director.playOnAwake;
			}
			set
			{
				_director.playOnAwake = value;
			}
		}
		//
		// 摘要:
		//     The PlayableGraph created by the PlayableDirector.
		public UnityEngine.Playables.PlayableGraph playableGraph
		{
			get
			{
				return _director.playableGraph;
			}
		}
		//
		// 摘要:
		//     The PlayableAsset that is used to instantiate a playable for playback.
		public UnityEngine.Playables.PlayableAsset playableAsset
		{
			get
			{
				return _director.playableAsset;
			}
			set
			{
				_director.playableAsset = value;
			}
		}
		//
		// 摘要:
		//     Controls how the time is incremented when it goes beyond the duration of the
		//     playable.
		public UnityEngine.Playables.DirectorWrapMode extrapolationMode
		{
			get
			{
				return _director.extrapolationMode;
			}
			set
			{
				_director.extrapolationMode = value;
			}
		}
		//
		// 摘要:
		//     The current playing state of the component. (Read Only)
		public UnityEngine.Playables.PlayState state
		{
			get
			{
				return _director.state;
			}
		}

		public event System.Action<IPlayableDirector> stopped
		{
			add
			{
				tsAnimPlayer.stopped += value;
			}
			remove
			{
				tsAnimPlayer.stopped -= value;
			}
		}
		public event System.Action<IPlayableDirector> played
		{
			add
			{
				tsAnimPlayer.played += value;
			}
			remove
			{
				tsAnimPlayer.played -= value;
			}
		}
		public event System.Action<IPlayableDirector> paused
		{
			add
			{
				tsAnimPlayer.paused += value;
			}
			remove
			{
				tsAnimPlayer.paused -= value;
			}
		}

		//
		// 摘要:
		//     Clears the binding of a reference object.
		//
		// 参数:
		//   key:
		//     The source object in the PlayableBinding.
		public void ClearGenericBinding(Object key)
		{
			_director.ClearGenericBinding(key);
		}
		//
		// 摘要:
		//     Clears an exposed reference value.
		//
		// 参数:
		//   id:
		//     Identifier of the ExposedReference.
		public void ClearReferenceValue(PropertyName id)
		{
			_director.ClearReferenceValue(id);
		}
		//
		// 摘要:
		//     Tells the PlayableDirector to evaluate it's PlayableGraph on the next update.
		public void DeferredEvaluate()
		{
			_director.DeferredEvaluate();
		}
		//
		// 摘要:
		//     Evaluates the currently playing Playable at the current time.
		public void Evaluate()
		{
			_director.Evaluate();
		}
		//
		// 摘要:
		//     Returns a binding to a reference object.
		//
		// 参数:
		//   key:
		//     The object that acts as a key.
		public Object GetGenericBinding(Object key)
		{
			return _director.GetGenericBinding(key);
		}
		public Object GetReferenceValue(PropertyName id, out bool idValid)
		{
			return _director.GetReferenceValue(id, out idValid);
		}
		//
		// 摘要:
		//     Pauses playback of the currently running playable.
		public void Pause()
		{
			_director.Pause();
		}
		//
		// 摘要:
		//     Instatiates a Playable using the provided PlayableAsset and starts playback.
		//
		// 参数:
		//   asset:
		//     An asset to instantiate a playable from.
		//
		//   mode:
		//     What to do when the time passes the duration of the playable.
		public void Play()
		{
			_director.Play();
		}
		//
		// 摘要:
		//     Instatiates a Playable using the provided PlayableAsset and starts playback.
		//
		// 参数:
		//   asset:
		//     An asset to instantiate a playable from.
		//
		//   mode:
		//     What to do when the time passes the duration of the playable.
		public void Play(UnityEngine.Playables.PlayableAsset asset, UnityEngine.Playables.DirectorWrapMode mode)
		{
			_director.Play(asset, mode);
		}
		//
		// 摘要:
		//     Instatiates a Playable using the provided PlayableAsset and starts playback.
		//
		// 参数:
		//   asset:
		//     An asset to instantiate a playable from.
		//
		//   mode:
		//     What to do when the time passes the duration of the playable.
		public void Play(UnityEngine.Playables.PlayableAsset asset)
		{
			_director.Play(asset);
		}
		//
		// 摘要:
		//     Rebinds each PlayableOutput of the PlayableGraph.
		public void RebindPlayableGraphOutputs()
		{
			_director.RebindPlayableGraphOutputs();
		}
		//
		// 摘要:
		//     Discards the existing PlayableGraph and creates a new instance.
		public void RebuildGraph()
		{
			_director.RebuildGraph();
		}
		//
		// 摘要:
		//     Resume playing a paused playable.
		public void Resume()
		{
			_director.Resume();
		}
		//
		// 摘要:
		//     Sets the binding of a reference object from a PlayableBinding.
		//
		// 参数:
		//   key:
		//     The source object in the PlayableBinding.
		//
		//   value:
		//     The object to bind to the key.
		public void SetGenericBinding(Object key, Object value)
		{
			_director.SetGenericBinding(key, value);
		}
		//
		// 摘要:
		//     Sets an ExposedReference value.
		//
		// 参数:
		//   id:
		//     Identifier of the ExposedReference.
		//
		//   value:
		//     The object to bind to set the reference value to.
		public void SetReferenceValue(PropertyName id, Object value)
		{
			_director.SetReferenceValue(id, value);
		}
		//
		// 摘要:
		//     Stops playback of the current Playable and destroys the corresponding graph.
		public void Stop()
		{
			tsAnimPlayer.Stop();
			_director.Stop();
		}

	}
}
