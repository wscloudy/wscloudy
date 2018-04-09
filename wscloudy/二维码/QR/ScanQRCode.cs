using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

/// <summary>
/// 扫描屏幕二维码
/// </summary>
namespace wscloudy.QRCodes
{
    public class ScanQR
    {
        public enum DeviceCap
        {
            DESKTOPVERTRES = 117,
            DESKTOPHORZRES = 118,
        }

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public static Point GetScreenPhysicalSize()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr desktop = g.GetHdc();
                int PhysicalScreenWidth = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPHORZRES);
                int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

                return new Point(PhysicalScreenWidth, PhysicalScreenHeight);
            }
        }

        private Rectangle GetScanRect(int width, int height, int index, out double stretch)
        {
            stretch = 1;
            if (index < 5)
            {
                const int div = 5;
                int w = width * 3 / div;
                int h = height * 3 / div;
                Point[] pt = new Point[5] {
                    new Point(1, 1),

                    new Point(0, 0),
                    new Point(0, 2),
                    new Point(2, 0),
                    new Point(2, 2),
                };
                return new Rectangle(pt[index].X * width / div, pt[index].Y * height / div, w, h);
            }
            {
                const int base_index = 5;
                if (index < base_index + 6)
                {
                    double[] s = new double[] {
                        1,
                        2,
                        3,
                        4,
                        6,
                        8
                    };
                    stretch = 1 / s[index - base_index];
                    return new Rectangle(0, 0, width, height);
                }
            }
            {
                const int base_index = 11;
                if (index < base_index + 8)
                {
                    const int hdiv = 7;
                    const int vdiv = 5;
                    int w = width * 3 / hdiv;
                    int h = height * 3 / vdiv;
                    Point[] pt = new Point[8] {
                        new Point(1, 1),
                        new Point(3, 1),

                        new Point(0, 0),
                        new Point(0, 2),

                        new Point(2, 0),
                        new Point(2, 2),

                        new Point(4, 0),
                        new Point(4, 2),
                    };
                    return new Rectangle(pt[index - base_index].X * width / hdiv, pt[index - base_index].Y * height / vdiv, w, h);
                }
            }
            return new Rectangle(0, 0, 0, 0);
        }

        private bool ScanQRCode(Screen screen, Bitmap fullImage, Rectangle cropRect, out string url, out Rectangle rect)
        {
            using (Bitmap target = new Bitmap(cropRect.Width, cropRect.Height))
            {
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(fullImage, new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                                    cropRect,
                                    GraphicsUnit.Pixel);
                }
                var source = new BitmapLuminanceSource(target);
                var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                QRCodeReader reader = new QRCodeReader();
                var result = reader.decode(bitmap);
                if (result != null)
                {
                    url = result.Text;
                    double minX = Int32.MaxValue, minY = Int32.MaxValue, maxX = 0, maxY = 0;
                    foreach (ResultPoint point in result.ResultPoints)
                    {
                        minX = Math.Min(minX, point.X);
                        minY = Math.Min(minY, point.Y);
                        maxX = Math.Max(maxX, point.X);
                        maxY = Math.Max(maxY, point.Y);
                    }
                    //rect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
                    rect = new Rectangle(cropRect.Left + (int)minX, cropRect.Top + (int)minY, (int)(maxX - minX), (int)(maxY - minY));
                    return true;
                }
            }
            url = "";
            rect = new Rectangle();
            return false;
        }

        private bool ScanQRCodeStretch(Screen screen, Bitmap fullImage, Rectangle cropRect, double mul, out string url, out Rectangle rect)
        {
            using (Bitmap target = new Bitmap((int)(cropRect.Width * mul), (int)(cropRect.Height * mul)))
            {
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(fullImage, new Rectangle(0, 0, target.Width, target.Height),
                                    cropRect,
                                    GraphicsUnit.Pixel);
                }
                var source = new BitmapLuminanceSource(target);
                var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                QRCodeReader reader = new QRCodeReader();
                var result = reader.decode(bitmap);
                if (result != null)
                {
                    url = result.Text;
                    double minX = Int32.MaxValue, minY = Int32.MaxValue, maxX = 0, maxY = 0;
                    foreach (ResultPoint point in result.ResultPoints)
                    {
                        minX = Math.Min(minX, point.X);
                        minY = Math.Min(minY, point.Y);
                        maxX = Math.Max(maxX, point.X);
                        maxY = Math.Max(maxY, point.Y);
                    }
                    //rect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
                    rect = new Rectangle(cropRect.Left + (int)(minX / mul), cropRect.Top + (int)(minY / mul), (int)((maxX - minX) / mul), (int)((maxY - minY) / mul));
                    return true;
                }
            }
            url = "";
            rect = new Rectangle();
            return false;
        }

        /// <summary>
        /// 屏幕上二维码转换为字符串
        /// </summary>
        /// <returns>二维码转化后的字符串</returns>
        public string ScanScreenQRCode()
        {
            Thread.Sleep(100);
            foreach (Screen screen in Screen.AllScreens)
            {
                Point screen_size = GetScreenPhysicalSize();
                using (Bitmap fullImage = new Bitmap(screen_size.X,
                                                screen_size.Y))
                {
                    using (Graphics g = Graphics.FromImage(fullImage))
                    {
                        g.CopyFromScreen(screen.Bounds.X,
                                         screen.Bounds.Y,
                                         0, 0,
                                         fullImage.Size,
                                         CopyPixelOperation.SourceCopy);
                    }
                    bool decode_fail = false;
                    for (int i = 0; i < 100; i++)
                    {
                        double stretch;
                        Rectangle cropRect = GetScanRect(fullImage.Width, fullImage.Height, i, out stretch);
                        if (cropRect.Width == 0)
                            break;

                        string buf;
                        Rectangle rect;
                        if (stretch == 1 ? ScanQRCode(screen, fullImage, cropRect, out buf, out rect) : ScanQRCodeStretch(screen, fullImage, cropRect, stretch, out buf, out rect))
                        {
                            
                            //var success = controller.AddServerBySSURL(url);
                            //QRCodeSplashForm splash = new QRCodeSplashForm();
                            //if (success)
                            //{
                            //    splash.FormClosed += splash_FormClosed;
                            //}
                            //else if (!ss_only)
                            //{
                            //    _urlToOpen = url;
                            //    //if (url.StartsWith("http://") || url.StartsWith("https://"))
                            //    //    splash.FormClosed += openURLFromQRCode;
                            //    //else
                            //    splash.FormClosed += showURLFromQRCode;
                            //}
                            //else
                            //{
                            //    decode_fail = true;
                            //    continue;
                            //}
                            //splash.Location = new Point(screen.Bounds.X, screen.Bounds.Y);
                            //double dpi = Screen.PrimaryScreen.Bounds.Width / (double)screen_size.X;
                            //splash.TargetRect = new Rectangle(
                            //    (int)(rect.Left * dpi + screen.Bounds.X),
                            //    (int)(rect.Top * dpi + screen.Bounds.Y),
                            //    (int)(rect.Width * dpi),
                            //    (int)(rect.Height * dpi));
                            //splash.Size = new Size(fullImage.Width, fullImage.Height);
                            //splash.Show();
                            return buf;
                        }
                    }
                    if (decode_fail)
                    {
                        MessageBox.Show("解析失败");
                        return "";
                    }
                }
            }
            MessageBox.Show("未找到二维码");
            return "";
        }
    }
}
