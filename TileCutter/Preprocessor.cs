using System;
using System.Drawing;
namespace TileCutter
{
	internal static class Preprocessor
	{
		public static void CalcZoomInfo(Bitmap image, out int minZoom, out int maxZoom)
		{
			minZoom = 1;
			int num = 0;
			int i = Math.Min(image.Width, image.Height);
			while (i > 256)
			{
				i /= 2;
				num++;
			}
			maxZoom = num + 1;
		}
		public static int GetTotalCount(Bitmap image, int minZoom, int maxZoom, int sourceZoom)
		{
			int num = 0;
			int width = image.Width;
			int height = image.Height;
			for (int i = minZoom; i <= maxZoom; i++)
			{
				num += Preprocessor.calcTiles(Convert.ToInt32(Math.Pow(2.0, (double)(i - sourceZoom)) * (double)width), Convert.ToInt32(Math.Pow(2.0, (double)(i - sourceZoom)) * (double)height));
			}
			return num;
		}
		private static int calcTiles(int width, int height)
		{
			double num = Math.Ceiling((double)width / 256.0 / 2.0);
			double num2 = Math.Ceiling((double)height / 256.0 / 2.0);
			return (int)Math.Round(4.0 * num * num2);
		}
	}
}
