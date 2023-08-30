using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            List<string> allFixtureFamilyNames = (from fixture in 
                                                      new FilteredElementCollector(doc)
                                                      .OfClass(typeof(Family))
                                                      .Cast<Family>()
                                                      .Where(x => x.FamilyCategory.Name.ToString() == "Lighting Fixture")
                                                      .ToList() 
                                                  select fixture.Name.ToString())
                                                  .ToList();

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
                    sameParameters, doc, OLD_PATH, NEW_PATH);

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
        public bool FindFieldByName(ScheduleDefinition schedDef, string fieldName, Document document)
        {
            bool returnBool = false;

            try
            {
                IList<ScheduleFieldId> fieldIds = schedDef.GetFieldOrder();

                foreach (ScheduleFieldId fieldId in fieldIds)
                {
                    if (schedDef.GetField(fieldId).GetSchedulableField().GetName(document) == fieldName)
                    {
                        returnBool = true;
                    }
                }
            }
            catch
            {
                returnBool = false;
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
        public void UpdateFixtureInfo(
            string updateTypeString, BuiltInCategory constLtgCat, string schedParamTest, 
            List<string> allLightingFamilyNames, List<ElementId> familiesToBeDeleted, 
            List<string> newStringParameters, List<string> oldStringParameters, 
            List<string> sameParameters, Document document, string oldPath, string newPath)
        {
            //collect all instances of Lighting Fixtures in project
            List<FamilyInstance> allLightingFamilies = 
                new FilteredElementCollector(document)
                .OfCategory(constLtgCat)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            //filter list of all fixtures if they contain or do not contain the test parameter
            List<FamilyInstance> filteredLightingFamilies = 
                (updateTypeString == "old" ? 
                from lf in allLightingFamilies 
                where lf.Symbol.LookupParameter(schedParamTest) != null 
                select lf : 
                from lf in allLightingFamilies 
                where lf.Symbol.LookupParameter(schedParamTest) == null 
                select lf)
                .ToList();

            foreach (FamilyInstance fixture in filteredLightingFamilies)
            {
                //collect FamilyInstance information of current Lighting Fixture
                FamilySymbol fixtureType = fixture.Symbol;
                string fixtureInfo = fixture.LookupParameter("Family and Type").AsValueString();

                //create variables to be used throughout function
                string familyNameTest;
                string familyPath;
                bool deleteDefaultSymbols = false;

                //create references to Lighting Fixture FamilySymbol to be loaded into project and updated
                if (updateTypeString == "old")
                {
                    familyNameTest = $"x{fixtureType.Family.Name}";
                    familyPath = $"{oldPath}/{familyNameTest}.rfa";
                }
                else
                {
                    familyNameTest = fixtureType.Family.Name.Substring(1);
                    familyPath = $"{newPath}/{familyNameTest}.rfa";
                }

                //from selected folder locations, load families into active project
                if (!familyNameTest.Contains(familyNameTest))
                {
                    document.LoadFamily(familyPath);

                    allLightingFamilyNames.Add(familyNameTest);
                    deleteDefaultSymbols = true;
                }

                //delete incorrect FamilySymbol (Type) from project
                if (deleteDefaultSymbols)
                {
                    List<ElementId> symbolsToDelete = 
                        new FilteredElementCollector(document)
                        .OfClass(typeof(FamilySymbol))
                        .ToElements()
                        .Where(x => x.LookupParameter("Family Name").AsString() == 
                        familyNameTest && !x.LookupParameter("Type Name").AsString().Contains("Standard"))
                        .Cast<ElementId>()
                        .ToList();

                    foreach (ElementId elemId in symbolsToDelete)
                    {
                        document.Delete(elemId);
                    }
                }

                //collect FamilySymbol element to modify
                FamilySymbol modifyFamilySymbol =
                    new FilteredElementCollector(document)
                    .OfClass(typeof(FamilySymbol))
                    .Where(x => x.LookupParameter("Family Name").AsString() == 
                    familyNameTest && x.LookupParameter("Type Name").AsString().Contains("Standard"))
                    .Cast<FamilySymbol>()
                    .First();

                //collect id of collected FamilySymbol
                ElementId incorrectFamily = fixtureType.Family.Id;

                if (!familiesToBeDeleted.Contains(incorrectFamily))
                {
                    familiesToBeDeleted.Add(incorrectFamily);
                }

                //duplicate FamilySymbol from incorrect FamilySymbol
                FamilySymbol newFixtureSymbol = modifyFamilySymbol.Duplicate(fixtureType.LookupParameter("Type Name").AsString()) as FamilySymbol;

                //Update all parameter information of duplicated FamilySymbol
                UpdateParameterInfo(updateTypeString, fixtureType, newFixtureSymbol, newStringParameters, oldStringParameters, sameParameters);

                UpdateDimensionParameters(fixtureType, newFixtureSymbol);

                //collect all FamilyInstances of the current Family and Type
                List<FamilyInstance> replaceAllTypesOf = 
                    new FilteredElementCollector(document)
                    .OfCategory(constLtgCat)
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Where(x => x.LookupParameter("Family and Type").AsValueString() == fixtureInfo)
                    .ToList();

                //replace all fixtures of same Family and Type to same FamilySymbol
                foreach (FamilyInstance famInst in replaceAllTypesOf)
                {
                    famInst.LookupParameter("Type").Set(newFixtureSymbol.Id);
                }
            }
        }
    }
}
