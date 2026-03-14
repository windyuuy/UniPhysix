namespace fsync
{
	public static class TSUtils
	{
		public static INetTimer Time => GameObjectManager.Inst.NetTimer;
		public static INetTimer ServerTime => GameObjectManager.Inst.ServerTimer;
	}

}
