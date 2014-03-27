using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Box2D.XNA;

namespace BitSits_Framework
{
    class Branch
    {
        public Body body;
        public RevoluteJoint revoJoint;

        GameContent gameContent;
        Texture2D dot, circle, square, cursor;

        public Fixture nearestFixture;
        int fixtureCount = 0;

        List<leaf> leaves = new List<leaf>();
        List<Apple> apples = new List<Apple>();
        Random random = new Random();

        public bool IsDead
        {
            get
            {
                return apples.Count == 0 && leaves.Count == 0 && body.GetFixtureList() == null;
            }
        }

        public bool IsConnected { get; private set; }
        bool wasConnected = false;

        bool newGrow, growApples = false;
        const float DestroyTime = 2.5f, AppleTime = 8.5f;
        float dTime, aTime;

        Path2D path = new Path2D();

        public Branch(Path2D path, GameContent gameContent, World world, Branch nearestBranch)
        {
            this.gameContent = gameContent;
            dot = gameContent.dot;
            circle = gameContent.jointCircle;
            square = gameContent.jointSquare;
            cursor = gameContent.cursor;

            BodyDef bd = new BodyDef();
            bd.position = path.Keys[0];
            bd.type = BodyType.Dynamic;
            body = world.CreateBody(bd);

            fixtureCount = path.Keys.Count;
            for (int i = 0; i < path.Keys.Count; i++)
                CreateFixture(path.Keys[i] - path.Keys[0]);

            if (nearestBranch != null)
            {
                RevoluteJointDef revJd = new RevoluteJointDef();
                revJd.bodyA = body;
                revJd.bodyB = nearestBranch.body;

                revJd.localAnchorA = Vector2.Zero;

                AABB aabb; nearestBranch.nearestFixture.GetAABB(out aabb);
                Vector2 p = aabb.GetCenter();
                revJd.localAnchorB = nearestBranch.nearestFixture.GetBody().GetLocalPoint(p);

                revJd.enableMotor = true;
                revJd.referenceAngle = nearestBranch.nearestFixture.GetBody().Rotation;

                revoJoint = (RevoluteJoint)world.CreateJoint(revJd);
                revoJoint.SetUserData((Branch)nearestBranch);

                nearestBranch.nearestFixture.SetUserData((Branch)this);
            }

            newGrow = true;
        }

        public void Grow(Path2D path)
        {
            AABB aabb; body.GetFixtureList().GetAABB(out aabb);
            Vector2 p = aabb.GetCenter();
            float d = Vector2.Distance(p, body.Position);
            float theta = (float)Math.Atan2((double)(p.Y - body.Position.Y),
                    (double)(p.X - body.Position.X));

            theta -= body.Rotation;
            Vector2 localEnd = d * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));

