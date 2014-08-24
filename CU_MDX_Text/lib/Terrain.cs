/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Terrain.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Terrain class

Created :  10/25/2005
Modified : 12/20/2005

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
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Generic;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit
{
	/// <summary>Terrain class</summary>
	public class Terrain : WorldTransform
	{
        private int[] m_height;
        private int m_numIndices;
        private VertexBuffer m_vb = null;
        private IndexBuffer m_ib = null;
        private Texture m_textureBase = null;
        private Texture m_textureDetail = null;
        private VertexFormats m_format;

        /// <summary>Create a new Terrain object.</summary>
        /// <param name="device">Direct3D Device</param>
        /// <param name="rawFile">RAW file name</param>
        /// <param name="terrainTexture">Texture file name</param>
		public Terrain( Device device, string rawFile, string terrainTexture )
		{
            // Load texture
            terrainTexture = Utility.GetMediaFile( terrainTexture );
            m_textureBase = new Texture( device, terrainTexture );
            
            m_format = PositionTextured.Format;

            Generate( device, rawFile );
        }

        /// <summary>Create a new Terrain object.</summary>
        /// <param name="device">Direct3D Device</param>
        /// <param name="rawFile">RAW file name</param>
        /// <param name="terrainTexture">Texture file name</param>
        public Terrain( Device device, string rawFile, string baseTexture, string detailTexture )
        {
            // Load textures
            baseTexture = Utility.GetMediaFile( baseTexture );
            ImageInformation baseInfo = Texture.GetImageInformationFromFile( baseTexture );
            m_textureBase = new Texture( device, baseTexture, baseInfo.Width, baseInfo.Height, 0, Usage.None, Format.Dxt1, Pool.Managed, Filter.Linear, Filter.Linear, 0, false, null );
            detailTexture = Utility.GetMediaFile( detailTexture );
            ImageInformation detailInfo = Texture.GetImageInformationFromFile( detailTexture );
            m_textureDetail = new Texture( device, detailTexture, detailInfo.Width, detailInfo.Height, 0, Usage.None, Format.Dxt1, Pool.Managed, Filter.Linear, Filter.Linear, 0, false, null );
            
            m_format = Vertex.Position2Textured.Format;

            Generate( device, rawFile );
        }

        /// <summary>Generates the vertex and index data</summary>
        /// <param name="device">D3D device</param>
        /// <param name="rawFile">.RAW file name</param>
        private void Generate( Device device, string rawFile )
        {
            // Load height map
            rawFile = Utility.GetMediaFile( rawFile );
            Stream stream = File.OpenRead( rawFile );
            long length = stream.Seek( 0, SeekOrigin.End );
            m_height = new int[length];
            stream.Seek( 0, SeekOrigin.Begin );
            for ( int i = 0; i < length; i++ )
            {
                m_height[i] = stream.ReadByte();
            }
            stream.Close();

            // Generate vertices
            // Sqrt limits terrain to square RAW files
            int width = (int)Math.Sqrt( (double)length );
            if ( m_textureDetail == null )
            {
                // Vertices with 1 set of texture coordinates
                PositionTextured[] verts = TriangleStripPlane.GeneratePositionTexturedWithHeight( width, width, m_height );
                m_vb = new VertexBuffer( device, verts.Length * PositionTextured.StrideSize, Usage.WriteOnly,
                    PositionTextured.Format, Pool.Managed, null );
                GraphicsBuffer<PositionTextured> buffer = m_vb.Lock<PositionTextured>( 0, 0, LockFlags.None );
                buffer.Write( verts );
                m_vb.Unlock();
                buffer.Dispose();
            }
            else
            {
                // Vertices with 2 sets of texture coordinates
                System.Diagnostics.Debug.WriteLine( PositionTextured.StrideSize.ToString() );
                System.Diagnostics.Debug.WriteLine( Vertex.Position2Textured.StrideSize.ToString() );
                Vertex.Position2Textured[] verts = TriangleStripPlane.GeneratePosition2TexturedWithHeight( width, width, m_height );
                m_vb = new VertexBuffer( device, verts.Length * Vertex.Position2Textured.StrideSize, Usage.WriteOnly, 
                    Vertex.Position2Textured.Format, Pool.Managed, null );
                GraphicsBuffer<Vertex.Position2Textured> buffer = m_vb.Lock<Vertex.Position2Textured>( 0, 0, LockFlags.None );
                buffer.Write( verts );
                m_vb.Unlock();
                buffer.Dispose();
            }
    
            // Generate indices
            int[] indices = TriangleStripPlane.GenerateIndices32( width, width );
            m_numIndices = indices.Length;
            m_ib = new IndexBuffer( device, indices.Length * sizeof( int ), Usage.WriteOnly, Pool.Managed, false, null );
            GraphicsBuffer<int> indexBuffer = m_ib.Lock<int>( 0, 0, LockFlags.None );
            indexBuffer.Write( indices );
            m_ib.Unlock();
            indexBuffer.Dispose();
        }

        /// <summary>Renders the terrain.</summary>
        /// <param name="device">Direct3D device</param>
        public void Render( Device device )
        {
            device.Transform.World = Transform;
            device.SetTexture( 0, m_textureBase );
            if ( m_textureDetail != null )
            {
                device.SetStreamSource( 0, m_vb, 0, Vertex.Position2Textured.StrideSize );
                device.SetTexture( 1, m_textureDetail );
                device.SetTextureState( 0, TextureStates.ColorArgument1, (int)TextureArgument.Texture );
                device.SetTextureState( 0, TextureStates.ColorOperation, (int)TextureOperation.SelectArg1 );
                device.SetTextureState( 1, TextureStates.ColorArgument1, (int)TextureArgument.Current );
                device.SetTextureState( 1, TextureStates.ColorArgument2, (int)TextureArgument.Texture );
                device.SetTextureState( 1, TextureStates.ColorOperation, (int)TextureOperation.AddSigned );
            }
            else
            {
                device.SetStreamSource( 0, m_vb, 0, PositionTextured.StrideSize );
            }
            device.Indices = m_ib;
            device.VertexFormat = m_format;
            device.DrawIndexedPrimitives( PrimitiveType.TriangleStrip, 0, 0, m_height.Length, 0, m_numIndices - 2);
        }

        /// <summary>Clean up resources</summary>
        public void Dispose()
        {
            if ( m_vb != null )
            {
                m_vb.Dispose();
                m_vb = null;
            }
            if ( m_ib != null )
            {
                m_ib.Dispose();
                m_ib = null;
            }
            if ( m_textureBase != null )
            {
                m_textureBase.Dispose();
                m_textureBase = null;
            }
            if ( m_textureDetail != null )
            {
                m_textureDetail.Dispose();
                m_textureDetail = null;
            }
        }
    }
}
