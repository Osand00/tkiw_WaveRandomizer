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

        private void RunGen_btn_Click(object sender, RoutedEventArgs e)
        {
            List<double> waveStrengths = [];
            Dictionary<string, double> enemyUnits = [];
            Dictionary<double, List<string>> enemyUnitsReverse = [];
            double stdDev;
            //Creation of enemyUnits Dictionary
            //verify filepath
            if (File.Exists(filePath_tbx.Text))
            {
                //create enemy unit map
                enemyUnits = LoadEnemyUnits(filePath_tbx.Text);
                enemyUnitsReverse = LoadEnemyUnitsReverse(enemyUnits, out stdDev);
            }
            else { throw new Exception("File path given is invalid"); }
            //Determine Wave Generation
            waveStrengths = WaveStrengthGen(strengthAlgo_cbo.SelectedItem.ToString());
            //Build Wave_presets_village
            Dictionary<int, List<string>> csvRows = WaveUnitGen(enemyUnits, enemyUnitsReverse, waveStrengths, stdDev, densityAlgo_cbo.SelectedItem.ToString(), out int numOfColsOut);
        }

        //pull enemy units out of the csv File
        private static Dictionary<string, double> LoadEnemyUnits(string filePath)
        {
            Dictionary<string, double> enemyUnits = [];
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

        //dictionary of enemy units with power as key instead of ID. create stdDev
        private static Dictionary<double, List<string>> LoadEnemyUnitsReverse(Dictionary<string, double> enemyUnits, out double stdDev)
        {
            Dictionary<double, List<string>> enemyUnitsReverse = [];
            foreach (KeyValuePair<string, double> entry in enemyUnits)
            {
                if (enemyUnitsReverse.TryGetValue(entry.Value, out List<string>? value))
                {
                    value.Add(entry.Key);
                }
                else
                {
                    enemyUnitsReverse.Add(entry.Value, new List<string>([entry.Key]));
                }
            }
            double avg = enemyUnitsReverse.Keys.Average();
            double sumOfSquaresOfDifferences = enemyUnitsReverse.Keys.Select(val => (val - avg) * (val - avg)).Sum();
            //total pop stdDev calc
            stdDev = Math.Sqrt(sumOfSquaresOfDifferences / enemyUnitsReverse.Keys.Count);
            //dont think it would be needed but this is sample pop stdDev
            //stdDev = Math.Sqrt(sumOfSquaresOfDifferences / (enemyUnitsReverse.Keys.Count - 1));
            return enemyUnitsReverse;
        }

        private static List<double> WaveStrengthGen(string? genType)
        {
            List<double> waveStrengths = [];
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

        private static Dictionary<int, List<string>> WaveUnitGen(Dictionary<string, double> enemyUnits,
                                                                 Dictionary<double, List<string>> enemyUnitsReverse,
                                                                 List<double> waveStrengths,
                                                                 double stdDev,
                                                                 string? densityAlgo,
                                                                 out int numOfColsOut)
        {
            Dictionary<int, List<string>> csvOut = [];
            int key = 1;
            int unitTarget = 0;
            double unitMeanStrength = 0;
            numOfColsOut = 0;
            Random _random = new();
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
                    if (csvOut.TryGetValue(key, out List<string>? value))
                    {
                        value.Add(GenNormalDistroUnit(enemyUnitsReverse, unitMeanStrength, stdDev));
                    }
                    else
                    {
                        csvOut.Add(key, [GenNormalDistroUnit(enemyUnitsReverse, unitMeanStrength, stdDev)]);
                    }
                    i += enemyUnits[csvOut[key].Last<string>()];
                }
                if (csvOut[key].Count > numOfColsOut)
                    numOfColsOut = csvOut[key].Count;
                key++;
            }
            return csvOut;
        }

        private static int TotalUnitTargetLinear(double waveStrength)
        {
            return Convert.ToInt32(Double.Round(.2*waveStrength, 0));
        }

        //TODO
        private static string GenNormalDistroUnit(Dictionary<double, List<string>> enemyUnitsReverse, double unitMeanStrength, double stdDev)
        {

            Random _random = new Random();
            double unitPower = NextDistributed(unitMeanStrength, stdDev, BoxMuller);
            //find unit closest to unitpower and return that unit
            List<string> values = enemyUnitsReverse[enemyUnitsReverse.Keys.OrderBy(item => Math.Abs(unitPower - item)).First()];
            if (values.Count > 1)
            {
                return values[_random.Next(0, values.Count)];
            }else
            {
                return values[0];
            }
        }

        //func double double doube means it needs a method that takes 2 doubles and returns a double
        public static double NextDistributed(double mean, double stdDev, Func<double, double, double> transform)
        {
            var transformed = double.NaN;
            Random _random = new();
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


        // Marsaglia polar transform - actually generates 2 normals
        //not used
        public static double MarsagliaPolar(double u1, double u2)
        {
            // map from [0, 1] -> [-1, 1]
            var v1 = 2.0 * u1 - 1.0;
            var v2 = 2.0 * u2 - 1.0;

            var s = v1 * v1 + v2 * v2;

            // reject numbers outside the unit circle
            if (s is >= 1.0 or 0)
                return double.NaN;

            return v1 * System.Math.Sqrt(-2.0 * System.Math.Log(s) / s);
        }
    }
}