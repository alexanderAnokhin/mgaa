﻿using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

namespace Completed
	
{
	
	public class BoardManager : MonoBehaviour
	{
		// Using Serializable allows us to embed a class with sub properties in the inspector.
		[Serializable]
		public class Count
		{
			public int minimum; 			//Minimum value for our Count class.
			public int maximum; 			//Maximum value for our Count class.
			
			
			//Assignment constructor.
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}


	    public string logFileName;                                      //Name of the LogFile
        public int columns = 8; 										//Number of columns in our game board.
		public int rows = 8;											//Number of rows in our game board.
		public Count wallCount = new Count (5, 9);						//Lower and upper limit for our random number of walls per level.
		public Count foodCount = new Count (1, 5);						//Lower and upper limit for our random number of food items per level.
		public GameObject exit;											//Prefab to spawn for exit.
		public GameObject[] floorTiles;									//Array of floor prefabs.
		public GameObject[] wallTiles;									//Array of wall prefabs.
		public GameObject[] foodTiles;									//Array of food prefabs.
		public GameObject[] enemyTiles;									//Array of enemy prefabs.
		public GameObject[] outerWallTiles;								//Array of outer tile prefabs.

	    private WriteToCSV csv;                                         //Instantiate the CSV file

        private Transform boardHolder;									//A variable to store a reference to the transform of our Board object.
		private List <Vector3> gridPositions = new List <Vector3> ();   //A list of possible locations to place tiles.

        private List<Vector3> areaCovered = new List<Vector3>();        //A list of covered Area by Gameobjects
        private List<Vector3> enemyCoverage = new List<Vector3>();      //A list of covered Area by Enemies

        private double percentageAreaCovered;                           //Percentage of the map covered by elements
        private double percentageEnemyCoverage;                         //Percentage of the map covered by enemies

        //private int noOfWallTiles;                                      //Number of initialised wall tiles
        //private int noOfFoodTiles;                                      //Number of initialised food tiles

        private Vector3 randomPosition;                                 //Vector of randomPosition
        private List<Vector3> bestPositionSolution = new List<Vector3>();


        //Clears our list gridPositions and prepares it to generate a new board.
        void InitialiseList ()
		{
			//Clear our list gridPositions.
			gridPositions.Clear ();
            areaCovered.Clear();
            enemyCoverage.Clear();

            //Loop through x axis (columns).
            for (int x = 1; x < columns-1; x++)
			{
				//Within each column, loop through y axis (rows).
				for(int y = 1; y < rows-1; y++)
				{
					//At each index add a new Vector3 to our list with the x and y coordinates of that position.
					gridPositions.Add (new Vector3(x, y, 0f));
				}
			}
		}
		
		
		//Sets up the outer walls and floor (background) of the game board.
		void BoardSetup ()
		{
			//Instantiate Board and set boardHolder to its transform.
			boardHolder = new GameObject ("Board").transform;
			
			//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
			for(int x = -1; x < columns + 1; x++)
			{
				//Loop along y axis, starting from -1 to place floor or outerwall tiles.
				for(int y = -1; y < rows + 1; y++)
				{
					//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
					GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];
					
					//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
					if(x == -1 || x == columns || y == -1 || y == rows)
						toInstantiate = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
					
					//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
					GameObject instance =
						Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
					
					//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
					instance.transform.SetParent (boardHolder);
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
        double[] LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            //Return Array
            double[] ObjectMinMaxCountOptimal = new double[4];

            //Optimal Value
            double optimalValue = Math.Round((double)((minimum + maximum) / 2), 0);
            Debug.Log("OPTIMAL VALUE: " + optimalValue);

            for (int j = 0; j < 4; j++)
            {

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

                tempPositions.Clear();

                // Get Range for the different content types and the actual implementation
                setObjectMinMaxCountOptimal(ObjectMinMaxCountOptimal, minimum, maximum, objectCount, optimalValue);
                Debug.Log(ObjectMinMaxCountOptimal);

                for (int i = 0; i < objectCount; i++)
                {
                    randomPosition = RandomPosition(tempGrid);
                    tempPositions.Add(randomPosition);

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

                        //Add positions to coveredArea Array
                        addToCoverage(areaCovered, randomPosition);
                        addToCoverage(areaCovered, upperArea);
                        addToCoverage(areaCovered, lowerArea);
                        addToCoverage(areaCovered, leftArea);
                        addToCoverage(areaCovered, rightArea);

                        if (i == objectCount - 1)
                        {
                            Debug.Log("ENEMY COUNT " + enemyCoverage.Count);
                            if (Math.Abs(enemyCoverage.Count - optimalValue) < Math.Abs(tempCoverage - optimalValue))
                            {
                                bestCoverage = enemyCoverage.Count;
                                bestPositionSolution = tempPositions;
                            }
                        }
                    }

                    //Occupied Area around other Tiles
                    else
                    {
                        areaCovered.Add(randomPosition);

                        if (i == objectCount - 1)
                        {
                            Debug.Log("WALL COUNT " + areaCovered.Count);
                            if (Math.Abs(areaCovered.Count - optimalValue) < Math.Abs(tempCoverage - optimalValue))
                            {
                                bestCoverage = areaCovered.Count;
                                bestPositionSolution = tempPositions;
                            }
                        }
                    }
                }

                if (j == 3)
                {
                    for (int h = 0; h < bestPositionSolution.Count - 1; h++)
                    {
                        //Choose a random tile from tileArray and assign it to tileChoice
                        GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
                        Debug.Log("Best COUNT: " + bestPositionSolution.Count);

                        Instantiate(tileChoice, RandomPosition(bestPositionSolution), Quaternion.identity);
                    }
                }
            }

            calculateCoverageOfMap();
            loggingWallAndFoodTiles(tileArray, optimalValue);

            return ObjectMinMaxCountOptimal;
        }
            

        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene (int level)
		{
			//Creates the outer walls and floor.
			BoardSetup ();
			
			//Reset our list of gridpositions.
			InitialiseList ();

            // CSV Log File creation and appends titles
            csv = new WriteToCSV(logFileName, level);

            //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
            double[] noOfWallTilesArray = LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum);
			
