using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

        public bool IsRun = false;

        /// <summary>
        /// Action After Clicking Button "Extract Files"
        /// </summary>
        /// <param name="fromDir"></param>
        /// <param name="toDir"></param>
        public void ExtractFilesFromSRIMOut(string fromDir, string toDir)
        {
            ref TrimInputFile iFile = ref Tasks[currentTaskIndex].TrimInput;

            string[] filesInDir = Directory.GetFiles(Path.Combine(fromDir,"SRIM Outputs"));
            if (filesInDir == null)
            {
                MessageBox.Show("No files to copy from \'SRIM Output\' dir!");
                return;
            }
            for (int i = 0; i < filesInDir.Length; i++)
            {
                var fileName = filesInDir[i].Split('\\')[^1];
                File.Copy(filesInDir[i], Path.Combine(toDir, fileName), true);
                /*
                //Копируем только выбранные в окне редактирования задачи файлы
                if ((iFile.IsTransmitt || iFile.IsBackscatt || iFile.IsSputter) && (fileName.ToUpper() == "TRIMOUT.TXT"))
                {
                    File.Copy(filesInDir[i], Path.Combine(toDir, fileName), true);
                }
                if (iFile.IsBackscatt && fileName.ToUpper() == "BACKSCAT.TXT")
                {
                    File.Copy(filesInDir[i], Path.Combine(toDir, fileName), true);
                    continue;
                }
                if (iFile.IsRanges && fileName.ToUpper() == "RANGE_3D.TXT")
                {
                    File.Copy(filesInDir[i], Path.Combine(toDir, fileName), true);
                    continue;
                }
                if (iFile.IsTransmitt && fileName.ToUpper() == "TRANSMIT.TXT")
                {
                    File.Copy(filesInDir[i], Path.Combine(toDir, fileName), true);
                    continue;
                }
                if (iFile.IsSputter && fileName.ToUpper() == "SPUTTER.TXT")
                {
                    File.Copy(filesInDir[i], Path.Combine(toDir, fileName), true);
                }
                */
            }
        }

        /// <summary>
        /// Action After Clicking Button "Run Task"
        /// </summary>
        public async void RunCurrentTask()
        {
            
            if (currentTaskIndex >= Tasks.Count || Tasks.Count == 0 || currentTaskIndex == -1)
            {
                MessageBox.Show("There is no acceptable task for run!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //Если задача неактивная, то пропускаем её
            if (!Tasks[currentTaskIndex].IsActive)
            {
                if (currentTaskIndex < Tasks.Count - 1) 
                    currentTaskIndex++;
                else
                    return;
            }

            //Проверяем наличия входных данных расчета
            if (Tasks[currentTaskIndex].TrimInput == null)
            {
                Trace.WriteLine("TRIM Input class is NULL for task " + Tasks[currentTaskIndex].Name + "! Calculation won't be launched", "ERROR");
                Trace.Flush();
                return;
            }

            //Переменные для хранения рабочей папки и местоположения файла TRIM.exe
            string workDir = Tasks[currentTaskIndex].WorkingDirectory;
            string TRIMExecutablePath = Path.Combine(SRIMDirectory, "TRIM.exe");

            //Проверяем существование исполняемого файла TRIM.exe
            if (!File.Exists(TRIMExecutablePath))
            {
                Trace.WriteLine("Work directory: " + CurrentWorkingPath, "INFO");
                Trace.WriteLine("Incorrect SRIM directory! \"" + TRIMExecutablePath + "\".", "ERROR");
                Trace.Flush();
                MessageBox.Show("Incorrect SRIM directory path! \"" + SRIMDirectory + "\". Please, choose or input SRIM directory at the top textbox of current window", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Если отсутствует выходная папка, то создаем новую
            if (!Directory.Exists(workDir))
            {
                try
                {
                    Directory.CreateDirectory(workDir);
                }
                catch(UnauthorizedAccessException unauthorizedEx)
                {
                    MessageBox.Show("Unathorized access to folder " + workDir + ". Error message content: " + unauthorizedEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                catch (IOException IOEx)
                {
                    MessageBox.Show("Input/Output exception for directory path '" + workDir + "'. Error message content: " + IOEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
            }
            
            IProgress<int> progress = new Progress<int>((int i) => prgBar.Value = i);
            statusLabelCtrl.Text = "Generation of TRIM input and TRIM data files...";
            await Tasks[currentTaskIndex].RunAsync(progress);

            try
            {
                //Copy TRIM.IN and TRIM.dat files to SRIM directory with overwrite
                File.Copy(Path.Combine(workDir, "TRIM.IN"), Path.Combine(SRIMDirectory, "TRIM.IN"), true);
                File.Copy(Path.Combine(workDir, "TRIM.dat"), Path.Combine(SRIMDirectory, "TRIM.dat"), true);
            }
            catch(UnauthorizedAccessException uaccessEx)
            {
                MessageBox.Show("Can't get access to folder. Error message content: " + uaccessEx.Message, "Unauthorized access", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Если предыдущие проверки пройдены, то начинаем процедуру запуска вычисления
            Trace.WriteLine("Block buttons before launching");
            //Block Edit and Delete buttons
            SetButtonsState(false);

            Tasks[currentTaskIndex].Status = "Launched";
            //Set run status
            IsRun = true;
            statusLabelCtrl.Text = "Calculation in progress";
            Trace.WriteLine("Launch TRIM.exe");
            await Task.Delay(300);
            LaunchTRIM();
            statusLabelCtrl.Text = "Calculation is done";
        }

        private async void ButtonExtractTaskClick()
        {
            Trace.WriteLine("Unblock buttons before launching");
            //Разблокируем кнопки Edit and Delete
            SetButtonsState(true);
            IsRun = false;                                                              //Меняем статус вычислений на "Не Запущено"

            string SRIMOuputDir = Path.Combine(SRIMDirectory, "SRIM Outputs");
            string destDir = Tasks[currentTaskIndex].WorkingDirectory;

            if (!Directory.Exists(SRIMOuputDir))
            {
                MessageBox.Show("It's impossible to extract files from unexisting SRIM directory! Check path " + SRIMOuputDir, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.WriteLine(string.Format("Files output path: {0}\nSRIM dir: {1}", Tasks[currentTaskIndex].WorkingDirectory, SRIMOuputDir), "ERROR");
                Trace.Flush();
                return;
            }

            if (!Directory.Exists(destDir))
            {
                Trace.WriteLine(string.Format("Files output path: {0}\nSRIM dir: {1}", Tasks[currentTaskIndex].WorkingDirectory, SRIMOuputDir), "ERROR");
                MessageBox.Show("Destination directory doesn't exist! Check path " + destDir, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Trace.Flush();
                return;
            }

            //Меняем информационное сообщение в статус-баре
            statusLabel.Text = "Extracting files...";
            await Task.Run(() => ExtractFilesFromSRIMOut(SRIMDirectory, destDir));                                //Извлекаем файлы из папки SRIMOutputs

            Tasks[currentTaskIndex].IsActive = false;
            Tasks[currentTaskIndex].Status = "Done";
            tasksList.Items.Refresh();

            if (currentTaskIndex > Tasks.Count - 1) tasksList.SelectedIndex = 0;            //Смещаем позицию курсора в списке вниз на 1 позицию
            else
                tasksList.SelectedIndex++;
            
        }

        public void UpdateButtonLabels()
        {
            if (!IsRun)
                currentTaskIndex = tasksList.SelectedIndex;

            if ((Tasks != null) && (Tasks.Count > 0) && (currentTaskIndex > -1))
            {
                //btnRunTask.IsEnabled = Tasks[currentTaskIndex].IsActive && !IsRun;
                btnRunTask.Content = "Run (" + Tasks[currentTaskIndex]?.Name + ")";

                //if (IsRun)
                //{
                //    statusLabel.Text = "Execution of task " + Tasks[currentTaskIndex].Name;
                //}
                //else
                //{
                //    statusLabel.Text = "Ready";
                //}
            }
        }

        public void LaunchTRIM()
        {
            Process TRIMProcess = new Process();
            ProcessStartInfo SI = new ProcessStartInfo();
            SI.WorkingDirectory = SRIMDirectory;
            SI.FileName = Path.Combine(SRIMDirectory, "TRIM.exe");

            TRIMProcess.StartInfo = SI;
            TRIMProcess.Start();
            if (cbAutoExtract.IsChecked.Value)
            {
                TRIMProcess.WaitForExit();
                ButtonExtractTaskClick();
            }
        }

        public void SetButtonsState(bool state)
        {
            btnAddTask.IsEnabled = state;
            btnEditTask.IsEnabled = state;
            btnRemoveTask.IsEnabled = state;
            btnSelectWorkDir.IsEnabled = state;
            btnSelect_SRIM_Dir.IsEnabled = state;
            btnRunTask.IsEnabled = state;
            btnExtractOutputFiles.IsEnabled = state;
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentTaskIndex >= Tasks.Count || Tasks.Count == 0 || currentTaskIndex == -1)
            {
                MessageBox.Show("There are no acceptable tasks! Create or choose task to calculate.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sender == btnRunTask)
            {
                RunCurrentTask();
                return;
            }
            if (sender == btnExtractOutputFiles)
            {
                ButtonExtractTaskClick();
                return;
            }
        }

    }
}
