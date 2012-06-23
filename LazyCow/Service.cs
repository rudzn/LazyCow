using System;
using System.Collections.Generic;
using System.Linq;
using IronCow;
using System.Timers;

namespace LazyCow
{
    

    class DueEventArgs : EventArgs
    {
        public Task T;

        public DueEventArgs(Task t)
        {
            this.T = t;
        }
    }

    class Service : System.Windows.Forms.Form
    {
        public Rtm Rtm;
        private string _frob;
        public event EventHandler Authed, Due;
        private Timer _authTimer;
        private readonly Dictionary<Task, Timer> _dueTimers;
        private HotKey _hk;
        private SmartAddWindow _saw;
        private List<Task> _tasksOfToday; 

        public List<Task> TasksOfToday
        {
            get { return _tasksOfToday.ToList(); }
        }

        public TaskListCollection TaskLists
        {
            get
            {
                return Rtm.TaskLists;
            }
        }

        public int IncompleteTasks
        {
            get
            {
                return Rtm.TaskLists.Where(tl => !tl.IsSmart).Sum(tl => Rtm.GetTasks(tl.Id, "status:incomplete").Count());
            }
        }

        public int IncompleteTasksToday
        {
            get
            {
                return this.TasksOfToday.Count(x => x.IsIncomplete);
            }
        }

        public Service()
        {
            _dueTimers = new Dictionary<Task, Timer>();
            Rtm = new Rtm("6d4e7d515e136fe1e3db21df242998ea", "9073739ea220ab96")
                      {RequestThrottling = new System.TimeSpan(0, 0, 0, 0)};
        }

        public void Reload()
        {
            Rtm = new Rtm("6d4e7d515e136fe1e3db21df242998ea", "9073739ea220ab96")
                      {RequestThrottling = new System.TimeSpan(0, 0, 0, 0)};
            Auth();
        }

        public void Auth()
        {
            GetToken(false);
            Authed += new EventHandler(ServiceAuthed);
        }

        void ServiceAuthed(object sender, EventArgs e)
        {
            Rtm.CacheIncompleteTasks();
            StartDueTimers(this.TasksOfToday);
        }

        #region Auth

        private void GetToken(bool renew)
        {
            string token = LazyCow.Properties.Settings.Default.RTM_token;

            if (token == string.Empty || renew)
            {
                _frob = Rtm.GetFrob();
                System.Diagnostics.Process.Start(Rtm.GetAuthenticationUrl(_frob, IronCow.AuthenticationPermissions.Write));
                _authTimer = new Timer {Interval = 1000, AutoReset = false};
                //1000ms
                _authTimer.Elapsed += new ElapsedEventHandler(AuthTimerElapsed);
                _authTimer.Start();
            }
            else
            {
                Authentication auth;
                if (Rtm.CheckToken(token, out auth))
                {
                    Rtm.AuthToken = token;

                    if (Rtm.CheckLogin())
                    {
                        _tasksOfToday = (from tl in Rtm.TaskLists where !tl.IsSmart from t in Rtm.GetTasks(tl.Id, "status:incomplete and (due:today or dueBefore:today)") select t).ToList();
                        Authed(this, new EventArgs());
                    }
                    else
                    {
                        Logout();
                        GetToken(false);
                    }
                }
                else
                {
                    Logout();
                    GetToken(false);
                }
            }
        }

        void AuthTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Rtm.AuthToken = Rtm.GetToken(_frob);
                LazyCow.Properties.Settings.Default.RTM_token = Rtm.AuthToken;
                LazyCow.Properties.Settings.Default.Save();
                _tasksOfToday = (from tl in Rtm.TaskLists where !tl.IsSmart from t in Rtm.GetTasks(tl.Id, "status:incomplete and (due:today or dueBefore:today)") select t).ToList();
                Authed(this, new EventArgs());
                if (_authTimer != null) _authTimer.Dispose();
            }
            catch
            {
                if (_authTimer != null) _authTimer.Start();
            }
        }

        public void Logout()
        {
            LazyCow.Properties.Settings.Default.RTM_token = string.Empty;
            LazyCow.Properties.Settings.Default.Save();
            _frob = string.Empty;
        }

        #endregion

        private void StartDueTimers(IEnumerable<Task> taskList)
        {
            foreach (var t in taskList)
            {
                var test = from d in _dueTimers where d.Key.Id == t.Id select d;
                if (test.Any()) continue;

                var now = DateTime.Now;
                if (t.DueDateTime < now || !t.HasDueTime) continue;

                var ti = new Timer();
                var ts = t.DueDateTime - now;
                ti.Interval = ts.Value.TotalMilliseconds;
                ti.AutoReset = false;
                ti.Elapsed += new ElapsedEventHandler(TiElapsed);
                _dueTimers.Add(t, ti);
                ti.Start();
            }
        }

        void TiElapsed(object sender, ElapsedEventArgs e)
        {
            if (!(sender is Timer)) return;
            try
            {
                var t = _dueTimers.First(x => x.Value == (sender as Timer)).Key;
                Due(this, new DueEventArgs(t));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #region Hotkey

        public void RegisterHotkey(System.Windows.Forms.Keys key)
        {
            _hk = new HotKey {OwnerForm = this};
            _hk.HotKeyPressed += new HotKey.HotKeyPressedEventHandler(HkHotKeyPressed);
            _hk.AddHotKey(key, HotKey.MODKEY.MOD_CONTROL | HotKey.MODKEY.MOD_SHIFT, "QuickAdd");
        }

        void HkHotKeyPressed(string HotKeyID)
        {
            if (_saw != null) return;
            _saw = new SmartAddWindow();
            _saw.FormClosed += new System.Windows.Forms.FormClosedEventHandler(SawFormClosed);
            _saw.Show();
        }

        void SawFormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            _saw.Dispose();
            _saw = null;
        }

        #endregion
    }
}
