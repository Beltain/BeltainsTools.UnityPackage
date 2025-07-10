using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BeltainsTools
{
    [System.Serializable]
    public class WeightedRandomGroup<T>
    {
        public List<WeightedItem<T>> Items;

        [System.Serializable]
        public class WeightedItem<T1> where T1 : T
        {
            public T1 Item;
            public int Weight;
        }

        public WeightedRandomGroup()
        {
            Items = new List<WeightedItem<T>>();
        }

        public WeightedRandomGroup(IEnumerable<T> collection, System.Func<T, int> weightCalculationFunction)
        {
            Items = new List<WeightedItem<T>>();
            foreach (T item in collection)
            {
                Items.Add(new WeightedItem<T>()
                {
                    Item = item,
                    Weight = weightCalculationFunction.Invoke(item)
                });
            }
        }

        /// <summary>Warning! Heavy on the garbage collector! Use sparingly!</summary>
        public T GetRandomItem(System.Func<T, bool> selectionFunction = null, System.Random random = null)
        {
            if (random == null)
                random = new System.Random();

            List<WeightedItem<T>> selectionList = selectionFunction == null ? Items : Items.Where(r => selectionFunction.Invoke(r.Item)).ToList();

            int sumRandomWeight = 0;
            foreach (WeightedItem<T> weightedItem in selectionList)
                sumRandomWeight += weightedItem.Weight;

            int randomRoll = random.Next(sumRandomWeight);
            foreach (WeightedItem<T> weightedItem in selectionList)
            {
                randomRoll -= weightedItem.Weight;
                if (randomRoll <= 0)
                    return weightedItem.Item;
            }
            //should never get here
            return default;
        }
    }
}
