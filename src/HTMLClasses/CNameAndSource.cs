/* CNameAndSource.cs
 * 
 * Copyright 2009 Alexander Curtis <alex@logicmill.com>
 * This file is part of GEDmill - A family history website creator
 * 
 * GEDmill is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * GEDmill is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with GEDmill.  If not, see <http://www.gnu.org/licenses/>.
 *
 *
 * History:  
 * 10Dec08 AlexC          Migrated from GEDmill 1.10
 *
 */

using System;
using System.Collections;

namespace GEDmill
{
    // A data structure to encapsulate an individual's name and source records that confirm it.
    // Also contains the HTML for the source reference superscript numbers for this particular name.
    // Used as an element in the list of all an individual's names.
    public class CNameAndSource
    {
        public ArrayList m_alSources;
        public string m_sName;
        public string m_sSourceHtml;

        // Noddy constructor
        public CNameAndSource( string name )
        {
            m_sSourceHtml = "";
            m_sName = name;
            m_alSources = new ArrayList();
        }
    }
}
