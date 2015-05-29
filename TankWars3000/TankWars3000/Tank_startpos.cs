﻿using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class Tank_startpos
    {
        public void Update(NetIncomingMessage incmsg, List<Tank> tanks)
        {
                if ((incmsg = Game1.Client.ReadMessage()) != null)
                {
                    if (incmsg.ReadByte() == (byte)PacketTypes.STARTPOS)
                    {
                        for (int i = 0; i < tanks.Count; i++ )
                        {
                            if (tanks[i].Name == incmsg.ReadString())
                            {
                                tanks[i].Name = incmsg.ReadString();
                                tanks[i].Angle = incmsg.ReadFloat();
                                tanks[i].Position = new Vector2(incmsg.ReadInt32(), incmsg.ReadInt32());
                            }
                            
                        }           
                 }
             }
        }
    }
}
