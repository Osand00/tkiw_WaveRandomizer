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
            filePath_tbx.Text = @"..\..\..\unit.csv";
            densityAlgo_cbo.Items.Add("Linear");
            densityAlgo_cbo.Items.Add("Normal");
            densityAlgo_cbo.SelectedItem = "Linear";
            strengthAlgo_cbo.Items.Add("Default");
            strengthAlgo_cbo.Items.Add("Hardcore");
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
        private Dictionary<string, double> LoadEnemyUnits(string filePath)
        {
            Dictionary<string, double> enemyUnits = new Dictionary<string, double>();
            string[] fullFileContent = File.ReadAllLines(filePath);
            //string[] unitFileHeaders = fullFileContent[0].Split(',');                         could be used to replace fullFileContent[0].Split(','). feels not worth
            string[] lineArr = new string[fullFileContent[0].Split(',').Length];
            int unitIdCol = 0, unitHpCol = 0, unitDmgCol = 0, unitAtkSpdCol = 0, unitDpsCol = 0, unitFactionCol = 0;
            double healthPower = 0, dpsPower = 0;


            //checks if the values given by the user are valid, if so saves them in healhPower and dpsPower
            if (Double.TryParse(hpCoef_tbx.Text, out healthPower)){
            }
            else{
                throw new Exception("Invalid input for health power.");
            }
            if (Double.TryParse(dmgCoef_tbx.Text, out dpsPower)){
            }
            else{
                throw new Exception("Invalid input for dps power.");
            }



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
                case "Hardcore":
                    waveStrengths.AddRange([
                        45, 45, 45, 45, 90, 90, 90, 90, 180, 180,
                        180, 180, 180, 180, 180, 180, 180, 180, 180, 360,
                        360, 360, 420, 420, 420, 420, 420, 600, 600, 600,
                        600, 600, 600, 600, 330, 900
                    ]);break;
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
                    case "Linear":
                        unitTarget = TotalUnitTargetLinear(wave);
                        break;
                    case "Normal":
                        throw new Exception("Density Algo not implamented");
                    //unitTarget = TotalUnitTargetNormal(wave);
                    //break;
                    default:
                        throw new Exception("Density Algo not given");
                }

                //generate a unit along a normal distribtion of unit target over wave stregth
                unitMeanStrength = wave / Convert.ToDouble(unitTarget);
                for (double i = 0; i < wave - wave*.10;)//runs until 90% of the wave is filled
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
            double stdDev = 1;
            double unitpower = NextDistributed(unitMeanStrength, stdDev, BoxMuller);
            //find unit closest to unitpower and return that unit
            return "";
        }

        //func double double doube means it needs a method that takes 2 doubles and returns a double
        public static double NextDistributed(double mean, double stdDev, Func<double, double, double> transform)
        {
            var transformed = double.NaN;
            Random _random = new Random();
            // this loop just allows us to adapt to any transform function which 
            // may provide invalid results based on random inputs.
            while (double.IsNaN(transformed))
            {
                // generate uniform randoms and pass to the supplied distribution
                // transform.
                var uniform1 = 1.0 - _random.NextDouble();
                var uniform2 = 1.0 - _random.NextDouble();
                transformed = transform(uniform1, uniform2);
            }

            // scale/shift by mean/std dev
            return mean + stdDev * transformed;
        }

        // Box-muller transform - produces normal dist val based on 2
        // uniformly distributed randoms
        public static double BoxMuller(double u1, double u2) =>
            System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);
    }
}