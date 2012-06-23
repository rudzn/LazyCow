﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using IronCow;
using LazyCow.Properties;
using wyDay.Controls;

namespace LazyCow
{
    class Program : Form
    {
        private static NotifyIcon _notico;
        public static Service Service;
        private static ContextMenu _cm;
        private static System.Timers.Timer _reloadTimer;
        private static List<MenuItem> _reloadSettings;

        private static string _lastTrayText;

        private static bool _todayReady, _listsReady, _authed;

        private static bool Ready { get { return _todayReady && _listsReady && _authed; } }

        //==========================================================================
        [STAThread]
        public static void Main(string[] astrArg)
        {
            _todayReady = false;
            _listsReady = false;
            _authed = false;

            // Kontextmenü erzeugen
            _cm = GetLoadingMenu();

            // NotifyIcon selbst erzeugen
            _notico = new NotifyIcon
                          {
                              Icon = Icon.FromHandle(LazyCow.Properties.Resources.traybg.GetHicon()),
                              Text = Resources.App_Title,
                              Visible = true,
                              ContextMenu = _cm
                          };

            _notico.DoubleClick += new EventHandler(NotifyIconDoubleClick);

            DrawIcon("...");

            Service = new Service();
            Service.Authed += new EventHandler(ServiceAuthed);
            Service.Auth();

            // Ohne Appplication.Run geht es nicht
            Application.Run();            
        }

        private static void ServiceDue(object sender, EventArgs e)
        {
            if (e is DueEventArgs)
            {
                Balloon((e as DueEventArgs).t.Name + " - " + (e as DueEventArgs).t.Due, true);
            }
        }

        private static void Balloon(string text, bool playSound=false)
        {
            _notico.ShowBalloonTip(10000, Resources.App_Title, text, ToolTipIcon.Info);
            if (playSound) System.Media.SystemSounds.Asterisk.Play();
        }

        private static void SetImages(ContextMenu menu)
        {
            var vm = new VistaMenu();

            ((System.ComponentModel.ISupportInitialize)(vm)).BeginInit();

            foreach (MenuItem item in menu.MenuItems)
            {
                if (item.Tag is Task)
                {
                    switch ((item.Tag as Task).Priority)
                    {
                        case TaskPriority.One:
                            vm.SetImage(item, LazyCow.Properties.Resources.prio1);
                            break;
                        case TaskPriority.Two:
                            vm.SetImage(item, LazyCow.Properties.Resources.prio2);
                            break;
                        case TaskPriority.Three:
                            vm.SetImage(item, LazyCow.Properties.Resources.prio3);
                            break;
                        default:
                            vm.SetImage(item, LazyCow.Properties.Resources.prio0);
                            break;
                    }
                }
                else if (item.Tag is TaskList)
                {
                    foreach (MenuItem task in item.MenuItems)
                    {
                        if (!(task.Tag is Task)) continue;
                        var t = task.Tag as Task;

                        switch (t.Priority)
                        {
                            case TaskPriority.One:
                                vm.SetImage(task, LazyCow.Properties.Resources.prio1);
                                break;
                            case TaskPriority.Two:
                                vm.SetImage(task, LazyCow.Properties.Resources.prio2);
                                break;
                            case TaskPriority.Three:
                                vm.SetImage(task, LazyCow.Properties.Resources.prio3);
                                break;
                            default:
                                vm.SetImage(task, LazyCow.Properties.Resources.prio0);
                                break;
                        }
                    }
                }
                
            }
            

            ((System.ComponentModel.ISupportInitialize)(vm)).EndInit();
        }

        private static void DrawIcon()
        {
            DrawIcon(_lastTrayText);
        }

        private static void DrawIcon(string text)
        {
            _lastTrayText = text;

            #region font styling

            int x = 3, y = -1, fontSize = 12;

            if (System.String.CompareOrdinal(text, "...") == 0)
            {
                x = 2;
                y = -4;
            }
            else
            {
                if (text.Length == 2)
                {
                    x = 0;
                }
                else if(text.Length > 2)
                {
                    x = -1;
                    y = 0;
                    fontSize = 10;
                    text = "99+";
                }
            }

            #endregion

            var icon = _authed ? LazyCow.Properties.Resources.traybg : LazyCow.Properties.Resources.traybg_unauthed;

            var g = Graphics.FromImage(icon);

            var font = new Font("Arial Narrow", fontSize, FontStyle.Bold);
            var brush = new SolidBrush(Color.White);
            var format = new StringFormat();

            g.DrawString(text, font, brush, x, y, format);

            font.Dispose();
            brush.Dispose();
            g.Dispose();

            _notico.Icon = Icon.FromHandle(icon.GetHicon());
        }

