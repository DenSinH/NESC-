using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NesEmulator
{
    public class PictureBoxWithInterpolationMode : PictureBox
    {
        public InterpolationMode InterpolationMode { get; set; }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
            base.OnPaint(paintEventArgs);
        }
    }

    partial class VideoDebug
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
            this.PatternTableLabel = new System.Windows.Forms.Label();
            this.OAMLabel = new System.Windows.Forms.Label();
            this.OAMText = new System.Windows.Forms.TextBox();
            this.OAMPanel = new System.Windows.Forms.Panel();
            this.CPULabel = new System.Windows.Forms.Label();
            this.ALabel = new System.Windows.Forms.Label();
            this.AContent = new System.Windows.Forms.Label();
            this.XContent = new System.Windows.Forms.Label();
            this.XLabel = new System.Windows.Forms.Label();
            this.YContent = new System.Windows.Forms.Label();
            this.YLabel = new System.Windows.Forms.Label();
            this.PCContent = new System.Windows.Forms.Label();
            this.PCLabel = new System.Windows.Forms.Label();
            this.SRLabel = new System.Windows.Forms.Label();
            this.NFlag = new System.Windows.Forms.Label();
            this.VFlag = new System.Windows.Forms.Label();
            this.BFlag = new System.Windows.Forms.Label();
            this._Flag = new System.Windows.Forms.Label();
            this.CFlag = new System.Windows.Forms.Label();
            this.ZFlag = new System.Windows.Forms.Label();
            this.IFlag = new System.Windows.Forms.Label();
            this.DFlag = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Palette7 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette6 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette5 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette4 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette3 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette2 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette1 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette0 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.PatternTable1 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.PatternTable0 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.OAMPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Palette7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable0)).BeginInit();
            this.SuspendLayout();
            // 
            // PatternTableLabel
            // 
            this.PatternTableLabel.AutoSize = true;
            this.PatternTableLabel.Location = new System.Drawing.Point(9, 20);
            this.PatternTableLabel.Name = "PatternTableLabel";
            this.PatternTableLabel.Size = new System.Drawing.Size(76, 13);
            this.PatternTableLabel.TabIndex = 2;
            this.PatternTableLabel.Text = "Pattern Tables";
            // 
            // OAMLabel
            // 
            this.OAMLabel.AutoSize = true;
            this.OAMLabel.Location = new System.Drawing.Point(533, 20);
            this.OAMLabel.Name = "OAMLabel";
            this.OAMLabel.Size = new System.Drawing.Size(31, 13);
            this.OAMLabel.TabIndex = 3;
            this.OAMLabel.Text = "OAM";
            // 
            // OAMText
            // 
            this.OAMText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.OAMText.Cursor = System.Windows.Forms.Cursors.Default;
            this.OAMText.Enabled = false;
            this.OAMText.Font = new System.Drawing.Font("Courier New", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OAMText.Location = new System.Drawing.Point(3, 3);
            this.OAMText.Multiline = true;
            this.OAMText.Name = "OAMText";
            this.OAMText.ReadOnly = true;
            this.OAMText.Size = new System.Drawing.Size(335, 1665);
            this.OAMText.TabIndex = 14;
            this.OAMText.WordWrap = false;
            // 
            // OAMPanel
            // 
            this.OAMPanel.AutoScroll = true;
            this.OAMPanel.Controls.Add(this.OAMText);
            this.OAMPanel.Location = new System.Drawing.Point(536, 36);
            this.OAMPanel.Name = "OAMPanel";
            this.OAMPanel.Size = new System.Drawing.Size(358, 510);
            this.OAMPanel.TabIndex = 15;
            // 
            // CPULabel
            // 
            this.CPULabel.AutoSize = true;
            this.CPULabel.Location = new System.Drawing.Point(12, 369);
            this.CPULabel.Name = "CPULabel";
            this.CPULabel.Size = new System.Drawing.Size(29, 13);
            this.CPULabel.TabIndex = 16;
            this.CPULabel.Text = "CPU";
            // 
            // ALabel
            // 
            this.ALabel.AutoSize = true;
            this.ALabel.Location = new System.Drawing.Point(12, 382);
            this.ALabel.Name = "ALabel";
            this.ALabel.Size = new System.Drawing.Size(14, 13);
            this.ALabel.TabIndex = 17;
            this.ALabel.Text = "A";
            // 
            // AContent
            // 
            this.AContent.AutoSize = true;
            this.AContent.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AContent.Location = new System.Drawing.Point(32, 382);
            this.AContent.Name = "AContent";
            this.AContent.Size = new System.Drawing.Size(21, 14);
            this.AContent.TabIndex = 18;
            this.AContent.Text = "00";
            // 
            // XContent
            // 
            this.XContent.AutoSize = true;
            this.XContent.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.XContent.Location = new System.Drawing.Point(32, 395);
            this.XContent.Name = "XContent";
            this.XContent.Size = new System.Drawing.Size(21, 14);
            this.XContent.TabIndex = 20;
            this.XContent.Text = "00";
            // 
            // XLabel
            // 
            this.XLabel.AutoSize = true;
            this.XLabel.Location = new System.Drawing.Point(12, 395);
            this.XLabel.Name = "XLabel";
            this.XLabel.Size = new System.Drawing.Size(14, 13);
            this.XLabel.TabIndex = 19;
            this.XLabel.Text = "X";
            // 
            // YContent
            // 
            this.YContent.AutoSize = true;
            this.YContent.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YContent.Location = new System.Drawing.Point(32, 408);
            this.YContent.Name = "YContent";
            this.YContent.Size = new System.Drawing.Size(21, 14);
            this.YContent.TabIndex = 22;
            this.YContent.Text = "00";
            // 
            // YLabel
            // 
            this.YLabel.AutoSize = true;
            this.YLabel.Location = new System.Drawing.Point(12, 408);
            this.YLabel.Name = "YLabel";
            this.YLabel.Size = new System.Drawing.Size(14, 13);
            this.YLabel.TabIndex = 21;
            this.YLabel.Text = "Y";
            // 
            // PCContent
            // 
            this.PCContent.AutoSize = true;
            this.PCContent.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PCContent.Location = new System.Drawing.Point(32, 421);
            this.PCContent.Name = "PCContent";
            this.PCContent.Size = new System.Drawing.Size(35, 14);
            this.PCContent.TabIndex = 24;
            this.PCContent.Text = "0000";
            // 
            // PCLabel
            // 
            this.PCLabel.AutoSize = true;
            this.PCLabel.Location = new System.Drawing.Point(12, 421);
            this.PCLabel.Name = "PCLabel";
            this.PCLabel.Size = new System.Drawing.Size(21, 13);
            this.PCLabel.TabIndex = 23;
            this.PCLabel.Text = "PC";
            // 
            // SRLabel
            // 
            this.SRLabel.AutoSize = true;
            this.SRLabel.Location = new System.Drawing.Point(12, 446);
            this.SRLabel.Name = "SRLabel";
            this.SRLabel.Size = new System.Drawing.Size(79, 13);
            this.SRLabel.TabIndex = 25;
            this.SRLabel.Text = "Status Register";
            // 
            // NFlag
            // 
            this.NFlag.AutoSize = true;
            this.NFlag.ForeColor = System.Drawing.Color.Red;
            this.NFlag.Location = new System.Drawing.Point(12, 459);
            this.NFlag.Name = "NFlag";
            this.NFlag.Size = new System.Drawing.Size(15, 13);
            this.NFlag.TabIndex = 26;
            this.NFlag.Text = "N";
            // 
            // VFlag
            // 
            this.VFlag.AutoSize = true;
            this.VFlag.ForeColor = System.Drawing.Color.Red;
            this.VFlag.Location = new System.Drawing.Point(24, 459);
            this.VFlag.Name = "VFlag";
            this.VFlag.Size = new System.Drawing.Size(14, 13);
            this.VFlag.TabIndex = 27;
            this.VFlag.Text = "V";
            // 
            // BFlag
            // 
            this.BFlag.AutoSize = true;
            this.BFlag.ForeColor = System.Drawing.Color.Red;
            this.BFlag.Location = new System.Drawing.Point(45, 459);
            this.BFlag.Name = "BFlag";
            this.BFlag.Size = new System.Drawing.Size(14, 13);
            this.BFlag.TabIndex = 29;
            this.BFlag.Text = "B";
            // 
            // _Flag
            // 
            this._Flag.AutoSize = true;
            this._Flag.ForeColor = System.Drawing.Color.Green;
            this._Flag.Location = new System.Drawing.Point(36, 459);
            this._Flag.Name = "_Flag";
            this._Flag.Size = new System.Drawing.Size(10, 13);
            this._Flag.TabIndex = 28;
            this._Flag.Text = "-";
            // 
            // CFlag
            // 
            this.CFlag.AutoSize = true;
            this.CFlag.ForeColor = System.Drawing.Color.Red;
            this.CFlag.Location = new System.Drawing.Point(94, 459);
            this.CFlag.Name = "CFlag";
            this.CFlag.Size = new System.Drawing.Size(14, 13);
            this.CFlag.TabIndex = 33;
            this.CFlag.Text = "C";
            // 
            // ZFlag
            // 
            this.ZFlag.AutoSize = true;
            this.ZFlag.ForeColor = System.Drawing.Color.Red;
            this.ZFlag.Location = new System.Drawing.Point(83, 459);
            this.ZFlag.Name = "ZFlag";
            this.ZFlag.Size = new System.Drawing.Size(14, 13);
            this.ZFlag.TabIndex = 32;
            this.ZFlag.Text = "Z";
            // 
            // IFlag
            // 
            this.IFlag.AutoSize = true;
            this.IFlag.ForeColor = System.Drawing.Color.Red;
            this.IFlag.Location = new System.Drawing.Point(70, 459);
            this.IFlag.Name = "IFlag";
            this.IFlag.Size = new System.Drawing.Size(10, 13);
            this.IFlag.TabIndex = 31;
            this.IFlag.Text = "I";
            // 
            // DFlag
            // 
            this.DFlag.AutoSize = true;
            this.DFlag.ForeColor = System.Drawing.Color.Green;
            this.DFlag.Location = new System.Drawing.Point(63, 459);
            this.DFlag.Name = "DFlag";
            this.DFlag.Size = new System.Drawing.Size(15, 13);
            this.DFlag.TabIndex = 30;
            this.DFlag.Text = "D";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Green;
            this.label1.Location = new System.Drawing.Point(58, 459);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "D";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(78, 459);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Z";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(89, 459);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 33;
            this.label3.Text = "C";
            // 
            // Palette7
            // 
            this.Palette7.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette7.Location = new System.Drawing.Point(406, 335);
            this.Palette7.Name = "Palette7";
            this.Palette7.Size = new System.Drawing.Size(124, 31);
            this.Palette7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Palette7.TabIndex = 13;
            this.Palette7.TabStop = false;
            this.Palette7.Click += new System.EventHandler(this.Palette7_Click);
            // 
            // Palette6
            // 
            this.Palette6.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette6.Location = new System.Drawing.Point(274, 335);
            this.Palette6.Name = "Palette6";
            this.Palette6.Size = new System.Drawing.Size(124, 31);
            this.Palette6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Palette6.TabIndex = 12;
            this.Palette6.TabStop = false;
            this.Palette6.Click += new System.EventHandler(this.Palette6_Click);
            // 
            // Palette5
            // 
            this.Palette5.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette5.Location = new System.Drawing.Point(144, 335);
            this.Palette5.Name = "Palette5";
            this.Palette5.Size = new System.Drawing.Size(124, 31);
            this.Palette5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Palette5.TabIndex = 11;
            this.Palette5.TabStop = false;
            this.Palette5.Click += new System.EventHandler(this.Palette5_Click);
            // 
            // Palette4
            // 
            this.Palette4.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette4.Location = new System.Drawing.Point(12, 335);
            this.Palette4.Name = "Palette4";
            this.Palette4.Size = new System.Drawing.Size(124, 31);
            this.Palette4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Palette4.TabIndex = 10;
            this.Palette4.TabStop = false;
            this.Palette4.Click += new System.EventHandler(this.Palette4_Click);
            // 
            // Palette3
            // 
            this.Palette3.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette3.Location = new System.Drawing.Point(406, 298);
            this.Palette3.Name = "Palette3";
            this.Palette3.Size = new System.Drawing.Size(124, 31);
            this.Palette3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Palette3.TabIndex = 9;
            this.Palette3.TabStop = false;
            this.Palette3.Click += new System.EventHandler(this.Palette3_Click);
            // 
            // Palette2
            // 
            this.Palette2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette2.Location = new System.Drawing.Point(274, 298);
            this.Palette2.Name = "Palette2";
            this.Palette2.Size = new System.Drawing.Size(124, 31);
            this.Palette2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Palette2.TabIndex = 8;
            this.Palette2.TabStop = false;
            this.Palette2.Click += new System.EventHandler(this.Palette2_Click);
            // 
            // Palette1
            // 
            this.Palette1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette1.Location = new System.Drawing.Point(144, 298);
            this.Palette1.Name = "Palette1";
            this.Palette1.Size = new System.Drawing.Size(124, 31);
            this.Palette1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Palette1.TabIndex = 7;
            this.Palette1.TabStop = false;
            this.Palette1.Click += new System.EventHandler(this.Palette1_Click);
            // 
            // Palette0
            // 
            this.Palette0.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette0.Location = new System.Drawing.Point(12, 298);
            this.Palette0.Name = "Palette0";
            this.Palette0.Size = new System.Drawing.Size(124, 31);
            this.Palette0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Palette0.TabIndex = 6;
            this.Palette0.TabStop = false;
            this.Palette0.Click += new System.EventHandler(this.Palette0_Click);
            // 
            // PatternTable1
            // 
            this.PatternTable1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.PatternTable1.Location = new System.Drawing.Point(274, 36);
            this.PatternTable1.Name = "PatternTable1";
            this.PatternTable1.Size = new System.Drawing.Size(256, 256);
            this.PatternTable1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PatternTable1.TabIndex = 1;
            this.PatternTable1.TabStop = false;
            // 
            // PatternTable0
            // 
            this.PatternTable0.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.PatternTable0.Location = new System.Drawing.Point(12, 36);
            this.PatternTable0.Name = "PatternTable0";
            this.PatternTable0.Size = new System.Drawing.Size(256, 256);
            this.PatternTable0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PatternTable0.TabIndex = 0;
            this.PatternTable0.TabStop = false;
            // 
            // VideoDebug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(906, 558);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CFlag);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ZFlag);
            this.Controls.Add(this.IFlag);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DFlag);
            this.Controls.Add(this.BFlag);
            this.Controls.Add(this._Flag);
            this.Controls.Add(this.VFlag);
            this.Controls.Add(this.NFlag);
            this.Controls.Add(this.SRLabel);
            this.Controls.Add(this.PCContent);
            this.Controls.Add(this.PCLabel);
            this.Controls.Add(this.YContent);
            this.Controls.Add(this.YLabel);
            this.Controls.Add(this.XContent);
            this.Controls.Add(this.XLabel);
            this.Controls.Add(this.AContent);
            this.Controls.Add(this.ALabel);
            this.Controls.Add(this.CPULabel);
            this.Controls.Add(this.OAMPanel);
            this.Controls.Add(this.Palette7);
            this.Controls.Add(this.Palette6);
            this.Controls.Add(this.Palette5);
            this.Controls.Add(this.Palette4);
            this.Controls.Add(this.Palette3);
            this.Controls.Add(this.Palette2);
            this.Controls.Add(this.Palette1);
            this.Controls.Add(this.Palette0);
            this.Controls.Add(this.OAMLabel);
            this.Controls.Add(this.PatternTableLabel);
            this.Controls.Add(this.PatternTable1);
            this.Controls.Add(this.PatternTable0);
            this.Name = "VideoDebug";
            this.Text = "VideoDebug";
            this.Load += new System.EventHandler(this.VideoDebug_Load);
            this.OAMPanel.ResumeLayout(false);
            this.OAMPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Palette7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable0)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBoxWithInterpolationMode PatternTable0;
        private PictureBoxWithInterpolationMode PatternTable1;
        private System.Windows.Forms.Label PatternTableLabel;
        private System.Windows.Forms.Label OAMLabel;
        private PictureBoxWithInterpolationMode Palette0;
        private PictureBoxWithInterpolationMode Palette1;
        private PictureBoxWithInterpolationMode Palette2;
        private PictureBoxWithInterpolationMode Palette3;
        private PictureBoxWithInterpolationMode Palette4;
        private PictureBoxWithInterpolationMode Palette5;
        private PictureBoxWithInterpolationMode Palette6;
        private PictureBoxWithInterpolationMode Palette7;
        private TextBox OAMText;
        private Panel OAMPanel;
        private Label CPULabel;
        private Label ALabel;
        private Label AContent;
        private Label XContent;
        private Label XLabel;
        private Label YContent;
        private Label YLabel;
        private Label PCContent;
        private Label PCLabel;
        private Label SRLabel;
        private Label NFlag;
        private Label VFlag;
        private Label BFlag;
        private Label _Flag;
        private Label CFlag;
        private Label ZFlag;
        private Label IFlag;
        private Label DFlag;
        private Label label1;
        private Label label2;
        private Label label3;
    }
}