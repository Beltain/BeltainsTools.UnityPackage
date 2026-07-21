using Newtonsoft.Json;

namespace BeltainsTools.Maths
{
    /// <summary>
    /// A json serializable pseudo-random number generator.
    /// Uses XorShift algorithm for fast, high-quality random numbers.
    /// </summary>
    [System.Serializable]
    public class XSRandom
    {
        [JsonProperty]
        private uint[] m_States = new uint[4];

        public XSRandom() : this((uint)System.Environment.TickCount) { }
        public XSRandom(uint seed)
        {
            Seed(seed, preWarm: true);
        }

        public XSRandom(uint s0, uint s1, uint s2, uint s3)
        {
            m_States[0] = s0;
            m_States[1] = s1;
            m_States[2] = s2;
            m_States[3] = s3;
        }

        public XSRandom Clone()
        {
            return new XSRandom(m_States[0], m_States[1], m_States[2], m_States[3]);
        }

        public override string ToString()
        {
            return new System.Text.StringBuilder().AppendFormat("({0}, {1}, {2}, {3})", m_States[0], m_States[1], m_States[2], m_States[3]).ToString();
        }

        /// <summary>Seed the random number generator with the provided <paramref name="seed"/>. Optionally pre-warm the generator by generating 8 random numbers to avoid initial low-quality outputs."</summary>
        public void Seed(uint seed, bool preWarm = true)
        {
            // SplitMix64-style:
            m_States[0] = seed;
            m_States[1] = seed ^ 0x9E3779B9;
            m_States[2] = seed ^ 0x85EBCA6B;
            m_States[3] = seed ^ 0xC2B2AE35;

            if (preWarm)
            {
                for (int i = 0; i < 8; i++)
                    NextUInt();
            }
        }


        /// <returns>A random unsigned integer.</returns>
        public uint NextUInt()
        {
            uint t = m_States[3];
            uint s = m_States[0];
            m_States[3] = m_States[2];
            m_States[2] = m_States[1];
            m_States[1] = s;

            t ^= t << 11;
            t ^= t >> 8;
            m_States[0] = t ^ s ^ (s >> 19);

            return m_States[0];
        }

        /// <returns>A random signed integer.</returns>
        public int NextInt() => (int)(NextUInt() & 0x7FFFFFFF);

        /// <returns>A random int in range inclusive min, exclusive max.</returns>
        public int NextInt(int min, int max) => min + (int)(NextUInt() % (uint)(max - min));

        /// <returns>A random float in range inclusive 0, exclusive 1.</returns>
        public float NextFloat() => (NextUInt() & 0x007FFFFF) / (float)0x800000;

        /// <returns>A random float in range inclusive min, exclusive max.</returns>
        public float NextFloat(float min, float max) => min + NextFloat() * (max - min);

        /// <returns>A random double in range inclusive 0, exclusive 1.</returns>
        public double NextDouble() => NextUInt() / (double)uint.MaxValue;

        /// <returns>True or false randomly.</returns>
        public bool NextBool() => (NextUInt() & 1) == 1;

        /// <returns>A random Vector2 with components in range inclusive 0, exclusive 1.</returns>
        public UnityEngine.Vector2 NextVector2() => new UnityEngine.Vector2(NextFloat(), NextFloat());

        /// <returns>A random Vector3 with components in range inclusive 0, exclusive 1.</returns>
        public UnityEngine.Vector3 NextVector3() => new UnityEngine.Vector3(NextFloat(), NextFloat(), NextFloat());

        /// <returns>A random Vector4 with components in range inclusive 0, exclusive 1.</returns>
        public UnityEngine.Vector4 NextVector4() => new UnityEngine.Vector4(NextFloat(), NextFloat(), NextFloat(), NextFloat());

        /// <returns>A random Quaternion with components in range inclusive 0, exclusive 1.</returns>
        public UnityEngine.Quaternion NextQuaternion() => new UnityEngine.Quaternion(NextFloat(), NextFloat(), NextFloat(), NextFloat());

        /// <returns>A random Color with components in range inclusive 0, exclusive 1.</returns>
        /// <param name="andAlpha">If true, the alpha channel will also be randomized. If false, alpha will be set to 1.</param>
        public UnityEngine.Color NextColor(bool andAlpha = false) => new UnityEngine.Color(NextFloat(), NextFloat(), NextFloat(), andAlpha ? NextFloat() : 1f);
    }
}