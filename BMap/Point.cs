using System;
namespace BMap
{
	public class Point
	{
		private double x;
		private double y;
		public double X
		{
			get
			{
				return this.x;
			}
			set
			{
				this.x = value;
			}
		}
		public double Y
		{
			get
			{
				return this.y;
			}
			set
			{
				this.y = value;
			}
		}
		public Point(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}
		public bool Equals(Point other)
		{
			return other != null && other.X == this.X && other.Y == this.Y;
		}
		public override string ToString()
		{
			return this.X + ", " + this.Y;
		}
	}
}
