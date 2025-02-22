﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Controls;
using ScottPlot;
using ScottPlot.Plottable;

namespace FallGuysStats {
    public partial class RoundStatsDisplay : MetroFramework.Forms.MetroForm {
        public Stats StatsForm { get; set; }
        public Dictionary<string, double[]> roundGraphData;
        public Dictionary<string, TimeSpan> roundDurationData;
        //public Dictionary<string, double[]> roundRecordData;
        public Dictionary<string, int[]> roundScoreData;
        public IOrderedEnumerable<KeyValuePair<string, string>> roundList;
        private RadialGaugePlot radialGauges;
        private readonly string[] labelList = {
            Multilingual.GetWord("main_played"), Multilingual.GetWord("level_detail_gold"),
            Multilingual.GetWord("level_detail_silver"), Multilingual.GetWord("level_detail_bronze"),
            Multilingual.GetWord("level_detail_pink"), Multilingual.GetWord("level_detail_eliminated")
        };
        private bool isInitComplete;
        private string goldMedalCount, silverMedalCount, bronzeMedalCount, pinkMedalCount, eliminatedMedalCount;
        private string goldMedalPercent, silverMedalPercent, bronzeMedalPercent, pinkMedalPercent, eliminatedMedalPercent;
        public RoundStatsDisplay() {
            this.InitializeComponent();
            this.Opacity = 0;
        }
        
        private class CustomPalette : IPalette {
            public string Name { get; } = "Custom Palette";

            public string Description { get; } = "Custom Palette";

            public Color[] Colors { get; } = {
                Color.FromArgb(31, 119, 180), Color.FromArgb(255, 215, 0), Color.FromArgb(192, 192, 192), Color.FromArgb(205, 127, 50), Color.FromArgb(255, 20, 147),
                Color.FromArgb(128, 0, 128), Color.FromArgb(227, 119, 194), Color.FromArgb(127, 127, 127), Color.FromArgb(188, 189, 34), Color.FromArgb(23, 190, 207)
            };
        }

        private void RoundStatsDisplay_Load(object sender, EventArgs e) {
            this.SetTheme(Stats.CurrentTheme);
            this.ChangeLanguage();
            
            this.cboRoundList.DataSource = new BindingSource(this.roundList, null);
            this.cboRoundList.DisplayMember = "Value";
            this.cboRoundList.ValueMember = "Key";

            this.formsPlot.Plot.Legend(location: Alignment.UpperRight);
            this.SetGraph();
            this.isInitComplete = true;
        }

        private void RoundStatsDisplay_Shown(object sender, EventArgs e) {
            this.Opacity = 1;
        }

        private void SetTheme(MetroThemeStyle theme) {
            this.SuspendLayout();
            if (theme == MetroThemeStyle.Dark) {
                this.formsPlot.Plot.Style(ScottPlot.Style.Black);
                this.formsPlot.Plot.Style(figureBackground: Color.FromArgb(17, 17, 17));
                this.formsPlot.Plot.Style(dataBackground: Color.FromArgb(17, 17, 17));
                this.formsPlot.Plot.Style(tick: Color.WhiteSmoke);
                foreach (Control c1 in Controls) {
                    if (c1 is MetroComboBox mcbo1) {
                        mcbo1.Theme = theme;
                    } else if (c1 is MetroLabel mlb1) {
                        mlb1.Theme = theme;
                    } else if (c1 is Label lb1) {
                        if (lb1.Name.Equals("lblRoundType") || lb1.Name.Equals("lblRoundTime") || lb1.Name.Equals("lblBestRecord") || lb1.Name.Equals("lblWorstRecord")) continue;
                        lb1.ForeColor = Color.DarkGray;
                    }
                }
            }
            this.Theme = theme;
            this.ResumeLayout();
        }

