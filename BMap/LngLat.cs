using System;
namespace BMap
{
	public class LngLat
	{
		private double lng;
		private double lat;
		public double Lng
		{
			get
			{
				return this.lng;
			}
			set
			{
				this.lng = value;
			}
		}
		public double Lat
		{
			get
			{
				return this.lat;
			}
			set
			{
				this.lat = value;
			}
		}
		public LngLat(double lng, double lat)
		{
			this.Lng = lng;
			this.Lat = lat;
		}
		public bool Equals(LngLat other)
		{
			return other != null && other.Lng == this.Lng && other.Lat == this.Lat;
		}
		public override string ToString()
		{
			return this.Lng + ", " + this.Lat;
		}
	}
}
