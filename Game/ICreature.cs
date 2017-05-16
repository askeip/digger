using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digger
{
    public interface ICreature
    {
        string GetImageFileName{get;}
        int GetDrawingPriority{get;}
        CreatureCommand Act(int x, int y);
        bool DeadInConflict(ICreature conflictedObject);
    }
}
