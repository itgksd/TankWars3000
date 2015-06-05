using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000_SERVER
{
    class bullet
    {
        Vector2 pos;
        float angle;
        private string name;
        private Rectangle rect;
        private Vector2 velocity;

        public bullet(float x, float y, float a, string n)
        {
            rect = new Rectangle((int)x, (int) y, 10, 10);
            pos = new Vector2(x, y);
            name = n;
            velocity = new Vector2((float)Math.Cos(a) * 10, (float)Math.Cos(a)) * 10;
            angle = a;
        }
        public void Update()
        {
            pos += velocity;
        }
        public float Angle
        {
            get
            {
                return angle;
            }
        }

        public Rectangle Rect
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
        public Vector2 Pos
        {
            get
            {
                return pos;
            }
        }
        public String Name
        {
            get
            {
                return name;
            }
        }
    }
}
