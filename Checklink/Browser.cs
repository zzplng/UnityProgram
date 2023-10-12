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

namespace Checklink
{
    public partial class Browser : Form
    {
        public bool isStop = false;
        public bool isLoad = false;
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
            Browser1 = new ChromiumWebBrowser("baidu.com");
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
            //timer = new System.Threading.Timer(Thread_Timer_Method, null, 5000, 5000);

            Thread.Sleep(3000);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            api = config.AppSettings.Settings["api"].Value + "?id={0}&isalive={1}&href={2}";
            getapi = config.AppSettings.Settings["getapi"].Value;

            var jsonstr = Getlist();
            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AdInfo>>(jsonstr);

            while (!isStop)
            {
                Browser1.ExecuteScriptAsync("window.open (\"http://www.zhihu.com\", \"newwindow2\", \"height=100, width=100, top=100, left=100,toolbar=no, menubar=no, scrollbars=no, resizable=no, location=no, status=no\")");

                WriteLog("暂停1min", false);
                Thread.Sleep(60000);//间隔30min
                WriteLog("", true);
                WriteLog("开始", false);
                foreach (var item in list)
                {
                    isLoad = false;
                    Thread.Sleep(30000);

                    if (isStop) break;
                    var id = item.Id;
                    var name = item.Name;
                    var href = item.Href;
                    var checkrule = item.CheckRule;

                    redirect(name, href);

                    Thread.Sleep(30000);

                    if (!isLoad)
                    {
                        CheckMore(id, name, href, out string newhref);
                        if (isLoad)
                        {
                            href = newhref;
                        }
                    }


                    if (isLoad)
                    {

                        if (checkrule == 1)
                        {
                            href = "";
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
                }
            }
        }

        public void redirect(string name, string href)
        {
            WriteLog(name + "\t" + href + "\t开始", false);
            Browser1.ExecuteScriptAsync("window.location.href='" + href + "'");
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

        private void Browser_Load(object sender, EventArgs e)
        {
            Browser1.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(chromeBrowser_IsFrameLoadEnd);
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
            try
            {

                if (e.Frame.IsValid)
                {
                    //var sss = e.Frame.Url;
                    //if (sss != null && sss != "about:blank")
                    //{
                    //    WriteLog("页面地址" + sss, false);
                    //}


                    var task = e.Frame.GetSourceAsync();
                    task.ContinueWith(t =>
                    {
                        if (!t.IsFaulted)
                        {
                            string resultStr = t.Result;
                            if (resultStr.Length > 1000)
                            {
                                var verifystr = resultStr.Substring(0, 1000);
                                if (verifystr.Contains("400") || verifystr.Contains("403") || verifystr.Contains("501") || verifystr.Contains("502") || verifystr.Contains("503") || verifystr.Contains("504"))
                                {
                                    isLoad = false;
                                    WriteLog("错误页面", false);
                                    return;
                                }

                                isLoad = true;
                                WriteLog("页面有内容", false);
                            }
                        }
                    });

                    //Browser1.ExecuteScriptAsync("alert('123')");
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("function tempFunction() {");
                    sb.AppendLine(" return document.getElementsByTagName('title')[0].innerHTML; ");
                    sb.AppendLine("}");
                    sb.AppendLine("tempFunction();");
                    var task01 = Browser1.GetBrowser().GetFrame(Browser1.GetBrowser().GetFrameNames()[0]).EvaluateScriptAsync(sb.ToString());
                    task01.ContinueWith(t =>
                    {
                        if (!t.IsFaulted)
                        {
                            var response = t.Result;
                            if (response.Success == true)
                            {
                                if (response.Result != null)
                                {
                                    string resultStr = response.Result.ToString();
                                    if (resultStr.Contains("请稍后") || resultStr.Contains("Just a moment"))
                                    {
                                        isLoad = true;
                                        WriteLog("防火墙页面", false);
                                    }
                                    if (resultStr.Contains("400") || resultStr.Contains("403") || resultStr.Contains("501") || resultStr.Contains("502") || resultStr.Contains("503") || resultStr.Contains("504"))
                                    {
                                        isLoad = false;
                                        WriteLog("错误页面", false);
                                        return;
                                    }
                                }
                            }
                        }
                    });


                    sb = new StringBuilder();
                    sb.AppendLine("function tempFunction2() {");
                    sb.AppendLine(" return document.getElementsByName('title')[0].content; ");
                    sb.AppendLine("}");
                    sb.AppendLine("tempFunction2();");
                    var task02 = Browser1.GetBrowser().GetFrame(Browser1.GetBrowser().GetFrameNames()[0]).EvaluateScriptAsync(sb.ToString());
                    task02.ContinueWith(t =>
                    {
                        if (!t.IsFaulted)
                        {
                            var response = t.Result;
                            if (response.Success == true)
                            {
                                if (response.Result != null)
                                {
                                    string resultStr = response.Result.ToString();
                                    if (resultStr.Length > 0)
                                    {
                                        isLoad = true;
                                        WriteLog("页面含标题" + resultStr, false);
                                    }
                                }
                            }
                        }
                    });

                    //WriteLog("加载了LoadEnd", false);
                }
                else
                {
                    WriteLog("不可用", false);
                }
            }
            catch (Exception ex) 
            {
                //Browser1.ExecuteScriptAsync("alert('" + ex.Message + "')");
                WriteLog(ex.Message, false);
            }

            
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


            }

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
            return false;
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
            return true;
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
