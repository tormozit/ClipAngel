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

            ModifiersState mod = new ModifiersState();
            mod.ReleaseAll();

            // Send CTLR+V
            const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, 0, 0);
            keybd_event((byte)'V', 0x2f, 0, 0);
            keybd_event((byte)'V', 0x2f, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, KEYEVENTF_KEYUP, 0);

            //mod.RestoreState();
        }

        public static void SendCopy()
        {
            ModifiersState mod = new ModifiersState();
            mod.ReleaseAll();

            // Send CTLR+C
            const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, 0, 0);
            keybd_event((byte)'C', 0x2f, 0, 0);
            keybd_event((byte)'C', 0x2f, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, KEYEVENTF_KEYUP, 0);

            //mod.RestoreState();
        }
        public class KeyboardInfo
        {
            private KeyboardInfo() { }
            [DllImport("user32")]
            private static extern short GetKeyState(int vKey);
            public static KeyStateInfo GetKeyState(Keys key)
            {
                short keyState = GetKeyState((int)key);
                byte[] bits = BitConverter.GetBytes(keyState);
                bool toggled = bits[0] > 0, pressed = bits[1] > 0;
                return new KeyStateInfo(key, pressed, toggled);
            }
        }

        public struct KeyStateInfo
        {
            Keys _key;
            bool _isPressed,
                _isToggled;
            public KeyStateInfo(Keys key,
                            bool ispressed,
                            bool istoggled)
            {
                _key = key;
                _isPressed = ispressed;
                _isToggled = istoggled;
            }
            public static KeyStateInfo Default
            {
                get
                {
                    return new KeyStateInfo(Keys.None,
                                                false,
                                                false);
                }
            }
            public Keys Key
            {
                get { return _key; }
            }
            public bool IsPressed
            {
                get { return _isPressed; }
            }
            public bool IsToggled
            {
                get { return _isToggled; }
            }
        }

        class ModifiersState
        {
            public bool rshift, lshift, lcontrol, rcontrol, lalt, ralt, lwin, rwin;

            // bad idea to restore modifiers state - can make pressed while is already not pressed
            public void RestoreState()
            {
                if (lshift)
                    keybd_event((byte)VirtualKeyCode.SHIFT, 0x2A, 0, 0);
                if (rshift)
                    keybd_event((byte)VirtualKeyCode.SHIFT, 0x36, 0, 0);
                if (lcontrol)
                    keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, 0, 0);
                if (rcontrol)
                    keybd_event((byte)VirtualKeyCode.CONTROL, 0x9D, 0, 0);
                if (lalt)
                    keybd_event((byte)VirtualKeyCode.MENU, 0x38, 0, 0);
                if (ralt)
                    keybd_event((byte)VirtualKeyCode.MENU, 0xB8, 0, 0);
                if (lwin)
                    keybd_event((byte)VirtualKeyCode.LWIN, 0x5B, 0, 0);
                if (rwin)
                    keybd_event((byte)VirtualKeyCode.RWIN, 0x5C, 0, 0);
            }

            public void ReleaseAll()
            {
                //lalt = KeyboardInfo.GetKeyState(Keys.LMenu).IsPressed;
                //ralt = KeyboardInfo.GetKeyState(Keys.RMenu).IsPressed;
                //lcontrol = KeyboardInfo.GetKeyState(Keys.LControlKey).IsPressed;
                //rcontrol = KeyboardInfo.GetKeyState(Keys.RControlKey).IsPressed;
                //lshift = KeyboardInfo.GetKeyState(Keys.LShiftKey).IsPressed;
                //rshift = KeyboardInfo.GetKeyState(Keys.RShiftKey).IsPressed;
                //lwin = KeyboardInfo.GetKeyState(Keys.LWin).IsPressed;
                //rwin = KeyboardInfo.GetKeyState(Keys.RWin).IsPressed;

                const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
                keybd_event((byte)VirtualKeyCode.SHIFT, 0x2A, KEYEVENTF_KEYUP, 0); // LEFT
                keybd_event((byte)VirtualKeyCode.SHIFT, 0x36, KEYEVENTF_KEYUP, 0);
                keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, KEYEVENTF_KEYUP, 0); // LEFT
                keybd_event((byte)VirtualKeyCode.CONTROL, 0x9D, KEYEVENTF_KEYUP, 0);
                keybd_event((byte)VirtualKeyCode.MENU, 0x38, KEYEVENTF_KEYUP, 0); // LEFT
                keybd_event((byte)VirtualKeyCode.MENU, 0xB8, KEYEVENTF_KEYUP, 0);
                keybd_event((byte)VirtualKeyCode.LWIN, 0x5B, KEYEVENTF_KEYUP, 0);
                keybd_event((byte)VirtualKeyCode.RWIN, 0x5C, KEYEVENTF_KEYUP, 0);
            }
        }
    }
}
