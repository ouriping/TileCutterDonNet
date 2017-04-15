using System;
namespace BMap
{
	public class LngLatProjection : Projection
	{
		public override Pixel LngLatToPixel(LngLat lngLat, double zoom)
		{
			Point point = this.LngLatToMercator(lngLat);
			double zoomUnits = ProjectionUtil.GetZoomUnits(zoom);
			return new Pixel((int)Math.Round(point.X / zoomUnits), (int)Math.Round(point.Y / zoomUnits));
		}
		public override LngLat PixelToLngLat(Pixel pixel, double zoom)
		{
			double zoomUnits = ProjectionUtil.GetZoomUnits(zoom);
			Point point = new Point((double)pixel.X * zoomUnits, (double)pixel.Y * zoomUnits);
			return this.MercatorToLngLat(point);
		}
		public Point LngLatToMercator(LngLat lngLat)
		{
			double[] array = new double[0];
			for (int i = 0; i < ProjectionUtil.LLBAND.Length; i++)
			{
				if (lngLat.Lat >= (double)ProjectionUtil.LLBAND[i])
				{
					array = ProjectionUtil.LL2MC[i];
					break;
				}
			}
			if (array.Length == 0)
			{
				for (int j = 0; j < ProjectionUtil.LLBAND.Length; j++)
				{
					if (lngLat.Lat <= (double)(-(double)ProjectionUtil.LLBAND[j]))
					{
						array = ProjectionUtil.LL2MC[j];
						break;
					}
				}
			}
			return this.convert(lngLat, array, 0);
		}
		public LngLat MercatorToLngLat(Point point)
		{
			double[] factor = new double[0];
			Point point2 = new Point(Math.Abs(point.X), Math.Abs(point.Y));
			for (int i = 0; i < ProjectionUtil.MCBAND.Length; i++)
			{
				if (point2.Y >= ProjectionUtil.MCBAND[i])
				{
					factor = ProjectionUtil.MC2LL[i];
					break;
				}
			}
			Point point3 = this.convert(new LngLat(point.X, point.Y), factor, 1);
			return new LngLat(point3.X, point3.Y);
		}
		private Point convert(LngLat lngLat, double[] factor, byte dir)
		{
			double num = factor[0] + factor[1] * Math.Abs(lngLat.Lng);
			double num2 = Math.Abs(lngLat.Lat) / factor[9];
			double num3 = factor[2] + factor[3] * num2 + factor[4] * num2 * num2 + factor[5] * num2 * num2 * num2 + factor[6] * num2 * num2 * num2 * num2 + factor[7] * num2 * num2 * num2 * num2 * num2 + factor[8] * num2 * num2 * num2 * num2 * num2 * num2;
			num *= (double)((lngLat.Lng < 0.0) ? -1 : 1);
			num3 *= (double)((lngLat.Lat < 0.0) ? -1 : 1);
			if (dir == 0)
			{
				return new Point(double.Parse(num.ToString("f2")), double.Parse(num3.ToString("f2")));
			}
			return new Point(double.Parse(num.ToString("f6")), double.Parse(num3.ToString("f6")));
		}
	}
}
