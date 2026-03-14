
using System;

namespace TrueSync
{
	public partial struct FP
	{

		public static void MultRef(ref FP x, ref FP y, out FP result)
		{
			var xl = x._serializedValue;
			var yl = y._serializedValue;

			var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
			var xhi = xl >> FRACTIONAL_PLACES;
			var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
			var yhi = yl >> FRACTIONAL_PLACES;

			long sum = 0;
			long hiResult = 0;
			long hihi = 0;
			try
			{
				checked
				{
					var lolo = xlo * ylo;
					var lohi = (long)xlo * yhi;
					var hilo = xhi * (long)ylo;
					hihi = xhi * yhi;

					var loResult = lolo >> FRACTIONAL_PLACES;
					var midResult1 = lohi;
					var midResult2 = hilo;
					hiResult = hihi << FRACTIONAL_PLACES;

					sum = (long)loResult + midResult1 + midResult2 + hiResult;
					result._serializedValue = sum;
					return;
				}
			}
			catch (OverflowException e)
			{
				// Debug.LogError($"overflow: {x},{y}, " + e);

				if (hihi > 0)
				{
					result._serializedValue = MaxValue._serializedValue;
					return;
				}
				else if (hihi < 0)
				{
					result._serializedValue = MinValue._serializedValue;
					return;
				}
			}

			result._serializedValue = 0;
			return;
		}


		/// <summary>
		/// Adds x and y. Performs saturating addition, i.e. in case of overflow, 
		/// rounds to MinValue or MaxValue depending on sign of operands.
		/// </summary>
		public static void AddRef(ref FP x, ref FP y, out FP result)
		{
			result._serializedValue = x._serializedValue + y._serializedValue;
		}


		public static FP R1, R2, R3, R4;
		public static FP M1, M2, M3;

		public static void MultiDot(ref FP a1, ref FP a2, ref FP a3, ref FP b1, ref FP b2, ref FP b3, out FP r1)
		{
			FP.MultRef(ref a1, ref b1, out R1);
			FP.MultRef(ref a2, ref b2, out R2);
			FP.MultRef(ref a3, ref b3, out R3);
			FP.AddRef(ref R1, ref R2, out R4);
			FP.AddRef(ref R3, ref R4, out r1);
		}

	}
}
