/* CGedcom.cs
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

using GEDmill.LLClasses;
using SharpGEDParser;
using SharpGEDParser.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable InconsistentNaming
// ReSharper disable SpecifyACultureInStringConversionExplicitly


namespace GEDmill
{
    // The character set that the incoming GEDCOM file may use
    public enum ECharset 
    {
        Unknown = 0,
        Unknown8bit = 1,
        Unicode = 2,
        UnicodeReversed = 3,
        Ascii = 4,
        Ansel = 5,
        Ansi = 6,
        UTF8 = 7,
        ISO8859_1 = 8
    };

    // CGedcom class parses and stores information from a GEDCOM file.
    public class CGedcom
    {
        // The GEDCOM filename
        private string m_sFilename;

        // The Windows form that contains the progress bar
        private IProgressCallback m_progressWindow;

        // True if whitespace is significant at the start of values associated with GEDCOM tags
        private bool m_bDataMayStartWithWhitespace;

        // True if whitespace is significant at the end of values associated with GEDCOM tags
        private bool m_bDataMayEndWithWhitespace;

        // Table used to mark records visited during the pruning process (pruning = selecting individuals to include/exclude from website)
        private Hashtable m_htVisited;

        // The tokenised lines of GEDCOM
        private ArrayList m_alLines;

        // The currently parsed line
        private int m_nLineIndex;

        // Count of bytes read
//        private long m_nBytesRead;

        // Count of bytes in total
//        private long m_nBytesTotal;

        // For decoding BLOBs
        private Hashtable m_htDecoding;

        // List of temp files created for blobs
        private ArrayList m_alTemporaryFiles;

        // For efficient lookups of CFamilyRecord from xref string.
        private Hashtable m_htFamilyRecordsXref;

        // For efficient lookups of CIndividualRecord from xref string.
        private Hashtable m_htIndividualRecordsXref;

        // For efficient lookups of CSourceRecord from xref string.
        private Hashtable m_htSourceRecordsXref;

        // GEDCOM data
        public CSubmissionRecord m_submissionRecord;
        public ArrayList m_alMultimediaRecords;
        public ArrayList m_alNoteRecords;
        public ArrayList m_alRepositoryRecords;     
        public ArrayList m_alSubmitterRecords;
        public ArrayList m_alAdoptedIndividuals;

        public List<CIndividualRecord> IndividualRecords { get; set; }
        public List<CSourceRecord> SourceRecords { get; set; }
        private List<CFamilyRecord> FamilyRecords { get; set; }

        // Constructor
        public CGedcom()
        {
        
            ClearOutParser();
            m_sFilename = "";
            m_progressWindow = null;
            m_htVisited = null;

            // Set up alphabet for decoding BLOB objects.
            m_htDecoding = new Hashtable();
            string sValidChars = "./0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            byte i = 0;
            foreach( char c in sValidChars )
            {
                m_htDecoding[c] = i++;
            }

            m_alTemporaryFiles = new ArrayList();
            m_alAdoptedIndividuals = new ArrayList();
        }

        // Destructor
        ~CGedcom()
        {
            // Remove all temporary files
            foreach( string filename in m_alTemporaryFiles )
            {
                try
                {
                    File.Delete( filename );
                }
                catch( IOException )
                {}
            }
        }

        // Initialise all data of gedcom parser
        private void ClearOutParser()
        {
            m_alLines = new ArrayList();
            m_nLineIndex = 0;
            m_htFamilyRecordsXref = new Hashtable(); 

            IndividualRecords = new List<CIndividualRecord>();
            SourceRecords = new List<CSourceRecord>();
            FamilyRecords = new List<CFamilyRecord>();

            m_htIndividualRecordsXref = new Hashtable();
            m_alMultimediaRecords = new ArrayList();
            m_alNoteRecords = new ArrayList();
            m_alRepositoryRecords = new ArrayList();
            m_htSourceRecordsXref = new Hashtable();
            m_alSubmitterRecords = new ArrayList();
            m_submissionRecord = null;
            m_bDataMayStartWithWhitespace = true;
        }

        private void addIndi(IndiRecord yagpIndi)
        {
            CIndividualRecord cir = CIndividualRecord.Translate(this, yagpIndi);
            IndividualRecords.Add(cir);
            m_htIndividualRecordsXref.Add(cir.m_xref, cir);
        }

        private void addFam(FamRecord yagpFam)
        {
            CFamilyRecord cfr = CFamilyRecord.Translate(this, yagpFam);
            FamilyRecords.Add(cfr);
            m_htFamilyRecordsXref.Add(cfr.m_xref, cfr);
        }

        private void addSource(SourceRecord yagpSour)
        {
            CSourceRecord cir = CSourceRecord.Translate(this, yagpSour);
            SourceRecords.Add(cir);
            m_htSourceRecordsXref.Add(cir.m_xref, cir);
        }

        private void addNote(NoteRecord yagpNote)
        {
            CNoteRecord nr = CNoteRecord.Translate(this, yagpNote);
            m_alNoteRecords.Add(nr);
        }

        private void addRepo(Repository yagp)
        {
            CRepositoryRecord rr = CRepositoryRecord.Translate(this, yagp);
            m_alRepositoryRecords.Add(rr);
        }

        private void addMedia(MediaRecord yagp)
        {
            CMultimediaRecord mr = CMultimediaRecord.Translate(this, yagp);
            m_alMultimediaRecords.Add(mr);
        }

        private void Translate()
        {
            foreach (var gedCommon in _yagp.Data)
            {
                if (gedCommon is IndiRecord)
                {
                    addIndi(gedCommon as IndiRecord);
                }
                else if (gedCommon is FamRecord)
                {
                    addFam(gedCommon as FamRecord);
                }
                else if (gedCommon is SourceRecord)
                {
                    addSource(gedCommon as SourceRecord);
                }
                else if (gedCommon is NoteRecord)
                {
                    addNote(gedCommon as NoteRecord);
                }
                else if (gedCommon is Repository)
                {
                    addRepo(gedCommon as Repository);
                }
                else if (gedCommon is MediaRecord)
                {
                    addMedia(gedCommon as MediaRecord);
                }
            }
        }

        private void ConvertMediaLinks()
        {
            // Create a list of MFRs unique to the individual
            LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Ordering individual's mfrs.");
            foreach (CIndividualRecord irMultimedia in IndividualRecords)
            {
                ConvertMultimediaLinks(irMultimedia.m_alMultimediaLinks, ref irMultimedia.m_alUniqueFileRefs);
            }
            // Create a list of MFRs unique to the source
            LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Ordering source's mfrs.");
            foreach (CSourceRecord srMultimedia in SourceRecords)
            {
                ConvertMultimediaLinks(srMultimedia.m_alMultimediaLinks, ref srMultimedia.m_alUniqueFileRefs);
            }
        }

        private FileRead _yagp;

        // Reads a GEDCOM file into a hierarchy of data structures
        public void ParseFile()
        {
            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note,  "ParseFile()" );

            CThreadError threaderror = new CThreadError(4, "No error"); // 2 = process was aborted, for signalling back to calling thread. 1= cancelled by user action
            ClearOutParser();
            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "ParseFile() ClearOutParser" ); 

            _yagp = new FileRead();
            _yagp.ReadGed(m_sFilename);
            LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "YAGP read complete");
            Translate();
            LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "YAGP translate complete");

            LinkAdoptees();
            AddBackRefs();
            JoinAllMmedia();
            ConvertMediaLinks();
            AddChildrenToFamilies();

            if (m_progressWindow != null)
            {
                LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Closing progress window");
                m_progressWindow.End(threaderror);
            }

            LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "All done.");

#if false
            string sParseLine="";
            FileStream fileStream = null;
            StreamReader streamReader = null;
            m_nLineIndex = -1; // Used to indicate to exception handling that stage2 parsing hasn't started

            try
            {
                m_progressWindow.Begin( 0, 100 );
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Progress window begun" );

                // Discern file character set by reading first 2 bytes of file
                FileStream fsCharsetDetection = new FileStream( Filename, FileMode.Open, FileAccess.Read );
                StreamReader srCharsetDetection = null;
                try
                {
                    byte[] abFirstBytes = new byte[2];
                    fsCharsetDetection.Read( abFirstBytes, 0, 2 );
                    fsCharsetDetection.Seek( 0, SeekOrigin.Begin );

                    m_ecCharset = ECharset.Unknown8bit;
                    string sCharset = "ANSI";
                    if( (abFirstBytes[0] == 0xff && abFirstBytes[1] == 0xfe) || (abFirstBytes[0] == 0x30 && abFirstBytes[1] == 0x00) )
                    {
                        m_ecCharset = ECharset.Unicode;
                    }
                    else if( (abFirstBytes[0] == 0xfe && abFirstBytes[1] == 0xff) || (abFirstBytes[0] == 0x00 && abFirstBytes[1] == 0x30) )
                    {
                        m_ecCharset = ECharset.UnicodeReversed;
                    }

                    switch( m_ecCharset )
                    {
                        case ECharset.Unicode:
                            srCharsetDetection = new StreamReader( fsCharsetDetection, Encoding.GetEncoding("UTF-16BE") );
                            break;
                        case ECharset.UnicodeReversed:
                            srCharsetDetection = new StreamReader( fsCharsetDetection, Encoding.GetEncoding("UTF-16LE") );
                            break;
                        default:
                            srCharsetDetection = new StreamReader( fsCharsetDetection, Encoding.GetEncoding("utf-8") );
                            break;
                    }
                    if( srCharsetDetection != null )
                    {
                        string sCharsetDetectionLine = "";
                        do
                        {
                            sCharsetDetectionLine = srCharsetDetection.ReadLine();
                            if( sCharsetDetectionLine != "" && sCharsetDetectionLine != null )
                            {
                                sCharsetDetectionLine = sCharsetDetectionLine.ToUpper();
                                sCharsetDetectionLine = sCharsetDetectionLine.Trim();
                            }
                        }
                        while( sCharsetDetectionLine != null && ( sCharsetDetectionLine.Length < 7 || sCharsetDetectionLine.Substring(0,7) != "1 CHAR " ) );
                        if( sCharsetDetectionLine != null )
                        {
                            sCharset = sCharsetDetectionLine.Substring(7);
                            if( m_ecCharset != ECharset.Unicode && m_ecCharset != ECharset.UnicodeReversed ) // If file is in unicode format, ignore charset string cos we already know format.
                            { 
                                // Using substring here to ignore trailing spaces
                                if( sCharset.Length >= 5 && sCharset.Substring(0,5) == "ASCII" )
                                {
                                    m_ecCharset = ECharset.Ascii;
                                }
                                else if( sCharset.Length >= 4 && sCharset.Substring(0,4) == "ANSI" )
                                {
                                    m_ecCharset = ECharset.Ansi;
                                }
                                else if( sCharset.Length >= 5 && sCharset.Substring(0,5) == "ANSEL" )
                                {
                                    m_ecCharset = ECharset.Ansel;
                                }
                                else if( sCharset.Length >= 4 && sCharset.Substring(0,4) == "UTF8" )
                                {
                                    m_ecCharset = ECharset.UTF8;
                                }
                                else if( sCharset.Length >= 5 && sCharset.Substring(0,5) == "UTF-8" )
                                {
                                    m_ecCharset = ECharset.UTF8;
                                }
                                else if( sCharset.Length >= 7 && sCharset.Substring(0,7) == "UNICODE" )
                                {
                                    m_ecCharset = ECharset.Unicode;
                                }
                                else if( sCharset.Length >= 5 && sCharset.Substring(0,5) == "UTF16" )
                                {
                                    m_ecCharset = ECharset.Unicode;
                                }
                                else if( sCharset.Length >= 6 && sCharset.Substring(0,6) == "UTF-16" )
                                {
                                    m_ecCharset = ECharset.Unicode;
                                }
                            }
                        }
                    }

                }
                catch( Exception e )
                {
                    LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Caught exception while trying to discern character set :" + e );
                    m_ecCharset = ECharset.UTF8;
                }

                if( srCharsetDetection != null )
                {
                    srCharsetDetection.Close();
                }
                if( fsCharsetDetection != null )
                {
                    fsCharsetDetection.Close();
                }

                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Opening file with charset " + m_ecCharset );
                fileStream = new FileStream( Filename, FileMode.Open, FileAccess.Read );

                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                switch( m_ecCharset )
                {
                    case ECharset.Ascii:
                        encoding = Encoding.GetEncoding("ascii");
                        break;
                    case ECharset.Ansi:
                        encoding = Encoding.GetEncoding("iso-8859-1");
                        break;
                    case ECharset.Ansel:
                        encoding = Encoding.GetEncoding("iso-8859-1");
                        break;
                    case ECharset.UTF8:
                        encoding = Encoding.GetEncoding("utf-8");
                        break;
                    case ECharset.Unicode:
                        encoding = Encoding.GetEncoding("UTF-16BE");
                        break;
                    case ECharset.UnicodeReversed:
                        encoding = Encoding.GetEncoding("UTF-16LE");
                        break;
                    default:
                        encoding = Encoding.GetEncoding("utf-8");
                        break;
                }

                streamReader = new StreamReader( fileStream, encoding );

                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Stream created" );

                m_nBytesRead = 0;
                m_nBytesTotal = fileStream.Length;
                uint uLineInFile = 0;

                // Read all lines in file into memory
                int nPercentComplete, nPreviousPercentComplete = 0;
                for(;;)
                {
                    if( m_progressWindow.IsAborting )
                    {
                        
                        LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Progress window aborting  (1)" ); 

                        return;
                    }

                    sParseLine = streamReader.ReadLine();
                    if( m_ecCharset == ECharset.Ansel )
                    {
                        sParseLine = ConvertAnsel( sParseLine );
                    }

                    if(  sParseLine == null )
                    {
                        LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "No more lines, end of file" ); 
                        // No more lines, end of file
                        break;
                    }

                    uLineInFile++;

                    m_nBytesRead += sParseLine.Length + 1; // Interim fix. Should actually count bytes according to whether gedcomLine ends with CR or CRLF.

                    nPercentComplete = (int)(m_nBytesRead * 100 / m_nBytesTotal);
                    if( nPercentComplete != nPreviousPercentComplete )
                    {
                        nPreviousPercentComplete = nPercentComplete;
                        m_progressWindow.SetText( String.Format( "Bytes read: {0}", m_nBytesRead ) );
                        if( nPercentComplete > 100 )
                        {
                            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Percent set to 100 (1)" ); 

                            // Safety valve. Prevents control from throwing.
                            nPercentComplete = 100;
                        }
                        m_progressWindow.StepTo( nPercentComplete );
                    }

                    if( sParseLine.Length != 0 )
                    {
                        try
                        {
                            CGedcomLine gedcomLine = ParseLine( sParseLine, uLineInFile );
                            if( gedcomLine != null )
                            {
                                m_alLines.Add( gedcomLine );
                            }
                        }
                        catch( CParsingException )
                        {
                            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Warning, String.Concat("Unable to parse line ", uLineInFile.ToString(), ":", sParseLine ) );
                        }
                    }
                    // Signal waiting app that parse has finished
                }
                if( m_progressWindow.IsAborting )
                {
                    LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Progress window aborting  (2)" ); 
                    return;
                }

                // Parse lines of file
                m_nLineIndex = 0;
                sParseLine="";
                int nLines = m_alLines.Count;

                m_progressWindow.SetText( String.Format( "Parsing file line: {0} out of {1}", m_nLineIndex, nLines ) );

                nPercentComplete = m_nLineIndex * 100 / nLines;
                if( nPercentComplete > 100 )
                {
                    // Safety valve. Prevents control from throwing.
                    nPercentComplete = 100;
                }
                m_progressWindow.StepTo( nPercentComplete );
                if( m_progressWindow.IsAborting )
                {
                    LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Progress window aborting  (3)" ); 
                    return;
                }

                if( !ParseHeader() )
                {
                    // Mandatory header missing/corrupt
                    throw new CParsingException( "Header missing or corrupt" );
                }
                m_progressWindow.SetText( String.Format( "Parsing file line: {0} out of {1}", m_nLineIndex, nLines ) );
                nPercentComplete = m_nLineIndex * 100 / nLines;
                if( nPercentComplete > 100 )
                {
                    // Safety valve. Prevents control from throwing.
                    nPercentComplete = 100;
                }
                m_progressWindow.StepTo( nPercentComplete );
                if( m_progressWindow.IsAborting )
                {
                    LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Progress window aborting  (4)" ); 
                    return;
                }

                m_submissionRecord = CSubmissionRecord.Parse( this, 0 );
                m_progressWindow.SetText( String.Format( "Parsing file line: {0} out of {1}", m_nLineIndex, nLines ) );
                nPercentComplete = m_nLineIndex * 100 / nLines;
                if( nPercentComplete > 100 )
                {
                    // Safety valve. Prevents control from throwing.
                    LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Percent set to 100 (2)" ); 
                    nPercentComplete = 100;
                }
                m_progressWindow.StepTo( nPercentComplete );
                if( m_progressWindow.IsAborting )
                {
                    LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Progress window aborting  (5)" ); 

                    return;
                }

                int nRecords = 0;

                CMultimediaRecord mr;
                CNoteRecord nr;
                CRepositoryRecord rr;
                CSourceRecord sr;
                CSubmitterRecord smr;

                bool bParsingSuccessful = false;
                bool bParsingFinished;
                do
                {
                    bParsingFinished = false;
                    m_progressWindow.SetText( String.Format( "Parsing file line: {0} out of {1}", m_nLineIndex, nLines ) );
                    nPercentComplete = m_nLineIndex * 100 / nLines;
                    if( nPercentComplete > 100 )
                    {
                        // Safety valve. Prevents control from throwing.
                        nPercentComplete = 100;
                    }
                    m_progressWindow.StepTo( nPercentComplete );
                    if( m_progressWindow.IsAborting )
                    {
                        LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Progress window aborting  (6)" ); 
                        return;
                    }

                    CFamilyRecord fr;
                    if( (fr = CFamilyRecord.Parse( this, 0 )) != null )
                    {
                        m_alFamilyRecords.Add( fr );
                        m_htFamilyRecordsXref.Add( fr.m_xref, fr );
                        ++nRecords;
                        bParsingFinished = false;
                    }
                    else
                    {
                        CIndividualRecord ir;
                        if( (ir = CIndividualRecord.Parse( this, 0)) != null )
                        {
                            IndividualRecords.Add( ir );
                            m_htIndividualRecordsXref.Add( ir.m_xref, ir );
                            ++nRecords;
                            bParsingFinished = false;
                        }
                        else if( (mr = CMultimediaRecord.Parse( this, 0 )) != null )
                        {
                            m_alMultimediaRecords.Add( mr );
                            ++nRecords;
                            bParsingFinished = false;
                        }
                        else if( (nr = CNoteRecord.Parse( this, 0 )) != null )
                        {
                            m_alNoteRecords.Add( nr );
                            ++nRecords;
                            bParsingFinished = false;
                        }
                        else if( (rr = CRepositoryRecord.Parse( this, 0 )) != null )
                        {
                            m_alRepositoryRecords.Add( rr );
                            ++nRecords;
                            bParsingFinished = false;
                        }
                        else if( (sr = CSourceRecord.Parse( this, 0 )) != null )
                        {
                            SourceRecords.Add( sr );
                            m_htSourceRecordsXref.Add( sr.m_xref, sr );
                            ++nRecords;
                            bParsingFinished = false;
                        }
                        else if( ( smr = CSubmitterRecord.Parse( this, 0 )) != null )
                        {
                            m_alSubmitterRecords.Add( smr );
                            ++nRecords;
                            bParsingFinished = false;
                        }
                        else if( ( GetLine( 0, "TRLR" )) != null )
                        {
                            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "TRLR found OK" );
                            bParsingSuccessful = true;
                        }
                        else
                        {
                            // Skip this unknown gedcomLine
                            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Warning, "WARNING: Couldn't parse line:" ); 
                            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Warning, GetLine().ToString() );
                            m_nLineIndex++;
                        }
                    }
                } // end do
                while( m_nLineIndex < nLines && !bParsingSuccessful && !bParsingFinished );
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Parsing ended normally." );


                // Tie up adopted individuals with their associated fr
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Linking adoptees." );
                foreach( CIndividualRecord adopIr in m_alAdoptedIndividuals )
                {
                    CEventDetail adopEvent = adopIr.GetEvent( "ADOP" );
                    if( adopEvent != null )
                    {
                        string adopFamXref = adopEvent.m_xrefFam;
                        bool adopHusband = adopEvent.m_bAdoptedByHusband;
                        bool adopWife = adopEvent.m_bAdoptedByWife;
                        CFamilyRecord adopFam = GetFamilyRecord( adopFamXref );
                        if( adopFam!=null && (adopHusband || adopWife) )
                        {
                            if( adopHusband )
                            {
                                CIndividualRecord irAdopHusband = GetIndividualRecord( adopFam.m_xrefHusband );
                                if( irAdopHusband != null )
                                {
                                    CIndividualEventStructure husbandAdopEvent = new CIndividualEventStructure( adopEvent );
                                    husbandAdopEvent.Type = "GEDMILL_ADOPTION_OF_CHILD"; // Special GEDmill only event
                                    husbandAdopEvent.m_eventDetail.m_xrefAdoptedChild = adopIr.m_xref;
                                    irAdopHusband.m_alIndividualEventStructures.Add( husbandAdopEvent );
                                }
                            }
                            if( adopWife )
                            {
                                CIndividualRecord irAdopWife = GetIndividualRecord( adopFam.m_xrefWife );
                                if( irAdopWife != null )
                                {
                                    CIndividualEventStructure wifeAdopEvent = new CIndividualEventStructure( adopEvent );
                                    wifeAdopEvent.Type = "GEDMILL_ADOPTION_OF_CHILD"; // Special GEDmill only event
                                    wifeAdopEvent.m_eventDetail.m_xrefAdoptedChild = adopIr.m_xref;
                                    irAdopWife.m_alIndividualEventStructures.Add( wifeAdopEvent );
                                }
                            }

                        }
                    }
                }

                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Linking indi backreferences." );
                foreach( CIndividualRecord brir in IndividualRecords )
                {
                    foreach( CIndividualEventStructure ies in brir.m_alIndividualEventStructures )
                    {
                        if( ies.m_eventDetail != null )
                        {
                            foreach( CSourceCitation sc in ies.m_eventDetail.m_alSourceCitations )
                            {
                                sc.AddBackreference(new CBackReference(ERecordType.Individual, brir.m_xref, ies.Type));
                                sc.AddPicFromCitationToRecord();
                            }
                            foreach( CNoteStructure ns in ies.m_eventDetail.m_alNoteStructures )
                            {
                                if( ns != null && ns.m_alSourceCitations != null )
                                {
                                    foreach( CSourceCitation sc in ns.m_alSourceCitations )
                                    {
                                        sc.AddBackreference(new CBackReference(ERecordType.Individual, brir.m_xref, ies.Type));
                                        sc.AddPicFromCitationToRecord();
                                    }
                                }
                            }
                        }
                    }
                    foreach( CPersonalNameStructure pns in brir.m_alPersonalNameStructures )
                    {
                        if( pns.m_personalNamePieces != null )
                        {
                            foreach( CSourceCitation sc in pns.m_personalNamePieces.m_alSourceCitations )
                            {
                                sc.AddBackreference( new CBackReference( ERecordType.Individual, brir.m_xref, "NAME" ) );
                                sc.AddPicFromCitationToRecord();
                            }
                        }
                    }
                    foreach( CSourceCitation sc in brir.m_alSourceCitations )
                    {
                        sc.AddBackreference( new CBackReference( ERecordType.Individual, brir.m_xref, "" ) );
                        sc.AddPicFromCitationToRecord();
                    }
                }
                foreach( CFamilyRecord brfr in m_alFamilyRecords )
                {
                    foreach( CFamilyEventStructure fes in brfr.m_alFamilyEventStructures )
                    {
                        if( fes.m_eventDetail != null )
                        {
                            foreach( CSourceCitation sc in fes.m_eventDetail.m_alSourceCitations )
                            {
                                sc.AddBackreference(new CBackReference(ERecordType.Family, brfr.m_xref, fes.Type));
                                sc.AddPicFromCitationToRecord();
                            }
                            foreach( CNoteStructure ns in fes.m_eventDetail.m_alNoteStructures )
                            {
                                if( ns != null && ns.m_alSourceCitations != null )
                                {

                                    foreach( CSourceCitation sc in ns.m_alSourceCitations )
                                    {
                                        sc.AddBackreference(new CBackReference(ERecordType.Family, brfr.m_xref, fes.Type));
                                        sc.AddPicFromCitationToRecord();
                                    }
                                }
                            }

                        }
                    }
                    foreach( CSourceCitation sc in brfr.m_alSourceCitations )
                    {
                        sc.AddBackreference( new CBackReference( ERecordType.Family, brfr.m_xref, "" ) );
                        sc.AddPicFromCitationToRecord();
                    }

                }
                foreach( CNoteRecord brnr in m_alNoteRecords )
                {
                    foreach( CSourceCitation sc in brnr.m_alSourceCitations )
                    {
                        sc.AddBackreference( new CBackReference( ERecordType.Note, brnr.m_xref, "" ) );
                        sc.AddPicFromCitationToRecord();
                    }
                }


                // Join together fragmented multimedia files
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Joining multimedia fragments." );
                // Go through all the MFRs in every link in every record.
                foreach( CSourceRecord isr in SourceRecords )
                {
                    JoinMultimedia( isr.m_alMultimediaLinks );
                }
                foreach( CIndividualRecord iir in IndividualRecords )
                {
                    JoinMultimedia( iir.m_alMultimediaLinks );
                }

                // Create a list of MFRs unique to the individual
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Ordering individual's mfrs." );
                foreach( CIndividualRecord irMultimedia in IndividualRecords )
                {
                    ConvertMultimediaLinks( irMultimedia.m_alMultimediaLinks, ref irMultimedia.m_alUniqueFileRefs );
                }
                // Create a list of MFRs unique to the source
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Ordering source's mfrs." );
                foreach( CSourceRecord srMultimedia in SourceRecords )
                {
                    ConvertMultimediaLinks( srMultimedia.m_alMultimediaLinks, ref srMultimedia.m_alUniqueFileRefs );
                }


                // Moved inside try block as any exception it threw would not otherwise be caught:
                AddChildrenToFamilies();

                // Ended normally
                threaderror.m_nError = 0; 
            }
            catch( ThreadAbortException e )
            {   
                // Abnormal abort
                threaderror.m_nError = 2; 
                threaderror.m_sMessage = "";
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Error, String.Format("Caught thread exception : {0}", e ) );
            }
            catch( ThreadInterruptedException e )
            {
                // Abnormal abort
                threaderror.m_nError = 2; 
                threaderror.m_sMessage = "";
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Error, String.Format( "Caught thread exception : {0}", e ) );
            }
            catch( CParsingException e )
            {
                // Abnormal abort
                threaderror.m_nError = 2; 
                threaderror.m_sMessage = "";
                string sLine = sParseLine;
                if( m_nLineIndex>=0 )
                { 
                    if( m_nLineIndex >= m_alLines.Count )
                    {
                        sLine = "EOF";
                    }
                    else
                    {
                        sLine = m_alLines[m_nLineIndex].ToString();
                    }
                }

                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Error, String.Format("Caught parsing exception in file {0}, line {1} ({2}) : {3}", Filename, m_nLineIndex, sLine, e ) );

                // And here, if we can
            }
            catch( IOException e )
            {
                // Abnormal abort, offer retry, file already open.
                threaderror.m_nError = 3; 
                threaderror.m_sMessage = "";
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Error, String.Format("Caught IO exception (line index={0}) : {1}", m_nLineIndex, e ) ); 
            }
            catch( Exception e )
            {
                // Abnormal abort
                threaderror.m_nError = 2; 
                threaderror.m_sMessage = "";
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Error, String.Format("Caught generic exception (line index={0}) : {1}", m_nLineIndex, e ) ); 
            }
            finally
            {
                LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Closing file" );

                if (streamReader != null)
                {
                    streamReader.Close();
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                }

                if( m_progressWindow != null )
                {
                    LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Closing progress window" ); 

                    m_progressWindow.End( threaderror );
                }

                // Don't need the memory any more:
                m_alLines.Clear(); 
            }
            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "All done." );
#endif
        }

        private void JoinAllMmedia()
        {
            // Join together fragmented multimedia files
            LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Joining multimedia fragments.");
            // Go through all the MFRs in every link in every record.
            foreach (CSourceRecord isr in SourceRecords)
            {
                JoinMultimedia(isr.m_alMultimediaLinks);
            }
            foreach (CIndividualRecord iir in IndividualRecords)
            {
                JoinMultimedia(iir.m_alMultimediaLinks);
            }
        }

        // Join together fragmented multimedia files
        private void JoinMultimedia( ArrayList alMultimediaLinks )
        {
            // A multimedia link ultimately points to a list of MFRs
            foreach( CMultimediaLink ml in alMultimediaLinks ) 
            {
                // The list of MFRs
                ArrayList alFileRefs = ml.GetFileReferences(); 
                if( alFileRefs != null )
                {
                    // Go through all MFRs in links
                    foreach( CMultimediaFileReference mfr in alFileRefs )
                    {
                        // If MFR has continuation MFR, join it, delete joined portion, note the continuation MFR, set current MFR = continuation MFR
                        // until no more continuation MFRs
                        // Go through all the MFRs in every link in every record. Delete the continuation MFRs. Shouldn't be neccessary as these continuation MFRs shouldn't be linked from anywhere, and the tmp files should have been deleted.
                        CMultimediaFileReference mfrCurrent = mfr;
                        string sMmFilename = mfr.m_sMultimediaFileReference;

                        while( mfrCurrent.m_xrefObj != null )
                        {
                            CMultimediaRecord mr = GetMultimediaRecord( mfrCurrent.m_xrefObj );
                            if( mr != null )
                            {
                                if( mr.m_alMultimediaFileReferences.Count == 1 )
                                {
                                    CMultimediaFileReference mfrNext = (CMultimediaFileReference)(mr.m_alMultimediaFileReferences[0]);

                                    if( mfrNext.m_sMultimediaFileReference != null )
                                    {
                                        // Cat mfr_record.m_multimediaFileReference to end of mmfilename
                                        try
                                        {
                                            JoinFiles( sMmFilename, mfrNext.m_sMultimediaFileReference );
                                            File.Delete( mfrNext.m_sMultimediaFileReference );
                                            mfrCurrent.m_xrefObj = null;
                                            mfrCurrent = mfrNext;
                                            // Should really remove deleted sFilename from m_temporaryFiles, but can leave in as File.Delete() will fail and code will skip the file.
                                        }
                                        catch( IOException e )
                                        {
                                            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Error, "Caught IO exception during concatenation:\r\n" + e );
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Warning, String.Format("Ignoring continuation object with multiple references:\"{0}\"", sMmFilename ) );
                                }
                            }
                        }
                    }
                }
            }
        }

        // Removes trailing whitespace
        public static void StripTrailingWhitespace( ref string sText )
        {
            if (sText == null)
            {
                return;
            }

            while( sText.Length > 0 && sText[sText.Length-1] == ' ' )
            {
                sText = sText.Substring( 0, sText.Length-1 );
            }
        }

        // Removes leading whitespace from text. Only the first character check includes tabs, the remainder of tabs don't count as whitespace. This is so that preformatted text keeps leading tabs in place. (Such as in note text)
        public static void ParseWhitespace( ref string sText )
        {
            if (sText == null)
            {
                return;
            }

            bool bIncludeTabsAsWhitespace = true;
            for( int i = 0; i < sText.Length; ++i )
            {
                if( (!bIncludeTabsAsWhitespace && sText[i] == '\t') || (Char.IsWhiteSpace( sText[i] ) == false) )
                {
                    sText = sText.Substring( i );
                    return;
                }
                bIncludeTabsAsWhitespace = false;
            }

            // String was all whitespace
            sText = null;
        }

        // Converts a GEDCOM number string to an integer
        public static Int32 ParseNumber( ref string sText )
        {
            int nSign = 1;

            string sTemp = sText;
            if (sTemp == null)
            {
                throw new CParsingException("Number formatting problem");
            }

            ParseWhitespace( ref sTemp );
            if (sTemp == null)
            {
                throw new CParsingException("Number format error");
            }

            int i = 0;

            if( sTemp[i] == '-' )
            {
                nSign = -1;
                ++i;
            }

            if(!char.IsDigit(sTemp[i]))
            {
                throw new CParsingException( "Number parse error" );
            }

            int nVal = 0;
            char c;
            while( i<sTemp.Length && char.IsDigit(c = sTemp[i]) )
            {
                nVal *= 10;
                nVal += (c - '0');
                i++;
            }

            sText = sTemp.Substring( i );
            return nSign * nVal;
        }

        // KBR TODO will want this
#if false
        // Parser for GEDCOM place hierarchy
        private void ParsePlaceHierarchy()
        {
            for(;;)
            {
                CGedcomLine gedcomLine = (CGedcomLine)m_alLines[ m_nLineIndex ];
                if( gedcomLine.Level != 2 )
                {
                    return;
                }
                switch( gedcomLine.Tag )
                {
                    case "FORM":
                        m_header.m_sPlaceHierarchy = gedcomLine.LineItem;
                        m_nLineIndex++;
                        break;
                    default:
                        LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Warning, "Unknown PlaceHierarchy tag " + gedcomLine.Tag ); 
                        m_nLineIndex++;
                        break;
                }
            }

        }
#endif

        // Return the next line of the parsed GEDCOM, ensuring that it meets the expected critera
        // Returns null if no lines left or criteria not matched.
        public CGedcomLine GetLine( int nLevelExpected, string sTagExpected )
        {
            CGedcomLine gedcomLine;
            
            int nLinesMax = m_alLines.Count;

            if (m_nLineIndex >= nLinesMax)
            {
                return null;
            }
            gedcomLine = (CGedcomLine)m_alLines[ m_nLineIndex ];

            if (gedcomLine == null)
            {
                return null;
            }

            int nLineIndex = m_nLineIndex; 

            if( ( gedcomLine.Level != nLevelExpected ) || ( gedcomLine.Tag != sTagExpected ) )
            {
                // Skip '_' tags
                while( gedcomLine.Tag != null && gedcomLine.Tag.Length>0 && gedcomLine.Tag[0] == '_' && gedcomLine.Tag != sTagExpected )
                {
                    int nUnderscoreLevel = gedcomLine.Level;
                    do
                    {
                        nLineIndex++;
                        if (nLineIndex >= nLinesMax)
                        {
                            return null;
                        }
                        gedcomLine = (CGedcomLine)m_alLines[ nLineIndex ];
                    }
                    while( gedcomLine.Level > nUnderscoreLevel );
                }       
            }

            if( ( gedcomLine.Level != nLevelExpected ) || ( gedcomLine.Tag != sTagExpected ) )
            {
                return null;
            }

            // Move gedcomLine pointer ahead of any skipped stuff
            m_nLineIndex = nLineIndex;

            return gedcomLine;
        }

        // Returns the current tokenised line of GEDCOM
        public CGedcomLine GetLine()
        {
            if (m_nLineIndex >= m_alLines.Count)
            {
                return new CGedcomLine(0, "", "TRLR", "", "", (uint)m_nLineIndex);
            }

            return (CGedcomLine)m_alLines[ m_nLineIndex ];
        }

        // Move on to the next tokenised line of GEDCOM
        public void IncrementLineIndex( int nIncrementAmount )
        {
            m_nLineIndex += nIncrementAmount;
        }

        // KBR TODO use hash for all records by xref
        // Returns the note record with the given xref id.
        public CNoteRecord GetNoteRecord(string xref)
        {
            if( string.IsNullOrEmpty(xref)  )
            {
                return null;
            }
            foreach( CNoteRecord nr in m_alNoteRecords )
            {
                if (nr.m_xref == xref)
                {
                    return nr;
                }
            }
            return null;
        }

        // Returns the family record with the given xref id.
        public CFamilyRecord GetFamilyRecord(string xref)
        {
            if( string.IsNullOrEmpty(xref) )
            {
                return null;
            }

            if( m_htFamilyRecordsXref.ContainsKey( xref ) )
            {
                return (CFamilyRecord)m_htFamilyRecordsXref[ xref ];
            }

            foreach( CFamilyRecord fr in FamilyRecords )
            {
                if (fr.m_xref == xref)
                {
                    return fr;
                }
            }
            return null;
        }

        // Returns the individual record with the given xref id.
        public CIndividualRecord GetIndividualRecord(string xref)
        {
            if( string.IsNullOrEmpty(xref)  )
            {
                return null;
            }

            if( m_htIndividualRecordsXref.ContainsKey( xref ) )
            {
                return (CIndividualRecord)m_htIndividualRecordsXref[ xref ];
            }

            foreach( CIndividualRecord ir in IndividualRecords )
            {
                if (ir.m_xref == xref)
                {
                    return ir;
                }
            }
            return null;
        }

        // Returns the source record with the given xref id.
        public CSourceRecord GetSourceRecord(string xref)
        {
            if( string.IsNullOrEmpty(xref)  )
            {
                return null;
            }

            if( m_htSourceRecordsXref.ContainsKey( xref ) )
            {
                return (CSourceRecord)m_htSourceRecordsXref[ xref ];
            }

            foreach( CSourceRecord sr in SourceRecords )
            {
                if (sr.m_xref == xref)
                {
                    return sr;
                }
            }
            return null;
        }

        // Returns the multimedia record with the given xref id.
        public CMultimediaRecord GetMultimediaRecord( string xref )
        {
            if( string.IsNullOrEmpty(xref)  )
            {
                return null;
            }

            foreach( CMultimediaRecord mr in m_alMultimediaRecords )
            {
                if (mr.m_xref == xref)
                {
                    return mr;
                }
            }
            return null;
        }

        // Returns the repository record with the given xref id.
        public CRepositoryRecord GetRepositoryRecord(string xref)
        {
            if (string.IsNullOrEmpty(xref))
            {
                return null;
            }

            foreach (CRepositoryRecord rr in m_alRepositoryRecords)
            {
                if (rr.m_xref == xref)
                {
                    return rr;
                }
            }
            return null;
        }

        // Converts xref string to a number for comparison and hashing purposes
        public static int MakeXrefNumber( string xref )
        {
            if (string.IsNullOrEmpty(xref))
            {
                return 0;
            }

            int val = 0;

            int l = xref.Length;
            if( --l >= 0 )
            {
                val += xref[l];
            }
            if( --l >= 0 )
            {
                val += 256 * xref[l];
            }
            if( --l >= 0 )
            {
                val += 256*256 * xref[l];
            }
            if( --l >= 0 )
            {
                val += 256*256*256 * xref[l];
            }

            return val;
        }

        // Safe accessor
        public string Filename
        {
            get
            {
                string sFilename;
                Monitor.Enter( this );
                sFilename = m_sFilename;
                Monitor.Exit( this );
                return sFilename;
            }
            set
            {
                Monitor.Enter( this );
                m_sFilename = value;
                Monitor.Exit( this );
            }
        }

        // Set a reference to the progress window
        public IProgressCallback ProgressCallback
        {
            set
            {
                m_progressWindow = value;
            }
        }

        // Returns the number of individual records found in the GEDCOM
        public int CountIndividuals
        {
            get
            {
                return IndividualRecords.Count;
            }
        }

        // Returns the number of source records found in the GEDCOM
        public int CountSources
        {
            get
            {
                return SourceRecords.Count;
            }
        }

        // Returns the xref string for the first individual in the gedcom that isn't set as restricted.
        public CIndividualRecord FirstUnrestrictedIndividual()
        {
            foreach( CIndividualRecord ir in IndividualRecords )
            {
                if (ir.Visibility() == CIndividualRecord.EVisibility.Visible)
                {
                    return ir;
                }
            }
            return null;
        }

        // Accessor
        public bool DataMayStartWithWhitespace
        {
            get
            {
                return m_bDataMayStartWithWhitespace;
            }
            set
            {
                m_bDataMayStartWithWhitespace = value;
            }
        }

        // Accessor
        public bool DataMayEndWithWhitespace
        {
            get
            {
                return m_bDataMayEndWithWhitespace;
            }
            set
            {
                m_bDataMayEndWithWhitespace = value;
            }
        }

        // Restricts presentation of sources connected with an individual (for when individual themselves are marked as restricted)
        public void RestrictAssociatedSources( CIndividualRecord ir )
        {
            // Restrict sources connected with individual directly
            foreach( CSourceCitation sc in ir.m_alSourceCitations )
            {
                RestrictSource( sc, true );
            }

            // Restrict sources connected with name
            foreach( CPersonalNameStructure pns in ir.m_alPersonalNameStructures )
            {
                if( pns.m_personalNamePieces != null )
                {
                    foreach( CSourceCitation sc in pns.m_personalNamePieces.m_alSourceCitations )
                    {
                        RestrictSource( sc, true );
                    }
                }
            }


            // Restrict sources connected with events
            foreach( CIndividualEventStructure ies in ir.m_alIndividualEventStructures )
            {
                if( ies.m_eventDetail != null )
                {
                    foreach( CSourceCitation sc in ies.m_eventDetail.m_alSourceCitations )
                    {
                        RestrictSource( sc, true );
                    }
                }
            }


            // Restrict sources connected with m_ldsIndividualOrdinances
            foreach( CLdsOrdinance lo in ir.m_alLdsIndividualOrdinances )
            {
                foreach( CSourceCitation sc in lo.m_alSourceCitations )
                {
                    RestrictSource( sc, true );
                }
            }


            // Restrict sources connected with m_associationStructures
            foreach( CAssociationStructure ass in ir.m_alAssociationStructures )
            {
                foreach( CSourceCitation sc in ass.m_alSourceCitations )
                {
                    RestrictSource( sc, true );
                }
            }

        }

        // Marks the given source citation as (un)restricted
        public void RestrictSource( CSourceCitation sc, bool bRestricted )
        {
            if( sc is CSourceCitationXref )
            {
                string xref = ((CSourceCitationXref)sc).m_xref;
                CSourceRecord sr = GetSourceRecord( xref );
                if( sr != null )
                {
                    sr.Restricted = bRestricted;
                }
            }
            else if( sc is CSourceCitationInLine )
            {
                sc.Restricted = bRestricted;
            }
        }

        // Gets the n'th family record that the given individual is a child in
        public CFamilyRecord GetFamilyByChild( CIndividualRecord ir, int n )
        {
            if( ir.m_alChildToFamilyLinks == null )
            {
                return null;    
            }

            if( n < 0 || n >= ir.m_alChildToFamilyLinks.Count )
            {
                return null;
            }

            CChildToFamilyLink cfl = (CChildToFamilyLink)(ir.m_alChildToFamilyLinks[ n ]);
            if( cfl == null )
            {
                return null;
            }

            return GetFamilyRecord( cfl.m_xrefFam );
        }

        // Gets the n'th family record that the given individual is a spouse in
        public CFamilyRecord GetFamilyBySpouse( CIndividualRecord ir, int n )
        {
            if( ir.m_alSpouseToFamilyLinks == null )
            {
                return null;    
            }

            if( n < 0 || n >= ir.m_alSpouseToFamilyLinks.Count )
            {
                return null;
            }

            CSpouseToFamilyLink sfl = (CSpouseToFamilyLink)(ir.m_alSpouseToFamilyLinks[ n ]);
            if( sfl == null )
            {
                return null;
            }

            return GetFamilyRecord( sfl.m_xrefFam );
        }

        // Gets the male spouse associated with the given family record
        public CIndividualRecord GetHusband( CFamilyRecord fr )
        {
            if( fr == null )
            {
                return null;
            }
            return GetIndividualRecord( fr.m_xrefHusband );
        }

        // Gets the female spouse associated with the given family record
        public CIndividualRecord GetWife( CFamilyRecord fr )
        {
            if( fr == null )
            {
                return null;
            }
            return GetIndividualRecord( fr.m_xrefWife );
        }

        // Put children in age order
        private void AddChildrenToFamilies()
        {
            foreach( CFamilyRecord fr in FamilyRecords )
            {
                fr.AddChildren( this );
            }
        }

        // Begin with an empty table in which to record all individuals visited during the pruning process.
        // TODO: A separate PruningMachine class, which has visited list as a member, created in constructor, and has gedcom reference as a member.
        public void BeginPruning()
        {
            m_htVisited = new Hashtable();
        }

        // No longer need the visited table, so free the memory.
        public void EndPruning()
        {
            m_htVisited = null;
        }

        // Restricts descendants only
        public void PruneDescendants( CIndividualRecord ir, bool bExclude )
        {
            m_htVisited[ ir.m_xref ] = true;
            PruneDescendants( ir, false, bExclude );    
        }

        // Restricts ancestors only
        public void PruneAncestors(CIndividualRecord ir, bool bExclude)
        {
            m_htVisited[ ir.m_xref ] = true;
            PruneAncestors( ir, false, bExclude );  
        }

        // Restricts descendants, and if required, their spouses and their spouses ancestors.
        private void PruneDescendants( CIndividualRecord ir, bool bPruneSpouses, bool bExclude )
        {
            int nFamily = 0;
            CFamilyRecord fr;
            while( (fr = GetFamilyBySpouse( ir, nFamily++ )) != null )
            {
                int nChild = 0;
                CIndividualRecord irChild;
                while( (irChild = fr.GetChildByBirthDate( nChild++ )) != null )
                {
                    if( m_htVisited.ContainsKey( irChild.m_xref ) )
                    {
                        continue;
                    }

                    if( bExclude )
                    {
                        if( !irChild.Restricted )
                        {
                            if( MainForm.m_mainForm != null )
                            {
                                MainForm.m_mainForm.m_nPruneExcluded++;
                            }
                            irChild.Restricted = true;
                        }
                    }
                    else
                    {
                        if( irChild.Restricted )
                        {
                            if( MainForm.m_mainForm != null )
                            {
                                MainForm.m_mainForm.m_nPruneIncluded++;
                            }
                            irChild.Restricted = false;
                        }
                    }
                    m_htVisited[ irChild.m_xref ] = true;
                    PruneDescendants( irChild, bPruneSpouses, bExclude );
                }

                if( bPruneSpouses )
                {
                    CIndividualRecord irFather = GetHusband( fr );
                    if( irFather != ir && irFather != null && !m_htVisited.ContainsKey( irFather.m_xref ) )
                    {
                        if( bExclude )
                        {
                            if( !irFather.Restricted )
                            {
                                if( MainForm.m_mainForm != null )
                                {
                                    MainForm.m_mainForm.m_nPruneExcluded++;
                                }
                                irFather.Restricted = true;
                            }
                        }
                        else
                        {
                            if( irFather.Restricted )
                            {
                                if( MainForm.m_mainForm != null )
                                {
                                    MainForm.m_mainForm.m_nPruneIncluded++;
                                }
                                irFather.Restricted = false;
                            }
                        }
                        m_htVisited[ irFather.m_xref ] = true;
                        PruneAncestors( irFather, bExclude );
                        PruneDescendants( irFather, false );
                    }
                    CIndividualRecord irMother = GetWife( fr );
                    if( irMother != ir && irMother != null && !m_htVisited.ContainsKey( irMother.m_xref ) )
                    {
                        if( bExclude )
                        {
                            if( !irMother.Restricted )
                            {
                                if( MainForm.m_mainForm != null )
                                {
                                    MainForm.m_mainForm.m_nPruneExcluded++;
                                }
                                irMother.Restricted = true;
                            }
                        }
                        else
                        {
                            if( irMother.Restricted )
                            {
                                if( MainForm.m_mainForm != null )
                                {
                                    MainForm.m_mainForm.m_nPruneIncluded++;
                                }
                                irMother.Restricted = false;
                            }
                        }
                        m_htVisited[ irMother.m_xref ] = true;
                        PruneAncestors( irMother, bExclude );
                        PruneDescendants( irMother, false );
                    }
                }
            }

        }

        
        // Restricts their siblings and their ancestors and their ancestors spouses
        private void PruneAncestors( CIndividualRecord ir, bool bPruneSpouses, bool bExclude )
        {
            int nFamily = 0;
            CFamilyRecord fr = null;
            while( (fr = GetFamilyByChild( ir, nFamily++ )) != null )
            {
                CIndividualRecord irFather = GetHusband( fr );
                if( irFather != null )
                {
                    if( !m_htVisited.ContainsKey( irFather.m_xref ) )
                    {
                        PruneAncestors( irFather, bExclude );
                    }
                    if( bExclude )
                    {
                        if( !irFather.Restricted )
                        {
                            if( MainForm.m_mainForm != null )
                            {
                                MainForm.m_mainForm.m_nPruneExcluded++;
                            }
                            irFather.Restricted = true;
                        }
                    }
                    else
                    {
                        if( irFather.Restricted )
                        {
                            if( MainForm.m_mainForm != null )
                            {
                                MainForm.m_mainForm.m_nPruneIncluded++;
                            }
                            irFather.Restricted = false;
                        }
                    }
                    m_htVisited[ irFather.m_xref ] = true;  
                }

                CIndividualRecord irMother = GetWife( fr );
                if( irMother != null )
                {
                    if( !m_htVisited.ContainsKey( irMother.m_xref ) )
                    {
                        PruneAncestors( irMother, bExclude );
                    }
                    if( bExclude )
                    {
                        if( !irMother.Restricted )
                        {
                            if( MainForm.m_mainForm != null )
                            {
                                MainForm.m_mainForm.m_nPruneExcluded++;
                            }
                            irMother.Restricted = true;
                        }
                    }
                    else
                    {
                        if( irMother.Restricted )
                        {
                            if( MainForm.m_mainForm != null )
                            {
                                MainForm.m_mainForm.m_nPruneIncluded++;
                            }
                            irMother.Restricted = false;
                        }
                    }
                    m_htVisited[ irMother.m_xref ] = true;  
                }
            }
        }
        
        // Exclude all individuals unless marked as visited
        public void PruneUnmarked()
        {
            foreach( CIndividualRecord ir in IndividualRecords )
            {
                if( !m_htVisited.ContainsKey( ir.m_xref ) )
                {
                    if( !ir.Restricted )
                    {
                        ir.Restricted = true;
                        if( MainForm.m_mainForm != null )
                        {
                            MainForm.m_mainForm.m_nPruneExcluded++;
                        }
                    }
                }
            }
        }

        // Recursively visit all individuals connected to this one and record them by adding their xrefs as keys in m_htVisited 
        public void PruneMarkConnected( CIndividualRecord ir )
        {
            int nFamily;
            CFamilyRecord fr;

            if( ir.Visibility() == CIndividualRecord.EVisibility.Invisible )
            {
                // Don't follow splits
                return; 
            }

            // Mark individual
            m_htVisited[ ir.m_xref ] = true;    

            // Mark all ancestors
            nFamily = 0;
            while( (fr = GetFamilyByChild( ir, nFamily++ )) != null )
            {
                CIndividualRecord irFather = GetHusband( fr );
                if( irFather != null )
                {
                    if( !m_htVisited.ContainsKey( irFather.m_xref ) )
                    {
                        PruneMarkConnected( irFather );
                    }
                }

                CIndividualRecord irMother = GetWife( fr );
                if( irMother != null )
                {
                    if( !m_htVisited.ContainsKey( irMother.m_xref ) )
                    {
                        PruneMarkConnected( irMother );
                    }
                }

                // If both parents are unknown, we can still navigate to the children. Parents appear in minitree as <unknown>, and children also appear.
                // If either mother or father is known, the marking will have happened above.
                if( irMother == null && irFather == null )
                {
                    int i = 0;
                    CIndividualRecord irChild;
                    while ((irChild = fr.GetChildByBirthDate(i)) != null)
                    {
                        i++;
                        if( m_htVisited.ContainsKey( irChild.m_xref ) == false )
                        {
                            PruneMarkConnected( irChild );
                        }
                    }
                }
                    
            }

            // Mark all descendants
            nFamily = 0;
            while( (fr = GetFamilyBySpouse( ir, nFamily++ )) != null )
            {
                int nChild = 0;
                CIndividualRecord irChild;
                while ((irChild = fr.GetChildByBirthDate(nChild++)) != null)
                {
                    if( m_htVisited.ContainsKey( irChild.m_xref ) )
                    {
                        continue;
                    }

                    PruneMarkConnected( irChild );
                }

                // Mark all spouses
                CIndividualRecord irFather = GetHusband( fr );
                if( irFather != ir && irFather != null && !m_htVisited.ContainsKey( irFather.m_xref ) )
                {
                    PruneMarkConnected( irFather );
                }
                CIndividualRecord irMother = GetWife( fr );
                if( irMother != ir && irMother != null && !m_htVisited.ContainsKey( irMother.m_xref ) )
                {
                    PruneMarkConnected( irMother );
                }
            }

        }


        // UU Decode the blob string and store in file (for multimedia embedded in the GEDCOM file)
        public string DecodeBlob( string sBlob )
        {
            // TODO Create temporary file
            string sFilename = Path.GetTempFileName();
            m_alTemporaryFiles.Add( sFilename );

            // TODO Check for file's existence, in case get called twice for the same sFilename
            FileStream fs = new FileStream( sFilename, FileMode.Create ); 
            BinaryWriter sw = new BinaryWriter( fs );

            int i = 0;
            int l = sBlob.Length - 3;

            byte c1, c2, c3, c4;
            byte b1, b2, b3;
            while(i<l)
            {

                if( m_htDecoding.ContainsKey( sBlob[i] ) )
                {
                    c1 = (byte)m_htDecoding[(sBlob[i++])];
                }
                else
                {
                    throw new CBlobException();
                }
                if( m_htDecoding.ContainsKey( sBlob[i] ) )
                {
                    c2 = (byte)m_htDecoding[(sBlob[i++])];
                }
                else
                {
                    throw new CBlobException();
                }
                if( m_htDecoding.ContainsKey( sBlob[i] ) )
                {
                    c3 = (byte)m_htDecoding[(sBlob[i++])];
                }
                else
                {
                    throw new CBlobException();
                }
                if( m_htDecoding.ContainsKey( sBlob[i] ) )
                {
                    c4 = (byte)m_htDecoding[(sBlob[i++])];
                }
                else
                {
                    throw new CBlobException();
                }

                // The following decodes Family Historian blobs. I think this might differ from the GEDCOM 5.5 spec in terms of bit ordering.
                b1 = (byte)((c2 & 0x03)<<6 | (c1 & 0x3f));
                b2 = (byte)((c3 & 0x0f)<<4 | (c2 & 0x3c)>>2);
                b3 = (byte)((c4 & 0x3f)<<2 | (c3 & 0x30)>>4);

                sw.Write( b1 );
                sw.Write( b2 );
                sw.Write( b3 );
            }

            sw.Close();

            return sFilename;
        }

        // Returns an ordered array of all family records that the given individual was a spouse in
        public ArrayList GetFamilyArray( CIndividualRecord ir )
        {
            if( ir == null )
            {
                return null;
            }

            ArrayList alFamily = new ArrayList();

            int nFamilies = 0;
            CFamilyRecord fr;
            while( (fr = GetFamilyBySpouse( ir, nFamilies )) != null )
            {
                alFamily.Add( fr );
                nFamilies++;
            }

            alFamily.Sort( new CFamilyRecord.FamilyComparer() );

            return alFamily;
        }

#if false // KBR might want this
        // Converts all ANSEL characters to their corresponding Unicode
        private string ConvertAnsel(string sLine)
        {
            if( sLine == null )
            {
                return null;
            }
            if( sLine == "" )
            {
                return "";
            }

            StringBuilder sbLineConverted = new StringBuilder( sLine.Length );
            char cCombiner = '\0';
            foreach( char c in sLine )
            {
                switch( (byte)c )
                {
                    case 0xA1:
                        sbLineConverted.Append( "\u0141" ); // slash l - uppercase  +   0141    latin capital letter L with stroke  +       +           
                        break;
                    case 0xA2:
                        sbLineConverted.Append( "\u00d8" ); //  slash o - uppercase +   00D8    latin capital letter O with stroke  +       +           
                        break;
                    case 0xA3:
                        sbLineConverted.Append( "\u0110" ); //  slash d - uppercase +   0110    latin capital letter d with stroke  +       o       1   
                        break;
                    case 0xa4:
                        sbLineConverted.Append( "\u00de" ); //  thorn - uppercase   +   00de    latin capital letter thorn  +   +   +           
                        break;
                    case 0xa5:
                        sbLineConverted.Append( "\u00c6" ); //  ligature ae - uppercase +   00c6    latin capital letter ae +   +   +           
                        break;
                    case 0xa6:
                        sbLineConverted.Append( "\u0152" ); //  ligature oe - uppercase +   0152    latin capital ligature Oe   +   +   +           
                        break;
                    case 0xa7:
                        sbLineConverted.Append( "\u02b9" ); //  miagkii znak    +   02b9    modified letter prime   +   +   +       2   
                        break;
                    case 0xa8:
                        sbLineConverted.Append( "\u00b7" ); //  middle dot  +   00b7    middle dot  +   +   +           
                        break;
                    case 0xa9:
                        sbLineConverted.Append( "\u266d" ); //  musical flat        266d    music flat sign +   +   +           
                        break;
                    case 0xaa:
                        sbLineConverted.Append( "\u00ae" ); //  patent mark     00ae    registered sign +       +           
                        break;
                    case 0xab:
                        sbLineConverted.Append( "\u00b1" ); //  plus-or-minus       00b1    plus-minus sign +   +   +           
                        break;
                    case 0xac:
                        sbLineConverted.Append( "\u01a0" ); //  hook o - uppercase      01a0    latin capital letter O with horn    +       +           
                        break;
                    case 0xad:
                        sbLineConverted.Append( "\u01af" ); //  hook u - uppercase      01af    latin capital letter U with horn    +       +           
                        break;
                    case 0xae:
                        sbLineConverted.Append( "\u02bc" ); //  alif    +   02bc    modifier letter apostrophe  ?   -   -       3   
                        break;
                    case 0xb0:
                        sbLineConverted.Append( "\u02bb" ); //  ayn +   02bb    modifier letter turned comma    ?   -   -       4   
                        break;
                    case 0xb1:
                        sbLineConverted.Append( "\u0142" ); //  slash l - lowercase +   0142    latin small letter L with stroke    +       +           
                        break;
                    case 0xb2:
                        sbLineConverted.Append( "\u00f8" ); //  slash o - lowercase +   00f8    latin small letter O with stroke    +       +           
                        break;
                    case 0xb3:
                        sbLineConverted.Append( "\u0111" ); //  slash d - lowercase +   0111    latin small letter d with stroke    +       +       5   
                        break;
                    case 0xb4:
                        sbLineConverted.Append( "\u00fe" ); //  thorn - lowercase   +   00fe    latin small letter thorn    +   +   +           
                        break;
                    case 0xb5:
                        sbLineConverted.Append( "\u00e6" ); //  ligature ae - lowercase +   00e6    latin small letter ae   +   +   +           
                        break;
                    case 0xb6:
                        sbLineConverted.Append( "\u0153" ); //  ligature oe - lowercase +   0153    latin small ligature Oe +   +   +           
                        break;
                    case 0xb7:
                        sbLineConverted.Append( "\u02ba" ); //  hard sign (tverdyi znak)        02ba    modified letter double prime    +   +   +       6   
                        break;
                    case 0xb8:
                        sbLineConverted.Append( "\u0131" ); //  dotless i - lowercase   +   0131    latin small letter dotless i    +   +   +           
                        break;
                    case 0xb9:
                        sbLineConverted.Append( "\u00a3" ); //  british pound   +   00a3    pound sign  +   +   +           
                        break;
                    case 0xba:
                        sbLineConverted.Append( "\u00f0" ); //  eth +   00f0    latin small letter eth  +   +   +       5   
                        break;
                    case 0xbc:
                        sbLineConverted.Append( "\u01a1" ); //  hook o - lowercase      01a1    latin small letter O with horn  +       +           
                        break;
                    case 0xbd:
                        sbLineConverted.Append( "\u01b0" ); //  hook u - lowercase      01b0    latin small letter U with horn  +       +           
                        break;
                    case 0xc0:
                        sbLineConverted.Append( "\u00b0" ); //  degree sign     00b0    degree sign +   +   +           
                        break;
                    case 0xc1:
                        sbLineConverted.Append( "\u2113" ); //  script l        2113    script small L  +   +   +           
                        break;
                    case 0xc2:
                        sbLineConverted.Append( "\u2117" ); //  phonograph copyright mark       2117    sound recording copyright   +       +           
                        break;
                    case 0xc3:
                        sbLineConverted.Append( "\u00a9" ); //  copyright symbol    +   00a9    copyright sign  +   +   +           
                        break;
                    case 0xc4:
                        sbLineConverted.Append( "\u266f" ); //  musical sharp       266f    music sharp sign    +   +   +           
                        break;
                    case 0xc5:
                        sbLineConverted.Append( "\u00bf" ); //  inverted question mark  +   00bf    inverted question mark  +   +   +           
                        break;
                    case 0xc6:
                        sbLineConverted.Append( "\u00a1" ); //  inverted exclamation mark   +   00a1    inverted exclamation mark   +   +   +           
                        break;
                    case 0xcf:
                        sbLineConverted.Append( "\u00df" ); //  es zet  +   00df    latin small letter sharp S  +   -   +       7   
                        break;
                    case 0xe0:
                        cCombiner = '\u0309'; //    low rising tone mark        0309    combining hook above    +   -   o   +       
                        break;
                    case 0xe1:
                        cCombiner = '\u0300'; //    grave accent    +   0300    combining grave accent  +   +   +   +       
                        break;
                    case 0xe2:
                        cCombiner = '\u0301'; //    acute accent    +   0301    combining acute accent  +   +   +   +       
                        break;
                    case 0xe3:
                        cCombiner = '\u0302'; //    circumflex accent   +   0302    combining circumflex accent +   +   +   +       
                        break;
                    case 0xe4:
                        cCombiner = '\u0303'; //    tilde   +   0303    combining tilde +   +   +   +       
                        break;
                    case 0xe5:
                        cCombiner = '\u0304'; //    macron  +   0304    combining macron    +   +   +   +       
                        break;
                    case 0xe6:
                        cCombiner = '\u0306'; //    breve   +   0306    combining breve +   +   +   +       
                        break;
                    case 0xe7:
                        cCombiner = '\u0307'; //    dot above   +   0307    combining dot above +   +   +   +       
                        break;
                    case 0xe8:
                        cCombiner = '\u0308'; //    umlaut (dieresis)   +   0308    combining diaeresis +   +   +   +       
                        break;
                    case 0xe9:
                        cCombiner = '\u030c'; //    hacek (caron)   +   030c    combining caron +   +   +   +   8   
                        break;
                    case 0xea:
                        cCombiner = '\u030a'; //    circle above (angstrom) +   030a    combining ring above    +       +   +       
                        break;
                    case 0xeb:
                        cCombiner = '\ufe20'; //    ligature, left half     fe20    combining ligature left half    ?   +   +   -       
                        break;
                    case 0xec:
                        cCombiner = '\ufe21'; //    ligature, right half        fe21    combining ligature right half   ?   +   +   -       
                        break;
                    case 0xed:
                        cCombiner = '\u0315'; //    high comma, off center  +   0315    combining comma above right +       +   -       
                        break;
                    case 0xee:
                        cCombiner = '\u030b'; //    double acute accent +   030b    combining double acute accent   +   +   +   +       
                        break;
                    case 0xef:
                        cCombiner = '\u0310'; //    candrabindu     0310    combining candrabindu   +   +   +   -       
                        break;
                    case 0xf0:
                        cCombiner = '\u0327'; //    cedilla +   0327    combining cedilla   +   +   +   +       
                        break;
                    case 0xf1:
                        cCombiner = '\u0328'; //    right hook  +   0328    combining ogonek    +   -   o   +       
                        break;
                    case 0xf2:
                        cCombiner = '\u0323'; //    dot below       0323    combining dot below +   +   +   +       
                        break;
                    case 0xf3:
                        cCombiner = '\u0324'; //    double dot below        0324    combining diaeresis below   +       +   +       
                        break;
                    case 0xf4:
                        cCombiner = '\u0325'; //    circle below        0325    combining ring below    +       +   +       
                        break;
                    case 0xf5:
                        cCombiner = '\u0333'; //    double underscore       0333    combining double low gedcomLine +       +   -       
                        break;
                    case 0xf6:
                        cCombiner = '\u0332'; //    underscore  +   0332    combining low gedcomLine    ?       o   ?   9   
                        break;
                    case 0xf7:
                        cCombiner = '\u0326'; //    left hook   +   0326    combining comma below   ?       o   -   10  
                        break;
                    case 0xf8:
                        cCombiner = '\u0328'; //    right cedilla       0328    combining ogonek    +   -   o   -   11  
                        break;
                    case 0xf9:
                        cCombiner = '\u032e'; //    half circle below       032e    combining breve below   +   -   o   +       
                        break;
                    case 0xfa:
                        cCombiner = '\ufe22'; //    double tilde, left half     fe22    combining double tilde left half    +   +   +   -       
                        break;
                    case 0xfb:
                        cCombiner = '\ufe23'; //    double tilde, right half        fe23    combining double tilde right half   +   +   +   -       
                        break;
                    case 0xfe:
                        cCombiner = '\u0313'; //    high comma, centered    +   0313    combining comma above   +       +   -    
                        break;
                    default:
                        sbLineConverted.Append( c );
                        if( cCombiner != '\0' )
                        {
                            sbLineConverted.Append( cCombiner );
                            cCombiner = '\0';
                        }
                        break;
                }
            }

            return sbLineConverted.ToString();

        }
#endif
        
        // Concatenate otherfile to end of basefile
        private void JoinFiles(string sBasefile, string sOtherfile)
        {
            FileStream fsOut = new FileStream( sBasefile, FileMode.Append );
            FileStream fsIn =  new FileStream( sOtherfile, FileMode.Open );

            byte[] buffer = new byte[65536];
            int nBytes = 0;
            do
            {
                nBytes = fsIn.Read( buffer, 0, 65536 );
                fsOut.Write( buffer, 0, nBytes );
            }
            while( nBytes == 65536 );

            fsOut.Close();
            fsIn.Close();

        }

        // Create a list of MFRs unique to the record
        public void ConvertMultimediaLinks( ArrayList alMultimediaLinks, ref ArrayList alUniqueFileRefs )
        {
            int nFileOrder = 0;
            alUniqueFileRefs.Clear();
            string xref = "";
            string sInputPath = Path.GetDirectoryName( MainForm.s_config.m_sInputFilename );

            foreach( CMultimediaLink ml in alMultimediaLinks )
            {
                CAsidPair asidPair = null;
                if( ml is CMultimediaLinkXref )
                {
                    // The only sort of ml that supports asids
                    CMultimediaLinkXref mlx = (CMultimediaLinkXref)ml;
                    string sAsid = mlx.m_sAsid;
                    if( sAsid != null && sAsid.Length > 0 )
                    {
                        xref = mlx.m_xref;
                        CMultimediaRecord mr = GetMultimediaRecord( xref );
                        if( mr != null )
                        {
                            if( mr.m_htAsidPairs.ContainsKey( sAsid ) )
                            {
                                asidPair = (CAsidPair)mr.m_htAsidPairs[ sAsid ];
                            }
                        }
                    }
                }

                ArrayList alFileRefs = ml.GetFileReferences();
                if( alFileRefs != null )
                {
                    foreach( CMultimediaFileReference mfr in alFileRefs )
                    {
                        if( mfr == null || mfr.m_sMultimediaFileReference == null || mfr.m_sMultimediaFileReference == "" )
                        {
                            continue;
                        }
                        // Correct pics that are missing a path
                        string path="" ;
                        try
                        {
                            path = Path.GetDirectoryName( mfr.m_sMultimediaFileReference );
                        }
                        catch( Exception e )
                        {
                            LogFile.TheLogFile.WriteLine( LogFile.DT_GEDCOM, LogFile.EDebugLevel.Error,  String.Format( "ConvertMultimediaLinks() Exception {0} caught for {1}.", e.Message, mfr.m_sMultimediaFileReference ) ); 
                        }

                        if( sInputPath != null && sInputPath.Length > 0 && (path == null || path.Length == 0) )
                        {
                            mfr.m_sMultimediaFileReference = Path.Combine( sInputPath, mfr.m_sMultimediaFileReference );
                        }

                        CMultimediaFileReference mfrUnique = new CMultimediaFileReference( mfr, false );
                        alUniqueFileRefs.Add( mfrUnique );

                        mfrUnique.m_nOrderIndex = nFileOrder++;
                        mfrUnique.m_asidPair = asidPair;
                        if( mfr.m_bEmbedded )
                        {
                            mfrUnique.m_xrefEmbedded = xref;
                        }
                        else
                        {
                            mfrUnique.m_xrefEmbedded = "";
                        }
                    }
                }
            }
        }

        private void AddBackRefs()
        {
            LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Linking source citation backreferences.");
            foreach (CIndividualRecord brir in IndividualRecords)
            {
                foreach (CIndividualEventStructure ies in brir.m_alIndividualEventStructures)
                {
                    if (ies.m_eventDetail != null)
                    {
                        foreach (CSourceCitation sc in ies.m_eventDetail.m_alSourceCitations)
                        {
                            sc.AddBackreference(new CBackReference(ERecordType.Individual, brir.m_xref, ies.Type));
                            sc.AddPicFromCitationToRecord();
                        }
                        foreach (CNoteStructure ns in ies.m_eventDetail.m_alNoteStructures)
                        {
                            if (ns != null && ns.m_alSourceCitations != null)
                            {
                                foreach (CSourceCitation sc in ns.m_alSourceCitations)
                                {
                                    sc.AddBackreference(new CBackReference(ERecordType.Individual, brir.m_xref, ies.Type));
                                    sc.AddPicFromCitationToRecord();
                                }
                            }
                        }
                    }
                }
                foreach (CPersonalNameStructure pns in brir.m_alPersonalNameStructures)
                {
                    if (pns.m_personalNamePieces != null)
                    {
                        foreach (CSourceCitation sc in pns.m_personalNamePieces.m_alSourceCitations)
                        {
                            sc.AddBackreference(new CBackReference(ERecordType.Individual, brir.m_xref, "NAME"));
                            sc.AddPicFromCitationToRecord();
                        }
                    }
                }
                foreach (CSourceCitation sc in brir.m_alSourceCitations)
                {
                    sc.AddBackreference(new CBackReference(ERecordType.Individual, brir.m_xref, ""));
                    sc.AddPicFromCitationToRecord();
                }
            }
            foreach (CFamilyRecord brfr in FamilyRecords)
            {
                foreach (CFamilyEventStructure fes in brfr.m_alFamilyEventStructures)
                {
                    if (fes.m_eventDetail != null)
                    {
                        foreach (CSourceCitation sc in fes.m_eventDetail.m_alSourceCitations)
                        {
                            sc.AddBackreference(new CBackReference(ERecordType.Family, brfr.m_xref, fes.Type));
                            sc.AddPicFromCitationToRecord();
                        }
                        foreach (CNoteStructure ns in fes.m_eventDetail.m_alNoteStructures)
                        {
                            if (ns != null && ns.m_alSourceCitations != null)
                            {

                                foreach (CSourceCitation sc in ns.m_alSourceCitations)
                                {
                                    sc.AddBackreference(new CBackReference(ERecordType.Family, brfr.m_xref, fes.Type));
                                    sc.AddPicFromCitationToRecord();
                                }
                            }
                        }

                    }
                }
                foreach (CSourceCitation sc in brfr.m_alSourceCitations)
                {
                    sc.AddBackreference(new CBackReference(ERecordType.Family, brfr.m_xref, ""));
                    sc.AddPicFromCitationToRecord();
                }

            }
            foreach (CNoteRecord brnr in m_alNoteRecords)
            {
                foreach (CSourceCitation sc in brnr.m_alSourceCitations)
                {
                    sc.AddBackreference(new CBackReference(ERecordType.Note, brnr.m_xref, ""));
                    sc.AddPicFromCitationToRecord();
                }
            }
            
        }

        private void LinkAdoptees()
        {
            // Tie up adopted individuals with their associated fr
            LogFile.TheLogFile.WriteLine(LogFile.DT_GEDCOM, LogFile.EDebugLevel.Note, "Linking adoptees.");
            foreach (CIndividualRecord adopIr in m_alAdoptedIndividuals)
            {
                CEventDetail adopEvent = adopIr.GetEvent("ADOP");
                if (adopEvent != null)
                {
                    string adopFamXref = adopEvent.m_xrefFam;
                    bool adopHusband = adopEvent.m_bAdoptedByHusband;
                    bool adopWife = adopEvent.m_bAdoptedByWife;
                    CFamilyRecord adopFam = GetFamilyRecord(adopFamXref);
                    if (adopFam != null && (adopHusband || adopWife))
                    {
                        if (adopHusband)
                        {
                            CIndividualRecord irAdopHusband = GetIndividualRecord(adopFam.m_xrefHusband);
                            if (irAdopHusband != null)
                            {
                                CIndividualEventStructure husbandAdopEvent = new CIndividualEventStructure(adopEvent);
                                husbandAdopEvent.Type = "GEDMILL_ADOPTION_OF_CHILD"; // Special GEDmill only event
                                husbandAdopEvent.m_eventDetail.m_xrefAdoptedChild = adopIr.m_xref;
                                irAdopHusband.m_alIndividualEventStructures.Add(husbandAdopEvent);
                            }
                        }
                        if (adopWife)
                        {
                            CIndividualRecord irAdopWife = GetIndividualRecord(adopFam.m_xrefWife);
                            if (irAdopWife != null)
                            {
                                CIndividualEventStructure wifeAdopEvent = new CIndividualEventStructure(adopEvent);
                                wifeAdopEvent.Type = "GEDMILL_ADOPTION_OF_CHILD"; // Special GEDmill only event
                                wifeAdopEvent.m_eventDetail.m_xrefAdoptedChild = adopIr.m_xref;
                                irAdopWife.m_alIndividualEventStructures.Add(wifeAdopEvent);
                            }
                        }

                    }
                }
            }
            
        }

    } // End of class CGedcom
} // End of namespace
