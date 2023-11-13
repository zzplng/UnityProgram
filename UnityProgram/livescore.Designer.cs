namespace UnityProgram
{
    partial class livescore
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
            this.richTextBox1 = new Sunny.UI.UIRichTextBox();
            this.btnStop = new Sunny.UI.UIButton();
            this.btnStart = new Sunny.UI.UIButton();
            this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.FillColor = System.Drawing.Color.White;
            this.richTextBox1.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.richTextBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richTextBox1.Location = new System.Drawing.Point(13, 106);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.richTextBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Padding = new System.Windows.Forms.Padding(2);
            this.richTextBox1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(58)))), ((int)(((byte)(92)))));
            this.richTextBox1.ScrollBarColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.richTextBox1.ShowText = false;
            this.richTextBox1.Size = new System.Drawing.Size(907, 473);
            this.richTextBox1.Style = Sunny.UI.UIStyle.Black;
            this.richTextBox1.TabIndex = 6;
            this.richTextBox1.TextAlignment = System.Drawing.ContentAlignment.TopLeft;
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
            this.btnStop.Location = new System.Drawing.Point(177, 36);
            this.btnStop.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStop.Name = "btnStop";
            this.btnStop.RectColor = System.Drawing.Color.Black;
            this.btnStop.RectHoverColor = System.Drawing.Color.Black;
            this.btnStop.RectPressColor = System.Drawing.Color.Black;
            this.btnStop.RectSelectedColor = System.Drawing.Color.Black;
            this.btnStop.Size = new System.Drawing.Size(100, 35);
            this.btnStop.Style = Sunny.UI.UIStyle.Custom;
            this.btnStop.StyleCustomMode = true;
            this.btnStop.TabIndex = 8;
            this.btnStop.Text = "停止";
            this.btnStop.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
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
            this.btnStart.Location = new System.Drawing.Point(13, 36);
            this.btnStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.btnStart.Name = "btnStart";
            this.btnStart.RectColor = System.Drawing.Color.Black;
            this.btnStart.RectHoverColor = System.Drawing.Color.Black;
            this.btnStart.RectPressColor = System.Drawing.Color.Black;
            this.btnStart.RectSelectedColor = System.Drawing.Color.Black;
            this.btnStart.Size = new System.Drawing.Size(100, 35);
            this.btnStart.Style = Sunny.UI.UIStyle.Custom;
            this.btnStart.StyleCustomMode = true;
            this.btnStart.TabIndex = 7;
            this.btnStart.Text = "开始";
            this.btnStart.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // uiStyleManager1
            // 
            this.uiStyleManager1.Style = Sunny.UI.UIStyle.Black;
            // 
            // livecore
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(935, 593);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.richTextBox1);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "livecore";
            this.PageIndex = 1002;
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.StyleCustomMode = true;
            this.Text = "livecore";
            this.ResumeLayout(false);

        }

        #endregion
        private Sunny.UI.UIRichTextBox richTextBox1;
        private Sunny.UI.UIButton btnStop;
        private Sunny.UI.UIButton btnStart;
        private Sunny.UI.UIStyleManager uiStyleManager1;
    }
}