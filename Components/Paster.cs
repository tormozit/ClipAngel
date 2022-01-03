using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
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

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        internal struct INPUT
        {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        internal struct MOUSEINPUT
        {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            /// get screen coordinates
            ClientToScreen(wndHandle, ref clientPoint);
            ClickOnPoint(clientPoint);
        }

        // http://stackoverflow.com/questions/10355286/programmatically-mouse-click-in-another-window
        public static void ClickOnPoint(Point clientPoint)
        {
            var oldPos = Cursor.Position;

            /// set cursor on coords, and press mouse
            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            var inputMouseDown = new INPUT();
            inputMouseDown.Type = 0; /// input type mouse
            inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

            var inputMouseUp = new INPUT();
            inputMouseUp.Type = 0; /// input type mouse
            inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

            var inputs = new INPUT[] {inputMouseDown, inputMouseUp};
            SendInput((uint) inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            /// return mouse 
            Cursor.Position = oldPos;
        }

        static public EventWaitHandle GetPasteEventWaiter(int processID = 0)
        {
            if (processID == 0)
                processID = Process.GetCurrentProcess().Id;
            EventWaitHandle pasteEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "ClipAngelPaste" + processID);
            return pasteEvent;
        }

        static public EventWaitHandle GetSendCharsEventWaiter(int processID = 0, bool slow = false)
        {
            if (processID == 0)
                processID = Process.GetCurrentProcess().Id;
            string name = "ClipAngelSendChars";
            if(slow)
                name += "Slow";
            else
                name += "Fast";
            EventWaitHandle pasteEvent = new EventWaitHandle(false, EventResetMode.ManualReset, name + processID);
            return pasteEvent;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public static void SendChars(Main main = null, bool slow = false)
        {
            string textToPaste = Clipboard.GetText();
            InputSimulator inputSimulator = new InputSimulator(); // http://inputsimulator.codeplex.com/
            main.ActivateAndCheckTargetWindow();
            //textToPaste = textToPaste.Replace("\r\n", "\n");
            if (!slow)
            {
                if (main.ActivateAndCheckTargetWindow(false))
                    inputSimulator.Keyboard.TextEntry(textToPaste);
            }
            else
            {
                Random random = new Random();
                foreach (char oneChar in textToPaste)
                {
                    // Top registered speed of human typing is 18 chars per second on 2020y 
                    Thread.Sleep(random.Next(70, 120));
                    if (!main.ActivateAndCheckTargetWindow(false))
                        break;
                    inputSimulator.Keyboard.TextEntry(oneChar);
                }
            }
        }

        public static void SendPaste(Main main = null)
        {
            ModifiersState mod = new ModifiersState();
            mod.ReleaseAll(false, main);
            InputSimulator simulator = new InputSimulator();
            simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            simulator.Keyboard.KeyPress(VirtualKeyCode.VK_V);
            simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
        }

        public static void SendCopy(bool waitForComplete = false)
        {
            ModifiersState mod = new ModifiersState();
            mod.ReleaseAll();

            // Send CTRL+C
            const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, 0, 0);
            keybd_event((byte)'C', 0x2e, 0, 0);
            keybd_event((byte)'C', 0x2e, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, KEYEVENTF_KEYUP, 0);
            if (waitForComplete)
                SendKeys.SendWait("^"); // Helps to wait event processing and data in clipboard

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

        public class ModifiersState
        {
            //public bool rshift, lshift, lcontrol, rcontrol, lalt, ralt, lwin, rwin;

            //// bad idea to restore modifiers state - can make pressed while is already not pressed
            //public void RestoreState()
            //{
            //    if (lshift)
            //        keybd_event((byte)VirtualKeyCode.SHIFT, 0x2A, 0, 0);
            //    if (rshift)
            //        keybd_event((byte)VirtualKeyCode.SHIFT, 0x36, 0, 0);
            //    if (lcontrol)
            //        keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, 0, 0);
            //    if (rcontrol)
            //        keybd_event((byte)VirtualKeyCode.CONTROL, 0x9D, 0, 0);
            //    if (lalt)
            //        keybd_event((byte)VirtualKeyCode.MENU, 0x38, 0, 0);
            //    if (ralt)
            //        keybd_event((byte)VirtualKeyCode.MENU, 0xB8, 0, 0);
            //    if (lwin)
            //        keybd_event((byte)VirtualKeyCode.LWIN, 0x5B, 0, 0);
            //    if (rwin)
            //        keybd_event((byte)VirtualKeyCode.RWIN, 0x5C, 0, 0);
            //}

            public void ReleaseAll(bool forced = false, Main main = null)
            {
                if (main != null)
                {
                    Main.ClipboardOwner clipboardOwner = main.GetClipboardOwnerLockerInfo(true); // Looks like this call eliminates ALT down state in remote desktop
                    //forced = forced || clipboardOwner.isRemoteDesktop;
                }
                // This way works good only with big delay. Check IncrementalPasteHotkey!
                //InputSimulator simulator = new InputSimulator();
                //if (forced || KeyboardInfo.GetKeyState(Keys.LShiftKey).IsPressed)
                //    simulator.Keyboard.KeyUp(VirtualKeyCode.LSHIFT);
                //if (forced || KeyboardInfo.GetKeyState(Keys.RShiftKey).IsPressed)
                //    simulator.Keyboard.KeyUp(VirtualKeyCode.RSHIFT);
                //if (forced || KeyboardInfo.GetKeyState(Keys.LControlKey).IsPressed)
                //    simulator.Keyboard.KeyUp(VirtualKeyCode.LCONTROL);
                //if (forced || KeyboardInfo.GetKeyState(Keys.RControlKey).IsPressed)
                //    simulator.Keyboard.KeyUp(VirtualKeyCode.RCONTROL);
                //if (forced || KeyboardInfo.GetKeyState(Keys.LMenu).IsPressed)
                //    simulator.Keyboard.KeyUp(VirtualKeyCode.LMENU);
                //if (forced || KeyboardInfo.GetKeyState(Keys.RMenu).IsPressed)
                //    simulator.Keyboard.KeyUp(VirtualKeyCode.RMENU);
                //if (forced || KeyboardInfo.GetKeyState(Keys.LWin).IsPressed)
                //    simulator.Keyboard.KeyUp(VirtualKeyCode.LWIN);
                //if (forced || KeyboardInfo.GetKeyState(Keys.RWin).IsPressed)
                //    simulator.Keyboard.KeyUp(VirtualKeyCode.RWIN);
                //simulator.Keyboard.Sleep(100);

                // This way is the best
                const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
                if (forced || KeyboardInfo.GetKeyState(Keys.LShiftKey).IsPressed)
                    keybd_event((byte)VirtualKeyCode.SHIFT, 0x2A, KEYEVENTF_KEYUP, 0); // LEFT
                if (forced || KeyboardInfo.GetKeyState(Keys.RShiftKey).IsPressed)
                    keybd_event((byte)VirtualKeyCode.SHIFT, 0x36, KEYEVENTF_KEYUP, 0);
                if (forced || KeyboardInfo.GetKeyState(Keys.LControlKey).IsPressed)
                    keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, KEYEVENTF_KEYUP, 0); // LEFT
                if (forced || KeyboardInfo.GetKeyState(Keys.RControlKey).IsPressed)
                    keybd_event((byte)VirtualKeyCode.CONTROL, 0x9D, KEYEVENTF_KEYUP, 0);
                if (forced || KeyboardInfo.GetKeyState(Keys.LMenu).IsPressed)
                    keybd_event((byte)VirtualKeyCode.MENU, 0x38, KEYEVENTF_KEYUP, 0); // LEFT
                if (forced || KeyboardInfo.GetKeyState(Keys.RMenu).IsPressed)
                    keybd_event((byte)VirtualKeyCode.MENU, 0xB8, KEYEVENTF_KEYUP, 0);
                if (forced || KeyboardInfo.GetKeyState(Keys.LWin).IsPressed)
                    keybd_event((byte)VirtualKeyCode.LWIN, 0x5B, KEYEVENTF_KEYUP, 0);
                if (forced || KeyboardInfo.GetKeyState(Keys.RWin).IsPressed)
                    keybd_event((byte)VirtualKeyCode.RWIN, 0x5C, KEYEVENTF_KEYUP, 0);
            }
        }
    }
}
