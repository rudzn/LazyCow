namespace LazyCow
{
    partial class SmartAddWindow
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.dueDatePicker = new System.Windows.Forms.DateTimePicker();
            this.dueTimePicker = new System.Windows.Forms.DateTimePicker();
            this.listComboBox = new System.Windows.Forms.ComboBox();
            this.tagComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(774, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.textBox1_PreviewKeyDown);
            // 
            // dueDatePicker
            // 
            this.dueDatePicker.Location = new System.Drawing.Point(12, 38);
            this.dueDatePicker.Name = "dueDatePicker";
            this.dueDatePicker.Size = new System.Drawing.Size(200, 20);
            this.dueDatePicker.TabIndex = 1;
            // 
            // dueTimePicker
            // 
            this.dueTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dueTimePicker.Location = new System.Drawing.Point(218, 38);
            this.dueTimePicker.Name = "dueTimePicker";
            this.dueTimePicker.ShowUpDown = true;
            this.dueTimePicker.Size = new System.Drawing.Size(200, 20);
            this.dueTimePicker.TabIndex = 2;
            // 
            // listComboBox
            // 
            this.listComboBox.FormattingEnabled = true;
            this.listComboBox.Location = new System.Drawing.Point(12, 64);
            this.listComboBox.Name = "listComboBox";
            this.listComboBox.Size = new System.Drawing.Size(200, 21);
            this.listComboBox.TabIndex = 3;
            // 
            // tagComboBox
            // 
            this.tagComboBox.FormattingEnabled = true;
            this.tagComboBox.Location = new System.Drawing.Point(218, 64);
            this.tagComboBox.Name = "tagComboBox";
            this.tagComboBox.Size = new System.Drawing.Size(200, 21);
            this.tagComboBox.TabIndex = 4;
            // 
            // SmartAddWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(798, 262);
            this.ControlBox = false;
            this.Controls.Add(this.tagComboBox);
            this.Controls.Add(this.listComboBox);
            this.Controls.Add(this.dueTimePicker);
            this.Controls.Add(this.dueDatePicker);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SmartAddWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SmartAddWindow_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.DateTimePicker dueDatePicker;
        private System.Windows.Forms.DateTimePicker dueTimePicker;
        private System.Windows.Forms.ComboBox listComboBox;
        private System.Windows.Forms.ComboBox tagComboBox;
    }
}