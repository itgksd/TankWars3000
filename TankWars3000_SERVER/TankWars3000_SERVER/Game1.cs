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
namespace TankWars3000_SERVER
{

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
        START,
        MOVE,
        SHOOT,
        TEST,
        LOBBYPLAYERLIST,
        COLOR,
        DEATH,
        GAMESTATE,
        DISCONNECTREASON,
        HEARTBEAT,
        STARTPOS
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
        DateTime nextUpdate;
        int amountOfPlayers = 8;
        int connectionAmount = 0;

        bool canCountTime = true;
        bool sendStartPos = true;

        List<bullet> bullets = new List<bullet>();
        Dictionary<string, Tank> tanks;
        System.Timers.Timer timer;
        Vector2 explosionPosition;

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

            timer = new System.Timers.Timer(5000);
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Check the connections
            NetOutgoingMessage outmsg = Server.CreateMessage();
            outmsg.Write((byte)PacketTypes.HEARTBEAT);
            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
            Debug.WriteLine("Sv-Sending heartbeat");
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
                    // så att Ingame bara skickar startpos en gång
                    sendStartPos = true;
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

                                    // godkänner klienten, detta måste tydligen göras
                                    incomingMessage.SenderConnection.Approve();
                                    connectionAmount++;
                                    
                                    // skapa ny Tank och lägg det i en lista
                                    tanks.Add(name, new Tank(name));

                                    Debug.WriteLine("Sv-Sending test message");
                                    // Skicka ett test paket för att låta klienten veta att anslutningen fungerar.. "Ni" kan nog ta bort den senare om den inte behövs längre
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
                                        // Ändra färgen på tanken

                                        playerName = incomingMessage.ReadString();
                                        Color playerColor = new Color(incomingMessage.ReadByte(), incomingMessage.ReadByte(), incomingMessage.ReadByte());

