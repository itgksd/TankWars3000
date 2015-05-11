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

        Vector2 speed, position, spawnPos, direction, textureOrigin;

        Color color = new Color();

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
            if (key.IsKeyDown(Keys.W))
        {
                position += direction * speed;
            }

            if (key.IsKeyDown(Keys.S))
            {
                position -= direction * speed;
            }
            if (key.IsKeyDown(Keys.D)/* && old.IsKeyUp(Keys.D)*/)
            {
                degrees += 6f;
                direction.X = (float)Math.Cos(degrees);
                direction.Y = (float)Math.Sin(degrees);
            }
            if (key.IsKeyDown(Keys.A)/* && old.IsKeyUp(Keys.A)*/)
            {
                degrees -= 6f;
                direction.X = (float)Math.Cos(degrees);
                direction.Y = (float)Math.Sin(degrees);
            }
            
            if (key.IsKeyDown(Keys.Space))
            {
                // shoot
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, collisionRect,Color.White, degrees, textureOrigin, 1.0f,SpriteEffects.None, 0f);
        }

        public Tank(ContentManager content)
        {
            direction = new Vector2(1, 0);
            textureOrigin.X = texture.Width / 2;
            textureOrigin.Y = texture.Height / 2;
            LoadContent(content);
        }
        #endregion
    }
}
