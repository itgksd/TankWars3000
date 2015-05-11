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

namespace TankWars3000
{
    enum GameStates
    {
        Lobby,
        Ingame,
        Scoreboard
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GameStates gameState;

        // The Player
        Tank tank;
        KeyboardState newKey;
        KeyboardState preKey;

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
            // TODO: Add your initialization logic here

            tank =new Tank(Content);

            gameState = GameStates.Lobby;

            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();

            screenRec = new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            lobby = new Lobby(Content);
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            newKey = Keyboard.GetState();

            if (gameState == GameStates.Lobby)
            {
                lobby.Update();
            }
            if (gameState == GameStates.Ingame)
            {
                // The player
                tank.Update(newKey, Content);
                newKey = preKey;
            }
            if (gameState == GameStates.Scoreboard)
            {

            }

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

             if (gameState == GameStates.Lobby)
                {
                    spriteBatch.Begin();
                    lobby.Draw(spriteBatch);
                    spriteBatch.End();
                }
                if (gameState == GameStates.Ingame)
                {
                    spriteBatch.Begin();

                    tank.Draw(spriteBatch);

                    spriteBatch.End();
                }
                if (gameState == GameStates.Scoreboard)
                {
                    spriteBatch.Begin();

                    spriteBatch.End();
                }
            base.Draw(gameTime);
        }
    }
}
