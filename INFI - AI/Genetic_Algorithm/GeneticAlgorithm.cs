using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_GenetiveAlgorithm;
using static Genetic_Algorithm.RandomRoutes;

namespace Genetic_Algorithm
{
    public class GeneticAlgorithm
    {
        #region Properties
        TspFileParser parser = new TspFileParser();
        public double MutationRate { get; set; }
        public double CombinationRate { get; set; }
        public int PopulationSize { get; set; }
        public int Generations { get; set; }
        public double[,] Distances { get; set; }
        public List<int[]> Routes { get; set; }
        public List<int[]> modifiedRoutes { get; set; }
        #endregion
        #region Constructor
        /// <summary>
        /// Constructor for the Algorithm
        /// </summary>
        /// <param name="mutation">mutation to population ratio</param>
        /// <param name="combination">combination to population ratio</param>
        /// <param name="population">size of the routes per generation</param>
        /// <param name="generation">how many epochs it runs through (1 generation = generation of mutated and combined routes)</param>
        /// <param name="filename">the tsp file location</param>
        public GeneticAlgorithm(double mutation, double combination, int population, int generation, string filename)
        { 
            this.MutationRate = mutation;
            this.CombinationRate = combination;
            this.PopulationSize = population;
            this.Generations = generation;
            this.Distances = parser.GetDistances(filename);
            Routes = new List<int[]>();
        }
        #endregion
        #region Main Phase
        /// <summary>
        /// Starts the algorithm and provides results
        /// </summary>
        public void Start()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            #region Generate random routes with shuffle algorithm and calculate first fitness scores of random routes
            Routes.Clear(); //Starts with empty routes
            int[] start = new int[Distances.GetLength(0)];
            for (int i = 0; i < start.Length; i++)
            {
                start[i] = i;
            }
            Random rnd = new Random();
            for (int j = 0; j < PopulationSize; j++)
            {
                rnd.Shuffle(start);
                Routes.Add((int[])start.Clone());
            }
            modifiedRoutes = Routes;
            Dictionary<int, double> sortedDictionary = GetSortedFitnessScores(modifiedRoutes);
            #endregion
            #region Iterate through n Generations and generate fitter routes (mutated and combined) and print every generations result
            for (int i = 0; i < Generations; i++)
            {

                Dictionary<int, double> selectedRoutes = sortedDictionary.Take(sortedDictionary.Count / 2).ToDictionary(k => k.Key, v => v.Value);
                Dictionary<int, double> fittestRoutes = sortedDictionary.Take(sortedDictionary.Count / 10).ToDictionary(k => k.Key, v => v.Value); 
                List<int[]> mutatedRoutes = Mutate(MutationRate, selectedRoutes);
                List<int[]> combinatedRoutes = Combination(CombinationRate, selectedRoutes);
                modifiedRoutes = mutatedRoutes.Concat(combinatedRoutes).ToList();

                sortedDictionary = GetSortedFitnessScores(modifiedRoutes);
                sortedDictionary.Concat(fittestRoutes).OrderBy(k => k.Key);
                Console.WriteLine($"Generation: {i}     Best: {sortedDictionary.ElementAt(0).Value}");
            }
            #endregion
            #region Print out the currently fittest route existent
            foreach (var city in modifiedRoutes[sortedDictionary.ElementAt(0).Key])
            {
                Console.Write(city +" ");
            }
            #endregion
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine($"\nElapsed time for {Generations} Generations: {ts.TotalSeconds}");
        }
        #endregion
        #region Calculate Fitness Score
        /// <summary>
        /// Calculates the fitness score which is the sum of all distances between the cities
        /// </summary>
        /// <param name="Routes">generated routes of the current generation</param>
        /// <returns>a dictionary with the route number as key and the distance as value</returns>
        public Dictionary<int, double> GetSortedFitnessScores(List<int[]> Routes)
        {

            Dictionary<int, double> dictscores = new Dictionary<int, double>();
            for (int i = 0; i < Routes.Count; i++)
            {
                double score = 0;
                for (int j = 0; j < Routes[i].GetLength(0) - 1; j++)
                {
                    score += Distances[Routes[i][j], Routes[i][j + 1]];
                }
                dictscores.Add(i, score);

            }
            IEnumerable<KeyValuePair<int, double>> sortedScores = from score in dictscores orderby score.Value ascending select score;
            Dictionary<int, double> sortedDictionary = sortedScores.ToDictionary(pair => pair.Key, pair => pair.Value);
            return sortedDictionary;
        }
        #endregion
        #region Mutation phase of the routes
        /// <summary>
        /// Mutates the routes
        /// Mutation means bitflipping two random positions of the random route
        /// </summary>
        /// <param name="mutationcount">mutation ratio of the population per generation</param>
        /// <param name="selectedRoutes">the fittest routes which get selected</param>
        /// <returns>Mutated routes in aspect of the ratio of the population size</returns>
        public List<int[]> Mutate(double mutationcount, Dictionary<int, double> selectedRoutes)
        {
            Random rnd = new Random();
            List<int[]> mutatedRoutes = new List<int[]>();
            for (int i = 0; i < mutationcount * PopulationSize; i++)
            {

                int randomnumber = selectedRoutes.ElementAt(rnd.Next(0, selectedRoutes.Count)).Key;
                int[] randomRoute = modifiedRoutes[selectedRoutes.ElementAt(rnd.Next(0, selectedRoutes.Count)).Key];
                int index1 = rnd.Next(0, randomRoute.Length);
                int index2 = rnd.Next(0, randomRoute.Length);

                int puffer = randomRoute[index1];
                randomRoute[index1] = randomRoute[index2];
                randomRoute[index2] = puffer;
                mutatedRoutes.Add(randomRoute);
            }
            return mutatedRoutes;


        }
        #endregion
        #region Combination phase of the routes
        /// <summary>
        /// Combination with the selected Routes
        /// Combination means picking out two random parent routes and taking a random amount of the first parent and filling it up with the second parent and creating a child
        /// </summary>
        /// <param name="combinationCount">combination ratio to the population size</param>
        /// <param name="selectedRoutes">the fittest routes get selected</param>
        /// <returns>combined routes in aspect of the combination ratio</returns>
        public List<int[]> Combination(double combinationCount, Dictionary<int, double> selectedRoutes)
        {
            Random rnd = new Random();
            List<int[]> combinatedRoutes = new List<int[]>();
            int routeLength = modifiedRoutes[selectedRoutes.ElementAt(0).Key].Length;
            List<int> producedChild = new List<int>();
            for (int i = 0; i < combinationCount * PopulationSize; i++)
            {
                int randomPosition = rnd.Next(0, routeLength);                
                //int[] parent1 = modifiedRoutes[selectedRoutes.ElementAt(rnd.Next(0, routeLength)).Key];
                int[] parent1 = modifiedRoutes[selectedRoutes.ElementAt(rnd.Next(0, selectedRoutes.Count)).Key];
                //int[] parent2 = modifiedRoutes[selectedRoutes.ElementAt(rnd.Next(0, routeLength)).Key];
                int[] parent2 = modifiedRoutes[selectedRoutes.ElementAt(rnd.Next(0, selectedRoutes.Count)).Key];
                producedChild = parent1.Take(randomPosition).ToList();
                int childLength = producedChild.Count;
                for (int j = 0; j < routeLength; j++)
                {
                    if (!producedChild.Contains(parent2[j])) {
                        producedChild.Add(parent2[j]);
                    }
                    else
                    {
                        continue;
                    }
                }
                combinatedRoutes.Add(producedChild.ToArray());

            }
            return combinatedRoutes;





        }
        #endregion
    }
}
