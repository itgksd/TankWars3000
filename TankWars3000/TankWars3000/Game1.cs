using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;

namespace TankWars3000
{
    public enum GameStates
    {
        Lobby,
        Ingame,
        Scoreboard
    }

    enum PacketTypes
    {
        LOGIN,
        READY,
        MOVE,
        SHOOT,
        TEST,
        LOBBYPLAYERLIST,
        COLOR,
        GAMESTATE
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static GameStates gameState;

        // Client Object
        public static NetClient Client;

        // The Player
        Tank tank;
        List<Tank> tanks = new List<Tank>();

        OldNewInput input = new OldNewInput();

        Lobby lobby;

        static Rectangle screenRec;
        static public Rectangle ScreenRec
        {
            get { return screenRec; }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            tank = new Tank(Content);
            gameState = GameStates.Lobby;

            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            IsMouseVisible = true;

            screenRec = new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            lobby = new Lobby(Content);
            Notify.LoadContent(Content);
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            //// Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            input.newKey = Keyboard.GetState();
            input.newMouse = Mouse.GetState();

            if (gameState == GameStates.Lobby)
            {
                lobby.Update(input);
            }
            if (gameState == GameStates.Ingame)
            {
                // The player
                    tank.Update(Content, graphics, tanks);
                    tank.Input(input, Content);
            }
            if (gameState == GameStates.Scoreboard)
            {

            }

            Notify.Update(gameTime);

            input.SetOldKey();
            input.SetOldMouse();

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

             if (gameState == GameStates.Lobby)
                {
                    spriteBatch.Begin();
                    lobby.Draw(spriteBatch);
                    Notify.Draw(spriteBatch);
                    spriteBatch.End();
                }
                if (gameState == GameStates.Ingame)
                {
                    spriteBatch.Begin();

                    tank.Draw(spriteBatch, tanks);
                    Notify.Draw(spriteBatch);

                    spriteBatch.End();
                }
                if (gameState == GameStates.Scoreboard)
                {
                    spriteBatch.Begin();


                    Notify.Draw(spriteBatch);

                    spriteBatch.End();
                }
            base.Draw(gameTime);
        }
    }
}
