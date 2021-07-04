using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRIM_Helper.Model
{
    public static class MiscelaniousFunctions
    {
        public static IonLocAndDir[] Read_TRANSMITT_File(string InputFileName, IProgress<int> progress)
        {
            if (string.IsNullOrEmpty(InputFileName) || !File.Exists(InputFileName))
            {
                throw new Exception("Input file path is empty or incorrect!");
            }

            var linesCount = 0;
            using (StreamReader reader = new StreamReader(InputFileName))
            {
                while (reader.ReadLine() != null)
                    linesCount++;
            }
            int offset = 12;

            IonLocAndDir[] values = new IonLocAndDir[linesCount - offset];

            using (FileStream fs = File.Open(InputFileName, FileMode.Open))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader reader = new StreamReader(bs))
                    {
                        //Skip the first 12 lines
                        for (int i = 0; i < offset; i++)
                        {
                            reader.ReadLine();
                        }
                        string line;
                        for (int i = 0; i < linesCount - offset; i++)
                        //while ((line = reader.ReadLine()) != null)
                        {
                            line = reader.ReadLine();
                            string buff = line.Split('T')[1];
                            string[] coords = buff?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[1];
                            if (coords.Length < 9)
                            {
                                Array.Resize<IonLocAndDir>(ref values, values.Length - 1);
                                continue;
                            }

                            double E, X, Y, Z, cosX, cosY, cosZ = 0.0;
                            double.TryParse(coords[2], out E);
                            double.TryParse(coords[3], out X);
                            double.TryParse(coords[4], out Y);
                            double.TryParse(coords[5], out Z);
                            double.TryParse(coords[6], out cosX);
                            double.TryParse(coords[7], out cosY);
                            double.TryParse(coords[8], out cosZ);

                            //Array.Resize<IonLocAndDir>(ref values, values.Length + 1);
                            //values[values.Length - 1] = new IonLocAndDir() { X = X, Y = Y, Z = Z };
                            values[i] = new IonLocAndDir() { X = X, Y = Y, Z = Z, CosX = cosX, CosY = cosY, CosZ = cosZ, Energy = E };
                            progress?.Report(100 * i / linesCount);
                        }
                    }
                }
            }

            return values;
        }
    }
}
