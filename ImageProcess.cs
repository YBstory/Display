using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
namespace RadarDisplay
{
    class ImageProcess
    {


        #region 图像处理部分
/// <summary>
/// 计算阈值
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

        private void ImgProcess(int width, int height, int mid, int ImgID, OpenTiff Open)
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
                    if (pixvalue > (mid + 50))
                    {
                        image.SetPixel(i, j, Color.FromArgb(255, 255, 0));
                    }
                    else if (pixvalue < (mid - 50))
                    {
                        image.SetPixel(i, j, Color.FromArgb(0, 255, 255));
                    }
                }
            }

            //form.pictureBox2.Image = image;
        }

        public void GetImg(Form1 form, OpenTiff Open)
        {
            int num = Open.num;
            Open.bufferMapGroup = new Bitmap[num];

            for (int i = 0; i < num; i++)
            {
                Bitmap newmap = new Bitmap(Open.TifMapGroup[i].Width, Open.TifMapGroup[i].Height);
                newmap = (Bitmap)Open.DeepCopyBitmap(Open.TifMapGroup[i]);
                Open.bufferMapGroup[i] = (Bitmap)Open.DeepCopyBitmap(newmap);
                //form.pictureBox2.Image = Open.bufferMapGroup[i];//
                //form.Refresh();//
            }

            Open.TifMapGroup.CopyTo(Open.bufferMapGroup, 0);

            int mid = CalculateImg(form);

            int width = Open.bufferMapGroup[0].Width;
            int height = Open.bufferMapGroup[0].Height;

            form.ucProcessLineExt1.Value = 0;
            for (int i = 0; i < Open.num; i++)
            {
                ImgProcess(width, height, mid, i, Open);
                form.ucProcessLineExt1.Value = i + 1;
                //form.pictureBox2.Image = Open.bufferMapGroup[i];//
                //form.Refresh();//
            }

        }
        #endregion
    }
}
