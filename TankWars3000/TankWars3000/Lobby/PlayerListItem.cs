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
            if (pixelTx == null || font == null || tankTx == null)
            {
                pixelTx = content.Load<Texture2D>("pixel");
                font = content.Load<SpriteFont>("DefFont");
                tankTx = content.Load<Texture2D>("Tank/Tank");
            }

            this.rectangle = new Rectangle((int)position.X, (int)position.Y, 400, 40);

            animationRec = new Rectangle(rectangle.X + rectangle.Width + 40, rectangle.Y, 0, rectangle.Height);
        }

        int ticksTimer = 0;
        public void Update()
        {
            // All for the animation! :D
            if (state == State.FILLING)
                ticksTimer++;
            if (state == State.FILLING && ticksTimer > 120) // <- To lazy for gametime
            {
                animationRec.X -= 7;
                animationRec.Width += 7;

                if (animationRec.X <= rectangle.X)
                    state = State.EMPTYING;

                animationColor.R -= 5;
            }
            else if (state == State.EMPTYING)
            {
                animationRec.Width -= 7;
                if (animationRec.Width <= 0)
                    state = State.NONE;

                animationColor.B -= 5;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (state == State.EMPTYING || state == State.NONE)
            {
                spriteBatch.Draw(pixelTx, rectangle, new Color(100, 100, 100, 0));

                // Only draw if this spot is being used to contain a player
                if (name != "")
                {
                    spriteBatch.Draw(tankTx, tankRec, tankColor);
                    spriteBatch.Draw(pixelTx, readyRec, ready ? Color.Lime : Color.Red);
                    spriteBatch.DrawString(font, name, new Vector2(rectangle.X + 60, rectangle.Y + 5), Color.White);
                }
            }

            if (state == State.FILLING || state == State.EMPTYING)
            {
                spriteBatch.Draw(pixelTx, animationRec, animationColor);
            }
        }
    }
}
