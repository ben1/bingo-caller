/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : CheckBox.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : CheckBox class

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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit.Gui
{
    /// <summary>A CheckBox control</summary>
    public class CheckBox : Control
    {
        protected List<Quad> m_normalQuads;
        protected List<Quad> m_overQuads;
        protected List<Quad> m_downQuads;
        protected List<Quad> m_disabledQuads;
        protected Quad m_marker;

        protected enum Section { Left, Right, Top, Bottom, Background, TopLeft, TopRight, BottomLeft, BottomRight };

        /// <summary>Default Constructor</summary>
        public CheckBox()
        {
            // Empty
        }

        /// <summary>Creates a CheckBox</summary>
        /// <param name="id">Control ID</param>
        /// <param name="screenRect">Screen rectangle</param>
        /// <param name="text">CheckBox text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="textAlign">Text alignment</param>
        /// <param name="isChecked">Whether the checkbox is initially checked</param>
        /// <param name="font">Initialized CUnit.BitmapFont instance</param>
        /// <param name="node">ControlNode from XML file</param>
        /// <param name="info">Texture ImageInformation</param>
        public CheckBox( int id, RectangleF screenRect, string text, float fontSize, ColorValue textColor, TextAlign textAlign, bool isChecked, BitmapFont font, ControlNode node, ImageInformation info )
        {
            m_id = id;
            m_text = text;
            m_textAlign = textAlign;
            m_fontSize = fontSize;
            m_textColor = textColor;
            m_bFont = font;
            m_data = isChecked;
            m_position = new PointF( screenRect.X, screenRect.Y );
            m_hotspot = new Rectangle( (int)screenRect.X, (int)screenRect.Y, (int)screenRect.Width, (int)screenRect.Height );

            m_normalQuads = new List<Quad>();
            m_overQuads = new List<Quad>();
            m_downQuads = new List<Quad>();
            m_disabledQuads = new List<Quad>();

            float z = 0f;
            float rhw = 1f;
            foreach ( ImageNode i in node.Images )
            {
                RectangleF rect = i.Rectangle;
                TransformedColoredTextured topLeft = new TransformedColoredTextured(
                    screenRect.X, screenRect.Y, z, rhw,
                    i.Color, rect.X / (float)info.Width, rect.Y / (float)info.Height );
                TransformedColoredTextured topRight = new TransformedColoredTextured(
                    screenRect.Right, screenRect.Y, z, rhw,
                    i.Color, rect.Right / (float)info.Width, rect.Y / (float)info.Height );
                TransformedColoredTextured bottomRight = new TransformedColoredTextured(
                    screenRect.Right, screenRect.Bottom, z, rhw,
                    i.Color, rect.Right / (float)info.Width, rect.Bottom / (float)info.Height );
                TransformedColoredTextured bottomLeft = new TransformedColoredTextured(
                    screenRect.X, screenRect.Bottom, z, rhw,
                    i.Color, rect.X / (float)info.Width, rect.Bottom / (float)info.Height );
                if ( i.Name == "Normal" )
                {
                    m_normalQuads.Add( new Quad( topLeft, topRight, bottomLeft, bottomRight ) );
                }
                else if ( i.Name == "Over" )
                {
                    m_overQuads.Add( new Quad( topLeft, topRight, bottomLeft, bottomRight ) );
                }
                else if ( i.Name == "Down" )
                {
                    m_downQuads.Add( new Quad( topLeft, topRight, bottomLeft, bottomRight ) );
                }
                else if ( i.Name == "Disabled" )
                {
                    m_disabledQuads.Add( new Quad( topLeft, topRight, bottomLeft, bottomRight ) );
                }
                else if ( i.Name == "CheckMark" )
                {
                    m_marker = new Quad( topLeft, topRight, bottomLeft, bottomRight );
                }
            }
            if ( isChecked )
            {
                // Add checkmark
                m_normalQuads.Add( (Quad)m_marker.Clone() );
                m_overQuads.Add( (Quad)m_marker.Clone() );
                m_downQuads.Add( (Quad)m_marker.Clone() );
                m_disabledQuads.Add( (Quad)m_marker.Clone() );
            }

            BuildText();
        }

        /// <summary>Mouse Release event</summary>
        /// <param name="cursor">Mouse position</param>
        protected override void OnMouseRelease( Point cursor )
        {
            m_data = !(bool)m_data;
            if ( (bool)m_data == true )
            {
                // Add checkmark
                m_normalQuads.Add( (Quad)m_marker.Clone() );
                m_overQuads.Add( (Quad)m_marker.Clone() );
                m_downQuads.Add( (Quad)m_marker.Clone() );
                m_disabledQuads.Add( (Quad)m_marker.Clone() );
            }
            else
            {
                // Remove checkmark
                m_normalQuads.RemoveAt( 1 );
                m_overQuads.RemoveAt( 1 );
                m_downQuads.RemoveAt( 1 );
                m_disabledQuads.RemoveAt( 1 );
            }
        }

        /// <summary>Gets the Control's current Quads</summary>
        public override List<Quad> Quads
        {
            get
            {
                switch ( m_state )
                {
                    case ControlState.Normal:
                        return m_normalQuads;
                    case ControlState.Over:
                        return m_overQuads;
                    case ControlState.Down:
                        return m_downQuads;
                    default:
                        return m_disabledQuads;
                }
            }
        }

        /// <summary>Gets and sets the Panel's position</summary>
        public override PointF Position
        {
            get { return m_position; }
            set
            {
                float xOffset = value.X - m_position.X;
                float yOffset = value.Y - m_position.Y;
                m_position = value;

                // Reposition Quads
                for ( int i = 0; i < m_normalQuads.Count; i++ )
                {
                    m_normalQuads[i].X += xOffset;
                    m_normalQuads[i].Y += yOffset;
                    m_overQuads[i].X += xOffset;
                    m_overQuads[i].Y += yOffset;
                    m_downQuads[i].X += xOffset;
                    m_downQuads[i].Y += yOffset;
                    m_disabledQuads[i].X += xOffset;
                    m_disabledQuads[i].Y += yOffset;
                }
                m_marker.X += xOffset;
                m_marker.Y += yOffset;

                // Reposition hotspot
                m_hotspot = new Rectangle( (int)m_normalQuads[0].X, (int)m_normalQuads[0].Y, (int)m_normalQuads[0].Width, (int)m_normalQuads[0].Height );


                for ( int i = 0; i < m_fontQuads.Count; i++ )
                {
                    m_fontQuads[i].X += xOffset;
                    m_fontQuads[i].Y += yOffset;
                }
            }
        }

        /// <summary>Builds the Control text</summary>
        protected override void BuildText()
        {
            // Create text
            BitmapFont.Align alignment = BitmapFont.Align.Left;
            switch ( m_textAlign )
            {
                case TextAlign.Left:
                    {
                        float x = m_normalQuads[0].X - 1000f;
                        float y = m_normalQuads[0].Y + ( m_normalQuads[0].Height / 2f ) - ( m_fontSize / 2f );
                        float width = 1000f - m_fontPadding;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Right;
                        break;
                    }
                case TextAlign.Right:
                    {
                        float x = m_normalQuads[0].Right + m_fontPadding;
                        float y = m_normalQuads[0].Y + ( m_normalQuads[0].Height / 2f ) - ( m_fontSize / 2f );
                        float width = 1000f;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Left;
                        break;
                    }
                case TextAlign.Top:
                    {
                        float x = m_normalQuads[0].X - 500f;
                        float y = m_normalQuads[0].Y - m_fontPadding - m_fontSize;
                        float width = 1000f + m_normalQuads[0].Width;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Center;
                        break;
                    }
                case TextAlign.Bottom:
                    {
                        float x = m_normalQuads[0].X - 500f;
                        float y = m_normalQuads[0].Bottom + m_fontPadding;
                        float width = 1000f + m_normalQuads[0].Width;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Center;
                        break;
                    }
                case TextAlign.Center:
                    {
                        float x = m_normalQuads[0].X - 500f;
                        float y = m_normalQuads[0].Y + ( m_normalQuads[0].Height / 2f ) - ( m_fontSize / 2f );
                        float width = 1000f + m_normalQuads[0].Width;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Center;
                        break;
                    }
            }
            int index = m_bFont.AddString( m_text, m_textRect, alignment, m_fontSize, m_textColor, true );
            List<FontQuad> fontQuads = m_bFont.GetProcessedQuads( index );
            m_bFont.ClearString( index );

            // Convert FontQuads to Quads
            m_fontQuads = new List<Quad>( fontQuads.Count );
            for ( int i = 0; i < fontQuads.Count; i++ )
            {
                m_fontQuads.Add( new Quad( fontQuads[i].TopLeft, fontQuads[i].TopRight, fontQuads[i].BottomLeft, fontQuads[i].BottomRight ) );
            }
        }

        /// <summary>Gets and sets whether the Control is checked or not.</summary>
        public virtual bool Checked
        {
            get { return (bool)m_data; }
            set
            {
                if ( (bool)m_data == false && value == true )
                {
                    // Add checkmark
                    m_normalQuads.Add( (Quad)m_marker.Clone() );
                    m_overQuads.Add( (Quad)m_marker.Clone() );
                    m_downQuads.Add( (Quad)m_marker.Clone() );
                    m_disabledQuads.Add( (Quad)m_marker.Clone() );
                }
                else if ( (bool)m_data == true && value == false )
                {
                    // Remove checkmark
                    m_normalQuads.RemoveAt( 1 );
                    m_overQuads.RemoveAt( 1 );
                    m_downQuads.RemoveAt( 1 );
                    m_disabledQuads.RemoveAt( 1 );
                }
                m_data = value;

            }
        }
    }
}
