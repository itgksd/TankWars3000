using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class Tank
    {
        int health;
        int damage;

        float direction;
        TimeSpan bulletRate;

        Vector2 speed;
        Vector2 pos;
        Vector2 spawnPos;

        String Name;
        Rectangle rect;

        bool alive;
        bool fired;

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
