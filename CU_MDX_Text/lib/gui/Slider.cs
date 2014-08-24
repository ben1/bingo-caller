/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Slider.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Slider class

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
    /// <summary>A Slider control</summary>
    public class Slider : Control
    {
        private List<Quad> m_normalQuads;
        private List<Quad> m_overQuads;
        private List<Quad> m_downQuads;
        private List<Quad> m_disabledQuads;
        private List<Rectangle> m_hotspots;
        private float m_min;
        private float m_max;

        private enum Section { Left, Right, Middle, Marker };

        /// <summary>Creates a Button</summary>
        /// <param name="id">Control ID</param>
        /// <param name="rectangle">Screen rectangle</param>
        /// <param name="text">Button text</param>
        /// <param name="node">ControlNode from XML file</param>
        /// <param name="info">Texture ImageInformation</param>
        public Slider( int id, PointF position, float width, float min, float max, float current, string text, float fontSize, ColorValue textColor, Control.TextAlign textAlignment, BitmapFont font, ControlNode node, ImageInformation info )
        {
            if ( max <= min )
            {
                throw new Exception( "Slider max must be greater than Slider min." );
            }

            m_id = id;
            m_text = text;
            m_fontSize = fontSize;
            m_textColor = textColor;
            m_bFont = font;
            m_textAlign = textAlignment;
            m_position = new PointF( position.X, position.Y );

            m_min = min;
            m_max = max;
            m_data = Math.Max( current, min );
            m_data = Math.Min( (float)m_data, max );

            m_normalQuads = new List<Quad>();
            m_overQuads = new List<Quad>();
            m_downQuads = new List<Quad>();
            m_disabledQuads = new List<Quad>();
            m_hotspots = new List<Rectangle>();

            // Initialize Lists so we can access them with indices
            m_hotspots.Add( new Rectangle() );
            m_hotspots.Add( new Rectangle() );

            for ( int i = 0; i < 4; i++ )
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
                    position.X, position.Y, z, rhw,
                    i.Color, rect.X / (float)info.Width, rect.Y / (float)info.Height );
                TransformedColoredTextured topRight = new TransformedColoredTextured(
                    position.X + rect.Width, position.Y, z, rhw,
                    i.Color, rect.Right / (float)info.Width, rect.Y / (float)info.Height );
                TransformedColoredTextured bottomRight = new TransformedColoredTextured(
                    position.X + rect.Width, position.Y + rect.Height, z, rhw,
                    i.Color, rect.Right / (float)info.Width, rect.Bottom / (float)info.Height );
                TransformedColoredTextured bottomLeft = new TransformedColoredTextured(
                    position.X, position.Y + rect.Height, z, rhw,
                    i.Color, rect.X / (float)info.Width, rect.Bottom / (float)info.Height );
                Quad q = new Quad( topLeft, topRight, bottomLeft, bottomRight );
                if ( i.Name.EndsWith( "LeftCap" ) )
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
                else if ( i.Name.EndsWith( "Middle" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Section.Middle] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.Middle] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.Middle] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.Middle] = q;
                    }
                }
                else if ( i.Name.EndsWith( "RightCap" ) )
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
                else if ( i.Name.EndsWith( "Marker" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Section.Marker] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Section.Marker] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_downQuads[(int)Section.Marker] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.Marker] = q;
                    }
                }
            }
            m_size = new SizeF( width, m_normalQuads[(int)Section.Middle].Height );
            PositionQuads();
        }

        /// <summary>Updates the current value based on the slider's position.</summary>
        private void ScreenXToCurrentValue()
        {
            float screenRange = m_normalQuads[(int)Section.Middle].Width - m_normalQuads[(int)Section.Marker].Width;
            m_data = ( ( m_max - m_min ) * ( m_normalQuads[(int)Section.Marker].X - m_normalQuads[(int)Section.Middle].X ) ) / screenRange + m_min;
        }

        /// <summary>Updates the slider's position based on the current value.</summary>
        private void CurrentValueToScreenX()
        {
            float screenRange = m_normalQuads[(int)Section.Middle].Width - m_normalQuads[(int)Section.Marker].Width;
            m_normalQuads[(int)Section.Marker].X = 
                ( ( ( (float)m_data - m_min ) / ( m_max - m_min ) ) * screenRange ) + m_normalQuads[(int)Section.Middle].X;
            m_overQuads[(int)Section.Marker].X = m_normalQuads[(int)Section.Marker].X;
            m_downQuads[(int)Section.Marker].X = m_normalQuads[(int)Section.Marker].X;
            m_disabledQuads[(int)Section.Marker].X = m_normalQuads[(int)Section.Marker].X;
        }

        /// <summary>Sets the value of the Slider</summary>
        /// <param name="value">New value</param>
        public void SetValue( float value )
        {
            // Constrain to max and min values
            value = Math.Min( m_max, value );
            value = Math.Max( m_min, value );
            m_data = value;
            CurrentValueToScreenX();
        }

        /// <summary>Checks is the mouse is over the Control's hotspot</summary>
        /// <param name="cursor">Mouse position</param>
        /// <returns>true if the cursor is over the Control's hotspot, false otherwise.</returns>
        public override bool Contains( Point cursor )
        {
            foreach ( Rectangle r in m_hotspots )
            {
                if ( r.Contains( cursor ) )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Mouse Down event</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        protected override void OnMouseDown( Point cursor, bool[] buttons )
        {
            // Move slider
            m_normalQuads[(int)Section.Marker].X = (float)cursor.X - ( m_normalQuads[(int)Section.Marker].Width / 2f );
            m_overQuads[(int)Section.Marker].X = m_normalQuads[(int)Section.Marker].X;
            m_downQuads[(int)Section.Marker].X = m_normalQuads[(int)Section.Marker].X;
            m_disabledQuads[(int)Section.Marker].X = m_normalQuads[(int)Section.Marker].X;

            // Constrain slider
            float screenRange = m_normalQuads[(int)Section.Middle].Width - m_normalQuads[(int)Section.Marker].Width;
            float max = screenRange + m_normalQuads[(int)Section.Middle].X;
            float min = m_normalQuads[(int)Section.Middle].X;

            if ( m_normalQuads[(int)Section.Marker].X > max )
            {
                m_normalQuads[(int)Section.Marker].X = max;
                m_overQuads[(int)Section.Marker].X = max;
                m_downQuads[(int)Section.Marker].X = max;
                m_disabledQuads[(int)Section.Marker].X = max;
            }
            if ( m_normalQuads[(int)Section.Marker].X < min )
            {
                m_normalQuads[(int)Section.Marker].X = min;
                m_overQuads[(int)Section.Marker].X = min;
                m_downQuads[(int)Section.Marker].X = min;
                m_disabledQuads[(int)Section.Marker].X = min;
            }
            ScreenXToCurrentValue();
            BuildHotspots();
        }

        /// <summary>Gets the Slider's current Quads</summary>
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


                for ( int i = 0; i < 4; i++ )
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
                BuildHotspots();
            }
        }

        /// <summary>Repositions the hot spot and text</summary>
        private void PositionQuads()
        {
            // Adjust middle x
            m_normalQuads[(int)Section.Middle].X = m_normalQuads[(int)Section.Left].Right;
            m_overQuads[(int)Section.Middle].X = m_overQuads[(int)Section.Left].Right;
            m_downQuads[(int)Section.Middle].X = m_downQuads[(int)Section.Left].Right;
            m_disabledQuads[(int)Section.Middle].X = m_disabledQuads[(int)Section.Left].Right;

            // Adjust middle width
            m_normalQuads[(int)Section.Middle].Width =
                m_size.Width - m_normalQuads[(int)Section.Left].Width -
                m_normalQuads[(int)Section.Right].Width;
            m_overQuads[(int)Section.Middle].Width =
                m_size.Width - m_overQuads[(int)Section.Left].Width -
                m_overQuads[(int)Section.Right].Width;
            m_downQuads[(int)Section.Middle].Width =
                m_size.Width - m_downQuads[(int)Section.Left].Width -
                m_downQuads[(int)Section.Right].Width;
            m_disabledQuads[(int)Section.Middle].Width =
                m_size.Width - m_disabledQuads[(int)Section.Left].Width -
                m_disabledQuads[(int)Section.Right].Width;

            // Adjust right X
            m_normalQuads[(int)Section.Right].X = m_normalQuads[(int)Section.Middle].Right;
            m_overQuads[(int)Section.Right].X = m_overQuads[(int)Section.Middle].Right;
            m_downQuads[(int)Section.Right].X = m_downQuads[(int)Section.Middle].Right;
            m_disabledQuads[(int)Section.Right].X = m_disabledQuads[(int)Section.Middle].Right;

            // Adjust marker Y
            m_normalQuads[(int)Section.Marker].Y -=
                ( m_normalQuads[(int)Section.Marker].Height - m_normalQuads[(int)Section.Middle].Height ) / 2;
            m_overQuads[(int)Section.Marker].Y -=
                ( m_overQuads[(int)Section.Marker].Height - m_overQuads[(int)Section.Middle].Height ) / 2;
            m_downQuads[(int)Section.Marker].Y -=
                ( m_downQuads[(int)Section.Marker].Height - m_downQuads[(int)Section.Middle].Height ) / 2;
            m_disabledQuads[(int)Section.Marker].Y -=
                ( m_disabledQuads[(int)Section.Marker].Height - m_disabledQuads[(int)Section.Middle].Height ) / 2;

            // Adjust marker
            CurrentValueToScreenX();

            BuildHotspots();

            BuildText();
        }

        /// <summary>Builds the hotspots</summary>
        private void BuildHotspots()
        {
            // Reposition hotspots
            // Marker is a hotspot
            m_hotspots[0] = new Rectangle(
                (int)m_normalQuads[(int)Section.Marker].X,
                (int)m_normalQuads[(int)Section.Marker].Y,
                (int)m_normalQuads[(int)Section.Marker].Width,
                (int)m_normalQuads[(int)Section.Marker].Height );

            // Slider is a hotspot with a pixel buffer around top and bottom
            m_hotspots[1] = new Rectangle(
                (int)m_normalQuads[(int)Section.Left].X,
                (int)m_normalQuads[(int)Section.Middle].Y - 2,
                (int)m_size.Width,
                (int)m_normalQuads[(int)Section.Middle].Height + 2 );
        }

        /// <summary>Builds the text</summary>
        protected override void BuildText()
        {
            // Create text
            BitmapFont.Align alignment = BitmapFont.Align.Left;
            switch ( m_textAlign )
            {
                case TextAlign.Left:
                    {
                        float x = m_normalQuads[(int)Section.Left].X - 1000f;
                        float y = m_normalQuads[(int)Section.Left].Y + 
                            ( m_normalQuads[(int)Section.Middle].Height / 2f ) - ( m_fontSize / 2f );
                        float width = 1000f - m_fontPadding;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Right;
                        break;
                    }
                case TextAlign.Right:
                    {
                        float x = m_normalQuads[(int)Section.Right].Right + m_fontPadding;
                        float y = m_normalQuads[(int)Section.Left].Y + 
                            ( m_normalQuads[(int)Section.Middle].Height / 2f ) - ( m_fontSize / 2f );
                        float width = 1000f;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Left;
                        break;
                    }
                case TextAlign.Top:
                    {
                        float x = m_normalQuads[(int)Section.Middle].X - 500f;
                        float y = m_normalQuads[(int)Section.Marker].Y - m_fontPadding - m_fontSize;
                        float width = 1000f + m_normalQuads[(int)Section.Middle].Width;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Center;
                        break;
                    }
                case TextAlign.Bottom:
                    {
                        float x = m_normalQuads[(int)Section.Middle].X - 500f;
                        float y = m_normalQuads[(int)Section.Marker].Bottom + m_fontPadding;
                        float width = 1000f + m_normalQuads[(int)Section.Middle].Width;
                        float height = m_fontSize * 2f;
                        m_textRect = new RectangleF( x, y, width, height );
                        alignment = BitmapFont.Align.Center;
                        break;
                    }
                case TextAlign.Center:
                    {
                        float x = m_normalQuads[(int)Section.Middle].X - 500f;
                        float y = m_normalQuads[(int)Section.Middle].Y + 
                            ( m_normalQuads[(int)Section.Middle].Height / 2f ) - ( m_fontSize / 2f );
                        float width = 1000f + m_normalQuads[(int)Section.Middle].Width;
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
    }
}
