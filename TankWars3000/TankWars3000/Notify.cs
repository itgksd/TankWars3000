using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    static class Notify
    {
        static Texture2D pixelTx;
        static SpriteFont font;

        static List<NotifyItem> items = new List<NotifyItem>();

        static public void LoadContent(ContentManager content)
        {
            pixelTx = content.Load<Texture2D>("pixel");
            font = content.Load<SpriteFont>("DefFont");
        }

        static public void Update(GameTime gameTime)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Update(gameTime);
                if (items[i].Statee == NotifyItem.State.done)
                    items.RemoveAt(i);
            }
        }

        static public void Draw(SpriteBatch spriteBatch)
        {
            items.ForEach(i => i.Draw(spriteBatch));
        }

        static public void NewMessage(string text, Color color)
        {
            items.Add(new NotifyItem(pixelTx, font, items.Count > 0 ? items[items.Count - 1].Y + 30 : 0, text, color));
        }




        private class NotifyItem
        {
            public enum State
            {
                goingdown, down, goingup, done
            }

            State state = State.goingdown;
            public State Statee
            {
                get { return state; }
            }

            static Texture2D pixelTx;
            static SpriteFont font;
            Rectangle rectangle;
            public Rectangle Rectangle
            {
                get { return rectangle; }
            }
            string message;
            Color color;
            TimeSpan timer;
            int y;
            public int Y
            {
                get { return y; }
            }

            public NotifyItem(Texture2D ppixelTx, SpriteFont ffont, int yy, string mmessage, Color ccolor)
            {
                pixelTx = ppixelTx;
                font = ffont;

                message = mmessage;
                color = ccolor;

                y = yy;


                rectangle = new Rectangle(0, -30, Game1.ScreenRec.Width, 30);
            }

            public void Update(GameTime gameTime)
            {
                if (rectangle.Y >= Y)
                    state = State.down;

                if (state == State.down && timer.TotalSeconds >= 5)
                    state = State.goingup;

                if (state == State.goingup && rectangle.Y <= -100)
                    state = State.done;


                if (state == State.goingdown)
                {
                    rectangle.Y += 2;
                }
                else if (state == State.down)
                {
                    timer += gameTime.ElapsedGameTime;
                }
                else if (state == State.goingup)
                {
                    rectangle.Y -= 2;
                }
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(pixelTx, rectangle, color * 0.8f);
                spriteBatch.DrawString(font, message, new Vector2(10, rectangle.Y), Color.White);
            }
        }
    }
}
