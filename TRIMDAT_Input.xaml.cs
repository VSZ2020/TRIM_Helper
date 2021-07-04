using System.Windows;
using TRIM_Helper.Model;

namespace TRIM_Helper
{
    /// <summary>
    /// Логика взаимодействия для TRIMDAT_Input.xaml
    /// </summary>
    public partial class TRIMDAT_Input : Window
    {
        public TRIMDAT_Input()
        {
            InitializeComponent();
        }

        public Ion ReadIonInputs()
        {
            Element elem = cmbIonsList.SelectedItem as Element;
            if (elem == null) throw new System.Exception("No element was selected");

            Ion ion = new Ion(0);

            ion.Z = elem.Z;
            double.TryParse(tbIonEnergyInput.Text, out ion.Energy);

            return ion;
            //if (ion.Name.Length < 7)
            //{
            //    int unsufficinentCounts = 7 - IonCollection.Name.Length;
            //    for (int i = 0; i < unsufficinentCounts; i++)
            //    {
            //        IonCollection.Name += " ";
            //    }
            //}
            //else
            //{
            //    IonCollection.Name = IonCollection.Name.Substring(0, 7);
            //}
        }
        public IonTarget ReadTargetInputs()
        {
            IonTarget target = new IonTarget();
            //Size-X read
            var buf = tbCoordX.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out target.Depth.Min);
                double.TryParse(buf[1], out target.Depth.Max);
            }
            //Size-Y read
            buf = tbCoordY.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out target.Width.Min);
                double.TryParse(buf[1], out target.Width.Max);
            }
            //Size-Z read
            buf = tbCoordZ.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out target.Height.Min);
                double.TryParse(buf[1], out target.Height.Max);
            }
            return target;
        }
        public AngleLimits ReadAngleInputs()
        {
            var limits = new AngleLimits();
            //Read angles
            var buf = tbZenithAngleLimitsInput.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out limits.Zenith.Min);
                double.TryParse(buf[1], out limits.Zenith.Max);
            }
            buf = tbAzimuthAngleLimitsInput.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out limits.Azimuth.Min);
                double.TryParse(buf[1], out limits.Azimuth.Max);
            }
            return limits;
        }
    }

}
