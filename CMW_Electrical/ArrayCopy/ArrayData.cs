using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMW_Electrical.ArrayCopy
{
    public class ArrayData
    {
        private string _X;
        private string _Y;
        private string _XDist;
        private string _YDist;
        public ArrayData()
        {
            X = "0";
            Y = "0";
            XDist = "0";
            YDist = "0";
        }
        /// <summary>
        /// String value of X to be converted / validated to double.
        /// Only changed in ArrayInfoWindow WPF. Do not adjust outside of ArrayInfoWindow WPF.
        /// </summary>
        public string X
        {
            get { return _X; }
            set
            {
                _X = value;
            }
        }
        public string XDist
        {
            get { return _XDist; }
            set
            {
                _XDist = value;
            }
        }
        public string Y
        {
            get { return _Y; }
            set
            {
                _Y = value;
            }
        }
        public string YDist
        {
            get { return _YDist; }
            set
            {
                _YDist = value;
            }
        }
        public double XQtyAsDouble
        {
            get
            {
                if (double.TryParse(X, out double val))
                {
                    return val;
                }
                else
                {
                    return 0;
                }
            }
        }
        public double YQtyAsDouble
        {
            get
            {
                if (double.TryParse(Y, out double val))
                {
                    return val;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double XDistAsDouble
        {
            get
            {
                if (double.TryParse(XDist, out double val))
                {
                    return val;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double YDistAsDouble
        {
            get
            {
                if (double.TryParse(YDist, out double val))
                {
                    return val;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
