namespace TRIM_Helper.Model
{
    /// <summary>
    /// Describe one chemical Element for Layer Compound
    /// </summary>
    public class Element
    {
        public byte Z;
        public double Mass = 0.0;
        
        public string Name;
        public double Stoich;
        public double E_d;
        public double lattice;
        public double surface;

        public int LayerIndex = 0;
    }
}
