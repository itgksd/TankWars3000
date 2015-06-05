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

        Texture2D texture;

        Rectangle collisionRect = new Rectangle();

        int damage              = 1;

        /*bool isAlvie;

        public bool IsAlive
        {
            get { return isAlvie; }
            set { isAlvie = true; }
        }
        */
        #endregion

        #region Methods

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }

        //need direction to make it move the right way, don't need angle anymore because of the texture
        public Bullet(ContentManager content, Vector2 newposition)
        {
            texture       = content.Load<Texture2D>("Tank/Bullet");
            position      = newposition;
        }
        #endregion
    }
}
