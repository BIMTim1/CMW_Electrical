using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

//Revit 2021 API
//Debug in Revit 2020

namespace CMW_Electrical
{
    public class CMW_Electrical_Ribbon : IExternalApplication
    {
        public const string versionNumber = "1.0";
        public const string releaseDate = "August 2023";

        static void AddRibbonPanel(UIControlledApplication application)
        {
            //Create a custom ribbon tab
            string tabName = "CMW - Electrical";
            application.CreateRibbonTab(tabName);

            //add ribbon panels
            RibbonPanel aboutPanel = application.CreateRibbonPanel(tabName, "About");
            RibbonPanel circuitPanel = application.CreateRibbonPanel(tabName, "Circuits");
            RibbonPanel devicePanel = application.CreateRibbonPanel(tabName, "Devices");
            RibbonPanel equipPanel = application.CreateRibbonPanel(tabName, "Equipment");
            RibbonPanel schedulePanel = application.CreateRibbonPanel(tabName, "Panel Schedules");
            RibbonPanel oneLinePanel = application.CreateRibbonPanel(tabName, "One-Line");

            //get dll assembly path
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;


            //------------create push button for GeneralInfoButton------------
            PushButtonData generalInfoData = new PushButtonData(
                "cmdAddinInformation",
                "Add-in" + System.Environment.NewLine + " Information ",
                thisAssemblyPath, "AddinInformation.AddinInfo");//<namespace name>.<cs name>

            PushButton generalInfoBtn = aboutPanel.AddItem(generalInfoData) as PushButton;
            //generalInfoBtn ToolTip Information
            generalInfoBtn.ToolTip = "CMTA Midwest - Electrical Add-in Information";
            //generalInfoBtn.LongDescription = "";
            //generalInfoBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/CircuitNoteTTImage.png"));
            //generalInfoBtn ContextualHelp Information
            //ContextualHelp generalInfoHelp = new ContextualHelp(ContextualHelpType.Url, "");
            //generalInfoBtn.SetContextualHelp(generalInfoHelp);
            //generalInfoBtn Image Information
            BitmapImage generalInfoImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/Info32x32.png"));
            generalInfoBtn.LargeImage = generalInfoImage;
            //create secondary image for Quick Access Toolbar
            //generalInfoBtn.Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/AddElecCctNote16x16.png"));


            //------------create push button for AddElecCircuitNote------------
            PushButtonData cctNoteData = new PushButtonData(
                "cmdAddElecCircuitNote",
                "Add Cct" + System.Environment.NewLine + " Note ",
                thisAssemblyPath, "AddNoteToElectricalCircuit.AddElecCircuitNote");//<namespace name>.<cs name>

            PushButton cctNoteBtn = circuitPanel.AddItem(cctNoteData) as PushButton;
            //cctNoteBtn ToolTip Information
            cctNoteBtn.ToolTip = "Updates Electrical Circuit Load Name parameters with custom Front or Back notes.";
            cctNoteBtn.LongDescription = "This tool will add the manual text parameter values of E_Circuit Note-Front and E_Circuit Note-Back to any applicable Electrical Circuit Load Name parameter. This function can be used to add EX to all existing Electrical Circuits or (NOTE 1) to the end of a Load Name to indicate a specific note defined by the user in the Panelboard Schedule.";
            cctNoteBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/CircuitNoteTTImage.png"));
            //cctNoteBtn ContextualHelp Information
            //ContextualHelp cctNoteHelp = new ContextualHelp(ContextualHelpType.Url, "");
            //cctNoteBtn.SetContextualHelp(cctNoteHelp);
            //cctNoteBtn Image Information
            BitmapImage cctNoteImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/AddElecCctNote32x32.png"));
            cctNoteBtn.LargeImage = cctNoteImage;
            //create secondary image for Quick Access Toolbar
            cctNoteBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/AddElecCctNote16x16.png"));


            //------------create push button for PanelScheduleReset------------
            PushButtonData schedResetData = new PushButtonData(
                "cmdPanelScheduleReset",
                "Reset" + System.Environment.NewLine + " Templates ",
                thisAssemblyPath, "ResetPanelScheduleTemplate.PanelScheduleReset");

            PushButton schedResetBtn = schedulePanel.AddItem(schedResetData) as PushButton;
            //schedResetBtn ToolTip Information
            schedResetBtn.ToolTip = "Reset All Panel Schedule Views.";
            schedResetBtn.LongDescription = "This tool will 'refresh' all Panel Schedule Views in a project. Any change made to a Panel Schedule Template will now be applied to all Panel Schedule Views. Any change made to Electrical Equipment families will now be correctly shown in the associated Panel Schedule View.";
            schedResetBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/ResetPanelSchedTTImage.png"));
            //schedResetBtn ContextualHelp Information
            //ContextualHelp schedResetHelp = new ContextualHelp(ContextualHelpType.Url, "");
            //schedResetBtn.SetContextualHelp(schedResetHelp);
            //create main image information
            BitmapImage schedResetImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/ResetPanelTemplate_32x32.png"));
            schedResetBtn.LargeImage = schedResetImage;
            //create secondary image for Quick Access Toolbar
            schedResetBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/ResetPanelTemplate_16x16.png"));


            //------------create push button for PanelTypeToSinglePhase------------
            PushButtonData pnlTypeToSingleData = new PushButtonData("cmdPanelTypeToSinglePhase",
                "Panel Type" + System.Environment.NewLine + " To 1-Ph ",
                thisAssemblyPath, "ChangePanelTypeToSinglePhase.PanelTypeToSinglePhase");

            PushButton pnlTypeToSingleBtn = equipPanel.AddItem(pnlTypeToSingleData) as PushButton;
            //pnlTypeToSingleBtn ToolTip Information
            pnlTypeToSingleBtn.ToolTip = "Change 3-Phase Panel Type to Single Phase";
            pnlTypeToSingleBtn.LongDescription = "Circumvent the limitations of default Revit properties by selecting an Electrical Equipment family to update from a 3-Phase distribution system to a 1-Phase distribution system and type. This tool will also reconnect existing circuits from the selected equipment to the source as well as any branch circuits.";
            //pnlTypeToSingleBtn.ToolTipImage = new BitmapImage(new Uri(""));
            //pnlTypeToSingleBtn ContextualHelp Information
            //ContextualHelp pnlTypeToSingleHelp = new ContextualHelp(ContextualHelpType.URL, ""pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/");
            //pnlTypeToSingleBtn.SetContextualHelp(pnlTypeToSingleHelp);
            //pnlTypeToSingleBtn Image Information
            BitmapImage pnlTypeToSingleImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/PanelToSinglePhase32x32.png"));
            pnlTypeToSingleBtn.LargeImage = pnlTypeToSingleImage;
            //pnlTypeToSingleBtn QuickAccess Image
            pnlTypeToSingleBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/PanelToSinglePhase16x16.png"));


            //------------create push button for MutliLegSwitchLegUpdate------------
            PushButtonData switchMultiLegData = new PushButtonData("cmdMultiLegSwitchLegUpdate",
                "Multi-Leg" + System.Environment.NewLine + " Id Update ",
                thisAssemblyPath, "UpdateMultiLegSwitchIds.MutliLegSwitchLegUpdate");

            PushButton switchMultiLegBtn = devicePanel.AddItem(switchMultiLegData) as PushButton;
            //switchMultiLegBtn ToolTip Information
            switchMultiLegBtn.ToolTip = "Updates Switch Id of E_LD_Nested Switch Leg Families";
            switchMultiLegBtn.LongDescription = "Updates all E_LD_Nested Switch Leg family Switch Id parameters to match host family E_SWId parameter values. If the host value is blank, the Switch Id value will be updated to nothing.";
            switchMultiLegBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/MultiLegSwitchIdTTImage.png"));
            //switchMultiLegBtn ContextualHelp Information
            //ContextualHelp switchMultiLegHelp = new ContextualHelp(ContextualHelpType.URL, ""pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/");
            //switchMultiLegBtn.SetContextualHelp(switchMultiLegHelp);
            //switchMultiLegBtn Image Information
            BitmapImage switchMultiLegImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/MultiLegSwitchIdUpdate32x32.png"));
            switchMultiLegBtn.LargeImage = switchMultiLegImage;
            //switchMultiLegBtn QuickAccess Image
            switchMultiLegBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/MultiLegSwitchIdUpdate16x16.png"));


            //------------create push button for DeviceSymbolsRotate------------
            PushButtonData rotateSymData = new PushButtonData("cmdRotateDeviceSymbols",
                "Rotate" + System.Environment.NewLine + " Text Symbols ",
                thisAssemblyPath, "RotateDeviceSymbols.DeviceSymbolsRotate");

            PushButton rotateSymBtn = devicePanel.AddItem(rotateSymData) as PushButton;
            //rotateSymBtn ToolTip Information
            rotateSymBtn.ToolTip = "Rotate Electrical Device Text Symbols";
            rotateSymBtn.LongDescription = "Rotates the nested Generic Annotation family of all Electrical devices if the U_D Symbol parameter is present.";
            //rotateSymBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/"));
            //rotateSymBtn ContextualHelp Information
            //ContextualHelp rotateSymHelp = new ContextualHelp(ContextualHelpType.URL, ""pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/");
            //rotateSymBtn.SetContextualHelp(rotateSymHelp);
            //rotateSymBtn Image Information
            BitmapImage rotateSymImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/SymbolRotate32x32.png"));
            rotateSymBtn.LargeImage = rotateSymImage;
            //rotateSymBtn QuickAccess Image
            rotateSymBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/SymbolRotate16x16.png"));


            //------------create push button for EquipNameUpdate------------
            PushButtonData equipUpdateData = new PushButtonData("cmdEquipNameUpdate",
                "Panel Name" + System.Environment.NewLine + " Updater ",
                thisAssemblyPath, "EquipNameUpdate.EquipInfoUpdate");

            PushButton equipUpdateBtn = equipPanel.AddItem(equipUpdateData) as PushButton;
            //equipUpdateBtn ToolTip Information
            equipUpdateBtn.ToolTip = "Update ALL Electrical Equipment Name Information";
            equipUpdateBtn.LongDescription = "";
            //equipUpdateBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/"));
            //equipUpdateBtn ContextualHelp Information
            //ContextualHelp equipUpdateHelp = new ContextualHelp(ContextualHelpType.URL, ""pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/");
            //equipUpdateBtn.SetContextualHelp(equipUpdateHelp);
            //equipUpdateBtn Image Information
            BitmapImage equipUpdateImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/EquipNameUpdate32x32.png"));
            equipUpdateBtn.LargeImage = equipUpdateImage;
            //equipUpdateBtn QuickAccess Image
            equipUpdateBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/EquipNameUpdate16x16.png"));


            //------------create push button for CreatePanelSchedules------------
            PushButtonData createPanSchedData = new PushButtonData("cmdCreatePanelSchedules",
                "Create Panel" + System.Environment.NewLine + " Schedules ",
                thisAssemblyPath, "CreatePanelSchedules.CreatePanelSchedules");

            PushButton createPanSchedBtn = schedulePanel.AddItem(createPanSchedData) as PushButton;
            //createPanSched ToolTip Information
            createPanSchedBtn.ToolTip = "Create Panel Schedules for All Electrical Equipment";
            createPanSchedBtn.LongDescription = "";
            //createPanSchedBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/"));
            //createPanSched ContextualHelp Information
            //ContextualHelp createPanSchedHelp = new ContextualHelp(ContextualHelpType.URL, ""pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/");
            //createPanSchedBtn.SetContextualHelp(createPanSchedHelp);
            //createPanSched Image Information
            BitmapImage createPanSchedImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/CreatePanelSchedules32x32.png"));
            createPanSchedBtn.LargeImage = createPanSchedImage;
            //equipUpdateBtn QuickAccess Image
            createPanSchedBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/CreatePanelSchedules16x16.png"));


            //------------create push button for FlipFacingOrientation------------
            PushButtonData flipFacingOrientData = new PushButtonData("cmdFlipFacingOrientation",
                "Flip Lighting" + System.Environment.NewLine + " Host Plane ",
                thisAssemblyPath, "FlipFacingOrientation.FlipFacingOrientationBySelection");

            PushButton flipFacingOrientBtn = devicePanel.AddItem(flipFacingOrientData) as PushButton;
            //flipFacingOrient ToolTip Information
            flipFacingOrientBtn.ToolTip = "Using a Selection Window, select multiple Lighting Fixtures to Flip the Facing Orientation.";
            flipFacingOrientBtn.LongDescription = "";
            //flipFacingOrientBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/"));
            //flipFacingOrient ContextualHelp Information
            //ContextualHelp flipFacingOrientHelp = new ContextualHelp(ContextualHelpType.URL, ""pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/");
            //flipFacingOrientBtn.SetContextualHelp(flipFacingOrientHelp);
            //flipFacingOrient Image Information
            BitmapImage flipFacingOrientImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/FlipFacingOrientation32x32.png"));
            flipFacingOrientBtn.LargeImage = flipFacingOrientImage;
            //equipUpdateBtn QuickAccess Image
            flipFacingOrientBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/FlipFacingOrientation16x16.png"));


            //------------create push button for UpdateMotorMOCP------------
            PushButtonData motorMOCPUpdateData = 
                new PushButtonData(
                    "cmdmotorMOCPUpdate",
                    "Update Motor MOCP",
                    thisAssemblyPath, "MotorMOCPUpdate.UpdateMotorMOCP")
            {
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/UpdateMotorMOCP16x16.png")),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/UpdateMotorMOCP32x32.png")),
                ToolTip = "Updates all Motor Electrical Circuit Ratings that have their MES_(MCA) MOCP parameter set to a non-blank value."
            };

