/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Control.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Base Control class

Created :  12/20/2005
Modified : 12/24/2005

Copyright (c) 2006 C-Unit.com

This software is provided 'as-is', without any express or implied warranty. In no event will 
the authors be held liable for any damages arising from the use of this software.

Permission is granted to anyone to use this software for any purpose, including commercial 
applications, and to alter it and redistribute it freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not claim that you wrote 
       the original software. If you use this software in a product, an acknowledgment in the 
       product documentation would be appreciated but is not required.

    2. Altered source versions must be plainly marked as such, and must not be misrepresented 
       as being the original software.

    3. This notice may not be removed or altered from any source distribution.

* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace CUnit.Gui
{
    /// <summary>Base Control class.</summary>
    public abstract class Control
    {
        public enum ControlState { Normal, Over, Down, Disabled };
        public enum TextAlign { Left, Right, Top, Bottom, Center };

        protected int m_id;
        protected List<Quad> m_quads;
        protected List<Quad> m_fontQuads;
        protected Rectangle m_hotspot;
        protected ControlState m_state;
        protected TextAlign m_textAlign;
        protected RectangleF m_textRect;
        protected SizeF m_size;
        protected bool m_disabled;
        protected bool m_hasFocus;
        protected bool m_mouseDown;
        protected object m_data;
        protected int m_zDepth;
        protected string m_text = string.Empty;
        protected bool m_hasTouchDownPoint;
        protected Point m_touchDownPoint;
        protected PointF m_position;
        protected BitmapFont m_bFont;
        protected float m_fontSize;
        protected ColorValue m_textColor;
        protected float m_fontPadding;

        private int m_startVertex;
        private int m_fontStartVertex;
        private int m_panelID;

        // Events
        public event GuiManager.ControlDelegate OnControl;

        /// <summary>Default constructor</summary>
        public Control()
        {
            m_panelID = 0;
            m_fontPadding = 5f;
            m_state = ControlState.Normal;
            m_textRect = new RectangleF();
            m_quads = new List<Quad>();
            m_fontQuads = new List<Quad>();
            m_disabled = false;
            m_hasFocus = false;
            m_hasTouchDownPoint = false;
            m_mouseDown = false;
            m_data = null;
        }

        /// <summary>Mouse handler</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        /// <param name="zDelta">Mouse wheel delta</param>
        /// <returns>true if the Control processed the mouse, false otherwise</returns>
        public bool MouseHandler( Point cursor, bool[] buttons, float zDelta )
        {
            // Grab the initial mouse down point
            if ( !m_hasTouchDownPoint && buttons[0] )
            {
                m_touchDownPoint = cursor;
                m_hasTouchDownPoint = true;
            }
            else if ( m_hasTouchDownPoint && !buttons[0] )
            {
                m_hasTouchDownPoint = false;
            }

            if ( ( !( this is EditBox ) && ( Contains( cursor ) || m_hasFocus ) ) || ( ( this is EditBox ) && Contains( cursor ) ) )
            {
                if ( zDelta != 0f )
                {
                    OnZDelta( zDelta );
                }
                // In order to send message, mouse must have been pressed and
                // released over the hotspot.
                if ( !buttons[0] )
                {
                    if ( m_mouseDown )
                    {
                        m_mouseDown = false;
                        if ( this is EditBox )
                        {
                            // EditBox has toggled focus
                            m_hasFocus = !m_hasFocus;
                        }
                        else
                        {
                            m_hasFocus = false;
                        }
                        if ( Contains( cursor ) )
                        {
                            OnMouseRelease( cursor );
                            if ( OnControl != null && !(this is Panel) && ( !(this is ListBox) || ( ( this is ListBox ) && ( this as ListBox ).HasNewData ) ) )
                            {
                                OnControl( m_id, Data );
                            }
                        }
                    }
                    bool result = true;
                    // ListBoxes need to continuously check mouse since it has sub parts that change on mouse over
                    // Mouseover on Panels should be able to reset over state on back controls.
                    if ( State == ControlState.Over && !( this is ListBox ) && !( this is Panel ) )
                    {
                        // State is already over
                        result = false;
                    }
                    

                    State = ControlState.Over;
                    OnMouseOver( cursor, buttons );
                    return result;
                }
                else if ( buttons[0] && m_hasTouchDownPoint && 
                    Contains( m_touchDownPoint ) || m_hasFocus )
                {
                    State = ControlState.Down;
                    OnMouseDown( cursor, buttons );

                    // Slider sends data while mouse is down
                    if ( ( this is Slider ) && OnControl != null )
                    {
                        OnControl( m_id, Data );
                    }

                    m_mouseDown = true;
                    if ( !( this is EditBox ) ) 
                    {
                        m_hasFocus = true;
                    }
                    return true;
                }
            }
            else if ( !Contains( cursor ) && State != ControlState.Normal )
            {
                if ( !buttons[0] && !( this is EditBox ) )
                {
                    m_hasFocus = false;
                }
                if ( !m_hasFocus )
                {
                    State = ControlState.Normal;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Keyboard handler.</summary>
        /// <param name="pressedKeys">List of pressed keys</param>
        /// <param name="pressedChar">Pressed character</param>
        /// <param name="pressedKey">Pressed key from Form used for repeatable keys</param>
        /// <returns>true if a Control processed the keyboard, false otherwise</returns>
        public virtual bool KeyboardHandler( List<System.Windows.Forms.Keys> pressedKeys, char pressedChar, int pressedKey )
        {
            if ( OnKeyDown( pressedKeys, pressedChar, pressedKey ) )
            {
                if ( OnControl != null )
                {
                    OnControl( m_id, Data );
                    return true;
                }
            }
            return false;
        }

        /// <summary>Key down handler.</summary>
        /// <param name="pressedKeys">List of pressed keys</param>
        /// <param name="pressedChar">Pressed character</param>
        /// <param name="pressedKey">Pressed key from Form used for repeatable keys</param>
        /// <returns>true if a Control processed the keyboard, false otherwise</returns>
        public virtual bool OnKeyDown( List<System.Windows.Forms.Keys> pressedKeys, char pressedChar, int pressedKey )
        {
            return false;
        }

        /// <summary>Checks is the mouse is over the Control's hotspot</summary>
        /// <param name="cursor">Mouse position</param>
        /// <returns>true if the cursor is over the Control's hotspot, false otherwise.</returns>
        public virtual bool Contains( Point cursor )
        {
            return m_hotspot.Contains( cursor );
        }

        /// <summary>Mouse Over event</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        protected virtual void OnMouseOver( Point cursor, bool[] buttons )
        {
            // Empty
        }

        /// <summary>Mouse Down event</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        protected virtual void OnMouseDown( Point cursor, bool[] buttons )
        {
            // Empty
        }

        /// <summary>Mouse Release event</summary>
        /// <param name="cursor">Mouse position</param>
        protected virtual void OnMouseRelease( Point cursor )
        {
            // Empty
        }

        /// <summary>Mouse wheel event</summary>
        /// <param name="cursor">Mouse wheel delta</param>
        protected virtual void OnZDelta( float zDelta )
        {
            // Empty
        }

        /// <summary>Build the text</summary>
        protected virtual void BuildText()
        {
            // Empty
        }

        /// <summary>Gets the Control's ID.</summary>
        public virtual int ID
        {
            get { return m_id; }
        }

        /// <summary>Gets the Control's Quads.</summary>
        public virtual List<Quad> Quads
        {
            get { return m_quads; }
        }

        /// <summary>Gets the Control's Quads.</summary>
        public virtual List<Quad> FontQuads
        {
            get { return m_fontQuads; }
        }

        /// <summary>Gets the Control's Data.</summary>
        public virtual object Data
        {
            get { return m_data; }
        }

        /// <summary>Gets and sets the Control's state.</summary>
        public virtual ControlState State
        {
            get { return m_state; }
            set { m_state = value; }
        }

        /// <summary>Gets and sets whether the Control is disabled.</summary>
        public virtual bool Disabled
        {
            get { return ( m_state == ControlState.Disabled ); }
            set
            {
                if ( value )
                {
                    State = ControlState.Disabled;
                }
                else
                {
                    State = ControlState.Normal;
                }
            }
        }

        /// <summary>Gets and sets the Control's Z-Depth</summary>
        public virtual int ZDepth
        {
            get { return m_zDepth; }
            set { m_zDepth = value; }
        }

        /// <summary>Gets and sets the Control's starting vertex in the GuiManager VertexBuffer</summary>
        public virtual int StartVertex
        {
            get { return m_startVertex; }
            set { m_startVertex = value; }
        }

        /// <summary>Gets and sets the Control's starting vertex in the GuiManager font VertexBuffer</summary>
        public virtual int FontStartVertex
        {
            get { return m_fontStartVertex; }
            set { m_fontStartVertex = value; }
        }

        /// <summary>Gets the Control's text.</summary>
        public virtual string Text
        {
            get { return m_text; }
            set { m_text = value; BuildText(); }
        }

        /// <summary>Gets and sets the Control's position.</summary>
        public virtual PointF Position
        {
            get { return m_position; }
            set { m_position = value; }
        }

        /// <summary>Gets and sets the Control's text padding from the Control.</summary>
        public virtual float FontPadding
        {
            get { return m_fontPadding; }
            set { m_fontPadding = value; }
        }

        /// <summary>Gets and sets the Control's associated Panel ID.</summary>
        public virtual int PanelID
        {
            get { return m_panelID; }
            set { m_panelID = value; }
        }

        /// <summary>Gets and sets whether the Control has focus.</summary>
        public virtual bool HasFocus
        {
            get { return m_hasFocus; }
            set { m_hasFocus = value; }
        }
    }

    /// <summary>Sorts Controls by Z Depth</summary>
    public class ControlSorter : IComparer<Control>
    {
        /// <summary>IComparer implementation</summary>
        /// <param name="x">Control 1</param>
        /// <param name="y">Control 2</param>
        public int Compare( Control x, Control y )
        {
            if ( x.ZDepth < y.ZDepth )
            {
                return 1;
            }
            if ( x.ZDepth == y.ZDepth )
            {
                // Panel goes underneath its Controls
                if ( ( x is Panel ) && !( y is Panel ) )
                {
                    return -1;
                }
                else if ( ( y is Panel ) && !( x is Panel ) )
                {
                    return 1;
                }

                // For Controls on same Panel, sort bottom to top
                if ( x.Position.Y < y.Position.Y )
                {
                    return 1;
                }
                if ( x.Position.Y > y.Position.Y )
                {
                    return -1;
                }
                return 0;
            }
            return -1;
        }
    }
}
