using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;
using System.Threading;
using System.Diagnostics;
namespace TankWars3000_SERVER{

    enum GameStates
    {
        Lobby,
        Ingame,
        Scoreboard
    }
    enum PacketTypes
    {
        LOGIN,
        READY,
        MOVE,
        SHOOT,
        TEST,
        LOBBYPLAYERLIST
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameStates gameState;

        // Server object
        static NetServer Server;
        // Configuration object
        static NetPeerConfiguration Config;
        NetIncomingMessage incomingMessage;
        DateTime previousUpdate;
        int amountOfPlayers = 8;
        int connectionAmount = 0;

        List<bullet> bullets = new List<bullet>();

        List<Tank> tanks;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            gameState = GameStates.Lobby;

            // Create new instance of configs. Parameter is "application Id". It has to be same on client and server.
            Config = new NetPeerConfiguration("game");

            // Set server port
            Config.Port = 14242;

            // Max client amount
            Config.MaximumConnections = amountOfPlayers;

            // Enable New messagetype. Explained later
            Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            // Create new server based on the configs just defined
            Server = new NetServer(Config);

            // Start it
            Server.Start();

            tanks = new List<Tank>();

        
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
           
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            
            while (true)
            {
                if (gameState == GameStates.Lobby)
                {

                    if ((incomingMessage = Server.ReadMessage()) != null)
                    {
                        switch (incomingMessage.MessageType)
                        {
                            //första paketet som skickas från klienten, skickas när klienten anroper Connect() metoden 
                            case NetIncomingMessageType.ConnectionApproval:

                                // kollar om det första paketet är ett login paket. 
                                // man kan göra om enums till bytes.
                                if (incomingMessage.ReadByte() == (byte)PacketTypes.LOGIN)
                                {
                                    String name = incomingMessage.ReadString();
                                    //kolla om namnet finns

                                    // godkänner klienten, detta måste tydligen göras
                                    incomingMessage.SenderConnection.Approve();
                                    connectionAmount++;
                                    
                                    // skapa ny Tank och lägg det i en lista
                                    tanks.Add(new Tank(name));

                                    Debug.WriteLine("Sv-Sending test message");
                                    // Skicka ett test paket för att låta klienten veta att anslutningen fungerar.. "Ni" kan nog ta bort den senare om den inte behövs längre
                                    NetOutgoingMessage testMsg = Server.CreateMessage();
                                    testMsg.Write((byte)PacketTypes.TEST);
                                    testMsg.Write("Test string");
                                    Server.SendMessage(testMsg, incomingMessage.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
                                }
                                break;
                            case NetIncomingMessageType.Data:

                                if (incomingMessage.ReadByte() == (byte)PacketTypes.READY)
                                {
                                    // markera Tanken/klienten som redo
                                    
                                    string playerName = incomingMessage.ReadString();
                                    bool playerReady = incomingMessage.ReadBoolean();

                                    Debug.WriteLine("Sv-Received ready packet. Name:" + playerName + "|Ready:" + playerReady);
                                }

                                // Send list of player to all everytime somebody sends anything to the server
                                NetOutgoingMessage outMsg =  Server.CreateMessage();
                                outMsg.Write((byte)PacketTypes.LOBBYPLAYERLIST);
                                Server.SendToAll(outMsg, NetDeliveryMethod.ReliableOrdered);
                                Debug.WriteLine("Sv-Send lobby-player-list to all");

                                break;
                            default:
                                Debug.WriteLine("Sv-" + incomingMessage.MessageType + " - " + incomingMessage.ReadString());
                                break;
                        }
                    }
                    if (connectionAmount == amountOfPlayers)
                    {
                        // lobbyn full och spelet kan börja
                        gameState = GameStates.Ingame;
                    }

                }


                if (gameState == GameStates.Ingame)
                {
                    if ((incomingMessage = Server.ReadMessage()) != null)
                    {
                       if(incomingMessage.ReadByte() == (byte)PacketTypes.MOVE) {
                           //Spara värden Server fick från client
                           String name = incomingMessage.ReadString();
                           int x = incomingMessage.ReadInt32();
                           int y = incomingMessage.ReadInt32();
                           float angle = incomingMessage.ReadFloat();
           

                           // kollision här tack
                           Collision(angle);

                           //Skicka alla värden till alla Clients
                           NetOutgoingMessage outmsg = Server.CreateMessage();
                           outmsg.Write((byte)PacketTypes.MOVE);
                           outmsg.Write(name);
                           outmsg.Write(angle);
                           outmsg.Write(x);
                           outmsg.Write(y);
                           Server.SendToAll(outmsg,NetDeliveryMethod.ReliableOrdered);
                       }

                       if (incomingMessage.ReadByte() == (byte)PacketTypes.SHOOT)
                       {
                           string name = incomingMessage.ReadString();
                           int x = incomingMessage.ReadInt32();
                           int y = incomingMessage.ReadInt32();
                           float angle = incomingMessage.ReadFloat();
                           bullets.Add(new bullet(x, y, angle, name));
                           NetOutgoingMessage outmsg = Server.CreateMessage();
                           outmsg.Write((byte)PacketTypes.SHOOT);
                           outmsg.Write(x);
                           outmsg.Write(y);
                           Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                       }
                    }


                }
                
                
                if (gameState == GameStates.Scoreboard)
                {

                }
            }
            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }



        private void Collision(float angle)
        {
            foreach (Tank tank1 in tanks)
            {
                foreach (Tank tank2 in tanks)
                {
                    if (tank1.Name != tank2.Name)
                    {
                        if (tank1.Tankrect.Intersects(tank2.Tankrect))
                        {
                            Vector2 collisionPosition1 = new Vector2();
                            collisionPosition1.X = tank1.Position.X + ((float)Math.Cos(angle + Math.PI));
                            collisionPosition1.Y = tank1.Position.Y + ((float)Math.Sin(angle + Math.PI));

                            tank1.Position = collisionPosition1;

                            Vector2 collisionPosition2 = new Vector2();
                            collisionPosition2.X = tank2.Position.X + ((float)Math.Cos(angle));
                            collisionPosition2.Y = tank2.Position.Y + ((float)Math.Sin(angle));

                            tank2.Position = collisionPosition2;

                        }
                    }
                }
            }
        }
        private void bulletCollision()
        {

        }
    }
}