using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BitSits_Framework
{
    class Path2D
    {
        public const float fixedDisplacement = 10;

        public readonly List<Vector2> Keys;

        public Path2D() { Keys = new List<Vector2>(); }

        public void AddPoint(Vector2 position)
        {
            if (Keys.Count == 0) Keys.Add(position);
            else
            {
                Vector2 prevPos = new Vector2(Keys[Keys.Count - 1].X, Keys[Keys.Count - 1].Y);

                while (Vector2.Distance(prevPos, position) >= fixedDisplacement)
                {
                    float a = prevPos.X, b = prevPos.Y, c = position.X, d = position.Y;

                    float P = (d - b);
                    float B = (c - a);
                    float H = (float)Math.Sqrt(P * P + B * B);

                    // cos = B / H  sin = P / H
                    Vector2 pathKey = prevPos + fixedDisplacement * new Vector2(B / H, P / H);

                    Keys.Add(pathKey); prevPos = pathKey;
                }
            }
        }

        public Vector2 GetPointOnCurve(float time)
        {
            Vector2 pathKey = new Vector2();
            time = MathHelper.Clamp(time, 0, Keys.Count - 1);

            for (int i = 0; i < Keys.Count; i++)
            {
                if (i == time)
                {
                    return Keys[i];
                }
                else if (i > time)
                {
                    float a = Keys[i - 1].X, b = Keys[i - 1].Y, c = Keys[i].X, d = Keys[i].Y;

                    float P = (d - b);
                    float B = (c - a);
                    float H = (float)Math.Sqrt(P * P + B * B);

                    float displacement = time - (i - 1);

                    return (new Vector2(a, b) + displacement * new Vector2(B, P));
                }
            }

            return pathKey;
        }
    }
}
