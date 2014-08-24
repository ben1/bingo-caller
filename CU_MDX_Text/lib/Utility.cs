/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

Title : Utility.cs
Author : Chad Vernon
URL : http://www.c-unit.com

Description : Utility methods. GetMediaFile and related methods taken from the 
DirectX Sample Framework.

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
using System.IO;
using Microsoft.DirectX;

namespace CUnit
{
	/// <summary>
	/// Summary description for Utility.
	/// </summary>
	public class Utility
	{
        // Constants for search folders
        private const string mediaFolder = @"media\";
        private static readonly string[] searchFolders = new string[] 
        { 
            @".\", @"..\", @"..\..\", @"{0}\", @"{0}\..\", @"{0}\..\..\", @"{0}\..\{1}\", @"{0}\..\..\{1}\" 
        };

        /// <summary>
        /// Constructor
        /// </summary>
		public Utility()
		{
            // Empty
		}

        /// <summary>
        /// Finds a media file. Adapted from the DirectX Sample Framework.
        /// </summary>
        /// <param name="file">Name of the file we're looking for.</param>
        /// <returns>The path of the file.</returns>
        /// <remarks>If the file cannot be found, an exception will be thrown.</remarks>
        public static string GetMediaFile( string file )
        {
            // Find out the executing assembly information
            System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string exeName = Path.GetFileNameWithoutExtension( executingAssembly.Location );
            string exeFolder = Path.GetDirectoryName( executingAssembly.Location );
            string filePath;

            // Search all the folders in searchFolders
            if ( SearchFolders( file, exeFolder, exeName, out filePath ) )
            {
                return filePath;
            }

            // Search all the folders in searchFolders with media\ appended to the end
            if ( SearchFolders( mediaFolder + file, exeFolder, exeName, out filePath ) )
            {
                return filePath;
            }

            throw new Exception( "Unable to find media file: " + file );
        }

        /// <summary>
        /// Searches the list of folders for the file. From the DirectX Sample Framework.
        /// </summary>
        /// <param name="filename">File we are looking for</param>
        /// <param name="exeFolder">Folder of the executable</param>
        /// <param name="exeName">Name of the executable</param>
        /// <param name="fullPath">Returned path if file is found.</param>
        /// <returns>true if the file was found; false otherwise</returns>
        private static bool SearchFolders( string filename, string exeFolder, string exeName, out string fullPath )
        {
            // Look through each folder to find the file
            for ( int i = 0; i < searchFolders.Length; i++ )
            {
                try
                {
                    FileInfo info = new FileInfo( string.Format( searchFolders[i], exeFolder, exeName) + filename );
                    if ( info.Exists )
                    {
                        fullPath = info.FullName;
                        return true;
                    }
                }
                catch ( NotSupportedException )
                {
                    continue;
                }
            }
            // Crap...didn't find it
            fullPath = string.Empty;
            return false;
        }
	}
}
