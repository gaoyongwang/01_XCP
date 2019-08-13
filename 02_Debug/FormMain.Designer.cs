namespace WindowsFormsApplication
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Init = new System.Windows.Forms.Button();
            this.DeInit = new System.Windows.Forms.Button();
            this.button_Connect = new System.Windows.Forms.Button();
            this.textBox_Log = new System.Windows.Forms.TextBox();
            this.timer_Read = new System.Windows.Forms.Timer(this.components);
            this.button_Disconnect = new System.Windows.Forms.Button();
            this.numericUpDown_UV = new System.Windows.Forms.NumericUpDown();
            this.button_Start = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_UV)).BeginInit();
            this.SuspendLayout();
            // 
            // Init
            // 
            this.Init.Location = new System.Drawing.Point(12, 12);
            this.Init.Name = "Init";
            this.Init.Size = new System.Drawing.Size(75, 23);
            this.Init.TabIndex = 0;
            this.Init.Text = "Init";
            this.Init.UseVisualStyleBackColor = true;
            this.Init.Click += new System.EventHandler(this.Init_Click);
            // 
            // DeInit
            // 
            this.DeInit.Location = new System.Drawing.Point(93, 12);
            this.DeInit.Name = "DeInit";
            this.DeInit.Size = new System.Drawing.Size(75, 23);
            this.DeInit.TabIndex = 1;
            this.DeInit.Text = "DeInit";
            this.DeInit.UseVisualStyleBackColor = true;
            this.DeInit.Click += new System.EventHandler(this.DeInit_Click);
            // 
            // button_Connect
            // 
            this.button_Connect.Location = new System.Drawing.Point(12, 41);
            this.button_Connect.Name = "button_Connect";
            this.button_Connect.Size = new System.Drawing.Size(75, 23);
            this.button_Connect.TabIndex = 2;
            this.button_Connect.Text = "Connect";
            this.button_Connect.UseVisualStyleBackColor = true;
            this.button_Connect.Click += new System.EventHandler(this.button_Connect_Click);
            // 
            // textBox_Log
            // 
            this.textBox_Log.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Log.BackColor = System.Drawing.Color.Black;
            this.textBox_Log.ForeColor = System.Drawing.Color.Lime;
            this.textBox_Log.Location = new System.Drawing.Point(12, 186);
            this.textBox_Log.Multiline = true;
            this.textBox_Log.Name = "textBox_Log";
            this.textBox_Log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_Log.Size = new System.Drawing.Size(260, 63);
            this.textBox_Log.TabIndex = 3;
            // 
            // timer_Read
            // 
            this.timer_Read.Interval = 1000;
            this.timer_Read.Tick += new System.EventHandler(this.timer_Read_Tick);
            // 
            // button_Disconnect
            // 
            this.button_Disconnect.Location = new System.Drawing.Point(93, 40);
            this.button_Disconnect.Name = "button_Disconnect";
            this.button_Disconnect.Size = new System.Drawing.Size(75, 23);
            this.button_Disconnect.TabIndex = 4;
            this.button_Disconnect.Text = "Disconnect";
            this.button_Disconnect.UseVisualStyleBackColor = true;
            this.button_Disconnect.Click += new System.EventHandler(this.button_Disconnect_Click);
            // 
            // numericUpDown_UV
            // 
            this.numericUpDown_UV.DecimalPlaces = 2;
            this.numericUpDown_UV.Location = new System.Drawing.Point(13, 71);
            this.numericUpDown_UV.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_UV.Name = "numericUpDown_UV";
            this.numericUpDown_UV.Size = new System.Drawing.Size(120, 21);
            this.numericUpDown_UV.TabIndex = 5;
            this.numericUpDown_UV.ValueChanged += new System.EventHandler(this.numericUpDown_UV_ValueChanged);
            // 
            // button_Start
            // 
            this.button_Start.Location = new System.Drawing.Point(174, 41);
            this.button_Start.Name = "button_Start";
            this.button_Start.Size = new System.Drawing.Size(75, 23);
            this.button_Start.TabIndex = 6;
            this.button_Start.Text = "Start";
            this.button_Start.UseVisualStyleBackColor = true;
            this.button_Start.Click += new System.EventHandler(this.button_Start_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.button_Start);
            this.Controls.Add(this.numericUpDown_UV);
            this.Controls.Add(this.button_Disconnect);
            this.Controls.Add(this.textBox_Log);
            this.Controls.Add(this.button_Connect);
            this.Controls.Add(this.DeInit);
            this.Controls.Add(this.Init);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "test";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_UV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Init;
        private System.Windows.Forms.Button DeInit;
        private System.Windows.Forms.Button button_Connect;
        private System.Windows.Forms.TextBox textBox_Log;
        private System.Windows.Forms.Timer timer_Read;
        private System.Windows.Forms.Button button_Disconnect;
        private System.Windows.Forms.NumericUpDown numericUpDown_UV;
        private System.Windows.Forms.Button button_Start;
    }
}