        private void SetGraph() {
            //this.formsPlot.Plot.Grid(false);
            //this.formsPlot.Plot.Frameless();
            KeyValuePair<string, string> selectedRoundPair = (KeyValuePair<string, string>)this.cboRoundList.SelectedItem;
            //string roundId = (string)this.cboRoundList.SelectedValue;
            string roundId = selectedRoundPair.Key;
            if (this.StatsForm.StatLookup.TryGetValue(roundId, out LevelStats level)) {
                this.picRoundIcon.Size = level.RoundBigIcon.Size;
                this.picRoundIcon.Image = level.RoundBigIcon;
                this.formsPlot.Plot.Title(level.Name);
                
                LevelType levelType = level.Type;
                this.lblRoundType.Text = levelType.LevelTitle(level.IsFinal);
                this.lblRoundType.borderColor = levelType.LevelDefaultColor(level.IsFinal);
                this.lblRoundType.backColor = levelType.LevelDefaultColor(level.IsFinal);
                this.lblRoundType.Width = TextRenderer.MeasureText(this.lblRoundType.Text, this.lblRoundType.Font).Width + 12;
                int recordType = ("round_pixelperfect_almond".Equals(roundId) ||
                                  "round_hoverboardsurvival_s4_show".Equals(roundId) ||
                                  "round_hoverboardsurvival2_almond".Equals(roundId) ||
                                  "round_snowy_scrap".Equals(roundId) ||
                                  "round_jinxed".Equals(roundId) ||
                                  "round_rocknroll".Equals(roundId) ||
                                  "round_conveyor_arena".Equals(roundId)) ? 1
                                : ("round_1v1_button_basher".Equals(roundId) || "round_1v1_volleyfall_symphony_launch_show".Equals(roundId)) ? 2
                                : levelType.FastestLabel();
                this.lblBestRecord.Left = this.lblRoundType.Right + 12;
                this.lblWorstRecord.Left = this.lblRoundType.Right + 12;
                this.lblBestRecord.Text = recordType == 0 ? $"{Multilingual.GetWord("overlay_longest")} : {level.Longest:m\\:ss\\.fff}" :
                                          recordType == 1 ? $"{Multilingual.GetWord("overlay_fastest")} : {level.Fastest:m\\:ss\\.fff}" :
                                          recordType == 2 ? $"{Multilingual.GetWord("overlay_best_score")} : {this.roundScoreData[roundId][0]}" : "-";
                this.lblWorstRecord.Text = recordType == 0 ? $"{Multilingual.GetWord("overlay_fastest")} : {level.Fastest:m\\:ss\\.fff}" :
                                           recordType == 1 ? $"{Multilingual.GetWord("overlay_longest")} : {level.Longest:m\\:ss\\.fff}" :
                                           recordType == 2 ? $"{Multilingual.GetWord("overlay_worst_score")} : {this.roundScoreData[roundId][1]}" : "-";
            } else {
                MatchCollection matches = Regex.Matches(roundId, @"^\d{4}-\d{4}-\d{4}$");
                if (matches.Count > 0) { // user creative round
                    if (this.StatsForm.StatLookup.TryGetValue(this.StatsForm.AllStats.Find(ri => roundId.Equals(ri.ShowNameId)).Name, out LevelStats creativeLevel)) {
                        this.picRoundIcon.Size = creativeLevel.RoundBigIcon.Size;
                        this.picRoundIcon.Image = creativeLevel.RoundBigIcon;
                        this.formsPlot.Plot.Title(selectedRoundPair.Value);
                        
                        LevelType levelType = creativeLevel.Type;
                        this.lblRoundType.Text = levelType.LevelTitle(creativeLevel.IsFinal);
                        this.lblRoundType.borderColor = levelType.LevelDefaultColor(creativeLevel.IsFinal);
                        this.lblRoundType.backColor = levelType.LevelDefaultColor(creativeLevel.IsFinal);
                        this.lblRoundType.Width = TextRenderer.MeasureText(this.lblRoundType.Text, this.lblRoundType.Font).Width + 12;
                        int recordType = levelType.FastestLabel();
                        this.lblBestRecord.Left = this.lblRoundType.Right + 12;
                        this.lblWorstRecord.Left = this.lblRoundType.Right + 12;
                        this.lblBestRecord.Text = recordType == 0 ? $"{Multilingual.GetWord("overlay_longest")} : {creativeLevel.Longest:m\\:ss\\.fff}" :
                            recordType == 1 ? $"{Multilingual.GetWord("overlay_fastest")} : {creativeLevel.Fastest:m\\:ss\\.fff}" :
                            recordType == 2 ? $"{Multilingual.GetWord("overlay_best_score")} : {this.roundScoreData[roundId][0]}" : "-";
                        this.lblWorstRecord.Text = recordType == 0 ? $"{Multilingual.GetWord("overlay_fastest")} : {creativeLevel.Fastest:m\\:ss\\.fff}" :
                            recordType == 1 ? $"{Multilingual.GetWord("overlay_longest")} : {creativeLevel.Longest:m\\:ss\\.fff}" :
                            recordType == 2 ? $"{Multilingual.GetWord("overlay_worst_score")} : {this.roundScoreData[roundId][1]}" : "-";
                    }
                }
            }

            TimeSpan duration = this.roundDurationData[roundId];
            this.lblRoundTime.Text = $"{Multilingual.GetWord("level_round_played_prefix")} {(int)duration.TotalHours}{Multilingual.GetWord("main_hour")}{duration:mm}{Multilingual.GetWord("main_min")}{duration:ss}{Multilingual.GetWord("main_sec")} {Multilingual.GetWord("level_round_played_suffix")}";
            double[] values = this.roundGraphData[roundId];
            
            this.formsPlot.Plot.Palette = new CustomPalette();

            this.goldMedalPercent = $@"{Math.Round((values[1] / values[0]) * 100, 2)}%";
            this.silverMedalPercent = $@"{Math.Round((values[2] / values[0]) * 100, 2)}%";
            this.bronzeMedalPercent = $@"{Math.Round((values[3] / values[0]) * 100, 2)}%";
            this.pinkMedalPercent = $@"{Math.Round((values[4] / values[0]) * 100, 2)}%";
            this.eliminatedMedalPercent = $@"{Math.Round((values[5] / values[0]) * 100, 2)}%";
            
            this.goldMedalCount = $@"{values[1]:N0}";
            this.silverMedalCount = $@"{values[2]:N0}";
            this.bronzeMedalCount = $@"{values[3]:N0}";
            this.pinkMedalCount = $@"{values[4]:N0}";
            this.eliminatedMedalCount = $@"{values[5]:N0}";
            
            this.lblCountGoldMedal.Text = this.goldMedalCount;
            this.lblCountSilverMedal.Text = this.silverMedalCount;
            this.lblCountBronzeMedal.Text = this.bronzeMedalCount;
            this.lblCountPinkMedal.Text = this.pinkMedalCount;
            this.lblCountEliminatedMedal.Text = this.eliminatedMedalCount;
            
            this.radialGauges = this.formsPlot.Plot.AddRadialGauge(values);
            this.radialGauges.OrderInsideOut = false;
            //this.radialGauges.Clockwise = false;
            this.radialGauges.SpaceFraction = .1;
            //this.radialGauges.BackgroundTransparencyFraction = .3;
            //this.radialGauges.StartingAngle = 270;
            this.radialGauges.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            this.radialGauges.LabelPositionFraction = 0;
            this.radialGauges.FontSizeFraction = .7;
            //this.radialGauges.Font.Color = Color.Black;
            this.radialGauges.Labels = this.labelList;
            this.formsPlot.Plot.AxisZoom(.9, .9);
            this.formsPlot.Refresh();
        }

