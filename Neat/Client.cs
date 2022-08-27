using System;
using UnityEngine;

namespace Neat {
    public class Client : IComparable<Client> {
        
        public int id { get; }
        public Neat neat { get; }

        public Genome genome;
        public float score;
        public Species species;
        public Color color;

        private Brain _brain;
        public Brain brain => _brain ?? (_brain = new Brain(genome));

        public Client(Neat neat, int id) {
            this.neat = neat;
            this.id = id;

            genome = new Genome(neat);
        }

        public void Mutate() {
            bool mutated = Mutation.HandleMutations(genome);
            if (mutated)
                _brain = null;
        }

        public int CompareTo(Client other) {
            return score.CompareTo(other.score);
        }

        protected bool Equals(Client other) {
            return id == other.id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Client) obj);
        }

        public override int GetHashCode() {
            return id;
        }
    }
}