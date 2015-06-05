using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class Tank
    {
        #region Atributes

        static SpriteFont testfont;

        static Vector2    textureOrigin;

        static Texture2D   texture;

        Vector2 position,  explositionPos;

        Vector2 direction       = new Vector2(1, 0);
        
        Vector2 speed           = new Vector2(5, 5);

        Color tankcolor         = new Color();

        TimeSpan reloadTime     = new TimeSpan(0, 0, 4);

        // Respawn
        float  r_timer;
        float  r_intervall = 1;
        int    r_time      = 5;
        string r_string;

        //
        float timer;

        float timerlimit = 50;
        //the limit is so that you need only update one float instead of everywhere it is used

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

        public void Update(ContentManager content, GraphicsDeviceManager graphics, Dictionary<string, Tank> tanks, List<TankTrack> tracks, GameTime gametime)
        {
                #region recieve action from server
                if ((incmsg = Game1.Client.ReadMessage()) != null)
                {
                    switch (incmsg.ReadByte())
                    {
                        case (byte)PacketTypes.MOVE:
                            string incmsg_name = incmsg.ReadString();
                            tanks[incmsg_name].Angle = incmsg.ReadFloat();
                            tanks[incmsg_name].Position = new Vector2(incmsg.ReadFloat(), incmsg.ReadFloat());
                            bool tempcollisionbool = incmsg.ReadBoolean();
                            if (tempcollisionbool)  //if bullets hits tanks
                            {
                                tanks[incmsg_name].explositionPos.X = incmsg.ReadFloat();
                                tanks[incmsg_name].explositionPos.Y = incmsg.ReadFloat();

                                Track(tracks, content);
                            }
                            break;

                        case (byte)PacketTypes.DEATH:
                                health = 0;
                                position = new Vector2(incmsg.ReadFloat(), incmsg.ReadFloat());
                            break;

                        case (byte)PacketTypes.SHOOT:
                            bullets = new List<Bullet>();
                            int j = incmsg.ReadInt32();
                            for (int i = 0; i < j; i++ )
                            {
                                Bullet bullet = new Bullet(content, new Vector2(incmsg.ReadFloat(), incmsg.ReadFloat()));
                                bullets.Add(bullet); 
                            }
                            break;

                        case (byte)PacketTypes.GAMESTATE:
                            Debug.WriteLine("Cl-Reveiced gamestate change");
                            Game1.gameState = (GameStates)incmsg.ReadByte();
                            break;

                        case (byte)PacketTypes.HEARTBEAT:
                            // Respond to the Heartbeat request of the server
                            Debug.WriteLine("Cl-Received heartbeat, responding");
                            NetOutgoingMessage outmsg = Game1.Client.CreateMessage();
                            outmsg.Write((byte)PacketTypes.HEARTBEAT);
                            outmsg.Write(Game1.tankname);
                            Game1.Client.SendMessage(outmsg, incmsg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                            break;

                        case (byte)PacketTypes.DISCONNECTREASON:
                            Debug.WriteLine("Cl-Kicked by server");
                            Notify.NewMessage("Kick reason: " + incmsg.ReadString(), Color.Purple);
                            Game1.gameState = GameStates.Lobby;
                            break;
                        default:
                            break;
                    }
                }
                #endregion

            if (health == 0)
            {
                r_timer += gametime.ElapsedGameTime.Milliseconds;

                r_string = r_time.ToString();

                if (r_timer == r_intervall)
                    r_time--;
                if (r_time == 0)
                {
                    health   = 3;
                    r_time   = 5;
                }
            }
        }

        public void Input(OldNewInput input, ContentManager content, Dictionary<string, Tank> tanks, GameTime gametime)
        {
            if (IsAlive == true)
            {
                NetOutgoingMessage outmsg;
                timer += gametime.ElapsedGameTime.Milliseconds;
                position = tanks[name].position;

                #region Movment
                if (input.newKey.IsKeyDown(Keys.W) && timer >= timerlimit)
                {

                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
                    position += direction * speed;
                    //tanks[name].position = position;
                    //update position, then send it to the server

                    //if (timer >= timerlimit)
                    //{
                        outmsg = Game1.Client.CreateMessage();
                        outmsg.Write((byte)PacketTypes.MOVE);
                        outmsg.Write(name);
                        outmsg.Write(position.X);
                        outmsg.Write(position.Y);
                        outmsg.Write(angle);
                        Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableUnordered);
                    //}
                }

                if (input.newKey.IsKeyDown(Keys.S) && timer >= timerlimit)
                {
                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
                    position -= direction * speed;
                    //tanks[name].position = position;
                    //update position, then send it to the server

                    //if (timer >= timerlimit)
                    //{
                        outmsg = Game1.Client.CreateMessage();
                        outmsg.Write((byte)PacketTypes.MOVE);
                        outmsg.Write(name);
                        outmsg.Write(position.X);
                        outmsg.Write(position.Y);
                        outmsg.Write(angle);
                        Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableUnordered);
                    //}
                }

                if (input.newKey.IsKeyDown(Keys.D) && timer >= timerlimit)
                {
                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
                    angle += MathHelper.Pi / 50;
                    //MathHelper.Pi * 2 is a full turn
                    // / 2 is 90 degrees
                    // divide to smaller pieces for less turn each button press

                    direction.X = (float)Math.Cos(angle);
                    direction.Y = (float)Math.Sin(angle);

                    //tanks[name].direction = direction;

                    //if (timer >= timerlimit)
                    //{
                        outmsg = Game1.Client.CreateMessage();
                        outmsg.Write((byte)PacketTypes.MOVE);
                        outmsg.Write(name);
                        outmsg.Write(position.X);
                        outmsg.Write(position.Y);
                        outmsg.Write(angle);
                        Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableUnordered);
                    //}
                }
                if (input.newKey.IsKeyDown(Keys.A) && timer >= timerlimit)
                {
                    //needs to CreateMessage() every time a button is pressed, which means more than once some updates
                    angle -= MathHelper.Pi / 50;
                    //MathHelper.Pi * 2 is a full turn
                    // / 2 is 90 degrees
                    // divide to smaller pieces for less turn each button press

                    direction.X = (float)Math.Cos(angle);
                    direction.Y = (float)Math.Sin(angle);

                    //tanks[name].direction = direction;

                    //if (timer >= timerlimit)
                    //{
                        outmsg = Game1.Client.CreateMessage();
                        outmsg.Write((byte)PacketTypes.MOVE);
                        outmsg.Write(name);
                        outmsg.Write(position.X);
                        outmsg.Write(position.Y);
                        outmsg.Write(angle);
                        Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableUnordered);
                    //}
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
                //Reset timer
                if (timer > timerlimit)
                {
                    timer = 0;
                }
                #endregion
            }
        }

        public void Draw(SpriteBatch spriteBatch, Dictionary<string, Tank> tanks)
        {
            if (health > 0)
            {
                foreach (KeyValuePair<string, Tank> tank in tanks)
                {
                    spriteBatch.Draw(tank.Value.Texture, tank.Value.position, null, tank.Value.tankcolor, tank.Value.angle, textureOrigin, 1.0f, SpriteEffects.None, 0f);
                }

                foreach (Bullet bullet in bullets)
                    bullet.Draw(spriteBatch);
            }
            else
                // Draws the respawn timer
                spriteBatch.DrawString(testfont, r_string, new Vector2(200, 200), Color.Black);
        }

        public Tank(ContentManager content, string name, Color color)
        {
            texture       = content.Load<Texture2D>("Tank/Tank");
            testfont      = content.Load<SpriteFont>("Testfont");
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