        private void Medal_MouseEnter(object sender, EventArgs e) {
            this.lblCountGoldMedal.Text = this.goldMedalPercent;
            this.lblCountSilverMedal.Text = this.silverMedalPercent;
            this.lblCountBronzeMedal.Text = this.bronzeMedalPercent;
            this.lblCountPinkMedal.Text = this.pinkMedalPercent;
            this.lblCountEliminatedMedal.Text = this.eliminatedMedalPercent;
        }

        private void Medal_MouseLeave(object sender, EventArgs e) {
            this.lblCountGoldMedal.Text = this.goldMedalCount;
            this.lblCountSilverMedal.Text = this.silverMedalCount;
            this.lblCountBronzeMedal.Text = this.bronzeMedalCount;
            this.lblCountPinkMedal.Text = this.pinkMedalCount;
            this.lblCountEliminatedMedal.Text = this.eliminatedMedalCount;
        }

        private void cboRoundList_SelectedIndexChanged(object sender, EventArgs e) {
            if (this.isInitComplete) {
                this.formsPlot.Plot.Clear();
                this.SetGraph();
            }
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if(keyData == Keys.Escape) {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ChangeLanguage() {
            this.lblRoundTime.Font = Overlay.GetDefaultFont(18, Stats.CurrentLanguage);
            this.lblBestRecord.Font = Overlay.GetDefaultFont(18, Stats.CurrentLanguage);
            this.lblWorstRecord.Font = Overlay.GetDefaultFont(18, Stats.CurrentLanguage);
            this.lblRoundType.Font = Overlay.GetDefaultFont(18, Stats.CurrentLanguage);
            this.lblCountGoldMedal.Font = Overlay.GetDefaultFont(45, 0);
            this.lblCountSilverMedal.Font = Overlay.GetDefaultFont(45, 0);
            this.lblCountBronzeMedal.Font = Overlay.GetDefaultFont(45, 0);
            this.lblCountPinkMedal.Font = Overlay.GetDefaultFont(45, 0);
            this.lblCountEliminatedMedal.Font = Overlay.GetDefaultFont(45, 0);
        }
    }
}