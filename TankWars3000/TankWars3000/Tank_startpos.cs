using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class Tank_startpos
    {
        string incmsg_name;
        public void Update(NetIncomingMessage incmsg, List<Tank> tanks)
        {
                if ((incmsg = Game1.Client.ReadMessage()) != null)
                {
                    if (incmsg.ReadByte() == (byte)PacketTypes.STARTPOS)
                    {
                        incmsg_name =incmsg.ReadString();
                        for (int i = 0; i < tanks.Count; i++ )
                        {
                            if (tanks[i].Name == incmsg_name)
                            {
                                tanks[i].Angle = incmsg.ReadFloat();
                                tanks[i].Position = new Vector2(incmsg.ReadInt32(), incmsg.ReadInt32());
                            }
                            
                        }           
                 }
             }
        }
    }
}
