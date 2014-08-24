/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : ListableItem.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : ListableItem for Controls that list items.

Created :  12/29/2005
Modified : 12/29/2005

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
using System.Text;
using Microsoft.DirectX;

namespace CUnit.Gui
{
    /// <summary>ListableItem for Controls that list items.</summary>
    public class ListableItem
    {
        private string m_text;
        private object m_data;
        private bool m_selected;
        private ColorValue m_textColor;

        /// <summary>Creates a new ListableItem.</summary>
        /// <param name="text">Text to display</param>
        /// <param name="data">Data for item to hold</param>
        public ListableItem( string text, object data )
        {
            m_text = text;
            m_data = data;
        }

        /// <summary>Gets the text of the ListableItem</summary>
        public string Text
        {
            get { return m_text; }
        }

        /// <summary>Gets and sets whether the ListableItem is selected.</summary>
        public bool Selected
        {
            get { return m_selected; }
            set { m_selected = value; }
        }

        /// <summary>Gets and sets the text color of the ListableItem</summary>
        public ColorValue TextColor
        {
            get { return m_textColor; }
            set { m_textColor = value; }
        }

        /// <summary>Gets the data of the ListableItem</summary>
        public object Data
        {
            get { return m_data; }
        }
    }
}
