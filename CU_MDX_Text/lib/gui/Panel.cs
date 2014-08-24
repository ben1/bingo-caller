/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Panel.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : GUI Panel class

Created :  12/20/2005
Modified : 12/22/2005

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
    /// <summary>A Panel. Panels are moveable containers that can hold other Controls</summary>
    public class Panel : Control
    {
        private int m_numControls;
        private bool m_locked;
        private float m_xOffset;
        private float m_yOffset;
        private enum Section { Left, Right, Top, Bottom, Background, TopLeft, TopRight, BottomLeft, BottomRight };
        /// <summary>Creates a Panel</summary>
        /// <param name="id">Control ID</param>
        /// <param name="screenRect">Screen screenRect</param>
        /// <param name="size">Button size in pixels</param>
        /// <param name="text">Button text</param>
        public Panel( int id, RectangleF screenRect, ControlNode node, ImageInformation info )
        {
            m_id = id;
            m_locked = false;
            m_numControls = 0;
            m_quads = new List<Quad>( 9 );
            m_position = new PointF( screenRect.X, screenRect.Y );
            m_size = new SizeF( screenRect.Width, screenRect.Height );

            // Initialize List so we can access it with indices
            for ( int i = 0; i < 9; i++ )
            {
                m_quads.Add( new Quad() );
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

                if ( i.Name == "LeftBorder" )
                {
                    m_quads[(int)Section.Left] = q;
                }
                else if ( i.Name == "TopBorder" )
                {
                    m_quads[(int)Section.Top] = q;
                }
                else if ( i.Name == "RightBorder" )
                {
                    m_quads[(int)Section.Right] = q;
                }
                else if ( i.Name == "BottomBorder" )
                {
                    m_quads[(int)Section.Bottom] = q;
                }
                else if ( i.Name == "Background" )
                {
                    m_quads[(int)Section.Background] = q;
                }
                else if ( i.Name == "TopLeftCorner" )
                {
                    m_quads[(int)Section.TopLeft] = q;
                }
                else if ( i.Name == "TopRightCorner" )
                {
                    m_quads[(int)Section.TopRight] = q;
                }
                else if ( i.Name == "BottomLeftCorner" )
                {
                    m_quads[(int)Section.BottomLeft] = q;
                }
                else if ( i.Name == "BottomRightCorner" )
                {
                    m_quads[(int)Section.BottomRight] = q;
                }
            }

            PositionQuads();
        }

        /// <summary>Mouse down event.</summary>
        /// <param name="cursor">Mouse position.</param>
        /// <param name="buttons">Mouse buttons.</param>
        protected override void OnMouseDown( Point cursor, bool[] buttons )
        {
            if ( m_locked )
            {
                return;
            }
            m_xOffset = cursor.X - m_touchDownPoint.X;
            m_yOffset = cursor.Y - m_touchDownPoint.Y;
            foreach ( Quad q in m_quads )
            {
                q.X += m_xOffset;
                q.Y += m_yOffset;
            }
            m_hotspot.X += (int)m_xOffset;
            m_hotspot.Y += (int)m_yOffset;
            m_touchDownPoint.X += (int)m_xOffset;
            m_touchDownPoint.Y += (int)m_yOffset;
        }

        /// <summary>Gets and sets the Panel's position</summary>
        public override PointF Position
        {
            get { return m_position; }
            set
            {
                m_position = value;

                for ( int i = 0; i < 9; i++ )
                {
                    m_quads[i].X = value.X;
                    m_quads[i].Y = value.Y;
                }
                PositionQuads();
            }
        }

        /// <summary>Repositions all the quads</summary>
        private void PositionQuads()
        {
            // Adjust middle column x
            m_quads[(int)Section.Top].X += m_quads[(int)Section.TopLeft].Width;
            m_quads[(int)Section.Background].X += m_quads[(int)Section.Left].Width;
            m_quads[(int)Section.Bottom].X += m_quads[(int)Section.BottomLeft].Width;

            // Adjust middle column width
            m_quads[(int)Section.Top].Width =
                m_size.Width - m_quads[(int)Section.TopLeft].Width -
                m_quads[(int)Section.TopRight].Width;
            m_quads[(int)Section.Background].Width =
                m_size.Width - m_quads[(int)Section.Left].Width -
                m_quads[(int)Section.Right].Width;
            m_quads[(int)Section.Bottom].Width =
                m_size.Width - m_quads[(int)Section.BottomLeft].Width -
                m_quads[(int)Section.BottomLeft].Width;

            // Adjust right column X
            m_quads[(int)Section.TopRight].X +=
                m_quads[(int)Section.TopLeft].Width +
                m_quads[(int)Section.Top].Width;
            m_quads[(int)Section.Right].X +=
                m_quads[(int)Section.Left].Width +
                m_quads[(int)Section.Background].Width;
            m_quads[(int)Section.BottomRight].X +=
                m_quads[(int)Section.BottomLeft].Width +
                m_quads[(int)Section.Bottom].Width;

            // Adjust middle row Y
            m_quads[(int)Section.Left].Y += m_quads[(int)Section.TopLeft].Height;
            m_quads[(int)Section.Background].Y += m_quads[(int)Section.Top].Height;
            m_quads[(int)Section.Right].Y += m_quads[(int)Section.TopRight].Height;

            // Adjust middle row height
            m_quads[(int)Section.Left].Height =
                m_size.Height - m_quads[(int)Section.TopLeft].Height -
                m_quads[(int)Section.BottomLeft].Height;
            m_quads[(int)Section.Background].Height =
                m_size.Height - m_quads[(int)Section.Top].Height -
                m_quads[(int)Section.Bottom].Height;
            m_quads[(int)Section.Right].Height =
                m_size.Height - m_quads[(int)Section.TopRight].Height -
                m_quads[(int)Section.BottomRight].Height;

            // Adjust bottom row Y
            m_quads[(int)Section.BottomLeft].Y +=
                m_quads[(int)Section.TopLeft].Height +
                m_quads[(int)Section.Left].Height;
            m_quads[(int)Section.Bottom].Y +=
                m_quads[(int)Section.Top].Height +
                m_quads[(int)Section.Background].Height;
            m_quads[(int)Section.BottomRight].Y +=
                m_quads[(int)Section.TopRight].Height +
                m_quads[(int)Section.Right].Height;

            // Reposition hotspot
            int hotspotWidth = (int)m_quads[(int)Section.Left].Width +
                    (int)m_quads[(int)Section.Background].Width +
                    (int)m_quads[(int)Section.Right].Width;
            int hotspotHeight = (int)m_quads[(int)Section.Top].Height +
                (int)m_quads[(int)Section.Background].Height +
                (int)m_quads[(int)Section.Bottom].Height;
            m_hotspot = new Rectangle( (int)m_quads[(int)Section.TopLeft].X,
                (int)m_quads[(int)Section.TopLeft].Y, hotspotWidth, hotspotHeight );
        }

        /// <summary>Gets and sets the number of controls in the Panel.</summary>
        public int NumControls
        {
            get { return m_numControls; }
            set { m_numControls = value; }
        }

        /// <summary>Gets the x offset used to drag the Panel.</summary>
        public float XOffset
        {
            get { return m_xOffset; }
        }

        /// <summary>Gets the y offset used to drag the Panel.</summary>
        public float YOffset
        {
            get { return m_yOffset; }
        }

        /// <summary>Gets the y offset used to drag the Panel.</summary>
        public bool Locked
        {
            get { return m_locked; }
            set { m_locked = value; }
        }
    }
}
