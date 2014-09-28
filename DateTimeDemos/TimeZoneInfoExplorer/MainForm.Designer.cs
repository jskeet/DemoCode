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
            System.Windows.Forms.GroupBox propertiesBox;
            System.Windows.Forms.TableLayoutPanel propertiesPanel;
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
            this.layoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.timeZones = new System.Windows.Forms.ComboBox();
            this.idValue = new System.Windows.Forms.Label();
            this.displayNameValue = new System.Windows.Forms.Label();
            this.standardOffsetLabel = new System.Windows.Forms.Label();
            this.standardNameValue = new System.Windows.Forms.Label();
            this.daylightNameValue = new System.Windows.Forms.Label();
            this.supportsDstValue = new System.Windows.Forms.Label();
            this.standardOffsetValue = new System.Windows.Forms.Label();
            this.zoneSelectionBox = new System.Windows.Forms.GroupBox();
            fromLabel = new System.Windows.Forms.Label();
            toLabel = new System.Windows.Forms.Label();
            offsetsGroupBox = new System.Windows.Forms.GroupBox();
            adjustmentRulesBox = new System.Windows.Forms.GroupBox();
            propertiesBox = new System.Windows.Forms.GroupBox();
            propertiesPanel = new System.Windows.Forms.TableLayoutPanel();
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
            this.layoutPanel.SuspendLayout();
            propertiesBox.SuspendLayout();
            propertiesPanel.SuspendLayout();
            this.zoneSelectionBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // fromLabel
            // 
            fromLabel.AutoSize = true;
            fromLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            fromLabel.Location = new System.Drawing.Point(3, 0);
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
            toLabel.Location = new System.Drawing.Point(3, 26);
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
            offsetsGroupBox.Location = new System.Drawing.Point(712, 109);
            offsetsGroupBox.Name = "offsetsGroupBox";
            offsetsGroupBox.Size = new System.Drawing.Size(200, 455);
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
            this.offsetsPanel.Size = new System.Drawing.Size(194, 436);
            this.offsetsPanel.TabIndex = 2;
            // 
            // offsetsFrom
            // 
            this.offsetsFrom.CustomFormat = "yyyy-MM-dd HH:mm";
            this.offsetsFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.offsetsFrom.Location = new System.Drawing.Point(42, 3);
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
            this.offsetsTo.Location = new System.Drawing.Point(42, 29);
            this.offsetsTo.Name = "offsetsTo";
            this.offsetsTo.Size = new System.Drawing.Size(149, 20);
            this.offsetsTo.TabIndex = 3;
            this.offsetsTo.Value = new System.DateTime(2000, 1, 1, 1, 0, 0, 0);
            this.offsetsTo.ValueChanged += new System.EventHandler(this.AdjustOffsets);
            // 
            // utcOffsets
            // 
            this.utcOffsets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.offsetsPanel.SetColumnSpan(this.utcOffsets, 2);
            this.utcOffsets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.utcOffsets.Location = new System.Drawing.Point(3, 55);
            this.utcOffsets.Name = "utcOffsets";
            this.utcOffsets.RowHeadersVisible = false;
            this.utcOffsets.ShowEditingIcon = false;
            this.utcOffsets.Size = new System.Drawing.Size(188, 378);
            this.utcOffsets.TabIndex = 4;
            // 
            // adjustmentRulesBox
            // 
            adjustmentRulesBox.Controls.Add(this.adjustmentRules);
            adjustmentRulesBox.Dock = System.Windows.Forms.DockStyle.Fill;
            adjustmentRulesBox.Location = new System.Drawing.Point(3, 109);
            adjustmentRulesBox.Name = "adjustmentRulesBox";
            adjustmentRulesBox.Size = new System.Drawing.Size(703, 455);
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
            this.adjustmentRules.ShowEditingIcon = false;
            this.adjustmentRules.Size = new System.Drawing.Size(697, 436);
            this.adjustmentRules.TabIndex = 1;
            // 
            // layoutPanel
            // 
            this.layoutPanel.AutoSize = true;
            this.layoutPanel.ColumnCount = 2;
            this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.layoutPanel.Controls.Add(offsetsGroupBox, 1, 1);
            this.layoutPanel.Controls.Add(adjustmentRulesBox, 0, 1);
            this.layoutPanel.Controls.Add(propertiesBox, 1, 0);
            this.layoutPanel.Controls.Add(this.zoneSelectionBox, 0, 0);
            this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutPanel.Location = new System.Drawing.Point(0, 0);
            this.layoutPanel.Name = "layoutPanel";
            this.layoutPanel.RowCount = 2;
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutPanel.Size = new System.Drawing.Size(915, 567);
            this.layoutPanel.TabIndex = 0;
            // 
            // timeZones
            // 
            this.timeZones.DisplayMember = "DisplayName";
            this.timeZones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeZones.FormattingEnabled = true;
            this.timeZones.Location = new System.Drawing.Point(3, 16);
            this.timeZones.Name = "timeZones";
            this.timeZones.Size = new System.Drawing.Size(697, 21);
            this.timeZones.TabIndex = 0;
            this.timeZones.SelectedIndexChanged += new System.EventHandler(this.AdjustToSelectedTimeZone);
            this.timeZones.SelectedValueChanged += new System.EventHandler(this.AdjustToSelectedTimeZone);
            // 
            // propertiesBox
            // 
            propertiesBox.Controls.Add(propertiesPanel);
            propertiesBox.Dock = System.Windows.Forms.DockStyle.Fill;
            propertiesBox.Location = new System.Drawing.Point(712, 3);
            propertiesBox.Name = "propertiesBox";
            propertiesBox.Size = new System.Drawing.Size(200, 100);
            propertiesBox.TabIndex = 5;
            propertiesBox.TabStop = false;
            propertiesBox.Text = "Time zone properties";
            // 
            // propertiesPanel
            // 
            propertiesPanel.ColumnCount = 2;
            propertiesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            propertiesPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            propertiesPanel.Controls.Add(idLabel, 0, 0);
            propertiesPanel.Controls.Add(displayNameLabel, 0, 1);
            propertiesPanel.Controls.Add(this.idValue, 1, 0);
            propertiesPanel.Controls.Add(this.displayNameValue, 1, 1);
            propertiesPanel.Controls.Add(standardNameLabel, 0, 2);
            propertiesPanel.Controls.Add(daylightNameLabel, 0, 3);
            propertiesPanel.Controls.Add(dstLabel, 0, 4);
            propertiesPanel.Controls.Add(this.standardOffsetLabel, 0, 5);
            propertiesPanel.Controls.Add(this.standardNameValue, 1, 2);
            propertiesPanel.Controls.Add(this.daylightNameValue, 1, 3);
            propertiesPanel.Controls.Add(this.supportsDstValue, 1, 4);
            propertiesPanel.Controls.Add(this.standardOffsetValue, 1, 5);
            propertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            propertiesPanel.Location = new System.Drawing.Point(3, 16);
            propertiesPanel.Name = "propertiesPanel";
            propertiesPanel.RowCount = 6;
            propertiesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            propertiesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            propertiesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            propertiesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            propertiesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            propertiesPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            propertiesPanel.Size = new System.Drawing.Size(194, 81);
            propertiesPanel.TabIndex = 0;
            // 
            // idLabel
            // 
            idLabel.AutoSize = true;
            idLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            idLabel.Location = new System.Drawing.Point(3, 0);
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
            displayNameLabel.Location = new System.Drawing.Point(3, 13);
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
            this.idValue.Location = new System.Drawing.Point(89, 0);
            this.idValue.Name = "idValue";
            this.idValue.Size = new System.Drawing.Size(102, 13);
            this.idValue.TabIndex = 2;
            this.idValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // displayNameValue
            // 
            this.displayNameValue.AutoSize = true;
            this.displayNameValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.displayNameValue.Location = new System.Drawing.Point(89, 13);
            this.displayNameValue.Name = "displayNameValue";
            this.displayNameValue.Size = new System.Drawing.Size(102, 13);
            this.displayNameValue.TabIndex = 3;
            this.displayNameValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // standardNameLabel
            // 
            standardNameLabel.AutoSize = true;
            standardNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            standardNameLabel.Location = new System.Drawing.Point(3, 26);
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
            daylightNameLabel.Location = new System.Drawing.Point(3, 39);
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
            dstLabel.Location = new System.Drawing.Point(3, 52);
            dstLabel.Name = "dstLabel";
            dstLabel.Size = new System.Drawing.Size(80, 13);
            dstLabel.TabIndex = 6;
            dstLabel.Text = "Supports DST?";
            dstLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // standardOffsetLabel
            // 
            this.standardOffsetLabel.AutoSize = true;
            this.standardOffsetLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.standardOffsetLabel.Location = new System.Drawing.Point(3, 68);
            this.standardOffsetLabel.Name = "standardOffsetLabel";
            this.standardOffsetLabel.Size = new System.Drawing.Size(80, 13);
            this.standardOffsetLabel.TabIndex = 7;
            this.standardOffsetLabel.Text = "Standard offset";
            // 
            // standardNameValue
            // 
            this.standardNameValue.AutoSize = true;
            this.standardNameValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standardNameValue.Location = new System.Drawing.Point(89, 26);
            this.standardNameValue.Name = "standardNameValue";
            this.standardNameValue.Size = new System.Drawing.Size(102, 13);
            this.standardNameValue.TabIndex = 8;
            // 
            // daylightNameValue
            // 
            this.daylightNameValue.AutoSize = true;
            this.daylightNameValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.daylightNameValue.Location = new System.Drawing.Point(89, 39);
            this.daylightNameValue.Name = "daylightNameValue";
            this.daylightNameValue.Size = new System.Drawing.Size(102, 13);
            this.daylightNameValue.TabIndex = 9;
            this.daylightNameValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // supportsDstValue
            // 
            this.supportsDstValue.AutoSize = true;
            this.supportsDstValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.supportsDstValue.Location = new System.Drawing.Point(89, 52);
            this.supportsDstValue.Name = "supportsDstValue";
            this.supportsDstValue.Size = new System.Drawing.Size(102, 13);
            this.supportsDstValue.TabIndex = 10;
            this.supportsDstValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // standardOffsetValue
            // 
            this.standardOffsetValue.AutoSize = true;
            this.standardOffsetValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.standardOffsetValue.Location = new System.Drawing.Point(89, 65);
            this.standardOffsetValue.Name = "standardOffsetValue";
            this.standardOffsetValue.Size = new System.Drawing.Size(102, 16);
            this.standardOffsetValue.TabIndex = 11;
            this.standardOffsetValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // zoneSelectionBox
            // 
            this.zoneSelectionBox.Controls.Add(this.timeZones);
            this.zoneSelectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoneSelectionBox.Location = new System.Drawing.Point(3, 3);
            this.zoneSelectionBox.Name = "zoneSelectionBox";
            this.zoneSelectionBox.Size = new System.Drawing.Size(703, 100);
            this.zoneSelectionBox.TabIndex = 6;
            this.zoneSelectionBox.TabStop = false;
            this.zoneSelectionBox.Text = "Time zone selection";
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
            this.layoutPanel.ResumeLayout(false);
            propertiesBox.ResumeLayout(false);
            propertiesPanel.ResumeLayout(false);
            propertiesPanel.PerformLayout();
            this.zoneSelectionBox.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox zoneSelectionBox;
    }
}

