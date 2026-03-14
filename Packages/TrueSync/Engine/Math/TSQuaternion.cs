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

namespace TrueSync
{

    /// <summary>
    /// A Quaternion representing an orientation.
    /// </summary>
    [Serializable]
    public struct TSQuaternion
    {

        /// <summary>The X component of the quaternion.</summary>
        public FP x;
        /// <summary>The Y component of the quaternion.</summary>
        public FP y;
        /// <summary>The Z component of the quaternion.</summary>
        public FP z;
        /// <summary>The W component of the quaternion.</summary>
        public FP w;

        public static readonly TSQuaternion identity;

        static TSQuaternion() {
            identity = new TSQuaternion(0, 0, 0, 1);
        }

        /// <summary>
        /// Initializes a new instance of the JQuaternion structure.
        /// </summary>
        /// <param name="x">The X component of the quaternion.</param>
        /// <param name="y">The Y component of the quaternion.</param>
        /// <param name="z">The Z component of the quaternion.</param>
        /// <param name="w">The W component of the quaternion.</param>
        public TSQuaternion(FP x, FP y, FP z, FP w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

		public TSQuaternion(TSQuaternion q)
		{
			this.x = q.x;
			this.y = q.y;
			this.z = q.z;
			this.w = q.w;
		}

		public void Set(FP new_x, FP new_y, FP new_z, FP new_w) {
            this.x = new_x;
            this.y = new_y;
            this.z = new_z;
            this.w = new_w;
        }

        public void SetFromToRotation(TSVector fromDirection, TSVector toDirection) {
            TSQuaternion targetRotation = TSQuaternion.FromToRotation(fromDirection, toDirection);
            this.Set(targetRotation.x, targetRotation.y, targetRotation.z, targetRotation.w);
        }

		// y,x,z
		public static void CreateFromYawPitchRoll(FP xx, FP yy, FP zz, out TSQuaternion result)
		{
			xx *= 0.5f;
			yy *= 0.5f;
			zz *= 0.5f;

			var sx = FP.Sin(xx);
			var cx = FP.Cos(xx);
			var sy = FP.Sin(yy);
			var cy = FP.Cos(yy);
			var sz = FP.Sin(zz);
			var cz = FP.Cos(zz);

			result.x = sx * cy * cz + cx * sy * sz;
			result.y = cx * sy * cz - sx * cy * sz;
			result.z = cx * cy * sz - sx * sy * cz;
			result.w = cx * cy * cz + sx * sy * sz;
		}

		FP toDegree(FP a)
		{
			return a * FP.Rad2Deg;
		}

		TSVector toEuler(bool outerZ = false)
		{
            TSVector result = new TSVector();

			var sqx = x * x;

			// x: Pitch
			// y: Roll
			// z: Yaw

			var t0 = -2.0f * (sqx + y * y) + 1.0f;
			var t1 = +2.0f * (w * y + x * z);
			var t2 = 2.0f * (x * w - y * z);
			var t3 = +2.0f * (w * z + x * y);
			var t4 = -2.0f * (sqx + z * z) + 1.0f;

			t2 = t2 > 1.0f ? 1.0f : t2;
			t2 = t2 < -1.0f ? -1.0f : t2;

			var Ez = FP.Atan2(t3, t4) * FP.Rad2Deg;
			var Ex = FP.Asin(t2) * FP.Rad2Deg;
			var Ey = FP.Atan2(t1, t0) * FP.Rad2Deg;

			if (Ez < 0)
            {
				Ez += 360;
            }
			if (Ex < 0)
            {
				Ex += 360;
            }
			if (Ey < 0)
            {
				Ey += 360;
            }

			result.x = Ex;
			result.y = Ey;
			result.z = Ez;

            return result;

		}

		public TSVector eulerAngles
		{
			get
			{
				return toEuler();
			}
			set
			{
				var result = Euler(value);
				this.x = result.x;
				this.y = result.y;
				this.z = result.z;
				this.w = result.w;
            }
		}

		public TSQuaternion Inverse()
		{
			return Inverse(this);
		}

		/// <summary>
		/// 求 delta: a -> b;
		/// 求夹角, 其中 b=delta*a, a 为初始角度, b为最终角度, delta为偏角.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static FP Angle(TSQuaternion a, TSQuaternion b) {
            TSQuaternion aInv = TSQuaternion.Inverse(a);
			TSQuaternion f = aInv * b;

            FP angle = FP.Acos(f.w) * 2 * FP.Rad2Deg;

            if (angle > 180) {
                angle = 360 - angle;
            }

            return angle;
        }

