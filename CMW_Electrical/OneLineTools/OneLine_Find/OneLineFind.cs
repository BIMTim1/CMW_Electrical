using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Selection;
using OLUpdateInfo;
using Autodesk.Revit.DB.Electrical;
using OneLineTools;
using CMW_Electrical;
using Autodesk.Revit.UI.Events;
using System.Diagnostics;

namespace OneLineFind
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OneLineFind : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string errorReport, ElementSet elementSet)
        {
            //define background Revit information to reference
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //check for EqConId Current Value parameter
            EqConIdCheck eqConIdCheck = new EqConIdCheck();
            bool eqConIdExists = eqConIdCheck.EqConIdCurrentValueExists(doc);

            if (!eqConIdExists)
            {
                errorReport = 
                    "The EqConId Current Value parameter does not exist in the current Document. Contact the BIM team for assistance.";

                return Result.Failed;
            }

            string schedName = "E_Working_Unassociated Electrical Schematic Elements";
            string commentsValue = "Schematic Element not Associated";

            using (Transaction trac = new Transaction(doc))
            {
                try
                {
                    trac.Start("CMWElec-Find Unassociated Elements");

                    //collect Electrical Equipment and Detail Item elements that are not associated to a Schematic item
                    List<BuiltInCategory> mCats = new List<BuiltInCategory>()
                    {
                        BuiltInCategory.OST_ElectricalEquipment,
                        BuiltInCategory.OST_DetailComponents
                    };

                    //create ElementMulticategoryFilter to be used by FilteredElementCollectors
                    ElementMulticategoryFilter mCatFilter = new ElementMulticategoryFilter(mCats);

                    //clear Comments parameter of items that may have not been assigned before
                    List<Element> clearList = 
                        new FilteredElementCollector(doc)
                        .WherePasses(mCatFilter)
                        .ToElements()
                        .Where(x => x.LookupParameter("EqConId").AsString().Contains("EqId") && 
                        x.LookupParameter("Comments").AsString() == commentsValue)
                        .ToList();

                    if (clearList.Any())
                    {
                        foreach (Element e in clearList)
                        {
                            e.LookupParameter("Comments").Set("");
                        }
                    }

                    List<Element> elems = 
                        new FilteredElementCollector(doc)
                        .WherePasses(mCatFilter)
                        .ToElements()
                        .Where(x=>x.LookupParameter("EqConId").AsString() == "not assigned" || 
                        x.LookupParameter("EqConId").AsString() == null)
                        .ToList();

                    if (!elems.Any())
                    {
                        errorReport = "There are no unassigned Electrical Equipment or Detail Item families. The tool will now cancel.";
                        elementSet.Insert(doc.ActiveView);

                        return Result.Cancelled;
                    }

                    foreach (Element e in elems)
                    {
                        e.LookupParameter("Comments").Set(commentsValue);
                    }

                    //get Multi-Category schedule or create if not already created
                    ViewSchedule mCatSched;

                    List<Element> schedCollector =
                        new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewSchedule))
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .Where(x => x.LookupParameter("Name").AsString() == schedName)
                        .ToList();

                    if (!schedCollector.Any())
                    {
                        mCatSched = CreateSchedule(doc, schedName, commentsValue);
                    }
                    else
                    {
                        mCatSched = schedCollector.First() as ViewSchedule;
                    }

                    //change ActiveView to ViewSchedule
                    uidoc.RequestViewChange(mCatSched);

                    trac.Commit();

                    return Result.Succeeded;
                }

                catch (Exception ex)
                {
                    errorReport = ex.Message;

                    return Result.Failed;
                }
            }
        }

        #region CreateSchedule
        /// <summary>
        /// Create the Multi-Category schedule for the user
        /// to easily find elements that are not associated
        /// between Electrical Equipment and Detail Items
        /// for use in connected information between the 
        /// One Line diagram and model content.
        /// </summary>
        internal ViewSchedule CreateSchedule(Document document, string scheduleName, string commentsVal)
        {
            ViewSchedule sched = 
                ViewSchedule.CreateSchedule(document, ElementId.InvalidElementId);
            sched.Name = scheduleName;

            //add parameters to schedule
            ScheduleDefinition schedDef = sched.Definition;

            ScheduleFilter schedFil = new ScheduleFilter();

            //collect all ScheduleFields that could be added to the schedule
            IList<SchedulableField> allFields = schedDef.GetSchedulableFields();

            ScheduleField commentsField = null;
            ScheduleField famTypeField = null;
            ScheduleField panNameField = null;
            ScheduleField panNameDetField = null;

            foreach (SchedulableField f in allFields)
            {
                string fName = f.GetName(document);

                if (fName == "Comments")
                {
                    commentsField = schedDef.AddField(f.FieldType, f.ParameterId);

                    //create ScheduleFilter for Comments parameter
                    ScheduleFieldId commentsId = commentsField.FieldId;
                    schedFil = new ScheduleFilter(commentsId, ScheduleFilterType.Contains, commentsVal);
                }
                else if (fName == "Family and Type")
                {
                    famTypeField = schedDef.AddField(f.FieldType, f.ParameterId);
                }
                else if (fName == "Panel Name")
                {
                    panNameField = schedDef.AddField(f.FieldType, f.ParameterId);
                }
                else if (fName == "Panel Name - Detail")
                {
                    panNameDetField = schedDef.AddField(f.FieldType, f.ParameterId);
                }
            }

            //sort added parameters in schedule
            IList<ScheduleFieldId> sortFieldList = new List<ScheduleFieldId>()
            {
                famTypeField.FieldId,
                panNameField.FieldId,
                panNameDetField.FieldId,
                commentsField.FieldId
            };

            schedDef.SetFieldOrder(sortFieldList);

            //add filter to schedule
            schedDef.AddFilter(schedFil);

            //set View Classification, Discipline, and Sub-Discipline parameters of newly created schedule
            sched.LookupParameter("View Classification").Set("Working");
            sched.LookupParameter("Sub-Discipline").Set("Power");
            sched.LookupParameter("Discipline").Set("Electrical");

            return sched;
        }
        #endregion // CreateSchedule
    }
}