            //------------create push button for motorUIDUpdate------------
            PushButtonData motorUIDUpdateData = 
                new PushButtonData(
                    "cmdMotorUIDUpdate", 
                    "Update Motor UID", 
                    thisAssemblyPath,
                    "MotorUIDUpdate.UpdateMotorUID")
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/UpdateMotorUID32x32.png")),
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/UpdateMotorUID16x16.png")),
                ToolTip = "Update Motor UID values based on hosted Mechanical Equipment."
            };

            //create motorPulldownButton
            PulldownButtonData motorPulldownButtonData = new PulldownButtonData("motorSplitButton", "Motor")
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/UpdateMotorMOCP32x32.png"))
            };
            PulldownButton motorPulldownButton = devicePanel.AddItem(motorPulldownButtonData) as PulldownButton;
            //add motorMOCPUpdate PushButton to motorPulldownButton SplitButton
            motorPulldownButton.AddPushButton(motorMOCPUpdateData);
            motorPulldownButton.AddPushButton(motorUIDUpdateData);


            //------------create push button for CorrectLightFixtures------------
            PushButtonData correctLightFixturesData = new PushButtonData("cmdCorrectLightFixtures",
                "Correct Light" + System.Environment.NewLine + " Fixtures ",
                thisAssemblyPath, "CorrectLightFixtures.CorrectLightFixtures");

            PushButton correctLightFixturesBtn = devicePanel.AddItem(correctLightFixturesData) as PushButton;
            //correctLightFixtures ToolTip Information
            correctLightFixturesBtn.ToolTip = "Updates all Lighting Fixtures to be the correct families based on the loaded E_LIGHT FIXTURE SCHEDULE.";
            correctLightFixturesBtn.LongDescription = "In October of 2022, the BIM Team updated all Lighting Fixture families" +
                " to use Multiline Text parameters in lieu of typical text parameters. " +
                "This change becomes confusion when jumping between various years of projects started " +
                "in various stages of the updated CMTA Midwest template. This tool will go through all " +
                "Lighting Fixture families in the project, compare the family parameters to the parameters " +
                "being used by the E_LIGHT FIXTURE SCHEDULE, and replace all Lighting Fixture families based on the referenced schedule.";
            //correctLightFixturesBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/"));
            //correctLightFixtures ContextualHelp Information
            //ContextualHelp correctLightFixturesHelp = new ContextualHelp(ContextualHelpType.URL, ""pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/");
            //correctLightFixturesBtn.SetContextualHelp(correctLightFixturesHelp);
            //correctLightFixtures Image Information
            BitmapImage correctLightFixturesImage = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/CorrectLightFixtures32x32.png"));
            correctLightFixturesBtn.LargeImage = correctLightFixturesImage;
            //equipUpdateBtn QuickAccess Image
            correctLightFixturesBtn.Image = new BitmapImage(new Uri(
                "pack://application:,,,/CMW_Electrical;component/Resources/CorrectLightFixtures16x16.png"));


            ////------------create push button for oneLineConnectAndPlace------------
            //PushButtonData oneLineConnectAndPlaceData = 
            //    new PushButtonData(
            //        "cmdOneLineConnectAndPlace", 
            //        "Power/" + System.Environment.NewLine + " Create ", //"Connect and Place Component"
            //        thisAssemblyPath, 
            //        "OneLineConnectAndPlace.OneLineConnectAndPlace")
            //{
            //        LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLConnectAndPlace32x32.png")),
            //        Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLConnectAndPlace16x16.png")),
            //        ToolTip = "Select an existing One-Line item to connect to, and place the down-stream equipment."
            //};


            //------------create push button for oneLineConnect------------
            PushButtonData oneLineConnectData = 
                new PushButtonData(
                    "cmdOneLineConnect", 
                    "Power", //"Connect Component"
                    thisAssemblyPath, 
                    "OneLineConnect.OneLineConnect")
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLConnect32x32.png")),
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLConnect16x16.png")),
                ToolTip = "Select One-Line Detail Items to be connected together.",
                LongDescription = "This tool will create feeder lines " +
                "and a circuit breaker (if applicable) on the Active One-Line Schematic. " +
                "If the selected Detail Items are associated to Electrical Equipment families in the model, " +
                "then an Electrical Circuit will be created between the associated Electrical Equipment families."
            };


            //------------create push button for oneLineUpdatePanelInfo------------
            PushButtonData oneLineUpdateDesignationsData = 
                new PushButtonData(
                    "cmdOneLineUpdateDesignations", 
                    "Update", //"Update Component or Panel Designation"
                    thisAssemblyPath,
                    "OneLineUpdateDesignations.OneLineUpdateDesignations")
            {
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLUpdatePanelName32x32.png")),
                    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLUpdatePanelName16x16.png")),
                    ToolTip = "From a selection dialog, determine whether to update Electrical Equipment Panel Names from schematic Detail Items or the inverse.",
                    LongDescription = "Any Electrical Equipment or Detail Item family with the same EqConId value will be updated as selected by the user."
                };


            PushButtonData oneLinePlaceEquipData =
                new PushButtonData(
                    "cmdOneLinePlaceEquip",
                    "Create", //"Place Equipment from One-Line"
                    thisAssemblyPath,
                    "OneLinePlaceEquip.OneLinePlaceEquip")
                {
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLPlaceEquip32x32.png")),
                    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLPlaceEquip16x16.png")),
                    ToolTip = "Select a Detail Item from your One-Line and Place the Corresponding Equipment in the model from a selected Level view."
            };


            PushButtonData oneLineAssociateData = 
                new PushButtonData(
                    "cmdOneLineAssociate", 
                    "Associate", //"Associate Equipment + Detail Item"
                    thisAssemblyPath, 
                    "OneLine_Associate.OneLineAssociate")
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLAssociate32x32.png")),
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLAssociate16x16.png")),
                ToolTip = "Select an Electrical Equipment family from a list of Panel Names and associate to a selected Detail Item family in One-Line."
            };


            PushButtonData oneLineDrawData = new PushButtonData(
                "cmdOneLineDraw", 
                "Draw", //"Draw Feeder"
                thisAssemblyPath, 
                "OneLineDraw.OneLineDraw")
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLDraw32x32.png")),
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLDraw16x16.png")),
                ToolTip = "Create Detail Item Lines for Feeder Representation in OneLine Schematic views."
            };


            PushButtonData oneLineCopy = new PushButtonData(
                "cmdOneLineCopy", 
                "Copy", //"Copy Components"
                thisAssemblyPath, 
                "OneLineCopy.OneLineCopy")
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLCopy32x32.png")),
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLCopy16x16.png")),
                ToolTip = "Copy One Line Detail Components and assign Model Building Elements."
            };


            PushButtonData oneLineSelectData = new PushButtonData(
                "cmdOneLineSelect", 
                "Select", 
                thisAssemblyPath, 
                "OneLineSelect.OneLineSelect")
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLSelect32x32.png")),
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLSelect16x16.png")),
                ToolTip = "Select an Detail Item or Electrical Equipment family and select the associated component in a Floor Plan View or Drafting View."
            };


            PushButtonData oneLineHalftoneExistingData = 
                new PushButtonData(
                    "cmdOneLineHalftoneExisting", 
                    "Halftone" + System.Environment.NewLine + " Existing ", 
                    thisAssemblyPath,
                    "OneLine_HalftoneExisting.OneLineHalftoneExisting")
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OLHalftoneExisting32x32.png")),
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/OlHalftoneExisting16x16.png")),
                ToolTip = "Sets the Graphic Override for all Existing phased content in active Schematic Drafting View to be Halftone."
            };
            

            PushButton oneLineAssociateBtn = oneLinePanel.AddItem(oneLineAssociateData) as PushButton;
            PushButton oneLinePlaceEquipBtn = oneLinePanel.AddItem(oneLinePlaceEquipData) as PushButton;
            //PushButton oneLineConnectAndPlaceBtn = oneLinePanel.AddItem(oneLineConnectAndPlaceData) as PushButton;
            PushButton oneLineConnectBtn = oneLinePanel.AddItem(oneLineConnectData) as PushButton;

            oneLinePanel.AddSeparator();

            PushButton oneLineUpdateDesignationsBtn = oneLinePanel.AddItem(oneLineUpdateDesignationsData) as PushButton;

            oneLinePanel.AddSeparator();

            PushButton oneLineCopyBtn = oneLinePanel.AddItem(oneLineCopy) as PushButton;
            PushButton oneLineDrawBtn = oneLinePanel.AddItem(oneLineDrawData) as PushButton;
            PushButton oneLineSelectBtn = oneLinePanel.AddItem(oneLineSelectData) as PushButton;

            oneLinePanel.AddSeparator();

            PushButton oneLineHalftoneExistingBtn = oneLinePanel.AddItem(oneLineHalftoneExistingData) as PushButton;


            //------------create push button for <button name>------------
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            //return result succeeded
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonPanel(application);
            return Result.Succeeded;
        }
    }
}
