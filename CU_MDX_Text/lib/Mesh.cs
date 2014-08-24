/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Mesh.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Mesh wrapper

Created :  10/25/2005
Modified : 02/03/2006

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
using Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;

namespace CUnit
{
	/// <summary>Mesh class</summary>
	public class Mesh : IDisposable
	{
        D3D.Material[] m_materials;
        D3D.Texture[] m_textures;
        D3D.Mesh m_mesh;

        /// <summary>Creates a new static mesh</summary>
        /// <param name="device">Direct3D Device</param>
        /// <param name="file">file name</param>
        public Mesh( D3D.Device device, string file )
        {
            file = Utility.GetMediaFile( file );
            GraphicsBuffer outputAdjacency = new GraphicsBuffer();
            D3D.MaterialList materials = new D3D.MaterialList();
            D3D.EffectInstanceList effects = new D3D.EffectInstanceList();
            m_mesh = new D3D.Mesh( device, file, D3D.MeshFlags.Managed, outputAdjacency, materials, effects );
            
            // Not using effects
            effects.Dispose();

            // Add normals if it doesn't have any
            // December SDK Maya exporter doesn't output any VertexFormat anyways so add position and texture also
            if ( ( m_mesh.VertexFormat & D3D.VertexFormats.PositionNormal ) != D3D.VertexFormats.PositionNormal )
            {
                D3D.Mesh tempMesh = m_mesh.Clone( device, m_mesh.Options.Value, m_mesh.VertexFormat | D3D.VertexFormats.PositionNormal | D3D.VertexFormats.Texture1 );
                tempMesh.ComputeNormals();
                m_mesh.Dispose();
                m_mesh = tempMesh;
            }

            // Attribute sort the mesh to enhance Mesh.DrawSubset performance
            m_mesh.GenerateAdjacency( 0.001f, outputAdjacency );
            m_mesh.OptimizeInPlace( Microsoft.DirectX.Direct3D.MeshFlags.OptimizeAttributeSort, outputAdjacency );
            outputAdjacency.Dispose();

            // Extract the material properties and texture names.
            m_textures  = new D3D.Texture[m_mesh.AttributeCount];
            m_materials = new D3D.Material[m_mesh.AttributeCount];
            for ( int i = 0; i < m_mesh.AttributeCount; i++ )
            {
                m_materials[i] = materials[i].Material;
            
                // Set the ambient color for the material. Direct3D
                // does not do this by default.
                m_materials[i].AmbientColor = m_materials[i].DiffuseColor;
            
                // Create the texture.
                if ( materials[i].TextureFileName != null && materials[i].TextureFileName.Length > 0 )
                {
                    string texture = System.IO.Path.GetFileName( materials[i].TextureFileName );
                    texture = Utility.GetMediaFile( texture );
                    D3D.ImageInformation info = D3D.Texture.GetImageInformationFromFile( texture );
                    m_textures[i] = new D3D.Texture( device, texture, info.Width, info.Height, 0, D3D.Usage.None, D3D.Format.Dxt1, D3D.Pool.Managed, D3D.Filter.Linear, D3D.Filter.Linear, 0, false, null );
                    //m_textures[i] = new D3D.Texture( device, texture );
                }
                else
                {
                    m_textures[i] = null;
                }
            }
        }

        /// <summary>Clean up resources</summary>
        public void Dispose()
        {
            if ( m_mesh != null )
            {
                m_mesh.Dispose();
                m_mesh = null;
            }
            m_materials = null;
            if ( m_textures != null )
            {
                for ( int i = 0; i < m_textures.Length; i++ )
                {
                    if ( m_textures[i] != null )
                    {
                        m_textures[i].Dispose();
                        m_textures[i] = null;
                    }
                }
                m_textures = null;
            }
        }

        /// <summary>Gets the source Microsoft.DirectX.Direct3D.Mesh</summary>
        public D3D.Mesh SourceMesh
        {
            get { return m_mesh; }
        }

        /// <summary>Gets the Mesh's Materials.</summary>
        public D3D.Material[] Materials
        {
            get { return m_materials; }
        }

        /// <summary>Gets the Mesh's Textures.</summary>
        public D3D.Texture[] Textures
        {
            get { return m_textures; }
        }
    }

    /// <summary>Instance of a mesh.</summary>
    public class MeshInstance : WorldTransform
    {
        public Mesh m_mesh = null;
        private D3D.BoundingSphere m_boundingSphere;

        /// <summary>Creates anew MeshInstance</summary>
        /// <param name="mesh">Mesh to reference</param>
        public MeshInstance( Mesh mesh )
        {
            m_mesh = mesh;

            // Compute bounding sphere
            using ( D3D.VertexBuffer buffer = m_mesh.SourceMesh.VertexBuffer )
            {
                GraphicsBuffer graphicsBuffer = buffer.Lock( 0, 0, D3D.LockFlags.None );
                m_boundingSphere = D3D.Geometry.ComputeBoundingSphere( graphicsBuffer, m_mesh.SourceMesh.VertexCount, m_mesh.SourceMesh.VertexFormat );
                buffer.Unlock();
            }
        }

        /// <summary>Render the mesh.</summary>
        /// <param name="device">Direct3D device</param>
        public void Render( D3D.Device device )
        {
            if ( device == null || m_mesh == null )
            {
                return;
            }
            device.Transform.World = Transform;
            device.VertexFormat = SourceMesh.VertexFormat;
            for ( int i = 0; i < m_mesh.Materials.Length; i++ )
            {
                // Set the material and texture for this subset.
                device.Material = m_mesh.Materials[i];
                device.SetTexture( 0, m_mesh.Textures[i] );
        
                // Draw the mesh subset.
                SourceMesh.DrawSubset( i );
            }
        }

        /// <summary>Clean up resources.</summary>
        public void Dispose()
        {
            // m_mesh is disposed in CUnit.Mesh
            m_mesh = null;
        }

        /// <summary>Gets the referenced Microsoft.DirectX.Direct3D.Mesh</summary>
        public D3D.Mesh SourceMesh
        {
            get { return m_mesh.SourceMesh; }
        }

        /// <summary>Gets and sets the bounding radius of the mesh.</summary>
        public float Radius
        {
            get { return m_boundingSphere.Radius; }
            set { m_boundingSphere.Radius = Math.Abs( value ); }
        }
    }
}
