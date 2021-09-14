using System.Collections.ObjectModel;
using TRIM_Helper.Model.LayerModel.MaterialModel;

namespace TRIM_Helper.Model.LayerModel
{
    public class TargetLayer
    {
        public string LayerName { get; set; } = "New layer";
        public string Description { get; set; }
        public double CompoundCorrection { get; set; } = 1.0;
        /// <summary>
        /// Layer compound state
        /// (0 - solid, 1 - gas)
        /// </summary>
        public bool IsGas { get; set; } = false;
        public double Depth { get; set; } = 1000;    //Angstrom
        /// <summary>
        /// Layer density
        /// > 0.0
        /// </summary>
        public double Density { get; set; } = 1.0;

        public ObservableCollection<Element> Elements;

        public TargetLayer()
        {
            Elements = new ObservableCollection<Element>();
        }
    }
}
