using Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace UnityProgram
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            using (var entity = new UnityEntities())
            {
                var ad = entity.Ad.FirstOrDefault();
                if (ad != null) 
                {
                    textBox1.Text = ad.Name;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = "tv31.32";

            string numstr = "";

            foreach (var c in url)
            {
                int num;

                if (int.TryParse(c.ToString(), out num))
                {
                    numstr += num.ToString();
                }
                else
                {
                    if (numstr.Length > 0)
                    {
                        break;
                    }
                }
            }

            MessageBox.Show(numstr.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var s = PingIpOrDomainName("www.coupang.com");

            MessageBox.Show(s.ToString());
        }



        public static bool PingIpOrDomainName(string strIpOrDName)
        {
            try
            {
                Ping objPingSender = new Ping();

                PingOptions objPinOptions = new PingOptions();

                objPinOptions.DontFragment = true;

                string data = "";

                byte[] buffer = Encoding.UTF8.GetBytes(data);

                int intTimeout = 120;

                PingReply objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);

                string strInfo = objPinReply.Status.ToString();

                if (strInfo == "Success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception)
            {
                return false;
            }

        }

        private void btnencode_Click(object sender, EventArgs e)
        {
            var encode = txtencode.Text;

            if (string.IsNullOrWhiteSpace(encode)) return;
            var idn = new IdnMapping();
            var url = idn.GetAscii(encode);
            txtdecode.Text = url;
        }

        private void btndecode_Click(object sender, EventArgs e)
        {

            var decode = txtdecode.Text;

            if (string.IsNullOrWhiteSpace(decode)) return;
            var idn = new IdnMapping();
            var url = idn.GetUnicode(decode);
            txtencode.Text = url;
        }
    }
}
