﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Controls;

namespace FallGuysStats {
    public partial class LeaderboardDisplay : MetroFramework.Forms.MetroForm {
        public Stats StatsForm { get; set; }
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
        private readonly string LEADERBOARD_API_URL = "https://data.fallalytics.com/api/leaderboard";
        private string key = String.Empty;
        private int totalPlayers, totalPages, currentPage, totalHeight, myRank;
        private DateTime refreshTime;
        public List<RankRound> roundlist;
        private List<RankInfo> recordholders;
        private List<RankInfo> nodata = new List<RankInfo>();
        
        public LeaderboardDisplay() {
            this.InitializeComponent();
            this.Opacity = 0;
            this.cboRoundList.MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
        }
        
        private void LeaderboardDisplay_Load(object sender, EventArgs e) {
            this.SetTheme(Stats.CurrentTheme);
            this.SetRoundList();
            
            this.gridLevelRank.CurrentCell = null;
            this.gridLevelRank.ClearSelection();
            this.gridLevelRank.MultiSelect = false;
            this.gridLevelRank.DataSource = this.nodata;
        }

        private void LeaderboardDisplay_Shown(object sender, EventArgs e) {
            this.Opacity = 1;
        }
        
        private void LeaderboardDisplay_Resize(object sender, EventArgs e) {
            this.mpsSpinner.Location = new Point((this.ClientSize.Width - this.mpsSpinner.Width) / 2, (this.ClientSize.Height - this.mpsSpinner.Height) / 2 + 20);
            this.lblSearchDescription.Location = new Point((this.ClientSize.Width - this.lblSearchDescription.Width) / 2, (this.ClientSize.Height - this.lblSearchDescription.Height) / 2 + 20);
        }
        
        private void SetTheme(MetroThemeStyle theme) {
            this.SuspendLayout();
            this.cboRoundList.Theme = theme;
            this.cboRoundList.BringToFront();
            this.mlRefreshList.Theme = theme;
            this.mlRefreshList.BringToFront();

            this.mtpLevelRankPage.Text = Multilingual.GetWord("main_round_name");
            
            this.mlMyRank.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
            this.mlMyRank.ForeColor = theme == MetroThemeStyle.Light ? Utils.GetColorBrightnessAdjustment(Color.Fuchsia, 0.6f) : Utils.GetColorBrightnessAdjustment(Color.GreenYellow, 0.5f);
            this.mlMyRank.BringToFront();
            
            this.mlVisitFallalytics.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
            this.mlVisitFallalytics.ForeColor = Utils.GetColorBrightnessAdjustment(Color.FromArgb(0, 174, 219), 0.6f);
            this.mlVisitFallalytics.Text = Multilingual.GetWord("leaderboard_see_full_rankings_in_fallalytics");
            this.mlVisitFallalytics.BringToFront();
            
            this.mtcTabControl.Theme = theme;
            this.mtpLevelRankPage.Theme = theme;
            
            this.lblSearchDescription.Theme = theme;
            this.lblSearchDescription.ForeColor = theme == MetroThemeStyle.Light ? Color.FromArgb(0, 174, 219) : Color.GreenYellow;
            this.lblSearchDescription.Text = Multilingual.GetWord("leaderboard_choose_a_round");
            this.lblSearchDescription.Location = new Point((this.gridLevelRank.Width - this.lblSearchDescription.Width) / 2, (this.gridLevelRank.Height - this.lblSearchDescription.Height) / 2);
            this.mpsSpinner.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
            this.mpsSpinner.Location = new Point((this.gridLevelRank.Width - this.mpsSpinner.Width) / 2, (this.gridLevelRank.Height - this.mpsSpinner.Height) / 2);
            
            this.gridLevelRank.Theme = theme;
            this.gridLevelRank.BackgroundColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(17, 17, 17);
            this.gridLevelRank.ColumnHeadersDefaultCellStyle = this.dataGridViewCellStyle1;
            this.gridLevelRank.DefaultCellStyle = this.dataGridViewCellStyle2;
            this.gridLevelRank.RowTemplate.Height = 32;
            
            this.dataGridViewCellStyle1.Font = Overlay.GetMainFont(14);
            this.dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            this.dataGridViewCellStyle1.BackColor = theme == MetroThemeStyle.Light ? Color.LightGray : Color.FromArgb(2, 2, 2);
            this.dataGridViewCellStyle1.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.DarkGray;
            this.dataGridViewCellStyle1.SelectionBackColor = theme == MetroThemeStyle.Light ? Color.Cyan : Color.DarkMagenta;
            this.dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            
            this.dataGridViewCellStyle2.Font = Overlay.GetMainFont(16);
            this.dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            this.dataGridViewCellStyle2.BackColor = theme == MetroThemeStyle.Light ? Color.White : Color.FromArgb(49, 51, 56);
            this.dataGridViewCellStyle2.ForeColor = theme == MetroThemeStyle.Light ? Color.Black : Color.WhiteSmoke;
            this.dataGridViewCellStyle2.SelectionBackColor = theme == MetroThemeStyle.Light ? Color.DeepSkyBlue : Color.SpringGreen;
            this.dataGridViewCellStyle2.SelectionForeColor = Color.Black;
            
            this.gridLevelRank.SetContextMenuTheme();
            
            this.Theme = theme;
            this.ResumeLayout();
            this.Refresh();
        }
        
