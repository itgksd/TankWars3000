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

        Tank tank;

        Lobby lobby;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            tank =new Tank();

            gameState = GameStates.Lobby;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            tank.LoadContent(Content);

            lobby = new Lobby(Content);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            while (true)
            {
                if (gameState == GameStates.Lobby)
                {
                    // lobby code here Dirkjan
                }
                if (gameState == GameStates.Ingame)
                {
                    tank.Update();
                }
                if (gameState == GameStates.Scoreboard)
                {

                }
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

             if (gameState == GameStates.Lobby)
                {
                    spriteBatch.Begin();

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
