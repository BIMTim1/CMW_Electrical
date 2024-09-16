using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMW_Electrical
{
    public class ViewCollector
    {
        public List<View> GetOpenViews(Document document)
        {
            List<View> openViews = new List<View>();

            UIDocument uiDoc = new UIDocument(document);
            IList<UIView> uiViews = uiDoc.GetOpenUIViews();

            foreach (UIView v in uiViews)
            {
                View view = document.GetElement(v.ViewId) as View;

                openViews.Add(view);
            }

            return openViews;
        }
    }
}
