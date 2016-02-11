using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists.
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

namespace Completed

{

    public class BoardManager : MonoBehaviour
    {
        // Using Serializable allows us to embed a class with sub properties in the inspector.
        [Serializable]
        public class Count
        {
            public int minimum;             //Minimum value for our Count class.
            public int maximum;             //Maximum value for our Count class.


            //Assignment constructor.
            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }

        public int columns = 8;                                         //Number of columns in our game board.
        public int rows = 8;                                            //Number of rows in our game board.
        public Count wallCount = new Count(25, 30);                     //Lower and upper limit for our random number of walls per level.
        public Count foodCount = new Count(0, 10);                      //Lower and upper limit for our random number of food items per level.
        public GameObject exit;                                         //Prefab to spawn for exit.
        public GameObject[] floorTiles;                                 //Array of floor prefabs.
        public GameObject[] wallTiles;                                  //Array of wall prefabs.
        public GameObject[] foodTiles;                                  //Array of food prefabs.
        public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
        public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.

        public FloodFillMap map = new FloodFillMap();                   //Map used for txt map output of floodfill
        public Cell[,] cell;                                            //Array of cells for flood fill

        private WriteToCSV csv;                                         //Instantiate the CSV file
        private double[,] csvWallContent = new double[11, 6];           //Array for wall placement related values (e.g. lowest amount of walls, highest amount of walls)
        private double[,] csvFoodContent = new double[11, 5];           //Array for food placement related values
        private double[,] csvEnemyContent = new double[11, 3];          //Array for enemy placement related values

        private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
        private List<Vector3> gridPositions = new List<Vector3>();      //A list of possible locations to place tiles.
        private List<Vector3> outerWalls = new List<Vector3>();         //A list of the positions of outer walls

        private List<Vector3> obstacles = new List<Vector3>();          //A list of the positions of obstacles
        private List<Vector3> enemyCoverage = new List<Vector3>();      //A list of covered Area by Enemies

        private Vector3 randomPosition;                                 //Vector of randomPosition
        private List<Vector3> bestPositionSolution = new List<Vector3>();   //List of positions of tiles of the best solution
        private GameObject[] bestTileChoiceArray;                       //Choose the best tile array for placement

        private double bestExploration = 0;                             //Saves best exploration value
        private double exploration;                                     //Exploration value

        private int bestFood = 1000;                                    //Saves best food fitness value
        private int foodFitness;                                        //Distance from actual food level to 10

        private int bestEnemyCoverageTotal = 0;                         //Saves best enemy coverage value
        private int enemyCoverageTotal;                                 //Enemy Coverage  
        private double bestDiff;                                        //Difference between generated solution fitness value (for comparison reasons)




        //Clears our list gridPositions and prepares it to generate a new board.
        void InitialiseList()
        {
            //Clear our list gridPositions, obstacles and enemyCoverage
            gridPositions.Clear();
            obstacles.Clear();
            enemyCoverage.Clear();

            //Loop through x axis (columns).
            for (int x = 0; x < columns; x++)
            {
                //Within each column, loop through y axis (rows).
                for (int y = 0; y < rows; y++)
                {
                    Vector3 position = new Vector3(x, y, 0f);
                    if (position != new Vector3(0, 0, 0f) & position != new Vector3(columns - 1, rows - 1, 0f) & position != new Vector3(0, 1, 0f) & position != new Vector3(1, 0, 0f))
                        //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                        gridPositions.Add(position);
                }
            }
        }


        //Sets up the outer walls and floor (background) of the game board.
        void BoardSetup()
        {
            //Instantiate Board and set boardHolder to its transform.
            boardHolder = new GameObject("Board").transform;

            //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            for (int x = -1; x < columns + 1; x++)
            {
                //Loop along y axis, starting from -1 to place floor or outerwall tiles.
                for (int y = -1; y < rows + 1; y++)
                {
                    //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                    GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                    //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    if (x == -1 || x == columns || y == -1 || y == rows)
                    {
                        toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                        outerWalls.Add(new Vector3(x, y, 0f));
                    }

                    //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    GameObject instance =
                        Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    instance.transform.SetParent(boardHolder);
                }
            }
        }


        //RandomPosition returns a random position from our list gridPositions.
        Vector3 RandomPosition(List<Vector3> positions)
        {
            //Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
            int randomIndex = Random.Range(0, positions.Count);
            //Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
            Vector3 randomPosition = positions[randomIndex];

            //Remove the entry at randomIndex from the list so that it can't be re-used.
            positions.RemoveAt(randomIndex);

            //Return the randomly selected Vector3 position.
            return randomPosition;
        }

