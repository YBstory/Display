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

namespace RadarDisplay
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //skinEngine1.SkinFile = "C:\\Users\\zyb71\\Desktop\\WinFormSkin\\WinFormSkin\\bin\\Debug\\Skins\\SportsBlue.ssk";
            TreeNode tnForm = new TreeNode("  文件");
            tnForm.Nodes.Add("打开文件");
            tnForm.Nodes.Add("打开文件夹");

            this.tvMenu.Nodes.Add(tnForm);

            TreeNode tnControl = new TreeNode("  控件");
            tnControl.Nodes.Add("表单控件");
            tnControl.Nodes.Add("按钮");


            this.tvMenu.Nodes.Add(tnControl);

            TreeNode tnControl2 = new TreeNode("  控件2");
            this.tvMenu.Nodes.Add(tnControl2);


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

        private void tvMenu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strName = e.Node.Text.Trim();
            
            switch (strName)
            {
                case "打开文件":
                    MessageBox.Show("Hello");
                    ucProcessLineExt1.MaxValue = 100;
                    for (int i = 0; i < 101; i++) {
                        ucProcessLineExt1.Value = i;
                    }
                    
                    break;
                case "打开文件夹":
                    MessageBox.Show("Hello");
                    break;
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
    }
}
