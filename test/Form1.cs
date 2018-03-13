using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using wscloudy.Threads;

namespace test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static void Fun()
        {
            System.Threading.Thread.Sleep(1000);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                ActionTimeout.CallWithTimeout(Fun, 500);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}
