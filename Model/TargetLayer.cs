namespace TRIM_Helper.Model
{
    public class TargetLayer
    {
        public string Description { get; set; }
        public double CompoundCorrection { get; set; }
        /// <summary>
        /// Layer compound state
        /// (0 - solid, 1 - gas)
        /// </summary>
        public int Phase { get; set; }
        public double Depth = 1000;    //Angstrom
        /// <summary>
        /// Layer density
        /// > 0.0
        /// </summary>
        public double Density;

    }
}
