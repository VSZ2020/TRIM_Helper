using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TRIM_Helper.Model;

namespace TRIM_Helper
{
    public partial class MainWindow : Window
    {
        int currentTaskIndex = -1;
        public static ObservableCollection<ComputationalTask> Tasks;
        public static string CurrentWorkingPath = "";
        public static string SRIMDirectory = "";

        public bool IsRunnned = false;


        /// <summary>
        /// Action After Clicking Button "Extract Files"
        /// </summary>
        /// <param name="SRIMWorkDir"></param>
        /// <param name="OutPath"></param>
        public void TaskExtractFiles(string SRIMWorkDir, string OutPath)
        {
            CheckCurrentFolder(OutPath);
            string[] filesInDir = Directory.GetFiles(SRIMWorkDir + "SRIM Outputs\\");
            if (filesInDir == null)
            {
                MessageBox.Show("No files were found!");
            }
            for (int i = 0; i < filesInDir.Length; i++)
            {
                var buffer = filesInDir[i].Split('\\');
                File.Copy(filesInDir[i], OutPath + buffer[^1], true);
            }
        }

        /// <summary>
        /// Action After Clicking Button "Run Task"
        /// </summary>
        public void TaskRunCurrentTask()
        {
            //if (!Tasks[currentTaskIndex].IsActive) return;
            if (Tasks[currentTaskIndex].TrimInput == null)
            {
                System.Diagnostics.Debug.WriteLine("TrimInput is NULL in task - " + this.Name + "! Calculation was not launched", "ERROR");
            }

            //Set run status
            IsRunnned = true;
            UpdateButtonLabels();

            IProgress<int> progress = new Progress<int>((int i) => prgBar.Value = i);
            Tasks[currentTaskIndex].Run(progress);
            //Copy Input Files And Run
            File.Copy(Tasks[currentTaskIndex].WorkingDirectory + "TRIM.IN", SRIMDirectory + "TRIM.IN", true);
            File.Copy(Tasks[currentTaskIndex].WorkingDirectory + "TRIM.dat", SRIMDirectory + "TRIM.dat", true);

            Tasks[currentTaskIndex].Status = "Runned";
            LaunchTRIM();
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentTaskIndex >= Tasks.Count || Tasks.Count == 0 || currentTaskIndex == -1)
            {
                MessageBox.Show("There are no acceptable tasks!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sender == btnRunTask)
            {
                CheckCurrentFolder(Tasks[currentTaskIndex].WorkingDirectory);
                TaskRunCurrentTask();

                //Block Edit and Delete button
                btnEditTask.IsEnabled = false;
                btnRemoveTask.IsEnabled = false;
            }
            if (sender == btnExtractOutputFiles)
            {
                string outPath = CurrentWorkingPath + Tasks[currentTaskIndex].Name + "\\";
                if (!Directory.Exists(outPath) || !Directory.Exists(SRIMDirectory + "SRIM Outputs\\"))
                {
                    MessageBox.Show("Unsuccessfull reading of out directory path. The action was cancelled");
                    return;
                }

                TaskExtractFiles(SRIMDirectory, outPath);
                IsRunnned = false;
                if (currentTaskIndex > Tasks.Count - 1) tasksList.SelectedIndex = 0;
                else
                    tasksList.SelectedIndex++;
                //UpdateButtonLabels();
                UpdateRunStatusAtList((currentTaskIndex > 0) ? currentTaskIndex - 1 : 0);

                //Unblock Edit and Delete button
                btnEditTask.IsEnabled = true;
                btnRemoveTask.IsEnabled = true;
            }
        }
        public void UpdateButtonLabels()
        {
            if (Tasks != null && Tasks.Count > 0)
			{
                //btnRunTask.IsEnabled = !IsRunnned && (Tasks.Count > 0) && Tasks[currentTaskIndex].IsActive;
                btnRunTask.Content = "Run " + Tasks[currentTaskIndex]?.Name ?? "None";
                if (IsRunnned)
                {
                    statusLabel.Text = $"Waiting for end of {Tasks[currentTaskIndex]?.Name ?? "None"} task ...";
                }
                else
                {
                    statusLabel.Text = "Ready";
                }
            }
        }

        public void UpdateRunStatusAtList(int Index)
        {
            //(tasksList.Items[Index] as ComputationalTask).Status = "Done";
            Tasks[Index].IsActive = false;
            Tasks[Index].Status = "Done";
        }

        public void LaunchTRIM()
        {
            if (!Directory.Exists(SRIMDirectory))
            {
                MessageBox.Show("SRIM Directory is incorrect!");
                return;
            }
            Process TRIMProcess = new Process();
            ProcessStartInfo SI = new ProcessStartInfo();
            SI.WorkingDirectory = SRIMDirectory;
            SI.FileName = SRIMDirectory + "TRIM.exe";
            TRIMProcess.StartInfo = SI;
            TRIMProcess.Start();
        }

        public void CheckCurrentFolder(string path)
		{
            if (!Directory.Exists(path))
			{
                Directory.CreateDirectory(path);
            }
		}
        //public void NaturalNuclides()
        //{
        //    int IonsCount = 100000;
        //    CurrentWorkingPath = "C:\\Users\\Slava Izgagin\\Desktop\\U-238 Group (With 3 CR-39)\\";
        //    SRIMDirectory = "C:\\Users\\Slava Izgagin\\Desktop\\SRIM\\";

        //    string[] TaskNames = new string[] { "U-238", "U-234", "Th-230", "Ra-226", "Rn-222", "Po-218", "Po-214", "Po-210" };
        //    //string[] TaskNames = new string[] { "Th-232","Th-228", "Ra-224", "Rn-220", "Po-216", "Bi-212", "Po-212"};

        //    //Source Ions
        //    AngleLimits limits = new AngleLimits() { Azimuth = new ValueRange() { Max = 360, Min = 0 }, Zenith = new ValueRange() { Max = 180, Min = 0} };
        //    Ion[] Ions = new Ion[8];
        //    Ions[0] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 4198, Z = 2, Angles = limits };
        //    Ions[1] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 4775, Z = 2, Angles = limits };
        //    Ions[2] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 4687, Z = 2, Angles = limits };
        //    Ions[3] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 4784, Z = 2, Angles = limits };
        //    Ions[4] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 5489, Z = 2, Angles = limits };
        //    Ions[5] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 6002, Z = 2, Angles = limits };
        //    Ions[6] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 7687, Z = 2, Angles = limits };
        //    Ions[7] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 5305, Z = 2, Angles = limits };
        //    //Ions[0] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 4012, Z = 2, Angles = limits };
        //    //Ions[1] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 5523, Z = 2, Angles = limits };
        //    //Ions[2] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 5686, Z = 2, Angles = limits };
        //    //Ions[3] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 6288, Z = 2, Angles = limits };
        //    //Ions[4] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 6778, Z = 2, Angles = limits };
        //    //Ions[5] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 6051, Z = 2, Angles = limits };
        //    //Ions[6] = new Ion(IonsCount) { Name = "He", Mass = 4.003, Energy = 8785, Z = 2, Angles = limits };


        //    //TargetLayers
        //    TargetLayer Layer = new TargetLayer() { CompoundCorrection = 1.0, Description = "Soda Lime Glass Layer", Phase = 0, Density = 2.33, Depth = 510000 };
        //    TargetLayer Layer2 = new TargetLayer() { CompoundCorrection = 1.0, Description = "CR-39 NTD", Phase = 0, Density = 1.03527027, Depth = 120000 };
        //    TargetLayer Layer3 = new TargetLayer() { CompoundCorrection = 1.0, Description = "CR-39 NTD", Phase = 0, Density = 1.03527027, Depth = 120000 };
        //    TargetLayer Layer4 = new TargetLayer() { CompoundCorrection = 1.0, Description = "CR-39 NTD", Phase = 0, Density = 1.03527027, Depth = 120000 };
        //    //TargetLayer Layer2 = new TargetLayer() { CompoundCorrection = 1.0, Description = "LR-115 Nuclear track detector (ICRU-510)", Phase = 0, Density = 1.48, Depth = 120000 };
        //    //TargetLayer Layer3 = new TargetLayer() { CompoundCorrection = 1.0, Description = "LR-115 Nuclear track detector (ICRU-510)", Phase = 0, Density = 1.48, Depth = 120000 };
        //    //TargetLayer Layer4 = new TargetLayer() { CompoundCorrection = 1.0, Description = "LR-115 Nuclear track detector (ICRU-510)", Phase = 0, Density = 1.48, Depth = 120000 };

        //    //Substances
        //    Element[] Elements = new Element[15];
        //    //Glass Layer properties
        //    Elements[0] = new Element() { Name = "O", Mass = 15.999, Z = 8, LayerIndex = 0, Stoich = 0.6, E_d = 28.0, lattice = 3.0, surface = 2.0 };
        //    Elements[1] = new Element() { Name = "Si", Mass = 28.086, Z = 14, LayerIndex = 0, Stoich = 0.25, E_d = 15.0, lattice = 2.0, surface = 4.7 };
        //    Elements[2] = new Element() { Name = "Na", Mass = 22.99, Z = 11, LayerIndex = 0, Stoich = 0.1, E_d = 25.0, lattice = 3.0, surface = 1.12 };
        //    Elements[3] = new Element() { Name = "Ca", Mass = 40.08, Z = 20, LayerIndex = 0, Stoich = 0.03, E_d = 25.0, lattice = 3.0, surface = 1.83 };
        //    Elements[4] = new Element() { Name = "Mg", Mass = 24.305, Z = 12, LayerIndex = 0, Stoich = 0.01, E_d = 25.0, lattice = 3.0, surface = 1.54 };
        //    Elements[5] = new Element() { Name = "Al", Mass = 26.982, Z = 13, LayerIndex = 0, Stoich = 0.01, E_d = 25.0, lattice = 3.0, surface = 3.36 };
            
        //    //LR-115 layer properties
        //    //Elements[6] = new Element() { Name = "H", Mass = 1.008, Z = 1, LayerIndex = 1, Stoich = 0.612245, E_d = 10.0, lattice = 3.0, surface = 2.0 };
        //    //Elements[7] = new Element() { Name = "C", Mass = 12.011, Z = 6, LayerIndex = 1, Stoich = 0.265306, E_d = 28.0, lattice = 3.0, surface = 7.41 };
        //    //Elements[8] = new Element() { Name = "N", Mass = 14.007, Z = 7, LayerIndex = 1, Stoich = 0.091837, E_d = 28.0, lattice = 3.0, surface = 2.0 };
        //    //Elements[9] = new Element() { Name = "O", Mass = 15.999, Z = 8, LayerIndex = 1, Stoich = 0.030612, E_d = 28.0, lattice = 3.0, surface = 2.0 };

        //    //CR-39 layer properties
        //    Elements[6] = new Element() { Name = "H", Mass = 1.008, Z = 1, LayerIndex = 1, Stoich = 0.486486, E_d = 10.0, lattice = 3.0, surface = 2.0 };
        //    Elements[7] = new Element() { Name = "C", Mass = 12.011, Z = 6, LayerIndex = 1, Stoich = 0.324324, E_d = 28.0, lattice = 3.0, surface = 7.41 };
        //    Elements[8] = new Element() { Name = "O", Mass = 15.999, Z = 8, LayerIndex = 1, Stoich = 0.189189, E_d = 28.0, lattice = 3.0, surface = 2.0 };

        //    Elements[9] = new Element() { Name = "H", Mass = 1.008, Z = 1, LayerIndex = 2, Stoich = 0.486486, E_d = 10.0, lattice = 3.0, surface = 2.0 };
        //    Elements[10] = new Element() { Name = "C", Mass = 12.011, Z = 6, LayerIndex = 2, Stoich = 0.324324, E_d = 28.0, lattice = 3.0, surface = 7.41 };
        //    Elements[11] = new Element() { Name = "O", Mass = 15.999, Z = 8, LayerIndex = 2, Stoich = 0.189189, E_d = 28.0, lattice = 3.0, surface = 2.0 };
            
        //    Elements[12] = new Element() { Name = "H", Mass = 1.008, Z = 1, LayerIndex = 3, Stoich = 0.486486, E_d = 10.0, lattice = 3.0, surface = 2.0 };
        //    Elements[13] = new Element() { Name = "C", Mass = 12.011, Z = 6, LayerIndex = 3, Stoich = 0.324324, E_d = 28.0, lattice = 3.0, surface = 7.41 };
        //    Elements[14] = new Element() { Name = "O", Mass = 15.999, Z = 8, LayerIndex = 3, Stoich = 0.189189, E_d = 28.0, lattice = 3.0, surface = 2.0 };

        //    //Elements[0] = new Element() { Name = "H", Mass = 1.008, Z = 1, LayerIndex = 0, Stoich = 0.612245, E_d = 10.0, lattice = 3.0, surface = 2.0 };
        //    //Elements[1] = new Element() { Name = "C", Mass = 12.011, Z = 6, LayerIndex = 0, Stoich = 0.265306, E_d = 28.0, lattice = 3.0, surface = 7.41 };
        //    //Elements[2] = new Element() { Name = "N", Mass = 14.007, Z = 7, LayerIndex = 0, Stoich = 0.091837, E_d = 28.0, lattice = 3.0, surface = 2.0 };
        //    //Elements[3] = new Element() { Name = "O", Mass = 15.999, Z = 8, LayerIndex = 0, Stoich = 0.030612, E_d = 28.0, lattice = 3.0, surface = 2.0 };


        //    //Arrays
        //    Tasks = new List<ComputationalTask>();
        //    for (int i = 0; i < Ions.Length; i++)
        //    {
        //        var newTask = new ComputationalTask() { Name = TaskNames[i]};
        //        string currDir = CurrentWorkingPath + newTask.Name + "\\";
        //        if (!Directory.Exists(currDir)) Directory.CreateDirectory(currDir);

        //        newTask.WorkingDirectory = currDir;
        //        //newTask.IsExternalCoordinatesUsing = true;
        //        //newTask.ExternalDataFilePath = currDir + "TRANSMIT.txt";
        //        newTask.TrimInput = new TrimInputFile(Ions[i], new TargetLayer[] { Layer, Layer2, Layer3, Layer4}, Elements) { Cascades = 4, IsBackscatt = true, IsTransmitt = true, IsRanges = true, PlotDepthMax = (int)(Layer.Depth + Layer2.Depth + Layer3.Depth + Layer4.Depth) };
        //        newTask.TrimOutput = new TRIMDatFile() { IonRowName = newTask.TrimInput.Ion.Name, IonsCount = IonsCount, CalcComment = newTask.Name};
        //        newTask.IsActive = true;
        //        Tasks.Add(newTask);
        //        //tasksList.Items.Add(Tasks[i].Name);
        //    }
        //    tasksList.ItemsSource = Tasks;
        //}
    }
}
