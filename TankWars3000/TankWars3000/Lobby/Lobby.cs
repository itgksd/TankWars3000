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

		// Setting Buttons
		NormalButton muteMusicBtn;
		NormalButton fullScreenBtn;

		List<PlayerListItem> playerList = new List<PlayerListItem>();

		DateTime lastBeat = DateTime.MaxValue;

		bool connectionConfirmed = false;

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

			muteMusicBtn = new NormalButton(content, new Vector2(50, Game1.ScreenRec.Height - 100), "Mute Music", MuteMusic, true);
			muteMusicBtn.TitleColor = Color.LightGray;
			fullScreenBtn = new NormalButton(content, new Vector2(255 + 50, Game1.ScreenRec.Height - 100), "Fullscreen", Fullscreen, true);
			fullScreenBtn.TitleColor = Color.LightGray;

			this.content = content;
		}

		public void Connect()
		{
			Notify.NewMessage("Connecting...", Color.LightBlue);

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

			connected = true;

			lastBeat = DateTime.Now;
		}

		public void Disconnect()
		{
			// Send a message that we are leaving the server
			NetOutgoingMessage outmsg = Game1.Client.CreateMessage();
			outmsg.Write((byte)PacketTypes.DISCONNECT); // Tell the server that we want to disconnect
			outmsg.Write(nameBtn.Text); // Tell the server who this came from, this can be massively abused by hackers xD
			Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);

			// Disconnect
			Game1.Client.Disconnect("Disconnect.By.User");

			// Reset some stuff
			nameBtn.Enabled = true;
			ipBtn.Enabled = true;
			readyBtn.Enabled = false;
			disconnectBtn.Enabled = false;
			connectBtn.Enabled = true;
			colorBtn.Enabled = false;

			connected = false;

			lastBeat = DateTime.MaxValue;
			connectionConfirmed = false;

			playerList.Clear();

			Notify.NewMessage("Disconnected!", Color.Orange);
		}

		public void ReadyChanged()
		{
			// Send ready to server
			//Debug.WriteLine("Cl-Sending ready");

			NetOutgoingMessage outmsg = Game1.Client.CreateMessage();

			outmsg.Write((byte)PacketTypes.READY); // Ready type
			outmsg.Write(nameBtn.Text); // Name
			outmsg.Write(readyBtn.IsTrue); // Ready status from button

			Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered); // Send
		}

		public void ColorChange()
		{
			// Send the new color
			//Debug.WriteLine("Cl-Sending color");

			NetOutgoingMessage outmsg = Game1.Client.CreateMessage();

			outmsg.Write((byte)PacketTypes.COLOR); // Tell the server to expect color data

			outmsg.Write(nameBtn.Text); // Name

			// Most convenient way of sending colors :P
			outmsg.Write(colorBtn.SelectedColor.R); // Color R
			outmsg.Write(colorBtn.SelectedColor.G); // Color G
			outmsg.Write(colorBtn.SelectedColor.B); // Color B

			Game1.Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered); // Send
		}

		public void Exit()
		{
			Disconnect();
			Environment.Exit(9);
		}

		public void MuteMusic()
		{
			background.PlayMusic = background.PlayMusic ? false : true;
			Notify.NewMessage("Music is now turned " + (background.PlayMusic ? "on" : "off"), Color.Black);
		}

		public void Fullscreen()
		{
			Game1.Fullscreen = Game1.Fullscreen ? false : true;
		}

		public void Update(OldNewInput input, List<Tank> tanks)
		{
			background.Update();

			ipBtn.Update(input); // Orkar inte bry mig om polyformism..
			nameBtn.Update(input);
			colorBtn.Update(input);
			readyBtn.Update(input);
			connectBtn.Update(input);
			disconnectBtn.Update(input);
			exitBtn.Update(input);
			muteMusicBtn.Update(input);
			fullScreenBtn.Update(input);
			playerList.ForEach(p => p.Update());

			NetIncomingMessage incom; // MEssage that will contain the message comming from the server
			if (connected && (incom = Game1.Client.ReadMessage()) != null) // Are there any new messanges?
			{
				if (incom.MessageType == NetIncomingMessageType.Data) // Is it a "data" message?
				{
					switch (incom.ReadByte())
					{
						case (byte)PacketTypes.LOBBYPLAYERLIST: // Handle a list of players (right side)
							Debug.WriteLine("Cl-Received the playerlist");

							bool animate = playerList.Count == 0 ? true : false; // Animate if the playerlist is new and empty
							if (playerList.Count == 0 || playerList[0].Statee == PlayerListItem.State.NONE) // To keep the game from removing the initial animation
							{
								playerList.Clear(); // Clear the "old" data

								int incommingPlayers = incom.ReadInt32();
								for (int k = 1; k <= incommingPlayers; k++)
								{
									string name = incom.ReadString();
									Color color = new Color(incom.ReadByte(), incom.ReadByte(), incom.ReadByte());
									bool ready = incom.ReadBoolean();

									playerList.Add(new PlayerListItem(content, new Vector2(Game1.ScreenRec.Width - 450, k * 50), name, color, ready, animate));
								}
								for (int i = incommingPlayers + 1; i <= 8; i++)
								{
									playerList.Add(new PlayerListItem(content, new Vector2(Game1.ScreenRec.Width - 450, i * 50), animate));
								}
							}

							ConfirmConnection();
							break;

						case (byte)PacketTypes.GAMESTATE:
							Debug.WriteLine("Cl-Reveiced gamestate change");
							background.PlayMusic = false;
							Notify.NewMessage("Starting Game!", Color.LightBlue);
							Game1.gameState = (GameStates)incom.ReadByte();
                            Game1.tankname = nameBtn.Text;

							// Skapa alla tank klasserna
							foreach (PlayerListItem player in playerList)
							{
								if (player.Name != "")
									tanks.Add(new Tank(content, player.Name, player.TankColor));
							}

							ConfirmConnection();
							break;

						case (byte)PacketTypes.HEARTBEAT:
							// Respond to the Heartbeat request of the server
							Debug.WriteLine("Cl-Received heartbeat, responding");
							NetOutgoingMessage outmsg = Game1.Client.CreateMessage();
							outmsg.Write((byte)PacketTypes.HEARTBEAT);
							outmsg.Write(nameBtn.Text);
							Game1.Client.SendMessage(outmsg, incom.SenderConnection, NetDeliveryMethod.ReliableOrdered);
							ConfirmConnection();
							break;

						case (byte)PacketTypes.DISCONNECTREASON:
							Debug.WriteLine("Cl-Deny packet received");
							Notify.NewMessage("Disconnect reason: " + incom.ReadString(), Color.Purple);
							Disconnect();
							break;

						default:
							break;
					}

					lastBeat = DateTime.Now; // Se alla anslutningar som en heartbeat
				}
			}

			// HeartBeat
			TimeSpan timeSinceLastBeat = DateTime.Now.Subtract(lastBeat);
			if (connected && timeSinceLastBeat.TotalSeconds > 10)
			{
				Notify.NewMessage("Connection lost", Color.Red);
				Disconnect();
			}
		}

		public void ConfirmConnection()
		{
			if (!connectionConfirmed)
			{
				connectionConfirmed = true;
				Notify.NewMessage("Connected!", Color.Lime);
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
			muteMusicBtn.Draw(spriteBatch);
			fullScreenBtn.Draw(spriteBatch);

			playerList.ForEach(p => p.Draw(spriteBatch));
		}
	}
}
