using System;

namespace Orvid.Graphics.FontSupport
{
    public class AffineTransform
	{
		static double AffineEpsilon = 1e-14;
		public double sx, shy, shx, sy, tx, ty;

		public AffineTransform(AffineTransform copyFrom)
		{
			sx = copyFrom.sx;
			shy = copyFrom.shy;
			shx = copyFrom.shx;
			sy = copyFrom.sy;
			tx = copyFrom.tx;
			ty = copyFrom.ty;
		}

		public AffineTransform(double v0, double v1, double v2,
					 double v3, double v4, double v5)
		{
			sx = v0;
			shy = v1;
			shx = v2;
			sy = v3;
			tx = v4;
			ty = v5;
		}

        public AffineTransform(double[] m)
		{
			sx = m[0];
			shy = m[1];
			shx = m[2];
			sy = m[3];
			tx = m[4];
			ty = m[5];
		}

        public static AffineTransform NewIdentity()
		{
            AffineTransform newAffine = new AffineTransform(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);

			return newAffine;
		}

        public static AffineTransform NewRotation(double angleRadians)
		{
            return new AffineTransform(Math.Cos(angleRadians), Math.Sin(angleRadians), -Math.Sin(angleRadians), Math.Cos(angleRadians), 0.0, 0.0);
		}

        public static AffineTransform NewScaling(double scale)
		{
            return new AffineTransform(scale, 0.0, 0.0, scale, 0.0, 0.0);
		}

        public static AffineTransform NewScaling(double x, double y)
		{
            return new AffineTransform(x, 0.0, 0.0, y, 0.0, 0.0);
		}

        public static AffineTransform NewTranslation(double x, double y)
		{
            return new AffineTransform(1.0, 0.0, 0.0, 1.0, x, y);
		}

        public static AffineTransform NewSkewing(double x, double y)
		{
            return new AffineTransform(1.0, Math.Tan(y), Math.Tan(x), 1.0, 0.0, 0.0);
		}

		public void Identity()
		{
			sx = sy = 1.0;
			shy = shx = tx = ty = 0.0;
		}

		public void Translate(double x, double y)
		{
			tx += x;
			ty += y;
		}

		public void Rotate(double angleRadians)
		{
			double ca = Math.Cos(angleRadians);
			double sa = Math.Sin(angleRadians);
			double t0 = sx * ca - shy * sa;
			double t2 = shx * ca - sy * sa;
			double t4 = tx * ca - ty * sa;
			shy = sx * sa + shy * ca;
			sy = shx * sa + sy * ca;
			ty = tx * sa + ty * ca;
			sx = t0;
			shx = t2;
			tx = t4;
		}

		public void Scale(double x, double y)
		{
			double mm0 = x;
			double mm3 = y;
			sx *= mm0;
			shx *= mm0;
			tx *= mm0;
			shy *= mm3;
			sy *= mm3;
			ty *= mm3;
		}

		public void Scale(double scaleAmount)
		{
			sx *= scaleAmount;
			shx *= scaleAmount;
			tx *= scaleAmount;
			shy *= scaleAmount;
			sy *= scaleAmount;
			ty *= scaleAmount;
		}

        void Multiply(AffineTransform m)
		{
			double t0 = sx * m.sx + shy * m.shx;
			double t2 = shx * m.sx + sy * m.shx;
			double t4 = tx * m.sx + ty * m.shx + m.tx;
			shy = sx * m.shy + shy * m.sy;
			sy = shx * m.shy + sy * m.sy;
			ty = tx * m.shy + ty * m.sy + m.ty;
			sx = t0;
			shx = t2;
			tx = t4;
		}

		public void Invert()
		{
			double d = DeterminantReciprocal;

			double t0 = sy * d;
			sy = sx * d;
			shy = -shy * d;
			shx = -shx * d;

			double t4 = -tx * t0 - ty * shx;
			ty = -tx * shy - ty * sy;

			sx = t0;
			tx = t4;
		}

        public static AffineTransform operator *(AffineTransform a, AffineTransform b)
		{
            AffineTransform temp = new AffineTransform(a);
			temp.Multiply(b);
			return temp;
		}

		public void Transform(ref double x, ref double y)
		{
			double tmp = x;
			x = tmp * sx + y * shx + tx;
			y = tmp * shy + y * sy + ty;
		}

		public void Transform(ref Vec2d pointToTransform)
		{
			Transform(ref pointToTransform.X, ref pointToTransform.Y);
		}

		public void InverseTransform(ref double x, ref double y)
		{
			double d = DeterminantReciprocal;
			double a = (x - tx) * d;
			double b = (y - ty) * d;
			x = a * sy - b * shx;
			y = b * sx - a * shy;
		}

		private double DeterminantReciprocal
		{
			get { return 1.0 / (sx * sy - shy * shx); }
		}


		public double GetScale()
		{
			double x = 0.707106781 * sx + 0.707106781 * shx;
			double y = 0.707106781 * shy + 0.707106781 * sy;
			return Math.Sqrt(x * x + y * y);
		}

		public bool IsValid(double epsilon)
		{
			return Math.Abs(sx) > epsilon && Math.Abs(sy) > epsilon;
		}

		public double Rotation()
		{
			double x1 = 0.0;
			double y1 = 0.0;
			double x2 = 1.0;
			double y2 = 0.0;
			Transform(ref x1, ref y1);
			Transform(ref x2, ref y2);
			return Math.Atan2(y2 - y1, x2 - x1);
		}

		public void Translation(out double dx, out double dy)
		{
			dx = tx;
			dy = ty;
		}

		public void Scaling(out double x, out double y)
		{
			double x1 = 0.0;
			double y1 = 0.0;
			double x2 = 1.0;
			double y2 = 1.0;
            AffineTransform t = new AffineTransform(this);
			t *= NewRotation(-Rotation());
			t.Transform(ref x1, ref y1);
			t.Transform(ref x2, ref y2);
			x = x2 - x1;
			y = y2 - y1;
		}

		public void ScalingAbs(out double x, out double y)
		{
			x = Math.Sqrt(sx * sx + shx * shx);
			y = Math.Sqrt(shy * shy + sy * sy);
		}
	}
}
