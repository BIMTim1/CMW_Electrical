using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace OneLine_FindDuplicates
{
    public class DuplicateElementData
    {
        private FamilyInstance _faminst;
        private Parameter _elemConId;
        private string _isDuplicate;

        public DuplicateElementData(Element elem)
        {
            _faminst = elem as FamilyInstance;
            _elemConId = _faminst.LookupParameter("EqConId");
            _isDuplicate = "Duplicate Element Id";
        }

        /// <summary>
        /// Get the EqConId assigned to the current element
        /// </summary>
        public string GetEqConId
        {
            get { return _elemConId.AsString(); }
        }

        /// <summary>
        /// Get Duplicate string informational data for form.
        /// </summary>
        public string GetDuplicateInfo
        {
            get { return _isDuplicate; }
        }
    }
}
