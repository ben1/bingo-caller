/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Graphics.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : D3D Device wrapper.

Created :  10/22/2005
Modified : 11/15/2005

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
using System.Windows.Forms;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace CUnit
{
    /// <summary>Device wrapper</summary>
    public class Graphics
    {
        private ArrayList m_adapters = new ArrayList();
        private DeviceSettings m_windowedSettings = null;
        private DeviceSettings m_fullscreenSettings = null;
        private DeviceSettings m_currentSettings = null;

        private Device m_device = null;
        private Control m_renderWindow = null;
        private DisplayMode m_displayMode;
        private bool m_windowed;
        
        #region Enumeration Arrays
        // Default arrays
        public static readonly Format[] AdapterFormats = new Format[] 
        {
            Format.X8R8G8B8, Format.X1R5G5B5, Format.R5G6B5, Format.A2R10G10B10  
        };
        public static readonly Format[] BackBufferFormats = new Format[] 
        {
            Format.A8R8G8B8, Format.X8R8G8B8, Format.R8G8B8, Format.A2R10G10B10, 
            Format.A8R3G3B2, Format.A4R4G4B4, Format.A1R5G5B5, Format.X4R4G4B4, 
            Format.X1R5G5B5, Format.R5G6B5, Format.R3G3B2
        };
        public static readonly DeviceType[] DeviceTypes = new DeviceType[] 
        {
            DeviceType.Hardware, DeviceType.Software, DeviceType.Reference
        };
        public static readonly MultiSampleType[] MultiSampleTypes = new MultiSampleType[]
        {
            MultiSampleType.None, MultiSampleType.NonMaskable, MultiSampleType.TwoSamples, 
            MultiSampleType.ThreeSamples, MultiSampleType.FourSamples, MultiSampleType.FiveSamples, 
            MultiSampleType.SixSamples, MultiSampleType.SevenSamples, MultiSampleType.EightSamples, 
            MultiSampleType.NineSamples, MultiSampleType.TenSamples, MultiSampleType.ElevenSamples, 
            MultiSampleType.ThirteenSamples, MultiSampleType.TwelveSamples, MultiSampleType.FourteenSamples, 
            MultiSampleType.FifteenSamples, MultiSampleType.SixteenSamples
        };
        public static readonly DepthFormat[] DepthFormats = new DepthFormat[]
        {
            DepthFormat.D32, DepthFormat.D24X4S4, DepthFormat.D24X8, DepthFormat.D24S8, DepthFormat.D16, DepthFormat.D15S1
        };
        public static readonly PresentInterval[] PresentIntervals = new PresentInterval[]
        {
            PresentInterval.Immediate, PresentInterval.Default, PresentInterval.One,
            PresentInterval.Two, PresentInterval.Three, PresentInterval.Four 
        };
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Graphics()
        {
            // Empty
        }

        /// <summary>Initializes Direct3D.</summary>
        /// <param name="windowed">True for windowed mode. False for fullscreen mode.</param>
        /// <param name="control">Render window.</param>
        /// <param name="desiredWidth">Desired backbuffer width</param>
        /// <param name="desiredHeight">Desired backbuffer height</param>
        public void Initialize( bool windowed, Control control, int desiredWidth, int desiredHeight )
        {
            m_windowed = windowed;
            m_renderWindow = control;
            m_displayMode = Manager.Adapters[0].CurrentDisplayMode;
            EnumerateAdapters();

            // Create the device settings
            m_windowedSettings = FindBestWindowedSettings();
            m_fullscreenSettings = FindBestFullscreenSettings();
            m_currentSettings = (windowed) ? m_windowedSettings : m_fullscreenSettings;

            ChangeDevice( m_currentSettings );
        }

        /// <summary>Changes the device with new settings.</summary>
        public void ChangeDevice( DeviceSettings newSettings )
        {
            if ( m_device != null )
            {
                m_device.Dispose();
                m_device = null;
            }
            try
            {
                m_device = new Device( (int)newSettings.AdapterOrdinal, newSettings.DeviceType, m_renderWindow.Handle, newSettings.BehaviorFlags, newSettings.PresentParameters );
            }
            catch ( DirectXException )
            {
                throw new DirectXException( "Unable to create the Direct3D device." );
            }
        }

        /// <summary>ReBuilds the PresentParameters class inpreparation for device reset.</summary>
        private void ResetPresentParameters()
        {
            if ( m_windowed )
            {
                m_currentSettings.PresentParameters.BackBufferWidth = 0;
                m_currentSettings.PresentParameters.BackBufferHeight = 0;
            }
        }

        /// <summary>Reset the device</summary>
        public void Reset()
        {
            if ( m_device != null )
            {
                ResetPresentParameters();
                m_device.Reset( m_currentSettings.PresentParameters );
            }
        }

        /// <summary>Enumerates through all the Adapters in the system.</summary>
        private void EnumerateAdapters()
        {
            foreach ( AdapterDetails a in Manager.Adapters )
            {
                AdapterEnum currentAdapter = new AdapterEnum();

                // Store Adapter Ordinal and Information
                currentAdapter.AdapterOrdinal = (uint)a.Adapter;
                currentAdapter.AdapterInformation = a.Information;
                currentAdapter.Description = a.Information.Description;

                // Get all the DisplayModes the Adapter supports
                ArrayList adapterFormatList = EnumerateDisplayModes( a, currentAdapter );
                // Get all the Devices the Adapter can make
                EnumerateDevices( currentAdapter, adapterFormatList );

                // Add the Adaptor to the list
                m_adapters.Add( currentAdapter );
            }
        }

        /// <summary>Enumerates through all supported DisplayModes for an Adapter.</summary>
        /// <param name="a">Adapter to enumerate.</param>
        /// <param name="currentAdapter">AdapterEnum that stores the list of DisplayModes.</param>
        /// <returns>A List of Adapter Formats used in the supported DisplayModes.</returns>
        private ArrayList EnumerateDisplayModes( AdapterDetails a, AdapterEnum currentAdapter )
        {
            ArrayList adapterFormatList = new ArrayList();
            for ( int i = 0; i < AdapterFormats.Length; i++ )
            {
                foreach ( DisplayMode d in a.SupportedDisplayModes[AdapterFormats[i]] )
                {
                    currentAdapter.DisplayModeList.Add( d );

                    // Add Adaptor Format used with this DisplayMode
                    if ( !adapterFormatList.Contains( d.Format ) )
                    {
                        adapterFormatList.Add( d.Format );
                    }
                }
            }
            DisplayModeSorter sorter = new DisplayModeSorter();
            currentAdapter.DisplayModeList.Sort( sorter );
            return adapterFormatList;
        }

        /// <summary>Enumerates through all the Devices an Adapter can make.</summary>
        /// <param name="currentAdapter"></param>
        /// <param name="adapterFormatList">List of Adapter Formats supported by the Adapter</param>
        private void EnumerateDevices( AdapterEnum currentAdapter, ArrayList adapterFormatList )
        {
            // Get all the Devices this Adapter can make
            foreach( DeviceType t in DeviceTypes )
            {
                DeviceEnum currentDevice = new DeviceEnum();

                // Store the DeviceType and Capabilities
                currentDevice.DeviceType = t;
                try
                {
                    currentDevice.Capabilities = Manager.GetDeviceCapabilities( (int)currentAdapter.AdapterOrdinal, currentDevice.DeviceType );
                }
                catch( DirectXException )
                {
                    // DeviceType.Software throws an exception. Can ignore
                }

                // Find the supported settings for this device
                EnumerateDeviceSettings( currentDevice, currentAdapter, adapterFormatList );

                // Add the Device to the list if it has any valid settings
                if ( currentDevice.SettingsList.Count > 0)
                {
                    currentAdapter.DeviceEnumList.Add( currentDevice );
                }
            }
        }

        /// <summary>Enumerates device combinations for a particular device.</summary>
        /// <param name="currentDevice">Device to enumerate.</param>
        /// <param name="currentAdapter"></param>
        /// <param name="adapterFormatList">List of supported Adapter Formats</param>
        private void EnumerateDeviceSettings( DeviceEnum currentDevice, AdapterEnum currentAdapter, ArrayList adapterFormatList )
        {
            // Go through each Adapter Format
            foreach ( Format adapterFormat in AdapterFormats )
            {
                // Go through each BackBuffer Format
                foreach( Format backbufferFormat in BackBufferFormats )
                {
                    // Check both windowed and fullscreen modes
                    for ( int i = 0; i < 2; i++ )
                    {
                        bool windowed = (i == 1);

                        // Skip if this is not a supported Device type
                        if ( !Manager.CheckDeviceType( (int)currentAdapter.AdapterOrdinal, currentDevice.DeviceType,
                            adapterFormat, backbufferFormat, windowed ) )
                        {
                            continue;
                        }

                        DeviceSettingsEnum deviceSettings = new DeviceSettingsEnum();

                        // Store the information
                        deviceSettings.AdapterInformation = currentAdapter;
                        deviceSettings.DeviceInformation = currentDevice;
                        deviceSettings.AdapterOrdinal = currentAdapter.AdapterOrdinal;
                        deviceSettings.DeviceType = currentDevice.DeviceType;
                        deviceSettings.AdapterFormat = adapterFormat;
                        deviceSettings.BackBufferFormat = backbufferFormat;
                        deviceSettings.IsWindowed = windowed;

                        // Create the settings Arrays
                        EnumerateDepthStencilFormats( deviceSettings );
                        EnumerateMultiSampleTypes( deviceSettings );
                        EnumerateVertexProcessingTypes( deviceSettings );
                        EnumeratePresentIntervals( deviceSettings);

                        // Add the settings to the Device's settings list
                        currentDevice.SettingsList.Add( deviceSettings );
                    }
                }
            }
        }

        /// <summary>Enumerates through all the Depth Stencil Formats compatible with the Device.</summary>
        /// <param name="deviceSettings">DeviceEnum with the Device info.</param>
        private void EnumerateDepthStencilFormats( DeviceSettingsEnum deviceSettings )
        {
            // Check each DepthFormat
            foreach ( DepthFormat depthFormat in DepthFormats )
            {
                // Check if the DepthFormat is a valid surface format
                if ( Manager.CheckDeviceFormat( (int)deviceSettings.AdapterOrdinal, deviceSettings.DeviceType, 
                    deviceSettings.AdapterFormat, Usage.DepthStencil, ResourceType.Surface, depthFormat ) )
                {
                    // Check if the DepthFormat is compatible with the BackBufferFormat
                    if ( Manager.CheckDepthStencilMatch( (int)deviceSettings.AdapterOrdinal, deviceSettings.DeviceType, 
                        deviceSettings.AdapterFormat, deviceSettings.BackBufferFormat, depthFormat ) )
                    {
                        // Add it to the list
                        deviceSettings.DepthStencilFormatList.Add( depthFormat );
                    }
                }
            }
        }

        /// <summary>Enumerates through all the MultiSampleTypes that are compatible with the Device.</summary>
        /// <param name="deviceSettings">DeviceEnum with the Device info.</param>
        private void EnumerateMultiSampleTypes( DeviceSettingsEnum deviceSettings )
        {
            // Check each MultiSampleType
            foreach ( MultiSampleType t in MultiSampleTypes )
            {
                int qualityLevels;
                // See if it's supported
                if ( Manager.CheckDeviceMultiSampleType( (int)deviceSettings.AdapterOrdinal, deviceSettings.DeviceType, 
                    deviceSettings.BackBufferFormat, deviceSettings.IsWindowed, t, out qualityLevels ) )
                {
                    deviceSettings.MultiSampleTypeList.Add( t );
                    deviceSettings.MultiSampleQualityList.Add( qualityLevels );
                }
            }
        }

        /// <summary>Enumerates through the VertexProcessing Types that are compatible with the Device.</summary>
        /// <param name="deviceSettings">DeviceEnum with the Device info.</param>
        private void EnumerateVertexProcessingTypes( DeviceSettingsEnum deviceSettings )
        {
            // Best option is stored first
            // Check for hardware T&L
            if ( deviceSettings.DeviceInformation.Capabilities.DeviceCaps.SupportsHardwareTransformAndLight )
            {
                // Check for pure device
                if ( deviceSettings.DeviceInformation.Capabilities.DeviceCaps.SupportsPureDevice )
                {
                    deviceSettings.VertexProcessingTypeList.Add( CreateFlags.PureDevice | CreateFlags.HardwareVertexProcessing );
                }
                deviceSettings.VertexProcessingTypeList.Add( CreateFlags.HardwareVertexProcessing );
                deviceSettings.VertexProcessingTypeList.Add( CreateFlags.MixedVertexProcessing );
            }

            // Always supports software
            deviceSettings.VertexProcessingTypeList.Add( CreateFlags.SoftwareVertexProcessing );
        }

        /// <summary>Enumerates through the PresentIntervals that are compatible with the Device.</summary>
        /// <param name="deviceSettings">DeviceEnum with the Device info.</param>
        private void EnumeratePresentIntervals( DeviceSettingsEnum deviceSettings )
        {
            foreach ( PresentInterval p in PresentIntervals )
            {
                // If the device is windowed, skip all the intervals above one
                if ( deviceSettings.IsWindowed )
                {
                    if ( (p == PresentInterval.Two) || (p == PresentInterval.Three) ||
                        (p == PresentInterval.Four) )
                    {
                        continue;
                    }
                }

                if ( p == PresentInterval.Default )
                {
                    // Default interval is always available
                    deviceSettings.PresentIntervalList.Add( p );
                }
                
                // Check if the PresentInterval is supported
                if ( (deviceSettings.DeviceInformation.Capabilities.PresentInterval & p) != 0 )
                {
                    deviceSettings.PresentIntervalList.Add( p );
                }
            }
        }

        /// <summary>Finds the best windowed Device settings supported by the system.</summary>
        /// <returns>A DeviceSettings class full with the best supported windowed settings.</returns>
        private DeviceSettings FindBestWindowedSettings()
        {
            DeviceSettingsEnum bestSettings = null;
            bool foundBest = false;
            // Loop through each adapter
            foreach ( AdapterEnum a in m_adapters )
            {
                // Loop through each device
                foreach ( DeviceEnum d in a.DeviceEnumList )
                {
                    // Loop through each device settings configuration
                    foreach ( DeviceSettingsEnum s in d.SettingsList )
                    {
                        // Must be windowed mode and the AdapterFormat must match current DisplayMode Format
                        if ( !s.IsWindowed || (s.AdapterFormat != m_displayMode.Format) )
                        {
                            continue;
                        }

                        // The best DeviceSettingsEnum is a DeviceType.Hardware Device
                        // where its BackBufferFormat is the same as the AdapterFormat
                        if ( (bestSettings == null) || 
                             ((s.DeviceType == DeviceType.Hardware) && (s.AdapterFormat == s.BackBufferFormat)) ||
                             ((bestSettings.DeviceType != DeviceType.Hardware) && (s.DeviceType == DeviceType.Hardware)) )
                        {
                            if ( !foundBest )
                            {
                                bestSettings = s;
                            }

                            if ( (s.DeviceType == DeviceType.Hardware) && (s.AdapterFormat == s.BackBufferFormat) )
                            {
                                foundBest = true;
                            }
                        }
                    }
                }
            }
            if ( bestSettings == null )
            {
                throw new DirectXException( "Unable to find any supported window mode settings." );
            }
            // Store the best settings
            DeviceSettings windowedSettings = new DeviceSettings();
            windowedSettings.AdapterFormat = bestSettings.AdapterFormat;
            windowedSettings.AdapterOrdinal = bestSettings.AdapterOrdinal;
            windowedSettings.BehaviorFlags = (CreateFlags)bestSettings.VertexProcessingTypeList[0];
            windowedSettings.Capabilities = bestSettings.DeviceInformation.Capabilities;
            windowedSettings.DeviceType = bestSettings.DeviceType;

            windowedSettings.PresentParameters = new PresentParameters();
            windowedSettings.PresentParameters.AutoDepthStencilFormat = (DepthFormat)bestSettings.DepthStencilFormatList[0];
            windowedSettings.PresentParameters.BackBufferCount = 1;
            windowedSettings.PresentParameters.BackBufferFormat = bestSettings.AdapterFormat;
            windowedSettings.PresentParameters.BackBufferHeight = 0;
            windowedSettings.PresentParameters.BackBufferWidth = 0;
            windowedSettings.PresentParameters.DeviceWindowHandle = m_renderWindow.Handle;
            windowedSettings.PresentParameters.EnableAutoDepthStencil = true;
            windowedSettings.PresentParameters.FullScreenRefreshRateInHz = 0;
            windowedSettings.PresentParameters.MultiSampleType = (MultiSampleType)bestSettings.MultiSampleTypeList[0];
            windowedSettings.PresentParameters.MultiSampleQuality = 0;
            windowedSettings.PresentParameters.PresentationInterval = (PresentInterval)bestSettings.PresentIntervalList[0];
            windowedSettings.PresentParameters.PresentFlag = PresentFlag.DiscardDepthStencil;
            windowedSettings.PresentParameters.SwapEffect = SwapEffect.Discard;
            windowedSettings.PresentParameters.IsWindowed = true;

            return windowedSettings;
        }

        /// <summary>Finds the best fullscreen Device settings supported by the system.</summary>
        /// <returns>A DeviceSettings class full with the best supported fullscreen settings.</returns>
        private DeviceSettings FindBestFullscreenSettings()
        {
            DeviceSettingsEnum bestSettings = null;
            bool foundBest = false;

            // Loop through each adapter
            foreach ( AdapterEnum a in m_adapters )
            {
                // Loop through each device
                foreach ( DeviceEnum d in a.DeviceEnumList )
                {
                    // Loop through each device settings configuration
                    foreach ( DeviceSettingsEnum s in d.SettingsList )
                    {
                        // Must be fullscreen mode
                        if ( s.IsWindowed )
                        {
                            continue;
                        }

                        // To make things easier, we'll say the best DeviceSettingsEnum 
                        // is a DeviceType.Hardware Device whose AdapterFormat is the same as the
                        // current DisplayMode Format and whose BackBufferFormat matches the
                        // AdapterFormat
                        if ( (bestSettings == null) || 
                             ((s.DeviceType == DeviceType.Hardware) && (s.AdapterFormat == m_displayMode.Format)) ||
                             ((bestSettings.DeviceType != DeviceType.Hardware) && (s.DeviceType == DeviceType.Hardware)) )
                        {
                            if ( !foundBest )
                            {
                                bestSettings = s;
                            }

                            if ( (s.DeviceType == DeviceType.Hardware) &&
                                (s.AdapterFormat == m_displayMode.Format) &&
                                (s.BackBufferFormat == s.AdapterFormat) )
                            {
                                foundBest = true;
                            }
                        }
                    }
                }
            }
            if ( bestSettings == null )
            {
                throw new DirectXException( "Unable to find any supported fullscreen mode settings." );
            }

            // Store the best settings
            DeviceSettings fullscreenSettings = new DeviceSettings();
            fullscreenSettings.AdapterFormat = bestSettings.AdapterFormat;
            fullscreenSettings.AdapterOrdinal = bestSettings.AdapterOrdinal;
            fullscreenSettings.BehaviorFlags = (CreateFlags)bestSettings.VertexProcessingTypeList[0];
            fullscreenSettings.Capabilities = bestSettings.DeviceInformation.Capabilities;
            fullscreenSettings.DeviceType = bestSettings.DeviceType;

            fullscreenSettings.PresentParameters = new PresentParameters();
            fullscreenSettings.PresentParameters.AutoDepthStencilFormat = (DepthFormat)bestSettings.DepthStencilFormatList[0];
            fullscreenSettings.PresentParameters.BackBufferCount = 1;
            fullscreenSettings.PresentParameters.BackBufferFormat = bestSettings.AdapterFormat;
            fullscreenSettings.PresentParameters.BackBufferHeight = m_displayMode.Height;
            fullscreenSettings.PresentParameters.BackBufferWidth = m_displayMode.Width;
            fullscreenSettings.PresentParameters.DeviceWindowHandle = m_renderWindow.Handle;
            fullscreenSettings.PresentParameters.EnableAutoDepthStencil = true;
            fullscreenSettings.PresentParameters.FullScreenRefreshRateInHz = m_displayMode.RefreshRate;
            fullscreenSettings.PresentParameters.MultiSampleType = (MultiSampleType)bestSettings.MultiSampleTypeList[0];
            fullscreenSettings.PresentParameters.MultiSampleQuality = 0;
            fullscreenSettings.PresentParameters.PresentationInterval = (PresentInterval)bestSettings.PresentIntervalList[0];
            fullscreenSettings.PresentParameters.PresentFlag = PresentFlag.DiscardDepthStencil;
            fullscreenSettings.PresentParameters.SwapEffect = SwapEffect.Discard;
            fullscreenSettings.PresentParameters.IsWindowed = false;

            return fullscreenSettings;
        }

        /// <summary>Gets and sets whether the Device is in windowed mode</summary>
        public bool Windowed 
        {
            get { return m_windowed; }
            set 
            { 
                m_windowed = value; 
                if ( !m_windowed )
                {
                    // Going to fullscreen mode
                    m_currentSettings = (DeviceSettings)m_fullscreenSettings.Clone();
                }
                else                 {
                    // Going to window mode
                    m_currentSettings = (DeviceSettings)m_windowedSettings.Clone();
                }
            } 
        }
        public Device Device { get { return m_device; } }
        public Size FullscreenSize { get { return new Size( m_fullscreenSettings.PresentParameters.BackBufferWidth, m_fullscreenSettings.PresentParameters.BackBufferHeight ); } }
        public ArrayList Adapters { get { return m_adapters; } }


        /// <summary>Gets the current settings</summary>
        public DeviceSettings CurrentSettings 
        { 
            get { return (DeviceSettings)m_currentSettings.Clone(); } 
        }
        
        /// <summary>Gets and sets the windowed settings</summary>
        public DeviceSettings WindowedSettings 
        { 
            get { return (DeviceSettings)m_windowedSettings.Clone(); }
            set { m_windowedSettings = value; }
        }

        /// <summary>Gets and sets the fullscreen settings</summary>
        public DeviceSettings FullscreenSettings 
        { 
            get { return (DeviceSettings)m_fullscreenSettings.Clone(); } 
            set { m_fullscreenSettings = value; } 
        }
    }

    /// <summary>Represents a system adaptor.</summary>
    public class AdapterEnum
    {
        public uint AdapterOrdinal;
        public AdapterInformation AdapterInformation;
        public ArrayList DisplayModeList = new ArrayList();
        public ArrayList DeviceEnumList = new ArrayList();
        public string Description;
    }

    /// <summary>Represents the capabilities and description of a single device.</summary>
    public class DeviceEnum
    {
        public uint AdapterOrdinal;
        public DeviceType DeviceType;
        public Capabilities Capabilities;
        public ArrayList SettingsList = new ArrayList();
    }

    /// <summary>Represents the various settings a device can have.</summary>
    public class DeviceSettingsEnum
    {
        public uint AdapterOrdinal;
        public DeviceType DeviceType;
        public Format AdapterFormat;
        public Format BackBufferFormat;
        public bool IsWindowed;

        // Array lists
        public ArrayList DepthStencilFormatList = new ArrayList();
        public ArrayList MultiSampleTypeList = new ArrayList();
        public ArrayList MultiSampleQualityList = new ArrayList();
        public ArrayList PresentIntervalList = new ArrayList();
        public ArrayList VertexProcessingTypeList = new ArrayList();

        public AdapterEnum AdapterInformation = null;
        public DeviceEnum DeviceInformation = null;
    }

    /// <summary>Represents a Device with single settings configuration.</summary>
    public class DeviceSettings : ICloneable
    {
        public uint AdapterOrdinal;
        public Format AdapterFormat;
        public CreateFlags BehaviorFlags;
        public Capabilities Capabilities;
        public DeviceType DeviceType;
        public PresentParameters PresentParameters;

        /// <summary>Clones the settings</summary>
        /// <returns>The cloned settings</returns>
        public object Clone()
        {
            DeviceSettings settings = new DeviceSettings();
            settings.AdapterOrdinal = AdapterOrdinal;
            settings.AdapterFormat = AdapterFormat;
            settings.BehaviorFlags = BehaviorFlags;
            settings.Capabilities = Capabilities;
            settings.DeviceType = DeviceType;
            settings.PresentParameters = (PresentParameters)PresentParameters.Copy();
            return settings;
        }
    }

    /// <summary>Used to sort display modes</summary>
    public class DisplayModeSorter : IComparer
    {
        /// <summary>Compare two display modes</summary>
        public int Compare( Object x, Object y )
        {
            DisplayMode d1 = (DisplayMode)x;
            DisplayMode d2 = (DisplayMode)y;
            if ( d1.Width > d2.Width )
            {
                return 1;
            }
            if ( d1.Width < d2.Width )
            {
                return -1;
            }
            if ( d1.Height > d2.Height )
            {
                return 1;
            }
            if ( d1.Height < d2.Height )
            {
                return -1;
            }
            if ( d1.Format > d2.Format )
            {
                return 1;
            }
            if ( d1.Format < d2.Format )
            {
                return -1;
            }
            if ( d1.RefreshRate > d2.RefreshRate )
            {
                return 1;
            }
            if ( d1.RefreshRate < d2.RefreshRate )
            {
                return -1;
            }
            return 0;
        }
    }
}
