using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CUnit
{
    partial class Framework
    {
        // Keyboard
        private List<Keys> m_pressedKeys = new List<Keys>();
        private List<Keys> m_keyLock = new List<Keys>();
        private int m_numPressedKeys = 0;
        private char m_pressedChar = char.MinValue;
        private int m_pressedKey = 0;

        // Mouse
        private System.Drawing.Point m_mousePosition;
        private int m_xDelta = 0;
        private int m_yDelta = 0;
        private int m_zDelta = 0;
        private bool[] m_mouseButtons = new bool[3];
        private int m_numPressedButtons = 0;

        /// <summary>Mouse move event.</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseMove( MouseEventArgs e )
        {
            System.Drawing.Point p = this.PointToClient( Cursor.Position );
            m_xDelta = p.X - m_mousePosition.X;
            m_yDelta = p.Y - m_mousePosition.Y;
            m_mousePosition = p;
        }

        /// <summary>Mouse wheel event.</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseWheel( MouseEventArgs e )
        {
            m_zDelta = e.Delta;
        }

        /// <summary>Mouse down event.</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseDown( MouseEventArgs e )
        {
            m_numPressedButtons++;
            switch ( e.Button )
            {
                case MouseButtons.Left:
                    m_mouseButtons[0] = true;
                    break;
                case MouseButtons.Middle:
                    m_mouseButtons[1] = true;
                    break;
                case MouseButtons.Right:
                    m_mouseButtons[2] = true;
                    break;
            }
        }

        /// <summary>Mouse up event.</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseUp( MouseEventArgs e )
        {
            switch ( e.Button )
            {
                case MouseButtons.Left:
                    m_mouseButtons[0] = false;
                    break;
                case MouseButtons.Middle:
                    m_mouseButtons[1] = false;
                    break;
                case MouseButtons.Right:
                    m_mouseButtons[2] = false;
                    break;
            }
        }

        /// <summary>Returns true if the main mouse buttons are all down.</summary>
        private bool NoButtonsDown
        {
            get { return m_mouseButtons[0] == m_mouseButtons[1] == m_mouseButtons[2] == false; }
        }

        /// <summary>Gets and sets the cursor positon</summary>
        public System.Drawing.Point CursorPosition
        {
            get { return m_mousePosition; }
            set 
            {
                m_mousePosition = value;
                Cursor.Position = this.PointToScreen( value );
            }
        }

        /// <summary>Key down event.</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnKeyDown( KeyEventArgs e )
        {
            if ( !m_pressedKeys.Contains( e.KeyCode ) && !m_keyLock.Contains( e.KeyCode ) )
            {
                m_pressedKeys.Add( e.KeyCode );
                m_numPressedKeys++;
            }
            // Store key value for repeatable non-character keys in GUI EditBox
            // such as backspace, arrows, and delete
            m_pressedKey = e.KeyValue;
        }

        /// <summary>Key press event.</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            // OnKeyPress lets us grab character keys
            m_pressedChar = e.KeyChar;
        }

        /// <summary>Key up event.</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnKeyUp( KeyEventArgs e )
        {
            m_pressedKeys.Remove( e.KeyCode );
            m_keyLock.Remove( e.KeyCode );
        }

        /// <summary>Locks a key so it is only read once per key down.</summary>
        /// <param name="key">Key to lock</param>
        public void LockKey( Keys key )
        {
            m_keyLock.Add( key );
        }
    }
}
