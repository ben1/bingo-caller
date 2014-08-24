/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Framework.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : C-Unit Framework

Created :  10/22/2005
Modified : 01/31/2005

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DirectInput = Microsoft.DirectX.DirectInput;

namespace CUnit
{
    public partial class Framework : Form
    {
        private Graphics m_graphics = null;
        private Timer m_timer = null;
        private StateManager m_gameStates = null;
        private FrameworkState m_state;
        private DeviceSettings m_newSettings = null;

        public Framework()
        {
            InitializeComponent();

            // Set default values
            m_state = new FrameworkState();
            m_graphics = new Graphics();
            m_timer = new Timer();
            m_state.WindowLocation = new Point( 0, 0 );
            m_state.WindowSize = new Size( 640, 480 );
            m_state.FillMode = FillMode.Solid;
            m_mousePosition = new System.Drawing.Point(
                MousePosition.X - this.ClientRectangle.X, MousePosition.Y - this.ClientRectangle.Y );
            this.Closed += new EventHandler( Framework_Closed );
            m_gameStates = new StateManager();
        }

        /// <summary>Initialize the Framework.</summary>
        /// <param name="windowed">True for window mode, false for fullscreen mode.</param>
        /// <param name="width">Window width</param>
        /// <param name="height">Window height</param>
        /// <param name="title">Text to display in the title bar.</param>
        /// <returns>True on success, false on failure</returns>
        public void Initialize( bool windowed, int width, int height, string title )
        {
            m_state.WindowSize = new Size( width, height );
            this.ClientSize = m_state.WindowSize;
            this.Text = title;
            this.Show();

            // Initialize Direct3D
            m_graphics.Initialize( windowed, this, width, height );

            // Set device events
            m_graphics.Device.DeviceLost += new System.EventHandler( this.OnLostDevice );
            m_graphics.Device.DeviceReset += new System.EventHandler( this.OnResetDevice );
            m_graphics.Device.Disposing += new System.EventHandler( this.OnDestroyDevice );

            // Create resources
            OnCreateDevice();
            OnResetDevice( this, null );

            Pause( false, false );
            m_state.Initialized = true;
        }

        /// <summary>Runs the main loop.</summary>
        public void Run()
        {
            Application.Idle += new EventHandler( this.OnApplicationIdle );
            Application.Run();
        }

        /// <summary>Called after the device is created. Create Pool.Managed resources here.</summary>
        private void OnCreateDevice()
        {
            m_gameStates.OnCreateDevice( m_graphics.Device );
        }

        /// <summary>Called after the device is reset. Create Pool.Default resources here.</summary>
        /// <param name="sender">Sending object</param>
        /// <param name="args">Event arguments</param>
        private void OnResetDevice( object sender, EventArgs args )
        {
            m_gameStates.OnResetDevice( m_graphics.Device );
        }

        /// <summary>Called when the device is lost. Release Pool.Default resources here.</summary>
        /// <param name="sender">Sending object</param>
        /// <param name="args">Event arguments</param>
        private void OnLostDevice( object sender, EventArgs args )
        {
            m_gameStates.OnLostDevice();
        }

        /// <summary>Called after the device is disposed. Release Pool.Managed resources here.</summary>
        /// <param name="sender">Sending object</param>
        /// <param name="args">Event arguments</param>
        private void OnDestroyDevice( object sender, EventArgs args )
        {
            m_gameStates.OnDestroyDevice();
        }

        /// <summary>Update the current frame.</summary>
        private void OnUpdateFrame()
        {
            if ( m_graphics.Device == null || m_graphics.Device.IsDisposed )
            {
                return;
            }

            m_timer.Update();

            // Process mouse event
            if ( m_numPressedButtons > 0 || m_xDelta != 0 || m_yDelta != 0 || m_zDelta != 0 )
            {
                m_gameStates.OnMouse( m_mousePosition, m_xDelta, m_yDelta, m_zDelta, m_mouseButtons, m_timer.ElapsedTime );
                m_xDelta = 0;
                m_yDelta = 0;
                m_zDelta = 0;
                // Need this or else app won't be able to process mouse releases
                if ( NoButtonsDown )
                {
                    m_numPressedButtons = 0;
                }
            }

            // Process keyboard event
            if ( m_numPressedKeys > 0 )
            {
                int oldLockCount = m_keyLock.Count;
                m_gameStates.OnKeyboard( m_pressedKeys, m_pressedChar, m_pressedKey, m_timer.ElapsedTime );

                // Application may be enumerating through pressedKeys, so we should
                // remove newly locked keys after the application enumerates.
                if ( oldLockCount != m_keyLock.Count )
                {
                    foreach ( Keys k in m_keyLock )
                    {
                        m_pressedKeys.Remove( k );
                    }
                }
                // m_numPressedKeys allows GameState to process keyup event.
                m_numPressedKeys = m_pressedKeys.Count;
                m_pressedChar = char.MinValue;
                m_pressedKey = 0;
            }

            m_gameStates.OnUpdateFrame( m_graphics.Device, m_timer.ElapsedTime );

            // New settings from DeviceOptionsDisplay
            if ( m_newSettings != null )
            {
                ChangeDevice( m_newSettings );
                m_newSettings = null;
            }

        }

