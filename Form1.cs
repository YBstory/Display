using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HZH_Controls;
using System.Threading;
using BitMiracle.LibTiff.Classic;
using HZH_Controls.Forms;
using System.IO;

namespace RadarDisplay
{
    delegate void SetValueCallback(int value);

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        OpenTiff Open = new OpenTiff();
        int i = 0;

        /// <summary>
        /// 子线程控制进度条
        /// </summary>
        /// <param name="value"></param>
        public void SetProcessBarValue(int value)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.ucProcessLineExt1.InvokeRequired)
            {
                SetValueCallback d = new SetValueCallback(SetProcessBarValue);
                this.Invoke(d, new object[] { value });
            }
            else

            {
                this.ucProcessLineExt1.Value = value;
            }
        }


        #region 所有按钮操作函数
        /// <summary>
        /// 读取图片，子线程函数
        /// </summary>
        private void newThread()
        {
            Open.ReadAllTif(this);
            Open.DisplayTif(this);
            Open.ReadImg = true;
        }

       /// <summary>
       /// 打开文件
       /// </summary>
        private  void OpenFile()
        {
            this.ucTrackBar1.Value = 1;
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "*.tif文件|*.tif|所有文件|*.*";
            open.Multiselect = true;//等于true表示可以选择多个文件

            if (open.ShowDialog() != DialogResult.OK)
            {

                return;
            }
            if (open.FileNames == null)
            {
                return;
            }

            //选择区域
            this.ucBtnExt3.Enabled = true;
            this.ucBtnExt4.Enabled = false;
            this.ucBtnExt9.Enabled = true;

            //图像处理
            this.ucBtnExt5.Enabled = true;
            this.ucBtnExt6.Enabled = true;
            this.ucBtnExt7.Enabled = true;


            Open.FilePaths = open.FileNames;
            Open.num = Open.FilePaths.GetLength(0);
            this.ucTrackBar1.MaxValue = Open.num;
            this.ucProcessLineExt1.MaxValue = Open.num;
            this.ucProcessLineExt1.Value = 0;

            //this.label4.Text = "正在读取图片......";
            Thread thread = new Thread(new ThreadStart(newThread));
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        private void OpenFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择Tiff影像所在文件夹";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
            }
            else
            {
                return;
            }

            string RootPath = dialog.SelectedPath;
            DirectoryInfo TheFolder = new DirectoryInfo(RootPath);
            int num = TheFolder.GetFiles().Length;

            Open.FilePaths = new string[num];
            int j = 0;
            foreach (FileInfo NextFile in TheFolder.GetFiles())
            {
                string FilePath = RootPath + "\\" + NextFile.Name;
                Open.FilePaths[j] = FilePath;
                j++;
            }

            //选择区域
            this.ucBtnExt3.Enabled = true;
            this.ucBtnExt4.Enabled = false;
            this.ucBtnExt9.Enabled = true;

            //图像处理
            this.ucBtnExt5.Enabled = true;
            this.ucBtnExt6.Enabled = true;
            this.ucBtnExt7.Enabled = true;


            Open.num = num;
            this.ucTrackBar1.MaxValue = num;
            this.ucTrackBar1.Value = 1;
            this.ucProcessLineExt1.MaxValue = num;
            this.ucProcessLineExt1.Value = 0;

            Thread thread = new Thread(new ThreadStart(newThread));
            thread.IsBackground = true;
            thread.Start();

        }

        /// <summary>
        /// 保存图片堆
        /// </summary>
        private void SaveTiffStack()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "保存位置";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
            }

            string RootPath = dialog.SelectedPath;

            Open.SaveStack(RootPath);
            FrmDialog.ShowDialog(this, "图片保存成功！");
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        private void DrawRec()
        {
            if (!ucBtnExt3.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }

            this.ucBtnExt4.Enabled = true;

            FrmDialog.ShowDialog(this, "拖动鼠标绘制矩形框，选择裁剪范围。");
            Open.LonSec = false;

            pictureBox1.Refresh();
            Open.RecEnd = new Point(-1, -1);
            Open.RecStart = new Point(-1, -1);
            Open.RecPaint = true;

            //Open.ReadAllOffTif(this);
            Open.DisplayTif(this);
        }

        /// <summary>
        /// 剪裁
        /// </summary>
        private void CutTiff()
        {
            if (!ucBtnExt4.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }

            if (Open.ReadImg == false)
            {
                FrmDialog.ShowDialog(this, "图片正在读取，请稍后进行该操作。");
                return;
            }
            this.ucProcessLineExt1.Value = 0;
            Open.RecPaint = false;
            if (Open.RecStart.X >= 0 && Open.RecEnd.X >= 0)
            {
                Open.ReadAllOffTif(this);
                Open.DisplayTif(this);
            }
            this.ucBtnExt4.Enabled = false;

            pictureBox2.Image = null;
        }

        /// <summary>
        /// 纵切面
        /// </summary>
        private void LongitudinalSection()
        {
            if (!ucBtnExt9.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }

            if (Open.ReadImg == false)
            {
                FrmDialog.ShowDialog(this, "图片正在读取，请稍后进行该操作。");
                return;
            }
            this.ucBtnExt4.Enabled = false;
            Open.RecPaint = false;
            pictureBox1.Refresh();
            Open.LonSec = true;
            FrmDialog.ShowDialog(this, "点击确定两点，确定绘制纵切面的区域。");
            
        }

        /// <summary>
        /// 图像增强
        /// </summary>
        private void ImageIntensifier()
        {
            if (!ucBtnExt5.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }

            ImageProcess ImgPro = new ImageProcess();
            ImgPro.histogram(Open);
            Open.DisplayTif(this);
        }

        private void GrayEXP()
        {
            if (!ucBtnExt5.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }

            ImageProcess ImgPro = new ImageProcess();
            ImgPro.gray_expand(this,Open);
            Open.DisplayTif(this);
        }

        /// <summary>
        /// 二维均值滤波
        /// </summary>
        private void MeanFilter_2D()
        {
            if (!ucBtnExt6.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }
            ImageProcess ImgPro = new ImageProcess();

            ImgPro.MeanFilterStack(this, Open);
            Open.DisplayTif(this);
        }

        /// <summary>
        /// 三维均值滤波
        /// </summary>
        private void MeanFilter_3D()
        {
            if (!ucBtnExt6.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }
            ImageProcess ImgPro = new ImageProcess();

            ImgPro.MeanFilter3D(this, Open);
            Open.DisplayTif(this);
        }

        /// <summary>
        /// 二维中值滤波
        /// </summary>
        private void MedianFilter_2D()
        {
            if (!ucBtnExt6.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }
            ImageProcess ImgPro = new ImageProcess();

            ImgPro.MedianFilterStack(this, Open);
            Open.DisplayTif(this);
        }

        /// <summary>
        /// 三维中值滤波
        /// </summary>
        private void MedianFilter_3D()
        {
            if (!ucBtnExt6.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }
            ImageProcess ImgPro = new ImageProcess();

            ImgPro.MedianFilter3D(this, Open);
            Open.DisplayTif(this);
        }

        /// <summary>
        /// 分析地物
        /// </summary>
        private void Analysis()
        {
            if (!ucBtnExt7.Enabled)
            {
                FrmDialog.ShowDialog(this, "尚未读取文件");
                return;
            }

            ImageProcess ImgPro = new ImageProcess();
            if (Open.ReadImg == false)
            {
                FrmDialog.ShowDialog(this, "图片正在读取，请稍后进行该操作。");
                return;
            }
            //button6.Enabled = true;
            ImgPro.GetImg(this, Open);
            Open.DisplayTif(this);
            pictureBox2.Image = null;
        }

        /// <summary>
        /// 转换为灰度影像
        /// </summary>
        private void Convert2G()
        {
            ImageProcess ImgPro = new ImageProcess();
            if (Open.ReadImg == false)
            {
                FrmDialog.ShowDialog(this, "图片正在读取，请稍后进行该操作。");
                return;
            }
            //button6.Enabled = true;
            ImgPro.Convert2GrayStack(this, Open);
            Open.DisplayTif(this);
        }
        #endregion


        /// <summary>
        /// Form加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //skinEngine1.SkinFile = "C:\\Users\\zyb71\\Desktop\\WinFormSkin\\WinFormSkin\\bin\\Debug\\Skins\\SportsBlue.ssk";

            //左侧树文字
            TreeNode tnForm = new TreeNode("  文件");
            tnForm.Nodes.Add("打开文件");
            tnForm.Nodes.Add("打开文件夹");
            tnForm.Nodes.Add("保存图片堆");

            this.tvMenu.Nodes.Add(tnForm);

            TreeNode tnControl = new TreeNode("  选择区域");
            tnControl.Nodes.Add("绘制矩形");
            tnControl.Nodes.Add("剪裁");
            tnControl.Nodes.Add("纵切面");

            this.tvMenu.Nodes.Add(tnControl);


            TreeNode tnControl2 = new TreeNode("  图像去噪");

            tnControl2.Nodes.Add("二维均值滤波");
            tnControl2.Nodes.Add("二维中值滤波");
            tnControl2.Nodes.Add("三维均值滤波");
            tnControl2.Nodes.Add("三维中值滤波");


            this.tvMenu.Nodes.Add(tnControl2);

            TreeNode tnControl3 = new TreeNode("  图像增强与分析");
            tnControl3.Nodes.Add("灰度拉伸");
            tnControl3.Nodes.Add("直方图均衡化");
            tnControl3.Nodes.Add("分析地物");
            tnControl3.Nodes.Add("转换为灰色影像");

            this.tvMenu.Nodes.Add(tnControl3);


            //按钮区域文件
            ucBtnExt1.BtnText = "打开文件";
            ucBtnExt2.BtnText = "打开文件夹";

            ucBtnExt3.BtnText = "绘制矩形";
            ucBtnExt4.BtnText = "剪裁";
            ucBtnExt9.BtnText = "纵切面";

            ucBtnExt5.BtnText = "图像增强";
            ucBtnExt6.BtnText = "图像去噪";
            ucBtnExt7.BtnText = "分析地物";
            ucBtnExt8.BtnText = "保存图片";

        }

        private void ucNavigationMenu1_ClickItemed(object sender, EventArgs e)
        {
            MessageBox.Show("HELLO");
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        /// <summary>
        /// 左侧按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void tvMenu_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string strName = e.Node.Text.Trim();

            switch (strName)
            {

                #region   文件按钮
                case "打开文件":
                    OpenFile();
                    break;

                case "打开文件夹":
                    OpenFolder();
                    break;

                case "保存图片堆":
                    SaveTiffStack();
                    break;

                #endregion

                #region 区域按钮
                case "绘制矩形":
                    DrawRec();
                    break;

                case "剪裁":
                    CutTiff();
                    break;

                case "纵切面":
                    LongitudinalSection();
                    break;
                #endregion


                #region 图像处理按钮
                case "直方图均衡化":
                    ImageIntensifier();
                    break;

                case "灰度拉伸":
                    GrayEXP();
                    break;

                case "分析地物":
                    Analysis();
                    break;

                case "二维均值滤波":
                    MeanFilter_2D();
                    break;

                case "二维中值滤波":
                    MedianFilter_2D();
                    break;

                case "三维均值滤波":
                    MeanFilter_3D();
                    break;

                case "三维中值滤波":
                    MedianFilter_3D();
                    break;

                case "转换为灰色影像":
                    Convert2G();
                    break;
                    #endregion
            }
        }


        private void tvMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }

        private void ucBtnExt1_Click(object sender, EventArgs e)
        {
            
        }



        private void ucTrackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (Open.firstImg)
            {
                Open.ID = (int)ucTrackBar1.Value - 1;
                //this.label1.Text = trackBar1.Value.ToString();
                Open.DisplayTif(this);
            }
            if (Open.LonSec)
            {
                Open.DisplayDeep(this, (int)ucTrackBar1.Value);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (Open.LonSec)
            {
                if (Open.DrawLine)
                {
                    Open.DrawLine = false;
                    Open.start = new Point(-1, -1);
                    Open.end = new Point(-1, -1);
                    pictureBox1.Refresh();
                }
                if (Open.start.X > 0)
                {
                    Open.end = e.Location;
                    Open.DrawLine = true;

                    Form1_Paint(sender, null);
                    Open.NewLonTiff(this);
                }
                else
                {
                    Open.start = e.Location;
                }
            }
            else
            {
                Open.DrawLine = false;
                Open.start = new Point(-1, -1);
                Open.end = new Point(-1, -1);
                pictureBox1.Refresh();
            }

            if (Open.RecPaint)
            {
                i = 1;
                Open.RecStart = e.Location;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (Open.RecPaint)
            {

                Open.RecEnd = e.Location;
                i = 0;

            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //this.label2.Text = e.X.ToString();
            //this.label3.Text = e.Y.ToString();

            if (Open.RecPaint && i > 0)
            {
                i++;
                Open.RecEnd = e.Location;
                pictureBox1.Refresh();
                Form1_Paint(sender, null);

            }
        }


        Pen mypen = new Pen(Color.Red, 3);//设置画笔属性
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (Open.DrawLine)
            {
                Graphics grfx = pictureBox1.CreateGraphics();
                grfx.DrawLine(mypen, Open.start, Open.end);
                grfx.Dispose();
            }
            if (Open.RecPaint)
            {
                Graphics grfx = pictureBox1.CreateGraphics();

                int width = Open.RecEnd.X - Open.RecStart.X;
                int height = Open.RecEnd.Y - Open.RecStart.Y;
                grfx.DrawRectangle(mypen, Open.RecStart.X, Open.RecStart.Y, width, height);

            }
        }

        private void ucBtnExt8_BtnClick(object sender, EventArgs e)
        {
            //保存图片
            bool isSave = true;
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.Title = "图片保存";
            saveImageDialog.Filter = @"tif|*.tif|jpeg|*.jpg|bmp|*.bmp|gif|*.gif";


            if (saveImageDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveImageDialog.FileName.ToString();

                if (fileName != "" && fileName != null)
                {
                    string fileExtName = fileName.Substring(fileName.LastIndexOf(".") + 1).ToString();

                    System.Drawing.Imaging.ImageFormat imgformat = null;

                    if (fileExtName != "")
                    {
                        switch (fileExtName)
                        {
                            case "jpg":
                                imgformat = System.Drawing.Imaging.ImageFormat.Jpeg;
                                break;
                            case "bmp":
                                imgformat = System.Drawing.Imaging.ImageFormat.Bmp;
                                break;
                            case "gif":
                                imgformat = System.Drawing.Imaging.ImageFormat.Gif;
                                break;
                            case "tif":
                                imgformat = System.Drawing.Imaging.ImageFormat.Tiff;
                                break;
                            default:
                                FrmDialog.ShowDialog(this,"只能存取为: jpg,bmp,gif,tif 格式");
                                isSave = false;
                                break;
                        }

                    }

                    //默认保存为JPG格式  
                    if (imgformat == null)
                    {
                        imgformat = System.Drawing.Imaging.ImageFormat.Tiff;
                    }

                    if (isSave)
                    {
                        try
                        {
                            this.pictureBox1.Image.Save(fileName, imgformat);
                            FrmDialog.ShowDialog(this,"图片已经成功保存!");
                        }
                        catch
                        {
                            FrmDialog.ShowDialog(this,"保存失败!");
                        }
                    }
                }
            }
        }

        private void tvMenu_AfterExpand(object sender, TreeViewEventArgs e)
        {
            
        }

        #region 界面按钮点击

        private void ucBtnExt1_BtnClick(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void ucBtnExt2_BtnClick(object sender, EventArgs e)
        {
            OpenFolder();
        }

        private void ucBtnExt3_BtnClick(object sender, EventArgs e)
        {
            DrawRec();
        }

        private void ucBtnExt4_BtnClick(object sender, EventArgs e)
        {
            CutTiff();
        }

        private void ucBtnExt9_BtnClick(object sender, EventArgs e)
        {
            LongitudinalSection();
        }

        private void ucBtnExt5_BtnClick(object sender, EventArgs e)
        {
            ImageIntensifier();
        }

        private void ucBtnExt6_BtnClick(object sender, EventArgs e)
        {

        }

        private void ucBtnExt7_BtnClick(object sender, EventArgs e)
        {
            Analysis();
        }
        #endregion


    }
}