        private static void DrawContaxtMenuWithLoading()
        {
            _cm = GetLoadingMenu();
            _notico.ContextMenu = _cm;

            var t = new Thread(new ThreadStart(DrawContextMenu));
            t.Start();
        }

        private static void DrawContextMenu()
        {
            var menu = new ContextMenu();

            menu = AddTodayItems(menu);
            menu = AddTaskLists(menu);
            menu = AppendCommonContextItems(menu);
            SetImages(menu);
            DrawIcon(Service.IncompleteTasksToday.ToString(CultureInfo.InvariantCulture));

            _cm = menu;
            _notico.ContextMenu = _cm;
        }

        private static ContextMenu GetLoadingMenu()
        {
            var menu = new ContextMenu();

            var item = new MenuItem {Text = "&Loading", Enabled = false, Index = 0};

            menu.MenuItems.Add(item);

            AppendCommonContextItems(menu);

            return menu;
        }

        private static ContextMenu AppendCommonContextItems(ContextMenu menu)
        {
            int iIndex = menu.MenuItems.Count - 1;


            var item = new MenuItem {Text = "-", Index = iIndex++};
            menu.MenuItems.Add(item);

            item = new MenuItem {Text = "&Reload", Index = iIndex++};
            item.Click += new System.EventHandler(ReloadClick);
            menu.MenuItems.Add(item);

            item = new MenuItem {Text = "&Logout", Index = iIndex++};
            item.Click += new System.EventHandler(LogoutClick);
            menu.MenuItems.Add(item);

            item = new MenuItem {Text = "-", Index = iIndex++};
            menu.MenuItems.Add(item);

            item = new MenuItem {Text = "&Settings", Index = iIndex++};
            item.Click += new System.EventHandler(SettingsClick);
            menu.MenuItems.Add(item);

            _reloadSettings = new List<MenuItem>();

            var subitem = new MenuItem {Text = "&Reload every 15min", Index = 0, Tag = 15};
            if (LazyCow.Properties.Settings.Default.auto_reload_minutes == (int)subitem.Tag) subitem.Checked = true;
            subitem.Click += new EventHandler(reloadChange_Click);
            item.MenuItems.Add(subitem);
            _reloadSettings.Add(subitem);

            subitem = new MenuItem {Text = "&Reload every 30min", Index = 1, Tag = 30};
            if (LazyCow.Properties.Settings.Default.auto_reload_minutes == (int)subitem.Tag) subitem.Checked = true;
            subitem.Click += new EventHandler(reloadChange_Click);
            item.MenuItems.Add(subitem);
            _reloadSettings.Add(subitem);

            subitem = new MenuItem {Text = "&Reload every 60min", Index = 2, Tag = 60};
            if (LazyCow.Properties.Settings.Default.auto_reload_minutes == (int)subitem.Tag) subitem.Checked = true;
            subitem.Click += new EventHandler(reloadChange_Click);
            item.MenuItems.Add(subitem);
            _reloadSettings.Add(subitem);

            subitem = new MenuItem {Text = "&Reload manual", Index = 3, Tag = 0};
            if (LazyCow.Properties.Settings.Default.auto_reload_minutes == (int)subitem.Tag) subitem.Checked = true;
            subitem.Click += new EventHandler(reloadChange_Click);
            item.MenuItems.Add(subitem);
            _reloadSettings.Add(subitem);

            item = new MenuItem {Text = "&Exit", Index = iIndex};
            item.Click += new System.EventHandler(ExitClick);
            menu.MenuItems.Add(item);

            return menu;
        }

        static void reloadChange_Click(object sender, EventArgs e)
        {
            if (!(sender is MenuItem)) return;
            if (!((sender as MenuItem).Tag is int)) return;
            
            foreach (var item in _reloadSettings)
            {
                item.Checked = (sender as MenuItem) == item;
            }

            var time = (int)(sender as MenuItem).Tag;

            if (time > 0)
            {
                LazyCow.Properties.Settings.Default.auto_reload = true;
                LazyCow.Properties.Settings.Default.auto_reload_minutes = time;
            }
            else
            {
                LazyCow.Properties.Settings.Default.auto_reload = false;
            }
            LazyCow.Properties.Settings.Default.Save();
        }

        private static void ServiceAuthed(object sender, EventArgs e)
        {
            _authed = true;
            DrawIcon();
            Service.Due += new EventHandler(ServiceDue);

            DrawContaxtMenuWithLoading();

            Service.RegisterHotkey(Keys.T);

            StartReloadTimer();
        }

