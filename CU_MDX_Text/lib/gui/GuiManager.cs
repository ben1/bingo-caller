/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : GuiManager.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Manages the entire GUI system.

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
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Direct3D.CustomVertex;
using Microsoft.DirectX.Generic;

namespace CUnit.Gui
{
    public class GuiManager
    {
        private List<Quad> m_quads;
        private List<FontQuad> m_fontQuads;
        private List<ControlNode> m_controlNodes;
        private List<Control> m_controls;
        private VertexBuffer m_vb = null;
        private Texture m_texture = null;
        private ImageInformation m_imageInfo;
        private string m_textureFileName;
        private string m_fntFile;
        private string m_fontImage;
        private string m_searchName;
        private bool m_inCallback;
        private bool m_dirtyBuffer;
        private bool m_panelDragging;
        private int m_openComboBox;
        private int m_activeEditBox;
        private BitmapFont m_bFont = null;
        private ControlDelegate m_onControl;
        public delegate void ControlDelegate( int controlID, object data );
        private const int MaxVertices = 4096;

        public GuiManager( string xmlStyleSheet, ControlDelegate onControl )
        {
            m_inCallback = false;
            m_panelDragging = false;
            m_dirtyBuffer = true;
            m_quads = new List<Quad>();
            m_fontQuads = new List<FontQuad>();
            m_controlNodes = new List<ControlNode>();
            m_controls = new List<Control>();
            m_onControl = onControl;
            m_openComboBox = -1;
            m_activeEditBox = -1;
            ParseXML( xmlStyleSheet );
            m_bFont = new BitmapFont( m_fntFile, m_fontImage );
        }

        /// <summary>Parses the XML file</summary>
        /// <param name="xmlFile">XML file name</param>
        private void ParseXML( string xmlFile )
        {
            XmlTextReader reader = new XmlTextReader( Utility.GetMediaFile( xmlFile ) );
            reader.WhitespaceHandling = WhitespaceHandling.None;
            while ( reader.Read() )
            {
                if ( reader.NodeType == XmlNodeType.Element )
                {
                    if ( reader.Name == "Gui" )
                    {
                        // Read in the image file name
                        for ( int i = 0; i < reader.AttributeCount; i++ )
                        {
                            reader.MoveToAttribute( i );
                            if ( reader.Name == "ImageFile" )
                            {
                                m_textureFileName = Utility.GetMediaFile( reader.Value );
                                m_imageInfo = Texture.GetImageInformationFromFile( m_textureFileName );
                            }
                            else if ( reader.Name == "FntFile" )
                            {
                                m_fntFile = reader.Value;
                            }
                            else if ( reader.Name == "FontImage" )
                            {
                                m_fontImage = reader.Value;
                            }
                        }
                    }
                    else if ( reader.Name == "Control" )
                    {
                        ControlNode controlNode = new ControlNode();
                        for ( int i = 0; i < reader.AttributeCount; i++ )
                        {
                            reader.MoveToAttribute( i );
                            if ( reader.Name == "Name" )
                            {
                                controlNode.Name = reader.Value;
                            }
                        }
                        // Read the Image elements of this Control
                        while ( reader.NodeType != XmlNodeType.EndElement )
                        {
                            reader.Read();
                            if ( ( reader.NodeType == XmlNodeType.Element ) && ( reader.Name == "Image" ) )
                            {
                                ImageNode imageNode = new ImageNode();
                                for ( int i = 0; i < reader.AttributeCount; i++ )
                                {
                                    reader.MoveToAttribute( i );
                                    if ( reader.Name == "Name" )
                                    {
                                        imageNode.Name = reader.Value;
                                    }
                                    else if ( reader.Name == "X" )
                                    {
                                        imageNode.Rectangle.X = reader.ReadContentAsFloat();
                                    }
                                    else if ( reader.Name == "Y" )
                                    {
                                        imageNode.Rectangle.Y = reader.ReadContentAsFloat();
                                    }
                                    else if ( reader.Name == "Width" )
                                    {
                                        imageNode.Rectangle.Width = reader.ReadContentAsFloat();
                                    }
                                    else if ( reader.Name == "Height" )
                                    {
                                        imageNode.Rectangle.Height = reader.ReadContentAsFloat();
                                    }
                                    else if ( reader.Name == "Color" )
                                    {
                                        imageNode.Color = StringToColor( reader.Value );
                                    }
                                }
                                controlNode.Images.Add( imageNode );
                            }
                        }
                        m_controlNodes.Add( controlNode );
                    }
                }
            }
        }

