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
        NormalButton disconnectBtn;

        public Lobby(ContentManager content)
        {
            background = new LobbyBackground(content);

            nameBtn  = new TextButton  (content, new Vector2(50, 50),  "UserName", TextButtonType.UserName, "Player");
            ipBtn    = new TextButton  (content, new Vector2(50, 110), "IP",       TextButtonType.IP,       "192.168.10.10", Connect);
            colorBtn = new ColorButton (content, new Vector2(50, 170), "Tank Color");
            readyBtn = new BoolButton  (content, new Vector2(50, 230), "Ready?", false);
            disconnectBtn  = new NormalButton(content, new Vector2(50, 290), "Disconnect", Disconnect);
            disconnectBtn.TitleColor = Color.Orange;
            exitBtn  = new NormalButton(content, new Vector2(50, 350), "Exit", Exit);
            exitBtn.TitleColor = Color.Red;
            
        }

        public void Connect()
        {
            nameBtn.Enabled = false;
            // Connect to server
        }

        public void Disconnect()
        {
            // Disconnect code
            nameBtn.Enabled = true;
        }

        public void Exit()
        {
            Environment.Exit(9);
        }

        public void Update(OldNewInput input)
        {
            background.Update();

            ipBtn.Update(input);
            nameBtn.Update(input);
            colorBtn.Update(input);
            readyBtn.Update(input);
            disconnectBtn.Update(input);
            exitBtn.Update(input);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            background.Draw(spriteBatch);

            ipBtn.Draw(spriteBatch);
            nameBtn.Draw(spriteBatch);
            colorBtn.Draw(spriteBatch);
            readyBtn.Draw(spriteBatch);
            disconnectBtn.Draw(spriteBatch);
            exitBtn.Draw(spriteBatch);
        }
    }
}
