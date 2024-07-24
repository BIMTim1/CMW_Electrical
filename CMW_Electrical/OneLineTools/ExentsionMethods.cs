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
using System.Runtime.Remoting.Messaging;

namespace OneLineTools
{
    internal static class ExentsionMethods
    {
        private static IEnumerable<View> FindAllViewsThatCanDisplayElements(this Document doc)
        {
            var filter = new ElementMulticlassFilter(new List<Type>
            {
                typeof(View3D),
                typeof(ViewPlan),
                typeof(ViewSection),
                typeof(ViewDrafting)
            });

            return new FilteredElementCollector(doc).WherePasses(filter).Cast<View>().Where(v => !v.IsTemplate);
        }

        public static IEnumerable<View> FindAllViewsWhereAllElementsVisible(this Element element)
        {
            if (null == element) throw new ArgumentNullException("elements");

            var e1 = element;

            if (null == e1) return new List<View>();

            var doc = e1.Document;

            var relevantViewList = doc.FindAllViewsThatCanDisplayElements();

            //var idsToCheck = from e in element select e.Id;
            List<ElementId> idsToCheck = new List<ElementId>
            {
                e1.Id
            };

            return from v 
                   in relevantViewList 
                   let idList = 
                   new FilteredElementCollector(doc, v.Id)
                   .WhereElementIsNotElementType()
                   .ToElementIds() 
                   where !idsToCheck.Except(idList).Any() 
                   select v;
        }

        public static bool IsElementVisibleInView(this View view, Element el)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));

            if (el == null) throw new ArgumentNullException(nameof(el));

            // Get the Document that contains the element
            var doc = el.Document;

            var elId = el.Id;

            var idRule = ParameterFilterRuleFactory
                .CreateEqualsRule(
                new ElementId(BuiltInParameter.ID_PARAM), 
                elId);

            var idFilter = new ElementParameterFilter(idRule);

            // Use an ElementCategoryFilter to speed up the
            // search, as ElementParameterFilter is a slow filter
            var cat = el.Category;
            var catFilter = new ElementCategoryFilter(cat.Id);

            var collector = new FilteredElementCollector(doc, view.Id).WhereElementIsNotElementType().WherePasses(catFilter).WherePasses(idFilter);

            // If the collector contains any items, then
            // we know that hte elemnt is visible in the
            // given view

            return collector.Any();
        }
    }
}
