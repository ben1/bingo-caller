/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : FontQuad.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Quad used to render bitmapped fonts

Created :  12/21/2005
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
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit
{
    /// <summary>Quad used to render bitmapped fonts</summary>
    public class FontQuad : Quad
    {
        private int m_lineNumber;
        private int m_wordNumber;
        private float m_sizeScale;
        private BitmapCharacter m_bitmapChar = null;
        private char m_character;
        private float m_wordWidth;

        /// <summary>Creates a new FontQuad</summary>
        /// <param name="topLeft">Top left vertex</param>
        /// <param name="topRight">Top right vertex</param>
        /// <param name="bottomLeft">Bottom left vertex</param>
        /// <param name="bottomRight">Bottom right vertex</param>
        public FontQuad( TransformedColoredTextured topLeft, TransformedColoredTextured topRight,
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

        /// <summary>Gets and sets the line number.</summary>
        public int LineNumber
        {
            get { return m_lineNumber; }
            set { m_lineNumber = value; }
        }

        /// <summary>Gets and sets the word number.</summary>
        public int WordNumber
        {
            get { return m_wordNumber; }
            set { m_wordNumber = value; }
        }

        /// <summary>Gets and sets the word width.</summary>
        public float WordWidth
        {
            get { return m_wordWidth; }
            set { m_wordWidth = value; }
        }

        /// <summary>Gets and sets the BitmapCharacter.</summary>
        public BitmapCharacter BitmapCharacter
        {
            get { return m_bitmapChar; }
            set { m_bitmapChar = (BitmapCharacter)value.Clone(); }
        }

        /// <summary>Gets and sets the character displayed in the quad.</summary>
        public char Character
        {
            get { return m_character; }
            set { m_character = value; }
        }

        /// <summary>Gets and sets the size scale.</summary>
        public float SizeScale
        {
            get { return m_sizeScale; }
            set { m_sizeScale = value; }
        }
    }
}
