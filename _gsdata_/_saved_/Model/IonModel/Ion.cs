using TRIM_Helper.Model.LayerModel.MaterialModel;
namespace TRIM_Helper.Model
{
    public class Ion: Element
    {
        /// <summary>
        /// Ion Energy in KeV
        /// </summary>
        public double Energy = 1000;

        public double Angle = 0;

        /// <summary>
        /// Property for Ion
        /// </summary>
        public double BraggCorr = 0.0;
        public Ion()
        {
            
        }

        public static Ion GetIonFromElement(Element el, double Energy = 1000, double angle = 0, double braggCoeff = 1.0)
		{
            Ion ion = new Ion();
            ion.Z = el.Z;
            ion.Mass = el.Mass;
            ion.Stoich = el.Stoich;
            ion.ElementName = el.ElementName;
            ion.lattice = el.lattice;
            ion.surface = el.surface;
            ion.E_d = el.E_d;

            ion.Energy = Energy;
            ion.Angle = angle;
            ion.BraggCorr = braggCoeff;

            return ion;
		}
    }
}
