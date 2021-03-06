﻿using Microsoft.Xna.Framework;
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
        static Texture2D pixel, tankTx;
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
            if (pixel == null || font == null || tankTx == null)
            {
                pixel = content.Load<Texture2D>("pixel");
                font = content.Load<SpriteFont>("DefFont");
                tankTx = content.Load<Texture2D>("tank/tank");
            }


            // Rectangles
            int height     = 40, gap = 5;
            posRect        = new Rectangle((int)position.X, (int)position.Y, 40, height);
            leftLineRect   = new Rectangle(posRect.X, posRect.Y, 4, posRect.Height);
            playerRect     = new Rectangle(posRect.X + posRect.Width + gap, (int)position.Y, 500, height);
            float procsize = (float)Decimal.Divide(playerRect.Height - 10, tankTx.Height);
            tankRect       = new Rectangle(playerRect.X + 5, playerRect.Y + 5, (int)(tankTx.Width * procsize), (int)(tankTx.Height * procsize));
            scoreRect      = new Rectangle(playerRect.X + playerRect.Width + gap, playerRect.Y, 60, height);
            killsRect      = new Rectangle(scoreRect.X + scoreRect.Width + gap, scoreRect.Y, 60, height);
            deathsRect     = new Rectangle(killsRect.X + killsRect.Width + gap, killsRect.Y, 60, height);

            // Text positions
            posPos    = new Vector2(posRect.X + 13, posRect.Y + 5);
            playerPos = new Vector2(playerRect.X + 60, playerRect.Y + 5);
            scorePos  = new Vector2(scoreRect.X + 5, scoreRect.Y + 5);
            killsPos  = new Vector2(killsRect.X + 5, killsRect.Y + 5);
            deathsPos = new Vector2(deathsRect.X + 10, deathsRect.Y + 5);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Position
            spriteBatch.Draw(pixel, posRect, new Color(100, 100, 100, 0));
            spriteBatch.Draw(pixel, leftLineRect, Color.Blue);
            spriteBatch.DrawString(font, ""+pos, posPos, Color.White);

            // Player
            spriteBatch.Draw(pixel, playerRect, new Color(100, 100, 100, 0));
            spriteBatch.Draw(tankTx, tankRect, color);
            spriteBatch.DrawString(font, name, playerPos, Color.White);

            // Score
            spriteBatch.Draw(pixel, scoreRect, new Color(100, 100, 100, 0));
            spriteBatch.DrawString(font, ""+score, scorePos, Color.White);

            // Kills
            spriteBatch.Draw(pixel, killsRect, new Color(100, 100, 100, 0));
            spriteBatch.DrawString(font, ""+kills, killsPos, Color.Lime);

            // Deaths
            spriteBatch.Draw(pixel, deathsRect, new Color(100, 100, 100, 0));
            spriteBatch.DrawString(font, ""+death, deathsPos, Color.Red);
        }
    }
}
