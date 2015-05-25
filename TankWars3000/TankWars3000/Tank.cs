using Lidgren.Network;
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

        string name;

        bool bulletFired = false;
        bool IsAlive = true;

        int health = 3;

        float angle;//angle in radians

        Vector2 speed, position, spawnPos, direction, textureOrigin;

        Color color             = new Color();

        TimeSpan reloadTime     = new TimeSpan(0, 0, 4);

        Texture2D texture;

        Rectangle collisionRect = new Rectangle();

        NetClient client;

        NetIncomingMessage incmsg;

        List<Bullet> bullets    = new List<Bullet>();
        List<Tank> tanks        = new List<Tank>();

        #endregion

        #region Methods

        public void Update(GraphicsDeviceManager graphics)
        {
            foreach (Bullet bullet in bullets)
                bullet.Update(graphics);

            // Remove bullet
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Update(graphics);     // Update
                if (bullets[i].IsAlive == false) // Remove if dead
                {
                    bullets.RemoveAt(i);
                    i--; // Fix index
                }
            }

            #region Window update
            if (position.X >= graphics.GraphicsDevice.Viewport.Width)
                position.X = graphics.GraphicsDevice.Viewport.Width - texture.Width;

            if (position.Y >= graphics.GraphicsDevice.Viewport.Height)
                position.Y = graphics.GraphicsDevice.Viewport.Height - texture.Height;

            if (position.X < 0)
                position.X = 0;

            if (position.Y < 0)
                position.Y = 0;
            #endregion

            
        }

        public void Input(OldNewInput input, ContentManager content)
        {
            if (IsAlive == true)
            {
                NetOutgoingMessage outmsg = client.CreateMessage();

                #region Movment
                if (input.newKey.IsKeyDown(Keys.W))
                {
                    position += direction * speed;
                    //update position, then send it to the server

                    outmsg.Write((byte)PacketTypes.MOVE);
                    outmsg.Write(name);
                    outmsg.Write(position.X);
                    outmsg.Write(position.Y);
                    outmsg.Write(angle);
                    client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }

                if (input.newKey.IsKeyDown(Keys.S))
                {
                    position -= direction * speed;
                    //update position, then send it to the server

                    outmsg.Write((byte)PacketTypes.MOVE);
                    outmsg.Write(name);
                    outmsg.Write(position.X);
                    outmsg.Write(position.Y);
                    outmsg.Write(angle);
                    client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }

                if (input.newKey.IsKeyDown(Keys.D))
                {
                    angle += MathHelper.Pi / 20;
                    //MathHelper.Pi * 2 is a full turn
                    // / 2 is 90 degrees
                    // divide to smaller pieces for less turn each button press

                    direction.X = (float)Math.Cos(angle);
                    direction.Y = (float)Math.Sin(angle);

                    outmsg.Write((byte)PacketTypes.MOVE);
                    outmsg.Write(name);
                    outmsg.Write(position.X);
                    outmsg.Write(position.Y);
                    outmsg.Write(angle);
                    client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
                if (input.newKey.IsKeyDown(Keys.A))
                {
                    angle -= MathHelper.Pi / 20;
                    //MathHelper.Pi * 2 is a full turn
                    // / 2 is 90 degrees
                    // divide to smaller pieces for less turn each button press

                    direction.X = (float)Math.Cos(angle);
                    direction.Y = (float)Math.Sin(angle);

                    outmsg.Write((byte)PacketTypes.MOVE);
                    outmsg.Write(name);
                    outmsg.Write(position.X);
                    outmsg.Write(position.Y);
                    outmsg.Write(angle);
                    client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
                #endregion

                if (input.newKey.IsKeyDown(Keys.Space) && input.oldKey.IsKeyUp(Keys.Space))
                {
                    outmsg.Write((byte)PacketTypes.SHOOT);
                    outmsg.Write(name);
                    outmsg.Write(position.X);
                    outmsg.Write(position.Y);
                    outmsg.Write(angle);
                    client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, collisionRect, Color.White, angle, textureOrigin, 1.0f,SpriteEffects.None, 0f);
            if ((incmsg = client.ReadMessage()) != null)
            {
                if (incmsg.ReadByte() == (byte)PacketTypes.MOVE)
                {
                    foreach (Tank tank in tanks)
                    {
                        int newX = incmsg.ReadInt32();
                        int newY = incmsg.ReadInt32();
                        float newAngle = incmsg.ReadFloat();
                        String newName = incmsg.ReadString();

                        spriteBatch.Draw(texture, new Vector2(newX, newY), collisionRect, Color.White, newAngle, textureOrigin, 1.0f, SpriteEffects.None, 0f);
                    }
                }
            }
            foreach (Bullet bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        public Tank(ContentManager content, NetClient client)
        {
            this.client = client;
            texture = content.Load<Texture2D>("Tank/TankTest");
            direction = new Vector2(1, 0);
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            speed = new Vector2(2, 2);
        }
        #endregion
    }
}
