using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    abstract class BaseButton
    {
        protected Texture2D pixel;
        protected SpriteFont font;

        protected Rectangle outsideRec;
        protected Rectangle insideRec;

        Color outSideColor, orgOutSideColor, insideColor;

        string title;
        Vector2 titlePos;
        Color titleColor;

        protected bool active;
        public bool Active
        {
            get { return active; }
        }

        public BaseButton(ContentManager content, Vector2 position, string title)
        {
            pixel = content.Load<Texture2D>("pixel");
            font = content.Load<SpriteFont>("DefFont");

            outsideRec = new Rectangle((int)position.X, (int)position.Y, 500, 50);
            insideRec = new Rectangle((int)(position.X + 250), (int)(position.Y + 10), 240, 30);

            outSideColor = orgOutSideColor = Color.LightGray;
            insideColor = Color.Gray;

            this.title = title;
            titlePos = new Vector2(position.X + 10, position.Y + 10);
            titleColor = Color.Black;
        }

        virtual public void Update(OldNewInput input)
        {
            if (new Rectangle(input.newMouse.X, input.newMouse.Y, 1, 1).Intersects(outsideRec) || active)
            {
                outSideColor = Color.WhiteSmoke;
            }
            else
            {
                outSideColor = orgOutSideColor;
            }
        }

        virtual public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, outsideRec, outSideColor);
            spriteBatch.Draw(pixel, insideRec, insideColor);
            spriteBatch.DrawString(font, title, titlePos, titleColor);
        }
    }
}
