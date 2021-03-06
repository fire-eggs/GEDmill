/* CStats.cs
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

namespace GEDmill.HTMLClasses
{
    // Data structure to contain statistics about the website being created.
    public class CStats
    {
        // The number of individual records in the website.
        public uint m_unIndividuals;

        // The number of source records in the website.
        public uint m_unSources;

        // The number of multimedia files in the website.
        public uint m_unMultimediaFiles;

        // True if the website includes multimedia files other than pictures.
        public bool m_bNonPicturesIncluded;

        public CStats()
        {
            m_unIndividuals = 0;
            m_unSources = 0;
            m_unMultimediaFiles = 0;
            m_bNonPicturesIncluded = false;
        }
    }
}
