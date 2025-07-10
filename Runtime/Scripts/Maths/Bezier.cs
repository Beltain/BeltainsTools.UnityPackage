using UnityEngine;

namespace BeltainsTools.Maths
{
    public struct Bezier
    {
        public Vector3 Point_A;
        public Vector3 Poll_A;
        public Vector3 Poll_B;
        public Vector3 Point_B;

        public Bezier(Vector3 Point_A, Vector3 Poll_A, Vector3 Poll_B, Vector3 Point_B)
        {
            this.Point_A = Point_A;
            this.Poll_A = Poll_A;
            this.Poll_B = Poll_B;
            this.Point_B = Point_B;
        }

        public Vector3 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            return Maths.MathB.CubicLerp(Point_A, Poll_A, Poll_B, Point_B, t);
        }
    }
}
