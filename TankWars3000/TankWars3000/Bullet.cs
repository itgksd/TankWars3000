using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class Bullet
    {
        #region Atributes

        Vector2 position, speed, direction, textureOrigin;

        Texture2D texture;

        Rectangle collisionRect = new Rectangle();

        int damage              = 1;

        float radians; //will be using radians not degrees so this name makes more sense

        #endregion

        #region Methods

        public void Update()
        {
            position += direction * speed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, collisionRect, Color.White, radians, textureOrigin, 1.0f, SpriteEffects.None, 0f);
        }

        //need raidans to draw bullet, direction to make it move the right way
        public Bullet(ContentManager content, float newradians ,Vector2 newdirection, Vector2 newposition)
        {
            texture       = content.Load<Texture2D>("Tank/Bullet");
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            speed         = new Vector2(15, 15);
            direction     = newdirection;
            radians       = newradians;
            position      = newposition;
        }
        #endregion
    }
}
