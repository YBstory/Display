using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
namespace RadarDisplay
{
    class ImageProcess
    {

        #region 分析地物部分

        int ImgID = 0;

        #region 取五张图
        /// <summary>
        /// 取样本图片
        /// </summary>
        /// <param name="Open"></param>
        /// <returns></returns>
        private int[] Raster(OpenTiff Open)
        {
            int width = Open.TifMapGroup[0].Width;
            int height = Open.TifMapGroup[0].Height;
            int num = Open.num;
            int interval = 1;
            if (num < 5)
            {
                num = Open.num;
            }
            else
            {
                interval = num / 5;
                num = 5;
            }

            int[] buffer = new int[width * height * num];
            int deep = 0;
            for(int k = 0; k < num; k++)
            {
                Bitmap image = Open.TifMapGroup[deep];
                for (int i = 0; i < width; i++)
                {
                    for(int j = 0; j < height; j++)
                    {

                        Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                        int value = 0xFF0000 & imgArgb;
                        value >>= 16;

                        buffer[width * height * k + i * height + j] = value;
                    }
                
                }
                deep = deep + interval;
            }

            return buffer;
        }
        #endregion

        /// <summary>
        /// 根据选区的样本计算阈值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private int CalculateImg(int[] buffer)
        {
            int length = buffer.Length;
            int[] TongJi = new int[256];
            for(int i = 0; i < 255; i++)
            {
                TongJi[i] = 0;
            }

            for(int i=0;i< length; i++)
            {
                TongJi[buffer[i]]++;
            }
            int max = 0;
            int id = -1;
            for (int i = 0; i < 255; i++)
            {
                if (TongJi[i] > max)
                {
                    max = TongJi[i];
                    id = i;
                }
            }

            //Array.Sort(buffer);
            //length = length / 2;
            //return buffer[length];\

            return id;
        }

        /// <summary>
        /// 计算单图阈值，选取显示的左图
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private int CalculateImg(Form1 form)
        {
            Bitmap image;
            image = (Bitmap)form.pictureBox1.Image.Clone();
            int width = image.Width;
            int height = image.Height;

            int[] raster = new int[width * height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                    raster[i * height + j] = 0xFF0000 & imgArgb;
                    raster[i * height + j] >>= 16;
                }
            }

            int[] rasterBac = new int[raster.Length];
            raster.CopyTo(rasterBac, 0);

            Array.Sort(rasterBac);
            int mid = rasterBac[(int)(width * height / 2)];              //获取中值


            return mid;
        }

