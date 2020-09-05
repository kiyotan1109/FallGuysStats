﻿namespace FallGuysStats {
    partial class Overlay {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lblStreak = new FallGuysStats.TransparentLabel();
            this.lblFinalChance = new FallGuysStats.TransparentLabel();
            this.lblQualifyChance = new FallGuysStats.TransparentLabel();
            this.lblFastest = new FallGuysStats.TransparentLabel();
            this.lblDuration = new FallGuysStats.TransparentLabel();
            this.lblPlayers = new FallGuysStats.TransparentLabel();
            this.lblName = new FallGuysStats.TransparentLabel();
            this.lblWins = new FallGuysStats.TransparentLabel();
            this.lblFinish = new FallGuysStats.TransparentLabel();
            this.SuspendLayout();
            // 
            // lblStreak
            // 
            this.lblStreak.Location = new System.Drawing.Point(21, 55);
            this.lblStreak.Name = "lblStreak";
            this.lblStreak.Size = new System.Drawing.Size(238, 22);
            this.lblStreak.TabIndex = 21;
            this.lblStreak.Text = "STREAK:";
            this.lblStreak.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblStreak.TextRight = "0 (BEST 0)";
            // 
            // lblFinalChance
            // 
            this.lblFinalChance.Location = new System.Drawing.Point(21, 32);
            this.lblFinalChance.Name = "lblFinalChance";
            this.lblFinalChance.Size = new System.Drawing.Size(238, 22);
            this.lblFinalChance.TabIndex = 5;
            this.lblFinalChance.Text = "FINAL:";
            this.lblFinalChance.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblFinalChance.TextRight = "0 - 0.0%";
            // 
            // lblQualifyChance
            // 
            this.lblQualifyChance.Location = new System.Drawing.Point(260, 32);
            this.lblQualifyChance.Name = "lblQualifyChance";
            this.lblQualifyChance.Size = new System.Drawing.Size(297, 22);
            this.lblQualifyChance.TabIndex = 14;
            this.lblQualifyChance.Text = "QUALIFY:";
            this.lblQualifyChance.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblQualifyChance.TextRight = "0 / 0 - 0.0%";
            // 
            // lblFastest
            // 
            this.lblFastest.Location = new System.Drawing.Point(260, 55);
            this.lblFastest.Name = "lblFastest";
            this.lblFastest.Size = new System.Drawing.Size(297, 22);
            this.lblFastest.TabIndex = 16;
            this.lblFastest.Text = "FASTEST:";
            this.lblFastest.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblFastest.TextRight = "-";
            // 
            // lblDuration
            // 
            this.lblDuration.Location = new System.Drawing.Point(558, 32);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(225, 22);
            this.lblDuration.TabIndex = 18;
            this.lblDuration.Text = "TIME:";
            this.lblDuration.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblDuration.TextRight = "-";
            // 
            // lblPlayers
            // 
            this.lblPlayers.Location = new System.Drawing.Point(558, 9);
            this.lblPlayers.Name = "lblPlayers";
            this.lblPlayers.Size = new System.Drawing.Size(225, 22);
            this.lblPlayers.TabIndex = 12;
            this.lblPlayers.Text = "PLAYERS:";
            this.lblPlayers.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblPlayers.TextRight = "0";
            // 
            // lblName
            // 
            this.lblName.AutoEllipsis = true;
            this.lblName.Location = new System.Drawing.Point(260, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(297, 22);
            this.lblName.TabIndex = 10;
            this.lblName.Text = "ROUND 1:";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblName.TextRight = "N/A";
            // 
            // lblWins
            // 
            this.lblWins.Location = new System.Drawing.Point(21, 9);
            this.lblWins.Name = "lblWins";
            this.lblWins.Size = new System.Drawing.Size(238, 22);
            this.lblWins.TabIndex = 1;
            this.lblWins.Text = "WINS:";
            this.lblWins.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblWins.TextRight = "0 - 0.0%";
            // 
            // lblFinish
            // 
            this.lblFinish.Location = new System.Drawing.Point(558, 55);
            this.lblFinish.Name = "lblFinish";
            this.lblFinish.Size = new System.Drawing.Size(225, 22);
            this.lblFinish.TabIndex = 20;
            this.lblFinish.Text = "FINISH:";
            this.lblFinish.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblFinish.TextRight = "-";
            // 
            // Overlay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Magenta;
            this.BackgroundImage = global::FallGuysStats.Properties.Resources.background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(786, 99);
            this.Controls.Add(this.lblStreak);
            this.Controls.Add(this.lblFinalChance);
            this.Controls.Add(this.lblQualifyChance);
            this.Controls.Add(this.lblFastest);
            this.Controls.Add(this.lblDuration);
            this.Controls.Add(this.lblPlayers);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblWins);
            this.Controls.Add(this.lblFinish);
            this.Cursor = System.Windows.Forms.Cursors.SizeAll;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "Overlay";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Overlay";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Overlay_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Overlay_MouseDown);
            this.ResumeLayout(false);

        }

        #endregion
        private TransparentLabel lblName;
        private TransparentLabel lblDuration;
        private TransparentLabel lblFinish;
        private TransparentLabel lblFastest;
        private TransparentLabel lblQualifyChance;
        private TransparentLabel lblWins;
        private TransparentLabel lblFinalChance;
        private TransparentLabel lblPlayers;
        private TransparentLabel lblStreak;
    }
}