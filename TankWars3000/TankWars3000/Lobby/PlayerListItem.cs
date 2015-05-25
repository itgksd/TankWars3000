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


        public PlayerListItem(ContentManager content, Vector2 position)
        {
            Load(content, position);
        }
        public PlayerListItem(ContentManager content, Vector2 position, string playerName, Color playerColor, bool ready)
        {
            Load(content, position);
            tankRec = new Rectangle(rectangle.X + 7, rectangle.Y + 5, 30, rectangle.Height - 10);
            readyRec = new Rectangle(rectangle.X, rectangle.Y, 4, rectangle.Height);

            name = playerName;
            tankColor = playerColor;
            this.ready = ready;
        }

        public void Load(ContentManager content, Vector2 position)
        {
            if (pixelTx == null || font == null || tankTx == null)
            {
                pixelTx = content.Load<Texture2D>("pixel");
                font = content.Load<SpriteFont>("DefFont");
                tankTx = content.Load<Texture2D>("Tank/tanktest");
            }

            this.rectangle = new Rectangle((int)position.X, (int)position.Y, 300, 30);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixelTx, rectangle, new Color(100, 100, 100, 0));

            // Only draw if this spot is being used to contain a player
            if (name != "")
            {
                spriteBatch.Draw(tankTx, tankRec, tankColor);
                spriteBatch.Draw(pixelTx, readyRec, ready ? Color.Lime : Color.Red);
                spriteBatch.DrawString(font, name, new Vector2(rectangle.X + 50, rectangle.Y), Color.White);
            }
        }
    }
}
