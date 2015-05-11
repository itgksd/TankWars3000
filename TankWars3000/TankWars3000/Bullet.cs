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

        float degrees;

        int damage = 1;

        Vector2 textureOrigin;

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
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
        }
        #endregion
    }
}
