using BMap;
using System;
using System.Drawing;
using System.IO;
namespace TileCutter
{
	internal class Cutter
	{
		private LngLat centerPosition = new LngLat(0.0, 0.0);
		private string sourceImageFile = "";
		private string outputPath = "";
		private int minZoom;
		private int maxZoom;
		private int sourceZoom;
		private int totalCount;
		private int finishCount;
		private bool isFinish;
		public Cutter(string imgFilePath, string outputPath, LngLat center, int minZoom, int maxZoom, int sourceZoom)
		{
			this.sourceImageFile = imgFilePath;
			this.outputPath = outputPath;
			this.centerPosition = center;
			this.minZoom = minZoom;
			this.maxZoom = maxZoom;
			this.sourceZoom = sourceZoom;
		}
		private void calculateCutPoint(Size size, int zoom, out BMap.Point centerTileCoord, out BMap.Point centerCutPoint)
		{
			LngLatProjection lngLatProjection = new LngLatProjection();
			BMap.Point point = lngLatProjection.LngLatToMercator(this.centerPosition);
			int zoomUnits = this.getZoomUnits(zoom);
			centerTileCoord = new BMap.Point(Math.Floor(Math.Floor(point.X / 256.0) / (double)zoomUnits), Math.Floor(Math.Floor(point.Y / 256.0) / (double)zoomUnits));
			int num = (int)Math.Round(point.X / (double)zoomUnits % 256.0);
			int num2 = (int)Math.Round(point.Y / (double)zoomUnits % 256.0);
			int num3 = size.Width / 2;
			int num4 = size.Height / 2;
			int num5 = num3 - num;
			int num6 = num4 + num2;
			if (point.X < 0.0)
			{
				num5 -= 256;
			}
			if (point.Y < 0.0)
			{
				num6 += 256;
			}
			centerCutPoint = new BMap.Point((double)num5, (double)num6);
		}
		private int getZoomUnits(int zoom)
		{
			return (int)Math.Pow(2.0, (double)(18 - zoom));
		}
		public void BeginCut()
		{
			this.isFinish = false;
			Bitmap bitmap = new Bitmap(this.sourceImageFile);
			this.cutImage(this.sourceImageFile, this.sourceZoom);
			for (int i = this.minZoom; i <= this.maxZoom; i++)
			{
				if (i != this.sourceZoom)
				{
					Size newSize = default(Size);
					double num = Math.Pow(2.0, (double)(this.sourceZoom - i));
					newSize.Height = (int)((double)bitmap.Height / num);
					newSize.Width = (int)((double)bitmap.Width / num);
					if (newSize.Width != 0 && newSize.Height != 0)
					{
						Bitmap bitmap2 = new Bitmap(bitmap, newSize);
						string text = string.Concat(new object[]
						{
							this.outputPath,
							"/temp",
							i,
							".png"
						});
						bitmap2.Save(text);
						this.cutImage(text, i);
						File.Delete(string.Concat(new object[]
						{
							this.outputPath,
							"/temp",
							i,
							".png"
						}));
						bitmap2.Dispose();
					}
				}
			}
			this.isFinish = true;
		}
		private void cutImage(string fileName, int zoom)
		{
			Bitmap bitmap = new Bitmap(fileName);
			BMap.Point point;
			BMap.Point point2;
			this.calculateCutPoint(bitmap.Size, zoom, out point, out point2);
			int num = (int)point2.X;
			int num2 = (int)point2.Y;
			Directory.CreateDirectory(this.outputPath + "/tiles/" + zoom);
			int width = bitmap.Size.Width;
			int height = bitmap.Size.Height;
			int num3 = 0;
			int num4 = 0;
			if ((width - num) % 256 != 0)
			{
				num3 = 1;
			}
			if (num2 % 256 != 0)
			{
				num4 = 1;
			}
			for (int i = 0; i < (width - num) / 256 + num3; i++)
			{
				for (int j = 0; j < num2 / 256 + num4; j++)
				{
					Bitmap bitmap2 = new Bitmap(256, 256);
					for (int k = 0; k < 256; k++)
					{
						for (int l = 0; l < 256; l++)
						{
							Color color = Color.FromArgb(0, 0, 0, 0);
							if (i * 256 + num + k >= 0 && i * 256 + num + k < bitmap.Width && num2 - (j + 1) * 256 + l >= 0 && num2 - (j + 1) * 256 + l < bitmap.Height)
							{
								color = bitmap.GetPixel(i * 256 + num + k, num2 - (j + 1) * 256 + l);
							}
							bitmap2.SetPixel(k, l, color);
						}
					}
					bitmap2.Save(string.Concat(new object[]
					{
						this.outputPath,
						"/tiles/",
						zoom,
						"/tile",
						point.X + (double)i,
						"_",
						point.Y + (double)j,
						".png"
					}));
					this.finishCount++;
				}
			}
			if ((height - num2) % 256 != 0)
			{
				num4 = 1;
			}
			for (int m = 0; m < (width - num) / 256 + num3; m++)
			{
				for (int n = 0; n < (height - num2) / 256 + num4; n++)
				{
					Bitmap bitmap3 = new Bitmap(256, 256);
					for (int num5 = 0; num5 < 256; num5++)
					{
						for (int num6 = 0; num6 < 256; num6++)
						{
							Color color2 = Color.FromArgb(0, 0, 0, 0);
							if (m * 256 + num + num5 >= 0 && m * 256 + num + num5 < bitmap.Width && n * 256 + num2 + num6 >= 0 && n * 256 + num2 + num6 < bitmap.Height)
							{
								color2 = bitmap.GetPixel(m * 256 + num + num5, n * 256 + num2 + num6);
							}
							bitmap3.SetPixel(num5, num6, color2);
						}
					}
					bitmap3.Save(string.Concat(new object[]
					{
						this.outputPath,
						"/tiles/",
						zoom,
						"/tile",
						point.X + (double)m,
						"_",
						point.Y - (double)n - 1.0,
						".png"
					}));
					this.finishCount++;
				}
			}
			if (num % 256 != 0)
			{
				num3 = 1;
			}
			for (int num7 = 0; num7 < num / 256 + num3; num7++)
			{
				for (int num8 = 0; num8 < (height - num2) / 256 + num4; num8++)
				{
					Bitmap bitmap4 = new Bitmap(256, 256);
					for (int num9 = 0; num9 < 256; num9++)
					{
						for (int num10 = 0; num10 < 256; num10++)
						{
							Color color3 = Color.FromArgb(0, 0, 0, 0);
							if (num - (num7 + 1) * 256 + num9 >= 0 && num - (num7 + 1) * 256 + num9 < bitmap.Width && num8 * 256 + num2 + num10 >= 0 && num8 * 256 + num2 + num10 < bitmap.Height)
							{
								color3 = bitmap.GetPixel(num - (num7 + 1) * 256 + num9, num8 * 256 + num2 + num10);
							}
							bitmap4.SetPixel(num9, num10, color3);
						}
					}
					bitmap4.Save(string.Concat(new object[]
					{
						this.outputPath,
						"/tiles/",
						zoom,
						"/tile",
						point.X - (double)num7 - 1.0,
						"_",
						point.Y - (double)num8 - 1.0,
						".png"
					}));
					this.finishCount++;
				}
			}
			if ((height - num2) % 256 != 0)
			{
				num4 = 1;
			}
			for (int num11 = 0; num11 < num / 256 + num3; num11++)
			{
				for (int num12 = 0; num12 < num2 / 256 + num4; num12++)
				{
					Bitmap bitmap5 = new Bitmap(256, 256);
					for (int num13 = 0; num13 < 256; num13++)
					{
						for (int num14 = 0; num14 < 256; num14++)
						{
							Color color4 = Color.FromArgb(0, 0, 0, 0);
							if (num - (num11 + 1) * 256 + num13 >= 0 && num - (num11 + 1) * 256 + num13 < bitmap.Width && num2 - (num12 + 1) * 256 + num14 >= 0 && num2 - (num12 + 1) * 256 + num14 < bitmap.Height)
							{
								color4 = bitmap.GetPixel(num - (num11 + 1) * 256 + num13, num2 - (num12 + 1) * 256 + num14);
							}
							bitmap5.SetPixel(num13, num14, color4);
						}
					}
					bitmap5.Save(string.Concat(new object[]
					{
						this.outputPath,
						"/tiles/",
						zoom,
						"/tile",
						point.X - (double)num11 - 1.0,
						"_",
						point.Y + (double)num12,
						".png"
					}));
					this.finishCount++;
				}
			}
			bitmap.Dispose();
		}
		public int GetFinishCount()
		{
			return this.finishCount;
		}
		public bool IsFinish()
		{
			return this.isFinish;
		}
	}
}
