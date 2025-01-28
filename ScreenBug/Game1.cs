using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;

namespace ScreenBug
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private readonly ScreenManager _screenManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
        }

        protected override void Initialize()
        {
            _screenManager.LoadScreen(new Screen1(this)); // LoadContent is ran twice
            base.Initialize();
            // _screenManager.LoadScreen(new Screen1(this)); // LoadContent is ran once
        }

        protected override void LoadContent() { }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }
    }

    public class Screen1(Game1 game) : GameScreen(game)
    {
        public override void LoadContent()
        {
            Debug.WriteLine("Hello from Screen1 LoadContent!");
        }
        public override void Draw(GameTime gameTime) { }

        public override void Update(GameTime gameTime) { }
    }
}