        /// <summary>Converts a hex string into a Color</summary>
        /// <param name="hexString">Hex stream of form 0x00000000 or 00000000</param>
        /// <returns>New Color</returns>
        private Color StringToColor( string hexString )
        {
            if ( hexString.IndexOf( "0x" ) >= 0 )
            {
                hexString = hexString.Remove( 0, 2 );
            }
            System.Globalization.NumberStyles style =
                System.Globalization.NumberStyles.AllowHexSpecifier;
            int alpha = int.Parse( hexString.Substring( 0, 2 ), style );
            int red = int.Parse( hexString.Substring( 2, 2 ), style );
            int green = int.Parse( hexString.Substring( 4, 2 ), style );
            int blue = int.Parse( hexString.Substring( 6, 2 ), style );
            return Color.FromArgb( alpha, red, green, blue );
        }

        /// <summary>Call after the device is created.</summary>
        /// <param name="device">D3D Device</param>
        public void OnCreateDevice( Device device )
        {
            m_texture = new Texture( device, m_textureFileName );
            if ( m_bFont != null )
            {
                m_bFont.OnCreateDevice( device );
            }
        }

        /// <summary>Call when the device is destroyed.</summary>
        public void OnDestroyDevice()
        {
            if ( m_texture != null )
            {
                m_texture.Dispose();
                m_texture = null;
            }
            if ( m_bFont != null )
            {
                m_bFont.OnDestroyDevice();
            }
        }

        /// <summary>Call when the device is lost.</summary>
        public void OnLostDevice()
        {
            if ( m_vb != null )
            {
                m_vb.Dispose();
                m_vb = null;
            }
            if ( m_bFont != null )
            {
                m_bFont.OnLostDevice();
            }
        }

        /// <summary>Call after the device is reset.</summary>
        /// <param name="device">D3D Device</param>
        public void OnResetDevice( Device device )
        {
            m_vb = new VertexBuffer( device, MaxVertices * TransformedColoredTextured.StrideSize,
                Usage.Dynamic | Usage.WriteOnly, TransformedColoredTextured.Format, Pool.Default, null );
            BuildQuadList();
            UpdateBuffer();
            if ( m_bFont != null )
            {
                m_bFont.OnResetDevice( device );
            }
        }

        /// <summary>Clears the Gui</summary>
        public void Clear()
        {
            m_quads.Clear();
            m_fontQuads.Clear();
            m_controlNodes.Clear();
            m_controls.Clear();
        }

        /// <summary>Creates a new Panel</summary>
        /// <param name="id">Control id</param>
        /// <param name="position">Panel position</param>
        /// <param name="size">Panel size</param>
        public void CreatePanel( int id, PointF position, SizeF size )
        {
            CheckUniqueID( id );
            m_searchName = "Panel";
            ControlNode node = m_controlNodes.Find( HasSearchName );
            if ( node == null )
            {
                throw new Exception( "Unable to find control node for type Panel.\r\n" );
            }
            Panel p = new Panel( id, new RectangleF( position, size ), node, m_imageInfo );
            AddControl( 0, p );
        }

        /// <summary>Creates a new text Label</summary>
        /// <param name="id">Control id</param>
        /// <param name="panelID">ID of Panel to associate Control with or 0 to make independant Control</param>
        /// <param name="position">Button position</param>
        /// <param name="size">Button size</param>
        /// <param name="text">Button text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="alignment">Text alignment</param>
        public void CreateLabel( int id, int panelID, PointF position, SizeF size, string text, float fontSize, ColorValue textColor, BitmapFont.Align alignment )
        {
            CheckUniqueID( id );
            Label c = new Label( id, new RectangleF( position, size ), text, fontSize, textColor, m_bFont, alignment );
            AddControl( panelID, c );
        }

