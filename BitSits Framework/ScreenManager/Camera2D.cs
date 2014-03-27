using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BitSits_Framework
{
    /// <summary>
    /// Very basic sample program for demonstrating a 2D Camera
    /// Controls are WASD for movement, QE for rotation, and ZC for zooming.
    /// </summary>
    class Camera2D : GameComponent
    {
        public Vector2 position;
        public float rotation;
        public float scale;
        public float speed;
        public bool manualcamera;

        public Matrix Transform { get { return transform; } }
        private Matrix transform;
        private GraphicsDevice graphicsDevice;

        public Camera2D(Game game, bool manualcamera)
            : base(game)
        {
            graphicsDevice = game.GraphicsDevice;
            this.manualcamera = manualcamera;
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            position = Vector2.Zero;
            rotation = 0;
            scale = 1;
            speed = 5;

            base.Initialize();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (manualcamera)
            {
                //translation controls, left stick xbox or WASD keyboard
                if (Keyboard.GetState().IsKeyDown(Keys.A)
                    || GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0) { position.X += speed; }
                if (Keyboard.GetState().IsKeyDown(Keys.D)
                    || GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > 0) { position.X -= speed; }
                if (Keyboard.GetState().IsKeyDown(Keys.S)
                    || GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < 0) { position.Y -= speed; }
                if (Keyboard.GetState().IsKeyDown(Keys.W)
                    || GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > 0) { position.Y += speed; }

                //rotation controls, right stick or QE keyboard
                if (Keyboard.GetState().IsKeyDown(Keys.Q)
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X < 0) { rotation += 0.01f; }
                if (Keyboard.GetState().IsKeyDown(Keys.E)
                    || GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X > 0) { rotation -= 0.01f; }

                //zoom/scale controls, left/right triggers or CZ keyboard
                if (Keyboard.GetState().IsKeyDown(Keys.C)
                    || GamePad.GetState(PlayerIndex.One).Triggers.Right > 0) { scale += 0.001f; }
                if (Keyboard.GetState().IsKeyDown(Keys.Z)
                    || GamePad.GetState(PlayerIndex.One).Triggers.Left > 0) { scale -= 0.001f; }
            }

            //scale = Math.Min((float)graphicsDevice.Viewport.Width / 640,
            //                (float)graphicsDevice.Viewport.Height / 480);

            transform = Matrix.CreateScale(new Vector3(scale, scale, 0))
                        * Matrix.CreateRotationZ(rotation)
                        * Matrix.CreateTranslation(new Vector3(position.X, position.Y, 0));

            base.Update(gameTime);
        }
    }
}
