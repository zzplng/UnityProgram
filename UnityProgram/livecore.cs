using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnityProgram
{
    public partial class livecore : UIPage
    {
        
		string postbbsapi, postimgapi, bbsisexistapi;
		List<JsonInfo> listjson = new List<JsonInfo>();
        public bool isStop = false;
        public delegate void MyInvoke(string txt, bool is_update);

        public livecore()
        {
            InitializeComponent();

        }


        private void btnStart_Click(object sender, EventArgs e)
        {

            isStop = false;
            richTextBox1.Text = "";
            Thread thread = new Thread(new ThreadStart(Run));
            thread.IsBackground = true;
            thread.Start();
            btnStart.Enabled = false;
        }


        public void Run()
        {
            var jsonpath = Path.Combine(Application.StartupPath, "apisetting.json");
            if (!File.Exists(jsonpath))
            {
                UIMessageDialog.ShowMessageDialog("找不到json文件", UILocalize.InfoTitle, false, UIStyle.Black);
                return;
            }
            var json = File.ReadAllText(jsonpath);
            try
            {
                listjson = JsonConvert.DeserializeObject<List<JsonInfo>>(json);
                listjson.RemoveAll(t => t.Site != "livecore"); //移除非此爬虫网站的json
            }
            catch (Exception ex)
            {
                UIMessageDialog.ShowMessageDialog("json配置：" + ex.Message, UILocalize.InfoTitle, false, UIStyle.Black);
                return;
            }
            if (listjson.Count <= 0)
            {
                UIMessageDialog.ShowMessageDialog("json没有数据", UILocalize.InfoTitle, false, UIStyle.Black);
                return;
            }


            //var url = "https://www.livescore.co.kr/bbs/board.php?bo_table=share&wr_id=2002984";
            //HandleData(url, DateTime.Now);

            //for (int n = 7628328; n > 6000000; n--)
            //{
            //    if (isStop) break;
            //    string url = "https://livescore.co.kr/link.php?n=" + n.ToString();
            //    HandleData(url);
            //}

            try
            {
                while (!isStop)
                {
                    WriteLog("暂停1h", false);
                    Thread.Sleep(3600000);
                    WriteLog("", true);
                    int page = 1;
                    int seq = 0;
                    DateTime today = DateTime.Now;
                    while (page <= 10)
                    {
                        WriteLog("暂停20s", false);
                        Thread.Sleep(20000);
                        if (isStop) break;
                        var url = "https://www.livescore.co.kr/bbs/board.php?bo_table=share&page=" + page.ToString();
                        page++;
                        string htmlStr = "";
                        WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  
                        WebResponse response = request.GetResponse();           //创建WebResponse对象  
                        Stream datastream = response.GetResponseStream();       //创建流对象  
                        Encoding ec = Encoding.UTF8;
                        StreamReader reader = new StreamReader(datastream, ec);
                        htmlStr = reader.ReadToEnd();                           //读取数据

                        if (htmlStr.Contains("history.go(-1)"))
                        {
                            WriteLog(url + "\t没有数据", false);
                            continue;
                        }

                        string board_hit_wrap = "";
                        string regex_board_hit_wrap = @"<ul[\s\S]id=""gall_ul""+.*?>([\s\S]*?)</ul>";
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
                        if (board_hit_wrap == "") { WriteLog("获取不到内容数据", false); continue; }
                        //WriteLog(board_hit_wrap, false);

                        //var subjectlist = get_subject_list(board_hit_wrap);
                        //if (subjectlist.Count == 0) { WriteLog("获取不到内容数据", false); return; }
                        //WriteLog(string.Join("",subjectlist), false);

                        //var list = new List<string>();
                        //foreach (var info in subjectlist)
                        //{
                        //    if (!info.Contains("시사게시판"))//排除敏感话题
                        //    {
                        //        list.Add(info);
                        //    }
                        //}

                        var urllist = get_url_list(board_hit_wrap);
                        if (urllist.Count == 0) { WriteLog("获取不到内容数据", false); continue; }
                        urllist = urllist.Distinct().ToList();
                        //WriteLog(string.Join("", urllist), false);

                        DateTime datetimenow = DateTime.Now;
                        if (datetimenow.Day != today.Day)
                        {
                            seq = 0;
                            today = DateTime.Now;
                        }
                        foreach (var l in urllist)
                        {
                            //重新读json，因为判重时移除了重复数据
                            listjson = JsonConvert.DeserializeObject<List<JsonInfo>>(json);
                            listjson.RemoveAll(t => t.Site != "livecore"); //移除非此爬虫网站的json


                            seq++;
                            var u = l.Replace("&amp;", "&").Replace("\"", "");
                            if (!u.Contains("page")) continue;
                            if (isStop) break;
                            HandleData(u, datetimenow.ToString("yyMMdd") + "_" + seq);
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

        public void HandleData(string url, string title)
        {
            try
            {
                WriteLog("暂停20s", false);
                Thread.Sleep(20000);
                WriteLog(url + "\t开始", false);
                string htmlStr = "";
                WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  
                WebResponse response = request.GetResponse();           //创建WebResponse对象  
                Stream datastream = response.GetResponseStream();       //创建流对象  
                Encoding ec = Encoding.UTF8;
                StreamReader reader = new StreamReader(datastream, ec);
                htmlStr = reader.ReadToEnd();                           //读取数据

                if (htmlStr.Contains("history.go(-1)"))
                {
                    WriteLog(url + "\t没有数据", false);
                    return;
                }

                //string tablestr = "";
                //string regex_html = @"<table+.*?>([\s\S]*?)</table*?>";
                //Regex regex = new Regex(regex_html, RegexOptions.IgnoreCase);
                //if (regex.IsMatch(htmlStr))
                //{
                //    MatchCollection matchCollection = regex.Matches(htmlStr);
                //    foreach (Match match in matchCollection)
                //    {
                //        var valueHtml = match.Value;
                //        tablestr += valueHtml;
                //    }
                //}
                //if (tablestr == "") { WriteLog("获取不到内容数据", false); return; }



                //var sqlexsist = string.Format("select count(*) from Topic where Title=N'{0}'", subjectstr);
                //SqlCommand cmdexsist = new SqlCommand(sqlexsist, conn);
                //var cntexsist = (int)cmdexsist.ExecuteScalar();

                //if (cntexsist > 0)
                //{
                //    WriteLog(subjectstr + "\t已存在", false);
                //    return;

                //}


                /* 获取时间*/
                //string timespan = "";
                //string regex_timetag = @"<span[\s\S]class=mw_basic_view_datetime+.*?>([\s\S]*?)</span*?>";
                //Regex regex_time = new Regex(regex_timetag, RegexOptions.IgnoreCase);
                //string regex_timetag1 = @"<span[\s\S]class='datetime'+.*?>([\s\S]*?)</span*?>";
                //Regex regex_time1 = new Regex(regex_timetag1, RegexOptions.IgnoreCase);
                //if (regex_time.IsMatch(tablestr))
                //            {
                //                MatchCollection matchCollection = regex_time.Matches(tablestr);
                //                foreach (Match match in matchCollection)
                //                {
                //                    var valueHtml = match.Value;
                //                    timespan += valueHtml;
                //                }
                //            }
                //            else if(regex_time1.IsMatch(tablestr))
                //{
                //	MatchCollection matchCollection = regex_time1.Matches(tablestr);
                //	foreach (Match match in matchCollection)
                //	{
                //		var valueHtml = match.Value;
                //		timespan += valueHtml;
                //	}
                //}
                //if (timespan == "") { WriteLog("获取不到时间数据", false); return; }
                //string time = "";
                //time = Regex.Replace(timespan, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
                //time = Regex.Replace(time, @"\[.*?\]", "", RegexOptions.IgnoreCase);
                ////textBox1.Text = subjectstr.Trim();
                //time = time.Replace("&quot;", "").Replace("&nbsp;", "").Replace("&#039;", "").Replace("&#65279;", "").Replace(" ", "").Replace("'", "").Trim();
                //time = time.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                //            time = time.Substring(0, time.IndexOf('(')) + " " + time.Substring(time.IndexOf(')') + 1);
                //            //WriteLog(time, false);

                //            if (datetimenow.AddHours(-10) > DateTime.Parse(time))
                //            {
                //                //isStop = true;
                //                WriteLog("帖子发布时间超过一小时已停止", false);
                //                //extimecnt++;
                //                //if (extimecnt > 50)
                //                //{
                //                //	isStop = true;
                //                //}
                //                return;
                //            }

                /* 获取时间 END*/


                string viewstr = "";
                string regex_viewtag = @"<div[\s\S]id=""bo_v_con""+.*?>([\s\S]*?)</div>";
                Regex regex_view = new Regex(regex_viewtag, RegexOptions.IgnoreCase);
                if (regex_view.IsMatch(htmlStr))
                {
                    MatchCollection matchCollection = regex_view.Matches(htmlStr);
                    foreach (Match match in matchCollection)
                    {
                        var valueHtml = match.Value;
                        viewstr += valueHtml;
                    }
                }
                if (viewstr == "") { WriteLog("获取不到内容数据", false); return; }

                viewstr = viewstr.Replace("&quot;", "").Replace("&nbsp;", "").Replace("'", "");
                viewstr = viewstr.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                if (viewstr.IndexOf("<style") > 0) viewstr = viewstr.Replace(viewstr.Substring(viewstr.IndexOf("<style"), viewstr.IndexOf("</style>") - viewstr.IndexOf("<style") + 8), "");
                if (viewstr.IndexOf("<script") > 0) viewstr = viewstr.Replace(viewstr.Substring(viewstr.IndexOf("<script"), viewstr.IndexOf("</script>") - viewstr.IndexOf("<script") + 9), "");
                //viewstr = Regex.Replace(viewstr, @"<script[\s\S]+.*?>([\s\S]*?)</script*?>", "", RegexOptions.IgnoreCase);
                //viewstr = Regex.Replace(viewstr, @"<style[\s\S]+.*?>([\s\S]*?)</style*?>", "", RegexOptions.IgnoreCase);

                //图片视频处理
                var imglist = get_img_list(viewstr);
                //var videolist = get_video_list(viewstr);



                WriteLog("正在下载资源...", false);
                var returnimgtag = DownLoadSource(imglist, url);
                //DownLoadSource(videolist);
                if (returnimgtag.Count == 0)
                {
                    WriteLog("已存在", false);
                    return;
                }


                //viewstr = viewstr.Replace(" ", "").Trim();

                viewstr = Regex.Replace(viewstr, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase); //去除所有标签
                //viewstr = Regex.Replace(viewstr, @"<(?!br).*?>", "", RegexOptions.IgnoreCase);   //去除所有标签，只剩br
                //viewstr = viewstr.Replace("이브라우저는비디오태그를지원하지않습니다.크롬을사용권장합니다.", "");//去除视频标签带的文字
                viewstr += string.Join("", returnimgtag);
                //viewstr += string.Join("", videolist).Replace("data-src", "src").Replace("이 브라우저는 비디오태그를 지원하지 않습니다. 크롬을 사용 권장합니다.","");//去除视频标签带的文字
                if (viewstr == "") { WriteLog("获取不到内容数据", false); return; }

                reader.Close();
                datastream.Close();
                response.Close();

                //var sql = string.Format("insert into Test values(N'{0}',N'{1}',N'{2}')", subjectstr, viewstr, url);
                //var sql = string.Format("insert into Topic(Content,CreateOn,Email,LastReplyTime,NodeId,ReplyCount,Title,[Top],[Type],UserId,ViewCount) values(N'{0}',GETDATE(),'{2}',GETDATE(),{3},0,N'{1}',0,1,'3012d9aa-438d-4445-bf51-8c4afe27dc6c',0)", viewstr, title, url, NodeId);
                //SqlCommand cmd = new SqlCommand(sql, conn);
                //cmd.ExecuteNonQuery();

                var random = new Random();
                var r = random.Next(99999);
                var errlist1 = PostBBS(title + r, viewstr);

                foreach (var err in errlist1)
                {
                    WriteLog(title + "\t" + err, false);
                }

                WriteLog(url + "\t完成", false);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, false);
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
            string regex_html = @"<div[\s\S]class='subject'+.*?>([\s\S]*?)</div*?>";
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
            string regex_html = @"""https://livescore.co.kr/bbs/board.php+.*?""";
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

        public List<string> DownLoadSource(string[] list, string url)
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

            foreach (var tag in list)
            {
                if (tag.Contains("img"))//图片
                {
                    MatchCollection mc = regimg.Matches(tag);
                    foreach (Match m in mc)
                    {
                        try
                        {
                            string filepath = "";
                            var src = m.Groups["src"].Value;

                            //var sqlexsist = string.Format("select count(*) from Topic where NodeId={1} and Email='{0}'", url, NodeId);
                            //SqlCommand cmdexsist = new SqlCommand(sqlexsist, conn);
                            //var cntexsist = (int)cmdexsist.ExecuteScalar();

                            //if (cntexsist > 0)
                            //{
                            //    return returntag;
                            //}


                            if (!src.Contains("http"))
                            {
                                returntag.Add("<img src=\"" + src + "\">");
                                filepath = path + src;        //图片路径命名 
                                src = "https://livescore.co.kr" + src;

                            }
                            else
                            {
                                //var s = src.Replace("http://img.livescore.co.kr", "");
                                //filepath = path + s;        //图片路径命名 
                                //returntag.Add("<img src=\"" + s + "\">");
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
                                src = "https://livescore.co.kr" + src;
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


        public List<string> isExist(string title)
        {
            List<string> errlist = new List<string>();
            List<JsonInfo> remove = new List<JsonInfo>();
            foreach (var info in listjson)
            {
                try
                {
                    bbsisexistapi = info.IsExist + "?title={0}";
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
                    if (htmlStr == "no")
                    {
                        remove.Add(info);
                        errlist.Add(info.Name + "：已存在");
                    }
                }
                catch (Exception ex)
                {
                    errlist.Add(info.Name + "：判重失败，" + ex.Message);
                }

            }
            foreach (var r in remove)
            {
                listjson.Remove(r);
            }
            return errlist;
        }


        public List<string> PostBBS(string title, string content)
        {
            List<string> errlist = new List<string>();
            foreach (var info in listjson)
            {
                try
                {
                    postbbsapi = info.PostBBS + "?title={0}&nodeId={1}";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(postbbsapi, title, info.NodeId));            //实例化WebRequest对象  
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

                    if (htmlStr != "ok")
                    {
                        errlist.Add(info.Name + "：写入失败，返回信息：" + htmlStr);
                    }
                }
                catch (Exception ex)
                {
                    errlist.Add(info.Name + "：写入失败，" + ex.Message);
                }
            }
            return errlist;
        }


        public List<string> PostImg(string filePath)
        {
            List<string> errlist = new List<string>();
            foreach (var info in listjson)
            {
                try
                {
                    postimgapi = info.PostImg;
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


                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    var responsestream = response.GetResponseStream();
                    Encoding ec = Encoding.UTF8;
                    StreamReader reader = new StreamReader(responsestream, ec);
                    var htmlStr = reader.ReadToEnd();
                    if (htmlStr != "ok")
                    {
                        errlist.Add(info.Name + "：写入文件失败，返回信息:" + htmlStr);
                    }
                }
                catch (Exception ex)
                {
                    errlist.Add(info.Name + "：写入文件失败，" + ex.Message);
                }
            }
            return errlist;
        }



        public class JsonInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Site { get; set; }
            public string IsExist { get; set; }
            public string PostBBS { get; set; }
            public string PostImg { get; set; }
            public string NodeId { get; set; }
        }
    }
}
