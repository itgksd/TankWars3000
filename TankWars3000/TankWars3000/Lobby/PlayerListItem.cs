using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class PlayerListItem
    {
        public enum State
        {
            NONE,
            FILLING,
            EMPTYING
        }

        State state = State.NONE;
        public State Statee
        {
            get { return state; }
        }
        Rectangle animationRec;
        Color animationColor = Color.White;

        static Texture2D pixelTx, tankTx;
        Rectangle rectangle, tankRec, readyRec;
        static SpriteFont font;

        String name = "";
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        Color tankColor;
        public Color TankColor
        {
            get { return tankColor; }
            set { tankColor = value; }
        }

        bool ready;
        public bool Ready
        {
            get { return ready; }
            set { ready = value; }
        }


        public PlayerListItem(ContentManager content, Vector2 position, bool animate = false)
        {
            Load(content, position);

            if (animate)
                state = State.FILLING;
        }
        public PlayerListItem(ContentManager content, Vector2 position, string playerName, Color playerColor, bool ready, bool animate = false)
        {
            Load(content, position);
            float procsize = (float)Decimal.Divide(rectangle.Height - 10, tankTx.Height);
            tankRec = new Rectangle(rectangle.X + 7, rectangle.Y + 5, (int)(tankTx.Width * procsize), (int)(tankTx.Height * procsize));
            readyRec = new Rectangle(rectangle.X, rectangle.Y, 4, rectangle.Height);

            name = playerName;
            tankColor = playerColor;
            this.ready = ready;

            if (animate)
                state = State.FILLING;
        }

        public void Load(ContentManager content, Vector2 position)
        {
            // Keeps it from loading of the hdd all the time. The variables are static
            if (pixelTx == null || font == null || tankTx == null)
            {
                pixelTx = content.Load<Texture2D>("pixel");
                font = content.Load<SpriteFont>("DefFont");
                tankTx = content.Load<Texture2D>("Tank/Tank");
            }

            // The size and location of the playerlist's backplate
            this.rectangle = new Rectangle((int)position.X, (int)position.Y, 400, 40);

            float procsize = (float)Decimal.Divide(rectangle.Height - 10, tankTx.Height);
            tankRec = new Rectangle(rectangle.X + 7, rectangle.Y + 5, (int)(tankTx.Width * procsize), (int)(tankTx.Height * procsize));

            readyRec = new Rectangle(rectangle.X, rectangle.Y, 4, rectangle.Height); // Ready notifier

            // The rectangle used for the start animation
            animationRec = new Rectangle(rectangle.X + rectangle.Width + 40, rectangle.Y, 0, rectangle.Height);
        }

        int ticksTimer = 0; // <- To lazy for gametime
        int aniSpeed = 9; // The speed of the animation
        public void Update()
        {
            // All for the animation! 

            if (state == State.FILLING) // Only tick the clock if we are filling
                ticksTimer++;

            // Wait X frames before starting the animate
            if (state == State.FILLING && ticksTimer > 120)
            {
                // Animate the rectangle going over the main rectangle from right to left. lower X while increasing Width
                animationRec.X -= aniSpeed;
                animationRec.Width += aniSpeed;

                if (animationRec.X <= rectangle.X)
                    state = State.EMPTYING;

                animationColor.R -= 5; // Change the color a little for some nicer effects
            }
            else if (state == State.EMPTYING)
            {
                animationRec.Width -= aniSpeed;

                if (animationRec.Width <= 0)
                    state = State.NONE; // We are done here!

                animationColor.B -= 5;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Hide the playerlistitem until the animation rectangle has covered the entire thing
            if (state == State.EMPTYING || state == State.NONE)
            {

                // Only draw if this spot is being used to contain a player
                if (name != "")
                {
                    spriteBatch.Draw(pixelTx, rectangle, new Color(100, 100, 100, 0)); // The "backplate"
                    spriteBatch.Draw(tankTx, tankRec, tankColor);
                    spriteBatch.Draw(pixelTx, readyRec, ready ? Color.Lime : Color.Red);
                    spriteBatch.DrawString(font, name, new Vector2(rectangle.X + 60, rectangle.Y + 5), Color.White);
                }
                else
                {
                    spriteBatch.Draw(pixelTx, rectangle, new Color(100, 100, 100, 0) * 0.6f); // The "backplate"
                    spriteBatch.Draw(pixelTx, readyRec, Color.Blue * 0.3f);
                    spriteBatch.Draw(tankTx, tankRec, Color.Black * 0.3f);
                    spriteBatch.DrawString(font, "Open", new Vector2(rectangle.X + 60, rectangle.Y + 5), Color.White * 0.3f);
                }
            }

            if (state == State.FILLING || state == State.EMPTYING) // No need to draw if it's not being used
            {
                spriteBatch.Draw(pixelTx, animationRec, animationColor);
            }
        }
    }
}
