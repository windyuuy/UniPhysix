using System.Collections.Generic;
/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Linq;
using UnityEngine;

namespace TrueSync
{
	/// <summary>
	/// A vector structure.
	/// </summary>
	[Serializable]
	public struct TSVector
	{

		public FP this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.x;
					case 1: return this.y;
					case 2: return this.z;
					default:
						throw new System.Exception("index out of boundary");
				}
			}
			set
			{
				switch (index)
				{
					case 0: this.x = value; break;
					case 1: this.y = value; break;
					case 2: this.z = value; break;
					default:
						throw new System.Exception("index out of boundary");
				}
			}
		}

		public FP this[UInt32 index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.x;
					case 1: return this.y;
					case 2: return this.z;
					default:
						throw new System.Exception("index out of boundary");
				}
			}
			set
			{
				switch (index)
				{
					case 0: this.x = value; break;
					case 1: this.y = value; break;
					case 2: this.z = value; break;
					default:
						throw new System.Exception("index out of boundary");
				}
			}
		}

		private static FP ZeroEpsilonSq = TSMath.Epsilon;
		internal static TSVector InternalZero;
		internal static TSVector Arbitrary;

		/// <summary>The X component of the vector.</summary>
		public FP x;
		/// <summary>The Y component of the vector.</summary>
		public FP y;
		/// <summary>The Z component of the vector.</summary>
		public FP z;

		#region Static readonly variables
		/// <summary>
		/// A vector with components (0,0,0);
		/// </summary>
		public static readonly TSVector zero;
		/// <summary>
		/// A vector with components (-1,0,0);
		/// </summary>
		public static readonly TSVector left;
		/// <summary>
		/// A vector with components (1,0,0);
		/// </summary>
		public static readonly TSVector right;
		/// <summary>
		/// A vector with components (0,1,0);
		/// </summary>
		public static readonly TSVector up;
		/// <summary>
		/// A vector with components (0,-1,0);
		/// </summary>
		public static readonly TSVector down;
		/// <summary>
		/// A vector with components (0,0,-1);
		/// </summary>
		public static readonly TSVector back;
		/// <summary>
		/// A vector with components (0,0,1);
		/// </summary>
		public static readonly TSVector forward;
		/// <summary>
		/// A vector with components (1,1,1);
		/// </summary>
		public static readonly TSVector one;
		/// <summary>
		/// A vector with components 
		/// (FP.MinValue,FP.MinValue,FP.MinValue);
		/// </summary>
		public static readonly TSVector MinValue;
		/// <summary>
		/// A vector with components 
		/// (FP.MaxValue,FP.MaxValue,FP.MaxValue);
		/// </summary>
		public static readonly TSVector MaxValue;
		#endregion

		#region Private static constructor
		static TSVector()
		{
			one = new TSVector(1, 1, 1);
			zero = new TSVector(0, 0, 0);
			left = new TSVector(-1, 0, 0);
			right = new TSVector(1, 0, 0);
			up = new TSVector(0, 1, 0);
			down = new TSVector(0, -1, 0);
			back = new TSVector(0, 0, -1);
			forward = new TSVector(0, 0, 1);
			MinValue = new TSVector(FP.MinValue);
			MaxValue = new TSVector(FP.MaxValue);
			Arbitrary = new TSVector(1, 1, 1);
			InternalZero = zero;
		}
		#endregion

		public static TSVector Abs(TSVector other)
		{
			return new TSVector(FP.Abs(other.x), FP.Abs(other.y), FP.Abs(other.z));
		}

		/// <summary>
		/// Gets the squared length of the vector.
		/// </summary>
		/// <returns>Returns the squared length of the vector.</returns>
		public FP sqrMagnitude
		{
			get
			{
				return (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z));
			}
		}

		/// <summary>
		/// Gets the length of the vector.
		/// </summary>
		/// <returns>Returns the length of the vector.</returns>
		public FP magnitude
		{
			get
			{
				FP num = ((this.x * this.x) + (this.y * this.y)) + (this.z * this.z);
				return FP.Sqrt(num);
			}
		}

		public static TSVector ClampMagnitude(TSVector vector, FP maxLength)
		{
			return Normalize(vector) * maxLength;
		}

		/// <summary>
		/// Gets a normalized version of the vector.
		/// </summary>
		/// <returns>Returns a normalized version of the vector.</returns>
		public TSVector normalized
		{
			get
			{
				TSVector result = new TSVector(this.x, this.y, this.z);
				result.Normalize();

				return result;
			}
		}

		/// <summary>
		/// Constructor initializing a new instance of the structure
		/// </summary>
		/// <param name="x">The X component of the vector.</param>
		/// <param name="y">The Y component of the vector.</param>
		/// <param name="z">The Z component of the vector.</param>

		public TSVector(int x, int y, int z)
		{
			this.x = (FP)x;
			this.y = (FP)y;
			this.z = (FP)z;
		}

		public TSVector(FP x, FP y, FP z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Multiplies each component of the vector by the same components of the provided vector.
		/// </summary>
		public void Scale(TSVector other)
		{
			this.x = x * other.x;
			this.y = y * other.y;
			this.z = z * other.z;
		}

		/// <summary>
		/// Sets all vector component to specific values.
		/// </summary>
		/// <param name="x">The X component of the vector.</param>
		/// <param name="y">The Y component of the vector.</param>
		/// <param name="z">The Z component of the vector.</param>
		public void Set(FP x, FP y, FP z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>
		/// Constructor initializing a new instance of the structure
		/// </summary>
		/// <param name="xyz">All components of the vector are set to xyz</param>
		public TSVector(FP xyz)
		{
			this.x = xyz;
			this.y = xyz;
			this.z = xyz;
		}

		public static TSVector Lerp(TSVector from, TSVector to, FP percent)
		{
			return from + (to - from) * percent;
		}

		/// <summary>
		/// Builds a string from the JVector.
		/// </summary>
		/// <returns>A string containing all three components.</returns>
		#region public override string ToString()
		public override string ToString()
		{
			// return string.Format("({0:f1}, {1:f1}, {2:f1})", x.AsFloat(), y.AsFloat(), z.AsFloat());
			return $"({x}, {y}, {z})";
		}
		#endregion

		/// <summary>
		/// Tests if an object is equal to this vector.
		/// </summary>
		/// <param name="obj">The object to test.</param>
		/// <returns>Returns true if they are euqal, otherwise false.</returns>
		#region public override bool Equals(object obj)
		public override bool Equals(object obj)
		{
			if (!(obj is TSVector)) return false;
			TSVector other = (TSVector)obj;

			return (((x == other.x) && (y == other.y)) && (z == other.z));
		}
		#endregion

		/// <summary>
		/// Multiplies each component of the vector by the same components of the provided vector.
		/// </summary>
		public static TSVector Scale(TSVector vecA, TSVector vecB)
		{
			TSVector result;
			result.x = vecA.x * vecB.x;
			result.y = vecA.y * vecB.y;
			result.z = vecA.z * vecB.z;

			return result;
		}

		/// </summary>
		public static void Scale(ref TSVector vecA, ref TSVector vecB, out TSVector result)
		{
			result.x = vecA.x * vecB.x;
			result.y = vecA.y * vecB.y;
			result.z = vecA.z * vecB.z;
		}

		/// <summary>
		/// Tests if two JVector are equal.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>Returns true if both values are equal, otherwise false.</returns>
		#region public static bool operator ==(JVector value1, JVector value2)
		public static bool operator ==(TSVector value1, TSVector value2)
		{
			return (((value1.x == value2.x) && (value1.y == value2.y)) && (value1.z == value2.z));
		}
		#endregion

		/// <summary>
		/// Tests if two JVector are not equal.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>Returns false if both values are equal, otherwise true.</returns>
		#region public static bool operator !=(JVector value1, JVector value2)
		public static bool operator !=(TSVector value1, TSVector value2)
		{
			// if ((value1.x == value2.x) && (value1.y == value2.y))
			// {
			// 	return (value1.z != value2.z);
			// }
			// return true;
			return (value1.x != value2.x) || (value1.z != value2.z) || (value1.y != value2.y);
		}
		#endregion

		/// <summary>
		/// Gets a vector with the minimum x,y and z values of both vectors.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>A vector with the minimum x,y and z values of both vectors.</returns>
		#region public static JVector Min(JVector value1, JVector value2)

		public static TSVector Min(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Min(ref value1, ref value2, out result);
			return result;
		}

		/// <summary>
		/// Gets a vector with the minimum x,y and z values of both vectors.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <param name="result">A vector with the minimum x,y and z values of both vectors.</param>
		public static void Min(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			result.x = (value1.x < value2.x) ? value1.x : value2.x;
			result.y = (value1.y < value2.y) ? value1.y : value2.y;
			result.z = (value1.z < value2.z) ? value1.z : value2.z;
		}
		#endregion

		/// <summary>
		/// Gets a vector with the maximum x,y and z values of both vectors.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>A vector with the maximum x,y and z values of both vectors.</returns>
		#region public static JVector Max(JVector value1, JVector value2)
		public static TSVector Max(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Max(ref value1, ref value2, out result);
			return result;
		}

		public static FP Distance(TSVector v1, TSVector v2)
		{
			return FP.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z));
		}

		public static FP DistanceSQ(TSVector v1, TSVector v2)
		{
			return (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z);
		}

		public static void DistanceSQ(ref TSVector v1, ref TSVector v2, out FP distanceSQ)
		{
			distanceSQ = (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z);
		}

		public FP GetLengthSQ()
		{
			return x * x + y * y + z * z;
		}

		public FP GetLength()
		{
			return FP.Sqrt(x * x + y * y + z * z);
		}

		/// <summary>
		/// Gets a vector with the maximum x,y and z values of both vectors.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <param name="result">A vector with the maximum x,y and z values of both vectors.</param>
		public static void Max(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			result.x = (value1.x > value2.x) ? value1.x : value2.x;
			result.y = (value1.y > value2.y) ? value1.y : value2.y;
			result.z = (value1.z > value2.z) ? value1.z : value2.z;
		}
		#endregion

		/// <summary>
		/// Sets the length of the vector to zero.
		/// </summary>
		#region public void MakeZero()
		public void MakeZero()
		{
			x = FP.Zero;
			y = FP.Zero;
			z = FP.Zero;
		}
		#endregion

		/// <summary>
		/// Checks if the length of the vector is zero.
		/// </summary>
		/// <returns>Returns true if the vector is zero, otherwise false.</returns>
		#region public bool IsZero()
		// public bool IsZero()
		// {
		// 	return (this.sqrMagnitude == FP.Zero);
		// }

		/// <summary>
		/// Checks if the length of the vector is nearly zero.
		/// </summary>
		/// <returns>Returns true if the vector is nearly zero, otherwise false.</returns>
		public bool IsNearlyZero()
		{
			return (this.sqrMagnitude < ZeroEpsilonSq);
		}
		#endregion

		/// <summary>
		/// Transforms a vector by the given matrix.
		/// </summary>
		/// <param name="position">The vector to transform.</param>
		/// <param name="matrix">The transform matrix.</param>
		/// <returns>The transformed vector.</returns>
		#region public static JVector Transform(JVector position, JMatrix matrix)
		public static TSVector Transform(TSVector position, TSMatrix matrix)
		{
			TSVector result;
			TSVector.Transform(ref position, ref matrix, out result);
			return result;
		}

		/// <summary>
		/// Transforms a vector by the given matrix.
		/// </summary>
		/// <param name="position">The vector to transform.</param>
		/// <param name="matrix">The transform matrix.</param>
		/// <param name="result">The transformed vector.</param>
		public static void Transform(ref TSVector position, ref TSMatrix matrix, out TSVector result)
		{
			FP num0 = ((position.x * matrix.M11) + (position.y * matrix.M21)) + (position.z * matrix.M31);
			FP num1 = ((position.x * matrix.M12) + (position.y * matrix.M22)) + (position.z * matrix.M32);
			FP num2 = ((position.x * matrix.M13) + (position.y * matrix.M23)) + (position.z * matrix.M33);

			result.x = num0;
			result.y = num1;
			result.z = num2;
		}

		public static TSVector TransposedTransform(TSVector position, TSMatrix matrix)
		{
			TSVector result;
			TSVector.TransposedTransform(ref position, ref matrix, out result);
			return result;
		}

		/// <summary>
		/// Transforms a vector by the transposed of the given Matrix.
		/// </summary>
		/// <param name="position">The vector to transform.</param>
		/// <param name="matrix">The transform matrix.</param>
		/// <param name="result">The transformed vector.</param>
		public static void TransposedTransform(ref TSVector position, ref TSMatrix matrix, out TSVector result)
		{
			FP num0 = ((position.x * matrix.M11) + (position.y * matrix.M12)) + (position.z * matrix.M13);
			FP num1 = ((position.x * matrix.M21) + (position.y * matrix.M22)) + (position.z * matrix.M23);
			FP num2 = ((position.x * matrix.M31) + (position.y * matrix.M32)) + (position.z * matrix.M33);

			result.x = num0;
			result.y = num1;
			result.z = num2;
		}
		#endregion

		/// <summary>
		/// 通过参照坐标和旋转变换矩阵求逆变换:
		/// - position => reversePosition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="referPos"></param>
		/// <param name="orientation"></param>
		/// <param name="reversePosition"></param>
		public static void ReverseTransform(ref TSVector position, ref TSVector referPos, ref TSMatrix orientation, out TSVector reversePosition)
		{
			// 求当前坐标系偏移
			var offset = position - referPos;
			// 通过旋转矩阵, 求逆变换, 得到逆坐标系偏移
			TransposedTransform(ref offset, ref orientation, out var reverseOffset);
			reversePosition = reverseOffset;
		}

		/// <summary>
		/// 通过参照坐标和旋转变换矩阵进行线性变换
		/// - position -> toPos
		/// </summary>
		/// <param name="position"></param>
		/// <param name="referPos"></param>
		/// <param name="orientation"></param>
		/// <param name="toPos"></param>
		public static void Transform(ref TSVector position, ref TSVector referPos, ref TSMatrix orientation, out TSVector toPosWorld)
		{
			Transform(ref position, ref orientation, out var rotToPos);
			// toPosWorld = rotToPos + referPos;
			TSVector.Add(ref rotToPos, ref referPos, out toPosWorld);
		}

		public FP Dot(TSVector vector2)
		{
			return TSVector.Dot(ref this, ref vector2);
		}

		public FP Dot(ref TSVector vector2)
		{
			return TSVector.Dot(ref this, ref vector2);
		}

		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>Returns the dot product of both vectors.</returns>
		#region public static FP Dot(JVector vector1, JVector vector2)
		public static FP Dot(TSVector vector1, TSVector vector2)
		{
			return TSVector.Dot(ref vector1, ref vector2);
		}


		/// <summary>
		/// Calculates the dot product of both vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>Returns the dot product of both vectors.</returns>
		public static FP Dot(ref TSVector vector1, ref TSVector vector2)
		{
			// return ((vector1.x * vector2.x) + (vector1.y * vector2.y)) + (vector1.z * vector2.z);
			FP.MultiDot(ref vector1.x, ref vector1.y, ref vector1.z, ref vector2.x, ref vector2.y, ref vector2.z, out FP.M1);
			return FP.M1;
		}
		#endregion

		// Projects a vector onto another vector.
		public static TSVector Project(TSVector vector, TSVector onNormal)
		{
			FP sqrtMag = Dot(onNormal, onNormal);
			if (sqrtMag < TSMath.Epsilon)
				return zero;
			else
				return onNormal * Dot(vector, onNormal) / sqrtMag;
		}

		// Projects a vector onto a plane defined by a normal orthogonal to the plane.
		public static TSVector ProjectOnPlane(TSVector vector, TSVector planeNormal)
		{
			return vector - Project(vector, planeNormal);
		}


		// Returns the angle in degrees between /from/ and /to/. This is always the smallest
		public static FP Angle(TSVector from, TSVector to)
		{
			return TSMath.Acos(TSMath.Clamp(Dot(from.normalized, to.normalized), -FP.ONE, FP.ONE)) * TSMath.Rad2Deg;
		}

		// The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
		// If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
		// The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
		public static FP SignedAngle(TSVector from, TSVector to, TSVector axis)
		{
			TSVector fromNorm = from.normalized, toNorm = to.normalized;
			FP unsignedAngle = TSMath.Acos(TSMath.Clamp(Dot(fromNorm, toNorm), -FP.ONE, FP.ONE)) * TSMath.Rad2Deg;
			FP sign = TSMath.Sign(Dot(axis, Cross(fromNorm, toNorm)));
			return unsignedAngle * sign;
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The sum of both vectors.</returns>
		#region public static void Add(JVector value1, JVector value2)
		public static TSVector Add(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Add(ref value1, ref value2, out result);
			return result;
		}

		/// <summary>
		/// Adds to vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The sum of both vectors.</param>
		public static void Add(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			// FP num0 = value1.x + value2.x;
			// FP num1 = value1.y + value2.y;
			// FP num2 = value1.z + value2.z;

			// result.x = num0;
			// result.y = num1;
			// result.z = num2;

			result.x = value1.x + value2.x;
			result.y = value1.y + value2.y;
			result.z = value1.z + value2.z;
		}

		public static void AddSelf(ref TSVector result, ref TSVector value1)
        {
			result.x += value1.x;
			result.y += value1.y;
			result.z += value1.z;
		}
		#endregion

		public void Divide(FP scaleFactor)
		{
			var rs = 1 / scaleFactor;
			this.x *= rs;
			this.y *= rs;
			this.z *= rs;
		}

		/// <summary>
		/// Divides a vector by a factor.
		/// </summary>
		/// <param name="value1">The vector to divide.</param>
		/// <param name="scaleFactor">The scale factor.</param>
		/// <returns>Returns the scaled vector.</returns>
		public static TSVector Divide(TSVector value1, FP scaleFactor)
		{
			TSVector result;
			TSVector.Divide(ref value1, scaleFactor, out result);
			return result;
		}

		/// <summary>
		/// Divides a vector by a factor.
		/// </summary>
		/// <param name="value1">The vector to divide.</param>
		/// <param name="scaleFactor">The scale factor.</param>
		/// <param name="result">Returns the scaled vector.</param>
		public static void Divide(ref TSVector value1, FP scaleFactor, out TSVector result)
		{
			// TODO: 优化精度, 包括 TSVector2,TSVector4 和成员函数 Divide
			var rs = 1 / scaleFactor;
			result.x = value1.x * rs;
			result.y = value1.y * rs;
			result.z = value1.z * rs;
		}

		/// <summary>
		/// Subtracts two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The difference of both vectors.</returns>
		#region public static JVector Subtract(JVector value1, JVector value2)
		public static TSVector Subtract(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Subtract(ref value1, ref value2, out result);
			return result;
		}

		/// <summary>
		/// Subtracts to vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The difference of both vectors.</param>
		public static void Subtract(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			// FP num0 = value1.x - value2.x;
			// FP num1 = value1.y - value2.y;
			// FP num2 = value1.z - value2.z;

			// result.x = num0;
			// result.y = num1;
			// result.z = num2;

			result.x = value1.x - value2.x;
			result.y = value1.y - value2.y;
			result.z = value1.z - value2.z;
		}
		#endregion

		/// <summary>
		/// The cross product of two vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>The cross product of both vectors.</returns>
		#region public static JVector Cross(JVector vector1, JVector vector2)
		public static TSVector Cross(TSVector vector1, TSVector vector2)
		{
			TSVector result;
			TSVector.Cross(ref vector1, ref vector2, out result);
			return result;
		}

		/// <summary>
		/// The cross product of two vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <param name="result">The cross product of both vectors.</param>
		public static void Cross(ref TSVector vector1, ref TSVector vector2, out TSVector result)
		{
			FP num3 = (vector1.y * vector2.z) - (vector1.z * vector2.y);
			FP num2 = (vector1.z * vector2.x) - (vector1.x * vector2.z);
			FP num = (vector1.x * vector2.y) - (vector1.y * vector2.x);
			result.x = num3;
			result.y = num2;
			result.z = num;
		}
		#endregion

		/// <summary>
		/// Gets the hashcode of the vector.
		/// </summary>
		/// <returns>Returns the hashcode of the vector.</returns>
		#region public override int GetHashCode()
		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
		}
		#endregion

		/// <summary>
		/// Inverses the direction of the vector.
		/// </summary>
		#region public static JVector Negate(JVector value)
		public void Negate()
		{
			this.x = -this.x;
			this.y = -this.y;
			this.z = -this.z;
		}

		/// <summary>
		/// Inverses the direction of a vector.
		/// </summary>
		/// <param name="value">The vector to inverse.</param>
		/// <returns>The negated vector.</returns>
		public static TSVector Negate(TSVector value)
		{
			TSVector result;
			TSVector.Negate(ref value, out result);
			return result;
		}

		/// <summary>
		/// Inverses the direction of a vector.
		/// </summary>
		/// <param name="value">The vector to inverse.</param>
		/// <param name="result">The negated vector.</param>
		public static void Negate(ref TSVector value, out TSVector result)
		{
			FP num0 = -value.x;
			FP num1 = -value.y;
			FP num2 = -value.z;

			result.x = num0;
			result.y = num1;
			result.z = num2;
		}
		#endregion

		/// <summary>
		/// Normalizes the given vector.
		/// </summary>
		/// <param name="value">The vector which should be normalized.</param>
		/// <returns>A normalized vector.</returns>
		#region public static JVector Normalize(JVector value)
		public static TSVector Normalize(TSVector value)
		{
			TSVector result;
			TSVector.Normalize(ref value, out result);
			return result;
		}

		/// <summary>
		/// Normalizes this vector.
		/// </summary>
		public void Normalize()
		{
			FP num2 = ((this.x * this.x) + (this.y * this.y)) + (this.z * this.z);
			FP num = FP.One / FP.Sqrt(num2);
			this.x *= num;
			this.y *= num;
			this.z *= num;
		}

		/// <summary>
		/// Normalizes this vector.
		/// </summary>
		public FP NormalizeR()
		{
			FP num2 = FP.Sqrt(
				((this.x * this.x) + (this.y * this.y)) + (this.z * this.z)
				);
			FP num = FP.One / num2;
			this.x *= num;
			this.y *= num;
			this.z *= num;

			return num2;
		}

		public FP Distance(TSVector vec)
		{
			return Distance(this, vec);
		}

		public FP DistanceSQ(TSVector vec)
		{
			return DistanceSQ(this, vec);
		}

		/// <summary>
		/// Normalizes the given vector.
		/// </summary>
		/// <param name="value">The vector which should be normalized.</param>
		/// <param name="result">A normalized vector.</param>
		public static void Normalize(ref TSVector value, out TSVector result)
		{
			FP num2 = ((value.x * value.x) + (value.y * value.y)) + (value.z * value.z);
			FP num = FP.One / FP.Sqrt(num2);
			result.x = value.x * num;
			result.y = value.y * num;
			result.z = value.z * num;
		}
		#endregion

		#region public static void Swap(ref JVector vector1, ref JVector vector2)

		/// <summary>
		/// Swaps the components of both vectors.
		/// </summary>
		/// <param name="vector1">The first vector to swap with the second.</param>
		/// <param name="vector2">The second vector to swap with the first.</param>
		public static void Swap(ref TSVector vector1, ref TSVector vector2)
		{
			FP temp;

			temp = vector1.x;
			vector1.x = vector2.x;
			vector2.x = temp;

			temp = vector1.y;
			vector1.y = vector2.y;
			vector2.y = temp;

			temp = vector1.z;
			vector1.z = vector2.z;
			vector2.z = temp;
		}
		#endregion

		/// <summary>
		/// Multiply a vector with a factor.
		/// </summary>
		/// <param name="value1">The vector to multiply.</param>
		/// <param name="scaleFactor">The scale factor.</param>
		/// <returns>Returns the multiplied vector.</returns>
		#region public static JVector Multiply(JVector value1, FP scaleFactor)
		public static TSVector Multiply(TSVector value1, FP scaleFactor)
		{
			TSVector result;
			TSVector.Multiply(ref value1, scaleFactor, out result);
			return result;
		}

		public TSVector MultiplyPair(TSVector value1)
		{
			return MultiplyPair(this, value1);
		}

		public void MultiplyPairSelf(TSVector value1)
		{
			this.x *= value1.x;
			this.y *= value1.y;
			this.z *= value1.z;
		}

		public static TSVector MultiplyPair(TSVector value1, TSVector value2)
		{
			TSVector result;
			result.x = value1.x * value2.x;
			result.y = value1.y * value2.y;
			result.z = value1.z * value2.z;
			return result;
		}

		public static void MultiplyPair(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			result.x = value1.x * value2.x;
			result.y = value1.y * value2.y;
			result.z = value1.z * value2.z;
		}


		public TSVector DivPair(TSVector value1)
		{
			return DivPair(this, value1);
		}

		public static TSVector DivPair(TSVector value1, TSVector value2)
		{
			TSVector result;
			if (value1.x == 0)
			{
				result.x = 0;
			}
			else
			{
				result.x = value1.x / value2.x;
			}

			if (value1.y == 0)
			{
				result.y = 0;
			}
			else
			{
				result.y = value1.y / value2.y;
			}

			if (value1.z == 0)
			{
				result.z = 0;
			}
			else
			{
				result.z = value1.z / value2.z;
			}

			return result;
		}

		/// <summary>
		/// Multiply a vector with a factor.
		/// </summary>
		/// <param name="value1">The vector to multiply.</param>
		/// <param name="scaleFactor">The scale factor.</param>
		/// <param name="result">Returns the multiplied vector.</param>
		public static void Multiply(ref TSVector value1, FP scaleFactor, out TSVector result)
		{
			result.x = value1.x * scaleFactor;
			result.y = value1.y * scaleFactor;
			result.z = value1.z * scaleFactor;
		}
		#endregion

		/// <summary>
		/// Calculates the cross product of two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>Returns the cross product of both.</returns>
		#region public static JVector operator %(JVector value1, JVector value2)
		public static TSVector operator %(TSVector value1, TSVector value2)
		{
			TSVector result; TSVector.Cross(ref value1, ref value2, out result);
			return result;
		}
		#endregion

		/// <summary>
		/// Calculates the dot product of two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>Returns the dot product of both.</returns>
		#region public static FP operator *(JVector value1, JVector value2)
		public static FP operator *(TSVector value1, TSVector value2)
		{
			return TSVector.Dot(ref value1, ref value2);
		}
		#endregion

		/// <summary>
		/// Multiplies a vector by a scale factor.
		/// </summary>
		/// <param name="value1">The vector to scale.</param>
		/// <param name="value2">The scale factor.</param>
		/// <returns>Returns the scaled vector.</returns>
		#region public static JVector operator *(JVector value1, FP value2)
		public static TSVector operator *(TSVector value1, FP value2)
		{
			TSVector result;
			TSVector.Multiply(ref value1, value2, out result);
			return result;
		}
		#endregion

		/// <summary>
		/// Multiplies a vector by a scale factor.
		/// </summary>
		/// <param name="value2">The vector to scale.</param>
		/// <param name="value1">The scale factor.</param>
		/// <returns>Returns the scaled vector.</returns>
		#region public static JVector operator *(FP value1, JVector value2)
		public static TSVector operator *(FP value1, TSVector value2)
		{
			TSVector result;
			TSVector.Multiply(ref value2, value1, out result);
			return result;
		}
		#endregion

		/// <summary>
		/// Subtracts two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The difference of both vectors.</returns>
		#region public static JVector operator -(JVector value1, JVector value2)
		public static TSVector operator -(TSVector value1, TSVector value2)
		{
			TSVector result; TSVector.Subtract(ref value1, ref value2, out result);
			return result;
		}
		#endregion

		/// <summary>
		/// Negetive vector.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The difference of both vectors.</returns>
		#region public static JVector operator -(JVector value1)
		public static TSVector operator -(TSVector value1)
		{
			return new TSVector(-value1.x, -value1.y, -value1.z);
		}
		#endregion

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The sum of both vectors.</returns>
		#region public static JVector operator +(JVector value1, JVector value2)
		public static TSVector operator +(TSVector value1, TSVector value2)
		{
			TSVector result; TSVector.Add(ref value1, ref value2, out result);
			return result;
		}
		#endregion

		/// <summary>
		/// Divides a vector by a factor.
		/// </summary>
		/// <param name="value1">The vector to divide.</param>
		/// <param name="scaleFactor">The scale factor.</param>
		/// <returns>Returns the scaled vector.</returns>
		public static TSVector operator /(TSVector value1, FP value2)
		{
			TSVector result;
			TSVector.Divide(ref value1, value2, out result);
			return result;
		}

		public TSVector2 ToTSVector2()
		{
			return new TSVector2(this.x, this.y);
		}

		public TSVector4 ToTSVector4()
		{
			return new TSVector4(this.x, this.y, this.z, FP.One);
		}

		#region myext
		public UnityEngine.Vector3 ToUEVector()
		{
			return new UnityEngine.Vector3(this.x.AsFloat(), this.y.AsFloat(), this.z.AsFloat());
		}
		public void ToUEVector(out UnityEngine.Vector3 vec)
		{
			// return new UnityEngine.Vector3(this.x.AsFloat(), this.y.AsFloat(), this.z.AsFloat());
			vec.x = this.x.AsFloat();
			vec.y = this.y.AsFloat();
			vec.z = this.z.AsFloat();
		}

		public static List<UnityEngine.Vector3> ToUEList(ICollection<TSVector> vectors)
		{
			var ls = (from vec in vectors
					  select new UnityEngine.Vector3(vec.x.AsFloat(), vec.y.AsFloat(), vec.z.AsFloat())
					  ).ToList();
			return ls;
		}

		public static UnityEngine.Vector3[] ToUEArray(ICollection<TSVector> vectors)
		{
			var ls = (from vec in vectors
					  select new UnityEngine.Vector3(vec.x.AsFloat(), vec.y.AsFloat(), vec.z.AsFloat())
					  ).ToList();
			return ls.ToArray();
		}

		public void MergeFrom(ref TSVector vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

		public void MergeFrom(TSVector vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

		public void MergeFrom(UnityEngine.Vector3 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

		public void MergeFrom(ref UnityEngine.Vector3 vector)
		{
			this.x = vector.x;
			this.y = vector.y;
			this.z = vector.z;
		}

		public static TSVector From(UnityEngine.Vector3 vector)
		{
			var ret = new TSVector();
			ret.MergeFrom(vector);
			return ret;
		}

		public static List<TSVector> FromList(ICollection<UnityEngine.Vector3> vectors)
		{
			var ls = (from vec in vectors
					  select From(vec)).ToList();
			return ls;
		}

		/// <summary>
		/// 旋转到指定方向
		/// </summary>
		/// <param name="current">当前方向</param>
		/// <param name="target">目标方向</param>
		/// <param name="maxRadiansDelta">最大偏转角度</param>
		/// <param name="maxMagnitudeDelta">最大长度变化</param>
		/// <returns></returns>
		public static TSVector RotateTowards(TSVector current, TSVector target, FP maxRadiansDelta, FP maxMagnitudeDelta)
		{
			// TODO: 实现 RotateTowards
			return target * 1;
			// var th = (current * target) / (current.magnitude * target.magnitude);
			// var angle = TSMath.Acos(th);

			// // 求出符合角度要求的偏转矢量
			// var final = target;
			// if ((maxRadiansDelta > 0 && maxRadiansDelta < angle)
			// 	|| (maxRadiansDelta < 0 && -maxRadiansDelta < (2 * TSMath.PI - angle)))
			// {
			// 	var base1 = current.normalized;
			// 	var a = current;
			// 	var b = target;
			// 	var n = -a.magnitude / (a * b);
			// 	var base2 = (a + n * b).normalized;
			// 	if ((target * base2) * maxRadiansDelta < 0)
			// 	{
			// 		base2 = -base2;
			// 	}

			// 	final = base1 * Math.Cos(TSMath.abs(maxRadiansDelta)) + base2 * *Math.Sin(TSMath.abs(maxRadiansDelta));
			// }
			// else
			// {
			// 	final = target.normalized;
			// }

			// // 求出符合长度要求的矢量
			// if (target.magnitude - current.magnitude)


		}

		#endregion

		public static TSVector Reflect(TSVector inDirection, TSVector inNormal)
		{
			FP num = -2 * TSVector.Dot(inNormal, inDirection);
			return new TSVector(num * inNormal.x + inDirection.x, num * inNormal.y + inDirection.y, num * inNormal.z + inDirection.z);
		}

		public bool IsZero()
		{
			var b = (x == 0) && (y == 0) && (z == 0);
			return b;
		}

		public static void ClampBoxXZWithRate(ref TSVector vec, ref FP hx, ref FP hz, out TSVector vec1)
		{
			var rateV = FP.Abs((vec.x * hz) / (vec.z * hx));
			if (rateV > 1)
			{
				// vec.x较大, 以 vec.z 为准
				vec1.x = hx * FP.Sign(vec.x);
				vec1.z = hx * vec.z / vec.x * FP.Sign(vec.z);
			}
			else if (rateV < 1)
			{
				// vec.z较大, 以 vec.x 为准
				vec1.z = hz * FP.Sign(vec.x);
				vec1.x = hz * vec.x / vec.z * FP.Sign(vec.z);
			}
			else
			{
				vec1.x = hx * FP.Sign(vec.x);
				vec1.z = hz * FP.Sign(vec.z);
			}
			vec1.y = vec.y;
		}

		public static bool ClampBox(ref TSVector vec, ref TSVector hrange, out TSVector vec1)
		{
			var changed = false;
			if (FP.Abs(vec.x) > hrange.x)
			{
				changed = true;
				vec1.x = FP.Sign(vec.x) * hrange.x;
			}
			else
			{
				vec1.x = vec.x;
			}
			if (FP.Abs(vec.y) > hrange.y)
			{
				changed = true;
				vec1.y = FP.Sign(vec.y) * hrange.y;
			}
			else
			{
				vec1.y = vec.y;
			}
			if (FP.Abs(vec.z) > hrange.z)
			{
				changed = true;
				vec1.z = FP.Sign(vec.z) * hrange.z;
			}
			else
			{
				vec1.z = vec.z;
			}

			return changed;
		}

		public static TSVector Average(TSVector vec1, TSVector vec2)
		{
			TSVector ovec;
			Average(ref vec1, ref vec2, out ovec);
			return ovec;
		}
		public static void Average(ref TSVector vec1, ref TSVector vec2, out TSVector ovec)
		{
			ovec.x = (vec1.x + vec2.x) * FP.myhalf;
			ovec.y = (vec1.y + vec2.y) * FP.myhalf;
			ovec.z = (vec1.z + vec2.z) * FP.myhalf;
		}

		public void Overwrite(ref UnityEngine.Vector3 vec)
		{
			vec.x = (float)this.x;
			vec.y = (float)this.y;
			vec.z = (float)this.z;
		}

		public static void Zero(out TSVector vec)
		{
			vec.x = FP.Zero;
			vec.y = FP.Zero;
			vec.z = FP.Zero;
		}

		public static TSVector max = new TSVector(FP.MaxValue, FP.MaxValue, FP.MaxValue);
	}

}