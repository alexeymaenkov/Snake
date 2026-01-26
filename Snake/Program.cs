using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using static System.Formats.Asn1.AsnWriter;

namespace Snake
{
    internal class Program
    {
        static int[] currentDirection = { 0, 0 };
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            Console.Title = "SNAKE PLISSKEN";

            int Xmap = 70;
            int Ymap = Xmap/2;

            Console.WindowHeight = Ymap + 15;
            Console.WindowWidth = Xmap + 45;

            Console.ForegroundColor = ConsoleColor.Red;
            char[,] logo = ReadLogo("logo.txt");
            DrawLogo(logo);

            
            Console.Write("\n\n                                     ->   ENTER YOUR NAME: ");
            Console.ForegroundColor = ConsoleColor.White;
            Player player = new Player(Console.ReadLine());
            Console.ResetColor();
            Console.Clear();

            Console.CursorVisible = false;

            Random random = new Random();

            int level = 1;
            int speed = 300;
            int score = 0;

            string rerun = "y";


            int snakeStartX = Xmap / 2;
            int snakeStartY = Ymap / 2;

            (int X, int Y) startPosition = (snakeStartX, snakeStartY);

            List<(int X, int Y)> snakeList = new List<(int, int)>
                {
                (snakeStartX, snakeStartY)
                };

