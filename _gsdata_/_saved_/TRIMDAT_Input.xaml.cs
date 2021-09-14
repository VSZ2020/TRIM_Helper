using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using TRIM_Helper.Model;
using TRIM_Helper.Model.LayerModel;
using TRIM_Helper.Model.LayerModel.MaterialModel;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace TRIM_Helper
{
    /// <summary>
    /// Логика взаимодействия для TRIMDAT_Input.xaml
    /// </summary>
    public partial class TRIMDAT_Input : Window
    {
        private List<Element> loadedElements;
        public TrimInputFile curTrimInp;
        public TRIMDatFile curTrimDat;
        int currentSelectedLayerIndex = -1;

        private ComputationalTask mTask = null;
       
        enum TasKWndMode
		{
            CREATE, EDIT
		}

        TasKWndMode WndMode = TasKWndMode.CREATE;

        public TRIMDAT_Input(ref ComputationalTask task)
        {
            InitializeComponent();

            //Load elements from external file
            loadedElements = Preloader.LoadMaterials();

            //Apply TRIM dat and input files data to local variables
            if (task != null)
			{
                mTask = task;
                this.curTrimInp = task.TrimInput;
                this.curTrimDat = task.TrimOutput;
                tbTaskName.Text = task.Name;
                WndMode = TasKWndMode.EDIT;
            }

            InitComponentBinds();

			btnOK.Click += BtnOKCancel_Click;
            btnCancel.Click += BtnOKCancel_Click;

			btnAddLayer.Click += BtnAdd_Click;
            btnAddElement.Click += BtnAdd_Click;
            btnEditLayer.Click += BtnEdit_Click;
            btnEditElement.Click += BtnEdit_Click;
			btnRemoveLayer.Click += BtnRemove_Click;
            btnRemoveElement.Click += BtnRemove_Click;

			btnCopyInputsBuffer.Click += BtnInputsBuffer_Click;
            btnPasteFromBuffer.Click += BtnInputsBuffer_Click;

			//Popup button actions
			popbtnLayerOK.Click += PopbtnOK_Click;
            popbtnElementOK.Click += PopbtnOK_Click;
			popbtnLayerCancel.Click += PopbtnCancel_Click;
            popbtnElementCancel.Click += PopbtnCancel_Click;
        }

		private void BtnInputsBuffer_Click(object sender, RoutedEventArgs e)
		{
			if (sender == btnCopyInputsBuffer)
			{
                bool IsInputsCorrect = Read_TRIMIN_Fields() && Read_TRIMDAT_Inputs();
                if (IsInputsCorrect)
                {
                    BufferClass.BufferInputFile = this.curTrimInp;
                    BufferClass.BufferDatFile = this.curTrimDat;
                }
                else
                    MessageBox.Show("Check inputs, before copying the prefs");
			}

            if (sender == btnPasteFromBuffer)
			{
                if (BufferClass.BufferInputFile != null && BufferClass.BufferDatFile != null)
				{
					try
					{
                        this.curTrimInp = BufferClass.BufferInputFile;
                        this.curTrimDat = BufferClass.BufferDatFile;

                        //Search selected ion
                        for (int i = 0; i < loadedElements.Count; i++)
						{
                            if (loadedElements[i].ElementName == curTrimInp._Ion.ElementName)
							{
                                cmbIonsList.SelectedIndex = i;
                                break;
							}
						}

                        //Copy other parameters


                        MessageBox.Show("Successfully pasted", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch(Exception ex)
					{
                        MessageBox.Show(string.Concat("An error occured during pasting of values from buffer", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
		}

		private void BtnOKCancel_Click(object sender, RoutedEventArgs e)
		{
			if (sender == btnOK)
			{

                if (Read_TRIMIN_Fields() && Read_TRIMDAT_Inputs())
				{
                    //Если оба условия выполнены и ощибок нет, то создаем(заменяем) задачу
                    //Parse task name
                    string taskName = tbTaskName.Text;
                    if (string.IsNullOrEmpty(taskName))
					{
                        Debug.WriteLine("The task name is incorrect. Edit it.", "ERROR");
                        MessageBox.Show("Incorrect task name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
					}
                    if (WndMode == TasKWndMode.EDIT)
					{
                        mTask.TrimInput = curTrimInp;
                        mTask.TrimOutput = curTrimDat;
                        mTask.Name = taskName;
                        mTask.WorkingDirectory = MainWindow.CurrentWorkingPath + taskName + "\\";
                    }
                    else
					{
                        MainWindow.Tasks.Add(new ComputationalTask()
                        {
                            TrimInput = curTrimInp,
                            TrimOutput = curTrimDat,
                            IsActive = true,
                            Name = taskName,
                            WorkingDirectory = MainWindow.CurrentWorkingPath + taskName + "\\"
                        });
                    }
                    
                    this.Close();
				}
                else
				{
                    Debug.WriteLine("It's impossible add new task due to error(s) in input fields", "ERROR");
				}
            }
            if (sender == btnCancel)
			{
                this.Close();
			}
		}

		private void PopbtnCancel_Click(object sender, RoutedEventArgs e)
		{
			if (sender == popbtnLayerCancel)
			{
                popupLayer.IsOpen = false;
            }     
            
            if (sender == popbtnElementCancel)
			{
                popupElement.IsOpen = false;
			}
		}

		private void PopbtnOK_Click(object sender, RoutedEventArgs e)
		{
			if (sender == popbtnElementOK)
			{
                int i = lbLayersList.SelectedIndex;
                if (i > -1)
                {
					try
					{
                        Element el = popcmbElement.SelectedValue as Element;
                        double stoich = 1.0;
                        double mass = 1.0;
                        double.TryParse(poptbStoich.Text, out stoich);
                        double.TryParse(poptbMass.Text, out mass);

                        el.Stoich = stoich;
                        el.Mass = mass;
                        curTrimInp.Layers[i].Elements.Add(el);
                        popupElement.IsOpen = false;
                    }
                    catch (Exception ex)
					{
                        MessageBox.Show(ex.Message);
                        Debug.WriteLine("Can't add new element. Check input boxes", "ERROR");
                        popupElement.IsOpen = false;
					}
                    
                }
            }
		}

		private void BtnRemove_Click(object sender, RoutedEventArgs e)
		{
            if (sender == btnRemoveLayer)
			{
                if (curTrimInp.Layers.Count < 2)
				{
                    MessageBox.Show("Nothing to delete");
				}
                if (curTrimInp.Layers.Count < 2)
				{
                    curTrimInp.Layers.RemoveAt(0);
                    currentSelectedLayerIndex = -1;
				}
			}
		    if (sender == btnRemoveElement)
			{
                int selInd = lbAtomsList.SelectedIndex;
                if (currentSelectedLayerIndex > -1 && selInd > -1)
                {
                    curTrimInp.Layers[currentSelectedLayerIndex].Elements.RemoveAt(selInd);
                }
                else
                    MessageBox.Show("Firslty, choose the Layer and corresponding Element");
			}
		}

		private void BtnEdit_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void BtnAdd_Click(object sender, RoutedEventArgs e)
		{
			if (sender == btnAddLayer)
			{
                //popupLayer.IsOpen = true;
                curTrimInp.Layers.Add(new TargetLayer());
			}

            if (sender == btnAddElement)
            {
                popupElement.IsOpen = true;
                if (loadedElements.Count > 0)
                    popcmbElement.SelectedIndex = 0;
                popbtnElementOK.IsEnabled = loadedElements.Count > 0;
            }
        }

        /// <summary>
        /// Инициализация всех связей полей и загруженных или введенных данных
        /// </summary>
        private void InitComponentBinds()
		{
            if (loadedElements == null)
			{
                loadedElements = GetDefaultElementslist();
                Debug.WriteLine("Default Ion was added!", "INFO");
            }
            if (curTrimInp == null)
                curTrimInp = new TrimInputFile(Ion.GetIonFromElement(loadedElements[0]), GetDefaultLayersCollection());
            if (curTrimDat == null)
                curTrimDat = new TRIMDatFile();
            
            
            cmbIonsList.ItemsSource = loadedElements;
            cmbIonsList.DisplayMemberPath = "ElementName";

            //Binding bndIonMass = new Binding();
            //bndIonMass.ElementName = "cmbIonsList";
            //bndIonMass.Path = new PropertyPath("SelectedValue.Mass");
            //bndIonMass.Mode = BindingMode.OneWay;
            //tbIonMassInput.SetBinding(TextBox.TextProperty, bndIonMass);
            cmbIonsList.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                int index = cmbIonsList.SelectedIndex;
                if (index > -1)
                {
                    tbIonMassInput.Text = loadedElements[index].Mass.ToString();
                }
            };
            //Choose the first element at list
            if (loadedElements.Count > 0)
                cmbIonsList.SelectedIndex = 0;

            lbLayersList.ItemsSource = curTrimInp.Layers;

            popcmbElement.ItemsSource = loadedElements;
            popcmbElement.DisplayMemberPath = "ElementName";

            Binding bndpopMass = new Binding();
            bndpopMass.ElementName = "popcmbElement";
            bndpopMass.Path = new PropertyPath("SelectedValue.Mass");
            bndpopMass.Mode = BindingMode.OneWay;
            poptbMass.SetBinding(TextBox.TextProperty, bndpopMass);

            Binding bndpopStoich = new Binding();
            bndpopStoich.ElementName = "popcmbElement";
            bndpopStoich.Path = new PropertyPath("SelectedValue.Stoich");
            bndpopStoich.Mode = BindingMode.OneWay;
            poptbStoich.SetBinding(TextBox.TextProperty, bndpopStoich);

            //Check boxes binds
            //Checkbox IsIonRange file
            Binding bndcbIsRange = new Binding();
            bndcbIsRange.Source = curTrimInp;
            bndcbIsRange.Path = new PropertyPath("IsRanges");
            cbIsRangeFile.SetBinding(CheckBox.IsCheckedProperty, bndcbIsRange);

            //Checkbox IsTransmit file
            Binding bndcbIsTransmit = new Binding();
            bndcbIsTransmit.Source = curTrimInp;
            bndcbIsTransmit.Path = new PropertyPath("IsTransmitt");
            cbIsTransmitFile.SetBinding(CheckBox.IsCheckedProperty, bndcbIsTransmit);

            //Checkbox IsBackscattering file
            Binding bndcbIsBack = new Binding();
            bndcbIsBack.Source = curTrimInp;
            bndcbIsBack.Path = new PropertyPath("IsBackscatt");
            cbIsBackscatFile.SetBinding(CheckBox.IsCheckedProperty, bndcbIsBack);

            //Checkbox IsSputtering file
            Binding bndcbIsSputt = new Binding();
            bndcbIsSputt.Source = curTrimInp;
            bndcbIsSputt.Path = new PropertyPath("IsSputter");
            cbIsSputterFile.SetBinding(CheckBox.IsCheckedProperty, bndcbIsSputt);

            //Textboxes for special options in input file
            //Textbox for seed number
            Binding bndtbSeed = new Binding();
            bndtbSeed.Source = curTrimInp;
            bndtbSeed.Path = new PropertyPath("RndSeed");
            tbSeedValue.SetBinding(TextBox.TextProperty, bndtbSeed);

            //Textbox for min plot value
            Binding bndtbMinPlotValue = new Binding();
            bndtbMinPlotValue.Source = curTrimInp;
            bndtbMinPlotValue.Path = new PropertyPath("PlotDepthMin");
            tbPlotMinDepth.SetBinding(TextBox.TextProperty, bndtbMinPlotValue);

            //Textbox for max plot
            Binding bndtbMaxPlotValue = new Binding();
            bndtbMaxPlotValue.Source = curTrimInp;
            bndtbMaxPlotValue.Path = new PropertyPath("PlotDepthMax");
            bndtbMaxPlotValue.Mode = BindingMode.TwoWay;
            tbPlotMaxDepth.SetBinding(TextBox.TextProperty, bndtbMaxPlotValue);

            //Textbox for Collision code: 0 - 2
            Binding bndctbIsColl = new Binding();
            bndctbIsColl.Source = curTrimInp;
            bndctbIsColl.Path = new PropertyPath("Collisions");
            tbCollisionsCode.SetBinding(TextBox.TextProperty, bndctbIsColl);

            //=============Binding for TRIM.DAT fields=================
            //IonsCountBinding
            Binding bndIonsCount = new Binding();
            bndIonsCount.Path = new PropertyPath(".");
            bndIonsCount.Source = curTrimDat.IonsCount;
            tbIonsCount.SetBinding(TextBox.TextProperty, bndIonsCount);

            //DecimalPoints binding
            Binding bndDecimal = new Binding();
            bndDecimal.Source = curTrimDat.DecimalPoints;
            bndDecimal.Path = new PropertyPath(".");
            tbDecimalPoints.SetBinding(TextBox.TextProperty, bndDecimal);

            Binding bndComment = new Binding();
            bndComment.Source = curTrimDat.CalcComment;
            bndComment.Path = new PropertyPath(".");
            tbCommentLine.SetBinding(TextBox.TextProperty, bndComment);

            //X-Coordinate
            Binding bndCoordX = new Binding();
            bndCoordX.Source = curTrimDat.IonStartX;
            bndCoordX.Path = new PropertyPath(".");
            tbCoordX.SetBinding(TextBox.TextProperty, bndCoordX);

            //Y-Coordinate
            Binding bndCoordY = new Binding();
            bndCoordY.Source = curTrimDat.IonStartY;
            bndCoordY.Path = new PropertyPath(".");
            tbCoordY.SetBinding(TextBox.TextProperty, bndCoordY);

            //Z-Coordinate
            Binding bndCoordZ = new Binding();
            bndCoordZ.Source = curTrimDat.IonStartZ;
            bndCoordZ.Path = new PropertyPath(".");
            tbCoordZ.SetBinding(TextBox.TextProperty, bndCoordZ);

            //X-cos
            Binding bndCosX = new Binding();
            bndCosX.Source = curTrimDat.IonStartCosX;
            bndCosX.Path = new PropertyPath(".");
            tbCosX.SetBinding(TextBox.TextProperty, bndCosX);

            //Y-cos
            Binding bndCosY = new Binding();
            bndCosY.Source = curTrimDat.IonStartCosY;
            bndCosY.Path = new PropertyPath(".");
            tbCosY.SetBinding(TextBox.TextProperty, bndCosY);

            //Z-cos
            Binding bndCosZ = new Binding();
            bndCosZ.Source = curTrimDat.IonStartCosZ;
            bndCosZ.Path = new PropertyPath(".");
            tbCosZ.SetBinding(TextBox.TextProperty, bndCosZ);

            //Checkbox random coordinate X binding
            Binding bndCheckCoordX = new Binding();
            bndCheckCoordX.Source = curTrimDat;
            bndCheckCoordX.Path = new PropertyPath("IsRandomX");
            cbRndX.SetBinding(CheckBox.IsCheckedProperty, bndCheckCoordX);

            //Checkbox random coordinate Y binding
            Binding bndCheckCoordY = new Binding();
            bndCheckCoordY.Source = curTrimDat;
            bndCheckCoordY.Path = new PropertyPath("IsRandomY");
            cbRndY.SetBinding(CheckBox.IsCheckedProperty, bndCheckCoordY);

            //Checkbox random coordinate Z binding
            Binding bndCheckCoordZ = new Binding();
            bndCheckCoordZ.Source = curTrimDat;
            bndCheckCoordZ.Path = new PropertyPath("IsRandomZ");
            cbRndZ.SetBinding(CheckBox.IsCheckedProperty, bndCheckCoordZ);

            //Checkbox random cos X binding
            Binding bndCheckCosX = new Binding();
            bndCheckCosX.Source = curTrimDat.IsRandomCosX;
            bndCheckCosX.Path = new PropertyPath(".");
            cbRndCosX.SetBinding(CheckBox.IsCheckedProperty, bndCheckCosX);

            //Checkbox random cos Y binding
            Binding bndCheckCosY = new Binding();
            bndCheckCosY.Source = curTrimDat.IsRandomCosY;
            bndCheckCosY.Path = new PropertyPath(".");
            cbRndCosY.SetBinding(CheckBox.IsCheckedProperty, bndCheckCosY);

            //Checkbox random cos Z binding
            Binding bndCheckCosZ = new Binding();
            bndCheckCosZ.Source = curTrimDat.IsRandomCosZ;
            bndCheckCosZ.Path = new PropertyPath(".");
            cbRndCosZ.SetBinding(CheckBox.IsCheckedProperty, bndCheckCosZ);

            lbLayersList.SelectionChanged += LbLayersList_SelectionChanged;
            //Selected the first default layer if it exists
            if (curTrimInp.Layers.Count > 0)
                lbLayersList.SelectedIndex = 0;

            //Select ion from the list (for task edit case)
            if (WndMode == TasKWndMode.EDIT && curTrimInp != null && curTrimInp._Ion != null)
            {
                bool foundIon = false;
                for (int i = 0; i < cmbIonsList.Items.Count; i++)
                    if (curTrimInp._Ion.ElementName == loadedElements[i].ElementName)
                    {
                        cmbIonsList.SelectedIndex = i;
                        foundIon = true;
                        break;
                    }
                if (!foundIon)
                {
                    loadedElements.Add(curTrimInp._Ion);
                }
                tbIonMassInput.Text = curTrimInp._Ion.Mass.ToString();
                tbIonEnergyInput.Text = curTrimInp._Ion.Energy.ToString();

                //Set custom target dimensions
                cbAutoTargetDepth.IsChecked = false;
                cbAutoTargetWidth.IsChecked = false;
                tbTargetLimitsInput.Text = curTrimDat.Target.Depth.Min.ToString() + ";" + curTrimDat.Target.Depth.Max.ToString();
                tbTargetWidthLimitsInput.Text = curTrimDat.Target.Width.Min.ToString() + ";" + curTrimDat.Target.Width.Max.ToString();
            }
        }

		private void LbLayersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (lbLayersList.SelectedIndex > -1)
			{
                lbAtomsList.ItemsSource = curTrimInp.Layers[lbLayersList.SelectedIndex].Elements;
                currentSelectedLayerIndex = lbLayersList.SelectedIndex;
            }
		}

		/// <summary>
		/// Reading of the whole inputs and checking it
		/// </summary>
		private bool CheckIonFields()
		{
            //Reading of Ion Inputs
            if (cmbIonsList.SelectedIndex == -1)
			{
                Debug.WriteLine("The ion was not selected from the list", "TRIM.IN Window");
                return false;
            }
            
            double bufMass = 1.0;                                   //Default Mass value if an error will be
            double bufEnergy = 1000.0;                              //Default Energy value if an error will be
            bool IsPassed = double.TryParse(tbIonMassInput.Text, out bufMass) && double.TryParse(tbIonEnergyInput.Text, out bufEnergy);
            if (!IsPassed)
                return false;
            curTrimInp._Ion = Ion.GetIonFromElement(cmbIonsList.Items[cmbIonsList.SelectedIndex] as Element, bufEnergy);
            curTrimInp._Ion.Mass = bufMass;
            //curTrimInp._Ion.Energy = bufEnergy;
            return true;
		}
        private bool Read_TRIMIN_Fields()
		{
            if (!CheckIonFields())
                return false;

            //Check Input Layers
            for (int i = 0; i < curTrimInp.Layers.Count; i++)
            {
                if (curTrimInp.Layers[i].Elements == null || curTrimInp.Layers[i].Elements.Count < 1)
                {
                    Debug.WriteLine("There are no elements in Layer: " + curTrimInp.Layers[i].LayerName, "ERROR");
                    MessageBox.Show("You should add one more elements to the layer " + curTrimInp.Layers[i].LayerName, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                //Check Layer Properties
                if (string.IsNullOrEmpty(curTrimInp.Layers[i].LayerName))
				{
                    MessageBox.Show("Enter layer \'" + curTrimInp.Layers[i].LayerName + "\' name", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
				}
                //Checking of Layer Elements
                //for (int j = 0; j < curTrimInp.Layers[i].Elements.Count; j++)
                //{
                //    var element = curTrimInp.Layers[i].Elements[j];

                //}
            }
            //Set additional parameters
            curTrimInp.Cascades = 4;
            curTrimInp.PlotType = 5;
            return true;
		}

        /// <summary>
        /// Reading of TRIM.DAT boxes and checking rules for values
        /// </summary>
        /// <returns></returns>
        private bool Read_TRIMDAT_Inputs()
		{
            //Check input fields empty
            if (string.IsNullOrEmpty(tbCalcRowName.Text) || 
                string.IsNullOrEmpty(tbIonsCount.Text) || 
                string.IsNullOrEmpty(tbDecimalPoints.Text) ||
                string.IsNullOrEmpty(tbTargetLimitsInput.Text) ||
                string.IsNullOrEmpty(tbZenithAngleLimitsInput.Text) ||
                string.IsNullOrEmpty(tbAzimuthAngleLimitsInput.Text))
			{
                Debug.WriteLine("One of the input filed is empty or whitespace!", "ERROR");
                return false;
			}
            if (curTrimDat == null)
                curTrimDat = new TRIMDatFile();
			//Reading inputs
			try
			{
                curTrimDat.IonsCount = int.Parse(tbIonsCount.Text);
                curTrimDat.DecimalPoints = int.Parse(tbDecimalPoints.Text);
                curTrimDat.IonStartX = double.Parse(tbCoordX.Text);
                curTrimDat.IonStartY = double.Parse(tbCoordY.Text);
                curTrimDat.IonStartZ = double.Parse(tbCoordZ.Text);
                curTrimDat.IonStartCosX = double.Parse(tbCosX.Text);
                curTrimDat.IonStartCosY = double.Parse(tbCosY.Text);
                curTrimDat.IonStartCosZ = double.Parse(tbCosZ.Text);
            }
            catch(Exception ex) 
			{
                Debug.WriteLine("Error in one of the input field", "ERROR");
                return false;
			}

            //Read angle constricts
            curTrimDat.Angles = ReadAngleInputs();
            //ReadTarget limits
            curTrimDat.Target = ReadTargetSize(cbAutoTargetDepth.IsChecked.Value, cbAutoTargetWidth.IsChecked.Value);

            //Set ions to count. this value is required only for progress bar in TRIM window.
            curTrimInp.Number = curTrimDat.IonsCount;

            //Set text pattern format for ion name rows
            curTrimDat.IonRowName = tbCalcRowName.Text;
            if (curTrimDat.IonRowName.Length < 7)
            {
                int unsufficinentCounts = 7 - curTrimDat.IonRowName.Length;
                for (int i = 0; i < unsufficinentCounts; i++)
                {
                    curTrimDat.IonRowName += " ";
                }
            }
            else
            {
                curTrimDat.IonRowName = curTrimDat.IonRowName.Substring(0, 7);
            }
            return true;
        }

        public IonTarget ReadTargetSize(bool IsAutoDepth, bool IsAutoWidthAndHeight)
        {
            IonTarget target = new IonTarget();                         //Create Taget Instance
            double sum = curTrimInp.GetLayersSumDepth();                //Get layers sum width
            if (IsAutoDepth)
			{
                target.Depth.Min = 0;
                target.Depth.Max = sum;
            }
            else
            {
                //Size-X read
                var buf = tbTargetLimitsInput.Text.Split(';');
                if (buf.Length > 1)
                {
                    double.TryParse(buf[0], out target.Depth.Min);
                    double.TryParse(buf[1], out target.Depth.Max);
                }

            }
            //Check AUTO parameter for Width and Height of target
            if (IsAutoWidthAndHeight)
            {
                double diff = sum / 2.0;
                target.Width.Max = diff;
                target.Width.Min = -diff;
                target.Height.Max = diff;
                target.Height.Min = -diff;
            }
            else
            {
                var buf = tbTargetWidthLimitsInput.Text.Split(';');
                if (buf.Length > 1)
                {
                    double.TryParse(buf[0], out target.Width.Min);
                    double.TryParse(buf[1], out target.Width.Max);

                    target.Height.Max = target.Width.Max;
                    target.Height.Min = target.Width.Min;
                }
            }
            
            return target;
        }
        public AngleLimits ReadAngleInputs()
        {
            var limits = new AngleLimits();
            //Read angles
            var buf = tbZenithAngleLimitsInput.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out limits.Zenith.Min);
                double.TryParse(buf[1], out limits.Zenith.Max);
            }
            buf = tbAzimuthAngleLimitsInput.Text.Split(';');
            if (buf.Length > 1)
            {
                double.TryParse(buf[0], out limits.Azimuth.Min);
                double.TryParse(buf[1], out limits.Azimuth.Max);
            }
            return limits;
        }

        private static ObservableCollection<TargetLayer> GetDefaultLayersCollection()
		{
            List<Element> defElements = GetDefaultElementslist();
            defElements[3].Stoich = 4.0;        //O-16
            var defElementsForMonazite = new ObservableCollection<Element>() { defElements[5], defElements[4], defElements[3] };

            defElements = GetDefaultElementslist();
            defElements[0].Stoich = 14.0;        //H-1
            defElements[2].Stoich = 16.0;        //C-12
            defElements[3].Stoich = 3.0;        //O-16
            var defElementsForLexan = new ObservableCollection<Element>() { defElements[0], defElements[2], defElements[3] };
            ObservableCollection<TargetLayer> Layers = new ObservableCollection<TargetLayer> { new TargetLayer() {
                    LayerName = "Monazite sand",
                    Density = 4.6,
                    IsGas = false,
                    Depth = 335000,
                    Elements = defElementsForMonazite },
            new TargetLayer() {
                LayerName = "Lexan (ICRU-219)",
                Density = 1.2,
                IsGas = false,
                Depth = 30000,
                Elements = defElementsForLexan,
                CompoundCorrection = 0.9776439445662715531335783332
            } };

            Debug.WriteLine("Default Layers was added!", "INFO");
            return Layers;
        }

        private static List<Element> GetDefaultElementslist()
		{
            Element H = new Element { ElementName = "H", Z = 1, Mass = 1.008, E_d = 10, lattice = 3, surface = 2.0 };
            Element He  = new Element { ElementName = "He", Z = 2, Mass = 4.003, E_d = 5.0, lattice = 1.0, surface = 2.0 };
            Element C = new Element { ElementName = "C", Z = 6, Mass = 12.011, E_d = 28, lattice = 3.0, surface = 7.41 };
            Element O   = new Element { ElementName = "O", Z = 8, Mass = 15.999, E_d = 28, lattice = 3.0, surface = 2.0 };
            Element P   = new Element { ElementName = "P", Z = 15, Mass = 30.974, E_d = 25.0, lattice = 3.0, surface = 3.27 };
            Element Th  = new Element { ElementName = "Th", Z = 90, Mass = 232, E_d = 25.0, lattice = 3.0, surface = 5.93 };
            return new List<Element> { H, He, C, O, P, Th };
        }
        public static ComputationalTask GetDefaultTask(string workDir, string taskName, double layerdepth)
		{
            var elList = GetDefaultElementslist();
            ComputationalTask task = new ComputationalTask();
            Ion ion = Ion.GetIonFromElement(elList[1]);
            ion.Energy = 6778;
            var layers = GetDefaultLayersCollection();
            layers[1].Depth = layerdepth;
            task.TrimInput = new TrimInputFile(ion, layers);
            task.TrimInput.Cascades = 4;
            task.TrimInput.Number = 10000;
            task.TrimInput.PlotDepthMin = 0;
            task.TrimInput.StoppingPowerVersion = 0;
            task.TrimInput.IsRanges = true;
            task.TrimInput.IsBackscatt = true;
            task.TrimInput.IsTransmitt = true;
            task.TrimInput.Collisions = 0;

            task.TrimOutput = new TRIMDatFile();
            task.TrimOutput.IsRandomCosX = true;
            task.TrimOutput.IsRandomCosY = true;
            task.TrimOutput.IsRandomCosZ = true;
            task.TrimOutput.IsRandomX = true;
            task.TrimOutput.IsRandomY = true;
            task.TrimOutput.IsRandomZ = true;
            task.TrimOutput.Target = new IonTarget();
            var target = task.TrimOutput.Target;
            target.Depth.Max = 335000;      double diff = 335000.0 / 2.0;
            target.Width.Max = diff;    target.Width.Min = -diff;
            target.Height.Max = diff;   target.Height.Min = -diff;

            var ang = task.TrimOutput.Angles;
            ang = new AngleLimits();
            ang.Azimuth.Max = 360;
            ang.Zenith.Max = 180;

            task.TrimOutput.IonRowName = "Ion";
            task.TrimOutput.DecimalPoints = 5;
            task.WorkingDirectory = workDir;
            task.IsActive = true;
            task.Name = taskName;
            return task;
        }
        private void UpdateMaxPlotValue()
		{
            if (curTrimInp != null)
			{
                if (curTrimInp.Layers != null)
				{
                    try
                    {
                        curTrimInp.PlotDepthMax = (float)curTrimInp.GetLayersSumDepth();

                    }
                    catch(Exception ex)
					{
                        Debug.WriteLine(ex.Message, "ERROR");
					}

                }
			}
			
		}
    }

}
