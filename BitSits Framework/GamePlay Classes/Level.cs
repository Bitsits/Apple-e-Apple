using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Box2D.XNA;

namespace BitSits_Framework
{
    class Level : IDisposable
    {
        #region Fields

        public int PreviousScore { get; private set; }
        public int Score { get; private set; }
        public int LevelIndex { get; private set; }

        public GameContent GameContent { get; private set; }

        World world = new World(new Vector2(0, 100f), true);

        Body gate, ground;
        List<Branch> branches = new List<Branch>();
        Branch nearestBranch;

        List<Apple> apples = new List<Apple>();
        List<Vector2> collectables = new List<Vector2>();

        public bool IsLevelUp { get { return collectables.Count == 0; } }

        Path2D path = new Path2D();

        Vector2 mousePos;

        #endregion

        #region Initialization


        public Level(GameContent gameContent, int levelIndex, int previousScore)
        {
            GameContent = gameContent;

            LevelIndex = levelIndex;
            PreviousScore = previousScore;
            Score = 0;

            LoadTiles(levelIndex);
        }

        private void LoadTiles(int levelIndex)
        {
            BodyDef bd = new BodyDef();
            bd.type = BodyType.Static;
            PolygonShape pShape = new PolygonShape();
            ground = world.CreateBody(bd);

            List<Vector2> lines = new List<Vector2>();
            lines = GameContent.content.Load<List<Vector2>>("Levels/" + levelIndex.ToString("00"));

            for (int i = 0; i < lines[0].X; i++)
                apples.Add(new Apple(new Vector2(lines[i + 1].X, 600 - lines[i + 1].Y),
                    ground, GameContent, world));

            int a = (int)lines[0].X + 1;
            for (int i = 0; i < lines[0].Y; i++)
                collectables.Add(new Vector2(lines[a + i].X, 600 - lines[a + i].Y));

            a = (int)lines[0].X + (int)lines[0].Y + 1;
            for (int i = a; i < lines.Count; i += 2)
            {
                pShape.SetAsEdge(new Vector2(lines[i].X, 600 - lines[i].Y),
                    new Vector2(lines[i + 1].X, 600 - lines[i + 1].Y));
                pShape._radius = 5;

                FixtureDef fd = new FixtureDef();
                fd.shape = pShape;
                //fd.filter.categoryBits = 6;

                ground.CreateFixture(fd);
            }

            if (LevelIndex == 4) LoadGate();
        }

        private void LoadGate()
        {
            int height = 80;
            Vector2 jointPos = new Vector2(493, 439);

            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            bd.position = jointPos + new Vector2(0, height / 2);
            gate = world.CreateBody(bd);

            PolygonShape pShape = new PolygonShape();
            pShape.SetAsBox(0.2f, height / 2);
            FixtureDef fd = new FixtureDef();
            fd.shape = pShape;
            fd.density = 0.3f;

            gate.CreateFixture(fd);

            RevoluteJointDef rjd = new RevoluteJointDef();
            rjd.bodyA = gate;
            rjd.bodyB = ground;
            rjd.localAnchorA = new Vector2(0, -height / 2);
            rjd.localAnchorB = jointPos;
            world.CreateJoint(rjd);
        }

        public void Dispose() { }


        #endregion

        #region Update and HandleInput


        public void Update(GameTime gameTime)
        {
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 10, 10);

            List<Branch> deadBranches = new List<Branch>();
            foreach (Branch b in branches)
            {
                b.Update(gameTime);

                if (b.IsDead) deadBranches.Add(b);
            }

            foreach (Apple a in apples)
            {
                a.Update(gameTime);
                if (a.AddBranch) branches.Add(a.stem);
            }

            foreach (Branch db in deadBranches) branches.Remove(db);

            float nearestDistance = 50;
            List<Vector2> removeCollectabes = new List<Vector2>();
            foreach (Vector2 v in collectables)
            {
                foreach (Branch b in branches)
                {
                    if (b.ApplesIntersectingCollectables(v, nearestDistance))
                    { removeCollectabes.Add(v); break; }
                }
            }

            foreach (Vector2 rc in removeCollectabes) collectables.Remove(rc);

            Score = CountBonusApples();
        }


        public void HandleInput(InputState input, int playerIndex)
        {
            MouseState prevMouseState = input.LastMouseState[0];
            MouseState mouseState = input.CurrentMouseState[0];
            mousePos = new Vector2(mouseState.X, mouseState.Y);

            if (prevMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released
                && path.Keys.Count != 0)
            {
                if (path.Keys.Count > 1)
                {
                    if (nearestBranch != null && nearestBranch.body.GetFixtureList() != null)
                    {
                        if (nearestBranch.body.GetFixtureList() == nearestBranch.nearestFixture)
                            nearestBranch.Grow(path);
                        else
                            branches.Add(new Branch(path, GameContent, world, nearestBranch));
                    }

                    //else branches.Add(new Branch(path, GameContent, world, null));
                }
                path = new Path2D();
            }

            if (prevMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released
                && nearestBranch != null && path.Keys.Count == 0)
            {
                nearestBranch.CutDown(nearestBranch.nearestFixture);
            }

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (GameContent.viewport.TitleSafeArea.Contains(new Point(mouseState.X, mouseState.Y)))
                    path.AddPoint(mousePos);
            }
            else
            {
                nearestBranch = null;
                float nearestDist = 10;
                foreach (Branch b in branches)
                {
                    float d = b.NearestFixtureDistance(mousePos, nearestDist);
                    if (d < nearestDist)
                    {
                        if (nearestBranch != null) nearestBranch.nearestFixture = null;

                        nearestBranch = b;
                        nearestDist = d;
                    }
                }
            }
        }


        public int CountBonusApples()
        {
            int i = 0;
            foreach (Branch b in branches) i += b.AppleCount();

            return i;
        }


        #endregion

        #region Draw

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GameContent.background[LevelIndex], Vector2.Zero, Color.White);

            foreach (Vector2 t in collectables)
                spriteBatch.Draw(GameContent.collectable, t, null, Color.White, 0,
                    new Vector2(GameContent.collectable.Width / 2), 1, SpriteEffects.None, 1);

            if (gate != null)
            {
                spriteBatch.Draw(GameContent.bar, gate.Position, null, Color.White, gate.Rotation,
                    new Vector2(GameContent.bar.Width / 2, GameContent.bar.Height / 2),
                    1, SpriteEffects.None, 1);
            }

            foreach (Branch b in branches) b.Draw(spriteBatch);

            for (int i = 0; i < path.Keys.Count; i++)
                spriteBatch.Draw(GameContent.dot, path.Keys[i], null, Color.White, 0,
                    new Vector2(GameContent.dot.Width / 2), 1, SpriteEffects.None, 1);

            foreach (Apple a in apples) a.Draw(spriteBatch);


            //spriteBatch.DrawString(GameContent.menufont, "X:" + mousePos.X + "\nY:" + mousePos.Y, mousePos, Color.Black);

            if (nearestBranch != null) nearestBranch.DrawMarker(spriteBatch);
        }

        #endregion
    }
}
