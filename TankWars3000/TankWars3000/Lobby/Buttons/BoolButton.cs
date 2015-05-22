using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class BoolButton : BaseButton
    {
        bool isTrue;
        public bool IsTrue
        {
            get { return isTrue; }
            set { isTrue = value; }
        }

        public BoolButton(ContentManager content, Vector2 position, string title, bool defBool) : base(content, position, title)
        {
            isTrue = defBool;
        }

        public override void Update(OldNewInput input)
        {
            base.Update(input);

            if (input.MouseRec.Intersects(outsideRec) && input.SingleLeftClick())
            {
                isTrue = isTrue ? false : true; // Toggle
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(pixel, insideRec, isTrue ? Color.Lime : Color.Red);
        }
    }
}
