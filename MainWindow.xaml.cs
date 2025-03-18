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
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;

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
            strengthAlgo_cbo.Items.Add("Double");
            strengthAlgo_cbo.SelectedItem = "Default";
        }

        // Existing code...
        private void runGen_btn_Click(object sender, RoutedEventArgs e)
        {
            List<double> waveStrengths = new List<double>();
            Dictionary<string, double> enemyUnits = new Dictionary<string, double>();
            // Creation of enemyUnits Dictionary
            // Verify filepath
            if (File.Exists(filePath_tbx.Text))
            {
                // Create enemy unit map
                enemyUnits = LoadEnemyUnits(filePath_tbx.Text);
            }
            else
            {
                throw new Exception("File path given is invalid");
            }

            // Determine Wave Generation
            waveStrengths = WaveStrengthGen(strengthAlgo_cbo.SelectedItem.ToString());

            // Build Wave_presets_village
            List<Dictionary<string, int>> csvRows = WavePresetString(enemyUnits, waveStrengths, densityAlgo_cbo.SelectedItem.ToString());

            string outputFile = @"..\..\..\Wave_Presets_village.csv";
            WriteCsvToFile(outputFile, csvRows, waveStrengths);
        }

        // New method to handle writing to the CSV file
        


        private Dictionary<string, double> LoadEnemyUnits(string filePath)
        {
            Dictionary<string, double> enemyUnits = new Dictionary<string, double>();
            string[] fullFileContent = File.ReadAllLines(filePath);
            //string[] unitFileHeaders = fullFileContent[0].Split(',');
            string[] lineArr = new string[fullFileContent[0].Split(',').Length];
            int unitIdCol = 0, unitHpCol = 0, unitDmgCol = 0, unitAtkSpdCol = 0, unitDpsCol = 0, unitFactionCol = 0;
            double healthPower = 0, dpsPower = 0;

            // Checks if the values given by the user are valid, if so saves them in healthPower and dpsPower
            if (Double.TryParse(hpCoef_tbx.Text, out healthPower)) { }
            else { throw new Exception("Invalid input for health power."); }
            if (Double.TryParse(dmgCoef_tbx.Text, out dpsPower)) { }
            else { throw new Exception("Invalid input for dps power."); }

            // Get col indices
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

            // Add to dictionary
            foreach (string line in fullFileContent)
            {
                // Skip headers and skip non enemies
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
                    waveStrengths.AddRange(new double[] {
                        10, 10, 10, 10, 20, 20, 20, 20, 45, 45,
                        45, 45, 45, 45, 45, 45, 45, 45, 45, 120,
                        120, 120, 210, 210, 210, 210, 210, 300, 300, 300,
                        300, 300, 300, 300, 165, 450
                    }); break;
                case "Double":
                    waveStrengths.AddRange(new double[] {
                        20, 20, 20, 20, 40, 40, 40, 40, 90, 90,
                        90, 90, 90, 90, 90, 90, 90, 90, 90, 240,
                        240, 240, 420, 420, 420, 420, 420, 600, 600, 600,
                        600, 600, 600, 600, 330, 900
                    });
                    break;
                default:
                    throw new Exception("Failed to provide acceptable Wave Strength Algo");
            }
            return waveStrengths;
        }

        private static List<Dictionary<string, int>> WavePresetString(Dictionary<string, double> enemyUnits, List<double> waveStrengths, string? densityAlgo)
        {
            // List to store all waves' unit counts
            List<Dictionary<string, int>> csvOut = new List<Dictionary<string, int>>();

            // Dictionary to track the unit count in the current wave (csvLine)
            Dictionary<string, int> csvLine = new Dictionary<string, int>();

            int unitTarget = 0;
            double unitMeanStrength = 0;

            // Build the waves
            foreach (double wave in waveStrengths)
            {
                // Determine the number of units for the current wave based on the density algorithm
                switch (densityAlgo)
                {
                    case "Linear":
                        unitTarget = TotalUnitTargetLinear(wave);
                        break;
                    case "Normal":
                        throw new Exception("Density Algo not implemented");
                    default:
                        throw new Exception("Density Algo not given");
                }

                // Generate a unit along a normal distribution of unit target over wave strength
                unitMeanStrength = wave / Convert.ToDouble(unitTarget);

                // Track the total strength accumulated in the wave
                double currentStrength = 0;

                while (currentStrength < wave * 0.90)  // Runs until 90% of the wave strength is filled
                {
                    string unitId = GenNormalDistroUnit(enemyUnits, unitMeanStrength);  // Get unit ID from normal distribution

                    // Update the count of units in csvLine for this unit ID
                    if (csvLine.ContainsKey(unitId))
                    {
                        csvLine[unitId]++;  // Increment the count of this unit in the current wave
                    }
                    else
                    {
                        csvLine.Add(unitId, 1);  // Add this unit with an initial count of 1
                    }

                    // Add the strength of the unit to the current strength (assuming strength is determined by unit's health/dps)
                    currentStrength += enemyUnits[unitId];  // Assuming `enemyUnits[unitId]` gives the strength of the unit
                }

                // After processing the wave, add the csvLine (unit count per wave) to csvOut
                csvOut.Add(new Dictionary<string, int>(csvLine));  // Store a copy of csvLine for the current wave

                // Optionally, reset csvLine for the next wave (clear it out)
                csvLine.Clear();  // Clear previous wave's unit data
            }

            // Return the list with all the waves' data
            return csvOut;
        }

        private static int TotalUnitTargetLinear(double waveStrength)
        {
            return Convert.ToInt32(Double.Round(0.2 * waveStrength, 0));
        }

        // Generate unit based on a normal distribution
        private static string GenNormalDistroUnit(Dictionary<string, double> enemyUnits, double unitMeanStrength)
        {
            // Standard deviation for the normal distribution
            double stdDev = 5;
            double unitPower = NextDistributed(unitMeanStrength, stdDev, BoxMuller);

            // Find unit closest to unitPower
            double closestDifference = double.MaxValue;
            string closestUnit = "";
            foreach (var unit in enemyUnits)
            {
                double diff = Math.Abs(unit.Value - unitPower);
                if (diff < closestDifference)
                {
                    closestDifference = diff;
                    closestUnit = unit.Key;
                }
            }
            return closestUnit;
        }

        // Func double double double means it needs a method that takes 2 doubles and returns a double
        public static double NextDistributed(double mean, double stdDev, Func<double, double, double> transform)
        {
            var transformed = double.NaN;
            Random _random = new Random();
            // This loop just allows us to adapt to any transform function which
            // may provide invalid results based on random inputs.
            while (double.IsNaN(transformed))
            {
                // Generate uniform randoms and pass to the supplied distribution transform.
                var uniform1 = 1.0 - _random.NextDouble();
                var uniform2 = 1.0 - _random.NextDouble();
                transformed = transform(uniform1, uniform2);
            }

            // Scale/shift by mean/std dev
            return mean + stdDev * transformed;
        }

        // Box-Muller transform - produces normal distribution value based on 2
        // uniformly distributed randoms
        public static double BoxMuller(double u1, double u2) =>
            System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);

        private void WriteCsvToFile(string outputFile, List<Dictionary<string, int>> csvRows, List<double> waveStrengths)
        {
            using var writer = new StreamWriter(outputFile);
            using var csvOut = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Write header row (wave number, mathematical cumulative strength, synergy bonus, total strength, unit and count)
            csvOut.WriteField("Level");
            csvOut.WriteField("wave preset id");
            csvOut.WriteField("Mathematical score");
            csvOut.WriteField("Bonus score");
            csvOut.WriteField("Total score");
            csvOut.WriteField("Unit");
            csvOut.WriteField("Amount");

            int maxPairs = 0;

            // Find the max number of units across all waves to dynamically handle extra columns
            foreach (var csvLine in csvRows)
            {
                if (csvLine.Count > maxPairs)
                {
                    maxPairs = csvLine.Count;
                }
            }

            // Add empty fields for the remaining columns based on the longest line
            for (int i = 0; i < maxPairs; i++)
            {
                csvOut.WriteField(""); // Empty fields for unused unit columns
                csvOut.WriteField(""); // Empty fields for unused count columns
            }

            csvOut.NextRecord();

            // Write each wave data
            int waveNumber = 1;
            foreach (var csvLine in csvRows)
            {
                csvOut.WriteField("village"); // Level name
                csvOut.WriteField(waveNumber); // Wave preset ID
                csvOut.WriteField(waveStrengths[waveNumber - 1]); // Mathematical cumulative strength
                csvOut.WriteField(""); // Synergy bonus (leave empty if not needed)
                csvOut.WriteField(waveStrengths[waveNumber - 1]); // Total strength
                waveNumber++;

                // Write each unit and count in the wave
                foreach (var kvp in csvLine)
                {
                    csvOut.WriteField(kvp.Key); // Unit name
                    csvOut.WriteField(kvp.Value); // Unit count
                }

                // Add empty fields for the remaining columns, based on the longest line (maxPairs)
                for (int i = csvLine.Count; i < maxPairs; i++)
                {
                    csvOut.WriteField(""); // Empty fields for unused unit columns
                    csvOut.WriteField(""); // Empty fields for unused count columns
                }

                csvOut.NextRecord();
            }
        }
    }
}

