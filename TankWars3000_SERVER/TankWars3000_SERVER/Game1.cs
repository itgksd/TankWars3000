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
        LOBBYPLAYERLIST,
        COLOR,
        GAMESTATE,
        DISCONNECTREASON,
        HEARTBEAT
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
        Dictionary<string, Tank> tanks;
        System.Timers.Timer timer;

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

            tanks = new Dictionary<string, Tank>();

        
            base.Initialize();

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Check the connections
            NetOutgoingMessage outmsg = Server.CreateMessage();
            outmsg.Write((byte)PacketTypes.HEARTBEAT);
            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);

            Thread.Sleep(500);

            // Check for messages
            NetIncomingMessage inmsg;
            while ((inmsg = Server.ReadMessage()) != null)
            {
                
            }
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
                            //f�rsta paketet som skickas fr�n klienten, skickas n�r klienten anroper Connect() metoden 
                            case NetIncomingMessageType.ConnectionApproval:

                                // kollar om det f�rsta paketet �r ett login paket. 
                                // man kan g�ra om enums till bytes.
                                if (incomingMessage.ReadByte() == (byte)PacketTypes.LOGIN)
                                {
                                    Thread.Sleep(100); // Helps with connecting. The server sends the test message faster than the client can start receive messages
                                    String name = incomingMessage.ReadString();
                                    //kolla om namnet finns
                                    if (tanks.ContainsKey(name))
                                    {
                                        incomingMessage.SenderConnection.Approve();
                                        Debug.WriteLine("Sv-Sending deny reason. Name already exist.");
                                        NetOutgoingMessage outmsg = Server.CreateMessage();
                                        outmsg.Write((byte)PacketTypes.DISCONNECTREASON);
                                        outmsg.Write("Your name is already used on this server!");
                                        Server.SendMessage(outmsg, incomingMessage.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
                                        incomingMessage.SenderConnection.Deny("Name already exists in the server!");
                                        break;
                                    }
                                    // If the server is full send a message before disconnecting
                                    if (tanks.Count >= amountOfPlayers)
                                    {
                                        incomingMessage.SenderConnection.Approve();
                                        Debug.WriteLine("Sv-Sending deny reason.Server full");
                                        NetOutgoingMessage outmsg = Server.CreateMessage();
                                        outmsg.Write((byte)PacketTypes.DISCONNECTREASON);
                                        outmsg.Write("Server is full!");
                                        Server.SendMessage(outmsg, incomingMessage.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
                                        incomingMessage.SenderConnection.Deny("Server full!");
                                        break;
                                    }

                                    // godk�nner klienten, detta m�ste tydligen g�ras
                                    incomingMessage.SenderConnection.Approve();
                                    connectionAmount++;
                                    
                                    // skapa ny Tank och l�gg det i en lista
                                    tanks.Add(name, new Tank(name));

                                    Debug.WriteLine("Sv-Sending test message");
                                    // Skicka ett test paket f�r att l�ta klienten veta att anslutningen fungerar.. "Ni" kan nog ta bort den senare om den inte beh�vs l�ngre
                                    NetOutgoingMessage testMsg = Server.CreateMessage();
                                    testMsg.Write((byte)PacketTypes.TEST);
                                    testMsg.Write("Test string");
                                    Server.SendMessage(testMsg, incomingMessage.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
                                }
                                break;
                            case NetIncomingMessageType.Data:


                                switch (incomingMessage.ReadByte())
                                {
                                    case (byte)PacketTypes.READY:
                                    // markera Tanken/klienten som redo

                                    string playerName = incomingMessage.ReadString();
                                    bool playerReady = incomingMessage.ReadBoolean();

                                        tanks[playerName].Ready = playerReady;

                                    Debug.WriteLine("Sv-Received ready packet. Name:" + playerName + "|Ready:" + playerReady);
                                        break;
                                    case (byte)PacketTypes.COLOR:
                                        // �ndra f�rgen p� tanken

                                        playerName = incomingMessage.ReadString();
                                        Color playerColor = new Color(incomingMessage.ReadByte(), incomingMessage.ReadByte(), incomingMessage.ReadByte());

                                        tanks[playerName].TankColor = playerColor;
                                        break;
                                }
                                break;
                            default:
                                Debug.WriteLine("Sv-" + incomingMessage.MessageType + " - " + incomingMessage.ReadString());
                                break;
                        }

                        #region Send list of player to all everytime somebody sends anything to the server
                        NetOutgoingMessage outMsg = Server.CreateMessage();
                        outMsg.Write((byte)PacketTypes.LOBBYPLAYERLIST);

                        // Packa ner viktigaste informationen om alla spelarna
                        outMsg.Write(tanks.Count); // Send the amount of players that will be send
                        foreach (KeyValuePair<string, Tank> tank in tanks)
                        {
                            outMsg.Write(tank.Key); // Name
                            outMsg.Write(tank.Value.TankColor.R); // Color R
                            outMsg.Write(tank.Value.TankColor.G); // Color G
                            outMsg.Write(tank.Value.TankColor.B); // Color B
                            outMsg.Write(tank.Value.Ready); // Ready
                        }

                        Server.SendToAll(outMsg, NetDeliveryMethod.ReliableOrdered);
                        Debug.WriteLine("Sv-Send lobby-player-list to all");
                        #endregion
                    }

                    // Check if we can start
                    int readyCount = 0;
                    foreach (KeyValuePair<string, Tank> tank in tanks)
                        if (tank.Value.Ready)
                            readyCount++;
                    if (tanks.Count > 1 && (float)Decimal.Divide(readyCount, tanks.Count) > 0.7f)
                    {
                        gameState = GameStates.Ingame;

                        NetOutgoingMessage outmsg = Server.CreateMessage();
                        outmsg.Write((byte)PacketTypes.GAMESTATE);
                        outmsg.Write((byte)GameStates.Ingame);
                        Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                    }
                }


                if (gameState == GameStates.Ingame)
                {
                    
                    DateTime currentDatetime = DateTime.Now;
                    currentDatetime = previousUpdate;
                    if ((incomingMessage = Server.ReadMessage()) != null)
                    {
                       if(incomingMessage.ReadByte() == (byte)PacketTypes.MOVE) {
                           //Spara v�rden Server fick fr�n client
                           String name = incomingMessage.ReadString();
                           int x = incomingMessage.ReadInt32();
                           int y = incomingMessage.ReadInt32();
                           float angle = incomingMessage.ReadFloat();
           

                           // kollision h�r tack
                           Collision(angle);

                           //Skicka alla v�rden till alla Clients
                           NetOutgoingMessage outmsg = Server.CreateMessage();
                           outmsg.Write((byte)PacketTypes.MOVE);
                           outmsg.Write(name);
                           outmsg.Write(angle);
                           outmsg.Write(x);
                           outmsg.Write(y);
                           outmsg.Write(bulletCollision());
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
            foreach (KeyValuePair<string, Tank> tank1 in tanks)
            {
                foreach (KeyValuePair<string, Tank> tank2 in tanks)
                {
                    if (tank1.Key != tank2.Key)
                    {
                        if (tank1.Value.Tankrect.Intersects(tank2.Value.Tankrect))
                        {
                            Vector2 collisionPosition1 = new Vector2();
                            collisionPosition1.X = tank1.Value.Position.X + ((float)Math.Cos(angle + Math.PI));
                            collisionPosition1.Y = tank1.Value.Position.Y + ((float)Math.Sin(angle + Math.PI));
                             
                            tank1.Value.Position = collisionPosition1;

                            Vector2 collisionPosition2 = new Vector2();
                            collisionPosition2.X = tank2.Value.Position.X + ((float)Math.Cos(angle));
                            collisionPosition2.Y = tank2.Value.Position.Y + ((float)Math.Sin(angle));

                            tank2.Value.Position = collisionPosition2;

                        }
                    }
                }
                        }
                    }
        private bool bulletCollision()
        {
            for (int i = 0; i <= bullets.Count -1;)
            {
                foreach (KeyValuePair<string, Tank> tank in tanks)
                {
                    if (bullets[i].Name != tank.Value.Name)
                    {
                        if (bullets[i].Rect.Intersects(tank.Value.Tankrect))
                        {

                            tank.Value.Health--;

                            Vector2 bulletcollisionposition = new Vector2();
                            bulletcollisionposition.X = tank.Value.Position.X + ((float)Math.Cos(bullets[i].Angle));
                            bulletcollisionposition.Y = tank.Value.Position.Y + ((float)Math.Sin(bullets[i].Angle));
                            tank.Value.Position = bulletcollisionposition;

                            bullets.RemoveAt(i);
                            return true;
                        }
                        else
                        {
                            i++;
                            return false;
                        }
                    }
                    else return false;
                }
            }
            return false;
        }
    }
}