﻿using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D;
using XI = Microsoft.Xna.Framework.Input;

namespace Code2015.GUI
{
    public enum MouseButton { Left, Middle, Right };

    public delegate void MouseClickHandler(MouseButton mouse, int x, int y);
    static class MouseInput
    {
        /// <summary>
        ///  上一次更新时鼠标的状态
        /// </summary>
        static XI.MouseState oldState;
        /// <summary>
        ///  当前鼠标状态
        /// </summary>
        static XI.MouseState currentState;


        /// <summary>
        ///  鼠标任一按键由按下变为松开时引发此事件
        /// </summary>
        public static event MouseClickHandler MouseUp;

        /// <summary>
        ///  鼠标任一按键由松开变为按下时引发此事件
        /// </summary>
        public static event MouseClickHandler MouseDown;


        public static int DX
        {
            get { return currentState.X - oldState.X; }
        }
        public static int DY 
        {
            get { return currentState.Y - oldState.Y; }
        }

        public static int X 
        {
            get { return currentState.X; }
        }

        public static int Y
        {
            get { return currentState.Y; }
        }

        public static int ScrollWheelValue 
        {
            get { return currentState.ScrollWheelValue; }
        }
        public static int DScrollWheelValue 
        {
            get { return currentState.ScrollWheelValue - oldState.ScrollWheelValue; }
        }
        public static bool IsMouseUpLeft
        {
            get;
            private set;
        }
        public static bool IsMouseDownLeft
        {
            get;
            private set;
        }
        public static bool IsMouseUpRight
        {
            get;
            private set;
        }
        public static bool IsMouseDownRight
        {
            get;
            private set;
        }

        public static bool IsLeftPressed 
        {
            get { return currentState.LeftButton == XI.ButtonState.Pressed; }
        }
        public static bool IsRightPressed
        {
            get { return currentState.RightButton == XI.ButtonState.Pressed; }
        }

        public static void Update(GameTime time)
        {
            IsMouseUpLeft = false;
            IsMouseUpRight = false;
            IsMouseDownLeft = false;
            IsMouseDownRight = false;

            oldState = currentState;
            currentState = XI.Mouse.GetState();

            if (currentState.LeftButton == XI.ButtonState.Pressed &&
                oldState.LeftButton == XI.ButtonState.Released)
            {
                if (MouseDown != null)
                {
                    MouseDown(MouseButton.Left, currentState.X, currentState.Y);
                }
                IsMouseDownLeft = true;
            }
            if (currentState.MiddleButton == XI.ButtonState.Pressed &&
                oldState.MiddleButton == XI.ButtonState.Released)
            {
                if (MouseDown != null)
                {
                    MouseDown(MouseButton.Middle, currentState.X, currentState.Y);
                }
            }
            if (currentState.RightButton == XI.ButtonState.Pressed &&
                 oldState.RightButton == XI.ButtonState.Released)
            {
                if (MouseDown != null)
                {
                    MouseDown(MouseButton.Right, currentState.X, currentState.Y);
                }
                IsMouseDownRight = true;
            }

            if (currentState.LeftButton == XI.ButtonState.Released &&
                oldState.LeftButton == XI.ButtonState.Pressed)
            {
                if (MouseUp != null)
                {
                    MouseUp(MouseButton.Left, currentState.X, currentState.Y);
                }
                IsMouseUpLeft = true;
            }
            if (currentState.MiddleButton == XI.ButtonState.Released &&
                oldState.MiddleButton == XI.ButtonState.Pressed)
            {
                if (MouseUp != null)
                {
                    MouseUp(MouseButton.Middle, currentState.X, currentState.Y);
                }
            }
            if (currentState.RightButton == XI.ButtonState.Released &&
                oldState.RightButton == XI.ButtonState.Pressed)
            {
                if (MouseUp != null)
                {
                    MouseUp(MouseButton.Right, currentState.X, currentState.Y);
                }
                IsMouseUpRight = true;
            }

        }
    }
}
