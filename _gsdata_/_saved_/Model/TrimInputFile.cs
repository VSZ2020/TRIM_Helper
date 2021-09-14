using System;
using TRIM_Helper.Model.LayerModel;
using TRIM_Helper.Model.LayerModel.MaterialModel;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace TRIM_Helper.Model
{
    public class TrimInputFile
    {
        private float _plotMaxDepth = 1000;
        public Ion _Ion { get; set; }
        public ObservableCollection<TargetLayer> Layers;

        public int  AutoSaveNumber = 1000;
        public byte PlotType = 0;    //0-5
        public float PlotDepthMin { get; set; } = 0;
        public float PlotDepthMax
        {
            get
            {
                if (Layers == null)
                    return 0;
                _plotMaxDepth = (float)GetLayersSumDepth();
                return _plotMaxDepth;
            }
			set
			{
                _plotMaxDepth = value;
			}
        }
        public byte Cascades = 1;   //1 - 7
        public byte StoppingPowerVersion = 0;

        //Disk Files
        public bool IsRanges { get; set; } = false;
        public bool IsBackscatt { get; set; } = false;
        public bool IsTransmitt { get; set; } = false;
        public bool IsSputter { get; set; } = false;
        /// <summary>
        /// Write collisions to file
        /// (0 - No, 1 -Ions, 2 - Ions+Recoils)
        /// </summary>
        public byte Collisions { get; set; } = 0; 
        public double EXYZ = 0.0;

        public int RndSeed { get; set; } = 0;
        public byte Reminders = 0;
        public int Number = 1000;                   //Amount of ions for modeling (optional value)

        public TrimInputFile(Ion ion, ObservableCollection<TargetLayer> layers)
        {
            this._Ion = ion;
            this.Layers = layers;
        }

		public void GenerateInputFile(string FilePath)
        {
            if (!Directory.Exists(FilePath)) throw new System.Exception("Directory " + FilePath + " doesn't exist!");
            int layersCount = Layers?.Count ?? 0;

            FilePath = (!string.IsNullOrEmpty(FilePath)) ? FilePath + "TRIM.IN" : "TRIM.IN";
            using (StreamWriter wr = new StreamWriter(FilePath))
            {
                wr.Write(
                    "==> SRIM-2013.00 This file controls TRIM Calculations.\r\n" +
                    "Ion: Z1, M1, Energy(keV), Angle, Number, Bragg Corr, AutoSave Number.\r\n");
                wr.WriteLine(string.Format(
                    "{0,-5} {1} {2} {3} {4} {5} {6}",
                    _Ion.Z,
                    _Ion.Mass,
                    _Ion.Energy,
                    _Ion.Angle,
                    Number,
                    _Ion.BraggCorr,
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

                //Count the layers for all target layers
                int allLayersElementCount = 0;
                for (int i = 0; i < layersCount; i++)
                    allLayersElementCount += Layers[i].Elements.Count;

                wr.WriteLine(
                    "Target material : Number of Elements & Layers\r\n" +
                    string.Format(
                    "\"{0,-40}\" {1} {2}",
                    (Layers != null && Layers.Count > 0) ? Layers[0].LayerName : "Empty Layer",
                    allLayersElementCount,
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

                int atomIndex = 1;
                for (int i = 0; i < layersCount; i++)
                {
                    bufStoichNumbers += string.Format(
                        " {0} \"{1}\" {2} {3}  ",
                        i + 1,
                        Layers[i].LayerName,
                        Layers[i].Depth,
                        Layers[i].Density);
                    int elementsCount = Layers[i].Elements.Count;
                    for (int j = 0; j < elementsCount; j++)
                    {
                        var element = Layers[i].Elements[j];
                        //Listing of atoms of all layers
                        bufTargetElements += string.Format("Atom {0} = {1} = {2, 10} {3,0:F3}",
                                atomIndex,
                                element.ElementName,
                                element.Z,
                                element.Mass);
                        if (j < elementsCount - 1)
                            bufTargetElements += "\r\n";

                        bufTargetElementsList += string.Format(" {0}({1})  ", element.ElementName, element.Z);
                        bufStoich += "  Stoich";
                        bufDisplaicements += string.Format(" {0,-8}", element.E_d);
                        bufLattice += string.Format(" {0,-8}", element.lattice);
                        bufBinding += string.Format(" {0,-8}", element.surface);
                        atomIndex++;
                    }
                    bufPhases += Layers[i].IsGas ? 1 : 0;
                    bufBraggCorr += Layers[i].CompoundCorrection.ToString();

                    //fill stoich numbers
                    for (int k = 0; k < layersCount; k++)
                    {
                        for (int h = 0; h < Layers[k].Elements.Count; h++)
                        {
                            if (k != i)
                                bufStoichNumbers += " 0.0 ";
                            else
                                bufStoichNumbers += string.Format(" {0}", Layers[k].Elements[h].Stoich);
                        }

                    }
                    //Add spaces
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
                wr.WriteLine(bufStoichNumbers.ToString());
                wr.WriteLine("0  Target layer phases (0=Solid, 1=Gas)\r\n" + bufPhases);

                //Lattice and displaicement parameters
                wr.WriteLine("Target Compound Corrections (Bragg)\r\n" + bufBraggCorr);
                wr.WriteLine("Individual target atom displacement energies (eV)\r\n" + bufDisplaicements);
                wr.WriteLine("Individual target atom lattice binding energies (eV)\r\n" + bufLattice);
                wr.WriteLine("Individual target atom surface binding energies (eV)\r\n" + bufBinding);
                wr.WriteLine("Stopping Power Version (1=2011, 0=2011)\r\n" + StoppingPowerVersion.ToString());
            }
        }

        public double GetLayersSumDepth()
		{
            double sum = 0.0;
            if (this.Layers != null)
			{
                for (int i = 0; i < Layers.Count; i++)
                {
                    if (Layers[i] == null)
                        continue;
                    sum += Layers[i].Depth;
                }
            }
            return sum;
		}
    }
}
