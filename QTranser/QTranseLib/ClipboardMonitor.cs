﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace QTranser.QTranseLib
{
    /// <summary>
    /// Clipboard Monitor class to notify if the clipboard content changes
    /// </summary>
    public class ClipboardMonitor
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private IntPtr Handle;

        /// <summary>
        /// Event for clipboard update notification.
        /// </summary>
        public event EventHandler ClipboardUpdate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">window or UserControl of the application.</param>
        /// <param name="start">Enable clipboard notification on startup or not.</param>
        public ClipboardMonitor(Object obj, bool start = true)
        {
            if (obj is Window)
            { Handle = new WindowInteropHelper((Window)obj).EnsureHandle(); }
            if (obj is UserControl)
            { Handle = ((HwndSource)PresentationSource.FromVisual((UserControl)obj)).Handle; }

            HwndSource.FromHwnd(Handle)?.AddHook(HwndHandler);
            if (start) Start();
        }

        /// <summary>
        /// Enable clipboard notification.
        /// </summary>
        public void Start()
        {
            NativeMethods.AddClipboardFormatListener(Handle);
        }

        /// <summary>
        /// Disable clipboard notification.
        /// </summary>
        public void Stop()
        {
            NativeMethods.RemoveClipboardFormatListener(Handle);
        }


        // 防止截切版被多次调用
        private int _i = 0;
        private int i
        {
            get
            {
                async void setI()
                {
                    await Task.Run(() =>
                    {
                        Thread.Sleep(20);
                        i = 0;
                    }
                    );
                }
                setI();
                return _i;
            }
            set
            {
                _i = value;
            }
        }
        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                if(i<1)
                {
                    this.ClipboardUpdate?.Invoke(this, new EventArgs());
                    i++;
                }
            }
            handled = false;
            return IntPtr.Zero;
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        }
    }
}
