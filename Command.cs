using System;
using System.Collections.Generic;
using System.Text;

namespace CHIPSQuickLog
{
    class Command
    {
        ConsoleKey Key;
        Action CmdAction;

        public Command(ConsoleKey key, Action action)
        {
            Key = key;
            CmdAction = action;
        }
    }
}