            fixtureCount += path.Keys.Count - 1;
            for (int i = 1; i < path.Keys.Count; i++)
            {
                d = Vector2.Distance(path.Keys[i], path.Keys[0]);
                theta = (float)Math.Atan2((double)(path.Keys[i].Y - path.Keys[0].Y),
                    (double)(path.Keys[i].X - path.Keys[0].X));
                theta -= body.Rotation;

                CreateFixture(localEnd + d * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)));
                //CreateFixture(body.GetLocalPoint(p) + path.Keys[i] - path.Keys[0]);
            }

            newGrow = true;
        }

        public void CutDown(Fixture cutDownFixture)
        {
            List<Fixture> destroyFix = new List<Fixture>();
            for (Fixture f = body.GetFixtureList(); f != null; f = f.GetNext())
            {
                destroyFix.Add(f);
                if (f == cutDownFixture) break;
            }

            fixtureCount -= destroyFix.Count;
            foreach (Fixture f in destroyFix)
            {
                object o = f.GetUserData();
                if (o is Branch)
                {
                    body.GetWorld().DestroyJoint(((Branch)o).revoJoint);
                    ((Branch)o).revoJoint = null;
                }

                body.DestroyFixture(f);
            }

            // Destroy Body if all fixture are destroyed
            if (fixtureCount == 0) body.GetWorld().DestroyBody(body);

            newGrow = true;
        }

        public void DropAndGrowLeaves(GameTime gameTime)
        {
            int appleCount = -1, leavesCount = -1, i = 0;

            if (!IsConnected || newGrow)
            { DropLeaves(); aTime = 0; growApples = true; }

            if ((newGrow || !wasConnected) && IsConnected)
            {
                leavesCount = (int)(0.3f * fixtureCount);
            }

            aTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (aTime > AppleTime && growApples)
            { appleCount = (int)(0.05f * fixtureCount); growApples = false; }

            newGrow = false;

            List<int> leavesIndex = new List<int>(); i = 0;
            while (i < leavesCount)
            {
                int a = random.Next((int)(0.7 * fixtureCount));
                if (!leavesIndex.Contains(a)) { leavesIndex.Add(a); i++; }
            }
            if (leavesIndex.Count == 0 && leavesCount == 0) leavesIndex.Add(0);

            List<int> appleIndex = new List<int>(); i = 0;
            while (i < appleCount)
            {
                int a = random.Next((int)(0.7 * fixtureCount));
                if (!appleIndex.Contains(a)) { appleIndex.Add(a); i++; }
            }

            i = 0;
            for (Fixture f = body.GetFixtureList(); f != null; f = f.GetNext())
            {
                if (leavesIndex.Count == 0 && appleIndex.Count == 0) break;

                if (f.GetUserData() is Branch) continue;

                if (leavesIndex.Contains(i))
                    leaves.Add(new leaf(f, random.Next(360), random.Next(5), gameContent));

                if (appleIndex.Contains(i))
                    apples.Add(new Apple(f, gameContent, f.GetBody().GetWorld()));

                leavesIndex.Remove(i); appleIndex.Remove(i); i++;
            }
        }

        private void DropLeaves()
        {
            foreach (leaf l in leaves) l.Drop();
            foreach (Apple a in apples) a.Drop();
        }

        private void CreateFixture(Vector2 localPosition)
        {
            CircleShape cShape = new CircleShape();

            cShape._radius = .5f;
            cShape._p = localPosition;

            FixtureDef fd = new FixtureDef();
            fd.shape = cShape;
            fd.density = 10.1f;
            //fd.filter.groupIndex = -1;
            //fd.filter.categoryBits = 4;
            //fd.filter.maskBits = 2;

            body.CreateFixture(fd);
        }

        public void Update(GameTime gameTime)
        {
            bool groundContact = false;

            for (ContactEdge ce = body.GetContactList(); ce != null; ce = ce.Next)
                if (ce.Contact.IsTouching() && ce.Other.GetType() == BodyType.Static) groundContact = true;

            if (groundContact || body.Position.Y > gameContent.viewport.Height * 2)
                dTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            else dTime = 0;
            if (dTime > DestroyTime) body.GetWorld().DestroyBody(body);


            IsConnected = true;

            if (revoJoint == null) IsConnected = false;
            else
            {
                object o = revoJoint.GetUserData();
                if (o is Branch)
                {
                    IsConnected = ((Branch)o).IsConnected;
                    if (((Branch)o).body.GetFixtureList() == null) IsConnected = false;
                }

                // If Ground is in contact
                if (groundContact) IsConnected = false;
            }

            DropAndGrowLeaves(gameTime);

            wasConnected = IsConnected;

            path = new Path2D();
            for (Fixture f = body.GetFixtureList(); f != null; f = f.GetNext())
            {
                // Clear if the other joint body is destroyed
                if (f.GetUserData() is Branch && ((Branch)f.GetUserData()).body.GetFixtureList() == null)
                    f.SetUserData(null);

                AABB aabb; f.GetAABB(out aabb);
                path.Keys.Add(aabb.GetCenter());
            }

            if (revoJoint != null && body.GetFixtureList() != null)
            {
                AABB aabb; body.GetFixtureList().GetAABB(out aabb);
                float length = Vector2.Distance(aabb.GetCenter(), body.Position);

                float angleError = revoJoint.GetJointAngle();
                float gain = 0.5f;
                int torqueFactor = 10000;

                if (angleError != 0)
                {
                    revoJoint.SetMotorSpeed(-gain * angleError);
                    revoJoint.SetMaxMotorTorque(torqueFactor * (float)Math.Min(Math.Abs(angleError), Math.PI / 18)
                        * (float)Math.Pow(length, 1.5f));
                }
            }

            UpdateLeavesAndApples(gameTime);

            if (!IsConnected && body.Position.Y > gameContent.viewport.Height * 3)
                body.GetWorld().DestroyBody(body);
        }

        private void UpdateLeavesAndApples(GameTime gameTime)
        {
            List<leaf> deadLeaves = new List<leaf>();
            foreach (leaf l in leaves)
            {
                l.Update(gameTime);
                if (l.State == LeafState.Dead) deadLeaves.Add(l);
            }

            List<Apple> deadApples = new List<Apple>();
            foreach (Apple a in apples)
            {
                a.Update(gameTime);
                if (a.State == LeafState.Dead) deadApples.Add(a);
            }

            foreach (leaf dl in deadLeaves) leaves.Remove(dl);
            foreach (Apple da in deadApples) apples.Remove(da);
        }

        public float NearestFixtureDistance(Vector2 mousePos, float nearestDistance)
        {
            nearestFixture = null;

            for (Fixture f = body.GetFixtureList(); f != null; f = f.GetNext())
            {
                // the fixture have a joint
                if (f.GetUserData() is Branch) continue;

                AABB aabb;
                f.GetAABB(out aabb);
                Vector2 center = aabb.GetCenter();
                float d = Vector2.Distance(center, mousePos);

                if (d < nearestDistance)
                {
                    nearestDistance = d;
                    nearestFixture = f;
                }
            }

            return nearestDistance;
        }

        public bool ApplesIntersectingCollectables(Vector2 collectable, float nearestDistance)
        {
            foreach (Apple a in apples)
            {
                AABB aabb = new AABB();
                a.body.GetFixtureList().GetAABB(out aabb);

                float d = Vector2.Distance(collectable, aabb.GetCenter());
                if (d < nearestDistance) return true;
            }

            return false;
        }

        public int AppleCount()
        {
            int i = 0;
            foreach (Apple a in apples)
                if (a.State == LeafState.Grow) i += 1;

            return i;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float endScale = 0.12f;
            float startScale = Math.Min(endScale + 0.1f + (float)path.Keys.Count / 300, 1f);
            float borderScale = 0.1f;
            for (float i = 0; i < path.Keys.Count; i += .5f)
            {
                float lerpFactor = 1 - (float)i / (float)(path.Keys.Count - 1);

                if (path.Keys.Count == 1) lerpFactor = 0;

                lerpFactor = (float)Math.Pow(lerpFactor, 0.8f);
                float scale = MathHelper.Lerp(startScale + borderScale, endScale + borderScale, lerpFactor);

                Vector2 v = path.GetPointOnCurve(i);

                //spriteBatch.Draw(cursor, v, null, Color.White, 0,
                // new Vector2(cursor.Width / 2), scale, SpriteEffects.None, 1);
            }

            for (float i = 0; i < path.Keys.Count; i += .5f)
            {
                float lerpFactor = 1 - (float)i / (float)(path.Keys.Count - 1);

                if (path.Keys.Count == 1) lerpFactor = 0;

                lerpFactor = (float)Math.Pow(lerpFactor, 0.8f);
                float scale = MathHelper.Lerp(startScale, endScale, lerpFactor);

                Vector2 v = path.GetPointOnCurve(i);

                spriteBatch.Draw(cursor, v, null, Color.Chocolate, 0,
                    new Vector2(cursor.Width / 2), scale, SpriteEffects.None, 1);
            }

            for (int i = 0; i < path.Keys.Count; i += 1)
                spriteBatch.Draw(cursor, path.Keys[i], null, Color.DarkGray, 0, new Vector2(cursor.Width / 2), .0f, SpriteEffects.None, 1);

            foreach (leaf l in leaves) l.Draw(spriteBatch);

            foreach (Apple a in apples) a.Draw(spriteBatch);
        }

        public void DrawMarker(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < path.Keys.Count; i++)
            {
                spriteBatch.Draw(dot, path.Keys[i], null, Color.White, 0,
                    new Vector2(dot.Width / 2), 1, SpriteEffects.None, 1);
            }

            AABB aabb;
            nearestFixture.GetAABB(out aabb);

            if (nearestFixture == body.GetFixtureList())
                spriteBatch.Draw(square, aabb.GetCenter(), null, Color.Black, 0,
                    new Vector2(square.Width / 2), 1, SpriteEffects.None, 1);

            else
                spriteBatch.Draw(circle, aabb.GetCenter(), null, Color.Black, 0,
                    new Vector2(circle.Width / 2), 1, SpriteEffects.None, 1);
        }
    }
}
