using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BitSits_Framework
{
    /// <summary>
    /// All the Contents of the Game is loaded and stored here
    /// so that all other screen can copy from here
    /// </summary>
    public class GameContent
    {
        public readonly ContentManager content;
        public readonly Viewport viewport;

        // Textures
        public readonly Texture2D blank;
        public readonly Texture2D gradient;
        public readonly Texture2D menuBackground;
        public readonly Texture2D[] leaves = new Texture2D[5];
        public readonly Texture2D jointCircle, jointSquare, dot, bar, cursor, apple, collectable;
        public readonly Texture2D[] background = new Texture2D[5];
        public readonly Texture2D pause, gameOver, collected;

        // Fonts
        public readonly SpriteFont menufont;

        // Songs
        public readonly Song song;

        // Sound Effects
        public readonly SoundEffect soundEffect;

        /// <summary>
        /// Load GameContents
        /// </summary>
        public GameContent(GameComponent screenManager)
        {
            content = new ContentManager(screenManager.Game.Services, "Content");
            viewport = screenManager.Game.GraphicsDevice.Viewport;

            blank = content.Load<Texture2D>("Graphics/blank");
            gradient = content.Load<Texture2D>("Graphics/gradient");
            menuBackground = content.Load<Texture2D>("Graphics/background");

            apple = content.Load<Texture2D>("Graphics/apple");
            jointCircle = content.Load<Texture2D>("Graphics/jointCircle");
            jointSquare = content.Load<Texture2D>("Graphics/jointSquare");
            dot = content.Load<Texture2D>("Graphics/dot");
            cursor = content.Load<Texture2D>("Graphics/cursor");
            collectable = content.Load<Texture2D>("Graphics/collectable");
            pause = content.Load<Texture2D>("Graphics/pause");
            collected = content.Load<Texture2D>("Graphics/collected");
            gameOver = content.Load<Texture2D>("Graphics/gameOver");
            bar = content.Load<Texture2D>("Graphics/bar");

            for (int i = 0; i < 5; i++)
                leaves[i] = content.Load<Texture2D>("Graphics/leaf" + i);

            for (int i = 0; i < 5; i++)
                background[i] = content.Load<Texture2D>("Graphics/background" + i);

            menufont = content.Load<SpriteFont>("Fonts/font");

            song = content.Load<Song>("Audio/Peace by Daichen");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;
            //MediaPlayer.Volume = 1.0f;

            //Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            screenManager.Game.ResetElapsedTime();
        }

        /// <summary>
        /// Unload GameContents
        /// </summary>
        public void UnloadContent() { content.Unload(); }
    }
}
