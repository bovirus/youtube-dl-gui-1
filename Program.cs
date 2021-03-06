﻿using System;
using System.Threading;
using System.Windows.Forms;

namespace youtube_dl_gui {
    static class Program {
        static Mutex mtx = new Mutex(true, "{youtube-dl-gui-2019-05-13}");
        public static readonly string UserAgent = "User-Agent: youtube-dl-gui/" + Properties.Settings.Default.appVersion;
        public static volatile bool IsDebug = false;
        public static volatile bool IsPortable = false;

        [STAThread]
        static void Main() {
         #if DEBUG
            IsDebug = true;
            string Date = DateTime.Now.Year + "-";
            if (DateTime.Now.Month.ToString().Length == 1) { Date += "0"; }
            Date += DateTime.Now.Month + "-";
            if (DateTime.Now.Day.ToString().Length == 1) { Date += "0"; }
            Date += DateTime.Now.Day;
            Properties.Settings.Default.debugDate = Date;
        #else 
            IsDebug = false;
        #endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (System.IO.File.Exists(Environment.CurrentDirectory + "\\youtube-dl-gui-updater.exe")) {
                System.IO.File.Delete(Environment.CurrentDirectory + "\\youtube-dl-gui-updater.exe");
            }

            if (IsDebug) {
                Application.Run(new frmMain());
            }
            else if (mtx.WaitOne(TimeSpan.Zero, true)) {
                // boot determines if the application can proceed.
                bool AllowLaunch = false;

                if (CheckSettings.IsPortable()) {
                    IsPortable = true;
                    CheckSettings.LoadPortableSettings();
                }

                if (Properties.Settings.Default.firstTime) {
                    if (MessageBox.Show("youtube-dl-gui is a visual extension to youtube-dl and is not affiliated with the developers of youtube-dl in any way.\n\nThis program (and I) does not condone piracy or illegally downloading of any video you do not own the rights to or is not in public domain.\n\nAny help regarding any problems when downloading anything illegal (in my jurisdiction) will be ignored. This message will not appear again.\n\nHave you read the above?", "youtube-dl-gui", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        Properties.Settings.Default.firstTime = false;

                        if (MessageBox.Show("Downloads are saved to your downloads folder by default, would you like to specify a different location now?\n(You can change this in the settings at any time)", "youtube-dl-gui", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                                fbd.Description = "Select a location to save downloads to";
                                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                                if (fbd.ShowDialog() == DialogResult.OK) {
                                    Downloads.Default.downloadPath = fbd.SelectedPath;
                                }
                                else {
                                    Downloads.Default.downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                                }

                                if (!IsPortable) {
                                    Downloads.Default.Save();
                                }
                            }
                        }
                        else {
                            Downloads.Default.downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                        }

                        if (!IsPortable) {
                            Properties.Settings.Default.Save();
                        }

                        CheckSettings.CreatePortableSettings();

                        AllowLaunch = true;
                    }
                }
                else {
                    AllowLaunch = true;
                }

                if (AllowLaunch) {
                    if (IsPortable) {
                        CheckSettings.LoadPortableSettings();
                    }

                    Application.Run(new frmMain());
                    mtx.ReleaseMutex();
                }
                else {
                    Environment.Exit(0);
                }
            }
            else {
                Controller.PostMessage((IntPtr)Controller.HWND_YTDLGUIBROADCAST, Controller.WM_SHOWYTDLGUIFORM, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
