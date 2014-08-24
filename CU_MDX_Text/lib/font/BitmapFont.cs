/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : BitmapFont.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Bitmap font wrapper based on the Angelcode bitmap font generator.
 * http://www.angelcode.com/products/bmfont/

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
using System.IO;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Generic;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit
{
    /// <summary>Bitmap font wrapper.</summary>
    public class BitmapFont
    {
        public enum Align { Left, Center, Right };
        private BitmapCharacterSet m_charSet;
        private List<FontQuad> m_quads;
        private List<StringBlock> m_strings;
        private string m_fntFile;
        private string m_textureFile;
        private Texture m_texture = null;
        private VertexBuffer m_vb = null;
        private const int MaxVertices = 4096;
        private int m_nextChar;

        /// <summary>Creates a new bitmap font.</summary>
        /// <param name="faceName">Font face name.</param>
        public BitmapFont( string fntFile, string textureFile )
        {
            m_quads = new List<FontQuad>();
            m_strings = new List<StringBlock>();
            m_fntFile = fntFile;
            m_textureFile = textureFile;
            m_charSet = new BitmapCharacterSet();
            ParseFNTFile();
        }

        /// <summary>Parses the FNT file.</summary>
        private void ParseFNTFile()
        {
            string fntFile = Utility.GetMediaFile( m_fntFile );
            StreamReader stream = new StreamReader( fntFile );
            string line;
            char[] separators = new char[] { ' ', '=' };
            while ( ( line = stream.ReadLine() ) != null )
            {
                string[] tokens = line.Split( separators );
                if ( tokens[0] == "info" )
                {
                    // Get rendered size
                    for ( int i = 1; i < tokens.Length; i++ )
                    {
                        if ( tokens[i] == "size" )
                        {
                            m_charSet.RenderedSize = int.Parse( tokens[i + 1] );
                        }
                    }
                }
                else if ( tokens[0] == "common" )
                {
                    // Fill out BitmapCharacterSet fields
                    for ( int i = 1; i < tokens.Length; i++ )
                    {
                        if ( tokens[i] == "lineHeight" )
                        {
                            m_charSet.LineHeight = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "base" )
                        {
                            m_charSet.Base = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "scaleW" )
                        {
                            m_charSet.Width = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "scaleH" )
                        {
                            m_charSet.Height = int.Parse( tokens[i + 1] );
                        }
                    }
                }
                else if ( tokens[0] == "char" )
                {
                    // New BitmapCharacter
                    int index = 0;
                    for ( int i = 1; i < tokens.Length; i++ )
                    {
                        if ( tokens[i] == "id" )
                        {
                            index = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "x" )
                        {
                            m_charSet.Characters[index].X = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "y" )
                        {
                            m_charSet.Characters[index].Y = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "width" )
                        {
                            m_charSet.Characters[index].Width = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "height" )
                        {
                            m_charSet.Characters[index].Height = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "xoffset" )
                        {
                            m_charSet.Characters[index].XOffset = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "yoffset" )
                        {
                            m_charSet.Characters[index].YOffset = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "xadvance" )
                        {
                            m_charSet.Characters[index].XAdvance = int.Parse( tokens[i + 1] );
                        }
                    }
                }
                else if ( tokens[0] == "kerning" )
                {
                    // Build kerning list
                    int index = 0;
                    Kerning k = new Kerning();
                    for ( int i = 1; i < tokens.Length; i++ )
                    {
                        if ( tokens[i] == "first" )
                        {
                            index = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "second" )
                        {
                            k.Second = int.Parse( tokens[i + 1] );
                        }
                        else if ( tokens[i] == "amount" )
                        {
                            k.Amount = int.Parse( tokens[i + 1] );
                        }
                    }
                    m_charSet.Characters[index].KerningList.Add( k );
                }
            }
            stream.Close();
        }

        /// <summary>Call when the device is created.</summary>
        /// <param name="device">D3D device.</param>
        public void OnCreateDevice( Device device )
        {
            m_texture = new Texture( device, Utility.GetMediaFile( m_textureFile ),
                m_charSet.Width, m_charSet.Height, 0, Usage.None, Format.Dxt3, Pool.Managed, 
                Filter.Linear, Filter.Linear, 0, false, null );
        }

        /// <summary>Call when the device is destroyed.</summary>
        public void OnDestroyDevice()
        {
            if ( m_texture != null )
            {
                m_texture.Dispose();
                m_texture = null;
            }
        }

        /// <summary>Call when the device is reset.</summary>
        /// <param name="device">D3D device.</param>
        public void OnResetDevice( Device device )
        {
            m_vb = new VertexBuffer( device, MaxVertices * TransformedColoredTextured.StrideSize,
                Usage.Dynamic | Usage.WriteOnly, TransformedColoredTextured.Format, 
                Pool.Default, null );
        }

        /// <summary>Call when the device is lost.</summary>
        public void OnLostDevice()
        {
            if ( m_vb != null )
            {
                m_vb.Dispose();
                m_vb = null;
            }
        }

        /// <summary>Adds a new string to the list to render.</summary>
        /// <param name="text">Text to render</param>
        /// <param name="textBox">Rectangle to constrain text</param>
        /// <param name="alignment">Font alignment</param>
        /// <param name="size">Font size</param>
        /// <param name="color">Color</param>
        /// <param name="kerning">true to use kerning, false otherwise.</param>
        /// <returns>The index of the added StringBlock</returns>
        public int AddString( string text, RectangleF textBox, Align alignment, float size, 
            ColorValue color, bool kerning )
        {
            StringBlock b = new StringBlock( text, textBox, alignment, size, color, kerning );
            m_strings.Add( b );
            int index = m_strings.Count - 1;
            m_quads.AddRange( GetProcessedQuads( index ) );
            return index;
        }

        /// <summary>Removes a string from the list of strings.</summary>
        /// <param name="i">Index to remove</param>
        public void ClearString( int i )
        {
            m_strings.RemoveAt( i );
        }

        /// <summary>Clears the list of strings</summary>
        public void ClearStrings()
        {
            m_strings.Clear();
            m_quads.Clear();
        }

        /// <summary>Renders the strings.</summary>
        /// <param name="device">D3D Device</param>
        public void Render( Device device )
        {
            if ( m_strings.Count <= 0 )
            {
                return;
            }
            
            // Add vertices to the buffer
            GraphicsBuffer<TransformedColoredTextured> gb = 
                m_vb.Lock<TransformedColoredTextured>( 0, 6 * m_quads.Count, LockFlags.Discard );

            foreach ( FontQuad q in m_quads )
            {
                gb.Write( q.Vertices );
            }

            m_vb.Unlock();

            // Set render states
            device.SetRenderState( RenderStates.ZEnable, false );
            device.SetRenderState( RenderStates.FillMode, (int)FillMode.Solid );
            device.SetRenderState( RenderStates.ZBufferWriteEnable, false );
            device.SetRenderState( RenderStates.FogEnable, false );
            device.SetRenderState( RenderStates.AlphaTestEnable, false );
            device.SetRenderState( RenderStates.AlphaBlendEnable, true );
            device.SetRenderState( RenderStates.SourceBlend, (int)Blend.SourceAlpha );
            device.SetRenderState( RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha );

            // Blend Texture and Vertex alphas
            device.SetTextureState( 0, TextureStates.ColorArgument1, (int)TextureArgument.Current );
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
            device.DrawPrimitives( PrimitiveType.TriangleList, 0, 2 * m_quads.Count );
        }

        /// <summary>Gets the list of Quads from a StringBlock all ready to render.</summary>
        /// <param name="index">Index into StringBlock List</param>
        /// <returns>List of Quads</returns>
        public List<FontQuad> GetProcessedQuads( int index )
        {
            if ( index >= m_strings.Count || index < 0 )
            {
                throw new Exception( "String block index out of range." );
            }

            List<FontQuad> quads = new List<FontQuad>();
            StringBlock b = m_strings[index];
            string text = b.Text;
            float x = b.TextBox.X;
            float y = b.TextBox.Y;
            float maxWidth = b.TextBox.Width;
            Align alignment = b.Alignment;
            float lineWidth = 0f;
            float sizeScale = b.Size / (float)m_charSet.RenderedSize;
            char lastChar = new char();
            int lineNumber = 1;
            int wordNumber = 1;
            float wordWidth = 0f;
            bool firstCharOfLine = true;

            float z = 0f;
            float rhw = 1f;

            for ( int i = 0; i < text.Length; i++ )
            {
                BitmapCharacter c = m_charSet.Characters[text[i]];
                float xOffset = c.XOffset * sizeScale;
                float yOffset = c.YOffset * sizeScale;
                float xAdvance = c.XAdvance * sizeScale;
                float width = c.Width * sizeScale;
                float height = c.Height * sizeScale;

                // Check vertical bounds
                if ( y + yOffset + height > b.TextBox.Bottom )
                {
                    break;
                }

                // Newline
                if ( text[i] == '\n' || text[i] == '\r' || ( lineWidth + xAdvance >= maxWidth ) )
                {
                    if ( alignment == Align.Left )
                    {
                        // Start at left
                        x = b.TextBox.X;
                    }
                    if ( alignment == Align.Center )
                    {
                        // Start in center
                        x = b.TextBox.X + ( maxWidth / 2f );
                    }
                    else if ( alignment == Align.Right )
                    {
                        // Start at right
                        x = b.TextBox.Right;
                    }

                    y += m_charSet.LineHeight * sizeScale;
                    float offset = 0f;

                    if ( ( lineWidth + xAdvance >= maxWidth ) && ( wordNumber != 1 ) )
                    {
                        // Next character extends past text box width
                        // We have to move the last word down one line
                        char newLineLastChar = new char();
                        lineWidth = 0f;
                        for ( int j = 0; j < quads.Count; j++ )
                        {
                            if ( alignment == Align.Left )
                            {
                                // Move current word to the left side of the text box
                                if ( ( quads[j].LineNumber == lineNumber ) &&
                                    ( quads[j].WordNumber == wordNumber ) )
                                {
                                    quads[j].LineNumber++;
                                    quads[j].WordNumber = 1;
                                    quads[j].X = x + ( quads[j].BitmapCharacter.XOffset * sizeScale );
                                    quads[j].Y = y + ( quads[j].BitmapCharacter.YOffset * sizeScale );
                                    x += quads[j].BitmapCharacter.XAdvance * sizeScale;
                                    lineWidth += quads[j].BitmapCharacter.XAdvance * sizeScale;
                                    if ( b.Kerning )
                                    {
                                        m_nextChar = quads[j].Character;
                                        Kerning kern = m_charSet.Characters[newLineLastChar].KerningList.Find( FindKerningNode );
                                        if ( kern != null )
                                        {
                                            x += kern.Amount * sizeScale;
                                            lineWidth += kern.Amount * sizeScale;
                                        }
                                    }
                                }
                            }
                            else if ( alignment == Align.Center )
                            {
                                if ( ( quads[j].LineNumber == lineNumber ) &&
                                    ( quads[j].WordNumber == wordNumber ) )
                                {
                                    // First move word down to next line
                                    quads[j].LineNumber++;
                                    quads[j].WordNumber = 1;
                                    quads[j].X = x + ( quads[j].BitmapCharacter.XOffset * sizeScale );
                                    quads[j].Y = y + ( quads[j].BitmapCharacter.YOffset * sizeScale );
                                    x += quads[j].BitmapCharacter.XAdvance * sizeScale;
                                    lineWidth += quads[j].BitmapCharacter.XAdvance * sizeScale;
                                    offset += quads[j].BitmapCharacter.XAdvance * sizeScale / 2f;
                                    float kerning = 0f;
                                    if ( b.Kerning )
                                    {
                                        m_nextChar = quads[j].Character;
                                        Kerning kern = m_charSet.Characters[newLineLastChar].KerningList.Find( FindKerningNode );
                                        if ( kern != null )
                                        {
                                            kerning = kern.Amount * sizeScale;
                                            x += kerning;
                                            lineWidth += kerning;
                                            offset += kerning / 2f;
                                        }
                                    }
                                }
                            }
                            else if ( alignment == Align.Right )
                            {
                                if ( ( quads[j].LineNumber == lineNumber ) &&
                                    ( quads[j].WordNumber == wordNumber ) )
                                {
                                    // Move character down to next line
                                    quads[j].LineNumber++;
                                    quads[j].WordNumber = 1;
                                    quads[j].X = x + ( quads[j].BitmapCharacter.XOffset * sizeScale );
                                    quads[j].Y = y + ( quads[j].BitmapCharacter.YOffset * sizeScale );
                                    lineWidth += quads[j].BitmapCharacter.XAdvance * sizeScale;
                                    x += quads[j].BitmapCharacter.XAdvance * sizeScale;
                                    offset += quads[j].BitmapCharacter.XAdvance * sizeScale;
                                    float kerning = 0f;
                                    if ( b.Kerning )
                                    {
                                        m_nextChar = quads[j].Character;
                                        Kerning kern = m_charSet.Characters[newLineLastChar].KerningList.Find( FindKerningNode );
                                        if ( kern != null )
                                        {
                                            kerning = kern.Amount * sizeScale;
                                            x += kerning;
                                            lineWidth += kerning;
                                            offset += kerning;
                                        }
                                    }
                                }
                            }
                            newLineLastChar = quads[j].Character;
                        }

                        // Make post-newline justifications
                        if ( alignment == Align.Center || alignment == Align.Right )
                        {
                            // Justify the new line
                            for ( int k = 0; k < quads.Count; k++ )
                            {
                                if ( quads[k].LineNumber == lineNumber + 1 )
                                {
                                    quads[k].X -= offset;
                                }
                            }
                            x -= offset;

                            // Rejustify the line it was moved from
                            for ( int k = 0; k < quads.Count; k++ )
                            {
                                if ( quads[k].LineNumber == lineNumber )
                                {
                                    quads[k].X += offset;
                                }
                            }
                        }
                    }
                    else
                    {
                        // New line without any "carry-down" word
                        firstCharOfLine = true;
                        lineWidth = 0f;
                    }

                    wordNumber = 1;
                    lineNumber++;
                    
                } // End new line check

                // Don't print these
                if ( text[i] == '\n' || text[i] == '\r' || text[i] == '\t' )
                {
                    continue;
                }

                // Set starting cursor for alignment
                if ( firstCharOfLine ) 
                {
                    if ( alignment == Align.Left )
                    {
                        // Start at left
                        x = b.TextBox.Left;
                    }
                    if ( alignment == Align.Center )
                    {
                        // Start in center
                        x = b.TextBox.Left + ( maxWidth / 2f );
                    }
                    else if ( alignment == Align.Right )
                    {
                        // Start at right
                        x = b.TextBox.Right;
                    }
                }

                // Adjust for kerning
                float kernAmount = 0f;
                if ( b.Kerning && !firstCharOfLine )
                {
                    m_nextChar = (char)text[i];
                    Kerning kern = m_charSet.Characters[lastChar].KerningList.Find( FindKerningNode );
                    if ( kern != null )
                    {
                        kernAmount = kern.Amount * sizeScale;
                        x += kernAmount;
                        lineWidth += kernAmount;
                        wordWidth += kernAmount;
                    }
                }

                firstCharOfLine = false;

                // Create the vertices
                TransformedColoredTextured topLeft = new TransformedColoredTextured(
                    x + xOffset, y + yOffset, z, rhw, b.Color.ToArgb(), 
                    (float)c.X / (float)m_charSet.Width,
                    (float)c.Y / (float)m_charSet.Height );
                TransformedColoredTextured topRight = new TransformedColoredTextured(
                    topLeft.X + width, y + yOffset, z, rhw, b.Color.ToArgb(), 
                    (float)( c.X + c.Width ) / (float)m_charSet.Width,
                    (float)c.Y / (float)m_charSet.Height );
                TransformedColoredTextured bottomRight = new TransformedColoredTextured(
                    topLeft.X + width, topLeft.Y + height, z, rhw, b.Color.ToArgb(), 
                    (float)( c.X + c.Width ) / (float)m_charSet.Width,
                    (float)( c.Y + c.Height ) / (float)m_charSet.Height );
                TransformedColoredTextured bottomLeft = new TransformedColoredTextured(
                    x + xOffset, topLeft.Y + height, z, rhw, b.Color.ToArgb(), 
                    (float)c.X / (float)m_charSet.Width,
                    (float)( c.Y + c.Height ) / (float)m_charSet.Height );

                // Create the quad
                FontQuad q = new FontQuad( topLeft, topRight, bottomLeft, bottomRight );
                q.LineNumber = lineNumber;
                if ( text[i] == ' ' && alignment == Align.Right )
                {
                    wordNumber++;
                    wordWidth = 0f;
                }
                q.WordNumber = wordNumber;
                wordWidth += xAdvance;
                q.WordWidth = wordWidth;
                q.BitmapCharacter = c;
                q.SizeScale = sizeScale;
                q.Character = text[i];
                quads.Add( q );

                if ( text[i] == ' ' && alignment == Align.Left )
                {
                    wordNumber++;
                    wordWidth = 0f;
                }

                x += xAdvance;
                lineWidth += xAdvance;
                lastChar = text[i];

                // Rejustify text
                if ( alignment == Align.Center )
                {
                    // We have to recenter all Quads since we addded a 
                    // new character
                    float offset = xAdvance / 2f;
                    if ( b.Kerning )
                    {
                        offset += kernAmount / 2f;
                    }
                    for ( int j = 0; j < quads.Count; j++ )
                    {
                        if ( quads[j].LineNumber == lineNumber )
                        {
                            quads[j].X -= offset;
                        }
                    }
                    x -= offset;
                }
                else if ( alignment == Align.Right )
                {
                    // We have to rejustify all Quads since we addded a 
                    // new character
                    float offset = 0f;
                    if ( b.Kerning )
                    {
                        offset += kernAmount;
                    }
                    for ( int j = 0; j < quads.Count; j++ )
                    {
                        if ( quads[j].LineNumber == lineNumber )
                        {
                            offset = xAdvance;
                            quads[j].X -= xAdvance;
                        }
                    }
                    x -= offset;
                }
            }
            return quads;
        }

        /// <summary>Gets the line height of a StringBlock.</summary>
        public float GetLineHeight( int index )
        {
            if ( index < 0 || index > m_strings.Count )
            {
                throw new Exception( "StringBlock index out of range." );
            }
            return m_charSet.LineHeight * ( m_strings[index].Size / m_charSet.RenderedSize );
        }

        /// <summary>Search predicate used to find nodes in m_kerningList</summary>
        /// <param name="node">Current node.</param>
        /// <returns>true if the node's name matches the desired node name, false otherwise.</returns>
        private bool FindKerningNode( Kerning node )
        {
            return ( node.Second == m_nextChar );
        }

        /// <summary>Gets the font texture.</summary>
        public Texture Texture
        {
            get { return m_texture; }
        }
    }

    /// <summary>Represents a single bitmap character set.</summary>
    class BitmapCharacterSet
    {
        public int LineHeight;
        public int Base;
        public int RenderedSize;
        public int Width;
        public int Height;
        public BitmapCharacter[] Characters;

        /// <summary>Creates a new BitmapCharacterSet</summary>
        public BitmapCharacterSet()
        {
            Characters = new BitmapCharacter[256];
            for ( int i = 0; i < 256; i++ )
            {
                Characters[i] = new BitmapCharacter();
            }
        }
    }

    /// <summary>Represents a single bitmap character.</summary>
    public class BitmapCharacter : ICloneable
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int XOffset;
        public int YOffset;
        public int XAdvance;
        public List<Kerning> KerningList = new List<Kerning>();

        /// <summary>Clones the BitmapCharacter</summary>
        /// <returns>Cloned BitmapCharacter</returns>
        public object Clone()
        {
            BitmapCharacter result = new BitmapCharacter();
            result.X = X;
            result.Y = Y;
            result.Width = Width;
            result.Height = Height;
            result.XOffset = XOffset;
            result.YOffset = YOffset;
            result.XAdvance = XAdvance;
            result.KerningList.AddRange( KerningList );
            return result;
        }
    }

    /// <summary>Represents kerning information for a character.</summary>
    public class Kerning
    {
        public int Second;
        public int Amount;
    }

    /// <summary>Individual string to load into vertex buffer.</summary>
    struct StringBlock
    {
        public string Text;
        public RectangleF TextBox;
        public BitmapFont.Align Alignment;
        public float Size;
        public ColorValue Color;
        public bool Kerning;

        /// <summary>Creates a new StringBlock</summary>
        /// <param name="text">Text to render</param>
        /// <param name="textBox">Text box to constrain text</param>
        /// <param name="alignment">Font alignment</param>
        /// <param name="size">Font size</param>
        /// <param name="color">Color</param>
        /// <param name="kerning">true to use kerning, false otherwise.</param>
        public StringBlock( string text, RectangleF textBox, BitmapFont.Align alignment, 
            float size, ColorValue color, bool kerning )
        {
            Text = text;
            TextBox = textBox;
            Alignment = alignment;
            Size = size;
            Color = color;
            Kerning = kerning;
        }
    }
}
