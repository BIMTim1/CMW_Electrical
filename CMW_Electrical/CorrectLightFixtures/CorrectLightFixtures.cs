using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.CodeDom;

namespace CorrectLightFixtures
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    internal class CorrectLightFixtures : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //collect default file information
            BuiltInCategory LTGBIC = BuiltInCategory.OST_LightingFixtures;

            const string OLD_PATH = "V:/Content/CMS/Lighting Fixtures/221019_Old Lighting Fixtures";
            const string NEW_PATH = "V:/Content/CMS/Lighting Fixtures";

            const string PARAM_TEST = "Light Fixture Schedule_ML-Manufacturer";

            List<ElementId> familiesToDelete = new List<ElementId>();

            ElementClassFilter elemFilter = new ElementClassFilter(typeof(FamilyInstance));

            //default parameters
            List<string> new_string_parameters = new List<string>()
            {
                "Light Fixture Schedule_ML-Manufacturer", "Light Fixture Schedule_ML-Series",
                "Light Fixture Schedule_ML-CRI", "Light Fixture Schedule_ML-Lumens",
                "Light Fixture Schedule_ML-Dimming", "Light Fixture Schedule_ML-Mounting",
                "Light Fixture Schedule_ML-Finish", "Light Fixture Schedule_ML-Dimensions"
            };

            List<string> old_string_parameters = new List<string>()
            {
                "Light Fixture Schedule_Manufacturer", "Light Fixture Schedule_Series",
                "Light Fixture Schedule_Lamp CRI", "Light Fixture Schedule_Lamp Lumens",
                "Light Fixture Schedule_Ballast Starting Notes", "Light Fixture Schedule_Mounting",
                "Light Fixture Schedule_Finish", "Light Fixture Schedule_Dimensions"
            };

            List<string> sameParameters = new List<string>()
            {
                "Light Fixture Schedule_Type", "Light Fixture Schedule_Lamp CCT Kelvin",
                "Description", "Type Comments", "Light Fixture Schedule_Voltage",
                "Light Fixture Schedule_Apparent Load", "Light Fixture Schedule_Number of Poles"
            };

            Dictionary<string, string> updatedNames = new Dictionary<string, string>()
            {
                {"E_LF_Surface-Track", "E_LF_Track"},
                {"E_LF_Recessed Downlight-Round", "xE_LF_Recessed Can"},
                {"E_LF_Surface Downlight-Round", "xE_LF_Surface Can"},
                {"E_LF_Recessed-Perimeter", "xE_LF_Perimeter"},
                {"E_LF_Emergency Battery_Wall", "xE_LF_Emergency Battery_Wall Half Scale"},
                {"E_LF_Exit Sign_Emergency_Wall", "xE_LF_Exit Sign_Emergency_Wall_Half Scale"},
                {"E_LF_Exit Sign_Wall", "xE_LF_Exit Sign_Wall_Half Scale"},
                {"E_LF_Exit Sign_Wall_End Mounted", "xE_LF_Exit Sign_Wall_Edge Mounted" }
            };

            //collect all Lighting Fixure names
            List<Family> allFixtureFamilies = 
                new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(x => x.FamilyCategory.Name == "Lighting Fixtures")
                .ToList();

            List<string> allFixtureFamilyNames = (from fixture in allFixtureFamilies select fixture.Name.ToString()).ToList();

            //collect schedule if name contains LIGHT FIXTURE SCHEDULE
            List<Element> lightingSchedules = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSchedule))
                .ToElements()
                .Where(x => x.Name != null && x.Name.ToString().Contains("LIGHT FIXTURE SCHEDULE"))
                .ToList();

            //test if E_LIGHT FIXTURE SCHEDULE contains Multiline Text parameters
            bool scheduleType = FindFieldByName((lightingSchedules.First() as ViewSchedule).Definition, PARAM_TEST, doc);

            Transaction trac = new Transaction(doc);

            try
            {
                string updateType;
                string transactionName;

                if (!scheduleType)
                {
                    updateType = "old";
                    transactionName = "Convert New Fixtures to Old";
                }
                else
                {
                    updateType = "new";
                    transactionName = "Convert Old Fixtures to New";
                }

                trac.Start(transactionName);

                UpdateFixtureInfo(
                    updateType, LTGBIC, PARAM_TEST, allFixtureFamilyNames,
                    familiesToDelete, new_string_parameters, old_string_parameters,
                    sameParameters, doc, OLD_PATH, NEW_PATH, updatedNames, elemFilter);

                //delete incorrect families from project
                foreach (ElementId elemId in familiesToDelete)
                {
                    doc.Delete(elemId);
                }

                trac.Commit();

                TaskDialog.Show("Lighting Fixtures Updated", "All Lighting Fixtures in the Project have been Corrected.");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("An error occurred", "An error has occurred. Contact the BIM team for assistance.");
                return Result.Failed;
            }
        }

        /// <summary>
        /// Collects parameter by name from selected Revit schedule.
        /// </summary>
        ///
        public bool FindFieldByName(ScheduleDefinition schedDef, string paramTest, Document document)
        {
            bool returnBool = false;

            IList<ScheduleFieldId> fieldIds = schedDef.GetFieldOrder();

            foreach (ScheduleFieldId fieldId in fieldIds)
            {
                ScheduleField field = schedDef.GetField(fieldId);

                if (!field.IsCalculatedField && !field.IsCombinedParameterField)
                {
                    if (field.GetSchedulableField().GetName(document) == paramTest)
                    {
                        returnBool = true;
                    }
                }
            }

            return returnBool;
        }

        /// <summary>
        /// Creates a dictionary of parameter lists based on which parameters are to be used in the project.
        /// Dictionary is used to update parameters of all Lighting Fixture FamilySymbols.
        /// </summary>
        /// 
        public void UpdateParameterInfo(string updateInfo, FamilySymbol oldFixtureType, 
            FamilySymbol newFixtureType, List<string> newParams, 
            List<string> oldParams, List<string> sameParams)
        {
            List<string[]> zippedList = new List<string[]>();
            Dictionary<string, string> paramDict = new Dictionary<string, string>();

            if (updateInfo == "old")
            {
                //zippedList = oldParams.Zip(newParams, (oldP, newP) => new[] { oldP, newP }).ToList();
                paramDict = oldParams.Zip(newParams, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                paramDict = newParams.Zip(oldParams, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
            }

            foreach (KeyValuePair<string, string> param in paramDict)
            {
                newFixtureType.LookupParameter(param.Key).Set(oldFixtureType.LookupParameter(param.Value).AsString());
            }

            foreach (string param in sameParams)
            {
                Parameter setParam = newFixtureType.LookupParameter(param);

                switch (setParam.StorageType)
                {
                    case StorageType.String:
                        setParam.Set(oldFixtureType.LookupParameter(param).AsString());
                        break;

                    case StorageType.Double:
                        setParam.Set(oldFixtureType.LookupParameter(param).AsDouble());
                        break;

                    case StorageType.Integer:
                        setParam.Set(oldFixtureType.LookupParameter(param).AsInteger());
                        break;
                }
            }
        }

        /// <summary>
        /// Collects all parameters of BuiltInParameterGroup.PG_GEOMETRY (Dimensions)
        /// to be updated in new Lighting Fixture FamilySymbol
        /// </summary>
        /// 
        public void UpdateDimensionParameters(FamilySymbol oldFixtureType, FamilySymbol newFixtureType)
        {
            //collect Dimension category parameters from incorrect fixture
            ParameterSet typeParams = oldFixtureType.Parameters;

            List<string> dimensionParams = new List<string>();

            foreach (Parameter param in typeParams)
            {
                if (param.Definition.ParameterGroup == 
                    BuiltInParameterGroup.PG_GEOMETRY && 
                    param.Definition.Name.ToString().Contains("Light Fixture Schedule") && 
                    !param.Definition.Name.ToString().Contains("Dimension"))
                {
                    dimensionParams.Add(param.Definition.Name.ToString());
                }
            }

            //update dimension parameters of updated Lighting Fixture FamilySymbol
            foreach (string paramName in dimensionParams)
            {
                newFixtureType.LookupParameter(paramName).Set(oldFixtureType.LookupParameter(paramName).AsDouble());
            }
        }

        /// <summary>
        /// Main project function. Collects FamilyInstance and FamilySymbol information to update incorrect Lighting Fixture Family Types in project.
        /// Collects information, and duplicates FamilySymbol information,
        /// then replaces all FamilySymbol types.
        /// </summary>
        /// <param name="updateTypeString">String to reference which type of updating will occur.</param>
        /// <param name="constLtgCat">BuiltInCategory reference for Lighting Fixtures.</param>
        /// <param name="schedParamTest">String to test parameter exists in E_LIGHT FIXTURE SCHEDULE</param>
        /// <param name="allLightingFamilyNames">List of strings of all unique Lighting Fixtures in project.</param>
        /// <param name="familiesToBeDeleted">List of ElementIds to be used in deletion from project after correct fixtures are loaded into project.</param>
        /// <param name="newStringParameters">List of strings of StorageType.String parameters to update.</param>
        /// <param name="oldStringParameters">List of strings of StorageType.String parameters from incorrect fixtures.</param>
        /// <param name="sameParameters">List of strings of parameters with various StorageTypes.</param>
        /// <param name="document">Active Document of project</param>
        /// <param name="newPath">File path of current Lighting Fixture families.</param>
        /// <param name="oldPath">File path of prior Lighting Fixture families.</param>
        /// <param name="updatedNameDict">Dictionary of names that would not match due to CMS updates.</param>
        public void UpdateFixtureInfo(
            string updateTypeString, BuiltInCategory constLtgCat, string schedParamTest, 
            List<string> allLightingFamilyNames, List<ElementId> familiesToBeDeleted, 
            List<string> newStringParameters, List<string> oldStringParameters, 
            List<string> sameParameters, Document document, string oldPath, string newPath, Dictionary<string, string> updatedNameDict, ElementClassFilter elem_filter)
        {
            //collect all Lighting Fixture FamilySymbols in project
            List<FamilySymbol> allLightingFamilies = 
                new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(constLtgCat)
                .Cast<FamilySymbol>()
                .ToList();

            //filter list of all fixtures if they contain or do not contain the test parameter
            List<FamilySymbol> filteredLightingFamilies = 
                (updateTypeString == "old" ? 
                from lf in allLightingFamilies 
                where lf.LookupParameter(schedParamTest) != null 
                select lf : 
                from lf in allLightingFamilies 
                where lf.LookupParameter(schedParamTest) == null 
                select lf)
                .ToList();

            //further update filteredLightingFamilies for FamilySymbols in project that have instances
            filteredLightingFamilies = (from lf in filteredLightingFamilies 
                                        where lf.GetDependentElements(elem_filter).Count() > 0 
                                        select lf)
                                        .ToList();

            foreach (FamilySymbol fixtureType in filteredLightingFamilies)
            {
                //create variables to be used throughout function
                string familyNameTest = fixtureType.Family.Name;
                string filePath = "";
                bool deleteDefaultSymbols = false;

                //iterate through Dictionary <key, value> pairs to see if
                //current Family DIName is part of Dictionary
                //update familyNameTest appropriately
                if (updateTypeString == "old")
                {
                    if (updatedNameDict.ContainsKey(familyNameTest))
                    {
                        updatedNameDict.TryGetValue(familyNameTest, out familyNameTest);
                    }
                    else
                    {
                        familyNameTest = $"x{familyNameTest}";
                    }

                    filePath = oldPath;
                }
                else
                {
                    if (!familyNameTest.StartsWith("x"))
                    {
                        fixtureType.Family.Name = $"x{familyNameTest}";
                        document.Regenerate();
                        //no need to update familyNameTest here as it will already be correct
                    }
                    else
                    {
                        familyNameTest = familyNameTest.Substring(1);
                    }

                    //test to see if familyNameTest is a value from updateNameDict
                    if (updatedNameDict.ContainsValue($"x{familyNameTest}"))
                    {
                        foreach (KeyValuePair<string, string> fName in updatedNameDict)
                        {
                            if (fName.Value == $"x{familyNameTest}")
                            {
                                familyNameTest = fName.Key;
                            }
                        }
                    }

                    filePath = newPath;
                }

                string familyPath = $"{filePath}/{familyNameTest}.rfa";
                bool IsValidPath = File.Exists(familyPath);

                if (!IsValidPath)
                {
                    break; //sad face
                }

                //from selected folder locations, load families into active project
                if (!allLightingFamilyNames.Contains(familyNameTest))
                {
                    document.LoadFamily(familyPath);

                    allLightingFamilyNames.Add(familyNameTest);
                    deleteDefaultSymbols = true;
                }

                //delete FamilySymbols of recently loaded family from project
                if (deleteDefaultSymbols)
                {
                    List<ElementId> symbolsToDelete = (from elem in
                        new FilteredElementCollector(document)
                        .OfClass(typeof(FamilySymbol))
                        .Where(x => x.LookupParameter("Family Name").AsString() ==
                        familyNameTest && !x.LookupParameter("Type Name").AsString().Contains("Standard"))
                        .ToList() select elem.Id).ToList();

                    foreach (ElementId elemId in symbolsToDelete)
                    {
                        document.Delete(elemId);
                    }
                }

                //collect FamilySymbol element to modify
                //FamilySymbol Family DIName should match familyNameTest variable,
                //and Type DIName should contain "Standard"
                //to duplicate from 'clean slate'.
                List<FamilySymbol> modifyFamilySymbols = 
                    new FilteredElementCollector(document)
                    .OfClass(typeof(FamilySymbol))
                    .ToElements()
                    .Where(x => x.LookupParameter("Family Name").AsString() == familyNameTest)
                    .Cast<FamilySymbol>()
                    .ToList();

                List<string> modifySymbolNames = (from x in modifyFamilySymbols 
                                                  select x.LookupParameter("Type Name").AsString())
                                                  .ToList();

                string typeName = fixtureType.LookupParameter("Type Name").AsString();
                FamilySymbol new_fixture_symbol = null;


                //if typeName already exists use that FamilySymbol
                //otherwise duplicate and use typeName
                if (modifySymbolNames.Contains(typeName))
                {
                    new_fixture_symbol = (from x in modifyFamilySymbols 
                                          where x.LookupParameter("Type Name").AsString() == typeName 
                                          select x)
                                          .First();
                }
                else
                {
                    new_fixture_symbol = modifyFamilySymbols.First().Duplicate(typeName) as FamilySymbol;
                }

                //collect id of collected FamilySymbol
                ElementId incorrectFamily = fixtureType.Family.Id;

                if (!familiesToBeDeleted.Contains(incorrectFamily))
                {
                    familiesToBeDeleted.Add(incorrectFamily);
                }

                //Update all parameter information of duplicated FamilySymbol
                UpdateParameterInfo(updateTypeString, fixtureType, new_fixture_symbol, newStringParameters, oldStringParameters, sameParameters);

                UpdateDimensionParameters(fixtureType, new_fixture_symbol);

                //replace all FamilyInstances of old FamilySymbol
                List<FamilyInstance> famInstances = (from fam in fixtureType.GetDependentElements(elem_filter) 
                                              select document.GetElement(fam) as FamilyInstance)
                                              .ToList();

                if (famInstances.Count() > 0)
                {
                    foreach (FamilyInstance famInst in famInstances)
                    {
                        famInst.LookupParameter("Type").Set(new_fixture_symbol.Id);
                    }
                }

                document.Regenerate();
            }
        }
    }
}