        /// <summary>Creates a new Button</summary>
        /// <param name="id">Control id</param>
        /// <param name="panelID">ID of Panel to associate Control with or 0 to make independant Control</param>
        /// <param name="position">Button position</param>
        /// <param name="size">Button size</param>
        /// <param name="text">Button text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        public void CreateButton( int id, int panelID, PointF position, SizeF size, string text, float fontSize, ColorValue textColor )
        {
            CheckUniqueID( id );
            m_searchName = "Button";
            ControlNode node = m_controlNodes.Find( HasSearchName );
            if ( node == null )
            {
                throw new Exception( "Unable to find control node for type Button.\r\n" );
            }
            Button c = new Button( id, new RectangleF( position, size ), text, fontSize, textColor, m_bFont, node, m_imageInfo );
            AddControl( panelID, c );
        }

        /// <summary>Creates a new CheckBox</summary>
        /// <param name="id">Control id</param>
        /// <param name="panelID">ID of Panel to associate Control with or 0 to make independant Control</param>
        /// <param name="position">Button position</param>
        /// <param name="size">Button size</param>
        /// <param name="text">Button text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="textAlignment">Which side of the control to place the text on.</param>
        /// <param name="isChecked">Whether the CheckBox is initially checked or not.</param>
        public void CreateCheckBox( int id, int panelID, PointF position, SizeF size, string text, float fontSize, ColorValue textColor, Control.TextAlign textAlignment, bool isChecked )
        {
            CheckUniqueID( id );
            m_searchName = "CheckBox";
            ControlNode node = m_controlNodes.Find( HasSearchName );
            if ( node == null )
            {
                throw new Exception( "Unable to find control node for type CheckBox.\r\n" );
            }
            CheckBox c = new CheckBox( id, new RectangleF( position, size ), text, fontSize, textColor, textAlignment, isChecked, m_bFont, node, m_imageInfo );
            AddControl( panelID, c );
        }

        /// <summary>Creates a new RadioButton</summary>
        /// <param name="id">Control id</param>
        /// <param name="panelID">ID of Panel to associate Control with or 0 to make independant Control</param>
        /// <param name="groupID">Radio button groupID. Only one button per group may be selected at once.</param>
        /// <param name="position">Button position</param>
        /// <param name="size">Button size</param>
        /// <param name="text">Button text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="textAlignment">Which side of the control to place the text on.</param>
        /// <param name="isSelected">Whether the RadioButton is initially selected.</param>
        public void CreateRadioButton( int id, int panelID, int groupID, PointF position, SizeF size, string text, float fontSize, ColorValue textColor, Control.TextAlign textAlignment, bool isSelected )
        {
            CheckUniqueID( id );
            m_searchName = "RadioButton";
            ControlNode node = m_controlNodes.Find( HasSearchName );
            if ( node == null )
            {
                throw new Exception( "Unable to find control node for type CheckBox.\r\n" );
            }
            RadioButton c = new RadioButton( id, groupID, new RectangleF( position, size ), text, fontSize, textColor, textAlignment, isSelected, m_bFont, node, m_imageInfo );

            // If RadioButton is selected, deselect RadioButtons of same groupID
            if ( isSelected )
            {
                c.NeedToDelectOthers = false;
                for ( int i = 0; i < m_controls.Count; i++ )
                {
                    if ( ( m_controls[i] is RadioButton ) &&
                        ( ( m_controls[i] as RadioButton ).GroupID == c.GroupID ) )
                    {
                        ( m_controls[i] as RadioButton ).Deselect();
                    }
                }
            }

            AddControl( panelID, c );
        }

        /// <summary>Creates a new Slider</summary>
        /// <param name="id">Control id</param>
        /// <param name="panelID">ID of Panel to associate Control with or 0 to make independant Control</param>
        /// <param name="position">Slider position</param>
        /// <param name="width">Slider width</param>
        /// <param name="min">Minimum slider value;</param>
        /// <param name="max">Maximum slider value;</param>
        /// <param name="current">Current slider value;</param>
        /// <param name="text">Slider text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="textAlignment">Which side of the control to place the text on.</param>
        public void CreateSlider( int id, int panelID, PointF position, float width, float min, float max, float current, string text, float fontSize, ColorValue textColor, Control.TextAlign textAlignment )
        {
            CheckUniqueID( id );
            m_searchName = "Slider";
            ControlNode node = m_controlNodes.Find( HasSearchName );
            if ( node == null )
            {
                throw new Exception( "Unable to find control node for type Slider.\r\n" );
            }
            Slider c = new Slider( id, position, width, min, max, current, text, fontSize, textColor, textAlignment, m_bFont, node, m_imageInfo );
            AddControl( panelID, c );
        }

