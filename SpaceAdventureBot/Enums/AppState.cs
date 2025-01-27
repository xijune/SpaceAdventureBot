using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceAdventureBot.Enums
{
    public enum ApplicationState
    {
        Started,         // Application is running
        FailedToStart,   // Application failed to start
        Crashed,         // Application crashed after launching
        Terminated,      // Application was running but is now closed
        Unknown          // Unknown state
    }
}
