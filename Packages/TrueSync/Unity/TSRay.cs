namespace TrueSync
{

    /**
    *  @brief Represents a ray with origin and direction. 
    **/
    public struct TSRay
	{
		public TSVector direction;
		public TSVector origin;

		public static readonly TSRay none = new TSRay();

		public TSRay (ref TSVector origin, ref TSVector direction)
		{
			this.origin = origin;
			this.direction = direction;
		}

		public TSRay(TSVector origin, TSVector direction):this(ref origin,ref direction)
		{
		}

	}
}

