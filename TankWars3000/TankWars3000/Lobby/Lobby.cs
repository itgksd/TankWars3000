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
using System.Threading;

namespace TankWars3000
{
    class Lobby
    {
        bool connected = false;

        LobbyBackground background;

        ContentManager content;

        // Buttons
        TextButton ipBtn, nameBtn;
        ColorButton colorBtn;
        BoolButton readyBtn;
        NormalButton exitBtn;
        NormalButton disconnectBtn;
        NormalButton connectBtn;

        List<PlayerListItem> playerList = new List<PlayerListItem>();

        public Lobby(ContentManager content)
        {
            background = new LobbyBackground(content);

            // Buttons
            nameBtn  = new TextButton  (content, new Vector2(50, 50),  "UserName", TextButtonType.UserName, "Player");
            ipBtn    = new TextButton  (content, new Vector2(50, 110), "IP",       TextButtonType.IP,       "127.0.0.1");
            colorBtn = new ColorButton (content, new Vector2(50, 170), "Tank Color", ColorChange);
            colorBtn.Enabled = false;
            readyBtn = new BoolButton  (content, new Vector2(50, 230), "Ready?", false, ReadyChanged);
            readyBtn.Enabled = false;
            connectBtn = new NormalButton(content, new Vector2(50, 290), "Connect", Connect, true);
            connectBtn.TitleColor = Color.Lime;
            disconnectBtn  = new NormalButton(content, new Vector2(255 + 50, 290), "Disconnect", Disconnect, true);
            disconnectBtn.TitleColor = Color.Orange;
            disconnectBtn.Enabled = false;
            exitBtn  = new NormalButton(content, new Vector2(50, 350), "Exit", Exit);
            exitBtn.TitleColor = Color.Red;

            this.content = content;
        }

