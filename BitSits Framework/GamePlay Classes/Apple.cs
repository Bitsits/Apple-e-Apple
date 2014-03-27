using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace BitSits_Framework
{
    class Apple
    {
        bool fallenFromAir = false;
        Vector2 originalPostion;

        World world;
        public Body body;
        Body ground;
        RevoluteJoint revoJoint;
        Fixture connectedFixture;

        GameContent gameContent;

        public Branch stem;
        public bool AddBranch { get; private set; }

        float scale, time;
        const float MaxTime = 3.5f;

        Texture2D texture;

        public LeafState State { get; private set; }

        public Apple(Fixture connectedFixture, GameContent gameContent, World world)
        {
            this.world = world;
            this.gameContent = gameContent;
            texture = gameContent.apple;

            fallenFromAir = false;

            CreatBody();

            State = LeafState.Grow;

            if (connectedFixture != null)
            {
                this.connectedFixture = connectedFixture;
                AABB aabb; connectedFixture.GetAABB(out aabb);
                body.Position = aabb.GetCenter();

                RevoluteJointDef rjd = new RevoluteJointDef();
                rjd.bodyA = body;
                rjd.bodyB = connectedFixture.GetBody();

                rjd.localAnchorA = Vector2.Zero;
                rjd.localAnchorB = connectedFixture.GetBody().GetLocalPoint(body.Position);

                revoJoint = (RevoluteJoint)world.CreateJoint(rjd);
            }
        }

        private void CreatBody()
        {
            State = LeafState.Drop;

            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            CircleShape cShape = new CircleShape();
            cShape._radius = texture.Width / 2 - 3;
            cShape._p = new Vector2(0, cShape._radius + 1);
            FixtureDef fd = new FixtureDef();
            fd.density = 0.001f;
            fd.friction = 0.5f;
            fd.restitution = 0.0f;
            fd.shape = cShape;
            //fd.filter.groupIndex = -1;
            //fd.filter.categoryBits = 2;
            //fd.filter.maskBits = 4;

            body = world.CreateBody(bd);
            body.CreateFixture(fd);

            scale = 0;
        }

        public Apple(Vector2 position, Body ground, GameContent gameContent, World world)
            : this(null, gameContent, world)
        {
            this.ground = ground;
            body.Position = position;
            originalPostion = position;
            State = LeafState.Drop;

            fallenFromAir = true;
        }

        public void Drop()
        {
            if (State == LeafState.Grow)
            {
                if (revoJoint.GetBodyB().GetFixtureList() != null)
                    world.DestroyJoint(revoJoint);
                State = LeafState.Drop;
            }
        }

        public void Update(GameTime gameTime)
        {
            // Fixture is removed from the body OR the body is Destroyed OR the fixture have a joint
            if (State == LeafState.Grow && connectedFixture != null && (connectedFixture.GetBody() == null
                || connectedFixture.GetBody().GetFixtureList() == null
                || connectedFixture.GetUserData() is Branch)) 
                Drop();

            scale = Math.Min(scale + 0.008f, 1f);
            AddBranch = false;

            if (State == LeafState.Drop)
            {
                if (body.Position.Y > gameContent.viewport.Height * 1.5f) State = LeafState.Dead;

                bool groundContact = false;
                for (ContactEdge ce = body.GetContactList(); ce != null; ce = ce.Next)
                    if (ce.Contact.IsTouching() && ce.Other.GetType() == BodyType.Static)
                        groundContact = true;

                if (groundContact) time += (float)gameTime.ElapsedGameTime.TotalSeconds;
                else time = 0;

                if (time > MaxTime) State = LeafState.Dead; ;

                if (fallenFromAir && time > MaxTime / 2)
                {
                    State = LeafState.Dead;
                    CreateStem();
                }
            }

            if (State == LeafState.Dead)
            {
                if (body.GetFixtureList() != null) body.GetWorld().DestroyBody(body);

                else if (fallenFromAir && (stem == null || stem.body.GetFixtureList() == null))
                {
                    CreatBody();
                    body.Position = originalPostion;
                }
            }
        }

        private void CreateStem()
        {
            AddBranch = true;

            AABB aabb = new AABB(); body.GetFixtureList().GetAABB(out aabb);
            Vector2 v = aabb.GetCenter() + new Vector2(0, 15);

            Path2D p = new Path2D();
            p.AddPoint(v); p.AddPoint(v + new Vector2(0, -10));

            stem = new Branch(p, gameContent, body.GetWorld(), null);
            RevoluteJointDef revJd = new RevoluteJointDef();
            revJd.bodyA = stem.body;
            revJd.bodyB = ground;

            revJd.collideConnected = true;

            revJd.localAnchorA = Vector2.Zero;
            revJd.localAnchorB = stem.body.Position;
            revJd.enableMotor = true;

            stem.revoJoint = (RevoluteJoint)world.CreateJoint(revJd);

            // A Small rotation sets the body in motion
            stem.body.Rotation = (float)Math.PI / 360;            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (State != LeafState.Dead)
                spriteBatch.Draw(texture, body.Position, null, Color.White,
                    body.Rotation, new Vector2(texture.Width / 2, 0), scale, SpriteEffects.None, 1);
        }
    }
}
