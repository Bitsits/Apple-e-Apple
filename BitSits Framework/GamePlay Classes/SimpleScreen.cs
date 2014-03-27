using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BitSits_Framework
{
    public enum ScreenType
    {
        Inter, GameOver, Intro, Pause
    }

    class SimpleScreen : GameScreen
    {
        GameContent gameContent;
        public int score, prevScore;
        float time;
        const float MaxWaitTime = 0.5f;

        ScreenType type;

        public SimpleScreen(ScreenType type)
        {
            this.type = type;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            gameContent = ScreenManager.GameContent;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputState input)
        {
            MouseState mouseState = input.CurrentMouseState[0];
            MouseState prevMouseState = input.LastMouseState[0];

            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released
                && gameContent.viewport.TitleSafeArea.Contains(new Point(mouseState.X, mouseState.Y))
                && time > MaxWaitTime)
            {
                if (type == ScreenType.Intro)
                    LoadingScreen.Load(ScreenManager, false, PlayerIndex.One, new GameplayScreen());
                else if (type == ScreenType.Inter || type == ScreenType.Pause)
                    this.ExitScreen();
                else if (type == ScreenType.GameOver)
                    LoadingScreen.Load(ScreenManager, false, null, new SimpleScreen(ScreenType.Intro));
            }

            base.HandleInput(input);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 1 / 2);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            if (type == ScreenType.Intro)
                spriteBatch.Draw(gameContent.menuBackground, Vector2.Zero, Color.White);

            else if (type == ScreenType.Inter || type == ScreenType.GameOver)
            {
                if (type == ScreenType.Inter)
                    spriteBatch.Draw(gameContent.collected, new Vector2(250, 242.5f), Color.White);
                if (type == ScreenType.GameOver)
                    spriteBatch.Draw(gameContent.gameOver, new Vector2(250, 242.5f), Color.White);
                spriteBatch.DrawString(gameContent.menufont,
                    prevScore + " + " + score + " = " + (prevScore + score).ToString(),
                    new Vector2(300, 300), Color.Black);
            }

            else
                if (type == ScreenType.Pause)
                    spriteBatch.Draw(gameContent.pause, new Vector2(250, 242.5f), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