        /// <summary>Render the current frame.</summary>
        private void OnRenderFrame()
        {
            if ( ( this.WindowState == FormWindowState.Minimized ) ||
                m_graphics.Device == null || m_graphics.Device.IsDisposed || m_state.FormClosing ||
                m_state.DeviceLost )
            {
                return;
            }

            try
            {
                m_gameStates.OnRenderFrame( m_graphics.Device, m_timer.ElapsedTime );
            }
            catch ( DeviceLostException )
            {
                // The device is lost
                System.Threading.Thread.Sleep( 50 );
                if ( !m_state.DeviceLost )
                {
                    Pause( true, true );
                    m_state.DeviceLost = true;
                }
            }
        }

        /// <summary>Resets the device with new settings or creates a new device.</summary>
        /// <param name="newSettings">New Device settings</param>
        public void ChangeDevice( DeviceSettings newSettings )
        {
            Pause( true, true );
            m_state.DisableResize = true;
            DeviceSettings oldSettings = m_graphics.CurrentSettings;

            if ( newSettings.PresentParameters.IsWindowed )
            {
                m_graphics.WindowedSettings = (DeviceSettings)newSettings.Clone();
            }
            else
            {
                m_graphics.FullscreenSettings = (DeviceSettings)newSettings.Clone();
            }
            m_graphics.Windowed = newSettings.PresentParameters.IsWindowed;

            // Set new window style
            if ( m_graphics.Windowed )
            {
                // Going to window mode
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.TopMost = false;
            }
            else
            {
                // Going to fullscreen mode
                this.FormBorderStyle = FormBorderStyle.None;
                // Save the current location/client size
                m_state.WindowLocation = this.Location;
                m_state.WindowSize = this.ClientSize;
            }

            // If AdapterOrdinal, DeviceType, and BehaviorFlags are the same, we can just do a Reset().
            // If they've changed, we need to do a complete device tear down/rebuild.
            if ( ( oldSettings.AdapterOrdinal == newSettings.AdapterOrdinal ) &&
                ( oldSettings.DeviceType == newSettings.DeviceType ) &&
                ( oldSettings.BehaviorFlags == newSettings.BehaviorFlags ) )
            {
                // We can just Reset the device
                m_graphics.Reset();
            }
            else
            {
                // Have to dispose and recreate the device
                m_graphics.ChangeDevice( newSettings );

                // Set device events
                m_graphics.Device.DeviceLost += new System.EventHandler( this.OnLostDevice );
                m_graphics.Device.DeviceReset += new System.EventHandler( this.OnResetDevice );
                m_graphics.Device.Disposing += new System.EventHandler( this.OnDestroyDevice );

                // Create resources
                OnCreateDevice();
                OnResetDevice( this, null );
            }

            if ( m_graphics.Windowed )
            {
                // Going to window mode
                // Restore the window size/location
                this.Location = m_state.WindowLocation;
                this.ClientSize = m_state.WindowSize;
            }

            m_state.DisableResize = false;
            Pause( false, false );
        }

        /// <summary>Toggles between window and fullscreen mode</summary>
        public void ToggleFullscreen()
        {
            // Flip windowed state
            m_graphics.Windowed = !m_graphics.Windowed;

            if ( m_graphics.Windowed )
            {
                ChangeDevice( m_graphics.WindowedSettings );
            }
            else
            {
                ChangeDevice( m_graphics.FullscreenSettings );
            }
        }

        /// <summary>Pushes a new state onto the frameworks State stack.</summary>
        /// <param name="state">New state</param>
        public void PushState( GameState state )
        {
            m_gameStates.Push( state );
        }

        /// <summary>Pauses or unpauses rendering and the timer.</summary>
        /// <param name="pauseRendering">True to pause rendering, false to unpause rendering.</param>
        /// <param name="pauseTimer">True to pause the timer, false to unpause the timer.</param>
        private void Pause( bool pauseRendering, bool pauseTimer )
        {
            if ( pauseRendering )
            {
                m_state.RenderingPausedCount++;
            }
            else
            {
                m_state.RenderingPausedCount = Math.Max( 0, m_state.RenderingPausedCount - 1 );
            }
            if ( pauseTimer )
            {
                m_state.TimerPausedCount++;
            }
            else
            {
                m_state.TimerPausedCount = Math.Max( 0, m_state.TimerPausedCount - 1 );
            }

            m_state.RenderingPaused = ( m_state.RenderingPausedCount > 0 );
            m_state.TimerPaused = ( m_state.TimerPausedCount > 0 );
            if ( m_state.TimerPaused && m_timer != null )
            {
                m_timer.Stop();
            }
            else if ( !m_state.TimerPaused && m_timer != null )
            {
                m_timer.Start();
            }
        }

