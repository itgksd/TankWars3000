using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class TankTrack
    {
        Texture2D texture;

        Vector2 position;

        float alpha;

        public void Update()
        {
            alpha -= 0.001f;
        }

        public TankTrack(ContentManager content)
        {
            texture = content.Load<Texture2D>("Tank/Trail");
        }
    }
}
