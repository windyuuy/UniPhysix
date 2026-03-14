using System;

namespace TrueSync
{
	public interface IBody
	{
		bool TSDisabled
		{
			get;
			set;
		}

		string Checkum();

		int layer
		{
			get;
			set;
		}
	}
}