        /// <summary>Takes a screen shot</summary>
        public void TakeScreenShot()
        {
            string fileName = DateTime.Now.ToString( "yyyyMMdd" ) + "_" + DateTime.Now.TimeOfDay.ToString();
            // Remove colons and junk
            fileName = fileName.Remove( 11, 1 );
            fileName = fileName.Remove( 13, 1 );
            fileName = fileName.Remove( 15, 1 );
            Surface backbuffer = m_graphics.Device.GetBackBuffer( 0, 0 );
            // Save functions are missing from December SDK...have to wait until new release.
            //Surface.Save( fileName + ".tga", ImageFileFormat.Tga, backbuffer );
            backbuffer.Dispose();
        }

        /// <summary>Window resize event. Reset the device, update and render the frame.</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnResize( EventArgs e )
        {
            if ( m_state.DisableResize ||
                ( this.WindowState == FormWindowState.Minimized )
                || m_graphics == null || m_state.DeviceLost || !m_state.Initialized )
            {
                return;
            }
            base.OnResize( e );
            try
            {
                m_graphics.Reset();
            }
            catch ( DeviceLostException )
            {
                if ( !m_state.DeviceLost )
                {
                    m_state.DeviceLost = true;
                    Pause( true, true );
                }
                return;
            }
            OnUpdateFrame();
            OnRenderFrame();
        }

        /// <summary>Application idle event. Updates and renders frames.</summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void OnApplicationIdle( object sender, EventArgs e )
        {
            if ( m_graphics.Device == null )
            {
                return;
            }
            while ( AppStillIdle )
            {
                if ( !m_graphics.Device.IsDisposed && !m_state.FormClosing && !m_state.DeviceLost &&
                    this.WindowState != FormWindowState.Minimized && m_state.Initialized )
                {
                    OnUpdateFrame();
                    OnRenderFrame();
                }
                else if ( m_state.DeviceLost )
                {
                    RegainLostDevice();
                }
            }
        }

        /// <summary>Tries to regain a lost device.</summary>
        private void RegainLostDevice()
        {
            // Check for lost device
            if ( !m_graphics.Device.CheckCooperativeLevel() )
            {
                ResultCode code = m_graphics.Device.CheckCooperativeLevelResult();
                if ( code == ResultCode.DeviceLost )
                {
                    // The device has been lost but cannot be reset at this time.  
                    // So wait until it can be reset.
                    System.Threading.Thread.Sleep( 50 );
                    return;
                }
                try
                {
                    m_state.DisableResize = true;
                    m_graphics.Reset();
                    m_state.DeviceLost = false;
                    m_state.DisableResize = false;
                    Pause( false, false );
                }
                catch ( DeviceLostException )
                {
                    // The device was lost again, so continue waiting until it can be reset.
                    System.Threading.Thread.Sleep( 50 );
                }
            }
        }

        /// <summary>Called when the Form is closed</summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void Framework_Closed( object sender, EventArgs e )
        {
            m_state.FormClosing = true;
            Application.Exit();
        }

        /// <summary>Returns whether the application is currently idle.</summary>
        private bool AppStillIdle
        {
            get
            {
                NativeMethods.Message msg;
                return !NativeMethods.PeekMessage( out msg, IntPtr.Zero, 0, 0, 0 );
            }
        }

        /// <summary>Gets the Framework's Graphics object</summary>
        public Graphics Graphics
        {
            get { return m_graphics; }
        }

        /// <summary>Gets and Sets new settings for the Device</summary>
        public DeviceSettings NewSettings
        {
            get { return m_newSettings; }
            set { m_newSettings = value; }
        }

        /// <summary>Returns the size of the current backbuffer.</summary>
        public Size DisplaySize
        {
            get
            {
                if ( m_graphics != null )
                {
                    if ( m_graphics.Windowed )
                    {
                        return this.ClientSize;
                    }
                    else
                    {
                        return m_graphics.FullscreenSize;
                    }
                }
                return this.ClientSize;
            }
        }

        /// <summary>Gets and sets the fillmode.</summary>
        public FillMode FillMode
        {
            get
            {
                return m_state.FillMode;
            }
            set
            {
                m_state.FillMode = value;
                if ( m_graphics.Device != null )
                {
                    m_graphics.Device.RenderState.FillMode = value;
                }
            }
        }

        /// <summary>Gets the framerate.</summary>
        public float FPS
        {
            get { return m_timer.FPS; }
        }

        /// <summary>Gets the current settings of the device.</summary>
        public DeviceSettings CurrentSettings
        {
            get
            {
                if ( m_graphics != null )
                {
                    return m_graphics.CurrentSettings;
                }
                return null;
            }
        }

      public Timer Timer { get { return m_timer; } }
    }
}