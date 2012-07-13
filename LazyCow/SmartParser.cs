using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace LazyCow
{
    public class SmartParser :INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        private string _text, _list, _location, _subject;
        private List<string> _tags; 
        private int _day, _month, _year, _hour, _minute;

        public DateTime Date {
            get
            {
                return new DateTime(_year, _month, _day, _hour, _minute, 0);
            }
        }
        public string Text
        {
            get { return _text; }
            set {
                _text = value;
                Parse();
            }
        }
        public string List
        {
            get { return _list; }
        }
        public List<string> Tags
        {
            get { return _tags; }
        } 
        public string Location
        {
            get { return _location; }
        }
        public string Subject
        {
            get { return _subject; }
        }

        public SmartParser()
        {
            var date    = DateTime.Now;

            _text       = string.Empty;
            _list       = string.Empty;
            _tags       = new List<string>();
            _year       = date.Year;
            _month      = date.Month;
            _day        = date.Day;
            _hour       = date.Hour;
            _minute     = date.Minute;
            _location   = string.Empty;
            _subject    = string.Empty;
        }

        private void Parse()
        {
            const string re1        = "(\\^)";
            const string re2        = "(\\s+)";
            const string yearShort  = "((?:(?:[0-2]?\\d{1})|(?:[3][01]{1}))[-:\\/.](?:[0]?[1-9]|[1][012])[-:\\/.](?:(?:\\d{1}\\d{1})))(?![\\d])"; // DDMMYY 1
            const string yearLong   = "((?:(?:[0-2]?\\d{1})|(?:[3][01]{1}))[-:\\/.](?:[0]?[1-9]|[1][012])[-:\\/.](?:(?:[1]{1}\\d{1}\\d{1}\\d{1})|(?:[2]{1}\\d{3})))(?![\\d])"; // DDMMYYYY
            const string time       = "((?:(?:[0-1][0-9])|(?:[2][0-3])|(?:[0-9])):(?:[0-5][0-9])(?::[0-5][0-9])?(?:\\s?(?:am|AM|pm|PM))?)"; // HourMinuteSec 1
            const string hash       = "(#)";
            const string word       = "((?:[a-zäöü0-9]+))";
            const string at         = "(@)";

            #region Year short

            var r = new Regex(re1 + yearShort, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(_text);
            if (m.Success)
            {
                var ddmmyy = m.Groups[2].ToString();

                var tmp = DateTime.ParseExact(ddmmyy, "dd.MM.yy", null);
                _day = tmp.Day;
                _month = tmp.Month;
                _year = tmp.Year;

                NotifyPropertyChanged("Date");
            }

            #endregion

            #region Year long

            r = new Regex(re1 + yearLong, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            m = r.Match(_text);
            if (m.Success)
            {
                var ddmmyy = m.Groups[2].ToString();

                var tmp = DateTime.ParseExact(ddmmyy, "dd.MM.yyyy", null);
                _day = tmp.Day;
                _month = tmp.Month;
                _year = tmp.Year;

                NotifyPropertyChanged("Date");
            }

            #endregion

            #region Time
            
            r = new Regex(re1 + yearShort + re2 + time, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            m = r.Match(_text);
            if (m.Success)
            {
                var ttime = m.Groups[4].ToString();
                var tmp = DateTime.ParseExact(ttime, "hh:mm", null);
                _hour = tmp.Hour;
                _minute = tmp.Minute;

                NotifyPropertyChanged("Date");
            }

            r = new Regex(re1 + yearLong + re2 + time, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            m = r.Match(_text);
            if (m.Success)
            {
                var ttime = m.Groups[4].ToString();
                var tmp = DateTime.ParseExact(ttime, "hh:mm", null);
                _hour = tmp.Hour;
                _minute = tmp.Minute;

                NotifyPropertyChanged("Date");
            }
            
            #endregion

            #region List & Tags

            r = new Regex(hash + word, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            m = r.Match(_text);
            if (m.Success)
            {
                _list = m.Groups[2].ToString();

                var tmp = _text;
                tmp = tmp.Substring(0, m.Groups[2].Index) + tmp.Substring(m.Groups[2].Index + m.Groups[2].Length);
                m = r.Match(tmp);

                NotifyPropertyChanged("List");

                while(m.Success)
                {
                    _tags.Add(m.Groups[2].ToString());
                    tmp = tmp.Substring(0, m.Groups[2].Index) + tmp.Substring(m.Groups[2].Index + m.Groups[2].Length);
                    m = r.Match(tmp);

                    NotifyPropertyChanged("Tags");
                }
            }

            #endregion

            #region Location

            r = new Regex(at + word, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            m = r.Match(_text);
            if (m.Success)
            {
                _location = m.Groups[2].ToString();

                NotifyPropertyChanged("Location");
            }

            #endregion

            #region Subject

            var split = _text.Split(' ');

            _subject = string.Empty;

            foreach (var s in split)
            {
                r = new Regex(re1 + yearShort, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var a = r.Match(s);

                r = new Regex(re1 + yearLong, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var b = r.Match(s);

                r = new Regex(re1 + yearShort + re2 + time, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var c = r.Match(s);

                if( s.Contains("@") ||
                    s.Contains("#") ||
                    s.Contains("^") ||
                    a.Success ||
                    b.Success ||
                    c.Success)
                {
                    continue;
                }
                else
                {
                    _subject += s + " ";
                }

                _subject = _subject.Trim();

                NotifyPropertyChanged("Subject");
            }

            #endregion
        }
    }
}
