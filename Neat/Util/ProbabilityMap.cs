using System;
using Random = UnityEngine.Random;

namespace Neat.Util {
    public class ProbabilityMap<E> {

        private Node dummy;
        private System.Collections.Generic.SortedSet<Node> set;
        private float totalProbability;

        public ProbabilityMap() {
            dummy = new Node();
            
            set = new System.Collections.Generic.SortedSet<Node>();
        }

        public bool Add(E element, float chance) {
            Node node = new Node(element, chance, totalProbability);
            if (set.Add(node)) {
                totalProbability += chance;
                return true;
            }

            return false;
        }

        public E Get() {
            float random = Random.Range(0f, totalProbability);
            dummy.offset = random;

            return set.GetViewBetween(set.Min, dummy).Max.value;
        }
        
        private class Node : IComparable<Node> {
            
            internal E value { get; }
            internal float chance { get; }
            internal float offset { get; set; }

            internal Node() {}
            
            internal Node(E value, float chance, float offset) {
                this.value = value;
                this.chance = chance;
                this.offset = offset;
            }

            public int CompareTo(Node other) {
                return offset.CompareTo(other.offset);
            }
        }
    }
}