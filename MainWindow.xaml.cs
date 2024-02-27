using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TRIM_Helper.Model;

namespace TRIM_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static System.Windows.Controls.TextBlock statusLabelCtrl;

        public MainWindow()
        {
            InitializeComponent();

            statusLabelCtrl = statusLabel;

            Tasks = new ObservableCollection<ComputationalTask>();

            btnAddTask.Click += BtnTask_Click;
            btnEditTask.Click += BtnTask_Click;
            btnRemoveTask.Click += BtnTask_Click;
            btnRecreateTasks.Click += BtnTask_Click;
            btnSetIonsCountToAll.Click += BtnTask_Click;
            btnExtractOutputFiles.Click += Button_Click;
            btnRunTask.Click += Button_Click;

            tasksList.SelectionChanged += TasksList_SelectionChanged;
            tasksList.MouseDoubleClick += TasksList_MouseDoubleClick;

            prgBar.Maximum = 100;

            RestoreTrimPath();
            tbSRIM_Path.TextChanged += TRIM_PathBox_TextChanged;
            tbWorkingPath.TextChanged += TRIM_PathBox_TextChanged;
            btnSelectWorkDir.Click += ButtonClick;
            btnSelect_SRIM_Dir.Click += ButtonClick;
            btnExtremeUnblockAllButtons.Click += ExtraButtonClick;

            this.Closing += MainWindow_Closing;
            tasksList.ItemsSource = Tasks;

            InitHotkeys();
            CreateDefaultTasks();
            UpdateButtonLabels();
            SetDebugListeners();

            
        }

        private void TasksList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TaskEditAction();
        }

        private void SetDebugListeners()
        {
            TextWriterTraceListener tr1 = new TextWriterTraceListener(Console.Out);
            TextWriterTraceListener tr2 = new TextWriterTraceListener(File.AppendText("Log.txt"));
            Trace.Listeners.Add(tr1);
            Trace.Listeners.Add(tr2);
            Trace.WriteLine("================" + DateTime.Now.ToString() + "================");
        }

        private void TRIM_PathBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (sender == tbSRIM_Path)
                SRIMDirectory = tbSRIM_Path.Text;
            if (sender == tbWorkingPath)
                CurrentWorkingPath = tbWorkingPath.Text;
        }

        private void BtnTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnAddTask)
            {
                TaskAddAction();
            }
            if (sender == btnEditTask)
            {
                TaskEditAction();   
            }
            if (sender == btnRemoveTask)
            {
                TaskRemoveAction();
            }
            if (sender == btnRecreateTasks)
            {
                RecreateTasksButtonclick();
            }

            if (sender == btnSetIonsCountToAll)
            {
                InputIonsCountBox inputBox = new InputIonsCountBox();
                inputBox.ShowDialog();

                if (InputIonsCountBox.ionsCount > 0)
                {
                    SetTasksIonCount(InputIonsCountBox.ionsCount);
                }
            }
        }

        private void TaskAddAction()
        {
            ComputationalTask tsk = null;
            TRIMDAT_Input addTaskWnd = new TRIMDAT_Input(ref tsk);
            addTaskWnd.CommandBindings.Add(new CommandBinding(HotKeys.cancelTaskWnd, (object sender, ExecutedRoutedEventArgs e) => addTaskWnd.Close()));
            addTaskWnd.ShowDialog();
        }

        private void TaskEditAction()
        {
            if (currentTaskIndex > -1)
            {
                ComputationalTask tsk = Tasks[currentTaskIndex];
                TRIMDAT_Input editTaskWnd = new TRIMDAT_Input(ref tsk);
                editTaskWnd.ShowDialog();
            }
            tasksList.Items.Refresh();
        }

        private void TaskRemoveAction()
        {
            if (Tasks.Count < 1)
            {
                MessageBox.Show("Nothing to delete!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int taskIndex = tasksList.SelectedIndex;
            if (taskIndex > -1)
            {
                Tasks.RemoveAt(taskIndex);
                if (currentTaskIndex == taskIndex)
                {
                    if (currentTaskIndex > 0)
                        currentTaskIndex--;
                    else
                        currentTaskIndex = 0;
                }
            }
        }

        private void RecreateTasksButtonclick()
        {
            //Remove all tasks
            if (Tasks != null && Tasks.Count>0)
            {
                while (Tasks.Count > 0)
                {
                    Tasks.RemoveAt(0);
                }
            }
            //Creation of new Tasks list
            CreateDefaultTasks();
        }

        private void SetTasksIonCount(int IonsCount)
        {
            if (Tasks != null)
            {
                for (int t = 0; t < Tasks.Count; t++)
                {
                    ref TRIMDatFile tDat = ref Tasks[t].TrimOutput;
                    ref TrimInputFile tInp = ref Tasks[t].TrimInput;
                    if (tDat != null && tInp != null)
                    {
                        tDat.IonsCount = IonsCount;
                        tInp.Number = IonsCount;
                    }
                    else
                        Trace.WriteLine("Error during changing ions count for Task " + Tasks[t].Name + "and depth " + tDat.Target.Depth.ToString() + " A.");
                }
                MessageBox.Show("Successfully completed!");
            }
            tasksList.Items.Refresh();
        }
        private void TasksList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateButtonLabels();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TRIM_Helper.Properties.Settings.Default.WorkingDir = tbWorkingPath.Text;
            TRIM_Helper.Properties.Settings.Default.SRIMDir = tbSRIM_Path.Text;
            TRIM_Helper.Properties.Settings.Default.Save();
        }
        public void RestoreTrimPath()
        {
            tbWorkingPath.Text = TRIM_Helper.Properties.Settings.Default.WorkingDir;
            tbSRIM_Path.Text = TRIM_Helper.Properties.Settings.Default.SRIMDir;
            CurrentWorkingPath = tbWorkingPath.Text;
            SRIMDirectory = tbSRIM_Path.Text;
        }

        public void ButtonClick(object sender, RoutedEventArgs e)
        {

            if (sender == btnSelectWorkDir)
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Title = "Enter any file name in working folder";
                if (dialog.ShowDialog().Value)
                {
                    tbWorkingPath.Text = Path.GetDirectoryName(dialog.FileName) + "\\";
                    CurrentWorkingPath = tbWorkingPath.Text;
                }

                return;
            }
            if (sender == btnSelect_SRIM_Dir)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Title = "Choose any file in SRIM folder";
                if (dialog.ShowDialog().Value)
                {
                    tbSRIM_Path.Text = Path.GetDirectoryName(dialog.FileName) + "\\";
                    SRIMDirectory = tbSRIM_Path.Text;
                }
                return;
            }
        }
        private void CreateDefaultTasks()
        {
            //Список толщин поглотителя
            double[] Depth = new double[] { 0.0, 3, 6, 9, 12, 15, 18, 21, 24, 30, 33, 36, 42, 51, 72, 75, 78 };             //Толщина в мкм
            //Список названий нуклидов
            string[] NuclideName = new string[] { "Th-232", "Th-228", "Ra-224", "Rn-220", "Po-216", "Bi-212", "Po-212" };
            //Список энергий нуклидов в KeV
            double[] NuclideEnergy = new double[] { 4012, 5523, 5686, 6288, 6778, 6051, 8785 };

            for (int j = 0; j < Depth.Length; j++)
            {
                double layerDepth = Depth[j];                                                                    
                for (int i = 0; i < 7; i++)
                {
                    ComputationalTask task = TRIMDAT_Input.GetDefaultTask(Path.Combine(CurrentWorkingPath, layerDepth.ToString() + " um", NuclideName[i]), NuclideName[i], layerDepth * 10000.0, NuclideEnergy[i]);
                    Tasks.Add(task);
                }
            }
        }

        public void InitHotkeys()
        {
            HotKeys.runTaskCmd.InputGestures.Add(new KeyGesture(Key.F5, ModifierKeys.None));
            HotKeys.extractTaskCmd.InputGestures.Add(new KeyGesture(Key.F7, ModifierKeys.None));

            HotKeys.addTaskCmd.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            HotKeys.editTaskCmd.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            HotKeys.deleteTaskCmd.InputGestures.Add(new KeyGesture(Key.Delete, ModifierKeys.None));

            HotKeys.cancelTaskWnd.InputGestures.Add(new KeyGesture(Key.Escape, ModifierKeys.None));

            CommandBindings.Add(new CommandBinding(HotKeys.runTaskCmd, (object sender, ExecutedRoutedEventArgs e) => RunCurrentTask()));
            CommandBindings.Add(new CommandBinding(HotKeys.extractTaskCmd, (object sender, ExecutedRoutedEventArgs e) => ButtonExtractTaskClick()));

            CommandBindings.Add(new CommandBinding(HotKeys.addTaskCmd, (object sender, ExecutedRoutedEventArgs e) => TaskAddAction()));
            CommandBindings.Add(new CommandBinding(HotKeys.editTaskCmd, (object sender, ExecutedRoutedEventArgs e) => TaskEditAction()));
            CommandBindings.Add(new CommandBinding(HotKeys.deleteTaskCmd, (object sender, ExecutedRoutedEventArgs e) => TaskRemoveAction()));
        }

        public void ExtraButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == btnExtremeUnblockAllButtons)
            {
                btnAddTask.IsEnabled = true;
                btnEditTask.IsEnabled = true;
                btnRemoveTask.IsEnabled = true;
                btnSelectWorkDir.IsEnabled = true;
                btnSelect_SRIM_Dir.IsEnabled = true;
                btnRunTask.IsEnabled = true;
                btnExtractOutputFiles.IsEnabled = true;
            }
        }
    }
}
