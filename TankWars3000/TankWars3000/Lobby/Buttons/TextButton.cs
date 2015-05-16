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
    public enum TextButtonType
    {
        IP,
        UserName
    }

    class TextButton : BaseButton
    {
        TextButtonType type;

        string text, prvText;

        public TextButton(ContentManager content, Vector2 position, string title, TextButtonType type, string defText) : base(content, position, title)
        {
            this.type = type;
            text = prvText = defText;
        }

        public override void Update(OldNewInput input)
        {
            base.Update(input);

            if (input.newMouse.LeftButton == ButtonState.Pressed && input.MouseRec.Intersects(rectangle))
            {
                active = true;
                text = "";
            }

            if (active)
            {
                char nextChar;
                if (input.TryConvertKeyboardInput(out nextChar))
                {
                    text += nextChar;
                }

                if (input.SingleKey(Keys.Back) && text.Length > 0)
                    text = text.Remove(text.Length - 1);

                if (input.SingleKey(Keys.Escape))
                {
                    active = false;
                    text = prvText;
                }
                else if (input.SingleKey(Keys.Enter))
                {
                    throw new NotImplementedException();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(font, active ? text + "_" : text, new Vector2(insideRec.X + 10, insideRec.Y), Color.Black);
        }
    }
}