        /// <summary>Creates a new Button</summary>
        /// <param name="id">Control id</param>
        /// <param name="panelID">ID of Panel to associate Control with or 0 to make independant Control</param>
        /// <param name="singleItemSelect">Whether single or multiple items can be selected</param>
        /// <param name="position">Button position</param>
        /// <param name="size">Button size</param>
        /// <param name="text">Button text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        public void CreateListBox( int id, int panelID, bool singleItemSelect, PointF position, SizeF size, float fontSize, ColorValue textColor )
        {
            CheckUniqueID( id );
            m_searchName = "ListBox";
            ControlNode node = m_controlNodes.Find( HasSearchName );
            if ( node == null )
            {
                throw new Exception( "Unable to find control node for type ListBox.\r\n" );
            }
            ListBox c = new ListBox( id, singleItemSelect, new RectangleF( position, size ), fontSize, textColor, m_bFont, node, m_imageInfo );
            AddControl( panelID, c );
        }

        /// <summary>Creates a new Button</summary>
        /// <param name="id">Control id</param>
        /// <param name="panelID">ID of Panel to associate Control with or 0 to make independant Control</param>
        /// <param name="singleItemSelect">true if only one item can be selected at a time, false to allow multiple selection</param>
        /// <param name="position">Button position</param>
        /// <param name="size">Button size</param>
        /// <param name="text">Button text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="textColor">Text color</param>
        public void CreateComboBox( int id, int panelID, PointF position, SizeF size, float openHeight, float fontSize, ColorValue textColor )
        {
            CheckUniqueID( id );
            m_searchName = "ComboBox";
            ControlNode node = m_controlNodes.Find( HasSearchName );
            if ( node == null )
            {
                throw new Exception( "Unable to find control node for type ComboBox.\r\n" );
            }
            ComboBox c = new ComboBox( id, new RectangleF( position, size ), openHeight, fontSize, textColor, m_bFont, node, m_imageInfo );
            AddControl( panelID, c );
        }

        /// <summary>Creates a new EditBox</summary>
        /// <param name="id">Control id</param>
        /// <param name="panelID">ID of Panel to associate Control with or 0 to make independant Control</param>
        /// <param name="singleItemSelect">Whether single or multiple items can be selected</param>
        /// <param name="position">Button position</param>
        /// <param name="size">Button size</param>
        /// <param name="text">Button text</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="maxLength">Max number of characters allowed in the edit box.</param>
        /// <param name="textColor">Text color</param>
        public void CreateEditBox( int id, int panelID, PointF position, SizeF size, string text, float fontSize, int maxLength, ColorValue textColor )
        {
            CheckUniqueID( id );
            m_searchName = "EditBox";
            ControlNode node = m_controlNodes.Find( HasSearchName );
            if ( node == null )
            {
                throw new Exception( "Unable to find control node for type EditBox.\r\n" );
            }
            EditBox c = new EditBox( id, new RectangleF( position, size ),text, fontSize, maxLength, textColor, m_bFont, node, m_imageInfo );
            AddControl( panelID, c );
        }

        /// <summary>Adds an item to a ListBox or ComboBox</summary>
        /// <param name="controlID">ID of Control to add Listable item.</param>
        /// <param name="text">Displayed text of item.</param>
        /// <param name="data">Data of item.</param>
        public void AddListableItem( int controlID, string text, object data )
        {
            Control c = this[controlID];
            if ( !( c is ListBox ) )
            {
                throw new Exception( "Error: Tried to add ListableItem to non-listable Control" );
            }
            ( c as ListBox ).AddItem( text, data );
        }

        /// <summary>Adds a new Control to the list</summary>
        /// <param name="panelID">PanelID to add Control to, or 0 for independant Control</param>
        /// <param name="c">New Control</param>
        private void AddControl( int panelID, Control c )
        {
            if ( panelID == 0 )
            {
                // Control doesn't have a Panel
                // Find highest zDepth
                if ( m_controls.Count > 0 )
                {
                    c.ZDepth = m_controls[0].ZDepth + 1;
                }
                else
                {
                    c.ZDepth = 1;
                }
            }
            else
            {
                AttachControlToPanel( panelID, c );
            }
            c.OnControl += new ControlDelegate( m_onControl );
            m_controls.Add( c );
            SortControls();
            BuildQuadList();
        }

