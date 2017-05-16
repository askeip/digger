using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Digger
{
    static class Program
    {
        public static int lvl { get; set; }
        [STAThread]
        static void Main()
        {
            lvl = 2;
            //new Game();
            //var form = new DiggerWindow();
            //form.KeyDown+=Game.KeyPressed            
            //Game.KeyPressed(Game.CreateMap(),form.KeyDown+=Game.KeyPressed);
            Application.Run(new DiggerWindow());
        }
    }
}