			//Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
			double[] noOfFoodTilesArray = LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum);
			
			//Determine number of enemies based on current level number, based on a logarithmic progression
			int enemyCount = (int)Mathf.Log(level, 2f);
			
			//Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
			LayoutObjectAtRandom (enemyTiles, enemyCount, enemyCount);
			
			//Instantiate the exit tile in the upper right hand corner of our game board
			Instantiate (exit, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);

            // Close the stream to CSV file
            csv.AppendSolution(noOfWallTilesArray, noOfFoodTilesArray, enemyCount, percentageEnemyCoverage, percentageAreaCovered);
            csv.Stop();
		}

        void setObjectMinMaxCountOptimal(double[] ObjectMinMaxCountOptimal, int minimum, int maximum, int objectCount, double optimalValue)
        {
            ObjectMinMaxCountOptimal[0] = minimum;
            ObjectMinMaxCountOptimal[1] = maximum + 1;
            ObjectMinMaxCountOptimal[2] = objectCount;
            ObjectMinMaxCountOptimal[3] = optimalValue;
        }

        void addToCoverage(List<Vector3> coverage, Vector3 position)
	    {
            //Add positions to coveredArea Array
            if (!coverage.Contains(position))
            {
                coverage.Add(position);
            }
        }

	    void calculateCoverageOfMap()
	    {
            //Calculate coverage of Map
            Debug.Log("Positions:" + gridPositions.Count);

            double coverage = areaCovered.Count;
            percentageAreaCovered = Math.Round(((coverage / 49) * 100), 2);
            Debug.Log("Coverage of Map:" + coverage);
            Debug.Log("Percentage Coverage: " + percentageAreaCovered + " %");

            double eCoverage = enemyCoverage.Count;
            percentageEnemyCoverage = Math.Round(((eCoverage / 49) * 100), 2);
            Debug.Log("Coverage by Enemies: " + eCoverage);
            Debug.Log("Percentage of Enemy Coverage: " + percentageEnemyCoverage);
        }

        //LOGGING: Number of wall and food tiles
	    void loggingWallAndFoodTiles(GameObject[] tileArray, double optimalValue)
	    {
            
            if (tileArray == wallTiles)
            {
                int noOfWallTiles = tileArray.Length;
                Debug.Log("Number of walls: " + noOfWallTiles);
                Debug.Log("Optimal Value of walls: " + optimalValue);
            }

            if (tileArray == foodTiles)
            {
                int noOfFoodTiles = tileArray.Length;
                Debug.Log("Number of food: " + noOfFoodTiles);
                Debug.Log("Optimal Value of food: " + optimalValue);
            }
            if (tileArray == enemyTiles)
            {
                int noOfEnemies = tileArray.Length;
                Debug.Log("Number of enemies: " + noOfEnemies);
                Debug.Log("Optimal Value of enemies: " + optimalValue);
            }
        }
    }
}
