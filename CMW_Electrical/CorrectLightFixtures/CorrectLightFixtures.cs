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

            List<string> same_double_parameters = new List<string>()
            {
                "Light Fixture Schedule_Voltage", "Light Fixture Schedule_Apparent Load"
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

        public void UpdateParameterInfo(string updateInfo, FamilySymbol oldFixtureType, 
            FamilySymbol newFixtureType, List<string> newParams, 
            List<string> oldParams, List<string> stringParams, List<string> integerParams)
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

            foreach (string param in stringParams)
            {
                newFixtureType.LookupParameter(param).Set(oldFixtureType.LookupParameter(param).ToString());
            }
            
            foreach (string param in integerParams)
            {
                newFixtureType.LookupParameter(param).Set(oldFixtureType.LookupParameter(param).AsDouble());
            }
        }
    }
}
