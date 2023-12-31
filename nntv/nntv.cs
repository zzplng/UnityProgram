using CefSharp.WinForms;
using CefSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace nntv
{
    public partial class nntv : Form
    {

        string postbbsapi, postimgapi, bbsisexistapi;
        public bool isStop = false;
        public delegate void MyInvoke(string txt, bool is_update);
        //SqlConnection conn;
        //public int extimecnt = 0;
        string NodeId;
		ChromiumWebBrowser Browser1;

		public nntv()
        {
            InitializeComponent();
            //conn = new SqlConnection();
            //string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MainForm.Properties.Settings.ConnectionString"].ConnectionString;
            //conn.ConnectionString = connStr;
            //conn.Open();


            //CefSettings settings = new CefSettings();
            //settings.SetOffScreenRenderingBestPerformanceArgs();
            //Cef.Initialize(settings);
            //InitializeChromium();

        }

		public void InitializeChromium()
		{
			Browser1 = new ChromiumWebBrowser("nntv01.com");
			this.Controls.Add(Browser1);
			Browser1.Dock = DockStyle.Fill;

		}

		private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtbbsisexistapi.Text) || string.IsNullOrEmpty(txtpostbbsapi.Text) || string.IsNullOrEmpty(txtpostimgapi.Text))
            {
                MessageBox.Show("请输入API");
                return;
            }

            bbsisexistapi = txtbbsisexistapi.Text + "?title={0}";
            postbbsapi = txtpostbbsapi.Text + "?title={0}";
            postimgapi = txtpostimgapi.Text;


            //if (string.IsNullOrEmpty(txtNodeId.Text))
            //{
            //    MessageBox.Show("请输入NodeId");
            //    return;
            //}
            NodeId = txtNodeId.Text;



            isStop = false;
            richTextBox1.Text = "";
            Thread thread = new Thread(new ThreadStart(Run));
            thread.IsBackground = true;
            thread.Start();
            btnStart.Enabled = false;
        }


        public void Run()
        {
            //url = "https://www.etoland.co.kr/bbs/board.php?bo_table=star_asia&wr_id=12605&is_hit=yes&page=1";
            //HandleData(url, DateTime.Now);

            //for (int n = 7628328; n > 6000000; n--)
            //{
            //    if (isStop) break;
            //    string url = "https://etoland.co.kr/link.php?n=" + n.ToString();
            //    HandleData(url);
            //}

            try
            {
                while (!isStop)
                {
                    WriteLog("暂停5min", false);
                    Thread.Sleep(300000);
                    WriteLog("", true);
                    int page = 1;
                    while (page <= 3)
                    {
                        WriteLog("暂停20s", false);
                        Thread.Sleep(20000);
                        if (isStop) break;
                        var url = "https://nntv01.com/bbs/board.php?bo_table=sports&page=" + page.ToString();
                        page++;
                        string htmlStr = "";
                        WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  
						WebResponse response = request.GetResponse();           //创建WebResponse对象  
                        Stream datastream = response.GetResponseStream();       //创建流对象  
                        //Encoding ec = Encoding.GetEncoding("EUC-KR");
                        Encoding ec = Encoding.UTF8;
                        StreamReader reader = new StreamReader(datastream, ec);
                        htmlStr = reader.ReadToEnd();                           //读取数据

                        if (htmlStr.Contains("history.go(-1)"))
                        {
                            WriteLog(url + "\t没有数据", false);
                            return;
                        }

                        string board_hit_wrap = "";
                        string regex_board_hit_wrap = @"<ul[\s\S]id=""list-body""+.*?>([\s\S]*?)</ul+.*?>";
                        Regex regex1 = new Regex(regex_board_hit_wrap, RegexOptions.IgnoreCase);
                        if (regex1.IsMatch(htmlStr))
                        {
                            MatchCollection matchCollection = regex1.Matches(htmlStr);
                            foreach (Match match in matchCollection)
                            {
                                var valueHtml = match.Value;
                                board_hit_wrap += valueHtml;
                            }
                        }
                        if (board_hit_wrap == "") { WriteLog("获取不到内容数据", false); return; }
                        //WriteLog(board_hit_wrap, false);

                        string board_hit_list = "";
                        string regex_board_hit_list = @"<li[\s\S]class=""list-item""+.*?>([\s\S]*?)</li*?>";
                        Regex regex2 = new Regex(regex_board_hit_list, RegexOptions.IgnoreCase);
                        if (regex2.IsMatch(board_hit_wrap))
                        {
                            MatchCollection matchCollection = regex2.Matches(board_hit_wrap);
                            foreach (Match match in matchCollection)
                            {
                                var valueHtml = match.Value;
                                board_hit_list += valueHtml;
                            }
                        }
                        if (board_hit_list == "") { WriteLog("获取不到内容数据", false); return; }
                        //WriteLog(tablestr, false);


                        var subjectlist = get_subject_list(board_hit_list);
                        if (subjectlist.Count == 0) { WriteLog("获取不到内容数据", false); return; }
                        //WriteLog(string.Join("",subjectlist), false);

                        var urllist = get_url_list(string.Join("", subjectlist));
                        if (urllist.Count == 0) { WriteLog("获取不到内容数据", false); return; }
                        //WriteLog(string.Join("", urllist), false);

                        DateTime datetimenow = DateTime.Now;

                        foreach (var l in urllist)
                        {
                            var u = l.Replace("\"", "");

                            if (isStop) break;
                            u = HttpUtility.HtmlDecode(u);
                            HandleData(u, datetimenow);
                        }

                        reader.Close();
                        datastream.Close();
                        response.Close();

                    }
                }

                WriteLog("操作已完成", false);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, false);
            }
        }

        public void HandleData(string url, DateTime datetimenow)
        {
            try
            {
                var random = new Random();
                var pause = random.Next(10, 20);
                WriteLog("随机暂停10s - 20s", false);
                Thread.Sleep(pause * 1000);
                WriteLog(url + "\t开始", false);
                string htmlStr = "";
                WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  
                WebResponse response = request.GetResponse();           //创建WebResponse对象  
                Stream datastream = response.GetResponseStream();       //创建流对象  
                //Encoding ec = Encoding.GetEncoding("EUC-KR");
                Encoding ec = Encoding.UTF8;
                StreamReader reader = new StreamReader(datastream, ec);
                htmlStr = reader.ReadToEnd();                           //读取数据

                if (htmlStr.Contains("history.go(-1)"))
                {
                    WriteLog(url + "\t没有数据", false);
                    return;
                }

                string tablestr = "";
                //string regex_html = @"<" + "title" + ".*?</" + "title" + ">";
                string regex_html = @"<article+.*?>([\s\S]*?)</article*?>";
                Regex regex = new Regex(regex_html, RegexOptions.IgnoreCase);
                if (regex.IsMatch(htmlStr))
                {
                    MatchCollection matchCollection = regex.Matches(htmlStr);
                    foreach (Match match in matchCollection)
                    {
                        var valueHtml = match.Value;
                        tablestr += valueHtml;
                    }
                }
                if (tablestr == "") { WriteLog("获取不到内容数据", false); return; }

                /* 获取标题*/
                string tdstr = "";
                string regex_tdtag = @"content=""+.*?""*?>";
                Regex regex_td = new Regex(regex_tdtag, RegexOptions.IgnoreCase);
                if (regex_td.IsMatch(tablestr))
                {
                    MatchCollection matchCollection = regex_td.Matches(tablestr);
                    foreach (Match match in matchCollection)
                    {
                        var valueHtml = match.Value;
                        tdstr += valueHtml;
                        break;
                    }
                }
                if (tdstr == "") { WriteLog("获取不到标题数据", false); return; }

                string subjectstr = "";
                subjectstr = Regex.Replace(tdstr, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
                subjectstr = Regex.Replace(subjectstr, @"\[.*?\]", "", RegexOptions.IgnoreCase);
                subjectstr = subjectstr.Replace("&quot;", "").Replace("&nbsp;", "").Replace("&#039;", "").Replace("&#65279;", "").Replace("&lt;","").Replace("&gt;", "").Replace("'", "");
                subjectstr = subjectstr.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                subjectstr = subjectstr.Replace("content=\"", "").Replace("\">", "").Replace("[네네티비]", "");
                subjectstr = subjectstr.Replace("이토", "");
                subjectstr = subjectstr.Trim();
                /* 获取标题 END*/


                /*获取类型（足球/篮球/棒球）*/
                string typestr = "";
                string regex_typestr = @"<div[\s\S]class=""view-details([\s\S]*?)</div>";
                Regex regex_type = new Regex(regex_typestr, RegexOptions.IgnoreCase);
                if (regex_type.IsMatch(tablestr))
                {
                    MatchCollection matchCollection = regex_type.Matches(tablestr);
                    foreach (Match match in matchCollection)
                    {
                        var valueHtml = match.Value;
                        typestr += valueHtml;
                        break;
                    }
                }
                var vertifytitle = subjectstr;
                if (typestr.Contains("축구"))//足球
                {
                    subjectstr = "足球" + subjectstr;
                }
                else if (typestr.Contains("야구"))//棒球
                {
                    subjectstr = "棒球" + subjectstr;
                }
                else if (typestr.Contains("농구"))//篮球
                {
                    subjectstr = "篮球" + subjectstr;
                }
                else//其他
                {
                    subjectstr = "其他" + subjectstr;
                }
                /*获取类型（足球/篮球/棒球）END*/

                //var sqlexsist = string.Format("select count(*) from Topic where Title=N'{0}'", subjectstr);
                //SqlCommand cmdexsist = new SqlCommand(sqlexsist, conn);
                //var cntexsist = (int)cmdexsist.ExecuteScalar();

                //if (cntexsist > 0)
                //            {
                //	WriteLog(subjectstr + "\t已存在", false);
                //	return;
                //}

                if (!isExist(vertifytitle))
                {
                    WriteLog(vertifytitle + "\t已存在", false);
                    return;
                }


                string viewstr = "";
                string regex_viewtag = @"class=""view-content"">([\s\S]*?)</div>";
                Regex regex_view = new Regex(regex_viewtag, RegexOptions.IgnoreCase);
                if (regex_view.IsMatch(tablestr))
                {
                    MatchCollection matchCollection = regex_view.Matches(tablestr);
                    foreach (Match match in matchCollection)
                    {
                        var valueHtml = match.Value;
                        viewstr += valueHtml;
                    }
                }
                if (viewstr == "") { WriteLog("获取不到内容数据", false);  return; }

                viewstr = viewstr.Replace("&quot;", "").Replace("&nbsp;", "").Replace("'", "");
                viewstr = viewstr.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                if (viewstr.IndexOf("<style") > 0) viewstr = viewstr.Replace(viewstr.Substring(viewstr.IndexOf("<style"), viewstr.IndexOf("</style>") - viewstr.IndexOf("<style") + 8), "");
                if (viewstr.IndexOf("<script") > 0) viewstr = viewstr.Replace(viewstr.Substring(viewstr.IndexOf("<script"), viewstr.IndexOf("</script>") - viewstr.IndexOf("<script") + 9), "");
                //viewstr = Regex.Replace(viewstr, @"<script[\s\S]+.*?>([\s\S]*?)</script*?>", "", RegexOptions.IgnoreCase);
                //viewstr = Regex.Replace(viewstr, @"<style[\s\S]+.*?>([\s\S]*?)</style*?>", "", RegexOptions.IgnoreCase);
                viewstr = viewstr.Replace("class=\"view-content\">", "");
                viewstr = viewstr.Replace("이토", "");

                //图片视频处理
                var imglist = get_img_list(viewstr);
                var videolist = get_video_list(viewstr);

                WriteLog("正在下载资源...", false);
                List<string> filelist1 = new List<string>();
                List<string> filelist2 = new List<string>();
                var returnimgtag = DownLoadSource(imglist, out filelist1);
                DownLoadSource(videolist, out filelist2);


                //viewstr = viewstr.Replace(" ", "").Trim();

                //viewstr = Regex.Replace(viewstr, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase); //去除所有标签
                viewstr = Regex.Replace(viewstr, @"<(?!br).*?>", "", RegexOptions.IgnoreCase);   //去除所有标签，只剩br
                viewstr = string.Join("", videolist) + viewstr;
                viewstr = string.Join("", returnimgtag) + viewstr;
				if (viewstr == "") { WriteLog("获取不到内容数据", false); return; }

				reader.Close();
                datastream.Close();
                response.Close();

                //var sql = string.Format("insert into Test values(N'{0}',N'{1}',N'{2}')", subjectstr, viewstr, url);
                //var sql = string.Format("insert into Topic(Content,CreateOn,LastReplyTime,NodeId,ReplyCount,Title,[Top],[Type],UserId,ViewCount) values(N'{0}',GETDATE(),GETDATE(),{2},0,N'{1}',0,1,'3012d9aa-438d-4445-bf51-8c4afe27dc6c',0)", viewstr, subjectstr, NodeId);
                //SqlCommand cmd = new SqlCommand(sql, conn);
                //cmd.ExecuteNonQuery();


                if (!PostBBS(subjectstr, viewstr))
                {
                    WriteLog("插入数据失败", false); return;
                }


                List<string> filelist = filelist1.Concat(filelist2).ToList();
                foreach (var file in filelist)
                {
                    PostImg(file);
                }

                WriteLog(vertifytitle + "\t完成", false);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, false);
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


        //public void Write(string Message, bool is_update)
        //{
        //    MyInvoke mi = new MyInvoke(WriteTitle);
        //    this.BeginInvoke(mi, new Object[] { Message, is_update });
        //}
        //public void WriteTitle(string Message, bool is_update)
        //{
        //    textBox1.Text = Message;
        //}



        public static string[] get_img_list(string html)
        {
            List<string> list = new List<string>();
            string regex_html = @"<img.*?>";
            Regex regex = new Regex(regex_html, RegexOptions.IgnoreCase);
            if (regex.IsMatch(html))
            {
                MatchCollection matchCollection = regex.Matches(html);
                foreach (Match match in matchCollection)
                {
                    var valueHtml = match.Value;
                    list.Add(valueHtml);//获取到的
                }
            }
            return list.ToArray();
        }


        public static string[] get_video_list(string html)
        {
            List<string> list = new List<string>();
            string regex_html = @"<video+.*?>([\s\S]*?)</video*?>";
            Regex regex = new Regex(regex_html, RegexOptions.IgnoreCase);
            if (regex.IsMatch(html))
            {
                MatchCollection matchCollection = regex.Matches(html);
                foreach (Match match in matchCollection)
                {
                    var valueHtml = match.Value;
                    list.Add(valueHtml);//获取到的
                }
            }
            return list.ToArray();
        }


		public static List<string> get_subject_list(string html)
		{
			List<string> list = new List<string>();
			string regex_html = @"<div[\s\S]class=""wr-subject""+.*?>([\s\S]*?)</div*?>";
			Regex regex = new Regex(regex_html, RegexOptions.IgnoreCase);
			if (regex.IsMatch(html))
			{
				MatchCollection matchCollection = regex.Matches(html);
				foreach (Match match in matchCollection)
				{
					var valueHtml = match.Value;
					list.Add(valueHtml);//获取到的
				}
			}
			return list;
		}



		public static List<string> get_url_list(string html)
		{
			List<string> list = new List<string>();
			string regex_html = @"""https://nntv01.com/bbs/board.php+.*?""";
			Regex regex = new Regex(regex_html, RegexOptions.IgnoreCase);
			if (regex.IsMatch(html))
			{
				MatchCollection matchCollection = regex.Matches(html);
				foreach (Match match in matchCollection)
				{
					var valueHtml = match.Value;
					list.Add(valueHtml);//获取到的
				}
			}
			return list;
		}

        public List<string> DownLoadSource(string[] list, out List<string> filelist)
        {
            //设置保存目录
            string path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "wwwroot");
            //不存在目录则创建
            if (!Directory.Exists(path))
            {
                //创建目录
                Directory.CreateDirectory(path);
            }
            Regex regimg = new Regex(@"<img.*?src=""(?<src>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);
            Regex regvideo = new Regex(@"<video.*?src=""(?<src>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);

            List<string> returntag = new List<string>();
            filelist = new List<string>();

            foreach (var tag in list)
            {
                if (tag.Contains("img"))//图片
                {
                    if (tag.Contains("data-cfsrc")) continue;
                    MatchCollection mc = regimg.Matches(tag);
                    foreach (Match m in mc)
                    {
                        try
                        {
                            string filepath = "";
                            var src = m.Groups["src"].Value;

                            if (src.Contains("https://nntv01.com/"))
                            {
                                src = src.Replace("https://nntv01.com", "");
                            }
                            if (!src.Contains("http"))
                            {
                                returntag.Add("<img src=\"" + src + "\">");
                                filepath = path + src;        //图片路径命名 
                                src = "https://nntv01.com/" + src;

                            }
                            else
                            {
                                returntag.Add("<img src=\"" + src + "\">");
                                continue;
                            }
                            WebRequest request = WebRequest.Create(src);//图片src内容
                            WebResponse response = request.GetResponse();
                            //文件流获取图片操作
                            Stream reader = response.GetResponseStream();
                            string savepath = filepath.Substring(0, filepath.LastIndexOf("/"));
                            if (!Directory.Exists(savepath))
                            {
                                //创建目录
                                Directory.CreateDirectory(savepath);
                            }
                            FileStream writer = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                            filelist.Add(filepath);
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
                        catch (Exception msg)
                        {
                            WriteLog(msg.Message, false);
                        }
                    }
                }
                if (tag.Contains("video"))//视频
                {
                    MatchCollection mc = regvideo.Matches(tag);
                    foreach (Match m in mc)
                    {
                        try
                        {
                            string filepath = "";
                            var src = m.Groups["src"].Value;
                            if (!src.Contains("http"))
                            {
                                filepath = path + src;        //图片路径命名 
                                src = "https://etoland.co.kr" + src;
                            }
                            else
                            {
                                continue;
                            }
                            WebRequest request = WebRequest.Create(src);//图片src内容
                            WebResponse response = request.GetResponse();
                            //文件流获取图片操作
                            Stream reader = response.GetResponseStream();
                            string savepath = filepath.Substring(0, filepath.LastIndexOf("/"));
                            if (!Directory.Exists(savepath))
                            {
                                //创建目录
                                Directory.CreateDirectory(savepath);
                            }
                            FileStream writer = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                            filelist.Add(filepath);
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
                        catch (Exception msg)
                        {
                            WriteLog(msg.Message, false);
                        }
                    }
                }
            }
            return returntag;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isStop = true;  
            btnStart.Enabled = true;
        }



        public bool isExist(string title)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(bbsisexistapi, title));            //实例化WebRequest对象  
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

            if (htmlStr == "no") return false;
            return true;
        }



        public bool PostBBS(string title, string content)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(postbbsapi, title));            //实例化WebRequest对象  
            request.KeepAlive = true;
            request.Date = DateTime.Now;
            request.Accept = "*/*";
            request.ContentType = "text/html";
            request.Method = "Post";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.ProtocolVersion = HttpVersion.Version10;
            request.Timeout = 30000;
            //request.ContentLength = 0;


            using (Stream reqStream = request.GetRequestStream())
            {
                var b = Encoding.UTF8.GetBytes(content);
                reqStream.Write(b, 0, b.Length);
                reqStream.Close();
            }

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


        public bool PostImg(string filePath)
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

            //var filePath = "D:\\Code\\UnityProgram\\PostBBS\\bin\\Debug\\3718152700_FkJQqiAz_1.mp4";
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
                WriteLog("文件传输异常： " + ex.Message, false);
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
            if (htmlStr == "ok") return true;

            WriteLog("文件传输异常： " + htmlStr, false);
            return false;
        }

        private void etoland_Load(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            txtbbsisexistapi.Text = config.AppSettings.Settings["bbsisexistapi"].Value;
            txtpostbbsapi.Text = config.AppSettings.Settings["postbbsapi"].Value;
            txtpostimgapi.Text = config.AppSettings.Settings["postimgapi"].Value;
        }

        private void etoland_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["bbsisexistapi"].Value = txtbbsisexistapi.Text;
            config.AppSettings.Settings["postbbsapi"].Value = txtpostbbsapi.Text;
            config.AppSettings.Settings["postimgapi"].Value = txtpostimgapi.Text;
            config.Save(ConfigurationSaveMode.Modified);

        }


    }


}
