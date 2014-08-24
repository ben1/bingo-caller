/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : WorldTransform.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : World matrix wrapper. Allows for translation, rotation, and scale

Created :  10/22/2005
Modified : 10/22/2005

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

namespace CUnit
{
    /// <summary>World matrix wrapper</summary>
    public class WorldTransform
    {
        private Matrix m_translate;
        private Matrix m_rotate;
        private Matrix m_scale;
        float m_rotationX, m_rotationY, m_rotationZ;

        /// <summary>Creates a new WorldTransform</summary>
        public WorldTransform()
        {
            Reset();
        }

      /// <summary>Sets our values based on an existing WorldTransform </summary>
      public void Set(WorldTransform w)
      {
        this.TranslateAbs(w.Position);
        this.RotateAbs(w.XRotation, w.YRotation, w.ZRotation);
        this.ScaleAbs(w.XScale, w.YScale, w.ZScale);
      }

        /// <summary>Reset the matrices to default position.</summary>
        public void Reset()
        {
            m_translate = Matrix.Identity;
            m_rotate = Matrix.Identity;
            m_scale = Matrix.Identity;
            m_rotationX = m_rotationY = m_rotationZ = 0.0f;
        }

        /// <summary>Absolute translation</summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public void TranslateAbs( float x, float y, float z )
        {
            m_translate.M41 = x;
            m_translate.M42 = y;
            m_translate.M43 = z;
        }

        /// <summary>Absolute translation</summary>
        /// <param name="translation">Translations vector</param>
        public void TranslateAbs( Vector3 translation )
        {
            TranslateAbs( translation.X, translation.Y, translation.Z );
        }

        /// <summary>Relative translation</summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public void TranslateRel( float x, float y, float z )
        {
            m_translate.M41 += x;
            m_translate.M42 += y;
            m_translate.M43 += z;
        }

        /// <summary>Relative translation</summary>
        /// <param name="translation">Translations vector</param>
        public void TranslateRel( Vector3 translation )
        {
            TranslateRel( translation.X, translation.Y, translation.Z );
        }

        /// <summary>Absolute rotation</summary>
        /// <param name="x">X radians</param>
        /// <param name="y">Y radians</param>
        /// <param name="z">Z radians</param>
        public void RotateAbs( float x, float y, float z )
        {
            m_rotationX = x;
            m_rotationY = y;
            m_rotationZ = z;
            m_rotate = Matrix.RotationYawPitchRoll( y, x, z );
        }

        /// <summary>Relative rotation</summary>
        /// <param name="x">X radians</param>
        /// <param name="y">Y radians</param>
        /// <param name="z">Z radians</param>
        public void RotateRel( float x, float y, float z )
        {
            m_rotationX += x;
            m_rotationY += y;
            m_rotationZ += z;
            m_rotate = Matrix.RotationYawPitchRoll( m_rotationY, m_rotationX, m_rotationZ );
        }

        /// <summary>Absolute scale.</summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public void ScaleAbs( float x, float y, float z )
        {
            m_scale.M11 = x;
            m_scale.M22 = y;
            m_scale.M33 = z;
        }

        /// <summary>Relative scale</summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public void ScaleRel( float x, float y, float z )
        {
            m_scale.M11 += x;
            m_scale.M22 += y;
            m_scale.M33 += z;
        }

        /// <summary>The combined transformation matrix.</summary>
        public Matrix Transform 
        { 
            get 
            { 
                return  m_scale * m_rotate * m_translate; 
            } 
        }

        /// <summary>Gets and sets the position vector</summary>
        public Vector3 Position
        {
            get { return new Vector3( m_translate.M41, m_translate.M42, m_translate.M43 ); }
            set
            {
                m_translate.M41 = value.X;
                m_translate.M42 = value.Y;
                m_translate.M43 = value.Z;
            }
        }

        /// <summary>Absolute x position.</summary>
        public float XPosition
        {
            get { return m_translate.M41; } 
            set { m_translate.M41 = value; } 
        }

        /// <summary>Absolute y position.</summary>
        public float YPosition
        { 
            get { return m_translate.M42; } 
            set { m_translate.M42 = value; } 
        }

        /// <summary>Absolute z position.</summary>
        public float ZPosition 
        { 
            get { return m_translate.M43; } 
            set { m_translate.M43 = value; } 
        }

        /// <summary>Absolute x rotation.</summary>
        public float XRotation 
        { 
            get { return m_rotationX; } 
            set { RotateAbs( value, m_rotationY, m_rotationZ ); } 
        }

        /// <summary>Absolute y rotation.</summary>
        public float YRotation 
        { 
            get { return m_rotationY; } 
            set { RotateAbs( m_rotationX, value, m_rotationZ ); } 
        }

        /// <summary>Absolute z rotation.</summary>
        public float ZRotation 
        { 
            get { return m_rotationZ; } 
            set { RotateAbs( m_rotationX, m_rotationY, value ); } 
        }

        /// <summary>Absolute x scale.</summary>
        public float XScale 
        { 
            get { return m_scale.M11; } 
            set { m_scale.M11 = value; } 
        }

        /// <summary>Absolute y scale.</summary>
        public float YScale 
        { 
            get { return m_scale.M22; } 
            set { m_scale.M22 = value; } 
        }
        
        /// <summary>Absolute z scale.</summary>
        public float ZScale 
        {
            get { return m_scale.M33; } 
            set { m_scale.M33 = value; } 
        }
    }
}