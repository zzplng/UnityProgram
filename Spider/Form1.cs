using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spider
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Run();

            //string u = "https://livescore.co.kr/bbs/board.php?bo_table=share&wr_id=2068569&page=1";
            //int startIndex = u.IndexOf("&wr_id=") + 7;
            //int sublength = u.IndexOf("&page=") - startIndex;
            //string wr_id = u.Substring(startIndex, sublength);
            //MessageBox.Show(wr_id);
        }


        public void Run()
        {

            //设置保存目录
            string path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "wwwroot");
            //不存在目录则创建
            if (!Directory.Exists(path))
            {
                //创建目录
                Directory.CreateDirectory(path);
            }

            for (int i = 0; i < 222; i++)
            {
                try
                {
                    string src = string.Format("https://www.cool112.com/images/lv/LV_{0}.gif", i);
                    string filename = "LV_" + i + ".gif";
                    string filepath = Path.Combine(path, filename);

                    WebRequest request = WebRequest.Create(src);//图片src内容
                    WebResponse response = request.GetResponse();
                    //文件流获取图片操作
                    Stream reader = response.GetResponseStream();
                    FileStream writer = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                    byte[] buff = new byte[512];
                    int c = 0;                                           //实际读取的字节数   
                    while ((c = reader.Read(buff, 0, buff.Length)) > 0)
                    {
                        writer.Write(buff, 0, c);
                    }
                    //释放资源
                    writer.Close();
                    writer.Dispose();
                    reader.Close();
                    reader.Dispose();
                    response.Close();
                }
                catch { }

            }
        }

        public void Run1()
        {
            string src = string.Format("https://a77.moviejoa.xyz/");

            WebRequest request = WebRequest.Create(src);
            
            WebResponse response = request.GetResponse();

            //文件流获取图片操作
            Stream reader = response.GetResponseStream();
            var re = new StreamReader(reader);
            string str = re.ReadToEnd();
            MessageBox.Show(str);
        }
    }
}
