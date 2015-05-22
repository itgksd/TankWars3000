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

        float direction;
        TimeSpan bulletRate;

        Vector2 speed;
        Vector2 pos;
        Vector2 spawnPos;

        private String name;
        private Rectangle rect;

        bool alive;
        bool fired;

        private int kills;
        private int deaths;
        
        public Tank()
        {
           
        }

        public void Update()
        {

        }

        private void CheckCollision()
        {

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
    }
}
