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
        LOGIN,            // <- Used by the lobby to send a connection request
        READY,            // <- Used by the lobby to change the "ready" status
        START,
        MOVE,
        SHOOT,
        LOBBYPLAYERLIST,  // <- Currently used by the lobby to send all the players to all of the clients
        COLOR,            // <- Lobby uses this to send a new tank color
        DEATH,
        GAMESTATE,        // <- Ingame/Lobby/Scoreboard change
        DISCONNECTREASON, // <- Disconnect with reason. e.g. tell the client that the server is full
        DISCONNECT,       // <- Used to disconnect without reason. Used only when a client disconnects itself. Has to include a name!
        HEARTBEAT,        // <- Used to see if the client/server is still alive
        STARTPOS,
        FINALSCOREBOARD
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch           spriteBatch;

        public static GameStates gameState;

        // Client Object
        public static NetClient Client;

        // The Player
        Tank_startpos tank_startpos;
        Tank tank;
        List<Tank> tanks       = new List<Tank>();

        NetIncomingMessage incmsg;

        List<TankTrack> tracks = new List<TankTrack>();

        OldNewInput input      = new OldNewInput();

        Lobby lobby;
        ScoreBoard scoreboard;

        static bool fullscreen = false;
        static public bool Fullscreen
        {
            get { return fullscreen; }
            set { fullscreen = value; }
        }

        static        Rectangle screenRec;
        static public Rectangle ScreenRec
        {
            get { return screenRec; }
        }

        public Game1()
        {
            graphics              = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            tank          = new Tank();
            tank_startpos = new Tank_startpos();

            gameState = GameStates.Lobby;

            //trail = new TankTrack(Content); Emil! Du har inte inkluderat själva klassen i din commit! Kolla i untracked files!

            graphics.PreferredBackBufferWidth  = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            IsMouseVisible                     = true;

            screenRec = new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            lobby = new Lobby(Content);
            scoreboard = new ScoreBoard(Content);
            Notify.LoadContent(Content);
            
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {

            input.newKey   = Keyboard.GetState();
            input.newMouse = Mouse.GetState();

            if (gameState  == GameStates.Lobby)
            {
                lobby.Update(input, tanks);
            }
            else if (gameState  == GameStates.Ingame)
            {
                // The player
                tank_startpos.Update(incmsg, tanks);
                tank.Update(Content, graphics, tanks, tracks);
                tank.Input(input, Content);

                // TankTrack
                for (int i = 0; i < tracks.Count; i++)
                {
                    tracks[i].Update();
                    if (tracks[i].Alpha <= 0)
                    {
                        tracks.RemoveAt(i);
                        i--;
                    }
                }
            }
            else if (gameState == GameStates.Scoreboard)
            {
                scoreboard.Update(input);
            }

            // TEMP
            if (input.SingleKey(Keys.O))
            {
                gameState = GameStates.Scoreboard;
                Notify.NewMessage("End of game! Scoreboard below!", Color.LightBlue);
            }

            Notify.Update(gameTime);

            input.SetOldKey();
            input.SetOldMouse();

            base.Update(gameTime);


            // Fullscreen
            if (graphics.IsFullScreen != fullscreen)
            {
                graphics.IsFullScreen = fullscreen;
                graphics.ApplyChanges();
            }
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
                else if (gameState == GameStates.Ingame)
                {
                    spriteBatch.Begin();

                    tank.Draw(spriteBatch, tanks);

                    tracks.ForEach(f => f.Draw(spriteBatch));

                    Notify.Draw(spriteBatch);

                    spriteBatch.End();
                }
                else if (gameState == GameStates.Scoreboard)
                {
                    spriteBatch.Begin();

                    scoreboard.Draw(spriteBatch);
                    Notify.Draw(spriteBatch);

                    spriteBatch.End();
                }
            base.Draw(gameTime);
        }
    }
}
