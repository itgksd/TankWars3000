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
                    // s� att Ingame bara skickar startpos en g�ng
                    sendStartPos = true;
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

                                    // godk�nner klienten, detta m�ste tydligen g�ras
                                    incomingMessage.SenderConnection.Approve();
                                    connectionAmount++;

                                    // skapa ny Tank och l�gg det i en lista
                                    tanks.Add(name, new Tank(name));

                                    //Debug.WriteLine("Sv-Sending test message");
                                    //// Skicka ett test paket f�r att l�ta klienten veta att anslutningen fungerar.. "Ni" kan nog ta bort den senare om den inte beh�vs l�ngre
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
                                        // �ndra f�rgen p� tanken

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
                    if (tanks.Count >= 1 && (float)Decimal.Divide(readyCount, tanks.Count) > 0.7f)
                    {
                        gameState = GameStates.Ingame; //Spelet l�mnar lobby och startar
                        Debug.WriteLine("Sv-Sending ingame message");
                        NetOutgoingMessage outmsg = Server.CreateMessage();
                        outmsg.Write((byte)PacketTypes.GAMESTATE);
                        outmsg.Write((byte)GameStates.Ingame);
                        Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                    }
                }

                //Spelet l�mnar lobby och startar
                if (gameState == GameStates.Ingame) 
                {
                    if (sendStartPos) //Skicka startpositioner
                    {
                        int y;
                        int x;
                        double degSum = (2 * Math.PI) / tanks.Count;
                        double deg = 0;

                        foreach (KeyValuePair<string, Tank> tank in tanks)
                        {

                            NetOutgoingMessage outmsg = Server.CreateMessage();
                            outmsg.Write((byte)PacketTypes.STARTPOS);
                            outmsg.Write(tank.Value.Name);


                            x = (int)((Math.Cos(deg)) * 350) + (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2);
                            y = (int)((Math.Sin(deg)) * 350) + (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2);
                            deg += degSum;

                            float angle = (float)(deg + Math.PI);

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
                    
                    // kolla om n�gon �r d�d
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

                                    case (byte)PacketTypes.MOVE: //Kolla om en tank r�r sig
                                        //Spara v�rden Server fick fr�n client
                                        name = incomingMessage.ReadString();
                                        int x = incomingMessage.ReadInt32();
                                        int y = incomingMessage.ReadInt32();
                                        float angle = incomingMessage.ReadFloat();

                                        Collision(angle);// kollisions hantering

                                        //Skicka alla v�rden till alla Clients
                                        NetOutgoingMessage outmsg = Server.CreateMessage();
                                        outmsg.Write((byte)PacketTypes.MOVE);
                                        outmsg.Write(name);
                                        outmsg.Write(angle);
                                        outmsg.Write(x);
                                        outmsg.Write(y);
                                        outmsg.Write(bulletCollision());
                                        if (bulletCollision() == true) //Kolla om en tank blev tr�ffad och d� skicka positionen om vad som h�nde
                                        {
                                            outmsg.Write(explosionPosition.X);
                                            outmsg.Write(explosionPosition.Y);
                                        }

                                        Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                                        break;

                                    case (byte)PacketTypes.SHOOT: //INformation om en tank sk�t
                                        name = incomingMessage.ReadString();
                                        x = incomingMessage.ReadInt32();
                                        y = incomingMessage.ReadInt32();
                                        angle = incomingMessage.ReadFloat();

                                        bullets.Add(new bullet(x, y, angle, name)); //Skapa bullet

                                        outmsg = Server.CreateMessage();
                                        outmsg.Write((byte)PacketTypes.SHOOT);
                                        outmsg.Write(name);
                                        outmsg.Write(x);
                                        outmsg.Write(y);
                                        Server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
                                        break;
                                }
                                break;
                        }

                    }
                }

                //Spelet slutar och clienten hamnar p� Scoreboard
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



        private void Collision(float angle) //Kollision av Tanks
        {
            foreach (KeyValuePair<string, Tank> tank1 in tanks)
            {
                foreach (KeyValuePair<string, Tank> tank2 in tanks)
                {
                    if (tank1.Key != tank2.Key)
                    {
                        if (tank1.Value.Tankrect.Intersects(tank2.Value.Tankrect)) //Sj�lva kollisionen
                        {
                            Vector2 collisionPosition1 = new Vector2();
                            collisionPosition1.X = tank1.Value.Position.X + ((float)Math.Cos(angle + Math.PI));
                            collisionPosition1.Y = tank1.Value.Position.Y + ((float)Math.Sin(angle + Math.PI));

                            tank1.Value.Position = collisionPosition1; // �ndra positionen av tank1 

                            Vector2 collisionPosition2 = new Vector2();
                            collisionPosition2.X = tank2.Value.Position.X + ((float)Math.Cos(angle));
                            collisionPosition2.Y = tank2.Value.Position.Y + ((float)Math.Sin(angle));

                            tank2.Value.Position = collisionPosition2; // �ndra positionen av tank2

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
                    if (bullets[i].Name != tank.Value.Name) // Tank kan inte tr�ffa sig sj�lv!
                    {
                        if (bullets[i].Rect.Intersects(tank.Value.Tankrect)) // Sj�lva kollisionen
                        {

                            tank.Value.Health--; // Tank blir skadad!

                            explosionPosition = tank.Value.Position; // Best�mma explosions-positionen �t client, v�rdet skickas senare genom MOVE-Package

                            Vector2 bulletcollisionposition = new Vector2();
                            bulletcollisionposition.X = tank.Value.Position.X + ((float)Math.Cos(bullets[i].Angle));
                            bulletcollisionposition.Y = tank.Value.Position.Y + ((float)Math.Sin(bullets[i].Angle));
                            tank.Value.Position = bulletcollisionposition; //Tank blir flyttad av kollisionen med bullet

                            bullets.RemoveAt(i); // Bullet "f�rst�rs" vid kollisionen
                            return true; // Att kollisionen h�nde skickas till client och skickar d� explosionposition
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