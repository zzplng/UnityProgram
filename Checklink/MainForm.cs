using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Checklink
{
    public partial class MainForm : Form
    {
        public bool isStop = false;
        public bool noupdate = false;
        public delegate void MyInvoke(string txt, bool is_update);
        //SqlConnection conn;
        //System.Threading.Timer timer;
        string api, getapi;

        public MainForm()
        {
            InitializeComponent();

        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtapi.Text) || string.IsNullOrEmpty(txtgetapi.Text))
            {
                MessageBox.Show("请输入API");
                return;
            }
            api = txtapi.Text + "?id={0}&isalive={1}&href={2}";
            getapi = txtgetapi.Text;

            isStop = false;
            //noupdate = chbNoupdate.Checked;
            richTextBox1.Text = "";
            Thread thread = new Thread(new ThreadStart(Run));
            thread.IsBackground = true;
            thread.Start();
            btnStart.Enabled = false;


            //isStop = false;
            //btnStart.Enabled = false;
            //richTextBox1.Text = "";
            //timer = new System.Threading.Timer(Thread_Timer_Method, null, 1000, 3600000 * 2);
        }

        void Thread_Timer_Method(object o)
        {
            //MessageBox.Show("1");
            Run();
        }


        public void Run()
        {
            try
            {
                //conn = new SqlConnection();
                //string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MainForm.Properties.Settings.ConnectionString"].ConnectionString;
                //conn.ConnectionString = connStr;
                //conn.Open();

                //var sql = "select * from Ad";
                //SqlCommand cmd = new SqlCommand(sql, conn);
                //SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                //DataTable dataTable = new DataTable();
                //adapter.Fill(dataTable);

                var jsonstr = Getlist();
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdInfo>>(jsonstr);

                //var list = new List<AdInfo>();
                //var info = new AdInfo();
                //info.Name = "123";
                //info.Id = "999999";
                //info.Href = "https://a77.moviejoa.xyz/";
                //list.Add(info);

                System.Net.ServicePointManager.DefaultConnectionLimit = 512;
                System.GC.Collect();

                while (!isStop)
                {
                    WriteLog("暂停30min", false);
                    Thread.Sleep(1800000);//间隔一小时
                    WriteLog("", true);
                    WriteLog("开始", false);
                    foreach (var item in list)
                    {
                        Thread.Sleep(3000);

                        if (isStop) break;
                        var id = item.Id;
                        var name = item.Name;
                        var href = item.Href;
                        var checkrule = item.CheckRule;
                        WriteLog(name + "\t" + href + "\t开始", false);
                        Thread.Sleep(2000);


                        try
                        {
                            if (string.IsNullOrEmpty(href))
                            {
                                WriteLog(name + "\t" + href + "\t失效", false);
                                continue;
                            }

                            string[] famoussite = { "netflix.com", "youtube.com" };
                            bool isfamous = false;
                            foreach(var f in famoussite)
                            {
                                if (href.Contains(f))
                                {
                                    isfamous = true;
                                    if (ChecklinkAPI(int.Parse(id), true, ""))
                                    {
                                        WriteLog(name + "\t" + href + "\t正常访问", false);
                                    }
                                    else
                                    {
                                        WriteLog(name + "\t" + href + "\t正常访问,状态更新失败", false);
                                    }
                                }
                            }
                            if (isfamous) continue;

                            var ohref = href;
                            href = Punycode(href);

                            if (ohref.LastIndexOf("/") > 8)
                            {
                                var ishttps = ohref.StartsWith("https");
                                var ourl = ohref.Substring(ohref.IndexOf("://") + 3);
                                var index = ourl.IndexOf("/");
                                var str = ourl.Substring(index);
                                if (str.Length > 2)
                                {
                                    //带参数
                                    var subhref = ishttps ? "https://" + ourl.Substring(0, index) : "http://" + ourl.Substring(0, index);
                                    href = Punycode(subhref) + str;
                                }
                            }


                            // 判断韩语
                            //if (System.Text.RegularExpressions.Regex.IsMatch(href, @"^[\uac00-\ud7ff]+$"))
                            //{
                            //    href = Punycode(href);
                            //}
                            //href = "https://newtoki301.com/";

                            ServicePointManager.Expect100Continue = false;
                            ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            //ServicePointManager.CheckCertificateRevocationList = false;
                            ServicePointManager.DefaultConnectionLimit = 512;

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(href);            //实例化WebRequest对象  
                            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                            request.KeepAlive = false;
                            request.Date = DateTime.Now;
                            request.Accept = "*/*";
                            request.ContentType = "text/html";
                            request.Method = "GET";
                            //request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
                            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                            request.ProtocolVersion = HttpVersion.Version10;
                            request.AllowAutoRedirect = true;
                            //request.UseDefaultCredentials = true;
                            request.Timeout = 120000;

                            //request.UseDefaultCredentials = true;
                            //request.ClientCertificates.Add(X509Certificate.CreateFromCertFile("c:\\motor.https.pem.cer"));


                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();           //创建WebResponse对象
                            var ruri = response.ResponseUri.ToString();
                            if (ruri.Replace("/", "") != href.Replace("/", ""))
                            {
                                string updatestr = ", 链接已更新";
                                if (response.ResponseUri.Host == request.RequestUri.Host)
                                {
                                    ruri = "";
                                    updatestr = "";
                                }

                                if (ruri.StartsWith("http://xn--") || ruri.StartsWith("https://xn--"))
                                {
                                    ruri = "";
                                    updatestr = "";
                                }

                                if (checkrule == 1)
                                {
                                    ruri = "";
                                    updatestr = "";
                                }

                                if (ChecklinkAPI(int.Parse(id), true, ruri))
                                {
                                    WriteLog(name + "\t" + href + "\t失效,自动跳转连接 " + ruri + "--成功" + updatestr, false);
                                }
                                else
                                {
                                    WriteLog(name + "\t" + href + "\t失效,自动跳转连接 " + ruri + "--成功,更新失败", false);
                                }
                                //WriteLog(name + "\t" + href + "\t失效,自动跳转连接 " + ruri, false);
                                //var updatesql = "update Ad set Href=N'" + ruri + "',IsAlive=1 where Id='" + id + "'";
                                //SqlCommand updatecmd = new SqlCommand(updatesql, conn);
                                //updatecmd.ExecuteNonQuery();
                            }
                            else
                            {
                                if (ChecklinkAPI(int.Parse(id), true, ""))
                                {
                                    WriteLog(name + "\t" + href + "\t正常访问", false);
                                }
                                else
                                {
                                    WriteLog(name + "\t" + href + "\t正常访问,状态更新失败", false);
                                }
                                //WriteLog(name + "\t" + href + "\t正常访问", false);
                                //var updatesql = "update Ad set IsAlive=1 where Id='" + id + "'";
                                //SqlCommand updatecmd = new SqlCommand(updatesql, conn);
                                //updatecmd.ExecuteNonQuery();
                            }


                            //Stream datastream = response.GetResponseStream();       //创建流对象  
                            //Encoding ec = Encoding.UTF8;
                            //StreamReader reader = new StreamReader(datastream, ec);
                            //var htmlStr = reader.ReadToEnd();                           //读取数据


                            //reader.Close();
                            //datastream.Close();
                            request.Abort();
                            response.Close();
                        }
                        catch (WebException ex)
                        {
                            //if (ex.Message.Contains("接收时发生错误"))
                            //    continue;

                            try { 
                            var response = ex.Response as HttpWebResponse;
                            if (response != null)
                            {
                                var stauscode = (int)response.StatusCode;
                                if (stauscode == 999)
                                {
                                    if (ChecklinkAPI(int.Parse(id), true, ""))
                                    {
                                        WriteLog(name + "\t" + href + "\t视为正常(999)", false);
                                    }
                                    else
                                    {
                                        WriteLog(name + "\t" + href + "\t视为正常(999),状态更新失败", false);
                                    }
                                    continue;
                                }
                            }

                            if (ChecklinkAPI(int.Parse(id), false, ""))
                            {
                                WriteLog(name + "\t" + href + "\t失效," + ex.Message, false);
                            }
                            else
                            {
                                WriteLog(name + "\t" + href + "\t失效,状态更新失败," + ex.Message, false);
                            }
                            //WriteLog(name + "\t" + href + "\t失效," + ex.Message, false);

                            //var updatesql = "update Ad set IsAlive=0 where Id='" + id + "'";
                            //SqlCommand updatecmd = new SqlCommand(updatesql, conn);
                            //updatecmd.ExecuteNonQuery();

                            bool issucess;
                            CheckMore(id, name, href, checkrule, out issucess);


                            ///Ping
                            //if (!issucess)
                            //{
                            //    WriteLog(name + "\t" + href + "\t正在尝试使用PING...", false);
                            //    string url = href.Replace("http://", "").Replace("https://", "");
                            //    string urlforping = "";
                            //    if (url.IndexOf("/") > 0)
                            //    {
                            //        urlforping = url.Substring(0, url.IndexOf("/"));
                            //    }
                            //    else
                            //    {
                            //        urlforping = url;
                            //    }
                            //    bool status = false;
                            //    for (int i = 0; i < 10; i++)
                            //    {
                            //        status = PingIpOrDomainName(urlforping);
                            //        if (status) break;
                            //    }
                            //    if (status)
                            //    {
                            //        WriteLog(name + "\t" + href + "\tPING成功", false);
                            //        ChecklinkAPI(int.Parse(id), true, "");
                            //    }
                            //    else
                            //    {
                            //        WriteLog(name + "\t" + href + "\tPING失败", false);
                            //        bool status11 = false;
                            //        CheckAgain11(id, name, href, checkrule, out status11);
                            //        if(!status11) CheckAgain13(id, name, href, checkrule);
                            //    }
                            //}
                            if (!issucess) 
                            { 
							    bool status11 = false, status13 = false, status10 = false, statusssl = false;
							    CheckAgain11(id, name, href, checkrule, out status11);
							    if (!status11) CheckAgain13(id, name, href, checkrule, out status13);
							    if (!status13) CheckAgain10(id, name, href, checkrule, out status10);
                                if (!status10) CheckAgainSSL(id, name, href, checkrule, out statusssl) ;
                            }


								System.GC.Collect();
                            }
                            catch (Exception e)
                            {
                                WriteLog(e.Message, false);
                            }

                            //HttpWebResponse response = ex.Response as HttpWebResponse;
                            //if (response.StatusCode != HttpStatusCode.Forbidden)
                            //{
                            //    throw;
                            //}
                            //Uri uri = new Uri(response.ResponseUri.ToString());
                        }
                    }

                }

                WriteLog("操作已完成", false);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, false);
            }
            finally
            {
                //conn.Close();
                //conn.Dispose();
            }
        }



        public void CheckMore(string id, string name, string href, int checkrule, out bool sucess)
        {
            string numstr = "";
            sucess = false;

            foreach (var c in href)
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

            if (numstr.Length == 0)
            {
                return;
            }


            WriteLog(name + "\t" + href + "\t链接含数字 检查中...", false);
            Thread.Sleep(2000);

            int changenum = int.Parse(numstr);

            for(int i = 1; i <= 3; i++) 
            {
                try
                {
                    var changednum = changenum + i;
                    var url = href.Replace(numstr, changednum.ToString());

                    WriteLog(name + "\t" + url + "\t链接含数字 检查中...", false);
                    Thread.Sleep(2000);

                    url = Punycode(url);


                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);            //实例化WebRequest对象  
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                    request.KeepAlive = false;
                    request.Date = DateTime.Now;
                    request.Accept = "*/*";
                    request.ContentType = "text/html";
                    request.Method = "GET";
                    //request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
                    request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    request.ProtocolVersion = HttpVersion.Version10;
                    request.UseDefaultCredentials = true;

                    //request.Timeout = 5000;

                    //ServicePointManager.Expect100Continue = false;
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();           //创建WebResponse对象
                    var ruri = response.ResponseUri.ToString();
                    if (ruri.Replace("/", "") != url.Replace("/", ""))
                    {
                        string updatestr = ", 链接已更新";
                        if (response.ResponseUri.Host == request.RequestUri.Host)
                        {
                            ruri = "";
                            updatestr = "";
                        }
                        if (ruri.StartsWith("http://xn--") || ruri.StartsWith("https://xn--"))
                        {
                            ruri = "";
                            updatestr = "";
                        }
                        if (checkrule == 1)
                        {
                            ruri = "";
                            updatestr = "";
                        }

                        if (ChecklinkAPI(int.Parse(id), true, ruri))
                        {
                            WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功" + updatestr, false);
                            sucess = true;
                        }
                        else
                        {
                            WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功,更新失败", false);
                            sucess = true;
                        }
                        //WriteLog(name + "\t" + url + "\t失效,自动跳转连接 " + ruri + "--成功,链接已更新", false);
                        //var updatesql = "update Ad set Href=N'" + ruri + "',IsAlive=1 where Id='" + id + "'";
                        //SqlCommand updatecmd = new SqlCommand(updatesql, conn);
                        //updatecmd.ExecuteNonQuery();
                    }
                    else
                    {
                        string updatestr = ", 链接已更新";
                        if (url.StartsWith("http://xn--") || url.StartsWith("https://xn--"))
                        {
                            url = "";
                            updatestr = "";
                        }
                        if (checkrule == 1)
                        {
                            url = "";
                            updatestr = "";
                        }

                        if (ChecklinkAPI(int.Parse(id), true, url))
                        {
                            WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功" + updatestr, false);
                            sucess = true;
                        }
                        else
                        {
                            WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功,更新失败", false);
                            sucess = true;
                        }
                        //WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功,链接已更新", false);
                        //var updatesql = "update Ad set Href=N'" + ruri + "',IsAlive=1 where Id='" + id + "'";
                        //SqlCommand updatecmd = new SqlCommand(updatesql, conn);
                        //updatecmd.ExecuteNonQuery();
                    }

                    request.Abort();
                    response.Close();
                }
                catch (Exception ex)
                {
                    WriteLog(ex.Message, false);
                }

            }

        }


		public void CheckAgainSSL(string id, string name, string href, int checkrule, out bool status)
		{
			WriteLog(name + "\t" + href + "\t更换协议请求ssl 检查中...", false);
			Thread.Sleep(2000);
			status = false;
			try
			{
				var url = href;
				Thread.Sleep(2000);

				url = Punycode(url);

				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);            //实例化WebRequest对象  
				request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
				request.KeepAlive = false;
				request.Date = DateTime.Now;
				request.Accept = "*/*";
				request.ContentType = "text/html";
				request.Method = "GET";
				//request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
				request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
				request.ProtocolVersion = HttpVersion.Version10;
				request.UseDefaultCredentials = true;

				//request.Timeout = 5000;

				//ServicePointManager.Expect100Continue = false;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();           //创建WebResponse对象
				var ruri = response.ResponseUri.ToString();
				if (ruri.Replace("/", "") != url.Replace("/", ""))
				{
					status = true;
					string updatestr = ", 链接已更新";
					if (response.ResponseUri.Host == request.RequestUri.Host)
					{
						ruri = "";
						updatestr = "";
					}
					if (ruri.StartsWith("http://xn--") || ruri.StartsWith("https://xn--"))
					{
						ruri = "";
						updatestr = "";
					}
					if (checkrule == 1)
					{
						ruri = "";
						updatestr = "";
					}

					if (ChecklinkAPI(int.Parse(id), true, ruri))
					{
						WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功" + updatestr, false);
					}
					else
					{
						WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功,更新失败", false);
					}
				}
				else
				{
					status = true;
					string updatestr = ", 链接已更新";
					if (url.StartsWith("http://xn--") || url.StartsWith("https://xn--"))
					{
						url = "";
						updatestr = "";
					}
					if (checkrule == 1)
					{
						url = "";
						updatestr = "";
					}

					if (ChecklinkAPI(int.Parse(id), true, url))
					{
						WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功" + updatestr, false);
					}
					else
					{
						WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功,更新失败", false);
					}
				}

				request.Abort();
				response.Close();
			}
			catch (Exception ex)
			{
				WriteLog(ex.Message, false);
			}


		}



		public void CheckAgain10(string id, string name, string href, int checkrule, out bool status)
		{
			WriteLog(name + "\t" + href + "\t更换协议请求1.0 检查中...", false);
			Thread.Sleep(2000);
			status = false;
			try
			{
				var url = href;
				Thread.Sleep(2000);

				url = Punycode(url);

				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);            //实例化WebRequest对象  
				request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
				request.KeepAlive = false;
				request.Date = DateTime.Now;
				request.Accept = "*/*";
				request.ContentType = "text/html";
				request.Method = "GET";
				//request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
				request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
				request.ProtocolVersion = HttpVersion.Version10;
				request.UseDefaultCredentials = true;

				//request.Timeout = 5000;

				//ServicePointManager.Expect100Continue = false;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();           //创建WebResponse对象
				var ruri = response.ResponseUri.ToString();
				if (ruri.Replace("/", "") != url.Replace("/", ""))
				{
					status = true;
					string updatestr = ", 链接已更新";
					if (response.ResponseUri.Host == request.RequestUri.Host)
					{
						ruri = "";
						updatestr = "";
					}
					if (ruri.StartsWith("http://xn--") || ruri.StartsWith("https://xn--"))
					{
						ruri = "";
						updatestr = "";
					}
					if (checkrule == 1)
					{
						ruri = "";
						updatestr = "";
					}

					if (ChecklinkAPI(int.Parse(id), true, ruri))
					{
						WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功" + updatestr, false);
					}
					else
					{
						WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功,更新失败", false);
					}
				}
				else
				{
					status = true;
					string updatestr = ", 链接已更新";
					if (url.StartsWith("http://xn--") || url.StartsWith("https://xn--"))
					{
						url = "";
						updatestr = "";
					}
					if (checkrule == 1)
					{
						url = "";
						updatestr = "";
					}

					if (ChecklinkAPI(int.Parse(id), true, url))
					{
						WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功" + updatestr, false);
					}
					else
					{
						WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功,更新失败", false);
					}
				}

				request.Abort();
				response.Close();
			}
			catch (Exception ex)
			{
				WriteLog(ex.Message, false);
			}


		}



		public void CheckAgain11(string id, string name, string href, int checkrule, out bool status)
        {
            WriteLog(name + "\t" + href + "\t更换协议请求1.1 检查中...", false);
            Thread.Sleep(2000);
            status = false;
            try
            {
                var url = href;
                Thread.Sleep(2000);

                url = Punycode(url);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);            //实例化WebRequest对象  
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                request.KeepAlive = false;
                request.Date = DateTime.Now;
                request.Accept = "*/*";
                request.ContentType = "text/html";
                request.Method = "GET";
                //request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.ProtocolVersion = HttpVersion.Version10;
                request.UseDefaultCredentials = true;

                //request.Timeout = 5000;

                //ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();           //创建WebResponse对象
                var ruri = response.ResponseUri.ToString();
                if (ruri.Replace("/", "") != url.Replace("/", ""))
                {
                    status = true;
                    string updatestr = ", 链接已更新";
                    if (response.ResponseUri.Host == request.RequestUri.Host)
                    {
                        ruri = "";
                        updatestr = "";
                    }
                    if (ruri.StartsWith("http://xn--") || ruri.StartsWith("https://xn--"))
                    {
                        ruri = "";
                        updatestr = "";
                    }
                    if (checkrule == 1)
                    {
                        ruri = "";
                        updatestr = "";
                    }

                    if (ChecklinkAPI(int.Parse(id), true, ruri))
                    {
                        WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功" + updatestr, false);
                    }
                    else
                    {
                        WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功,更新失败", false);
                    }
                }
                else
                {
                    status = true;
                    string updatestr = ", 链接已更新";
                    if (url.StartsWith("http://xn--") || url.StartsWith("https://xn--"))
                    {
                        url = "";
                        updatestr = "";
                    }
                    if (checkrule == 1)
                    {
                        url = "";
                        updatestr = "";
                    }

                    if (ChecklinkAPI(int.Parse(id), true, url))
                    {
                        WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功" + updatestr, false);
                    }
                    else
                    {
                        WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功,更新失败", false);
                    }
                }

                request.Abort();
                response.Close();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, false);
            }


        }


        public void CheckAgain13(string id, string name, string href, int checkrule, out bool status)
        {
            WriteLog(name + "\t" + href + "\t更换协议请求1.3 检查中...", false);
            Thread.Sleep(2000);
			status = false;

			try
            {
                var url = href;
                Thread.Sleep(2000);

                url = Punycode(url);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);            //实例化WebRequest对象  
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
                request.KeepAlive = false;
                request.Date = DateTime.Now;
                request.Accept = "*/*";
                request.ContentType = "text/html";
                request.Method = "GET";
                //request.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.ProtocolVersion = HttpVersion.Version10;
                request.UseDefaultCredentials = true;

                //request.Timeout = 5000;

                //ServicePointManager.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)0x3000;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();           //创建WebResponse对象
                var ruri = response.ResponseUri.ToString();
                if (ruri.Replace("/", "") != url.Replace("/", ""))
				{
					status = true;
					string updatestr = ", 链接已更新";
                    if (response.ResponseUri.Host == request.RequestUri.Host)
                    {
                        ruri = "";
                        updatestr = "";
                    }
                    if (ruri.StartsWith("http://xn--") || ruri.StartsWith("https://xn--"))
                    {
                        ruri = "";
                        updatestr = "";
                    }
                    if (checkrule == 1)
                    {
                        ruri = "";
                        updatestr = "";
                    }

                    if (ChecklinkAPI(int.Parse(id), true, ruri))
                    {
                        WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功" + updatestr, false);
                    }
                    else
                    {
                        WriteLog(name + "\t" + href + "\t失效,检查 " + ruri + "--成功,更新失败", false);
                    }
                }
                else
				{
					status = true;
					string updatestr = ", 链接已更新";
                    if (url.StartsWith("http://xn--") || url.StartsWith("https://xn--"))
                    {
                        url = "";
                        updatestr = "";
                    }
                    if (checkrule == 1)
                    {
                        url = "";
                        updatestr = "";
                    }

                    if (ChecklinkAPI(int.Parse(id), true, url))
                    {
                        WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功" + updatestr, false);
                    }
                    else
                    {
                        WriteLog(name + "\t" + href + "\t失效,检查 " + url + "--成功,更新失败", false);
                    }
                }

                request.Abort();
                response.Close();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, false);
            }


        }



        private void btnStop_Click(object sender, EventArgs e)
        {
            isStop = true;
            btnStart.Enabled = true;
            //timer.Dispose();
        }


        public void WriteLog(string Message, bool is_update)
        {
            MyInvoke mi = new MyInvoke(WriteTxt);
            this.BeginInvoke(mi, new Object[] { Message, is_update });
        }
        public void WriteTxt(string Message, bool is_update)
        {
            if (is_update)
            {
                richTextBox1.Text = Message;
            }
            else
            {
                Message = DateTime.Now.ToShortTimeString() + "\t" + Message + "\r\n";
                richTextBox1.Text = Message + richTextBox1.Text;
            }
        }
        public static string Punycode(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            var idn = new IdnMapping();
            var url = idn.GetAscii(str);
            return url;
        }


        public bool ChecklinkAPI(int id, bool isalive, string href)
        {
            //if (noupdate) href = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(api, id, isalive, href));            //实例化WebRequest对象  
            request.KeepAlive = true;
            request.Date = DateTime.Now;
            request.Accept = "*/*";
            request.ContentType = "text/html";
            request.Method = "Post";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.ProtocolVersion = HttpVersion.Version10;
            request.Timeout = 30000;
            request.ContentLength = 0;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var responsestream = response.GetResponseStream();
            Encoding ec = Encoding.UTF8;
            StreamReader reader = new StreamReader(responsestream, ec);
            var htmlStr = reader.ReadToEnd();

            request.Abort();
            response.Close();
            reader.Dispose();

            if (htmlStr == "ok") return true;
            return false;
        }


        public string Getlist()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getapi);            //实例化WebRequest对象  
            request.KeepAlive = true;
            request.Date = DateTime.Now;
            request.Accept = "*/*";
            request.ContentType = "text/plain";
            request.Method = "Get";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.ProtocolVersion = HttpVersion.Version10;
            request.Timeout = 30000;
            request.ContentLength = 0;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var responsestream = response.GetResponseStream();
            Encoding ec = Encoding.UTF8;
            StreamReader reader = new StreamReader(responsestream, ec);
            var htmlStr = reader.ReadToEnd();

            request.Abort();
            response.Close();
            reader.Dispose();

            return htmlStr;
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | SecurityProtocolType.SystemDefault;


            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            txtapi.Text = config.AppSettings.Settings["api"].Value;
            txtgetapi.Text = config.AppSettings.Settings["getapi"].Value;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["api"].Value = txtapi.Text;
            config.AppSettings.Settings["getapi"].Value = txtgetapi.Text;
            config.Save(ConfigurationSaveMode.Modified);
        }

        public class AdInfo
        {
            public string Id { get; set; }
            public string Href { get; set; }
            public string Name { get; set; }
            public int CheckRule { get; set; }
        }
    }
}
