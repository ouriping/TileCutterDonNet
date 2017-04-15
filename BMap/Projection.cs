using System;
namespace BMap
{
	public abstract class Projection
	{
		public abstract Pixel LngLatToPixel(LngLat lngLat, double zoom);
		public abstract LngLat PixelToLngLat(Pixel pixel, double zoom);
	}
}
