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

        public NormalButton(ContentManager content, Vector2 position, string title) : base(content, position, title)
        {
            useInsideRec = false;
        }

        public override void Update(OldNewInput input)
        {
            base.Update(input);

            if (input.MouseRec.Intersects(outsideRec) && input.SingleLeftClick())
            {
                pressed = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