        private void ImgProcess(int width, int height, int mid, int ImgID, OpenTiff Open, FileStream fs1, FileStream fs2)
        {

            Bitmap image;
            image = Open.TifMapGroup[ImgID];

            int[] raster = new int[width * height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                    raster[i * height + j] = 0xFF0000 & imgArgb;
                    raster[i * height + j] >>= 16;
                }
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int pixvalue = raster[i * height + j];
                    if (pixvalue > (mid + 60))
                    {
                        //获得字节数组
                        string buffer = i.ToString() + "," + j.ToString() + "," + ImgID.ToString() + "\n";
                        byte[] data = System.Text.Encoding.Default.GetBytes(buffer);
                        //开始写入
                        fs1.Write(data, 0, data.Length);

                        image.SetPixel(i, j, Color.FromArgb(255, 255, 0));
                        //image.SetPixel(i, j, Color.FromArgb(255,140,0));



                    }
                    else if (pixvalue < (mid - 50))
                    {

                        //获得字节数组
                        string buffer = i.ToString() + "," + j.ToString() + "," + ImgID.ToString() + "\n";
                        byte[] data = System.Text.Encoding.Default.GetBytes(buffer);
                        //开始写入
                        fs2.Write(data, 0, data.Length);


                        image.SetPixel(i, j, Color.FromArgb(0, 255, 255));
                        //image.SetPixel(i, j, Color.FromArgb(204, 255, 204));
                    }
                    //if (pixvalue < 10)
                    //{
                    //    image.SetPixel(i, j, Color.FromArgb(255, 255, 0));
                    //}
                }
            }

        }

       /// <summary>
       /// 地物分析
       /// </summary>
       /// <param name="form"></param>
       /// <param name="Open"></param>
        public void GetImg(Form1 form, OpenTiff Open)
        {
            ImgID = 0;
            int num = Open.num;
            Open.bufferMapGroup = new Bitmap[num];

            for (int i = 0; i < num; i++)
            {
                Bitmap newmap = new Bitmap(Open.TifMapGroup[i].Width, Open.TifMapGroup[i].Height);
                newmap = (Bitmap)Open.DeepCopyBitmap(Open.TifMapGroup[i]);
                Open.bufferMapGroup[i] = (Bitmap)Open.DeepCopyBitmap(newmap);
                ImgID++;
            }

            //Open.TifMapGroup.CopyTo(Open.bufferMapGroup, 0);

            int[] raster = Raster(Open);
            int mid = CalculateImg(raster);

            int width = Open.TifMapGroup[0].Width;
            int height = Open.TifMapGroup[0].Height;

            FileStream fs1 = new FileStream("C:\\Users\\zyb71\\Desktop\\RadarCode\\RadarDisplay\\bin\\Debug\\k1.txt", FileMode.Create);
            FileStream fs2 = new FileStream("C:\\Users\\zyb71\\Desktop\\RadarCode\\RadarDisplay\\bin\\Debug\\k2.txt", FileMode.Create);



            form.ucProcessLineExt1.Value = 0;
            for (int i = 0; i < Open.num; i++)
            {
                ImgProcess(width, height, mid, i, Open, fs1,fs2);
                form.ucProcessLineExt1.Value = i + 1;
                //form.pictureBox2.Image = Open.bufferMapGroup[i];//
                //form.Refresh();//
            }


            //清空缓冲区、关闭流
            fs1.Flush();
            fs1.Close();
            fs2.Flush();
            fs2.Close();
        }
        #endregion

        #region 影像去噪部分


        /// <summary>
        /// 二维均值滤波
        /// </summary>
        /// <param name="image"></param>
        private void MeanFilter(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            int[] raster = new int[width * height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                    raster[i * height + j] = 0xFF0000 & imgArgb;
                    raster[i * height + j] >>= 16;
                }
            }

            
            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {

                    int buffer = (
                        raster[(i - 1) * height + (j - 1)] + raster[(i) * height + (j - 1)] + raster[(i + 1) * height + (j - 1)]
                        + raster[(i - 1) * height + (j)] + raster[(i) * height + (j)] + raster[(i + 1) * height + (j)]
                        + raster[(i - 1) * height + (j + 1)] + raster[(i) * height + (j + 1)] + raster[(i + 1) * height + (j + 1)]
                        ) / 9;

                    image.SetPixel(i, j, Color.FromArgb(buffer, buffer, buffer));
                }
            }
        }

        /// <summary>
        /// 二维中值滤波
        /// </summary>
        /// <param name="image"></param>
        private void MedianFilter(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            int[] raster = new int[width * height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                    raster[i * height + j] = 0xFF0000 & imgArgb;
                    raster[i * height + j] >>= 16;
                }
            }


            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    int[] buffer = new int[9];
                    int bufferID = 0;

                    for (int ii = i - 1; ii < i + 2; ii++)
                    {
                        for (int jj = j - 1; jj < j + 2; jj++)
                        {
                            buffer[bufferID] = raster[ii * height + jj];
                            bufferID++;
                        }
                    }
                    Array.Sort(buffer);

                    image.SetPixel(i, j, Color.FromArgb(buffer[4], buffer[4], buffer[4]));
                }
            }
        }

        /// <summary>
        /// 均值滤波——分层二维处理
        /// </summary>
        /// <param name="form"></param>
        /// <param name="Open"></param>
        public void MeanFilterStack(Form1 form, OpenTiff Open)
        {
            form.ucProcessLineExt1.Value = 0;
            for(int i = 0; i < Open.num; i++)
            {
                MeanFilter(Open.TifMapGroup[i]);
                form.ucProcessLineExt1.Value = i + 1;
            }
        }

        /// <summary>
        /// 中值滤波——分层二维处理
        /// </summary>
        /// <param name="form"></param>
        /// <param name="Open"></param>
        public void MedianFilterStack(Form1 form, OpenTiff Open)
        {
            form.ucProcessLineExt1.Value = 0;
            for (int i = 0; i < Open.num; i++)
            {
                MedianFilter(Open.TifMapGroup[i]);
                form.ucProcessLineExt1.Value = i + 1;
            }
        }

        /// <summary>
        /// 均值滤波——三维处理
        /// </summary>
        /// <param name="form"></param>
        /// <param name="Open"></param>
        public void MeanFilter3D(Form1 form,OpenTiff Open)
        {
            form.ucProcessLineExt1.Value = 0;
            int width = Open.TifMapGroup[0].Width;
            int height = Open.TifMapGroup[0].Height;
            int num = Open.num;
            int[] raster = new int[width * height*num];


            for (int z = 0; z < num; z++)
            {

                Bitmap TiffBuffer = Open.TifMapGroup[z];

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        Int32 imgArgb = TiffBuffer.GetPixel(i, j).ToArgb();
                        raster[z * height * width + i * height + j] = 0xFF0000 & imgArgb;
                        raster[z * height * width + i * height + j] >>= 16;
                    }
                }

            }


            for (int z = 1; z < num - 1; z++)
            {

                Bitmap TiffBuffer = Open.TifMapGroup[z];

                for (int i = 1; i < width - 1; i++)
                {
                    for (int j = 1; j < height - 1; j++)
                    {

                        int sum = 0;
                        for (int zz = z - 1; zz < z + 2; zz++)
                        {
                            for (int ii = i - 1; ii < i + 2; ii++)
                            {
                                for (int jj = j - 1; jj < j + 2; jj++)
                                {
                                    sum = sum + raster[zz * width * height + ii * height + jj];
                                }
                            }
                        }

                        sum = (int)((double)sum / 27 + 0.5);
                        TiffBuffer.SetPixel(i, j, Color.FromArgb(sum, sum, sum));
                    }
                }
                form.ucProcessLineExt1.Value = z;
            }

            MeanFilter(Open.TifMapGroup[0]);
            MeanFilter(Open.TifMapGroup[num-1]);
        }

        /// <summary>
        /// 中值滤波——三维处理
        /// </summary>
        /// <param name="form"></param>
        /// <param name="Open"></param>
        public void MedianFilter3D(Form1 form, OpenTiff Open)
        {
            form.ucProcessLineExt1.Value = 0;
            int width = Open.TifMapGroup[0].Width;
            int height = Open.TifMapGroup[0].Height;
            int num = Open.num;
            int[] raster = new int[width * height * num];


            for (int z = 0; z < num; z++)
            {

                Bitmap TiffBuffer = Open.TifMapGroup[z];

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        Int32 imgArgb = TiffBuffer.GetPixel(i, j).ToArgb();
                        raster[z * height * width + i * height + j] = 0xFF0000 & imgArgb;
                        raster[z * height * width + i * height + j] >>= 16;
                    }
                }

            }


            for (int z = 1; z < num - 1; z++)
            {

                Bitmap TiffBuffer = Open.TifMapGroup[z];


                for (int i = 1; i < width - 1; i++)
                {
                    for (int j = 1; j < height - 1; j++)
                    {

                        int[] buffer = new int[27];
                        int bufferID = 0;

                        for (int zz = z - 1; zz < z + 2; zz++)
                        {
                            for (int ii = i - 1; ii < i + 2; ii++)
                            {
                                for (int jj = j - 1; jj < j + 2; jj++)
                                {
                                    buffer[bufferID] = raster[zz * width * height + ii * height + jj];
                                    bufferID++;
                                }
                            }
                        }

                        Array.Sort(buffer);
                        int median = buffer[13];
                        TiffBuffer.SetPixel(i, j, Color.FromArgb(median, median, median));
                    }
                }
                form.ucProcessLineExt1.Value = z;
            }

            MedianFilter(Open.TifMapGroup[0]);
            MedianFilter(Open.TifMapGroup[num - 1]);
        }
        #endregion

        #region 直方图均衡化部分

        int[] gray = new int[256]; //记录每个灰度级别下的像素个数
        double[] gray_prob = new double[256];  //记录灰度分布密度
        double[] gray_distribution = new double[256];  //记录累计密度
        int[] gray_equal = new int[256];   //均衡化后的灰度值
 

        int gray_sum = 0;  //像素总数
        /// <summary>
        /// 计算直方图均衡化数组
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private int[] GrayEqual(Bitmap image)
        {
            for(int i = 0; i < 256; i++)
            {
                gray[i] = 0;
                gray_prob[i] = 0;
                gray_distribution[i] = 0;
                gray_equal[i] = 0;
            }

            int width = image.Width;
            int height = image.Height;

            int[] raster = new int[width * height];
            //统计每个灰度下的像素个数
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                    raster[i * height + j] = 0xFF0000 & imgArgb;
                    raster[i * height + j] >>= 16;

                    gray[raster[i * height + j]]++;
                    gray_sum++;
                }
            }

            //统计灰度频率
            for (int i = 0; i < 256; i++)
            {
                gray_prob[i] = ((double)gray[i] / gray_sum);
            }

            //计算累计密度
            gray_distribution[0] = gray_prob[0];
            for (int i = 1; i < 256; i++)
            {
                gray_distribution[i] = gray_distribution[i - 1] + gray_prob[i];
            }

            //重新计算均衡化后的灰度值，四舍五入。参考公式：(N-1)*T+0.5
            for (int i = 0; i < 256; i++)
            {
                gray_equal[i] = (int)(255 * gray_distribution[i] + 0.5);
            }

            return gray_equal;
        }

        private void Replace(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            int[] raster = new int[width * height];
            //统计每个灰度下的像素个数
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                    raster[i * height + j] = 0xFF0000 & imgArgb;
                    raster[i * height + j] >>= 16;

                    int value = gray_equal[raster[i * height + j]];
                    image.SetPixel(i, j, Color.FromArgb(value, value, value));
                }
            }
        }

        public void histogram(OpenTiff Open)
        {

            gray_equal = GrayEqual(Open.TifMapGroup[0]);

            int num = Open.num;

            for(int i = 0; i < num; i++)
            {
                Replace(Open.TifMapGroup[i]);
            }
        }
        #endregion


        #region 灰度拉伸部分
        public void gray_expand(Form1 form,OpenTiff Open)
        {

            double mina = 60, maxa = 195;         
            double minb = 30, maxb = 225;
            int[] raster = Raster(Open);
            //Array.Sort(raster);
            //double min = (double)raster[0];
            //double max = (double)raster[raster.Length - 1];

            //double a = 255 / (max - min);
            //double b = 255 * min / (min - max);

            double a1 = minb / mina;
            double b1 = 0;

            double a2 = (maxb - minb) / (maxa - mina);
            double b2 = (maxa * minb - mina * maxb) / (maxa - mina);

            double a3 = (255 - maxb) / (255 - maxa);
            double b3 = (255 * maxb - maxa * 255) / (255 - maxa);


            int[] replace = new int[256];

            //for(int i = 0; i < min; i++)
            //{
            //    replace[i] = 0;
            //}
            //for(int i = (int)min; i < max+1; i++)
            //{
            //    replace[i] = (int)(a * i + b + 0.5);
            //    if (replace[i] < 0) replace[i] = 0;
            //    if (replace[i] > 255) replace[i] = 255;
            //}
            //for (int i= (int)max + 1; i < 256; i++)
            //{
            //    replace[i] = 255;
            //}

            for(int i=0;i< mina; i++)
            {
                replace[i] = (int)(a1 * i + b1 + 0.5);
                if (replace[i] < 0) replace[i] = 0;
                if (replace[i] > 255) replace[i] = 255;
            }
            for (int i = (int)mina; i < maxa; i++)
            {
                replace[i] = (int)(a2 * i + b2 + 0.5);
                if (replace[i] < 0) replace[i] = 0;
                if (replace[i] > 255) replace[i] = 255;
            }
            for (int i = (int)maxa; i < 256; i++)
            {
                replace[i] = (int)(a3 * i + b3 + 0.5);
                if (replace[i] < 0) replace[i] = 0;
                if (replace[i] > 255) replace[i] = 255;
            }
            FileStream fs = new FileStream("replace.txt", FileMode.Create);
            for (int i = 0; i < 256; i++)
            {

                //获得字节数组
                string buffer = replace[i].ToString() + "\n";
                byte[] data = System.Text.Encoding.Default.GetBytes(buffer);
                //开始写入
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();

            int num = Open.num;
            int width = Open.TifMapGroup[0].Width;
            int height = Open.TifMapGroup[0].Height;
            form.ucProcessLineExt1.Value = 0;
            for (int z = 0; z < num; z++)
            {

                Bitmap TiffBuffer = Open.TifMapGroup[z];

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        Int32 imgArgb = TiffBuffer.GetPixel(i, j).ToArgb();
                        int value = 0xFF0000 & imgArgb;
                        value >>= 16;
                        value = replace[value];
                        TiffBuffer.SetPixel(i, j, Color.FromArgb(value, value, value));
                    }
                }
                form.ucProcessLineExt1.Value = z + 1;
            }
        }
        #endregion


        #region 转换为灰色影像
        private void Convert2Gray(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            int[] raster = new int[width * height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                    raster[i * height + j] = 0xFF0000 & imgArgb;
                    raster[i * height + j] >>= 16;

                    image.SetPixel(i, j, Color.FromArgb(raster[i * height + j], raster[i * height + j], raster[i * height + j]));
                }
            }
        }

        /// <summary>
        /// 转换为灰度影像
        /// </summary>
        public void Convert2GrayStack(Form1 form, OpenTiff Open)
        {
            form.ucProcessLineExt1.Value = 0;
            for (int i = 0; i < Open.num; i++)
            {
                Convert2Gray(Open.TifMapGroup[i]);
                form.ucProcessLineExt1.Value = i + 1;
            }
        }
        #endregion


        private void Convert(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            //统计每个灰度下的像素个数
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Int32 imgArgb = image.GetPixel(i, j).ToArgb();
                    int value = 0xFF0000 & imgArgb;
                    value >>= 16;

                    if (value < 128) value = 255 - value;
                    image.SetPixel(i, j, Color.FromArgb(value, value, value));
                }
            }
        }

        public void Dark2Light(Form1 Form, OpenTiff Open)
        {
            Form.ucProcessLineExt1.Value = 0;

            int num = Open.num;

            for (int i = 0; i < num; i++)
            {
                
                Convert(Open.TifMapGroup[i]);
                Form.ucProcessLineExt1.Value = i + 1;
            }

        }



    }
}
