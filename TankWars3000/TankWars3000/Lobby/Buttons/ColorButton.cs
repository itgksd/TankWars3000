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
    class ColorButton : BaseButton
    {
        Texture2D rainbow;

        // Arrow variables
        Texture2D arrow;
        Vector2 lowerArrow, upperArrow;
        int selectedX;

        Color selectedColor;
        public Color SelectedColor
        {
            get { return selectedColor; }
        }

        Action colorChangeEvent;

        public ColorButton(ContentManager content, Vector2 position, string title, Action colorChangeEvent = null) : base(content, position, title)
        {
            rainbow = content.Load<Texture2D>("images/rainbowColors");

            arrow = content.Load<Texture2D>("images/triangle");
            selectedX = insideRec.X + insideRec.Width / 2;

            this.colorChangeEvent = colorChangeEvent;
        }

        public override void Update(OldNewInput input)
        {
            base.Update(input);

            if (enabled && input.MouseRec.Intersects(insideRec) && input.newMouse.LeftButton == ButtonState.Pressed)
            {
                selectedX = input.newMouse.X;

                Color[] pixelColors = new Color[rainbow.Width * rainbow.Height];
                rainbow.GetData(pixelColors);

                selectedColor = pixelColors[(input.newMouse.X - insideRec.X) + ((input.newMouse.Y - insideRec.Y) * rainbow.Width)];

                // Event
                if (colorChangeEvent != null)
                    colorChangeEvent();
            }


            // Update the locations of the indicator arrows
            lowerArrow = new Vector2(selectedX - arrow.Width / 2, insideRec.Y + (insideRec.Height - arrow.Height));
            upperArrow = new Vector2(selectedX - arrow.Width / 2, insideRec.Y);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.Draw(rainbow, insideRec, enabled ? Color.White : Color.White * disabledAplha);

            // Draw the arrows
            spriteBatch.Draw(arrow, lowerArrow, enabled ? Color.White : Color.White * disabledAplha);
            spriteBatch.Draw(arrow, upperArrow, null, enabled ? Color.White : Color.White * disabledAplha, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
        }
    }
}
