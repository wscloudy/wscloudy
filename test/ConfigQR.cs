using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using wscloudy.QRCodes;

namespace test
{
    public partial class ConfigQR : Form
    {
        public ConfigQR()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PrintQR pr = new PrintQR(textBox1.Text + "\r\n+1\r\n");
            pictureBox1.Image = Image.FromHbitmap(pr.GetQR(200).GetHbitmap());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            textBox1.Text = "";
            Application.DoEvents();
            Application.DoEvents();
            ScanQR sc = new ScanQR();
            string ret = sc.ScanScreenQRCode();
            if (ret != "")
            {
                textBox1.Text = ret;
            }
            Application.DoEvents();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                string path = AppDomain.CurrentDomain.BaseDirectory + "QR_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp";
                bmp.Save(path);
                MessageBox.Show("保存成功：" + path);
            }
            catch
            {
                MessageBox.Show("保存失败");
            }
        }

        private void ConfigQR_Load(object sender, EventArgs e)
        {

        }
    }
}
