// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace chdk_ptp_test
{
    partial class Form1
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
            this.devicecombobox = new System.Windows.Forms.ComboBox();
            this.connect_button = new System.Windows.Forms.Button();
            this.refreshbutton = new System.Windows.Forms.Button();
            this.disconnectbutton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.getimagebutton = new System.Windows.Forms.Button();
            this.recordbutton = new System.Windows.Forms.Button();
            this.playbackbutton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // devicecombobox
            // 
            this.devicecombobox.FormattingEnabled = true;
            this.devicecombobox.Location = new System.Drawing.Point(12, 12);
            this.devicecombobox.Name = "devicecombobox";
            this.devicecombobox.Size = new System.Drawing.Size(314, 21);
            this.devicecombobox.TabIndex = 0;
            // 
            // connect_button
            // 
            this.connect_button.Location = new System.Drawing.Point(332, 10);
            this.connect_button.Name = "connect_button";
            this.connect_button.Size = new System.Drawing.Size(75, 23);
            this.connect_button.TabIndex = 1;
            this.connect_button.Text = "Connect";
            this.connect_button.UseVisualStyleBackColor = true;
            this.connect_button.Click += new System.EventHandler(this.connectbutton_Click);
            // 
            // refreshbutton
            // 
            this.refreshbutton.Location = new System.Drawing.Point(413, 10);
            this.refreshbutton.Name = "refreshbutton";
            this.refreshbutton.Size = new System.Drawing.Size(75, 23);
            this.refreshbutton.TabIndex = 2;
            this.refreshbutton.Text = "Refresh List";
            this.refreshbutton.UseVisualStyleBackColor = true;
            this.refreshbutton.Click += new System.EventHandler(this.refreshbutton_Click);
            // 
            // disconnectbutton
            // 
            this.disconnectbutton.Location = new System.Drawing.Point(332, 39);
            this.disconnectbutton.Name = "disconnectbutton";
            this.disconnectbutton.Size = new System.Drawing.Size(75, 23);
            this.disconnectbutton.TabIndex = 3;
            this.disconnectbutton.Text = "Disconnect";
            this.disconnectbutton.UseVisualStyleBackColor = true;
            this.disconnectbutton.Click += new System.EventHandler(this.disconnectbutton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Not connected";
            // 
            // getimagebutton
            // 
            this.getimagebutton.Location = new System.Drawing.Point(12, 68);
            this.getimagebutton.Name = "getimagebutton";
            this.getimagebutton.Size = new System.Drawing.Size(75, 23);
            this.getimagebutton.TabIndex = 5;
            this.getimagebutton.Text = "Get Image";
            this.getimagebutton.UseVisualStyleBackColor = true;
            this.getimagebutton.Click += new System.EventHandler(this.getimagebutton_Click);
            // 
            // recordbutton
            // 
            this.recordbutton.Location = new System.Drawing.Point(170, 68);
            this.recordbutton.Name = "recordbutton";
            this.recordbutton.Size = new System.Drawing.Size(75, 23);
            this.recordbutton.TabIndex = 6;
            this.recordbutton.Text = "Record";
            this.recordbutton.UseVisualStyleBackColor = true;
            this.recordbutton.Click += new System.EventHandler(this.recordbutton_Click);
            // 
            // playbackbutton
            // 
            this.playbackbutton.Location = new System.Drawing.Point(251, 68);
            this.playbackbutton.Name = "playbackbutton";
            this.playbackbutton.Size = new System.Drawing.Size(75, 23);
            this.playbackbutton.TabIndex = 7;
            this.playbackbutton.Text = "Playback";
            this.playbackbutton.UseVisualStyleBackColor = true;
            this.playbackbutton.Click += new System.EventHandler(this.playbackbutton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(127, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Mode:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 372);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.playbackbutton);
            this.Controls.Add(this.recordbutton);
            this.Controls.Add(this.getimagebutton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.disconnectbutton);
            this.Controls.Add(this.refreshbutton);
            this.Controls.Add(this.connect_button);
            this.Controls.Add(this.devicecombobox);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "CHDK PTP Test";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox devicecombobox;
        private System.Windows.Forms.Button connect_button;
        private System.Windows.Forms.Button refreshbutton;
        private System.Windows.Forms.Button disconnectbutton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button getimagebutton;
        private System.Windows.Forms.Button recordbutton;
        private System.Windows.Forms.Button playbackbutton;
        private System.Windows.Forms.Label label2;
    }
}

