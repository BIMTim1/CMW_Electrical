using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

//Revit 2023 API
//Debug in Revit 2022

namespace CMW_Electrical
{
    public class CMW_Electrical_Ribbon : IExternalApplication
    {
        public const string versionNumber = "1.1.0";
        public const string releaseDate = "January 2025";
        public const string bimProjectUrl = "https://wearelegence.sharepoint.com/:l:/r/sites/CMTAMidwestBIM/Lists/CMW%20Electrical%20Tools?e=e8U3xA";

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

            //define generic ContextualHelp
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, bimProjectUrl);

            //define typical Uri path
            string uriPath = "pack://application:,,,/CMW_Electrical;component/Resources/";

            //get dll assembly path
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;


            //------------create push button for GeneralInfoButton------------
            PushButtonData generalInfoData = new PushButtonData(
                "cmdAddinInformation",
                "Add-in" + System.Environment.NewLine + " Information ",
                thisAssemblyPath, "AddinInformation.AddinInfo")
            {
                ToolTip = "CMTA Midwest - Electrical Add-in Information",
                LargeImage = new BitmapImage(new Uri($"{uriPath}Info32x32.png"))
            };

            PushButton generalInfoBtn = aboutPanel.AddItem(generalInfoData) as PushButton;
            //generalInfoBtn.SetContextualHelp(contextHelp);


