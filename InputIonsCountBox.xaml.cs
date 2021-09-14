using System.Windows;

namespace TRIM_Helper
{
    /// <summary>
    /// Логика взаимодействия для InputIonsCountBox.xaml
    /// </summary>
    public partial class InputIonsCountBox : Window
    {
        static public int ionsCount = 10000;
        
        public InputIonsCountBox()
        {
            InitializeComponent();
            btnOK.Click += Btn_Click;
            btnCancel.Click += Btn_Click;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnOK)
            {
                CheckIonsInput();
            }
            if (sender == btnCancel)
            {
                ionsCount = 0;
                this.Close();
            }
        }

        private void CheckIonsInput()
        {
            int ions = 0;
            if (int.TryParse(tbIonsCount.Text, out ions))
            {
                ionsCount = ions;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid ions count value! Value should be number and greater than 10. Please, input again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
