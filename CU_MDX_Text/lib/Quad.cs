/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Quad.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : A screen space Quad formed of TransformedColoredTextured vertices.

Created :  12/20/2005
Modified : 12/23/2005

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
using System.Collections.Generic;
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit
{
    public class Quad : ICloneable
    {
        protected TransformedColoredTextured[] m_vertices;

        public Quad()
        {
            // Empty
        }

        /// <summary>Creates a new Quad</summary>
        /// <param name="topLeft">Top left vertex.</param>
        /// <param name="topRight">Top right vertex.</param>
        /// <param name="bottomLeft">Bottom left vertex.</param>
        /// <param name="bottomRight">Bottom right vertex.</param>
        public Quad( TransformedColoredTextured topLeft, TransformedColoredTextured topRight,
            TransformedColoredTextured bottomLeft, TransformedColoredTextured bottomRight )
        {
            m_vertices = new TransformedColoredTextured[6];
            m_vertices[0] = topLeft;
            m_vertices[1] = bottomRight;
            m_vertices[2] = bottomLeft;
            m_vertices[3] = topLeft;
            m_vertices[4] = topRight;
            m_vertices[5] = bottomRight;
        }

        /// <summary>Gets and sets the vertices.</summary>
        public TransformedColoredTextured[] Vertices
        {
            get { return m_vertices; }
            set { value.CopyTo( m_vertices, 0 ); }
        }

        /// <summary>Gets the top left vertex.</summary>
        public TransformedColoredTextured TopLeft
        {
            get { return m_vertices[0]; }
        }

        /// <summary>Gets the top right vertex.</summary>
        public TransformedColoredTextured TopRight
        {
            get { return m_vertices[4]; }
        }

        /// <summary>Gets the bottom left vertex.</summary>
        public TransformedColoredTextured BottomLeft
        {
            get { return m_vertices[2]; }
        }

        /// <summary>Gets the bottom right vertex.</summary>
        public TransformedColoredTextured BottomRight
        {
            get { return m_vertices[5]; }
        }

        /// <summary>Gets and sets the X coordinate.</summary>
        public float X
        {
            get { return m_vertices[0].X; }
            set
            {
                float width = Width;
                m_vertices[0].X = value;
                m_vertices[1].X = value + width;
                m_vertices[2].X = value;
                m_vertices[3].X = value;
                m_vertices[4].X = value + width;
                m_vertices[5].X = value + width;
            }
        }

        /// <summary>Gets and sets the Y coordinate.</summary>
        public float Y
        {
            get { return m_vertices[0].Y; }
            set
            {
                float height = Height;
                m_vertices[0].Y = value;
                m_vertices[1].Y = value + height;
                m_vertices[2].Y = value + height;
                m_vertices[3].Y = value;
                m_vertices[4].Y = value;
                m_vertices[5].Y = value + height;
            }
        }

        /// <summary>Gets and sets the width.</summary>
        public float Width
        {
            get { return m_vertices[4].X - m_vertices[0].X; }
            set
            {
                m_vertices[1].X = m_vertices[0].X + value;
                m_vertices[4].X = m_vertices[0].X + value;
                m_vertices[5].X = m_vertices[0].X + value;
            }
        }

        /// <summary>Gets and sets the height.</summary>
        public float Height
        {
            get { return m_vertices[2].Y - m_vertices[0].Y; }
            set
            {
                m_vertices[1].Y = m_vertices[0].Y + value;
                m_vertices[2].Y = m_vertices[0].Y + value;
                m_vertices[5].Y = m_vertices[0].Y + value;
            }
        }

        /// <summary>Gets the X coordinate of the right.</summary>
        public float Right
        {
            get { return X + Width; }
        }

        /// <summary>Gets the Y coordinate of the bottom.</summary>
        public float Bottom
        {
            get { return Y + Height; }
        }

        /// <summary>Gets and sets the Quad's color.</summary>
        public int Color
        {
            get { return m_vertices[0].ColorValue; }
            set
            {
                for ( int i = 0; i < 6; i++ )
                {
                    m_vertices[i].ColorValue = value;
                }
            }
        }

        /// <summary>Writes the Quad to a string</summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            string result = "X = " + X.ToString();
            result += "\nY = " + Y.ToString();
            result += "\nWidth = " + Width.ToString();
            result += "\nHeight = " + Height.ToString();
            return result;
        }

        /// <summary>Clones the Quad.</summary>
        /// <returns>Cloned Quad</returns>
        public object Clone()
        {
            return new Quad( m_vertices[0], m_vertices[4], m_vertices[2], m_vertices[5] );
        }

    }
}
