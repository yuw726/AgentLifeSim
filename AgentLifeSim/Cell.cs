using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgentLifeSim
{
    class Cell
    {
        public List<Agent> agent;           // List of agents, that dwell in this cell
        public List<int> situation;         // List of events in this cell

        public Cell()
        {
            agent = new List<Agent>();
            situation = new List<int>();
        }
    }
}
