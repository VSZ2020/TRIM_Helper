using System;
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
            
            string[] filesInDir = Directory.GetFiles(Path.Combine(SRIMWorkDir + "SRIM Outputs"));
            if (filesInDir == null)
            {
                MessageBox.Show("No files were found!");
            }
            for (int i = 0; i < filesInDir.Length; i++)
            {
                var buffer = filesInDir[i].Split('\\');
                File.Copy(filesInDir[i], Path.Combine(OutPath,buffer[^1]), true);
            }
        }

        /// <summary>
        /// Action After Clicking Button "Run Task"
        /// </summary>
        public void RunCurrentTask()
        {
            //if (!Tasks[currentTaskIndex].IsActive) return;
            if (Tasks[currentTaskIndex].TrimInput == null)
            {
                Debug.WriteLine("TrimInput is NULL in task - " + this.Name + "! Calculation was not launched", "ERROR");
            }

            //Set run status
            IsRunnned = true;
            UpdateButtonLabels();

            IProgress<int> progress = new Progress<int>((int i) => prgBar.Value = i);
            Tasks[currentTaskIndex].Run(progress);
            //Copy Input Files And Run
            File.Copy(Path.Combine(Tasks[currentTaskIndex].WorkingDirectory, "TRIM.IN"), Path.Combine(SRIMDirectory, "TRIM.IN"), true);
            File.Copy(Path.Combine(Tasks[currentTaskIndex].WorkingDirectory, "TRIM.dat"), Path.Combine(SRIMDirectory, "TRIM.dat"), true);

            Tasks[currentTaskIndex].Status = "Runned";
            LaunchTRIM();
        }

        private void TaskRunAction()
        {
            if (currentTaskIndex >= Tasks.Count || Tasks.Count == 0 || currentTaskIndex == -1)
            {
                MessageBox.Show("There are no acceptable tasks!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(Path.Combine(SRIMDirectory, "TRIM.exe")))
            {
                MessageBox.Show("Incorrect SRIM Directory! Please, choose or input working SRIM directory at the top of current window", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CheckCurrentFolder(Tasks[currentTaskIndex].WorkingDirectory);

            //Block Edit and Delete button
            btnEditTask.IsEnabled = false;
            btnRemoveTask.IsEnabled = false;

            RunCurrentTask();  
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
                TaskRunAction();
            }
            if (sender == btnExtractOutputFiles)
            {
                ButtonExtractTaskClick();
            }
        }
        private void ButtonExtractTaskClick()
        {
            string SRIMOuputDir = Path.Combine(SRIMDirectory, "SRIM Outputs");

            if (!Directory.Exists(Tasks[currentTaskIndex].WorkingDirectory) || !Directory.Exists(SRIMOuputDir))
            {
                Trace.WriteLine(string.Format("Files output path: {0}\nSRIM dir: {1}", Tasks[currentTaskIndex].WorkingDirectory, SRIMOuputDir), "ERROR");
                MessageBox.Show("Unsuccessfull reading of out directory path. The action was cancelled");
                return;
            }
            
            TaskExtractFiles(SRIMDirectory, Tasks[currentTaskIndex].WorkingDirectory);      //Извлекаем файлы из папки SRIMOutputs
            IsRunnned = false;                                                              //Меняем статус вычислений на "Не Запущено"
            if (currentTaskIndex > Tasks.Count - 1) tasksList.SelectedIndex = 0;            //Смещаем позицию курсора в списке
            else
                tasksList.SelectedIndex++;
            UpdateRunStatusAtList((currentTaskIndex > 0) ? currentTaskIndex - 1 : 0);       //Обновляем заголовок кнопки запуска

            //Разблокируем кнопки Edit and Delete
            btnEditTask.IsEnabled = true;
            btnRemoveTask.IsEnabled = true;

            Trace.Flush();
        }

        public void UpdateButtonLabels()
        {
            if (Tasks != null && Tasks.Count > 0)
            {
                //btnRunTask.IsEnabled = !IsRunnned && (Tasks.Count > 0) && Tasks[currentTaskIndex].IsActive;
                btnRunTask.Content = "Run " + Tasks[currentTaskIndex]?.Name + " (F5)" ?? "None";
                if (IsRunnned)
                {
                    statusLabel.Text = $"Waiting for the end of {Tasks[currentTaskIndex]?.Name ?? "None"} task ...";
                }
                else
                {
                    statusLabel.Text = "Ready";
                }
            }
        }

        public void UpdateRunStatusAtList(int Index)
        {
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
            SI.FileName = Path.Combine(SRIMDirectory, "TRIM.exe");
            TRIMProcess.StartInfo = SI;
            TRIMProcess.Start();
            Trace.WriteLine("TRIM process was started!", "INFO");
            if (cbAutoExtract.IsChecked.Value)
                TRIMProcess.WaitForExit();

            if (cbAutoExtract.IsChecked.Value)
            {
                Trace.WriteLine("TRIM process ended!", "INFO");
                ButtonExtractTaskClick();
            }
                
            Trace.Flush();
        }

        public void CheckCurrentFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        
    }
}
