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
        Texture2D smoketx, backGlow;
        //Rectangle f1, f2, r1, r2;
        Video video;
        VideoPlayer player;
        Texture2D videoTexture;

        public LobbyBackground(ContentManager content)
        {
            //smoketx = content.Load<Texture2D>("images/smokee");
            backGlow = content.Load<Texture2D>("images/backGlow");

            //f1 = new Rectangle(0, 0, Game1.ScreenRec.Width, Game1.ScreenRec.Height);
            //f2 = new Rectangle(-Game1.ScreenRec.Width, 0, Game1.ScreenRec.Width, Game1.ScreenRec.Height);
            //r1 = new Rectangle(0, 0, Game1.ScreenRec.Width, Game1.ScreenRec.Height);
            //r2 = new Rectangle(Game1.ScreenRec.Width, 0, Game1.ScreenRec.Width, Game1.ScreenRec.Height);

            video = content.Load<Video>("smoke_1_1");
            player = new VideoPlayer();
        }

        public void Update()
        {
            //if (f1.X > Game1.ScreenRec.Width)
            //    f1.X = -Game1.ScreenRec.Width;
            //if (f2.X > Game1.ScreenRec.Width)
            //    f2.X = -Game1.ScreenRec.Width;

            //if (r1.X < -Game1.ScreenRec.Width)
            //    r1.X = Game1.ScreenRec.Width;
            //if (r2.X < -Game1.ScreenRec.Width)
            //    r2.X = Game1.ScreenRec.Width;

            //f1.X += 1;
            //f2.X += 1;
            //r1.X -= 1;
            //r2.X -= 1;


            if (player.State == MediaState.Stopped)
            {
                player.IsLooped = true;
                player.Play(video);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(smoketx, f1, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
            //spriteBatch.Draw(smoketx, f2, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
            //spriteBatch.Draw(smoketx, r1, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);
            //spriteBatch.Draw(smoketx, r2, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);

            // Only call GetTexture if a video is playing or paused
            if (player.State != MediaState.Stopped)
                videoTexture = player.GetTexture();
            // Draw the video, if we have a texture to draw.
            if (videoTexture != null)
            {
                spriteBatch.Draw(videoTexture, new Rectangle(0, 0, Game1.ScreenRec.Width, Game1.ScreenRec.Height + 2), Color.White);
            }

            spriteBatch.Draw(backGlow, Game1.ScreenRec, Color.White);
        }
    }
}
