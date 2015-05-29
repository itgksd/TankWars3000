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
        Rectangle posRect, leftLineRect, playerRect, tankRect, scoreRect, killsRect, deathsRect;
        int kills, death, score, pos;
        string name;
        Color color;
        static SpriteFont font;
        Vector2 scorePos, playerPos, posPos, killsPos, deathsPos, position;

        public Vector2 Position
        {
            get { return position; }
        }
        public int Height
        {
            get { return posRect.Height; }
        }

        public ScoreBoardItem (ContentManager content, Vector2 position, int kills, int death, int pos, int score, string name, Color color)
        {
            this.position = position;
            this.kills    = kills;
            this.death    = death;
            this.score    = score;
            this.pos      = pos;
            this.name     = name;
            this.color    = color;

            // Resources
            if (pixel == null || font == null)
            {
                pixel = content.Load<Texture2D>("pixel");
                font = content.Load<SpriteFont>("DefFont");
            }


            // Rectangles
            int height = 50;
            posRect      = new Rectangle((int)position.X, (int)position.Y, 136, height);
            leftLineRect = new Rectangle(posRect.X, posRect.Y, 4, posRect.Height);
            playerRect   = new Rectangle(posRect.X + posRect.Width + 10, (int)position.Y, 500, height);
            tankRect     = new Rectangle(0,0,10,10); // Ändra senare
            scoreRect    = new Rectangle(playerRect.X + playerRect.Width + 10, playerRect.Y, 60, height);
            killsRect    = new Rectangle(scoreRect.X + scoreRect.Width + 10, scoreRect.Y, 60, height);
            deathsRect   = new Rectangle(killsRect.X + killsRect.Width + 10, killsRect.Y, 60, height);

            // Text positions
            posPos    = new Vector2(posRect.X + 6, posRect.Y);
            playerPos = new Vector2(playerRect.X + 50, playerRect.Y);
            scorePos  = new Vector2(scoreRect.X + 5, scoreRect.Y);
            killsPos  = new Vector2(killsRect.X + 5, killsRect.Y);
            deathsPos = new Vector2(deathsRect.X + 5, deathsRect.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Position
            spriteBatch.Draw(pixel, posRect, Color.White);
            spriteBatch.Draw(pixel, leftLineRect, Color.Blue);
            spriteBatch.DrawString(font, ""+score, posPos, Color.White);

            // Player
            spriteBatch.Draw(pixel, playerRect, Color.White);
            // Draw tank
            // Draw name

            // Score
            spriteBatch.Draw(pixel, scoreRect, Color.White);
            // Draw score

            // Kills
            spriteBatch.Draw(pixel, killsRect, Color.White);
            // Draw kills

            // Deaths
            spriteBatch.Draw(pixel, deathsRect, Color.White);
            // Draw deaths
        }
    }
}
