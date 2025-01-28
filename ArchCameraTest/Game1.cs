using System.Collections.Generic;
using System.Diagnostics;
using Arch.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace ArchCameraTest
{
    internal class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private OrthographicCamera _camera;
        private World _world;
        private List<IUpdateSystem> updateSystems;
        private List<IDrawSystem> drawSystems;

        public Game1()
        {
            Debug.WriteLine("Starting Dungeons...");
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _camera = new OrthographicCamera(viewportAdapter) { Zoom = 5f };
            _camera.LookAt(Vector2.Zero);
            _world = World.Create();
            updateSystems = [new CameraSystem(_world, _camera), new PlayerInputSystem(_world)];
            drawSystems = [new RenderSystem(_world, _spriteBatch, _camera)];
            base.Initialize();
        }

        protected override void LoadContent()
        {
            var playerTexture = Content.Load<Texture2D>("img/player");
            var bgTexture = Content.Load<Texture2D>("img/bg");
            _world.Create(
                new TextureComponent { Texture = playerTexture },
                new ControlComponent(),
                new PositionComponent { Vec2 = Vector2.Zero }
            );
            for (int y = -2; y < 3; y++)
            {
                for (int x = -2; x < 3; x++)
                {
                    _world.Create(
                        new TextureComponent { Texture = bgTexture },
                        new PositionComponent { Vec2 = new Vector2(x * 16, y * 16) }
                    );
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            foreach (var system in updateSystems)
            {
                system.Update(gameTime);
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            foreach (var system in drawSystems)
            {
                system.Draw(gameTime);
            }
            base.Draw(gameTime);
        }
    }
    internal abstract class ArchSystem(World world, QueryDescription query)
    {
        protected World world = world;
        protected QueryDescription query = query;
    }

    internal interface IDrawSystem
    {
        abstract void Draw(GameTime gameTime);
    }

    internal interface IUpdateSystem
    {
        void Update(GameTime gameTime);
    }

    internal class PlayerInputSystem(World world) : ArchSystem(world, new QueryDescription().WithAll<ControlComponent>()), IUpdateSystem
    {
        public void Update(GameTime gameTime)
        {
            var delta = Vector2.Zero;
            var kState = Keyboard.GetState();
            if (kState.IsKeyDown(Keys.W))
            {
                delta = new Vector2(0, -2);
            }
            if (kState.IsKeyDown(Keys.S))
            {
                delta = new Vector2(0, 2);
            }
            if (kState.IsKeyDown(Keys.A))
            {
                delta = new Vector2(-2, 0);
            }
            if (kState.IsKeyDown(Keys.D))
            {
                delta = new Vector2(2, 0);
            }
            if (delta != Vector2.Zero)
            {
                world.Query(in query, (ref PositionComponent position) =>
                {
                    position.Vec2 += delta;
                });
            }
        }
    }

    internal class RenderSystem(World world, SpriteBatch spriteBatch, OrthographicCamera camera) : ArchSystem(world, new QueryDescription().WithAll<PositionComponent, TextureComponent>()), IDrawSystem
    {
        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(
                transformMatrix: camera.GetViewMatrix(),
                samplerState: SamplerState.PointClamp
            );
            world.Query(in query, (ref PositionComponent pos, ref TextureComponent texture) =>
            {
                spriteBatch.Draw(texture.Texture, pos.Vec2, Color.White);
            });
            spriteBatch.End();
        }
    }

    internal class CameraSystem(World world, OrthographicCamera camera) : ArchSystem(world, new QueryDescription().WithAll<PositionComponent, ControlComponent>()), IUpdateSystem
    {
        public void Update(GameTime gameTime)
        {
            world.Query(in query, (ref PositionComponent position) =>
            {
                camera.LookAt(position.Vec2);
            });
        }
    }

    internal record ControlComponent();

    internal record TextureComponent
    {
        public required Texture2D Texture { get; set; }
    }

    public class PositionComponent
    {
        public required Vector2 Vec2 { get; set; }
    }
}
