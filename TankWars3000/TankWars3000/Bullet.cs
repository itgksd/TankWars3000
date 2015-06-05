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

        Vector2 position, speed, textureOrigin;

        Texture2D texture;

        Rectangle collisionRect = new Rectangle();

        string name;

        int damage              = 1;

        bool isAlvie;

        public bool IsAlive
        {
            get { return isAlvie; }
            set { isAlvie = true; }
        }

        #endregion

        #region Methods

        public void Update(GraphicsDeviceManager graphics)
        {
            if (position.X > graphics.GraphicsDevice.Viewport.Width)
                isAlvie = false;
            if (position.Y > graphics.GraphicsDevice.Viewport.Height)
                isAlvie = false;
            if (position.Y < 0)
                isAlvie = false;
            if (position.X < 0)
                isAlvie = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }

        //need direction to make it move the right way, don't need angle anymore because of the texture
        public Bullet(ContentManager content,string newname, Vector2 newposition)
        {
            texture       = content.Load<Texture2D>("Tank/Bullet");
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            speed         = new Vector2(15, 15);
            position      = newposition;
            name          = newname;
        }
        #endregion
    }
}
