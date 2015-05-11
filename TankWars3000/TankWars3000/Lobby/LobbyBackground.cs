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
        Video video;
        VideoPlayer player;
        Texture2D videoTexture;

        public LobbyBackground(ContentManager content)
        {
            video = content.Load<Video>("video/smoke.wmv");
            player = new VideoPlayer();
        }

        public void Update()
        {
            if (player.State == MediaState.Stopped)
            {
                player.IsLooped = true;
                player.Play(video);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Only call GetTexture if a video is playing or paused
            if (player.State != MediaState.Stopped)
                videoTexture = player.GetTexture();

            // Draw the video, if we have a texture to draw.
            if (videoTexture != null)
            {
                spriteBatch.Draw(videoTexture, Game1.ScreenRec, Color.White);
            }
        }
    }
}