        /// <summary>Attaches a Control to a Panel</summary>
        /// <param name="panelID">Panel's ID</param>
        /// <param name="c">Control</param>
        private void AttachControlToPanel( int panelID, Control c )
        {
            // Attach Control to Panel 
            Control p = this[panelID];
            if ( !( p is Panel ) )
            {
                throw new Exception( "Error: Tried to attach Control to non-panel Control." );
            }
            c.PanelID = panelID;
            c.ZDepth = p.ZDepth;
            ( p as Panel ).NumControls++;
            c.Position += new SizeF( p.Position.X, p.Position.Y );
        }

        /// <summary>Deletes the Control with the specified ID.</summary>
        /// <param name="id">Control ID</param>
        public void DeleteControl( int id )
        {
            for ( int i = 0; i < m_controls.Count; i++ )
            {
                if ( m_controls[i].ID == id )
                {
                    m_controls.RemoveAt( i );
                    BuildQuadList();
                    UpdateBuffer();
                    break;
                }
            }
        }

        /// <summary>Rebuilds the list of Quads.</summary>
        private void BuildQuadList()
        {
            m_quads.Clear();
            m_fontQuads.Clear();
            for ( int i = 0; i < m_controls.Count; i++ )
            {
                // Set starting vertices to access when we render the Control
                m_controls[i].StartVertex = m_quads.Count * 6;
                m_quads.AddRange( m_controls[i].Quads );
                m_controls[i].FontStartVertex = m_quads.Count * 6;
                m_quads.AddRange( m_controls[i].FontQuads );
            }
        }

        /// <summary>Writes all the vertices to the vertex buffers.</summary>
        private void UpdateBuffer()
        {
            if ( m_inCallback )
            {
                return;
            }
            GraphicsBuffer<TransformedColoredTextured> data =
                m_vb.Lock<TransformedColoredTextured>( 0, 6 * m_quads.Count, LockFlags.Discard );
            for ( int i = 0; i < m_quads.Count; i++ )
            {
                data.Write( m_quads[i].Vertices );
            }
            m_vb.Unlock();
            m_dirtyBuffer = false;
        }

        /// <summary>Renders the GUI</summary>
        /// <param name="device">D3D Device</param>
        public void Render( Device device )
        {
            if ( m_dirtyBuffer )
            {
                BuildQuadList();
                UpdateBuffer();
            }

            // Set render states
            device.SetRenderState( RenderStates.ZEnable, false );
            device.SetRenderState( RenderStates.FillMode, (int)FillMode.Solid );
            device.SetRenderState( RenderStates.ZBufferWriteEnable, false );
            device.SetRenderState( RenderStates.FogEnable, false );
            device.SetRenderState( RenderStates.AlphaTestEnable, false );
            device.SetRenderState( RenderStates.AlphaBlendEnable, true );
            device.SetRenderState( RenderStates.SourceBlend, (int)Blend.SourceAlpha );
            device.SetRenderState( RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha );

            // Blend alphas
            device.SetTextureState( 0, TextureStates.ColorArgument1, (int)TextureArgument.Texture );
            device.SetTextureState( 0, TextureStates.AlphaArgument1, (int)TextureArgument.Texture );
            device.SetTextureState( 0, TextureStates.AlphaArgument2, (int)TextureArgument.Diffuse );
            device.SetTextureState( 0, TextureStates.AlphaOperation, (int)TextureOperation.Modulate );

            // Set sampler states
            device.SetSamplerState( 0, SamplerStates.MinFilter, (int)Filter.Linear );
            device.SetSamplerState( 0, SamplerStates.MagFilter, (int)Filter.Linear );
            device.SetSamplerState( 0, SamplerStates.MipFilter, (int)Filter.Linear );

            // Render
            device.VertexFormat = TransformedColoredTextured.Format;
            device.SetTexture( 0, m_texture );
            device.SetStreamSource( 0, m_vb, 0, TransformedColoredTextured.StrideSize );
            foreach ( Control c in m_controls )
            {
                device.DrawPrimitives( PrimitiveType.TriangleList, c.StartVertex, 2 * c.Quads.Count );
                if ( c.Text != string.Empty && c.Text != "" )
                {
                    device.SetTexture( 0, m_bFont.Texture );
                    device.DrawPrimitives( PrimitiveType.TriangleList, c.FontStartVertex, 2 * c.FontQuads.Count );
                    device.SetTexture( 0, m_texture );
                }

            }
        }

