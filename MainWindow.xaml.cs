using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TRIM_Helper.Model;

namespace TRIM_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Ion IonCollection;
        public IonTarget Target;
        public AngleLimits Angles;

        
        public enum GenerateFlag
        {
            FIRST_FILE, SECOND_FILE, REVERSED_FILE
        }

        public MainWindow()
        {
            InitializeComponent();

            Target = new IonTarget();
            Angles = new AngleLimits();

            btnGenerateInput.Click += ButtonClick;
            btnSelectOutPath.Click += ButtonClick;
            btnSelectWorkDir.Click += ButtonClick;
            btnOpenRangeFileDialog.Click += ButtonClick;
            btnGenerateSecondTRIMDataFile.Click += ButtonClick;

            btnExtractOutputFiles.Click += Button_Click;
            btnRunTask.Click += Button_Click;
            tasksList.SelectionChanged += TasksList_SelectionChanged;

            prgBar.Maximum = 100;

            RestoreTrimPath();

            this.Closing += MainWindow_Closing;

            //Testing
            //NaturalNuclides();
            //UpdateButtonLabels(); 
            
        }

        private void TasksList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (tasksList.SelectedIndex > -1)
            {
                currentTaskIndex = tasksList.SelectedIndex;
                UpdateButtonLabels();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TRIM_Helper.Properties.Settings.Default.TRIMdatFilePath = tbInputOutFolder.Text;
            TRIM_Helper.Properties.Settings.Default.Save();
        }
        public void RestoreTrimPath()
        {
            tbInputOutFolder.Text = TRIM_Helper.Properties.Settings.Default.TRIMdatFilePath;
        }

        public double GenerateRandomIn(double MaxValue, double MinValue = 0)
        {
            Random rnd = new Random();
            return rnd.NextDouble() * (MaxValue - MinValue) + MinValue;
        }
        public void ReadIonInputs()
        {
            byte.TryParse(tbZ.Text, out IonCollection.Z);
            double.TryParse(tbEnergy.Text, out IonCollection.Energy);

            IonCollection.Name = tbIonRowName.Text;
            if (IonCollection.Name.Length < 7)
            {
                int unsufficinentCounts = 7 - IonCollection.Name.Length;
                for (int i = 0; i < unsufficinentCounts; i++)
                {
                    IonCollection.Name += " ";
                }
            } else
            {
                IonCollection.Name = IonCollection.Name.Substring(0, 7);
            }
        }
        public void ReadTargetInputs()
        {
            //Size-X read
            var buf = targetDepth.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out Target.Depth.Min);
                double.TryParse(buf[1], out Target.Depth.Max);
            }
            //Size-Y read
            buf = targetWidth.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out Target.Width.Min);
                double.TryParse(buf[1], out Target.Width.Max);
            }
            //Size-Z read
            buf = targetHeight.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out Target.Height.Min);
                double.TryParse(buf[1], out Target.Height.Max);
            }
        }
        public void ReadAngleInputs()
        {
            //Read angles
            var buf = IonZenithAngleLimits.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out Angles.Zenith.Min);
                double.TryParse(buf[1], out Angles.Zenith.Max);
            }
            buf = IonAzimuthAngleLimits.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out Angles.Azimuth.Min);
                double.TryParse(buf[1], out Angles.Azimuth.Max);
            }
        }

        public IonLocAndDir[] ReadInputCoordinates(string InputFileName, IProgress<int> progress)
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
            int offset = 22;

            IonLocAndDir[] values = new IonLocAndDir[linesCount - offset];

            using (FileStream fs = File.Open(InputFileName, FileMode.Open))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader reader = new StreamReader(bs))
                    {
                        //Skip the first 22 lines
                        for (int i = 0; i < offset; i++)
                        {
                            reader.ReadLine();
                        }
                        string line;
                        for (int i = 0; i < linesCount - offset; i++)
                        //while ((line = reader.ReadLine()) != null)
                        {
                            line = reader.ReadLine();
                            string[] coords = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (coords.Length < 4) continue;

                            double X, Y, Z = 0.0;
                            double.TryParse(coords[1], out X);
                            double.TryParse(coords[2], out Y);
                            double.TryParse(coords[3], out Z);

                            //Array.Resize<IonLocAndDir>(ref values, values.Length + 1);
                            //values[values.Length - 1] = new IonLocAndDir() { X = X, Y = Y, Z = Z };
                            values[i] = new IonLocAndDir() { X = X, Y = Y, Z = Z };
                            progress?.Report(100 * i / linesCount);
                        }
                    }
                }
            }
            
            return values;
        }

        public void GeneratePositions(ref Ion Ions)
        {
            double startX, startY, startZ = 0.0;
            double.TryParse(IonXcoord.Text, out startX);
            double.TryParse(IonYcoord.Text, out startY);
            double.TryParse(IonZcoord.Text, out startZ);

            int ionCount = Ions.IonsPos.Length;
            for (int i = 0; i < ionCount; i++)
            {
                Ions.IonsPos[i].X = (checkRndX.IsChecked.Value) ? GenerateRandomIn(Target.Depth.Max, Target.Depth.Min) : startX;
                Ions.IonsPos[i].Y = (checkRndY.IsChecked.Value) ? GenerateRandomIn(Target.Width.Max, Target.Width.Min) : startY;
                Ions.IonsPos[i].Z = (checkRndZ.IsChecked.Value) ? GenerateRandomIn(Target.Height.Max, Target.Height.Min) : startZ;
            }
        }

        public void GenerateAngles(ref Ion Ions)
        {
            double angX, angY, angZ = 0.0;
            double.TryParse(IonCosX.Text, out angX);
            double.TryParse(IonCosY.Text, out angY);
            double.TryParse(IonCosZ.Text, out angZ);

            int ionCount = Ions.IonsPos.Length;
            for (int i = 0; i < ionCount; i++)
            {
                var rndZenith = GenerateRandomIn(Angles.Zenith.Max, Angles.Zenith.Min);
                var rndAzimuth = GenerateRandomIn(Angles.Azimuth.Max, Angles.Azimuth.Min);
                Ions.IonsPos[i].CosX = (checkRndCosX.IsChecked.Value) ? Math.Cos(rndZenith) : angX;
                Ions.IonsPos[i].CosY = (checkRndCosY.IsChecked.Value) ? Math.Sin(rndZenith) * Math.Sin(rndAzimuth) : angY;
                Ions.IonsPos[i].CosZ = (checkRndCosZ.IsChecked.Value) ? Math.Sin(rndZenith) * Math.Cos(rndAzimuth) : angZ;
            }
        }

        public void ReverseCoordinates(ref IonLocAndDir[] coords, double Xmax)
        {
            if (coords == null) coords = new IonLocAndDir[0];

            for (int i = 0; i < coords.Length; i++)
            {
                coords[i].X = Xmax - coords[i].X;
            }
        }
        public void GenerateInputFile(string FilePath, Ion Ions, IProgress<int> progress, int precise = 5, string comment = "")
        {
            if (Ions == null)
            {
                throw new Exception("Empty Ions List");
            }

            var linesCount = Ions.IonsPos.Length;
            string OutFileName = (!string.IsNullOrEmpty(FilePath)) ? FilePath : "TRIM.dat";

            using (StreamWriter writer = new StreamWriter(OutFileName))
            {
                //Writing the first 10 lines
                var bufString = "----- TRIM with various Incident Ion Energies/Angles and Depths -----\r\n" +
                                "This files was generated by TRIM Input Generator\r\n" +
                                "Creation data " + DateTime.Now + "\r\n" +
                                "Target Dimensions:\r\n" +
                                $"X: ({Target.Depth.Min},{Target.Depth.Max})\tY: ({Target.Width.Min},{Target.Width.Max})\tZ:({Target.Height.Min},{Target.Height.Max})\r\n" +
                                "Angle Limits:\r\n" +
                                $"Zenith: ({Angles.Zenith.Min},{Angles.Zenith.Max})\tAzimuth: ({Angles.Azimuth.Min},{Angles.Azimuth.Max})\r\n" +
                                comment + "\r\n" +
                                $"Ions Count: {linesCount}\r\n";

                bufString += string.Format("{0,-7}{1,-5}{2,12}{3,15}{4,15}{5,15}{6,15}{7,15}{8,15}", 
                    "Name", "Z", "Energy(eV)", "Depth(A)", "Left(A)", "Right(A)", "Cos(x)", "Cos(y)", "Cos(z)");
                writer.WriteLine(bufString);

                //Start writing data
                for (int row = 0; row < linesCount; row++)
                {
                    string outString = string.Format(
                            "{0,-7}{1,-5}{2,12}{3,15:F" + precise + "}{4,15:F" + precise + "}{5,15:F" + precise + "}{6,15:F" + precise + "}{7,15:F" + precise + "}{8,15:F"+ precise +"}",
                            Ions.Name,
                            Ions.Z,
                            Ions.Energy * 1000.0,
                            Ions.IonsPos[row].X,
                            Ions.IonsPos[row].Y,
                            Ions.IonsPos[row].Z,
                            Ions.IonsPos[row].CosX,
                            Ions.IonsPos[row].CosY,
                            Ions.IonsPos[row].CosZ);
                    writer.WriteLine(outString);
                    progress?.Report(100 * row / linesCount);
                    
                }

                MessageBox.Show("Job done!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public async void RunGeneration(GenerateFlag flag)
        {
            btnGenerateInput.IsEnabled = false;
            btnGenerateSecondTRIMDataFile.IsEnabled = false;

            int ionsCount = 1000;
            try
            {
                prgBar.Value = 0;
                IProgress<int> progress = new Progress<int>((int i) => prgBar.Value = i);

                int.TryParse(tbIonsCount.Text, out ionsCount);
                string path = tbInputOutFolder.Text;
                string rangeFilePath = tbRangeFilePath.Text;
                string comment = tbCommentLineText.Text;
                int precise = 5;
                int.TryParse(tbDecimalPointsCount.Text, out precise);
                IonCollection = new Ion(ionsCount);

                ReadIonInputs();
                ReadTargetInputs();
                ReadAngleInputs();
                switch(flag)
                   {
                    case GenerateFlag.SECOND_FILE:
                        {
                            IonCollection.IonsPos = await Task.Run(() => ReadInputCoordinates(rangeFilePath, progress));
                            break;
                        }
                    case GenerateFlag.REVERSED_FILE:
                        {
                            double Xmax = 3000;
                            double.TryParse(tbTargetMaxWidth2.Text,out Xmax);
                            IonCollection.IonsPos = await Task.Run(() => ReadInputCoordinates(rangeFilePath, progress));
                            ReverseCoordinates(ref IonCollection.IonsPos, Xmax);
                            break;
                        }
                    default:
                        {
                            GeneratePositions(ref IonCollection);
                            break;
                        }
                   }
                    
                GenerateAngles(ref IonCollection);

                await Task.Run(() => GenerateInputFile(path, IonCollection, progress, precise, comment));
                //GenerateInputFile(tbInputOutFolder.Text, IonCollection, progress);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Generation was interrupted due to error(s): " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            btnGenerateInput.IsEnabled = true;
            btnGenerateSecondTRIMDataFile.IsEnabled = true;
        }

        public void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == btnGenerateInput)
            {
                RunGeneration(GenerateFlag.FIRST_FILE);
            }
            if (sender == btnSelectWorkDir)
            {
                Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.DefaultExt = "TRIM.exe";
                dialog.Filter = "TRIM Executable File (TRIM.exe) |TRIM.exe| All Executable Files |*.exe";
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog().Value)
                {
                    tbSRIM_OutPath.Text = dialog.FileName;
                }
                
                return;
            }
            if (sender == btnSelectOutPath)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.DefaultExt = "*.dat";
                dialog.AddExtension = true;
                dialog.Filter = "TRIM.dat File |*.dat";
                if (dialog.ShowDialog().Value)
                {
                    tbInputOutFolder.Text = dialog.FileName;
                }
                return;
            }
            if (sender == btnOpenRangeFileDialog)
            {
                Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.DefaultExt = "*.txt";
                dialog.Filter = "Text File (*.txt) |*.txt| All Files |*.*";
                dialog.CheckFileExists = true;
                if (dialog.ShowDialog().Value)
                {
                    tbRangeFilePath.Text = dialog.FileName;
                }

                return;
            }
            if (sender == btnGenerateSecondTRIMDataFile)
            {
                RunGeneration(GenerateFlag.SECOND_FILE);
                return;
            }
            if (sender == btnGenerateReversedCorrds)
            {
                RunGeneration(GenerateFlag.REVERSED_FILE);
                return;
            }
        }
    }
}
