using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000_SERVER
{
   public class Tank
    {
        int health;
        int damage;

        float angle;
        TimeSpan bulletRate;

        Vector2 speed;
        Vector2 pos;
        Vector2 spawnPos;

        DateTime lastBeat;
        public DateTime LastBeat
        {
            get { return lastBeat; }
            set { lastBeat = value; }
        }

        Color tankColor = Color.White;
       public Color TankColor
        {
            get { return tankColor; }
            set { tankColor = value; }
        }

       bool ready;
       public bool Ready
       {
           get { return ready; }
           set { ready = value; }
       }

        private String name;
        private Rectangle rect;

        bool alive;
        bool fired;

        private int kills;
        private int deaths;
        
        public Tank(String n)
        {
            name = n;
            health = 3;

            lastBeat = DateTime.Now;
        }

        public int Health
        {
            get
            {
                return health;
            }
            set
            {
                health = value;
            }
        }


        public Rectangle Tankrect
        {
            get
            {
                return rect;
            }
            set
            {
                rect = value;
            }
        }
       public String Name
        {
            get
            {
                return name;
            }
        }

       public Vector2 Position
       {
           get
           {
               return pos;
           }
           set
           {
               pos = value;
           }
       }

       public int Kills
       {
           get 
           {
               return kills;
           }
           set
           {
               kills = value;
           }
       }
       public int Deaths
       {
           get
           {
               return deaths;
           }
           set
           {
               deaths = value;
           }
       }
       public float Angle
       {
           get { return angle; }
           set { angle = value; }
       }
    }
}
