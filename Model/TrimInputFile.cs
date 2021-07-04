using System;
using System.IO;

namespace TRIM_Helper.Model
{
    public class TrimInputFile
    {
        public Ion Ion { get; set; }
        private TargetLayer[] _Layers;
        private Element[] _LayerComponents;
        public Element[] Elements
        {
            get
            {
                return _LayerComponents;
            }
        }

        public int AutoSaveNumber = 1000;
        public byte PlotType = 0;    //0-5
        public int PlotDepthMin = 0;
        public int PlotDepthMax = 1000;
        public byte Cascades = 1;   //1 - 7
        public byte StoppingPowerVersion = 0;

        //Disk Files
        public bool IsRanges = false;
        public bool IsBackscatt = false;
        public bool IsTransmitt = false;
        public bool IsSputter = false;
        /// <summary>
        /// Write collisions to file
        /// (0 - No, 1 -Ions, 2 - Ions+Recoils)
        /// </summary>
        public byte  Collisions = 0; 
        public double EXYZ = 0.0;

        public int RndSeed = 0;
        public byte Reminders = 0;

        public TargetLayer[] Layers
        {
            get
            {
                return _Layers;
            }
        }

        public TrimInputFile(Ion ion, TargetLayer[] layers, Element[] substance)
        {
            this.Ion = ion;
            this._Layers = layers;
            this._LayerComponents = substance;
        }



        public void GenerateInputFile(string FilePath)
        {
            if (!Directory.Exists(FilePath)) throw new System.Exception("Directory " + FilePath + " doesn't exist!");
            int layersCount = Layers?.Length ?? 0;
            int elementsCount = Elements?.Length ?? 0;

            FilePath = (!string.IsNullOrEmpty(FilePath)) ? FilePath + "TRIM.IN" : "TRIM.IN";
            using (FileStream fs = File.OpenWrite(FilePath))
            {
                using (StreamWriter wr = new StreamWriter(fs))
                {
                    wr.Write(
                        "==> SRIM-2013.00 This file controls TRIM Calculations.\r\n" +
                        "Ion: Z1, M1, Energy(keV), Angle, Number, Bragg Corr, AutoSave Number.\r\n");
                    wr.WriteLine(string.Format(
                        "{0,-5} {1} {2} {3} {4} {5} {6}",
                        Ion.Z,
                        Ion.Mass,
                        Ion.Energy,
                        Ion.Angle,
                        Ion.Number,
                        Ion.BraggCorr,
                        AutoSaveNumber
                        ));
                    wr.WriteLine(
                        "Cascades(1=No;2=Full;3=Sputt;4-5=Ions;6-7=Neutrons), Random Number Seed, Reminders\r\n" +
                        string.Format("{0} {1} {2}\r\n", Cascades, RndSeed, Reminders) +
                        "Diskfiles (0=no,1=yes): Ranges, Backscatt, Transmit, Sputtered, Collisions(1=Ion;2=Ion+Recoils), Special EXYZ.txt file");
                    wr.WriteLine(string.Format(
                        "{0} {1} {2} {3} {4} {5}",
                        (IsRanges) ? 1 : 0,
                        (IsBackscatt) ? 1 : 0,
                        (IsTransmitt) ? 1 : 0,
                        (IsSputter) ? 1 : 0,
                        Collisions,
                        EXYZ
                        ));
                    wr.WriteLine(
                        "Target material : Number of Elements & Layers\r\n" + 
                        string.Format(
                        "\"{0,-40}\" {1} {2}",
                        (Layers != null && Layers.Length > 0) ? Layers[0].Description : "Empty",
                        elementsCount,
                        layersCount
                        ));
                    wr.WriteLine("PlotType (0-5); Plot Depths: Xmin, Xmax(Ang.) [=0 0 for Viewing Full Target]\r\n" + 
                        string.Format(
                        "{0} {1} {2}",
                        PlotType,
                        PlotDepthMin,
                        PlotDepthMax
                        ));

                    string bufTargetElements = "";
                    string bufTargetElementsList = "";
                    string bufStoich = "";
                    string bufStoichNumbers = "";
                    string bufPhases = "";
                    string bufBraggCorr = "";
                    string bufDisplaicements = "";
                    string bufLattice = "";
                    string bufBinding = "";

                    for (int i = 0; i < elementsCount; i++)
                    {
                        bufTargetElements += string.Format(
                            "Atom {0} = {1} = {2,10} {3,0:F3}",
                            i + 1,
                            Elements[i].Name,
                            Elements[i].Z,
                            Elements[i].Mass);
                        if (i < elementsCount - 1) 
                            bufTargetElements += "\r\n";

                        bufTargetElementsList += string.Format(" {0}({1})  ", Elements[i].Name, Elements[i].Z);
                        bufStoich += "  Stoich";
                        bufDisplaicements += string.Format(" {0,-8}", Elements[i].E_d);
                        bufLattice += string.Format(" {0,-8}", Elements[i].lattice);
                        bufBinding += string.Format(" {0,-8}", Elements[i].surface);
                    }
                    for (int i = 0; i < layersCount; i++)
                    {
                        bufStoichNumbers += string.Format(
                            " {0} \"{1}\" {2} {3}  ",
                            i + 1,
                            Layers[i].Description,
                            Layers[i].Depth,
                            Layers[i].Density);
                        for (int j = 0; j < elementsCount; j++)
                        {
                            bufStoichNumbers += string.Format(" {0}", (Elements[j].LayerIndex == i) ? Elements[j].Stoich : 0);
                        }
                        bufPhases += Layers[i].Phase.ToString();
                        bufBraggCorr += Layers[i].CompoundCorrection;

                        if (i < layersCount - 1)
                        {
                            bufStoichNumbers += "\r\n";
                            bufPhases += " ";
                            bufBraggCorr += " ";
                        }
                    }
                    wr.WriteLine("Target Elements:    Z   Mass(amu)\r\n" + bufTargetElements);
                    wr.WriteLine("Layer   Layer Name /               Width Density  " + bufTargetElementsList);
                    wr.WriteLine("Numb.   Description                (Ang) (g/cm3)  " + bufStoich);
                    wr.WriteLine(bufStoichNumbers);
                    wr.WriteLine("0  Target layer phases (0=Solid, 1=Gas)\r\n" + bufPhases);

                    //Lattice and displaicement parameters
                    wr.WriteLine("Target Compound Corrections (Bragg)\r\n" + bufBraggCorr);
                    wr.WriteLine("Individual target atom displacement energies (eV)\r\n" + bufDisplaicements);
                    wr.WriteLine("Individual target atom lattice binding energies (eV)\r\n" + bufLattice);
                    wr.WriteLine("Individual target atom surface binding energies (eV)\r\n" + bufBinding);
                    wr.WriteLine("Stopping Power Version (1=2011, 0=2011)\r\n" + StoppingPowerVersion.ToString());
                }
            }
        }
    }
}
