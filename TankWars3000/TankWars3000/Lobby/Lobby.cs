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
        BoolButton boolBtn;

        public Lobby(ContentManager content)
        {
            background = new LobbyBackground(content);

            ipBtn = new TextButton(content, new Vector2(100, 100), "IP", TextButtonType.IP, "192.168.10.10");
            nameBtn = new TextButton(content, new Vector2(100, 160), "UserName", TextButtonType.UserName, "Player");
            colorBtn = new ColorButton(content, new Vector2(100, 220), "Tank Color");
            boolBtn = new BoolButton(content, new Vector2(100, 280), "Ready?", false);
        }

        public void Update(OldNewInput input)
        {
            background.Update();

            ipBtn.Update(input);
            nameBtn.Update(input);
            colorBtn.Update(input);
            boolBtn.Update(input);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            background.Draw(spriteBatch);

            ipBtn.Draw(spriteBatch);
            nameBtn.Draw(spriteBatch);
            colorBtn.Draw(spriteBatch);
            boolBtn.Draw(spriteBatch);
        }
    }
}
