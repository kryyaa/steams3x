namespace steamsex
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkTextBox = new System.Windows.Forms.RichTextBox();
            this.installButton = new System.Windows.Forms.Button();
            this.uninstallButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.openTgChannelButton = new System.Windows.Forms.Button();
            this.repairButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AutoDetectSteam = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.linkTextBox);
            this.groupBox1.Controls.Add(this.installButton);
            this.groupBox1.Controls.Add(this.uninstallButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(888, 129);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "[steams3x tool]";
            // 
            // linkTextBox
            // 
            this.linkTextBox.Location = new System.Drawing.Point(6, 21);
            this.linkTextBox.Name = "linkTextBox";
            this.linkTextBox.Size = new System.Drawing.Size(714, 96);
            this.linkTextBox.TabIndex = 2;
            this.linkTextBox.Text = "steam link #1\nsteam link #2\nsteam link #3";
            this.linkTextBox.TextChanged += new System.EventHandler(this.linkTextBox_TextChanged);
            // 
            // installButton
            // 
            this.installButton.Location = new System.Drawing.Point(726, 21);
            this.installButton.Name = "installButton";
            this.installButton.Size = new System.Drawing.Size(156, 43);
            this.installButton.TabIndex = 1;
            this.installButton.Text = "install";
            this.installButton.UseVisualStyleBackColor = true;
            this.installButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // uninstallButton
            // 
            this.uninstallButton.Location = new System.Drawing.Point(726, 76);
            this.uninstallButton.Name = "uninstallButton";
            this.uninstallButton.Size = new System.Drawing.Size(156, 43);
            this.uninstallButton.TabIndex = 0;
            this.uninstallButton.Text = "uninstall";
            this.uninstallButton.UseVisualStyleBackColor = true;
            this.uninstallButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.AutoDetectSteam);
            this.groupBox2.Controls.Add(this.openTgChannelButton);
            this.groupBox2.Controls.Add(this.repairButton);
            this.groupBox2.Location = new System.Drawing.Point(12, 147);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(345, 125);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "[misc]";
            // 
            // openTgChannelButton
            // 
            this.openTgChannelButton.Location = new System.Drawing.Point(6, 73);
            this.openTgChannelButton.Name = "openTgChannelButton";
            this.openTgChannelButton.Size = new System.Drawing.Size(185, 46);
            this.openTgChannelButton.TabIndex = 2;
            this.openTgChannelButton.Text = "telegram channel";
            this.openTgChannelButton.UseVisualStyleBackColor = true;
            this.openTgChannelButton.Click += new System.EventHandler(this.openTgChannelButton_Click);
            // 
            // repairButton
            // 
            this.repairButton.Location = new System.Drawing.Point(6, 21);
            this.repairButton.Name = "repairButton";
            this.repairButton.Size = new System.Drawing.Size(185, 46);
            this.repairButton.TabIndex = 0;
            this.repairButton.Text = "repair";
            this.repairButton.UseVisualStyleBackColor = true;
            this.repairButton.Click += new System.EventHandler(this.repairButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(363, 147);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(531, 125);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "[about]";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(105, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "информативный about текст";
            // 
            // AutoDetectSteam
            // 
            this.AutoDetectSteam.AutoSize = true;
            this.AutoDetectSteam.Location = new System.Drawing.Point(197, 87);
            this.AutoDetectSteam.Name = "AutoDetectSteam";
            this.AutoDetectSteam.Size = new System.Drawing.Size(138, 20);
            this.AutoDetectSteam.TabIndex = 3;
            this.AutoDetectSteam.Text = "Auto Detect steam";
            this.AutoDetectSteam.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(912, 284);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "[steams3x]";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button installButton;
        private System.Windows.Forms.Button uninstallButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button repairButton;
        private System.Windows.Forms.RichTextBox linkTextBox;
        private System.Windows.Forms.Button openTgChannelButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox AutoDetectSteam;
    }
}

