using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace AgentLifeSim
{
    abstract class Agent
    {
        public int    id;                   // Unique ID of individual
        public int    type;                 // Agent type: human, wolf, rabbit, tree, cabbage
        public int    xLocation;            // Coordinates of current location
        public int    yLocation;                
        public int    age;                  // Age of agent
        public int    maxAge;               // Age limit, causes agents "death" when exceeded
        public int    breedColldown;        // Period when agent can't breed 
                                            // (breed seasons and periods of maturing)
        public int    hunger;               // Expresses a hunger property of living creature
        public bool   breedable = false;    // Another variable to define when agent can breed. TODO: check, can we use the only variable
        public bool   huntSkill = false;    // Defines if agent knows how to hunt
        public bool   runSkill  = false;    // Defines if agent knows how to run from enemies
        public Image  image;                // Image representation, depends on agents type
        public String symbol;               // Symbol representation, depends on agents type

        public Agent() { }                  // Abstract constructor

        // Methods, describing activity of agents (overrided in each specific class)
        public abstract int  Move(Random rand, List<Cell> sight);    
        public abstract bool Breed(int type);                       
        public abstract bool Eat(int type);
        public abstract bool Age();
    }

    // ------------------------------------------------------------------
    //
    class Cabbage : Agent
    {
        public Cabbage(int pID, int pAge = 0, int pxLocation = 0, int pyLocation = 0)
        {
            this.id             = pID;
            this.type           = 1;
            this.age            = pAge;
            this.xLocation      = pxLocation;
            this.yLocation      = pyLocation;
            this.image          = AgentLifeSim.Properties.Resources.cabbage1;
            this.maxAge         = 2 * 12;
            this.breedColldown  = 12;
            this.breedable      = false;
            this.symbol         = "C";
        }

        public override int Move(Random rand, List<Cell> sight)
        {
            return 0;       // Cabbage can't move
        }

        public override bool Breed(int type)
        {
            if (type == this.type)
            {
                this.breedColldown = 12;
                this.breedable = false;
                return true;
            }

            return false;
        }

        public override bool Eat(int type)
        {
            return false;   // Cabbage doesn't eat
        }

        public override bool Age()
        {
            ++this.age;
            --this.breedColldown;
            if (this.age == this.maxAge)
            {
                return true;
            }
            if (this.breedColldown <= 0)
            {
                this.breedable = true;
            }
            return false;
        }
    }

    // ------------------------------------------------------------------
    //
    class Human : Agent
    {
        public Human(int pID, int pAge = 0, int pxLocation = 0, int pyLocation = 0)
        {
            this.id            = pID;
            this.type          = 3;
            this.age           = pAge;        
            this.xLocation     = pxLocation;
            this.yLocation     = pyLocation;
            this.image         = AgentLifeSim.Properties.Resources.human1;
            this.maxAge        = 70 * 12;
            this.breedColldown = 15 * 12 - this.age;
            this.breedable     = false;
            this.symbol        = "H";
            this.hunger        = 24;
        }

        public override int Move(Random rand, List<Cell> sight) 
        {
            int direction = 5;              // Default: no direction 
            if (runSkill == true)           // First instinct - to run from predators
            {
                double distance = 1000;     // Any big number
                int targetX = this.xLocation;
                int targetY = this.yLocation;

                // Look for wolves (agent type 4) in sight 
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type == 4) != null))
                {
                    Agent enemy = cell.agent.Find(ag => ag.type == 4);
                    int a = enemy.xLocation - this.xLocation;
                    int b = enemy.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }

                // Run the opposite side
                if (targetX > this.xLocation)
                {
                    --direction;
                }
                else if (targetX < this.xLocation)
                {
                    ++direction;
                }
                if (targetY > this.yLocation)
                {
                    direction -= 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction += 3;
                }

                // Accept any direction except idle
                if (direction != 5)
                {
                    return direction;
                }
                else
                {
                    return rand.Next(1, 10);
                }
            }

            // If agent knows how to hunt, he won't starve. Probably
            if (huntSkill == true)
            {
                double distance = 1000;
                int targetX = this.xLocation;
                int targetY = this.yLocation;

                // Types are arranged according to the food chain
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type < this.type) != null))
                {
                    Agent victim = cell.agent.Find(ag => ag.type < this.type);
                    int a = victim.xLocation - this.xLocation;
                    int b = victim.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }
                if (targetX > this.xLocation)
                {
                    ++direction;
                }
                else if (targetX < this.xLocation)
                {
                    --direction;
                }
                if (targetY > this.yLocation)
                {
                    direction += 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction -= 3;
                }

                if (direction != 5)
                {
                    return direction;
                }
                else
                {
                    return rand.Next(1, 10);
                }
            }

            // If agent is very hungry, it won't require many skills to hunt cabbage (type 1)
            if (hunger < 12)
            {
                double distance = 1000;
                int targetX = this.xLocation;
                int targetY = this.yLocation;
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type == 1) != null))
                {
                    Agent victim = cell.agent.Find(ag => ag.type == 1);
                    int a = victim.xLocation - this.xLocation;
                    int b = victim.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }
                if (targetX > this.xLocation)
                {
                    ++direction;
                }
                else if (targetX < this.xLocation)
                {
                    --direction;
                }
                if (targetY > this.yLocation)
                {
                    direction += 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction -= 3;
                }

                if (direction != 5)
                {
                    return direction;
                }
                else
                {
                    return rand.Next(1, 10);
                }
            }

            // If everything is safe, agent can find a partner for breeding
            if (hunger >= 12)
            {
                double distance = 1000;
                int targetX = this.xLocation;
                int targetY = this.yLocation;
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type == this.type) != null))
                {
                    Agent partner = cell.agent.Find(ag => ag.type == this.type);
                    int a = partner.xLocation - this.xLocation;
                    int b = partner.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }
                if (targetX > this.xLocation)
                {
                    ++direction;
                }
                else if (targetX < this.xLocation)
                {
                    --direction;
                }
                if (targetY > this.yLocation)
                {
                    direction += 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction -= 3;
                }

                if (direction != 5)
                {
                    return direction;
                }
                else
                {
                    return rand.Next(1, 10);
                }
            }

            // If something unexpected happens, rely on random
            return rand.Next(1, 10);
        }

        public override bool Breed(int type)
        {
            if (this.hunger < 3) return false;      // No sex, if you are dying from hunger
            if (type == this.type)
            {
                breedColldown = 1 * 12;
                return true;
            }

            return false;
        }

        public override bool Eat(int type) 
        {
            if (type < this.type)
            {
                this.hunger = 24;                   // The real problem is to tune these constants
                return true;
            }
            return false;
        }

        public override bool Age()
        {
            if (--this.hunger == 0) return true;        // Dies from hunger
            if (++this.age == this.maxAge) return true; // Dies from age
            if (--this.breedColldown <= 0) this.breedable = true;
            return false;                               // Lives long and prospers
        }
    }

    // ------------------------------------------------------------------
    //
    class Wolf : Agent
    {
        public Wolf(int pID, int pAge = 0, int pxLocation = 0, int pyLocation = 0)
        {
            this.id             = pID;
            this.type           = 4;
            this.age            = pAge;
            this.xLocation      = pxLocation;
            this.yLocation      = pyLocation;
            this.image          = AgentLifeSim.Properties.Resources.wolf1;
            this.maxAge         = 10 * 12;
            this.breedColldown  = (1 - this.age) * 12;
            this.breedable      = false;
            this.symbol         = "W";
            this.hunger         = 48;
        }

        public override int Move(Random rand, List<Cell> sight)
        {
            int direction = 5;

            // And wolves don't hunt if they aren't hungry
            if (huntSkill == true && this.hunger < 12)
            {
                double distance = 1000;
                int targetX = this.xLocation;
                int targetY = this.yLocation;

                // Wolves don't hunt cabbage (type 1), so...
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type == 2 || a.type == 3) != null))
                {
                    Agent victim = cell.agent.Find(ag => ag.type == 2 || ag.type == 3);
                    int a = victim.xLocation - this.xLocation;
                    int b = victim.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }
                if (targetX > this.xLocation)
                {
                    ++direction;
                }
                else if (targetX < this.xLocation)
                {
                    --direction;
                }
                if (targetY > this.yLocation)
                {
                    direction += 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction -= 3;
                }

                if (direction != 5)
                {
                    return direction;
                }
                else
                {
                    return rand.Next(1, 10);
                }
            }

            // Everything is safe, wolf can breed
            if (hunger >= 12)   
            {
                double distance = 1000;
                int targetX = this.xLocation;
                int targetY = this.yLocation;
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type == this.type) != null))
                {
                    Agent partner = cell.agent.Find(ag => ag.type == this.type);
                    int a = partner.xLocation - this.xLocation;
                    int b = partner.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }
                if (targetX > this.xLocation)
                {
                    ++direction;
                }
                else if (targetX < this.xLocation)
                {
                    --direction;
                }
                if (targetY > this.yLocation)
                {
                    direction += 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction -= 3;
                }

                if (direction != 5)
                {
                    return direction;
                }
                else
                {
                    return rand.Next(1, 10);
                }
            }

            return rand.Next(1, 10);
        }

        public override bool Breed(int type)
        {
            if (this.hunger < 3) return false;
            if (type == this.type)
            {
                this.breedColldown = 6;
                return true;
            }

            return false;
        }

        public override bool Eat(int type)
        {
            if (type != 1 && type < this.type)
            {
                this.hunger = 48;
                return true;
            }
            return false;
        }

        public override bool Age()
        {
            if (--this.hunger == 0) return true;
            if (++this.age == this.maxAge) return true;
            if (--this.breedColldown <= 0) this.breedable = true;
            return false;
        }
    }

    // ------------------------------------------------------------------
    //
    class Rabbit : Agent
    {
        public Rabbit(int pID, int pAge = 0, int pxLocation = 0, int pyLocation = 0)
        {
            this.id             = pID;
            this.type           = 2;
            this.age            = pAge;
            this.xLocation      = pxLocation;
            this.yLocation      = pyLocation;
            this.image          = AgentLifeSim.Properties.Resources.rabbit1;
            this.maxAge         = 8 * 12;
            this.breedColldown  = (1 - this.age) * 12;
            this.breedable      = false;
            this.symbol         = "R";
            this.hunger         = 24;
        }

        public override int Move(Random rand, List<Cell> sight)
        {
            int direction = 5;
            if (runSkill == true)
            {
                double distance = 1000;
                int targetX = this.xLocation;
                int targetY = this.yLocation;
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type == 4 || a.type == 3) != null))
                {
                    Agent enemy = cell.agent.Find(ag => ag.type == 4 || ag.type == 3);
                    int a = enemy.xLocation - this.xLocation;
                    int b = enemy.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }
                if (targetX > this.xLocation)
                {
                    --direction;
                }
                else if (targetX < this.xLocation)
                {
                    ++direction;
                }
                if (targetY > this.yLocation)
                {
                    direction -= 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction += 3;
                }

                if (direction != 5)
                {
                    return direction;
                }
            }
            if (hunger < 12)    
            {
                double distance = 1000;
                int targetX = this.xLocation;
                int targetY = this.yLocation;
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type == 1) != null))
                {
                    Agent victim = cell.agent.Find(ag => ag.type == 1);
                    int a = victim.xLocation - this.xLocation;
                    int b = victim.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }
                if (targetX > this.xLocation)
                {
                    ++direction;
                }
                else if (targetX < this.xLocation)
                {
                    --direction;
                }
                if (targetY > this.yLocation)
                {
                    direction += 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction -= 3;
                }

                if (direction != 5)
                {
                    return direction;
                }
                else
                {
                    return rand.Next(1, 10);
                }
            }            
            if (hunger >= 12)   
            {
                double distance = 1000;
                int targetX = this.xLocation;
                int targetY = this.yLocation;
                foreach (Cell cell in sight.FindAll(x => x.agent.Find(a => a.type == this.type) != null))
                {
                    Agent partner = cell.agent.Find(ag => ag.type == this.type);
                    int a = partner.xLocation - this.xLocation;
                    int b = partner.yLocation - this.yLocation;
                    if (distance < Math.Sqrt(a * a + b * b))
                    {
                        distance = Math.Sqrt(a * a + b * b);
                        targetX = a;
                        targetY = b;
                    }
                }
                if (targetX > this.xLocation)
                {
                    ++direction;
                }
                else if (targetX < this.xLocation)
                {
                    --direction;
                }
                if (targetY > this.yLocation)
                {
                    direction += 3;
                }
                else if (targetY < this.yLocation)
                {
                    direction -= 3;
                }

                if (direction != 5)
                {
                    return direction;
                }
                else
                {
                    return rand.Next(1, 10);
                }
            }

            return rand.Next(1, 10);
        }

        public override bool Breed(int type)
        {
            if (this.hunger < 3) return false; 
            if (type == this.type)
            {
                this.breedColldown = 3;
                return true;
            }

            return false;
        }

        public override bool Eat(int type)
        {
            if (type < this.type)
            {
                this.hunger = 18;
                return true;
            }
            return false;
        }

        public override bool Age()
        {
            if (--this.hunger == 0) return true;
            if (++this.age == this.maxAge) return true;
            if (--this.breedColldown <= 0)  this.breedable = true;
            return false;
        }
    }

    // ------------------------------------------------------------------
    //
    class Tree : Agent
    {
        public Tree(int pID, int pAge = 0, int pxLocation = 0, int pyLocation = 0)
        {
            this.id             = pID;
            this.type           = 5;
            this.age            = pAge;
            this.xLocation      = pxLocation;
            this.yLocation      = pyLocation;
            this.image          = AgentLifeSim.Properties.Resources.tree1;
            this.maxAge         = 150 * 12;
            this.breedColldown  = 45 * 12;
            this.breedable      = false;
            this.symbol         = "T";
        }

        public override int Move(Random rand, List<Cell> sight)
        {
            return 0;
        }

        public override bool Breed(int type)
        {
            if (type == this.type)
            {
                this.breedColldown = 80;
                return true;
            }

            return false;
        }

        public override bool Eat(int type)
        {
            return false;
        }

        public override bool Age()
        {
            if (++this.age == this.maxAge)
            {
                return true;
            }
            if (--this.breedColldown <= 0)
            {
                this.breedable = true;
            }
            return false;
        }
    }
}