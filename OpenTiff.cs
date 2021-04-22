using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BitMiracle.LibTiff.Classic;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


namespace RadarDisplay
{
    class OpenTiff
    {

        public bool LonSec = false;  //绘制纵切面
        public bool RecPaint = false;    //截图绘制矩形
        #region 读入TIF图像的变量设置
        public string path;
        public string[] FilePaths;
        public int num = 0;
        public bool play = false;

        public bool firstImg = false;
        private int width;// 存储读入的TIF图像的宽度
        private int height;//存储读入的TIF图像的高度
        private int imageSize;
        private int[] raster;
        private Bitmap TifMap;//声明一个图像，存储读入的TIF图像
        private Bitmap TifMapbuffer;//声明一个图像，存储读入的TIF图像


        public Bitmap[] TifMapGroup;//声明一个图像，存储读入的TIF图像
        public Bitmap[] bufferMapGroup;//作为暂存的图像

        public bool ReadImg = false;

        public int ID = 0;


        public Point start = new Point(-1, -1);
        public Point end;
        public Point RecStart = new Point(-1, -1);
        public Point RecEnd = new Point(-1, -1);

        public bool DrawLine = false;
        public Bitmap LonTiff;//保存纵截面
        public Bitmap LonTiffBuff; //纵截面备份
        #endregion

        #region 获取某像素红 绿 蓝分量
        public Color GetPixColor(int x, int y, int[] raster, int width, int height)
        {
            int offset = (height - y - 1) * width + x;
            int red = Tiff.GetR(raster[offset]);
            int green = Tiff.GetG(raster[offset]);
            int blue = Tiff.GetB(raster[offset]);

            return Color.FromArgb(red, green, blue);
        }
        #endregion

        #region 获取某像素信息
        public int[] GetInfo(int x, int y, int[] raster, int width, int height)
        {
            int[] rgb = new int[5];
            int offset = (height - y - 1) * width + x;
            int red = Tiff.GetR(raster[offset]);
            int green = Tiff.GetG(raster[offset]);
            int blue = Tiff.GetB(raster[offset]);
            rgb[0] = red;
            rgb[1] = green;
            rgb[2] = blue;
            rgb[3] = x;
            rgb[4] = y;
            return rgb;
        }
        #endregion

        #region 读取Tif图像
        public void ReadTif()
        {
            Tiff image = Tiff.Open(path, "r");
            if (image == null)
            {
                return;
            }
            do
            {
                FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
                width = value[0].ToInt();

                value = image.GetField(TiffTag.IMAGELENGTH);
                height = value[0].ToInt();

                imageSize = width * height;
                raster = new int[imageSize];


                TifMap = new Bitmap(width, height);
                TifMapbuffer = new Bitmap(width, height);


                if (!image.ReadRGBAImage(width, height, raster))
                {
                    return;
                }
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {

                        TifMap.SetPixel(i, j, GetPixColor(i, j, raster, width, height));
                        TifMapbuffer.SetPixel(i, j, GetPixColor(i, j, raster, width, height));
                    }
                }
            }
            while (image.ReadDirectory());

        }
        #endregion

        #region 读取全部tif影像
        public void ReadAllTif(Form1 form)
        {
           
            //IMin = 10000;
            //IMax = -1;
            //JMin = 10000;
            //JMax = -1;
            TifMapGroup = new Bitmap[num];
            bufferMapGroup=new Bitmap[num];
            for (int i = 0; i < num; i++)
            {

                path = FilePaths[i];
                ReadTif();

                TifMapGroup[i] = TifMap;
                bufferMapGroup[i] = TifMapbuffer;
                firstImg = true;
                int a = form.ucProcessLineExt1.Value;
                form.SetProcessBarValue(i + 1);
            }
        }
        #endregion


        //#region 计算白边范围
        //public void CalBlank(Form1 form)
        //{
        //    Tiff image = Tiff.Open(path, "r");
        //    if (image == null)
        //    {
        //        return;
        //    }
        //    do
        //    {
        //        FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
        //        width = value[0].ToInt();

