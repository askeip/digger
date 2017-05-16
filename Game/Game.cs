using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Digger
{
    public class Game
    {
        public readonly ICreature[,] Map;
        public int MapWidth { get;private set; }
        public int MapHeight { get;private set; }
        public int Scores { get; set; }
        public bool IsOver { get; set; }
        public string LastWords { get; set; }
        public int DigX { get; set; }
        public int DigY { get; set; }
        //public TexturePoints[] monstersGoTo { get { return monstersGoTo; } set { monstersGoTo = new TexturePoints[monsters.Count]; } }
        //public int monsterNum { get { return monsterNum; } set { monsterNum = 0; } }

        public string Key { get;private set; }
        public Game(string[] map) 
        {
            //TODO: Инициализируйте здесь 
            //map = File.ReadAllLines("Maps//Map" + Program.lvl.ToString() + ".txt");
            MapWidth = map[0].Length-1;
            MapHeight = map.Length-1;
            var rnd = new Random();
            IsOver = false;
            Map = new ICreature[MapWidth, MapHeight];
            for (int i = 0; i < MapHeight; i++)
                for (int j = 0; j < MapWidth; j++)
                {
                    switch (map[i][j])
                    {
                        case '#': {
                            Map[j,i] = new Terrain();
                            break;
                        }
                        case '$': {
                            Map[j, i] = new Sack(this);
                            break;}
                        case '@': {
                            Map[j, i] = new Monster(this);
                            break;}
                        case '%': {
                            Map[j, i] = new Digger(this);
                            DigX = j;
                            DigY = i;
                            break;}
                    }
                }
            Scores = 0;
        }
        public void KeyPressed(object sender,KeyEventArgs e)
        {
            Key = e.KeyCode.ToString();
            if (Key=="R")
            {
                Application.Restart();
            }
            if (Key == "Q")
                Application.Exit();
        }
        public void KeyUp(object sender,KeyEventArgs e)
        {
            Key = "Space";
        }
    }
    public class TexturePoints
    {
        public int X;
        public int Y;
    }
    public class Terrain : ICreature
    {
        //public string Image;
        public string GetImageFileName { get { return "Terrain.png"; } }

        public int GetDrawingPriority {get { return 2; }}

        public CreatureCommand Act(int x, int y)
        {
            var com = new CreatureCommand { DeltaX = 0, DeltaY = 0 };
            return com;
            //throw new NotImplementedException();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return true;
            //throw new NotImplementedException();
        }
    }
 
    public class Digger : ICreature
    {
        readonly Game game;
        public Digger(Game game)
        {
            this.game = game;
        }
        public string GetImageFileName { get { return "Digger.png"; } }

        public int GetDrawingPriority { get { return 1; } }

        public CreatureCommand Act(int x, int y)
        {
            if (game.Scores>=1500)
            {
                game.IsOver = true;
                game.LastWords = "ПРАВЛЕНИЕ ПУТИНА ОСТАНОВЛЕНО,ВЫ ВЫИГРАЛИ!!!!";
                //Program.lvl++;
                //DiggerWindow.game = new Game(File.ReadAllLines("Maps//Map" + Program.lvl.ToString() + ".txt"));
            }
            var com = new CreatureCommand { DeltaX = 0, DeltaY = 0 };
            if (game.Key == "A" && (game.Map[x - 1, y] == null || game.Map[x - 1, y] is Monster)) game.Map[x - 1, y] = new Shot(game);
            if (game.Key == "D" && (game.Map[x + 1, y] == null || game.Map[x + 1, y] is Monster)) game.Map[x + 1, y] = new Shot(game);
            //if (DiggerWindow.game.Key == "D") Move = 1;
            if (game.Key == "Right" && game.DigX < game.MapWidth - 1) com = Checking(game.DigX + 1, game.DigY, com);
            if (game.Key == "Left" && game.DigX > 0) com = Checking(game.DigX - 1, game.DigY, com);
            if (game.Key == "Up" && game.DigY > 0) com = Checking(game.DigX, game.DigY - 1, com);
            if (game.Key == "Down" && game.DigY < game.MapHeight - 1) com = Checking(game.DigX, game.DigY + 1, com);
            game.DigX += com.DeltaX;
            game.DigY += com.DeltaY;
            return com;
        }
        public CreatureCommand Checking (int x,int y,CreatureCommand com)
        {
            if (game.Map[x, y] != null)
            { if (game.Map[x, y] is Sack || game.Map[x, y] is Monster) return com; }
            return com = new CreatureCommand { DeltaX = x - game.DigX, DeltaY = y - game.DigY };
        }
        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Terrain) return false;
            if (conflictedObject is Digger) return true;
            if (conflictedObject is Monster)
            {
                game.IsOver = true;
                game.LastWords = "GAME IS OVER PUTIN WINS!!!";
                return true;
            }
            if (conflictedObject is Sack)
            {
                game.IsOver = true;
                game.LastWords = "ВАС РАЗДАВИЛО ПОД ДАВЛЕНИЕМ ТОЛПЫ НА МИТИНГЕ!!!";
                return true;
            }
            else if (conflictedObject is Sack) return false;
            return false;
        }
    }
    public class Sack : ICreature
    {
        readonly Game game;
        public Sack(Game game)
        {
            this.game = game;
        }
        public bool Movement=false;
        public string GetImageFileName { get { return "Sack.png"; } }

        public int GetDrawingPriority { get { return 0; } }

        public CreatureCommand Act(int x, int y)
        {
            var com = new CreatureCommand { DeltaX = 0, DeltaY = 0 };
            if (y != game.MapHeight - 1 && game.Map[x, y + 1] != null)
            {
                if ((game.Map[x, y + 1] is Terrain || game.Map[x, y + 1] is Sack) && Movement)
                    return new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = new Gold(game) };
                else if (game.Map[x, y + 1] is Sack) return com;
                else if (game.Map[x, y + 1] is Digger)
                {
                    com.DeltaY = 1;
                    Movement = true;
                }
                if (game.Map[x, y + 1] is Terrain) return com;
                //return com;
            }
            else
            {
                if (y != game.MapHeight - 1)
                {
                    Movement = true;
                    return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
                }
                else if (!Movement) return com;
                else return new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = new Gold(game) };
            }
             return new CreatureCommand { DeltaX = 0, DeltaY = 1 }; 
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            //if (conflictedObject.GetImageFileName() == "Sack.png") return true;
            //if (conflictedObject.GetImageFileName() == "Digger.png") Act(Game.DigX, Game.DigY);
            return false;
            //throw new NotImplementedException();
        }
        //public CreatureCommand Move (x,y);
        //{

        //}
    }
    public class Gold : ICreature
    {
        readonly Game game;
        public Gold(Game game)
        {
            this.game = game;
        }
        public string GetImageFileName { get { return "Gold.png"; } }

        public int GetDrawingPriority { get { return 2; } }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
            //throw new NotImplementedException();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Digger) 
            {
                game.Scores += 250;
                //return true;
            }
            return true;
            //throw new NotImplementedException();
        }
    }
    public class Shot : ICreature
    {
        readonly Game game;
        public Shot(Game game)
        {
            this.game = game;
        }
        int move = 0;
        bool changed = false;
        public int Move
        {
            get
            {
                if (!changed)
                {
                    if (game.Key == "A")
                    {
                        changed = true;
                        move= -1;
                    }
                    else if (game.Key == "D")
                    {
                        changed = true;
                        move= 1;
                    }
                }
                return move;
            }
        }
        public string GetImageFileName
        {
            get { return "shot.png"; }
        }

        public int GetDrawingPriority
        {
            get { return 0; }
        }

        public CreatureCommand Act(int x, int y)
        {
            if (x > 0 && x < game.MapWidth - 1 && (game.Map[x + Move, y] == null || game.Map[x + Move, y] is Monster || game.Map[x + Move, y] is Shot))
                return new CreatureCommand { DeltaX = Move };
            else return new CreatureCommand { TransformTo = new Monster(game) };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }
    }

    public class Monster : ICreature
    {
        readonly Game game;
        public Monster(Game game)
        {
            this.game = game;
        }
        public string GetImageFileName { get { return "Monster.png"; } }

        public int GetDrawingPriority { get { return 0; } }

        public CreatureCommand Act(int x, int y)
        {
            var com = new CreatureCommand { DeltaX = 0, DeltaY = 0 };
            if (x < game.DigX && x < game.MapWidth - 1) com = Checking(x, y, x + 1, y, com);
            if (x > game.DigX && x > 0) com = Checking(x, y, x - 1, y, com);
            if (y < game.DigY && y < game.MapHeight - 1 && com.DeltaX == 0) com = Checking(x, y, x, y + 1, com);
            if (y > game.DigY && y > 0 && com.DeltaX == 0) com = Checking(x, y, x, y - 1, com);
                return com;
        }
        public CreatureCommand Checking(int xOld,int yOld,int x, int y, CreatureCommand com)
        {
            if (game.Map[x, y] != null)
            {
                if (game.Map[x, y] is Sack || game.Map[x, y] is Terrain
                 || game.Map[x, y] is Monster || game.Map[x, y] is Shot) return com;
            }
            return com = new CreatureCommand { DeltaX = x - xOld, DeltaY = y - yOld };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Sack)
            {
                game.Scores += 500;
                return true;
            }
            if (conflictedObject is Shot) return true;
            return false;
            //throw new NotImplementedException();
        }

    }
    ////public class AStar
    //{
    //    int[,] Map;
    //    int MapWidth;
    //    int MapHeight;
    //     const int Wall = -2;
    //    const int Blank = 1;
    //    int LastX;
    //    int LastY;


    //    //int[,] WayMap;
    //    /// <summary>
    //    /// Конструктор
    //    /// </summary>
    //    public void ReadMap()
    //    {
    //        MapWidth = Game.MapWidth;
    //        MapHeight = Game.MapHeight;
    //        Map = new int[MapWidth, MapHeight];
    //        for (int i = 0; i < MapWidth - 1;i++ )
    //            for (int j = 0; j < MapHeight- 1; j++)
    //            {
    //                if (Game.Map[i, j] == null||Game.Map[i, j] is Gold ||
    //                        Game.Map[i, j] is Digger || Game.Map[i, j] is Monster) Map[i, j] = Blank;
    //                else Map[i, j] = Wall;
    //            }
    //            //WayMap = new int[10, 10];
    //    }
    //    /// <summary>
    //    /// Отображение карты
    //    /// </summary>
    //    //public void DrawMap()
    //    //{
    //    //    for (int y = 0; y < MapHeight; y++)
    //    //    {
    //    //        Console.WriteLine();
    //    //        for (int x = 0; x < MapWidth; x++)
    //    //            if (Map[y, x] == 1)
    //    //                Console.Write("+");
    //    //            else
    //    //                Console.Write(" ");
    //    //    }
    //    //    Console.ReadKey();
    //    //    FindWave(1, 1, 3, 4);
    //    //}
    //    /// <summary>
    //    /// Поиск пути
    //    /// </summary>
    //    /// <param name="startX">Координата старта X</param>
    //    /// <param name="startY">Координата старта Y</param>
    //    /// <param name="targetX">Координата финиша X</param>
    //    /// <param name="targetY">Координата финиша Y</param>
    //    public int[] FindWave(int startX, int startY, int targetX, int targetY)
    //    {
    //        ReadMap();
    //        bool add = true;
    //        int[,] cMap = new int[MapWidth, MapHeight];
    //        int x, y, step = 0;
    //        for (x = 0; x < MapWidth-1; x++)
    //            for (y = 0; y < MapHeight-1; y++)
    //            {
    //                if (Map[x,y] == Wall)
    //                    cMap[x, y] = Wall;//индикатор стены
    //                else
    //                    cMap[x, y] = -Blank;//индикатор еще не ступали сюда
    //            }
    //        cMap[targetX, targetY] = 0;//Начинаем с финиша
    //        while (add == true)
    //        {
    //            add = false;
    //            for (x = 1; x < MapWidth - 2; x++)
    //                for (y = 1; y < MapHeight -2 ; y++)
    //                {
    //                    if (cMap[x, y] == step)
    //                    {
    //                        //Ставим значение шага+1 в соседние ячейки (если они проходимы)
    //                        if (y - 1 >= 0 && cMap[x - 1, y] != -2 && cMap[x - 1, y] == -1)
    //                            cMap[x - 1, y] = step + 1;
    //                        if (x - 1 >= 0 && cMap[x, y - 1] != -2 && cMap[x, y - 1] == -1)
    //                            cMap[x, y - 1] = step + 1;
    //                        if (y + 1 < MapWidth && cMap[x + 1, y] != -2 && cMap[x + 1, y] == -1)
    //                            cMap[x + 1, y] = step + 1;
    //                        if (x + 1 < MapHeight && cMap[x, y + 1] != -2 && cMap[x, y + 1] == -1)
    //                            cMap[x, y + 1] = step + 1;
    //                        LastX = x;
    //                        LastY = y;
    //                    }
    //                }
    //            step++;
    //            add = true;
    //            if (cMap[startX, startY] != -1 && step < MapWidth * MapHeight)//решение найдено
    //                add = false;//return new int[] {LastX,LastY};
    //            else
    //            {
    //                LastX = targetX;
    //                LastY = targetY;
    //            }
    //           // if (step > MapWidth * MapHeight)//решение не найдено
    //               // return 
    //        }
    //        return new int[] { LastX, LastY };
    //        //Отрисовываем карты
    //        //for (y = 0; y < MapHeight; y++)
    //        //{
    //        //    Console.WriteLine();
    //        //    for (x = 0; x < MapWidth; x++)
    //        //        if (cMap[y, x] == -1)
    //        //            Console.Write(" ");
    //        //        else
    //        //            if (cMap[y, x] == -2)
    //        //                Console.Write("#");
    //        //            else
    //        //                if (y == startY && x == startX)
    //        //                    Console.Write("S");
    //        //                else
    //        //                    if (y == targetY && x == targetX)
    //        //                        Console.Write("F");
    //        //                    else
    //        //                        if (cMap[y, x] > -1)
    //        //                            Console.Write("{0}", cMap[y, x]);

    //        //}
    //        //Console.ReadKey();
    //    }
    //}
}
