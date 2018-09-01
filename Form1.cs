using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace HotKeyed
{



    public partial class Form1 : Form
    {

        int tickCounter;
        IntPtr lastForegroundWindow;

        const int WM_HOTKEY = 0x312;
            

        int lfw_x;
        int lfw_y;

        int tickWaitLatency;

        [DllImport("user32", SetLastError = true)]
        public static extern int GetForegroundWindow();
        GlobalHotkeys LeftSnap, RightSnap, WholeSnap, MinSnap;

        public Form1()
        {
            tickCounter = 0;
            InitializeComponent();

            tickWaitLatency = 3;

        

            //  hotkey.UnregisterGlobalHotKey();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            

            LeftSnap = new GlobalHotkeys();
            RightSnap = new GlobalHotkeys();
            WholeSnap = new GlobalHotkeys();
            MinSnap = new GlobalHotkeys();

            LeftSnap.UnregisterGlobalHotKey();
            RightSnap.UnregisterGlobalHotKey();
            WholeSnap.UnregisterGlobalHotKey();
            MinSnap.UnregisterGlobalHotKey();

            LeftSnap.RegisterGlobalHotKey((int)Keys.Left, 8, this.Handle);
            WholeSnap.RegisterGlobalHotKey((int)Keys.Up, 8, this.Handle);
            RightSnap.RegisterGlobalHotKey((int)Keys.Right, 8, this.Handle);
            MinSnap.RegisterGlobalHotKey((int)Keys.Down, 8, this.Handle);
          
           
        }


        private String GetWindowTitle(IntPtr hwnd)
        {
           //StringBuilder sb = new StringBuilder();
          //GetWindowText(hwnd, sb, 9999);
           // return sb.ToString();
            return null;
        }


        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int _Left;
            public int _Top;
            public int _Right;
            public int _Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler)
                : this()
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static Boolean WindowIsSizable(IntPtr hwnd)
        {
            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            GetWindowInfo(hwnd, ref info);

            bool isSizable = (info.dwStyle & 0x00040000L) == 0x00040000L;

            return isSizable;
        }

        private static int GetWindowXPos(IntPtr hwnd)
        {
            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            GetWindowInfo(hwnd, ref info);

            int xPos = info.rcWindow._Left;

            return xPos;
        }

        private static int GetWindowXWPos(IntPtr hwnd)
        {
            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            GetWindowInfo(hwnd, ref info);

            int xPos = info.rcWindow._Right;

            return xPos;
        }


        private static int GetWindowYPos(IntPtr hwnd)
        {
            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            GetWindowInfo(hwnd, ref info);

            int yPos = info.rcWindow._Top;

            return yPos;
        }

        private static Boolean MouseIsOnWindowCaptionBar()
        {
            IntPtr hwnd = new IntPtr(GetForegroundWindow());

            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            GetWindowInfo(hwnd, ref info);

            int cx, cy;
            int cax, cay;

            cx = (int) info.cxWindowBorders;
            cy = (int) info.cyWindowBorders;

            cax = (int)(info.rcClient._Right - info.rcClient._Left) + 3;
            cay = (int)(info.rcClient._Bottom - info.rcClient._Top) + 3;

            int wcay = info.rcWindow._Bottom - cay;

            int wcay_min = GetWindowYPos(hwnd);

            if (Cursor.Position.X > GetWindowXPos(hwnd) && Cursor.Position.X < GetWindowXWPos(hwnd)
                && Cursor.Position.Y > wcay_min && Cursor.Position.Y < (wcay_min + wcay + 10))
            {
              //  MessageBox.Show(Cursor.Position.X.ToString() + " " + Cursor.Position.Y.ToString());
                return true;
            }

            return false;
        }

        private static Boolean IsMouseOnLeftEdge()
        {
            int x = Cursor.Position.X;

            return (x <= 0) ? true : false;
        }

        private static Boolean IsMouseOnRightEdge()
        {
           return (Cursor.Position.X >= Screen.PrimaryScreen.Bounds.Right - 5) ? true : false;
        }

        private static Boolean IsMouseOnTopEdge()
        {
            return (Cursor.Position.Y <= Screen.PrimaryScreen.Bounds.Top + 15) ? true : false;
        }



        private static Boolean IsWindowInLeftZone(IntPtr hwnd)
        {
            int x = GetWindowXPos(hwnd);

            int sx = Screen.PrimaryScreen.Bounds.Width;

            int zx = Screen.PrimaryScreen.Bounds.Width / 4;

            if (x < zx)
                return true;

            return false;
        }

        private static Boolean IsWindowInRightZone(IntPtr hwnd)
        {
            int x = GetWindowXWPos(hwnd);

            int sx = Screen.PrimaryScreen.WorkingArea.Width;

            int zx = sx - (int)Decimal.Divide(sx, 4);

            if (x > zx)
            {
                return true;
            }

            return false;
        }

        private static Boolean IsWindowInTopZone(IntPtr hwnd)
        {
            int y = GetWindowYPos(hwnd);

            int sy = Screen.PrimaryScreen.Bounds.Height;

            int zy = sy / 32;

            if (y < zy)
            {
                return true;
            }

            return false;
        }

        private Boolean AreSameForegroundWindowCoords()
        {
            IntPtr hwnd = new IntPtr(GetForegroundWindow());

            int wXPos = GetWindowXPos(hwnd);
            int wYPos = GetWindowYPos(hwnd);

            if (wXPos == lfw_x && wYPos == lfw_y)
                return true;

            lfw_x = wXPos;
            lfw_y = wYPos;

            return false;
        }

        private Boolean IsSameForegroundWindow()
        {
            IntPtr hwnd = new IntPtr(GetForegroundWindow());

            if (hwnd == lastForegroundWindow)
                return true;

            lastForegroundWindow = hwnd;
            // MessageBox.Show("!");
            return false;
        }

        private void SnapWindowLeft()
        {
            if (WindowIsSizable(new IntPtr(GetForegroundWindow())))
            {
                Rectangle screenie = Screen.PrimaryScreen.WorkingArea;
                int halfWidth = screenie.Width / 2;


                MoveWindow(new IntPtr(GetForegroundWindow()), 0, 0, halfWidth, screenie.Height, true);
            }
        }

        private void SnapWindowRight()
        {
            if (WindowIsSizable(new IntPtr(GetForegroundWindow())))
            {
                // MessageBox.Show("RIGHT"+GetWindowTitle(new IntPtr(GetForegroundWindow())));
                Rectangle screenie = Screen.PrimaryScreen.WorkingArea;
                int halfWidth = screenie.Width / 2;
                int halfHeight = screenie.Height / 2;

                MoveWindow(new IntPtr(GetForegroundWindow()), halfWidth, 0, halfWidth, screenie.Height, true);
            }
        }

        private void SnapWindowFill()
        {
            if (WindowIsSizable(new IntPtr(GetForegroundWindow())))
            {
                // MessageBox.Show("UP" + GetWindowTitle(new IntPtr(GetForegroundWindow())));
                Rectangle screenie = Screen.PrimaryScreen.WorkingArea;
                int halfWidth = screenie.Width / 2;
                ShowWindow(new IntPtr(GetForegroundWindow()), 9);

                MoveWindow(new IntPtr(GetForegroundWindow()), 0, 0, screenie.Width, screenie.Height, true);
            }
        }

        protected override void WndProc(ref Message m)
        {
      

            switch (m.Msg)
            {
                              
                case WM_HOTKEY:
                    {

                        if ((short)m.WParam == LeftSnap.HotkeyID)
                        {
                            //  MessageBox.Show("LEFT" + GetWindowTitle(new IntPtr(GetForegroundWindow())));
                            SnapWindowLeft();
                        }
                        else if ((short)m.WParam == WholeSnap.HotkeyID)
                        {
                            SnapWindowFill();
                        }
                        else if ((short)m.WParam == RightSnap.HotkeyID)
                        {
                            SnapWindowRight();
                        }

                        else if ((short)m.WParam == MinSnap.HotkeyID)
                        {
                            if (WindowIsSizable(new IntPtr(GetForegroundWindow())))
                            {
                                // MessageBox.Show("RIGHT"+GetWindowTitle(new IntPtr(GetForegroundWindow())));
                                Rectangle screenie = Screen.PrimaryScreen.Bounds;
                                int halfWidth = screenie.Width / 2;
                                int halfHeight = screenie.Height / 2;

                                ShowWindow(new IntPtr(GetForegroundWindow()), 2);
                            }
                        }

                        break;


                    }
                default:
                    {
                        base.WndProc(ref m);
                        break;
                    }
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GC.Collect();
            this.Visible = false;
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(200);
        }

        private bool IsMouseDownAnywhere()
        {
            return GetAsyncKeyState(Keys.LButton) != 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            IntPtr hwnd = new IntPtr(GetForegroundWindow());

            if (!IsSameForegroundWindow() || !AreSameForegroundWindowCoords())
                tickCounter = 0;

            if (IsMouseOnLeftEdge() && tickCounter > tickWaitLatency && !IsMouseDownAnywhere() && MouseIsOnWindowCaptionBar())
            {
                SnapWindowLeft();
                //    MessageBox.Show("SNAP");
                tickCounter = 0;
            }

            if (IsMouseOnRightEdge() && tickCounter > tickWaitLatency && !IsMouseDownAnywhere() && MouseIsOnWindowCaptionBar())
            {
                SnapWindowRight();
                //    MessageBox.Show("SNAP");
                tickCounter = 0;
            }

            if (IsMouseOnTopEdge() && tickCounter > tickWaitLatency+2 && !IsMouseDownAnywhere() && MouseIsOnWindowCaptionBar())
            {
                SnapWindowFill();
               // MessageBox.Show("SNAP");
                tickCounter = 0;
            }

            if (IsWindowInLeftZone(hwnd) && IsMouseDownAnywhere() && IsMouseOnLeftEdge())
            {
                //   Console.Beep();
                tickCounter++;
            }
            else if (IsWindowInRightZone(hwnd) && IsMouseDownAnywhere() && IsMouseOnRightEdge())
            {
                //   Console.Beep();
                tickCounter++;
            }
            else if (IsWindowInTopZone(hwnd) && IsMouseDownAnywhere() && IsMouseOnTopEdge())
            {
                tickCounter++;
            }
            else
            {
                tickCounter = 0;
            }



        }

        private void waitLatencyChooser_ValueChanged(object sender, EventArgs e)
        {
            tickWaitLatency = (int)waitLatencyChooser.Value;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MouseIsOnWindowCaptionBar();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Visible = false;
            button1_Click(null, null);
        }

    }
}
