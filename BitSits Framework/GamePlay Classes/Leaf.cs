using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace BitSits_Framework
{
    public enum LeafState { Grow, Drop, Dead }

    class leaf
    {
        Vector2 position;
        Fixture connectedFixture;
        GameContent gameContent;
        Texture2D texture;
        float rotation, scale = 0, rotaionDirection = 1, oriRotation;
        const float growTime = 1.5f;
        const float MaxRotation = (float)Math.PI / 10;

        public LeafState State { get; private set; }

        public leaf(Fixture fixture, float rotation, int index, GameContent gameContent)
        {
            this.gameContent = gameContent;
            this.connectedFixture = fixture;
            this.rotation = rotation / 180 * (float)Math.PI;
            oriRotation = this.rotation;

            AABB aabb; fixture.GetAABB(out aabb);
            position = aabb.GetCenter();

            texture = gameContent.leaves[index];
        }

        public void Drop() { State = LeafState.Drop; }

        public void Update(GameTime gameTime)
        {
            // Fixture is removed from the body OR the body is Destroyed OR the fixture have a joint
            if (connectedFixture.GetBody() == null || connectedFixture.GetBody().GetFixtureList() == null
                || connectedFixture.GetUserData() is Branch)
                State = LeafState.Drop;

            if (State == LeafState.Grow)
            {
                scale = Math.Min(scale + 0.01f, 1f);

                rotation += rotaionDirection * (float)Math.PI / 1000;

                if (Math.Abs(rotation - oriRotation) > MaxRotation)
                {
                    rotation = oriRotation + rotaionDirection * MaxRotation;
                    rotaionDirection *= -1;
                }

                AABB aabb; connectedFixture.GetAABB(out aabb);
                position = aabb.GetCenter();
            }
            if (State == LeafState.Drop)
            {
                position += new Vector2(0, 1);
                rotation += rotaionDirection * (float)Math.PI / 700;

                if (position.Y > gameContent.viewport.Height * 1.5f) State = LeafState.Dead;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White, rotation,
                new Vector2(texture.Width / 2, texture.Height), scale, SpriteEffects.None, 1);
        }
    }
}
