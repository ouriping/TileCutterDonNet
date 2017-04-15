using System;
namespace BMap
{
	public class Pixel
	{
		private int x;
		private int y;
		public int X
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
		public int Y
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
		public Pixel(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
		public bool Equals(Pixel other)
		{
			return other != null && other.X == this.X && other.Y == this.Y;
		}
		public override string ToString()
		{
			return this.X + ", " + this.Y;
		}
	}
}