		/// <summary>
		/// 求 delta: start -> end;
		/// 求夹角, 其中 end=delta*start, start 为初始角度, end为最终角度, delta为偏角.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public static TSQuaternion Delta(TSQuaternion start, TSQuaternion end)
		{
			TSQuaternion aInv = TSQuaternion.Inverse(start);
			TSQuaternion f = aInv * end;
			return f;
		}

		public static void Divide(ref TSQuaternion end, ref TSQuaternion start, out TSQuaternion result)
		{
			TSQuaternion.Inverse(ref start, out result);
			TSQuaternion.Multiply(ref result, ref end, out result);
		}

		/// <summary>
		/// Quaternions are added.
		/// </summary>
		/// <param name="quaternion1">The first quaternion.</param>
		/// <param name="quaternion2">The second quaternion.</param>
		/// <returns>The sum of both quaternions.</returns>
		#region public static JQuaternion Add(JQuaternion quaternion1, JQuaternion quaternion2)
		public static TSQuaternion Add(TSQuaternion quaternion1, TSQuaternion quaternion2)
        {
            TSQuaternion result;
            TSQuaternion.Add(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        public static TSQuaternion LookRotation(TSVector forward) {
            return CreateFromMatrix(TSMatrix.LookAt(forward, TSVector.up));
        }

        public static TSQuaternion LookRotation(TSVector forward, TSVector upwards) {
            return CreateFromMatrix(TSMatrix.LookAt(forward, upwards));
        }

        public static TSQuaternion Slerp(TSQuaternion from, TSQuaternion to, FP t) {
            t = TSMath.Clamp(t, 0, 1);

            FP dot = Dot(from, to);

            if (dot < 0.0f) {
                to = Multiply(to, -1);
                dot = -dot;
            }

            FP halfTheta = FP.Acos(dot);

            return Multiply(Multiply(from, FP.Sin((1 - t) * halfTheta)) + Multiply(to, FP.Sin(t * halfTheta)), 1 / FP.Sin(halfTheta));
        }

        public static TSQuaternion RotateTowards(TSQuaternion from, TSQuaternion to, FP maxDegreesDelta) {
            FP dot = Dot(from, to);

            if (dot < 0.0f) {
                to = Multiply(to, -1);
                dot = -dot;
            }

            FP halfTheta = FP.Acos(dot);
            FP theta = halfTheta * 2;

            maxDegreesDelta *= FP.Deg2Rad;

            if (maxDegreesDelta >= theta) {
                return to;
            }

            maxDegreesDelta /= theta;

            return Multiply(Multiply(from, FP.Sin((1 - maxDegreesDelta) * halfTheta)) + Multiply(to, FP.Sin(maxDegreesDelta * halfTheta)), 1 / FP.Sin(halfTheta));
        }

        public static TSQuaternion Euler(FP x, FP y, FP z) {
            x *= FP.Deg2Rad;
            y *= FP.Deg2Rad;
            z *= FP.Deg2Rad;

            TSQuaternion rotation;
			TSQuaternion.CreateFromYawPitchRoll(x, y, z, out rotation);

            return rotation;
        }

        public static TSQuaternion Euler(TSVector eulerAngles) {
            return Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        }

        public static TSQuaternion AngleAxis(FP angle, TSVector axis) {
            axis = axis * FP.Deg2Rad;
            axis.Normalize();

            FP halfAngle = angle * FP.Deg2Rad * FP.Half;

            TSQuaternion rotation;
            FP sin = FP.Sin(halfAngle);

            rotation.x = axis.x * sin;
            rotation.y = axis.y * sin;
            rotation.z = axis.z * sin;
            rotation.w = FP.Cos(halfAngle);

            return rotation;
        }


		/// <summary>
		/// Quaternions are added.
		/// </summary>
		/// <param name="quaternion1">The first quaternion.</param>
		/// <param name="quaternion2">The second quaternion.</param>
		/// <param name="result">The sum of both quaternions.</param>
		public static void Add(ref TSQuaternion quaternion1, ref TSQuaternion quaternion2, out TSQuaternion result)
        {
            result.x = quaternion1.x + quaternion2.x;
            result.y = quaternion1.y + quaternion2.y;
            result.z = quaternion1.z + quaternion2.z;
            result.w = quaternion1.w + quaternion2.w;
        }
        #endregion

        public static TSQuaternion Conjugate(TSQuaternion value)
        {
            TSQuaternion quaternion;
            quaternion.x = -value.x;
            quaternion.y = -value.y;
            quaternion.z = -value.z;
            quaternion.w = value.w;
            return quaternion;
        }

		public static void Conjugate(ref TSQuaternion value, out TSQuaternion quaternion)
		{
			quaternion.x = -value.x;
			quaternion.y = -value.y;
			quaternion.z = -value.z;
			quaternion.w = value.w;
		}

		public static FP Dot(TSQuaternion a, TSQuaternion b)
		{
            return a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z;
        }

		/// <summary>
		/// q^-1 =q*/|q|2
		/// </summary>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static TSQuaternion Inverse(TSQuaternion rotation) {
            FP invNorm = FP.One / ((rotation.x * rotation.x) + (rotation.y * rotation.y) + (rotation.z * rotation.z) + (rotation.w * rotation.w));
            return TSQuaternion.Multiply(TSQuaternion.Conjugate(rotation), invNorm);
        }

		public static void Inverse(ref TSQuaternion rotation, out TSQuaternion result)
		{
			FP invNorm = FP.One / ((rotation.x * rotation.x) + (rotation.y * rotation.y) + (rotation.z * rotation.z) + (rotation.w * rotation.w));
			TSQuaternion.Conjugate(ref rotation, out result);
			TSQuaternion.Multiply(ref result, invNorm, out result);
		}

		/// <summary>
		/// q^-1 = q* =(-x, -y, -z, w)
		/// </summary>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static TSQuaternion InverseOrientation(TSQuaternion rotation)
		{
			return TSQuaternion.Conjugate(rotation);
		}

		public static TSQuaternion FromToRotation(TSVector fromVector, TSVector toVector) {
            TSVector w = TSVector.Cross(fromVector, toVector);
            TSQuaternion q = new TSQuaternion(w.x, w.y, w.z, TSVector.Dot(fromVector, toVector));
            q.w += FP.Sqrt(fromVector.sqrMagnitude * toVector.sqrMagnitude);
            q.Normalize();

            return q;
        }

        public static TSQuaternion Lerp(TSQuaternion a, TSQuaternion b, FP t) {
            t = TSMath.Clamp(t, FP.Zero, FP.One);

            return LerpUnclamped(a, b, t);
        }

        public static TSQuaternion LerpUnclamped(TSQuaternion a, TSQuaternion b, FP t) {
            TSQuaternion result = TSQuaternion.Multiply(a, (1 - t)) + TSQuaternion.Multiply(b, t);
            result.Normalize();

            return result;
        }

        /// <summary>
        /// Quaternions are subtracted.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <returns>The difference of both quaternions.</returns>
        #region public static JQuaternion Subtract(JQuaternion quaternion1, JQuaternion quaternion2)
        public static TSQuaternion Subtract(TSQuaternion quaternion1, TSQuaternion quaternion2)
        {
            TSQuaternion result;
            TSQuaternion.Subtract(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        /// <summary>
        /// Quaternions are subtracted.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <param name="result">The difference of both quaternions.</param>
        public static void Subtract(ref TSQuaternion quaternion1, ref TSQuaternion quaternion2, out TSQuaternion result)
        {
            result.x = quaternion1.x - quaternion2.x;
            result.y = quaternion1.y - quaternion2.y;
            result.z = quaternion1.z - quaternion2.z;
            result.w = quaternion1.w - quaternion2.w;
        }
        #endregion

        /// <summary>
        /// Multiply two quaternions.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <returns>The product of both quaternions.</returns>
        #region public static JQuaternion Multiply(JQuaternion quaternion1, JQuaternion quaternion2)
        public static TSQuaternion Multiply(TSQuaternion quaternion1, TSQuaternion quaternion2)
        {
            TSQuaternion result;
            TSQuaternion.Multiply(ref quaternion1, ref quaternion2, out result);
            return result;
        }

        /// <summary>
        /// Multiply two quaternions.
        /// </summary>
        /// <param name="quaternion1">The first quaternion.</param>
        /// <param name="quaternion2">The second quaternion.</param>
        /// <param name="result">The product of both quaternions.</param>
        public static void Multiply(ref TSQuaternion quaternion1, ref TSQuaternion quaternion2, out TSQuaternion result)
        {
            FP x = quaternion1.x;
            FP y = quaternion1.y;
            FP z = quaternion1.z;
            FP w = quaternion1.w;
            FP num4 = quaternion2.x;
            FP num3 = quaternion2.y;
            FP num2 = quaternion2.z;
            FP num = quaternion2.w;
            FP num12 = (y * num2) - (z * num3);
            FP num11 = (z * num4) - (x * num2);
            FP num10 = (x * num3) - (y * num4);
            FP num9 = ((x * num4) + (y * num3)) + (z * num2);
            result.x = ((x * num) + (num4 * w)) + num12;
            result.y = ((y * num) + (num3 * w)) + num11;
            result.z = ((z * num) + (num2 * w)) + num10;
            result.w = (w * num) - num9;
        }
        #endregion

        /// <summary>
        /// Scale a quaternion
        /// </summary>
        /// <param name="quaternion1">The quaternion to scale.</param>
        /// <param name="scaleFactor">Scale factor.</param>
        /// <returns>The scaled quaternion.</returns>
        #region public static JQuaternion Multiply(JQuaternion quaternion1, FP scaleFactor)
        public static TSQuaternion Multiply(TSQuaternion quaternion1, FP scaleFactor)
        {
            TSQuaternion result;
            TSQuaternion.Multiply(ref quaternion1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Scale a quaternion
        /// </summary>
        /// <param name="quaternion1">The quaternion to scale.</param>
        /// <param name="scaleFactor">Scale factor.</param>
        /// <param name="result">The scaled quaternion.</param>
        public static void Multiply(ref TSQuaternion quaternion1, FP scaleFactor, out TSQuaternion result)
        {
            result.x = quaternion1.x * scaleFactor;
            result.y = quaternion1.y * scaleFactor;
            result.z = quaternion1.z * scaleFactor;
            result.w = quaternion1.w * scaleFactor;
        }
		#endregion

		/// <summary>
		/// Sets the length of the quaternion to one.
		/// </summary>
		#region public void Normalize()
		public void Normalize()
        {
            FP num2 = (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z)) + (this.w * this.w);
            FP num = 1 / (FP.Sqrt(num2));
            this.x *= num;
            this.y *= num;
            this.z *= num;
			this.w *= num;
        }
		#endregion

		public TSQuaternion normalized
		{
			get
			{
				var copy = new TSQuaternion(this);
				copy.Normalize();
				return copy;
			}
		}

		/// <summary>
		/// Gets the squared length of the vector.
		/// </summary>
		/// <returns>Returns the squared length of the vector.</returns>
		public FP sqrMagnitude
		{
			get
			{
				return (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z) + (this.w * this.w));
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
				FP num = ((this.x * this.x) + (this.y * this.y)) + (this.z * this.z) + (this.w * this.w);
				return FP.Sqrt(num);
			}
		}

		/// <summary>
		/// Creates a quaternion from a matrix.
		/// </summary>
		/// <param name="matrix">A matrix representing an orientation.</param>
		/// <returns>JQuaternion representing an orientation.</returns>
		#region public static JQuaternion CreateFromMatrix(JMatrix matrix)
		public static TSQuaternion CreateFromMatrix(TSMatrix matrix)
        {
            TSQuaternion result;
            TSQuaternion.CreateFromMatrix(ref matrix, out result);
            return result;
        }

        /// <summary>
        /// Creates a quaternion from a matrix.
        /// </summary>
        /// <param name="matrix">A matrix representing an orientation.</param>
        /// <param name="result">JQuaternion representing an orientation.</param>
        public static void CreateFromMatrix(ref TSMatrix matrix, out TSQuaternion result)
        {
            FP num8 = (matrix.M11 + matrix.M22) + matrix.M33;
            if (num8 > FP.Zero)
            {
                FP num = FP.Sqrt((num8 + FP.One));
                result.w = num * FP.Half;
                num = FP.Half / num;
                result.x = (matrix.M23 - matrix.M32) * num;
                result.y = (matrix.M31 - matrix.M13) * num;
                result.z = (matrix.M12 - matrix.M21) * num;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                FP num7 = FP.Sqrt((((FP.One + matrix.M11) - matrix.M22) - matrix.M33));
                FP num4 = FP.Half / num7;
                result.x = FP.Half * num7;
                result.y = (matrix.M12 + matrix.M21) * num4;
                result.z = (matrix.M13 + matrix.M31) * num4;
                result.w = (matrix.M23 - matrix.M32) * num4;
            }
            else if (matrix.M22 > matrix.M33)
            {
                FP num6 = FP.Sqrt((((FP.One + matrix.M22) - matrix.M11) - matrix.M33));
                FP num3 = FP.Half / num6;
                result.x = (matrix.M21 + matrix.M12) * num3;
                result.y = FP.Half * num6;
                result.z = (matrix.M32 + matrix.M23) * num3;
                result.w = (matrix.M31 - matrix.M13) * num3;
            }
            else
            {
                FP num5 = FP.Sqrt((((FP.One + matrix.M33) - matrix.M11) - matrix.M22));
                FP num2 = FP.Half / num5;
                result.x = (matrix.M31 + matrix.M13) * num2;
                result.y = (matrix.M32 + matrix.M23) * num2;
                result.z = FP.Half * num5;
                result.w = (matrix.M12 - matrix.M21) * num2;
            }
        }
        #endregion

        /// <summary>
        /// Multiply two quaternions.
        /// </summary>
        /// <param name="value1">The first quaternion.</param>
        /// <param name="value2">The second quaternion.</param>
        /// <returns>The product of both quaternions.</returns>
        #region public static FP operator *(JQuaternion value1, JQuaternion value2)
        public static TSQuaternion operator *(TSQuaternion value1, TSQuaternion value2)
        {
            TSQuaternion result;
            TSQuaternion.Multiply(ref value1, ref value2,out result);
            return result;
        }
		#endregion

		/// <summary>
		/// Multiply two quaternions.
		/// </summary>
		/// <param name="value1">The first quaternion.</param>
		/// <param name="value2">The second quaternion.</param>
		/// <returns>The product of both quaternions.</returns>
		#region public static FP operator /(JQuaternion value1, JQuaternion value2)
		public static TSQuaternion operator /(TSQuaternion end, TSQuaternion start)
		{
			return TSQuaternion.Delta(start, end);
		}
		#endregion

		/// <summary>
		/// Add two quaternions.
		/// </summary>
		/// <param name="value1">The first quaternion.</param>
		/// <param name="value2">The second quaternion.</param>
		/// <returns>The sum of both quaternions.</returns>
		#region public static FP operator +(JQuaternion value1, JQuaternion value2)
		public static TSQuaternion operator +(TSQuaternion value1, TSQuaternion value2)
        {
            TSQuaternion result;
            TSQuaternion.Add(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Subtract two quaternions.
        /// </summary>
        /// <param name="value1">The first quaternion.</param>
        /// <param name="value2">The second quaternion.</param>
        /// <returns>The difference of both quaternions.</returns>
        #region public static FP operator -(JQuaternion value1, JQuaternion value2)
        public static TSQuaternion operator -(TSQuaternion value1, TSQuaternion value2)
        {
            TSQuaternion result;
            TSQuaternion.Subtract(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /**
         *  @brief Rotates a {@link TSVector} by the {@link TSQuanternion}.
         **/
        public static TSVector operator *(TSQuaternion quat, TSVector vec) {
            FP num = quat.x * 2f;
            FP num2 = quat.y * 2f;
            FP num3 = quat.z * 2f;
            FP num4 = quat.x * num;
            FP num5 = quat.y * num2;
            FP num6 = quat.z * num3;
            FP num7 = quat.x * num2;
            FP num8 = quat.x * num3;
            FP num9 = quat.y * num3;
            FP num10 = quat.w * num;
            FP num11 = quat.w * num2;
            FP num12 = quat.w * num3;

            TSVector result;
            result.x = (1f - (num5 + num6)) * vec.x + (num7 - num12) * vec.y + (num8 + num11) * vec.z;
            result.y = (num7 + num12) * vec.x + (1f - (num4 + num6)) * vec.y + (num9 - num10) * vec.z;
            result.z = (num8 - num11) * vec.x + (num9 + num10) * vec.y + (1f - (num4 + num5)) * vec.z;

            return result;
        }

        public override string ToString() {
            return string.Format("({0:f1}, {1:f1}, {2:f1}, {3:f1})", x.AsFloat(), y.AsFloat(), z.AsFloat(), w.AsFloat());
        }

		/// <summary>
		/// Tests if two TSQuaternion are equal.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>Returns true if both values are equal, otherwise false.</returns>
		#region public static bool operator ==(TSQuaternion value1, TSQuaternion value2)
		public static bool operator ==(TSQuaternion value1, TSQuaternion value2)
		{
			return (((value1.x == value2.x) && (value1.y == value2.y))
				&& (value1.z == value2.z) && (value1.w == value2.w));
		}
		#endregion

		/// <summary>
		/// Tests if two TSQuaternion are not equal.
		/// </summary>
		/// <param name="value1">The first value.</param>
		/// <param name="value2">The second value.</param>
		/// <returns>Returns false if both values are equal, otherwise true.</returns>
		#region public static bool operator !=(TSQuaternion value1, TSQuaternion value2)
		public static bool operator !=(TSQuaternion value1, TSQuaternion value2)
		{
			// if ((value1.x == value2.x) && (value1.y == value2.y) && (value1.z == value2.z))
			// {
			// 	return (value1.w != value2.w);
			// }
			// return true;
			return
				value1.x != value2.x ||
				value1.y != value2.y ||
				value1.z != value2.z ||
				value1.w != value2.w;
		}
		#endregion

		public void Overwrite(ref UnityEngine.Quaternion quat)
		{
			quat.x = (float)this.x;
			quat.y = (float)this.y;
			quat.z = (float)this.z;
			quat.w = (float)this.w;
		}
	}
}
