
using UnityEngine;
using UnityEngine.Playables;

namespace TrueSync.Anim
{
	public interface IPlayableDirector : IMonoBehaviour, IExposedPropertyTable
	{
		/// <summary>
		/// 原始的 PlayableDirector 对象
		/// </summary>
		PlayableDirector director { get; }

		//
		// 摘要:
		//     The duration of the Playable in seconds.
		double duration { get; }
		//
		// 摘要:
		//     The time at which the Playable should start when first played.
		double initialTime { get; set; }
		//
		// 摘要:
		//     The component's current time. This value is incremented according to the IPlayableDirector.timeUpdateMode
		//     when it is playing. You can also change this value manually.
		double time { get; set; }
		//
		// 摘要:
		//     Controls how time is incremented when playing back.
		DirectorUpdateMode timeUpdateMode { get; set; }
		//
		// 摘要:
		//     Whether the playable asset will start playing back as soon as the component awakes.
		bool playOnAwake { get; set; }
		//
		// 摘要:
		//     The PlayableGraph created by the IPlayableDirector.
		PlayableGraph playableGraph { get; }
		//
		// 摘要:
		//     The PlayableAsset that is used to instantiate a playable for playback.
		PlayableAsset playableAsset { get; set; }
		//
		// 摘要:
		//     Controls how the time is incremented when it goes beyond the duration of the
		//     playable.
		DirectorWrapMode extrapolationMode { get; set; }
		//
		// 摘要:
		//     The current playing state of the component. (Read Only)
		PlayState state { get; }

		event System.Action<IPlayableDirector> stopped;
		event System.Action<IPlayableDirector> played;
		event System.Action<IPlayableDirector> paused;

		//
		// 摘要:
		//     Clears the binding of a reference object.
		//
		// 参数:
		//   key:
		//     The source object in the PlayableBinding.
		void ClearGenericBinding(Object key);
		//
		// 摘要:
		//     Tells the IPlayableDirector to evaluate it's PlayableGraph on the next update.
		void DeferredEvaluate();
		//
		// 摘要:
		//     Evaluates the currently playing Playable at the current time.
		void Evaluate();
		//
		// 摘要:
		//     Returns a binding to a reference object.
		//
		// 参数:
		//   key:
		//     The object that acts as a key.
		Object GetGenericBinding(Object key);
		//
		// 摘要:
		//     Pauses playback of the currently running playable.
		void Pause();
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
		void Play();
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
		void Play(PlayableAsset asset, DirectorWrapMode mode);
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
		void Play(PlayableAsset asset);
		//
		// 摘要:
		//     Rebinds each PlayableOutput of the PlayableGraph.
		void RebindPlayableGraphOutputs();
		//
		// 摘要:
		//     Discards the existing PlayableGraph and creates a new instance.
		void RebuildGraph();
		//
		// 摘要:
		//     Resume playing a paused playable.
		void Resume();
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
		void SetGenericBinding(Object key, Object value);
		//
		// 摘要:
		//     Stops playback of the current Playable and destroys the corresponding graph.
		void Stop();
	}
}
