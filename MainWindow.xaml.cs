using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CsvHelper;

namespace tkiw_WaveRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            densityAlgo_cbo.Items.Add("linear");
            densityAlgo_cbo.SelectedItem = "linear";
            strengthAlgo_cbo.Items.Add("Default");
            strengthAlgo_cbo.SelectedItem = "Default";
            hpCoef_tbx.Text = "0.02";
            dmgCoef_tbx.Text = "0.3";
            filePath_tbx.Visibility = filePath_lbn.Visibility = UseUnitFilePreLoad_cbx.IsChecked.GetValueOrDefault() ? Visibility.Hidden : Visibility.Visible;
            filePathBouns_txt.Visibility = filePathBouns_lbn.Visibility = UseDefaultBouns_cbx.IsChecked.GetValueOrDefault() ? Visibility.Hidden : Visibility.Visible;
            hpCoef_tbx.Visibility = hpCoef_lbl.Visibility = ChangeHpCof_cbx.IsChecked.GetValueOrDefault() ? Visibility.Visible : Visibility.Hidden;
            dmgCoef_tbx.Visibility = dmgCoef_lbl.Visibility = ChangeDmgCof_cbx.IsChecked.GetValueOrDefault() ? Visibility.Visible : Visibility.Hidden;
        }

        private void UseUnitFilePreLoad_cbx_Checked(object sender, RoutedEventArgs e)
        {
            filePath_tbx.Visibility = filePath_lbn.Visibility = UseUnitFilePreLoad_cbx.IsChecked.GetValueOrDefault() ? Visibility.Hidden : Visibility.Visible;
            filePath_tbx.Text = "";
        }

        private void UseDefaultBouns_cbx_Checked(object sender, RoutedEventArgs e)
        {
            filePathBouns_txt.Visibility = filePathBouns_lbn.Visibility = UseDefaultBouns_cbx.IsChecked.GetValueOrDefault() ? Visibility.Hidden : Visibility.Visible;
            filePathBouns_txt.Text = "";
        }

        private void ChangeHpCof_cbx_Checked(object sender, RoutedEventArgs e)
        {
            hpCoef_tbx.Visibility = hpCoef_lbl.Visibility = ChangeHpCof_cbx.IsChecked.GetValueOrDefault() ? Visibility.Visible : Visibility.Hidden;
            hpCoef_tbx.Text = ChangeHpCof_cbx.IsChecked.GetValueOrDefault() ? hpCoef_tbx.Text : "0.02";
        }

        private void ChangeDmgCof_cbx_Checked(object sender, RoutedEventArgs e)
        {
            dmgCoef_tbx.Visibility = dmgCoef_lbl.Visibility = ChangeDmgCof_cbx.IsChecked.GetValueOrDefault() ? Visibility.Visible : Visibility.Hidden;
            dmgCoef_tbx.Text = ChangeDmgCof_cbx.IsChecked.GetValueOrDefault() ? dmgCoef_tbx.Text : "0.3";
        }

        private void RunGen_btn_Click(object sender, RoutedEventArgs e)
        {
            WaveGenHelper Generator = new WaveGenHelper();
            //load unit data
            Generator.LoadEnemyUnits(filePath_tbx.Text);
            //load bonuses
            Generator.LoadEnemyUnitsBonus(filePathBouns_txt.Text);
            //generate wave stregths
            Generator.WaveStrengthGen(strengthAlgo_cbo.SelectedItem.ToString() ?? "");

            //Generates values needed for endless wave generation 
            //and
            //Builds corresponding Wave_template_village
            /**TODO: 
            1. Divide this into 2 methods
            2. Make a button to innitially hide the Prophecy amount and weeks between waves
            3. Make a check to see if the file path is valid
            **/
            Generator.GenerateVillageData(TemplateFolderPathOut_tbx.Text,maxProphecy_tbx.Text, waveChange_tbx.Text);

            //generate the waves 
            /**
            TODO:
            1.make dragon spawn only on the last wave;
            2.Compress the unit wave lenght, instead of 
            goblin_boss,1,goblin_boss,1,goblin_boss,1
            make it output goblin_boss,3
            **/
            Generator.WaveUnitGen(densityAlgo_cbo.SelectedItem.ToString() ?? "");
            //Build Wave_presets_village
            try
            {
                Generator.WriteCsvToPresetFile(PresetFolderPathOut_tbx.Text);
            }
            catch
            {
                throw new Exception("failed writing file");
            }
        }

        private void maxWave_tbx_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}