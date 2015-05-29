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

        Vector2 speed, position, spawnPos, direction, textureOrigin, explositionPos;

        Color tankcolor             = new Color();

        TimeSpan reloadTime     = new TimeSpan(0, 0, 4);

        Texture2D texture;

        Rectangle collisionRect = new Rectangle();

        NetIncomingMessage incmsg;

        List<Bullet> bullets    = new List<Bullet>();

        // Track
        TankTrack traxck;
        Vector2   trackpos;
        bool      lastRightTrack;

        string name;

        bool bulletFired        = false;
        bool IsAlive            = true;
        bool startPos           = false;

        int health              = 3;

        float angle             = 0;//angle in radians

        #endregion

        #region Methods

        public void Update(ContentManager content, GraphicsDeviceManager graphics, List<Tank> tanks, List<TankTrack> tracks)
        {

            #region start position
            if (!startPos)
            {
                if ((incmsg = Game1.Client.ReadMessage()) != null)
                {
                    if (incmsg.ReadByte() == (byte)PacketTypes.STARTPOS)
                    {
                        foreach (Tank tank in tanks)
                        {
                            tank.name = incmsg.ReadString();
                            tank.angle = incmsg.ReadFloat();
                            tank.position.X = incmsg.ReadInt32();
                            tank.position.Y = incmsg.ReadInt32();
                        }
                        startPos = true;
                    }
                }
            }
            #endregion
            else
            {
                #region Bullet
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
                #endregion

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

                #region Track
                // track
                if (Vector2.Distance(trackpos, position) > 40)
                {
                    Vector2 footStepPos;
                    if (lastRightTrack)
                        footStepPos = Vector2.Transform(textureOrigin + /*offset>*/new Vector2(-50, -73), Matrix.CreateRotationZ(angle)) + position;
                    else
                        footStepPos = Vector2.Transform(textureOrigin + /*offset>*/new Vector2(-50, -33), Matrix.CreateRotationZ(angle)) + position;

                    lastRightTrack = lastRightTrack ? false : true; // Toggle

                    tracks.Add(new TankTrack(content, trackpos, angle)); // Add

                    trackpos = position;
                }
                #endregion

                if ((incmsg = Game1.Client.ReadMessage()) != null)
                {
                    switch (incmsg.ReadByte())
                    {
                        case (byte)PacketTypes.MOVE:
                            foreach (Tank tank in tanks)
                            {
                                tank.name = incmsg.ReadString();
                                tank.angle = incmsg.ReadFloat();
                                tank.position.X = incmsg.ReadInt32();
                                tank.position.Y = incmsg.ReadInt32();
                                try
                                {
                                    tank.explositionPos.X = incmsg.ReadInt32();
                                    tank.explositionPos.Y = incmsg.ReadInt32();
                                }
                                catch (Exception ex)
                                { }
                            }
                            break;

                        case (byte)PacketTypes.SHOOT:
                            Bullet bullet = new Bullet(content, incmsg.ReadString(), new Vector2(incmsg.ReadUInt32(), incmsg.ReadUInt32()));
                            bullets.Add(bullet);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void Input(OldNewInput input, ContentManager content)
        {
            if (IsAlive == true)
            {
                NetOutgoingMessage outmsg = Game1.Client.CreateMessage();

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
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
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
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }

                if (input.newKey.IsKeyDown(Keys.D))
                {
                    angle       += MathHelper.Pi / 20;
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
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
                if (input.newKey.IsKeyDown(Keys.A))
                {
                    angle       -= MathHelper.Pi / 20;
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
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
                #endregion

                if (input.newKey.IsKeyDown(Keys.Space) && input.oldKey.IsKeyUp(Keys.Space))
                {
                    outmsg.Write((byte)PacketTypes.SHOOT);
                    outmsg.Write(name);
                    outmsg.Write(position.X);
                    outmsg.Write(position.Y);
                    outmsg.Write(angle);
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, List<Tank> tanks)
        {
            if ((incmsg               = Game1.Client.ReadMessage()) != null)
            {
                if (incmsg.ReadByte() == (byte)PacketTypes.MOVE)
                {
                    foreach (Tank tank in tanks)
                    {
                        spriteBatch.Draw(tank.texture, tank.position, tank.collisionRect, tank.tankcolor, tank.angle, textureOrigin, 1.0f, SpriteEffects.None, 0f);
                    }
                }
            }
            foreach (Bullet bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        public Tank(ContentManager content, string name, Color color)
        {
            texture       = content.Load<Texture2D>("Tank/Tank");
            direction     = new Vector2(1, 0);
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            speed         = new Vector2(2, 2);
            this.name = name;
            tankcolor = color;
        }
        public Tank()
        {

        }
        #endregion
    }
}