            while (rerun == "y")
            {
                int snakePreyX = random.Next(1, Xmap - 1);
                int snakePreyY = random.Next(1, Ymap - 1);

                int snakeEnemyX = random.Next(6, Xmap - 6);
                int snakeEnemyY = random.Next(6, Ymap - 6);

                int directionEnemyX = 1;
                int directionEnemyY = 1;

                Console.SetCursorPosition(0, 0);
                ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();

                bool gameOver = false;
                bool levelUp = false;

                bool isPaused = false;

                int foodNumbers = 15;

                List<(int X, int Y)> foodList = new List<(int, int)> { };

                char[,] map = GetMap(Xmap, Ymap, foodNumbers);
                DrawMap(map);
                GetFoodList(Xmap, Ymap, ref foodList, foodNumbers, random);

                LeaderboardJson.ShowTopInGame(Xmap, 10);

                bool preyRip = true;
                bool enemyRip = true;
                bool preyLife = true;
                bool enemyLife = true;

                while (gameOver == false)
                {
                    if (Console.KeyAvailable)
                    {
                        pressedKey = Console.ReadKey(true);
                        if (pressedKey.Key == ConsoleKey.Spacebar)
                        {
                            isPaused = !isPaused;
                        }
                        else if (pressedKey.Key == ConsoleKey.Escape)
                        {
                            gameOver = true;
                        }
                    }
                    
                    if (isPaused == true)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(85, 33);
                        Console.Write(" >>>SPACE<<<");
                        Console.SetCursorPosition(85, 34);
                        Console.Write(" ----------- ");
                        Console.SetCursorPosition(85, 35);
                        Console.Write("| P A U S E |");
                        Console.SetCursorPosition(85, 36);
                        Console.Write(" ----------- ");
                        Console.ResetColor();
                        Console.ReadKey(true);
                        isPaused = !isPaused;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.SetCursorPosition(85, 34);
                        Console.Write(" ----------- ");
                        Console.SetCursorPosition(85, 35);
                        Console.Write("| P A U S E |");
                        Console.SetCursorPosition(85, 36);
                        Console.Write(" ----------- ");
                        Console.ResetColor();
                    }

                    for (int i = 0; i < foodList.Count; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.SetCursorPosition(foodList[i].X, foodList[i].Y);
                        Console.Write('*');
                    }

                    DrawInfo(ref Xmap, ref Ymap, ref level, ref speed, ref score);

                    HandleInput(pressedKey, snakeList, ref foodList, map, ref gameOver, ref levelUp, ref score, ref startPosition, ref speed, ref Xmap, ref Ymap);
                    
                    var head = snakeList[0];

                    CheckSpecialObjectsCollision(head, snakeList, ref snakePreyX, ref snakePreyY, ref snakeEnemyX, ref snakeEnemyY, ref gameOver, ref levelUp, ref score, ref startPosition, ref preyLife, ref enemyLife, ref speed, ref preyRip, ref enemyRip);

                    if (enemyLife == true/* && level > 2*/)
                    {
                        enemyRip = false;
                        bool colorFrame = false;
                        DrawEnemy(random, ref snakeList, ref foodList, map, ref snakeEnemyX, ref snakeEnemyY, ref score, ref enemyLife, ref Xmap, ref Ymap, ref colorFrame, ref directionEnemyX, ref directionEnemyY);
                    }

                    if (preyLife == true/* && level > 1*/)
                    {
                        preyRip = false;
                        DrawPrey(random, ref snakeList, map, ref snakePreyX, ref snakePreyY, ref score, ref preyLife);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.SetCursorPosition(snakePreyX, snakePreyY);
                        Console.Write("S");
                    }

                    for (int i = 0; i < snakeList.Count; i++)
                    {
                        Console.SetCursorPosition(snakeList[i].X, snakeList[i].Y);
                        if (i == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("@");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("*");
                            Console.ResetColor();
                        }
                    }

                    Thread.Sleep(speed);

                    if (foodList.Count == 0)
                    {
                        speed = speed - (speed / 20);
                        level++;
                        levelUp = true;
                        preyLife = true;
                        enemyLife = true;
                        preyRip = false;
                        rerun = "y";
                        foodList.Clear();
                        GetFoodList(Xmap, Ymap, ref foodList, foodNumbers, random);
                    }
                }
                if (levelUp == false)
                {
                    Console.Clear();
                    LeaderboardJson.SaveScore(player.Name, score, level);
                    LeaderboardJson.ShowTop(5);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nGAME OVER\n");
                    Console.WriteLine($"Level: {level}  ||  Speed: {speed}  ||  Score: {score}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\nХотите попробовать еще раз?");
                    Console.WriteLine("\n\nENTER - да");
                    Console.WriteLine("\n\nESC - нет");
                    Console.ResetColor();
                    ConsoleKeyInfo userKeyChoise = Console.ReadKey();
                    if (userKeyChoise.Key == ConsoleKey.Enter)
                    {
                        rerun = "y";
                    }
                    else if (userKeyChoise.Key == ConsoleKey.Escape)
                    {
                        rerun = "n";
                    }
                    Console.Clear();
                    speed = 300;
                    level = 1;
                    score = 0;
                }
            }
        }
        //==================================================LOGO=================================================================

        private static int GetMaxLenghtOfLine(string[] lines)
        {
            int maxLenght = lines[0].Length;
            return maxLenght;
        }
        private static char[,] ReadLogo(string path)
        {
            string[] file = File.ReadAllLines("logo.txt");

            char[,] logo = new char[GetMaxLenghtOfLine(file), file.Length];

            for (int x = 0; x < logo.GetLength(0); x++)
                for (int y = 0; y < logo.GetLength(1); y++)
                    logo[x, y] = file[y][x];

            return logo;
        }
        private static void DrawLogo(char[,] logo)
        {
            for (int y = 0; y < logo.GetLength(1); y++)
            {
                for (int x = 0; x < logo.GetLength(0); x++)
                {
                    Console.Write(logo[x, y]);
                }
                Console.Write("\n");
                Thread.Sleep(20);
            }
        }
        //===================================================MAP=============================================================

        private static char[,] GetMap(int X, int Y, int foodNumbers)
        {
            char border = '#';
            char field = ' ';

            char[,] map = new char[X, Y];

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if ((i == 0) || (j == 0) || (i == X - 1) || (j == Y - 1))
                    {
                        map[i, j] = border;
                    }
                    else { map[i, j] = field; }
                }
            }
            return map;
        }
        //==============================================FOODLIST================================================================

        private static void GetFoodList(int Xmap, int Ymap, ref List<(int X, int Y)> foodList, int foodNumbers, Random random)
        {
            for (int i = 0; i < foodNumbers; i++)
            {
                (int X, int Y) newFood = (random.Next(2, Xmap - 2), random.Next(2, Ymap - 2));
                foodList.Insert(i, newFood);
            }
        }
        private static void DrawMap(char[,] map)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    Console.Write(map[x, y]);
                }
                Console.Write("\n");
            }
        }
        //=========================================GET DIRECTION=================================================================

        private static int[] GetDirection(ConsoleKeyInfo pressedKey)
        {
            int[] newDirection = { currentDirection[0], currentDirection[1] };

            if (pressedKey.Key == ConsoleKey.UpArrow)
                newDirection = new int[] { 0, -1 };
            else if (pressedKey.Key == ConsoleKey.DownArrow)
                newDirection = new int[] { 0, 1 };
            else if (pressedKey.Key == ConsoleKey.LeftArrow)
                newDirection = new int[] { -1, 0 };
            else if (pressedKey.Key == ConsoleKey.RightArrow)
                newDirection = new int[] { 1, 0 };

            if (!(newDirection[0] == -currentDirection[0] && newDirection[1] == -currentDirection[1]))
            {
                currentDirection = newDirection;
            }
            return currentDirection;
        }
        //===============================================DRAW INFO=============================================================

        private static void DrawInfo(ref int Xmap, ref int Ymap, ref int level, ref int speed, ref int score)
        {
            Console.SetCursorPosition(Xmap + 2, 15);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"=========== C O N T R O L S ===========");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(88, 18);
            Console.Write(" ---- ");
            Console.SetCursorPosition(88, 19);
            Console.Write("|    |");
            Console.SetCursorPosition(88, 20);
            Console.Write("| UP |");
            Console.SetCursorPosition(88, 21);
            Console.Write("|    |");
            Console.SetCursorPosition(88, 22);
            Console.Write(" ---- ");

            Console.SetCursorPosition(80, 23);
            Console.Write(" ----- ");
            Console.SetCursorPosition(80, 24);
            Console.Write("|     |");
            Console.SetCursorPosition(80, 25);
            Console.Write("| LEFT|");
            Console.SetCursorPosition(80, 26);
            Console.Write("|     |");
            Console.SetCursorPosition(80, 27);
            Console.Write(" ----- ");

            Console.SetCursorPosition(95, 23);
            Console.Write(" ----- ");
            Console.SetCursorPosition(95, 24);
            Console.Write("|     |");
            Console.SetCursorPosition(95, 25);
            Console.Write("|RIGHT|");
            Console.SetCursorPosition(95, 26);
            Console.Write("|     |");
            Console.SetCursorPosition(95, 27);
            Console.Write(" ----- ");

            Console.SetCursorPosition(88, 28);
            Console.Write(" ---- ");
            Console.SetCursorPosition(88, 29);
            Console.Write("|    |");
            Console.SetCursorPosition(88, 30);
            Console.Write("|DOWN|");
            Console.SetCursorPosition(88, 31);
            Console.Write("|    |");
            Console.SetCursorPosition(88, 32);
            Console.Write(" ---- ");
            Console.ResetColor();


            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(85, 34);
            Console.Write(" ----------- ");
            Console.SetCursorPosition(85, 35);
            Console.Write("| P A U S E |");
            Console.SetCursorPosition(85, 36);
            Console.Write(" ----------- ");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.SetCursorPosition((Xmap / 2) - 20, Ymap + 1);
            Console.Write($" Level: {level}  ||  Speed: {speed}  ||  Score: {score} ");
            Console.ResetColor();
            Console.SetCursorPosition((Xmap / 2) - 20, Ymap + 3);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"S = SPEED UP  &  SCORE х 2");
            Console.ResetColor();
            Console.SetCursorPosition((Xmap / 2) - 20, Ymap + 5);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"D");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"/");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"X = SUDDEN DEATH");
            Console.ResetColor();
        }
        //=================================================HANDLE INPUT===========================================================
        private static void HandleInput(ConsoleKeyInfo pressedKey, List<(int X, int Y)> snakeList, ref List<(int X, int Y)> foodList, char[,] map, ref bool gameOver, ref bool levelUp, ref int score, ref (int X, int Y) startPosition, ref int speed, ref int Xmap, ref int Ymap)
        {

            int[] direction = GetDirection(pressedKey);

            (int X, int Y) newHead = (snakeList[0].X + direction[0], snakeList[0].Y + direction[1]);

            char nextCell = map[snakeList[0].X, snakeList[0].Y];

            for (int i = 1; i < snakeList.Count; i++)
            {
                if (newHead == snakeList[i])
                {
                    snakeList.RemoveRange(0, snakeList.Count);
                    gameOver = true;
                    levelUp = false;
                    snakeList.Add(startPosition);
                }
            }
            snakeList.Insert(0, newHead);

            if (foodList.Any(seg => seg.X == newHead.X && seg.Y == newHead.Y))
            {
                score++;
                foodList.Remove(newHead);
                snakeList.Insert(0, newHead);
            }

            if (nextCell == ' ')
            {
                int lastSnake = snakeList.Count;
                Console.SetCursorPosition(snakeList[lastSnake - 1].X, snakeList[lastSnake - 1].Y);
                Console.Write(" ");
                snakeList.RemoveAt(snakeList.Count - 1);
            }
            else if (nextCell == '#')
            {
                snakeList.RemoveRange(0, snakeList.Count);
                gameOver = true;
                levelUp = false;
                snakeList.Add(startPosition);
            }
        }
        //===================================================COLLISION=========================================================

        private static void CheckSpecialObjectsCollision((int X, int Y)  head, List<(int X, int Y)> snakeList, ref int snakePreyX, ref int snakePreyY, ref int snakeEnemyX, ref int snakeEnemyY, ref bool gameOver, ref bool levelUp, ref int score, ref (int X, int Y) startPosition, ref bool preyLife, ref bool enemyLife, ref int speed, ref bool preyRip, ref bool enemyRip)
        {
            if (head == (snakePreyX, snakePreyY) && preyRip == false)
            {
                score = score * 2;
                speed = speed - (speed / 10);
                preyLife = false;
                preyRip = true;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.SetCursorPosition(snakePreyX - 1, snakePreyY - 1);
                Console.Write("+");
                Console.SetCursorPosition(snakePreyX + 1, snakePreyY + 1);
                Console.Write("+");
                Console.SetCursorPosition(snakePreyX - 1, snakePreyY + 1);
                Console.Write("+");
                Console.SetCursorPosition(snakePreyX - 1, snakePreyY + 1);
                Console.Write("+");
                Console.ResetColor();
            }
            if (head == (snakeEnemyX, snakeEnemyY) && enemyRip == false)
            {
                Console.ResetColor();
                enemyLife = false;
                snakeList.RemoveRange(0, snakeList.Count);
                gameOver = true;
                levelUp = false;
                snakeList.Add(startPosition);
            }

        }

        //===================================================PREY=========================================================

        private static void DrawPrey(Random random, ref List<(int X, int Y)> snakeList, char[,] map, ref int snakePreyX, ref int snakePreyY, ref int score, ref bool enemyLife)
        {
            int directionPreyX = random.Next(-1, 2);
            int directionPreyY = random.Next(-1, 2);

            int nextPreyPositionX = snakePreyX + directionPreyX;
            int nextPreyPositionY = snakePreyY + directionPreyY;

            (int X, int Y) nextXYPreyCell = (nextPreyPositionX, nextPreyPositionY);
            char nextPreyCell = map[nextPreyPositionX, nextPreyPositionY];

            bool preyOnSnake = false;

            for (int i = 0; i < snakeList.Count; i++)
            {
                if (nextXYPreyCell == snakeList[i])
                {
                    preyOnSnake = true; break;
                }
            }
            if (nextPreyCell == ' ' && preyOnSnake == false)
            {
                Console.SetCursorPosition(snakePreyX, snakePreyY);
                Console.Write(' ');
                snakePreyX = nextPreyPositionX;
                snakePreyY = nextPreyPositionY;
            }
        }
        //==================================================ENEMY==========================================================

        private static void DrawEnemy(Random random, ref List<(int X, int Y)> snakeList, ref List<(int X, int Y)> foodList, char[,] map, ref int snakeEnemyX, ref int snakeEnemyY, ref int score, ref bool enemyLife, ref int Xmap, ref int Ymap, ref bool colorFrame, ref int directionEnemyX, ref int directionEnemyY)
        {
            int nextEnemyPositionX;
            int nextEnemyPositionY;

            if (snakeList[0].X <= (snakeEnemyX + 5) && snakeList[0].Y <= (snakeEnemyY + 5) && snakeList[0].X >= (snakeEnemyX - 5) && snakeList[0].Y >= (snakeEnemyY - 5))
            {
                colorFrame = true;
                if (snakeList[0].X <= snakeEnemyX) { nextEnemyPositionX = snakeEnemyX - 1; }
                else { nextEnemyPositionX = snakeEnemyX + 1; }

                if (snakeList[0].Y <= snakeEnemyY) { nextEnemyPositionY = snakeEnemyY - 1; }
                else { nextEnemyPositionY = snakeEnemyY + 1; }
            }
            else
            {
                colorFrame = false;

                if (snakeEnemyX == 6 && snakeEnemyY == 6)
                {
                    directionEnemyX = 1;
                    directionEnemyY = 1;
                    nextEnemyPositionX = snakeEnemyX + directionEnemyX;
                    nextEnemyPositionY = snakeEnemyY + directionEnemyY;
                }
                else if (snakeEnemyX == Xmap - 7 && snakeEnemyY == Ymap - 7)
                {
                    directionEnemyX = -1;
                    directionEnemyY = -1;
                    nextEnemyPositionX = snakeEnemyX + directionEnemyX;
                    nextEnemyPositionY = snakeEnemyY + directionEnemyY;
                }
                else if (snakeEnemyX == 6 && snakeEnemyY == Ymap - 7)
                {
                    directionEnemyX = 1;
                    directionEnemyY = -1;
                    nextEnemyPositionX = snakeEnemyX + directionEnemyX;
                    nextEnemyPositionY = snakeEnemyY + directionEnemyY;
                }
                else if (snakeEnemyX == Xmap - 7 && snakeEnemyY == 6)
                {
                    directionEnemyX = -1;
                    directionEnemyY = 1;
                    nextEnemyPositionX = snakeEnemyX + directionEnemyX;
                    nextEnemyPositionY = snakeEnemyY + directionEnemyY;
                }
                else if (snakeEnemyX == 6 || snakeEnemyX == Xmap - 7)
                {
                    directionEnemyX = directionEnemyX * -1;
                    nextEnemyPositionX = snakeEnemyX + directionEnemyX;
                    nextEnemyPositionY = snakeEnemyY + directionEnemyY;
                }
                else if (snakeEnemyY == 6 || snakeEnemyY == Ymap - 7)
                {
                    directionEnemyY = directionEnemyY * -1;
                    nextEnemyPositionX = snakeEnemyX + directionEnemyX;
                    nextEnemyPositionY = snakeEnemyY + directionEnemyY;
                }
                else 
                {
                    nextEnemyPositionX = snakeEnemyX + directionEnemyX;
                    nextEnemyPositionY = snakeEnemyY + directionEnemyY;
                }
            }

            (int X, int Y) nextXYEnemyCell = (nextEnemyPositionX, nextEnemyPositionY);
            char nextEnemyCell = map[nextEnemyPositionX, nextEnemyPositionY];

            bool enemyOnSnake = false;

            for (int i = 1; i < snakeList.Count; i++)
            {
                if (nextXYEnemyCell == snakeList[i])
                {
                    enemyOnSnake = true;
                    directionEnemyX = directionEnemyX * -1;
                    directionEnemyY = directionEnemyY * -1;
                    nextEnemyPositionX = snakeEnemyX + directionEnemyX;
                    nextEnemyPositionY = snakeEnemyY + directionEnemyY;
                    break;
                }
            }

            if (foodList.Any(seg => seg.X == nextXYEnemyCell.X && seg.Y == nextXYEnemyCell.Y))
            {
                score--;
                foodList.Remove(nextXYEnemyCell);
            }

            if (nextXYEnemyCell == snakeList[0])
                {
                    enemyOnSnake = true;
                }
            if (nextEnemyCell == ' ' && enemyOnSnake == false && nextEnemyPositionX > 5 && nextEnemyPositionX < (Xmap - 6) && nextEnemyPositionY > 5 && nextEnemyPositionY < (Ymap - 6))
            {
                Console.SetCursorPosition(snakeEnemyX, snakeEnemyY);
                Console.Write(' ');

                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 5);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 4);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 3);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 2);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 1);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 0);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 1);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 2);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 3);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 4);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 5);
                Console.Write("          ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 4);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 3);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 2);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 1);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 0);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 1);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 2);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 3);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 4);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 5);
                Console.Write(" ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 5);
                Console.Write("          ");

                snakeEnemyX = nextEnemyPositionX;
                snakeEnemyY = nextEnemyPositionY;
            }
            if (colorFrame == true)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(snakeEnemyX, snakeEnemyY);
                Console.Write("X");

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 4);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 2);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 0);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 2);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 4);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 5);
                Console.Write(" + + + + +");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 4);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 2);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 0);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 2);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 4);
                Console.Write("+");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 5);
                Console.Write(" + + + + +");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.SetCursorPosition(snakeEnemyX, snakeEnemyY);
                Console.Write("D");
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 4);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 2);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY + 0);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 2);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX + 5, snakeEnemyY - 4);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 5);
                Console.Write(" . . . . . ");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 4);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 2);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY - 0);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 2);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 4);
                Console.Write(".");
                Console.SetCursorPosition(snakeEnemyX - 5, snakeEnemyY + 5);
                Console.Write(" . . . . . ");
            }
        }
        //===================================================================================================================

        class Player
        {
            public string Name;
            public Player(string name)
            {
            Name = name;
            }
        }
        //===================================================================================================================

        public class ScoreEntry
        {
            public string Name { get; set; }
            public int Score { get; set; }
            public int Level { get; set; }
        }
        //===================================================================================================================

        public static class LeaderboardJson
        {
            private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "leaderboard.json");
            private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { WriteIndented = true };

            public static void SaveScore(string name, int score, int level)
            {
                var all = LoadAll();
                all.Add(new ScoreEntry { Name = name, Score = score, Level = level });
                var sorted = all.OrderByDescending(s => s.Score).ThenBy(s => s.Level).ToList();
                File.WriteAllText(FilePath, JsonSerializer.Serialize(sorted, JsonOpts));
            }

            public static List<ScoreEntry> LoadAll()
            {
                if (!File.Exists(FilePath)) return new List<ScoreEntry>();
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
            }

            public static void ShowTop(int topN = 9)
            {
                var top = LoadAll().Take(topN).ToList();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"\n========== TOP-{topN} PLAYERS =========");
                Console.ResetColor();

                int rank = 1;
                foreach (var s in top)
                {
                    Console.WriteLine($"{rank,1}. {s.Name,-6}  Score: {s.Score,-6}  Level: {s.Level,1}");
                    rank++;
                }
            }
            public static void ShowTopInGame(int Xmap, int topN = 10)
            {
                var top = LoadAll().Take(topN).ToList();
                Console.SetCursorPosition(Xmap + 2, 2);
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"============ TOP-{topN} PLAYERS ===========");
                Console.ResetColor();

                int Xtop5 = 4;
                int rank = 1;
                foreach (var s in top)
                {
                    Console.SetCursorPosition(Xmap + 2, Xtop5);
                    Console.WriteLine($"{rank,2}. {s.Name,-10}  Score: {s.Score,-5}  Level: {s.Level,1}");
                    rank++;
                    Xtop5++;
                }
            }
        }
    }
}