        //LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum, int level, int playerFoodPoints)
        {
            Debug.Log("------------NEXT ELEMENT------------");

            //Instantiate gameobjects for tile selection
            GameObject[] tileChoiceArray;
            GameObject tempTileChoice;

            //Reset fitness values
            if (tileArray == foodTiles)
            {
                bestFood = 1000;
            }
            if (tileArray == wallTiles)
            {
                bestExploration = 0;
            }
            if (tileArray == enemyTiles)
            {
                bestEnemyCoverageTotal = 0;
            }

            //Clear the positions of the best solution to avoid taking old solutions to the loop
            bestPositionSolution.Clear();

            //Target Value for optimization
            double targetValue = setTargetValue(tileArray, minimum);

            //Loop for optimization of fitness values
            for (int j = 0; j < 10; j++)
            {
                Debug.Log("------------- Run - " + j + " -------------");

                //Choose a random number of objects to instantiate within the minimum and maximum limits
                int objectCount = Random.Range(minimum, maximum + 1);

                //Variables for temporary storing values
                List<Vector3> tempPositions = new List<Vector3>();
                List<Vector3> tempGrid = new List<Vector3>(gridPositions);
                tileChoiceArray = new GameObject[objectCount];

                //Clear temporary Lists to make sure they are empty
                tempPositions.Clear();
                enemyCoverage.Clear();

                //Loop that places every object at a position
                for (int i = 0; i < objectCount; i++)
                {
                    //Get one random position out of the available positions
                    randomPosition = RandomPosition(tempGrid);

                    //Add the position to the temporary solution
                    tempPositions.Add(randomPosition);

                    tempTileChoice = tileArray[Random.Range(0, tileArray.Length)];
                    tileChoiceArray[i] = tempTileChoice;

                    //Add position to calculation map
                    cell = map.AddCell(cell, randomPosition, tempTileChoice.tag);

                    //Occupied area around Enemy
                    if (tileArray == enemyTiles)
                    {
                        Vector3 upperArea = randomPosition;
                        upperArea.y = upperArea.y + 1;
                        Vector3 lowerArea = randomPosition;
                        lowerArea.y = lowerArea.y - 1;
                        Vector3 leftArea = randomPosition;
                        leftArea.x = leftArea.x - 1;
                        Vector3 rightArea = randomPosition;
                        rightArea.x = rightArea.x + 1;

                        //Add position to covered Area by Enemy Array
                        addToCoverage(enemyCoverage, randomPosition);
                        addToCoverage(enemyCoverage, upperArea);
                        addToCoverage(enemyCoverage, lowerArea);
                        addToCoverage(enemyCoverage, leftArea);
                        addToCoverage(enemyCoverage, rightArea);
                    }
                }

                //Optimization, CSV and txt map creation for wall tiles
                if (tileArray == wallTiles)
                {
                    exploration = Math.Round(map.GetExplorationRatio(cell, new Cell(0, 0, "n"), new Cell(columns - 1, rows - 1, "n")), 2);
                    map.DeleteTiles(cell, "w");
                    map.DeleteTiles(cell, "p");
                    double diff = Math.Abs(targetValue - exploration);
                    fillWallArray(csvWallContent, j, 0, minimum, maximum, tempPositions.Count, targetValue, exploration, diff);

                    //Check if current temporary solution is better than the current best solution
                    if (Math.Abs(exploration - targetValue) < Math.Abs(bestExploration - targetValue))
                    {
                        bestExploration = exploration;
                        bestPositionSolution = tempPositions;
                        bestTileChoiceArray = tileChoiceArray;
                        bestDiff = diff;
                        fillWallArray(csvWallContent, 10, 0, minimum, maximum, bestPositionSolution.Count, targetValue, bestExploration, bestDiff);
                    }
                }

                //Optimization, CSV and txt map creation for food tiles
                if (tileArray == foodTiles)
                {
                    foodFitness = map.GetChallengeMeasure(cell, playerFoodPoints, 10);
                    map.DeleteTiles(cell, "f");
                    map.DeleteTiles(cell, "s");
                    fillFoodArray(csvFoodContent, j, 0, minimum, maximum, tempPositions.Count, targetValue, foodFitness);

                    //Check if current temporary solution is better than the current best solution
                    if (foodFitness < bestFood)
                    {
                        bestFood = foodFitness;
                        bestPositionSolution = tempPositions;
                        bestTileChoiceArray = tileChoiceArray;
                        fillFoodArray(csvFoodContent, 10, 0, minimum, maximum, bestPositionSolution.Count, targetValue, bestFood);
                    }
                }

                //Optimization, CSV and txt map creation for enemies
                if (tileArray == enemyTiles)
                {
                    map.DeleteTiles(cell, "e");
                    enemyCoverageTotal = enemyCoverage.Count;
                    csvEnemyContent[j, 0] = tempPositions.Count;
                    csvEnemyContent[j, 1] = targetValue;
                    csvEnemyContent[j, 2] = enemyCoverageTotal;

                    //Check if current temporary solution is better than the current best solution
                    if (Math.Abs(enemyCoverageTotal - targetValue) <= Math.Abs(bestEnemyCoverageTotal - targetValue))
                    {
                        bestEnemyCoverageTotal = enemyCoverageTotal;
                        bestPositionSolution = tempPositions;
                        bestTileChoiceArray = tileChoiceArray;
                        csvEnemyContent[10, 0] = bestPositionSolution.Count;
                        csvEnemyContent[10, 1] = targetValue;
                        csvEnemyContent[10, 2] = bestEnemyCoverageTotal;
                    }
                }

                //In the last iteration of the loop the elements have to be instantiated at the positions of the best solution
                if (j == 9)
                {
                    //As much units are placed as are positions in the best solution
                    int unitLimit = bestPositionSolution.Count;

                    //For wall tiles every wall has to be added to the obstacles for the calculation of the enemy coverage
                    if (tileArray == wallTiles)
                    {
                        for (int l = 0; l < bestPositionSolution.Count; l++)
                        {
                            obstacles.Add(bestPositionSolution[l]);
                        }
                    }

                    //Instantiate every unit one by one
                    for (int h = 0; h < unitLimit; h++)
                    {
                        //Choose a random tile from tileArray and assign it to tileChoice
                        GameObject tileChoice = bestTileChoiceArray[h];

                        //Get a random position for the unit from the positions of the best solution
                        Vector3 position = RandomPosition(bestPositionSolution);

                        gridPositions.Remove(position);

                        cell = map.AddCell(cell, position, tileChoice.tag);

                        //Instantiate unit at the choosen position
                        Instantiate(tileChoice, position, Quaternion.identity);
                    }
                }
            }
        }


        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene(int level, int playerFoodPoints)
        {
            //Creates the outer walls and floor.
            BoardSetup();

            //Reset our list of gridpositions.
            InitialiseList();

            // CSV Log File creation and appends titles
            csv = new WriteToCSV(level);

            //Generate map for calculation
            cell = map.GenerateMap(columns, rows);

            //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum, level, playerFoodPoints);
            csv.AppendWallSolution(csvWallContent);

            //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum, level, playerFoodPoints);
            csv.AppendFoodSolution(csvFoodContent);

            //Determine number of enemies based on current level number, based on a logarithmic progression
            int enemyCount = DecideEnemyCount(foodFitness, level);

            //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount, level, playerFoodPoints);
            csv.AppendEnemySolution(csvEnemyContent);

            //Draw map into txt file
            map.DrawMap(cell);

            //Instantiate the exit tile in the upper right hand corner of our game board
            Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);

