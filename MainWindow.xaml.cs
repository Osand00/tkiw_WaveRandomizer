using Microsoft.VisualBasic;
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
        }

        private void runGen_btn_Click(object sender, RoutedEventArgs e)
        {
            List<double> waveStrengths = new List<double>();
            Dictionary<string, double> enemyUnits = new Dictionary<string, double>();
            int numOfColsOut = 0;
            //Creation of enemyUnits Dictionary
            //verify filepath
            if (File.Exists(filePath_tbx.Text))
            {
                //create enemy unit map
                enemyUnits = LoadEnemyUnits(filePath_tbx.Text);
            }
            else { throw new Exception("File path given is invalid"); }
            //Determine Wave Generation
            waveStrengths = WaveStrengthGen(strengthAlgo_cbo.SelectedItem.ToString());
            //Build Wave_presets_village
            Dictionary<int, List<string>> csvRows = WavePresetString(enemyUnits, waveStrengths, densityAlgo_cbo.SelectedItem.ToString(), ref numOfColsOut);
        }
        private static Dictionary<string, double> LoadEnemyUnits(string filePath)
        {
            Dictionary<string, double> enemyUnits = new Dictionary<string, double>();
            string[] fullFileContent = File.ReadAllLines(filePath);
            //string[] unitFileHeaders = fullFileContent[0].Split(',');                         could be used to replace fullFileContent[0].Split(','). feels not worth
            string[] lineArr = new string[fullFileContent[0].Split(',').Length];
            int unitIdCol = 0, unitHpCol = 0, unitDmgCol = 0, unitAtkSpdCol = 0, unitDpsCol = 0, unitFactionCol = 0;
            double healthPower = 0.025;
            double dpsPower = 0.4;

            //get col indices
            for (int i = 0; i < fullFileContent[0].Split(',').Length; i++)
            {
                switch (fullFileContent[0].Split(',')[i])
                {
                    case "id":
                        unitIdCol = i; break;
                    case "Health":
                        unitHpCol = i; break;
                    case "Dmg":
                        unitDmgCol = i; break;
                    case "Atk Speed":
                        unitAtkSpdCol = i; break;
                    case "dps":
                        unitDpsCol = i; break;
                    case "Faction":
                        unitFactionCol = i; break;
                }
            }
            //add to dictionary
            foreach (string line in fullFileContent)
            {
                //skip headers and skip non enemies
                if (line == fullFileContent[0] || !line.Split(',')[unitFactionCol].Equals("enemy"))
                {
                    continue;
                }
                enemyUnits.Add(line.Split(',')[unitIdCol],
                    Double.Round(Convert.ToDouble(line.Split(',')[unitHpCol]) * healthPower + Convert.ToDouble(line.Split(',')[unitDpsCol]) * dpsPower, 2));
            }
            return enemyUnits;
        }
        
        private static List<double> WaveStrengthGen(string? genType)
        {
            List<double> waveStrengths = new List<double>();
            switch (genType)
            {
                case "Default":
                    waveStrengths.AddRange([
                        10, 10, 10, 10, 20, 20, 20, 20, 45, 45,
                        45, 45, 45, 45, 45, 45, 45, 45, 45, 120,
                        120, 120, 210, 210, 210, 210, 210, 300, 300, 300,
                        300, 300, 300, 300, 165, 450
                    ]); break;
                default:
                    throw new Exception("Failed to provide acceptable Wave Strength Algo");
            }
            return waveStrengths;
        }

        private static Dictionary<int, List<string>> WavePresetString(Dictionary<string, double> enemyUnits, List<double> waveStrengths, string? densityAlgo, ref int numOfColsOut)
        {
            Dictionary<int, List<string>> csvOut = new Dictionary<int, List<string>>();
            int key = 1;
            int currentRowTotalCol = 0;
            int unitTarget = 0;
            double unitMeanStrength = 0;
            //build the waves
            foreach (double wave in waveStrengths)
            {
                //check what desity method we will use to determine the number of units we want
                switch (densityAlgo)
                {
                    case "linear":
                        unitTarget = TotalUnitTargetLinear(wave); break;
                    default :
                        throw new Exception("Density Algo not given");
                }
                //generate a unit along a normal distribtion of unit target over wave stregth
                unitMeanStrength = wave / Convert.ToDouble(unitTarget);
                for (double i = 0; i < wave - wave*.10;)
                {
                    if (csvOut.ContainsKey(key))
                    {
                        csvOut[key].Add(GenNormalDistroUnit(enemyUnits, unitMeanStrength));
                    }
                    else
                    {
                        csvOut.Add(key, [GenNormalDistroUnit(enemyUnits, unitMeanStrength)]);
                    }
                    i = i + enemyUnits[csvOut[key].Last<string>()];
                }
            }
            return csvOut;
        }

        private static int TotalUnitTargetLinear(double waveStrength)
        {
            return Convert.ToInt32(Double.Round(.2*waveStrength, 0));
        }

        //TODO
        private static string GenNormalDistroUnit(Dictionary<string, double> enemyUnits, double unitMeanStrength)
        {
            //TODO Build method to pull unit based on a normal distro around unitMeanStrength
            return "";
        }
    }
}