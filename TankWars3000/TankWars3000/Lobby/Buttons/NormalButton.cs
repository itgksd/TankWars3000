using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class NormalButton : BaseButton
    {
        Action onClickEvent;

        bool pressed = false;
        public bool Pressed
        {
            get { return pressed; }
            set { pressed = value; }
        }
        
        public Color TitleColor
        {
            get { return titleColor; }
            set { titleColor = value; }
        }

        public NormalButton(ContentManager content, Vector2 position, string title, Action onClickEvent = null, bool halfSize = false) : base(content, position, title)
        {
            useInsideRec = false;
            this.onClickEvent = onClickEvent;

            if (halfSize)
                outsideRec.Width = outsideRec.Width / 2 - 5;
        }

        public override void Update(OldNewInput input)
        {
            base.Update(input);

            if (enabled && input.MouseRec.Intersects(outsideRec) && input.SingleLeftClick())
            {
                pressed = true;

                if (onClickEvent != null)
                    onClickEvent();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
