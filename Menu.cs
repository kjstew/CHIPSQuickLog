using System;
using System.Collections.Generic;
using System.Text;

namespace CHIPSQuickLog
{
    class Menu
    {
        List<Command> CmdList;
        Func<string> MenuDisplayer;

        public Menu(List<Command> cmdList, Func<string> menuDisplayer)
        {
            CmdList = cmdList;
            MenuDisplayer = menuDisplayer;
        }
    }
}
