using System;
using System.Drawing;
using ZXing.QrCode.Internal;

/// <summary>
/// 生成二维码
/// </summary>
namespace wscloudy.QRCodes
{
    public class PrintQR
    {
        string buff;

        /// <summary>
        /// 字符串转化为二维码
        /// </summary>
        /// <param name="t_buf">需要转化的字符串</param>
        public PrintQR(string t_buf)
        {
            buff = t_buf;
        }

        private bool CheckBuff()
        {
            if (buff == null || buff == "")
                return false;
            else
                return true;
        }

        public void SetQRString(string t_buf)
        {
            buff = t_buf;
        }

        public static int GetDpiMul()
        {
            int dpi;
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpi = (int)graphics.DpiX;
            }
            return (dpi * 4 + 48) / 96;
        }

        /// <summary>
        /// 获取二维码位图
        /// </summary>
        /// <param name="ngnl">原始logo</param>
        /// <param name="width">二维码显示多大，默认350，数字越大则图越大</param>
        /// <returns></returns>
        public Bitmap GetQR(int width = 350, Bitmap ngnl = null)
        {
            if (buff == null || buff == "") return null;
            if (ngnl == null)
            {
                ngnl = new Bitmap(10, 10);
            }
            int dpi_mul = GetDpiMul();
            width = width * dpi_mul / 4;
            string qrText = buff;
            QRCode code = ZXing.QrCode.Internal.Encoder.encode(qrText, ErrorCorrectionLevel.M);
            ByteMatrix m = code.Matrix;
            int blockSize = Math.Max(width / (m.Width + 2), 1);
            Bitmap drawArea = new Bitmap(((m.Width + 2) * blockSize), ((m.Height + 2) * blockSize));
            using (Graphics g = Graphics.FromImage(drawArea))
            {
                g.Clear(Color.White);
                using (Brush b = new SolidBrush(Color.Black))
                {
                    for (int row = 0; row < m.Width; row++)
                    {
                        for (int col = 0; col < m.Height; col++)
                        {
                            if (m[row, col] != 0)
                            {
                                g.FillRectangle(b, blockSize * (row + 1), blockSize * (col + 1),
                                    blockSize, blockSize);
                            }
                        }
                    }
                }
                int div = 13, div_l = 5, div_r = 8;
                int l = (m.Width * div_l + div - 1) / div * blockSize, r = (m.Width * div_r + div - 1) / div * blockSize;
                g.DrawImage(ngnl, new Rectangle(l + blockSize, l + blockSize, r - l, r - l));
            }
            return drawArea;
        }
    }
}
