using System.Collections;
using System.Collections.Generic;
using Neat.Util;
using UnityEngine;

namespace Neat {
    public class Genome {

        public Neat neat { get; }
        public Util.SortedSet<ConnectionGene> connections { get; }
        public Util.SortedSet<NodeGene> nodes { get; }

        public Genome(Neat neat) {
            this.neat = neat;
            
            connections = new Util.SortedSet<ConnectionGene>(1 << (Neat.MAX_NODE_BITS << 1));
            nodes = new Util.SortedSet<NodeGene>(Neat.MAX_NODES);
            
            // Now we have to setup input and output nodes
            foreach (int id in neat.inputNodes) {
                nodes.Add(neat.GetNode(id));
            }

            foreach (int id in neat.outputNodes) {
                nodes.Add(neat.GetNode(id));
            }
        }

        public void Remove(NodeGene node) {
            IEnumerator<ConnectionGene> enumerator = node.incoming.GetEnumerator();
        }
        
        public float Distance(Genome g2) {
            Genome g1 = this;

            if ((g1.connections.last?.id ?? 0) < (g2.connections.last?.id ?? 0)) {
                g1 = g2;
                g2 = this;
            }
            
            // Keep track of gene data
            int disjoint = 0;
            int excess = 0;
            float weightDifference = 0f;
            int similar = 0;

            // Loop using iterators instead of index in order to take 
            // advantage of SortedSet performance benefits.
            Iterator<ConnectionGene> connections1 = g1.connections.Iterator();
            Iterator<ConnectionGene> connections2 = g2.connections.Iterator();

            ConnectionGene connection1 = null;
            ConnectionGene connection2 = null;
            bool skip1 = false;
            bool skip2 = false;
            int finalIndex = 0;

            while (connections1.HasNext() && connections2.HasNext()) {

                if (!skip1 && finalIndex++ >= 0)
                    connection1 = connections1.MoveNext();
                if (!skip2)
                    connection2 = connections2.MoveNext();

                // Reset skippers
                skip1 = skip2 = false;

                // Determine if the genes align, or are disjointed. We always
                // want them to be aligned so we can more accurately compare them.
                if (connection1.id == connection2.id) {
                    similar++;
                    weightDifference += Mathf.Abs(connection1.weight - connection2.weight);
                } else if (connection1.id > connection2.id) {
                    disjoint++;
                    skip1 = true;
                } else {
                    disjoint++;
                    skip2 = true;
                }
            }

            if (similar != 0) weightDifference /= similar;
            excess = g1.connections.size - finalIndex;
            float n = Mathf.Max(g1.connections.size, g2.connections.size);
            if (n < 20)
                n = 1;

            return disjoint / n * neat.properties[Property.DISJOINT_FACTOR]
                   + excess / n * neat.properties[Property.EXCESS_FACTOR]
                   + weightDifference * neat.properties[Property.WEIGHT_FACTOR];
        }
        
        public static Genome operator *(Genome g1, Genome g2) {
            Neat neat = g1.neat;
            Genome child = new Genome(neat);

            Iterator<ConnectionGene> connections1 = g1.connections.Iterator();
            Iterator<ConnectionGene> connections2 = g2.connections.Iterator();

            ConnectionGene connection1 = null;
            ConnectionGene connection2 = null;
            bool skip1 = false;
            bool skip2 = false;

            while (connections1.HasNext() && connections2.HasNext()) {

                if (!skip1)
                    connection1 = connections1.MoveNext();
                if (!skip2)
                    connection2 = connections2.MoveNext();

                // Reset skippers
                skip1 = skip2 = false;

                // Determine if the genes align, or are disjointed. We always
                // want them to be aligned so we can more accurately compare them.
                if (connection1.id == connection2.id) {
                    child.connections.Add(new ConnectionGene(Random.value > 0.5f ? connection1 : connection2));

                } else if (connection1.id > connection2.id) {
                    skip1 = true;
                } else {
                    child.connections.Add(new ConnectionGene(connection1));
                    skip2 = true;
                }
            }

            while (connections1.HasNext()) {
                child.connections.Add(new ConnectionGene(connections1.MoveNext()));
            }

            foreach (ConnectionGene c in child.connections) {
                child.nodes.Add(c.from);
                child.nodes.Add(c.to);
            }

            return child;
        }
    }
}