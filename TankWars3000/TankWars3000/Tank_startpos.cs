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
        bool startposbool = true;
        int count = 0;

        public bool Startposbool
        {
            get { return startposbool; }
            set { startposbool = value; }
        }

        public void Update(NetIncomingMessage incmsg, Dictionary<string, Tank> tanks)
        {
            while (startposbool)
            {
                if ((incmsg = Game1.Client.ReadMessage()) != null)
                {
                        if (incmsg.ReadByte() == (byte)PacketTypes.STARTPOS)
                        {
                            incmsg_name = incmsg.ReadString();
                            tanks[incmsg_name].Angle = incmsg.ReadFloat();
                            tanks[incmsg_name].Position = new Vector2(incmsg.ReadFloat(), incmsg.ReadFloat());
                            count++;
                        }
                 }
                //get out of the loop once every tank has recieved their startpos
                if(count == tanks.Count)
                    startposbool = false;
             }
        }
    }
}
