//FloodFill
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Completed
{
    public class Cell
    {
        public int x;
        public int y;
        public string type;
        /*
		Available types:
			"n" - neutral,
			"e" - enemy,
			"s" - soda,
			"f" - food,
			"w" - wall,
			"p" - passed
		*/
        public Cell(int x, int y, string type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
    }
    public class FloodFillMap
    {
        //Transform map to another view
        public Cell[,] GenerateMap(int columns, int rows)
        {
            Cell[,] cells = new Cell[columns, rows];
            for (int i = 0; i < cells.GetLength(1); i++)
            {
                for (int j = 0; j < cells.GetLength(0); j++)
                {
                    cells[i, j] = new Cell(i, j, "n");
                }
            }
            return cells;
        }

        public void DrawMap(Cell[,] cells)
        {
            FileStream fs;      // Filestream for connecting to the session csv file
            StreamWriter sw;    // StreamWriter for writing into the session csv file
            string filePath;    // String for storing the session-file file path
            string fileName = "map";
            int level;
            string savePath = Environment.CurrentDirectory + "/maps";
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            filePath = string.Format("{0}/{1}_{2}.txt", savePath, dateTime, fileName);

            fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
            sw = new StreamWriter(fs);
            string str = "";
            for (int i = cells.GetLength(1) - 1; i >= 0; i--)
            {
                for (int j = 0; j < cells.GetLength(0); j++)
                {
                    str += cells[j, i].type + "(" + cells[j, i].x.ToString() + "," + cells[j, i].y.ToString() + ") ";
                }
                //Debug.Log(str);
                sw.WriteLine(str);
                sw.Flush();
                str = "";
            }
            sw.Close();
            fs.Close();
        }

        public bool checkAvailability(Cell cell)
        {
            bool isAvailable = false;
            if (cell.type == "n" || cell.type == "s" || cell.type == "f")
            {
                isAvailable = true;
            }
            return isAvailable;
        }

        public Cell[,] AddCell(Cell[,] cell, Vector3 position, string tag)
        {
            cell[(int)Math.Round(position.x), (int)Math.Round(position.y)].x = (int)Math.Round(position.x);
            cell[(int)Math.Round(position.x), (int)Math.Round(position.y)].y = (int)Math.Round(position.y);

            Dictionary<string, string> elements = new Dictionary<string, string>();
            elements.Add("Wall", "w");
            elements.Add("Enemy", "e");
            elements.Add("Soda", "s");
            elements.Add("Food", "f");
            string value = "";
            if (elements.TryGetValue(tag, out value))
            {
                cell[(int)Math.Round(position.x), (int)Math.Round(position.y)].type = value;
            }
            return cell;
        }

        public double GetExplorationRatio(Cell[,] cells, Cell start, Cell end)
        {
            cells = FloodFill(cells, start, end);
            double neutralTiles = System.Convert.ToDouble(CountTiles(cells, "n"));
            double passedTiles = System.Convert.ToDouble(CountTiles(cells, "p"));
            double ratio = passedTiles / (neutralTiles + passedTiles);

            Debug.Log("All passible - " + (neutralTiles + passedTiles));
            Debug.Log("Passed - " + passedTiles);
            return ratio;
        }

        public void DeleteTiles(Cell[,] cells, string type)
        {
            for (int i = 0; i < cells.GetLength(1); i++)
            {
                for (int j = 0; j < cells.GetLength(0); j++)
                {
                    if (cells[i, j].type == type)
                    {
                        cells[i, j].type = "n";
                    }
                }
            }
        }

        public int GetMapFoodValue(Cell[,] cells)
        {
            int foodValue = 0;
            for (int i = 0; i < cells.GetLength(1); i++)
            {
                for (int j = 0; j < cells.GetLength(0); j++)
                {
                    if (cells[i, j].type == "s")
                    {
                        foodValue += 20;
                    }
                    if (cells[i, j].type == "f")
                    {
                        foodValue += 10;
                    }
                }
            }
            return foodValue;
        }

        public int GetChallengeMeasure(Cell[,] cells, int food, int challengeValue)
        {
            int answer = food;
            answer += GetMapFoodValue(cells);
            answer -= CountTiles(cells, "w") * 4 + CountTiles(cells, "n");
            answer = GreaterValue(0, answer - challengeValue);
            return answer;
        }


        public int SmallerValue(int one, int two)
        {
            if (one < two)
            {
                return one;
            }
            else
            {
                return two;
            }
        }

        public int GreaterValue(int one, int two)
        {
            if (one > two)
            {
                return one;
            }
            else
            {
                return two;
            }
        }

        public int CountTiles(Cell[,] cells, string value)
        {
            int numberOfTiles = 0;
            for (int i = 0; i < cells.GetLength(1); i++)
            {
                for (int j = 0; j < cells.GetLength(0); j++)
                {
                    if (cells[i, j].type == value)
                    {
                        numberOfTiles += 1;
                    }
                }
            }
            return numberOfTiles;
        }


        public Cell[,] FloodFill(Cell[,] cells, Cell start, Cell end)
        {
            Queue<Cell> q = new Queue<Cell>();
            q.Enqueue(start);
            Cell current;
            cells[start.x, start.y].type = "p";
            bool endReached = false;
            while (q.Count > 0)
            {
                current = q.Dequeue();

                if (current.x + 1 < cells.GetLength(0))
                {
                    if (checkAvailability(cells[current.x + 1, current.y]))
                    {
                        cells[current.x + 1, current.y].type = "p";
                        q.Enqueue(cells[current.x + 1, current.y]);
                    }
                }
                if (current.y + 1 < cells.GetLength(1))
                {
                    if (checkAvailability(cells[current.x, current.y + 1]))
                    {
                        cells[current.x, current.y + 1].type = "p";
                        q.Enqueue(cells[current.x, current.y + 1]);
                    }
                }
                if (current.x - 1 >= 0)
                {
                    if (checkAvailability(cells[current.x - 1, current.y]))
                    {
                        cells[current.x - 1, current.y].type = "p";
                        q.Enqueue(cells[current.x - 1, current.y]);
                    }
                }
                if (current.y - 1 >= 0)
                {
                    if (checkAvailability(cells[current.x, current.y - 1]))
                    {
                        cells[current.x, current.y - 1].type = "p";
                        q.Enqueue(cells[current.x, current.y - 1]);
                    }
                }

            }
            return cells;
        }
    }
}