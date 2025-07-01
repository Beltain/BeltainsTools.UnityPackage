using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeltainsTools.Pathfinding
{
    public static class AStar
    {
        public class Node<T> where T : new()
        {
            public T Object;

            public IEnumerable<T> Neighbours;

            public int HeuristicCost; // cost to target //h 
            public int TravelFromOriginCost; // cost from origin //g
            public int TotalCost; // total heuristic + travel from origin cost //f

            public Node<T> CameFrom; // the node from which the current cost was last calculated


            public Node(T originObject)
            {
                Object = originObject;
                HeuristicCost = int.MaxValue;
                TravelFromOriginCost = int.MaxValue;
                TotalCost = int.MaxValue;
                CameFrom = null;
            }

            public void RecalculateTotalCost()
            {
                TotalCost = TravelFromOriginCost == int.MaxValue || HeuristicCost == int.MaxValue ? int.MaxValue : TravelFromOriginCost + HeuristicCost;
            }
        }

        static T[] ConnectedNodesToPath<T>(Node<T> endNode) where T : new()
        {
            Stack<T> pathBackStack = new Stack<T>();

            Node<T> currentNode = endNode;
            do
            {
                pathBackStack.Push(currentNode.Object);
                currentNode = currentNode.CameFrom;
            }
            while (currentNode != null);

            T[] path = new T[pathBackStack.Count];
            for (int i = 0; i < path.Length; i++)
                path[i] = pathBackStack.Pop();

            return path;
        }

        /// <summary>Run the A* search algorithm from an origin object attempting to reach a target object, with neighbours calculated per visited-node</summary>
        /// <returns>An array of objects representing the path from the origin object to the target object (or closest if target was unreachable)</returns>
        public static T[] Solve<T>(T originObject, T targetObject, System.Func<Node<T>, IEnumerable<T>> getNeighboursFunction, System.Func<Node<T>, Node<T>, int> fromToCostFunction, System.Func<Node<T>, int> heuristicCostFunction, int maxIterations = 1000) where T : new()
        {
            // record all nodes
            Dictionary<T, Node<T>> nodeLookup = new Dictionary<T, Node<T>>();
            Node<T> GetOrCreateNode(T nodeObject)
            {
                if(!nodeLookup.ContainsKey(nodeObject))
                    nodeLookup[nodeObject] = new Node<T>(nodeObject);
                return nodeLookup[nodeObject];
            }

            int _CalculateTravelCost(Node<T> fromNode, Node<T> forNode)
            {
                return forNode.Object.Equals(originObject) ? 
                    0 : fromNode.TravelFromOriginCost + fromToCostFunction(fromNode, forNode);
            }

            int _CalculateHeuristic(Node<T> node)
            {
                return heuristicCostFunction.Invoke(node);
            }

            // openNodes are ones we still need to 'visit'
            List<Node<T>> openNodes = new List<Node<T>>() { GetOrCreateNode(originObject) };

            // Set start node costs
            openNodes[0].TravelFromOriginCost = _CalculateTravelCost(null, openNodes[0]);
            openNodes[0].HeuristicCost = _CalculateHeuristic(openNodes[0]);
            openNodes[0].RecalculateTotalCost();

            int iterations = 0;
            // Start the search loop
            while(openNodes.Count > 0 && (maxIterations == -1 || iterations < maxIterations))
            {
                iterations++;

                // sort elements by total cost so that the element at 0 is the lowest cost
                openNodes.Sort((Node<T> a, Node<T> b) => a.TotalCost.CompareTo(b.TotalCost));
                Node<T> curNode = openNodes[0]; // work hereafter on the element with the lowest cost

                if (curNode.Object.Equals(targetObject))
                    // we made it to the target! return the path!
                    return ConnectedNodesToPath(curNode); // TODO

                // generate neighbours so we can update them
                if (curNode.Neighbours == null)
                    curNode.Neighbours = getNeighboursFunction.Invoke(curNode);

                openNodes.Remove(curNode);
                foreach (T _neighbour in curNode.Neighbours)
                {
                    Node<T> neighbourNode = GetOrCreateNode(_neighbour);
                    // calc travel cost to neighbour from our current node
                    int travelCostFromCurNode = _CalculateTravelCost(curNode, neighbourNode);
                    if (travelCostFromCurNode >= neighbourNode.TravelFromOriginCost) // check if we're not a better path
                        continue; // it already has a better path

                    // update the best path to the neighbour and add it to the open set to continue calculations from
                    neighbourNode.CameFrom = curNode;
                    neighbourNode.HeuristicCost = _CalculateHeuristic(neighbourNode);
                    neighbourNode.TravelFromOriginCost = travelCostFromCurNode;
                    neighbourNode.RecalculateTotalCost();

                    if (!openNodes.Contains(neighbourNode)) // add as a node to calculate from
                        openNodes.Add(neighbourNode);
                }
            }

            //we couldn't finish the path to the target but just return the closest F cost one and call it a day
            Node<T> closest = nodeLookup.Values.OrderBy(r => r.HeuristicCost).FirstOrDefault();
            if (closest != null)
                return ConnectedNodesToPath(closest);
            else
            {
                d.LogWarning($"Unable to generate any kind of path between origin {originObject} and target {targetObject} after {iterations} iterations!");
                return new T[0];
            }
        }
    }
}
