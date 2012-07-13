using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using IronCow;

namespace LazyCow
{
    public partial class SmartAddWindow : Form
    {
        private DateTime _dt;
        private int _year, _month, _day, _hours, _minutes;
        private List<KeyValuePair<string, TaskList>> _lists;
        private bool _predictHash;

        private readonly SmartParser _parser;

        public SmartAddWindow()
        {
            InitializeComponent();

            _predictHash = true;

            _parser = new SmartParser();

            textBox1.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
            textBox1.DataBindings.Add("Text", _parser, "Text");

            dueDatePicker.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
            dueDatePicker.DataBindings.Add("Value", _parser, "Date");

            dueTimePicker.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
            dueTimePicker.DataBindings.Add("Value", _parser, "Date");

            //FillComboboxes();
        }

        private void FillComboboxes()
        {
            _lists = new List<KeyValuePair<string, TaskList>>();

            foreach (var kvp in Program.Service.Rtm.TaskLists.Select(tl => new KeyValuePair<string, TaskList>(tl.Name, tl)))
            {
                _lists.Add(kvp);
            }

            _lists = _lists.OrderBy(x => x.Key).ToList();

            listComboBox.DataSource = new BindingSource(_lists, null);
            listComboBox.DisplayMember = "Key";
            listComboBox.ValueMember = "Value";

            //SLOOOOOOOWWWWWWW!!!!!!
            tagComboBox.DataSource = new BindingSource(Program.Service.Rtm.GetTags(), null);
        }

        #region ESC close

        private void SmartAddWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                this.Close();
            }
        }

        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                this.Close();
            }

            if (e.KeyData == Keys.Back || e.KeyData == Keys.Delete)
            {
                _predictHash = false;
            }
            else
            {
                _predictHash = true;
            }
        }

        #endregion
    }
}
