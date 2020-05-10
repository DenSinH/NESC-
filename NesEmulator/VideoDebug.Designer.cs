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
            this.PatternTable0 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.PatternTable1 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.PatternTableLabel = new System.Windows.Forms.Label();
            this.OAMLabel = new System.Windows.Forms.Label();
            this.Palette0 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette1 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette2 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette3 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette4 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette5 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette6 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.Palette7 = new NesEmulator.PictureBoxWithInterpolationMode();
            this.OAMText = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette7)).BeginInit();
            this.SuspendLayout();
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
            // Palette6
            // 
            this.Palette6.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.Palette6.Location = new System.Drawing.Point(276, 335);
            this.Palette6.Name = "Palette6";
            this.Palette6.Size = new System.Drawing.Size(124, 31);
            this.Palette6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Palette6.TabIndex = 12;
            this.Palette6.TabStop = false;
            this.Palette6.Click += new System.EventHandler(this.Palette6_Click);
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
            // OAMText
            // 
            this.OAMText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.OAMText.Font = new System.Drawing.Font("Courier New", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OAMText.Location = new System.Drawing.Point(536, 36);
            this.OAMText.Multiline = true;
            this.OAMText.Name = "OAMText";
            this.OAMText.ReadOnly = true;
            this.OAMText.Size = new System.Drawing.Size(358, 510);
            this.OAMText.TabIndex = 14;
            this.OAMText.WordWrap = false;
            // 
            // VideoDebug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(906, 558);
            this.Controls.Add(this.OAMText);
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
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Palette7)).EndInit();
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
    }
}