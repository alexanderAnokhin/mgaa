using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Completed
{
    public class WriteToCSV
    {
        private FileStream fsWall;      // Filestream for connecting to the session csv file
        private StreamWriter swWall;    // StreamWriter for writing into the session csv file
        private FileStream fsFood;
        private StreamWriter swFood;
        private FileStream fsEnemy;
        private StreamWriter swEnemy;

        private string wallFileName = "Wall_Day_";
        private string foodFileName = "Food_Day_";
        private string enemyFileName = "Enemy_Day_";
        private int level;       

        public WriteToCSV(int level)
        {
            this.wallFileName = wallFileName + level;
            this.foodFileName = foodFileName + level;
            this.enemyFileName = enemyFileName + level;
            this.level = level;
            CreateLogFiles();            // Creates the log file in the Logs directory
            AppendAllTitles();
        }

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

        public void AppendAllTitles()
        {
            AppendWallTitles();
            AppendFoodTitles();
            AppendEnemyTitles();
        }

        public void AppendWallTitles()
        {
            StringBuilder titleText = new StringBuilder("DateTime,Day,");
            titleText.Append("Min Wall Tiles,Max Wall Titles,Wall Tiles,Wall Tiles Target Value,Exploration,");
            swWall.WriteLine(titleText.ToString());
            swWall.Flush();
        }

        public void AppendFoodTitles()
        {
            StringBuilder titleText = new StringBuilder("DateTime,Day,");
            titleText.Append("Min Food Tiles,Max Food Tiles,Food Tiles,Food Tiles Target Value,Food Fitness,");
            swFood.WriteLine(titleText.ToString());
            swFood.Flush();
        }

        public void AppendEnemyTitles()
        {
            StringBuilder titleText = new StringBuilder("DateTime,Day,");
            titleText.Append("Enemies,Enemy coverage");
            swEnemy.WriteLine(titleText.ToString());
            swEnemy.Flush();
        }

        public void AppendWallSolution(double[,] solution)
        {
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            for (int i = 0; i < 10; i++)
            {
                StringBuilder solutionText = new StringBuilder(dateTime + "," + level + ",");
                for (int j = 0; j < 5; j++)
                {
                    solutionText.Append(solution[i, j] + ",");
                }
                solutionText.Append(solution[i, 5]);
                swWall.WriteLine(solutionText.ToString());
                swWall.Flush();
            }
        }

        public void AppendFoodSolution(double[,] solution)
        {
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            for (int i = 0; i < 10; i++)
            {
                StringBuilder solutionText = new StringBuilder(dateTime + "," + level + ",");
                for (int j = 0; j < 4; j++)
                {
                    solutionText.Append(solution[i, j] + ",");
                }
                solutionText.Append(solution[i, 4]);
                swFood.WriteLine(solutionText.ToString());
                swFood.Flush();
            }
        }

        public void AppendEnemySolution(double[,] solution)
        {
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            for (int i = 0; i < 10; i++)
            {
                StringBuilder solutionText = new StringBuilder(dateTime + "," + level + ",");
                solutionText.Append(solution[i, 0] + "," + solution[i, 1]);
                swEnemy.WriteLine(solutionText.ToString());
                swEnemy.Flush();
            }
        }

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
