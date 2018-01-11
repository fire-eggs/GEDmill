/* CRecord.cs
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
using SharpGEDParser.Model;

namespace GEDmill.LLClasses
{
    // Base class for the various records used in GEDCOM (individual, source etc.)
    public class CRecord : CParserObject
    {
        // XREF, a unique identifier for the record
        public string m_xref;

        // GEDCOM user reference numbers
        public ArrayList m_alUserReferenceNumbers;

        // GEDCOM automated record id
        public string m_sAutomatedRecordId;

        // GEDCOM change date
        public CChangeDate m_changeDate;

        // Constructor
        public CRecord( CGedcom gedcom ) : base( gedcom )
        {
            m_alUserReferenceNumbers = new ArrayList();
        }

        public static void Translate(CRecord rec, GEDCommon yagp)
        {
            // Common yagp translation
            rec.m_xref = yagp.Ident;

            rec.m_sAutomatedRecordId = yagp.RIN;

            foreach (var refN in yagp.REFNs)
            {
                CUserReferenceNumber urn = new CUserReferenceNumber(rec.Gedcom);
                urn.m_sUserReferenceNumber = refN.Value;
                urn.m_sUserReferenceType = ""; // KBR TODO yagp doesn't yet handle REFN.TYPE !
                rec.m_alUserReferenceNumbers.Add(urn);
            }

            // KBR TODO html output parses the date which we just converted, then uses ToString on it ...
            if (yagp.CHAN.Date.HasValue)
            {
                rec.m_changeDate = new CChangeDate(rec.Gedcom);
                rec.m_changeDate.m_sChangeDate = yagp.CHAN.Date.Value.ToString("dd MMM yyyy");
                rec.m_changeDate.m_sTimeValue = ""; // won't accept null
            }
        }

        // Parser
        public bool ParseRecord( CGedcom gedcom, int level )
        {
            CGedcomLine line;
            CUserReferenceNumber urn;
            CChangeDate cd;
            bool bParsingFinished;
            bool bGotSomething = false;
            do
            {
                bParsingFinished = true;

                if( (line = gedcom.GetLine( level+1, "RIN" )) != null )
                {
                    m_sAutomatedRecordId = line.LineItem;
                    gedcom.IncrementLineIndex(1);
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (urn = CUserReferenceNumber.Parse( gedcom, level+1 )) != null )
                {
                    m_alUserReferenceNumbers.Add( urn );
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (cd = CChangeDate.Parse( gedcom, level+1 )) != null )
                {
                    m_changeDate = cd;
                    bParsingFinished = false;
                    bGotSomething = true;
                }
            }
            while( !bParsingFinished );

            return bGotSomething;
        }

    } // End of class
} // End of namespace
