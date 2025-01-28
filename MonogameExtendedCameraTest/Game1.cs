using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.ViewportAdapters;

namespace MonogameExtendedCameraTest
{
    internal class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private OrthographicCamera _camera;
        private World _world;

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
            _world = new WorldBuilder()
                .AddSystem(new PlayerInputSystem())
                .AddSystem(new RenderSystem(_spriteBatch, _camera))
                .AddSystem(new CameraSystem(_camera))
                .Build();
            Components.Add(_world);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            var playerTexture = Content.Load<Texture2D>("img/player");
            var bgTexture = Content.Load<Texture2D>("img/bg");

            for (int y = -2; y < 3; y++)
            {
                for (int x = -2; x < 3; x++)
                {
                    var bg = _world.CreateEntity();
                    bg.Attach(new TextureComponent { Texture = bgTexture });
                    bg.Attach(new PositionComponent { Vec2 = new Vector2(x * 16, y * 16) });
                }
            }

            var player = _world.CreateEntity();
            player.Attach(new TextureComponent { Texture = playerTexture });
            player.Attach(new ControlComponent());
            player.Attach(new PositionComponent { Vec2 = Vector2.Zero });
        }

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

    internal class PlayerInputSystem() : EntityUpdateSystem(Aspect.All(typeof(ControlComponent)))
    {
        private ComponentMapper<PositionComponent> _posMapper;

        public override void Initialize(IComponentMapperService mapperService)
        {
            _posMapper = mapperService.GetMapper<PositionComponent>();
        }

        public override void Update(GameTime gameTime)
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
                foreach (var entityId in ActiveEntities)
                {
                    var position = _posMapper.Get(entityId);
                    position.Vec2 += delta;
                }
            }
        }
    }

    internal class RenderSystem(SpriteBatch spriteBatch, OrthographicCamera camera) : EntitySystem(Aspect.All(typeof(PositionComponent), typeof(TextureComponent))), IDrawSystem
    {
        private ComponentMapper<PositionComponent> _positionMapper;
        private ComponentMapper<TextureComponent> _textureMapper;

        public override void Initialize(IComponentMapperService mapperService)
        {
            _positionMapper = mapperService.GetMapper<PositionComponent>();
            _textureMapper = mapperService.GetMapper<TextureComponent>();
        }

        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(
                transformMatrix: camera.GetViewMatrix(),
                samplerState: SamplerState.PointClamp
            );

            foreach (var entity in ActiveEntities)
            {
                var pos = _positionMapper.Get(entity);
                var texture = _textureMapper.Get(entity);
                spriteBatch.Draw(texture.Texture, pos.Vec2, Color.White);
            }

            spriteBatch.End();
        }
    }

    internal class CameraSystem(OrthographicCamera camera) : EntityProcessingSystem(Aspect.All(typeof(ControlComponent), typeof(PositionComponent)))
    {
        private ComponentMapper<PositionComponent> _positionMapper;

        public override void Initialize(IComponentMapperService mapperService)
        {
            _positionMapper = mapperService.GetMapper<PositionComponent>();
        }

        public override void Process(GameTime gameTime, int entityId)
        {
            var position = _positionMapper.Get(entityId);
            camera.LookAt(position.Vec2);
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
