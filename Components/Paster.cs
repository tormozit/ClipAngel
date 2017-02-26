using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace ClipAngel
{
    static class Paster
    {
        //public static event MyEventHandler MyEvent;
        //public class MyEventArgs : EventArgs
        //{
        //    public string EventType;
        //    public string Text;

        //    public MyEventArgs(string eventType, string text = "")
        //    {
        //        this.EventType = eventType;
        //        this.Text = text;
        //    }
        //}
        //public delegate void MyEventHandler(object sender, MyEventArgs eventArgs);

        //public static void FireEvent(object sender, MyEventArgs eventArgs)
        //{
        //    MyEvent(sender, eventArgs);
        //}

        static public EventWaitHandle GetPasteEventWaiter(int processID = 0)
        {
            if (processID == 0)
                processID = Process.GetCurrentProcess().Id;
            EventWaitHandle pasteEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "ClipAngelPaste" + processID);
            return pasteEvent;
        }

        static public EventWaitHandle GetSendCharsEventWaiter(int processID = 0)
        {
            if (processID == 0)
                processID = Process.GetCurrentProcess().Id;
            EventWaitHandle pasteEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "ClipAngelSendChars" + processID);
            return pasteEvent;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public static void SendChars()
        {
            string textToPaste = Clipboard.GetText();
            InputSimulator inputSimulator = new InputSimulator(); // http://inputsimulator.codeplex.com/
            inputSimulator.Keyboard.TextEntry(textToPaste);
        }

        public static void SendPaste()
        {
            // Spyed from AceText. Works in all windows including CMD and RDP
            const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

            // Release all key modifiers
            keybd_event((byte)VirtualKeyCode.SHIFT, 0x2A, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VirtualKeyCode.SHIFT, 0x36, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VirtualKeyCode.MENU, 0x38, KEYEVENTF_KEYUP, 0); // LEFT
            keybd_event((byte)VirtualKeyCode.LWIN, 0x5B, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VirtualKeyCode.RWIN, 0x5C, KEYEVENTF_KEYUP, 0);

            // Send CTLR+V
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, 0, 0);
            keybd_event((byte)'V', 0x2f, 0, 0);
            keybd_event((byte)'V', 0x2f, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, KEYEVENTF_KEYUP, 0);
        }

    }
}
