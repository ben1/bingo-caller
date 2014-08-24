/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Framework.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Framework state variables

Created :  01/14/2005
Modified : 01/14/2005

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
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace CUnit
{
    public struct FrameworkState
    {
        public bool Initialized;
        public bool DeviceLost;
        public bool FormClosing;
        public bool DisableResize;
        public bool DeviceOptionsShowing;
        public bool RenderingPaused;
        public int RenderingPausedCount;
        public int TimerPausedCount;
        public bool TimerPaused;
        public Point WindowLocation;
        public Size WindowSize;
        public FillMode FillMode;
    }
}
