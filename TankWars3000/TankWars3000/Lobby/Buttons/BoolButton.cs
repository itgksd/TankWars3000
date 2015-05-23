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

        Action checkedChangeEvent;

        public BoolButton(ContentManager content, Vector2 position, string title, bool defBool, Action checkedChangeEvent = null) : base(content, position, title)
        {
            isTrue = defBool;
            this.checkedChangeEvent = checkedChangeEvent;
        }

        public override void Update(OldNewInput input)
        {
            base.Update(input);

            if (enabled && input.MouseRec.Intersects(outsideRec) && input.SingleLeftClick())
            {
                isTrue = isTrue ? false : true; // Toggle

                if (checkedChangeEvent != null)
                    checkedChangeEvent();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(pixel, insideRec, isTrue ? enabled ? Color.Lime : Color.Lime * disabledAplha : enabled ? Color.Red : Color.Red * disabledAplha);
        }
    }
}
