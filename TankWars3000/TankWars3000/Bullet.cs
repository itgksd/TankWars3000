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

        Vector2 position;
        Vector2 speed;

        Texture2D texture;

        Rectangle collisionRect = new Rectangle();

        Vector2 textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);

        float degrees;

        int damage = 1;

        #endregion
        #region Methods

        public void Update()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, collisionRect, Color.White, degrees, textureOrigin, 1.0f, SpriteEffects.None, 0f);
        }

        public Bullet(ContentManager content)
        {
            texture = content.Load<Texture2D>("Tank/Bullet");
        }
        #endregion
    }
}
