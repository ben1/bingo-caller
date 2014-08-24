/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Font.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Microsoft.DirectX.Direct3D.Font wrapper.

Created :  10/25/2005
Modified : 10/25/2005

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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace CUnit
{
	/// <summary>Font wrapper</summary>
	public class Font
	{
        Microsoft.DirectX.Direct3D.Font m_font = null;
        private int m_size;

        public enum Align { Left, Center, Right, TopRight, TopLeft, BottomRight, BottomLeft };

        /// <summary>Default constructor</summary>
		public Font()
		{
            // Empty
		}

        /// <summary>Create a new font.</summary>
        /// <param name="device">Direct3D Device</param>
        /// <param name="faceName">Font face name</param>
        /// <param name="size">Size of the font</param>
        /// <param name="bold">true for bold text, false for normal text</param>
        /// <param name="italic">true for italic text, false for normal text</param>
        public Font( Device device, string faceName, int size, bool bold, bool italic )
        {
            m_size = size;
            m_font = new Microsoft.DirectX.Direct3D.Font( device, -size, 0,
                ( bold ) ? FontWeight.Bold : FontWeight.Normal, 1, italic, 
                CharacterSet.Default, Precision.Default, FontQuality.Default, 
                PitchAndFamily.DefaultPitch | PitchAndFamily.FamilyDoNotCare, faceName );
        }

        /// <summary>Print some 2D text.</summary>
        /// <param name="sprite">Sprite for batch printing</param>
        /// <param name="text">Text to print</param>
        /// <param name="xPosition">X position in window coordinates</param>
        /// <param name="yPosition">Y position in window coordinates</param>
        /// <param name="textBoxWidth">Width to constrain text in</param>
        /// <param name="textBoxHeight">Height to constrain text in</param>
        /// <param name="color">Color of the text</param>
        /// <param name="alignment">CUnit.Font.Alignment enum</param>
        public void Print( Sprite sprite, string text, int xPosition, int yPosition,
            int textBoxWidth, int textBoxHeight, int color, Align alignment )
        {
            if ( m_font == null )
            {
                return;
            }
            DrawStringFormat format = 0;
            if ( textBoxWidth == 0 )
            {
                format |= DrawStringFormat.NoClip;
            }
            else
            {
                format |= DrawStringFormat.WordBreak;
                switch ( alignment )
                {
                    case Align.Left:
                        format |= DrawStringFormat.Left;
                        break;
                    case Align.Center:
                        format |= DrawStringFormat.Center;
                        break;
                    case Align.Right:
                        format |= DrawStringFormat.Right;
                        break;
                    case Align.TopRight:
                        format |= DrawStringFormat.Right | DrawStringFormat.Top;
                        break;
                    case Align.BottomRight:
                        format |= DrawStringFormat.Right | DrawStringFormat.Bottom;
                        break;
                    case Align.TopLeft:
                        format |= DrawStringFormat.Left | DrawStringFormat.Top;
                        break;
                    case Align.BottomLeft:
                        format |= DrawStringFormat.Left | DrawStringFormat.Bottom;
                        break;
                }
                if ( textBoxHeight == 0 )
                {
                    // A width is specified, but not a height.
                    // Make it seem like height is infinite
                    textBoxHeight = 2000;
                }
            }
            format |= DrawStringFormat.ExpandTabs;
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle( xPosition, yPosition, textBoxWidth, textBoxHeight );
            m_font.DrawString( sprite, text, rect, (DrawStringFormat)format, color );
        }

        /// <summary>Print some 2D text.</summary>
        /// <param name="text">Text to print</param>
        /// <param name="xPosition">X position in window coordinates</param>
        /// <param name="yPosition">Y position in window coordinates</param>
        /// <param name="color">Color of the text</param>
        public void Print( string text, int xPosition, int yPosition, int color )
        {
            Print( null, text, xPosition, yPosition, 0, 0, color, Align.Left );
        }

        /// <summary>Print some 2D text.</summary>
        /// <param name="sprite">Sprite for batch printing</param>
        /// <param name="text">Text to print</param>
        /// <param name="xPosition">X position in window coordinates</param>
        /// <param name="yPosition">Y position in window coordinates</param>
        /// <param name="color">Color of the text</param>
        public void Print( Sprite sprite, string text, int xPosition, int yPosition, int color )
        {
            Print( sprite, text, xPosition, yPosition, 0, 0, color, Align.Left );
        }

        /// <summary>Call after the device is reset.</summary>
        public void OnResetDevice()
        {
            if ( m_font != null )
            {
                m_font.OnResetDevice();
            }
        }

        /// <summary>Call when the device is lost</summary>
        public void OnLostDevice()
        {
            if ( m_font != null && !m_font.IsDisposed )
            {
                m_font.OnLostDevice();
            }
        }

        /// <summary>Call when the device is destrroyed</summary>
        public void OnDestroyDevice()
        {
            if ( m_font != null )
            {
                m_font.Dispose();
                m_font = null;
            }
        }

        /// <summary>Gets the font size.</summary>
        public int Size
        {
            get { return m_size; }
        }
    }
}
