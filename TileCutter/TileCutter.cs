using BMap;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
namespace TileCutter
{
	[ComVisible(true)]
	[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class TileCutter : Form
	{
		public enum Step
		{
			SelectImage,
			SelectPath,
			SetOutputFormat,
			SetCenter,
			SetZoom,
			SetLayer,
			BeginCut
		}
		public enum OutputFormat
		{
			TilesAndCodes,
			TilesOnly
		}
		private const byte STEP_NUM = 7;
		private GroupBox[] grpArray = new GroupBox[7];
		private Label[] lblStepArray = new Label[7];
		private byte curStep;
		private string sourceImageFileName = "";
		private string outputPath = "";
		private TileCutter.OutputFormat outputFormat;
		private LngLat centerPosition = new LngLat(0.0, 0.0);
		private bool autoZoom = true;
		private int minZoom;
		private int maxZoom;
		private int sourceZoom;
		private LayerUsage layerUsage;
		private string mapTypeName = "MyMap";
		private int totalCount;
		private int finishCount;
		private Cutter myCutter;
		private IContainer components;
		private Label lblSelImage;
		private Label lblLayerSet;
		private Label lblZoomSet;
		private Label lblCenterSet;
		private Label lblOutputFormat;
		private Label lblPathSet;
		private Label lblCurProcessName;
		private GroupBox grpSelImage;
		private Button btnSelImage;
		private TextBox txtFilePath;
		private Label label2;
		private Button btnNext;
		private Button btnPrev;
		private GroupBox grpCenterSet;
		private Label label5;
		private Label label6;
		private TextBox txtLat;
		private Label label8;
		private TextBox txtLng;
		private Label label7;
		private GroupBox grpZoomSet;
		private ComboBox cbxSourceZoom;
		private ComboBox cbxMaxZoom;
		private ComboBox cbxMinZoom;
		private Label lblSourceZoom;
		private Label lblMaxZoom;
		private Label lblMinZoom;
		private RadioButton rdAutoZoom;
		private Label label11;
		private Label label12;
		private GroupBox grpLayerSet;
		private RadioButton rdTileLayer;
		private RadioButton rdMapType;
		private Label label17;
		private Label label18;
		private TextBox txtMapTypeName;
		private GroupBox grpBeginCut;
		private Label label14;
		private Label label15;
		private Label label19;
		private Label lblBeginCut;
		private OpenFileDialog openFileDialog;
		private FolderBrowserDialog folderBrowserDialog;
		private GroupBox grpPathSet;
		private Button btnSetPath;
		private TextBox txtOutputPath;
		private Label label1;
		private Label lblTitlePathSet;
		private GroupBox grpOutputFormat;
		private RadioButton rdTilesOnly;
		private RadioButton rdTilesAndCode;
		private Label label3;
		private Label label4;
		private Label lblLngLatError;
		private RadioButton rdCustomZoom;
		private Label label9;
		private Label label10;
		private Label label13;
		private Label label22;
		private Label label21;
		private ProgressBar proBar;
		private Label lblFinalLayer;
		private Label lblFinalZoom;
		private Label lblFinalCenter;
		private Label lblFinalOutputFormat;
		private TextBox txtFinalOutputPath;
		private TextBox txtFinalImagePath;
		private PictureBox picStepBg;
		private Label lblPreview;
		private PictureBox picPreview;
		private Label lblCutProcess;
		private System.Windows.Forms.Timer cutTimer;
		private WebBrowser browser;
		private Label label16;
		public TileCutter()
		{
			this.InitializeComponent();
			this.browser.ObjectForScripting = this;
			this.GenPickerPage();
			this.browser.Url = new Uri(AppDomain.CurrentDomain.BaseDirectory + "picker.html", UriKind.Absolute);
		}
		private void TileCutter_Load(object sender, EventArgs e)
		{
			this.grpArray[0] = this.grpSelImage;
			this.grpArray[1] = this.grpPathSet;
			this.grpArray[2] = this.grpOutputFormat;
			this.grpArray[3] = this.grpCenterSet;
			this.grpArray[4] = this.grpZoomSet;
			this.grpArray[5] = this.grpLayerSet;
			this.grpArray[6] = this.grpBeginCut;
			this.lblStepArray[0] = this.lblSelImage;
			this.lblStepArray[1] = this.lblPathSet;
			this.lblStepArray[2] = this.lblOutputFormat;
			this.lblStepArray[3] = this.lblCenterSet;
			this.lblStepArray[4] = this.lblZoomSet;
			this.lblStepArray[5] = this.lblLayerSet;
			this.lblStepArray[6] = this.lblBeginCut;
			this.updateInterface();
		}
		private void btnNext_Click(object sender, EventArgs e)
		{
			if (this.curStep == 3)
			{
				double num = double.Parse(this.txtLng.Text);
				double num2 = double.Parse(this.txtLat.Text);
				if (num > 180.0 || num < -180.0)
				{
					this.txtLng.Focus();
					return;
				}
				if (num2 > 90.0 || num2 < -90.0)
				{
					this.txtLat.Focus();
					return;
				}
				this.centerPosition.Lng = num;
				this.centerPosition.Lat = num2;
			}
			if (this.curStep == 4)
			{
				if (this.rdAutoZoom.Checked)
				{
					this.autoZoom = true;
				}
				else
				{
					if (this.rdCustomZoom.Checked)
					{
						this.autoZoom = false;
						if (this.cbxMinZoom.Text == "" || this.cbxMaxZoom.Text == "" || this.cbxSourceZoom.Text == "")
						{
							MessageBox.Show("请填写级别信息");
							return;
						}
						this.minZoom = Convert.ToInt32(this.cbxMinZoom.Text);
						this.maxZoom = Convert.ToInt32(this.cbxMaxZoom.Text);
						this.sourceZoom = Convert.ToInt32(this.cbxSourceZoom.Text);
						if (this.maxZoom < this.minZoom)
						{
							MessageBox.Show("最大级别不能小于最小级别");
							return;
						}
						if (this.sourceZoom < this.minZoom || this.sourceZoom > this.maxZoom)
						{
							MessageBox.Show(string.Concat(new string[]
							{
								"原图级别应位于",
								this.minZoom.ToString(),
								"与",
								this.maxZoom.ToString(),
								"之间"
							}));
							return;
						}
					}
				}
			}
			if (this.curStep == 5)
			{
				if (this.rdMapType.Checked)
				{
					this.layerUsage = LayerUsage.AsMapType;
					if (this.txtMapTypeName.Text == "")
					{
						MessageBox.Show("请填写地图类型名称");
						return;
					}
				}
				else
				{
					if (this.rdTileLayer.Checked)
					{
						this.layerUsage = LayerUsage.AsTileLayer;
					}
				}
			}
			if (this.curStep == 6)
			{
				this.btnNext.Enabled = false;
				this.btnPrev.Enabled = false;
				this.lblCutProcess.Text = "切图进行中...";
				Bitmap image = new Bitmap(this.sourceImageFileName);
				if (this.minZoom == 0 || this.maxZoom == 0)
				{
					Preprocessor.CalcZoomInfo(image, out this.minZoom, out this.maxZoom);
					this.sourceZoom = this.maxZoom;
				}
				this.totalCount = Preprocessor.GetTotalCount(image, this.minZoom, this.maxZoom, this.sourceZoom);
				this.proBar.Minimum = 0;
				this.proBar.Maximum = this.totalCount;
				this.proBar.Value = 0;
				this.cutTimer.Start();
				Thread thread = new Thread(new ThreadStart(this.beginCut));
				thread.Start();
			}
			if (this.curStep < 6)
			{
				this.curStep += 1;
				if (this.curStep == 6)
				{
					this.lblCutProcess.Text = "准备就绪";
					this.proBar.Value = 0;
					this.txtFinalImagePath.Text = this.sourceImageFileName;
					this.txtFinalOutputPath.Text = this.outputPath;
					this.lblFinalCenter.Text = this.centerPosition.Lng + ", " + this.centerPosition.Lat;
					if (this.autoZoom)
					{
						this.lblFinalZoom.Text = "自动控制";
					}
					else
					{
						this.lblFinalZoom.Text = string.Concat(new object[]
						{
							"最小级别：",
							this.minZoom,
							"，最大级别：",
							this.maxZoom,
							"，原图所在级别：",
							this.sourceZoom
						});
					}
					if (this.outputFormat == TileCutter.OutputFormat.TilesAndCodes)
					{
						this.lblFinalOutputFormat.Text = "图块及代码";
					}
					else
					{
						if (this.outputFormat == TileCutter.OutputFormat.TilesOnly)
						{
							this.lblFinalOutputFormat.Text = "仅图块";
						}
					}
					if (this.layerUsage == LayerUsage.AsMapType)
					{
						this.lblFinalLayer.Text = "做为地图类型，名称：" + this.mapTypeName;
					}
					else
					{
						if (this.layerUsage == LayerUsage.AsTileLayer)
						{
							this.lblFinalLayer.Text = "做为独立图层";
						}
					}
				}
				if (this.curStep == 6)
				{
					this.btnNext.Text = "开始切图";
				}
				this.updateInterface();
				return;
			}
		}
		private void beginCut()
		{
			this.myCutter = new Cutter(this.sourceImageFileName, this.outputPath, this.centerPosition, this.minZoom, this.maxZoom, this.sourceZoom);
			this.myCutter.BeginCut();
			if (this.outputFormat == TileCutter.OutputFormat.TilesAndCodes)
			{
				HTMLGenerator hTMLGenerator;
				if (this.layerUsage == LayerUsage.AsMapType)
				{
					hTMLGenerator = new HTMLGenerator(this.outputPath, this.layerUsage, this.centerPosition, this.minZoom, this.maxZoom, this.sourceZoom, this.mapTypeName);
				}
				else
				{
					hTMLGenerator = new HTMLGenerator(this.outputPath, this.layerUsage, this.centerPosition, this.minZoom, this.maxZoom, this.sourceZoom);
				}
				hTMLGenerator.Generate();
			}
		}
		private void updateInterface()
		{
			for (int i = 0; i < 7; i++)
			{
				if (i == (int)this.curStep)
				{
					this.grpArray[i].Visible = true;
					this.grpArray[i].Visible = true;
					this.lblStepArray[i].ForeColor = Color.Black;
				}
				else
				{
					this.grpArray[i].Visible = false;
					this.lblStepArray[i].ForeColor = Color.Gray;
				}
			}
			if (this.curStep == 0)
			{
				this.btnPrev.Enabled = false;
				if (this.sourceImageFileName == "")
				{
					this.btnNext.Enabled = false;
				}
				else
				{
					this.btnNext.Enabled = true;
				}
			}
			if (this.curStep == 1)
			{
				this.btnPrev.Enabled = true;
				if (this.outputPath == "")
				{
					this.btnNext.Enabled = false;
					return;
				}
				this.btnNext.Enabled = true;
			}
		}
		private void btnSelImage_Click(object sender, EventArgs e)
		{
			if (this.openFileDialog.ShowDialog() == DialogResult.OK)
			{
				this.sourceImageFileName = this.openFileDialog.FileName;
				this.txtFilePath.Text = this.sourceImageFileName;
				this.btnNext.Enabled = true;
				Image image = Image.FromFile(this.sourceImageFileName);
				Size size = new Size(image.Width, image.Height);
				Size newSize = new Size(size.Width, size.Height);
				if (size.Height > this.picPreview.Height || size.Width > this.picPreview.Width)
				{
					newSize.Height = this.picPreview.Height;
					newSize.Width = Convert.ToInt32(Convert.ToDouble(newSize.Height) / Convert.ToDouble(size.Height) * (double)size.Width);
					if (newSize.Width > this.picPreview.Width)
					{
						newSize.Width = this.picPreview.Width;
						newSize.Height = newSize.Width / size.Width * size.Height;
					}
				}
				Bitmap image2 = new Bitmap(image, newSize);
				this.picPreview.Image = image2;
				this.lblPreview.Visible = true;
			}
		}
		private void btnPrev_Click(object sender, EventArgs e)
		{
			if (this.curStep > 0)
			{
				this.curStep -= 1;
			}
			if (this.curStep == 5)
			{
				this.btnNext.Text = "下一步";
			}
			this.updateInterface();
		}
		private void btnSetPath_Click(object sender, EventArgs e)
		{
			if (this.folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				this.outputPath = this.folderBrowserDialog.SelectedPath;
				this.txtOutputPath.Text = this.outputPath;
				this.btnNext.Enabled = true;
			}
		}
		private void rdTilesAndCode_CheckedChanged(object sender, EventArgs e)
		{
			this.setOutputFormat();
		}
		private void rdTilesOnly_CheckedChanged(object sender, EventArgs e)
		{
			this.setOutputFormat();
		}
		private void setOutputFormat()
		{
			if (this.rdTilesAndCode.Checked)
			{
				this.outputFormat = TileCutter.OutputFormat.TilesAndCodes;
				return;
			}
			this.outputFormat = TileCutter.OutputFormat.TilesOnly;
		}
		private void rdAutoZoom_CheckedChanged(object sender, EventArgs e)
		{
			if (this.rdAutoZoom.Checked)
			{
				this.lblMaxZoom.Enabled = false;
				this.lblMinZoom.Enabled = false;
				this.lblSourceZoom.Enabled = false;
				this.cbxMinZoom.Enabled = false;
				this.cbxMaxZoom.Enabled = false;
				this.cbxSourceZoom.Enabled = false;
				this.autoZoom = true;
				return;
			}
			this.lblMaxZoom.Enabled = true;
			this.lblMinZoom.Enabled = true;
			this.lblSourceZoom.Enabled = true;
			this.cbxMinZoom.Enabled = true;
			this.cbxMaxZoom.Enabled = true;
			this.cbxSourceZoom.Enabled = true;
			this.autoZoom = false;
		}
		private void cutTimer_Tick(object sender, EventArgs e)
		{
			this.finishCount = this.myCutter.GetFinishCount();
			if (this.finishCount <= this.proBar.Maximum)
			{
				this.proBar.Value = this.finishCount;
			}
			else
			{
				this.proBar.Value = this.proBar.Maximum;
			}
			if (this.myCutter.IsFinish())
			{
				this.cutTimer.Stop();
				this.btnNext.Enabled = true;
				this.btnPrev.Enabled = true;
				this.lblCutProcess.Text = "完成";
			}
		}
		public void ShowLngLat(string lng, string lat)
		{
			this.txtLng.Text = lng;
			this.txtLat.Text = lat;
		}
		private void GenPickerPage()
		{
			StreamWriter streamWriter = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "picker.html");
			streamWriter.WriteLine("<!DOCTYPE html>");
			streamWriter.WriteLine("<html>");
			streamWriter.WriteLine("<head>");
			streamWriter.WriteLine("<title>拾取坐标</title>");
			streamWriter.WriteLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
			streamWriter.WriteLine("<script type=\"text/javascript\" src=\"http://api.map.baidu.com/api?v=1.2\"></script>");
			streamWriter.WriteLine("<style>body{margin:0px;padding:0px;}</style>");
			streamWriter.WriteLine("</head>");
			streamWriter.WriteLine("<body>");
			streamWriter.WriteLine("<div id=\"map\" style=\"width:442px;height:238px;\"></div>");
			streamWriter.WriteLine("<script type=\"text/javascript\">");
			streamWriter.WriteLine("var map=new BMap.Map('map');map.centerAndZoom(new BMap.Point(116.404,39.915),11);map.setDefaultCursor('default');var menu=new BMap.ContextMenu();menu.addItem(new BMap.MenuItem('获取此处坐标',function(a){window.external.ShowLngLat(a.lng.toString(),a.lat.toString())}));map.addContextMenu(menu);map.addControl(new BMap.NavigationControl({type: BMAP_NAVIGATION_CONTROL_ZOOM}))");
			streamWriter.WriteLine("</script>");
			streamWriter.WriteLine("</body>");
			streamWriter.WriteLine("</html>");
			streamWriter.Close();
		}
		private void RemovePickerPage()
		{
			File.Delete(AppDomain.CurrentDomain.BaseDirectory + "picker.html");
		}
		private void TileCutter_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.RemovePickerPage();
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.lblSelImage = new System.Windows.Forms.Label();
            this.lblLayerSet = new System.Windows.Forms.Label();
            this.lblZoomSet = new System.Windows.Forms.Label();
            this.lblCenterSet = new System.Windows.Forms.Label();
            this.lblOutputFormat = new System.Windows.Forms.Label();
            this.lblPathSet = new System.Windows.Forms.Label();
            this.lblCurProcessName = new System.Windows.Forms.Label();
            this.grpSelImage = new System.Windows.Forms.GroupBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.btnSelImage = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrev = new System.Windows.Forms.Button();
            this.grpCenterSet = new System.Windows.Forms.GroupBox();
            this.label16 = new System.Windows.Forms.Label();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.lblLngLatError = new System.Windows.Forms.Label();
            this.txtLat = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtLng = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.grpZoomSet = new System.Windows.Forms.GroupBox();
            this.rdCustomZoom = new System.Windows.Forms.RadioButton();
            this.cbxSourceZoom = new System.Windows.Forms.ComboBox();
            this.cbxMaxZoom = new System.Windows.Forms.ComboBox();
            this.cbxMinZoom = new System.Windows.Forms.ComboBox();
            this.lblSourceZoom = new System.Windows.Forms.Label();
            this.lblMaxZoom = new System.Windows.Forms.Label();
            this.lblMinZoom = new System.Windows.Forms.Label();
            this.rdAutoZoom = new System.Windows.Forms.RadioButton();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.grpLayerSet = new System.Windows.Forms.GroupBox();
            this.txtMapTypeName = new System.Windows.Forms.TextBox();
            this.rdMapType = new System.Windows.Forms.RadioButton();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.rdTileLayer = new System.Windows.Forms.RadioButton();
            this.grpBeginCut = new System.Windows.Forms.GroupBox();
            this.lblCutProcess = new System.Windows.Forms.Label();
            this.txtFinalOutputPath = new System.Windows.Forms.TextBox();
            this.txtFinalImagePath = new System.Windows.Forms.TextBox();
            this.lblFinalLayer = new System.Windows.Forms.Label();
            this.lblFinalZoom = new System.Windows.Forms.Label();
            this.lblFinalCenter = new System.Windows.Forms.Label();
            this.lblFinalOutputFormat = new System.Windows.Forms.Label();
            this.proBar = new System.Windows.Forms.ProgressBar();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.lblBeginCut = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.grpPathSet = new System.Windows.Forms.GroupBox();
            this.btnSetPath = new System.Windows.Forms.Button();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblTitlePathSet = new System.Windows.Forms.Label();
            this.grpOutputFormat = new System.Windows.Forms.GroupBox();
            this.rdTilesOnly = new System.Windows.Forms.RadioButton();
            this.rdTilesAndCode = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.picStepBg = new System.Windows.Forms.PictureBox();
            this.cutTimer = new System.Windows.Forms.Timer(this.components);
            this.grpSelImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.grpCenterSet.SuspendLayout();
            this.grpZoomSet.SuspendLayout();
            this.grpLayerSet.SuspendLayout();
            this.grpBeginCut.SuspendLayout();
            this.grpPathSet.SuspendLayout();
            this.grpOutputFormat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStepBg)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSelImage
            // 
            this.lblSelImage.AutoSize = true;
            this.lblSelImage.ForeColor = System.Drawing.Color.Gray;
            this.lblSelImage.Location = new System.Drawing.Point(23, 72);
            this.lblSelImage.Name = "lblSelImage";
            this.lblSelImage.Size = new System.Drawing.Size(53, 12);
            this.lblSelImage.TabIndex = 19;
            this.lblSelImage.Text = "选择图片";
            // 
            // lblLayerSet
            // 
            this.lblLayerSet.AutoSize = true;
            this.lblLayerSet.ForeColor = System.Drawing.Color.Gray;
            this.lblLayerSet.Location = new System.Drawing.Point(23, 267);
            this.lblLayerSet.Name = "lblLayerSet";
            this.lblLayerSet.Size = new System.Drawing.Size(53, 12);
            this.lblLayerSet.TabIndex = 18;
            this.lblLayerSet.Text = "图层设置";
            // 
            // lblZoomSet
            // 
            this.lblZoomSet.AutoSize = true;
            this.lblZoomSet.ForeColor = System.Drawing.Color.Gray;
            this.lblZoomSet.Location = new System.Drawing.Point(23, 228);
            this.lblZoomSet.Name = "lblZoomSet";
            this.lblZoomSet.Size = new System.Drawing.Size(53, 12);
            this.lblZoomSet.TabIndex = 17;
            this.lblZoomSet.Text = "级别设置";
            // 
            // lblCenterSet
            // 
            this.lblCenterSet.AutoSize = true;
            this.lblCenterSet.ForeColor = System.Drawing.Color.Gray;
            this.lblCenterSet.Location = new System.Drawing.Point(23, 189);
            this.lblCenterSet.Name = "lblCenterSet";
            this.lblCenterSet.Size = new System.Drawing.Size(53, 12);
            this.lblCenterSet.TabIndex = 16;
            this.lblCenterSet.Text = "坐标设置";
            // 
            // lblOutputFormat
            // 
            this.lblOutputFormat.AutoSize = true;
            this.lblOutputFormat.ForeColor = System.Drawing.Color.Gray;
            this.lblOutputFormat.Location = new System.Drawing.Point(23, 150);
            this.lblOutputFormat.Name = "lblOutputFormat";
            this.lblOutputFormat.Size = new System.Drawing.Size(53, 12);
            this.lblOutputFormat.TabIndex = 15;
            this.lblOutputFormat.Text = "输出类型";
            // 
            // lblPathSet
            // 
            this.lblPathSet.AutoSize = true;
            this.lblPathSet.ForeColor = System.Drawing.Color.Gray;
            this.lblPathSet.Location = new System.Drawing.Point(23, 111);
            this.lblPathSet.Name = "lblPathSet";
            this.lblPathSet.Size = new System.Drawing.Size(53, 12);
            this.lblPathSet.TabIndex = 14;
            this.lblPathSet.Text = "路径设置";
            // 
            // lblCurProcessName
            // 
            this.lblCurProcessName.AutoSize = true;
            this.lblCurProcessName.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurProcessName.Location = new System.Drawing.Point(6, 17);
            this.lblCurProcessName.Name = "lblCurProcessName";
            this.lblCurProcessName.Size = new System.Drawing.Size(72, 16);
            this.lblCurProcessName.TabIndex = 20;
            this.lblCurProcessName.Text = "选择图片";
            // 
            // grpSelImage
            // 
            this.grpSelImage.BackColor = System.Drawing.Color.White;
            this.grpSelImage.Controls.Add(this.lblPreview);
            this.grpSelImage.Controls.Add(this.picPreview);
            this.grpSelImage.Controls.Add(this.btnSelImage);
            this.grpSelImage.Controls.Add(this.txtFilePath);
            this.grpSelImage.Controls.Add(this.label2);
            this.grpSelImage.Controls.Add(this.lblCurProcessName);
            this.grpSelImage.Location = new System.Drawing.Point(99, 12);
            this.grpSelImage.Name = "grpSelImage";
            this.grpSelImage.Size = new System.Drawing.Size(473, 386);
            this.grpSelImage.TabIndex = 21;
            this.grpSelImage.TabStop = false;
            this.grpSelImage.Visible = false;
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Location = new System.Drawing.Point(12, 133);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(29, 12);
            this.lblPreview.TabIndex = 25;
            this.lblPreview.Text = "预览";
            this.lblPreview.Visible = false;
            // 
            // picPreview
            // 
            this.picPreview.Location = new System.Drawing.Point(11, 151);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(447, 221);
            this.picPreview.TabIndex = 24;
            this.picPreview.TabStop = false;
            // 
            // btnSelImage
            // 
            this.btnSelImage.Location = new System.Drawing.Point(392, 98);
            this.btnSelImage.Name = "btnSelImage";
            this.btnSelImage.Size = new System.Drawing.Size(75, 23);
            this.btnSelImage.TabIndex = 23;
            this.btnSelImage.Text = "浏览...";
            this.btnSelImage.UseVisualStyleBackColor = true;
            this.btnSelImage.Click += new System.EventHandler(this.btnSelImage_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(11, 99);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(375, 21);
            this.txtFilePath.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(149, 12);
            this.label2.TabIndex = 21;
            this.label2.Text = "选择您需要切图的图片来源";
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(497, 427);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 22;
            this.btnNext.Text = "下一步";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(416, 427);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(75, 23);
            this.btnPrev.TabIndex = 23;
            this.btnPrev.Text = "上一步";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // grpCenterSet
            // 
            this.grpCenterSet.BackColor = System.Drawing.Color.White;
            this.grpCenterSet.Controls.Add(this.label16);
            this.grpCenterSet.Controls.Add(this.browser);
            this.grpCenterSet.Controls.Add(this.lblLngLatError);
            this.grpCenterSet.Controls.Add(this.txtLat);
            this.grpCenterSet.Controls.Add(this.label8);
            this.grpCenterSet.Controls.Add(this.txtLng);
            this.grpCenterSet.Controls.Add(this.label7);
            this.grpCenterSet.Controls.Add(this.label5);
            this.grpCenterSet.Controls.Add(this.label6);
            this.grpCenterSet.Location = new System.Drawing.Point(99, 12);
            this.grpCenterSet.Name = "grpCenterSet";
            this.grpCenterSet.Size = new System.Drawing.Size(473, 386);
            this.grpCenterSet.TabIndex = 26;
            this.grpCenterSet.TabStop = false;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(293, 108);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(161, 12);
            this.label16.TabIndex = 28;
            this.label16.Text = "在地图中通过右键可选择坐标";
            // 
            // browser
            // 
            this.browser.Location = new System.Drawing.Point(16, 134);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.ScrollBarsEnabled = false;
            this.browser.Size = new System.Drawing.Size(442, 238);
            this.browser.TabIndex = 27;
            // 
            // lblLngLatError
            // 
            this.lblLngLatError.AutoSize = true;
            this.lblLngLatError.ForeColor = System.Drawing.Color.Red;
            this.lblLngLatError.Location = new System.Drawing.Point(293, 108);
            this.lblLngLatError.Name = "lblLngLatError";
            this.lblLngLatError.Size = new System.Drawing.Size(0, 12);
            this.lblLngLatError.TabIndex = 26;
            // 
            // txtLat
            // 
            this.txtLat.Location = new System.Drawing.Point(187, 105);
            this.txtLat.Name = "txtLat";
            this.txtLat.Size = new System.Drawing.Size(100, 21);
            this.txtLat.TabIndex = 25;
            this.txtLat.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(152, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 24;
            this.label8.Text = "纬度";
            // 
            // txtLng
            // 
            this.txtLng.Location = new System.Drawing.Point(46, 105);
            this.txtLng.Name = "txtLng";
            this.txtLng.Size = new System.Drawing.Size(100, 21);
            this.txtLng.TabIndex = 23;
            this.txtLng.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 109);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 22;
            this.label7.Text = "经度";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(425, 12);
            this.label5.TabIndex = 21;
            this.label5.Text = "设置图片中心位置坐标（经度取值范围：-180到180，纬度取值范围：-90到90）";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(6, 17);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 16);
            this.label6.TabIndex = 20;
            this.label6.Text = "坐标设置";
            // 
            // grpZoomSet
            // 
            this.grpZoomSet.BackColor = System.Drawing.Color.White;
            this.grpZoomSet.Controls.Add(this.rdCustomZoom);
            this.grpZoomSet.Controls.Add(this.cbxSourceZoom);
            this.grpZoomSet.Controls.Add(this.cbxMaxZoom);
            this.grpZoomSet.Controls.Add(this.cbxMinZoom);
            this.grpZoomSet.Controls.Add(this.lblSourceZoom);
            this.grpZoomSet.Controls.Add(this.lblMaxZoom);
            this.grpZoomSet.Controls.Add(this.lblMinZoom);
            this.grpZoomSet.Controls.Add(this.rdAutoZoom);
            this.grpZoomSet.Controls.Add(this.label11);
            this.grpZoomSet.Controls.Add(this.label12);
            this.grpZoomSet.Location = new System.Drawing.Point(99, 12);
            this.grpZoomSet.Name = "grpZoomSet";
            this.grpZoomSet.Size = new System.Drawing.Size(473, 386);
            this.grpZoomSet.TabIndex = 27;
            this.grpZoomSet.TabStop = false;
            // 
            // rdCustomZoom
            // 
            this.rdCustomZoom.AutoSize = true;
            this.rdCustomZoom.Location = new System.Drawing.Point(11, 124);
            this.rdCustomZoom.Name = "rdCustomZoom";
            this.rdCustomZoom.Size = new System.Drawing.Size(59, 16);
            this.rdCustomZoom.TabIndex = 33;
            this.rdCustomZoom.Text = "自定义";
            this.rdCustomZoom.UseVisualStyleBackColor = true;
            // 
            // cbxSourceZoom
            // 
            this.cbxSourceZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxSourceZoom.Enabled = false;
            this.cbxSourceZoom.FormattingEnabled = true;
            this.cbxSourceZoom.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18"});
            this.cbxSourceZoom.Location = new System.Drawing.Point(187, 179);
            this.cbxSourceZoom.Name = "cbxSourceZoom";
            this.cbxSourceZoom.Size = new System.Drawing.Size(90, 20);
            this.cbxSourceZoom.TabIndex = 32;
            // 
            // cbxMaxZoom
            // 
            this.cbxMaxZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMaxZoom.Enabled = false;
            this.cbxMaxZoom.FormattingEnabled = true;
            this.cbxMaxZoom.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18"});
            this.cbxMaxZoom.Location = new System.Drawing.Point(187, 151);
            this.cbxMaxZoom.Name = "cbxMaxZoom";
            this.cbxMaxZoom.Size = new System.Drawing.Size(90, 20);
            this.cbxMaxZoom.TabIndex = 31;
            // 
            // cbxMinZoom
            // 
            this.cbxMinZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMinZoom.Enabled = false;
            this.cbxMinZoom.FormattingEnabled = true;
            this.cbxMinZoom.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18"});
            this.cbxMinZoom.Location = new System.Drawing.Point(187, 124);
            this.cbxMinZoom.Name = "cbxMinZoom";
            this.cbxMinZoom.Size = new System.Drawing.Size(90, 20);
            this.cbxMinZoom.TabIndex = 30;
            // 
            // lblSourceZoom
            // 
            this.lblSourceZoom.AutoSize = true;
            this.lblSourceZoom.Enabled = false;
            this.lblSourceZoom.Location = new System.Drawing.Point(104, 183);
            this.lblSourceZoom.Name = "lblSourceZoom";
            this.lblSourceZoom.Size = new System.Drawing.Size(77, 12);
            this.lblSourceZoom.TabIndex = 29;
            this.lblSourceZoom.Text = "原图所在级别";
            // 
            // lblMaxZoom
            // 
            this.lblMaxZoom.AutoSize = true;
            this.lblMaxZoom.Enabled = false;
            this.lblMaxZoom.Location = new System.Drawing.Point(105, 155);
            this.lblMaxZoom.Name = "lblMaxZoom";
            this.lblMaxZoom.Size = new System.Drawing.Size(53, 12);
            this.lblMaxZoom.TabIndex = 28;
            this.lblMaxZoom.Text = "最大级别";
            // 
            // lblMinZoom
            // 
            this.lblMinZoom.AutoSize = true;
            this.lblMinZoom.Enabled = false;
            this.lblMinZoom.Location = new System.Drawing.Point(105, 128);
            this.lblMinZoom.Name = "lblMinZoom";
            this.lblMinZoom.Size = new System.Drawing.Size(53, 12);
            this.lblMinZoom.TabIndex = 27;
            this.lblMinZoom.Text = "最小级别";
            // 
            // rdAutoZoom
            // 
            this.rdAutoZoom.AutoSize = true;
            this.rdAutoZoom.Checked = true;
            this.rdAutoZoom.Location = new System.Drawing.Point(11, 105);
            this.rdAutoZoom.Name = "rdAutoZoom";
            this.rdAutoZoom.Size = new System.Drawing.Size(71, 16);
            this.rdAutoZoom.TabIndex = 25;
            this.rdAutoZoom.TabStop = true;
            this.rdAutoZoom.Text = "自动控制";
            this.rdAutoZoom.UseVisualStyleBackColor = true;
            this.rdAutoZoom.CheckedChanged += new System.EventHandler(this.rdAutoZoom_CheckedChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 60);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 12);
            this.label11.TabIndex = 21;
            this.label11.Text = "设置级别范围";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(6, 17);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(72, 16);
            this.label12.TabIndex = 20;
            this.label12.Text = "级别设置";
            // 
            // grpLayerSet
            // 
            this.grpLayerSet.BackColor = System.Drawing.Color.White;
            this.grpLayerSet.Controls.Add(this.txtMapTypeName);
            this.grpLayerSet.Controls.Add(this.rdMapType);
            this.grpLayerSet.Controls.Add(this.label17);
            this.grpLayerSet.Controls.Add(this.label18);
            this.grpLayerSet.Controls.Add(this.rdTileLayer);
            this.grpLayerSet.Location = new System.Drawing.Point(99, 12);
            this.grpLayerSet.Name = "grpLayerSet";
            this.grpLayerSet.Size = new System.Drawing.Size(473, 386);
            this.grpLayerSet.TabIndex = 33;
            this.grpLayerSet.TabStop = false;
            // 
            // txtMapTypeName
            // 
            this.txtMapTypeName.Location = new System.Drawing.Point(192, 103);
            this.txtMapTypeName.Name = "txtMapTypeName";
            this.txtMapTypeName.Size = new System.Drawing.Size(172, 21);
            this.txtMapTypeName.TabIndex = 33;
            this.txtMapTypeName.Text = "MyMap";
            // 
            // rdMapType
            // 
            this.rdMapType.AutoSize = true;
            this.rdMapType.Checked = true;
            this.rdMapType.Location = new System.Drawing.Point(11, 105);
            this.rdMapType.Name = "rdMapType";
            this.rdMapType.Size = new System.Drawing.Size(179, 16);
            this.rdMapType.TabIndex = 25;
            this.rdMapType.TabStop = true;
            this.rdMapType.Text = "做为地图类型，地图类型名称";
            this.rdMapType.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(9, 60);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(77, 12);
            this.label17.TabIndex = 21;
            this.label17.Text = "设置图层信息";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label18.Location = new System.Drawing.Point(6, 17);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(72, 16);
            this.label18.TabIndex = 20;
            this.label18.Text = "图层设置";
            // 
            // rdTileLayer
            // 
            this.rdTileLayer.AutoSize = true;
            this.rdTileLayer.Location = new System.Drawing.Point(11, 127);
            this.rdTileLayer.Name = "rdTileLayer";
            this.rdTileLayer.Size = new System.Drawing.Size(95, 16);
            this.rdTileLayer.TabIndex = 26;
            this.rdTileLayer.Text = "做为独立图层";
            this.rdTileLayer.UseVisualStyleBackColor = true;
            // 
            // grpBeginCut
            // 
            this.grpBeginCut.BackColor = System.Drawing.Color.White;
            this.grpBeginCut.Controls.Add(this.lblCutProcess);
            this.grpBeginCut.Controls.Add(this.txtFinalOutputPath);
            this.grpBeginCut.Controls.Add(this.txtFinalImagePath);
            this.grpBeginCut.Controls.Add(this.lblFinalLayer);
            this.grpBeginCut.Controls.Add(this.lblFinalZoom);
            this.grpBeginCut.Controls.Add(this.lblFinalCenter);
            this.grpBeginCut.Controls.Add(this.lblFinalOutputFormat);
            this.grpBeginCut.Controls.Add(this.proBar);
            this.grpBeginCut.Controls.Add(this.label22);
            this.grpBeginCut.Controls.Add(this.label21);
            this.grpBeginCut.Controls.Add(this.label13);
            this.grpBeginCut.Controls.Add(this.label10);
            this.grpBeginCut.Controls.Add(this.label9);
            this.grpBeginCut.Controls.Add(this.label14);
            this.grpBeginCut.Controls.Add(this.label15);
            this.grpBeginCut.Controls.Add(this.label19);
            this.grpBeginCut.Location = new System.Drawing.Point(99, 12);
            this.grpBeginCut.Name = "grpBeginCut";
            this.grpBeginCut.Size = new System.Drawing.Size(473, 386);
            this.grpBeginCut.TabIndex = 35;
            this.grpBeginCut.TabStop = false;
            // 
            // lblCutProcess
            // 
            this.lblCutProcess.AutoSize = true;
            this.lblCutProcess.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCutProcess.Location = new System.Drawing.Point(12, 317);
            this.lblCutProcess.Name = "lblCutProcess";
            this.lblCutProcess.Size = new System.Drawing.Size(98, 21);
            this.lblCutProcess.TabIndex = 49;
            this.lblCutProcess.Text = "准备就绪";
            // 
            // txtFinalOutputPath
            // 
            this.txtFinalOutputPath.Location = new System.Drawing.Point(84, 130);
            this.txtFinalOutputPath.Name = "txtFinalOutputPath";
            this.txtFinalOutputPath.ReadOnly = true;
            this.txtFinalOutputPath.Size = new System.Drawing.Size(374, 21);
            this.txtFinalOutputPath.TabIndex = 48;
            // 
            // txtFinalImagePath
            // 
            this.txtFinalImagePath.Location = new System.Drawing.Point(84, 99);
            this.txtFinalImagePath.Name = "txtFinalImagePath";
            this.txtFinalImagePath.ReadOnly = true;
            this.txtFinalImagePath.Size = new System.Drawing.Size(374, 21);
            this.txtFinalImagePath.TabIndex = 47;
            // 
            // lblFinalLayer
            // 
            this.lblFinalLayer.AutoSize = true;
            this.lblFinalLayer.Location = new System.Drawing.Point(82, 258);
            this.lblFinalLayer.Name = "lblFinalLayer";
            this.lblFinalLayer.Size = new System.Drawing.Size(0, 12);
            this.lblFinalLayer.TabIndex = 45;
            // 
            // lblFinalZoom
            // 
            this.lblFinalZoom.AutoSize = true;
            this.lblFinalZoom.Location = new System.Drawing.Point(82, 227);
            this.lblFinalZoom.Name = "lblFinalZoom";
            this.lblFinalZoom.Size = new System.Drawing.Size(0, 12);
            this.lblFinalZoom.TabIndex = 44;
            // 
            // lblFinalCenter
            // 
            this.lblFinalCenter.AutoSize = true;
            this.lblFinalCenter.Location = new System.Drawing.Point(82, 196);
            this.lblFinalCenter.Name = "lblFinalCenter";
            this.lblFinalCenter.Size = new System.Drawing.Size(0, 12);
            this.lblFinalCenter.TabIndex = 43;
            // 
            // lblFinalOutputFormat
            // 
            this.lblFinalOutputFormat.AutoSize = true;
            this.lblFinalOutputFormat.Location = new System.Drawing.Point(82, 165);
            this.lblFinalOutputFormat.Name = "lblFinalOutputFormat";
            this.lblFinalOutputFormat.Size = new System.Drawing.Size(0, 12);
            this.lblFinalOutputFormat.TabIndex = 42;
            // 
            // proBar
            // 
            this.proBar.Location = new System.Drawing.Point(14, 349);
            this.proBar.Name = "proBar";
            this.proBar.Size = new System.Drawing.Size(444, 23);
            this.proBar.TabIndex = 41;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(11, 134);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(65, 12);
            this.label22.TabIndex = 40;
            this.label22.Text = "输出路径：";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(11, 103);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(65, 12);
            this.label21.TabIndex = 39;
            this.label21.Text = "图片路径：";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(11, 258);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(65, 12);
            this.label13.TabIndex = 37;
            this.label13.Text = "图层设置：";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 196);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 12);
            this.label10.TabIndex = 36;
            this.label10.Text = "中心坐标：";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 165);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 12);
            this.label9.TabIndex = 35;
            this.label9.Text = "输出类型：";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(11, 227);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 12);
            this.label14.TabIndex = 27;
            this.label14.Text = "级别设置：";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(9, 60);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(341, 12);
            this.label15.TabIndex = 21;
            this.label15.Text = "点击“开始切图”进行切图，点击“上一步”返回修改配置信息";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label19.Location = new System.Drawing.Point(6, 17);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(72, 16);
            this.label19.TabIndex = 20;
            this.label19.Text = "开始切图";
            // 
            // lblBeginCut
            // 
            this.lblBeginCut.AutoSize = true;
            this.lblBeginCut.ForeColor = System.Drawing.Color.Gray;
            this.lblBeginCut.Location = new System.Drawing.Point(23, 304);
            this.lblBeginCut.Name = "lblBeginCut";
            this.lblBeginCut.Size = new System.Drawing.Size(53, 12);
            this.lblBeginCut.TabIndex = 36;
            this.lblBeginCut.Text = "开始切图";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "支持的图片文件|*.bmp;*.jpg;*.jpeg;*.png;*.gif|所有文件|*.*";
            // 
            // grpPathSet
            // 
            this.grpPathSet.BackColor = System.Drawing.Color.White;
            this.grpPathSet.Controls.Add(this.btnSetPath);
            this.grpPathSet.Controls.Add(this.txtOutputPath);
            this.grpPathSet.Controls.Add(this.label1);
            this.grpPathSet.Controls.Add(this.lblTitlePathSet);
            this.grpPathSet.Location = new System.Drawing.Point(99, 12);
            this.grpPathSet.Name = "grpPathSet";
            this.grpPathSet.Size = new System.Drawing.Size(473, 386);
            this.grpPathSet.TabIndex = 38;
            this.grpPathSet.TabStop = false;
            // 
            // btnSetPath
            // 
            this.btnSetPath.Location = new System.Drawing.Point(392, 98);
            this.btnSetPath.Name = "btnSetPath";
            this.btnSetPath.Size = new System.Drawing.Size(75, 23);
            this.btnSetPath.TabIndex = 23;
            this.btnSetPath.Text = "选择...";
            this.btnSetPath.UseVisualStyleBackColor = true;
            this.btnSetPath.Click += new System.EventHandler(this.btnSetPath_Click);
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Location = new System.Drawing.Point(11, 99);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.ReadOnly = true;
            this.txtOutputPath.Size = new System.Drawing.Size(375, 21);
            this.txtOutputPath.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 21;
            this.label1.Text = "设置输出的目录";
            // 
            // lblTitlePathSet
            // 
            this.lblTitlePathSet.AutoSize = true;
            this.lblTitlePathSet.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTitlePathSet.Location = new System.Drawing.Point(6, 17);
            this.lblTitlePathSet.Name = "lblTitlePathSet";
            this.lblTitlePathSet.Size = new System.Drawing.Size(72, 16);
            this.lblTitlePathSet.TabIndex = 20;
            this.lblTitlePathSet.Text = "输出路径";
            // 
            // grpOutputFormat
            // 
            this.grpOutputFormat.BackColor = System.Drawing.Color.White;
            this.grpOutputFormat.Controls.Add(this.rdTilesOnly);
            this.grpOutputFormat.Controls.Add(this.rdTilesAndCode);
            this.grpOutputFormat.Controls.Add(this.label3);
            this.grpOutputFormat.Controls.Add(this.label4);
            this.grpOutputFormat.Location = new System.Drawing.Point(99, 12);
            this.grpOutputFormat.Name = "grpOutputFormat";
            this.grpOutputFormat.Size = new System.Drawing.Size(473, 387);
            this.grpOutputFormat.TabIndex = 39;
            this.grpOutputFormat.TabStop = false;
            // 
            // rdTilesOnly
            // 
            this.rdTilesOnly.AutoSize = true;
            this.rdTilesOnly.Location = new System.Drawing.Point(11, 128);
            this.rdTilesOnly.Name = "rdTilesOnly";
            this.rdTilesOnly.Size = new System.Drawing.Size(59, 16);
            this.rdTilesOnly.TabIndex = 23;
            this.rdTilesOnly.TabStop = true;
            this.rdTilesOnly.Text = "仅图块";
            this.rdTilesOnly.UseVisualStyleBackColor = true;
            this.rdTilesOnly.CheckedChanged += new System.EventHandler(this.rdTilesOnly_CheckedChanged);
            // 
            // rdTilesAndCode
            // 
            this.rdTilesAndCode.AutoSize = true;
            this.rdTilesAndCode.Checked = true;
            this.rdTilesAndCode.Location = new System.Drawing.Point(11, 105);
            this.rdTilesAndCode.Name = "rdTilesAndCode";
            this.rdTilesAndCode.Size = new System.Drawing.Size(83, 16);
            this.rdTilesAndCode.TabIndex = 22;
            this.rdTilesAndCode.TabStop = true;
            this.rdTilesAndCode.Text = "图块及代码";
            this.rdTilesAndCode.UseVisualStyleBackColor = true;
            this.rdTilesAndCode.CheckedChanged += new System.EventHandler(this.rdTilesAndCode_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 21;
            this.label3.Text = "设置输出类型";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(6, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 16);
            this.label4.TabIndex = 20;
            this.label4.Text = "输出类型";
            // 
            // picStepBg
            // 
            this.picStepBg.Location = new System.Drawing.Point(0, 63);
            this.picStepBg.Name = "picStepBg";
            this.picStepBg.Size = new System.Drawing.Size(100, 30);
            this.picStepBg.TabIndex = 40;
            this.picStepBg.TabStop = false;
            // 
            // cutTimer
            // 
            this.cutTimer.Interval = 200;
            this.cutTimer.Tick += new System.EventHandler(this.cutTimer_Tick);
            // 
            // TileCutter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(769, 562);
            this.Controls.Add(this.grpCenterSet);
            this.Controls.Add(this.grpBeginCut);
            this.Controls.Add(this.grpSelImage);
            this.Controls.Add(this.grpLayerSet);
            this.Controls.Add(this.grpOutputFormat);
            this.Controls.Add(this.grpZoomSet);
            this.Controls.Add(this.grpPathSet);
            this.Controls.Add(this.lblBeginCut);
            this.Controls.Add(this.btnPrev);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.lblSelImage);
            this.Controls.Add(this.lblLayerSet);
            this.Controls.Add(this.lblZoomSet);
            this.Controls.Add(this.lblCenterSet);
            this.Controls.Add(this.lblOutputFormat);
            this.Controls.Add(this.lblPathSet);
            this.Controls.Add(this.picStepBg);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "TileCutter";
            this.Text = "TileCutter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TileCutter_FormClosing);
            this.Load += new System.EventHandler(this.TileCutter_Load);
            this.grpSelImage.ResumeLayout(false);
            this.grpSelImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.grpCenterSet.ResumeLayout(false);
            this.grpCenterSet.PerformLayout();
            this.grpZoomSet.ResumeLayout(false);
            this.grpZoomSet.PerformLayout();
            this.grpLayerSet.ResumeLayout(false);
            this.grpLayerSet.PerformLayout();
            this.grpBeginCut.ResumeLayout(false);
            this.grpBeginCut.PerformLayout();
            this.grpPathSet.ResumeLayout(false);
            this.grpPathSet.PerformLayout();
            this.grpOutputFormat.ResumeLayout(false);
            this.grpOutputFormat.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStepBg)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}
}