        private static int MinToMilli(int minutes)
        {
            return minutes * 60000;
        }

        private static void StartReloadTimer()
        {
            if (_reloadTimer != null) _reloadTimer.Dispose();
            _reloadTimer = new System.Timers.Timer
                               {
                                   AutoReset = false,
                                   Interval = MinToMilli(LazyCow.Properties.Settings.Default.auto_reload_minutes)
                               };
            _reloadTimer.Elapsed += new System.Timers.ElapsedEventHandler(ReloadElapsed);
            _reloadTimer.Start();
        }

        static void ReloadElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (LazyCow.Properties.Settings.Default.auto_reload)
            {
                Reload();
            }
        }

        private static void Reload()
        {
            DrawIcon("...");
            Service.Reload();
        }

        private static ContextMenu AddTaskLists(ContextMenu menu)
        {
            int index = _cm.MenuItems.Count;

            var item = new MenuItem {Text = "-", Index = index++};
            menu.MenuItems.Add(item);

            foreach (TaskList tl in Service.Rtm.TaskLists.OrderBy(x=>x.Name))
            {
                item = new MenuItem {Text = tl.Name};
                var count = Service.Rtm.GetTasks(tl.Id, "status:incomplete").Count();
                if (count > 0) item.Text += " (" + count.ToString(CultureInfo.InvariantCulture) + ")";
                item.Index = index++;
                item.Tag = tl;

                int i = 0;

                foreach (var t in Service.Rtm.GetTasks(tl.Id, "status:incomplete").OrderBy(x => x.Priority).ThenBy(x => x.DueDateTime))
                {
                    var subitem = new MenuItem {Text = t.Name};
                    if (t.DueDateTime != null)
                    {
                        if (DateTime.Now.Year != t.DueDateTime.Value.Year)
                        {
                            subitem.Text += "\t" + String.Format("{0:dd/MM/yy}", t.DueDateTime);
                        }
                        else
                        {
                            subitem.Text += "\t";
                            if(DateTime.Now.Month != t.DueDateTime.Value.Month || DateTime.Now.Day != t.DueDateTime.Value.Day) subitem.Text += String.Format("{0:dd/MM}", t.DueDateTime);
                        }
                        if (t.HasDueTime)
                        {
                            subitem.Text += " " + String.Format("{0:HH:mm}", t.DueDateTime);
                        }
                    }
                    subitem.Index = i++;
                    subitem.Tag = t;
                    item.MenuItems.Add(subitem);
                }

                menu.MenuItems.Add(item);
            }

            return menu;
        }

        private static ContextMenu AddTodayItems(ContextMenu menu)
        {
            var index = 0;

            //TODO: Tasks für heute einfügen
            foreach (var t in Service.TasksOfToday)
            {
                var item = new MenuItem {Text = t.Name};
                if (t.DueDateTime != null)
                {
                    if (DateTime.Now.Year != t.DueDateTime.Value.Year)
                    {
                        item.Text += "\t" + String.Format("{0:dd/MM/yy}", t.DueDateTime);
                    }
                    else
                    {
                        item.Text += "\t";
                        if (DateTime.Now.Month != t.DueDateTime.Value.Month || DateTime.Now.Day != t.DueDateTime.Value.Day) item.Text += String.Format("{0:dd/MM}", t.DueDateTime);
                    }
                    if (t.HasDueTime)
                    {
                        item.Text += " " + String.Format("{0:HH:mm}", t.DueDateTime);
                    }
                }
                item.Index = index++;
                item.Tag = t;
                item.Click += new System.EventHandler(TaskClick);
                menu.MenuItems.Add(item);
            }

            return menu;
        }

        //==========================================================================
        private static void TaskClick(Object sender, EventArgs e)
        {
            //TODO: Task öffnen
        }

        //==========================================================================
        private static void ReloadClick(Object sender, EventArgs e)
        {
            Reload();
        }

        //==========================================================================
        private static void LogoutClick(Object sender, EventArgs e)
        {
            Service.Logout();
            _notico.Dispose();
            Application.Exit();
        }

        //==========================================================================
        private static void ExitClick(Object sender, EventArgs e)
        {
            _notico.Dispose();
            Application.Exit();
        }

        //==========================================================================
        private static void SettingsClick(Object sender, EventArgs e)
        {
            // nur als Beispiel:
            // new MyForm ().Show ();
        }

        //==========================================================================
        private static void NotifyIconDoubleClick(Object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.rememberthemilk.com/");
        }
    }
}
