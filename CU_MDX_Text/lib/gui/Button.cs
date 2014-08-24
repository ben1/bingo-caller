/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Button.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Button class

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
    /// <summary>A button control</summary>
    public class Button : Control
    {
        private List<Quad> m_normalQuads;
        private List<Quad> m_overQuads;
        private List<Quad> m_downQuads;
        private List<Quad> m_disabledQuads;
        
        private enum Section { Left, Right, Top, Bottom, Background, TopLeft, TopRight, BottomLeft, BottomRight };

        /// <summary>Creates a Button</summary>
        /// <param name="id">Control ID</param>
        /// <param name="screenRect">Screen rectangle</param>
        /// <param name="text">Button text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="font">Bitmap font</param>
        /// <param name="node">ControlNode from XML file</param>
        /// <param name="info">Texture ImageInformation</param>
        public Button( int id, RectangleF screenRect, string text, float fontSize, ColorValue textColor, BitmapFont font, ControlNode node, ImageInformation info )
        {
            m_id = id;
            m_text = text;
            m_fontSize = fontSize;
            m_textColor = textColor;
            m_bFont = font;
            m_position = new PointF( screenRect.X, screenRect.Y );
            m_size = new SizeF( screenRect.Width, screenRect.Height );

            m_normalQuads = new List<Quad>();
            m_overQuads = new List<Quad>();
            m_downQuads = new List<Quad>();
            m_disabledQuads = new List<Quad>();

            // Initialize Lists so we can access them with indices
            for ( int i = 0; i < 9; i++ )
            {
                m_normalQuads.Add( new Quad() );
                m_overQuads.Add( new Quad() );
                m_downQuads.Add( new Quad() );
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
                        m_normalQuads[(int)Section.TopLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.TopLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.TopLeft] = q;
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
                        m_normalQuads[(int)Section.Top] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.Top] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.Top] = q;
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
                        m_normalQuads[(int)Section.TopRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.TopRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.TopRight] = q;
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
                        m_normalQuads[(int)Section.Left] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.Left] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.Left] = q;
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
                        m_normalQuads[(int)Section.Background] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.Background] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.Background] = q;
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
                        m_normalQuads[(int)Section.Right] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.Right] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.Right] = q;
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
                        m_normalQuads[(int)Section.BottomLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.BottomLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.BottomLeft] = q;
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
                        m_normalQuads[(int)Section.Bottom] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.Bottom] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.Bottom] = q;
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
                        m_normalQuads[(int)Section.BottomRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.BottomRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.BottomRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.BottomRight] = q;
                    }
                }
            }

            PositionQuads();
        }

        /// <summary>Gets the Button's current Quads</summary>
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

                for ( int i = 0; i < 9; i++ )
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
                for ( int i = 0; i < m_fontQuads.Count; i++ )
                {
                    m_fontQuads[i].X += xOffset;
                    m_fontQuads[i].Y += yOffset;
                }
                BuildHotspot();
            }
        }

        /// <summary>Repositions the hot spot and text</summary>
        private void PositionQuads()
        {
            // Adjust middle column x
            m_normalQuads[(int)Section.Top].X = m_normalQuads[(int)Section.TopLeft].Right;
            m_overQuads[(int)Section.Top].X = m_overQuads[(int)Section.TopLeft].Right;
            m_downQuads[(int)Section.Top].X = m_downQuads[(int)Section.TopLeft].Right;
            m_disabledQuads[(int)Section.Top].X = m_disabledQuads[(int)Section.TopLeft].Right;

            m_normalQuads[(int)Section.Background].X = m_normalQuads[(int)Section.Left].Right;
            m_overQuads[(int)Section.Background].X = m_overQuads[(int)Section.Left].Right;
            m_downQuads[(int)Section.Background].X = m_downQuads[(int)Section.Left].Right;
            m_disabledQuads[(int)Section.Background].X = m_disabledQuads[(int)Section.Left].Right;

            m_normalQuads[(int)Section.Bottom].X = m_normalQuads[(int)Section.BottomLeft].Right;
            m_overQuads[(int)Section.Bottom].X = m_overQuads[(int)Section.BottomLeft].Right;
            m_downQuads[(int)Section.Bottom].X = m_downQuads[(int)Section.BottomLeft].Right;
            m_disabledQuads[(int)Section.Bottom].X = m_disabledQuads[(int)Section.BottomLeft].Right;

            // Adjust middle column width
            m_normalQuads[(int)Section.Top].Width =
                m_size.Width - m_normalQuads[(int)Section.TopLeft].Width -
                m_normalQuads[(int)Section.TopRight].Width;
            m_overQuads[(int)Section.Top].Width =
                m_size.Width - m_overQuads[(int)Section.TopLeft].Width -
                m_overQuads[(int)Section.TopRight].Width;
            m_downQuads[(int)Section.Top].Width =
                m_size.Width - m_downQuads[(int)Section.TopLeft].Width -
                m_downQuads[(int)Section.TopRight].Width;
            m_disabledQuads[(int)Section.Top].Width =
                m_size.Width - m_disabledQuads[(int)Section.TopLeft].Width -
                m_disabledQuads[(int)Section.TopRight].Width;

            m_normalQuads[(int)Section.Background].Width =
                m_size.Width - m_normalQuads[(int)Section.Left].Width -
                m_normalQuads[(int)Section.Right].Width;
            m_overQuads[(int)Section.Background].Width =
                m_size.Width - m_overQuads[(int)Section.Left].Width -
                m_overQuads[(int)Section.Right].Width;
            m_downQuads[(int)Section.Background].Width =
                m_size.Width - m_downQuads[(int)Section.Left].Width -
                m_downQuads[(int)Section.Right].Width;
            m_disabledQuads[(int)Section.Background].Width =
                m_size.Width - m_disabledQuads[(int)Section.Left].Width -
                m_disabledQuads[(int)Section.Right].Width;

            m_normalQuads[(int)Section.Bottom].Width =
                m_size.Width - m_normalQuads[(int)Section.BottomLeft].Width -
                m_normalQuads[(int)Section.BottomLeft].Width;
            m_overQuads[(int)Section.Bottom].Width =
                m_size.Width - m_overQuads[(int)Section.BottomLeft].Width -
                m_overQuads[(int)Section.BottomLeft].Width;
            m_downQuads[(int)Section.Bottom].Width = m_size.Width -
                m_downQuads[(int)Section.BottomLeft].Width -
                m_downQuads[(int)Section.BottomLeft].Width;
            m_disabledQuads[(int)Section.Bottom].Width =
                m_size.Width - m_disabledQuads[(int)Section.BottomLeft].Width -
                m_disabledQuads[(int)Section.BottomLeft].Width;

            // Adjust right column X
            m_normalQuads[(int)Section.TopRight].X = m_normalQuads[(int)Section.Top].Right;
            m_overQuads[(int)Section.TopRight].X = m_overQuads[(int)Section.Top].Right;
            m_downQuads[(int)Section.TopRight].X = m_downQuads[(int)Section.Top].Right;
            m_disabledQuads[(int)Section.TopRight].X = m_disabledQuads[(int)Section.Top].Right;

            m_normalQuads[(int)Section.Right].X = m_normalQuads[(int)Section.Background].Right;
            m_overQuads[(int)Section.Right].X = m_overQuads[(int)Section.Background].Right;
            m_downQuads[(int)Section.Right].X = m_downQuads[(int)Section.Background].Right;
            m_disabledQuads[(int)Section.Right].X = m_disabledQuads[(int)Section.Background].Right;

            m_normalQuads[(int)Section.BottomRight].X = m_normalQuads[(int)Section.Bottom].Right;
            m_overQuads[(int)Section.BottomRight].X = m_overQuads[(int)Section.Bottom].Right;
            m_downQuads[(int)Section.BottomRight].X = m_downQuads[(int)Section.Bottom].Right;
            m_disabledQuads[(int)Section.BottomRight].X = m_disabledQuads[(int)Section.Bottom].Right;

            // Adjust middle row Y
            m_normalQuads[(int)Section.Left].Y = m_normalQuads[(int)Section.TopLeft].Bottom;
            m_overQuads[(int)Section.Left].Y = m_overQuads[(int)Section.TopLeft].Bottom;
            m_downQuads[(int)Section.Left].Y = m_downQuads[(int)Section.TopLeft].Bottom;
            m_disabledQuads[(int)Section.Left].Y = m_disabledQuads[(int)Section.TopLeft].Bottom;

            m_normalQuads[(int)Section.Background].Y = m_normalQuads[(int)Section.Top].Bottom;
            m_overQuads[(int)Section.Background].Y = m_overQuads[(int)Section.Top].Bottom;
            m_downQuads[(int)Section.Background].Y = m_downQuads[(int)Section.Top].Bottom;
            m_disabledQuads[(int)Section.Background].Y = m_disabledQuads[(int)Section.Top].Bottom;

            m_normalQuads[(int)Section.Right].Y = m_normalQuads[(int)Section.TopRight].Bottom;
            m_overQuads[(int)Section.Right].Y = m_overQuads[(int)Section.TopRight].Bottom;
            m_downQuads[(int)Section.Right].Y = m_downQuads[(int)Section.TopRight].Bottom;
            m_disabledQuads[(int)Section.Right].Y = m_disabledQuads[(int)Section.TopRight].Bottom;

            // Adjust middle row height
            m_normalQuads[(int)Section.Left].Height =
                m_size.Height - m_normalQuads[(int)Section.TopLeft].Height -
                m_normalQuads[(int)Section.BottomLeft].Height;
            m_overQuads[(int)Section.Left].Height =
                m_size.Height - m_overQuads[(int)Section.TopLeft].Height -
                m_overQuads[(int)Section.BottomLeft].Height;
            m_downQuads[(int)Section.Left].Height =
                m_size.Height - m_downQuads[(int)Section.TopLeft].Height -
                m_downQuads[(int)Section.BottomLeft].Height;
            m_disabledQuads[(int)Section.Left].Height =
                m_size.Height - m_disabledQuads[(int)Section.TopLeft].Height -
                m_disabledQuads[(int)Section.BottomLeft].Height;

            m_normalQuads[(int)Section.Background].Height =
                m_size.Height - m_normalQuads[(int)Section.Top].Height -
                m_normalQuads[(int)Section.Bottom].Height;
            m_overQuads[(int)Section.Background].Height =
                m_size.Height - m_overQuads[(int)Section.Top].Height -
                m_overQuads[(int)Section.Bottom].Height;
            m_downQuads[(int)Section.Background].Height =
                m_size.Height - m_downQuads[(int)Section.Top].Height -
                m_downQuads[(int)Section.Bottom].Height;
            m_disabledQuads[(int)Section.Background].Height =
                m_size.Height - m_disabledQuads[(int)Section.Top].Height -
                m_disabledQuads[(int)Section.Bottom].Height;

            m_normalQuads[(int)Section.Right].Height =
                m_size.Height - m_normalQuads[(int)Section.TopRight].Height -
                m_normalQuads[(int)Section.BottomRight].Height;
            m_overQuads[(int)Section.Right].Height =
                m_size.Height - m_overQuads[(int)Section.TopRight].Height -
                m_overQuads[(int)Section.BottomRight].Height;
            m_downQuads[(int)Section.Right].Height = m_size.Height -
                m_downQuads[(int)Section.TopRight].Height -
                m_downQuads[(int)Section.BottomRight].Height;
            m_disabledQuads[(int)Section.Right].Height =
                m_size.Height - m_disabledQuads[(int)Section.TopRight].Height -
                m_disabledQuads[(int)Section.BottomRight].Height;

            // Adjust bottom row Y
            m_normalQuads[(int)Section.BottomLeft].Y = m_normalQuads[(int)Section.Left].Bottom;
            m_overQuads[(int)Section.BottomLeft].Y = m_overQuads[(int)Section.Left].Bottom;
            m_downQuads[(int)Section.BottomLeft].Y = m_downQuads[(int)Section.Left].Bottom;
            m_disabledQuads[(int)Section.BottomLeft].Y = m_disabledQuads[(int)Section.Left].Bottom;

            m_normalQuads[(int)Section.Bottom].Y = m_normalQuads[(int)Section.Background].Bottom;
            m_overQuads[(int)Section.Bottom].Y = m_overQuads[(int)Section.Background].Bottom;
            m_downQuads[(int)Section.Bottom].Y = m_downQuads[(int)Section.Background].Bottom;
            m_disabledQuads[(int)Section.Bottom].Y = m_disabledQuads[(int)Section.Background].Bottom;

            m_normalQuads[(int)Section.BottomRight].Y = m_normalQuads[(int)Section.Right].Bottom;
            m_overQuads[(int)Section.BottomRight].Y = m_overQuads[(int)Section.Right].Bottom;
            m_downQuads[(int)Section.BottomRight].Y = m_downQuads[(int)Section.Right].Bottom;
            m_disabledQuads[(int)Section.BottomRight].Y = m_disabledQuads[(int)Section.Right].Bottom;

            BuildHotspot();

            BuildText();
        }

        /// <summary>Builds the text</summary>
        protected override void BuildText()
        {
            int index = m_bFont.AddString( m_text, new RectangleF( (float)m_hotspot.X,
                (float)m_hotspot.Y + ( (float)m_hotspot.Height / 2f ) - ( m_fontSize / 2f ),
                m_hotspot.Width, m_hotspot.Height ), BitmapFont.Align.Center, m_fontSize, m_textColor, true );
            List<FontQuad> fontQuads = m_bFont.GetProcessedQuads( index );
            m_bFont.ClearString( index );

            // Convert FontQuads to Quads
            m_fontQuads = new List<Quad>( fontQuads.Count );
            for ( int i = 0; i < fontQuads.Count; i++ )
            {
                m_fontQuads.Add( new Quad( fontQuads[i].TopLeft, fontQuads[i].TopRight, 
                    fontQuads[i].BottomLeft, fontQuads[i].BottomRight ) );
            }
        }

        /// <summary>Builds the hotspot</summary>
        private void BuildHotspot()
        {
            int hotspotWidth = (int)m_normalQuads[(int)Section.Right].Right - (int)m_normalQuads[(int)Section.Left].X;
            int hotspotHeight = (int)m_normalQuads[(int)Section.Bottom].Bottom - (int)m_normalQuads[(int)Section.Top].Y;
            m_hotspot = new Rectangle( (int)m_normalQuads[(int)Section.TopLeft].X,
                (int)m_normalQuads[(int)Section.TopLeft].Y, hotspotWidth, hotspotHeight );
        }
    }
}
