using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        public Rectangle OutsideRec
        {
            get { return outsideRec; }
        }
        protected Rectangle insideRec;
        protected bool useInsideRec = true;

        Color outSideColor, orgOutSideColor, insideColor;

        string title;
        Vector2 titlePos;
        protected Color titleColor;

        protected bool active;
        public bool Active
        {
            get { return active; }
        }

        protected bool enabled = true;
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        protected float disabledAplha = 0.5f;

        SoundEffect clickSound;
        protected bool soundEffect = true;
        public bool SoundEffect
        {
            get { return soundEffect; }
            set { soundEffect = value; }
        }

        public BaseButton(ContentManager content, Vector2 position, string title)
        {
            pixel = content.Load<Texture2D>("pixel");
            font = content.Load<SpriteFont>("DefFont");

            outsideRec = new Rectangle((int)position.X, (int)position.Y, 500, 50);
            insideRec = new Rectangle((int)(position.X + 250), (int)(position.Y + 10), 240, 30);

            outSideColor = orgOutSideColor = new Color(50, 50, 50, 0);//Color.LightGray;
            insideColor = Color.Gray;

            this.title = title;
            titlePos = new Vector2(position.X + 10, position.Y + 10);
            titleColor = Color.White;

            clickSound = content.Load<SoundEffect>("Sound/buttonClick");
        }

        virtual public void Update(OldNewInput input)
        {
            if (enabled && (input.MouseRec.Intersects(outsideRec) || active))
            {
                outSideColor = Color.DarkGray;
            }
            else
            {
                outSideColor = orgOutSideColor;
            }

            if (enabled && input.MouseRec.Intersects(outsideRec) && input.SingleLeftClick() && soundEffect)
            {
                clickSound.Play();
            }
        }

        virtual public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, outsideRec, enabled ? outSideColor : outSideColor * disabledAplha);
            if (useInsideRec)
                spriteBatch.Draw(pixel, insideRec, enabled ? insideColor : insideColor * disabledAplha);
            spriteBatch.DrawString(font, title, titlePos, enabled ? titleColor : titleColor * disabledAplha);
        }
    }
}
