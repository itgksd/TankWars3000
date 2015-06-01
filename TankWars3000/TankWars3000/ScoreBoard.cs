using Lidgren.Network;
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
        LobbyBackground background;
        NormalButton toLobbyBtn;

        public ScoreBoard(ContentManager content)
        {
            this.content = content;
            scoreBoardItems = new List<ScoreBoardItem>();
            background = new LobbyBackground(content);
            background.LeftGlowX = 800 - background.LeftGlowWidth;

            toLobbyBtn = new NormalButton(content, new Vector2(50, Game1.ScreenRec.Height - 100), "Return to lobby", ReturnToLobby, true);
        }

        public void ReturnToLobby()
        {
            Game1.gameState = GameStates.Lobby;
        }

        public void Update(OldNewInput input)
        {
            background.Update();
            toLobbyBtn.Update(input);

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
                            
                            
                            scoreBoardItems.Clear();
                            
                            Debug.WriteLine("Cl-Recevied final scoreboard");
                            
                            int count = incom.ReadInt32();
                            List<TempPlayerContainer> tmpPCs = new List<TempPlayerContainer>();
                            for (int i = 0; i < count; i++)
                            {
                                //
                                TempPlayerContainer tmpPC = new TempPlayerContainer();
                                tmpPC.name   = incom.ReadString();
                                tmpPC.kills  = incom.ReadInt32();
                                tmpPC.deaths = incom.ReadInt32();
                                tmpPC.color  = new Color(incom.ReadByte(), incom.ReadByte(), incom.ReadByte());
                                tmpPC.score  = (tmpPC.kills * 2) - tmpPC.deaths;
                                tmpPCs.Add(tmpPC);
                            }

                            // Sortera
                            tmpPCs = tmpPCs.OrderBy(o => o.score).ToList();

                            // Skapa listan som kommer ritas ut
                            for (int i = 0; i < count; i++)
                            {
                                Vector2 pos;
                                if (scoreBoardItems.Count > 0)
                                    pos = new Vector2(50, scoreBoardItems[scoreBoardItems.Count - 1].Position.Y + scoreBoardItems[scoreBoardItems.Count - 1].Height + 10);
                                else
                                    pos = new Vector2(50, 50);

                                scoreBoardItems.Add(new ScoreBoardItem(content, pos, tmpPCs[i].kills, tmpPCs[i].deaths, i + 1, tmpPCs[i].score, tmpPCs[i].name, tmpPCs[i].color));
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
            background.Draw(spriteBatch);
            toLobbyBtn.Draw(spriteBatch);

            scoreBoardItems.ForEach(s => s.Draw(spriteBatch));
        }

        class TempPlayerContainer
        {
            public string name;
            public int kills, deaths, score;
            public Color color;
        }
    }
}
