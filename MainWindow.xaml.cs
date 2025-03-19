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
            //generate the waves 
            Generator.WaveUnitGen(densityAlgo_cbo.SelectedItem.ToString() ?? "");
            //Build Wave_presets_village
            try
            {
                Generator.WriteCsvToFile(FolderPathOut_tbx.Text);
            }
            catch
            {
                throw new Exception("failed writing file");
            }
        }
    }
}