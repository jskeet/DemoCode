namespace TimeZoneInfoExplorer
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
            System.Windows.Forms.Label fromLabel;
            System.Windows.Forms.Label toLabel;
            System.Windows.Forms.GroupBox offsetsGroupBox;
            System.Windows.Forms.GroupBox adjustmentRulesBox;
            System.Windows.Forms.TableLayoutPanel zonePanel;
            System.Windows.Forms.Label idLabel;
            System.Windows.Forms.Label displayNameLabel;
            System.Windows.Forms.Label standardNameLabel;
            System.Windows.Forms.Label daylightNameLabel;
            System.Windows.Forms.Label dstLabel;
            this.offsetsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.offsetsFrom = new System.Windows.Forms.DateTimePicker();
            this.offsetsTo = new System.Windows.Forms.DateTimePicker();
            this.utcOffsets = new System.Windows.Forms.DataGridView();
            this.adjustmentRules = new System.Windows.Forms.DataGridView();
            this.timeZones = new System.Windows.Forms.ComboBox();
            this.idValue = new System.Windows.Forms.Label();
            this.displayNameValue = new System.Windows.Forms.Label();
            this.standardOffsetLabel = new System.Windows.Forms.Label();
            this.standardNameValue = new System.Windows.Forms.Label();
            this.daylightNameValue = new System.Windows.Forms.Label();
            this.supportsDstValue = new System.Windows.Forms.Label();
            this.standardOffsetValue = new System.Windows.Forms.Label();
            this.layoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.zoneBox = new System.Windows.Forms.GroupBox();
            fromLabel = new System.Windows.Forms.Label();
            toLabel = new System.Windows.Forms.Label();
            offsetsGroupBox = new System.Windows.Forms.GroupBox();
            adjustmentRulesBox = new System.Windows.Forms.GroupBox();
            zonePanel = new System.Windows.Forms.TableLayoutPanel();
            idLabel = new System.Windows.Forms.Label();
            displayNameLabel = new System.Windows.Forms.Label();
            standardNameLabel = new System.Windows.Forms.Label();
            daylightNameLabel = new System.Windows.Forms.Label();
            dstLabel = new System.Windows.Forms.Label();
            offsetsGroupBox.SuspendLayout();
            this.offsetsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.utcOffsets)).BeginInit();
            adjustmentRulesBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.adjustmentRules)).BeginInit();
            zonePanel.SuspendLayout();
            this.layoutPanel.SuspendLayout();
            this.zoneBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // fromLabel
            // 
            fromLabel.AutoSize = true;
            fromLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            fromLabel.Location = new System.Drawing.Point(0, 0);
            fromLabel.Margin = new System.Windows.Forms.Padding(0);
            fromLabel.Name = "fromLabel";
            fromLabel.Size = new System.Drawing.Size(33, 26);
            fromLabel.TabIndex = 1;
            fromLabel.Text = "From:";
            fromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            toLabel.Location = new System.Drawing.Point(0, 26);
            toLabel.Margin = new System.Windows.Forms.Padding(0);
            toLabel.Name = "toLabel";
            toLabel.Size = new System.Drawing.Size(33, 26);
            toLabel.TabIndex = 2;
            toLabel.Text = "To:";
            toLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // offsetsGroupBox
            // 
            offsetsGroupBox.Controls.Add(this.offsetsPanel);
            offsetsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            offsetsGroupBox.Location = new System.Drawing.Point(403, 133);
            offsetsGroupBox.Name = "offsetsGroupBox";
            offsetsGroupBox.Size = new System.Drawing.Size(509, 431);
            offsetsGroupBox.TabIndex = 3;
            offsetsGroupBox.TabStop = false;
            offsetsGroupBox.Text = "Offsets from UTC";
            // 
            // offsetsPanel
            // 
            this.offsetsPanel.ColumnCount = 2;
            this.offsetsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.offsetsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.offsetsPanel.Controls.Add(this.offsetsFrom, 1, 0);
            this.offsetsPanel.Controls.Add(fromLabel, 0, 0);
            this.offsetsPanel.Controls.Add(toLabel, 0, 1);
            this.offsetsPanel.Controls.Add(this.offsetsTo, 1, 1);
            this.offsetsPanel.Controls.Add(this.utcOffsets, 0, 2);
            this.offsetsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.offsetsPanel.Location = new System.Drawing.Point(3, 16);
            this.offsetsPanel.Name = "offsetsPanel";
            this.offsetsPanel.RowCount = 3;
            this.offsetsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.offsetsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.offsetsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.offsetsPanel.Size = new System.Drawing.Size(503, 412);
            this.offsetsPanel.TabIndex = 2;
            // 
            // offsetsFrom
            // 
            this.offsetsFrom.CustomFormat = "yyyy-MM-dd HH:mm";
            this.offsetsFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.offsetsFrom.Location = new System.Drawing.Point(36, 3);
            this.offsetsFrom.Name = "offsetsFrom";
            this.offsetsFrom.Size = new System.Drawing.Size(149, 20);
            this.offsetsFrom.TabIndex = 0;
            this.offsetsFrom.Value = new System.DateTime(1999, 12, 31, 23, 0, 0, 0);
            this.offsetsFrom.ValueChanged += new System.EventHandler(this.AdjustOffsets);
            // 
            // offsetsTo
            // 
            this.offsetsTo.CustomFormat = "yyyy-MM-dd HH:mm";
            this.offsetsTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.offsetsTo.Location = new System.Drawing.Point(36, 29);
            this.offsetsTo.Name = "offsetsTo";
            this.offsetsTo.Size = new System.Drawing.Size(149, 20);
            this.offsetsTo.TabIndex = 3;
            this.offsetsTo.Value = new System.DateTime(2000, 1, 1, 1, 0, 0, 0);
            this.offsetsTo.ValueChanged += new System.EventHandler(this.AdjustOffsets);
            // 
            // utcOffsets
            // 
            this.utcOffsets.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.utcOffsets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.offsetsPanel.SetColumnSpan(this.utcOffsets, 2);
            this.utcOffsets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.utcOffsets.Location = new System.Drawing.Point(0, 55);
            this.utcOffsets.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.utcOffsets.Name = "utcOffsets";
            this.utcOffsets.RowHeadersVisible = false;
            this.utcOffsets.ShowEditingIcon = false;
            this.utcOffsets.Size = new System.Drawing.Size(503, 354);
            this.utcOffsets.TabIndex = 4;
            // 
            // adjustmentRulesBox
            // 
            adjustmentRulesBox.Controls.Add(this.adjustmentRules);
            adjustmentRulesBox.Dock = System.Windows.Forms.DockStyle.Fill;
            adjustmentRulesBox.Location = new System.Drawing.Point(3, 133);
            adjustmentRulesBox.Name = "adjustmentRulesBox";
            adjustmentRulesBox.Size = new System.Drawing.Size(394, 431);
            adjustmentRulesBox.TabIndex = 4;
            adjustmentRulesBox.TabStop = false;
            adjustmentRulesBox.Text = "Adjustment rules";
            // 
            // adjustmentRules
            // 
            this.adjustmentRules.AllowUserToAddRows = false;
            this.adjustmentRules.AllowUserToDeleteRows = false;
            this.adjustmentRules.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.adjustmentRules.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.adjustmentRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.adjustmentRules.Location = new System.Drawing.Point(3, 16);
            this.adjustmentRules.Name = "adjustmentRules";
            this.adjustmentRules.ReadOnly = true;
            this.adjustmentRules.RowHeadersVisible = false;
            this.adjustmentRules.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.adjustmentRules.ShowEditingIcon = false;
            this.adjustmentRules.Size = new System.Drawing.Size(388, 412);
            this.adjustmentRules.TabIndex = 1;
            // 
            // zonePanel
            // 
            zonePanel.AutoSize = true;
            zonePanel.ColumnCount = 2;
            zonePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            zonePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            zonePanel.Controls.Add(this.timeZones, 0, 0);
            zonePanel.Controls.Add(idLabel, 0, 1);
            zonePanel.Controls.Add(displayNameLabel, 0, 2);
            zonePanel.Controls.Add(this.idValue, 1, 1);
            zonePanel.Controls.Add(this.displayNameValue, 1, 2);
            zonePanel.Controls.Add(standardNameLabel, 0, 3);
            zonePanel.Controls.Add(daylightNameLabel, 0, 4);
            zonePanel.Controls.Add(dstLabel, 0, 5);
            zonePanel.Controls.Add(this.standardOffsetLabel, 0, 6);
            zonePanel.Controls.Add(this.standardNameValue, 1, 3);
            zonePanel.Controls.Add(this.daylightNameValue, 1, 4);
            zonePanel.Controls.Add(this.supportsDstValue, 1, 5);
            zonePanel.Controls.Add(this.standardOffsetValue, 1, 6);
            zonePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            zonePanel.Location = new System.Drawing.Point(3, 16);
            zonePanel.Name = "zonePanel";
            zonePanel.RowCount = 7;
            zonePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            zonePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            zonePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            zonePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            zonePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            zonePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            zonePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            zonePanel.Size = new System.Drawing.Size(903, 105);
            zonePanel.TabIndex = 0;
            // 
            // timeZones
            // 
            zonePanel.SetColumnSpan(this.timeZones, 2);
            this.timeZones.DisplayMember = "DisplayName";
            this.timeZones.FormattingEnabled = true;
            this.timeZones.Location = new System.Drawing.Point(3, 3);
            this.timeZones.Name = "timeZones";
            this.timeZones.Size = new System.Drawing.Size(500, 21);
            this.timeZones.TabIndex = 0;
            this.timeZones.SelectedIndexChanged += new System.EventHandler(this.AdjustToSelectedTimeZone);
            this.timeZones.SelectedValueChanged += new System.EventHandler(this.AdjustToSelectedTimeZone);
            // 
            // idLabel
            // 
            idLabel.AutoSize = true;
            idLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            idLabel.Location = new System.Drawing.Point(3, 27);
            idLabel.Name = "idLabel";
            idLabel.Size = new System.Drawing.Size(80, 13);
            idLabel.TabIndex = 0;
            idLabel.Text = "ID";
            idLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // displayNameLabel
            // 
            displayNameLabel.AutoSize = true;
            displayNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            displayNameLabel.Location = new System.Drawing.Point(3, 40);
            displayNameLabel.Name = "displayNameLabel";
            displayNameLabel.Size = new System.Drawing.Size(80, 13);
            displayNameLabel.TabIndex = 1;
            displayNameLabel.Text = "Display name";
            displayNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // idValue
            // 
            this.idValue.AutoSize = true;
            this.idValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.idValue.Location = new System.Drawing.Point(89, 27);
            this.idValue.Name = "idValue";
            this.idValue.Size = new System.Drawing.Size(811, 13);
            this.idValue.TabIndex = 2;
            this.idValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // displayNameValue
            // 
            this.displayNameValue.AutoSize = true;
            this.displayNameValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displayNameValue.Location = new System.Drawing.Point(89, 40);
            this.displayNameValue.Name = "displayNameValue";
            this.displayNameValue.Size = new System.Drawing.Size(811, 13);
            this.displayNameValue.TabIndex = 3;
            this.displayNameValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // standardNameLabel
            // 
            standardNameLabel.AutoSize = true;
            standardNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            standardNameLabel.Location = new System.Drawing.Point(3, 53);
            standardNameLabel.Name = "standardNameLabel";
            standardNameLabel.Size = new System.Drawing.Size(80, 13);
            standardNameLabel.TabIndex = 4;
            standardNameLabel.Text = "Standard name";
            standardNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // daylightNameLabel
            // 
            daylightNameLabel.AutoSize = true;
            daylightNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            daylightNameLabel.Location = new System.Drawing.Point(3, 66);
            daylightNameLabel.Name = "daylightNameLabel";
            daylightNameLabel.Size = new System.Drawing.Size(80, 13);
            daylightNameLabel.TabIndex = 5;
            daylightNameLabel.Text = "Daylight name";
            daylightNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dstLabel
            // 
            dstLabel.AutoSize = true;
            dstLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            dstLabel.Location = new System.Drawing.Point(3, 79);
            dstLabel.Name = "dstLabel";
            dstLabel.Size = new System.Drawing.Size(80, 13);
            dstLabel.TabIndex = 6;
            dstLabel.Text = "Supports DST?";
            dstLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // standardOffsetLabel
            // 
            this.standardOffsetLabel.AutoSize = true;
            this.standardOffsetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standardOffsetLabel.Location = new System.Drawing.Point(3, 92);
            this.standardOffsetLabel.Name = "standardOffsetLabel";
            this.standardOffsetLabel.Size = new System.Drawing.Size(80, 13);
            this.standardOffsetLabel.TabIndex = 7;
            this.standardOffsetLabel.Text = "Standard offset";
            // 
            // standardNameValue
            // 
            this.standardNameValue.AutoSize = true;
            this.standardNameValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standardNameValue.Location = new System.Drawing.Point(89, 53);
            this.standardNameValue.Name = "standardNameValue";
            this.standardNameValue.Size = new System.Drawing.Size(811, 13);
            this.standardNameValue.TabIndex = 8;
            // 
            // daylightNameValue
            // 
            this.daylightNameValue.AutoSize = true;
            this.daylightNameValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.daylightNameValue.Location = new System.Drawing.Point(89, 66);
            this.daylightNameValue.Name = "daylightNameValue";
            this.daylightNameValue.Size = new System.Drawing.Size(811, 13);
            this.daylightNameValue.TabIndex = 9;
            this.daylightNameValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // supportsDstValue
            // 
            this.supportsDstValue.AutoSize = true;
            this.supportsDstValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.supportsDstValue.Location = new System.Drawing.Point(89, 79);
            this.supportsDstValue.Name = "supportsDstValue";
            this.supportsDstValue.Size = new System.Drawing.Size(811, 13);
            this.supportsDstValue.TabIndex = 10;
            this.supportsDstValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // standardOffsetValue
            // 
            this.standardOffsetValue.AutoSize = true;
            this.standardOffsetValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standardOffsetValue.Location = new System.Drawing.Point(89, 92);
            this.standardOffsetValue.Name = "standardOffsetValue";
            this.standardOffsetValue.Size = new System.Drawing.Size(811, 13);
            this.standardOffsetValue.TabIndex = 11;
            // 
            // layoutPanel
            // 
            this.layoutPanel.AutoSize = true;
            this.layoutPanel.ColumnCount = 2;
            this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutPanel.Controls.Add(offsetsGroupBox, 1, 1);
            this.layoutPanel.Controls.Add(adjustmentRulesBox, 0, 1);
            this.layoutPanel.Controls.Add(this.zoneBox, 0, 0);
            this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutPanel.Location = new System.Drawing.Point(0, 0);
            this.layoutPanel.Name = "layoutPanel";
            this.layoutPanel.RowCount = 2;
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutPanel.Size = new System.Drawing.Size(915, 567);
            this.layoutPanel.TabIndex = 0;
            // 
            // zoneBox
            // 
            this.zoneBox.AutoSize = true;
            this.layoutPanel.SetColumnSpan(this.zoneBox, 2);
            this.zoneBox.Controls.Add(zonePanel);
            this.zoneBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoneBox.Location = new System.Drawing.Point(3, 3);
            this.zoneBox.Name = "zoneBox";
            this.zoneBox.Size = new System.Drawing.Size(909, 124);
            this.zoneBox.TabIndex = 6;
            this.zoneBox.TabStop = false;
            this.zoneBox.Text = "Time zone selection";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(915, 567);
            this.Controls.Add(this.layoutPanel);
            this.Name = "MainForm";
            this.Text = "TimeZoneInfo Explorer";
            this.Load += new System.EventHandler(this.PopulateTimeZones);
            offsetsGroupBox.ResumeLayout(false);
            this.offsetsPanel.ResumeLayout(false);
            this.offsetsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.utcOffsets)).EndInit();
            adjustmentRulesBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.adjustmentRules)).EndInit();
            zonePanel.ResumeLayout(false);
            zonePanel.PerformLayout();
            this.layoutPanel.ResumeLayout(false);
            this.layoutPanel.PerformLayout();
            this.zoneBox.ResumeLayout(false);
            this.zoneBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox timeZones;
        private System.Windows.Forms.TableLayoutPanel offsetsPanel;
        private System.Windows.Forms.DateTimePicker offsetsFrom;
        private System.Windows.Forms.DateTimePicker offsetsTo;
        private System.Windows.Forms.DataGridView adjustmentRules;
        private System.Windows.Forms.TableLayoutPanel layoutPanel;
        private System.Windows.Forms.DataGridView utcOffsets;
        private System.Windows.Forms.Label idValue;
        private System.Windows.Forms.Label displayNameValue;
        private System.Windows.Forms.Label standardOffsetLabel;
        private System.Windows.Forms.Label standardNameValue;
        private System.Windows.Forms.Label daylightNameValue;
        private System.Windows.Forms.Label supportsDstValue;
        private System.Windows.Forms.Label standardOffsetValue;
        private System.Windows.Forms.GroupBox zoneBox;
    }
}