        public void Connect()
        {
            nameBtn.Enabled = false;
            ipBtn.Enabled = false;
            readyBtn.IsTrue = false;
            readyBtn.Enabled = true;
            disconnectBtn.Enabled = true;
            connectBtn.Enabled = false;
            colorBtn.Enabled = true;
            // Connect to server

            NetPeerConfiguration Config = new NetPeerConfiguration("game");

            Game1.Client = new NetClient(Config);
            NetOutgoingMessage outmsg = Game1.Client.CreateMessage();

            Game1.Client.Start();

            outmsg.Write((byte)PacketTypes.LOGIN);

            outmsg.Write(nameBtn.Text);

            Game1.Client.Connect(ipBtn.Text, 14242, outmsg);

            #region Wait for test connection
            // Wait for aproval.. might freeze the game tho
            bool canStart = false;

            // Message that will contain the aproval msg (or something else)
            NetIncomingMessage incommsg;
            long loopCount = 0; // Used to break if it takes to long

            while (!canStart)
            {
                if ((incommsg = Game1.Client.ReadMessage()) != null)
                {
                    switch (incommsg.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            switch (incommsg.ReadByte())
                            {
                                case (byte)PacketTypes.TEST:
                                    // Read message
                                    string testMsg = incommsg.ReadString();
                                    Debug.WriteLine("Cl-Test message received, connection working");
                                    Notify.NewMessage("Connected!", Color.Lime);
                                    connected = true;
                                    // Add players and stuff
                                    canStart = true;
                                    break;
                                case (byte)PacketTypes.DISCONNECTREASON:
                                    Debug.WriteLine("Cl-Deny packet received");
                                    Notify.NewMessage("Disconnect reason: " + incommsg.ReadString(), Color.Purple);
                                    canStart = true;
                                    Disconnect();
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case NetIncomingMessageType.WarningMessage:
                            string msgW = incommsg.ReadString();
                            Debug.WriteLine("Cl-" + incommsg.MessageType + " - " + msgW);
                            Notify.NewMessage("Warning: " + msgW, Color.Purple);
                            break;
                        default:
                            string msg = incommsg.ReadString();
                            Debug.WriteLine("Cl-" + incommsg.MessageType + " - " + msg);
                            break;
                    }
                }

                loopCount++;
                if (loopCount > 200000000) // Keeps it from freezing alltogether when not being able to connect.. A timeout i guess
                {
                    Debug.WriteLine("Cl-Timeout, can't connect");
                    Notify.NewMessage("Connection failed, timeout :(", Color.Red);
                    Disconnect();
                    break;
                }
            }
            #endregion
        }

        public void Disconnect()
        {
            Game1.Client.Disconnect("Disconnect.By.User");
            // Disconnect code
            nameBtn.Enabled = true;
            ipBtn.Enabled = true;
            readyBtn.Enabled = false;
            disconnectBtn.Enabled = false;
            connectBtn.Enabled = true;
            colorBtn.Enabled = false;

            connected = false;

            playerList.Clear();

            Notify.NewMessage("Disconnected!", Color.Orange);
        }

        public void ReadyChanged()
        {
            // Send ready to server
            //Debug.WriteLine("Cl-Sending ready");

            NetOutgoingMessage outmsg = Game1.Client.CreateMessage();

            outmsg.Write((byte)PacketTypes.READY);

            outmsg.Write(nameBtn.Text);

            outmsg.Write(readyBtn.IsTrue);

            Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
        }

        public void ColorChange()
        {
            // Send the new color
            //Debug.WriteLine("Cl-Sending color");

            NetOutgoingMessage outmsg = Game1.Client.CreateMessage();

            outmsg.Write((byte)PacketTypes.COLOR);

            outmsg.Write(nameBtn.Text); // Name

            outmsg.Write(colorBtn.SelectedColor.R); // Color R
            outmsg.Write(colorBtn.SelectedColor.G); // Color G
            outmsg.Write(colorBtn.SelectedColor.B); // Color B

            Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
        }

        public void Exit()
        {
            Environment.Exit(9);
        }

        public void Update(OldNewInput input)
        {
            background.Update();

            ipBtn.Update(input);
            nameBtn.Update(input);
            colorBtn.Update(input);
            readyBtn.Update(input);
            connectBtn.Update(input);
            disconnectBtn.Update(input);
            exitBtn.Update(input);

            NetIncomingMessage incom;
            if (connected && (incom = Game1.Client.ReadMessage()) != null) // Are there any new messanges?
            {
                if (incom.MessageType == NetIncomingMessageType.Data) // Is it a "data" message?
                {
                    switch (incom.ReadByte())
                    {
                        case (byte)PacketTypes.LOBBYPLAYERLIST:
                            Debug.WriteLine("Cl-Received the playerlist");

                            playerList.Clear(); // Clear the "old" data

                            int incommingPlayers = incom.ReadInt32();
                            for (int k = 1; k <= incommingPlayers; k++)
                            {
                                string name = incom.ReadString();
                                Color color = new Color(incom.ReadByte(), incom.ReadByte(), incom.ReadByte());
                                bool ready = incom.ReadBoolean();

                                playerList.Add(new PlayerListItem(content, new Vector2(Game1.ScreenRec.Width - 350, k * 40), name, color, ready));
                            }
                            for (int i = incommingPlayers + 1; i <= 8; i++)
                            {
                                playerList.Add(new PlayerListItem(content, new Vector2(Game1.ScreenRec.Width - 350, i * 40)));
                            }
                            break;
                        case (byte)PacketTypes.GAMESTATE:
                            Debug.WriteLine("Cl-Reveiced gamestate change");
                            background.PlayMusic = false;
                            Notify.NewMessage("Starting Game!", Color.LightBlue);
                            Game1.gameState = (GameStates)incom.ReadByte();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            background.Draw(spriteBatch);

            ipBtn.Draw(spriteBatch);
            nameBtn.Draw(spriteBatch);
            colorBtn.Draw(spriteBatch);
            readyBtn.Draw(spriteBatch);
            connectBtn.Draw(spriteBatch);
            disconnectBtn.Draw(spriteBatch);
            exitBtn.Draw(spriteBatch);

            playerList.ForEach(p => p.Draw(spriteBatch));
        }
    }
}
