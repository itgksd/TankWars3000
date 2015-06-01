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

        static SpriteFont testfont;

        static Vector2 textureOrigin;

        static Texture2D texture;

        Vector2 position, direction, explositionPos;
        
        Vector2 speed           = new Vector2(5, 5);

        Color tankcolor         = new Color();

        TimeSpan reloadTime     = new TimeSpan(0, 0, 4);

        Rectangle collisionRect = new Rectangle();

        NetIncomingMessage incmsg;

        List<Bullet> bullets    = new List<Bullet>();

        // Track
        TankTrack track;
        Vector2   trackpos;
        bool      lastRightTrack;

        string name;

        bool bulletFired        = false;
        bool IsAlive            = true;

        int health              = 3;

        float angle             = 0;//angle in radians
        #endregion

        #region properties
        public string Name
        {
            get {return name;}
            set { name = value; }
        }
        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Texture2D Texture
        {
            get { return texture; }
        }
        public SpriteFont TestFont
        {
            get { return testfont; }
        }
        #endregion


        #region Methods

        public void Update(ContentManager content, GraphicsDeviceManager graphics, List<Tank> tanks, List<TankTrack> tracks)
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
            
                #region recieve action from server
                if ((incmsg = Game1.Client.ReadMessage()) != null)
                {
                    switch (incmsg.ReadByte())
                    {
                        case (byte)PacketTypes.MOVE:
                            string incmsg_name = incmsg.ReadString();
                            for (int i  = 0; i < tanks.Count; i++ )
                            {
                                if (tanks[i].Name == incmsg_name)
                               {
                                    tanks[i].Angle    = incmsg.ReadFloat();
                                    tanks[i].Position = new Vector2(incmsg.ReadFloat(), incmsg.ReadFloat());
                                    bool tempcollisionbool = incmsg.ReadBoolean();
                                    try     //Server will not always send position for explosion so try to read it
                                    {
                                        tanks[i].explositionPos.X = incmsg.ReadFloat();
                                        tanks[i].explositionPos.Y = incmsg.ReadFloat();

                                        Track(tracks, content);
                                    }
                                    catch (Exception ex)
                                    { }
                               }
                            
                                
                            }
                            break;

                        case (byte)PacketTypes.SHOOT:
                            Bullet bullet = new Bullet(content, incmsg.ReadString(), new Vector2(incmsg.ReadFloat(), incmsg.ReadFloat()));
                            bullets.Add(bullet);
                            break;
                        default:
                            break;
                    }
                }
                #endregion
        }

        public void Input(OldNewInput input, ContentManager content, List<Tank> tanks)
        {
            if (IsAlive == true)
            {
                NetOutgoingMessage outmsg = Game1.Client.CreateMessage();

                for (int i = 0; i < tanks.Count; i++ )
                {
                    if (name == tanks[i].name)
                        position = tanks[i].position;
                }


                #region Movment
                if (input.newKey.IsKeyDown(Keys.W))
                {
                    outmsg = Game1.Client.CreateMessage();

                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
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
                    //TEMP outmsg = Game1.Client.CreateMessage();
                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
                    position -= direction * speed;
                    //update position, then send it to the server

                    outmsg.Write((byte)PacketTypes.MOVE);
                    outmsg.Write(name);
                    outmsg.Write(position.X);
                    outmsg.Write(position.Y);
                    outmsg.Write(angle);
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }

                if (input.newKey.IsKeyDown(Keys.D) /*&& input.oldKey.IsKeyUp(Keys.D) && input.newKey.IsKeyUp(Keys.A)bortkommenterat för test*/)
                {
                    outmsg = Game1.Client.CreateMessage();
                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
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
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
                if (input.newKey.IsKeyDown(Keys.A) /*&& input.oldKey.IsKeyUp(Keys.A) && input.newKey.IsKeyUp(Keys.D) bortkommenterat för test*/)
                {
                    outmsg = Game1.Client.CreateMessage();
                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
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
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
                #endregion

                #region shoot
                if (input.newKey.IsKeyDown(Keys.Space) && input.oldKey.IsKeyUp(Keys.Space))
                {
                    outmsg = Game1.Client.CreateMessage();
                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
                    outmsg.Write((byte)PacketTypes.SHOOT);
                    outmsg.Write(name);
                    outmsg.Write(position.X);
                    outmsg.Write(position.Y);
                    outmsg.Write(angle);
                    Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
                }
                #endregion
            }
        }

        public void Draw(SpriteBatch spriteBatch, List<Tank> tanks)
        {
            foreach (Tank tank in tanks)
            {
                tank.collisionRect = new Rectangle((int)tank.position.X, (int)tank.position.Y, tank.Texture.Width, tank.Texture.Height);
                
                //give values to the rectangle here because it needs to be up-to-date every time the tank is drawn
                spriteBatch.Draw(tank.Texture, tank.position, tank.collisionRect, tank.tankcolor, tank.angle, textureOrigin, 1.0f, SpriteEffects.None,1f);
                
                //writes the value of the tanks vector2 position, the spritefonts' position has an offset of 200 on the x-axis
                spriteBatch.DrawString(tank.TestFont, tank.position.ToString(), new Vector2(tank.position.X + 200, tank.position.Y), Color.Wheat);
            
            }
            
            foreach (Bullet bullet in bullets)
                bullet.Draw(spriteBatch);
        }

        public Tank(ContentManager content, string name, Color color)
        {
            texture       = content.Load<Texture2D>("Tank/Tank");
            testfont      = content.Load<SpriteFont>("Testfont");
            direction     = new Vector2(1, 0);
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            this.name = name;
            tankcolor = color;
        }
        public Tank() {} //need to make one tank that is empty to run through the tank list
                         //since i have integrated the List<Tank> with the tank class.
                         //this is to make Game1 cleaner.

        private void Track(List<TankTrack> tracks ,ContentManager content)
        {
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
        }
        #endregion
    }
}
