using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Text.RegularExpressions;

namespace Mail
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}
		private void button1_Click(object sender, EventArgs e)
		{

			SendMail("123", "123", "tcnewsoft@hotmail.com", "1050425503@qq.com", "", "", "", "");
		}


		/// <summary>
		/// 发送邮件方法
		/// </summary>
		/// <param name="Subject">邮件标题</param>
		/// <param name="Body">邮件内容</param>
		/// <param name="FromMail">发件人邮箱</param>
		/// <param name="ToMail">收件人邮箱(多个收件人地址用";"号隔开)</param>
		/// <param name="AuthorizationCode">发件人授权码（需要通过在邮箱设置中获取）</param>
		/// <param name="ReplyTo">对方回复邮件时默认的接收地址（不设置也是可以的）</param>
		/// <param name="CCMail">//邮件的抄送者(多个抄送人用";"号隔开)（不设置也是可以的）</param>
		/// <param name="File_Path">附件的地址（不设置也是可以的）</param>
		public static void SendMail(string Subject, string Body, string FromMail, string ToMail, string AuthorizationCode, string ReplyTo, string CCMail, string File_Path)
		{
			try
			{
				//实例化一个发送邮件类。
				System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();

				mailMessage.BodyEncoding = System.Text.Encoding.UTF8;

				//邮件的优先级，分为 Low, Normal, High，通常用 Normal即可
				mailMessage.Priority = MailPriority.Normal;

				//发件人邮箱地址。
				mailMessage.From = new MailAddress(FromMail);

				//收件人邮箱地址。需要群发就写多个
				//拆分邮箱地址
				List<string> ToMaillist = ToMail.Split(';').ToList();
				for (int i = 0; i < ToMaillist.Count; i++)
				{
					mailMessage.To.Add(new MailAddress(ToMaillist[i]));  //收件人邮箱地址。
				}

				if (ReplyTo == "" || ReplyTo == null)
				{
					ReplyTo = FromMail;
				}

				if (CCMail != "" && CCMail != null)
				{
					List<string> CCMaillist = ToMail.Split(';').ToList();
					for (int i = 0; i < CCMaillist.Count; i++)
					{
						//邮件的抄送者，支持群发
						mailMessage.CC.Add(new MailAddress(CCMail));
					}
				}
				//如果你的邮件标题包含中文，这里一定要指定，否则对方收到的极有可能是乱码。
				mailMessage.SubjectEncoding = Encoding.UTF8;

				//邮件正文是否是HTML格式
				mailMessage.IsBodyHtml = true;

				//邮件标题。
				mailMessage.Subject = Subject;
				//邮件内容。
				mailMessage.Body = Body;

				//设置邮件的附件，将在客户端选择的附件先上传到服务器保存一个，然后加入到mail中  
				if (File_Path != "" && File_Path != null)
				{
					//将附件添加到邮件
					mailMessage.Attachments.Add(new Attachment(File_Path));
					//获取或设置此电子邮件的发送通知。
					mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
				}

				//实例化一个SmtpClient类。
				SmtpClient client = new SmtpClient();

				#region 设置邮件服务器地址

				//在这里我使用的是163邮箱，所以是smtp.163.com，如果你使用的是qq邮箱，那么就是smtp.qq.com。
				// client.Host = "smtp.163.com";
				if (FromMail.Length != 0)
				{
					//根据发件人的邮件地址判断发件服务器地址   默认端口一般是25
					string[] addressor = FromMail.Trim().Split(new Char[] { '@', '.' });
					switch (addressor[1])
					{
						case "163":
							client.Host = "smtp.163.com";
							break;
						case "126":
							client.Host = "smtp.126.com";
							break;
						case "qq":
							client.Host = "smtp.qq.com";
							break;
						case "gmail":
							client.Host = "smtp.gmail.com";
							break;
						case "hotmail":
							client.Host = "smtp.office365.com";//outlook邮箱
															   //client.Port = 587;
							break;
						case "foxmail":
							client.Host = "smtp.foxmail.com";
							break;
						case "sina":
							client.Host = "smtp.sina.com.cn";
							break;
						default:
							client.Host = "smtp.exmail.qq.com";//qq企业邮箱
							break;
					}
				}
				#endregion

				client.Port = 587;

				//使用安全加密连接。
				client.EnableSsl = true;
				//不和请求一块发送。
				client.UseDefaultCredentials = false;

				//验证发件人身份(发件人的邮箱，邮箱里的生成授权码);
				client.Credentials = new NetworkCredential("tcnewsoft@hotmail.com", "kkkk0914");

				//如果发送失败，SMTP 服务器将发送 失败邮件告诉我  
				mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
				//发送
				client.Send(mailMessage);
				Console.WriteLine("发送成功");
			}
			catch (Exception ex)
			{

				Console.WriteLine("发送失败" + ex.Message);
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var username = textBox1.Text;
			Regex rg = new Regex("^[0-9a-zA-Z\uAC00-\uD7AF]$");
			foreach (var c in username)
			{
				var s = c.ToString();
				if (!rg.IsMatch(s))
				{
					MessageBox.Show(c.ToString());
				}
			}
		}
	}
}
