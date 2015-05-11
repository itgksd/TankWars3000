using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;

namespace TankWars3000
{
    class LobbyBackground
    {
        Texture2D smoketx;

        public LobbyBackground(ContentManager content)
        {
            smoketx = content.Load<Texture2D>("images/smoke");
        }

        public void Update()
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
        }
    }
}
