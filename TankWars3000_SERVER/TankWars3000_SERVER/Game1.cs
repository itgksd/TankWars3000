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
        LOGIN,            // <- Used by the lobby to send a connection request
        READY,            // <- Used by the lobby to change the "ready" status
        START,
        MOVE,
        SHOOT,
        LOBBYPLAYERLIST,  // <- Currently used by the lobby to send all the players to all of the clients
        COLOR,            // <- Lobby uses this to send a new tank color
        DEATH,
        GAMESTATE,        // <- Ingame/Lobby/Scoreboard change
        DISCONNECTREASON, // <- Disconnect with reason. e.g. tell the client that the server is full
        DISCONNECT,       // <- Used to disconnect without reason. Used only when a client disconnects itself. Has to include a name!
        HEARTBEAT,        // <- Used to see if the client/server is still alive
        STARTPOS,
        FINALSCOREBOARD
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameStates gameState;

        int tankWidth = 136;
        int tankHeight = 76;

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
            //Debug.WriteLine("Sv-Sending heartbeat");
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
                                        //outmsg.Write("Your name is already used on this server!");
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

                                    //Debug.WriteLine("Sv-Sending test message");
                                    //// Skicka ett test paket för att låta klienten veta att anslutningen fungerar.. "Ni" kan nog ta bort den senare om den inte behövs längre
                                    //NetOutgoingMessage testMsg = Server.CreateMessage();
                                    //testMsg.Write((byte)PacketTypes.TEST);
                                    //testMsg.Write("Test string");
                                    //Server.SendMessage(testMsg, incomingMessage.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
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
                                        
                                    case (byte)PacketTypes.DISCONNECT:
                                        name = incomingMessage.ReadString(); // Get name
                                        tanks.Remove(name); // Remove its tank
                                        incomingMessage.SenderConnection.Disconnect("Disconnect.By.Request"); // Disconnect the connection
                                        Debug.WriteLine("Sv-Disconnect By Request: " + name);
                                        break;
                                }
                                break;
                            default:
                                Debug.WriteLine("Sv-" + incomingMessage.MessageType + " - " + incomingMessage.ReadString());
                                break;
                        }

                        // Send list of player to all everytime somebody sends anything to the server
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

                    }

                    // Check if we can start
                    int readyCount = 0;
                    foreach (KeyValuePair<string, Tank> tank in tanks)
                        if (tank.Value.Ready)
                            readyCount++;
                    if (tanks.Count > 0 && (float)Decimal.Divide(readyCount, tanks.Count) > 0.7f) // > 1 !!!
                    {
                        gameState = GameStates.Ingame; //Spelet lämnar lobby och startar
                        Debug.WriteLine("Sv-Sending ingame message");
                        NetOutgoingMessage outmsg = Server.CreateMessage();
                        outmsg.Write((byte)PacketTypes.GAMESTATE);
                        outmsg.Write((byte)GameStates.Ingame);
                        Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                    }
                }

                //Spelet lämnar lobby och startar
                if (gameState == GameStates.Ingame)
                {
                    if (sendStartPos) //Skicka startpositioner
                    {
                        float y;
                        float x;
                        double degSum = (2 * Math.PI) / tanks.Count;
                        double deg = 0;

                        foreach (KeyValuePair<string, Tank> tank in tanks)
                        {

                            NetOutgoingMessage outmsg = Server.CreateMessage();
                            outmsg.Write((byte)PacketTypes.STARTPOS);
                            //outmsg.Write(tank.Value.Name);


                            x = (float)((Math.Cos(deg)) * 350) + (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2);
                            y = (float)((Math.Sin(deg)) * 350) + (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2);
                            deg += degSum;

                            float angle = (float)(deg + Math.PI);

                            tank.Value.Tankrect = new Rectangle((int) x,(int) y, tankWidth, tankHeight);
                            tank.Value.Angle = angle;

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
                    
                    // kolla om någon är död
                    foreach (KeyValuePair<string, Tank> tank in tanks)
                    {
                        if (tank.Value.Health <= 0)
                        {
                            NetOutgoingMessage outmsg = Server.CreateMessage();
                            outmsg.Write((byte)PacketTypes.DEATH);
                            outmsg.Write(tank.Value.Name);
                            Random r = new Random();
                            int x;
                            int y;
                            while (true)
                            {
                                x = r.Next(1366);
                                y = r.Next(768);

                                Rectangle temp = new Rectangle(x, y, 136, 76);
                                if (tank.Value.Tankrect.Intersects(temp))
                                {
                                    break;
                                }
                            }
                            outmsg.Write(x);
                            outmsg.Write(y);
                            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                        }
                    }
                    if ((incomingMessage = Server.ReadMessage()) != null) //Ta emot meddelanden och hantering av dessa
                    {
                        switch (incomingMessage.MessageType)
                        {

                            case NetIncomingMessageType.Data:

                                switch (incomingMessage.ReadByte())
                                {

                                    case (byte)PacketTypes.HEARTBEAT:
                                        string name = incomingMessage.ReadString();
                                        tanks[name].LastBeat = DateTime.Now;
                                        Debug.WriteLine("Sv-HeartBeat respons for " + name);
                                        break;

                                    case (byte)PacketTypes.MOVE: //Kolla om en tank rör sig
                                        //Spara värden Server fick från client
                                        name = incomingMessage.ReadString();
                                        float x = incomingMessage.ReadFloat();
                                        float y = incomingMessage.ReadFloat();
                                        float angle = incomingMessage.ReadFloat();
                                        NetOutgoingMessage outmsg = Server.CreateMessage();

                                        foreach (KeyValuePair<string, Tank> tank in tanks)
                                        {
                                            if (tank.Key == name)
                                            {
                                                tank.Value.Tankrect = new Rectangle((int) x, (int) y, tankWidth, tankHeight);
                                                tank.Value.Angle = angle;
                                                tank.Value.Position = new Vector2(x, y);
                                            }
                                        }

                                        // kollision här tack
                                        if (!Collision())
                                        {
                                            outmsg.Write((byte)PacketTypes.MOVE);
                                            outmsg.Write(name);
                                            outmsg.Write(angle);
                                            outmsg.Write(x);
                                            outmsg.Write(y);
                                            outmsg.Write(bulletCollision());
                                            if (bulletCollision() == true) //Kolla om en tank blev träffad och då skicka positionen om vad som hände
                                            {
                                                outmsg.Write(explosionPosition.X);
                                                outmsg.Write(explosionPosition.Y);
                                            }

                                            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                                        }

                                        //Skicka alla värden till alla Clients
                                      
                                        
                                        break;

                                    case (byte)PacketTypes.SHOOT: //INformation om en tank sköt
                                        name = incomingMessage.ReadString();
                                        x = incomingMessage.ReadInt32();
                                        y = incomingMessage.ReadInt32();
                                        angle = incomingMessage.ReadFloat();

                                        bullets.Add(new bullet((int) x,(int) y, angle, name)); //Skapa bullet

                                        NetOutgoingMessage outmsg2 = Server.CreateMessage();
                                        outmsg2.Write((byte)PacketTypes.SHOOT);
                                        outmsg2.Write(name);
                                        outmsg2.Write(x);
                                        outmsg2.Write(y);
                                        Server.SendToAll(outmsg2, NetDeliveryMethod.ReliableOrdered);
                                        break;
                                }
                                break;
                        }
                    }
                    NetOutgoingMessage outtmsg = Server.CreateMessage();
                    outtmsg.Write((byte)PacketTypes.FINALSCOREBOARD);
                    outtmsg.Write(tanks.Count);
                    foreach (KeyValuePair<string, Tank> tank in tanks)
                    {
                        outtmsg.Write(tank.Value.Name);
                        outtmsg.Write(tank.Value.Kills);
                        outtmsg.Write(tank.Value.Deaths);
                        outtmsg.Write(tank.Value.TankColor.R);
                        outtmsg.Write(tank.Value.TankColor.G);
                        outtmsg.Write(tank.Value.TankColor.B);
                    }
                    Server.SendToAll(outtmsg, NetDeliveryMethod.ReliableOrdered);
                    Debug.WriteLine("Sv-Sending final scoreboard from ingame");
                }

                //Spelet slutar och clienten hamnar på Scoreboard
                if (gameState == GameStates.Scoreboard)
                {
                    NetOutgoingMessage outmsg = Server.CreateMessage();
                    outmsg.Write((byte)PacketTypes.FINALSCOREBOARD);
                    outmsg.Write(tanks.Count);
                    foreach (KeyValuePair<string, Tank> tank in tanks)
                    {
                        outmsg.Write(tank.Value.Name);
                        outmsg.Write(tank.Value.Kills);
                        outmsg.Write(tank.Value.Deaths);
                        outmsg.Write(tank.Value.TankColor.R);
                        outmsg.Write(tank.Value.TankColor.G);
                        outmsg.Write(tank.Value.TankColor.B);
                    }
                    Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                    Debug.WriteLine("Sv-Sending final scoreboard");
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
                Vector2 bulletPos = new Vector2(bullet.XPos, bullet.YPos);
                float x = (float)Math.Cos((double)(bullet.Angle));
                float y = (float)Math.Sin((double)(bullet.Angle));

                Vector2 velocity = new Vector2(x * 5, y * 5);
                bulletPos += velocity;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }



        private bool Collision() //Kollision av Tanks
        {
            foreach (KeyValuePair<string, Tank> tank1 in tanks)
            {
                foreach (KeyValuePair<string, Tank> tank2 in tanks)
                {
                    if (tank1.Key != tank2.Key)
                    {
                        if (tank1.Value.Tankrect.Intersects(tank2.Value.Tankrect)) //Själva kollisionen
                        {
                            
                            Vector2 collisionPosition1 = new Vector2(); //Ändra positionen av Tank1
                            collisionPosition1.X = tank1.Value.Position.X + ((float)Math.Cos(tank1.Value.Angle + Math.PI));
                            collisionPosition1.Y = tank1.Value.Position.Y + ((float)Math.Sin(tank1.Value.Angle + Math.PI));

                            NetOutgoingMessage outmsg = Server.CreateMessage(); //Skapa meddelande till Client för ändring av Tank1
                            outmsg.Write((byte)PacketTypes.MOVE);
                            outmsg.Write(tank1.Key);
                            outmsg.Write(tank1.Value.Angle);
                            outmsg.Write(tank1.Value.Position.X);
                            outmsg.Write(tank1.Value.Position.Y);
                            outmsg.Write(bulletCollision());
                            if (bulletCollision() == true) //Kolla om en tank blev träffad och då skicka positionen om vad som hände
                            {
                                outmsg.Write(explosionPosition.X);
                                outmsg.Write(explosionPosition.Y);
                            }

                            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);

                            Vector2 collisionPosition2 = new Vector2(); //Ändra position av Tank 2
                            collisionPosition2.X = tank2.Value.Position.X + ((float)Math.Cos(tank1.Value.Angle));
                            collisionPosition2.Y = tank2.Value.Position.Y + ((float)Math.Sin(tank1.Value.Angle));

                            outmsg = Server.CreateMessage(); //Skapa meddelande till Client för ändring av Tank2
                            outmsg.Write((byte)PacketTypes.MOVE);
                            outmsg.Write(tank2.Key);
                            outmsg.Write(tank2.Value.Angle);
                            outmsg.Write(tank2.Value.Position.X);
                            outmsg.Write(tank2.Value.Position.Y);
                            outmsg.Write(bulletCollision());
                            if (bulletCollision() == true) //Kolla om en tank blev träffad och då skicka positionen om vad som hände
                            {
                                outmsg.Write(explosionPosition.X);
                                outmsg.Write(explosionPosition.Y);
                            }

                            Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                            return true;
                        }
                    }
                }
            }
            return false;
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

                            tanks[bullets[i].Name].Kills++;
                            tank.Value.Deaths++;

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