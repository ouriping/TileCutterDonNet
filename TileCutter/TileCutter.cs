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
			this.components = new Container();
			this.lblSelImage = new Label();
			this.lblLayerSet = new Label();
			this.lblZoomSet = new Label();
			this.lblCenterSet = new Label();
			this.lblOutputFormat = new Label();
			this.lblPathSet = new Label();
			this.lblCurProcessName = new Label();
			this.grpSelImage = new GroupBox();
			this.lblPreview = new Label();
			this.picPreview = new PictureBox();
			this.btnSelImage = new Button();
			this.txtFilePath = new TextBox();
			this.label2 = new Label();
			this.btnNext = new Button();
			this.btnPrev = new Button();
			this.grpCenterSet = new GroupBox();
			this.browser = new WebBrowser();
			this.lblLngLatError = new Label();
			this.txtLat = new TextBox();
			this.label8 = new Label();
			this.txtLng = new TextBox();
			this.label7 = new Label();
			this.label5 = new Label();
			this.label6 = new Label();
			this.grpZoomSet = new GroupBox();
			this.rdCustomZoom = new RadioButton();
			this.cbxSourceZoom = new ComboBox();
			this.cbxMaxZoom = new ComboBox();
			this.cbxMinZoom = new ComboBox();
			this.lblSourceZoom = new Label();
			this.lblMaxZoom = new Label();
			this.lblMinZoom = new Label();
			this.rdAutoZoom = new RadioButton();
			this.label11 = new Label();
			this.label12 = new Label();
			this.grpLayerSet = new GroupBox();
			this.txtMapTypeName = new TextBox();
			this.rdMapType = new RadioButton();
			this.label17 = new Label();
			this.label18 = new Label();
			this.rdTileLayer = new RadioButton();
			this.grpBeginCut = new GroupBox();
			this.lblCutProcess = new Label();
			this.txtFinalOutputPath = new TextBox();
			this.txtFinalImagePath = new TextBox();
			this.lblFinalLayer = new Label();
			this.lblFinalZoom = new Label();
			this.lblFinalCenter = new Label();
			this.lblFinalOutputFormat = new Label();
			this.proBar = new ProgressBar();
			this.label22 = new Label();
			this.label21 = new Label();
			this.label13 = new Label();
			this.label10 = new Label();
			this.label9 = new Label();
			this.label14 = new Label();
			this.label15 = new Label();
			this.label19 = new Label();
			this.lblBeginCut = new Label();
			this.openFileDialog = new OpenFileDialog();
			this.folderBrowserDialog = new FolderBrowserDialog();
			this.grpPathSet = new GroupBox();
			this.btnSetPath = new Button();
			this.txtOutputPath = new TextBox();
			this.label1 = new Label();
			this.lblTitlePathSet = new Label();
			this.grpOutputFormat = new GroupBox();
			this.rdTilesOnly = new RadioButton();
			this.rdTilesAndCode = new RadioButton();
			this.label3 = new Label();
			this.label4 = new Label();
			this.picStepBg = new PictureBox();
			this.cutTimer = new System.Windows.Forms.Timer(this.components);
			this.label16 = new Label();
			this.grpSelImage.SuspendLayout();
			((ISupportInitialize)this.picPreview).BeginInit();
			this.grpCenterSet.SuspendLayout();
			this.grpZoomSet.SuspendLayout();
			this.grpLayerSet.SuspendLayout();
			this.grpBeginCut.SuspendLayout();
			this.grpPathSet.SuspendLayout();
			this.grpOutputFormat.SuspendLayout();
			((ISupportInitialize)this.picStepBg).BeginInit();
			base.SuspendLayout();
			this.lblSelImage.AutoSize = true;
			this.lblSelImage.ForeColor = Color.Gray;
			this.lblSelImage.Location = new System.Drawing.Point(23, 72);
			this.lblSelImage.Name = "lblSelImage";
			this.lblSelImage.Size = new Size(53, 12);
			this.lblSelImage.TabIndex = 19;
			this.lblSelImage.Text = "选择图片";
			this.lblLayerSet.AutoSize = true;
			this.lblLayerSet.ForeColor = Color.Gray;
			this.lblLayerSet.Location = new System.Drawing.Point(23, 267);
			this.lblLayerSet.Name = "lblLayerSet";
			this.lblLayerSet.Size = new Size(53, 12);
			this.lblLayerSet.TabIndex = 18;
			this.lblLayerSet.Text = "图层设置";
			this.lblZoomSet.AutoSize = true;
			this.lblZoomSet.ForeColor = Color.Gray;
			this.lblZoomSet.Location = new System.Drawing.Point(23, 228);
			this.lblZoomSet.Name = "lblZoomSet";
			this.lblZoomSet.Size = new Size(53, 12);
			this.lblZoomSet.TabIndex = 17;
			this.lblZoomSet.Text = "级别设置";
			this.lblCenterSet.AutoSize = true;
			this.lblCenterSet.ForeColor = Color.Gray;
			this.lblCenterSet.Location = new System.Drawing.Point(23, 189);
			this.lblCenterSet.Name = "lblCenterSet";
			this.lblCenterSet.Size = new Size(53, 12);
			this.lblCenterSet.TabIndex = 16;
			this.lblCenterSet.Text = "坐标设置";
			this.lblOutputFormat.AutoSize = true;
			this.lblOutputFormat.ForeColor = Color.Gray;
			this.lblOutputFormat.Location = new System.Drawing.Point(23, 150);
			this.lblOutputFormat.Name = "lblOutputFormat";
			this.lblOutputFormat.Size = new Size(53, 12);
			this.lblOutputFormat.TabIndex = 15;
			this.lblOutputFormat.Text = "输出类型";
			this.lblPathSet.AutoSize = true;
			this.lblPathSet.ForeColor = Color.Gray;
			this.lblPathSet.Location = new System.Drawing.Point(23, 111);
			this.lblPathSet.Name = "lblPathSet";
			this.lblPathSet.Size = new Size(53, 12);
			this.lblPathSet.TabIndex = 14;
			this.lblPathSet.Text = "路径设置";
			this.lblCurProcessName.AutoSize = true;
			this.lblCurProcessName.Font = new Font("宋体", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.lblCurProcessName.Location = new System.Drawing.Point(6, 17);
			this.lblCurProcessName.Name = "lblCurProcessName";
			this.lblCurProcessName.Size = new Size(72, 16);
			this.lblCurProcessName.TabIndex = 20;
			this.lblCurProcessName.Text = "选择图片";
			this.grpSelImage.BackColor = Color.White;
			this.grpSelImage.Controls.Add(this.lblPreview);
			this.grpSelImage.Controls.Add(this.picPreview);
			this.grpSelImage.Controls.Add(this.btnSelImage);
			this.grpSelImage.Controls.Add(this.txtFilePath);
			this.grpSelImage.Controls.Add(this.label2);
			this.grpSelImage.Controls.Add(this.lblCurProcessName);
			this.grpSelImage.Location = new System.Drawing.Point(99, 12);
			this.grpSelImage.Name = "grpSelImage";
			this.grpSelImage.Size = new Size(473, 386);
			this.grpSelImage.TabIndex = 21;
			this.grpSelImage.TabStop = false;
			this.grpSelImage.Visible = false;
			this.lblPreview.AutoSize = true;
			this.lblPreview.Location = new System.Drawing.Point(12, 133);
			this.lblPreview.Name = "lblPreview";
			this.lblPreview.Size = new Size(29, 12);
			this.lblPreview.TabIndex = 25;
			this.lblPreview.Text = "预览";
			this.lblPreview.Visible = false;
			this.picPreview.Location = new System.Drawing.Point(11, 151);
			this.picPreview.Name = "picPreview";
			this.picPreview.Size = new Size(447, 221);
			this.picPreview.TabIndex = 24;
			this.picPreview.TabStop = false;
			this.btnSelImage.Location = new System.Drawing.Point(392, 98);
			this.btnSelImage.Name = "btnSelImage";
			this.btnSelImage.Size = new Size(75, 23);
			this.btnSelImage.TabIndex = 23;
			this.btnSelImage.Text = "浏览...";
			this.btnSelImage.UseVisualStyleBackColor = true;
			this.btnSelImage.Click += new EventHandler(this.btnSelImage_Click);
			this.txtFilePath.Location = new System.Drawing.Point(11, 99);
			this.txtFilePath.Name = "txtFilePath";
			this.txtFilePath.ReadOnly = true;
			this.txtFilePath.Size = new Size(375, 21);
			this.txtFilePath.TabIndex = 22;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 60);
			this.label2.Name = "label2";
			this.label2.Size = new Size(149, 12);
			this.label2.TabIndex = 21;
			this.label2.Text = "选择您需要切图的图片来源";
			this.btnNext.Location = new System.Drawing.Point(497, 427);
			this.btnNext.Name = "btnNext";
			this.btnNext.Size = new Size(75, 23);
			this.btnNext.TabIndex = 22;
			this.btnNext.Text = "下一步";
			this.btnNext.UseVisualStyleBackColor = true;
			this.btnNext.Click += new EventHandler(this.btnNext_Click);
			this.btnPrev.Location = new System.Drawing.Point(416, 427);
			this.btnPrev.Name = "btnPrev";
			this.btnPrev.Size = new Size(75, 23);
			this.btnPrev.TabIndex = 23;
			this.btnPrev.Text = "上一步";
			this.btnPrev.UseVisualStyleBackColor = true;
			this.btnPrev.Click += new EventHandler(this.btnPrev_Click);
			this.grpCenterSet.BackColor = Color.White;
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
			this.grpCenterSet.Size = new Size(473, 386);
			this.grpCenterSet.TabIndex = 26;
			this.grpCenterSet.TabStop = false;
			this.browser.Location = new System.Drawing.Point(16, 134);
			this.browser.MinimumSize = new Size(20, 20);
			this.browser.Name = "browser";
			this.browser.ScrollBarsEnabled = false;
			this.browser.Size = new Size(442, 238);
			this.browser.TabIndex = 27;
			this.lblLngLatError.AutoSize = true;
			this.lblLngLatError.ForeColor = Color.Red;
			this.lblLngLatError.Location = new System.Drawing.Point(293, 108);
			this.lblLngLatError.Name = "lblLngLatError";
			this.lblLngLatError.Size = new Size(0, 12);
			this.lblLngLatError.TabIndex = 26;
			this.txtLat.Location = new System.Drawing.Point(187, 105);
			this.txtLat.Name = "txtLat";
			this.txtLat.Size = new Size(100, 21);
			this.txtLat.TabIndex = 25;
			this.txtLat.Text = "0";
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(152, 109);
			this.label8.Name = "label8";
			this.label8.Size = new Size(29, 12);
			this.label8.TabIndex = 24;
			this.label8.Text = "纬度";
			this.txtLng.Location = new System.Drawing.Point(46, 105);
			this.txtLng.Name = "txtLng";
			this.txtLng.Size = new Size(100, 21);
			this.txtLng.TabIndex = 23;
			this.txtLng.Text = "0";
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(11, 109);
			this.label7.Name = "label7";
			this.label7.Size = new Size(29, 12);
			this.label7.TabIndex = 22;
			this.label7.Text = "经度";
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 60);
			this.label5.Name = "label5";
			this.label5.Size = new Size(425, 12);
			this.label5.TabIndex = 21;
			this.label5.Text = "设置图片中心位置坐标（经度取值范围：-180到180，纬度取值范围：-90到90）";
			this.label6.AutoSize = true;
			this.label6.Font = new Font("宋体", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.label6.Location = new System.Drawing.Point(6, 17);
			this.label6.Name = "label6";
			this.label6.Size = new Size(72, 16);
			this.label6.TabIndex = 20;
			this.label6.Text = "坐标设置";
			this.grpZoomSet.BackColor = Color.White;
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
			this.grpZoomSet.Size = new Size(473, 386);
			this.grpZoomSet.TabIndex = 27;
			this.grpZoomSet.TabStop = false;
			this.rdCustomZoom.AutoSize = true;
			this.rdCustomZoom.Location = new System.Drawing.Point(11, 124);
			this.rdCustomZoom.Name = "rdCustomZoom";
			this.rdCustomZoom.Size = new Size(59, 16);
			this.rdCustomZoom.TabIndex = 33;
			this.rdCustomZoom.Text = "自定义";
			this.rdCustomZoom.UseVisualStyleBackColor = true;
			this.cbxSourceZoom.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cbxSourceZoom.Enabled = false;
			this.cbxSourceZoom.FormattingEnabled = true;
			this.cbxSourceZoom.Items.AddRange(new object[]
			{
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
				"18"
			});
			this.cbxSourceZoom.Location = new System.Drawing.Point(187, 179);
			this.cbxSourceZoom.Name = "cbxSourceZoom";
			this.cbxSourceZoom.Size = new Size(90, 20);
			this.cbxSourceZoom.TabIndex = 32;
			this.cbxMaxZoom.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cbxMaxZoom.Enabled = false;
			this.cbxMaxZoom.FormattingEnabled = true;
			this.cbxMaxZoom.Items.AddRange(new object[]
			{
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
				"18"
			});
			this.cbxMaxZoom.Location = new System.Drawing.Point(187, 151);
			this.cbxMaxZoom.Name = "cbxMaxZoom";
			this.cbxMaxZoom.Size = new Size(90, 20);
			this.cbxMaxZoom.TabIndex = 31;
			this.cbxMinZoom.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cbxMinZoom.Enabled = false;
			this.cbxMinZoom.FormattingEnabled = true;
			this.cbxMinZoom.Items.AddRange(new object[]
			{
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
				"18"
			});
			this.cbxMinZoom.Location = new System.Drawing.Point(187, 124);
			this.cbxMinZoom.Name = "cbxMinZoom";
			this.cbxMinZoom.Size = new Size(90, 20);
			this.cbxMinZoom.TabIndex = 30;
			this.lblSourceZoom.AutoSize = true;
			this.lblSourceZoom.Enabled = false;
			this.lblSourceZoom.Location = new System.Drawing.Point(104, 183);
			this.lblSourceZoom.Name = "lblSourceZoom";
			this.lblSourceZoom.Size = new Size(77, 12);
			this.lblSourceZoom.TabIndex = 29;
			this.lblSourceZoom.Text = "原图所在级别";
			this.lblMaxZoom.AutoSize = true;
			this.lblMaxZoom.Enabled = false;
			this.lblMaxZoom.Location = new System.Drawing.Point(105, 155);
			this.lblMaxZoom.Name = "lblMaxZoom";
			this.lblMaxZoom.Size = new Size(53, 12);
			this.lblMaxZoom.TabIndex = 28;
			this.lblMaxZoom.Text = "最大级别";
			this.lblMinZoom.AutoSize = true;
			this.lblMinZoom.Enabled = false;
			this.lblMinZoom.Location = new System.Drawing.Point(105, 128);
			this.lblMinZoom.Name = "lblMinZoom";
			this.lblMinZoom.Size = new Size(53, 12);
			this.lblMinZoom.TabIndex = 27;
			this.lblMinZoom.Text = "最小级别";
			this.rdAutoZoom.AutoSize = true;
			this.rdAutoZoom.Checked = true;
			this.rdAutoZoom.Location = new System.Drawing.Point(11, 105);
			this.rdAutoZoom.Name = "rdAutoZoom";
			this.rdAutoZoom.Size = new Size(71, 16);
			this.rdAutoZoom.TabIndex = 25;
			this.rdAutoZoom.TabStop = true;
			this.rdAutoZoom.Text = "自动控制";
			this.rdAutoZoom.UseVisualStyleBackColor = true;
			this.rdAutoZoom.CheckedChanged += new EventHandler(this.rdAutoZoom_CheckedChanged);
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(9, 60);
			this.label11.Name = "label11";
			this.label11.Size = new Size(77, 12);
			this.label11.TabIndex = 21;
			this.label11.Text = "设置级别范围";
			this.label12.AutoSize = true;
			this.label12.Font = new Font("宋体", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.label12.Location = new System.Drawing.Point(6, 17);
			this.label12.Name = "label12";
			this.label12.Size = new Size(72, 16);
			this.label12.TabIndex = 20;
			this.label12.Text = "级别设置";
			this.grpLayerSet.BackColor = Color.White;
			this.grpLayerSet.Controls.Add(this.txtMapTypeName);
			this.grpLayerSet.Controls.Add(this.rdMapType);
			this.grpLayerSet.Controls.Add(this.label17);
			this.grpLayerSet.Controls.Add(this.label18);
			this.grpLayerSet.Controls.Add(this.rdTileLayer);
			this.grpLayerSet.Location = new System.Drawing.Point(99, 12);
			this.grpLayerSet.Name = "grpLayerSet";
			this.grpLayerSet.Size = new Size(473, 386);
			this.grpLayerSet.TabIndex = 33;
			this.grpLayerSet.TabStop = false;
			this.txtMapTypeName.Location = new System.Drawing.Point(192, 103);
			this.txtMapTypeName.Name = "txtMapTypeName";
			this.txtMapTypeName.Size = new Size(172, 21);
			this.txtMapTypeName.TabIndex = 33;
			this.txtMapTypeName.Text = "MyMap";
			this.rdMapType.AutoSize = true;
			this.rdMapType.Checked = true;
			this.rdMapType.Location = new System.Drawing.Point(11, 105);
			this.rdMapType.Name = "rdMapType";
			this.rdMapType.Size = new Size(179, 16);
			this.rdMapType.TabIndex = 25;
			this.rdMapType.TabStop = true;
			this.rdMapType.Text = "做为地图类型，地图类型名称";
			this.rdMapType.UseVisualStyleBackColor = true;
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(9, 60);
			this.label17.Name = "label17";
			this.label17.Size = new Size(77, 12);
			this.label17.TabIndex = 21;
			this.label17.Text = "设置图层信息";
			this.label18.AutoSize = true;
			this.label18.Font = new Font("宋体", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.label18.Location = new System.Drawing.Point(6, 17);
			this.label18.Name = "label18";
			this.label18.Size = new Size(72, 16);
			this.label18.TabIndex = 20;
			this.label18.Text = "图层设置";
			this.rdTileLayer.AutoSize = true;
			this.rdTileLayer.Location = new System.Drawing.Point(11, 127);
			this.rdTileLayer.Name = "rdTileLayer";
			this.rdTileLayer.Size = new Size(95, 16);
			this.rdTileLayer.TabIndex = 26;
			this.rdTileLayer.Text = "做为独立图层";
			this.rdTileLayer.UseVisualStyleBackColor = true;
			this.grpBeginCut.BackColor = Color.White;
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
			this.grpBeginCut.Size = new Size(473, 386);
			this.grpBeginCut.TabIndex = 35;
			this.grpBeginCut.TabStop = false;
			this.lblCutProcess.AutoSize = true;
			this.lblCutProcess.Font = new Font("宋体", 15.75f, FontStyle.Bold, GraphicsUnit.Point, 134);
			this.lblCutProcess.Location = new System.Drawing.Point(12, 317);
			this.lblCutProcess.Name = "lblCutProcess";
			this.lblCutProcess.Size = new Size(98, 21);
			this.lblCutProcess.TabIndex = 49;
			this.lblCutProcess.Text = "准备就绪";
			this.txtFinalOutputPath.Location = new System.Drawing.Point(84, 130);
			this.txtFinalOutputPath.Name = "txtFinalOutputPath";
			this.txtFinalOutputPath.ReadOnly = true;
			this.txtFinalOutputPath.Size = new Size(374, 21);
			this.txtFinalOutputPath.TabIndex = 48;
			this.txtFinalImagePath.Location = new System.Drawing.Point(84, 99);
			this.txtFinalImagePath.Name = "txtFinalImagePath";
			this.txtFinalImagePath.ReadOnly = true;
			this.txtFinalImagePath.Size = new Size(374, 21);
			this.txtFinalImagePath.TabIndex = 47;
			this.lblFinalLayer.AutoSize = true;
			this.lblFinalLayer.Location = new System.Drawing.Point(82, 258);
			this.lblFinalLayer.Name = "lblFinalLayer";
			this.lblFinalLayer.Size = new Size(0, 12);
			this.lblFinalLayer.TabIndex = 45;
			this.lblFinalZoom.AutoSize = true;
			this.lblFinalZoom.Location = new System.Drawing.Point(82, 227);
			this.lblFinalZoom.Name = "lblFinalZoom";
			this.lblFinalZoom.Size = new Size(0, 12);
			this.lblFinalZoom.TabIndex = 44;
			this.lblFinalCenter.AutoSize = true;
			this.lblFinalCenter.Location = new System.Drawing.Point(82, 196);
			this.lblFinalCenter.Name = "lblFinalCenter";
			this.lblFinalCenter.Size = new Size(0, 12);
			this.lblFinalCenter.TabIndex = 43;
			this.lblFinalOutputFormat.AutoSize = true;
			this.lblFinalOutputFormat.Location = new System.Drawing.Point(82, 165);
			this.lblFinalOutputFormat.Name = "lblFinalOutputFormat";
			this.lblFinalOutputFormat.Size = new Size(0, 12);
			this.lblFinalOutputFormat.TabIndex = 42;
			this.proBar.Location = new System.Drawing.Point(14, 349);
			this.proBar.Name = "proBar";
			this.proBar.Size = new Size(444, 23);
			this.proBar.TabIndex = 41;
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(11, 134);
			this.label22.Name = "label22";
			this.label22.Size = new Size(65, 12);
			this.label22.TabIndex = 40;
			this.label22.Text = "输出路径：";
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(11, 103);
			this.label21.Name = "label21";
			this.label21.Size = new Size(65, 12);
			this.label21.TabIndex = 39;
			this.label21.Text = "图片路径：";
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(11, 258);
			this.label13.Name = "label13";
			this.label13.Size = new Size(65, 12);
			this.label13.TabIndex = 37;
			this.label13.Text = "图层设置：";
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(11, 196);
			this.label10.Name = "label10";
			this.label10.Size = new Size(65, 12);
			this.label10.TabIndex = 36;
			this.label10.Text = "中心坐标：";
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(11, 165);
			this.label9.Name = "label9";
			this.label9.Size = new Size(65, 12);
			this.label9.TabIndex = 35;
			this.label9.Text = "输出类型：";
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(11, 227);
			this.label14.Name = "label14";
			this.label14.Size = new Size(65, 12);
			this.label14.TabIndex = 27;
			this.label14.Text = "级别设置：";
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(9, 60);
			this.label15.Name = "label15";
			this.label15.Size = new Size(341, 12);
			this.label15.TabIndex = 21;
			this.label15.Text = "点击“开始切图”进行切图，点击“上一步”返回修改配置信息";
			this.label19.AutoSize = true;
			this.label19.Font = new Font("宋体", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.label19.Location = new System.Drawing.Point(6, 17);
			this.label19.Name = "label19";
			this.label19.Size = new Size(72, 16);
			this.label19.TabIndex = 20;
			this.label19.Text = "开始切图";
			this.lblBeginCut.AutoSize = true;
			this.lblBeginCut.ForeColor = Color.Gray;
			this.lblBeginCut.Location = new System.Drawing.Point(23, 304);
			this.lblBeginCut.Name = "lblBeginCut";
			this.lblBeginCut.Size = new Size(53, 12);
			this.lblBeginCut.TabIndex = 36;
			this.lblBeginCut.Text = "开始切图";
			this.openFileDialog.Filter = "支持的图片文件|*.bmp;*.jpg;*.jpeg;*.png;*.gif|所有文件|*.*";
			this.grpPathSet.BackColor = Color.White;
			this.grpPathSet.Controls.Add(this.btnSetPath);
			this.grpPathSet.Controls.Add(this.txtOutputPath);
			this.grpPathSet.Controls.Add(this.label1);
			this.grpPathSet.Controls.Add(this.lblTitlePathSet);
			this.grpPathSet.Location = new System.Drawing.Point(99, 12);
			this.grpPathSet.Name = "grpPathSet";
			this.grpPathSet.Size = new Size(473, 386);
			this.grpPathSet.TabIndex = 38;
			this.grpPathSet.TabStop = false;
			this.btnSetPath.Location = new System.Drawing.Point(392, 98);
			this.btnSetPath.Name = "btnSetPath";
			this.btnSetPath.Size = new Size(75, 23);
			this.btnSetPath.TabIndex = 23;
			this.btnSetPath.Text = "选择...";
			this.btnSetPath.UseVisualStyleBackColor = true;
			this.btnSetPath.Click += new EventHandler(this.btnSetPath_Click);
			this.txtOutputPath.Location = new System.Drawing.Point(11, 99);
			this.txtOutputPath.Name = "txtOutputPath";
			this.txtOutputPath.ReadOnly = true;
			this.txtOutputPath.Size = new Size(375, 21);
			this.txtOutputPath.TabIndex = 22;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 60);
			this.label1.Name = "label1";
			this.label1.Size = new Size(89, 12);
			this.label1.TabIndex = 21;
			this.label1.Text = "设置输出的目录";
			this.lblTitlePathSet.AutoSize = true;
			this.lblTitlePathSet.Font = new Font("宋体", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.lblTitlePathSet.Location = new System.Drawing.Point(6, 17);
			this.lblTitlePathSet.Name = "lblTitlePathSet";
			this.lblTitlePathSet.Size = new Size(72, 16);
			this.lblTitlePathSet.TabIndex = 20;
			this.lblTitlePathSet.Text = "输出路径";
			this.grpOutputFormat.BackColor = Color.White;
			this.grpOutputFormat.Controls.Add(this.rdTilesOnly);
			this.grpOutputFormat.Controls.Add(this.rdTilesAndCode);
			this.grpOutputFormat.Controls.Add(this.label3);
			this.grpOutputFormat.Controls.Add(this.label4);
			this.grpOutputFormat.Location = new System.Drawing.Point(99, 12);
			this.grpOutputFormat.Name = "grpOutputFormat";
			this.grpOutputFormat.Size = new Size(473, 387);
			this.grpOutputFormat.TabIndex = 39;
			this.grpOutputFormat.TabStop = false;
			this.rdTilesOnly.AutoSize = true;
			this.rdTilesOnly.Location = new System.Drawing.Point(11, 128);
			this.rdTilesOnly.Name = "rdTilesOnly";
			this.rdTilesOnly.Size = new Size(59, 16);
			this.rdTilesOnly.TabIndex = 23;
			this.rdTilesOnly.TabStop = true;
			this.rdTilesOnly.Text = "仅图块";
			this.rdTilesOnly.UseVisualStyleBackColor = true;
			this.rdTilesOnly.CheckedChanged += new EventHandler(this.rdTilesOnly_CheckedChanged);
			this.rdTilesAndCode.AutoSize = true;
			this.rdTilesAndCode.Checked = true;
			this.rdTilesAndCode.Location = new System.Drawing.Point(11, 105);
			this.rdTilesAndCode.Name = "rdTilesAndCode";
			this.rdTilesAndCode.Size = new Size(83, 16);
			this.rdTilesAndCode.TabIndex = 22;
			this.rdTilesAndCode.TabStop = true;
			this.rdTilesAndCode.Text = "图块及代码";
			this.rdTilesAndCode.UseVisualStyleBackColor = true;
			this.rdTilesAndCode.CheckedChanged += new EventHandler(this.rdTilesAndCode_CheckedChanged);
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 60);
			this.label3.Name = "label3";
			this.label3.Size = new Size(77, 12);
			this.label3.TabIndex = 21;
			this.label3.Text = "设置输出类型";
			this.label4.AutoSize = true;
			this.label4.Font = new Font("宋体", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
			this.label4.Location = new System.Drawing.Point(6, 17);
			this.label4.Name = "label4";
			this.label4.Size = new Size(72, 16);
			this.label4.TabIndex = 20;
			this.label4.Text = "输出类型";
			this.picStepBg.Location = new System.Drawing.Point(0, 63);
			this.picStepBg.Name = "picStepBg";
			this.picStepBg.Size = new Size(100, 30);
			this.picStepBg.TabIndex = 40;
			this.picStepBg.TabStop = false;
			this.cutTimer.Interval = 200;
			this.cutTimer.Tick += new EventHandler(this.cutTimer_Tick);
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(293, 108);
			this.label16.Name = "label16";
			this.label16.Size = new Size(161, 12);
			this.label16.TabIndex = 28;
			this.label16.Text = "在地图中通过右键可选择坐标";
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			this.BackColor = Color.White;
			base.ClientSize = new Size(584, 462);
			base.Controls.Add(this.grpCenterSet);
			base.Controls.Add(this.grpBeginCut);
			base.Controls.Add(this.grpSelImage);
			base.Controls.Add(this.grpLayerSet);
			base.Controls.Add(this.grpOutputFormat);
			base.Controls.Add(this.grpZoomSet);
			base.Controls.Add(this.grpPathSet);
			base.Controls.Add(this.lblBeginCut);
			base.Controls.Add(this.btnPrev);
			base.Controls.Add(this.btnNext);
			base.Controls.Add(this.lblSelImage);
			base.Controls.Add(this.lblLayerSet);
			base.Controls.Add(this.lblZoomSet);
			base.Controls.Add(this.lblCenterSet);
			base.Controls.Add(this.lblOutputFormat);
			base.Controls.Add(this.lblPathSet);
			base.Controls.Add(this.picStepBg);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.MaximizeBox = false;
			base.Name = "TileCutter";
			this.Text = "TileCutter";
			base.FormClosing += new FormClosingEventHandler(this.TileCutter_FormClosing);
			base.Load += new EventHandler(this.TileCutter_Load);
			this.grpSelImage.ResumeLayout(false);
			this.grpSelImage.PerformLayout();
			((ISupportInitialize)this.picPreview).EndInit();
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
			((ISupportInitialize)this.picStepBg).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
