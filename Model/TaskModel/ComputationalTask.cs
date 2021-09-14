﻿using System;
using System.Collections.ObjectModel;
using TRIM_Helper.Model.LayerModel;

namespace TRIM_Helper.Model
{
    public class ComputationalTask
    {
        public bool IsExternalCoordinatesUsing = false;
        public string ExternalDataFilePath;
        private string _Status;
        public bool IsActive { get; set; }
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
            }
        }
        public string Name { get; set; }

        public TrimInputFile TrimInput;
        public TRIMDatFile TrimOutput;
        public string WorkingDirectory;

        /// <summary>
        /// Return Count of Layers
        /// </summary>
        public int LayersCount
        {
            get
            {
                return TrimInput?.Layers.Count ?? 0;
            }
        }

        //public double TotalDepth { get; set; }
        public double TotalDepth
        {
            get
            {
                double D = 0.0;
                ref ObservableCollection<TargetLayer> layers = ref TrimInput.Layers;
                for (int i = 0; i < TrimInput.Layers.Count; i++)
                    D += layers[i].Depth;
                return D;
            }
        }

        public ComputationalTask()
        {
            Status = "Ready";
        }

        /// <summary>
        /// Launch The Computationsl Task
        /// </summary>
        /// <param name="progress"></param>
        public void Run(IProgress<int> progress)
        {
            //var Layers = TrimInput?.Layers ?? new TargetLayer[] { new TargetLayer() { Description = "Null Layer", Phase = 0, Depth = 1000 } };
            this.Status = "Generating TRIM.IN";
            TrimInput.GenerateInputFile(WorkingDirectory);
            this.Status = "Generating TRIM.dat";
            TrimOutput.GenerateDataFile(WorkingDirectory, TrimInput._Ion, TrimInput.Layers, progress, IsExternalCoordinatesUsing, ExternalDataFilePath);
        }
    }
}