        //        value = image.GetField(TiffTag.IMAGELENGTH);
        //        height = value[0].ToInt();

        //        imageSize = width * height;
        //        raster = new int[imageSize];


        //        if (!image.ReadRGBAImage(width, height, raster))
        //        {
        //            return;
        //        }
        //        for (int i = 0; i < width; i++)
        //        {
        //            for (int j = 0; j < height; j++)
        //            {

        //                if (GetPixColor(i, j, raster, width, height) == Color.FromArgb(158,101,58))
        //                {
        //                    if (IMax < i) IMax = i;
        //                    if (IMin > i) IMin = i;
        //                    if (JMax < j) JMax = j;
        //                    if (JMin > j) JMin = j;
        //                }
        //            }
        //        }
        //    }
        //    while (image.ReadDirectory());
        //}
        //#endregion

        #region 读取剪裁图片
        public void ReadOffTif(int PicID)
        {
            //Tiff image = Tiff.Open(path, "r");
            //if (image == null)
            //{
            //    return;
            //}

            //do
            //{
            //FieldValue[] value = image.GetField(TiffTag.IMAGEWIDTH);
            //width = value[0].ToInt();

            //value = image.GetField(TiffTag.IMAGELENGTH);
            //height = value[0].ToInt();

            //imageSize = width * height;
            //raster = new int[imageSize];

            Bitmap bufferMap = TifMapGroup[PicID];
            TifMap = new Bitmap(RecEnd.X - RecStart.X - 1, RecEnd.Y - RecStart.Y - 1);


            //if (!image.ReadRGBAImage(width, height, raster))
            //{
            //    return;
            //}
            for (int i = RecStart.X + 1; i < RecEnd.X; i++)
            {
                for (int j = RecStart.Y + 1; j < RecEnd.Y; j++)
                {

                    TifMap.SetPixel(i - RecStart.X - 1, j - RecStart.Y - 1, bufferMap.GetPixel(i, j));
                }
            }
            //}
            //while (image.ReadDirectory());

        }
        #endregion

        #region 读取全部去白边图片
        public void ReadAllOffTif(Form1 form)
        {

            //CalBlank(form);
            RecStart = CalLocation(form, RecStart);
            RecEnd = CalLocation(form, RecEnd);

            if (RecStart.X < 0 || RecEnd.X < 0)
            {
                return;
            }

            for (int i = 0; i < num; i++)
            {

                path = FilePaths[i];
                ReadOffTif(i);
                TifMapGroup[i] = TifMap;
                form.ucProcessLineExt1.Value++;
            }
        }
        #endregion

        #region 原始坐标计算
        //计算原始坐标
        public Point CalLocation(Form1 form, Point e)
        {
            int originalWidth = form.pictureBox1.Image.Width;//图片分辨率
            int originalHeight = form.pictureBox1.Image.Height;

            PropertyInfo rectangleProperty = form.pictureBox1.GetType().GetProperty("ImageRectangle", BindingFlags.Instance | BindingFlags.NonPublic);
            Rectangle rectangle = (Rectangle)rectangleProperty.GetValue(form.pictureBox1, null);

            int currentWidth = rectangle.Width;
            int currentHeight = rectangle.Height;

            double rate1 = (double)currentWidth / (double)originalWidth;
            double rate2 = (double)currentHeight / (double)originalHeight;

            int black_left_width = (currentWidth == form.pictureBox1.Width) ? 0 : (form.pictureBox1.Width - currentWidth) / 2;
            int black_top_height = (currentHeight == form.pictureBox1.Height) ? 0 : (form.pictureBox1.Height - currentHeight) / 2;

            int zoom_x = e.X - black_left_width;
            int zoom_y = e.Y - black_top_height;

            double original_x = (double)zoom_x / rate1;
            double original_y = (double)zoom_y / rate2;

            if (original_x < 0 || original_y < 0 || original_x >= TifMapGroup[0].Width || original_y >= TifMapGroup[0].Height)
            {
                MessageBox.Show("超出边界");
                form.ucBtnExt4.Enabled = false;
                return new Point(-1, -1);
            }
            Point p = new Point((int)original_x, (int)original_y);
            return p;
        }
        #endregion

