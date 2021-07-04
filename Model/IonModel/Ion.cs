namespace TRIM_Helper.Model
{
    public class Ion
    {
        public string Name;
        /// <summary>
        /// Ion order number
        /// </summary>
        public byte Z = 1;

        public double Mass = 0.0;
        public int Number = 1000;
        /// <summary>
        /// Ion Energy In KeV
        /// </summary>
        public double Energy = 100;

        public double Angle = 0;

        public double BraggCorr = 0.0;

        /// <summary>
        /// Define incidence angle limits
        /// </summary>
        public AngleLimits Angles;

        /// <summary>
        /// A number of Start Locations and Directions of Ion
        /// </summary>
        public IonLocAndDir[] IonsPos;

        public Ion(int IonsCount)
        {
            IonsPos = new IonLocAndDir[IonsCount];
        }
    }
}
