/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : StateManager.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Game state manager

Created :  01/21/2006
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
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.DirectX.Direct3D;

namespace CUnit
{
    public class StateManager
    {
        Stack<GameState> m_stack;

        /// <summary>Creates a new StateManager</summary>
        public StateManager()
        {
            m_stack = new Stack<GameState>();
        }

        /// <summary>Pushes a State onto the stack.</summary>
        /// <param name="state">New State.</param>
        public void Push( GameState state )
        {
            m_stack.Push( state );
        }

        /// <summary>Pops a State off the stack.</summary>
        /// <returns>The State popped or null if the stack is empty.</returns>
        public GameState Pop()
        {
            if ( m_stack.Count > 0 )
            {
                // Release resources before pop
                m_stack.Peek().OnLostDevice();
                m_stack.Peek().OnDestroyDevice();
                return m_stack.Pop();
            }
            return null;
        }

        /// <summary>Peeks the top of the stack.</summary>
        /// <returns>The State at the top of the Stack or null if the stack is empty.</returns>
        public GameState Peek()
        {
            if ( m_stack.Count > 0 )
            {
                return m_stack.Peek();
            }
            return null;
        }

        /// <summary>Call when the Device is created.</summary>
        /// <param name="device">D3D Device</param>
        public void OnCreateDevice( Device device )
        {
            foreach ( GameState s in m_stack )
            {
                s.OnCreateDevice( device );
            }
        }

        /// <summary>Call when the Device is reset.</summary>
        /// <param name="device">D3D Device</param>
        public void OnResetDevice( Device device )
        {
            foreach ( GameState s in m_stack )
            {
                s.OnResetDevice( device );
            }
        }

        /// <summary>Call when the Device is lost.</summary>
        public void OnLostDevice()
        {
            foreach ( GameState s in m_stack )
            {
                s.OnLostDevice();
            }
        }

        /// <summary>Call when the Device is destroyed.</summary>
        public void OnDestroyDevice()
        {
            foreach ( GameState s in m_stack )
            {
                s.OnDestroyDevice();
            }
        }

        /// <summary>Updates the frame prior to rendering.</summary>
        /// <param name="device">D3D Device</param>
        /// <param name="elapsedTime">Time since last frame</param>
        public void OnUpdateFrame( Device device, float elapsedTime )
        {
            if ( m_stack.Count > 0 )
            {
                m_stack.Peek().OnUpdateFrame( device, elapsedTime );
                // Check if the State is finished
                if ( m_stack.Peek().DoneWithState )
                {
                    Pop();
                }
            }
        }

        /// <summary>Renders the current frame of the current state.</summary>
        /// <param name="device">D3D Device</param>
        /// <param name="elapsedTime">Time since last frame</param>
        public void OnRenderFrame( Device device, float elapsedTime )
        {
            if ( m_stack.Count > 0 )
            {
                m_stack.Peek().OnRenderFrame( device, elapsedTime );
            }
        }

        /// <summary>Keyboard handler.</summary>
        /// <param name="pressedKeys">List of pressed keys.</param>
        /// <param name="pressedChar">Character read from keyboard.</param>
        /// <param name="pressedKey">Keycode read from keyboard.</param>
        /// <param name="elapsedTime">Time since last frame</param>
        public void OnKeyboard( List<Keys> pressedKeys, char pressedChar, int pressedKey, float elapsedTime )
        {
            if ( m_stack.Count > 0 )
            {
                m_stack.Peek().OnKeyboard( pressedKeys, pressedChar, pressedKey, elapsedTime );
            }
        }

        /// <summary>Mouse handler.</summary>
        /// <param name="position">Mouse position in client coordinates</param>
        /// <param name="xDelta">X-axis delta.</param>
        /// <param name="yDelta">Y-axis delta.</param>
        /// <param name="zDelta">Wheel delta.</param>
        /// <param name="buttons">Mouse button state.</param>
        /// <param name="elapsedTime">Time since last frame</param>
        public void OnMouse( Point position, int xDelta, int yDelta, int zDelta, bool[] buttons, float elapsedTime )
        {
            if ( m_stack.Count > 0 )
            {
                m_stack.Peek().OnMouse( position, xDelta, yDelta, zDelta, buttons, elapsedTime );
            }
        }

        /// <summary>GUI handler.</summary>
        /// <param name="controlID">Control ID</param>
        /// <param name="data">Control data</param>
        public void OnControl( int controlID, object data )
        {
            if ( m_stack.Count > 0 )
            {
                m_stack.Peek().OnControl( controlID, data );
            }
        }
    }

    /// <summary>A game state.</summary>
    /// <remarks>Contains a method for each stage of execution.</remarks>
    public abstract class GameState
    {
        public abstract void OnCreateDevice( Device device );
        public abstract void OnResetDevice( Device device );
        public abstract void OnLostDevice();
        public abstract void OnDestroyDevice();
        public abstract void OnUpdateFrame( Device device, float elapsedTime );
        public abstract void OnRenderFrame( Device device, float elapsedTime );
        public abstract void OnKeyboard( List<Keys> keyState, char pressedChar, int pressedKey, 
            float elapsedTime );
        public abstract void OnMouse( Point position, int xDelta, int yDelta, int zDelta, 
            bool[] buttons, float elapsedTime );
        public abstract void OnControl( int controlID, object data );
        public abstract bool DoneWithState
        {
            get;
        }
    }
}