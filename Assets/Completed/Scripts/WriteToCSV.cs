using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Completed
{
    public class WriteToCSV
    {
        private FileStream fs;      // Filestream for connecting to the session csv file
        private StreamWriter sw;    // StreamWriter for writing into the session csv file
        private string filePath;    // String for storing the session-file file path
        private string fileName;    
        private int level;       

        public WriteToCSV(string fileName, int level)
        {
            this.fileName = fileName + "_" + level;
            this.level = level;
            CreateLogFile();            // Creates the log file in the Logs directory
            AppendTitles();             // Appends the first row of the file
        }

        public void CreateLogFile()
        {
            string savePath = Environment.CurrentDirectory + "/CSVLogs";
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            filePath = string.Format("{0}/{1}_{2}.csv", savePath, dateTime, fileName);

            fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
            sw = new StreamWriter(fs);
        }

        public void AppendTitles()
        {
            StringBuilder titleText = new StringBuilder("DateTime,Day,");
            titleText.Append("Min Wall Tiles,Max Wall Titles,Wall Tiles,Wall Tiles Target Value,Exploration");
            titleText.Append("Min Food Tiles,Max Food Tiles,Food Tiles,Food Tiles Target Value,FoodFitness");
            titleText.Append("Enemies,Enemy coverage,");
            sw.WriteLine(titleText.ToString());
            sw.Flush();
        }

        public void AppendSolution(double[] noOfWallTiles, double[] noOfFoodTiles, int noOfEnemies, double enemyCoverage)
        {
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            StringBuilder solutionText = new StringBuilder(dateTime + "," + level + ",");
            solutionText.Append(noOfWallTiles[0] + "," + noOfWallTiles[1] + "," + noOfWallTiles[2] + "," + noOfWallTiles[3] + "," + noOfWallTiles[4]);
            solutionText.Append(noOfFoodTiles[0] + "," + noOfFoodTiles[1] + "," + noOfFoodTiles[2] + "," + noOfFoodTiles[3] + "," + noOfFoodTiles[4]);
            solutionText.Append(noOfEnemies + "," + enemyCoverage);
            sw.WriteLine(solutionText.ToString());
            sw.Flush();
        }

        public void Stop()
        {
            sw.Close();
            fs.Close();
        }
    }
}