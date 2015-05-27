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
        private string name;
        private Rectangle rect;

        public bullet(int x, int y, float a, string n)
        {
            rect = new Rectangle(x, y, 10, 6);
            name = n;
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
        public String Name
        {
            get
            {
                return name;
            }
        }
        public int XPos
        {
            get
            {
                return xPos;
            }
        }
        public int YPos
        {
            get
            {
                return yPos;
            }
        }
    }
}
