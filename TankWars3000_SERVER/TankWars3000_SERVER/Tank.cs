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

        String name;
        Rectangle rect;

        bool alive;
        bool fired;

        public int kills;
        public int deaths;
        
        public Tank()
        {
           
        }

        public void Update()
        {

        }

        private void CheckCollision()
        {

        }
    }
}
