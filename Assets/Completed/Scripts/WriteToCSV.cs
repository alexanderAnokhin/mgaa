using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Completed
{
    public class WriteToCSV
    {
        private FileStream fsWall;      // Filestream for connecting to the wall csv file
        private StreamWriter swWall;    // StreamWriter for writing into the wall csv file
        private FileStream fsFood;      // Filestream for connecting to the food csv file
        private StreamWriter swFood;    // StreamWriter for connecting to the food csv file
        private FileStream fsEnemy;     // Filestream for connecting to the enemy csv file
        private StreamWriter swEnemy;   // StreamWriter for connecting to the enemy csv file

        private string wallFileName = "Wall_Day_";      // Set filename for the wall csv file
        private string foodFileName = "Food_Day_";      // Set filename for the food csv file
        private string enemyFileName = "Enemy_Day_";    // Set filename for the enemy csv file
        private int level;                              // Variable to save the current level

        /// <summary>
        /// Constructor for the CSV file
        /// </summary>
        /// <param name="level">Level needed for the file name</param>
        public WriteToCSV(int level)
        {
            this.wallFileName = wallFileName + level;
            this.foodFileName = foodFileName + level;
            this.enemyFileName = enemyFileName + level;
            this.level = level;
            CreateLogFiles();           // Creates the log files in the Logs directory
            AppendAllTitles();          // Appends the title row to every csv file
        }

        /// <summary>
        /// Creates all csv files (wall, food, enemy)
        /// Opens streams to the files to write into them
        /// </summary>
        public void CreateLogFiles()
        {
            string savePath = Environment.CurrentDirectory + "/CSVLogs";
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            string wallFilePath = string.Format("{0}/{1}_{2}.csv", savePath, dateTime, wallFileName);
            fsWall = new FileStream(wallFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
            swWall = new StreamWriter(fsWall);

            string foodFilePath = string.Format("{0}/{1}_{2}.csv", savePath, dateTime, foodFileName);
            fsFood = new FileStream(foodFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
            swFood = new StreamWriter(fsFood);

            string enemyFilePath = string.Format("{0}/{1}_{2}.csv", savePath, dateTime, enemyFileName);
            fsEnemy = new FileStream(enemyFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
            swEnemy = new StreamWriter(fsEnemy);
        }

        /// <summary>
        /// Appends titles to all three csv files simultaneously
        /// </summary>
        public void AppendAllTitles()
        {
            AppendWallTitles();
            AppendFoodTitles();
            AppendEnemyTitles();
        }

        /// <summary>
        /// Appends titles specific to the wall csv file
        /// </summary>
        public void AppendWallTitles()
        {
            StringBuilder titleText = new StringBuilder("DateTime,Day,");
            titleText.Append("Min Wall Tiles,Max Wall Titles,Wall Tiles,Exploration Target Value,Exploration,Difference");
            swWall.WriteLine(titleText.ToString());
            swWall.Flush();
        }

        /// <summary>
        /// Appends titles specific to the food csv file
        /// </summary>
        public void AppendFoodTitles()
        {
            StringBuilder titleText = new StringBuilder("DateTime,Day,");
            titleText.Append("Min Food Tiles,Max Food Tiles,Food Tiles,Food Tiles Target Value,Food Fitness");
            swFood.WriteLine(titleText.ToString());
            swFood.Flush();
        }

        /// <summary>
        /// Appends titles specific to the enemy csv file
        /// </summary>
        public void AppendEnemyTitles()
        {
            StringBuilder titleText = new StringBuilder("DateTime,Day,");
            titleText.Append("Enemies,Target Value,Enemy coverage");
            swEnemy.WriteLine(titleText.ToString());
            swEnemy.Flush();
        }

        /// <summary>
        /// Appends all solutions to the wall csv file
        /// </summary>
        /// <param name="solution">Array which stores all solutions</param>
        public void AppendWallSolution(double[,] solution)
        {
            // Get current DateTime for first input of a row
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Append the solutions row by row
            for (int i = 0; i < 11; i++)
            {
                StringBuilder solutionText = new StringBuilder();
                // If we're at the last entry of the array we output the Best Solution
                if (i == 10)
                {
                    solutionText.Append("Best Solution," + level + ",");
                    for (int j = 0; j < 5; j++)
                    {
                        solutionText.Append(solution[i, j] + ",");
                    }
                    solutionText.Append(solution[i, 5]);
                    swWall.WriteLine(solutionText.ToString());
                    swWall.Flush();
                }
                // Else we just write all the other solutions in order
                else
                {
                    solutionText.Append(dateTime + "," + level + ",");
                    for (int j = 0; j < 5; j++)
                    {
                        solutionText.Append(solution[i, j] + ",");
                    }
                    solutionText.Append(solution[i, 5]);
                    swWall.WriteLine(solutionText.ToString());
                    swWall.Flush();
                }
            }
        }

        /// <summary>
        /// Appends all solutions to the food csv file
        /// </summary>
        /// <param name="solution">Array which stores all solutions</param>
        public void AppendFoodSolution(double[,] solution)
        {
            // Get current DateTime for first input of a row
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Append the solutions row by row
            for (int i = 0; i < 11; i++)
            {
                StringBuilder solutionText = new StringBuilder();
                // If we're at the last entry of the array we output the Best Solution
                if (i == 10)
                {
                    solutionText.Append("Best Solution," + level + ",");
                    for (int j = 0; j < 4; j++)
                    {
                        solutionText.Append(solution[i, j] + ",");
                    }
                    solutionText.Append(solution[i, 4]);
                    swFood.WriteLine(solutionText.ToString());
                    swFood.Flush();
                }
                // Else we just write all the other solutions in order
                else
                {
                    solutionText.Append(dateTime + "," + level + ",");
                    for (int j = 0; j < 4; j++)
                    {
                        solutionText.Append(solution[i, j] + ",");
                    }
                    solutionText.Append(solution[i, 4]);
                    swFood.WriteLine(solutionText.ToString());
                    swFood.Flush();
                }
            }
        }

        /// <summary>
        /// Appends all solutions to the food csv file
        /// </summary>
        /// <param name="solution">Array which stores all solutions</param>
        public void AppendEnemySolution(double[,] solution)
        {
            // Get current DateTime for first input of a row
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Append the solutions row by row
            for (int i = 0; i < 11; i++)
            {
                StringBuilder solutionText = new StringBuilder();
                // If we're at the last entry of the array we output the Best Solution
                if (i == 10)
                {
                    solutionText.Append("Best Solution" + "," + level + ",");
                    solutionText.Append(solution[i, 0] + "," + solution[i, 1] + "," + solution[i, 2]);
                    swEnemy.WriteLine(solutionText.ToString());
                    swEnemy.Flush();
                }
                // Else we just write all the other solutions in order
                else
                {
                    solutionText.Append(dateTime + "," + level + ",");
                    solutionText.Append(solution[i, 0] + "," + solution[i, 1] + "," + solution[i, 2]);
                    swEnemy.WriteLine(solutionText.ToString());
                    swEnemy.Flush();
                }
            }
        }

        /// <summary>
        /// Close the connection to all csv files
        /// </summary>
        public void Stop()
        {
            swWall.Close();
            fsWall.Close();
            swFood.Close();
            fsFood.Close();
            swEnemy.Close();
            fsEnemy.Close();
        }
    }
}
