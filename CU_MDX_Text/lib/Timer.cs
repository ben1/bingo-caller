/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Timer.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Handles timing and frames per second.

Created :  10/24/2005
Modified : 10/24/2005

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

namespace CUnit
{
	/// <summary>Timing and frames per second class</summary>
	public class Timer
	{
        private long m_ticksPerSecond;
        private long m_currentTime;
        private long m_lastTime;
        private long m_lastFPSUpdate;
        private long m_FPSUpdateInterval;
        private uint m_numFrames;
        private float m_runningTime;
        private float m_timeElapsed;
        private float m_fps;
        private bool m_timerStopped;

        /// <summary>Creates a new Timer</summary>
		public Timer()
		{
            // Find the frequency, or amount of ticks per second
            NativeMethods.QueryPerformanceFrequency( ref m_ticksPerSecond );

            m_timerStopped = true;
            // Update the FPS every half second.
            m_FPSUpdateInterval = m_ticksPerSecond >> 1;
		}

        /// <summary>Starts the timer.</summary>
        public void Start()
        {
            if ( !Stopped )
            {
                return;
            }
            NativeMethods.QueryPerformanceCounter( ref m_lastTime );
            m_timerStopped = false;
        }

        /// <summary>Stops the timer.</summary>
        public void Stop()
        {
            if ( Stopped )
            {
                return;
            }
            long stopTime = 0;
            NativeMethods.QueryPerformanceCounter( ref stopTime );
            m_runningTime += (float)(stopTime - m_lastTime) / (float)m_ticksPerSecond;
            m_timerStopped = true;
        }

        /// <summary>Updates the timer.</summary>
        public void Update()
        {
            if ( Stopped )
            {
                return;
            }

            // Get the current time
            NativeMethods.QueryPerformanceCounter( ref m_currentTime );

            // Update time elapsed since last frame
            m_timeElapsed = (float)(m_currentTime - m_lastTime) / (float)m_ticksPerSecond;
            m_runningTime += m_timeElapsed;

            // Update FPS
            m_numFrames++;
            if ( m_currentTime - m_lastFPSUpdate >= m_FPSUpdateInterval )
            {
                float currentTime = (float)m_currentTime / (float)m_ticksPerSecond;
                float lastTime = (float)m_lastFPSUpdate / (float)m_ticksPerSecond;
                m_fps = (float)m_numFrames / (currentTime - lastTime);

                m_lastFPSUpdate = m_currentTime;
                m_numFrames = 0;
            }

            m_lastTime = m_currentTime;
        }

        /// <summary>Is the timer stopped?</summary>
        public bool Stopped 
        { 
            get { return m_timerStopped; } 
        }
        
        /// <summary>Frames per second</summary>
        public float FPS 
        { 
            get { return m_fps; } 
        }
        
        /// <summary>Elapsed time since last update. If the timer is stopped, returns 0.</summary>
        public float ElapsedTime 
        {
            get 
            { 
                if ( Stopped )
                {
                    return 0;
                }
                return m_timeElapsed; 
            } 
        }

        /// <summary>Total running time.</summary>
        public float RunningTime 
        { 
            get { return m_runningTime; } 
        }
	}
}