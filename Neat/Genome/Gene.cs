using System;

namespace Neat {
    public abstract class Gene : IComparable<Gene> {
        
        public int id { get; set; }

        protected Gene(int id) {
            this.id = id;
        }

        public override int GetHashCode() {
            return id;
        }

        public int CompareTo(Gene other) {
            return id < other.id ? -1 : (id == other.id ? 0 : 1);
        }
    }
}