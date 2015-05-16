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
        List<BaseButton> buttons;

        public Lobby(ContentManager content)
        {
            background = new LobbyBackground(content);

            buttons = new List<BaseButton>();
            buttons.Add(new TextButton(content, new Vector2(100, 100), "IP", TextButtonType.IP, "192.168.10.10"));
            buttons.Add(new TextButton(content, new Vector2(100, 160), "UserName", TextButtonType.UserName, "Player"));
        }

        public void Update(OldNewInput input)
        {
            background.Update();

            buttons.ForEach(b => b.Update(input));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            background.Draw(spriteBatch);

            buttons.ForEach(b => b.Draw(spriteBatch));
        }
    }
}
