using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Neat {
    public class Mutation {

        public static readonly Mutation RANDOM_WEIGHT = new Mutation(MutateRandomWeight);
        public static readonly Mutation WEIGHT_SHIFT = new Mutation(MutateWeightShift);
        public static readonly Mutation TOGGLE = new Mutation(MutateToggle);
        public static readonly Mutation ADD_LINK = new Mutation(MutateAddLink);
        public static readonly Mutation ADD_NODE = new Mutation(MutateAddNode);
        public static readonly Mutation REMOVE_NODE = new Mutation(MutateRemoveNode);

        private readonly Func<Genome, bool> function;

        private Mutation(Func<Genome, bool> function) {
            this.function = function;
        }

        /**
         * Returns true if the genome was modified by the mutation. 
         */
        public bool Mutate(Genome genome) {
            return function.Invoke(genome);
        }

        public static bool HandleMutations(Genome genome) {
            Neat neat = genome.neat;
            bool mutated = false;

            if (neat.properties[Property.MUTATE_LINK] > Random.value)
                mutated = ADD_LINK.Mutate(genome);
            if (neat.properties[Property.MUTATE_NODE] > Random.value)
                mutated |= ADD_NODE.Mutate(genome);
            if (neat.properties[Property.MUTATE_RANDOM_WEIGHT] > Random.value)
                mutated |= RANDOM_WEIGHT.Mutate(genome);
            if (neat.properties[Property.MUTATE_SHIFT_WEIGHT] > Random.value)
                mutated |= WEIGHT_SHIFT.Mutate(genome);
            if (neat.properties[Property.MUTATE_TOGGLE_LINK] > Random.value)
                mutated |= TOGGLE.Mutate(genome);
            if (neat.properties[Property.MUTATE_REMOVE_NODE] > Random.value)
                mutated |= REMOVE_NODE.Mutate(genome);
            
            return mutated;
        }
        
        
        private static bool MutateRandomWeight(Genome genome) {
            if (genome.connections.Empty())
                return false;

            ConnectionGene connection = genome.connections.Random();
            float random = Random.Range(-1f, 1f);
            float property = genome.neat.properties[Property.RANDOM_WEIGHT_STRENGTH];
            connection.weight = random * property;
            return true;
        }

        private static bool MutateWeightShift(Genome genome) {
            if (genome.connections.Empty())
                return false;

            ConnectionGene connection = genome.connections.Random();
            float random = Random.Range(-1f, 1f);
            float property = genome.neat.properties[Property.SHIFT_WEIGHT_STRENGTH];
            connection.weight += random * property;
            return true;
        }
        
        private static bool MutateToggle(Genome genome) {
            if (genome.connections.Empty())
                return false;

            ConnectionGene connection = genome.connections.Random();
            connection.enabled = !connection.enabled;
            return true;
        }

        private static bool MutateAddLink(Genome genome) {
            const int ATTEMPTS = 20;

            for (int i = 0; i < ATTEMPTS; i++) {
                NodeGene a = genome.nodes.Random();
                NodeGene b = genome.nodes.Random();
                
                // Connections go left -> right
                if (Mathf.Approximately(a.x, b.x))
                    continue;

                // Make sure a is to the left of b
                if (a.x > b.x)
                    (a, b) = (b, a);

                ConnectionGene connection = new ConnectionGene(a, b);
                if (genome.connections.Contains(connection))
                    continue;

                connection = genome.neat.GetConnection(connection);
                connection.weight = 0;
                // TODO consider a random weight here?
                
                a.outgoing.Add(connection);
                b.incoming.Add(connection);
                genome.connections.AddSorted(connection);
                return true;
            }

            return false;
        }

        private static bool MutateAddNode(Genome genome) {
            if (genome.connections.Empty())
                return false;

            ConnectionGene connection = genome.connections.Random();
            NodeGene middle;
            
            int replaceId = genome.neat.GetReplaceIndex(connection);
            if (replaceId == 0) {
                middle = genome.neat.CreateNode();
                middle.x = (connection.from.x + connection.to.x) / 2f;
                middle.y = (connection.from.y + connection.to.y) / 2f;
                genome.neat.SetReplaceIndex(connection, middle.id);
            }
            else {
                middle = genome.neat.GetNode(replaceId);
            }

            ConnectionGene a = genome.neat.GetConnection(new ConnectionGene(connection.from, middle));
            ConnectionGene b = genome.neat.GetConnection(new ConnectionGene(middle, connection.to));

            a.weight = 1f;
            b.weight = connection.weight;
            b.enabled = connection.enabled;
            
            connection.from.outgoing.Add(a);
            middle.incoming.Add(a);
            middle.outgoing.Add(b);
            connection.to.incoming.Add(b);

            genome.connections.Remove(connection);
            genome.connections.AddSorted(a);
            genome.connections.AddSorted(b);

            genome.nodes.AddSorted(middle);
            return true;
        }

        private static bool MutateRemoveNode(Genome genome) {
            const int ATTEMPTS = 5;

            for (int i = 0; i < ATTEMPTS; i++) {
                int id = Random.Range(0, genome.nodes.size);
                NodeGene node = genome.neat.GetNode(id);

                if (node.type == NodeType.HIDDEN) {
                    genome.Remove(node);
                    return true;
                }
            }

            return false;
        }
    }
}