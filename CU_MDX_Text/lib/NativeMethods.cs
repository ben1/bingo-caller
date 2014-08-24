/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : NativeMethods.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Native methods

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
using System.Runtime.InteropServices;

namespace CUnit
{
	/// <summary>
	/// Summary description for NativeMethods.
	/// </summary>
    public class NativeMethods
    {
        /// <summary>Windows Message</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr hWnd;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet=CharSet.Auto)]
        public static extern bool PeekMessage( out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags );
        
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32")]
        public static extern bool QueryPerformanceFrequency( ref long PerformanceFrequency );

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32")]
        public static extern bool QueryPerformanceCounter( ref long PerformanceCount );
    }
}
