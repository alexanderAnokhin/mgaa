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


        public string logFileName;                                      //Name of the LogFile
        public int columns = 8;                                         //Number of columns in our game board.
        public int rows = 8;                                            //Number of rows in our game board.
        public Count wallCount = new Count(25, 30);                       //Lower and upper limit for our random number of walls per level.
        public Count foodCount = new Count(0, 10);                       //Lower and upper limit for our random number of food items per level.
        public GameObject exit;                                         //Prefab to spawn for exit.
        public GameObject[] floorTiles;                                 //Array of floor prefabs.
        public GameObject[] wallTiles;                                  //Array of wall prefabs.
        public GameObject[] foodTiles;                                  //Array of food prefabs.
        public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
        public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.

        public FloodFillMap map = new FloodFillMap();
        public Cell[,] cell;

        private WriteToCSV csv;                                         //Instantiate the CSV file

        private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
        private List<Vector3> gridPositions = new List<Vector3>();   //A list of possible locations to place tiles.

        private List<Vector3> areaCovered = new List<Vector3>();        //A list of covered Area by Gameobjects
        private List<Vector3> enemyCoverage = new List<Vector3>();      //A list of covered Area by Enemies
        private List<Vector3> outerWalls = new List<Vector3>();         //A list of the positions of outer walls
        private List<Vector3> obstacles = new List<Vector3>();          //A list of the positions of obstacles

        private double percentageEnemyCoverage;                         //Percentage of the map covered by enemies

        private Vector3 randomPosition;                                 //Vector of randomPosition
        private List<Vector3> bestPositionSolution = new List<Vector3>();
        private GameObject[] bestTileChoiceArray;

        private double exploration;                                     //Exploration value
        private int foodFitness;                                        //Distance from actual food level to 10

        double[] noOfWallTilesArray = new double[5];                                    //Array of Min|Max|Actual|Target|Exploration
        double[] noOfFoodTilesArray = new double[5];                                    //Array of Min|Max|Actual|Target|FoodFitness


        //Clears our list gridPositions and prepares it to generate a new board.
        void InitialiseList()
        {
            //Clear our list gridPositions.
            gridPositions.Clear();
            areaCovered.Clear();
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

            GameObject[] tileChoiceArray;
            GameObject tempTileChoice;
            //Target Value
            double targetValue = 4;
            setTargetValue(targetValue, tileArray, minimum);

            for (int j = 0; j < 10; j++)
            {
                Debug.Log("------------- Run - " + j + " -------------");

                enemyCoverage.Clear();
                areaCovered.Clear();

                //Variables
                //Choose a random number of objects to instantiate within the minimum and maximum limits
                int objectCount = Random.Range(minimum, maximum + 1);
                int bestCoverage = 0;
                int tempCoverage = 0;

                List<Vector3> currentBestPositions = new List<Vector3>();
                List<Vector3> tempPositions = new List<Vector3>();
                List<Vector3> tempGrid = new List<Vector3>(gridPositions);
                tileChoiceArray =  new GameObject[objectCount];

                tempPositions.Clear();

                for (int i = 0; i < objectCount; i++)
                {
                    Debug.Log("Round: " + i);
                    randomPosition = RandomPosition(tempGrid);
                    tempPositions.Add(randomPosition);
                    
                    tempTileChoice = tileArray[Random.Range(0, tileArray.Length)];
                    tileChoiceArray[i]=tempTileChoice;
                    //Add position to calculation map
                    cell = map.AddCell(cell,randomPosition,tempTileChoice.tag);

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

                    //Occupied Area around other Tiles
                    else
                    {
                        if (tileArray == wallTiles)
                        {
                            obstacles.Add(randomPosition);
                        }
                    }
                }

                if (Math.Abs(enemyCoverage.Count - targetValue) <= Math.Abs(tempCoverage - targetValue))
                {
                    bestCoverage = enemyCoverage.Count;
                    bestPositionSolution = tempPositions;
                    bestTileChoiceArray = tileChoiceArray;
                }

                if (tileArray == wallTiles)
                {
                    //exploration = 0.55; //TODO
                    exploration = map.GetExplorationRatio(cell,new Cell(0,0,"n"), new Cell(columns-1,rows-1,"n"));
                    Debug.Log("exploration - "+exploration);
                    map.DeleteTiles(cell,"w");
                    map.DeleteTiles(cell,"p");
                    fillArray(noOfWallTilesArray, minimum, maximum, tempPositions.Count, targetValue, exploration);
                }

                if (tileArray == foodTiles)
                {
                    //foodFitness = 40; //TODO
                    foodFitness = map.GetChallengeMeasure(cell,playerFoodPoints,10);
                    Debug.Log("foodFitness - "+ foodFitness);
                    map.DeleteTiles(cell,"f");
                    map.DeleteTiles(cell,"s");
                    fillArray(noOfWallTilesArray, minimum, maximum, tempPositions.Count, targetValue, exploration);
                }
                if (tileArray == enemyTiles)
                {
                    map.DeleteTiles(cell,"e");
                    calculateCoverageOfMap();
                }

                if (j == 9)
                {
                    int unitLimit = bestPositionSolution.Count;
                    Debug.Log("Unit Limit: " + unitLimit);

                    for (int h = 0; h < unitLimit; h++)
                    {
                        //Choose a random tile from tileArray and assign it to tileChoice
                        GameObject tileChoice = bestTileChoiceArray[h];//tileArray[Random.Range(0, tileArray.Length)];
                        
                        Vector3 position = RandomPosition(bestPositionSolution);

                        gridPositions.Remove(position);

                        cell = map.AddCell(cell,position,tileChoice.tag);

                        Instantiate(tileChoice, position, Quaternion.identity);
                    }
                }
            }
            loggingWallAndFoodTiles(tileArray, targetValue, bestPositionSolution.Count);
        }


        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene(int level, int playerFoodPoints)
        {
            //Creates the outer walls and floor.
            BoardSetup();

            //Reset our list of gridpositions.
            InitialiseList();

            // CSV Log File creation and appends titles
            csv = new WriteToCSV(logFileName, level);

            //Generate map for calculation
            cell = map.GenerateMap(columns,rows);

            //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum, level, playerFoodPoints);

            //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum, level, playerFoodPoints);

            //Determine number of enemies based on current level number, based on a logarithmic progression
            int enemyCount = 0;
            if (foodFitness <= 40)
            {
                enemyCount = (int)Mathf.Log(level, 2f);
            }
            if (foodFitness > 40 && foodFitness <= 80)
            {
                enemyCount = (int)Mathf.Log(level, 2f) + 1;
            }
            if (foodFitness > 80)
            {
                enemyCount = (int)Mathf.Log(level, 2f) + 2;
            }

            //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount, level, playerFoodPoints);

            map.DrawMap(cell);
            //Instantiate the exit tile in the upper right hand corner of our game board
            Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);

            // Close the stream to CSV file
            csv.AppendSolution(noOfWallTilesArray, noOfFoodTilesArray, enemyCount, percentageEnemyCoverage);
            csv.Stop();
        }

        void setObjectMinMaxCountTargetFitness(double[] ObjectMinMaxCountTargetFitness, int minimum, int maximum, int objectCount, double targetValue, double fitnessValue)
        {
            ObjectMinMaxCountTargetFitness[0] = minimum;
            ObjectMinMaxCountTargetFitness[1] = maximum;
            ObjectMinMaxCountTargetFitness[2] = objectCount;
            ObjectMinMaxCountTargetFitness[3] = targetValue;
            ObjectMinMaxCountTargetFitness[4] = fitnessValue;
        }

        void addToCoverage(List<Vector3> coverage, Vector3 position)
        {
            //Add positions to coveredArea Array
            if (!coverage.Contains(position) && !outerWalls.Contains(position) && !obstacles.Contains(position))
            {
                coverage.Add(position);
            }
        }

        void calculateCoverageOfMap()
        {
            //Calculate coverage of Map
            Debug.Log("Positions:" + gridPositions.Count);

            double eCoverage = enemyCoverage.Count;
            percentageEnemyCoverage = Math.Round(((eCoverage / 49) * 100), 2);
            Debug.Log("Coverage by Enemies: " + eCoverage);
            Debug.Log("Percentage of Enemy Coverage: " + percentageEnemyCoverage);
        }

        //LOGGING: Number of wall and food tiles
        void loggingWallAndFoodTiles(GameObject[] tileArray, double optimalValue, double actualvalue)
        {

            if (tileArray == wallTiles)
            {
                double noOfWallTiles = actualvalue;
                Debug.Log("Optimal Value of walls: " + optimalValue);
            }

            if (tileArray == foodTiles)
            {
                double noOfFoodTiles = actualvalue;
                Debug.Log("Optimal Value of food: " + optimalValue);
            }
            if (tileArray == enemyTiles)
            {
                double noOfEnemies = actualvalue;
                Debug.Log("Optimal Value of enemies: " + optimalValue);
            }
        }

        //Fill function for wall and food array
        void fillArray(double[] ArrayToFill, int minimum, int maximum, int actualValue, double targetValue, double fitnessValue)
        {
            ArrayToFill[0] = minimum;
            ArrayToFill[1] = maximum;
            ArrayToFill[2] = actualValue;
            ArrayToFill[3] = targetValue;
            ArrayToFill[4] = fitnessValue;
        }

        void setTargetValue(double targetValue, GameObject[] tileArray, int minimum)
        {
            if (tileArray == enemyTiles)
            {
                targetValue = Random.Range((minimum * 3 + 2), (minimum * 5));
            }
            if (tileArray == foodTiles)
            {
                if (exploration >= 0.7)
                {
                    targetValue = Random.Range(0, 3);
                }
                if (exploration < 0.7 && exploration >= 0.4)
                {
                    targetValue = Random.Range(3, 6);
                }
                if (exploration < 0.4)
                {
                    targetValue = Random.Range(6, 9);
                }
            }
            else
            {
                targetValue = 0.55;
            }
        }
    }
}
