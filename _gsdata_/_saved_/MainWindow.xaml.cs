using System;
using System.Collections.ObjectModel;
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
        public MainWindow()
        {
            InitializeComponent();

            Tasks = new ObservableCollection<ComputationalTask>();

			btnAddTask.Click += BtnTask_Click;
            btnEditTask.Click += BtnTask_Click;
            btnRemoveTask.Click += BtnTask_Click;
            btnExtractOutputFiles.Click += Button_Click;
            btnRunTask.Click += Button_Click;
            tasksList.SelectionChanged += TasksList_SelectionChanged;

            prgBar.Maximum = 100;

            RestoreTrimPath();
			tbSRIM_Path.TextChanged += TRIM_PathBox_TextChanged;
            tbWorkingPath.TextChanged += TRIM_PathBox_TextChanged;
            btnSelectWorkDir.Click += ButtonClick;
            btnSelect_SRIM_Dir.Click += ButtonClick;

            this.Closing += MainWindow_Closing;
            tasksList.ItemsSource = Tasks;
            //Testing
            //NaturalNuclides();
            UpdateButtonLabels();
            CreateDefaultTasks();
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
                ComputationalTask tsk = null;
                TRIMDAT_Input addTaskWnd = new TRIMDAT_Input(ref tsk);
                addTaskWnd.ShowDialog();
			}
            if (sender == btnEditTask)
			{
                if (currentTaskIndex > -1)
				{
                    ComputationalTask tsk = Tasks[currentTaskIndex];
                    TRIMDAT_Input editTaskWnd = new TRIMDAT_Input(ref tsk);
                    editTaskWnd.ShowDialog();
                }
                
            }
            if (sender == btnRemoveTask)
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
                    tbWorkingPath.Text = Path.GetDirectoryName(dialog.FileName) + "\\" ;
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
                }
                return;
            }
        }
        private void CreateDefaultTasks()
		{
            double d = 0;
            while (d <= 33)
			{
                string taskName = d.ToString() + " um";
                ComputationalTask task = TRIMDAT_Input.GetDefaultTask(CurrentWorkingPath + taskName + "\\", taskName, d * 10000);
                Tasks.Add(task);
                d += 3;
			}
		}
    }
}