        private void cboRoundList_SelectedIndexChanged(object sender, EventArgs e) {
            if (((ImageComboBox)sender).SelectedIndex == -1 || ((ImageItem)((ImageComboBox)sender).SelectedItem).Data[0].Equals(this.key)) { return; }
            this.key = ((ImageItem)((ImageComboBox)sender).SelectedItem).Data[0];
            // this.totalHeight = 0;
            this.currentPage = 0;
            this.SetGridList(this.key);
        }

        private void metroLink_MouseEnter(object sender, EventArgs e) {
            if (sender.Equals(this.mlMyRank)) {
                ((MetroLink)sender).ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Fuchsia : Color.GreenYellow;
            } else if (sender.Equals(this.mlVisitFallalytics)) {
                ((MetroLink)sender).ForeColor = Color.FromArgb(0, 174, 219);
            }
        }
        
        private void metroLink_MouseLeave(object sender, EventArgs e) {
            if (sender.Equals(this.mlMyRank)) {
                ((MetroLink)sender).ForeColor = this.Theme == MetroThemeStyle.Light ? Utils.GetColorBrightnessAdjustment(Color.Fuchsia, 0.6f) : Utils.GetColorBrightnessAdjustment(Color.GreenYellow, 0.5f);
            } else if (sender.Equals(this.mlVisitFallalytics)) {
                ((MetroLink)sender).ForeColor = Utils.GetColorBrightnessAdjustment(Color.FromArgb(0, 174, 219), 0.6f);
            }
        }

