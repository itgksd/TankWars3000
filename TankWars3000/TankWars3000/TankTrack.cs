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
        Texture2D    texture;

        Vector2      position;

        float        angle;

        float        alpha;
        public float Alpha
        {
            get { return alpha; }
        }

        public void Update()
        {
            alpha -= 0.001f;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }

        public TankTrack(ContentManager content, Vector2 position, float angle)
        {
            texture       = content.Load<Texture2D>("Tank/Trail");
            this.position = position;
            this.angle    = angle;

        }
    }
}
