using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class ScoreBoardItem
    {
        static Texture2D pixel;
        Rectangle posRect, playerRect, tankRect, scoreRect, killsRect, deathsRect;
        int kills, death, score, pos;
        string name;
        Color color;
        float percentWidth, percentHeight;
        static SpriteFont font;

        public ScoreBoardItem (Vector2 position, int kills, int death, int pos, int score, string name, Color color, ContentManager content)
        {
            this.kills = kills;
            this.death = death;
            this.score = score;
            this.pos = pos;
            this.name = name;
            this.color = color;

            int height = 50;
            posRect = new Rectangle((int)position.X, (int)position.Y, 136, height);
            //playerRect = new Rectangle(


        }
    }
}