        #region 创建纵截面图片
        public void NewLonTiff(Form1 form)
        {
            Point RealStart = new Point();
            Point RealEnd = new Point();

            RealStart = CalLocation(form, start);
            RealEnd = CalLocation(form, end);

            if (RealStart.X < 0 || RealEnd.X < 0)
            {
                return;
            }
            int widthLon;
            int heightLon = num;
            int deltaX = System.Math.Abs(RealStart.X - RealEnd.X);
            int deltaY = System.Math.Abs(RealStart.Y - RealEnd.Y);
            double k;
            double x, y;

            if (deltaX >= deltaY)
            {
                widthLon = deltaX;
                k = (double)(RealStart.Y - RealEnd.Y) / (double)(RealStart.X - RealEnd.X);

                if (RealStart.X >= RealEnd.X)
                {
                    x = (double)RealEnd.X;
                    y = (double)RealEnd.Y;
                }
                else
                {
                    x = (double)RealStart.X;
                    y = (double)RealStart.Y;
                }

            }
            else
            {
                widthLon = deltaY;
                k = (double)(RealStart.X - RealEnd.X) / (double)(RealStart.Y - RealEnd.Y);

                if (RealStart.Y >= RealEnd.Y)
                {
                    x = (double)RealEnd.X;
                    y = (double)RealEnd.Y;
                }
                else
                {
                    x = (double)RealStart.X;
                    y = (double)RealStart.Y;
                }
            }

            LonTiff = new Bitmap(widthLon + 1, heightLon);
            LonTiffBuff = new Bitmap(widthLon + 1, heightLon);


            if (deltaX >= deltaY)
            {
                for (int i = 0; i <= widthLon; i++)
                {
                    for (int j = 0; j < heightLon; j++)
                    {

                        LonTiff.SetPixel(i, j, TifMapGroup[j].GetPixel(((int)Math.Round(x+0.5)), ((int)Math.Round(y+0.5))));
                    }
                    x = x + 1;
                    y = y + k;
                }
            }
            else
            {
                for (int i = 0; i <= widthLon; i++)
                {
                    for (int j = 0; j < heightLon; j++)
                    {
                        LonTiff.SetPixel(i, j, TifMapGroup[j].GetPixel((int)Math.Round(x+0.5), (int)Math.Round(y+0.5)));
                    }
                    y = y + 1;
                    x = x + k;
                }
            }

            LonTiffBuff = (Bitmap)DeepCopyBitmap(LonTiff);
            form.pictureBox2.Image = LonTiff;
            
        }
        #endregion

        public void DisplayDeep(Form1 form, int ProgressID)
        {
            int widthLon = LonTiff.Width;
            int heightLon = LonTiff.Height;

            LonTiff = new Bitmap(widthLon, heightLon);
            LonTiff = (Bitmap)DeepCopyBitmap(LonTiffBuff);
            for (int i = 0; i < widthLon; i++)
            {
                LonTiff.SetPixel(i, ProgressID - 1, Color.FromArgb(255, 0, 0));
            }


            form.pictureBox2.Image = LonTiff;
        }

        #region 显示图像
        public void DisplayTif(Form1 form)
        {
            //Point start = new Point(0, 0);
            //Point end = new Point(100, 100);
            //Graphics g = Graphics.FromImage(TifMapGroup[ID]);

            //Brush brush = new SolidBrush(Color.Red);
            //Pen pen = new Pen(brush);
            //g.DrawLine(pen, start, end);
            //g.DrawLine(pen, 0,0,150,200);

            form.pictureBox1.Image = TifMapGroup[ID];


        }
        #endregion

        #region 读取图像信息(已注释)

