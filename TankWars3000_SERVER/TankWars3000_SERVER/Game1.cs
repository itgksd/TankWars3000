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
        SHOOT
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

        int amountOfPlayers = 8;
        int connectionAmount = 0;

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
                                }
                                break;
                            case NetIncomingMessageType.Data:

                                if (incomingMessage.ReadByte() == (byte)PacketTypes.READY)
                                {
                                    
                                    // markera Tanken/klienten som redo
                                }
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
                           int x = incomingMessage.ReadInt32();
                           int y = incomingMessage.ReadInt32();
                           float angle = incomingMessage.ReadFloat();
                           String name = incomingMessage.ReadString();

                           // kollision här tack
                           Collision(angle);

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
                           int x = incomingMessage.ReadInt32();
                           int y = incomingMessage.ReadInt32();

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
                            
                        }
                    }
                }
            }
        }
    }
}