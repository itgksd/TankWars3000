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
    class Tank
    {
        #region Atributes

        int health = 3;

        float degrees = 0;

        Vector2 speed;
        Vector2 position;
        Vector2 spawnPos;

        Color color = new Color;

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
            texture = content.Load<Texture2D>("Tank/TankTest");
        }

        public void Update(KeyboardState key)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, collisionRect,Color.White);
        }

        public Tank()
        {
            
        }
        #endregion
    }
}
