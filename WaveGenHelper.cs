using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace tkiw_WaveRandomizer
{
    class WaveGenHelper
    {
        private Dictionary<string, double> enemyUnits = [];
        private Dictionary<double, List<string>> enemyUnitsReverse = [];
        private double stdDev = 0;
        private List<double> waveStrengths = [];
        private Dictionary<int, List<string>> csvRows = [];
        private int numOfColsOut = 0;

        public void LoadEnemyUnits(string filePath = "")
        {
            string[] fullFileContent;
            if (String.IsNullOrEmpty(filePath))
            {
                fullFileContent = ["id,Unit,Faction,Health,Dmg,Dmg Type,Atk speed,Atk range,dps",
                                "goblin_crab_rider,Crab Rider,enemy,700,40,melee,0.6,0,66.67",
                                "red_eye,Cyclops,enemy,3000,35,melee,1.5,0,23.33",
                                "dragon_boss,Dragon Boss,enemy,13000,155,melee,1.3,0,119.23",
                                "ent,Ent,enemy,370,15,melee,0.6,0,25",
                                "goblin_bandit,Goblin Bandit,enemy,50,5,melee,0.7,0,7.14",
                                "goblin_bat_rider,Goblin Bat Rider,enemy,290,12,melee,0.3,0,40",
                                "goblin_boss,Goblin Boss,enemy,3250,38,melee,1.2,0,31.67",
                                "goblin_crossbowman,Goblin Crossbowman,enemy,100,4,ranged,1,70,4",
                                "goblin_mage_fire,Goblin Fire Mage,enemy,100,20,ranged,1,80,20",
                                "goblin_giant,Goblin Giant,enemy,210,30,melee,0.9,0,33.33",
                                "goblin_mage_healer,Goblin Healer Mage,enemy,90,0,melee,0,0,0",
                                "goblin_mage_lightning,Goblin Lightning Mage,enemy,90,11,ranged,2,70,5.5",
                                "goblin_lizard,Goblin Lizard,enemy,195,12,melee,0.6,5,20",
                                "goblin_pig,Goblin Pig,enemy,480,10,melee,0.9,0,11.11",
                                "goblin_shaman,Goblin Shaman,enemy,120,7,melee,0.6,0,11.67",
                                "goblin_swordsman,Goblin Swordsman,enemy,105,8,melee,0.6,0,13.33",
                                "golem,Golem,enemy,760,12,melee,0.9,0,13.33",
                                "sunfaced,Sunfaced,enemy,1000,50,melee,1.5,0,33.33",
                                "goblin_wall_buster,Wall Buster,enemy,230,0,,0,0,0",];
            }
            else if (File.Exists(filePath))
            {
                fullFileContent = File.ReadAllLines(filePath);
            }else
            {
                throw new Exception("invalid file path for load enemy untis");
            }
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
            LoadEnemyUnitsReverse();
        }

        public void LoadEnemyUnitsBonus(string filePath = "")
        {
            string[] bonusLines;
            if (String.IsNullOrEmpty(filePath))
            {
                bonusLines = [];
            }
            else if (File.Exists(filePath))
            {
                bonusLines = File.ReadAllLines(filePath);
            }
            else
            {
                throw new Exception("invalid file path for load enemy bonus");
            }
            foreach (string line in bonusLines)
            {
                string[] parts = line.Split(',');

                if (parts.Length != 2) continue; // Skip invalid lines

                string unitName = parts[0];
                if (!double.TryParse(parts[1].Trim(), out double bonusPower))
                {
                    continue; // Skip if bonus power is not a valid number
                }

                // If the unit exists in enemyUnits, add the bonus power
                if (enemyUnits.ContainsKey(unitName))
                {
                    enemyUnits[unitName] += bonusPower;
                }
            }
        }

        public void WaveStrengthGen(string genType)
        {
            switch (genType)
            {
                case "Default":
                    waveStrengths.AddRange([
                        10, 10, 10, 10, 20, 20, 20, 45, 45, 45,
                        20, 20, 20, 20, 20, 45, 45, 45, 120, 120,
                        120, 40, 40, 45, 45, 45, 65, 65, 65, 120,
                        120, 120, 45, 165, 130, 130, 130, 185, 185, 185,
                        210, 210, 210, 130, 130, 210, 210, 210, 255, 255,
                        255, 300, 300, 300, 210, 450
                    ]); break;
                default:
                    throw new Exception("Failed to provide acceptable Wave Strength Algo");
            }
        }

        public void WaveUnitGen(string densityAlgo)
        {
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
                    default:
                        throw new Exception("Density Algo not given");
                }
                //generate a unit along a normal distribtion of unit target over wave stregth
                unitMeanStrength = wave / Convert.ToDouble(unitTarget);
                for (double i = 0; i < wave - wave * .10;)
                {
                    if (csvRows.TryGetValue(key, out List<string>? value))
                    {
                        value.Add(GenNormalDistroUnit(unitMeanStrength));
                    }
                    else
                    {
                        csvRows.Add(key, [GenNormalDistroUnit(unitMeanStrength)]);
                    }
                    i += enemyUnits[csvRows[key].Last<string>()];
                }
                if (csvRows[key].Count > numOfColsOut)
                    numOfColsOut = csvRows[key].Count;
                key++;
            }
        }

        public void WriteCsvToFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Header fields
                writer.Write("Level,wave preset id,Mathematical power,Bonus power,Total power,Unit,Amount");

                int maxPairs = 0;

                // Find the max number of units across all waves to dynamically handle extra columns
                foreach (var csvLine in csvRows.Values)
                {
                    if (csvLine.Count > maxPairs)
                    {
                        maxPairs = csvLine.Count;
                    }
                }

                // Add empty columns for the header to match the maxPairs
                for (int i = 1; i < maxPairs; i++)
                {
                    writer.Write(","); // Empty unit field
                    writer.Write(","); // Empty count field
                }

                // Write a new line after the header
                writer.WriteLine();

                // Write wave data
                int waveNumber = 1;
                foreach (var csvLine in csvRows.Values)
                {
                    // Write the wave preset data
                    if (waveNumber == 1) { writer.Write("village"); }
                    writer.Write("," + waveNumber + ",");
                    writer.Write(waveStrengths[waveNumber - 1] + ","); // Mathematical cumulative strength
                    writer.Write(","); // Synergy bonus (leave empty if not needed)
                    writer.Write(waveStrengths[waveNumber - 1]); // Total strength
                    writer.Write(",");

                    // Write each unit and count in the wave
                    foreach (var unit in csvLine)
                    {
                        writer.Write(unit + ",1,");
                    }

                    // Add empty fields for the remaining columns, based on the longest line (maxPairs)
                    for (int i = csvLine.Count; i < maxPairs; i++)
                    {
                        writer.Write(","); // Empty unit field
                        writer.Write(","); // Empty count field
                    }

                    // New line after each wave data
                    writer.WriteLine();
                    waveNumber++;
                }
            }
        }

        private static int TotalUnitTargetLinear(double waveStrength)
        {
            return Convert.ToInt32(Double.Round(.2 * waveStrength, 0));
        }

        private string GenNormalDistroUnit(double unitMeanStrength)
        {

            Random _random = new Random();
            double unitPower = NextDistributed(unitMeanStrength, BoxMuller);
            //find unit closest to unitpower and return that unit
            List<string> values = enemyUnitsReverse[enemyUnitsReverse.Keys.OrderBy(item => Math.Abs(unitPower - item)).First()];
            if (values.Count > 1)
            {
                return values[_random.Next(0, values.Count)];
            }
            else
            {
                return values[0];
            }
        }

        //func double double doube means it needs a method that takes 2 doubles and returns a double
        private double NextDistributed(double mean, Func<double, double, double> transform)
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
        private static double BoxMuller(double u1, double u2) =>
            System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);


        // Marsaglia polar transform - actually generates 2 normals
        //not used
        private static double MarsagliaPolar(double u1, double u2)
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

        private void LoadEnemyUnitsReverse()
        {
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
        }
    }
}
