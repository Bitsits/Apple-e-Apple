using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BitSits_Framework
{
    /// <summary>
    /// Sample showing how to manage different game states, with transitions
    /// between menu screens, a loading screen, the game itself, and a pause
    /// menu. This main game class is extremely simple: all the interesting
    /// stuff happens in the ScreenManager component.
    /// </summary>
    public class BitSitsGames : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        #endregion

        #region Initialization


        /// <summary>
        /// The main game constructor.
        /// </summary>
        public BitSitsGames()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

            IsMouseVisible = true;

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            LoadingScreen.Load(screenManager, true, null, new SimpleScreen(ScreenType.Intro));

            // TEST LEVELS
            //LoadingScreen.Load(screenManager, false, PlayerIndex.One, new GameplayScreen());
        }


        #endregion

        #region Draw


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (BitSitsGames game = new BitSitsGames())
            {
                game.Run();
            }
        }
    }

    #endregion
}
