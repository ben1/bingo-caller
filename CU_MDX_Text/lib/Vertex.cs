/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Vertex.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Custom vertices.

Created :  10/26/2005
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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace CUnit.Vertex
{
    /// <summary>Vertex with Position and two sets of texture coordinates</summary>
    public struct Position2Textured
    {
        public float X;
        public float Y;
        public float Z;
        public float Tu1;
        public float Tv1;
        public float Tu2;
        public float Tv2;
        public static readonly VertexFormats Format = VertexFormats.Position | VertexFormats.Texture2;
        public static readonly VertexElement[] Declarator = new VertexElement[]
        {
            new VertexElement( 0, 0, DeclarationType.Float3, 
                DeclarationMethod.Default, DeclarationUsage.Position, 0 ),
            new VertexElement( 0, 12, DeclarationType.Float2, 
                DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0 ),
            new VertexElement( 0, 20, DeclarationType.Float2, 
                DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0 ),
            VertexElement.VertexDeclarationEnd 
        };
        public static readonly int StrideSize = 
            VertexInformation.GetDeclarationVertexSize( Declarator, 0 );

        /// <summary>Creates a vertex with a position and two texture coordinates.</summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="z">Z position</param>
        /// <param name="u1">First texture coordinate U</param>
        /// <param name="v1">First texture coordinate V</param>
        /// <param name="u2">Second texture coordinate U</param>
        /// <param name="v2">Second texture coordinate V</param>
        public Position2Textured( float x, float y, float z, float u1, float v1, float u2, float v2 )
        {
            X = x;
            Y = y;
            Z = z;
            Tu1 = u1;
            Tv1 = v1;
            Tu2 = u2;
            Tv2 = v2;
        }

        /// <summary>Creates a vertex with a position and two texture coordinates.</summary>
        /// <param name="position">Position</param>
        /// <param name="u1">First texture coordinate U</param>
        /// <param name="v1">First texture coordinate V</param>
        /// <param name="u2">Second texture coordinate U</param>
        /// <param name="v2">Second texture coordinate V</param>
        public Position2Textured( Vector3 position, float u1, float v1, float u2, float v2 )
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;
            Tu1 = u1;
            Tv1 = v1;
            Tu2 = u2;
            Tv2 = v2;
        }

        /// <summary>Gets and sets the position</summary>
        public Vector3 Position
        {
            get
            {
                return new Vector3( X, Y, Z );
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }
    }
}
