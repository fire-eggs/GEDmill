/* CEventDetail.cs
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

// ReSharper disable InconsistentNaming
// ReSharper disable StringCompareToIsCultureSpecific
// ReSharper disable UseObjectOrCollectionInitializer

namespace GEDmill.LLClasses
{
    // GEDCOM Event Detail. See GEDCOM standard for details on GEDCOM data.
    public class CEventDetail : CParserObject
    {
        // GEDCOM data
        public string m_sEventOrFactClassification;
        public CPGDate m_dateValue;     
        public CPlaceStructure m_placeStructure;        
        public CAddressStructure m_addressStructure;
        public string m_sResponsibleAgency;     
        public string m_sReligiousAffiliation;      
        public string m_sCauseOfEvent;      
        public string m_sRestrictionNotice;     
        public ArrayList m_alNoteStructures;        
        public ArrayList m_alSourceCitations;       
        public ArrayList m_alMultimediaLinks;       
        public string m_sAgeAtEvent;        
        public string m_sHusbandAgeAtEvent;     
        public string m_sWifeAgeAtEvent;        
        public string m_xrefFam;        
        public bool m_bAdoptedByHusband;        
        public bool m_bAdoptedByWife;       
        public string m_xrefAdoptedChild;
        public string m_sAlternativePlace;

        // From _OVER tag (From Genesconnected I believe)
        public string m_sOverview; 

        // Constructor
        public CEventDetail( CGedcom gedcom ) : base( gedcom )
        {
            m_alSourceCitations = new ArrayList();
            m_alNoteStructures = new ArrayList();
            m_alMultimediaLinks = new ArrayList();
        }

        // Copy constructor
        public CEventDetail( CEventDetail ed ) : base( ed.Gedcom )
        {
            m_sEventOrFactClassification = ed.m_sEventOrFactClassification;

            m_dateValue = ed.m_dateValue != null ? new CPGDate( ed.m_dateValue ) : null;
            m_placeStructure = ed.m_placeStructure != null ? new CPlaceStructure( ed.m_placeStructure ) : null;
            m_addressStructure = ed.m_addressStructure != null ? new CAddressStructure( ed.m_addressStructure ) : null; 
                  
            m_sResponsibleAgency = ed.m_sResponsibleAgency;
            m_sReligiousAffiliation = ed.m_sReligiousAffiliation;
            m_sCauseOfEvent = ed.m_sCauseOfEvent;
            m_sRestrictionNotice = ed.m_sRestrictionNotice;
            m_alNoteStructures = new ArrayList();
            foreach( CNoteStructure ns in ed.m_alNoteStructures )
            {
                m_alNoteStructures.Add( ns.CopyConstructor() );
            }
            m_alSourceCitations = new ArrayList();
            foreach( CSourceCitation sc in ed.m_alSourceCitations )
            {
                m_alSourceCitations.Add( sc.CopyConstructor() );
            }
            m_alMultimediaLinks = new ArrayList();
            foreach( CMultimediaLink ml in ed.m_alMultimediaLinks )
            {
                m_alMultimediaLinks.Add( ml.CopyConstructor() );
            }
            m_sAgeAtEvent = ed.m_sAgeAtEvent;
            m_sHusbandAgeAtEvent = ed.m_sHusbandAgeAtEvent;
            m_sWifeAgeAtEvent = ed.m_sWifeAgeAtEvent;
            m_xrefFam = ed.m_xrefFam;
            m_bAdoptedByHusband = ed.m_bAdoptedByHusband;
            m_bAdoptedByWife = ed.m_bAdoptedByWife;
            m_xrefAdoptedChild = ed.m_xrefAdoptedChild;
            m_sAlternativePlace = ed.m_sAlternativePlace;
        }

        public static CEventDetail Translate(CGedcom gedcom, EventCommon ev)
        {
            CEventDetail ed = new CEventDetail(gedcom);
            ed.Type = ev.Tag;
            if (!string.IsNullOrEmpty(ev.Date))
                ed.m_dateValue = CPGDate.Parse(ev.Date.Trim());
            ed.m_placeStructure = CPlaceStructure.Translate(gedcom, ev);
            ed.m_addressStructure = CAddressStructure.Translate(gedcom, ev.Address);
            ed.m_sCauseOfEvent = ev.Cause;

            foreach (var sourceCit in ev.Cits)
            {
                CSourceCitation sc = CSourceCitation.Translate(gedcom, sourceCit);
                ed.m_alSourceCitations.Add(sc);
            }

            foreach (var note in ev.Notes)
            {
                CNoteStructure ns = CNoteStructure.Translate(gedcom, note);
                ed.m_alNoteStructures.Add(ns);
            }

            return ed;
        }

        // Parser
        public static CEventDetail Parse( CGedcom gedcom, int nLevel )
        {
            bool bParsingFinished;
            bool bGotSomething;

            // Temporary holders for class members.
            string sEventOrFactClassification = "";
            CPGDate dateValue = null;
            CPlaceStructure ps, placeStructure = null;
            CAddressStructure ads, addressStructure = null;
            string sResponsibleAgency = "";
            string sReligiousAffiliation = "";
            string sCauseOfEvent = "";
            string sRestrictionNotice = "";
            ArrayList alNoteStructures = new ArrayList();
            ArrayList alSourceCitations = new ArrayList();
            ArrayList alMultimediaLinks = new ArrayList();      
            string sAgeAtEvent = "";
            string sHusbandAgeAtEvent = "";
            string sWifeAgeAtEvent = "";
            string xrefFam="";
            bool bAdoptedByHusband = false;
            bool bAdoptedByWife = false;
            string sPlaceAlternative = "";
            string sOverview = "";


            CSourceCitation sc;
            CMultimediaLink ml;

            bGotSomething = false;
            do
            {
                bParsingFinished = true;

                CGedcomLine gedcomLine;
                if( (gedcomLine = gedcom.GetLine(nLevel, "FAMC")) != null )
                {
                    xrefFam = gedcomLine.LinePointer;
                    gedcom.IncrementLineIndex(1);
                    // Test for underscore items first so that parser doesn't skip them later
                    if( (gedcomLine = gedcom.GetLine(nLevel, "_PLAC")) != null )
                    {
                        sPlaceAlternative = gedcomLine.LineItem;
                        gedcom.IncrementLineIndex(1);
                        bGotSomething = true;
                    }
                    else if( (gedcomLine = gedcom.GetLine(nLevel, "_OVER")) != null )
                    {
                        sOverview = gedcomLine.LineItem;
                        gedcom.IncrementLineIndex(1);

                        bool bParsingFinished3;
                        do
                        {
                            bParsingFinished3 = true;
                            if( (gedcomLine = gedcom.GetLine(nLevel+1, "CONC")) != null )
                            {
                                sOverview += gedcomLine.LineItem;
                                gedcom.IncrementLineIndex(1);
                                bParsingFinished3 = false;
                            }
                            else if( (gedcomLine = gedcom.GetLine(nLevel+1, "CONT")) != null )
                            {
                                sOverview += "\n" + gedcomLine.LineItem;
                                gedcom.IncrementLineIndex(1);
                                bParsingFinished3 = false;
                            }
                        }
                        while( !bParsingFinished3 );                    
                        
                        bGotSomething = true;
                    }
                    else if( (gedcomLine = gedcom.GetLine(nLevel+1, "ADOP")) != null )
                    {
                        string adoptedByWhichParent = gedcomLine.LineItem;
                        if( adoptedByWhichParent != null && adoptedByWhichParent.Length >= 4 )
                        {
                            try
                            {
                                if( adoptedByWhichParent.Substring(0,4).ToUpper().CompareTo( "BOTH" ) == 0 )
                                {
                                    bAdoptedByHusband = true;
                                    bAdoptedByWife = true;
                                }
                                else if( adoptedByWhichParent.Substring(0,4).ToUpper().CompareTo( "HUSB" ) == 0 )
                                {
                                    bAdoptedByHusband = true;
                                }
                                else if( adoptedByWhichParent.Substring(0,4).ToUpper().CompareTo( "WIFE" ) == 0 )
                                {
                                    bAdoptedByWife = true;
                                }
                            }
                            catch( ArgumentOutOfRangeException )
                            {
                                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Warning, String.Format("Unusual ADOP parameter :{0}", adoptedByWhichParent) );
                            }
                        }
                        gedcom.IncrementLineIndex(1);
                    }               
                    bParsingFinished = false;
                }               
                else if( (gedcomLine = gedcom.GetLine(nLevel, "TYPE")) != null )
                {
                    sEventOrFactClassification = gedcomLine.LineItem;
                    gedcom.IncrementLineIndex(1);
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (gedcomLine = gedcom.GetLine(nLevel, "DATE")) != null )
                {
                    
                    dateValue = CPGDate.Parse( gedcomLine.LineItem );
                    gedcom.IncrementLineIndex(1);
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (ps = CPlaceStructure.Parse( gedcom, nLevel )) != null )
                {
                    placeStructure = ps;
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (ads = CAddressStructure.Parse( gedcom, nLevel )) != null )
                {
                    addressStructure = ads;
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (gedcomLine = gedcom.GetLine(nLevel, "AGNC")) != null )
                {
                    sResponsibleAgency = gedcomLine.LineItem;
                    gedcom.IncrementLineIndex(1);
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (gedcomLine = gedcom.GetLine(nLevel, "RELI")) != null )
                {
                    sReligiousAffiliation = gedcomLine.LineItem;
                    gedcom.IncrementLineIndex(1);
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (gedcomLine = gedcom.GetLine(nLevel, "CAUS")) != null )
                {
                    sCauseOfEvent = gedcomLine.LineItem;
                    gedcom.IncrementLineIndex(1);
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (gedcomLine = gedcom.GetLine(nLevel, "RESN")) != null )
                {
                    sRestrictionNotice = gedcomLine.LineItem;
                    gedcom.IncrementLineIndex(1);
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else if( (sc = CSourceCitation.Parse( gedcom, nLevel )) != null )
                {
                    alSourceCitations.Add( sc );
                    bParsingFinished = false;
                    bGotSomething = true;
                }               
                else if( (ml = CMultimediaLink.Parse( gedcom, nLevel )) != null )
                {
                    alMultimediaLinks.Add( ml );
                    bParsingFinished = false;
                    bGotSomething = true;
                }
                else
                {
                    CNoteStructure ns;
                    if( (ns = CNoteStructure.Parse( gedcom, nLevel )) != null )
                    {
                        alNoteStructures.Add( ns );
                        bParsingFinished = false;
                        bGotSomething = true;
                    }
                    else if( (gedcomLine = gedcom.GetLine(nLevel, "AGE")) != null )
                    {
                        sAgeAtEvent = gedcomLine.LineItem;
                        gedcom.IncrementLineIndex(1);
                        bParsingFinished = false;
                        bGotSomething = true;
                    }
                    else if( (gedcomLine = gedcom.GetLine(nLevel, "HUSB")) != null )
                    {
                        gedcom.IncrementLineIndex(1);
                        if( (gedcomLine = gedcom.GetLine(nLevel+1, "AGE")) != null )
                        {
                            sHusbandAgeAtEvent = gedcomLine.LineItem;
                            gedcom.IncrementLineIndex(1);
                            bParsingFinished = false;
                            bGotSomething = true;
                        }
                    }               
                    else if( (gedcomLine = gedcom.GetLine(nLevel, "WIFE")) != null )
                    {
                        gedcom.IncrementLineIndex(1);
                        if( (gedcomLine = gedcom.GetLine(nLevel+1, "AGE")) != null )
                        {
                            sWifeAgeAtEvent = gedcomLine.LineItem;
                            gedcom.IncrementLineIndex(1);
                            bParsingFinished = false;
                            bGotSomething = true;
                        }
                    }               
                    else if( ( gedcomLine = gedcom.GetLine()).Level >= nLevel )
                    {
                        LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Warning, "Unknown tag :" );
                        LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Warning, gedcomLine.ToString() );
                        gedcom.IncrementLineIndex(1);
                        bParsingFinished = false;
                    }
                }
            }
            while( !bParsingFinished );

            if( !bGotSomething )
            {
                return null;
            }

            CEventDetail ed = new CEventDetail( gedcom );
            ed.m_sEventOrFactClassification = sEventOrFactClassification;
            ed.m_dateValue = dateValue;
            ed.m_placeStructure = placeStructure;
            ed.m_addressStructure = addressStructure;
            ed.m_sResponsibleAgency = sResponsibleAgency;
            ed.m_sReligiousAffiliation = sReligiousAffiliation;
            ed.m_sCauseOfEvent = sCauseOfEvent;
            ed.m_sRestrictionNotice = sRestrictionNotice;
            ed.m_alNoteStructures = alNoteStructures;
            ed.m_alSourceCitations = alSourceCitations;
            ed.m_alMultimediaLinks = alMultimediaLinks;     
            ed.m_sAgeAtEvent = sAgeAtEvent;
            ed.m_sHusbandAgeAtEvent = sHusbandAgeAtEvent;
            ed.m_sWifeAgeAtEvent = sWifeAgeAtEvent;
            ed.m_xrefFam = xrefFam;
            ed.m_bAdoptedByHusband = bAdoptedByHusband;
            ed.m_bAdoptedByWife = bAdoptedByWife;
            ed.m_xrefAdoptedChild = "";
            ed.m_sAlternativePlace = sPlaceAlternative;
            ed.m_sOverview = sOverview;

            return ed;
        }
    }
}
