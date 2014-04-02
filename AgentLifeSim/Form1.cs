using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace AgentLifeSim
{
    public partial class Form1 : Form
    {
        StreamWriter sw;                // File stream for statistics
        String resultsFilePath;         // Path for statistics file
        List<Agent> agents;             // List of individuals (agents)
        Cell[] cells;                   // Array of grid cells
        Random rand;                    // Random number generator stuff
        int worldsSize = 200;           // Width (= height) of grid world
        int globalID = 0;               // Global ID of an agent
        int year = 0;                   // Years of simulation counter (= 12 steps)
        int humans = 0;                 // Quantity of human agents
        int rabbits = 0;                // Quantity of rabbit agents
        int cabbages = 0;               // Quantity of cabbage agents
        int wolves = 0;                 // Quantity of wolf agents
        int trees = 0;                  // Quantity of tree agents
        int stopYear = 5000;            // When simulation lasts too long
        int representation = 3;         // Type of visual representation:
                                        // 1 - Icons, 2 - Symbols, 3 - No representation
        bool writeStats = false;        // When do not need to write stats, set to false

        // ------------------------------------------------------------------
        // Form constructor
        //
        public Form1()
        {
            InitializeComponent();
            FirstInit();
        }

        // ------------------------------------------------------------------
        // First initialization of the world
        //
        private void FirstInit()
        {
            // Choose the last set of settings
            StreamReader sr = new StreamReader("defaultSet.ini");
            String settingsFile = sr.ReadLine();
            sr.Close();

            // Read settings from file
            StreamReader settings = new StreamReader(settingsFile);
            worldsSize = Int32.Parse(settings.ReadLine());
            humans = Int32.Parse(settings.ReadLine());
            wolves = Int32.Parse(settings.ReadLine());
            rabbits = Int32.Parse(settings.ReadLine());
            cabbages = Int32.Parse(settings.ReadLine());
            trees = Int32.Parse(settings.ReadLine());
            timer1.Interval = Int32.Parse(settings.ReadLine());
            stopYear = Int32.Parse(settings.ReadLine());
            representation = Int32.Parse(settings.ReadLine());
            settings.Close();
            // Set gui elements according to read settings
            if (representation == 1) radioButton1.Checked = true;
            if (representation == 2) radioButton2.Checked = true;
            if (representation == 3) radioButton3.Checked = true;            
            numericUpDown1.Value = worldsSize;
            numericUpDown2.Value = humans;
            numericUpDown3.Value = wolves;
            numericUpDown4.Value = rabbits;
            numericUpDown5.Value = cabbages;
            numericUpDown6.Value = trees;

            // Init basic variables
            cells = new Cell[worldsSize * worldsSize];
            rand = new Random();
            for (int i = 0; i != worldsSize * worldsSize; i++)
            {
                cells[i] = new Cell();
            }

            startButton.Text = "Start";
            toolStripStatusLabel1.Text = "Ready";
            // Generate datagrid
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;
            
            for (int i = 0; i != worldsSize; i++)
            {
                if (representation == 1 || representation == 3) dataGridView1.Columns.Add(new DataGridViewImageColumn());
                if (representation == 2) dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
                dataGridView1.Rows.Add(new DataGridViewRow());
            }
            // Representation influences on DataGridColumn type
            if (representation == 1 || representation == 3)
            {
                foreach (DataGridViewImageColumn col in dataGridView1.Columns)
                {
                    col.Width = 20;
                    col.Image = new Bitmap(AgentLifeSim.Properties.Resources.empty);
                }
            }
            if (representation == 2)
            {
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    col.Width = 20;                    
                }
            }
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Height = 20;
            }
            // Finish initialization
            agents = new List<Agent>();
            year = 0;
        }

        // ------------------------------------------------------------------
        // Initialization of the world (when reinitialization is needed)
        //
        private void InitSimulation()
        {
            // Restore initial state of gui elements
            label7.Text = worldsSize.ToString() + " x " + worldsSize.ToString();
            label7.Visible = true;
            label8.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
            label11.Visible = true;
            label12.Visible = true;
            numericUpDown1.Visible = false;
            numericUpDown2.Visible = false;
            numericUpDown3.Visible = false;
            numericUpDown4.Visible = false;
            numericUpDown5.Visible = false;
            numericUpDown6.Visible = false;
            // Restore basic variables
            worldsSize = (int)numericUpDown1.Value;
            humans = (int)numericUpDown2.Value;
            wolves = (int)numericUpDown3.Value;
            rabbits = (int)numericUpDown4.Value;
            cabbages = (int)numericUpDown5.Value;
            trees = (int)numericUpDown6.Value;

            cells = new Cell[worldsSize * worldsSize];
            for (int i = 0; i != worldsSize * worldsSize; i++)
            {
                cells[i] = new Cell();
            }

            startButton.Text = "Start";

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;

            for (int i = 0; i != worldsSize; i++)
            {
                if (representation == 1 || representation == 3) dataGridView1.Columns.Add(new DataGridViewImageColumn());
                if (representation == 2) dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
                dataGridView1.Rows.Add(new DataGridViewRow());
            }

            if (representation == 1 || representation == 3)
            {
                foreach (DataGridViewImageColumn col in dataGridView1.Columns)
                {
                    col.Width = 20;
                    col.Image = new Bitmap(AgentLifeSim.Properties.Resources.empty);
                }
            }
            if (representation == 2)
            {
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    col.Width = 20;
                }
            }
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Height = 20;
            }

            agents = new List<Agent>();
            year = 0;
        }

        // ------------------------------------------------------------------
        // Population of the world by new individuals - generation of agents
        //
        private void LaunchSimulation()
        {
            // Visual elements stuff
            label7.Text = worldsSize.ToString() + " x " + worldsSize.ToString();
            label7.Visible = true;
            label8.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
            label11.Visible = true;
            label12.Visible = true;
            numericUpDown1.Visible = false;
            numericUpDown2.Visible = false;
            numericUpDown3.Visible = false;
            numericUpDown4.Visible = false;
            numericUpDown5.Visible = false;
            numericUpDown6.Visible = false;
            worldsSize = (int)numericUpDown1.Value;
            humans = (int)numericUpDown2.Value;
            wolves = (int)numericUpDown3.Value;
            rabbits = (int)numericUpDown4.Value;
            cabbages = (int)numericUpDown5.Value;
            trees = (int)numericUpDown6.Value;
            
            // Create a new statistics file with excel table
            resultsFilePath = "chartdata" + DateTime.Now.ToString("HHmmss") + ".xls";
            if (writeStats)
            {
                sw = File.CreateText(resultsFilePath);
                sw.Write("<table border=1>");
                sw.Write("<tr>");
                sw.Write("<th>");
                sw.Write("humans");
                sw.Write("</th>");
                sw.Write("<th>");
                sw.Write("wolves");
                sw.Write("</th>");
                sw.Write("<th>");
                sw.Write("rabbits");
                sw.Write("</th>");
                sw.Write("<th>");
                sw.Write("cabbages");
                sw.Write("</th>");
                sw.Write("<th>");
                sw.Write("trees");
                sw.Write("</th>");
                sw.Write("</tr>");
            }

            // Populate area
            for (int itrees = 0; itrees != trees; itrees++)
            {
                CreateAgentOfType(5, rand.Next(150 * 12));  // Create a tree with a random age
            }
            for (int ihumans = 0; ihumans != humans; ihumans++)
            {
                CreateAgentOfType(3, rand.Next(70 * 12));
            }
            for (int iwolves = 0; iwolves != wolves; iwolves++)
            {
                CreateAgentOfType(4, rand.Next(10 * 12));
            }
            for (int irabbits = 0; irabbits != rabbits; irabbits++)
            {
                CreateAgentOfType(2, rand.Next(8 * 12));
            } 
            for (int icabbage = 0; icabbage != cabbages; icabbage++)
            {
                CreateAgentOfType(1, 0);
            }                                                 

            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
            radioButton3.Enabled = false;
            startButton.Text = "Stop";
            timer1.Start();
        }

        // ------------------------------------------------------------------
        // Generation of agent of a certain type
        //
        private void CreateAgentOfType(int type, int age = 0, int px = -1, int py = -1)
        {
            int x = rand.Next(0, worldsSize);               // Generate random coordinates
            int y = rand.Next(0, worldsSize);
            if (px != -1) x = px;                           // Or use those which are requested by func caller
            if (py != -1) y = py;
            int id = globalID;                              // Save the id of agent
            Agent thisAgent = null;                         // Create an object

            if (type == 1)
            {                   
                Agent agent = cells[x + worldsSize * y].agent.Find(ag => ag.type == 5);
                if (agent != null) return;                  // If no tree in this location, create a cabbage
                thisAgent = new Cabbage(id, age, x, y);                
            }
            if (type == 2)
            {                                               // Don't need a tree check, this agent will move to another cell next move
                thisAgent = new Rabbit(id, age, x, y);
            }
            if (type == 3)
            {
                thisAgent = new Human(id, age, x, y);
            }
            if (type == 4)
            {
                thisAgent = new Wolf(id, age, x, y);
            }
            if (type == 5)
            {
                Agent agent = cells[x + worldsSize * y].agent.Find(ag => ag.type == 1 || ag.type == 5);
                if (agent != null) return;
                thisAgent = new Tree(id, age, x, y);
            }
            
            agents.Add(thisAgent);                          // Push to agents list
            cells[x + worldsSize * y].agent.Add(thisAgent); // Push to cell inhabitants list

            // Define how this agent will be represented in datagrid
            if (representation == 1) dataGridView1[x, y].Value = new Bitmap(thisAgent.image);
            if (representation == 2) dataGridView1[x, y].Value = thisAgent.symbol;            
            globalID++;                                     // Increment ID, get ready for creation of new agent
        }

        // ------------------------------------------------------------------
        // Simulation cycle function
        //
        private void RunSimulation()
        {
            ExecuteAgingStage();
            ExecuteMoveStage();
            ExecuteActionStage();
        }

        // ------------------------------------------------------------------
        // Aging of agents stage
        //
        private void ExecuteAgingStage()
        {
            for (int i = 0; i != agents.Count(); i++)
            {
                if (agents[i].Age())    // Returns if agent is old enough to die
                {
                    cells[agents[i].xLocation + worldsSize * agents[i].yLocation].agent.Remove(agents[i]);
                    agents.Remove(agents[i]);          
                }
            }
        }

        // ------------------------------------------------------------------
        // Move stage
        //
        private void ExecuteMoveStage()
        {
            for (int i = 0; i != agents.Count; i++)
            {
                MoveThisAgent(i);       // Just move each agent
            }
        }

        // ------------------------------------------------------------------
        // Agent move function
        //
        int x = 0;          // Need to use global variable for coordinates
        int y = 0;          // because I don't want to pass x,y as arguments

        private void MoveThisAgent(int i)
        {
            if (agents[i].type == 1 || agents[i].type == 5)
            {                
                return;     // Trees and cabbages don't move, so exit
            }

            // All agents have sight vision in 7x7 area (and they are in its center)
            List<Cell> sight = new List<Cell>();
            for (int t1 = ((agents[i].xLocation - 3 > 0) ? agents[i].xLocation - 3 : 0); t1 != ((agents[i].xLocation + 4 < worldsSize) ? agents[i].xLocation + 4 : worldsSize - 1); t1++)
                for (int t2 = ((agents[i].yLocation - 3 > 0) ? agents[i].yLocation - 3 : 0); t2 != ((agents[i].yLocation + 4 < worldsSize) ? agents[i].yLocation + 4 : worldsSize - 1); t2++)
                    sight.Add(cells[t1 + worldsSize * t2]);     // Set up the sight
                        
            switch (agents[i].Move(rand, sight))
            {               // Ugly transformation from 3x3 position number to x,y
                case 1:  x = -1; y = -1; break;
                case 2:  x =  0; y = -1; break;
                case 3:  x =  1; y = -1; break;
                case 4:  x = -1; y =  0; break;
                case 5:  x =  0; y =  0; break;
                case 6:  x =  1; y =  0; break;
                case 7:  x = -1; y =  1; break;
                case 8:  x =  0; y =  1; break;
                case 9:  x =  1; y =  1; break;
                default: x =  0; y =  0; break;
            }

            ValidateMoveOfAgent(i);         // Check if agent can actually move to the cell he wants
            
            // Change location: remove from old cell, add to new
            cells[agents[i].xLocation + worldsSize * agents[i].yLocation].agent.Remove(agents[i]);  
            if (representation == 1) dataGridView1[agents[i].xLocation, agents[i].yLocation].Value = new Bitmap(AgentLifeSim.Properties.Resources.empty);
            if (representation == 2) dataGridView1[agents[i].xLocation, agents[i].yLocation].Value = "";
            
            agents[i].xLocation += x;
            agents[i].yLocation += y;
            cells[agents[i].xLocation + worldsSize * agents[i].yLocation].agent.Add(agents[i]);
            if (representation == 1) dataGridView1[agents[i].xLocation, agents[i].yLocation].Value = new Bitmap(agents[i].image);
            if (representation == 2) dataGridView1[agents[i].xLocation, agents[i].yLocation].Value = agents[i].symbol;

        }

        // ------------------------------------------------------------------
        // Move validation fuction
        //
        private void ValidateMoveOfAgent(int i)
        {       // Check if agent doesn't want to move beyond to edge of the world
            if (agents[i].xLocation + x < 0 || agents[i].xLocation + x >= worldsSize)
            {   // If he does, force him to move to specific direction
                x = 0; y = rand.Next(-1,1);
            }
            if (agents[i].yLocation + y < 0 || agents[i].yLocation + y >= worldsSize)
            {
                x = rand.Next(-1, 1); y = 0;
                if (agents[i].xLocation + x < 0 || agents[i].xLocation + x >= worldsSize)
                {
                    x = 0 - x; y = 0;
                }
            }

            // Now check if no trees grow where individual wants to go
            foreach (Agent agent in cells[agents[i].xLocation + x + worldsSize * (agents[i].yLocation + y)].agent)
            {
                if (agent.type == 5) 
                {
                    if (y == 0)
                    {
                        while (y==0) y = rand.Next(-1, 1);
                        if (agents[i].xLocation + x >= 0 &&
                            agents[i].xLocation + x < worldsSize &&
                            agents[i].yLocation + y >= 0 &&
                            agents[i].yLocation + y < worldsSize)
                        {
                            ValidateMoveOfAgent(i);     // And check again if new location is available 
                            return;
                        }
                        else
                        {
                            y = 0 - y;
                            ValidateMoveOfAgent(i);
                            return;
                        }
                    }
                    if (x == 0)
                    {
                        while (x == 0) x = rand.Next(-1, 1);
                        if (agents[i].xLocation + x >= 0 &&
                            agents[i].xLocation + x < worldsSize &&
                            agents[i].yLocation + y >= 0 &&
                            agents[i].yLocation + y < worldsSize)
                        {
                            ValidateMoveOfAgent(i);
                            return;
                        }
                        else
                        {
                            x = 0 - x;
                            ValidateMoveOfAgent(i);
                            return;
                        }
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Action stage fuction
        //
        private void ExecuteActionStage()
        {
            List<Agent> toKill = new List<Agent>();     // List of individuals doomed to die
            List<Agent> toBore = new List<Agent>();     // List of individuals lucky to be born

            for (int i = 0; i != cells.Count(); i++)
            {
                if (cells[i].agent.Count > 1)           // If there two or more agents in the cell
                {                                       // see how they interact
                    cells[i].situation.Clear();
                    toKill.Clear();
                    foreach (Agent j in cells[i].agent)
                    {
                        foreach (Agent k in cells[i].agent)
                        {
                            if (j.id != k.id)
                            {
                                if (j.Eat(k.type))
                                {
                                    toKill.Add(k);      // Add k agent to the death list
                                    cells[i].situation.Add(j.type*10+k.type); // Situation: j eats k
                                }                                
                            }
                        }
                    }

                    // Kill all agents in the death list
                    foreach (Agent k in toKill)
                    {                        
                        agents.Remove(k);                        
                        cells[i].agent.Remove(k);
                    }

                    toBore.Clear();
                    foreach (Agent j in cells[i].agent)
                    {
                        foreach (Agent k in cells[i].agent)
                        {
                            if (j != k && j.type != 1 && j.type != 5 &&     // Cabbages and trees breed otherway
                                j.breedable == true && k.breedable == true)
                            {
                                if (j.Breed(k.type))
                                {
                                    toBore.Add(k);  // Mark k as mother 
                                    j.breedable = false;
                                    k.breedable = false;
                                }
                            }                            
                        }
                        if (j.type == 1 || j.type == 5)
                        {
                            if (j.breedable == true)
                            {
                                if (j.Breed(j.type))
                                {
                                    toBore.Add(j);
                                }
                            }
                        }
                    }

                    foreach (Agent k in toBore)
                    {
                        CreateAgentOfType(k.type, 0, k.xLocation, k.yLocation);
                        
                        // Cabbage has several children
                        if (k.type == 1)
                        {
                            for (int g = 0; g != 4; g++)
                            {
                                int newX = k.xLocation + rand.Next(-1, 1);
                                int newY = k.yLocation + rand.Next(-1, 1);
                                CreateAgentOfType(k.type, 0, newX, newY);                                
                            }
                        }
                    }
                }
                else if (cells[i].agent.Count == 1) // For separate standing trees and cabbages
                {
                    toBore.Clear();
                    foreach (Agent j in cells[i].agent)
                    {
                        if (j.type == 1 || j.type == 5)
                        {
                            if (j.breedable == true)
                            {
                                if (j.Breed(j.type))
                                {
                                    toBore.Add(j);
                                }
                            }                            
                        }
                    }
                    foreach (Agent k in toBore)
                    {                        
                        if (k.type == 1)
                        {
                            for (int g = 0; g != 4; g++)
                            {                                
                                int newX = k.xLocation + rand.Next(-1, 1);
                                int newY = k.yLocation + rand.Next(-1, 1);
                                CreateAgentOfType(k.type, 0, newX, newY);
                            }
                        }
                    }
                }
            }
            // Learning
            for (int i = 0; i != agents.Count(); i++)
            {
                if (agents[i].type != 1 && agents[i].type != 5)
                {
                    if (agents[i].runSkill == false)
                    {
                        List<Cell> sight = new List<Cell>();
                        for (int t1 = ((agents[i].xLocation - 3 > 0) ? agents[i].xLocation - 3 : 0); t1 != ((agents[i].xLocation + 4 < worldsSize) ? agents[i].xLocation + 4 : worldsSize - 1); t1++)
                            for (int t2 = ((agents[i].yLocation - 3 > 0) ? agents[i].yLocation - 3 : 0); t2 != ((agents[i].yLocation + 4 < worldsSize) ? agents[i].yLocation + 4 : worldsSize - 1); t2++)
                                sight.Add(cells[t1 + worldsSize * t2]);
                        foreach (Cell s in sight)
                        {       // situation j.type*10 + k.type = j eats k
                            if (s.situation.Exists(b => b % 10 == agents[i].type))
                            {
                                agents[i].runSkill = true;
                            }
                        }
                    }
                    if (agents[i].huntSkill == false)
                    {
                        List<Cell> sight = new List<Cell>();
                        for (int t1 = ((agents[i].xLocation - 3 > 0) ? agents[i].xLocation - 3 : 0); t1 != ((agents[i].xLocation + 4 < worldsSize) ? agents[i].xLocation + 4 : worldsSize - 1); t1++)
                            for (int t2 = ((agents[i].yLocation - 3 > 0) ? agents[i].yLocation - 3 : 0); t2 != ((agents[i].yLocation + 4 < worldsSize) ? agents[i].yLocation + 4 : worldsSize - 1); t2++)
                                sight.Add(cells[t1 + worldsSize * t2]);
                        if (sight.Exists(a => a.situation.Exists(b => b / 10 == agents[i].type)))
                        {
                            agents[i].huntSkill = true;
                        }
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Simulation stop function
        //
        private void StopSimulation()
        {
            startButton.Text = "Start";
            timer1.Stop();
            InitSimulation();
            label7.Visible = false;
            label8.Visible = false;
            label9.Visible = false;
            label10.Visible = false;
            label11.Visible = false;
            label12.Visible = false;
            numericUpDown1.Visible = true;
            numericUpDown2.Visible = true;
            numericUpDown3.Visible = true;
            numericUpDown4.Visible = true;
            numericUpDown5.Visible = true;
            numericUpDown6.Visible = true;
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            radioButton3.Enabled = true;
        }

        // ------------------------------------------------------------------
        // One step of simulation
        //
        private void timer1_Tick(object sender, EventArgs e)
        {
            RunSimulation();

            // Force stop at 30000 agents quantity
            if (agents.Count > 30000)       
            {
                StopSimulation();
                String message = "Low memory, simulation stopped";
                if (writeStats)
                {
                    message += ". Results in " + resultsFilePath;
                    sw.Close();
                }
                MessageBox.Show(message);
                return;
            }

            // Gather statistics
            humans = 0;
            rabbits = 0;
            cabbages = 0;
            wolves = 0;
            trees = 0;
            for (int i = 0; i != agents.Count; i++)
            {
                if (agents[i].type == 1)
                {
                    ++cabbages;
                }
                else if (agents[i].type == 2)
                {
                    ++rabbits;
                }
                else if (agents[i].type == 3)
                {
                    ++humans;
                }
                else if (agents[i].type == 4)
                {
                    ++wolves;
                }
                else if (agents[i].type == 5)
                {
                    ++trees;
                }
            }

            ++year;
            if (year == stopYear)
            {
                String message = "Simulation finished";
                if (writeStats)
                {
                    message += ". You can find results in " + resultsFilePath;
                    sw.Write("</table>");
                }
                StopSimulation();
                MessageBox.Show(message);
                sw.Close();
                return;
            }
            toolStripStatusLabel1.Text = "Year: " + year / 12 + "; Humans: " + humans + "; Wolves: " + wolves + "; Rabbits: " + rabbits +
                                "; Cabbages: " + cabbages + "; Trees: " + trees;
            toolStripStatusLabel1.Visible = true;
            label8.Text = humans.ToString();
            label9.Text = wolves.ToString();
            label10.Text = rabbits.ToString();
            label11.Text = cabbages.ToString();
            label12.Text = trees.ToString();

            if (writeStats)
            {
                sw.Write("<tr>");
                sw.Write("<th>");
                sw.Write(humans);
                sw.Write("</th>");
                sw.Write("<th>");
                sw.Write(wolves);
                sw.Write("</th>");
                sw.Write("<th>");
                sw.Write(rabbits);
                sw.Write("</th>");
                sw.Write("<th>");
                sw.Write(cabbages);
                sw.Write("</th>");
                sw.Write("<th>");
                sw.Write(trees);
                sw.Write("</th>");
                sw.Write("</tr>");
            }
        }

        // ------------------------------------------------------------------
        // Interface part
        //
        private void startButton_Click(object sender, EventArgs e)
        { 
            if (startButton.Text == "Start")
            {
                InitSimulation(); 
                LaunchSimulation();
            }
            else
            {
                StopSimulation();
                if (writeStats)
                {
                    sw.Close();
                }
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            if (sw!=null) sw.Close();
            this.Close();
            this.Dispose();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            panel1.Width = this.Width - panel2.Width - 20;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }

        private void quitProgrammToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            if (sw != null) sw.Close();
            this.Close();
            this.Dispose();
        }

        private void loadSettingsFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Settings files (*.ini) |*.ini";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter defFile = new StreamWriter("defaultSet.ini");
                defFile.Write(ofd.FileName);
                defFile.Close();
                FirstInit();
            }
        }

        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Settings files (*.ini) |*.ini";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter saveFile = new StreamWriter(sfd.FileName);
                saveFile.WriteLine(numericUpDown1.Value.ToString());
                saveFile.WriteLine(numericUpDown2.Value.ToString());
                saveFile.WriteLine(numericUpDown3.Value.ToString());
                saveFile.WriteLine(numericUpDown4.Value.ToString());
                saveFile.WriteLine(numericUpDown5.Value.ToString());
                saveFile.WriteLine(numericUpDown6.Value.ToString());
                saveFile.WriteLine(timer1.Interval.ToString());
                saveFile.WriteLine(stopYear.ToString());
                if (radioButton1.Checked) saveFile.WriteLine("1");
                if (radioButton2.Checked) saveFile.WriteLine("2");
                if (radioButton3.Checked) saveFile.WriteLine("3");
                saveFile.Close();
                StreamWriter defFile = new StreamWriter("defaultSet.ini");
                defFile.Write(sfd.FileName);
                defFile.Close();
            }

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            representation = 2;            
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            representation = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            representation = 3;
        }
    }
}

