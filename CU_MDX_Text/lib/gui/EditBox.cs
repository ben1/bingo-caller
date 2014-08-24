/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : EditBox.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Text EditBox class

Created :  12/29/2005
Modified : 01/31/2005

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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Direct3D.CustomVertex;
using DirectInput = Microsoft.DirectX.DirectInput;

namespace CUnit.Gui
{
    public class EditBox : Control
    {
        private const int Backspace = (int)System.Windows.Forms.Keys.Back;
        private const int Delete = (int)System.Windows.Forms.Keys.Delete;
        private const int Left = (int)System.Windows.Forms.Keys.Left;
        private const int Right = (int)System.Windows.Forms.Keys.Right;
        private const int End = (int)System.Windows.Forms.Keys.End;
        private const int Home = (int)System.Windows.Forms.Keys.Home;
        private const int Return = (int)System.Windows.Forms.Keys.Return;

        private List<Quad> m_disabledQuads;
        private Quad m_cursor;
        private int m_cursorPosition;
        private int m_maxLength;
        private enum Section { Left, Right, Top, Bottom, Background, TopLeft, TopRight, BottomLeft, BottomRight };

        /// <summary>Creates an EditBox</summary>
        /// <param name="id">Control ID</param>
        /// <param name="screenRect">Screen rectangle</param>
        /// <param name="text">EditBox text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="maxLength">Max number of characters allowed in the edit box.</param>
        /// <param name="textColor">Text color</param>
        /// <param name="font">Bitmap font</param>
        /// <param name="node">ControlNode from XML file</param>
        /// <param name="info">Texture ImageInformation</param>
        public EditBox( int id, RectangleF screenRect, string text, float fontSize, int maxLength, ColorValue textColor, BitmapFont font, ControlNode node, ImageInformation info )
        {
            m_id = id;
            if ( text.Length > maxLength )
            {
                text.Remove( maxLength - 1 );
            }
            m_text = text;
            m_data = text;
            m_fontSize = fontSize;
            m_textColor = textColor;
            m_bFont = font;
            m_position = new PointF( screenRect.X, screenRect.Y );
            m_size = new SizeF( screenRect.Width, screenRect.Height );
            m_cursorPosition = 0;
            m_maxLength = maxLength;

            m_quads = new List<Quad>();
            m_disabledQuads = new List<Quad>();

            // Initialize Lists so we can access them with indices
            for ( int i = 0; i < 9; i++ )
            {
                m_quads.Add( new Quad() );
                m_disabledQuads.Add( new Quad() );
            }

            float z = 0f;
            float rhw = 1f;
            foreach ( ImageNode i in node.Images )
            {
                RectangleF rect = i.Rectangle;

                TransformedColoredTextured topLeft = new TransformedColoredTextured(
                    screenRect.X, screenRect.Y, z, rhw,
                    i.Color, rect.X / (float)info.Width, rect.Y / (float)info.Height );
                TransformedColoredTextured topRight = new TransformedColoredTextured(
                    screenRect.X + rect.Width, screenRect.Y, z, rhw,
                    i.Color, rect.Right / (float)info.Width, rect.Y / (float)info.Height );
                TransformedColoredTextured bottomRight = new TransformedColoredTextured(
                    screenRect.X + rect.Width, screenRect.Y + rect.Height, z, rhw,
                    i.Color, rect.Right / (float)info.Width, rect.Bottom / (float)info.Height );
                TransformedColoredTextured bottomLeft = new TransformedColoredTextured(
                    screenRect.X, screenRect.Y + rect.Height, z, rhw,
                    i.Color, rect.X / (float)info.Width, rect.Bottom / (float)info.Height );
                Quad q = new Quad( topLeft, topRight, bottomLeft, bottomRight );
                if ( i.Name.EndsWith( "TopLeftCorner" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.TopLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.TopLeft] = q;
                    }
                }
                else if ( i.Name.EndsWith( "TopBorder" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.Top] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.Top] = q;
                    }
                }
                else if ( i.Name.EndsWith( "TopRightCorner" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.TopRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.TopRight] = q;
                    }
                }
                else if ( i.Name.EndsWith( "LeftBorder" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.Left] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.Left] = q;
                    }
                }
                else if ( i.Name.EndsWith( "Background" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.Background] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.Background] = q;
                    }
                }
                else if ( i.Name.EndsWith( "RightBorder" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.Right] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.Right] = q;
                    }
                }
                else if ( i.Name.EndsWith( "BottomLeftCorner" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.BottomLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.BottomLeft] = q;
                    }
                }
                else if ( i.Name.EndsWith( "BottomBorder" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.Bottom] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.Bottom] = q;
                    }
                }
                else if ( i.Name.EndsWith( "BottomRightCorner" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.BottomRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.BottomRight] = q;
                    }
                }
                else if ( i.Name == "NormalCursor" )
                {
                    m_cursor = q;
                }
            }

            PositionQuads();
        }

        /// <summary>Mouse Release event</summary>
        /// <param name="cursor">Mouse position</param>
        protected override void OnMouseRelease( Point cursor )
        {
            if ( m_hasFocus )
            {
                // Add cursor
                if ( m_quads.Count == 9 )
                {
                    m_quads.Add( m_cursor );
                }
            }
            else
            {
                // Remove cursor
                m_quads.RemoveAt( 9 );
            }
        }

        /// <summary>Releases EditBox focus</summary>
        public void ReleaseFocus()
        {
            if ( m_hasFocus )
            {
                m_hasFocus = false;
                // Remove cursor
                m_quads.RemoveAt( 9 );
            }
        }

        /// <summary>Key down handler.</summary>
        /// <param name="pressedKeys">List of pressed keys</param>
        /// <param name="pressedChar">Pressed character</param>
        /// <param name="pressedKey">Pressed key from Form used for repeatable keys</param>
        /// <returns>true if a Control processed the keyboard, false otherwise</returns>
        public override bool OnKeyDown( List<Keys> pressedKeys, char pressedChar, int pressedKey )
        {
            if ( !m_hasFocus )
            {
                return false;
            }
            // Non-displayable character
            if ( pressedKey == Backspace )
            {
                if ( m_cursorPosition > 0 )
                {
                    m_cursorPosition--;
                    m_text = m_text.Remove( m_cursorPosition, 1 );
                }
            }
            else if ( pressedKey == Delete )
            {
                if ( m_cursorPosition < m_text.Length )
                {
                    m_text = m_text.Remove( m_cursorPosition, 1 );
                }
            }
            else if ( pressedKey == Left )
            {
                if ( m_cursorPosition <= 0 )
                {
                    return false;
                }
                if ( pressedKeys.Contains( Keys.ControlKey ) )
                {
                    if ( m_text[m_cursorPosition - 1] == ' ' )
                    {
                        while ( m_cursorPosition != 0 && m_text[m_cursorPosition - 1] == ' ' )
                        {
                            m_cursorPosition--;
                        }
                    }
                    else
                    {
                        while ( m_cursorPosition != 0 && m_text[m_cursorPosition - 1] != ' ' )
                        {
                            m_cursorPosition--;
                        }
                    }
                }
                else
                {
                    m_cursorPosition--;
                }
            }
            else if ( pressedKey == Right )
            {
                if ( m_cursorPosition >= m_text.Length )
                {
                    return false;
                }
                if ( pressedKeys.Contains( Keys.ControlKey ) )
                {
                    if ( m_text[m_cursorPosition] == ' ' )
                    {
                        while ( m_cursorPosition < m_text.Length && m_text[m_cursorPosition] == ' ' )
                        {
                            m_cursorPosition++;
                        }
                    }
                    else
                    {
                        while ( m_cursorPosition < m_text.Length && m_text[m_cursorPosition]  != ' ' )
                        {
                            m_cursorPosition++;
                        }
                    }
                }
                else
                {
                    m_cursorPosition++;
                }
            }
            else if ( pressedKey == End )
            {
                m_cursorPosition = m_text.Length;
            }
            else if ( pressedKey == Home )
            {
                m_cursorPosition = 0;
            }
            else if ( pressedKey == Return )
            {
                ReleaseFocus();
                return true;
            }

            if ( char.IsLetterOrDigit( pressedChar ) || char.IsPunctuation( pressedChar ) || char.IsSymbol( pressedChar ) || char.IsWhiteSpace( pressedChar ) )
            {
                if ( m_text.Length < m_maxLength && pressedChar != char.MinValue )
                {
                    if ( m_cursorPosition == m_text.Length )
                    {
                        m_text += pressedChar.ToString();
                    }
                    else
                    {
                        m_text = m_text.Insert( m_cursorPosition, pressedChar.ToString() );
                    }
                    m_cursorPosition++;
                }
            }

            m_data = m_text;
            BuildText();
            return true;
        }

        /// <summary>Gets and sets the EditBox's position</summary>
        public override PointF Position
        {
            get { return m_position; }
            set
            {
                float xOffset = value.X - m_position.X;
                float yOffset = value.Y - m_position.Y;
                m_position = value;

                for ( int i = 0; i < 9; i++ )
                {
                    m_quads[i].X += xOffset;
                    m_quads[i].Y += yOffset;
                    m_disabledQuads[i].X += xOffset;
                    m_disabledQuads[i].Y += yOffset;
                }
                for ( int i = 0; i < m_fontQuads.Count; i++ )
                {
                    m_fontQuads[i].X += xOffset;
                    m_fontQuads[i].Y += yOffset;
                }
                m_cursor.X += xOffset;
                m_cursor.Y += yOffset;
                BuildHotspot();
            }
        }

        /// <summary>Repositions the hot spot and text</summary>
        private void PositionQuads()
        {
            // Adjust middle column x
            m_quads[(int)Section.Top].X = m_quads[(int)Section.TopLeft].Right;
            m_quads[(int)Section.Background].X = m_quads[(int)Section.Left].Right;
            m_quads[(int)Section.Bottom].X = m_quads[(int)Section.BottomLeft].Right;
            m_disabledQuads[(int)Section.Top].X = m_disabledQuads[(int)Section.TopLeft].Right;
            m_disabledQuads[(int)Section.Background].X = m_disabledQuads[(int)Section.Left].Right;
            m_disabledQuads[(int)Section.Bottom].X = m_disabledQuads[(int)Section.BottomLeft].Right;

            // Adjust middle column width
            m_quads[(int)Section.Top].Width =
                m_size.Width - m_quads[(int)Section.TopLeft].Width -
                m_quads[(int)Section.TopRight].Width;
            m_disabledQuads[(int)Section.Top].Width =
                m_size.Width - m_disabledQuads[(int)Section.TopLeft].Width -
                m_disabledQuads[(int)Section.TopRight].Width;

            m_quads[(int)Section.Background].Width =
                m_size.Width - m_quads[(int)Section.Left].Width -
                m_quads[(int)Section.Right].Width;
            m_disabledQuads[(int)Section.Background].Width =
                m_size.Width - m_disabledQuads[(int)Section.Left].Width -
                m_disabledQuads[(int)Section.Right].Width;

            m_quads[(int)Section.Bottom].Width =
                m_size.Width - m_quads[(int)Section.BottomLeft].Width -
                m_quads[(int)Section.BottomLeft].Width;
            m_disabledQuads[(int)Section.Bottom].Width =
                m_size.Width - m_disabledQuads[(int)Section.BottomLeft].Width -
                m_disabledQuads[(int)Section.BottomLeft].Width;

            // Adjust right column X
            m_quads[(int)Section.TopRight].X = m_quads[(int)Section.Top].Right;
            m_quads[(int)Section.Right].X = m_quads[(int)Section.Background].Right;
            m_quads[(int)Section.BottomRight].X = m_quads[(int)Section.Bottom].Right;
            m_disabledQuads[(int)Section.TopRight].X = m_disabledQuads[(int)Section.Top].Right;
            m_disabledQuads[(int)Section.Right].X = m_disabledQuads[(int)Section.Background].Right;
            m_disabledQuads[(int)Section.BottomRight].X = m_disabledQuads[(int)Section.Bottom].Right;

            // Adjust middle row Y
            m_quads[(int)Section.Left].Y = m_quads[(int)Section.TopLeft].Bottom;
            m_quads[(int)Section.Background].Y = m_quads[(int)Section.Top].Bottom;
            m_quads[(int)Section.Right].Y = m_quads[(int)Section.TopRight].Bottom;
            m_disabledQuads[(int)Section.Left].Y = m_disabledQuads[(int)Section.TopLeft].Bottom;
            m_disabledQuads[(int)Section.Background].Y = m_disabledQuads[(int)Section.Top].Bottom;
            m_disabledQuads[(int)Section.Right].Y = m_disabledQuads[(int)Section.TopRight].Bottom;

            // Adjust middle row height
            m_quads[(int)Section.Left].Height =
                m_size.Height - m_quads[(int)Section.TopLeft].Height -
                m_quads[(int)Section.BottomLeft].Height;
            m_disabledQuads[(int)Section.Left].Height =
                m_size.Height - m_disabledQuads[(int)Section.TopLeft].Height -
                m_disabledQuads[(int)Section.BottomLeft].Height;

            m_quads[(int)Section.Background].Height =
                m_size.Height - m_quads[(int)Section.Top].Height -
                m_quads[(int)Section.Bottom].Height;
            m_disabledQuads[(int)Section.Background].Height =
                m_size.Height - m_disabledQuads[(int)Section.Top].Height -
                m_disabledQuads[(int)Section.Bottom].Height;

            m_quads[(int)Section.Right].Height =
                m_size.Height - m_quads[(int)Section.TopRight].Height -
                m_quads[(int)Section.BottomRight].Height;
            m_disabledQuads[(int)Section.Right].Height =
                m_size.Height - m_disabledQuads[(int)Section.TopRight].Height -
                m_disabledQuads[(int)Section.BottomRight].Height;

            // Adjust bottom row Y
            m_quads[(int)Section.BottomLeft].Y = m_quads[(int)Section.Left].Bottom;
            m_quads[(int)Section.Bottom].Y = m_quads[(int)Section.Background].Bottom;
            m_quads[(int)Section.BottomRight].Y = m_quads[(int)Section.Right].Bottom;
            m_disabledQuads[(int)Section.BottomLeft].Y = m_disabledQuads[(int)Section.Left].Bottom;
            m_disabledQuads[(int)Section.Bottom].Y = m_disabledQuads[(int)Section.Background].Bottom;
            m_disabledQuads[(int)Section.BottomRight].Y = m_disabledQuads[(int)Section.Right].Bottom;

            // Adjust cursor
            m_cursor.Height = m_fontSize;
            m_cursor.Width = 2f;

            BuildHotspot();

            // Place cursor at the end of the string
            m_cursorPosition = m_text.Length;

            BuildText();

        }

        /// <summary>Builds the text</summary>
        protected override void BuildText()
        {
            int index = m_bFont.AddString( m_text, new RectangleF( m_quads[(int)Section.Background].X + 2f,
                m_quads[(int)Section.Background].Y + 2f, m_quads[(int)Section.Background].Width, 
                m_quads[(int)Section.Background].Height ), BitmapFont.Align.Left, m_fontSize, m_textColor, true );
            List<FontQuad> fontQuads = m_bFont.GetProcessedQuads( index );
            m_bFont.ClearString( index );

            // Convert FontQuads to Quads
            m_fontQuads = new List<Quad>( fontQuads.Count );
            for ( int i = 0; i < fontQuads.Count; i++ )
            {
                m_fontQuads.Add( new Quad( fontQuads[i].TopLeft, fontQuads[i].TopRight, fontQuads[i].BottomLeft, fontQuads[i].BottomRight ) );
            }
            if ( m_cursorPosition != m_text.Length )
            {
                m_cursor.X = fontQuads[m_cursorPosition].X - ( fontQuads[m_cursorPosition].BitmapCharacter.XOffset * fontQuads[m_cursorPosition].SizeScale );
                m_cursor.Y = fontQuads[m_cursorPosition].Y - ( fontQuads[m_cursorPosition].BitmapCharacter.YOffset * fontQuads[m_cursorPosition].SizeScale );
            }
            else if ( m_text.Length > 0 )
            {
                m_cursor.X = fontQuads[m_cursorPosition - 1].X + ( fontQuads[m_cursorPosition - 1].BitmapCharacter.XAdvance * fontQuads[m_cursorPosition - 1].SizeScale );
                m_cursor.Y = fontQuads[m_cursorPosition - 1].Y - ( fontQuads[m_cursorPosition - 1].BitmapCharacter.YOffset * fontQuads[m_cursorPosition - 1].SizeScale );
            }
            else if ( m_text.Length == 0 )
            {
                m_cursor.X = m_quads[(int)Section.Background].X + 2f;
                m_cursor.Y = m_quads[(int)Section.Background].Y + 2f;
            }
        }

        /// <summary>Builds the hotspot</summary>
        private void BuildHotspot()
        {
            int hotspotWidth = (int)m_quads[(int)Section.Right].Right - (int)m_quads[(int)Section.Left].X;
            int hotspotHeight = (int)m_quads[(int)Section.Bottom].Bottom - (int)m_quads[(int)Section.Top].Y;
            m_hotspot = new Rectangle( (int)m_quads[(int)Section.TopLeft].X,
                (int)m_quads[(int)Section.TopLeft].Y, hotspotWidth, hotspotHeight );
        }

        /// <summary>Gets and sets whether the Control is disabled.</summary>
        public override bool Disabled
        {
            get
            {
                return base.Disabled;
            }
            set
            {
                base.Disabled = value;
                if ( Disabled )
                {
                    ReleaseFocus();
                }
            }
        }

        /// <summary>Gets the EditBox's current Quads</summary>
        public override List<Quad> Quads
        {
            get
            {
                switch ( m_state )
                {
                    case ControlState.Disabled:
                        return m_disabledQuads;
                    default:
                        return m_quads;
                }
            }
        }

        /// <summary>Gets and sets the Control's state.</summary>
        public override ControlState State
        {
            get
            {
                return base.State;
            }
            set
            {
                base.State = value;
                if ( Disabled )
                {
                    ReleaseFocus();
                }
            }
        }
    }
}