        private void SetRoundList() {
            this.cboRoundList.Enabled = false;
            this.cboRoundList.SetImageItemData(new List<ImageItem>());
            this.mpsSpinner.Visible = true;
            if ((DateTime.UtcNow - this.StatsForm.leaderboardRoundListLoadTime).TotalHours >= 1 || this.roundlist == null) {
                Task.Run(() => this.DataLoad()).ContinueWith(prevTask => {
                    List<ImageItem> roundItemList = new List<ImageItem>();
                    foreach (RankRound round in this.roundlist) {
                        foreach (string id in round.ids) {
                            if (LevelStats.ALL.TryGetValue(id, out LevelStats levelStats)) {
                                roundItemList.Add(new ImageItem(Utils.ResizeImageHeight(levelStats.RoundBigIcon, 23), levelStats.Name, Overlay.GetMainFont(15f), new[] { round.queryname, levelStats.Id }));
                                break;
                            }
                        }
                    }
                    roundItemList.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Text, y.Text));
                    this.StatsForm.leaderboardRoundListLoadTime = DateTime.UtcNow;
                    this.BeginInvoke((MethodInvoker)delegate {
                        if (prevTask.Result) {
                            this.mpsSpinner.Visible = false;
                            this.lblSearchDescription.Visible = true;
                            this.cboRoundList.SetImageItemData(roundItemList);
                            this.cboRoundList.Enabled = true;
                        } else {
                            this.mpsSpinner.Visible = false;
                            this.gridLevelRank.DataSource = this.nodata;
                            this.mlRefreshList.Visible = false;
                            this.mlVisitFallalytics.Visible = false;
                            this.lblSearchDescription.Text = Multilingual.GetWord("level_detail_no_data_caption");
                            this.lblSearchDescription.Visible = true;
                            this.cboRoundList.Enabled = false;
                        }
                    });
                });
            } else {
                List<ImageItem> roundItemList = new List<ImageItem>();
                foreach (RankRound round in this.roundlist) {
                    foreach (string id in round.ids) {
                        if (LevelStats.ALL.TryGetValue(id, out LevelStats levelStats)) {
                            roundItemList.Add(new ImageItem(Utils.ResizeImageHeight(levelStats.RoundBigIcon, 23), levelStats.Name, Overlay.GetMainFont(15f), new[] { round.queryname, levelStats.Id }));
                            break;
                        }
                    }
                }
                roundItemList.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Text, y.Text));
                this.BeginInvoke((MethodInvoker)delegate {
                    this.mpsSpinner.Visible = false;
                    this.lblSearchDescription.Visible = true;
                    this.cboRoundList.SetImageItemData(roundItemList);
                    this.cboRoundList.Enabled = true;
                });
            }
        }

        private void SetLeaderboardUI(int index) {
            // this.Text = $@"     {this.cboRoundList.SelectedName} ({Multilingual.GetWord("leaderboard_total_players_prefix")}{this.totalPlayers}{Multilingual.GetWord("leaderboard_total_players_suffix")})";
            this.mtpLevelRankPage.Text = $@"{this.cboRoundList.SelectedName} ({Multilingual.GetWord("leaderboard_total_players_prefix")}{this.totalPlayers}{Multilingual.GetWord("leaderboard_total_players_suffix")})";
            this.mpsSpinner.Visible = false;
            this.gridLevelRank.DataSource = this.recordholders;
            Application.DoEvents();
            if (index != -1) {
                int displayedRowCount = this.gridLevelRank.DisplayedRowCount(false);
                int firstDisplayedScrollingRowIndex = index - (displayedRowCount / 2);
                this.gridLevelRank.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex < 0 ? 0 : firstDisplayedScrollingRowIndex;
                
                this.myRank = index + 1;
                this.mlMyRank.Visible = true;
                this.mlMyRank.Text = $@"{Utils.AppendOrdinal(this.myRank)} {Stats.OnlineServiceNickname}";
                if (this.myRank == 1) {
                    this.mlMyRank.Image = Properties.Resources.medal_gold_grid_icon;
                } else {
                    double percentage = ((double)(this.myRank - 1) / (this.totalPlayers - 1)) * 100;
                    if (percentage <= 20) {
                        this.mlMyRank.Image = Properties.Resources.medal_silver_grid_icon;
                    } else if (percentage <= 50) {
                        this.mlMyRank.Image = Properties.Resources.medal_bronze_grid_icon;
                    } else {
                        this.mlMyRank.Image = Properties.Resources.medal_pink_grid_icon;
                    }
                }
                this.mlMyRank.Location = new Point(this.Width - this.mlMyRank.Width - 5, this.mtcTabControl.Top + 5);
            }
            this.mlVisitFallalytics.Location = new Point(this.Width - this.mlVisitFallalytics.Width - 5, index != -1 ? this.mlMyRank.Top - this.mlVisitFallalytics.Height - 3 : this.mtcTabControl.Top + 5);
            
            this.BackImage = LevelStats.ALL.TryGetValue(((ImageItem)this.cboRoundList.SelectedItem).Data[1], out LevelStats levelStats) ? levelStats.RoundBigIcon : ((ImageItem)this.cboRoundList.SelectedItem).Image;
            this.mlRefreshList.Location = new Point(this.cboRoundList.Right + 15, this.cboRoundList.Location.Y);
            this.mlRefreshList.Visible = true;
            
            this.mlVisitFallalytics.Visible = true;
            this.cboRoundList.Enabled = true;
        }

        private void SetGridNoData() {
            this.Text = $@"     {Multilingual.GetWord("leaderboard_menu_title")}";
            this.mpsSpinner.Visible = false;
            this.gridLevelRank.DataSource = this.nodata;
            this.mlRefreshList.Visible = false;
            this.mlVisitFallalytics.Visible = false;
            this.BackImage = this.Theme == MetroThemeStyle.Light ? Properties.Resources.leaderboard_icon : Properties.Resources.leaderboard_gray_icon;
            this.lblSearchDescription.Text = Multilingual.GetWord("level_detail_no_data_caption");
            this.lblSearchDescription.Visible = true;
            this.mlMyRank.Visible = false;
            this.Invalidate();
        }
        
        private void SetGridList(string queryKey) {
            this.cboRoundList.Enabled = false;
            this.mlRefreshList.Visible = false;
            this.lblSearchDescription.Visible = false;
            this.mlMyRank.Visible = false;
            this.mlVisitFallalytics.Visible = false;
            this.mpsSpinner.Visible = true;
            this.gridLevelRank.DataSource = this.nodata;

            try {
                Task.Run(() => this.DataLoadBulk(queryKey)).ContinueWith(prevTask => {
                    int index = this.recordholders.FindIndex(r => Stats.OnlineServiceId.Equals(r.onlineServiceId) && (int)Stats.OnlineServiceType == int.Parse(r.onlineServiceType));
                    this.BeginInvoke((MethodInvoker)delegate {
                        if (prevTask.Result) {
                            this.SetLeaderboardUI(index);
                        } else {
                            this.SetGridNoData();
                        }
                        this.refreshTime = DateTime.Now;
                    });
                });
            } catch {
                // ignored
            }
        }

        private bool DataLoadBulk(string queryKey) {
            bool result;
            string json;
            JsonSerializerOptions options;
            Leaderboard leaderboard;
            using (ApiWebClient web = new ApiWebClient()) {
                try {
                    this.recordholders = null;
                    web.Headers.Add("X-Authorization-Key", Environment.GetEnvironmentVariable("FALLALYTICS_KEY"));
                    json = web.DownloadString($"{this.LEADERBOARD_API_URL}?round={queryKey}&p=1");
                    options = new JsonSerializerOptions();
                    options.Converters.Add(new RecordHolderConverter());
                    leaderboard = JsonSerializer.Deserialize<Leaderboard>(json, options);
                    result = leaderboard.found;
                    if (result) {
                        this.totalPlayers = leaderboard.total;
                        this.totalPages = (int)Math.Ceiling((this.totalPlayers > 1000 ? 1000 : this.totalPlayers) / 100f);
                        for (int i = 0; i < leaderboard.recordholders.Count; i++) {
                            leaderboard.recordholders[i].rank = i + 1;
                        }
                        this.recordholders = leaderboard.recordholders;
                        if (this.totalPages > 1) {
                            var tasks = new List<Task>();
                            HttpClient client = new HttpClient();
                            client.DefaultRequestHeaders.Add("X-Authorization-Key", Environment.GetEnvironmentVariable("FALLALYTICS_KEY"));
                            for (int i = 2; i <= this.totalPages; i++) {
                                int page = i;
                                tasks.Add(Task.Run(async () => {
                                    HttpResponseMessage response = await client.GetAsync($"{this.LEADERBOARD_API_URL}?round={queryKey}&p={page}");
                                    if (response.IsSuccessStatusCode) {
                                        json = await response.Content.ReadAsStringAsync();
                                        options = new JsonSerializerOptions();
                                        options.Converters.Add(new RecordHolderConverter());
                                        leaderboard = JsonSerializer.Deserialize<Leaderboard>(json, options);
                                        for (int j = 0; j < leaderboard.recordholders.Count; j++) {
                                            leaderboard.recordholders[j].rank = j + 1 + ((page - 1) * 100);
                                        }
                                        this.recordholders.AddRange(leaderboard.recordholders);
                                    }
                                }));
                            }
                            Task.WhenAll(tasks).Wait();
                            this.recordholders.Sort((r1, r2) => r1.rank.CompareTo(r2.rank));
                        }
                    } else {
                        this.totalPages = 0;
                        this.totalPlayers = 0;
                    }
                } catch {
                    result = false;
                    this.totalPages = 0;
                    this.totalPlayers = 0;
                }
            }
            return result;
        }

        private bool DataLoad(string queryKey = null) {
            bool result;
            using (ApiWebClient web = new ApiWebClient()) {
                web.Headers.Add("X-Authorization-Key", Environment.GetEnvironmentVariable("FALLALYTICS_KEY"));
                if (string.IsNullOrEmpty(queryKey)) { // round list
                    try {
                        string json = web.DownloadString($"{this.LEADERBOARD_API_URL}s");
                        var options = new JsonSerializerOptions();
                        options.Converters.Add(new RoundConverter());
                        var availableRound = JsonSerializer.Deserialize<AvailableRound>(json, options);
                        result = availableRound.found;
                        this.roundlist = availableRound.leaderboards;
                        this.StatsForm.leaderboardRoundlist = availableRound.leaderboards;
                    } catch {
                        result = false;
                    }
                } else {
                    try {
                        string json = web.DownloadString($"{this.LEADERBOARD_API_URL}?round={queryKey}&p={this.currentPage + 1}");
                        var options = new JsonSerializerOptions();
                        options.Converters.Add(new RecordHolderConverter());
                        Leaderboard leaderboard = JsonSerializer.Deserialize<Leaderboard>(json, options);
                        result = leaderboard.found;
                        if (result) {
                            this.totalPlayers = leaderboard.total;
                            this.totalPages = (int)Math.Ceiling((this.totalPlayers > 1000 ? 1000 : this.totalPlayers) / 100f);
                            for (int i = 0; i < leaderboard.recordholders.Count; i++) {
                                leaderboard.recordholders[i].rank = i + 1 + (this.currentPage * 100);
                            }
                            this.recordholders = leaderboard.recordholders;
                        } else {
                            this.totalPages = 0;
                            this.totalPlayers = 0;
                        }
                    } catch {
                        result = false;
                        this.totalPages = 0;
                        this.totalPlayers = 0;
                    }
                }
            }
            return result;
        }
        
        private int GetDataGridViewColumnWidth(string columnName) {
            switch (columnName) {
                case "rank":
                    return 50;
                case "show":
                    return 0;
                case "flag":
                    return 35;
                case "platform":
                    return 45;
                case "medal":
                    return 30;
                case "onlineServiceNickname":
                    return 0;
                case "record":
                    return 150;
                case "finish":
                    return 200;
                default:
                    return 0;
            }
        }
        
        private void gridLevelRank_DataSourceChanged(object sender, EventArgs e) {
            if (this.gridLevelRank.Columns.Count == 0) { return; }
            int pos = 0;
            // this.gridLevelRank.Columns["round"].Visible = false;
            this.gridLevelRank.Columns["isAnonymous"].Visible = false;
            this.gridLevelRank.Columns["country"].Visible = false;
            this.gridLevelRank.Columns["onlineServiceType"].Visible = false;
            this.gridLevelRank.Columns["onlineServiceId"].Visible = false;
            this.gridLevelRank.Columns.Add(new DataGridViewImageColumn { Name = "medal", ImageLayout = DataGridViewImageCellLayout.Zoom, ToolTipText = Multilingual.GetWord("") });
            this.gridLevelRank.Setup("medal", pos++, this.GetDataGridViewColumnWidth("medal"), "", DataGridViewContentAlignment.MiddleCenter);
            this.gridLevelRank.Columns["medal"].DefaultCellStyle.NullValue = null;
            this.gridLevelRank.Setup("rank", pos++, this.GetDataGridViewColumnWidth("rank"), $"{Multilingual.GetWord("leaderboard_grid_header_rank")}", DataGridViewContentAlignment.MiddleLeft);
            this.gridLevelRank.Columns["rank"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.gridLevelRank.Setup("show", pos++, this.GetDataGridViewColumnWidth("show"), $"{Multilingual.GetWord("leaderboard_grid_header_show")}", DataGridViewContentAlignment.MiddleLeft);
            this.gridLevelRank.Columns["show"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.gridLevelRank.Columns.Add(new DataGridViewImageColumn { Name = "platform", ImageLayout = DataGridViewImageCellLayout.Zoom, ToolTipText = Multilingual.GetWord("") });
            this.gridLevelRank.Setup("platform", pos++, this.GetDataGridViewColumnWidth("platform"), "", DataGridViewContentAlignment.MiddleCenter);
            this.gridLevelRank.Columns["platform"].DefaultCellStyle.NullValue = null;
            this.gridLevelRank.Columns.Add(new DataGridViewImageColumn { Name = "flag", ImageLayout = DataGridViewImageCellLayout.Normal, ToolTipText = Multilingual.GetWord("") });
            this.gridLevelRank.Setup("flag", pos++, this.GetDataGridViewColumnWidth("flag"), "", DataGridViewContentAlignment.MiddleLeft);
            this.gridLevelRank.Columns["flag"].DefaultCellStyle.NullValue = null;
            this.gridLevelRank.Setup("onlineServiceNickname", pos++, this.GetDataGridViewColumnWidth("onlineServiceNickname"), $"{Multilingual.GetWord("leaderboard_grid_header_player")}", DataGridViewContentAlignment.MiddleLeft);
            this.gridLevelRank.Columns["onlineServiceNickname"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.gridLevelRank.Setup("record", pos++, this.GetDataGridViewColumnWidth("record"), $"{Multilingual.GetWord("leaderboard_grid_header_record")}", DataGridViewContentAlignment.MiddleLeft);
            this.gridLevelRank.Columns["record"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.gridLevelRank.Setup("finish", pos++, this.GetDataGridViewColumnWidth("finish"), $"{Multilingual.GetWord("leaderboard_grid_header_finish")}", DataGridViewContentAlignment.MiddleLeft);
            this.gridLevelRank.Columns["finish"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            
            // foreach (DataGridViewRow row in this.gridLevelRank.Rows) {
            //     this.totalHeight += row.Height;
            // }
        }
        
        private void gridLevelRank_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= this.gridLevelRank.Rows.Count) { return; }

            RankInfo info = this.gridLevelRank.Rows[e.RowIndex].DataBoundItem as RankInfo;
            if (Stats.OnlineServiceId.Equals(info.onlineServiceId)) {
                this.gridLevelRank.Rows[e.RowIndex].DefaultCellStyle.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Fuchsia : Color.GreenYellow;
            }
            if (this.gridLevelRank.Columns[e.ColumnIndex].Name == "rank") {
                e.CellStyle.Font = Overlay.GetMainFont(16f, FontStyle.Bold);
            } else if (this.gridLevelRank.Columns[e.ColumnIndex].Name == "show") {
                if (!string.IsNullOrEmpty((string)e.Value)) {
                    e.Value = Multilingual.GetShowName((string)e.Value) ?? e.Value;
                }
            } else if (this.gridLevelRank.Columns[e.ColumnIndex].Name == "flag") {
                if (!info.isAnonymous && !string.IsNullOrEmpty(info.country)) this.gridLevelRank.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Multilingual.GetCountryName(info.country);
                e.Value = info.isAnonymous ? Properties.Resources.country_unknown_icon : (string.IsNullOrEmpty(info.country) ? Properties.Resources.country_unknown_icon : (Image)Properties.Resources.ResourceManager.GetObject($"country_{info.country.ToLower()}_icon"));
            } else if (this.gridLevelRank.Columns[e.ColumnIndex].Name == "platform") {
                if (!info.isAnonymous) {
                    this.gridLevelRank.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Multilingual.GetWord((info.onlineServiceType == "0" ? "level_detail_online_platform_eos" : "level_detail_online_platform_steam"));
                }
                e.Value = info.isAnonymous ? null : (info.onlineServiceType == "0" ? Properties.Resources.epic_grid_icon : Properties.Resources.steam_grid_icon);
            } else if (this.gridLevelRank.Columns[e.ColumnIndex].Name == "medal") {
                if (info.rank == 1) {
                    this.gridLevelRank.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Multilingual.GetWord("level_detail_gold");
                    e.Value = Properties.Resources.medal_gold_grid_icon;
                } else {
                    double percentage = ((double)(info.rank - 1) / (this.totalPlayers - 1)) * 100;
                    if (percentage <= 20) {
                        this.gridLevelRank.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Multilingual.GetWord("level_detail_silver");
                        e.Value = Properties.Resources.medal_silver_grid_icon;
                    } else if (percentage <= 50) {
                        this.gridLevelRank.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Multilingual.GetWord("level_detail_bronze");
                        e.Value = Properties.Resources.medal_bronze_grid_icon;
                    } else {
                        this.gridLevelRank.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Multilingual.GetWord("level_detail_pink");
                        e.Value = Properties.Resources.medal_pink_grid_icon;
                    }
                }
            } else if (this.gridLevelRank.Columns[e.ColumnIndex].Name == "onlineServiceNickname") {
                if (info.rank == 1) {
                    e.CellStyle.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Goldenrod : Color.Gold;
                }
            } else if (this.gridLevelRank.Columns[e.ColumnIndex].Name == "record") {
                if (info.rank == 1) {
                    e.CellStyle.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Goldenrod : Color.Gold;
                }
                e.Value = Utils.FormatTime((double)e.Value);
            } else if (this.gridLevelRank.Columns[e.ColumnIndex].Name == "finish") {
                this.gridLevelRank.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = ((DateTime)e.Value).ToString(Multilingual.GetWord("level_grid_date_format"));
                e.Value = Utils.GetRelativeTime((DateTime)e.Value);
            }
            
            if (e.RowIndex % 2 == 0) {
                this.gridLevelRank.Rows[e.RowIndex].DefaultCellStyle.BackColor = this.Theme == MetroThemeStyle.Light ? Color.FromArgb(225, 235, 255) : Color.FromArgb(40, 66, 66);
                // this.gridLevelRank.Rows[e.RowIndex].DefaultCellStyle.ForeColor = this.Theme == MetroThemeStyle.Light ? Color.Black : Color.WhiteSmoke;
            } else {
                this.gridLevelRank.Rows[e.RowIndex].DefaultCellStyle.BackColor = this.Theme == MetroThemeStyle.Light ? Color.WhiteSmoke : Color.FromArgb(49, 51, 56);
            }
        }
        
        private void gridLevelRank_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (this.recordholders == null) return;
            string columnName = this.gridLevelRank.Columns[e.ColumnIndex].Name;
            SortOrder sortOrder = this.gridLevelRank.GetSortOrder(columnName);
            if (sortOrder == SortOrder.None) { columnName = "rank"; }
            
            this.recordholders.Sort(delegate (RankInfo one, RankInfo two) {
                int rankCompare = one.rank.CompareTo(two.rank);
                int recordCompare = one.record.CompareTo(two.record);
                if (sortOrder == SortOrder.Descending) {
                    (one, two) = (two, one);
                }

                switch (columnName) {
                    case "medal":
                        rankCompare = one.rank.CompareTo(two.rank);
                        return rankCompare != 0 ? rankCompare : recordCompare;
                    case "rank":
                        rankCompare = one.rank.CompareTo(two.rank);
                        return rankCompare != 0 ? rankCompare : recordCompare;
                    case "show":
                        int showCompare = String.Compare(one.show, two.show, StringComparison.OrdinalIgnoreCase);
                        return showCompare != 0 ? showCompare : rankCompare;
                    case "platform":
                        int platformCompare = String.Compare(one.onlineServiceType, two.onlineServiceType, StringComparison.OrdinalIgnoreCase);
                        return platformCompare != 0 ? platformCompare : rankCompare;
                    case "flag":
                        int countryCompare = String.Compare(one.country, two.country, StringComparison.OrdinalIgnoreCase);
                        return countryCompare != 0 ? countryCompare : rankCompare;
                    case "onlineServiceNickname":
                        int nicknameCompare = String.Compare(one.onlineServiceNickname, two.onlineServiceNickname, StringComparison.OrdinalIgnoreCase);
                        return nicknameCompare != 0 ? nicknameCompare : rankCompare;
                    case "record":
                        recordCompare = one.record.CompareTo(two.record);
                        return recordCompare != 0 ? recordCompare : rankCompare;
                    case "finish":
                        int finishCompare = one.finish.CompareTo(two.finish);
                        return finishCompare != 0 ? finishCompare : rankCompare;
                    default:
                        return 0;
                }
            });
            
            this.gridLevelRank.DataSource = null;
            this.gridLevelRank.DataSource = this.recordholders;
            this.gridLevelRank.Columns[columnName].HeaderCell.SortGlyphDirection = sortOrder;
        }
        
        // private void gridLevelRank_Scroll(object sender, ScrollEventArgs e) {
        //     if (this.totalHeight - this.gridLevelRank.Height < this.gridLevelRank.VerticalScrollingOffset) {
        //         // to do
        //     }
        // }

        private void gridLevelRank_SelectionChanged(object sender, EventArgs e) {
            this.gridLevelRank.ClearSelection();
        }
        
        private void LeaderboardDisplay_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                this.Close();
            }
        }

        private void link_Click(object sender, EventArgs e) {
            if (sender.Equals(this.mlVisitFallalytics)) {
                Process.Start($"https://fallalytics.com/leaderboards/speedrun/{this.key}/1");
            } else if (sender.Equals(this.mlRefreshList)) {
                if (!string.IsNullOrEmpty(this.key)) {
                    TimeSpan difference = DateTime.Now - this.refreshTime;
                    if (difference.TotalSeconds > 8) {
                        this.SetGridList(this.key);
                    }
                }
            } else if (sender.Equals(this.mlMyRank)) {
                int displayedRowCount = this.gridLevelRank.DisplayedRowCount(false);
                int index = this.recordholders.FindIndex(r => Stats.OnlineServiceId.Equals(r.onlineServiceId) && (int)Stats.OnlineServiceType == int.Parse(r.onlineServiceType));
                int firstDisplayedScrollingRowIndex = index - (displayedRowCount / 2);
                this.gridLevelRank.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex < 0 ? 0 : firstDisplayedScrollingRowIndex;
            }
        }
    }
}