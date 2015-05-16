using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using System.Media;

namespace TankWars3000
{
    class LobbyBackground
    {
        Texture2D smoketx, backGlow, leftGlow;
        int leftGlowX;
        //Rectangle f1, f2, r1, r2;
        Video video;
        VideoPlayer player;
        Texture2D videoTexture;

        Song song;

        public LobbyBackground(ContentManager content)
        {
            backGlow = content.Load<Texture2D>("images/backGlow");
            leftGlow = content.Load<Texture2D>("images/leftGlow");
            leftGlowX = -leftGlow.Width + 560;

            video = content.Load<Video>("smoke_1_1");
            player = new VideoPlayer();

            song = content.Load<Song>("Sound/menuMusic");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
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
                spriteBatch.Draw(videoTexture, new Rectangle(0, 0, Game1.ScreenRec.Width, Game1.ScreenRec.Height + 2), Color.White);
            }

            spriteBatch.Draw(leftGlow, new Rectangle(leftGlowX, 0, leftGlow.Width, Game1.ScreenRec.Height), Color.White);
            spriteBatch.Draw(backGlow, Game1.ScreenRec, Color.White);
        }
    }
}
