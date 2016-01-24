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

        public WriteToCSV(string fileName)
        {
            this.fileName = fileName;
            CreateLogFile();            // Creates the log file in the Logs directory
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
            // If the file is empty write the title row with parameter names
            if (new FileInfo(filePath).Length == 0)
            {
                StringBuilder titleText = new StringBuilder("DateTime,Wall Tiles,Food Tiles,Enemies");
                sw.WriteLine(titleText.ToString());
                sw.Flush();
            }
        }

        public void AppendSolution(int noOfWallTiles, int noOfFoodTiles, int noOfEnemies)
        {
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            StringBuilder solutionText = new StringBuilder(dateTime + "," + noOfWallTiles + "," + noOfFoodTiles + "," + noOfEnemies);
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