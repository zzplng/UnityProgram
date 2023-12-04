using CefSharp.WinForms;
using CefSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using static Checklink.MainForm;
using System.Net;
using System.Configuration;
using System.Security.Policy;
using CefSharp.DevTools.Network;
using System.IO.Compression;
using Newtonsoft.Json;

namespace Checklink
{
    public partial class Browser : Form
    {
        public bool isStop = false;
        public bool isLoad = false;
        public bool isError = false;
        public string newurl;
        public string lasturl;
        ChromiumWebBrowser Browser1;
        System.Threading.Timer timer;
        string api, getapi;
        string currenturl;
        private delegate void MyDelegate();
        public delegate void MyInvoke(string txt, bool is_update);


        public Browser()
        {
            InitializeComponent();
            CefSettings settings = new CefSettings();
            settings.SetOffScreenRenderingBestPerformanceArgs();
            Cef.Initialize(settings);
            InitializeChromium();
            //setintervel();

            timer = new System.Threading.Timer(setintervel, null, 5000, Timeout.Infinite);
        }
        public Browser(string[] args)
        {
            InitializeComponent();
            CefSettings settings = new CefSettings();
            settings.SetOffScreenRenderingBestPerformanceArgs();
            Cef.Initialize(settings);
            InitializeChromium(args[0]);
            //setintervel();
        }
        public void InitializeChromium()
        {
            Browser1 = new ChromiumWebBrowser("google.com");
            this.Controls.Add(Browser1);
            Browser1.Dock = DockStyle.Fill;
        }
        public void InitializeChromium(string args)
        {
            Browser1 = new ChromiumWebBrowser(args);
            this.Controls.Add(Browser1);
            Browser1.Dock = DockStyle.Fill;
        }

