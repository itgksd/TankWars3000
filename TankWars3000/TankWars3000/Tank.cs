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

        List<Bullet> bullets = new List<Bullet>();

        #endregion

        #region Methods

        public void Update(KeyboardState key, ContentManager content)
        {
            if (key.IsKeyDown(Keys.W))
                position += direction * speed;

            if (key.IsKeyDown(Keys.S))
                position -= direction * speed;

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
                bullets.Add(new Bullet(content, degrees, direction, position));
            }

            foreach (Bullet bullet in bullets)
                bullet.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, collisionRect, Color.White, degrees, textureOrigin, 1.0f,SpriteEffects.None, 0f);
            foreach (Bullet bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        public Tank(ContentManager content)
        {
            texture = content.Load<Texture2D>("Tank/TankTest");
            direction = new Vector2(1, 0);
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
        }
        #endregion
    }
}
