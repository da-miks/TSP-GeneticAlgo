using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TSP_GenetiveAlgorithm {
    class TspFileParser {
        public double[,] GetDistances(string filename) {
            (double x, double y)[] positions = GetPositions(filename);
            System.Diagnostics.Debug.WriteLine("Calculating distances...");
            double[,] distances = new double[positions.Length, positions.Length];
            for(int x = 0; x < positions.Length; x++)
                for (int y = 0; y<positions.Length; y++) {
                    // Euclidan distance c = sqrt((x-x1)^2 + (y-y1)^2)
                    if (x == y)
                        continue;
                    distances[x, y] = Math.Sqrt((positions[x].x - positions[y].x) * (positions[x].x - positions[y].x) + (positions[x].y - positions[y].y) * (positions[x].y - positions[y].y));
                }
            return distances;
        }
        (double x, double y)[] GetPositions(string filename) {
            string search = "NODE_COORD_SECTION";
            bool searchFound = false;
            System.Diagnostics.Debug.WriteLine("Reading file...");
            string[] lines = System.IO.File.ReadAllLines(filename);
            System.Diagnostics.Debug.WriteLine("File read.");
            List<(double x, double y)> ret = new List<(double x, double y)>(lines.Length);
            foreach(var line in lines) {
                if (line != search && !searchFound) {
                    continue;
                }
                if (line == search) {
                    searchFound = true;
                    continue;
                }
                if (line == "EOF")
                    continue;

                // Now we found a line. parse it.
                string[] parts = line.Split(' ');
                if(parts.Length >= 3)
                {
                ret.Add((double.Parse(parts[1], CultureInfo.InvariantCulture), double.Parse(parts[2],CultureInfo.InvariantCulture)));

                }
            }
            System.Diagnostics.Debug.WriteLine("File parsed");
            return ret.ToArray();
        }
    }
}
