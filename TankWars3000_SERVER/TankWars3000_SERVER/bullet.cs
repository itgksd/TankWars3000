using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000_SERVER
{
    class bullet
    {
        int xPos;
        int yPos;
        float angle;
        string name;
        private Rectangle rect;

        public bullet(int x, int y, float a, string n)
        {
            rect = new Rectangle(x, y, 10, 6);
            name = n;
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
    }
}
