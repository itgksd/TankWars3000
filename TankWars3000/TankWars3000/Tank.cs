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

        float angle;//angle in radians

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

        public void Update(OldNewInput input, ContentManager content, GraphicsDeviceManager graphics)
        {
            #region input
            if (input.newKey.IsKeyDown(Keys.W))
                position += direction * speed;

            if (input.newKey.IsKeyDown(Keys.S))
                position -= direction * speed;

            if (input.newKey.IsKeyDown(Keys.D))
            {
                angle += MathHelper.Pi / 20; 
                //MathHelper.Pi * 2 is a full turn
                // / 2 is 90 degrees
                // divide to smaller pieces for less turn each button press

                direction.X = (float)Math.Cos(angle);
                direction.Y = (float)Math.Sin(angle);
            }
            if (input.newKey.IsKeyDown(Keys.A))
            {
                angle -= MathHelper.Pi / 20;
                //MathHelper.Pi * 2 is a full turn
                // / 2 is 90 degrees
                // divide to smaller pieces for less turn each button press

                direction.X = (float)Math.Cos(angle);
                direction.Y = (float)Math.Sin(angle);
            }
            
            if (input.newKey.IsKeyDown(Keys.Space) && input.oldKey.IsKeyUp(Keys.Space))
            {
                bullets.Add(new Bullet(content, angle, direction, position));
            }
            #endregion

            #region position and viewport
            if (position.X >= graphics.Viewport.Width)
                position.X = graphics.Viewport.Width - texture.Width;

            if (position.Y >= graphics.Viewport.Height)
                position.Y = graphics.Viewport.Height - texture.Height;

            if (position.X < 0)
                position.X = 0;

            if (position.Y < 0)
                position.Y = 0;
            #endregion

            foreach (Bullet bullet in bullets)
                bullet.Update();
        }

        public void CheckCollision()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, collisionRect, Color.White, angle, textureOrigin, 1.0f,SpriteEffects.None, 0f);
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
