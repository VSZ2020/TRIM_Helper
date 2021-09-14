using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRIM_Helper.Model
{
    public class IonTarget
    {
        /// <summary>
        /// X-size
        /// </summary>
        public ValueRange Depth;
        /// <summary>
        /// Y-size
        /// </summary>
        public ValueRange Width;
        /// <summary>
        /// Z-size
        /// </summary>
        public ValueRange Height;

        public IonTarget()
        {

        }
    }
}
