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
using wscloudy.NetTool;

namespace test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private object Fun()
        {
            System.Threading.Thread.Sleep(3000);
            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                FuncTimeout.EventNeedRun action = delegate (object[] param)
                {
                    //调用自定义函数
                    return Fun();
                };
                FuncTimeout ft = new FuncTimeout(action, 2000);
                var result = ft.doAction("1", "2", DateTime.Now);
                if (result != null)
                {
                    MessageBox.Show("normal end");
                }
                else
                {
                    MessageBox.Show("bad end");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(NetTools.GetHostNameByIp("172.16.20.63"));
        }
    }
}