        public void setintervel(Object o)
        {
            try
            {
                timer.Dispose();
                //timer = new System.Threading.Timer(Thread_Timer_Method, null, 5000, 5000);

                //Thread.Sleep(3000);

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                api = config.AppSettings.Settings["api"].Value + "?id={0}&isalive={1}&href={2}";
                getapi = config.AppSettings.Settings["getapi"].Value;


                //var list = new List<AdInfo>();
                //var info = new AdInfo();
                //info.Name = "123";
                //info.Id = "999";
                //info.Href = "https://bozatv49.com";
                //list.Add(info);

                while (!isStop)
                {
                    //Browser1.ExecuteScriptAsync("window.open (\"http://www.zhihu.com\", \"newwindow2\", \"height=100, width=100, top=100, left=100,toolbar=no, menubar=no, scrollbars=no, resizable=no, location=no, status=no\")");

                    #region
                    /*****************json********************/
                    //List<JsonInfo> listjson = new List<JsonInfo>();
                    //var jsonpath = Path.Combine(Application.StartupPath, "apisetting.json");
                    //if (!File.Exists(jsonpath))
                    //{
                    //    WriteLog("找不到json文件");
                    //    isStop = true;
                    //    break;
                    //}
                    //var json = File.ReadAllText(jsonpath);
                    //listjson = JsonConvert.DeserializeObject<List<JsonInfo>>(json);
                    //if (listjson.Count <= 0)
                    //{
                    //    WriteLog("json没有数据");
                    //    isStop = true;
                    //    break;
                    //}
                    //foreach (var j in listjson)
                    //{ 
                    //}

                    //合并检查
                    //方案一：根据SiteId区分，每个链接单独检查并更新（时间长，多个站时间长）
                    //方案二：合并相同网站链接，然后在更新时分别检查各API接口（单独的慢，处理复杂）

                    //不合并检查
                    //每个站单独一个检查程序

                    /*************************************/
                    #endregion

                    var jsonstr = Getlist();
                    var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdInfo>>(jsonstr);

                    WriteLog("暂停1min", false);
                    Thread.Sleep(60000);//间隔30min
                    WriteLog("", true);
                    WriteLog("开始", false);
                    foreach (var item in list)
                    {
                        WriteLog("==================间隔20s=================");
                        Thread.Sleep(20000);

                        isLoad = false;
                        isError = false;
                        newurl = "";

                        if (isStop) break;
                        var id = item.Id;
                        var name = item.Name;
                        var href = item.Href;
                        var checkrule = item.CheckRule;

                        if (string.IsNullOrEmpty(href))
                        {
                            WriteLog(name + "\t无链接跳过", false);
                            continue;
                        }

                        redirect(name, href);

                        Thread.Sleep(30000);

                        CheckHtml(href);

                        Thread.Sleep(20000);

                        for (int i = 1; i <= 8; i++)//多检查几遍，各种问题可能获取不到
                        {
                            if (!isLoad)
                            {
                                WriteLog(name + "\t" + href + "\t开始 重试第" + i + "次", false);

                                redirect(name, href);

                                Thread.Sleep(30000);

                                CheckHtml(href);

                                Thread.Sleep(10000);
                            }
                            else
                            {
                                break;
                            }
                        }

						if (!isLoad)
						{
                            CheckHtml(href);
                            Thread.Sleep(3000);
                        }
						if (!isLoad)
						{
                            CheckHtml(href);
                            Thread.Sleep(3000);
                        }
						if (!isLoad)
						{
                            CheckHtml(href);
                            Thread.Sleep(3000);
                        }


						for (int i = 1; i <= 3; i++)
                        {
                            if (!isLoad)
                            {
                                WriteLog(name + "\t" + href + "\t开始 加长重试第" + i + "次", false);

                                redirect(name, href);

                                Thread.Sleep(100000); //有超长加载时间的，延长

                                CheckHtml(href);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (!isLoad)
                        {
                            //WebRequest
                            if (!isError)
                            {
                                CheckHttp(id, name, href);
                            }
                        }

                        if (!isLoad)
                        {
                            //链接含数字，自增三次测试

                            CheckMore(id, name, href, out string newhref);
                            if (isLoad)
                            {
                                href = newhref;
                            }
                        }


                        if (isLoad)
                        {
                            if (newurl != "")
                            {
                                href = newurl;
                            }

                            if (checkrule == 1)
                            {
                                href = "";
                            }

							WriteLog("最终结果Href:" + href);

                            if (!string.IsNullOrEmpty(href) && href.Contains("www.warning.or.kr"))
							{
								if (ChecklinkAPI(int.Parse(id), false, href))
								{
									WriteLog(name + "\t" + href + "\t失效,更新成功", false);
								}
								else
								{
									WriteLog(name + "\t" + href + "\t失效,更新失败", false);
								}
                                continue;
							}

							if (!string.IsNullOrEmpty(lasturl) && !string.IsNullOrEmpty(href) && (href.Contains(lasturl) || lasturl.Contains(href)))
                            {
                                WriteLog("与上个url一致,退出更新");
                                continue;
                            }

                            if (ChecklinkAPI(int.Parse(id), true, href))
                            {
                                WriteLog(name + "\t" + href + "\t成功,更新成功", false);
                            }
                            else
                            {
                                WriteLog(name + "\t" + href + "\t成功,更新失败", false);
                            }
                        }
                        else
                        {
                            if (ChecklinkAPI(int.Parse(id), false, ""))
                            {
                                WriteLog(name + "\t" + href + "\t失效,状态更新成功", false);
                            }
                            else
                            {
                                WriteLog(name + "\t" + href + "\t失效,状态更新失败", false);
                            }
                        }


                        lasturl = href;

                    }
                    MyDelegate my = new MyDelegate(writetxt);
                    this.BeginInvoke(my);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, false);
            }
        }

        public void redirect(string name, string href)
        {
            WriteLog(name + "\t" + href + "\t开始", false);
            Browser1.ExecuteScriptAsync("window.location.href='" + href + "'");
            //Browser1.Load(href);
        }

        void Thread_Timer_Method(object o)
        {
            Browser1.ExecuteScriptAsync("window.location.href='http://www.douban.com'");
            //MessageBox.Show("1");
            //MyDelegate my = new MyDelegate(doclose);
            //this.BeginInvoke(my);
        }

        void doclose()
        {
            timer.Dispose();
            this.Dispose();
            this.Close();
        }

        public void writetxt()
        {
            //写入txt
            try
            {
                var direc = Application.StartupPath + "\\" + "log";
                if (!Directory.Exists(direc))
                {
                    Directory.CreateDirectory(direc);
                }
                var filepath = direc + "\\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt";
                var note = richTextBox1.Text;
                FileStream fs = new FileStream(filepath, FileMode.CreateNew, FileAccess.ReadWrite);
                var nodebytes = Encoding.UTF8.GetBytes(note);

                fs.Write(nodebytes, 0, nodebytes.Length);
                fs.Close();
                fs.Dispose();
            }
            catch
            {
            }
        }

        private void Browser_Load(object sender, EventArgs e)
        {
            //Browser1.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(chromeBrowser_IsFrameLoadEnd);
            //Browser1.AddressChanged += Browser_AddressChanged;
            //Browser1.DownloadHandler = new DownloadHandler(this);
            Browser1.LifeSpanHandler = new LifeSpanHandler();
            Browser1.JsDialogHandler = new JsDialogHandler();

        }
        private void Browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            this.currenturl = e.Address;
            WriteLog(currenturl, false);
        }


        void chromeBrowser_IsFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
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


        public void CheckMore(string id, string name, string href, out string newhref)
        {
            string numstr = "";
            newhref = "";

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

            for (int i = 1; i <= 3; i++)
            {
                Thread.Sleep(30000);
                if (isLoad) continue;

                var changednum = changenum + i;
                var url = href.Replace(numstr, changednum.ToString());
                newhref = url;

                WriteLog(name + "\t" + url + "\t链接含数字 检查中...", false);
                Thread.Sleep(2000);

                redirect(name, url);

                Thread.Sleep(30000);

                CheckHtml(href);

                Thread.Sleep(10000);

            }

        }

        public void CheckHtml(string href)
        {
            try
            {
                var body = Browser1.EvaluateScriptAsync("document.getElementsByTagName('body')[0].innerHTML;").Result;
                if (body != null && body.Success)
                {
                    var resultStrbody = body.ToString();
                    if (resultStrbody.Length > 1000)
                    {
                        WriteLog("body有内容");
                    }
                }
                else
                {
                    WriteLog(body.Message, false);
                }


                //var frames = Browser1.GetBrowser().GetFrameNames();
                var mainframe = Browser1.GetBrowser().MainFrame;

                var frameurl = mainframe.Url;
                WriteLog("页面地址" + frameurl);
                if (!frameurl.Contains("xn--"))
                {
                    if (frameurl != href && frameurl != "null")
                    {
                        if (frameurl.Contains(href) || href.Contains(frameurl))
                        {
                            WriteLog("页面地址包含");
                        }
                        else
                        {
                            WriteLog("页面地址不同,将更新");
                            newurl = frameurl;
                        }
                    }
                }
                else
                {
                    //punycode
                    try
                    {
                        var unicodeurl = Unicode(frameurl);

                        if (unicodeurl != href)
                        {
                            if (unicodeurl.Contains(href) || href.Contains(unicodeurl))
                            {
                                WriteLog("页面地址包含");
                            }
                            else
                            {
                                WriteLog("页面地址不同,将更新");
                                newurl = unicodeurl;
                            }
                        }
                    }
                    catch
                    { }
                }


                var resultStr = mainframe.GetSourceAsync().Result;
                if (resultStr.Length > 1000)
                {
                    isLoad = true;
                    WriteLog("页面有内容");
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("function tempFunction() {");
                sb.AppendLine(" return document.getElementsByTagName('title')[0].innerHTML; ");
                sb.AppendLine("}");
                sb.AppendLine("tempFunction();");
                var task01 = mainframe.EvaluateScriptAsync(sb.ToString()).Result;
                if (task01.Success == true)
                {
                    if (task01.Result != null)
                    {
                        string resultStr01 = task01.Result.ToString();
                        if (resultStr01.Contains("请稍后") || resultStr01.Contains("Just a moment"))
                        {
                            isLoad = true;
                            WriteLog("防火墙页面");
                        }
                        if ((resultStr01.Contains("400") || resultStr01.Contains("403") || resultStr01.Contains("501") || resultStr01.Contains("502") || resultStr01.Contains("503")
                        || resultStr01.Contains("504")) && resultStr01.Length < 1000)
                        {
                            isLoad = false;
                            isError = true;
                            WriteLog("错误页面");
                        }
                        WriteLog("页面含标题" + resultStr01);
                    }
                }


                sb = new StringBuilder();
                sb.AppendLine("function tempFunction2() {");
                sb.AppendLine(" return document.getElementsByName('title')[0].content; ");
                sb.AppendLine("}");
                sb.AppendLine("tempFunction2();");
                var task02 = mainframe.EvaluateScriptAsync(sb.ToString()).Result;
                if (task02.Success == true)
                {
                    if (task02.Result != null)
                    {
                        string resultStr02 = task02.Result.ToString();
                        if (resultStr02.Length > 0)
                        {
                            isLoad = true;
                            WriteLog("页面含标题" + resultStr02);
                        }
                    }
                }

                sb = new StringBuilder();
                sb.AppendLine("function tempFunction3() {");
                sb.AppendLine(" return location.origin; ");
                sb.AppendLine("}");
                sb.AppendLine("tempFunction3();");
                var task03 = mainframe.EvaluateScriptAsync(sb.ToString()).Result;
                if (task03.Success == true)
                {
                    if (task03.Result != null)
                    {
                        string resultStr03 = task03.Result.ToString();
                        resultStr03 += "/";
                        if (resultStr03.Length > 0 && resultStr03 != "null/")
                        {
                            isLoad = true;
                            WriteLog("页面源地址:" + resultStr03);
                            if (!resultStr03.Contains("xn--"))
                            {
                                if (resultStr03 != href && resultStr03 != "null")
                                {
                                    if (href.Contains(resultStr03) || resultStr03.Contains(href))
                                    {
                                        WriteLog("页面源地址包含");
                                    }
                                    else
                                    {
                                        WriteLog("页面源地址不同,将更新");
                                        newurl = resultStr03;
                                    }
                                }
                            }
                            else
                            {
                                //punycode
                                try
                                {
                                    var unicodeurl = Unicode(resultStr03);

                                    if (unicodeurl != href)
                                    {
                                        if (unicodeurl.Contains(href) || href.Contains(unicodeurl))
                                        {
                                            WriteLog("页面源地址包含");
                                        }
                                        else
                                        {
                                            WriteLog("页面源地址不同,将更新");
                                            newurl = unicodeurl;
                                        }
                                    }
                                }
                                catch
                                { }
                            }
                        }
                    }
                }

            }
            catch (Exception ex) 
            {
                WriteLog(ex.Message, false);
            }
        }


        public void CheckHttp(string id, string name, string href)
        {

            Thread.Sleep(3000);

            WriteLog(name + "\t" + href + "\tHttp开始", false);
            Thread.Sleep(2000);


            try
            {
                if (string.IsNullOrEmpty(href))
                {
                    WriteLog(name + "\t" + href + "\t无链接", false);
                    return;
                }

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


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();           //创建WebResponse对象
                Stream stream = response.GetResponseStream();
				if (response.ContentEncoding == "gzip")
				{
					stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
				}
				StreamReader reader = new StreamReader(stream);
                var resultStr = reader.ReadToEnd();
                if (resultStr.Length > 1000)
                {
                    var verifystr = resultStr.Substring(0, 1000);
                    if (verifystr.Contains("400") || verifystr.Contains("403") || verifystr.Contains("501") || verifystr.Contains("502") || verifystr.Contains("503") || verifystr.Contains("504"))
                    {
                        isLoad = false;
                        isError = true;
                        WriteLog("错误页面", false);
                    }
                    else
                    {
                        isLoad = true;
                        WriteLog("Http页面有内容", false);
                    }
                }
                var code = response.StatusCode;
                if (code != HttpStatusCode.OK)
                {
                    isLoad = false;
                    WriteLog("错误 HttpCode" + code, false);
                }

                request.Abort();
                response.Close();
            }
            catch (WebException ex)
            {
                WriteLog(ex.Message, false);
                System.GC.Collect();
            }

        }


        public void WriteLog(string Message, bool is_update = false)
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

    }


    public class JsonInfo
    {
        public string SiteId { get; set; }
        public string Site { get; set; }
        public string API { get; set; }
        public string GetAPI { get; set; }
    }


    public class DownloadHandler : IDownloadHandler
    {


        //public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        //public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        Browser mainForm;

        public DownloadHandler(Browser form)
        {
            mainForm = form;
        }

        public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        {
            throw new NotImplementedException();
        }

        public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            //var handler = OnBeforeDownloadFired;
            //if (handler != null)
            //{
            //    handler(this, downloadItem);
            //}

            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                }
            }
        }

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            throw new NotImplementedException();
        }

        public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            //var handler = OnDownloadUpdatedFired;
            //if (handler != null)
            //{
            //    handler(this, downloadItem);
            //}
            if (downloadItem.IsComplete)
            {
                MessageBox.Show("下载成功");

            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// js c#回调类
    /// </summary>
    public class ScriptCallbackManager
    {
        /// <summary>
        /// 查找电脑信息
        /// </summary>
        /// <param name="javascriptCallback"></param>
        public void FindComputerInfo(IJavascriptCallback javascriptCallback)
        {

            Task.Factory.StartNew(async () =>
            {

                using (javascriptCallback)
                {
                    //Computer computer = new Computer();
                    //string response = JsonConvert.SerializeObject(new
                    //{
                    //    cpu_id = computer.CPU_Id,
                    //    disk_id = computer.Disk_Id,
                    //    host_name = computer.HostName,
                    //    networkcard = computer.NetworkCard,
                    //    serialNumber = computer.SerialNumber_Manufacturer_Product.Item1,
                    //    manufacturer = computer.SerialNumber_Manufacturer_Product.Item2,
                    //    product = computer.SerialNumber_Manufacturer_Product.Item3,
                    //});
                    string response = "";
                    await javascriptCallback.ExecuteAsync(response);
                }
            });

        }


    }


    internal class LifeSpanHandler : ILifeSpanHandler
    {
        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return false;//返回true会导致弹出的窗口关不掉
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }


        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }


        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            return true;//返回true才会阻止弹出窗口，返回false无效
        }
    }

    public class JsDialogHandler : IJsDialogHandler
    {
        public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
        {
            return true;
        }

        public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnJSDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        {
            //suppressMessage = true;
            callback.Continue(true);
            return true;
        }

        public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }
    }
}
