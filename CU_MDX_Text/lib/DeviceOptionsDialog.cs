/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : DeviceOptionsDialog.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Options Dialog

Created :  11/13/2005
Modified : 01/31/2006

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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace CUnit
{
    /// <summary>Used to adjust Device options</summary>
    public class DeviceOptionsDialog : GameState
    {
        public enum ControlID
        {
            Panel = 1, DisplayAdapter, DisplayAdapterText, RenderDevice, RenderDeviceText,
            RadioWindow, RadioFullscreen, AdapterFormat, AdapterFormatText, FullscreenResolution, FullscreenResolutionText,
            RefreshRate, RefreshRateText, BackBufferFormat, BackBufferFormatText, DepthStencilFormat,
            DepthStencilFormatText, MultiSampleType, MultiSampleTypeText, MultiSampleQuality, MultiSampleQualityText,
            VertexProcessing, VertexProcessingText, PresentInterval, PresentIntervalText, OK, Cancel
        }
        private Gui.GuiManager m_gui = null;
        private Graphics m_graphics;
        private Framework m_framework;
        private DeviceSettings m_newSettings = null;
        private ArrayList m_adapterList;
        private bool m_doneSetting;
        private bool m_hasNewSettings;
        private const float Width = 360f;
        private const float Height = 400f;

        /// <summary>Creates a new VideoOptionsDialog.</summary>
        /// <param name="device">Direct3D Device</param>
        /// <param name="settings">Current DeviceSettings</param>
        /// <param name="adapterList">ArrayList of adapters</param>
        public DeviceOptionsDialog( Framework framework )
        {
            m_framework = framework;
            m_graphics = framework.Graphics;
            m_newSettings = m_graphics.CurrentSettings;
            m_adapterList = m_graphics.Adapters;
            OnCreateDevice( m_graphics.Device );
            OnResetDevice( m_graphics.Device );
        }

        /// <summary>Updates the Controls based on the desired settings</summary>
        /// <param name="settings">Desired settings</param>
        private void UpdateControlValues( DeviceSettings settings )
        {
            // Clear everything
            ( m_gui[(int)ControlID.DisplayAdapter] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.RenderDevice] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.AdapterFormat] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.FullscreenResolution] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.RefreshRate] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.BackBufferFormat] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.DepthStencilFormat] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.MultiSampleType] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.MultiSampleQuality] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.VertexProcessing] as Gui.ComboBox ).Clear();
            ( m_gui[(int)ControlID.PresentInterval] as Gui.ComboBox ).Clear();

            // Fill it all back up based on the passed in settings
            foreach ( AdapterEnum a in m_adapterList )
            {
                // Display Adapters
                m_gui.AddListableItem( (int)ControlID.DisplayAdapter, a.Description, a.AdapterOrdinal );
                if ( settings.AdapterOrdinal == a.AdapterOrdinal )
                {
                    ( m_gui[(int)ControlID.DisplayAdapter] as Gui.ComboBox ).SelectItem( a.Description );
                }

                // Everything below depends on AdapterOrdinal
                if ( a.AdapterOrdinal != settings.AdapterOrdinal )
                {
                    continue;
                }
                foreach ( DisplayMode d in a.DisplayModeList )
                {
                    // Refresh Rate
                    if ( !( m_gui[(int)ControlID.RefreshRate] as Gui.ComboBox ).ContainsItem( d.RefreshRate.ToString() + " Hz" ) )
                    {
                        // Available refresh rates depend on resolution
                        if ( ( d.Width == settings.PresentParameters.BackBufferWidth ) &&
                            ( d.Height == settings.PresentParameters.BackBufferHeight ) &&
                            ( !settings.PresentParameters.IsWindowed ) )
                        {
                            m_gui.AddListableItem( (int)ControlID.RefreshRate, d.RefreshRate.ToString() + " Hz", d.RefreshRate );
                            if ( ( ( d.RefreshRate == settings.PresentParameters.FullScreenRefreshRateInHz ) &&
                            ( !settings.PresentParameters.IsWindowed ) && settings.PresentParameters.FullScreenRefreshRateInHz != 0 ) ||
                            ( m_gui[(int)ControlID.RefreshRate].Data as ArrayList).Count == 0 )
                            {
                                ( m_gui[(int)ControlID.RefreshRate] as Gui.ComboBox ).SelectItem( d.RefreshRate.ToString() + " Hz" );
                            }
                        }
                        else if ( settings.PresentParameters.IsWindowed || settings.PresentParameters.FullScreenRefreshRateInHz == 0 )
                        {
                            // Select current refresh rate if in windowed mode
                            DisplayMode displayMode = Manager.Adapters[(int)a.AdapterOrdinal].CurrentDisplayMode;
                            if ( !( m_gui[(int)ControlID.RefreshRate] as Gui.ComboBox ).ContainsItem( displayMode.RefreshRate.ToString() + " Hz" ) )
                            {
                                m_gui.AddListableItem( (int)ControlID.RefreshRate, displayMode.RefreshRate.ToString() + " Hz", d.RefreshRate );
                            }
                            ( m_gui[(int)ControlID.RefreshRate] as Gui.ComboBox ).SelectItem( displayMode.RefreshRate.ToString() + " Hz" );
                        }
                    }

                    // Fullscreen Resolution
                    if ( settings.PresentParameters.IsWindowed )
                    {
                        // Select current resolution if in windowed mode
                        DisplayMode displayMode = Manager.Adapters[(int)a.AdapterOrdinal].CurrentDisplayMode;
                        string text = string.Format( "{0} by {1}", displayMode.Width, displayMode.Height );
                        m_gui.AddListableItem( (int)ControlID.FullscreenResolution, text, new Size( displayMode.Width, displayMode.Height ) );
                        ( m_gui[(int)ControlID.FullscreenResolution] as Gui.ComboBox ).SelectItem( text );
                    }
                    else
                    {
                        if ( d.Format != settings.AdapterFormat )
                        {
                            continue;
                        }
                        string text = string.Format( "{0} by {1}", d.Width, d.Height );
                        if ( !( m_gui[(int)ControlID.FullscreenResolution] as Gui.ComboBox ).ContainsItem( text ) )
                        {
                            m_gui.AddListableItem( (int)ControlID.FullscreenResolution, text, new Size( d.Width, d.Height ) );
                            if ( ( d.Width == settings.PresentParameters.BackBufferWidth ) &&
                                 ( d.Height == settings.PresentParameters.BackBufferHeight ) )
                            {
                                ( m_gui[(int)ControlID.FullscreenResolution] as Gui.ComboBox ).SelectItem( text );
                            }
                        }
                    }
                }

                foreach ( DeviceEnum d in a.DeviceEnumList )
                {
                    // Render Devices
                    m_gui.AddListableItem( (int)ControlID.RenderDevice, d.DeviceType.ToString(), d.DeviceType );
                    if ( settings.DeviceType == d.DeviceType )
                    {
                        ( m_gui[(int)ControlID.RenderDevice] as Gui.ComboBox ).SelectItem( d.DeviceType.ToString() );
                    }

                    // Everything below depends on DeviceType
                    if ( d.DeviceType != settings.DeviceType )
                    {
                        continue;
                    }
                    foreach ( DeviceSettingsEnum s in d.SettingsList )
                    {
                        // Everything below depends on windowed
                        if ( s.IsWindowed != settings.PresentParameters.IsWindowed )
                        {
                            continue;
                        }
                        // Adapter Formats
                        if ( !( m_gui[(int)ControlID.AdapterFormat] as Gui.ComboBox ).ContainsItem( s.AdapterFormat.ToString() ) )
                        {
                            m_gui.AddListableItem( (int)ControlID.AdapterFormat, s.AdapterFormat.ToString(), s.AdapterFormat );
                            if ( s.AdapterFormat == settings.AdapterFormat )
                            {
                                ( m_gui[(int)ControlID.AdapterFormat] as Gui.ComboBox ).SelectItem( s.AdapterFormat.ToString() );
                            }
                        }

                        // Everything below depends on AdapterFormat
                        if ( s.AdapterFormat != settings.AdapterFormat )
                        {
                            continue;
                        }

                        // BackBuffer Formats
                        if ( !( m_gui[(int)ControlID.BackBufferFormat] as Gui.ComboBox ).ContainsItem( s.BackBufferFormat.ToString() ) )
                        {
                            ( m_gui[(int)ControlID.BackBufferFormat] as Gui.ComboBox ).AddItem( s.BackBufferFormat.ToString(), s.BackBufferFormat );
                            if ( s.BackBufferFormat == settings.PresentParameters.BackBufferFormat )
                            {
                                ( m_gui[(int)ControlID.BackBufferFormat] as Gui.ComboBox ).SelectItem( s.BackBufferFormat.ToString() );
                            }
                        }

                        // Depth/Stencil Formats
                        foreach ( DepthFormat f in s.DepthStencilFormatList )
                        {
                            if ( !( m_gui[(int)ControlID.DepthStencilFormat] as Gui.ComboBox ).ContainsItem( f.ToString() ) )
                            {
                                ( m_gui[(int)ControlID.DepthStencilFormat] as Gui.ComboBox ).AddItem( f.ToString(), f );
                                if ( f == settings.PresentParameters.AutoDepthStencilFormat )
                                {
                                    ( m_gui[(int)ControlID.DepthStencilFormat] as Gui.ComboBox ).SelectItem( f.ToString() );
                                }
                            }
                        }

                        // Everything below depends on BackBufferFormat
                        if ( s.BackBufferFormat != settings.PresentParameters.BackBufferFormat )
                        {
                            continue;
                        }

                        // Multisample Types
                        foreach ( MultiSampleType t in s.MultiSampleTypeList )
                        {
                            if ( !( m_gui[(int)ControlID.MultiSampleType] as Gui.ComboBox ).ContainsItem( t.ToString() ) )
                            {
                                m_gui.AddListableItem( (int)ControlID.MultiSampleType, t.ToString(), t );
                                if ( t == settings.PresentParameters.MultiSampleType )
                                {
                                    ( m_gui[(int)ControlID.MultiSampleType] as Gui.ComboBox ).SelectItem( t.ToString() );
                                }
                            }
                        }

                        // Multisample Qualities
                        int maxQuality = 0;
                        MultiSampleType currentType = settings.PresentParameters.MultiSampleType;
                        for ( int i = 0; i < s.MultiSampleTypeList.Count; i++ )
                        {
                            MultiSampleType msType = (MultiSampleType)s.MultiSampleTypeList[i];
                            if ( msType == currentType )
                            {
                                maxQuality = (int)s.MultiSampleQualityList[i];
                            }
                        }
                        for ( int i = 0; i < maxQuality; i++ )
                        {
                            if ( !( m_gui[(int)ControlID.MultiSampleQuality] as Gui.ComboBox ).ContainsItem( i.ToString() ) )
                            {
                                m_gui.AddListableItem( (int)ControlID.MultiSampleQuality, i.ToString(), i );
                                if ( i == settings.PresentParameters.MultiSampleQuality )
                                {
                                    ( m_gui[(int)ControlID.MultiSampleQuality] as Gui.ComboBox ).SelectItem( i.ToString() );
                                }
                            }
                        }

                        // VertexProcessing
                        foreach ( CreateFlags f in s.VertexProcessingTypeList )
                        {
                            string text = f.ToString();
                            if ( !( m_gui[(int)ControlID.VertexProcessing] as Gui.ComboBox ).ContainsItem( text ) )
                            {
                                if ( f == ( CreateFlags.PureDevice | CreateFlags.HardwareVertexProcessing ) )
                                {
                                    text = "PureDevice";
                                }
                                m_gui.AddListableItem( (int)ControlID.VertexProcessing, text, f );
                                if ( f == settings.BehaviorFlags )
                                {
                                    ( m_gui[(int)ControlID.VertexProcessing] as Gui.ComboBox ).SelectItem( text );
                                }
                            }
                        }

                        // Present interval
                        foreach ( PresentInterval p in s.PresentIntervalList )
                        {
                            if ( !( m_gui[(int)ControlID.PresentInterval] as Gui.ComboBox ).ContainsItem( p.ToString() ) )
                            {
                                m_gui.AddListableItem( (int)ControlID.PresentInterval, p.ToString(), p );
                                if ( p == settings.PresentParameters.PresentationInterval )
                                {
                                    ( m_gui[(int)ControlID.PresentInterval] as Gui.ComboBox ).SelectItem( p.ToString() );
                                }
                            }
                        }
                    }
                }
            }

            // Window/Fullscreen mode
            ( m_gui[(int)ControlID.RadioWindow] as Gui.RadioButton ).Checked = settings.PresentParameters.IsWindowed;
            ( m_gui[(int)ControlID.RadioFullscreen] as Gui.RadioButton ).Checked = !settings.PresentParameters.IsWindowed;

            m_gui.DirtyBuffer = true;
        }

        /// <summary>Call when the device is created</summary>
        /// <param name="device">D3D Device</param>
        public override void OnCreateDevice( Device device )
        {
            float x = ( (float)m_graphics.CurrentSettings.PresentParameters.BackBufferWidth / 2f ) - ( Width / 2f );
            float y = ( (float)m_graphics.CurrentSettings.PresentParameters.BackBufferHeight / 2f ) - ( Height / 2f );
            m_gui = new Gui.GuiManager( "CUnit.xml", new Gui.GuiManager.ControlDelegate( OnControl ) );
            m_gui.CreatePanel( (int)ControlID.Panel, new PointF( x, y ), new SizeF( Width, Height ) );
            ( m_gui[(int)ControlID.Panel] as Gui.Panel ).Locked = true;

            int panel = (int)ControlID.Panel;
            float dropSize = 155f;
            float fontSize = 14f;
            x = 140f;
            y = 15f;
            // Add Controls
            m_gui.CreateComboBox( (int)ControlID.DisplayAdapter, panel, new PointF( x, y ), new SizeF( 200.0f, 20.0f ), 80f, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.DisplayAdapterText, panel, new PointF( 0f, y ), new SizeF( x - 5f, 20.0f ), "Display Adapter", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.RenderDevice, panel, new PointF( x, y + 25f ), new SizeF( 200.0f, 20.0f ), 80f, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.RenderDeviceText, panel, new PointF( 0f, y + 25f ), new SizeF( x - 5f, 20.0f ), "Render Device", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateRadioButton( (int)ControlID.RadioWindow, panel, 0, new PointF( x, y + 50f ), new SizeF( 20.0f, 20.0f ), "Windowed", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), CUnit.Gui.Control.TextAlign.Right, true );
            m_gui.CreateRadioButton( (int)ControlID.RadioFullscreen, panel, 0, new PointF( 243f, y + 50f ), new SizeF( 20.0f, 20.0f ), "Fullscreen", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), CUnit.Gui.Control.TextAlign.Right, false );
            m_gui.CreateComboBox( (int)ControlID.AdapterFormat, panel, new PointF( x, y + 75f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.AdapterFormatText, panel, new PointF( 0f, y + 75f ), new SizeF( x - 5f, 20.0f ), "Adapter Format", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.FullscreenResolution, panel, new PointF( x, y + 100f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.FullscreenResolutionText, panel, new PointF( 0f, y + 100f ), new SizeF( x - 5f, 20.0f ), "Screen Resolution", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.RefreshRate, panel, new PointF( x, y + 125f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.RefreshRateText, panel, new PointF( 0f, y + 125f ), new SizeF( x - 5f, 20.0f ), "Refresh Rate", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.BackBufferFormat, panel, new PointF( x, y + 150f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.BackBufferFormatText, panel, new PointF( 0f, y + 150f ), new SizeF( x - 5f, 20.0f ), "Backbuffer Format", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.DepthStencilFormat, panel, new PointF( x, y + 175f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.DepthStencilFormatText, panel, new PointF( 0f, y + 175f ), new SizeF( x - 5f, 20.0f ), "Depth/Stencil Format", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.MultiSampleType, panel, new PointF( x, y + 200f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.MultiSampleTypeText, panel, new PointF( 0f, y + 200f ), new SizeF( x - 5f, 20.0f ), "Multisample Type", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.MultiSampleQuality, panel, new PointF( x, y + 225f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.MultiSampleQualityText, panel, new PointF( 0f, y + 225f ), new SizeF( x - 5f, 20.0f ), "Multisample Quality", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.VertexProcessing, panel, new PointF( x, y + 250f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.VertexProcessingText, panel, new PointF( 0f, y + 250f ), new SizeF( x - 5f, 20.0f ), "Vertex Processing", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateComboBox( (int)ControlID.PresentInterval, panel, new PointF( x, y + 275f ), new SizeF( 200.0f, 20.0f ), dropSize, fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateLabel( (int)ControlID.PresentIntervalText, panel, new PointF( 0f, y + 275f ), new SizeF( x - 5f, 20.0f ), "Present Interval", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ), BitmapFont.Align.Right );
            m_gui.CreateButton( (int)ControlID.OK, panel, new PointF( x, y + 300f ), new SizeF( 75f, 20f ), "OK", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );
            m_gui.CreateButton( (int)ControlID.Cancel, panel, new PointF( 275, y + 300f ), new SizeF( 75f, 20f ), "Cancel", fontSize, new ColorValue( 0.0f, 0.0f, 0.0f ) );

            if ( m_newSettings.PresentParameters.IsWindowed )
            {
                // Disable Controls only meant for fullscreen
                m_gui[(int)ControlID.AdapterFormat].Disabled = true;
                m_gui[(int)ControlID.FullscreenResolution].Disabled = true;
                m_gui[(int)ControlID.RefreshRate].Disabled = true;
            }
            if ( m_gui != null )
            {
                m_gui.OnCreateDevice( device );
            }
            UpdateControlValues( m_newSettings );
        }

        /// <summary>Call when the device is reset</summary>
        /// <param name="device">D3D device</param>
        public override void OnResetDevice( Device device )
        {
            if ( m_gui != null )
            {
                // Center the Panel
                m_gui.OnResetDevice( device );
                float x = ( (float)m_graphics.CurrentSettings.PresentParameters.BackBufferWidth / 2f ) - ( Width / 2f );
                float y = ( (float)m_graphics.CurrentSettings.PresentParameters.BackBufferHeight / 2f ) - ( Height / 2f );
                m_gui.SetPosition( (int)ControlID.Panel, new PointF( x, y ) );
            }
        }

        /// <summary>Call when the device is lost.</summary>
        public override void OnLostDevice()
        {
            if ( m_gui != null )
            {
                m_gui.OnLostDevice();
            }
        }

        /// <summary>Call when the device is destroyed.</summary>
        public override void OnDestroyDevice()
        {
            if ( m_gui != null )
            {
                m_gui.OnDestroyDevice();
                m_gui = null;
            }
        }

        /// <summary>Update the current frame.</summary>
        /// <param name="device">D3D Device</param>
        /// <param name="elapsedTime">Time elapsed since last frame</param>
        public override void OnUpdateFrame( Device device, float elapsedTime )
        {
            if ( m_hasNewSettings )
            {
                // Flag framework to change the device
                m_framework.NewSettings = m_newSettings;
            }
        }

        /// <summary>Renders the current frame.</summary>
        /// <param name="device">D3D Device</param>
        /// <param name="elapsedTime">Time elapsed since last frame</param>
        public override void OnRenderFrame( Device device, float elapsedTime )
        {
            device.Clear( ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1f, 0 );
            device.BeginScene();
            if ( m_gui != null )
            {
                m_gui.Render( device );
            }
            device.EndScene();
            device.Present();
        }

        /// <summary>Keyboard handler.</summary>
        /// <param name="pressedKeys">List of pressed keys.</param>
        /// <param name="pressedChar">Character read from keyboard.</param>
        /// <param name="pressedKey">Keycode read from keyboard.</param>
        /// <param name="elapsedTime">Time since last frame</param>
        public override void OnKeyboard( List<System.Windows.Forms.Keys> pressedKeys, char pressedChar, int pressedKey, float elapsedTime )
        {
            // Empty
        }

        /// <summary>Mouse handler.</summary>
        /// <param name="position">Mouse position in client coordinates</param>
        /// <param name="xDelta">X-axis delta.</param>
        /// <param name="yDelta">Y-axis delta.</param>
        /// <param name="zDelta">Wheel delta.</param>
        /// <param name="buttons">Mouse button state.</param>
        /// <param name="elapsedTime">Time since last frame</param>
        public override void OnMouse( Point position, int xDelta, int yDelta, int zDelta, bool[] buttons, float elapsedTime )
        {
            if ( m_gui != null )
            {
                m_gui.MouseHandler( position, buttons, zDelta );
            }
        }

        /// <summary>Gui Control handler</summary>
        /// <param name="controlID">Control ID</param>
        /// <param name="data">Control data</param>
        public override void OnControl( int controlID, object data )
        {
            if ( m_gui[controlID] is Gui.ComboBox )
            {
                data = ( (ArrayList)data )[0];
            }
            switch ( controlID )
            {
                case (int)ControlID.DisplayAdapter:
                    m_newSettings.AdapterOrdinal = (uint)data;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.RenderDevice:
                    m_newSettings.DeviceType = (DeviceType)data;
                    if ( m_newSettings.DeviceType == DeviceType.Reference )
                    {
                        m_newSettings.PresentParameters.PresentationInterval = PresentInterval.Default;
                    }
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.RadioFullscreen:
                    m_gui[(int)ControlID.RefreshRate].Disabled = false;
                    m_gui[(int)ControlID.FullscreenResolution].Disabled = false;
                    m_gui[(int)ControlID.AdapterFormat].Disabled = false;
                    m_newSettings.PresentParameters.IsWindowed = false;
                    m_newSettings.PresentParameters.BackBufferFormat = m_newSettings.AdapterFormat;
                    // Reset displaymode
                    DisplayMode mode = Manager.Adapters[(int)m_newSettings.AdapterOrdinal].CurrentDisplayMode;
                    m_newSettings.PresentParameters.BackBufferWidth = mode.Width;
                    m_newSettings.PresentParameters.BackBufferHeight = mode.Height;
                    m_newSettings.PresentParameters.FullScreenRefreshRateInHz = mode.RefreshRate;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.RadioWindow:
                    m_gui[(int)ControlID.RefreshRate].Disabled = true;
                    m_gui[(int)ControlID.FullscreenResolution].Disabled = true;
                    m_gui[(int)ControlID.AdapterFormat].Disabled = true;
                    m_newSettings.PresentParameters.IsWindowed = true;
                    m_newSettings.PresentParameters.BackBufferWidth = 0;
                    m_newSettings.PresentParameters.BackBufferHeight = 0;
                    m_newSettings.PresentParameters.FullScreenRefreshRateInHz = 0;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.AdapterFormat:
                    m_newSettings.AdapterFormat = (Format)data;
                    m_newSettings.PresentParameters.BackBufferFormat = (Format)data;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.FullscreenResolution:
                    m_newSettings.PresentParameters.BackBufferWidth = ( (Size)data ).Width;
                    m_newSettings.PresentParameters.BackBufferHeight = ( (Size)data ).Height;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.RefreshRate:
                    m_newSettings.PresentParameters.FullScreenRefreshRateInHz = (int)data;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.BackBufferFormat:
                    m_newSettings.PresentParameters.BackBufferFormat = (Format)data;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.DepthStencilFormat:
                    m_newSettings.PresentParameters.AutoDepthStencilFormat = (DepthFormat)data;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.MultiSampleType:
                    m_newSettings.PresentParameters.MultiSampleType = (MultiSampleType)data;
                    if ( m_newSettings.PresentParameters.MultiSampleType != MultiSampleType.NonMaskable )
                    {
                        m_newSettings.PresentParameters.MultiSampleQuality = 0;
                    }
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.MultiSampleQuality:
                    m_newSettings.PresentParameters.MultiSampleQuality = (int)data;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.VertexProcessing:
                    m_newSettings.BehaviorFlags = (CreateFlags)data;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.PresentInterval:
                    m_newSettings.PresentParameters.PresentationInterval = (PresentInterval)data;
                    UpdateControlValues( m_newSettings );
                    break;
                case (int)ControlID.OK:
                    m_doneSetting = true;
                    m_hasNewSettings = true;
                    break;
                case (int)ControlID.Cancel:
                    m_doneSetting = true;
                    break;
            }
        }

        /// <summary>Tells StateManager when to pop this state off the stack.</summary>
        public override bool DoneWithState
        {
            get { return m_doneSetting; }
        }
    }
}
