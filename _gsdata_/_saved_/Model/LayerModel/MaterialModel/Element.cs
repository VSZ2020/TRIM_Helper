namespace TRIM_Helper.Model.LayerModel.MaterialModel
{
    /// <summary>
    /// Describe one chemical Element for Layer Compound
    /// </summary>
    public class Element
    {
        public byte Z { get; set; } = 1;
        public double Mass { get; set; } = 1.0;
        
        public string ElementName { get; set; }
        public double Stoich { get; set; } = 1.0;
        public double E_d;
        public double lattice;
        public double surface;
    }
}
