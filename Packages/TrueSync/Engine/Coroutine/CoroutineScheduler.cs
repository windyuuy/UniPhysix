using System.Collections;

/// <summary>
/// CoroutineScheduler.cs
/// 
/// Port of the Javascript version from 
/// http://www.unifycommunity.com/wiki/index.php?title=CoroutineScheduler
/// 
/// Linked list node type used by coroutine scheduler to track scheduling of coroutines.
/// 
/// 
/// BMBF Researchproject http://playfm.htw-berlin.de
/// PlayFM - Serious Games für den IT-gestützten Wissenstransfer im Facility Management 
///	Gefördert durch das bmb+f - Programm Forschung an Fachhochschulen profUntFH
///	
///	<author>Frank.Otto@htw-berlin.de</author>
///
/// 
/// A simple coroutine scheduler. Coroutines can yield until the next update
/// "yield;", until a given number of updates "yield anInt", until a given
/// amount of seconds "yield aFloat;", or until another coroutine has finished
/// "yield scheduler.StartCoroutine(Coroutine())".
/// 
/// Multiple scheduler instances are supported and can be very useful. A
/// coroutine running under one scheduler can yield (wait) for a coroutine
/// running under a completely different scheduler instance.
/// 
/// Unity's YieldInstruction classes are not used because I cannot
/// access their internal data needed for scheduling. Semantics are slightly
/// different from Unity's scheduler. For example, in Unity if you start a
/// coroutine it will run up to its first yield immediately, while in this
/// scheduler it will not run until the next time UpdateAllCoroutines is called.
/// This feature allows any code to start coroutines at any time, while
/// making sure the started coroutines only run at specific times.
/// 
/// You should not depend on update order between coroutines running on the same
/// update. For example, StartCoroutine(A), StartCoroutine(B), StartCoroutine(C)
/// where A, B, C => while(true) { print(A|B|C); yield; }, do not expect "ABC" or
/// "CBA" or any other specific ordering.
/// </summary>
/// 
/// 
namespace TrueSync {
	public class CoroutineScheduler
	{

		TSCoroutine first = null;
		TSCoroutine end = null;
		//FP currentTime;

		AbstractLockstep lockStep;

		public CoroutineScheduler(AbstractLockstep lockStep) {
			this.lockStep = lockStep;
		}

		public void SetLockStep(AbstractLockstep lockStep)
		{
			this.lockStep = lockStep;
		}

		/**
	   * Starts a coroutine, the coroutine does not run immediately but on the
	   * next call to UpdateAllCoroutines. The execution of a coroutine can
	   * be paused at any point using the yield statement. The yield return value
	   * specifies when the coroutine is resumed.
	   */

		public TSCoroutine StartCoroutine(IEnumerator fiber)
		{
			// if function does not have a yield, fiber will be null and we no-op
			if (fiber == null) {
				return null;
			}
			// create coroutine node and run until we reach first yield
			TSCoroutine coroutine = new TSCoroutine(fiber);
			AddCoroutine (coroutine);

			return coroutine;
		}

		/**
	   * Stops all coroutines running on this behaviour. Use of this method is
	   * discouraged, think of a natural way for your coroutines to finish
	   * on their own instead of being forcefully stopped before they finish.
	   * If you need finer control over stopping coroutines you can use multiple
	   * schedulers.
	   */
		public void StopAllCoroutines ()
		{
			first = null;
			end = null;
		}

		/**
	   * Returns true if this scheduler has any coroutines. You can use this to
	   * check if all coroutines have finished or been stopped.
	   */
		public bool HasCoroutines ()
		{
			return first != null;
		}

		//public void UpdateAllCoroutines ()
		//{
		//InputDataBase oldInputData = TrueSyncInput.CurrentSimulationData;
		//UpdateAllCoroutines ();
		//TrueSyncInput.CurrentSimulationData = (InputData) oldInputData;
		//}

		/**
	   * Runs all active coroutines until their next yield. Caller must provide
	   * the current frame and time. This allows for schedulers to run under
	   * frame and time regimes other than the Unity's main game loop.
	   */
		public void UpdateAllCoroutinesSide() { UpdateAllCoroutines(); }
		public void UpdateAllCoroutines()
		{
			TSCoroutine coroutine = this.first;
			while (coroutine != null) {
				// store listNext before coroutine finishes and is removed from the list
				//TSCoroutine listNext = coroutine.listNext;
				try
				{
					if (coroutine.waitForCoroutine == null)
					{
						// initial update
						UpdateCoroutine(coroutine);
					}
					else if (coroutine.waitForCoroutine != null && coroutine.waitForCoroutine.finished)
					{
						//removeCoroutine(coroutine.waitForCoroutine);
						coroutine.waitForCoroutine = null;
						UpdateCoroutine(coroutine);
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogError(e);
				}
				coroutine = coroutine.listNext;
			}
		}

		/**
	   * Executes coroutine until next yield. If coroutine has finished, flags
	   * it as finished and removes it from scheduler list.
	   */
		private void UpdateCoroutine(TSCoroutine coroutine)
		{
#if OSYNC
			if (coroutine.playerId > -1) {
				TrueSyncInput.CurrentSimulationData = (InputData) lockStep.GetInputData (coroutine.playerId);
			}
#endif

			IEnumerator fiber = coroutine.fiber;

			var ok = fiber.MoveNext();
			if (ok) {
				var current = fiber.Current;
				if(current is IEnumerator)
                {
					coroutine.waitForCoroutine = StartCoroutine(current as IEnumerator);
				} else if(current is UnityEngine.WaitForSeconds)
                {
					var waitForSeconds = current as UnityEngine.WaitForSeconds;
					var f = waitForSeconds.GetType().GetField("m_Seconds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
					var v= (float)f.GetValue(waitForSeconds);
					var duration = (FP)v;
					coroutine.waitForCoroutine = StartCoroutine(new fsync.WaitForSeconds(duration));
                }
			} else {
				// coroutine finished
				coroutine.finished = true;
				removeCoroutine(coroutine);
			}
		}

		private void AddCoroutine(TSCoroutine coroutine)
		{
            if (this.first == null)
            {
				this.first = coroutine;
				this.end = this.first;
            }
            else
			{
				coroutine.listPrevious = end;
				this.end.listNext = coroutine;
				this.end = coroutine;
            }
		}

		public void RemoveCoroutine(TSCoroutine coroutine)
        {
            try
            {
				this.removeCoroutine(coroutine);
			}catch(System.Exception e)
            {
				UnityEngine.Debug.LogError(e);
            }
		}

		private void removeCoroutine(TSCoroutine coroutine)
		{
            if (this.end == coroutine)
            {
				this.end = coroutine.listPrevious;
            }
			if (this.first == coroutine) {
				// remove first
				this.first = coroutine.listNext;
			} else {
				// not head of list
				if (coroutine.listNext != null) {
					// remove between
					coroutine.listPrevious.listNext = coroutine.listNext;
					coroutine.listNext.listPrevious = coroutine.listPrevious;
				} else if (coroutine.listPrevious != null) {
					// and listNext is null
					coroutine.listPrevious.listNext = null;
					// remove last
				}
			}
			coroutine.listPrevious = null;
			coroutine.listNext = null;

			coroutine.OnFinished?.Invoke(coroutine);
		}

	}//class
}