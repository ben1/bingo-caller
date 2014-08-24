/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : ComboBox.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Text Label class

Created :  12/29/2005
Modified : 12/29/2005

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
using System.Drawing;
using Microsoft.DirectX;

namespace CUnit.Gui
{
    class Label : Control
    {
        private BitmapFont.Align m_alignment;
        public Label( int id, RectangleF screenRect, string text, float fontSize, ColorValue textColor, BitmapFont font, BitmapFont.Align alignment )
        {
            m_id = id;
            m_text = text;
            m_fontSize = fontSize;
            m_textColor = textColor;
            m_bFont = font;
            m_position = new PointF( screenRect.X, screenRect.Y );
            m_size = new SizeF( screenRect.Width, screenRect.Height );
            m_alignment = alignment;
            m_hotspot = new Rectangle( (int)screenRect.X, (int)screenRect.Y, (int)screenRect.Width, (int)screenRect.Height );
            BuildText();
        }

        /// <summary>Builds the text</summary>
        protected override void BuildText()
        {
            int index = m_bFont.AddString( m_text, new RectangleF( m_position.X, m_position.Y,
                m_size.Width, m_size.Height ), m_alignment, m_fontSize, m_textColor, true );
            List<FontQuad> fontQuads = m_bFont.GetProcessedQuads( index );
            m_bFont.ClearString( index );

            // Convert FontQuads to Quads
            m_fontQuads = new List<Quad>( fontQuads.Count );
            for ( int i = 0; i < fontQuads.Count; i++ )
            {
                m_fontQuads.Add( new Quad( fontQuads[i].TopLeft, fontQuads[i].TopRight, fontQuads[i].BottomLeft, fontQuads[i].BottomRight ) );
            }
        }

        /// <summary>Gets and sets the Control's position</summary>
        public override PointF Position
        {
            get { return base.Position; }
            set
            {
                float xOffset = value.X - m_position.X;
                float yOffset = value.Y - m_position.Y;
                m_position = value;
                for ( int i = 0; i < m_fontQuads.Count; i++ )
                {
                    m_fontQuads[i].X += xOffset;
                    m_fontQuads[i].Y += yOffset;
                }
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                m_text = value;
                BuildText();
            }
        }
    }
}
