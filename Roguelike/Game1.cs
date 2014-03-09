﻿#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion Using Statements

namespace Roguelike
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Gameplay gamePlay;

        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //contentManager = new ContentManager();
            ContentManager.tPlayer = Content.Load<Texture2D>("Sprites/dwarf");
            ContentManager.tWall = Content.Load<Texture2D>("Sprites/wall");
            ContentManager.tPotion = Content.Load<Texture2D>("Sprites/potion");
            ContentManager.tDoor = Content.Load<Texture2D>("Sprites/door");
            ContentManager.tDoorOpen = Content.Load<Texture2D>("Sprites/door_open");
            ContentManager.tSkeleton = Content.Load<Texture2D>("Sprites/skeleton");
            ContentManager.font = Content.Load<SpriteFont>("Fonts/tahoma");

            gamePlay = new Gameplay(graphics);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            gamePlay.Update();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Silver);

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, gamePlay.controller.camera.GetTransformMatrix());
            gamePlay.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}