            // Close the stream to CSV files
            csv.Stop();
        }

        /// <summary>
        /// Calculate amount of enemies for the current level to be created depending on the foodfitness
        /// </summary>
        /// <param name="foodFitness">Foodfitness of the last level as indicator how you performed in the last level</param>
        /// <param name="level">Current level</param>
        /// <returns></returns>
        int DecideEnemyCount(int foodFitness, int level)
        {
            if (foodFitness <= 40)
            {
                return (int)Mathf.Log(level, 2f);
            }
            if (foodFitness > 40 && foodFitness <= 80)
            {
                return (int)Mathf.Log(level, 2f) + 1;
            }
            if (foodFitness > 80)
            {
                return (int)Mathf.Log(level, 2f) + 2;
            }
            else return (int)Mathf.Log(level, 2f);
        }

        //Add positions to coveredArea Array
        void addToCoverage(List<Vector3> coverage, Vector3 position)
        {
            if (!coverage.Contains(position) && !outerWalls.Contains(position) && !obstacles.Contains(position))
            {
                coverage.Add(position);
            }
        }

        //Fill function for wall array
        void fillWallArray(double[,] array, int j, int x, int minimum, int maximum, int actualValue, double targetValue, double fitnessValue, double diff)
        {
            array[j, x] = minimum;
            array[j, x + 1] = maximum;
            array[j, x + 2] = actualValue;
            array[j, x + 3] = targetValue;
            array[j, x + 4] = fitnessValue;
            array[j, x + 5] = diff;
        }

        //Fill function for food array
        void fillFoodArray(double[,] array, int j, int x, int minimum, int maximum, int actualValue, double targetValue, double fitnessValue)
        {
            array[j, x] = minimum;
            array[j, x + 1] = maximum;
            array[j, x + 2] = actualValue;
            array[j, x + 3] = targetValue;
            array[j, x + 4] = fitnessValue;
        }


        /// <summary>
        /// Sets target values for every relevant tile set
        /// </summary>
        /// <param name="tileArray"> type of object to position</param>
        /// <param name="minimum"> minimum value of placed tiles</param>
        /// <returns></returns>
        double setTargetValue(GameObject[] tileArray, int minimum)
        {
            double targetValue = 0;

            //Target value for generation of enemies
            if (tileArray == enemyTiles)
            {
                targetValue = Random.Range((minimum * 3 + 2), (minimum * 5));
            }

            //Target value for generation of food
            if (tileArray == foodTiles)
            {
                if (bestExploration >= 0.7)
                {
                    targetValue = 2;
                }
                if (bestExploration < 0.7 && bestExploration >= 0.4)
                {
                    targetValue = 5;
                }
                if (bestExploration < 0.4)
                {
                    targetValue = 8;
                }
            }

            //Target value for generation of walls
            if (tileArray == wallTiles)
            {
                targetValue = 0.55;
            }

            return targetValue;
        }
    }
}