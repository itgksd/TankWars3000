﻿using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class ScoreBoard
    {
        List<ScoreBoardItem> scoreBoardItems;
        ContentManager content;

        public ScoreBoard(ContentManager content)
        {
            this.content = content;
            scoreBoardItems = new List<ScoreBoardItem>();
        }

        public void Update()
        {
            NetIncomingMessage incom; // MEssage that will contain the message comming from the server
            if ((incom = Game1.Client.ReadMessage()) != null) // Are there any new messanges?
            {
                if (incom.MessageType == NetIncomingMessageType.Data) // Is it a "data" message?
                {
                    switch (incom.ReadByte())
                    {
                        case (byte)PacketTypes.FINALSCOREBOARD:
                            // Packet contains:
                            /// PacketType
                            /// Player Amount \/ loop
                            ///   Player Name
                            ///   Player Kills
                            ///   Player Deaths
                            ///   Player Color R
                            ///   Player Color G
                            ///   Player Color B

                            for (int i = 0; i < incom.ReadInt32(); i++)
                            {
                                Vector2 pos;
                                if (scoreBoardItems.Count > 0)
                                    pos = new Vector2(50, scoreBoardItems[scoreBoardItems.Count - 1].Position.Y + scoreBoardItems[scoreBoardItems.Count - 1].Height + 10);
                                else
                                    pos = new Vector2(50, 50);

                                string name = incom.ReadString();
                                int kills   = incom.ReadInt32();
                                int deaths  = incom.ReadInt32();
                                Color color = new Color(incom.ReadByte(), incom.ReadByte(), incom.ReadByte());
                                int score = kills - deaths;

                                scoreBoardItems.Add(new ScoreBoardItem(content, pos, kills, deaths, i, score, name, color));
                            }

                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            scoreBoardItems.ForEach(s => s.Draw(spriteBatch));
        }
    }
}