        //public DataSet ReadInfo()
        //{
        //    Tiff image = Tiff.Open(FilePath.Path, "r");
        //    if (image == null)
        //    {
        //        MessageBox.Show("请读取图像");
        //    }
        //    ds = new DataSet();
        //    table = new DataTable();
        //    column1 = new DataColumn("R");
        //    column2 = new DataColumn("G");
        //    column3 = new DataColumn("B");
        //    column4 = new DataColumn("X");
        //    column5 = new DataColumn("Y");
        //    table.Columns.Add(column1);
        //    table.Columns.Add(column2);
        //    table.Columns.Add(column3);
        //    table.Columns.Add(column4);
        //    table.Columns.Add(column5);
        //    for (int i = 0; i < width; i++)
        //    {
        //        for (int j = 0; j < height; j++)
        //        {
        //            int[] rgbxy = GetInfo(i, j, raster, width, height);
        //            dataRow = table.NewRow();
        //            for (int k = 0; k < 5; k++)
        //            {
        //                dataRow[k] = rgbxy[k].ToString();
        //            }
        //            table.Rows.Add(dataRow);
        //        }
        //    }
        //    ds.Tables.Add(table);
        //    return ds;
        //}
        #endregion

        #region 显示信息(已注释)
        //public void DisplayInfo(Form1 form)
        //{
        //    DataSet data = new DataSet();
        //    data = ReadInfo();
        //    form.dataGridView1.DataSource = data.Tables[0];
        //}
        #endregion

        #region   图像深拷贝
        //单图像深拷贝
        public object DeepCopyBitmap(Bitmap map)
        {
            BinaryFormatter Formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));
            MemoryStream stream = new MemoryStream();
            Formatter.Serialize(stream, map);
            stream.Position = 0;
            object clonedObj = Formatter.Deserialize(stream);
            stream.Close();
            return clonedObj;
        }

        #endregion

        #region 保存影像堆
        public void SaveStack(string SavePath)
        {
            System.Drawing.Imaging.ImageFormat imgformat = System.Drawing.Imaging.ImageFormat.Tiff;

            for(int i = 0; i < num; i++)
            {
                string buffer = i.ToString();
                string path = SavePath + "\\Save" + buffer + ".tif";
                TifMapGroup[i].Save(path, imgformat);
            }
        }
        #endregion


        #region 批量纵切
        public void CutLon(Form1 Form,string SavePath)
        {
            width = TifMapGroup[0].Width;
            height = TifMapGroup[0].Height;
            System.Drawing.Imaging.ImageFormat imgformat = System.Drawing.Imaging.ImageFormat.Tiff;
            Bitmap[] TifMap = new Bitmap[width];
            Form.ucProcessLineExt1.Value = 0;
            Form.ucProcessLineExt1.MaxValue = width-1;
            for (int i=0;i< width; i++)
            {
                Bitmap pic= new Bitmap(height,num);
                for(int j = 0; j < num; j++)
                {
                    for(int k = 0; k < height; k++)
                    {
                        pic.SetPixel(k,j, TifMapGroup[j].GetPixel(i, k));
                    }
                }
                TifMap[i] = pic;
                string buffer = i.ToString();
                string path = SavePath + "\\Save" + buffer + ".tif";
                pic.Save(path, imgformat);
                Form.ucProcessLineExt1.Value++;
            }

    }
        #endregion

        #region 还原最初

        public void rerurnO(Form1 Form)
        {
            width = bufferMapGroup[0].Width;
            height = bufferMapGroup[0].Height;

            Form.ucProcessLineExt1.Value = 0;
            for(int i = 0; i < num; i++)
            {
                TifMap = new Bitmap(width, height);
                for(int j = 0; j < width; j++)
                {
                    for(int k = 0; k < height; k++)
                    {
                        TifMap.SetPixel(j, k, bufferMapGroup[i].GetPixel(j, k));
                    }
                }
                TifMapGroup[i] = TifMap;
                Form.ucProcessLineExt1.Value = i + 1;
            }

        }
        #endregion
    }

}
