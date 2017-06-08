/* Form1.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

// ReSharper disable InconsistentNaming
// ReSharper disable SpecifyACultureInStringConversionExplicitly
// ReSharper disable UseObjectOrCollectionInitializer


namespace GEDmill
{   
    // The main from from which the application is operated. Contains the GUI controls and the control handlers.
    // This file contains the GUI building code. The file Form2.cs contains the GUI event handlers.
    public partial class MainForm
    {
        // Enum used to identify release version when loading changes stored with earlier versions of the app
        // Only need to add new version if "changes-file" has changed. E.g. 1.11.0 uses same format as v1p10.
        enum EVersion { pre1p8, v1p8, v1p9, v1p10 };

        // The current version of this app, for display purposes.
        public static string m_sSoftwareVersion = "1.11.0";

        // The name of the app for display purposes
        public static string m_sSoftwareName = "GEDmill " + m_sSoftwareVersion;

        // Effectively a global reference to the app's GUI object!
        public static MainForm m_mainForm = null;

        // Stores user's configuration parameters (mostly set using Settings panes)
        public static CConfig s_config;

        // Filename for the online help (as in "on the same system", as opposed to offline e.g. printed manual)
        public static string m_sHelpFilename = "GEDmill Help.chm";

        // Specifies which panel of the wizard the user is viewing (i.e. which stage in the app they are at)
        private int m_nCurrentPanel;

        // Application has an important state whereby it displays the settings panes. 
        // The main GUI navigation buttons behave differently in this mode.
        private bool m_bConfigPanelOn;

        // The Settings button changes its label when the settings panes are displayed
        // These strings define the labels in each state
        private const string m_sConfigButtonTextOn = "&Settings...";
        private const string m_sConfigButtonTextOff = "&OK";

        // Scales the size of the main GUI
        private readonly Point m_ptDefaultButtonSize;

        // Scales the size of the config panels GUI
        private readonly Point m_ptConfigButtonSize;
        
        // Public so CGedcom can change it. Should really refactor so that it's a member of CGedcom.
        public int m_nPruneExcluded;

        // Public so CGedcom can change it. Should really refactor so that it's a member of CGedcom.
        public int m_nPruneIncluded;

        // Indicates user has made changes to data from GEDCOM file
        public bool m_bPrunepanelDataChanged;

        // Check event gets called when program builds the list. Don't want to enable buttons in that case.
        private bool m_bDisablePrunepanelCheckEvent; 

        // The GUI controls and control handlers
        private Button              m_buttonNext;
        private Button              m_buttonBack;
        private Button              m_buttonCancel;
        private Button              m_buttonSettings;
        private Button              m_buttonSettingsCancel;
        private Button              m_buttonHelp;
        private Panel               m_panelWelcome;
        private Panel               m_panelChooseGedcom;
        private Panel               m_panelChooseOutput;
        private Panel               m_panelPruneRecords;
        private Panel               m_panelSelectKey;
        private Panel               m_panelAllDone;
        private TabControl          m_tabcontrolConfigPanel; 
        private Label               m_labelConfigFrontImageEdit;
        private TextBox             m_textboxConfigFrontImageEdit;
        private Button              m_buttonConfigFrontImageBrowse; 
        private TextBox             m_textboxConfigBackImageEdit;
        private Label               m_labelConfigBackImageEdit;
        private Button              m_buttonConfigBackImageBrowse;
        private Label               m_labelConfigIndiImageSize;
        private Label               m_labelConfigIndiImageWidth;
        private TextBox             m_textboxConfigIndiImageWidth;
        private Label               m_labelConfigIndiImageHeight;
        private TextBox             m_textboxConfigIndiImageHeight;
        private Label               m_labelConfigSourceImageSize;
        private Label               m_labelConfigSourceImageWidth;
        private TextBox             m_textboxConfigSourceImageWidth;
        private Label               m_labelConfigSourceImageHeight;
        private TextBox             m_textboxConfigSourceImageHeight;
        private Label               m_labelConfigThumbnailImageSize;
        private Label               m_labelConfigThumbnailImageWidth;
        private TextBox             m_textboxConfigThumbnailImageWidth;
        private Label               m_labelConfigThumbnailImageHeight;
        private TextBox             m_textboxConfigThumbnailImageHeight;
        private Label               m_labelConfigCharset;
        private ComboBox            m_comboboxConfigCharset;
        private Label               m_labelConfigHtmlExtn;
        private ComboBox            m_comboboxConfigHtmlExtn;
        private CheckBox            m_checkboxConfigW3C;
        private CheckBox            m_checkboxConfigUserRecFilename;
        private Label               m_labelConfigCustomFooter;
        private TextBox             m_textboxConfigCustomFooter;
        private Label               m_labelConfigFooterIsHtml;
        private CheckBox            m_checkboxConfigFooterIsHtml;
        private CheckBox            m_checkboxConfigConserveTreeWidth;
        private CheckBox            m_checkboxConfigKeepSiblingOrder;
        private GroupBox            m_groupboxMiniTreeColours;
        private Button              m_buttonConfigMiniTreeColourIndiBackground;
        private Button              m_buttonConfigMiniTreeColourIndiHighlight;
        private Button              m_buttonConfigMiniTreeColourIndiBgConcealed;
        private Button              m_buttonConfigMiniTreeColourIndiShade;
        private Button              m_buttonConfigMiniTreeColourIndiText;
        private Button              m_buttonConfigMiniTreeColourIndiLink;
        private Button              m_buttonConfigMiniTreeColourBranch;
        private Button              m_buttonConfigMiniTreeColourIndiBorder;
        private Button              m_buttonConfigMiniTreeColourIndiFgConcealed;
        private CheckBox            m_checkboxConfigAllowMultimedia;
        private Label               m_labelConfigNoName;
        private TextBox             m_textboxConfigNoName;
        private GroupBox            m_groupboxConfigWithheldName;
        private RadioButton         m_radiobuttonConfigWithheldNameLabel;
        private RadioButton         m_radiobuttonConfigWithheldNameName;
        private TextBox             m_textboxConfigWithheldName;
        private CheckBox            m_checkboxConfigCapNames;
        private CheckBox            m_checkboxConfigCapEvents;
        private CheckBox            m_checkboxConfigHideEmails;
        private CheckBox            m_checkboxConfigOccupationHeadline;
        private CheckBox            m_checkboxConfigAllowTrailingSpaces;
        private CheckBox            m_checkboxConfigShowWithheldRecords;
        private Label               m_labelConfigTabSpaces;
        private TextBox             m_textboxConfigTabSpaces;
        private Label               m_labelConfigCommentaryIsHtml; // Opening bracket
        private CheckBox            m_checkboxConfigCommentaryIsHtml;
        private Label               m_labelConfigCommentary;
        private TextBox             m_textboxConfigCommentary;
        private Label               m_labelConfigUserLink;
        private TextBox             m_textboxConfigUserLink;
        private Label               m_labelConfigEmail;
        private TextBox             m_textboxConfigEmail;
        private TextBox             m_textboxConfigIndexName;
        private Label               m_labelConfigIndexName;
        private Label               m_labelConfigIndexNameExtn;
        private CheckBox            m_checkboxConfigPreserveFrontPage;
        private TextBox             m_textboxConfigStylesheetName;
        private Label               m_labelConfigStylesheetName;
        private Label               m_labelConfigStylesheetNameExtn;
        private CheckBox            m_checkboxConfigPreserveStylesheet;
        private CheckBox            m_checkboxConfigIncludeHelppage;
        private CheckBox            m_checkboxConfigStats;
        private CheckBox            m_checkboxConfigCdrom;
        private CheckBox            m_checkboxConfigIndiImages;
        private CheckBox            m_checkboxConfigNonPictures;
        private CheckBox            m_checkboxConfigKeepOriginals;
        private CheckBox            m_checkboxConfigRenameOriginals;
        private CheckBox            m_checkboxConfigMultiPageIndex;
        private CheckBox            m_checkboxConfigUserRefInIndex;
        private Label               m_labelConfigMultiPageIndexNumber;
        private TextBox             m_textboxConfigMultiPageIndexNumber;
        private CheckBox            m_checkboxConfigTreeDiagrams;
        private CheckBox            m_checkboxConfigTreeDiagramsFakeBg;
        private Label               m_labelConfigTreeDiagramsFormat;
        private ComboBox            m_comboboxConfigTreeDiagramsFormat;
        private CheckBox            m_checkboxConfigUseBom;
        private CheckBox            m_checkboxConfigSupressBackreferences;
        private Label               m_labelWelcomeContinue;
        private Label               m_labelWelcomeVersion;
        private Label               m_labelWelcomeSubtitle;
        private PictureBox          m_picturebox;
        private Label               m_labelChooseGedcomInstructions;
        private Button              m_buttonChooseGedcomBrowse;
        private Label               m_labelChooseGedcom;
        private TextBox             m_textboxChooseGedcom;
        private TextBox             m_textboxChooseOutput;
        private Label               m_labelChooseOutputInstructions;
        private Label               m_labelChooseOutput;
        private Label               m_labelChooseOutputContinue;
        private Button              m_buttonChooseOutputBrowse;
        private Label               m_labelPruneRecordsContinue;
        private SortableListView    m_listviewPruneRecordsIndis;
        private SortableListView    m_listviewPruneRecordsSources;
        private Label               m_labelPruneRecordsInstructions;
        private Label               m_labelPruneRecordsButtons;
        private Button              m_buttonPruneRecordsSave;
        private Button              m_buttonPruneRecordsLoad;
        // TODO how different from  m_labelSelectKeyIndividuals?
        private Label               m_labelSelectKey; 
        private TextBox             m_textboxSelectKey;
        private Label               m_labelSelectKeyIndividuals;
        private ListBox             m_listboxSelectKey;
        private Button              m_buttonSelectKeyAdd;
        private Button              m_buttonSelectKeyDelete;
        private Label               m_labelSelectKeyInstructions;
        private Label               m_labelAllDoneThankYou;
        private Label               m_labelAllDoneDirectory;
        private Label               m_labelAllDoneStartFile;
        private CheckBox            m_checkboxAllDoneShowSite;
        private LinkLabel           m_linklabelAllDone;
        private PictureBox          m_pictureBoxWelcome;
        // Required designer variable.
        private readonly Container components = null;
        private ContextMenu         m_contextmenuPruneRecordsIndis;
        private ContextMenu         m_contextmenuPruneRecordsSources;
        private MenuItem            m_menuitemPruneRecordsIndisUnconnected;
        private MenuItem            m_menuitemPruneRecordsIndisDescendantsExc;
        private MenuItem            m_menuitemPruneRecordsIndisDescendantsInc;
        private MenuItem            m_menuitemPruneRecordsIndisAncestorsInc;
        private MenuItem            m_menuitemPruneRecordsIndisAncestorsExc;
        private MenuItem            m_menuitemPruneRecordsIndisDetails;
        private MenuItem            m_menuitemPruneRecordsSourcesRemovePics;
        private MenuItem            m_menuitemPruneRecordsSourcesDetails;
        private HelpProvider        m_helpProvider;
        private TabControl          m_tabcontrolPruneRecords;
        // Exclude individuals and/or their images.
        private TabPage             m_tabpagePruneRecordsIndis;
        // Exclude sources and/or their images.
        private TabPage             m_tabpagePruneRecordsSources;

        private readonly ColorDialog         m_colordialogConfigMiniTree;

        // When user redefines the mini tree colours, these hold the new colours until they click OK.
        private Color               m_colorConfigMiniTreeBranch;
        private Color               m_colorConfigMiniTreeIndiBorder;
        private Color               m_colorConfigMiniTreeIndiBackground;
        private Color               m_colorConfigMiniTreeIndiHighlight;
        private Color               m_colorConfigMiniTreeIndiBgConcealed;
        private Color               m_colorConfigMiniTreeIndiFgConcealed;
        private Color               m_colorConfigMiniTreeIndiShade;
        private Color               m_colorConfigMiniTreeIndiText;
        private Color               m_colorConfigMiniTreeIndiLink;
        private Color               m_colorConfigMiniTreeBackground;

        // Constructor. Initialise and create GUI.
        public MainForm( bool bResetConfig )
        {
            // Set some values that scale the size of the GUI
            m_ptDefaultButtonSize = new Point(75, 23);
            m_ptConfigButtonSize = new Point(92, 23);

            m_colordialogConfigMiniTree = new ColorDialog();
            m_colordialogConfigMiniTree.FullOpen = true;
            m_colordialogConfigMiniTree.SolidColorOnly = true;
        
            s_config = new CConfig();
            if( !bResetConfig )
            {
                // Read back any previously stored settings.
                s_config.RecoverSettings();
            }
            else
            {
                // Save default settings without neeeding user to complete app
                s_config.StoreSettings();
            }

            // Creates the entire GUI
            InitializeComponent();

            m_nCurrentPanel = 1;
            m_bConfigPanelOn = false;
            m_nPruneExcluded = 0;
            m_nPruneExcluded = 0;

            CListableName.s_sUnknownName = s_config.m_sUnknownName;
            CListableBool.s_sUnknownName = s_config.m_sUnknownName;

            m_bPrunepanelDataChanged = false;
            m_bDisablePrunepanelCheckEvent = false;

            ShowCurrentPanel();
        }

        // Clean up any resources being used.
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        // This is the main function that builds the whole GUI.
        private void InitializeComponent()
        {
            ResourceManager resources = new ResourceManager(typeof(MainForm));
            
            CreateControls();

            SuspendAllLayout();

            InitialiseButtons();
            InitialiseWelcomePanel(resources);
            InitialiseChooseGedcomPanel();
            InitialisePruneRecordsPanel();
            InitialiseKeyIndividualsPanel();
            InitialiseSettingsPanes();
            InitialiseChooseOutputPanel();
            InitialiseAllDonePanel();
            AddSettingsPanes();
            InitialiseMainForm(resources);

            ResumeAllLayout();
        }


        // Calls SuspendLayout on the GUI elements
        private void SuspendAllLayout()
        {
            m_panelWelcome.SuspendLayout();
            m_panelChooseGedcom.SuspendLayout();
            m_panelChooseOutput.SuspendLayout();
            m_panelPruneRecords.SuspendLayout();
            m_panelSelectKey.SuspendLayout();
            m_tabcontrolConfigPanel.SuspendLayout();
            m_panelAllDone.SuspendLayout();
            SuspendLayout();
        }

        // Calls ResumeLayout on the GUI elements
        private void ResumeAllLayout()
        {
            m_panelWelcome.ResumeLayout(false);
            m_panelChooseGedcom.ResumeLayout(false);
            m_panelChooseOutput.ResumeLayout(false);
            m_panelPruneRecords.ResumeLayout(false);
            m_panelSelectKey.ResumeLayout(false);
            m_tabcontrolConfigPanel.ResumeLayout(false);
            m_panelAllDone.ResumeLayout(false);
            ResumeLayout(false);
        }

        // Builds the GUI
        private void InitialiseMainForm(ResourceManager resources)
        {
            AutoScaleMode = AutoScaleMode.None;
            AutoScaleBaseSize = new Size(5, 13);
            ClientSize = new Size(506, 320);
            Controls.Add(m_panelWelcome);
            Controls.Add(m_panelChooseGedcom);
            Controls.Add(m_panelPruneRecords);
            Controls.Add(m_panelSelectKey);
            Controls.Add(m_tabcontrolConfigPanel);
            Controls.Add(m_panelChooseOutput);
            Controls.Add(m_panelAllDone);
            Controls.Add(m_buttonCancel);
            Controls.Add(m_buttonSettings);
            Controls.Add(m_buttonSettingsCancel);
            Controls.Add(m_buttonHelp);
            Controls.Add(m_buttonBack);
            Controls.Add(m_buttonNext);
            Controls.Add(m_picturebox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = ((Icon)(resources.GetObject("$Icon")));
            MaximumSize = new Size(512, 355);
            MinimumSize = new Size(512, 355);
            Name = "MainForm";
            Text = "GEDmill";
        }

        // Instantiates all the GUI elements
        private void CreateControls()
        {
            m_buttonNext = new Button();
            m_buttonBack = new Button();
            m_buttonCancel = new Button();
            m_buttonHelp = new Button();
            m_buttonSettings = new Button();
            m_buttonSettingsCancel = new Button();
            m_labelConfigFrontImageEdit = new Label();
            m_textboxConfigFrontImageEdit = new TextBox();
            m_buttonConfigFrontImageBrowse = new Button();
            m_textboxConfigBackImageEdit = new TextBox();
            m_labelConfigBackImageEdit = new Label();
            m_buttonConfigBackImageBrowse = new Button();
            m_labelConfigIndiImageSize = new Label();
            m_labelConfigIndiImageWidth = new Label();
            m_textboxConfigIndiImageWidth = new TextBox();
            m_labelConfigIndiImageHeight = new Label();
            m_textboxConfigIndiImageHeight = new TextBox();
            m_labelConfigSourceImageSize = new Label();
            m_labelConfigSourceImageWidth = new Label();
            m_textboxConfigSourceImageWidth = new TextBox();
            m_labelConfigSourceImageHeight = new Label();
            m_textboxConfigSourceImageHeight = new TextBox();
            m_labelConfigThumbnailImageSize = new Label();
            m_labelConfigThumbnailImageWidth = new Label();
            m_textboxConfigThumbnailImageWidth = new TextBox();
            m_labelConfigThumbnailImageHeight = new Label();
            m_textboxConfigThumbnailImageHeight = new TextBox();
            m_labelConfigCharset = new Label();
            m_comboboxConfigCharset = new ComboBox();
            m_labelConfigHtmlExtn = new Label();
            m_comboboxConfigHtmlExtn = new ComboBox();
            m_checkboxConfigW3C = new CheckBox();
            m_checkboxConfigUserRecFilename = new CheckBox();
            m_labelConfigCustomFooter = new Label();
            m_textboxConfigCustomFooter = new TextBox();
            m_labelConfigFooterIsHtml = new Label();
            m_checkboxConfigFooterIsHtml = new CheckBox();
            m_checkboxConfigConserveTreeWidth = new CheckBox();
            m_checkboxConfigKeepSiblingOrder = new CheckBox();
            m_groupboxMiniTreeColours = new GroupBox();
            m_buttonConfigMiniTreeColourIndiBackground = new Button();
            m_buttonConfigMiniTreeColourIndiHighlight = new Button();
            m_buttonConfigMiniTreeColourIndiBgConcealed = new Button();
            m_buttonConfigMiniTreeColourIndiShade = new Button();
            m_buttonConfigMiniTreeColourIndiText = new Button();
            m_buttonConfigMiniTreeColourIndiLink = new Button();
            m_buttonConfigMiniTreeColourBranch = new Button();
            m_buttonConfigMiniTreeColourIndiBorder = new Button();
            m_buttonConfigMiniTreeColourIndiFgConcealed = new Button();
            m_checkboxConfigAllowMultimedia = new CheckBox();
            m_checkboxConfigUseBom = new CheckBox();
            m_checkboxConfigSupressBackreferences = new CheckBox();
            m_labelConfigNoName = new Label();
            m_textboxConfigNoName = new TextBox();
            m_groupboxConfigWithheldName = new GroupBox();
            m_radiobuttonConfigWithheldNameLabel = new RadioButton();
            m_radiobuttonConfigWithheldNameName = new RadioButton();
            m_textboxConfigWithheldName = new TextBox();
            m_checkboxConfigCapNames = new CheckBox();
            m_checkboxConfigCapEvents = new CheckBox();
            m_checkboxConfigHideEmails = new CheckBox();
            m_checkboxConfigOccupationHeadline = new CheckBox();
            m_checkboxConfigAllowTrailingSpaces = new CheckBox();
            m_checkboxConfigShowWithheldRecords = new CheckBox();
            m_labelConfigTabSpaces = new Label();
            m_textboxConfigTabSpaces = new TextBox();
            m_labelConfigCommentary = new Label();
            m_labelConfigCommentaryIsHtml = new Label(); // Opening bracket
            m_checkboxConfigCommentaryIsHtml = new CheckBox();
            m_textboxConfigCommentary = new TextBox();
            m_labelConfigEmail = new Label();
            m_textboxConfigEmail = new TextBox();
            m_labelConfigUserLink = new Label();
            m_textboxConfigUserLink = new TextBox();
            m_textboxConfigIndexName = new TextBox();
            m_labelConfigIndexName = new Label();
            m_labelConfigIndexNameExtn = new Label();
            m_checkboxConfigPreserveFrontPage = new CheckBox();
            m_textboxConfigStylesheetName = new TextBox();
            m_labelConfigStylesheetName = new Label();
            m_labelConfigStylesheetNameExtn = new Label();
            m_checkboxConfigPreserveStylesheet = new CheckBox();
            m_checkboxConfigIncludeHelppage = new CheckBox();
            m_checkboxConfigStats = new CheckBox();
            m_checkboxConfigTreeDiagrams = new CheckBox();
            m_checkboxConfigTreeDiagramsFakeBg = new CheckBox();
            m_labelConfigTreeDiagramsFormat = new Label();
            m_comboboxConfigTreeDiagramsFormat = new ComboBox();
            m_checkboxConfigMultiPageIndex = new CheckBox();
            m_checkboxConfigUserRefInIndex = new CheckBox();
            m_labelConfigMultiPageIndexNumber = new Label();
            m_textboxConfigMultiPageIndexNumber = new TextBox();
            m_checkboxConfigIndiImages = new CheckBox();
            m_checkboxConfigNonPictures = new CheckBox();
            m_checkboxConfigCdrom = new CheckBox();
            m_checkboxConfigRenameOriginals = new CheckBox();
            m_checkboxConfigKeepOriginals = new CheckBox();
            m_panelWelcome = new Panel();
            m_pictureBoxWelcome = new PictureBox();
            m_labelWelcomeContinue = new Label();
            m_labelWelcomeVersion = new Label();
            m_labelWelcomeSubtitle = new Label();
            m_picturebox = new PictureBox();
            m_panelChooseGedcom = new Panel();
            m_buttonChooseGedcomBrowse = new Button();
            m_labelChooseGedcom = new Label();
            m_labelChooseGedcomInstructions = new Label();
            m_textboxChooseGedcom = new TextBox();
            m_panelChooseOutput = new Panel();
            m_textboxChooseOutput = new TextBox();
            m_labelChooseOutputInstructions = new Label();
            m_labelChooseOutput = new Label();
            m_buttonChooseOutputBrowse = new Button();
            m_labelChooseOutputContinue = new Label();
            m_panelPruneRecords = new Panel();
            m_labelPruneRecordsContinue = new Label();
            m_listviewPruneRecordsIndis = new SortableListView();
            m_listviewPruneRecordsSources = new SortableListView();
            m_labelPruneRecordsInstructions = new Label();
            m_labelPruneRecordsButtons = new Label();
            m_buttonPruneRecordsSave = new Button();
            m_buttonPruneRecordsLoad = new Button();
            m_panelSelectKey = new Panel();
            m_tabcontrolConfigPanel = new TabControl();
            m_tabcontrolPruneRecords = new TabControl();
            m_labelSelectKey = new Label();
            m_textboxSelectKey = new TextBox();
            m_labelSelectKeyInstructions = new Label();
            m_labelSelectKeyIndividuals = new Label();
            m_listboxSelectKey = new ListBox();
            m_buttonSelectKeyAdd = new Button();
            m_buttonSelectKeyDelete = new Button();
            m_panelAllDone = new Panel();
            m_checkboxAllDoneShowSite = new CheckBox();
            m_linklabelAllDone = new LinkLabel();
            m_labelAllDoneThankYou = new Label();
            m_labelAllDoneDirectory = new Label();
            m_labelAllDoneStartFile = new Label();
            m_contextmenuPruneRecordsIndis = new ContextMenu();
            m_contextmenuPruneRecordsSources = new ContextMenu();
            m_helpProvider = new HelpProvider();
            m_helpProvider.HelpNamespace = s_config.m_sApplicationPath + "\\" + m_sHelpFilename;
        }

        // Builds the main GUI buttons: Next, Back, Help, Settings, Quit
        private void InitialiseButtons()
        {
            // 
            // nextButton
            // 
            m_buttonNext.Location = new Point(424, 288);
            m_buttonNext.Name = "m_buttonNext";
            m_buttonNext.TabIndex = 7;
            m_buttonNext.Text = "&Next >";
            m_buttonNext.Click += nextButton_click;

            // 
            // backButton
            // 
            m_buttonBack.Location = new Point(344, 288);
            m_buttonBack.Name = "m_buttonBack";
            m_buttonBack.TabIndex = 8;
            m_buttonBack.Text = "< &Back";
            m_buttonBack.Click += backButton_click;

            // 
            // cancelButton
            // 
            m_buttonCancel.Location = new Point(8, 288);
            m_buttonBack.Name = "m_buttonBack";
            m_buttonCancel.TabIndex = 10;
            m_buttonCancel.Text = "&Quit";
            m_buttonCancel.Click += cancelButton_click;

            // 
            // helpButton
            // 
            m_buttonHelp.Location = new Point(186, 288);
            m_buttonBack.Name = "m_buttonBack";
            m_buttonHelp.TabIndex = 11;
            m_buttonHelp.Text = "&Help";
            m_buttonHelp.Click += helpButton_click;
            m_helpProvider.SetHelpKeyword(m_buttonHelp, "HelpButtonHelpKeyword");
            m_helpProvider.SetHelpNavigator(m_buttonHelp, HelpNavigator.TableOfContents);
            m_helpProvider.SetShowHelp(m_buttonHelp, true);

            // 
            // configButton
            // 
            m_buttonSettings.Location = new Point(88, 288);
            m_buttonSettings.Name = "m_buttonSettings";
            m_buttonSettings.Size = new Size(m_ptConfigButtonSize);
            m_buttonSettings.TabIndex = 12;
            m_buttonSettings.Text = m_sConfigButtonTextOn;
            m_buttonSettings.Click += configButton_click;

            // 
            // configCancelButton
            // 
            m_buttonSettingsCancel.Location = new Point(424, 288);
            m_buttonSettingsCancel.Name = "m_buttonSettingsCancel";
            m_buttonSettingsCancel.TabIndex = 13;
            m_buttonSettingsCancel.Text = "&Cancel";
            m_buttonSettingsCancel.Click += configCancelButton_click;
            m_buttonSettingsCancel.Visible = false; // Only visible when configPanel is on
        }



        // Builds the GUI for the first panel, which introduces the application and displays the version number.
        private void InitialiseWelcomePanel(ResourceManager resources)
        {
            // 
            // Welcome Panel 
            // 
            m_panelWelcome.Controls.Add(m_pictureBoxWelcome);
            m_panelWelcome.Controls.Add(m_labelWelcomeContinue);
            m_panelWelcome.Controls.Add(m_labelWelcomeVersion);
            m_panelWelcome.Controls.Add(m_labelWelcomeSubtitle);
            m_panelWelcome.Location = new Point(216, 0);
            m_panelWelcome.Name = "m_panelWelcome";
            m_panelWelcome.Size = new Size(280, 272);
            m_panelWelcome.TabIndex = 6;


            // 
            // Welcome Panel PictureBox
            // 
            m_picturebox.Image = ((Image)(resources.GetObject("panel1PictureBox.Image")));
            m_picturebox.Location = new Point(8, 8);
            m_picturebox.Name = "m_picturebox";
            m_picturebox.Size = new Size(200, 264);
            m_picturebox.TabIndex = 8;
            m_picturebox.TabStop = false;

            // 
            // Welcome Panel pictureBox1
            // 
            m_pictureBoxWelcome.Image = ((Image)(resources.GetObject("title.Image")));
            m_pictureBoxWelcome.Size = new Size(177, 65);
            m_pictureBoxWelcome.Location = new Point(56, 50);
            m_pictureBoxWelcome.Name = "m_pictureBoxWelcome";
            m_pictureBoxWelcome.TabIndex = 8;
            m_pictureBoxWelcome.TabStop = false;

            // 
            // Welcome Panel ContinueLabel
            // 
            m_labelWelcomeContinue.Location = new Point(0, 256);
            m_labelWelcomeContinue.Name = "m_labelWelcomeContinue";
            m_labelWelcomeContinue.Size = new Size(268, 16);
            m_labelWelcomeContinue.TabIndex = 5;
            m_labelWelcomeContinue.Text = "To continue, click Next.";
            m_labelWelcomeContinue.TextAlign = ContentAlignment.BottomRight;

            // 
            // Welcome Panel versionLabel
            // 
            m_labelWelcomeVersion.Location = new Point(88, 116);
            m_labelWelcomeVersion.Name = "m_labelWelcomeVersion";
            m_labelWelcomeVersion.Size = new Size(112, 16);
            m_labelWelcomeVersion.TabIndex = 4;
            m_labelWelcomeVersion.Text = "version " + m_sSoftwareVersion;
            m_labelWelcomeVersion.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // Welcome Panel SubtitleLabel
            // 
            m_labelWelcomeSubtitle.Font = new Font("Arial", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            m_labelWelcomeSubtitle.Location = new Point(0, 138);
            m_labelWelcomeSubtitle.Name = "m_labelWelcomeSubtitle";
            m_labelWelcomeSubtitle.Size = new Size(280, 54);
            m_labelWelcomeSubtitle.TabIndex = 3;
            m_labelWelcomeSubtitle.Text = "Family History Website Creator";
            m_labelWelcomeSubtitle.TextAlign = ContentAlignment.MiddleCenter;
        }

        // Builds the GUI for the panel where the user chooses which GEDCOM file to process
        private void InitialiseChooseGedcomPanel()
        {
            // 
            // Choose Gedcom 
            // 
            m_panelChooseGedcom.Controls.Add(m_buttonChooseGedcomBrowse);
            m_panelChooseGedcom.Controls.Add(m_labelChooseGedcom);
            m_panelChooseGedcom.Controls.Add(m_labelChooseGedcomInstructions);
            m_panelChooseGedcom.Controls.Add(m_textboxChooseGedcom);
            m_panelChooseGedcom.Location = new Point(216, 0);
            m_panelChooseGedcom.Name = "m_panelChooseGedcom";
            m_panelChooseGedcom.Size = new Size(280, 272);
            m_panelChooseGedcom.TabIndex = 6;

            // 
            // Choose Gedcom BrowseButton
            // 
            m_buttonChooseGedcomBrowse.Location = new Point(200, 120);
            m_buttonChooseGedcomBrowse.Name = "m_buttonChooseGedcomBrowse";
            m_buttonChooseGedcomBrowse.TabIndex = 4;
            m_buttonChooseGedcomBrowse.Text = "B&rowse...";
            m_buttonChooseGedcomBrowse.Click += buttonChooseGedcomBrowse_click;

            // 
            // Choose Gedcom EditLabel
            // 
            m_labelChooseGedcom.Location = new Point(0, 96);
            m_labelChooseGedcom.Name = "m_labelChooseGedcom";
            m_labelChooseGedcom.RightToLeft = RightToLeft.No;
            m_labelChooseGedcom.Size = new Size(152, 24);
            m_labelChooseGedcom.TabIndex = 3;
            m_labelChooseGedcom.Text = "&File:";
            m_labelChooseGedcom.TextAlign = ContentAlignment.BottomLeft;

            // 
            // Choose Gedcom InstructionLabel
            // 
            m_labelChooseGedcomInstructions.Location = new Point(0, 16);
            m_labelChooseGedcomInstructions.Name = "m_labelChooseGedcomInstructions";
            m_labelChooseGedcomInstructions.Size = new Size(288, 80);
            m_labelChooseGedcomInstructions.TabIndex = 2;
            m_labelChooseGedcomInstructions.Text = "First, please select the GEDCOM file containing your family tree data. Most Family Tree software is capable of exporting its data in GEDCOM format, even though this may not be the format it usually uses.";

            // 
            // Choose Gedcom EditBox
            // 
            m_textboxChooseGedcom.Location = new Point(0, 120);
            m_textboxChooseGedcom.Name = "m_textboxChooseGedcom";
            m_textboxChooseGedcom.Size = new Size(192, 20);
            m_textboxChooseGedcom.TabIndex = 1;
            m_textboxChooseGedcom.Text = "";
            m_textboxChooseGedcom.TextChanged += textboxChooseGedcom_textChanged;

        }

        // Builds the GUI for the Prune panel, i.e. the panel where the user selects which individuals and sources to include/exculde from the website.
        private void InitialisePruneRecordsPanel()
        {
            m_panelPruneRecords.Controls.Add(m_labelPruneRecordsContinue);
            m_panelPruneRecords.Controls.Add(m_labelPruneRecordsInstructions);
            m_panelPruneRecords.Controls.Add(m_labelPruneRecordsButtons);
            m_panelPruneRecords.Controls.Add(m_buttonPruneRecordsSave);
            m_panelPruneRecords.Controls.Add(m_buttonPruneRecordsLoad);
            m_panelPruneRecords.Location = new Point(0, 0);
            m_panelPruneRecords.Name = "m_panelPruneRecords";
            m_panelPruneRecords.Size = new Size(496, 272);
            m_panelPruneRecords.TabIndex = 11;
            m_panelPruneRecords.Controls.Add(m_tabcontrolPruneRecords);

            // 
            // m_labelPruneRecordsContinue
            // 
            m_labelPruneRecordsContinue.Location = new Point(256, 288);
            m_labelPruneRecordsContinue.Name = "m_labelPruneRecordsContinue";
            m_labelPruneRecordsContinue.Size = new Size(256, 16);
            m_labelPruneRecordsContinue.TabIndex = 5;
            m_labelPruneRecordsContinue.Text = "When you have finished selecting, click Next.";

            //
            // m_pruneIndividualsContextMenu
            //
            m_menuitemPruneRecordsIndisDescendantsExc = new MenuItem("E&xclude all descendants of this person", pruneIndividualsContextMenuDescendantsExc_Click);
            m_menuitemPruneRecordsIndisAncestorsExc = new MenuItem("Exclude all &ancestors of this person", pruneIndividualsContextMenuAncestorsExc_Click);
            m_menuitemPruneRecordsIndisDescendantsInc = new MenuItem("In&clude all descendants of this person", pruneIndividualsContextMenuDescendantsInc_Click);
            m_menuitemPruneRecordsIndisAncestorsInc = new MenuItem("Include all a&ncestors of this person", pruneIndividualsContextMenuAncestorsInc_Click);
            m_menuitemPruneRecordsIndisUnconnected = new MenuItem("E&xclude individuals unless navigable from this person", pruneIndividualsContextMenuUnconnected_Click);
            m_menuitemPruneRecordsIndisDetails = new MenuItem("&Details and pictures...", pruneIndividualsContextMenuDetails_Click);
            m_contextmenuPruneRecordsIndis.MenuItems.Add(m_menuitemPruneRecordsIndisDetails);
            m_contextmenuPruneRecordsIndis.MenuItems.Add(new MenuItem("-"));
            m_contextmenuPruneRecordsIndis.MenuItems.Add(m_menuitemPruneRecordsIndisDescendantsExc);
            m_contextmenuPruneRecordsIndis.MenuItems.Add(m_menuitemPruneRecordsIndisDescendantsInc);
            m_contextmenuPruneRecordsIndis.MenuItems.Add(m_menuitemPruneRecordsIndisAncestorsExc);
            m_contextmenuPruneRecordsIndis.MenuItems.Add(m_menuitemPruneRecordsIndisAncestorsInc);
            m_contextmenuPruneRecordsIndis.MenuItems.Add(new MenuItem("-"));
            m_contextmenuPruneRecordsIndis.MenuItems.Add(new MenuItem("&Include everyone", pruneIndividualsContextMenuInclude_Click));
            m_contextmenuPruneRecordsIndis.MenuItems.Add(new MenuItem("&Exclude everyone", pruneIndividualsContextMenuExclude_Click));
            m_contextmenuPruneRecordsIndis.MenuItems.Add(new MenuItem("Exclude everyone still a&live (and those born in last 100 years)", pruneIndividualsContextMenuAlive_Click));
            m_contextmenuPruneRecordsIndis.MenuItems.Add(new MenuItem("-"));
            m_contextmenuPruneRecordsIndis.MenuItems.Add(m_menuitemPruneRecordsIndisUnconnected);
            m_contextmenuPruneRecordsIndis.Popup += pruneIndividualsContextMenu_popup;

            //
            // m_pruneSourcesContextMenu
            //
            m_menuitemPruneRecordsSourcesDetails = new MenuItem("&Details and pictures...", pruneSourcesContextMenuDetails_Click);
            m_contextmenuPruneRecordsSources.MenuItems.Add(m_menuitemPruneRecordsSourcesDetails);
            m_contextmenuPruneRecordsSources.MenuItems.Add(new MenuItem("-"));
            m_contextmenuPruneRecordsSources.MenuItems.Add(new MenuItem("&Include all sources", pruneSourcesContextMenuInclude_Click));
            m_contextmenuPruneRecordsSources.MenuItems.Add(new MenuItem("&Exclude all sources", pruneSourcesContextMenuExclude_Click));
            m_contextmenuPruneRecordsSources.MenuItems.Add(new MenuItem("-"));
            m_menuitemPruneRecordsSourcesRemovePics = new MenuItem("&Remove pictures from selected sources", pruneSourcesContextMenuRemovePics_Click);
            m_contextmenuPruneRecordsSources.MenuItems.Add(m_menuitemPruneRecordsSourcesRemovePics);
            m_contextmenuPruneRecordsSources.Popup += pruneSourcesContextMenu_popup;

            // 
            // panel3ListView
            //          
            m_tabcontrolPruneRecords.Location = new Point(108, 65);
            m_tabcontrolPruneRecords.Name = "m_tabcontrolPruneRecords";
            m_tabcontrolPruneRecords.Size = new Size(388, 207);
            m_tabcontrolPruneRecords.TabIndex = 4;
            m_tabpagePruneRecordsIndis = new TabPage("Individuals");
            m_tabcontrolPruneRecords.TabPages.Add(m_tabpagePruneRecordsIndis);
            m_tabpagePruneRecordsSources = new TabPage("Sources");
            m_tabcontrolPruneRecords.TabPages.Add(m_tabpagePruneRecordsSources);

            // 
            // panel3ListView
            // 
            m_listviewPruneRecordsIndis.CheckBoxes = true;
            m_listviewPruneRecordsIndis.Location = new Point(0, 0);
            m_listviewPruneRecordsIndis.Name = "m_listviewPruneRecordsIndis";
            m_listviewPruneRecordsIndis.Size = new Size(381, 181);
            m_listviewPruneRecordsIndis.TabIndex = 4;
            m_listviewPruneRecordsIndis.View = View.Details;
            m_listviewPruneRecordsIndis.ColumnClick += m_listviewPruneRecordsIndis.ColumnClickHandler;
            m_listviewPruneRecordsIndis.ItemCheck += listviewPruneRecordsIndis_ItemCheck;
            m_listviewPruneRecordsIndis.ContextMenu = m_contextmenuPruneRecordsIndis;
            m_listviewPruneRecordsIndis.FullRowSelect = true;
            m_listviewPruneRecordsIndis.GridLines = true;
            m_listviewPruneRecordsIndis.AllowColumnReorder = true;
            m_tabpagePruneRecordsIndis.Controls.Add(m_listviewPruneRecordsIndis);

            // 
            // panel3ListView2
            // 
            m_listviewPruneRecordsSources.CheckBoxes = true;
            m_listviewPruneRecordsSources.Location = new Point(0, 0);
            m_listviewPruneRecordsSources.Name = "m_listviewPruneRecordsSources";
            m_listviewPruneRecordsSources.Size = new Size(381, 181);
            m_listviewPruneRecordsSources.TabIndex = 4;
            m_listviewPruneRecordsSources.View = View.Details;
            m_listviewPruneRecordsSources.ColumnClick += m_listviewPruneRecordsSources.ColumnClickHandler;
            m_listviewPruneRecordsSources.ItemCheck += listviewPruneRecordsSources_ItemCheck;
            m_listviewPruneRecordsSources.ContextMenu = m_contextmenuPruneRecordsSources;
            m_listviewPruneRecordsSources.FullRowSelect = true;
            m_listviewPruneRecordsSources.GridLines = true;
            m_listviewPruneRecordsSources.AllowColumnReorder = true;
            m_tabpagePruneRecordsSources.Controls.Add(m_listviewPruneRecordsSources);

            // 
            // panel3InstructionLabel
            // 
            m_labelPruneRecordsInstructions.Location = new Point(8, 16);
            m_labelPruneRecordsInstructions.Name = "m_labelPruneRecordsInstructions";
            m_labelPruneRecordsInstructions.Size = new Size(488, 45);
            m_labelPruneRecordsInstructions.TabIndex = 3;
            m_labelPruneRecordsInstructions.Text = "Now, you can specify any individuals and sources you don\'t want to appear in the website. " +
                "Clear the box next to their name to prevent them from appearing - those left ticked " +
                "will appear.";

            // 
            // panel3ButtonsLabel
            // 
            m_labelPruneRecordsButtons.Location = new Point(8, 70);
            m_labelPruneRecordsButtons.Name = "m_labelPruneRecordsButtons";
            m_labelPruneRecordsButtons.Size = new Size(92, 95);
            m_labelPruneRecordsButtons.TabIndex = 9;
            m_labelPruneRecordsButtons.Text = "Right-click on the list for more options, including adding pictures...";

            // 
            // panel3LoadButton
            // 
            m_buttonPruneRecordsLoad.Location = new Point(7, 166);
            m_buttonPruneRecordsLoad.Name = "m_buttonPruneRecordsLoad";
            m_buttonPruneRecordsLoad.TabIndex = 10;
            m_buttonPruneRecordsLoad.Text = "&Load changes...";
            m_buttonPruneRecordsLoad.Click += buttonPruneRecordsLoad_click;
            m_buttonPruneRecordsLoad.Size = new Size(96, 40);

            // 
            // panel3SaveButton
            // 
            m_buttonPruneRecordsSave.Location = new Point(7, 210);
            m_buttonPruneRecordsSave.Name = "m_buttonPruneRecordsSave";
            m_buttonPruneRecordsSave.TabIndex = 11;
            m_buttonPruneRecordsSave.Text = "S&ave changes...";
            m_buttonPruneRecordsSave.Click += buttonPruneRecordsSave_click;
            m_buttonPruneRecordsSave.Size = new Size(96, 40);
        }

        // Builds the GUI for the panel where the user selects which individuals to include on the front page.
        private void InitialiseKeyIndividualsPanel()
        {
            // 
            // Key Indidivuals panel
            // 
            m_panelSelectKey.Controls.Add(m_labelSelectKey);
            m_panelSelectKey.Controls.Add(m_textboxSelectKey);
            m_panelSelectKey.Controls.Add(m_labelSelectKeyIndividuals);
            m_panelSelectKey.Controls.Add(m_listboxSelectKey);
            m_panelSelectKey.Controls.Add(m_buttonSelectKeyAdd);
            m_panelSelectKey.Controls.Add(m_buttonSelectKeyDelete);
            m_panelSelectKey.Controls.Add(m_labelSelectKeyInstructions);
            m_panelSelectKey.Location = new Point(216, 0);
            m_panelSelectKey.Name = "m_panelSelectKey";
            m_panelSelectKey.Size = new Size(280, 272);
            m_panelSelectKey.TabIndex = 12;

            // 
            // Key Indidivuals EditLabel1
            // 
            m_labelSelectKey.Location = new Point(0, 120);
            m_labelSelectKey.Name = "panel4EditLabel1";
            m_labelSelectKey.Size = new Size(184, 24);
            m_labelSelectKey.TabIndex = 2;
            m_labelSelectKey.Text = "&Website Title:";
            m_labelSelectKey.TextAlign = ContentAlignment.BottomLeft;

            // 
            // Key Indidivuals EditBox1
            // 
            m_textboxSelectKey.Location = new Point(0, 144);
            m_textboxSelectKey.Name = "panel4EditBox1";
            m_textboxSelectKey.Size = new Size(274, 20);
            m_textboxSelectKey.TabIndex = 1;
            m_textboxSelectKey.Text = "";

            // 
            // Key Indidivuals Label
            // 
            m_labelSelectKeyIndividuals.Location = new Point(0, 182);
            m_labelSelectKeyIndividuals.Name = "panel4KeyIndividualsLabel";
            m_labelSelectKeyIndividuals.Size = new Size(184, 24);
            m_labelSelectKeyIndividuals.TabIndex = 3;
            m_labelSelectKeyIndividuals.Text = "&Key Individuals:";
            m_labelSelectKeyIndividuals.TextAlign = ContentAlignment.BottomLeft;

            // 
            // Key Indidivuals ListBox
            // 
            m_listboxSelectKey.Location = new Point(0, 206);
            m_listboxSelectKey.Name = "panel4KeyIndividualsListBox";
            m_listboxSelectKey.Size = new Size(192, 68);
            m_listboxSelectKey.TabIndex = 4;
            m_listboxSelectKey.Text = "";
            m_listboxSelectKey.SelectedValueChanged += listboxSelectKey_selectedValueChanged;

            // 
            // Key Indidivuals Add Button
            // 
            m_buttonSelectKeyAdd.Location = new Point(200, 206);
            m_buttonSelectKeyAdd.Name = "panel4KeyIndividualsAddButton";
            m_buttonSelectKeyAdd.TabIndex = 6;
            m_buttonSelectKeyAdd.Text = "&Add...";
            m_buttonSelectKeyAdd.Click += buttonSelectKeyAdd_click;

            // 
            // Key Indidivuals Delete Button
            // 
            m_buttonSelectKeyDelete.Location = new Point(200, 236);
            m_buttonSelectKeyDelete.Name = "panel4KeyIndividualsDeleteButton";
            m_buttonSelectKeyDelete.TabIndex = 7;
            m_buttonSelectKeyDelete.Text = "&Remove";
            m_buttonSelectKeyDelete.Click += buttonSelectKeyDelete_click;

            // 
            // Key Indidivuals Instruction Label
            // 
            m_labelSelectKeyInstructions.Location = new Point(0, 16);
            m_labelSelectKeyInstructions.Name = "panel4InstructionLabel";
            m_labelSelectKeyInstructions.Size = new Size(288, 96);
            m_labelSelectKeyInstructions.TabIndex = 0;
            m_labelSelectKeyInstructions.Text = "Next, you can choose a title for the front page of your website. " +
                "Leave it blank if you don\'t want a title.";
            m_labelSelectKeyInstructions.Text += "\r\n\r\nYou can also select which people feature as key individuals on the front page.";
        }

        // Builds the GUI for the panel where the user chooses the output directory
        private void InitialiseChooseOutputPanel()
        {
            // 
            // Choose Output panel
            // 
            m_panelChooseOutput.Controls.Add(m_textboxChooseOutput);
            m_panelChooseOutput.Controls.Add(m_labelChooseOutputInstructions);
            m_panelChooseOutput.Controls.Add(m_labelChooseOutput);
            m_panelChooseOutput.Controls.Add(m_buttonChooseOutputBrowse);
            m_panelChooseOutput.Controls.Add(m_labelChooseOutputContinue);
            m_panelChooseOutput.Location = new Point(216, 0);
            m_panelChooseOutput.Name = "m_panelChooseOutput";
            m_panelChooseOutput.Size = new Size(280, 272);
            m_panelChooseOutput.TabIndex = 11;

            // 
            // Choose Output EditBox
            // 
            m_textboxChooseOutput.Location = new Point(0, 120);
            m_textboxChooseOutput.Name = "m_textboxChooseOutput";
            m_textboxChooseOutput.Size = new Size(192, 20);
            m_textboxChooseOutput.TabIndex = 4;
            m_textboxChooseOutput.Text = "";
            m_textboxChooseOutput.TextChanged += textboxChooseOutput_textChanged;

            // 
            // m_labelChooseOutput
            // 
            m_labelChooseOutput.Location = new Point(0, 96);
            m_labelChooseOutput.Name = "m_labelChooseOutput";
            m_labelChooseOutput.RightToLeft = RightToLeft.No;
            m_labelChooseOutput.Size = new Size(152, 24);
            m_labelChooseOutput.TabIndex = 5;
            m_labelChooseOutput.Text = "&Folder:";
            m_labelChooseOutput.TextAlign = ContentAlignment.BottomLeft;

            // 
            // m_buttonChooseOutputBrowse
            // 
            m_buttonChooseOutputBrowse.Location = new Point(200, 120);
            m_buttonChooseOutputBrowse.Name = "m_buttonChooseOutputBrowse";
            m_buttonChooseOutputBrowse.TabIndex = 6;
            m_buttonChooseOutputBrowse.Text = "B&rowse...";
            m_buttonChooseOutputBrowse.Click += buttonChooseOutputBrowse_click;

            // 
            // m_labelChooseOutputInstructions
            // 
            m_labelChooseOutputInstructions.Location = new Point(0, 16);
            m_labelChooseOutputInstructions.Name = "m_labelChooseOutputInstructions";
            m_labelChooseOutputInstructions.Size = new Size(280, 80);
            m_labelChooseOutputInstructions.TabIndex = 3;
            m_labelChooseOutputInstructions.Text = "Finally, select the folder where you wish to the website files to be created. If " +
                "the folder doesn\'t exist already it will be created for you.";

            // 
            // m_labelChooseOutputContinue
            // 
            m_labelChooseOutputContinue.Location = new Point(256, 288);
            m_labelChooseOutputContinue.Name = "m_labelChooseOutputContinue";
            m_labelChooseOutputContinue.Size = new Size(256, 16);
            m_labelChooseOutputContinue.TabIndex = 15;
            m_labelChooseOutputContinue.Text = "Click Next to create the web pages...";
        }

        // Builds the GUI for the finishing panel
        private void InitialiseAllDonePanel()
        {
            // 
            // m_panelAllDone
            // 
            m_panelAllDone.Controls.Add(m_checkboxAllDoneShowSite);
            m_panelAllDone.Controls.Add(m_linklabelAllDone);
            m_panelAllDone.Controls.Add(m_labelAllDoneThankYou);
            m_panelAllDone.Controls.Add(m_labelAllDoneDirectory);
            m_panelAllDone.Controls.Add(m_labelAllDoneStartFile);
            m_panelAllDone.Location = new Point(216, 0);
            m_panelAllDone.Name = "panel6";
            m_panelAllDone.Size = new Size(280, 272);
            m_panelAllDone.TabIndex = 12;

            //
            // m_checkboxAllDoneShowSite
            // 
            m_checkboxAllDoneShowSite.Location = new Point(0, 250);
            m_checkboxAllDoneShowSite.Name = "panel6WebsiteCheckBox";
            m_checkboxAllDoneShowSite.Size = new Size(288, 24);
            m_checkboxAllDoneShowSite.TabIndex = 7;
            m_checkboxAllDoneShowSite.Text = "&Display web pages after program finishes.";

            // 
            // m_linklabelAllDone
            // 
            m_linklabelAllDone.Location = new Point(0, 52);
            m_linklabelAllDone.Name = "panel6FolderLink";
            m_linklabelAllDone.Size = new Size(288, 48);
            m_linklabelAllDone.TabIndex = 7;
            m_linklabelAllDone.TabStop = true;
            m_linklabelAllDone.Text = "<path>";
            m_linklabelAllDone.TextAlign = ContentAlignment.TopLeft;
            m_linklabelAllDone.LinkClicked += linklabelAllDone_click;

            // 
            // m_labelAllDoneThankYou
            // 
            m_labelAllDoneThankYou.Location = new Point(0, 230);
            m_labelAllDoneThankYou.Name = "label1";
            m_labelAllDoneThankYou.Size = new Size(288, 24);
            m_labelAllDoneThankYou.TabIndex = 3;
            m_labelAllDoneThankYou.Text = "Thank you for using GEDmill.";
            m_labelAllDoneThankYou.TextAlign = ContentAlignment.TopLeft;

            // 
            // m_labelAllDoneDirectory
            // 
            m_labelAllDoneDirectory.Location = new Point(0, 16);
            m_labelAllDoneDirectory.Name = "label3";
            m_labelAllDoneDirectory.Size = new Size(280, 48);
            m_labelAllDoneDirectory.TabIndex = 0;
            m_labelAllDoneDirectory.Text = "The website files have been generated and put in ";
            m_labelAllDoneDirectory.TextAlign = ContentAlignment.TopLeft;

            // 
            // m_labelAllDoneStartFile
            // 
            m_labelAllDoneStartFile.Location = new Point(0, 104);
            m_labelAllDoneStartFile.Name = "label3a";
            m_labelAllDoneStartFile.Size = new Size(288, 48);
            m_labelAllDoneStartFile.TabIndex = 0;
            m_labelAllDoneStartFile.Text = ""; // Filled in programatically to say "(The front page for the website is the file home.html)"
            m_labelAllDoneStartFile.TextAlign = ContentAlignment.TopLeft;
        }

        // Builds the GUI for the config panel
        private void InitialiseSettingsPanes()
        {
            // Checklist for adding new controls to configPanel: (copy configPanel_Commentary_EditBox)
            // 1. Create a member variable for the control, and one for the label
            // 2. Create a new instance of the control & label
            // 3. Add control to configPanel
            // 4. Initialise in LoadConfigPanelSettings()
            // 5. Save in SaveConfigPanelSettings()

            InitialiseSettingsWebpagesPane();
            InitialiseSettingsImagesPane();
            InitialiseSettingsGedcomPane();
            InitialiseSettingsTreeDiagramsPane();
            InitialiseSettingsAdvancedPane();
        }

        // Builds the GUI for the webpages pane in the config panel
        private void InitialiseSettingsWebpagesPane()
        {
            // 
            // configPanel_Commentary_Label (Webpages)
            // 
            m_labelConfigCommentary.Location = new Point(9, 0);
            m_labelConfigCommentary.Name = "m_labelConfigCommentary";
            m_labelConfigCommentary.RightToLeft = RightToLeft.No;
            m_labelConfigCommentary.Size = new Size(200, 24);
            m_labelConfigCommentary.TabIndex = 1;
            m_labelConfigCommentary.Text = "Commentary for &title page:";
            m_labelConfigCommentary.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_Commentary_EditBox (Webpages)
            // 
            m_textboxConfigCommentary.Location = new Point(9, 26);
            m_textboxConfigCommentary.Name = "m_textboxConfigCommentary";
            m_textboxConfigCommentary.Size = new Size(240, 70);
            m_textboxConfigCommentary.TabIndex = 2;
            m_textboxConfigCommentary.Text = "";
            m_textboxConfigCommentary.Multiline = true;

            // 
            // configPanel_CommentaryIsHtml_Label (Webpages)
            // 
            m_labelConfigCommentaryIsHtml.Location = new Point(9, 91);
            m_labelConfigCommentaryIsHtml.Name = "m_labelConfigCommentaryIsHtml";
            m_labelConfigCommentaryIsHtml.RightToLeft = RightToLeft.No;
            m_labelConfigCommentaryIsHtml.Size = new Size(8, 24);
            m_labelConfigCommentaryIsHtml.TabIndex = 3;
            m_labelConfigCommentaryIsHtml.Text = "(";
            m_labelConfigCommentaryIsHtml.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_CommentaryIsHtml_CheckBox (Webpages)
            // 
            m_checkboxConfigCommentaryIsHtml.Location = new Point(19, 96);
            m_checkboxConfigCommentaryIsHtml.Name = "m_checkboxConfigCommentaryIsHtml";
            m_checkboxConfigCommentaryIsHtml.Size = new Size(190, 24);
            m_checkboxConfigCommentaryIsHtml.TabIndex = 4;
            m_checkboxConfigCommentaryIsHtml.Text = "the a&bove text is HTML)";

            // 
            // configPanel_UserLink_Label (Webpages)
            // 
            m_labelConfigUserLink.Location = new Point(9, 121);
            m_labelConfigUserLink.Name = "m_labelConfigUserLink";
            m_labelConfigUserLink.RightToLeft = RightToLeft.No;
            m_labelConfigUserLink.Size = new Size(260, 24);
            m_labelConfigUserLink.TabIndex = 5;
            m_labelConfigUserLink.Text = "&Link to your website: (with http:// prefix)";
            m_labelConfigUserLink.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_UserLink_EditBox (Webpages)
            // 
            m_textboxConfigUserLink.Location = new Point(9, 147);
            m_textboxConfigUserLink.Name = "m_textboxConfigUserLink";
            m_textboxConfigUserLink.Size = new Size(240, 20);
            m_textboxConfigUserLink.TabIndex = 7;
            m_textboxConfigUserLink.Text = "";
            m_textboxConfigUserLink.Multiline = false;

            // 
            // configPanel_CustomFooter_Label (Webpages)
            // 
            m_labelConfigCustomFooter.Location = new Point(9, 172);
            m_labelConfigCustomFooter.Name = "m_labelConfigCustomFooter";
            m_labelConfigCustomFooter.RightToLeft = RightToLeft.No;
            m_labelConfigCustomFooter.Size = new Size(224, 24);
            m_labelConfigCustomFooter.TabIndex = 8;
            m_labelConfigCustomFooter.Text = "Te&xt for page footer:";
            m_labelConfigCustomFooter.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_CustomFooter_EditBox (Webpages)
            //
            m_textboxConfigCustomFooter.Location = new Point(9, 198);
            m_textboxConfigCustomFooter.Name = "m_textboxConfigCustomFooter";
            m_textboxConfigCustomFooter.Size = new Size(200, 20);
            m_textboxConfigCustomFooter.Text = "";
            m_textboxConfigCustomFooter.TabIndex = 9;

            // 
            // configPanel_FooterIsHtml_Label (Webpages)
            // 
            m_labelConfigFooterIsHtml.Location = new Point(9, 213);
            m_labelConfigFooterIsHtml.Name = "m_labelConfigFooterIsHtml";
            m_labelConfigFooterIsHtml.RightToLeft = RightToLeft.No;
            m_labelConfigFooterIsHtml.Size = new Size(8, 24);
            m_labelConfigFooterIsHtml.TabIndex = 10;
            m_labelConfigFooterIsHtml.Text = "(";
            m_labelConfigFooterIsHtml.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_FooterIsHtml_CheckBox (Webpages)
            // 
            m_checkboxConfigFooterIsHtml.Location = new Point(19, 218);
            m_checkboxConfigFooterIsHtml.Name = "m_checkboxConfigFooterIsHtml";
            m_checkboxConfigFooterIsHtml.Size = new Size(190, 24);
            m_checkboxConfigFooterIsHtml.TabIndex = 11;
            m_checkboxConfigFooterIsHtml.Text = "the abo&ve text is HTML)";

            //
            // configPanel_Stats_CheckBox (Webpages)
            // 
            m_checkboxConfigStats.Location = new Point(266, 7);
            m_checkboxConfigStats.Name = "m_checkboxConfigStats";
            m_checkboxConfigStats.Size = new Size(200, 20);
            m_checkboxConfigStats.Text = "Include website &statistics";
            m_checkboxConfigStats.TabIndex = 12;

            //
            // configPanel_CDROM_CheckBox (Webpages)
            // 
            m_checkboxConfigCdrom.Location = new Point(266, 30);
            m_checkboxConfigCdrom.Name = "m_checkboxConfigCdrom";
            m_checkboxConfigCdrom.Size = new Size(200, 20);
            m_checkboxConfigCdrom.Text = "Create CD-ROM &auto-run files";
            m_checkboxConfigCdrom.TabIndex = 13;

            //
            // configPanel_MultiPageIndex_CheckBox (Webpages)
            // 
            m_checkboxConfigMultiPageIndex.Location = new Point(266, 53);
            m_checkboxConfigMultiPageIndex.Name = "m_checkboxConfigMultiPageIndex";
            m_checkboxConfigMultiPageIndex.Size = new Size(220, 20);
            m_checkboxConfigMultiPageIndex.Text = "&Multi-page individuals index";
            m_checkboxConfigMultiPageIndex.TabIndex = 14;
            m_checkboxConfigMultiPageIndex.Click += configPanel_MultiPageIndex_CheckBox_click;

            //
            // configPanel_UserRefInIndex_CheckBox (Webpages)
            //
            m_checkboxConfigUserRefInIndex.Location = new Point(266, 76);
            m_checkboxConfigUserRefInIndex.Name = "m_checkboxConfigUserRefInIndex";
            m_checkboxConfigUserRefInIndex.Size = new Size(220, 20);
            m_checkboxConfigUserRefInIndex.Text = "&User Reference numbers in index";
            m_checkboxConfigUserRefInIndex.TabIndex = 15;

            // 
            // configPanel_MultiPageIndexNumber_Label (Webpages)
            // 
            m_labelConfigMultiPageIndexNumber.Location = new Point(266, 96);
            m_labelConfigMultiPageIndexNumber.Name = "m_labelConfigMultiPageIndexNumber";
            m_labelConfigMultiPageIndexNumber.RightToLeft = RightToLeft.No;
            m_labelConfigMultiPageIndexNumber.Size = new Size(170, 24);
            m_labelConfigMultiPageIndexNumber.TabIndex = 16;
            m_labelConfigMultiPageIndexNumber.Text = "&Individuals per index page:";
            m_labelConfigMultiPageIndexNumber.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_MultiPageIndexNumber_TextBox (Webpages)
            // 
            m_textboxConfigMultiPageIndexNumber.Location = new Point(446, 100);
            m_textboxConfigMultiPageIndexNumber.Name = "m_textboxConfigMultiPageIndexNumber";
            m_textboxConfigMultiPageIndexNumber.Size = new Size(45, 20);
            m_textboxConfigMultiPageIndexNumber.TabIndex = 17;
            m_textboxConfigMultiPageIndexNumber.Text = "";

            // 
            // configPanel_IndexName_Label (Webpages)
            // 
            m_labelConfigIndexName.Location = new Point(266, 126);
            m_labelConfigIndexName.Name = "m_labelConfigIndexName";
            m_labelConfigIndexName.RightToLeft = RightToLeft.No;
            m_labelConfigIndexName.Size = new Size(224, 20);
            m_labelConfigIndexName.TabIndex = 18;
            m_labelConfigIndexName.Text = "Name of &front page file:";
            m_labelConfigIndexName.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_IndexName_EditBox (Webpages)
            // 
            m_textboxConfigIndexName.Location = new Point(266, 148);
            m_textboxConfigIndexName.Name = "m_textboxConfigIndexName";
            m_textboxConfigIndexName.Size = new Size(175, 20);
            m_textboxConfigIndexName.TabIndex = 19;
            m_textboxConfigIndexName.Text = "";
            m_textboxConfigIndexName.Multiline = false;

            // 
            // configPanel_IndexName_ExtnLabel (Webpages)
            // 
            m_labelConfigIndexNameExtn.Location = new Point(440, 141);
            m_labelConfigIndexNameExtn.Name = "m_labelConfigIndexNameExtn";
            m_labelConfigIndexNameExtn.RightToLeft = RightToLeft.No;
            m_labelConfigIndexNameExtn.Size = new Size(60, 24);
            m_labelConfigIndexNameExtn.TabIndex = 20;
            m_labelConfigIndexNameExtn.Text = ""; //Filled programatically
            m_labelConfigIndexNameExtn.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_PreserveFrontPage_CheckBox (Webpages)
            // 
            m_checkboxConfigPreserveFrontPage.Location = new Point(266, 170);
            m_checkboxConfigPreserveFrontPage.Name = "m_checkboxConfigPreserveFrontPage";
            m_checkboxConfigPreserveFrontPage.Size = new Size(250, 20);
            m_checkboxConfigPreserveFrontPage.Text = "&Do not generate new front page";
            m_checkboxConfigPreserveFrontPage.TabIndex = 21;

            // 
            // configPanel_Email_Label (Webpages)
            // 
            m_labelConfigEmail.Location = new Point(266, 190);
            m_labelConfigEmail.Name = "m_labelConfigEmail";
            m_labelConfigEmail.RightToLeft = RightToLeft.No;
            m_labelConfigEmail.Size = new Size(220, 24);
            m_labelConfigEmail.TabIndex = 22;
            m_labelConfigEmail.Text = "&Email address to put on front page:";
            m_labelConfigEmail.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_Email_EditBox (Webpages)
            // 
            m_textboxConfigEmail.Location = new Point(266, 216);
            m_textboxConfigEmail.Name = "m_textboxConfigEmail";
            m_textboxConfigEmail.Size = new Size(220, 20);
            m_textboxConfigEmail.TabIndex = 23;
            m_textboxConfigEmail.Text = "";
            m_textboxConfigEmail.Multiline = false;
        }

        // Builds the GUI for the images pane in the config panel
        private void InitialiseSettingsImagesPane()
        {
            // 
            // configPanel_BackImage_EditLabel (Images)
            // 
            m_labelConfigBackImageEdit.Location = new Point(9, 0);
            m_labelConfigBackImageEdit.Name = "m_labelConfigBackImageEdit";
            m_labelConfigBackImageEdit.RightToLeft = RightToLeft.No;
            m_labelConfigBackImageEdit.Size = new Size(156, 24);
            m_labelConfigBackImageEdit.TabIndex = 1;
            m_labelConfigBackImageEdit.Text = "&Background image:";
            m_labelConfigBackImageEdit.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_BackImage_EditBox (Images)
            // 
            m_textboxConfigBackImageEdit.Location = new Point(9, 26);
            m_textboxConfigBackImageEdit.Name = "m_textboxConfigBackImageEdit";
            m_textboxConfigBackImageEdit.Size = new Size(191, 20);
            m_textboxConfigBackImageEdit.TabIndex = 2;
            m_textboxConfigBackImageEdit.Text = "";

            // 
            // configPanel_BackImage_BrowseButton (Images)
            // 
            m_buttonConfigBackImageBrowse.Location = new Point(208, 25);
            m_buttonConfigBackImageBrowse.Name = "m_buttonConfigBackImageBrowse";
            m_buttonConfigBackImageBrowse.TabIndex = 3;
            m_buttonConfigBackImageBrowse.Text = "B&rowse...";
            m_buttonConfigBackImageBrowse.Click += configPanel_BackImage_BrowseButton_click;

            // 
            // configPanel_FrontImage_EditLabel (Images)
            // 
            m_labelConfigFrontImageEdit.Location = new Point(9, 46);
            m_labelConfigFrontImageEdit.Name = "m_labelConfigFrontImageEdit";
            m_labelConfigFrontImageEdit.RightToLeft = RightToLeft.No;
            m_labelConfigFrontImageEdit.Size = new Size(156, 20);
            m_labelConfigFrontImageEdit.TabIndex = 4;
            m_labelConfigFrontImageEdit.Text = "&Picture on front page:";
            m_labelConfigFrontImageEdit.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_FrontImage_EditBox (Images)
            // 
            m_textboxConfigFrontImageEdit.Location = new Point(9, 68);
            m_textboxConfigFrontImageEdit.Name = "m_textboxConfigFrontImageEdit";
            m_textboxConfigFrontImageEdit.Size = new Size(191, 20);
            m_textboxConfigFrontImageEdit.TabIndex = 5;
            m_textboxConfigFrontImageEdit.Text = "";

            // 
            // configPanel_FrontImage_BrowseButton (Images)
            // 
            m_buttonConfigFrontImageBrowse.Location = new Point(208, 68);
            m_buttonConfigFrontImageBrowse.Name = "m_buttonConfigFrontImageBrowse";
            m_buttonConfigFrontImageBrowse.TabIndex = 6;
            m_buttonConfigFrontImageBrowse.Text = "Br&owse...";
            m_buttonConfigFrontImageBrowse.Click += configPanel_FrontImage_BrowseButton_click;

            // 
            // configPanel_IndiImageSize_Label (Images)
            // 
            m_labelConfigIndiImageSize.Location = new Point(9, 108);
            m_labelConfigIndiImageSize.Name = "m_labelConfigIndiImageSize";
            m_labelConfigIndiImageSize.RightToLeft = RightToLeft.No;
            m_labelConfigIndiImageSize.Size = new Size(256, 24);
            m_labelConfigIndiImageSize.TabIndex = 7;
            m_labelConfigIndiImageSize.Text = "Maximum size of individual images";
            m_labelConfigIndiImageSize.TextAlign = ContentAlignment.BottomLeft;

            // 
            // configPanel_IndiImageWidth_Label (Images)
            // 
            m_labelConfigIndiImageWidth.Location = new Point(9, 138);
            m_labelConfigIndiImageWidth.Name = "m_labelConfigIndiImageWidth";
            m_labelConfigIndiImageWidth.RightToLeft = RightToLeft.No;
            m_labelConfigIndiImageWidth.Size = new Size(50, 24);
            m_labelConfigIndiImageWidth.TabIndex = 8;
            m_labelConfigIndiImageWidth.Text = "&Width:";
            m_labelConfigIndiImageWidth.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_IndiImageWidth_EditBox (Images)
            // 
            m_textboxConfigIndiImageWidth.Location = new Point(61, 138);
            m_textboxConfigIndiImageWidth.Name = "m_textboxConfigIndiImageWidth";
            m_textboxConfigIndiImageWidth.Size = new Size(34, 20);
            m_textboxConfigIndiImageWidth.TabIndex = 9;
            m_textboxConfigIndiImageWidth.Text = "";

            // 
            // configPanel_IndiImageHeight_Label (Images)
            // 
            m_labelConfigIndiImageHeight.Location = new Point(109, 138);
            m_labelConfigIndiImageHeight.Name = "m_labelConfigIndiImageHeight";
            m_labelConfigIndiImageHeight.RightToLeft = RightToLeft.No;
            m_labelConfigIndiImageHeight.Size = new Size(50, 24);
            m_labelConfigIndiImageHeight.TabIndex = 10;
            m_labelConfigIndiImageHeight.Text = "&Height:";
            m_labelConfigIndiImageHeight.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_IndiImageHeight_EditBox (Images)
            // 
            m_textboxConfigIndiImageHeight.Location = new Point(162, 138);
            m_textboxConfigIndiImageHeight.Name = "m_textboxConfigIndiImageHeight";
            m_textboxConfigIndiImageHeight.Size = new Size(34, 20);
            m_textboxConfigIndiImageHeight.TabIndex = 11;
            m_textboxConfigIndiImageHeight.Text = "";

            // 
            // configPanel_SourceImageSize_Label (Images)
            // 
            m_labelConfigSourceImageSize.Location = new Point(9, 167);
            m_labelConfigSourceImageSize.Name = "m_labelConfigSourceImageSize";
            m_labelConfigSourceImageSize.RightToLeft = RightToLeft.No;
            m_labelConfigSourceImageSize.Size = new Size(256, 24);
            m_labelConfigSourceImageSize.TabIndex = 12;
            m_labelConfigSourceImageSize.Text = "Maximum size of source images";
            m_labelConfigSourceImageSize.TextAlign = ContentAlignment.BottomLeft;

            // 
            // configPanel_SourceImageWidth_Label (Images)
            // 
            m_labelConfigSourceImageWidth.Location = new Point(9, 193);
            m_labelConfigSourceImageWidth.Name = "m_labelConfigSourceImageWidth";
            m_labelConfigSourceImageWidth.RightToLeft = RightToLeft.No;
            m_labelConfigSourceImageWidth.Size = new Size(50, 24);
            m_labelConfigSourceImageWidth.TabIndex = 13;
            m_labelConfigSourceImageWidth.Text = "W&idth:";
            m_labelConfigSourceImageWidth.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_SourceImageWidth_EditBox (Images)
            // 
            m_textboxConfigSourceImageWidth.Location = new Point(60, 197);
            m_textboxConfigSourceImageWidth.Name = "m_textboxConfigSourceImageWidth";
            m_textboxConfigSourceImageWidth.Size = new Size(34, 20);
            m_textboxConfigSourceImageWidth.TabIndex = 14;
            m_textboxConfigSourceImageWidth.Text = "";

            // 
            // configPanel_SourceImageHeight_Label (Images)
            // 
            m_labelConfigSourceImageHeight.Location = new Point(109, 193);
            m_labelConfigSourceImageHeight.Name = "m_labelConfigSourceImageHeight";
            m_labelConfigSourceImageHeight.RightToLeft = RightToLeft.No;
            m_labelConfigSourceImageHeight.Size = new Size(50, 24);
            m_labelConfigSourceImageHeight.TabIndex = 15;
            m_labelConfigSourceImageHeight.Text = "H&eight:";
            m_labelConfigSourceImageHeight.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_SourceImageHeight_EditBox (Images)
            // 
            m_textboxConfigSourceImageHeight.Location = new Point(162, 197);
            m_textboxConfigSourceImageHeight.Name = "m_textboxConfigSourceImageHeight";
            m_textboxConfigSourceImageHeight.Size = new Size(34, 20);
            m_textboxConfigSourceImageHeight.TabIndex = 16;
            m_textboxConfigSourceImageHeight.Text = "";

            //
            // configPanel_AllowMultimedia_CheckBox (Images)
            // 
            m_checkboxConfigAllowMultimedia.Location = new Point(300, 8);
            m_checkboxConfigAllowMultimedia.Name = "m_checkboxConfigAllowMultimedia";
            m_checkboxConfigAllowMultimedia.Size = new Size(190, 24);
            m_checkboxConfigAllowMultimedia.TabIndex = 5;
            m_checkboxConfigAllowMultimedia.Text = "&Allow images etc.";
            m_checkboxConfigAllowMultimedia.Click += configPanel_AllowMultimedia_CheckBox_click;

            //
            // configPanel_RenameOriginals_CheckBox (Images)
            // 
            m_checkboxConfigRenameOriginals.Location = new Point(300, 38);
            m_checkboxConfigRenameOriginals.Name = "m_checkboxConfigRenameOriginals";
            m_checkboxConfigRenameOriginals.Size = new Size(200, 30);
            m_checkboxConfigRenameOriginals.Text = "Re&name files";
            m_checkboxConfigRenameOriginals.TabIndex = 17;

            //
            // configPanel_KeepOriginals_CheckBox (Images)
            // 
            m_checkboxConfigKeepOriginals.Location = new Point(300, 64);
            m_checkboxConfigKeepOriginals.Name = "m_checkboxConfigKeepOriginals";
            m_checkboxConfigKeepOriginals.Size = new Size(200, 40);
            m_checkboxConfigKeepOriginals.Text = "In&clude original (full-size) files";
            m_checkboxConfigKeepOriginals.TabIndex = 18;

            //
            // configPanel_NonPictures_CheckBox (Images)
            // 
            m_checkboxConfigNonPictures.Location = new Point(266, 120);
            m_checkboxConfigNonPictures.Name = "m_checkboxConfigNonPictures";
            m_checkboxConfigNonPictures.Size = new Size(200, 20);
            m_checkboxConfigNonPictures.Text = "&Allow files other than pictures";
            m_checkboxConfigNonPictures.TabIndex = 19;

            //
            // configPanel_IndiImages_CheckBox (Images)
            // 
            m_checkboxConfigIndiImages.Location = new Point(266, 147);
            m_checkboxConfigIndiImages.Name = "m_checkboxConfigIndiImages";
            m_checkboxConfigIndiImages.Size = new Size(200, 20);
            m_checkboxConfigIndiImages.Text = "&Multiple individual images";
            m_checkboxConfigIndiImages.TabIndex = 20;
            m_checkboxConfigIndiImages.Click += configPanel_IndiImages_CheckBox_click;

            // 
            // configPanel_ThumbnailImageSize_Label (Images)
            // 
            m_labelConfigThumbnailImageSize.Location = new Point(266, 167);
            m_labelConfigThumbnailImageSize.Name = "m_labelConfigThumbnailImageSize";
            m_labelConfigThumbnailImageSize.RightToLeft = RightToLeft.No;
            m_labelConfigThumbnailImageSize.Size = new Size(256, 24);
            m_labelConfigThumbnailImageSize.TabIndex = 21;
            m_labelConfigThumbnailImageSize.Text = "Maximum size of thumbnail images";
            m_labelConfigThumbnailImageSize.TextAlign = ContentAlignment.BottomLeft;

            // 
            // configPanel_ThumbnailImageWidth_Label (Images)
            // 
            m_labelConfigThumbnailImageWidth.Location = new Point(266, 193);
            m_labelConfigThumbnailImageWidth.Name = "m_labelConfigThumbnailImageWidth";
            m_labelConfigThumbnailImageWidth.RightToLeft = RightToLeft.No;
            m_labelConfigThumbnailImageWidth.Size = new Size(50, 24);
            m_labelConfigThumbnailImageWidth.TabIndex = 22;
            m_labelConfigThumbnailImageWidth.Text = "Wid&th:";
            m_labelConfigThumbnailImageWidth.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_ThumbnailImageWidth_EditBox (Images)
            // 
            m_textboxConfigThumbnailImageWidth.Location = new Point(317, 197);
            m_textboxConfigThumbnailImageWidth.Name = "m_textboxConfigThumbnailImageWidth";
            m_textboxConfigThumbnailImageWidth.Size = new Size(34, 20);
            m_textboxConfigThumbnailImageWidth.TabIndex = 23;
            m_textboxConfigThumbnailImageWidth.Text = "";

            // 
            // configPanel_ThumbnailImageHeight_Label (Images)
            // 
            m_labelConfigThumbnailImageHeight.Location = new Point(366, 193);
            m_labelConfigThumbnailImageHeight.Name = "m_labelConfigThumbnailImageHeight";
            m_labelConfigThumbnailImageHeight.RightToLeft = RightToLeft.No;
            m_labelConfigThumbnailImageHeight.Size = new Size(50, 24);
            m_labelConfigThumbnailImageHeight.TabIndex = 24;
            m_labelConfigThumbnailImageHeight.Text = "Hei&ght:";
            m_labelConfigThumbnailImageHeight.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_ThumbnailImageHeight_EditBox (Images)
            // 
            m_textboxConfigThumbnailImageHeight.Location = new Point(419, 197);
            m_textboxConfigThumbnailImageHeight.Name = "m_textboxConfigThumbnailImageHeight";
            m_textboxConfigThumbnailImageHeight.Size = new Size(34, 20);
            m_textboxConfigThumbnailImageHeight.TabIndex = 25;
            m_textboxConfigThumbnailImageHeight.Text = "";
        }

        // Builds the GUI for the GEDCOM pane in the config panel
        private void InitialiseSettingsGedcomPane()
        {
            // 
            // configPanel_TabSpaces_Label (GEDCOM)
            // 
            m_labelConfigTabSpaces.Location = new Point(6, 0);
            m_labelConfigTabSpaces.Name = "m_labelConfigTabSpaces";
            m_labelConfigTabSpaces.RightToLeft = RightToLeft.No;
            m_labelConfigTabSpaces.Size = new Size(188, 24);
            m_labelConfigTabSpaces.TabIndex = 1;
            m_labelConfigTabSpaces.Text = "&Num spaces to replace tabs:";
            m_labelConfigTabSpaces.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_TabSpaces_EditBox (GEDCOM)
            // 
            m_textboxConfigTabSpaces.Location = new Point(203, 4);
            m_textboxConfigTabSpaces.Name = "m_textboxConfigTabSpaces";
            m_textboxConfigTabSpaces.Size = new Size(31, 20);
            m_textboxConfigTabSpaces.TabIndex = 2;
            m_textboxConfigTabSpaces.Text = "";

            // 
            // configPanel_NoName_Label (GEDCOM)
            // 
            m_labelConfigNoName.Location = new Point(6, 24);
            m_labelConfigNoName.Name = "m_labelConfigNoName";
            m_labelConfigNoName.RightToLeft = RightToLeft.No;
            m_labelConfigNoName.Size = new Size(200, 24);
            m_labelConfigNoName.TabIndex = 3;
            m_labelConfigNoName.Text = "Show &missing names as:";
            m_labelConfigNoName.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_NoName_EditBox (GEDCOM)
            // 
            m_textboxConfigNoName.Location = new Point(6, 48);
            m_textboxConfigNoName.Name = "m_textboxConfigNoName";
            m_textboxConfigNoName.Size = new Size(228, 20);
            m_textboxConfigNoName.TabIndex = 4;
            m_textboxConfigNoName.Text = "";

            //
            // configPanel_ShowWithheldRecords_CheckBox (GEDCOM)
            // 
            m_checkboxConfigShowWithheldRecords.Location = new Point(6, 86);
            m_checkboxConfigShowWithheldRecords.Name = "m_checkboxConfigShowWithheldRecords";
            m_checkboxConfigShowWithheldRecords.Size = new Size(200, 16);
            m_checkboxConfigShowWithheldRecords.TabIndex = 5;
            m_checkboxConfigShowWithheldRecords.Text = "Include &withheld records";
            m_checkboxConfigShowWithheldRecords.Click += configPanel_ShowWithheldRecords_CheckBox_click;

            // 
            // configPanel_WithheldName_GroupBox (GEDCOM)
            // 
            m_groupboxConfigWithheldName.Location = new Point(6, 113);
            m_groupboxConfigWithheldName.Name = "m_groupboxConfigWithheldName";
            m_groupboxConfigWithheldName.Size = new Size(228, 104);
            m_groupboxConfigWithheldName.TabIndex = 6;
            m_groupboxConfigWithheldName.Text = "Label w&ithheld records with:";
            m_groupboxConfigWithheldName.FlatStyle = FlatStyle.System;

            // 
            // configPanel_WithheldName_Label (GEDCOM)
            // 
            m_radiobuttonConfigWithheldNameLabel.Location = new Point(10, 18);
            m_radiobuttonConfigWithheldNameLabel.Name = "m_radiobuttonConfigWithheldNameLabel";
            m_radiobuttonConfigWithheldNameLabel.RightToLeft = RightToLeft.No;
            m_radiobuttonConfigWithheldNameLabel.Size = new Size(180, 20);
            m_radiobuttonConfigWithheldNameLabel.TabIndex = 7;
            m_radiobuttonConfigWithheldNameLabel.Text = "this &text:";
            m_radiobuttonConfigWithheldNameLabel.Click += configPanel_WithheldName_Label_click;

            //
            // configPanel_WithheldName_EditBox (GEDCOM)
            // 
            m_textboxConfigWithheldName.Location = new Point(28, 38);
            m_textboxConfigWithheldName.Name = "m_textboxConfigWithheldName";
            m_textboxConfigWithheldName.Size = new Size(188, 20);
            m_textboxConfigWithheldName.TabIndex = 8;
            m_textboxConfigWithheldName.Text = "";

            // 
            // configPanel_WithheldName_Name (GEDCOM)
            // 
            m_radiobuttonConfigWithheldNameName.Location = new Point(10, 72);
            m_radiobuttonConfigWithheldNameName.Name = "m_radiobuttonConfigWithheldNameName";
            m_radiobuttonConfigWithheldNameName.RightToLeft = RightToLeft.No;
            m_radiobuttonConfigWithheldNameName.Size = new Size(180, 20);
            m_radiobuttonConfigWithheldNameName.TabIndex = 9;
            m_radiobuttonConfigWithheldNameName.Text = "the individual's n&ame";
            m_radiobuttonConfigWithheldNameName.Click += configPanel_WithheldName_Label_click;

            //
            // configPanel_CapNames_CheckBox (GEDCOM)
            // 
            m_checkboxConfigCapNames.Location = new Point(266, 7);
            m_checkboxConfigCapNames.Name = "m_checkboxConfigCapNames";
            m_checkboxConfigCapNames.Size = new Size(200, 20);
            m_checkboxConfigCapNames.TabIndex = 10;
            m_checkboxConfigCapNames.Text = "&Put surnames in CAPITALS";

            //
            // configPanel_CapEvents_CheckBox (GEDCOM)
            // 
            m_checkboxConfigCapEvents.Location = new Point(266, 34);
            m_checkboxConfigCapEvents.Name = "m_checkboxConfigCapEvents";
            m_checkboxConfigCapEvents.Size = new Size(260, 20);
            m_checkboxConfigCapEvents.TabIndex = 11;
            m_checkboxConfigCapEvents.Text = "&Start events with a capital letter";

            //
            // configPanel_HideEmails_CheckBox (GEDCOM)
            // 
            m_checkboxConfigHideEmails.Location = new Point(266, 60);
            m_checkboxConfigHideEmails.Name = "m_checkboxConfigHideEmails";
            m_checkboxConfigHideEmails.Size = new Size(260, 20);
            m_checkboxConfigHideEmails.TabIndex = 12;
            m_checkboxConfigHideEmails.Text = "Don't show &email addresses";

            //
            // configPanel_OccupationHeadline_CheckBox (GEDCOM)
            // 
            m_checkboxConfigOccupationHeadline.Location = new Point(266, 86);
            m_checkboxConfigOccupationHeadline.Name = "m_checkboxConfigOccupationHeadline";
            m_checkboxConfigOccupationHeadline.Size = new Size(260, 20);
            m_checkboxConfigOccupationHeadline.TabIndex = 13;
            m_checkboxConfigOccupationHeadline.Text = "Show occupation in pa&ge heading";

            //
            // configPanel_AllowTrailingSpaces_CheckBox (GEDCOM)
            // 
            m_checkboxConfigAllowTrailingSpaces.Location = new Point(266, 110);
            m_checkboxConfigAllowTrailingSpaces.Name = "m_checkboxConfigAllowTrailingSpaces";
            m_checkboxConfigAllowTrailingSpaces.Size = new Size(260, 20);
            m_checkboxConfigAllowTrailingSpaces.TabIndex = 14;
            m_checkboxConfigAllowTrailingSpaces.Text = "Preserve t&railing spaces in GEDCOM";
        }

        // Builds the GUI for the Tree Diagrams pane in the config panel
        private void InitialiseSettingsTreeDiagramsPane()
        {
            //
            // configPanel_TreeDiagrams_CheckBox (Tree Diagrams)
            // 
            m_checkboxConfigTreeDiagrams.Location = new Point(8, 8);
            m_checkboxConfigTreeDiagrams.Name = "m_checkboxConfigTreeDiagrams";
            m_checkboxConfigTreeDiagrams.Size = new Size(200, 20);
            m_checkboxConfigTreeDiagrams.TabIndex = 2;
            m_checkboxConfigTreeDiagrams.Text = "Include &tree diagrams";
            m_checkboxConfigTreeDiagrams.Click += configPanel_TreeDiagrams_CheckBox_click;

            // 
            // configPanel_TreeDiagramsFormat_Label (Tree Diagrams)
            // 
            m_labelConfigTreeDiagramsFormat.Location = new Point(22, 25);
            m_labelConfigTreeDiagramsFormat.Name = "m_labelConfigTreeDiagramsFormat";
            m_labelConfigTreeDiagramsFormat.RightToLeft = RightToLeft.No;
            m_labelConfigTreeDiagramsFormat.Size = new Size(134, 24);
            m_labelConfigTreeDiagramsFormat.TabIndex = 3;
            m_labelConfigTreeDiagramsFormat.Text = "&File format:";
            m_labelConfigTreeDiagramsFormat.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_TreeDiagramsFormat_ComboBox (Tree Diagrams)
            // 
            m_comboboxConfigTreeDiagramsFormat.Location = new Point(158, 30);
            m_comboboxConfigTreeDiagramsFormat.Name = "m_comboboxConfigTreeDiagramsFormat";
            m_comboboxConfigTreeDiagramsFormat.Size = new Size(85, 20);
            m_comboboxConfigTreeDiagramsFormat.TabIndex = 4;
            m_comboboxConfigTreeDiagramsFormat.DropDownWidth = 40;
            m_comboboxConfigTreeDiagramsFormat.DropDownStyle = ComboBoxStyle.DropDownList;

            //
            // configPanel_TreeDiagramsFakeBG_CheckBox (Tree Diagrams)
            // 
            m_checkboxConfigTreeDiagramsFakeBg.Location = new Point(8, 66);
            m_checkboxConfigTreeDiagramsFakeBg.Name = "m_checkboxConfigTreeDiagramsFakeBg";
            m_checkboxConfigTreeDiagramsFakeBg.Size = new Size(200, 20);
            m_checkboxConfigTreeDiagramsFakeBg.TabIndex = 5;
            m_checkboxConfigTreeDiagramsFakeBg.Text = "&Simulate transparency";

            //
            // configPanel_ConserveTreeWidth_CheckBox (Tree Diagrams)
            // 
            m_checkboxConfigConserveTreeWidth.Location = new Point(8, 90);
            m_checkboxConfigConserveTreeWidth.Name = "m_checkboxConfigConserveTreeWidth";
            m_checkboxConfigConserveTreeWidth.Size = new Size(190, 24);
            m_checkboxConfigConserveTreeWidth.TabIndex = 6;
            m_checkboxConfigConserveTreeWidth.Text = "Conserve tree &width";

            //
            // configPanel_KeepSiblingOrder_CheckBox (Tree Diagrams)
            // 
            m_checkboxConfigKeepSiblingOrder.Location = new Point(8, 114);
            m_checkboxConfigKeepSiblingOrder.Name = "m_checkboxConfigKeepSiblingOrder";
            m_checkboxConfigKeepSiblingOrder.Size = new Size(230, 24);
            m_checkboxConfigKeepSiblingOrder.TabIndex = 7;
            m_checkboxConfigKeepSiblingOrder.Text = "Keep s&ibling order from GEDCOM";

            //
            // configPanel_MiniTreeColours_GroupBox (Tree Diagrams)
            // 
            m_groupboxMiniTreeColours.Location = new Point(260, 11);
            m_groupboxMiniTreeColours.Name = "m_groupboxMiniTreeColours";
            m_groupboxMiniTreeColours.Size = new Size(230, 224);
            m_groupboxMiniTreeColours.TabIndex = 8;
            m_groupboxMiniTreeColours.Text = "Colours";
            m_groupboxMiniTreeColours.FlatStyle = FlatStyle.System;

            //
            // configPanel_MiniTreeColourIndiHighlight_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourIndiHighlight.Location = new Point(12, 24);
            m_buttonConfigMiniTreeColourIndiHighlight.Name = "m_buttonConfigMiniTreeColourIndiHighlight";
            m_buttonConfigMiniTreeColourIndiHighlight.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourIndiHighlight.TabIndex = 9;
            m_buttonConfigMiniTreeColourIndiHighlight.Text = "Selected &box";
            m_buttonConfigMiniTreeColourIndiHighlight.Click += configPanel_MiniTreeColourIndiHighlight_Button_click;

            //
            // configPanel_MiniTreeColourIndiText_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourIndiText.Location = new Point(122, 24);
            m_buttonConfigMiniTreeColourIndiText.Name = "m_buttonConfigMiniTreeColourIndiText";
            m_buttonConfigMiniTreeColourIndiText.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourIndiText.TabIndex = 10;
            m_buttonConfigMiniTreeColourIndiText.Text = "Selected te&xt";
            m_buttonConfigMiniTreeColourIndiText.Click += configPanel_MiniTreeColourIndiText_Button_click;

            //
            // configPanel_MiniTreeColourIndiBackground_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourIndiBackground.Location = new Point(12, 60);
            m_buttonConfigMiniTreeColourIndiBackground.Name = "m_buttonConfigMiniTreeColourIndiBackground";
            m_buttonConfigMiniTreeColourIndiBackground.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourIndiBackground.TabIndex = 11;
            m_buttonConfigMiniTreeColourIndiBackground.Text = "&General box";
            m_buttonConfigMiniTreeColourIndiBackground.BackColor = Color.FromArgb(255, 0, 0);
            m_buttonConfigMiniTreeColourIndiBackground.Click += configPanel_MiniTreeColourIndiBackground_Button_click;

            //
            // configPanel_MiniTreeColourIndiLink_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourIndiLink.Location = new Point(122, 60);
            m_buttonConfigMiniTreeColourIndiLink.Name = "m_buttonConfigMiniTreeColourIndiLink";
            m_buttonConfigMiniTreeColourIndiLink.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourIndiLink.TabIndex = 12;
            m_buttonConfigMiniTreeColourIndiLink.Text = "&Link text";
            m_buttonConfigMiniTreeColourIndiLink.Click += configPanel_MiniTreeColourIndiLink_Button_click;

            //
            // configPanel_MiniTreeColourIndiBgConcealed_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourIndiBgConcealed.Location = new Point(12, 96);
            m_buttonConfigMiniTreeColourIndiBgConcealed.Name = "m_buttonConfigMiniTreeColourIndiBgConcealed";
            m_buttonConfigMiniTreeColourIndiBgConcealed.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourIndiBgConcealed.TabIndex = 13;
            m_buttonConfigMiniTreeColourIndiBgConcealed.Text = "&Private box";
            m_buttonConfigMiniTreeColourIndiBgConcealed.Click += configPanel_MiniTreeColourIndiBgConcealed_Button_click;

            //
            // configPanel_MiniTreeColourIndiFgConcealed_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourIndiFgConcealed.Location = new Point(122, 96);
            m_buttonConfigMiniTreeColourIndiFgConcealed.Name = "m_buttonConfigMiniTreeColourIndiFgConcealed";
            m_buttonConfigMiniTreeColourIndiFgConcealed.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourIndiFgConcealed.TabIndex = 14;
            m_buttonConfigMiniTreeColourIndiFgConcealed.Text = "P&rivate text";
            m_buttonConfigMiniTreeColourIndiFgConcealed.Click += configPanel_MiniTreeColourIndiFgConcealed_Button_click;

            //
            // configPanel_MiniTreeColourIndiShade_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourIndiShade.Location = new Point(12, 132);
            m_buttonConfigMiniTreeColourIndiShade.Name = "m_buttonConfigMiniTreeColourIndiShade";
            m_buttonConfigMiniTreeColourIndiShade.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourIndiShade.TabIndex = 15;
            m_buttonConfigMiniTreeColourIndiShade.Text = "Spous&e box";
            m_buttonConfigMiniTreeColourIndiShade.Click += configPanel_MiniTreeColourIndiShade_Button_click;

            //
            // configPanel_MiniTreeColourBranch_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourBranch.Location = new Point(12, 168);
            m_buttonConfigMiniTreeColourBranch.Name = "m_buttonConfigMiniTreeColourBranch";
            m_buttonConfigMiniTreeColourBranch.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourBranch.TabIndex = 16;
            m_buttonConfigMiniTreeColourBranch.Text = "Br&anches";
            m_buttonConfigMiniTreeColourBranch.Click += configPanel_MiniTreeColourBranch_Button_click;

            //
            // configPanel_MiniTreeColourIndiBorder_Button (Tree Diagrams)
            // 
            m_buttonConfigMiniTreeColourIndiBorder.Location = new Point(122, 168);
            m_buttonConfigMiniTreeColourIndiBorder.Name = "m_buttonConfigMiniTreeColourIndiBorder";
            m_buttonConfigMiniTreeColourIndiBorder.Size = new Size(98, 24);
            m_buttonConfigMiniTreeColourIndiBorder.TabIndex = 17;
            m_buttonConfigMiniTreeColourIndiBorder.Text = "Box bor&ders";
            m_buttonConfigMiniTreeColourIndiBorder.Click += configPanel_MiniTreeColourIndiBorder_Button_click;
        }

        // Builds the GUI for the Advanced pane in the config panel
        private void InitialiseSettingsAdvancedPane()
        {
            // 
            // configPanel_Charset_Label  (Advanced)
            // 
            m_labelConfigCharset.Location = new Point(9, 0);
            m_labelConfigCharset.Name = "m_labelConfigCharset";
            m_labelConfigCharset.RightToLeft = RightToLeft.No;
            m_labelConfigCharset.Size = new Size(120, 24);
            m_labelConfigCharset.TabIndex = 1;
            m_labelConfigCharset.Text = "Ch&aracter set:";
            m_labelConfigCharset.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_Charset_ComboBox (Advanced)
            // 
            m_comboboxConfigCharset.Location = new Point(139, 1);
            m_comboboxConfigCharset.Name = "m_comboboxConfigCharset";
            m_comboboxConfigCharset.Size = new Size(95, 20);
            m_comboboxConfigCharset.TabIndex = 2;
            m_comboboxConfigCharset.DropDownWidth = 40;
            m_comboboxConfigCharset.DropDownStyle = ComboBoxStyle.DropDownList;
            m_comboboxConfigCharset.SelectedIndexChanged += configPanel_Charset_ComboBox_changed;

            //
            // configPanel_UseBom_CheckBox (Advanced)
            // 
            m_checkboxConfigUseBom.Location = new Point(11, 26);
            m_checkboxConfigUseBom.Name = "m_checkboxConfigUseBom";
            m_checkboxConfigUseBom.Size = new Size(200, 20);
            m_checkboxConfigUseBom.Text = "Include &byte order mark (BOM)";
            m_checkboxConfigUseBom.TabIndex = 3;

            // 
            // configPanel_HTMLExtn_Label (Advanced)
            // 
            m_labelConfigHtmlExtn.Location = new Point(9, 54);
            m_labelConfigHtmlExtn.Name = "m_labelConfigHtmlExtn";
            m_labelConfigHtmlExtn.RightToLeft = RightToLeft.No;
            m_labelConfigHtmlExtn.Size = new Size(140, 24);
            m_labelConfigHtmlExtn.TabIndex = 4;
            m_labelConfigHtmlExtn.Text = "H&TML file extension:";
            m_labelConfigHtmlExtn.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_HTMLExtn_ComboBox  (Advanced)
            // 
            m_comboboxConfigHtmlExtn.Location = new Point(149, 55);
            m_comboboxConfigHtmlExtn.Name = "m_comboboxConfigHtmlExtn";
            m_comboboxConfigHtmlExtn.Size = new Size(85, 20);
            m_comboboxConfigHtmlExtn.TabIndex = 5;
            m_comboboxConfigHtmlExtn.DropDownWidth = 40;
            m_comboboxConfigHtmlExtn.DropDownStyle = ComboBoxStyle.DropDownList;

            //
            // configPanel_W3C_CheckBox (Advanced)
            // 
            m_checkboxConfigW3C.Location = new Point(11, 91);
            m_checkboxConfigW3C.Name = "m_checkboxConfigW3C";
            m_checkboxConfigW3C.Size = new Size(200, 20);
            m_checkboxConfigW3C.Text = "Add &W3C validator sticker";
            m_checkboxConfigW3C.TabIndex = 6;

            //
            // configPanel_user_rec_filename_CheckBox (Advanced)
            // 
            m_checkboxConfigUserRecFilename.Location = new Point(11, 112);
            m_checkboxConfigUserRecFilename.Name = "m_checkboxConfigUserRecFilename";
            m_checkboxConfigUserRecFilename.Size = new Size(240, 24);
            m_checkboxConfigUserRecFilename.Text = "&Use custom record number for filenames";
            m_checkboxConfigUserRecFilename.TabIndex = 7;

            //
            // configPanel_SupressBackreferences_CheckBox (Advanced)
            // 
            m_checkboxConfigSupressBackreferences.Location = new Point(11, 136);
            m_checkboxConfigSupressBackreferences.Name = "m_checkboxConfigSupressBackreferences";
            m_checkboxConfigSupressBackreferences.Size = new Size(250, 20);
            m_checkboxConfigSupressBackreferences.Text = "List c&iting records on source pages";
            m_checkboxConfigSupressBackreferences.TabIndex = 8;

            // 
            // m_labelConfigStylesheetName (Advanced)
            // 
            m_labelConfigStylesheetName.Location = new Point(266, 0);
            m_labelConfigStylesheetName.Name = "m_labelConfigStylesheetName";
            m_labelConfigStylesheetName.RightToLeft = RightToLeft.No;
            m_labelConfigStylesheetName.Size = new Size(224, 24);
            m_labelConfigStylesheetName.TabIndex = 9;
            m_labelConfigStylesheetName.Text = "Name of st&ylesheet:";
            m_labelConfigStylesheetName.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_StylesheetName_EditBox (Advanced)
            // 
            m_textboxConfigStylesheetName.Location = new Point(266, 32);
            m_textboxConfigStylesheetName.Name = "m_textboxConfigStylesheetName";
            m_textboxConfigStylesheetName.Size = new Size(175, 20);
            m_textboxConfigStylesheetName.TabIndex = 10;
            m_textboxConfigStylesheetName.Text = "";
            m_textboxConfigStylesheetName.Multiline = false;

            // 
            // configPanel_StylesheetName_ExtnLabel (Advanced)
            // 
            m_labelConfigStylesheetNameExtn.Location = new Point(440, 27);
            m_labelConfigStylesheetNameExtn.Name = "m_labelConfigStylesheetNameExtn";
            m_labelConfigStylesheetNameExtn.RightToLeft = RightToLeft.No;
            m_labelConfigStylesheetNameExtn.Size = new Size(60, 24);
            m_labelConfigStylesheetNameExtn.TabIndex = 11;
            m_labelConfigStylesheetNameExtn.Text = ".css";
            m_labelConfigStylesheetNameExtn.TextAlign = ContentAlignment.BottomLeft;

            //
            // configPanel_PreserveStylesheet_CheckBox (Advanced)
            // 
            m_checkboxConfigPreserveStylesheet.Location = new Point(266, 56);
            m_checkboxConfigPreserveStylesheet.Name = "m_checkboxConfigPreserveStylesheet";
            m_checkboxConfigPreserveStylesheet.Size = new Size(250, 20);
            m_checkboxConfigPreserveStylesheet.Text = "Do &not generate new stylesheet";
            m_checkboxConfigPreserveStylesheet.TabIndex = 12;

            //
            // m_checkboxConfigExcludeHelppage (Advanced)
            // 
            m_checkboxConfigIncludeHelppage.Location = new Point(266, 91);
            m_checkboxConfigIncludeHelppage.Name = "m_checkboxConfigExcludeHelppage";
            m_checkboxConfigIncludeHelppage.Size = new Size(250, 20);
            m_checkboxConfigIncludeHelppage.Text = "Include help page";
            m_checkboxConfigIncludeHelppage.TabIndex = 15;
        }

        // Adds the panes to the config panel
        private void AddSettingsPanes()
        {
            TabPage tabPageSettingsWebpages = new TabPage("Webpages");
            tabPageSettingsWebpages.Controls.Add(m_labelConfigCommentary);
            tabPageSettingsWebpages.Controls.Add(m_textboxConfigCommentary);
            tabPageSettingsWebpages.Controls.Add(m_labelConfigCommentaryIsHtml);
            tabPageSettingsWebpages.Controls.Add(m_checkboxConfigCommentaryIsHtml);
            tabPageSettingsWebpages.Controls.Add(m_labelConfigEmail);
            tabPageSettingsWebpages.Controls.Add(m_textboxConfigEmail);
            tabPageSettingsWebpages.Controls.Add(m_labelConfigUserLink);
            tabPageSettingsWebpages.Controls.Add(m_textboxConfigUserLink);
            tabPageSettingsWebpages.Controls.Add(m_textboxConfigIndexName);
            tabPageSettingsWebpages.Controls.Add(m_labelConfigIndexName);
            tabPageSettingsWebpages.Controls.Add(m_labelConfigIndexNameExtn);
            tabPageSettingsWebpages.Controls.Add(m_checkboxConfigPreserveFrontPage);
            tabPageSettingsWebpages.Controls.Add(m_checkboxConfigStats);
            tabPageSettingsWebpages.Controls.Add(m_checkboxConfigMultiPageIndex);
            tabPageSettingsWebpages.Controls.Add(m_checkboxConfigUserRefInIndex);
            tabPageSettingsWebpages.Controls.Add(m_labelConfigMultiPageIndexNumber);
            tabPageSettingsWebpages.Controls.Add(m_textboxConfigMultiPageIndexNumber);
            tabPageSettingsWebpages.Controls.Add(m_checkboxConfigCdrom);
            tabPageSettingsWebpages.Controls.Add(m_labelConfigCustomFooter);
            tabPageSettingsWebpages.Controls.Add(m_textboxConfigCustomFooter);
            tabPageSettingsWebpages.Controls.Add(m_labelConfigFooterIsHtml);
            tabPageSettingsWebpages.Controls.Add(m_checkboxConfigFooterIsHtml);
            m_tabcontrolConfigPanel.TabPages.Add(tabPageSettingsWebpages);

            TabPage tabPageSettingsImages = new TabPage("Images");
            tabPageSettingsImages.Controls.Add(m_labelConfigFrontImageEdit);
            tabPageSettingsImages.Controls.Add(m_textboxConfigFrontImageEdit);
            tabPageSettingsImages.Controls.Add(m_buttonConfigFrontImageBrowse);
            tabPageSettingsImages.Controls.Add(m_textboxConfigBackImageEdit);
            tabPageSettingsImages.Controls.Add(m_labelConfigBackImageEdit);
            tabPageSettingsImages.Controls.Add(m_buttonConfigBackImageBrowse);
            tabPageSettingsImages.Controls.Add(m_labelConfigIndiImageSize);
            tabPageSettingsImages.Controls.Add(m_labelConfigIndiImageWidth);
            tabPageSettingsImages.Controls.Add(m_textboxConfigIndiImageWidth);
            tabPageSettingsImages.Controls.Add(m_labelConfigIndiImageHeight);
            tabPageSettingsImages.Controls.Add(m_textboxConfigIndiImageHeight);
            tabPageSettingsImages.Controls.Add(m_labelConfigSourceImageSize);
            tabPageSettingsImages.Controls.Add(m_labelConfigSourceImageWidth);
            tabPageSettingsImages.Controls.Add(m_textboxConfigSourceImageWidth);
            tabPageSettingsImages.Controls.Add(m_labelConfigSourceImageHeight);
            tabPageSettingsImages.Controls.Add(m_textboxConfigSourceImageHeight);
            tabPageSettingsImages.Controls.Add(m_labelConfigThumbnailImageSize);
            tabPageSettingsImages.Controls.Add(m_labelConfigThumbnailImageWidth);
            tabPageSettingsImages.Controls.Add(m_textboxConfigThumbnailImageWidth);
            tabPageSettingsImages.Controls.Add(m_labelConfigThumbnailImageHeight);
            tabPageSettingsImages.Controls.Add(m_textboxConfigThumbnailImageHeight);
            tabPageSettingsImages.Controls.Add(m_checkboxConfigIndiImages);
            tabPageSettingsImages.Controls.Add(m_checkboxConfigNonPictures);
            tabPageSettingsImages.Controls.Add(m_checkboxConfigRenameOriginals);
            tabPageSettingsImages.Controls.Add(m_checkboxConfigKeepOriginals);
            tabPageSettingsImages.Controls.Add(m_checkboxConfigAllowMultimedia);
            m_tabcontrolConfigPanel.TabPages.Add(tabPageSettingsImages);

            TabPage tabPageSettingsGedcom = new TabPage("GEDCOM");
            tabPageSettingsGedcom.Controls.Add(m_labelConfigNoName);
            tabPageSettingsGedcom.Controls.Add(m_textboxConfigNoName);
            m_groupboxConfigWithheldName.Controls.Add(m_radiobuttonConfigWithheldNameLabel);
            m_groupboxConfigWithheldName.Controls.Add(m_textboxConfigWithheldName);
            m_groupboxConfigWithheldName.Controls.Add(m_radiobuttonConfigWithheldNameName);
            tabPageSettingsGedcom.Controls.Add(m_groupboxConfigWithheldName);
            tabPageSettingsGedcom.Controls.Add(m_checkboxConfigCapNames);
            tabPageSettingsGedcom.Controls.Add(m_checkboxConfigCapEvents);
            tabPageSettingsGedcom.Controls.Add(m_checkboxConfigHideEmails);
            tabPageSettingsGedcom.Controls.Add(m_checkboxConfigOccupationHeadline);
            tabPageSettingsGedcom.Controls.Add(m_checkboxConfigAllowTrailingSpaces);
            tabPageSettingsGedcom.Controls.Add(m_checkboxConfigShowWithheldRecords);
            tabPageSettingsGedcom.Controls.Add(m_labelConfigTabSpaces);
            tabPageSettingsGedcom.Controls.Add(m_textboxConfigTabSpaces);
            m_tabcontrolConfigPanel.TabPages.Add(tabPageSettingsGedcom);

            TabPage tabPageSettingsTreeDiagrams = new TabPage("Tree Diagrams");
            tabPageSettingsTreeDiagrams.Controls.Add(m_checkboxConfigTreeDiagrams);
            tabPageSettingsTreeDiagrams.Controls.Add(m_checkboxConfigTreeDiagramsFakeBg);
            tabPageSettingsTreeDiagrams.Controls.Add(m_labelConfigTreeDiagramsFormat);
            tabPageSettingsTreeDiagrams.Controls.Add(m_comboboxConfigTreeDiagramsFormat);
            tabPageSettingsTreeDiagrams.Controls.Add(m_checkboxConfigConserveTreeWidth);
            tabPageSettingsTreeDiagrams.Controls.Add(m_checkboxConfigKeepSiblingOrder);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourIndiBackground);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourIndiHighlight);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourIndiBgConcealed);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourIndiShade);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourIndiText);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourIndiLink);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourBranch);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourIndiBorder);
            m_groupboxMiniTreeColours.Controls.Add(m_buttonConfigMiniTreeColourIndiFgConcealed);
            tabPageSettingsTreeDiagrams.Controls.Add(m_groupboxMiniTreeColours);
            m_tabcontrolConfigPanel.TabPages.Add(tabPageSettingsTreeDiagrams);

            TabPage tabPageSettingsAdvanced = new TabPage("Advanced");
            tabPageSettingsAdvanced.Controls.Add(m_labelConfigCharset);
            tabPageSettingsAdvanced.Controls.Add(m_comboboxConfigCharset);
            tabPageSettingsAdvanced.Controls.Add(m_labelConfigHtmlExtn);
            tabPageSettingsAdvanced.Controls.Add(m_comboboxConfigHtmlExtn);
            tabPageSettingsAdvanced.Controls.Add(m_checkboxConfigW3C);
            tabPageSettingsAdvanced.Controls.Add(m_checkboxConfigUserRecFilename);
            tabPageSettingsAdvanced.Controls.Add(m_textboxConfigStylesheetName);
            tabPageSettingsAdvanced.Controls.Add(m_labelConfigStylesheetName);
            tabPageSettingsAdvanced.Controls.Add(m_labelConfigStylesheetNameExtn);
            tabPageSettingsAdvanced.Controls.Add(m_checkboxConfigPreserveStylesheet);
            tabPageSettingsAdvanced.Controls.Add(m_checkboxConfigUseBom);
            tabPageSettingsAdvanced.Controls.Add(m_checkboxConfigSupressBackreferences);
            tabPageSettingsAdvanced.Controls.Add(m_checkboxConfigIncludeHelppage);
            m_tabcontrolConfigPanel.TabPages.Add(tabPageSettingsAdvanced);

            m_tabcontrolConfigPanel.Location = new Point(0, 0);
            m_tabcontrolConfigPanel.Name = "configTabs";
            m_tabcontrolConfigPanel.Size = new Size(507, 272);
            m_tabcontrolConfigPanel.TabIndex = 12;
        }



        // The main entry point for the application.
        [STAThread]
        static void Main( string[] args ) 
        {
            bool bResetConfig = false;
            bool bNextArgIsFilename = false;
            string sLogFilename = "";

            // User can hold down Ctrl and Shift while app starts to enable log file creation
            // People will find this easier than altering command gedcomLine options!
            if(ModifierKeys == (Keys.Shift | Keys.Control) )
            {
                LogFile.TheLogFile.SetLogLevel( LogFile.EDebugLevel.All );
                LogFile.TheLogFile.StartLogFile( "C:\\gedmill.txt" );
            }

            if( args != null )
            {
                foreach( string sArg in args )
                {
                    if( bNextArgIsFilename )
                    {
                        sLogFilename = sArg;
                        bNextArgIsFilename = false;
                    }
                    else
                    {
                        switch( sArg ) 
                        {
                            case "-logfile":
                            {
                                LogFile.TheLogFile.SetLogLevel( LogFile.EDebugLevel.All );
                                LogFile.TheLogFile.SetDebugAllowFilter( LogFile.DT_ALL^LogFile.DT_GEDCOM ); // Everything but gedcom
                                bNextArgIsFilename = true;
                                break;
                            }
                            case "-debug":
                            {
                                LogFile.TheLogFile.SetLogLevel( LogFile.EDebugLevel.All );
                                LogFile.TheLogFile.SetDebugAllowFilter( LogFile.DT_ALL^LogFile.DT_GEDCOM ); // Everything but gedcom
                                break;
                            }
                            case "-debug_gedcom":
                            {
                                LogFile.TheLogFile.SetLogLevel( LogFile.EDebugLevel.All );
                                LogFile.TheLogFile.SetDebugAllowFilter( LogFile.DT_ALL ); // All
                                break;
                            }
                            case "-reset":
                            {
                                bResetConfig = true;
                                break;
                            }
                        }
                    }
                }
            }
            if( sLogFilename != "" )
            {
                LogFile.TheLogFile.StartLogFile( sLogFilename );
            }
            string sStartTime = DateTime.Now.ToString();
            LogFile.TheLogFile.WriteLine( LogFile.DT_APP, LogFile.EDebugLevel.Note, m_sSoftwareName + " started at " + sStartTime );

            m_mainForm = new MainForm( bResetConfig );

            Application.Run( m_mainForm );

            LogFile.TheLogFile.StopLogFile();
        }

    } // End of class MainForm


}