                                        tanks[playerName].TankColor = playerColor;
                                        break;
                                    case (byte)PacketTypes.HEARTBEAT:
                                        string name = incomingMessage.ReadString();
                                        tanks[name].LastBeat = DateTime.Now;
                                        Debug.WriteLine("Sv-HeartBeat respons for " + name);
                                        break;
                                }
                                break;
                            default:
                                Debug.WriteLine("Sv-" + incomingMessage.MessageType + " - " + incomingMessage.ReadString());
                                break;
                                }

                                // Send list of player to all everytime somebody sends anything to the server
                                NetOutgoingMessage outMsg =  Server.CreateMessage();
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

                    }

                    // Check if we can start
                    int readyCount = 0;
                    foreach (KeyValuePair<string, Tank> tank in tanks)
                        if (tank.Value.Ready)
                            readyCount++;
                    if (tanks.Count > 1 && (float)Decimal.Divide(readyCount, tanks.Count) > 0.7f)
                    {
                        gameState = GameStates.Ingame;
                        Debug.WriteLine("Sv-Sending ingame message");
                        NetOutgoingMessage outmsg = Server.CreateMessage();
                        outmsg.Write((byte)PacketTypes.GAMESTATE);
                        outmsg.Write((byte)GameStates.Ingame);
                        Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                    }
                }


                if (gameState == GameStates.Ingame)
                {
                    if (sendStartPos)
                    {
                        int y;
                        int x;
                        double degSum = (2* Math.PI) / tanks.Count;
                        double deg = 0;
                        
                        foreach (KeyValuePair<string, Tank> tank in tanks)
                        {
                            
                            NetOutgoingMessage outmsg = Server.CreateMessage();
                            outmsg.Write((byte)PacketTypes.STARTPOS);
                            outmsg.Write(tank.Value.Name);
                        

                            x = (int) ((Math.Cos(deg)) * 350) + (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2) ;
                            y = (int)((Math.Sin(deg)) * 350) + (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2);
                            deg += degSum;

                            float angle = (float) (deg + Math.PI);

                            outmsg.Write(tank.Value.Name);
                            outmsg.Write(angle);
                            outmsg.Write(x);
                            outmsg.Write(y);
                            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                        }
    
                        sendStartPos = false;
                    }

                    if (DateTime.Now > nextUpdate)
                    {
                        // uppdatera bullet
                        nextUpdate = DateTime.Now.AddMilliseconds(100);
                        UpdateAndSendBullets();
                    }
                    
                    foreach(KeyValuePair<string,Tank> tank in tanks) 
                    {
                        if (tank.Value.Health <= 0)
                        {
                            NetOutgoingMessage outmsg = Server.CreateMessage();
                            outmsg.Write((byte)PacketTypes.DEATH);
                            Random r = new Random();
                            while (true)
                            {
                                int x = r.Next(1366);
                                int y = r.Next(768);
                                
                                break;
                            }
                        

                            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                        }
                    }


                    if ((incomingMessage = Server.ReadMessage()) != null)
                    {
                        if (incomingMessage.ReadByte() == (byte)PacketTypes.MOVE)
                        {
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
                           outmsg.Write(bulletCollision());
                            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                           if (bulletCollision() == true)
                           {
                               outmsg.Write(explosionPosition.X);
                               outmsg.Write(explosionPosition.Y);
                           }
                           
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
                            outmsg.Write(name);
                           outmsg.Write(x);
                           outmsg.Write(y);
                           Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                       }
                    }


                }
                
                
                if (gameState == GameStates.Scoreboard)
                {

                }


                // Ta bort gammla anslutningar
                List<string> tmpKeys = new List<string>();
                foreach (KeyValuePair<string, Tank> tank in tanks)
                {
                    if ((DateTime.Now - tank.Value.LastBeat).TotalSeconds >= 31)
                    {
                        tmpKeys.Add(tank.Value.Name);
                    }
                }
                foreach (string item in tmpKeys)
                {
                    tanks.Remove(item);
                }
            }
            base.Update(gameTime);
        }

        public void UpdateAndSendBullets()
        {
            foreach (bullet bullet in bullets)
            {
                // update bullet pos and send

                
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }



        private void Collision(float angle) //Kollision av Tanks
        {
            foreach (KeyValuePair<string, Tank> tank1 in tanks)
            {
                foreach (KeyValuePair<string, Tank> tank2 in tanks)
                {
                    if (tank1.Key != tank2.Key)
                    {
                        if (tank1.Value.Tankrect.Intersects(tank2.Value.Tankrect)) //Själva kollisionen
                        {
                            Vector2 collisionPosition1 = new Vector2();
                            collisionPosition1.X = tank1.Value.Position.X + ((float)Math.Cos(angle + Math.PI));
                            collisionPosition1.Y = tank1.Value.Position.Y + ((float)Math.Sin(angle + Math.PI));
                             
                            tank1.Value.Position = collisionPosition1; // Ändra positionen av tank1 

                            Vector2 collisionPosition2 = new Vector2();
                            collisionPosition2.X = tank2.Value.Position.X + ((float)Math.Cos(angle));
                            collisionPosition2.Y = tank2.Value.Position.Y + ((float)Math.Sin(angle));

                            tank2.Value.Position = collisionPosition2; // Ändra positionen av tank2

                        }
                    }
                }
                        }
                    }
        private bool bulletCollision() // Kolla om bullet kolliderar med Tank
        {
            for (int i = 0; i <= bullets.Count - 1; )
            {
                foreach (KeyValuePair<string, Tank> tank in tanks)
                {
                    if (bullets[i].Name != tank.Value.Name) // Tank kan inte träffa sig själv!
                    {
                        if (bullets[i].Rect.Intersects(tank.Value.Tankrect)) // Själva kollisionen
                        {

                            tank.Value.Health--; // Tank blir skadad!

                            explosionPosition = tank.Value.Position; // Bestämma explosions-positionen åt client, värdet skickas senare genom MOVE-Package

                            Vector2 bulletcollisionposition = new Vector2();
                            bulletcollisionposition.X = tank.Value.Position.X + ((float)Math.Cos(bullets[i].Angle));
                            bulletcollisionposition.Y = tank.Value.Position.Y + ((float)Math.Sin(bullets[i].Angle));
                            tank.Value.Position = bulletcollisionposition; //Tank blir flyttad av kollisionen med bullet

                            bullets.RemoveAt(i); // Bullet "förstörs" vid kollisionen
                            return true; // Att kollisionen hände skickas till client och skickar då explosionposition
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