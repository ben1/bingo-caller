/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : ListBox.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : ListBox class

Created :  12/26/2005
Modified : 12/26/2005

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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit.Gui
{
    /// <summary>
    /// A ListBox control
    /// </summary>
    public class ListBox : Control
    {
        protected List<Quad> m_scrollUp;
        protected List<Quad> m_scrollDown;
        protected List<Quad> m_scrollMarker;
        protected List<Quad> m_disabledQuads;
        protected List<Quad> m_highlightQuads;
        protected List<Quad> m_normalHighlights;
        protected List<Quad> m_disabledHighlights;
        protected List<ListableItem> m_items;
        protected List<Rectangle> m_hotspots;
        protected int m_topItem;
        protected float m_itemHeight;
        protected float m_minMarkerHeight;
        protected int m_numDisplayedItems;
        protected int m_highlightColor;
        protected int m_disabledHighlightColor;
        protected bool m_scrolling;
        protected bool m_overState;
        protected bool m_singleItemListBox;
        protected bool m_newData;

        protected enum Section { Left, Right, Top, Bottom, Background, TopLeft, TopRight, BottomLeft, BottomRight, ScrollUp, ScrollDown, ScrollBody, ScrollMarker };
        protected enum HotSpot { Background, ScrollUp, ScrollDown, ScrollMarker, ScrollBody };
        protected enum Highlight { Normal, Disabled };

        /// <summary>Default Constructor</summary>
        public ListBox()
        {
            // Empty
        }

        /// <summary>Creates a ListBox</summary>
        /// <param name="id">Control ID</param>
        /// <param name="singleItemSelect">Whether single or multiple items can be selected</param>
        /// <param name="screenRect">Screen rectangle</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="font">Bitmap Font</param>
        /// <param name="node">ControlNode from XML file</param>
        /// <param name="info">Texture ImageInformation</param>
        public ListBox( int id, bool singleItemSelect, RectangleF screenRect, float fontSize, ColorValue textColor, BitmapFont font, ControlNode node, ImageInformation info )
        {
            m_topItem = 0;
            m_id = id;
            m_minMarkerHeight = 5f;
            m_fontSize = fontSize;
            m_itemHeight = 0f;
            m_textColor = textColor;
            m_bFont = font;
            m_scrolling = false;
            m_overState = false;
            m_newData = false;
            m_singleItemListBox = singleItemSelect;
            m_position = new PointF( screenRect.X, screenRect.Y );
            m_size = new SizeF( screenRect.Width, screenRect.Height );
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

            // Initialize Lists so we can access them with indices
            m_highlightQuads.Add( new Quad() );
            m_highlightQuads.Add( new Quad() );
            for ( int i = 0; i < 5; i++ )
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
                        m_quads[(int)Section.TopLeft] = q;
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
                        m_quads[(int)Section.Top] = q;
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
                        m_quads[(int)Section.TopRight] = q;
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
                        m_quads[(int)Section.Left] = q;
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
                        m_quads[(int)Section.Background] = q;
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
                        m_quads[(int)Section.Right] = q;
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
                        m_quads[(int)Section.BottomLeft] = q;
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
                        m_quads[(int)Section.Bottom] = q;
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
                        m_quads[(int)Section.BottomRight] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.BottomRight] = q;
                    }
                }
                else if ( i.Name.EndsWith( "ScrollBody" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        m_quads[(int)Section.ScrollBody] = q;
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.ScrollBody] = q;
                    }
                }
                else if ( i.Name.EndsWith( "Highlight" ) )
                {
                    if ( i.Name.StartsWith( "Normal" ) )
                    {
                        q.Height = m_fontSize + 6f;
                        m_highlightQuads[(int)Highlight.Normal] = q;
                        m_highlightColor = i.Color.ToArgb();
                    }
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledHighlightColor = i.Color.ToArgb();
                        m_highlightQuads[(int)Highlight.Disabled] = q;
                    }
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
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_scrollUp[(int)ControlState.Disabled] = q;
                        m_disabledQuads[(int)Section.ScrollUp] = q;
                    }
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
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_scrollDown[(int)ControlState.Disabled] = q;
                        m_disabledQuads[(int)Section.ScrollDown] = q;
                    }
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
                    else if ( i.Name.StartsWith( "Disabled" ) )
                    {
                        m_disabledQuads[(int)Section.ScrollMarker] = q;
                        m_scrollMarker[(int)ControlState.Disabled] = q;
                    }
                }
            }

            PositionQuads();
        }

        /// <summary>Adds a new item to the ListBox.</summary>
        /// <param name="text">Item text.</param>
        /// <param name="data">Item data.</param>
        public virtual void AddItem( string text, object data )
        {
            m_items.Add( new ListableItem( text, data ) );
            BuildText();

            // Highlights are designated by item index
            Quad normalHighlight = (Quad)m_highlightQuads[(int)Highlight.Normal].Clone();
            normalHighlight.X = m_quads[(int)Section.Background].X;
            normalHighlight.Width = m_quads[(int)Section.Background].Width;
            normalHighlight.Color = 0;
            m_normalHighlights.Add( normalHighlight );
            m_quads.Add( normalHighlight );

            Quad disabledHighlight = (Quad)m_highlightQuads[(int)Highlight.Disabled].Clone();
            disabledHighlight.X = m_quads[(int)Section.Background].X;
            disabledHighlight.Width = m_quads[(int)Section.Background].Width;
            disabledHighlight.Color = 0;
            m_disabledHighlights.Add( disabledHighlight );
            m_disabledQuads.Add( disabledHighlight );

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
            m_numDisplayedItems = (int)( m_quads[(int)Section.Background].Height / m_itemHeight );
            m_numDisplayedItems = Math.Min( m_numDisplayedItems, m_items.Count );
            BuildText();
            BuildHotspots();
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
            if ( m_overState )
            {
                m_overState = false;
                m_quads[(int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Normal];
                m_quads[(int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Normal];
                m_quads[(int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Normal];
            }
            return false;
        }

        /// <summary>Mouse Over event</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        protected override void OnMouseOver( Point cursor, bool[] buttons )
        {
            if ( m_hotspots[(int)HotSpot.ScrollUp].Contains( cursor ) )
            {
                m_overState = true;
                m_quads[(int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Over];
            }
            else
            {
                m_quads[(int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Normal];
            }
            if ( m_hotspots[(int)HotSpot.ScrollDown].Contains( cursor ) )
            {
                m_overState = true;
                m_quads[(int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Over];
            }
            else
            {
                m_quads[(int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Normal];
            }
            if ( m_hotspots[(int)HotSpot.ScrollMarker].Contains( cursor ) )
            {
                m_overState = true;
                m_quads[(int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Over];
            }
            else
            {
                m_quads[(int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Normal];
            }
        }

        /// <summary>Mouse Down event</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        protected override void OnMouseDown( Point cursor, bool[] buttons )
        {
            if ( m_hotspots[(int)HotSpot.ScrollUp].Contains( cursor ) && !m_hasFocus )
            {
                m_quads[(int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Down];
            }
            else if ( !m_hasFocus )
            {
                m_quads[(int)Section.ScrollUp] = m_scrollUp[(int)ControlState.Normal];
            }
            if ( m_hotspots[(int)HotSpot.ScrollDown].Contains( cursor ) && !m_hasFocus )
            {
                m_quads[(int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Down];
            }
            else if ( !m_hasFocus )
            {
                m_quads[(int)Section.ScrollDown] = m_scrollDown[(int)ControlState.Normal];
            }
            if ( m_hotspots[(int)HotSpot.ScrollMarker].Contains( cursor ) && !m_hasFocus )
            {
                m_quads[(int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Down];
            }
            else if ( !m_hasFocus )
            {
                m_quads[(int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Normal];
            }
            if ( ( m_hotspots[(int)HotSpot.ScrollBody].Contains( cursor ) || m_scrolling ) && 
                cursor.Y < m_quads[(int)Section.ScrollBody].Bottom && 
                cursor.Y > m_quads[(int)Section.ScrollBody].Y )
            {
                // Scroll with draggable scroll box
                m_scrolling = true;
                m_quads[(int)Section.ScrollMarker] = m_scrollMarker[(int)ControlState.Down];

                // Move scroll marker
                for ( int i = 0; i < 4; i++ )
                {
                    m_scrollMarker[i].Y =
                    m_scrollMarker[i].Y = cursor.Y - ( m_scrollMarker[i].Height / 2f );
                    m_scrollMarker[i].Y = Math.Max( m_scrollMarker[i].Y,
                        m_quads[(int)Section.ScrollBody].Y );
                    m_scrollMarker[i].Y = Math.Min( m_scrollMarker[i].Y,
                        m_quads[(int)Section.ScrollDown].Y - m_scrollMarker[i].Height );
                }
                m_disabledQuads[(int)Section.ScrollMarker].Y = m_quads[(int)Section.ScrollMarker].Y;

                // Change item range

                int oldTopItem = m_topItem;
                int divisionHeight = (int)m_quads[(int)Section.ScrollBody].Height / ( m_items.Count - m_numDisplayedItems + 1 );
                if ( divisionHeight > 0 )
                {
                    m_topItem = ( cursor.Y - (int)m_quads[(int)Section.ScrollBody].Y ) / divisionHeight;
                    m_topItem = Math.Min( m_topItem, m_items.Count - m_numDisplayedItems );
                }
                else
                {
                    m_topItem = 0;
                }

                // Scroll highlights also
                float highlightOffset = (float)( oldTopItem - m_topItem ) * m_itemHeight;
                for ( int i = 0; i < m_normalHighlights.Count; i++ )
                {
                    m_normalHighlights[i].Y += highlightOffset;
                    m_disabledHighlights[i].Y += highlightOffset;
                    if ( m_normalHighlights[i].Y < m_quads[(int)Section.Background].Y ||
                        m_normalHighlights[i].Bottom > m_quads[(int)Section.Background].Bottom )
                    {
                        m_normalHighlights[i].Color = 0;
                        m_disabledHighlights[i].Color = 0;
                    }
                    else if ( m_items[i].Selected )
                    {
                        m_normalHighlights[i].Color = m_highlightColor;
                        m_disabledHighlights[i].Color = m_disabledHighlightColor;
                    }
                }

                // Update the text and hotspots
                BuildText();
                BuildHotspots();
            }
        }

        /// <summary>Mouse Release event</summary>
        /// <param name="cursor">Mouse position</param>
        protected override void OnMouseRelease( Point cursor )
        {
            m_scrolling = false;
            m_newData = false;
            if ( m_hotspots[(int)HotSpot.ScrollDown].Contains( cursor ) )
            {
                ScrollDown();
                BuildHotspots();

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
                    m_items[highlightedItem].Selected = !m_items[highlightedItem].Selected;
                    if ( m_items[highlightedItem].Selected )
                    {
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

                        // Turn alpha on
                        m_normalHighlights[highlightedItem].Color = m_highlightColor;
                        m_disabledHighlights[highlightedItem].Color = m_disabledHighlightColor;

                        // Deselect other items if a single item list box
                        if ( m_singleItemListBox )
                        {
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
                        }
                        ( (ArrayList)m_data ).Add( m_items[highlightedItem].Data );

                        float newY = ( highlightedItem - m_topItem ) * m_itemHeight + (int)m_quads[(int)Section.Background].Y;
                        m_normalHighlights[highlightedItem].Y = newY;
                        m_disabledHighlights[highlightedItem].Y = newY;
                        m_normalHighlights[highlightedItem].Color = m_highlightColor;
                        m_disabledHighlights[highlightedItem].Color = m_disabledHighlightColor;
                    }
                    else
                    {
                        // Item was deselected
                        m_newData = true;
                        ( (ArrayList)m_data ).Remove( m_items[highlightedItem].Data );
                        m_normalHighlights[highlightedItem].Color = 0;
                        m_disabledHighlights[highlightedItem].Color = 0;
                    }
                }
            }
            BuildHotspots();
        }

        /// <summary>Scrolls down one line</summary>
        protected virtual void ScrollDown()
        {
            if ( m_topItem + m_numDisplayedItems < m_items.Count )
            {
                m_topItem++;

                // Move marker down
                int divisionHeight = (int)m_quads[(int)Section.ScrollBody].Height / ( m_items.Count - m_numDisplayedItems + 1 );
                for ( int i = 0; i < 4; i++ )
                {
                    m_scrollMarker[i].Y = m_quads[(int)Section.ScrollBody].Y + ( (float)m_topItem * (float)divisionHeight );
                }

                BuildText();

                // Adjust the highlights
                for ( int i = 0; i < m_normalHighlights.Count; i++ )
                {
                    m_normalHighlights[i].Y -= m_itemHeight;
                    m_disabledHighlights[i].Y -= m_itemHeight;

                    if ( m_normalHighlights[i].Y < m_quads[(int)Section.Background].Y ||
                        m_normalHighlights[i].Bottom > m_quads[(int)Section.Background].Bottom )
                    {
                        m_normalHighlights[i].Color = 0;
                        m_disabledHighlights[i].Color = 0;
                    }
                    else if ( m_items[i].Selected )
                    {
                        m_normalHighlights[i].Color = m_highlightColor;
                        m_disabledHighlights[i].Color = m_disabledHighlightColor;
                    }
                }
            }
            else
            {
                for ( int i = 0; i < 4; i++ )
                {
                    m_scrollMarker[i].Y = m_quads[(int)Section.ScrollBody].Bottom - m_quads[(int)Section.ScrollMarker].Height;
                }
            }
        }

        /// <summary>Scrolls up one line</summary>
        protected virtual void ScrollUp()
        {
            if ( m_topItem > 0 )
            {
                m_topItem--;

                // Move marker up
                int divisionHeight = (int)m_quads[(int)Section.ScrollBody].Height / ( m_items.Count - m_numDisplayedItems + 1 );
                for ( int i = 0; i < 4; i++ )
                {
                    m_scrollMarker[i].Y = m_quads[(int)Section.ScrollBody].Y + ( (float)m_topItem * (float)divisionHeight );
                }

                BuildText();

                // Adjust the highlights
                for ( int i = 0; i < m_normalHighlights.Count; i++ )
                {
                    m_normalHighlights[i].Y += m_itemHeight;
                    m_disabledHighlights[i].Y += m_itemHeight;

                    if ( m_normalHighlights[i].Y < m_quads[(int)Section.Background].Y ||
                        m_normalHighlights[i].Bottom > m_quads[(int)Section.Background].Bottom )
                    {
                        m_normalHighlights[i].Color = 0;
                        m_disabledHighlights[i].Color = 0;
                    }
                    else if ( m_items[i].Selected )
                    {
                        m_normalHighlights[i].Color = m_highlightColor;
                        m_disabledHighlights[i].Color = m_disabledHighlightColor;
                    }
                }
            }
            else
            {
                for ( int i = 0; i < 4; i++ )
                {
                    m_scrollMarker[i].Y = m_quads[(int)Section.ScrollBody].Y;
                }
            }
        }

        /// <summary>Mouse wheel event</summary>
        /// <param name="cursor">Mouse wheel delta</param>
        protected override void OnZDelta( float zDelta )
        {
            if ( zDelta > 0f )
            {
                ScrollUp();
            }
            else
            {
                ScrollDown();
            }
        }

        /// <summary>Gets the Button's current Quads</summary>
        public override List<Quad> Quads
        {
            get
            {
                switch ( m_state )
                {
                    case ControlState.Disabled:
                        return m_disabledQuads;
                    default:
                        return m_quads;
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
                for ( int i = 0; i < m_quads.Count; i++ )
                {
                    if ( i == (int)Section.ScrollUp || i == (int)Section.ScrollDown || i == (int)Section.ScrollMarker )
                    {
                        continue;
                    }
                    m_quads[i].X += xOffset;
                    m_quads[i].Y += yOffset;
                    m_disabledQuads[i].X += xOffset;
                    m_disabledQuads[i].Y += yOffset;
                }
                for ( int i = 0; i < 4; i++ )
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
        protected virtual void PositionQuads()
        {
            // Adjust middle column x
            m_quads[(int)Section.Top].X = m_quads[(int)Section.TopLeft].Right;
            m_disabledQuads[(int)Section.Top].X = m_disabledQuads[(int)Section.TopLeft].Right;

            m_quads[(int)Section.Background].X = m_quads[(int)Section.Left].Right;
            m_disabledQuads[(int)Section.Background].X = m_disabledQuads[(int)Section.Left].Right;

            m_quads[(int)Section.Bottom].X = m_quads[(int)Section.BottomLeft].Right;
            m_disabledQuads[(int)Section.Bottom].X = m_disabledQuads[(int)Section.BottomLeft].Right;

            // Adjust middle column width
            m_quads[(int)Section.Top].Width =
                m_size.Width - m_quads[(int)Section.TopLeft].Width -
                m_quads[(int)Section.TopRight].Width - m_quads[(int)Section.ScrollUp].Width;
            m_disabledQuads[(int)Section.Top].Width =
                m_size.Width - m_disabledQuads[(int)Section.TopLeft].Width -
                m_disabledQuads[(int)Section.TopRight].Width - m_disabledQuads[(int)Section.ScrollUp].Width;

            m_quads[(int)Section.Background].Width =
                m_size.Width - m_quads[(int)Section.Left].Width -
                m_quads[(int)Section.Right].Width - m_quads[(int)Section.ScrollBody].Width;
            m_disabledQuads[(int)Section.Background].Width =
                m_size.Width - m_disabledQuads[(int)Section.Left].Width -
                m_disabledQuads[(int)Section.Right].Width - m_disabledQuads[(int)Section.ScrollBody].Width;

            m_quads[(int)Section.Bottom].Width =
                m_size.Width - m_quads[(int)Section.BottomLeft].Width -
                m_quads[(int)Section.BottomRight].Width - m_quads[(int)Section.ScrollDown].Width;
            m_disabledQuads[(int)Section.Bottom].Width =
                m_size.Width - m_disabledQuads[(int)Section.BottomLeft].Width -
                m_disabledQuads[(int)Section.BottomRight].Width - m_disabledQuads[(int)Section.ScrollDown].Width;

            // Adjust right column X
            m_quads[(int)Section.TopRight].X =
                m_quads[(int)Section.Top].Right;
            m_disabledQuads[(int)Section.TopRight].X =
                m_disabledQuads[(int)Section.Top].Right;

            m_quads[(int)Section.Right].X =
                m_quads[(int)Section.Background].Right;
            m_disabledQuads[(int)Section.Right].X =
                m_disabledQuads[(int)Section.Background].Right;

            m_quads[(int)Section.BottomRight].X =
                m_quads[(int)Section.Bottom].Right;
            m_disabledQuads[(int)Section.BottomRight].X =
                m_disabledQuads[(int)Section.Bottom].Right;

            // Adjust middle row Y
            m_quads[(int)Section.Left].Y = m_quads[(int)Section.TopLeft].Bottom;
            m_disabledQuads[(int)Section.Left].Y = m_disabledQuads[(int)Section.TopLeft].Bottom;

            m_quads[(int)Section.Background].Y = m_quads[(int)Section.Top].Bottom;
            m_disabledQuads[(int)Section.Background].Y = m_disabledQuads[(int)Section.Top].Bottom;

            m_quads[(int)Section.Right].Y = m_quads[(int)Section.TopRight].Bottom;
            m_disabledQuads[(int)Section.Right].Y = m_disabledQuads[(int)Section.TopRight].Bottom;

            // Adjust middle row height
            m_quads[(int)Section.Left].Height =
                m_size.Height - m_quads[(int)Section.TopLeft].Height -
                m_quads[(int)Section.BottomLeft].Height;
            m_disabledQuads[(int)Section.Left].Height =
                m_size.Height - m_disabledQuads[(int)Section.TopLeft].Height -
                m_disabledQuads[(int)Section.BottomLeft].Height;

            m_quads[(int)Section.Background].Height =
                m_size.Height - m_quads[(int)Section.Top].Height -
                m_quads[(int)Section.Bottom].Height;
            m_disabledQuads[(int)Section.Background].Height =
                m_size.Height - m_disabledQuads[(int)Section.Top].Height -
                m_disabledQuads[(int)Section.Bottom].Height;

            m_quads[(int)Section.Right].Height =
                m_size.Height - m_quads[(int)Section.TopRight].Height -
                m_quads[(int)Section.BottomRight].Height;
            m_disabledQuads[(int)Section.Right].Height =
                m_size.Height - m_disabledQuads[(int)Section.TopRight].Height -
                m_disabledQuads[(int)Section.BottomRight].Height;

            // Adjust bottom row Y
            m_quads[(int)Section.BottomLeft].Y =
                m_quads[(int)Section.Left].Bottom;
            m_disabledQuads[(int)Section.BottomLeft].Y =
                m_disabledQuads[(int)Section.Left].Bottom;

            m_quads[(int)Section.Bottom].Y =
                m_quads[(int)Section.Background].Bottom;
            m_disabledQuads[(int)Section.Bottom].Y =
                m_disabledQuads[(int)Section.Background].Bottom;

            m_quads[(int)Section.BottomRight].Y =
                m_quads[(int)Section.Right].Bottom;
            m_disabledQuads[(int)Section.BottomRight].Y =
                m_disabledQuads[(int)Section.Right].Bottom;

            // Adjust scroll bar X
            for ( int i = 0; i < 4; i++ )
            {
                m_scrollUp[i].X = m_quads[(int)Section.TopRight].Right;
                m_scrollDown[i].X = m_quads[(int)Section.BottomRight].Right;
                m_scrollMarker[i].X = m_quads[(int)Section.Right].Right;
            }

            m_quads[(int)Section.ScrollBody].X =
                m_quads[(int)Section.Right].Right;
            m_disabledQuads[(int)Section.ScrollBody].X =
                m_disabledQuads[(int)Section.Right].Right;

            // Adjust scroll bar Y
            m_quads[(int)Section.ScrollBody].Y =
                m_quads[(int)Section.ScrollUp].Bottom;
            m_disabledQuads[(int)Section.ScrollBody].Y =
                m_disabledQuads[(int)Section.ScrollUp].Bottom;

            m_quads[(int)Section.ScrollBody].Height =
                m_size.Height - m_quads[(int)Section.ScrollUp].Height -
                m_quads[(int)Section.ScrollDown].Height;
            m_disabledQuads[(int)Section.ScrollBody].Height =
                m_size.Height - m_disabledQuads[(int)Section.ScrollUp].Height -
                m_disabledQuads[(int)Section.ScrollDown].Height;

            for ( int i = 0; i < 4; i++ )
            {
                m_scrollDown[i].Y = m_quads[(int)Section.ScrollBody].Bottom;
                m_scrollMarker[i].Y = m_quads[(int)Section.ScrollUp].Bottom;
            }

            BuildHotspots();

            BuildText();
        }

        /// <summary>Builds the hotspots</summary>
        protected virtual void BuildHotspots()
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
        }

        /// <summary>Builds the text</summary>
        protected virtual new void BuildText()
        {
            m_text = "";
            for ( int i = m_topItem; i < m_items.Count; i++ )
            {
                m_text += m_items[i].Text + "\n";
            }
            int index = m_bFont.AddString( m_text, new RectangleF(
                    m_quads[(int)Section.Background].X + 2f, m_quads[(int)Section.Background].Y,
                    m_quads[(int)Section.Background].Width, m_quads[(int)Section.Background].Height ),
                    BitmapFont.Align.Left, m_fontSize, m_textColor, true );
            List<FontQuad> fontQuads = m_bFont.GetProcessedQuads( index );

            m_numDisplayedItems = (int)(m_quads[(int)Section.Background].Height / m_itemHeight);
            m_numDisplayedItems = Math.Min( m_numDisplayedItems, m_items.Count );

            // Highlight height is dependent on line height
            float highlightHeight = m_itemHeight;// +( m_itemHeight / 10f );
            for ( int i = 0; i < m_normalHighlights.Count; i++ )
            {
                m_normalHighlights[i].Height = highlightHeight;
                m_disabledHighlights[i].Height = highlightHeight;
            }

            // Convert FontQuads to Quads
            m_fontQuads = new List<Quad>();
            for ( int j = 0; j < fontQuads.Count; j++ )
            {
                m_fontQuads.Add( new Quad( fontQuads[j].TopLeft, fontQuads[j].TopRight, fontQuads[j].BottomLeft, fontQuads[j].BottomRight ) );
            }
            m_bFont.ClearString( index );
        }

        /// <summary>Checks if the cursor is in the scrollable background</summary>
        /// <param name="cursor">Mouse position</param>
        /// <returns>true if the cursor is in the scrollable background, false otherwise</returns>
        public bool BackgroundContainsCursor( Point cursor )
        {
            return m_hotspots[(int)HotSpot.Background].Contains( cursor );
        }

        /// <summary>Checks if an item is in the list of items.</summary>
        /// <param name="text">Item text</param>
        /// <returns>true if the item is already in the ListBox, false otherwise.</returns>
        public virtual bool ContainsItem( string text )
        {
            for ( int i = 0; i < m_items.Count; i++ )
            {
                if ( m_items[i].Text == text )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Clears the ListBox's current selection</summary>
        public virtual void Clear()
        {
            if ( m_quads.Count > 13 )
            {
                m_quads.RemoveRange( 13, m_items.Count );
                m_normalHighlights.Clear();
                m_disabledHighlights.Clear();
            }
            ( (ArrayList)m_data ).Clear();
            m_items.Clear();
        }

        /// <summary>Selects an item</summary>
        /// <param name="itemText">Text of item to select</param>
        /// <returns>ID of the selected item.</returns>
        public virtual int SelectItem( string itemText )
        {
            int id = -1;
            if ( m_singleItemListBox )
            {
                ( (ArrayList)m_data ).Clear();
            }

            for ( int i = 0; i < m_items.Count; i++ )
            {
                if ( m_items[i].Text != itemText )
                {
                    // Deselect other items if a single item list box
                    if ( m_singleItemListBox )
                    {
                        m_items[i].Selected = false;
                        m_normalHighlights[i].Color = 0;
                        m_disabledHighlights[i].Color = 0;
                    }
                    continue;
                }
                id = i;
                m_items[i].Selected = true;
                ( (ArrayList)m_data ).Add( m_items[i].Data );

                // Turn alpha on
                m_normalHighlights[i].Color = m_highlightColor;
                m_disabledHighlights[i].Color = m_disabledHighlightColor;
                
                int oldTopItem = m_topItem;

                m_topItem = Math.Min( i, m_items.Count - m_numDisplayedItems );

                // Adjust highlights for multiple selection lists
                if ( !m_singleItemListBox )
                {
                    float highlightOffset = ( m_topItem - oldTopItem ) * m_itemHeight;
                    for ( int j = 0; j < m_normalHighlights.Count; j++ )
                    {
                        m_normalHighlights[j].Y -= highlightOffset;
                        m_disabledHighlights[j].Y -= highlightOffset;
                        if ( m_normalHighlights[j].Y < m_quads[(int)Section.Background].Y ||
                            m_normalHighlights[j].Bottom > m_quads[(int)Section.Background].Bottom )
                        {
                            m_normalHighlights[j].Color = 0;
                            m_disabledHighlights[j].Color = 0;
                        }
                        else if ( m_items[j].Selected )
                        {
                            m_normalHighlights[j].Color = m_highlightColor;
                            m_disabledHighlights[j].Color = m_disabledHighlightColor;
                        }
                    }
                }

                // Move Scroll Marker
                int divisionHeight = (int)m_quads[(int)Section.ScrollBody].Height / ( m_items.Count - m_numDisplayedItems + 1 );
                for ( int j = 0; j < 4; j++ )
                {
                    m_scrollMarker[j].Y = m_quads[(int)Section.ScrollBody].Y + (float)m_topItem * (float)divisionHeight;
                }

                float newY = ( i - m_topItem ) * m_itemHeight + (int)m_quads[(int)Section.Background].Y;
                m_normalHighlights[i].Y = newY;
                m_disabledHighlights[i].Y = newY;
                m_normalHighlights[i].Color = m_highlightColor;
                m_disabledHighlights[i].Color = m_disabledHighlightColor;

                if ( !( this is ComboBox ) )
                {
                    BuildText();
                }
            }
            return id;
        }

        /// <summary>Gets whether a new item has been selected.</summary>
        public bool HasNewData
        {
            get { return m_newData; }
        }
    }
}
