using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class Lobby
    {
        LobbyBackground background;

        public Lobby(ContentManager content)
        {
            background = new LobbyBackground(content);
        }

        public void Update()
        {
            background.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //background.Draw();
        }
    }
}
