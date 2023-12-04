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
using static nntv.MainForm;
using System.Net;
using System.Configuration;
using System.Security.Policy;
using CefSharp.DevTools.Network;
using System.IO.Compression;

namespace nntv
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
            redirect("", "https://nntv01.com/");
        }

        public void redirect(string name, string href)
        {
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
        }


        void chromeBrowser_IsFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
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
