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

        private void Form1_Load(object sender, EventArgs e)
        {
            //skinEngine1.SkinFile = "C:\\Users\\zyb71\\Desktop\\WinFormSkin\\WinFormSkin\\bin\\Debug\\Skins\\SportsBlue.ssk";

            //左侧树文字
            TreeNode tnForm = new TreeNode("  文件");
            tnForm.Nodes.Add("打开文件");
            tnForm.Nodes.Add("打开文件夹");

            this.tvMenu.Nodes.Add(tnForm);

            TreeNode tnControl = new TreeNode("  选择区域");
            tnControl.Nodes.Add("绘制矩形");
            tnControl.Nodes.Add("剪裁");
            tnControl.Nodes.Add("纵切面");

            this.tvMenu.Nodes.Add(tnControl);


            TreeNode tnControl2 = new TreeNode("  图像处理");
            tnControl2.Nodes.Add("图像增强");
            tnControl2.Nodes.Add("图像去噪");
            tnControl2.Nodes.Add("分析地物");
            //tnControl2.Nodes.Add("保存图片");

            this.tvMenu.Nodes.Add(tnControl2);


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
        private void newThread()
        {
            Open.ReadAllTif(this);
            Open.DisplayTif(this);
            Open.ReadImg = true;
            
        }
        private void tvMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strName = e.Node.Text.Trim();
            
            switch (strName)
            {

                #region   文件按钮
                case "打开文件":

                    //this.ucBtnExt3.Enabled = true;
                    //this.ucBtnExt4.Enabled = true;
                    //this.button4.Enabled = true;
                    //this.button5.Enabled = false;
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
                    Open.FilePaths = open.FileNames;
                    Open.num = Open.FilePaths.GetLength(0);
                    this.ucTrackBar1.MaxValue = Open.num;
                    //string pathsource = open.FileName;
                    //FilePath.Path = pathsource;

                    //Open.ReadTif();


                    this.ucProcessLineExt1.MaxValue = Open.num;

                    this.ucProcessLineExt1.Value = 0;

                    //this.label4.Text = "正在读取图片......";
                    Thread thread = new Thread(new ThreadStart(newThread));
                    thread.IsBackground = true;
                    thread.Start();

                    break;
                case "打开文件夹":
                    FrmDialog.ShowDialog(this, "这是一个没有取消按钮的提示框", "模式窗体测试");
                    break;
                #endregion


                #region 区域按钮

                case "绘制矩形":
                    //this.button5.Enabled = true;

                    FrmDialog.ShowDialog(this,"拖动鼠标绘制矩形框，选择裁剪范围。");
                    Open.LonSec = false;

                    pictureBox1.Refresh();
                    Open.RecEnd = new Point(-1, -1);
                    Open.RecStart = new Point(-1, -1);
                    Open.RecPaint = true;

                    //Open.ReadAllOffTif(this);
                    Open.DisplayTif(this);

                    break;

                case "剪裁":
                    if (Open.ReadImg == false)
                    {
                        FrmDialog.ShowDialog(this,"图片正在读取，请稍后进行该操作。");
                        return;
                    }
                    this.ucProcessLineExt1.Value = 0;
                    Open.RecPaint = false;
                    if (Open.RecStart.X >= 0 && Open.RecEnd.X >= 0)
                    {
                        Open.ReadAllOffTif(this);
                        Open.DisplayTif(this);
                    }
                    //this.button5.Enabled = false;

                    break;

                case "纵切面":
                    if (Open.ReadImg == false)
                    {
                        FrmDialog.ShowDialog(this,"图片正在读取，请稍后进行该操作。");
                        return;
                    }
                    //this.button5.Enabled = false;
                    Open.RecPaint = false;
                    pictureBox1.Refresh();
                    Open.LonSec = true;
                    FrmDialog.ShowDialog(this,"点击确定两点，确定绘制纵切面的区域。");
                    break;
                #endregion


                #region 图像处理按钮
                case "图像增强":
                    MessageBox.Show("Hello");
                    break;

                case "图像去噪":
                    MessageBox.Show("Hello");
                    break;

                case "分析地物":
                    ImageProcess ImgPro = new ImageProcess();
                    if (Open.ReadImg == false)
                    {
                        FrmDialog.ShowDialog(this,"图片正在读取，请稍后进行该操作。");
                        return;
                    }
                    //button6.Enabled = true;
                    ImgPro.GetImg(this, Open);
                    Open.DisplayTif(this);
                    break;

                #endregion
            }

        }

        private void ucBtnExt1_Click(object sender, EventArgs e)
        {
            
        }

        private void ucBtnExt1_BtnClick(object sender, EventArgs e)
        {

        }

        private void ucBtnExt2_BtnClick(object sender, EventArgs e)
        {
            //ControlHelper.ThreadRunExt(this, () =>
            //{
            //    Thread.Sleep(5000);
            //    //ControlHelper.ThreadInvokerControl(this, () =>
            //    //{
            //    //    HZH_Controls.Forms.FrmTips.ShowTipsSuccess(this, "FrmWaiting测试");
            //    //});
            //}, null, this);

        }

        private void ucBtnExt3_BtnClick(object sender, EventArgs e)
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
    }
}
