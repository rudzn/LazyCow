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

        public SmartAddWindow()
        {
            InitializeComponent();

            _predictHash = true;

            _dt = new DateTime();
            _year = DateTime.Now.Year;
            _month = DateTime.Now.Month;
            _day = DateTime.Now.Day;
            _hours = DateTime.Now.Hour;
            _minutes = DateTime.Now.Minute;

            FillComboboxes();
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

        #region TextBox parsing

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            var txt = textBox.Text;

            const string re1 = "(\\^)";
            const string re2 = "(\\s+)";
            const string yearShort = "((?:(?:[0-2]?\\d{1})|(?:[3][01]{1}))[-:\\/.](?:[0]?[1-9]|[1][012])[-:\\/.](?:(?:\\d{1}\\d{1})))(?![\\d])"; // DDMMYY 1
            const string yearLong = "((?:(?:[0-2]?\\d{1})|(?:[3][01]{1}))[-:\\/.](?:[0]?[1-9]|[1][012])[-:\\/.](?:(?:[1]{1}\\d{1}\\d{1}\\d{1})|(?:[2]{1}\\d{3})))(?![\\d])"; // DDMMYYYY
            const string time = "((?:(?:[0-1][0-9])|(?:[2][0-3])|(?:[0-9])):(?:[0-5][0-9])(?::[0-5][0-9])?(?:\\s?(?:am|AM|pm|PM))?)"; // HourMinuteSec 1
            const string hash = "(#)";
            const string word = "((?:[a-zäöü][a-zäöü]+))";

            #region Year short

            var r = new Regex(re1 + yearShort , RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(txt);
            if (m.Success)
            {
                var ddmmyy1 = m.Groups[2].ToString();
                Console.WriteLine(ddmmyy1);
                try
                {
                    var tmp = DateTime.ParseExact(ddmmyy1, "dd.MM.yy", null);
                    _day = tmp.Day;
                    _month = tmp.Month;
                    _year = tmp.Year;
                    _dt = new DateTime(_year, _month, _day, _hours, _minutes, 0);
                    Console.WriteLine(_dt.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            #endregion

            #region Year long

            r = new Regex(re1 + yearLong, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            m = r.Match(txt);
            if (m.Success)
            {
                var ddmmyy1 = m.Groups[2].ToString();
                Console.WriteLine(ddmmyy1);
                try
                {
                    var tmp = DateTime.ParseExact(ddmmyy1, "dd.MM.yyyy", null);
                    _day = tmp.Day;
                    _month = tmp.Month;
                    _year = tmp.Year;
                    _dt = new DateTime(_year, _month, _day, _hours, _minutes, 0);
                    Console.WriteLine(_dt.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            #endregion

            #region Time

            r = new Regex(re1 + yearShort + re2 + time, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            m = r.Match(txt);
            if (m.Success)
            {
                var ttime = m.Groups[4].ToString();
                Console.WriteLine(ttime);
                try
                {
                    var tmp = DateTime.ParseExact(ttime, "hh:mm", null);
                    _hours = tmp.Hour;
                    _minutes = tmp.Minute;
                    _dt = new DateTime(_year, _month, _day, _hours, _minutes, 0);
                    Console.WriteLine(_dt.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            r = new Regex(re1 + yearLong + re2 + time, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            m = r.Match(txt);
            if (m.Success)
            {
                var ttime = m.Groups[4].ToString();
                Console.WriteLine(ttime);
                try
                {
                    var tmp = DateTime.ParseExact(ttime, "hh:mm", null);
                    _hours = tmp.Hour;
                    _minutes = tmp.Minute;
                    _dt = new DateTime(_year, _month, _day, _hours, _minutes, 0);
                    Console.WriteLine(_dt.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            #endregion

            #region Set DateTimePicker

            try
            {
                dueDatePicker.Value = new DateTime(_year, _month, _day, _hours, _minutes, 0);
                dueTimePicker.Value = new DateTime(_year, _month, _day, _hours, _minutes, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            #endregion

            #region List & Tag

            if (_predictHash)
            {
                r = new Regex(hash + word, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                m = r.Match(txt);
                if (m.Success)
                {
                    String wrd = m.Groups[2].ToString();
                    Console.WriteLine(wrd);

                    var q = from w in _lists where String.CompareOrdinal(w.Key, wrd) == 0 select w;
                    Console.WriteLine("Count for " + wrd + ": " + q.Count());
                    if (!q.Any())
                    {

                        var list = from l in _lists where l.Key.StartsWith(wrd, true, System.Globalization.CultureInfo.CurrentCulture) select l;

                        try
                        {
                            string suggestion = list.First().Key;

                            textBox1.TextChanged -= textBox1_TextChanged;

                            int selectionStart = textBox1.Text.Length;
                            textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - wrd.Length) + suggestion;
                            textBox1.SelectionStart = selectionStart;
                            textBox1.SelectionLength = suggestion.Length;

                            textBox1.TextChanged += new EventHandler(textBox1_TextChanged);
                        }
                        catch
                        { }
                    }
                    else
                    {
                        listComboBox.Text = q.First().Key;
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}
