namespace fsync.viewbind
{
	public class CharacterController : ViewData
	{
		Vector3 motion = new Vector3();

		UnityEngine.CharacterController value = null;

		public CharacterController Set(UnityEngine.CharacterController value)
		{
			return this;
		}

		public virtual void Bind(UnityEngine.CharacterController v)
		{
			this.value = v;
			this.Set(v);
		}

		public void Move(UnityEngine.Vector3 motion)
		{
			this.motion.Set(motion);
		}
		public override void UpdateDirty()
		{
			if (this.motion.IsDirty)
			{
				this.value.Move(motion.Value);
			}
		}
	}
}
