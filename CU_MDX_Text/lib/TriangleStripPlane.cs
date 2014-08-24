/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : TriangleStripPlane.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Triangle Strip Plane creator.

Created :  10/26/2005
Modified : 10/26/2005

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
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit
{
    /// <summary>
    /// The TriangleStripPlane is used to create a grid from a single
    /// indexed triangle strip. The grid will lie on the XZ-plane.
    /// </summary>
    public class TriangleStripPlane
    {
        /// <summary>Generate PositionNormalTextured vertices.</summary>
        /// <param name="verticesAlongWidth">Number of vertices along the width</param>
        /// <param name="verticesAlongLength">Number of vertices along the length</param>
        /// <returns>Array of PositionNormalTextured vertices</returns>
        public static PositionNormalTextured[] GeneratePositionNormalTextured( int verticesAlongWidth, int verticesAlongLength )
        {
            if ( verticesAlongLength < 2 || verticesAlongWidth < 2 )
            {
                throw new Exception( "Can't create a strip with the specified dimensions" );
            }

            PositionNormalTextured[] verts = new PositionNormalTextured[verticesAlongWidth * verticesAlongLength];
            for ( int z = 0; z < verticesAlongLength; z++ )
            {
                for ( int x = 0; x < verticesAlongWidth; x++ )
                {
                    // Center the grid in model space
                    float halfWidth = ((float)verticesAlongWidth - 1.0f) / 2.0f;
                    float halfLength = ((float)verticesAlongLength - 1.0f) / 2.0f;
                    PositionNormalTextured vertex = new PositionNormalTextured();
                    vertex.X = (float)x - halfWidth;
                    vertex.Y = 0.0f;
                    vertex.Z = (float)z - halfLength;
                    vertex.Nx = 0.0f;
                    vertex.Ny = 1.0f;
                    vertex.Nz = 0.0f;
                    vertex.U = (float)x / (verticesAlongWidth - 1);
                    vertex.V = (float)z / (verticesAlongLength - 1);
                    verts[z * verticesAlongLength + x] = vertex;
                }
            }
            return verts;
        }

        /// <summary>Generate PositionTextured vertices with custom height values</summary>
        /// <param name="verticesAlongWidth">Number of vertices along the width</param>
        /// <param name="verticesAlongLength">Number of vertices along the length</param>
        /// <param name="height">Height values</param>
        /// <returns>Array of PositionTextured vertices</returns>
        public static PositionTextured[] GeneratePositionTexturedWithHeight( int verticesAlongWidth, int verticesAlongLength, int[] height )
        {
            if ( verticesAlongLength < 2 || verticesAlongWidth < 2 )
            {
                throw new Exception( "Can't create a strip with the specified dimensions" );
            }

            PositionTextured[] verts = new PositionTextured[verticesAlongWidth * verticesAlongLength];
            for ( int z = 0; z < verticesAlongLength; z++ )
            {
                for ( int x = 0; x < verticesAlongWidth; x++ )
                {
                    // Center the grid in model space
                    float halfWidth = ((float)verticesAlongWidth - 1.0f) / 2.0f;
                    float halfLength = ((float)verticesAlongLength - 1.0f) / 2.0f;
                    PositionTextured vertex = new PositionTextured();
                    vertex.X = (float)x - halfWidth;
                    vertex.Y = (float)height[z * verticesAlongLength + x];
                    vertex.Z = (float)z - halfLength;
                    vertex.U = (float)x / (verticesAlongWidth - 1);
                    vertex.V = (float)z / (verticesAlongLength - 1);
                    verts[z * verticesAlongLength + x] = vertex;
                }
            }
            return verts;
        }

        /// <summary>Generate PositionTextured vertices with custom height values</summary>
        /// <param name="verticesAlongWidth">Number of vertices along the width</param>
        /// <param name="verticesAlongLength">Number of vertices along the length</param>
        /// <param name="height">Height values</param>
        /// <returns>Array of PositionTextured vertices</returns>
        public static Vertex.Position2Textured[] GeneratePosition2TexturedWithHeight( int verticesAlongWidth, int verticesAlongLength, int[] height )
        {
            if ( verticesAlongLength < 2 || verticesAlongWidth < 2 )
            {
                throw new Exception( "Can't create a strip with the specified dimensions" );
            }

            Vertex.Position2Textured[] verts = new Vertex.Position2Textured[verticesAlongWidth * verticesAlongLength];
            for ( int z = 0; z < verticesAlongLength; z++ )
            {
                for ( int x = 0; x < verticesAlongWidth; x++ )
                {
                    // Center the grid in model space
                    float halfWidth = ((float)verticesAlongWidth - 1.0f) / 2.0f;
                    float halfLength = ((float)verticesAlongLength - 1.0f) / 2.0f;
                    Vertex.Position2Textured vertex;
                    vertex.X = (float)x - halfWidth;
                    vertex.Y = (float)height[z * verticesAlongLength + x];
                    vertex.Z = (float)z - halfLength;
                    vertex.Tu1 = (float)x / (verticesAlongWidth - 1);
                    vertex.Tv1 = (float)z / (verticesAlongLength - 1);
                    vertex.Tu2 = (float)x / 10.0f;
                    vertex.Tv2 = (float)z / 10.0f;
                    verts[z * verticesAlongLength + x] = vertex;
                }
            }
            return verts;
        }

        /// <summary>Generate the 32-bit indices for a plane.</summary>
        /// <param name="verticesAlongWidth">Number of vertices along the width</param>
        /// <param name="verticesAlongLength">Number of vertices along the length</param>
        /// <returns>32-bit indices for an indexed triangle strip plane</returns>
        public static int[] GenerateIndices32( int verticesAlongWidth, int verticesAlongLength )
        {
            int numIndices = (verticesAlongWidth * 2) * (verticesAlongLength - 1) + (verticesAlongLength - 2);

            int[] indices = new int[numIndices];
            int index = 0;
            for ( int z = 0; z < verticesAlongLength - 1; z++ )
            {
                // Even rows move left to right, odd rows move right to left.
                if ( z % 2 == 0 )
                {
                    // Even row
                    int x;
                    for ( x = 0; x < verticesAlongWidth; x++ )
                    {
                        indices[index++] = x + (z * verticesAlongWidth);
                        indices[index++] = x + (z * verticesAlongWidth) + verticesAlongWidth;
                    }
                    // Insert degenerate vertex if this isn't the last row
                    if ( z != verticesAlongLength - 2)
                    {
                        indices[index++] = --x + (z * verticesAlongWidth);
                    }
                } 
                else
                {
                    // Odd row
                    int x;
                    for ( x = verticesAlongWidth - 1; x >= 0; x-- )
                    {
                        indices[index++] = x + (z * verticesAlongWidth);
                        indices[index++] = x + (z * verticesAlongWidth) + verticesAlongWidth;
                    }
                    // Insert degenerate vertex if this isn't the last row
                    if ( z != verticesAlongLength - 2)
                    {
                        indices[index++] = ++x + (z * verticesAlongWidth);
                    }
                }
            } 
            return indices;
        }
        /// <summary>Generate the 16-bit indices for a plane.</summary>
        /// <param name="verticesAlongWidth">Number of vertices along the width</param>
        /// <param name="verticesAlongLength">Number of vertices along the length</param>
        /// <returns>16-bit indices for an indexed triangle strip plane</returns>
        public static ushort[] GenerateIndices16( int verticesAlongWidth, int verticesAlongLength )
        {
            int numIndices = ( verticesAlongWidth * 2 ) * ( verticesAlongLength - 1 ) + ( verticesAlongLength - 2 );

            ushort[] indices = new ushort[numIndices];
            int index = 0;
            for ( int z = 0; z < verticesAlongLength - 1; z++ )
            {
                // Even rows move left to right, odd rows move right to left.
                if ( z % 2 == 0 )
                {
                    // Even row
                    int x;
                    for ( x = 0; x < verticesAlongWidth; x++ )
                    {
                        indices[index++] = (ushort)(x + ( z * verticesAlongWidth ));
                        indices[index++] = (ushort)(x + ( z * verticesAlongWidth ) + verticesAlongWidth);
                    }
                    // Insert degenerate vertex if this isn't the last row
                    if ( z != verticesAlongLength - 2 )
                    {
                        indices[index++] = (ushort)(--x + ( z * verticesAlongWidth ));
                    }
                }
                else
                {
                    // Odd row
                    int x;
                    for ( x = verticesAlongWidth - 1; x >= 0; x-- )
                    {
                        indices[index++] = (ushort)(x + ( z * verticesAlongWidth ));
                        indices[index++] = (ushort)(x + ( z * verticesAlongWidth ) + verticesAlongWidth);
                    }
                    // Insert degenerate vertex if this isn't the last row
                    if ( z != verticesAlongLength - 2 )
                    {
                        indices[index++] = (ushort)(++x + ( z * verticesAlongWidth ));
                    }
                }
            }
            return indices;
        }
    }
}
