using BMap;
using System;
using System.IO;
namespace TileCutter
{
	internal class HTMLGenerator
	{
		private LayerUsage layerUsage;
		private string outputPath;
		private int minZoom;
		private int maxZoom;
		private int sourceZoom;
		private LngLat center;
		private string mapTypeName;
		public HTMLGenerator(string outputPath, LayerUsage layerUsage, LngLat center, int minZoom, int maxZoom, int sourceZoom)
		{
			this.outputPath = outputPath;
			this.layerUsage = layerUsage;
			this.center = center;
			this.minZoom = minZoom;
			this.maxZoom = maxZoom;
			this.sourceZoom = sourceZoom;
		}
		public HTMLGenerator(string outputPath, LayerUsage layerUsage, LngLat center, int minZoom, int maxZoom, int sourceZoom, string mapTypeName) : this(outputPath, layerUsage, center, minZoom, maxZoom, sourceZoom)
		{
			this.mapTypeName = mapTypeName;
		}
		public void Generate()
		{
			StreamWriter streamWriter = File.CreateText(this.outputPath + "/index.html");
			streamWriter.WriteLine("<!DOCTYPE html>");
			streamWriter.WriteLine("<html>");
			streamWriter.WriteLine("<head>");
			streamWriter.WriteLine("<title>自定义地图类型</title>");
			streamWriter.WriteLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
			streamWriter.WriteLine("<script type=\"text/javascript\" src=\"http://api.map.baidu.com/api?v=1.2\"></script>");
			streamWriter.WriteLine("</head>");
			streamWriter.WriteLine("<body>");
			streamWriter.WriteLine("<div id=\"map\" style=\"width:800px;height:540px\"></div>");
			streamWriter.WriteLine("<script type=\"text/javascript\">");
			streamWriter.WriteLine("var tileLayer = new BMap.TileLayer();");
			streamWriter.WriteLine("tileLayer.getTilesUrl = function(tileCoord, zoom) {");
			streamWriter.WriteLine("    var x = tileCoord.x;");
			streamWriter.WriteLine("    var y = tileCoord.y;");
			streamWriter.WriteLine("    return 'tiles/' + zoom + '/tile' + x + '_' + y + '.png';");
			streamWriter.WriteLine("}");
			if (this.layerUsage == LayerUsage.AsMapType)
			{
				streamWriter.WriteLine(string.Concat(new object[]
				{
					"var ",
					this.mapTypeName,
					" = new BMap.MapType('",
					this.mapTypeName,
					"', tileLayer, {minZoom: ",
					this.minZoom,
					", maxZoom: ",
					this.maxZoom,
					"});"
				}));
				streamWriter.WriteLine("var map = new BMap.Map('map', {mapType: " + this.mapTypeName + "});");
			}
			else
			{
				streamWriter.WriteLine("var map = new BMap.Map('map');");
				streamWriter.WriteLine("map.addTileLayer(tileLayer);");
			}
			streamWriter.WriteLine("map.addControl(new BMap.NavigationControl());");
			streamWriter.WriteLine(string.Concat(new object[]
			{
				"map.centerAndZoom(new BMap.Point(",
				this.center.Lng,
				", ",
				this.center.Lat,
				"), ",
				this.sourceZoom,
				");"
			}));
			streamWriter.WriteLine("</script>");
			streamWriter.WriteLine("</body>");
			streamWriter.WriteLine("</html>");
			streamWriter.Close();
		}
	}
}
