using System;
using System.Collections.Generic;
using System.Linq;
using Neat.Util;
using UnityEngine;

namespace Neat {

    public class Neat {
        
        public const int MAX_NODE_BITS = 8;
        public const int MAX_NODES = 1 << MAX_NODE_BITS;

        public int[] inputNodes { get; }
        public int[] outputNodes { get; }

        public Dictionary<Property, float> properties { get; }
        public Dictionary<ConnectionGene, ConnectionGene> connections { get; private set; }
        public List<NodeGene> nodes { get; private set; }

        public List<Client> clients { get; private set; }
        private List<Species> species;

        public Neat(int inputNodes, int outputNodes) {
            this.inputNodes = new int[inputNodes];
            this.outputNodes = new int[outputNodes];

            properties = new Dictionary<Property, float>
                {
                    [Property.SPECIES_DISTANCE] = 4f,
                    [Property.EXCESS_FACTOR] = 1f,
                    [Property.DISJOINT_FACTOR] = 1f,
                    [Property.WEIGHT_FACTOR] = 1f,
                    [Property.RANDOM_WEIGHT_STRENGTH] = 1f,
                    [Property.SHIFT_WEIGHT_STRENGTH] = 0.25f,
                    [Property.MUTATE_RANDOM_WEIGHT] = 0.10f,
                    [Property.MUTATE_SHIFT_WEIGHT] = 0.25f,
                    [Property.MUTATE_TOGGLE_LINK] = 0.05f,
                    [Property.MUTATE_LINK] = 0.10f,
                    [Property.MUTATE_NODE] = 0.5f,
                    [Property.MUTATE_REMOVE_NODE] = 0.02f,
                    [Property.SURVIVAL_CHANCE] = 0.75f
                };
        }
        
        public void Reset() {
            nodes = new List<NodeGene>();
            
            for (int i = 0; i < inputNodes.Length; i++) {
                NodeGene node = CreateNode(NodeType.INPUT);
                inputNodes[i] = node.id;
                node.x = 0.1f;
                node.y = (i + 1.0f) / (inputNodes.Length + 1.0f);
            }

            for (int i = 0; i < outputNodes.Length; i++) {
                NodeGene node = CreateNode(NodeType.OUTPUT);
                outputNodes[i] = node.id;
                node.x = 0.9f;
                node.y = (i + 1.0f) / (outputNodes.Length + 1.0f);
            }

            connections = new Dictionary<ConnectionGene, ConnectionGene>();
        }

        
        // * ----- CREATION METHODS ----- * //
        
        
        public NodeGene CreateNode(NodeType type = NodeType.HIDDEN) {
            NodeGene node = new NodeGene(type, nodes.Count);
            nodes.Add(node);
            return node;
        }

        public ConnectionGene GetConnection(ConnectionGene connection) {
            ConnectionGene current = connections[connection];

            if (current == null) {
                int id = connections.Count;
                connection.id = id;
                connections[connection] = connection;
                return new ConnectionGene(connection);
            }

            connection.id = current.id;
            return connection;
        }

        public NodeGene GetNode(int id) {
            NodeGene node = nodes[id];
            return new NodeGene(node);
        }

        public int GetReplaceIndex(ConnectionGene connection) {
            ConnectionGene current = connections[connection];
            return current?.replaceId ?? 0;
        }

        public void SetReplaceIndex(ConnectionGene connection, int id) {
            ConnectionGene current = connections[connection];
            current.replaceId = id;
        }

        public void GenerateClients(int amount) {
            if (amount < 10 || amount > 1000)
                throw new Exception(amount + " is too many OR too few clients");
            
            clients = new List<Client>(amount);
            for (int i = 0; i < amount; i++) {
                clients.Add(new Client(this, i));
                clients[i].color = Color.HSVToRGB((float) i / amount, 1f, 1f);
            }

            species = new List<Species>();
        }

        public void Evolve() {
            species.ForEach(species => species.Reset());

            foreach (Client client in clients) {
                if (client.species != null) continue;

                bool found = species.Any(s => s.Put(client));
                if (!found) 
                    species.Add(new Species(client));
            }
            
            species.ForEach(species => species.Evaluate());
            
            float killPercentage = 1f - properties[Property.SURVIVAL_CHANCE];
            species.ForEach(species => species.Kill(killPercentage));

            // Remove extinct species. Since we just killed off
            // a percentage of their population, some species will
            // be too small to survive.
            species.RemoveAll(species => {
                if (species.clients.Count < 2) {
                    species.Kill();
                    return true;
                }

                return false;
            });
            
            // When reproducing (see below), we want successful species
            // to have a higher chance to have offspring. Thus we use the
            // average score of each client in the species as the chance
            // for a new offspring to belong to that species. 
            // TODO consider redoing score calculations AFTER removing?
            ProbabilityMap<Species> random = new ProbabilityMap<Species>();
            foreach (Species species in this.species) {
                random.Add(species, species.score);
            }

            // For any client who is currently dead, add it to a species.
            // When choosing the species, we BIAS to successful species. 
            foreach (Client client in clients) {
                if (client.species == null) {
                    Species species = random.Get();
                    client.genome = species.Breed();
                    species.Put(client, true);
                }
            }

            // Make a small tweak to every currently living species. 
            // TODO to make this better match nature, should we only
            // TODO tweak newly spawned clients?
            foreach (Client client in clients) {
                client.Mutate();
            }
        }

        public void Debug() {

            Console.WriteLine("SCORE         | CLIENTS");
            Console.WriteLine("-----------------------");
            foreach (Species species in species) {
                Console.WriteLine($"{species.score,13} | {species.clients.Count,7}");
            }
        }
    }
}