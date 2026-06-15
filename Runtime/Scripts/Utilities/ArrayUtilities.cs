using UnityEngine;

namespace BeltainsTools.Utilities
{
    public static class ArrayUtilities
    {
        //public static void Test()
        //{
        //    RaycastHit[] hits = null;
        //    Ray ray = new Ray();
        //    DoAndFit(ref hits, array => Physics.RaycastNonAlloc(ray, array));
        //}

        /// <summary>Tries to execute the <paramref name="arraySizeTargetFunc"/> and if the result doesn't fit within the array, resizes the array and repeats the func</summary>
        public static void DoAndFit<T>(ref T[] array, System.Func<T[], int> arraySizeTargetFunc)
        {
            int requiredSize = arraySizeTargetFunc.Invoke(array);
            if (!TryFitPowTwo(ref array, requiredSize))
                arraySizeTargetFunc.Invoke(array);
        }

        /// <summary>Resizes the array until it's larger or equal to the required size and at the next highest power of two size (10(8) -> 16, 7(4) -> 8, 16 -> 32)</summary>
        /// <returns>Whether the array can currently fit the required size (true if no resize was necessary)</returns>
        public static bool TryFitPowTwo<T>(ref T[] array, int requiredSize)
        {
            int arraySize = array != null ? array.Length : 0;
            if (arraySize >= requiredSize)
                return true;

            int idealArraySize = GetNextPowerOfTwoFrom(requiredSize);
            if (array == null)
                array = new T[idealArraySize];
            else
                System.Array.Resize(ref array, idealArraySize);

            return false;
        }

        private static int GetNextPowerOfTwoFrom(int num)
        {
            if (num == int.MaxValue)
                return num;

            int pow2Num = 1;
            while (pow2Num < num)
                pow2Num *= 2;
            return pow2Num;
        }
    }
}
