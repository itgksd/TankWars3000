using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class Tank
    {
        #region Atributes

        int health = 3;
        int damage = 1;

        float degrees;

        Vector2 speed;
        Vector2 position;
        Vector2 spawnPos;

        TimeSpan reloadTime = new TimeSpan(0, 0, 4);

        Texture2D texture;

        string name;

        bool bulletFired = false;
        bool isAlive = true;

        Rectangle collisionRect = new Rectangle();

        #endregion
        #region Methods

        public void LoadContent(ContentManager content)
        {

        }

        public void Update()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {

        }

        public Tank()
        {

        }
        #endregion
    }
}
