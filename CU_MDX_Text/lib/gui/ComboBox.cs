/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : ComboBox.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : ComboBox class

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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit.Gui
{
    public class ComboBox : ListBox
    {
        protected bool m_isOpen;
        protected int m_selectedItem;
        protected float m_openHeight;
        // Open quads will be m_quads since it's already implemented in ListBox
        protected List<Quad> m_normalQuads;
        protected List<Quad> m_overQuads;
        protected List<Quad> m_disabledClosedQuads;

        protected enum Box
        {
            Left, Right, Top, Bottom, Background, TopLeft, TopRight,
            BottomLeft, BottomRight, OpenArrow
        };

        protected new enum Section 
        {
            Left, Right, Top, Bottom, Background, TopLeft, TopRight, 
            BottomLeft, BottomRight, ScrollUp, ScrollDown, ScrollBody,
            ScrollMarker, OpenArrow
        };
        protected new enum HotSpot { Background, ScrollUp, ScrollDown, ScrollMarker, ScrollBody, Box };

        /// <summary>Creates a ListBox</summary>
        /// <param name="id">Control ID</param>
        /// <param name="screenRect">Screen rectangle</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="font">Bitmap Font</param>
        /// <param name="node">ControlNode from XML file</param>
        /// <param name="info">Texture ImageInformation</param>
        public ComboBox( int id, RectangleF screenRect, float openHeight, float fontSize, ColorValue textColor, BitmapFont font, ControlNode node, ImageInformation info )
        {
            m_singleItemListBox = true;
            m_isOpen = false;
            m_topItem = 0;
            m_id = id;
            m_fontSize = fontSize;
            m_itemHeight = 0f;
            m_textColor = textColor;
            m_bFont = font;
            m_scrolling = false;
            m_overState = false;
            m_singleItemListBox = true;
            // Fill m_text up with dummy text so the FontQuads are picked up by GuiManager
            m_text = "x";
            m_selectedItem = -1;
            m_position = new PointF( screenRect.X, screenRect.Y );
            m_size = new SizeF( screenRect.Width, screenRect.Height );
            m_openHeight = openHeight;
            if ( m_openHeight <= m_size.Height )
            {
                throw new Exception( "ComboBox open height must be greater than closed height." );
            }
            m_data = new ArrayList();

            int index = m_bFont.AddString( "x", new RectangleF( 0f, 0f, 0f, 0f ), BitmapFont.Align.Left, m_fontSize, m_textColor, true );
            m_itemHeight = m_bFont.GetLineHeight( index );
            m_bFont.ClearString( index );

            m_scrollUp = new List<Quad>();
            m_scrollDown = new List<Quad>();
            m_scrollMarker = new List<Quad>();
            m_quads = new List<Quad>();
            m_disabledQuads = new List<Quad>();
            m_normalHighlights = new List<Quad>();
            m_disabledHighlights = new List<Quad>();
            m_highlightQuads = new List<Quad>();
            m_items = new List<ListableItem>();
            m_hotspots = new List<Rectangle>();
            m_normalQuads = new List<Quad>();
            m_overQuads = new List<Quad>();
            m_disabledClosedQuads = new List<Quad>();

            // Initialize Lists so we can access them with indices
            m_highlightQuads.Add( new Quad() );
            m_highlightQuads.Add( new Quad() );
            for ( int i = 0; i < 6; i++ )
            {
                m_hotspots.Add( new Rectangle() );
            }
            for ( int i = 0; i < 4; i++ )
            {
                m_scrollUp.Add( new Quad() );
                m_scrollDown.Add( new Quad() );
                m_scrollMarker.Add( new Quad() );
            }
            for ( int i = 0; i < 13; i++ )
            {
                m_quads.Add( new Quad() );
            }
            // m_disabledQuads aren't used in ComboBox but are used in ListBox, so
            // initialize the List so ListBox will still work
            TransformedColoredTextured t = new TransformedColoredTextured( 0f, 0f, 0f, 0f, 0, 0f, 0f );
            for ( int i = 0; i < 13; i++ )
            {
                m_disabledQuads.Add( new Quad( t, t, t, t ) );
            }
            for ( int i = 0; i < 10; i++ )
            {
                m_normalQuads.Add( new Quad() );
                m_overQuads.Add( new Quad() );
                m_disabledClosedQuads.Add( new Quad() );
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
                        m_normalQuads[(int)Box.TopLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.TopLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.TopLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.TopLeft] = q;
                    }
                }
                else if ( i.Name.EndsWith( "TopBorder" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.Top] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.Top] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.Top] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.Top] = q;
                    }
                }
                else if ( i.Name.EndsWith( "TopRightCorner" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.TopRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.TopRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.TopRight] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.TopRight] = q;
                    }
                }
                else if ( i.Name.EndsWith( "LeftBorder" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.Left] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.Left] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.Left] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.Left] = q;
                    }
                }
                else if ( i.Name.EndsWith( "Background" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.Background] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.Background] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.Background] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.Background] = q;
                    }
                }
                else if ( i.Name.EndsWith( "RightBorder" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.Right] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.Right] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.Right] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.Right] = q;
                    }
                }
                else if ( i.Name.EndsWith( "BottomLeftCorner" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.BottomLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.BottomLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.BottomLeft] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.BottomLeft] = q;
                    }
                }
                else if ( i.Name.EndsWith( "BottomBorder" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.Bottom] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.Bottom] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.Bottom] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.Bottom] = q;
                    }
                }
                else if ( i.Name.EndsWith( "BottomRightCorner" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.BottomRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.BottomRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.BottomRight] = q;
                    }
                    else if ( i.Name.StartsWith( "List" ) )
                    {
                        m_quads[(int)Section.BottomRight] = q;
                    }
                }
                else if ( i.Name.EndsWith( "OpenArrow" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_normalQuads[(int)Box.OpenArrow] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_overQuads[(int)Box.OpenArrow] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledClosedQuads[(int)Box.OpenArrow] = q;
                    }
                }
                else if ( i.Name == "ListScrollBody" )
                {
                    m_quads[(int)Section.ScrollBody] = q;
                }
                else if ( i.Name == "ListHighlight" )
                {
                    m_highlightQuads[(int)Highlight.Normal] = q;
                    m_highlightColor = i.Color.ToArgb();
                    // Dummy Disabled highlight for ListBox implementation
                    m_highlightQuads[(int)Highlight.Disabled] = (Quad)q.Clone();
                }
                else if ( i.Name.EndsWith( "ScrollUp" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.ScrollUp] = q;
                        m_scrollUp[(int)ControlState.Normal] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_scrollUp[(int)ControlState.Over] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_scrollUp[(int)ControlState.Down] = q;
                    }
                    // Junk for inherited ListBox
                    m_scrollUp[(int)ControlState.Disabled] = (Quad)q.Clone();
                }
                else if ( i.Name.EndsWith( "ScrollDown" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.ScrollDown] = q;
                        m_scrollDown[(int)ControlState.Normal] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_scrollDown[(int)ControlState.Over] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_scrollDown[(int)ControlState.Down] = q;
                    }
                    // Junk for inherited ListBox
                    m_scrollDown[(int)ControlState.Disabled] = (Quad)q.Clone();
                }
                else if ( i.Name.EndsWith( "ScrollMarker" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.ScrollMarker] = q;
                        m_scrollMarker[(int)ControlState.Normal] = q;
                    }
                    else if ( i.Name.StartsWith( "Over" ) )
                    {
                        m_scrollMarker[(int)ControlState.Over] = q;
                    }
                    else if ( i.Name.StartsWith( "Down" ) )
                    {
                        m_scrollMarker[(int)ControlState.Down] = q;
                    }
                    // Junk for inherited ListBox
                    m_scrollMarker[(int)ControlState.Disabled] = (Quad)q.Clone();
                }
            }

            PositionQuads();
        }

        /// <summary>Checks is the mouse is over the Control's hotspot</summary>
        /// <param name="cursor">Mouse position</param>
        /// <returns>true if the cursor is over the Control's hotspot, false otherwise.</returns>
        public override bool Contains( Point cursor )
        {
            if ( m_isOpen )
            {
                foreach ( Rectangle r in m_hotspots )
                {
                    if ( r.Contains( cursor ) )
                    {
                        return true;
                    }
                }
                if ( m_overState )
                {
                    m_overState = false;
                    m_quads[(int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Normal];
                    m_quads[(int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Normal];
                    m_quads[(int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Normal];
                }
            }
            else if ( m_hotspots[(int)HotSpot.Box].Contains( cursor ) )
            {
                return true;
            }
            return false;
        }

        /// <summary>Mouse Over event</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        protected override void OnMouseOver( Point cursor, bool[] buttons )
        {
            if ( m_isOpen )
            {
                // Create rollover effect
                if ( m_hotspots[(int)HotSpot.ScrollUp].Contains( cursor ) )
                {
                    m_overState = true;
                    m_overQuads[10 + (int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Over];
                }
                else
                {
                    m_overQuads[10 + (int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Normal];
                }
                if ( m_hotspots[(int)HotSpot.ScrollDown].Contains( cursor ) )
                {
                    m_overState = true;
                    m_overQuads[10 + (int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Over];
                }
                else
                {
                    m_overQuads[10 + (int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Normal];
                }
                if ( m_hotspots[(int)HotSpot.ScrollMarker].Contains( cursor ) )
                {
                    m_overState = true;
                    m_overQuads[10 + (int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Over];
                }
                else
                {
                    m_overQuads[10 + (int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Normal];
                }

                // Move highlight along with mouse
                int highlightedItem = ( cursor.Y - (int)m_quads[(int)Section.Background].Y ) / (int)m_itemHeight;
                if ( m_hotspots[(int)HotSpot.Background].Contains( cursor ) )
                {
                    if ( ( highlightedItem < m_numDisplayedItems ) && ( highlightedItem + m_topItem < m_items.Count ) )
                    {
                        int highlight = 0;
                        if ( m_selectedItem >= 0 )
                        {
                            highlight = m_selectedItem;
                        }
                        highlightedItem += m_topItem;
                        float newY = ( highlightedItem - m_topItem ) * m_itemHeight + (int)m_quads[(int)Section.Background].Y;
                        m_normalHighlights[highlight].Y = newY;
                        m_normalHighlights[highlight].Color  = m_highlightColor;
                    }
                }
            }
        }

        /// <summary>Mouse Down event</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        protected override void OnMouseDown( Point cursor, bool[] buttons )
        {
            if ( m_isOpen )
            {
                base.OnMouseDown( cursor, buttons );

                // Create pushdown effect
                if ( m_hotspots[(int)HotSpot.ScrollUp].Contains( cursor ) && !m_hasFocus )
                {
                    m_overQuads[10 + (int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Down];
                }
                else if ( !m_hasFocus )
                {
                    m_overQuads[10 + (int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Normal];
                }
                if ( m_hotspots[(int)HotSpot.ScrollDown].Contains( cursor ) && !m_hasFocus )
                {
                    m_overQuads[10 + (int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Down];
                }
                else if ( !m_hasFocus )
                {
                    m_overQuads[10 + (int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Normal];
                }
                if ( m_hotspots[(int)HotSpot.ScrollMarker].Contains( cursor ) && !m_hasFocus )
                {
                    m_overQuads[10 + (int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Down];
                }
                else if ( !m_hasFocus )
                {
                    m_overQuads[10 + (int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Normal];
                }
            }
        }

        /// <summary>Mouse Release event</summary>
        /// <param name="cursor">Mouse position</param>
        protected override void OnMouseRelease( System.Drawing.Point cursor )
        {
            m_newData = false;
            if ( m_isOpen )
            {
                if ( m_hotspots[(int)HotSpot.ScrollDown].Contains( cursor ) )
                {
                    ScrollDown();
                }
                else if ( m_hotspots[(int)HotSpot.ScrollUp].Contains( cursor ) )
                {
                    ScrollUp();
                }
                else if ( m_hotspots[(int)HotSpot.Background].Contains( cursor ) )
                {
                    int highlightedItem = ( cursor.Y - (int)m_quads[(int)Section.Background].Y ) / (int)m_itemHeight;
                    if ( ( highlightedItem < m_numDisplayedItems ) && ( highlightedItem + m_topItem < m_items.Count ) )
                    {
                        highlightedItem += m_topItem;

                        // Check for new data
                        m_newData = true;
                        foreach ( object o in (ArrayList)m_data )
                        {
                            if ( m_items[highlightedItem].Data == o )
                            {
                                m_newData = false;
                                break;
                            }
                        }

                        m_items[highlightedItem].Selected = true;
                        m_selectedItem = highlightedItem;
                        // Turn alpha on
                        m_normalHighlights[highlightedItem].Color = m_highlightColor;
                        m_disabledHighlights[highlightedItem].Color = m_disabledHighlightColor;

                        // Deselect other items if a single item list box
                        ( (ArrayList)m_data ).Clear();
                        for ( int i = 0; i < m_items.Count; i++ )
                        {
                            if ( i != highlightedItem )
                            {
                                m_items[i].Selected = false;
                                m_normalHighlights[i].Color = 0;
                                m_disabledHighlights[i].Color = 0;
                            }
                        }
                        ( (ArrayList)m_data ).Add( m_items[highlightedItem].Data );
                        m_text = m_items[highlightedItem].Text;
                        float newY = ( highlightedItem - m_topItem ) * m_itemHeight + (int)m_quads[(int)Section.Background].Y;
                        m_normalHighlights[highlightedItem].Y = newY;
                        m_disabledHighlights[highlightedItem].Y = newY;
                        m_normalHighlights[highlightedItem].Color = m_highlightColor;
                        m_disabledHighlights[highlightedItem].Color = m_disabledHighlightColor;

                        if ( !m_scrolling )
                        {
                            m_isOpen = false;
                            m_overQuads.RemoveRange( 10, m_quads.Count );
                            BuildText();
                        }
                        
                    }
                }
                else if ( m_hotspots[(int)HotSpot.Box].Contains( cursor ) && !m_scrolling || !Contains( cursor ) )
                {
                    m_isOpen = false;
                    m_overQuads.RemoveRange( 10, m_quads.Count );
                    BuildText();
                }
            }
            else if ( m_hotspots[(int)HotSpot.Box].Contains( cursor ) )
            {
                m_isOpen = true;
                if ( m_selectedItem >= 0 )
                {
                    // Highlight may be active from mouseover
                    m_normalHighlights[0].Color = 0;

                    // Turn on selected highlight and move it to position
                    m_normalHighlights[m_selectedItem].Color = m_highlightColor;
                    m_topItem = Math.Min( m_selectedItem, m_items.Count - m_numDisplayedItems );
                    float newY = ( m_selectedItem - m_topItem ) * m_itemHeight + (int)m_quads[(int)Section.Background].Y;
                    m_normalHighlights[m_selectedItem].Y = newY;

                    // Move Scroll Marker
                    int divisionHeight = (int)m_quads[(int)Section.ScrollBody].Height / ( m_items.Count - m_numDisplayedItems + 1 );
                    for ( int i = 0; i < 4; i++ )
                    {
                        m_scrollMarker[i].Y = m_quads[(int)Section.ScrollBody].Y + (float)m_topItem * (float)divisionHeight;
                    }
                }

                m_overQuads.AddRange( m_quads );
                BuildText();

                for ( int i = 0; i < 4; i++ )
                {
                    if ( m_numDisplayedItems == m_items.Count )
                    {
                        m_scrollMarker[i].Height = 0f;
                    }
                    else
                    {
                        m_scrollMarker[i].Height = m_quads[(int)Section.ScrollBody].Height / ( (float)m_items.Count - (float)m_numDisplayedItems + 1 );
                        m_scrollMarker[i].Height = Math.Max( m_scrollMarker[i].Height, m_minMarkerHeight );
                    }
                }
            }
            BuildHotspots();

            m_scrolling = false;
        }

        /// <summary>Gets the Contol's current Quads</summary>
        public override List<Quad> Quads
        {
            get
            {
                switch ( m_state )
                {
                    case ControlState.Disabled:
                        return m_disabledClosedQuads;
                    case ControlState.Over:
                        return m_overQuads;
                    case ControlState.Down:
                        return m_overQuads;
                    default:
                        if ( m_isOpen )
                        {
                            return m_overQuads;
                        }
                        return m_normalQuads;
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
                for ( int i = 0; i < m_normalQuads.Count; i++ )
                {
                    m_normalQuads[i].X += xOffset;
                    m_normalQuads[i].Y += yOffset;
                    m_overQuads[i].X += xOffset;
                    m_overQuads[i].Y += yOffset;
                    m_disabledClosedQuads[i].X += xOffset;
                    m_disabledClosedQuads[i].Y += yOffset;
                }
                for ( int i = 0; i < m_quads.Count; i++ )
                {
                    if ( i == (int)Section.ScrollUp || i == (int)Section.ScrollDown || i == (int)Section.ScrollMarker )
                    {
                        continue;
                    }
                    m_quads[i].X += xOffset;
                    m_quads[i].Y += yOffset;
                }
                for ( int i = 0; i < 3; i++ )
                {
                    m_scrollUp[i].X += xOffset;
                    m_scrollUp[i].Y += yOffset;
                    m_scrollDown[i].X += xOffset;
                    m_scrollDown[i].Y += yOffset;
                    m_scrollMarker[i].X += xOffset;
                    m_scrollMarker[i].Y += yOffset;
                }

                BuildHotspots();

                for ( int i = 0; i < m_fontQuads.Count; i++ )
                {
                    m_fontQuads[i].X += xOffset;
                    m_fontQuads[i].Y += yOffset;
                }
            }
        }

        /// <summary>Positions the Quads</summary>
        protected override void PositionQuads()
        {
            // Adjust box middle column X
            m_normalQuads[(int)Box.Top].X = m_normalQuads[(int)Box.TopLeft].Right;
            m_normalQuads[(int)Box.Background].X = m_normalQuads[(int)Box.Left].Right;
            m_normalQuads[(int)Box.Bottom].X = m_normalQuads[(int)Box.BottomLeft].Right;
            m_overQuads[(int)Box.Top].X = m_overQuads[(int)Box.TopLeft].Right;
            m_overQuads[(int)Box.Background].X = m_overQuads[(int)Box.Left].Right;
            m_overQuads[(int)Box.Bottom].X = m_overQuads[(int)Box.BottomLeft].Right;
            m_disabledClosedQuads[(int)Box.Top].X = m_disabledClosedQuads[(int)Box.TopLeft].Right;
            m_disabledClosedQuads[(int)Box.Background].X = m_disabledClosedQuads[(int)Box.Left].Right;
            m_disabledClosedQuads[(int)Box.Bottom].X = m_disabledClosedQuads[(int)Box.BottomLeft].Right;

            // Adjust box middle row Y
            m_normalQuads[(int)Box.Left].Y = m_normalQuads[(int)Box.TopLeft].Bottom;
            m_normalQuads[(int)Box.Background].Y = m_normalQuads[(int)Box.Top].Bottom;
            m_normalQuads[(int)Box.Right].Y = m_normalQuads[(int)Box.TopRight].Bottom;
            m_overQuads[(int)Box.Left].Y = m_overQuads[(int)Box.TopLeft].Bottom;
            m_overQuads[(int)Box.Background].Y = m_overQuads[(int)Box.Top].Bottom;
            m_overQuads[(int)Box.Right].Y = m_overQuads[(int)Box.TopRight].Bottom;
            m_disabledClosedQuads[(int)Box.Left].Y = m_disabledClosedQuads[(int)Box.TopLeft].Bottom;
            m_disabledClosedQuads[(int)Box.Background].Y = m_disabledClosedQuads[(int)Box.Top].Bottom;
            m_disabledClosedQuads[(int)Box.Right].Y = m_disabledClosedQuads[(int)Box.TopRight].Bottom;

            // Adjust box middle row height
            m_normalQuads[(int)Box.Left].Height =
                m_size.Height - m_normalQuads[(int)Box.TopLeft].Height -
                m_normalQuads[(int)Box.BottomLeft].Height;
            m_normalQuads[(int)Box.Background].Height =
                m_size.Height - m_normalQuads[(int)Box.Top].Height -
                m_normalQuads[(int)Box.Bottom].Height;
            m_normalQuads[(int)Box.Right].Height =
                m_size.Height - m_quads[(int)Section.TopRight].Height -
                m_normalQuads[(int)Box.BottomRight].Height;

            m_overQuads[(int)Box.Left].Height =
                m_size.Height - m_overQuads[(int)Box.TopLeft].Height -
                m_overQuads[(int)Box.BottomLeft].Height;
            m_overQuads[(int)Box.Background].Height =
                m_size.Height - m_overQuads[(int)Box.Top].Height -
                m_overQuads[(int)Box.Bottom].Height;
            m_overQuads[(int)Box.Right].Height =
                m_size.Height - m_quads[(int)Section.TopRight].Height -
                m_overQuads[(int)Box.BottomRight].Height;

            m_disabledClosedQuads[(int)Box.Left].Height =
                m_size.Height - m_disabledClosedQuads[(int)Box.TopLeft].Height -
                m_disabledClosedQuads[(int)Box.BottomLeft].Height;
            m_disabledClosedQuads[(int)Box.Background].Height =
                m_size.Height - m_disabledClosedQuads[(int)Box.Top].Height -
                m_disabledClosedQuads[(int)Box.Bottom].Height;
            m_disabledClosedQuads[(int)Box.Right].Height =
                m_size.Height - m_quads[(int)Section.TopRight].Height -
                m_disabledClosedQuads[(int)Box.BottomRight].Height;

            // Adjust box bottom row Y
            m_normalQuads[(int)Box.BottomLeft].Y = m_normalQuads[(int)Box.Left].Bottom;
            m_normalQuads[(int)Box.Bottom].Y = m_normalQuads[(int)Box.Background].Bottom;
            m_normalQuads[(int)Box.BottomRight].Y = m_normalQuads[(int)Box.Right].Bottom;
            m_overQuads[(int)Box.BottomLeft].Y = m_overQuads[(int)Box.Left].Bottom;
            m_overQuads[(int)Box.Bottom].Y = m_overQuads[(int)Box.Background].Bottom;
            m_overQuads[(int)Box.BottomRight].Y = m_overQuads[(int)Box.Right].Bottom;
            m_disabledClosedQuads[(int)Box.BottomLeft].Y = m_disabledClosedQuads[(int)Box.Left].Bottom;
            m_disabledClosedQuads[(int)Box.Bottom].Y = m_disabledClosedQuads[(int)Box.Background].Bottom;
            m_disabledClosedQuads[(int)Box.BottomRight].Y = m_disabledClosedQuads[(int)Box.Right].Bottom;

            // Adjust open arrow dimensions
            m_normalQuads[(int)Box.OpenArrow].Height = m_normalQuads[(int)Box.Bottom].Bottom - m_normalQuads[(int)Box.Top].Y;
            m_overQuads[(int)Box.OpenArrow].Height = m_overQuads[(int)Box.Bottom].Bottom - m_overQuads[(int)Box.Top].Y;
            m_disabledClosedQuads[(int)Box.OpenArrow].Height = m_disabledClosedQuads[(int)Box.Bottom].Bottom - m_disabledClosedQuads[(int)Box.Top].Y;

            m_normalQuads[(int)Box.OpenArrow].Width = m_normalQuads[(int)Box.OpenArrow].Height;
            m_overQuads[(int)Box.OpenArrow].Width = m_overQuads[(int)Box.OpenArrow].Height;
            m_disabledClosedQuads[(int)Box.OpenArrow].Width = m_disabledClosedQuads[(int)Box.OpenArrow].Height;

            // Adjust box middle column width
            m_normalQuads[(int)Box.Top].Width =
                m_size.Width - m_normalQuads[(int)Box.TopLeft].Width -
                m_normalQuads[(int)Box.TopRight].Width - m_normalQuads[(int)Box.OpenArrow].Width;
            m_normalQuads[(int)Box.Background].Width =
                m_size.Width - m_normalQuads[(int)Box.Left].Width -
                m_normalQuads[(int)Box.Right].Width - m_normalQuads[(int)Box.OpenArrow].Width;
            m_normalQuads[(int)Box.Bottom].Width =
                m_size.Width - m_quads[(int)Section.BottomLeft].Width -
                m_normalQuads[(int)Box.BottomRight].Width - m_normalQuads[(int)Box.OpenArrow].Width;

            m_overQuads[(int)Box.Top].Width =
                m_size.Width - m_overQuads[(int)Box.TopLeft].Width -
                m_overQuads[(int)Box.TopRight].Width - m_overQuads[(int)Box.OpenArrow].Width;
            m_overQuads[(int)Box.Background].Width =
                m_size.Width - m_overQuads[(int)Box.Left].Width -
                m_overQuads[(int)Box.Right].Width - m_overQuads[(int)Box.OpenArrow].Width;
            m_overQuads[(int)Box.Bottom].Width =
                m_size.Width - m_quads[(int)Section.BottomLeft].Width -
                m_overQuads[(int)Box.BottomRight].Width - m_overQuads[(int)Box.OpenArrow].Width;

            m_disabledClosedQuads[(int)Box.Top].Width =
                m_size.Width - m_disabledClosedQuads[(int)Box.TopLeft].Width -
                m_disabledClosedQuads[(int)Box.TopRight].Width - m_disabledClosedQuads[(int)Box.OpenArrow].Width;
            m_disabledClosedQuads[(int)Box.Background].Width =
                m_size.Width - m_disabledClosedQuads[(int)Box.Left].Width -
                m_disabledClosedQuads[(int)Box.Right].Width - m_disabledClosedQuads[(int)Box.OpenArrow].Width;
            m_disabledClosedQuads[(int)Box.Bottom].Width =
                m_size.Width - m_quads[(int)Section.BottomLeft].Width -
                m_disabledClosedQuads[(int)Box.BottomRight].Width - m_disabledClosedQuads[(int)Box.OpenArrow].Width;

            // Adjust box right column x
            m_normalQuads[(int)Box.TopRight].X = m_normalQuads[(int)Box.Top].Right;
            m_normalQuads[(int)Box.Right].X = m_normalQuads[(int)Box.Background].Right;
            m_normalQuads[(int)Box.BottomRight].X = m_normalQuads[(int)Box.Bottom].Right;
            m_overQuads[(int)Box.TopRight].X = m_overQuads[(int)Box.Top].Right;
            m_overQuads[(int)Box.Right].X = m_overQuads[(int)Box.Background].Right;
            m_overQuads[(int)Box.BottomRight].X = m_overQuads[(int)Box.Bottom].Right;
            m_disabledClosedQuads[(int)Box.TopRight].X = m_disabledClosedQuads[(int)Box.Top].Right;
            m_disabledClosedQuads[(int)Box.Right].X = m_disabledClosedQuads[(int)Box.Background].Right;
            m_disabledClosedQuads[(int)Box.BottomRight].X = m_disabledClosedQuads[(int)Box.Bottom].Right;

            // Adjust open arrow
            m_normalQuads[(int)Box.OpenArrow].X = m_normalQuads[(int)Box.TopRight].Right;
            m_overQuads[(int)Box.OpenArrow].X = m_overQuads[(int)Box.TopRight].Right;
            m_disabledClosedQuads[(int)Box.OpenArrow].X = m_disabledClosedQuads[(int)Box.TopRight].Right;

            m_quads[(int)Section.TopLeft].Y = m_overQuads[(int)Box.BottomLeft].Bottom;
            m_quads[(int)Section.Top].Y = m_overQuads[(int)Box.Bottom].Bottom;
            m_quads[(int)Section.TopRight].Y = m_overQuads[(int)Box.BottomRight].Bottom;
            for ( int i = 0; i < 4; i++ )
            {
                m_scrollUp[i].Y = m_overQuads[(int)Box.OpenArrow].Bottom;
            }

            float oldHeight = m_size.Height;
            m_size.Height = m_openHeight - m_size.Height;
            base.PositionQuads();
            m_size.Height = oldHeight;

        }

        /// <summary>Builds the hotspots</summary>
        protected override void BuildHotspots()
        {
            m_hotspots[(int)HotSpot.Background] = new Rectangle(
                (int)m_quads[(int)Section.Background].X,
                (int)m_quads[(int)Section.Background].Y,
                (int)m_quads[(int)Section.Background].Width,
                (int)m_quads[(int)Section.Background].Height );

            m_hotspots[(int)HotSpot.ScrollBody] = new Rectangle(
                (int)m_quads[(int)Section.ScrollBody].X,
                (int)m_quads[(int)Section.ScrollBody].Y,
                (int)m_quads[(int)Section.ScrollBody].Width,
                (int)m_quads[(int)Section.ScrollBody].Height );

            m_hotspots[(int)HotSpot.ScrollUp] = new Rectangle(
                (int)m_quads[(int)Section.ScrollUp].X,
                (int)m_quads[(int)Section.ScrollUp].Y,
                (int)m_quads[(int)Section.ScrollUp].Width,
                (int)m_quads[(int)Section.ScrollUp].Height );

            m_hotspots[(int)HotSpot.ScrollDown] = new Rectangle(
                (int)m_quads[(int)Section.ScrollDown].X,
                (int)m_quads[(int)Section.ScrollDown].Y,
                (int)m_quads[(int)Section.ScrollDown].Width,
                (int)m_quads[(int)Section.ScrollDown].Height );

            m_hotspots[(int)HotSpot.ScrollMarker] = new Rectangle(
                (int)m_quads[(int)Section.ScrollMarker].X,
                (int)m_quads[(int)Section.ScrollMarker].Y,
                (int)m_quads[(int)Section.ScrollMarker].Width,
                (int)m_quads[(int)Section.ScrollMarker].Height );

            m_hotspots[(int)HotSpot.Box] = new Rectangle(
                (int)m_normalQuads[(int)Box.TopLeft].X,
                (int)m_normalQuads[(int)Box.TopLeft].Y,
                (int)m_normalQuads[(int)Box.OpenArrow].Right - (int)m_normalQuads[(int)Box.Left].X,
                (int)m_normalQuads[(int)Box.Bottom].Bottom - (int)m_normalQuads[(int)Box.Top].Y );
        }

        /// <summary>Builds the text</summary>
        protected override void BuildText()
        {
            m_fontQuads = new List<Quad>();

            // Add selected item text
            if ( m_selectedItem >= 0 )
            {
                float y = m_normalQuads[(int)Box.Background].Y +
                    ( m_normalQuads[(int)Box.Background].Height / 2f ) - ( m_itemHeight / 2f );
                int index = m_bFont.AddString( m_items[m_selectedItem].Text, new RectangleF(
                        m_normalQuads[(int)Box.Background].X + 2f, y,
                        m_quads[(int)Section.Background].Width, m_quads[(int)Section.Background].Height ),
                        BitmapFont.Align.Left, m_fontSize, m_textColor, true );
                List<FontQuad> selectedFontQuads = m_bFont.GetProcessedQuads( index );
                m_bFont.ClearString( index );

                // Convert FontQuads to Quads
                for ( int j = 0; j < selectedFontQuads.Count; j++ )
                {
                    m_fontQuads.Add( new Quad( selectedFontQuads[j].TopLeft, selectedFontQuads[j].TopRight, selectedFontQuads[j].BottomLeft, selectedFontQuads[j].BottomRight ) );
                }
            }

            if ( m_isOpen )
            {
                string text = "";
                for ( int i = m_topItem; i < m_items.Count; i++ )
                {
                    text += m_items[i].Text + "\n";
                }
                int index = m_bFont.AddString( text, new RectangleF(
                        m_quads[(int)Section.Background].X + 2f, m_quads[(int)Section.Background].Y,
                        m_quads[(int)Section.Background].Width, m_quads[(int)Section.Background].Height ),
                        BitmapFont.Align.Left, m_fontSize, m_textColor, true );
                List<FontQuad> fontQuads = m_bFont.GetProcessedQuads( index );
                m_bFont.ClearString( index );
                

                // Highlight height is dependent on line height
                float highlightHeight = m_itemHeight;
                for ( int i = 0; i < m_normalHighlights.Count; i++ )
                {
                    m_normalHighlights[i].Height = highlightHeight;
                    m_disabledHighlights[i].Height = highlightHeight;
                }

                // Convert FontQuads to Quads
                for ( int j = 0; j < fontQuads.Count; j++ )
                {
                    m_fontQuads.Add( new Quad( fontQuads[j].TopLeft, fontQuads[j].TopRight, fontQuads[j].BottomLeft, fontQuads[j].BottomRight ) );
                }
            }
        }

        /// <summary>Selects an item.</summary>
        /// <param name="itemText">Item text</param>
        /// <returns>ID of the selected item.</returns>
        public override int SelectItem( string itemText )
        {
            m_selectedItem = base.SelectItem( itemText );

            BuildText();

            m_topItem = Math.Min( m_selectedItem, m_items.Count - m_numDisplayedItems );

            return m_selectedItem;
        }

        /// <summary>Clears the ComboBox's selected item.</summary>
        public override void Clear()
        {
            base.Clear();
            m_selectedItem = -1;
            BuildText();
        }

        /// <summary>Gets and sets whether the ComboBox is open.</summary>
        public bool IsOpen
        {
            get { return m_isOpen; }
            set
            {
                if ( value && !m_isOpen )
                {
                    m_overQuads.AddRange( m_quads );
                }
                else if ( !value && m_isOpen )
                {
                    m_overQuads.RemoveRange( 10, m_quads.Count );
                }
                m_isOpen = value;
                BuildText();
            }
        }
    }
}
