﻿using Autodesk.Revit.Attributes;
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

            Transaction trac = new Transaction(doc);

            return Result.Succeeded;
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
    }
}