            PushButtonData tagByRefData = new PushButtonData(
                "cmdTagByReference",
                "Tag By" + System.Environment.NewLine + " Reference ",
                thisAssemblyPath,
                "AnnotateByReference.TagByReference")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}TagByRef32x32.png")),
                Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/TagByRef16x16.png")),
                ToolTip = "Tag elements from a reference."
            };

            PushButton tagByRefBtn = devicePanel.AddItem(tagByRefData) as PushButton;
            ContextualHelp tagByRefContextHelp = new ContextualHelp(ContextualHelpType.Url, "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Tag%20by%20Reference.mov?csf=1&web=1&e=obBMF5");
            tagByRefBtn.SetContextualHelp(tagByRefContextHelp);


            //------------create push button for AddElecCircuitNote------------
            //PushButtonData cctNoteData = new PushButtonData(
            //    "cmdAddElecCircuitNote",
            //    "Add Cct" + System.Environment.NewLine + " Note ",
            //    thisAssemblyPath, "AddNoteToElectricalCircuit.AddElecCircuitNote")
            //{
            //    ToolTip = "Updates Electrical Circuit Load Name parameters with custom Front or Back notes.",
            //    LongDescription = "This tool will add the manual text parameter values of E_Circuit Note-Front and E_Circuit Note-Back to any applicable Electrical Circuit Load Name parameter. This function can be used to add EX to all existing Electrical Circuits or (NOTE 1) to the end of a Load Name to indicate a specific note defined by the user in the Panelboard Schedule.",
            //    ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/CircuitNoteTTImage.png")),
            //    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/AddElecCctNote32x32.png")),
            //    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/AddElecCctNote16x16.png"))
            //};

            //PushButton cctNoteBtn = circuitPanel.AddItem(cctNoteData) as PushButton;


            //------------create push button for PanelScheduleReset------------
            PushButtonData schedResetData =
                new PushButtonData(
                    "cmdPanelScheduleReset",
                    "Reset" + System.Environment.NewLine + " Templates ",
                    thisAssemblyPath,
                    "ResetPanelScheduleTemplate.PanelScheduleReset")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}ResetPanelTemplate_32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}ResetPanelTemplate_16x16.png")),
                    ToolTip = "Reset All Panel Schedule Views.",
                    LongDescription = "This tool will 'refresh' all Panel Schedule Views in a project. Any change made to a Panel Schedule Template will now be applied to all Panel Schedule Views. Any change made to Electrical Equipment families will now be correctly shown in the associated Panel Schedule View.",
                    ToolTipImage = new BitmapImage(new Uri($"{uriPath}ToolTipImages/ResetPanelSchedTTImage.png"))
                };
            PushButton schedResetBtn = schedulePanel.AddItem(schedResetData) as PushButton;
            ContextualHelp schedResetContextHelp = new ContextualHelp(ContextualHelpType.Url, "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Reset%20Templates.mov?csf=1&web=1&e=BYF1rZ");
            schedResetBtn.SetContextualHelp(schedResetContextHelp);


            //------------create push button for PanelTypeToSinglePhase------------
            PushButtonData pnlTypeToSingleData =
                new PushButtonData(
                    "cmdPanelTypeToSinglePhase", 
                    "Change" + System.Environment.NewLine + " Distribution ", 
                    thisAssemblyPath,
                    "ChangePanelTypeToSinglePhase.PanelTypeToSinglePhase")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}PanelToSinglePhase32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}PanelToSinglePhase16x16.png")),
                    ToolTip = "Change Equipment Family, Type, and Distribution System",
                    LongDescription = "Circumvent the limitations of default Revit properties by selecting an Electrical Equipment family to update from the current family and type to the selected family and type. The tool will guide you through the process of reconnecting equipment and devices as Distriubtion Systems are changed."
                };

            PushButton pnlTypeToSingleBtn = equipPanel.AddItem(pnlTypeToSingleData) as PushButton;
            ContextualHelp changeDistContextHelp = new ContextualHelp(ContextualHelpType.Url, 
                "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Change%20Distribution.mov?csf=1&web=1&e=2cjJRY");
            pnlTypeToSingleBtn.SetContextualHelp(changeDistContextHelp);


            ////------------create push button for MutliLegSwitchLegUpdate------------
            //PushButtonData switchMultiLegData = new PushButtonData("cmdMultiLegSwitchLegUpdate",
            //    "Multi-Leg" + System.Environment.NewLine + " Id Update ",
            //    thisAssemblyPath, "UpdateMultiLegSwitchIds.MutliLegSwitchLegUpdate")
            //{
            //    ToolTip = "Updates Switch Id of E_LD_Nested Switch Leg Families",
            //    LongDescription = "Updates all E_LD_Nested Switch Leg family Switch Id parameters to match host family E_SWId parameter values. If the host value is blank, the Switch Id value will be updated to nothing.",
            //    ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/MultiLegSwitchIdTTImage.png")),
            //    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/MultiLegSwitchIdUpdate32x32.png")),
            //    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/MultiLegSwitchIdUpdate16x16.png"))
            //};

            //PushButton switchMultiLegBtn = devicePanel.AddItem(switchMultiLegData) as PushButton;


            //------------create push button for DeviceSymbolsRotate------------
            PushButtonData rotateSymData =
                new PushButtonData(
                    "cmdRotateDeviceSymbols",
                    "Rotate" + System.Environment.NewLine + " Text Symbols ",
                    thisAssemblyPath,
                    "RotateDeviceSymbols.DeviceSymbolsRotate")
                {
                    LargeImage = new BitmapImage(new Uri(
                $"{uriPath}SymbolRotate32x32.png")),
                    Image = new BitmapImage(new Uri(
                $"{uriPath}SymbolRotate16x16.png")),
                    ToolTip = "Rotate Electrical Device Text Symbols",
                    LongDescription = "Rotates the nested Generic Annotation family of all Electrical devices if the U_D Symbol parameter is present."
                };
            PushButton rotateSymBtn = devicePanel.AddItem(rotateSymData) as PushButton;
            ContextualHelp rotateSymContextHelp = new ContextualHelp(ContextualHelpType.Url, "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Rotate%20Text%20Symbols.mov?csf=1&web=1&e=PBf6Px");
            rotateSymBtn.SetContextualHelp(rotateSymContextHelp);


            //------------create push button for EquipNameUpdate------------
            PushButtonData equipUpdateData =
                new PushButtonData(
                    "cmdEquipNameUpdate",
                    "Panel Name Updater",
                    thisAssemblyPath,
                    "EquipNameUpdate.EquipInfoUpdate")
                {
                    LargeImage = new BitmapImage(new Uri(
                $"{uriPath}EquipNameUpdate32x32.png")),
                    Image = new BitmapImage(new Uri(
                $"{uriPath}EquipNameUpdate16x16.png")),
                    ToolTip = "Update ALL Electrical Equipment Name Information",
                    LongDescription = "This tool will update the associated Electrical Circuit Load Name and Panel Schedule Name parameter values from the Panel Name parameter."
                };


            //------------create push button for EquipCircuitUpdate------------
            PushButtonData equipCircuitUpdateData =
                new PushButtonData(
                    "cmdEquipCircuitUpdate",
                    "Panel Circuit Updater",
                    thisAssemblyPath,
                    "EquipCircuitUpdate.EquipCircuitUpdate")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}PnlUpdateRating32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}PnlUpdateRating16x16.png")),
                    ToolTip = "Update the Rating and Frame parameters of all Electrical Equipment instances.",
                    LongDescription = "This tool will take the Mains and E_Bussing values of all Electrical Equipment families " +
                "and use these values to update the associated Electrical Circuit of the Electrical Equipment."
                };

            //create equipUpdatePulldownButton
            PulldownButtonData equipUpdatePulldownButtonData = new PulldownButtonData(
                "equipUpdatePullDown", 
                "Equipment" + System.Environment.NewLine + " Update ")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}EquipNameUpdate32x32.png")),
                ToolTip = "Update Electrical Equipment information based on Panel Name or Mains and MCB Rating."
            };

            PulldownButton equipUpdatePulldownButton = equipPanel.AddItem(equipUpdatePulldownButtonData) as PulldownButton;
            //set ContextualHelp for PulldownButton
            equipUpdatePulldownButton.SetContextualHelp(contextHelp);

            equipUpdatePulldownButton.AddPushButton(equipUpdateData);
            equipUpdatePulldownButton.AddPushButton(equipCircuitUpdateData);

            //PushButton equipUpdateBtn = equipPanel.AddItem(equipUpdateData) as PushButton;


            //------------create push button for CreatePanelSchedules------------
            PushButtonData createPanSchedData =
                new PushButtonData(
                    "cmdCreatePanelSchedules",
                    "Create Panel" + System.Environment.NewLine + " Schedules ",
                    thisAssemblyPath,
                    "CreatePanelSchedules.CreatePanelSchedules")
                {
                    Image = new BitmapImage(new Uri(
                $"{uriPath}CreatePanelSchedules16x16.png")),
                    LargeImage = new BitmapImage(new Uri(
                $"{uriPath}CreatePanelSchedules32x32.png")),
                    ToolTip = "Create Panel Schedules for All Electrical Equipment"
                };
            PushButton createPanSchedBtn = schedulePanel.AddItem(createPanSchedData) as PushButton;
            //set ContextualHelpfor PushButton
            ContextualHelp createPanSchedContextHelp = new ContextualHelp(ContextualHelpType.Url, "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Create%20Panel%20Schedules.mov?csf=1&web=1&e=3CpYHn");
            createPanSchedBtn.SetContextualHelp(createPanSchedContextHelp);


            //------------create push button for FlipFacingOrientation------------
            PushButtonData flipFacingOrientData =
                new PushButtonData(
                    "cmdFlipFacingOrientation",
                "Flip Host" + System.Environment.NewLine + " Plane ",
                thisAssemblyPath,
                "FlipFacingOrientation.FlipFacingOrientationBySelection")
                {
                    LargeImage = new BitmapImage(new Uri(
                        $"{uriPath}FlipFacingOrientation32x32.png")),
                    Image = new BitmapImage(new Uri(
                        $"{uriPath}FlipFacingOrientation16x16.png")),
                    ToolTip = "Using a Selection Window, select multiple electrical Devices or Fixtures to Flip the Facing Orientation."
                };
            PushButton flipFacingOrientBtn = devicePanel.AddItem(flipFacingOrientData) as PushButton;
            //set ContextualHelp for PushButton
            ContextualHelp flipFacingOrientContextHelp = new ContextualHelp(ContextualHelpType.Url, "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Flip%20Lighting%20Host%20Plane.mov?csf=1&web=1&e=XeDadh");
            flipFacingOrientBtn.SetContextualHelp(flipFacingOrientContextHelp);


            //------------create push button for UpdateMotorMOCP------------
            PushButtonData motorMOCPUpdateData =
                new PushButtonData(
                    "cmdmotorMOCPUpdate",
                    "Update Motor MOCP",
                    thisAssemblyPath, "MotorMOCPUpdate.UpdateMotorMOCP")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}UpdateMotorMOCP32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}UpdateMotorMOCP16x16.png")),
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
                    LargeImage = new BitmapImage(new Uri($"{uriPath}UpdateMotorUID32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}UpdateMotorUID16x16.png")),
                    ToolTip = "Select Motors to update UID and associated Electrical Circuit Load Name from host Mechanical Equipment.",
                    LongDescription = "The tool will prompt the user to select E_EF_Motor elements in the active view to update the UID parameter " +
                "and associated Electrical Circuit Load Name parameter of any E_EF_Motor family selected that is hosted to a Mechanical Equipment family. " +
                "The tool will update E_EF_Motor values based on the Identity Mark parameter of the host Mechanical Equipment family."
                };

            //create motorPulldownButton
            PulldownButtonData motorPulldownButtonData = new PulldownButtonData("motorSplitButton", "Motor")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}UpdateMotorMOCP32x32.png")),
                ToolTip = "Update Motor Electrical Circuit Rating or Motor UID values."
            };
            PulldownButton motorPulldownButton = devicePanel.AddItem(motorPulldownButtonData) as PulldownButton;
            //set ContextualHelp for PulldownButton
            motorPulldownButton.SetContextualHelp(contextHelp);
            //add motorMOCPUpdate PushButton to motorPulldownButton SplitButton
            motorPulldownButton.AddPushButton(motorMOCPUpdateData);
            motorPulldownButton.AddPushButton(motorUIDUpdateData);


            ////------------create push button for CorrectLightFixtures------------
            //PushButtonData correctLightFixturesData = new PushButtonData("cmdCorrectLightFixtures",
            //    "Correct Light" + System.Environment.NewLine + " Fixtures ",
            //    thisAssemblyPath, "CorrectLightFixtures.CorrectLightFixtures");

            //PushButton correctLightFixturesBtn = devicePanel.AddItem(correctLightFixturesData) as PushButton;
            ////correctLightFixtures ToolTip Information
            //correctLightFixturesBtn.ToolTip = "Updates all Lighting Fixtures to be the correct families based on the loaded E_LIGHT FIXTURE SCHEDULE.";
            //correctLightFixturesBtn.LongDescription = "In October of 2022, the BIM Team updated all Lighting Fixture families" +
            //    " to use Multiline Text parameters in lieu of typical text parameters. " +
            //    "This change becomes confusing when jumping between various years of projects started " +
            //    "in various stages of the updated CMTA Midwest template. This tool will go through all " +
            //    "Lighting Fixture families in the project, compare the family parameters to the parameters " +
            //    "being used by the E_LIGHT FIXTURE SCHEDULE, and replace all Lighting Fixture families based on the referenced schedule.";
            ////correctLightFixturesBtn.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/"));
            ////correctLightFixtures ContextualHelp Information
            ////ContextualHelp correctLightFixturesHelp = new ContextualHelp(ContextualHelpType.URL, ""pack://application:,,,/CMW_Electrical;component/Resources/ToolTipImages/");
            ////correctLightFixturesBtn.SetContextualHelp(correctLightFixturesHelp);
            ////correctLightFixtures Image Information
            //BitmapImage correctLightFixturesImage = new BitmapImage(new Uri(
            //    "pack://application:,,,/CMW_Electrical;component/Resources/CorrectLightFixtures32x32.png"));
            //correctLightFixturesBtn.LargeImage = correctLightFixturesImage;
            ////equipUpdateBtn QuickAccess Image
            //correctLightFixturesBtn.Image = new BitmapImage(new Uri(
            //    "pack://application:,,,/CMW_Electrical;component/Resources/CorrectLightFixtures16x16.png"));


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
                    LargeImage = new BitmapImage(new Uri($"{uriPath}OLConnect32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}OLConnect16x16.png")),
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
                    LargeImage = new BitmapImage(new Uri($"{uriPath}OLUpdatePanelName32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}OLUpdatePanelName16x16.png")),
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
                    LargeImage = new BitmapImage(new Uri($"{uriPath}OLPlaceEquip32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}OLPlaceEquip16x16.png")),
                    ToolTip = "Select a Detail Item or Electrical Equipment family from the available list to place in your active view.",
                    LongDescription = "If you are in a Drafting View, the dialog will display Electrical Equipment that can be associated to a new element. " +
                    "Place an instance of the object selected. " +
                    "If you are in a Floor Plan View, the dialog will display Detail Items that can be associated to new elements. " +
                    "Place an instance of the object selected."
                };


            PushButtonData oneLineAssociateData =
                new PushButtonData(
                    "cmdOneLineAssociate",
                    "Associate", //"Associate Equipment + Detail Item"
                    thisAssemblyPath,
                    "OneLine_Associate.OneLineAssociate")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}OLAssociate32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}OLAssociate16x16.png")),
                    ToolTip = "Select a Detail Item or Electrical Equipment family from the available list to associate to an Electrical Equipment or Detail Item instance in your active view."
                };


            PushButtonData oneLineDrawData = new PushButtonData(
                "cmdOneLineDraw",
                "Draw", //"Draw Feeder"
                thisAssemblyPath,
                "OneLineDraw.OneLineDraw")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}OLDraw32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}OLDraw16x16.png")),
                ToolTip = "Create Detail Item Lines for Feeder Representation in OneLine Schematic views."
            };


            PushButtonData oneLineCopy = new PushButtonData(
                "cmdOneLineCopy",
                "Copy", //"Copy Components"
                thisAssemblyPath,
                "OneLineCopy.OneLineCopy")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}OLCopy32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}OLCopy16x16.png")),
                ToolTip = "Copy One Line Detail Components and assign Model Building Elements."
            };


            PushButtonData oneLineSelectData = new PushButtonData(
                "cmdOneLineSelect",
                "Select",
                thisAssemblyPath,
                "OneLineSelect.OneLineSelect")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}OLSelect32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}OLSelect16x16.png")),
                ToolTip = "Select an Detail Item or Electrical Equipment family and select the associated component in a Floor Plan View or Drafting View."
            };

            PushButtonData oneLineFindData = new PushButtonData(
                "cmdOneLineFind",
                "Find Not Associated",
                thisAssemblyPath,
                "OneLineFind.OneLineFind")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}OLFind32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}OLFind16x16.png")),
                ToolTip = "Opens a dialog for users to find unassociated Electrical Equipment and Detail Items.",
                LongDescription = "Unassociated elements that are dispalyed in the dialog can be selected. Upon selection, " +
                "the tool will jump to a view that contains these elements, select the elements, " +
                "and zoom, centered to the selected element."
            };

            PushButtonData oneLineFindDuplicatesData = new PushButtonData("cmdOneLineFindDuplicates", "Find Duplicates", thisAssemblyPath, "OneLine_FindDuplicates.OneLineFindDuplicates")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}OLFindDuplicates32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}OLFindDuplicates16x16.png")),
                ToolTip = "Opens a dialog for users to find Electrical Equipment or Detail Items that have duplicate EqConId values.",
                LongDescription = "This tool allows users to find any elements in the model that were copied and thus contain the same referencing Id for the CMW-Electrical tools. Duplicate elements will be updated through the tools and will cause problems if not resolved properly. The tool will reference Electrical Equipment if the tool is launched from a FloorPlan or 3D view, or Detail Items if launched through the OneLine Schematic View."
            };

            PushButtonData oneLineFindDisconnectedData =
                new PushButtonData(
                    "cmdOneLineFindDisconnected",
                    "Find Disconnected",
                    thisAssemblyPath,
                    "OneLine_FindDisconnected.OneLineFindDisconnected")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}OLFindDisconnected32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}OLFindDisconnected16x16.png")),
                    ToolTip = "Opens a dialog for users to find Electrical Equipment or Detail Items that have a disconnected EqConId value."
                };

            PushButtonData oneLineHalftoneExistingData =
                new PushButtonData(
                    "cmdOneLineHalftoneExisting",
                    "Halftone" + System.Environment.NewLine + " Existing ",
                    thisAssemblyPath,
                    "OneLine_HalftoneExisting.OneLineHalftoneExisting")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}OLHalftoneExisting32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}OlHalftoneExisting16x16.png")),
                    ToolTip = "Sets the Graphic Override for all Existing phased content in active Schematic Drafting View to be Halftone."
                };

            PushButtonData oneLineRemoveData =
                new PushButtonData(
                    "cmdOneLineRemove",
                    "Remove",
                    thisAssemblyPath,
                    "OneLine_Remove.OneLineRemove")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}OLRemove32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}OLRemove16x16.png")),
                    ToolTip = "Remove a selected Detail Item or Electrical Equipment family from the model.",
                    LongDescription = "The tool will prompt the user to select whether or not to keep any associated elements made through the CMW - Electrical tools."
                };

            PushButtonData oneLineClearData =
                new PushButtonData(
                    "cmdOneLineClear",
                    "Clear",
                    thisAssemblyPath,
                    "OneLine_Clear.OneLineClear")
                {
                    LargeImage = new BitmapImage(new Uri($"{uriPath}OLClear32x32.png")),
                    Image = new BitmapImage(new Uri($"{uriPath}OLClear16x16.png")),
                    ToolTip = "Clear / remove the EqConId of a selected Electrical Equipment or Detail Item family.",
                    LongDescription = "The EqConId parameter is set through the workflow of using the CMW - Electrical toolset. " +
                "If an element becomes disconnect (Detail Item or Electrical Equipment family is deleted) " +
                "then this tool can clear the existing value to prepare the family for a different connection."
                };

            List<RibbonItem> oneLineStackedItems = new List<RibbonItem>();
            oneLineStackedItems.AddRange(oneLinePanel.AddStackedItems(oneLineAssociateData, oneLinePlaceEquipData, oneLineConnectData));
            oneLineStackedItems.AddRange(oneLinePanel.AddStackedItems(oneLineUpdateDesignationsData, oneLineRemoveData, oneLineClearData));
            oneLineStackedItems.AddRange(oneLinePanel.AddStackedItems(oneLineCopy, oneLineDrawData, oneLineSelectData));

            PulldownButtonData oneLineFindButtonData = new PulldownButtonData("findSplitButton", "Find")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}OLFind32x32.png")),
                ToolTip = "Find Electrical Equipment or Detail Item instances in the model that are unassociated, duplicated, or disconnected."
            };
            PulldownButton oneLineFindButton = oneLinePanel.AddItem(oneLineFindButtonData) as PulldownButton;
            //oneLineFindButton.SetContextualHelp(contextHelp);

            oneLineFindButton.AddPushButton(oneLineFindData);
            oneLineFindButton.AddPushButton(oneLineFindDuplicatesData);
            oneLineFindButton.AddPushButton(oneLineFindDisconnectedData);

            oneLinePanel.AddSeparator();

            PushButton oneLineHalftoneExistingBtn = oneLinePanel.AddItem(oneLineHalftoneExistingData) as PushButton;
            //oneLineHalftoneExistingBtn.SetContextualHelp(contextHelp);



            ////create pushbuttons for AlignTagTools
            //PushButtonData tagAlignTopData = 
            //    new PushButtonData(
            //        "cmdTagAlignTop",
            //        "Align Top",
            //        thisAssemblyPath,
            //        "AlignTagTools.TagAlignTop")
            //{
            //        LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignTop32x32.png")),
            //        Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignTop16x16.png")),
            //        ToolTip = ""
            //};

            //PushButtonData tagAlignBottomData = 
            //    new PushButtonData(
            //        "cmdTagAlignBottom", 
            //        "Align Bottom", 
            //        thisAssemblyPath, 
            //        "AlignTagTools.TagAlignBottom")
            //{
            //        LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignBottom32x32.png")),
            //        Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignBottom16x16.png")),
            //        ToolTip = ""
            //};

            //PushButtonData tagAlignLeftData = 
            //    new PushButtonData(
            //        "cmdTagAlignLeft", 
            //        "Align Left", 
            //        thisAssemblyPath, 
            //        "AlignTagTools.TagAlignLeft")
            //{
            //    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignLeft32x32.png")),
            //    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignLeft16x16.png")),
            //    ToolTip = ""
            //};

            //PushButtonData tagAlignRightData = 
            //    new PushButtonData(
            //    "cmdTagAlignRight", 
            //    "Align Right", 
            //    thisAssemblyPath, 
            //    "AlignTagTools.TagAlignRight")
            //{
            //    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignRight32x32.png")),
            //    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignRight16x16.png")),
            //    ToolTip = ""
            //};

            //PushButtonData tagAlignTopAndCenterData = 
            //    new PushButtonData(
            //        "cmdTagAlignTopAndCenter", 
            //        "Align Top and Center",
            //        thisAssemblyPath, 
            //        "AlignTagTools.TagAlignTopAndCenter")
            //{
            //    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignTopCenter32x32.png")),
            //    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignTopCenter16x16.png")),
            //    ToolTip = ""
            //};

            //PushButtonData tagAlignBottomAndCenterData = 
            //    new PushButtonData(
            //        "cmdTagAlignBottomAndCenter", 
            //        "Align Bottom and Center",
            //        thisAssemblyPath, 
            //        "AlignTagTools.TagAlignBottomAndCenter")
            //{
            //    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignBottomCenter32x32.png")),
            //    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignBottomCenter16x16.png")),
            //    ToolTip = ""
            //};

            //PushButtonData tagAlignCenterData = 
            //    new PushButtonData(
            //        "cmdTagAlignCenter", 
            //        "Align Center", 
            //        thisAssemblyPath, 
            //        "AlignTagTools.TagAlignCenter")
            //{
            //    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignCenter32x32.png")),
            //    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/tagAlignBottomCenter16x16.png")),
            //    ToolTip = ""
            //};


            //SplitButtonData splitButtonTagAlignData = new SplitButtonData("Tag Align", "Tag Align");
            //SplitButton splitButtonTagAlign = devicePanel.AddItem(splitButtonTagAlignData) as SplitButton;
            //splitButtonTagAlign.IsSynchronizedWithCurrentItem = true;
            ////add PushButtonDatas of AlignTagTools to SplitButton
            //splitButtonTagAlign.AddPushButton(tagAlignTopData);
            //splitButtonTagAlign.AddPushButton(tagAlignBottomData);
            //splitButtonTagAlign.AddPushButton(tagAlignLeftData);
            //splitButtonTagAlign.AddPushButton(tagAlignRightData);
            //splitButtonTagAlign.AddPushButton(tagAlignCenterData);
            //splitButtonTagAlign.AddPushButton(tagAlignTopAndCenterData);
            //splitButtonTagAlign.AddPushButton(tagAlignBottomAndCenterData);


            //------------create push button for panelSchedFinalFormat------------
            PushButtonData panelSchedFinalFormatData = new PushButtonData(
                "cmdPanelSchedFinalFormat",
                "Panel Schedule Final Format",
                thisAssemblyPath,
                "PanelSchedFormatting.PanelSchedFinalFormat")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}PanelSchedFormat32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}PanelSchedFormat16x16.png")),
                ToolTip = "Moves all circuit breakers up to be aligned to the top of the active Panelboard Schedule and adds Spares to all remaining circuit breakers. NOTE: Per NEC, 20% of panelboards circuit breakers shall be Spares."
            };


            //------------create push button for panelSchedSpareAndSpaceAlign------------
            PushButtonData panelSchedSpareAndSpaceAlignData = new PushButtonData(
                "cmdPanelSchedSpareAlign",
                "Spare and Space Align",
                thisAssemblyPath,
                "PanelSchedFormatting.SpareAndSpaceAlign")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}PanelSchedSpareAlign32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}PanelSchedSpareAlign16x16.png")),
                ToolTip = "All Spares assigned to a Panelboard will have their text justified to the right of the CIRCUIT DESCRIPTION column. All Spaces assigned to a Panelboard schedule will have their text justified to the center of the CIRCUIT DESCRIPTION column."
            };


            PulldownButtonData panelSchedFormatData = new PulldownButtonData(
                "panelSchedFormatButton",
                "Panel Schedule" + System.Environment.NewLine + " Formatting")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}PanelSchedFormat32x32.png")),
                ToolTip = "Update the formatting of Panel Schedules and Circuit Breaker text justification."
            };
            PulldownButton panelSchedFormatBtn = schedulePanel.AddItem(panelSchedFormatData) as PulldownButton;
            //set ContextualHelp for PulldownButton
            panelSchedFormatBtn.SetContextualHelp(contextHelp);

            panelSchedFormatBtn.AddPushButton(panelSchedFinalFormatData);
            panelSchedFormatBtn.AddPushButton(panelSchedSpareAndSpaceAlignData);


            //------------create push button for panelLegendUpdate------------
            //PushButtonData panelLegendUpdateData = new PushButtonData(
            //    "cmdPanelLegendUpdate", 
            //    "Schedule Legend" + System.Environment.NewLine + "Autofill", 
            //    thisAssemblyPath, 
            //    "ScheduleLegendUpdate.PanelLegendUpdate")
            //{
            //    LargeImage = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/PanelLegendAutofill32x32.png")),
            //    Image = new BitmapImage(new Uri("pack://application:,,,/CMW_Electrical;component/Resources/PanelLegendAutofill16x16.png")),
            //    ToolTip = "Updates the E_GA_Schedule Legend of the current sheet.",
            //    LongDescription = "Updates the E_GA_Schedule Legend on the current sheet based on " +
            //        "the number of PanelScheduleInstances and ScheduleSheetInstances on the current sheet. " +
            //        "NOTE: If the current view is not a Sheet View, the tool will cancel."
            //};

            //PushButton panelLegendUpdateBtn = schedulePanel.AddItem(panelLegendUpdateData) as PushButton;

            //------------create push button for <button name>------------

            PushButtonData getCircuitsPlaceTextData = new PushButtonData(
                "getCircuitsPlaceTextButton", 
                "Place Typ" + System.Environment.NewLine + " Circuit Text", 
                thisAssemblyPath, 
                "GetCircuitsAndPlaceText.GetCircuitsAndPlaceText")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}GetCircuitsAndPlaceText32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}GetCircuitsAndPlaceText16x16.png")),
                ToolTip = "Create Text Notes in Selected Spaces from Electrical Circuits."
            };

            PushButton getCircuitsPlaceTextBtn = circuitPanel.AddItem(getCircuitsPlaceTextData) as PushButton;
            //set ContextualHelp for PushButton
            ContextualHelp getCircuitsPlaceTextContextHelp = new ContextualHelp(ContextualHelpType.Url,
                "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Place%20Typ%20Circuit%20Text.mov?csf=1&web=1&e=ZoPFXH");
            getCircuitsPlaceTextBtn.SetContextualHelp(getCircuitsPlaceTextContextHelp);


            PushButtonData nestedFamilyFontUpdateData = new PushButtonData(
                "updateNestedFamilyFontButton", 
                "Font" + System.Environment.NewLine + " Refresh ", 
                thisAssemblyPath, 
                "NestedFontUpdate.UpdateNestedFamilyFont")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}NestedFontUpdate32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}NestedFontUpdate16x16.png")),
                ToolTip = "Update the nested Font of a Family.",
                LongDescription = "This tool will update the nested annotation font of a selected family to be default ARchitxt. " +
                "This tool helps speed up the process of correcting families from an error that can occur after " +
                "a project is upgraded to a later version of Revit."
            };

            PushButton nestedFamilyFontUpdateBtn = devicePanel.AddItem(nestedFamilyFontUpdateData) as PushButton;
            //set ContextualHelp for PushButton
            ContextualHelp nestedFamFontContextHelp = new ContextualHelp(ContextualHelpType.Url, 
                "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Font%20Refresh.mov?csf=1&web=1&e=gQtxFP");
            nestedFamilyFontUpdateBtn.SetContextualHelp(nestedFamFontContextHelp);


            PushButtonData changeWorksetFromSelectionData = new PushButtonData(
                "changeWorksetFromSelectionButton", 
                "Make Workset" + System.Environment.NewLine + " Current ", 
                thisAssemblyPath, 
                "ChangeWorksetFromSelection.ChangeWorksetFromSelection")
            {
                LargeImage = new BitmapImage(new Uri($"{uriPath}WorksetChange32x32.png")),
                Image = new BitmapImage(new Uri($"{uriPath}WorksetChange16x16.png")),
                ToolTip = "Change active Workset to the Workset of a Selected Element."
            };

            PushButton changeWorksetFromSelectionBtn = devicePanel.AddItem(changeWorksetFromSelectionData) as PushButton;
            //set ContextualHelp for PushButton
            ContextualHelp changeWorksetFromSelectionContextHelp = new ContextualHelp(ContextualHelpType.Url, 
                "https://wearelegence.sharepoint.com/:v:/r/sites/CMTAMidwestBIM/Shared%20Documents/Video/CMW%20Electrical%20Add-in/Make%20Workset%20Current.mov?csf=1&web=1&e=xeNzYE");
            changeWorksetFromSelectionBtn.SetContextualHelp(changeWorksetFromSelectionContextHelp);
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            //add shutdown for Modeless forms

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
