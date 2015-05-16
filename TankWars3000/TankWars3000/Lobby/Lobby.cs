using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWars3000
{
    class Lobby
    {
        LobbyBackground background;

        TextButton ipBtn, nameBtn;
        ColorButton colorBtn;
        BoolButton readyBtn;
        NormalButton exitBtn;

        public Lobby(ContentManager content)
        {
            background = new LobbyBackground(content);

            ipBtn    = new TextButton  (content, new Vector2(50, 50),  "IP",       TextButtonType.IP,       "192.168.10.10");
            nameBtn  = new TextButton  (content, new Vector2(50, 110), "UserName", TextButtonType.UserName, "Player");
            colorBtn = new ColorButton (content, new Vector2(50, 170), "Tank Color");
            readyBtn = new BoolButton  (content, new Vector2(50, 230), "Ready?", false);
            exitBtn  = new NormalButton(content, new Vector2(50, 290), "Exit");
            exitBtn.TitleColor = Color.Red;
        }

        public void Update(OldNewInput input)
        {
            background.Update();

            ipBtn.Update(input);
            nameBtn.Update(input);
            colorBtn.Update(input);
            readyBtn.Update(input);
            exitBtn.Update(input);

            if (exitBtn.Pressed) // If somebody pressed the exit button
                Environment.Exit(0);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            background.Draw(spriteBatch);

            ipBtn.Draw(spriteBatch);
            nameBtn.Draw(spriteBatch);
            colorBtn.Draw(spriteBatch);
            readyBtn.Draw(spriteBatch);
            exitBtn.Draw(spriteBatch);
        }
    }
}
