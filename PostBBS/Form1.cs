using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PostBBS
{
    public partial class Form1 : Form
    {
        string postbbsapi = @"http://localhost:27154/bbs/PostBBS";
        string postimgapi = @"http://localhost:27154/bbs/PostImage";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postimgapi);            //实例化WebRequest对象  
            request.KeepAlive = true;
            request.Date = DateTime.Now;
            request.Accept = "*/*";
            //request.ContentType = "text/html";
            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
            request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
            request.Method = "Post";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.ProtocolVersion = HttpVersion.Version10;
            request.Timeout = 30000;

            //using (Stream reqStream = request.GetRequestStream())
            //{
            //    //reqStream.Write(data, 0, data.Length);
            //    //reqStream.Close();
            //}

            var filePath = "D:\\Code\\UnityProgram\\PostBBS\\bin\\Debug\\3718152700_FkJQqiAz_1.mp4";
            //读取file文件
            //FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            //BinaryReader binaryReader = new BinaryReader(fileStream);


            try
            {
                byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                int pos = filePath.LastIndexOf("\\");
                string fileName = filePath.Substring(pos + 1);

                //请求头部信息 
                StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));

                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());

                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                byte[] bArr = new byte[fs.Length];
                fs.Read(bArr, 0, bArr.Length);
                fs.Close();

                Stream postStream = request.GetRequestStream();
                postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                postStream.Write(bArr, 0, bArr.Length);
                postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                postStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("文件传输异常： " + ex.Message);
            }
            finally
            {
                //fileStream.Close();
                //binaryReader.Close();
            }


            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var responsestream = response.GetResponseStream();
            Encoding ec = Encoding.UTF8;
            StreamReader reader = new StreamReader(responsestream, ec);
            var htmlStr = reader.ReadToEnd();
            MessageBox.Show(htmlStr);
        }
    }
}
