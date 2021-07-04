using System;
using System.ComponentModel;
using System.Threading.Tasks;
namespace TRIM_Helper.Model
{
    public class ComputationalTask: INotifyPropertyChanged
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
                OnPropertyChanged(null);
            }
        }
        public string Name { get; set; }
        public TrimInputFile TrimInput;
        public TRIMDatFile TrimOutput;
        public string WorkingDirectory;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        /// <summary>
        /// Return Count of Layers
        /// </summary>
        public int LayersCount
        {
            get
            {
                return TrimInput?.Layers?.Length ?? 0;
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
            var Layers = TrimInput?.Layers ?? new TargetLayer[] { new TargetLayer() { Description = "Null Layer", Phase = 0, Depth = 1000 } };
            this.Status = "Generating TRIM.IN";
            TrimInput.GenerateInputFile(WorkingDirectory);
            this.Status = "Generating TRIM.dat";
            TrimOutput.GenerateDataFile(WorkingDirectory, TrimInput.Ion, Layers, progress, IsExternalCoordinatesUsing, ExternalDataFilePath);
        }
    }
}
