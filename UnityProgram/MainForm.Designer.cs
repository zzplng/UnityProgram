namespace UnityProgram
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
			this.uiTabControl1 = new Sunny.UI.UITabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.btnStop = new Sunny.UI.UIButton();
			this.richTextBox1 = new Sunny.UI.UIRichTextBox();
			this.btnStart = new Sunny.UI.UIButton();
			this.uiTabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// uiStyleManager1
			// 
			this.uiStyleManager1.Style = Sunny.UI.UIStyle.Black;
			// 
			// uiTabControl1
			// 
			this.uiTabControl1.Controls.Add(this.tabPage1);
			this.uiTabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
			this.uiTabControl1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.uiTabControl1.Frame = this;
			this.uiTabControl1.ItemSize = new System.Drawing.Size(150, 40);
			this.uiTabControl1.Location = new System.Drawing.Point(3, 38);
			this.uiTabControl1.MainPage = "";
			this.uiTabControl1.Name = "uiTabControl1";
			this.uiTabControl1.SelectedIndex = 0;
			this.uiTabControl1.Size = new System.Drawing.Size(935, 633);
			this.uiTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.uiTabControl1.Style = Sunny.UI.UIStyle.Black;
			this.uiTabControl1.TabIndex = 1;
			this.uiTabControl1.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.btnStop);
			this.tabPage1.Controls.Add(this.richTextBox1);
			this.tabPage1.Controls.Add(this.btnStart);
			this.tabPage1.Location = new System.Drawing.Point(0, 40);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(935, 593);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "etoland";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// btnStop
			// 
			this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnStop.FillColor = System.Drawing.Color.Black;
			this.btnStop.FillColor2 = System.Drawing.Color.Black;
			this.btnStop.FillHoverColor = System.Drawing.Color.Black;
			this.btnStop.FillPressColor = System.Drawing.Color.Black;
			this.btnStop.FillSelectedColor = System.Drawing.Color.Black;
			this.btnStop.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btnStop.Location = new System.Drawing.Point(181, 33);
			this.btnStop.MinimumSize = new System.Drawing.Size(1, 1);
			this.btnStop.Name = "btnStop";
			this.btnStop.RectColor = System.Drawing.Color.Black;
			this.btnStop.RectHoverColor = System.Drawing.Color.Black;
			this.btnStop.RectPressColor = System.Drawing.Color.Black;
			this.btnStop.RectSelectedColor = System.Drawing.Color.Black;
			this.btnStop.Size = new System.Drawing.Size(100, 35);
			this.btnStop.Style = Sunny.UI.UIStyle.Custom;
			this.btnStop.StyleCustomMode = true;
			this.btnStop.TabIndex = 3;
			this.btnStop.Text = "停止";
			this.btnStop.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// richTextBox1
			// 
			this.richTextBox1.FillColor = System.Drawing.Color.White;
			this.richTextBox1.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
			this.richTextBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.richTextBox1.Location = new System.Drawing.Point(17, 94);
			this.richTextBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.richTextBox1.MinimumSize = new System.Drawing.Size(1, 1);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Padding = new System.Windows.Forms.Padding(2);
			this.richTextBox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(58)))), ((int)(((byte)(92)))));
			this.richTextBox1.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
			this.richTextBox1.ShowText = false;
			this.richTextBox1.Size = new System.Drawing.Size(904, 481);
			this.richTextBox1.Style = Sunny.UI.UIStyle.Black;
			this.richTextBox1.TabIndex = 2;
			this.richTextBox1.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
			// 
			// btnStart
			// 
			this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
			this.btnStart.FillColor = System.Drawing.Color.Black;
			this.btnStart.FillColor2 = System.Drawing.Color.Black;
			this.btnStart.FillHoverColor = System.Drawing.Color.Black;
			this.btnStart.FillPressColor = System.Drawing.Color.Black;
			this.btnStart.FillSelectedColor = System.Drawing.Color.Black;
			this.btnStart.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btnStart.Location = new System.Drawing.Point(17, 33);
			this.btnStart.MinimumSize = new System.Drawing.Size(1, 1);
			this.btnStart.Name = "btnStart";
			this.btnStart.RectColor = System.Drawing.Color.Black;
			this.btnStart.RectHoverColor = System.Drawing.Color.Black;
			this.btnStart.RectPressColor = System.Drawing.Color.Black;
			this.btnStart.RectSelectedColor = System.Drawing.Color.Black;
			this.btnStart.Size = new System.Drawing.Size(100, 35);
			this.btnStart.Style = Sunny.UI.UIStyle.Custom;
			this.btnStart.StyleCustomMode = true;
			this.btnStart.TabIndex = 0;
			this.btnStart.Text = "开始";
			this.btnStart.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// MainForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(941, 674);
			this.ControlBoxFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.Controls.Add(this.uiTabControl1);
			this.ForeColor = System.Drawing.Color.White;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainTabControl = this.uiTabControl1;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(58)))), ((int)(((byte)(92)))));
			this.ShowTitleIcon = true;
			this.Style = Sunny.UI.UIStyle.Black;
			this.Text = "MainForm";
			this.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(21)))), ((int)(((byte)(21)))));
			this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 800, 450);
			this.uiTabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIStyleManager uiStyleManager1;
        private Sunny.UI.UITabControl uiTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
		private Sunny.UI.UIButton btnStart;
		private Sunny.UI.UIRichTextBox richTextBox1;
		private Sunny.UI.UIButton btnStop;
	}
}