        /// <summary>Keyboard handler.</summary>
        /// <param name="pressedKeys">List of pressed keys</param>
        /// <param name="pressedChar">Pressed character</param>
        /// <param name="pressedKey">Pressed key from Form used for repeatable keys</param>
        /// <returns>true if a Control processed the keyboard, false otherwise</returns>
        public bool KeyBoardHandler( List<System.Windows.Forms.Keys> pressedKeys, char pressedChar, int pressedKey )
        {
            // Go front to back
            for ( int i = m_controls.Count - 1; i >= 0; i-- )
            {
                if ( m_controls[i].Disabled )
                {
                    // Ignore disabled Controls
                    continue;
                }
                if ( m_controls[i].KeyboardHandler( pressedKeys, pressedChar, pressedKey ) )
                {
                    BuildQuadList();
                    UpdateBuffer();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Mouse handler</summary>
        /// <param name="cursor">Mouse position</param>
        /// <param name="buttons">Mouse buttons</param>
        /// <param name="zDelta">Mouse wheel delta</param>
        /// <returns>true if the gui handled a mouse-click, false otherwise.</returns>
        public bool MouseHandler( Point cursor, bool[] buttons, float zDelta )
        {
            bool result = false;
            m_inCallback = true;
            // Go front to back
            for ( int i = m_controls.Count - 1; i >= 0; i-- )
            {
                if ( m_controls[i].Disabled )
                {
                    // Ignore disabled Controls
                    continue;
                }
                if ( m_panelDragging && !buttons[0] )
                {
                    m_panelDragging = false;
                }
                int numControls = m_controls.Count;
                if ( ( !m_panelDragging || ( m_controls[i] is Panel ) ) && m_controls[i].MouseHandler( cursor, buttons, zDelta ) )
                {
                    if ( numControls != m_controls.Count )
                    {
                        // If we're here, we used a control to delete another control, so 
                        // the index will be missing. Return to prevent IndexOutOfBounds
                        return true;
                    }
                    if ( ( m_controls[i] is Panel ) && ( m_controls[i].State == Control.ControlState.Down ) )
                    {
                        m_panelDragging = true;
                    }

                    if ( ( m_controls[i] is ComboBox ) && ( m_controls[i] as ComboBox ).IsOpen )
                    {
                        if ( ( m_openComboBox != -1 ) && m_openComboBox != m_controls[i].ID )
                        {
                            // Close any open ComboBox
                            for ( int j = 0; j < m_controls.Count; j++ )
                            {
                                if ( m_controls[j].ID == m_openComboBox )
                                {
                                    ( m_controls[j] as ComboBox ).IsOpen = false;
                                    break;
                                }
                            }
                        }
                        m_openComboBox = m_controls[i].ID;
                    }

                    if ( ( m_controls[i] is EditBox ) && m_controls[i].HasFocus )
                    {
                        m_activeEditBox = i;
                    }

                    m_dirtyBuffer = true;

                    // For Controls in a Panel, mouse may have just been released from another 
                    // Control's focus, so we need to make sure we reset that Control's state
                    if ( ( m_controls[i].State == Control.ControlState.Over ) && ( m_controls[i].PanelID != 0 )
                        && !( m_controls[i] is Panel ) )
                    {
                        for ( int j = m_controls.Count - 1;  j >= 0 && m_controls[j].ZDepth == 1; j-- )
                        {
                            if ( m_controls[j].State == Control.ControlState.Down )
                            {
                                m_controls[j].State = Control.ControlState.Normal;
                                break;
                            }
                        }
                    }

                    // If new RadioButton was selected, deselect RadioButtons of same groupID
                    if ( ( m_controls[i] is RadioButton ) && ( ( m_controls[i] as RadioButton ).NeedToDelectOthers ) )
                    {
                        ( m_controls[i] as RadioButton ).NeedToDelectOthers = false;
                        for ( int j = 0; j < m_controls.Count; j++ )
                        {
                            if ( i == j )
                            {
                                continue;
                            }
                            if ( ( m_controls[j] is RadioButton ) &&
                                ( ( m_controls[j] as RadioButton ).GroupID == ( m_controls[i] as RadioButton ).GroupID ) )
                            {
                                ( m_controls[j] as RadioButton ).Deselect();
                            }
                        }
                    }

                    // We may have moved from a back Control to a 
                    // front control so reset over states
                    for ( int j = 0; j < i; j++ )
                    {
                        if ( m_controls[j].State == Control.ControlState.Over )
                        {
                            m_controls[j].State = Control.ControlState.Normal;
                            break;
                        }
                    }

                    if ( m_controls[i].State == Control.ControlState.Down )
                    {
                        result = true;

                        // Mouse is down over another control so close any open ComboBox
                        if ( ( m_openComboBox != -1 ) && ( m_openComboBox != m_controls[i].ID ) )
                        {
                            // Close the open ComboBox
                            for ( int j = 0; j < m_controls.Count; j++ )
                            {
                                if ( m_controls[j].ID == m_openComboBox )
                                {
                                    ( m_controls[j] as ComboBox ).IsOpen = false;
                                    m_openComboBox = -1;
                                    break;
                                }
                            }
                        }

                        if ( ( m_activeEditBox != -1 ) && ( m_activeEditBox != i ) )
                        {
                            // Release EditBox focus
                            ( m_controls[m_activeEditBox] as EditBox ).ReleaseFocus();
                            m_activeEditBox = -1;
                        }

                        Control c = m_controls[i];
                        // Adjust Z Depths
                        if ( m_controls[i].ZDepth != 1 )
                        {
                            if ( ( m_controls[i] is Panel ) && ( ( m_controls[i] as Panel ).NumControls > 0 ) )
                            {
                                // Control is Panel with Controls inside.
                                // Move Panel and its Controls to the front
                                for ( int j = 0; j < m_controls.Count; j++ )
                                {
                                    if ( m_controls[j].PanelID == m_controls[i].ID )
                                    {
                                        m_controls[j].ZDepth = 1;
                                    }
                                    else if ( m_controls[j].ZDepth < m_controls[i].ZDepth )
                                    {
                                        m_controls[j].ZDepth++;
                                    }
                                }
                                m_controls[i].ZDepth = 1;
                            }
                            else if ( m_controls[i].PanelID != 0 )
                            {
                                // Control is inside a Panel
                                // Move Panel and its Controls to the front
                                for ( int j = 0; j < m_controls.Count; j++ )
                                {
                                    if ( ( ( m_controls[j].PanelID == m_controls[i].PanelID ) ||
                                        ( m_controls[j].ID == m_controls[i].PanelID ) ) && ( i != j ) )
                                    {
                                        m_controls[j].ZDepth = 1;
                                    }
                                    else if ( m_controls[j].ZDepth < m_controls[i].ZDepth )
                                    {
                                        m_controls[j].ZDepth++;
                                    }
                                }
                                m_controls[i].ZDepth = 1;
                            }
                            else
                            {
                                // Control is either an independent Control or a Panel
                                for ( int j = 0; j < m_controls.Count; j++ )
                                {
                                    if ( m_controls[j].ZDepth < m_controls[i].ZDepth )
                                    {
                                        m_controls[j].ZDepth++;
                                    }
                                }
                                m_controls[i].ZDepth = 1;
                            }

                            // Resort the Controls
                            SortControls();
                        }

                        if ( ( c is Panel ) && ( c as Panel ).NumControls > 0 && !( c as Panel ).Locked )
                        {
                            // Panel is being dragged, move its Controls with it
                            for ( int j = m_controls.Count - 1; j >= 0; j-- )
                            {
                                PointF position = m_controls[j].Position;
                                position.X += ( c as Panel ).XOffset;
                                position.Y += ( c as Panel ).YOffset;
                                m_controls[j].Position = position;
                                if ( m_controls[j].ID == c.ID )
                                {
                                    // Reached the Panel
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
                else if ( buttons[0] )
                {
                    // Clicked off a control so close any open ComboBox
                    if ( m_openComboBox != -1 ) 
                    {
                        ComboBox openComboBox = this[m_openComboBox] as ComboBox;
                        // Close the open ComboBox
                        if ( !openComboBox.Contains( cursor ) )
                        {
                            openComboBox.IsOpen = false;
                            m_openComboBox = -1;
                        }
                    }

                    if ( ( m_activeEditBox != -1 ) && ( m_activeEditBox != i ) )
                    {
                        // Release EditBox focus
                        ( m_controls[m_activeEditBox] as EditBox ).ReleaseFocus();
                        m_activeEditBox = -1;
                    }
                }
            }
            m_inCallback = false;
            return result;
        }

        /// <summary>Sorts the controls by Z Depth</summary>
        private void SortControls()
        {
            ControlSorter sorter = new ControlSorter();
            m_controls.Sort( sorter );
        }

        /// <summary>Sets a position for a Control</summary>
        /// <param name="id">Control ID</param>
        /// <param name="position">New position</param>
        public void SetPosition( int id, PointF position )
        {
            Control c = this[id];
            if ( c == null )
            {
                return;
            }
            if ( c is Panel && ( c as Panel ).NumControls > 0 )
            {
                float xOffset = c.Position.X - position.X;
                float yOffset = c.Position.Y - position.Y;
                
                // Move Panel
                c.Position = position;

                // Move Controls of Panel
                for ( int i = 0; i < m_controls.Count; i++ )
                {
                    if ( m_controls[i].PanelID == c.ID )
                    {
                        PointF newPosition = m_controls[i].Position;
                        newPosition.X -= xOffset;
                        newPosition.Y -= yOffset;
                        m_controls[i].Position = newPosition;
                    }
                }
            }
            else
            {
                c.Position = position;
            }
            UpdateBuffer();
        }

        /// <summary>Sets a new slider value.</summary>
        /// <param name="id">Slider ID</param>
        /// <param name="value">New Value</param>
        public void SetSliderValue( int id, float value )
        {
            Control c = this[id];
            if ( c is Slider )
            {
                ( c as Slider ).SetValue( value );
                m_dirtyBuffer = true;
            }
        }

        /// <summary>Disables or enables a Control.</summary>
        /// <param name="id">Control ID</param>
        /// <param name="disabled">true or false</param>
        /// <remarks>If the Control is a Panel, all the Controls in the Panel will also be 
        /// disabled of enabled.</remarks>
        public void DisableControl( int id, bool disabled )
        {
            Control c = this[id];
            if ( c == null )
            {
                return;
            }
            if ( c.PanelID > 0 )
            {
                Control panel = this[c.PanelID];
                if ( panel.Disabled && !disabled )
                {
                    // Control must have same state as its containing Panel
                    return;
                }
            }
            c.Disabled = disabled;
            if ( ( c is Panel ) && ( ( c as Panel ).NumControls > 0 ) )
            {
                // Set all the Controls in the Panel
                for ( int j = 0; j < m_controls.Count; j++ )
                {
                    if ( m_controls[j].PanelID == id )
                    {
                        m_controls[j].Disabled = disabled;
                    }
                }
            }
            BuildQuadList();
            UpdateBuffer();
        }

        /// <summary>Makes sure an id is not currently in use</summary>
        /// <param name="id">ID to check</param>
        private void CheckUniqueID( int id )
        {
            foreach ( Control c in m_controls )
            {
                if ( c.ID == id )
                {
                    throw new Exception( "Control ID: " + id + " is already in use." );
                }
            }
        }

        /// <summary>Gets the Control with the corresponding ID. Returns null if Control wasn't found.</summary>
        public Control this[int i]
        {
            get
            {
                for ( int index = 0; index < m_controls.Count; index++ )
                {
                    if ( i == m_controls[index].ID )
                    {
                        return m_controls[index];
                    }
                }
                return null;
            }
        }

        /// <summary>Gets and sets whether the GUI needs to update its buffer.</summary>
        public bool DirtyBuffer
        {
            get { return m_dirtyBuffer; }
            set { m_dirtyBuffer = value; }
        }

        /// <summary>Search predicate used to find nodes in m_controlNodes</summary>
        /// <param name="node">Current node.</param>
        /// <returns>true if the node's name matches the desired node name, false otherwise.</returns>
        private bool HasSearchName( ControlNode node )
        {
            return ( node.Name == m_searchName );
        }
    }

    /// <summary>A ControlNode of the XML file</summary>
    public class ControlNode
    {
        public string Name;
        public List<ImageNode> Images = new List<ImageNode>();
    }

    /// <summary>An ImageNode of the XML file</summary>
    public class ImageNode
    {
        public string Name;
        public Color Color;
        public RectangleF Rectangle;
    }
}
