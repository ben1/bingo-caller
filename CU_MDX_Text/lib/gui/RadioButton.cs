/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : RadioButton.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : RadioButton class

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
    /// <summary>A RadioButton control</summary>
    public class RadioButton : CheckBox
    {
        private int m_groupID;
        private bool m_needToDeselectOthers;

        /// <summary>Creates a RadioButton</summary>
        /// <param name="id">Control ID</param>
        /// <param name="groupID">GroupID to associate with RadioButton</param>
        /// <param name="screenRect">Screen rectangle</param>
        /// <param name="text">CheckBox text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="textAlign">Text alignment</param>
        /// <param name="isSelected">Whether the RadioButton is initially selected.</param>
        /// <param name="font">Initialized CUnit.BitmapFont instance</param>
        /// <param name="node">ControlNode from XML file</param>
        /// <param name="info">Texture ImageInformation</param>
        public RadioButton( int id, int groupID, RectangleF screenRect, string text, float fontSize, ColorValue textColor, TextAlign textAlign, bool isSelected, BitmapFont font, ControlNode node, ImageInformation info )
        {
            m_id = id;
            m_groupID = groupID;
            m_needToDeselectOthers = isSelected;
            m_text = text;
            m_textAlign = textAlign;
            m_fontSize = fontSize;
            m_textColor = textColor;
            m_bFont = font;
            m_data = isSelected;
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
                else if ( i.Name == "RadioMark" )
                {
                    m_marker = new Quad( topLeft, topRight, bottomLeft, bottomRight );
                }
            }
            if ( isSelected )
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
            m_data = true;
            if ( (bool)m_data == true && ( m_normalQuads.Count < 2 ) )
            {
                m_needToDeselectOthers = true;

                // Add radio mark
                m_normalQuads.Add( (Quad)m_marker.Clone() );
                m_overQuads.Add( (Quad)m_marker.Clone() );
                m_downQuads.Add( (Quad)m_marker.Clone() );
                m_disabledQuads.Add( (Quad)m_marker.Clone() );
            }
        }

        /// <summary>Deselects the RadioButton.</summary>
        public void Deselect()
        {
            m_data = false;
            if ( m_normalQuads.Count > 1 )
            {
                m_needToDeselectOthers = false;

                // Remove checkmark
                m_normalQuads.RemoveAt( 1 );
                m_overQuads.RemoveAt( 1 );
                m_downQuads.RemoveAt( 1 );
                m_disabledQuads.RemoveAt( 1 );
            }
        }

        /// <summary>Gets the group id.</summary>
        public int GroupID
        {
            get { return m_groupID; }
        }

        /// <summary>Gets and sets whether the GuiManager needs to 
        /// deselect the other RadioButtons in this groupID.</summary>
        public bool NeedToDelectOthers
        {
            get { return m_needToDeselectOthers; }
            set { m_needToDeselectOthers = value; }
        }

    }
}
