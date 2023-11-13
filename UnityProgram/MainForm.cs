using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
	public partial class MainForm : UIForm
	{

		string postbbsapi, postimgapi, bbsisexistapi;
		private List<JsonInfo> listjson = new List<JsonInfo>();

		public bool isStop = false;
		public delegate void MyInvoke(string txt, bool is_update);

		public MainForm()
		{
			InitializeComponent();

			AddPage(new livescore(), 1002);

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
				UIMessageDialog.ShowMessageDialog("找不到json文件", UILocalize.InfoTitle, false, Style);
				return;
			}
			var json = File.ReadAllText(jsonpath);
			try
			{
				listjson = JsonConvert.DeserializeObject<List<JsonInfo>>(json);
				listjson.RemoveAll(t => t.Site != "etoland"); //移除非此爬虫网站的json
			}
			catch (Exception ex)
			{
				UIMessageDialog.ShowMessageDialog("json配置：" + ex.Message, UILocalize.InfoTitle, false, Style);
				return;
			}
			if (listjson.Count <= 0)
			{
				UIMessageDialog.ShowMessageDialog("json没有数据", UILocalize.InfoTitle, false, Style);
				return;
			}


			//var url = "https://etoland.co.kr/link.php?n=7614386";
			////url = "https://etoland.co.kr/link.php?n=7628328";
			////url = "https://etoland.co.kr/link.php?n=7628318";
			////url = "https://etoland.co.kr/link.php?n=7628283";
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
						WriteLog("暂停60s", false);
						Thread.Sleep(60000);
						if (isStop) break;
						var url = "https://www.etoland.co.kr/pages/hit.php?&page=" + page.ToString();
						page++;
						string htmlStr = "";
						WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  
						WebResponse response = request.GetResponse();           //创建WebResponse对象  
						Stream datastream = response.GetResponseStream();       //创建流对象  
						Encoding ec = Encoding.GetEncoding("EUC-KR");
						StreamReader reader = new StreamReader(datastream, ec);
						htmlStr = reader.ReadToEnd();                           //读取数据

						if (htmlStr.Contains("history.go(-1)"))
						{
							WriteLog(url + "\t没有数据", false);
							return;
						}

						string board_hit_wrap = "";
						string regex_board_hit_wrap = @"<div[\s\S]class='board_hit_wrap'+.*?>([\s\S]*?)<div[\s\S]class='write_pages_wrap'+.*?>";
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
						string regex_board_hit_list = @"<ul[\s\S]class='board_hit_list'+.*?>([\s\S]*?)</ul*?>";
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

						var list = new List<string>();
						foreach (var info in subjectlist)
						{
							if (!info.Contains("시사게시판"))//排除敏感话题
							{
								list.Add(info);
							}
						}

						var urllist = get_url_list(string.Join("", list));
						if (urllist.Count == 0) { WriteLog("获取不到内容数据", false); return; }
						//WriteLog(string.Join("", urllist), false);

						DateTime datetimenow = DateTime.Now;

						foreach (var l in urllist)
						{
							//Thread.Sleep(10000);//每个链接间隔十秒
							var u = "https://www.etoland.co.kr" + l.Replace("'", "");

							//重新读json，因为判重时移除了重复数据
							listjson = JsonConvert.DeserializeObject<List<JsonInfo>>(json);
							listjson.RemoveAll(t => t.Site != "etoland"); //移除非此爬虫网站的json

							if (isStop) break;
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
				Encoding ec = Encoding.GetEncoding("EUC-KR");
				StreamReader reader = new StreamReader(datastream, ec);
				htmlStr = reader.ReadToEnd();                           //读取数据

				if (htmlStr.Contains("history.go(-1)"))
				{
					WriteLog(url + "\t没有数据", false);
					return;
				}

				string tablestr = "";
				//string regex_html = @"<" + "title" + ".*?</" + "title" + ">";
				string regex_html = @"<table+.*?>([\s\S]*?)</table*?>";
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
				string regex_tdtag = @"<td[\s\S]class=mw_basic_view_subject+.*?>([\s\S]*?)</td*?>";
				Regex regex_td = new Regex(regex_tdtag, RegexOptions.IgnoreCase);
				if (regex_td.IsMatch(tablestr))
				{
					MatchCollection matchCollection = regex_td.Matches(tablestr);
					foreach (Match match in matchCollection)
					{
						var valueHtml = match.Value;
						tdstr += valueHtml;
					}
				}
				if (tdstr == "")
				{
					string regex_tdtag1 = @"<div[\s\S]class=title+.*?>([\s\S]*?)</div*?>";
					Regex regex_td1 = new Regex(regex_tdtag1, RegexOptions.IgnoreCase);
					if (regex_td1.IsMatch(tablestr))
					{
						MatchCollection matchCollection = regex_td1.Matches(tablestr);
						foreach (Match match in matchCollection)
						{
							var valueHtml = match.Value;
							tdstr += valueHtml;
						}
					}
				}
				if (tdstr == "")
				{
					string regex_html1 = @"<div[\s\S]class='board_view_wrap'+.*?>([\s\S]*?)<div[\s\S]id=""mw_good""+.*?>";
					Regex regex1 = new Regex(regex_html1, RegexOptions.IgnoreCase);
					if (regex1.IsMatch(htmlStr))
					{
						MatchCollection matchCollection = regex1.Matches(htmlStr);
						foreach (Match match in matchCollection)
						{
							var valueHtml = match.Value;
							tablestr += valueHtml;
						}
					}

					string regex_tdtag1 = @"<div[\s\S]class='title'+.*?>([\s\S]*?)</div*?>";
					Regex regex_td1 = new Regex(regex_tdtag1, RegexOptions.IgnoreCase);
					if (regex_td1.IsMatch(tablestr))
					{
						MatchCollection matchCollection = regex_td1.Matches(tablestr);
						foreach (Match match in matchCollection)
						{
							var valueHtml = match.Value;
							tdstr += valueHtml;
						}
					}
				}
				if (tdstr == "") { WriteLog("获取不到标题数据", false); return; }

				string subjectstr = "";
				subjectstr = Regex.Replace(tdstr, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
				subjectstr = Regex.Replace(subjectstr, @"\[.*?\]", "", RegexOptions.IgnoreCase);
				//textBox1.Text = subjectstr.Trim();
				subjectstr = subjectstr.Replace("&quot;", "").Replace("&nbsp;", "").Replace("&#039;", "").Replace("&#034;", "").Replace("&#65279;", "").Replace("&lt;", "").Replace("&gt;", "").Replace("'", "").Trim();
				subjectstr = subjectstr.Replace("\n", "").Replace("\t", "").Replace("\r", "");
				//Write(subjectstr.TrimStart().TrimEnd(), false);
				/* 获取标题 END*/

				//var sqlexsist = string.Format("select count(*) from Topic where Title=N'{0}'", subjectstr);
				//SqlCommand cmdexsist = new SqlCommand(sqlexsist, conn);
				//var cntexsist = (int)cmdexsist.ExecuteScalar();

				//if (cntexsist > 0)
				//{
				//    WriteLog(subjectstr + "\t已存在", false);
				//    return;
				//}

				//if (!isExist(subjectstr))
				//{
				//	WriteLog(subjectstr + "\t已存在", false);
				//	return;
				//}
				var errlist = isExist(subjectstr);

				foreach (var err in errlist)
				{
					WriteLog(subjectstr + "\t" + err, false);
				}
				if (listjson.Count == 0)
				{
					WriteLog(subjectstr + "\t已存在", false);
					return;
				}


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


				//tablestr = tablestr.Substring(0, tablestr.IndexOf("mw_good"));


				tablestr = Regex.Replace(tablestr, @"<div[\s\S]class='view_document_address'+.*?>([\s\S]*?)</div*?>", "", RegexOptions.IgnoreCase);

				string viewstr = "";
				string regex_viewtag = @"<div[\s\S]id=view_content+.*?>([\s\S]*?)<div[\s\S]id=""mw_good""+.*?>";
				//regex_viewtag = @"<div[\s\S]id=view_content+.*?>([\s\S]*?).*?";
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
				if (viewstr == "")
				{
					string regex_viewtag1 = @"<div[\s\S]id='view_content'+.*?>([\s\S]*?)<div[\s\S]id=""mw_good""+.*?>";
					Regex regex_view1 = new Regex(regex_viewtag1, RegexOptions.IgnoreCase);
					if (regex_view1.IsMatch(tablestr))
					{
						MatchCollection matchCollection = regex_view1.Matches(tablestr);
						foreach (Match match in matchCollection)
						{
							var valueHtml = match.Value;
							viewstr += valueHtml;
						}
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
				var videolist = get_video_list(viewstr);

				WriteLog("正在下载资源...", false);
				List<string> filelist1 = new List<string>();
				List<string> filelist2 = new List<string>();
				var returnimgtag = DownLoadSource(imglist, out filelist1);
				DownLoadSource(videolist, out filelist2);



				//viewstr = viewstr.Replace(" ", "").Trim();

				//viewstr = Regex.Replace(viewstr, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase); //去除所有标签
				viewstr = Regex.Replace(viewstr, @"<(?!br).*?>", "", RegexOptions.IgnoreCase);   //去除所有标签，只剩br
				viewstr = viewstr.Replace("이브라우저는비디오태그를지원하지않습니다.크롬을사용권장합니다.", "");//去除视频标签带的文字
				viewstr += string.Join("", returnimgtag);
				viewstr += string.Join("", videolist).Replace("data-src", "src").Replace("이 브라우저는 비디오태그를 지원하지 않습니다. 크롬을 사용 권장합니다.", "");//去除视频标签带的文字
				if (viewstr == "") { WriteLog("获取不到内容数据", false); return; }

				reader.Close();
				datastream.Close();
				response.Close();

				//var sql = string.Format("insert into Topic(Content,CreateOn,LastReplyTime,NodeId,ReplyCount,Title,[Top],[Type],UserId,ViewCount) values(N'{0}',GETDATE(),GETDATE(),12,0,N'{1}',0,1,'3012d9aa-438d-4445-bf51-8c4afe27dc6c',0)", viewstr, subjectstr);
				//SqlCommand cmd = new SqlCommand(sql, conn);
				//cmd.ExecuteNonQuery();

				//if (!PostBBS(subjectstr, viewstr))
				//{
				//	WriteLog("插入数据失败", false); return;
				//}
				var errlist1 = PostBBS(subjectstr, viewstr);

				foreach (var err in errlist1)
				{
					WriteLog(subjectstr + "\t" + err, false);
				}

				List<string> filelist = filelist1.Concat(filelist2).ToList();
				int fileindex = 0;
				foreach (var file in filelist)
				{
					fileindex++;
					WriteLog("文件传输中..." + fileindex);
					Thread.Sleep(5000);
					var errlist2 = PostImg(file);
					foreach (var err in errlist2)
					{
						WriteLog(subjectstr + "\t" + err, false);
					}
				}

				WriteLog(subjectstr + "\t完成", false);
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
			string regex_html = @"'/bbs/board.php+.*?'";
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
			Regex regimg = new Regex(@"<img.*?data-src=""(?<datasrc>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);
			Regex regvideo = new Regex(@"<video.*?data-src=""(?<datasrc>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);

			List<string> returntag = new List<string>();
			filelist = new List<string>();

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
							var src = m.Groups["datasrc"].Value;
							if (!src.Contains("http"))
							{
								returntag.Add("<img src=\"" + src + "\">");
								filepath = path + src;        //图片路径命名 
								src = "https://etoland.co.kr" + src;

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
					if (mc.Count == 0)
					{
						Regex regimg1 = new Regex(@"<img.*?src=""(?<src>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);
						MatchCollection mc1 = regimg1.Matches(tag);
						foreach (Match m in mc1)
						{
							try
							{
								string filepath = "";
								var src = m.Groups["src"].Value;
								if (!src.Contains("http"))
								{
									returntag.Add("<img src=\"" + src + "\">");
									filepath = path + src;        //图片路径命名 
									src = "https://etoland.co.kr" + src;

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
				}
				if (tag.Contains("video"))//视频
				{
					MatchCollection mc = regvideo.Matches(tag);
					foreach (Match m in mc)
					{
						try
						{
							string filepath = "";
							var src = m.Groups["datasrc"].Value;
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
						errlist.Add(info.Name + "：写入失败");
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

			int pos = filePath.LastIndexOf("\\");
			string fileName = filePath.Substring(pos + 1);

			//请求头部信息 
			StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));
			byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());

			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			byte[] bArr = new byte[fs.Length];
			fs.Read(bArr, 0, bArr.Length);
			fs.Close();

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

					byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
					byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

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



		private class JsonInfo
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
