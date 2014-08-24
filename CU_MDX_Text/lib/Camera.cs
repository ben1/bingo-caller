/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Camera.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Camera classes.

Created :  10/25/2005
Modified : 10/25/2005

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
	/// <summary>First person camera</summary>
	public class Camera
	{
        private Matrix  m_view;      
        private Matrix  m_projection;
        private Vector3 m_right;     
        private Vector3 m_up;        
        private Vector3 m_look;      
        private Vector3 m_position;  
        private Vector3 m_lookAt;    
        private Vector3 m_velocity;  
        private Plane[] m_frustum;
        private float   m_yaw;       
        private float   m_pitch;     
        private float   m_maxPitch;
        private float   m_maxVelocity;
        private float   m_fov;       
        private float   m_aspect;    
        private float   m_nearPlane; 
        private float   m_farPlane;  
        private bool    m_invertY;
        private bool    m_enableYMovement;

        /// <summary>Creates a new Camera</summary>
		public Camera()
		{
            m_frustum = new Plane[6];
            m_maxPitch = Geometry.DegreeToRadian( 89.0f );
            m_maxVelocity = 1.0f;
            m_invertY = false;
            m_enableYMovement = true;
            m_position = new Vector3();
            m_velocity = new Vector3();
            m_look = new Vector3( 0.0f, 0.0f, 1.0f );
            CreateProjectionMatrix( (float)Math.PI / 3.0f, 1.3f, 1.0f, 1000.0f );
            Update();
		}

        /// <summary>Creates the projection matrix.</summary>
        /// <param name="fov">Field of view</param>
        /// <param name="aspect">Aspect ratio</param>
        /// <param name="near">Near plane</param>
        /// <param name="far">Far plane</param>
        private void CreateProjectionMatrix( float fov, float aspect, float near, float far )
        {
            m_fov = fov;
            m_aspect = aspect;
            m_nearPlane = near;
            m_farPlane = far;
            m_projection = Matrix.PerspectiveFieldOfViewLeftHanded( m_fov, m_aspect, m_nearPlane, m_farPlane );
        }

        /// <summary>Moves the camera forward and backward.</summary>
        /// <param name="units">Amount to move</param>
        public void MoveForward( float units )
        {
            if ( m_enableYMovement )
            {
                m_velocity += m_look * units;
            }
            else
            {
                Vector3 moveVector = new Vector3( m_look.X, 0.0f, m_look.Z );
                moveVector.Normalize();
                moveVector *= units;
                m_velocity += moveVector;
            }
        }

        /// <summary>Moves the camera left and right.</summary>
        /// <param name="units">Amount to move</param>
        public void Strafe( float units )
        {
            m_velocity += m_right * units;
        }

        /// <summary>Moves the camera up and down.</summary>
        /// <param name="units">Amount to move.</param>
        public void MoveUp( float units )
        {
            m_velocity.Y += units;
        }

        /// <summary>Yaw the camera around its Y-axis.</summary>
        /// <param name="radians">Radians to yaw.</param>
        public void Yaw( float radians )
        {
            if ( radians == 0.0f )
            {
                // Don't bother
                return;
            }
            Matrix rotation = Matrix.RotationAxis( m_up, radians );
            m_right = Vector3.TransformNormal( m_right, rotation );
            m_look = Vector3.TransformNormal( m_look, rotation );
        }

        /// <summary>Pitch the camera around its X-axis.</summary>
        /// <param name="radians">Radians to pitch.</param>
        public void Pitch( float radians )
        {
            if ( radians == 0.0f )
            {
                // Don't bother
                return;
            }

            radians = (m_invertY) ? -radians : radians;
            m_pitch -= radians;

            if ( m_pitch > m_maxPitch )
            {
                radians += m_pitch - m_maxPitch;
            }
            else if ( m_pitch < -m_maxPitch )
            {
                radians += m_pitch + m_maxPitch;
            }

            Matrix rotation = Matrix.RotationAxis( m_right, radians );
            m_up = Vector3.TransformNormal( m_up, rotation );
            m_look = Vector3.TransformNormal( m_look, rotation );
        }
        
        /// <summary>Roll the camera around its Z-axis.</summary>
        /// <param name="radians">Radians to roll.</param>
        public void Roll( float radians )
        {
            if ( radians == 0.0f )
            {
                // Don't bother
                return;
            }
            Matrix rotation = Matrix.RotationAxis( m_look, radians );
            m_right = Vector3.TransformNormal( m_right, rotation );
            m_up = Vector3.TransformNormal( m_up, rotation );
        }

        /// <summary>Updates the camera and creates a new view matrix.</summary>
        public void Update()
        {
            // Cap velocity to max velocity
            if ( Vector3.Length( m_velocity ) > m_maxVelocity )
            {
                m_velocity = Vector3.Normalize( m_velocity ) * m_maxVelocity;
            }

            // Move the camera
            m_position += m_velocity;
            // Could decelerate here. I'll just stop completely.
            m_velocity = new Vector3();
            m_lookAt = m_position + m_look;

            // Calculate the new view matrix
            Vector3 up = new Vector3( 0.0f, 1.0f, 0.0f );
            m_view = Matrix.LookAtLeftHanded( Position, LookAt, up );

            // Calculate new view frustum
            BuildViewFrustum();

            // Set the camera axes from the view matrix
            m_right.X = m_view.M11;  
            m_right.Y = m_view.M21;  
            m_right.Z = m_view.M31;  
            m_up.X = m_view.M12;
            m_up.Y = m_view.M22;
            m_up.Z = m_view.M32;
            m_look.X = m_view.M13;
            m_look.Y = m_view.M23;
            m_look.Z = m_view.M33;

            // Calculate yaw and pitch
            float lookLengthOnXZ = (float)Math.Sqrt( m_look.Z * m_look.Z + m_look.X * m_look.X );
            m_pitch = (float)Math.Atan2( m_look.Y, lookLengthOnXZ );
            m_yaw   = (float)Math.Atan2( m_look.X, m_look.Z );
        }

        /// <summary>
        /// Build the view frustum planes using the current view/projection matrices
        /// </summary>
        public void BuildViewFrustum()
        {
            Matrix viewProjection = m_view * m_projection;
            
            // Left plane
            m_frustum[0].A = viewProjection.M14 + viewProjection.M11;
            m_frustum[0].B = viewProjection.M24 + viewProjection.M21;
            m_frustum[0].C = viewProjection.M34 + viewProjection.M31;
            m_frustum[0].D = viewProjection.M44 + viewProjection.M41;

            // Right plane
            m_frustum[1].A = viewProjection.M14 - viewProjection.M11;   
            m_frustum[1].B = viewProjection.M24 - viewProjection.M21;
            m_frustum[1].C = viewProjection.M34 - viewProjection.M31;
            m_frustum[1].D = viewProjection.M44 - viewProjection.M41;

            // Top plane
            m_frustum[2].A = viewProjection.M14 - viewProjection.M12;   
            m_frustum[2].B = viewProjection.M24 - viewProjection.M22;
            m_frustum[2].C = viewProjection.M34 - viewProjection.M32;
            m_frustum[2].D = viewProjection.M44 - viewProjection.M42;

            // Bottom plane
            m_frustum[3].A = viewProjection.M14 + viewProjection.M12;   
            m_frustum[3].B = viewProjection.M24 + viewProjection.M22;
            m_frustum[3].C = viewProjection.M34 + viewProjection.M32;
            m_frustum[3].D = viewProjection.M44 + viewProjection.M42;

            // Near plane
            m_frustum[4].A = viewProjection.M13;   
            m_frustum[4].B = viewProjection.M23;
            m_frustum[4].C = viewProjection.M33;
            m_frustum[4].D = viewProjection.M43;

            // Far plane
            m_frustum[5].A = viewProjection.M14 - viewProjection.M13;   
            m_frustum[5].B = viewProjection.M24 - viewProjection.M23;
            m_frustum[5].C = viewProjection.M34 - viewProjection.M33;
            m_frustum[5].D = viewProjection.M44 - viewProjection.M43;

            // Normalize planes
            for ( int i = 0; i < 6; i++ )
            {
                m_frustum[i] = Plane.Normalize( m_frustum[i] );
            }
        }

        /// <summary>Checks whether a sphere is inside the camera's view frustum.</summary>
        /// <param name="position">Position of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        /// <returns>true if the sphere is in the frustum, false otherwise</returns>
        public bool SphereInFrustum( Vector3 position, float radius )
        {
            Vector4 position4 = new Vector4( position.X, position.Y, position.Z, 1f );
            for ( int i = 0; i < 6; i++ )
            {
                if ( Plane.Dot( m_frustum[i], position4 ) + radius < 0 )
                {
                    // Outside the frustum, reject it!
                    return false;
                }
            }
            return true;
        }

        /// <summary>Gets the view matrix</summary>
        public Matrix View
        {
            get { return m_view; }
        }

        /// <summary>Gets the projection matrix</summary>
        public Matrix Projection
        {
            get { return m_projection; }
        }

        /// <summary>Gets and sets the position of the camera</summary>
        public Vector3 Position
        {
            get { return m_position; }
            set { m_position = value; }
        }

        /// <summary>Gets and sets the position of the camera</summary>
        public Vector3 LookAt
        {
            get { return m_lookAt; }
            set
            {
                m_lookAt = value;
                m_look = Vector3.Normalize( m_lookAt - m_position );
            }
        }

        /// <summary>Gets and sets the field of view</summary>
        public float FOV
        {
            get { return m_fov; }
            set { CreateProjectionMatrix( value, m_aspect, m_nearPlane, m_farPlane ); }
        }

        /// <summary>Gets and sets the aspect ratio</summary>
        public float AspectRatio
        {
            get { return m_aspect; }
            set { CreateProjectionMatrix( m_fov, value, m_nearPlane, m_farPlane ); }
        }

        /// <summary>Gets and sets the near plane</summary>
        public float NearPlane
        {
            get { return m_nearPlane; }
            set { CreateProjectionMatrix( m_fov, m_aspect, value, m_farPlane ); }
        }

        /// <summary>Gets and sets the far plane </summary>
        public float FarPlane
        {
            get { return m_farPlane; }
            set { CreateProjectionMatrix( m_fov, m_aspect, m_nearPlane, value ); }
        }

        /// <summary>Gets and sets the maximum camera velocity</summary>
        public float MaxVelocity
        {
            get { return m_maxVelocity; }
            set { m_maxVelocity = value; }
        }

        /// <summary>Gets and sets whether the y-axis is inverted.</summary>
        public bool InvertY
        {
            get { return m_invertY; }
            set { m_invertY = value; }
        }

        /// <summary>Gets the camera's pitch</summary>
        public float CameraPitch
        {
            get { return m_pitch; }
        }

        /// <summary>Gets the camera's yaw</summary>
        public float CameraYaw
        {
            get { return m_yaw; }
        }

        /// <summary>Gets and sets the maximum pitch in radians.</summary>
        public float MaxPitch
        {
            get { return m_maxPitch; }
            set { m_maxPitch = value; }
        }

        /// <summary>Gets and sets whether the camera can move along its Y-axis.</summary>
        public bool EnableYMovement
        {
            get { return m_enableYMovement; }
            set { m_enableYMovement = value; }
        }
	}
}
