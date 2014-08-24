/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Sprite.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Classes to help work with sprites

Created :  11/03/2005
Modified : 11/03/2005

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
    /// <summary>Sprite wrapper</summary>
    public class SpriteBase : WorldTransform
    {
        protected Vector3 m_pivot;

        /// <summary>Default constructor</summary>
        public SpriteBase()
        {
            // Empty
        }

        /// <summary>Draws the sprite using the Sprite.Transform property.</summary>
        /// <param name="sprite">Sprite to draw with.</param>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="srcRect">Rectangle that specifies which portion of the texture to draw.</param>
        public void Draw( Sprite sprite, Texture texture, Rectangle srcRect, Color color )
        {
            sprite.Transform = Transform;
            sprite.Draw( texture, srcRect, m_pivot, Vector3.Empty, color );
        }

        /// <summary>Flips the sprite horizontally.</summary>
        public void FlipHorizontal()
        {
            XScale = -XScale;
        }

        /// <summary>Flips the sprite vertically.</summary>
        public void FlipVertical()
        {
            YScale = -YScale;
        }

        /// <summary>Gets and sets the sprite's pivot point.</summary>
        public Vector3 Pivot
        {
            get { return m_pivot; }
            set { m_pivot = value; }
        }
    }

    /// <summary>An animated sprite.</summary>
    public class SpriteAnimated : SpriteBase
    {
        protected int m_cellHeight;
        protected int m_cellWidth;
        protected int m_currentFrame;
        protected int m_numColumns;
        protected int m_startCell;
        protected int m_endCell;
        protected float m_lastFrameUpdate;
        protected float m_secondsPerFrame;

        /// <summary>Default Constructor</summary>
        public SpriteAnimated()
        {
            // Empty
        }

        /// <summary>Constructor</summary>
        /// <param name="cellHeight">Height of one cell</param>
        /// <param name="cellWidth">Width of one cell</param>
        /// <param name="secondsPerFrame">Time to display a frame</param>
        /// <param name="numColumns">Number of columns in the texture</param>
        /// <param name="startCell">Cell to start initial animation</param>
        /// <param name="endCell">Cell to end initial animation</param>
        public SpriteAnimated( int cellHeight, int cellWidth, float secondsPerFrame, int numColumns, int startCell, int endCell )
        {
            m_cellHeight = cellHeight;
            m_cellWidth = cellWidth;
            m_numColumns = numColumns;
            m_secondsPerFrame = secondsPerFrame;
            m_currentFrame = startCell;
            m_startCell = startCell;
            m_endCell = endCell;
        }

        /// <summary>Advances the frame of animation.</summary>
        /// <param name="elapsedTime">Time elapsed since last frame.</param>
        public void Update( float elapsedTime )
        {
            m_lastFrameUpdate += elapsedTime;
            // Update frame based on time elapsed
            if ( m_lastFrameUpdate >= m_secondsPerFrame )
            {
                // Loop frame if on last frame.
                m_currentFrame = (m_currentFrame == m_endCell) ? m_startCell : m_currentFrame + 1;
                m_lastFrameUpdate -= m_secondsPerFrame;
            }
        }

        /// <summary>Draws the sprite.</summary>
        /// <param name="sprite">Sprite to draw with</param>
        /// <param name="texture">Texture to draw</param>
        public void Draw( Sprite sprite, Texture texture )
        {
            int row = m_currentFrame / m_numColumns;
            int column = m_currentFrame % m_numColumns;
            Rectangle rect = new Rectangle( column * m_cellWidth, row * m_cellHeight, m_cellWidth, m_cellHeight );
            base.Draw( sprite, texture, rect, Color.White );
        }

        /// <summary>Gets and sets the starting animation cell.</summary>
        public int StartCell
        {
            get { return m_startCell; }
            set { m_startCell = value; m_currentFrame = m_startCell; }
        }

        /// <summary>Gets and sets the ending animation cell.</summary>
        public int EndCell
        {
            get { return m_endCell; }
            set { m_endCell = value; }
        }

        /// <summary>Gets and sets the time each frame is displayed.</summary>
        public float SecondsPerFrame
        {
            get { return m_secondsPerFrame; }
            set { m_secondsPerFrame = value; }
        }
    }
}
