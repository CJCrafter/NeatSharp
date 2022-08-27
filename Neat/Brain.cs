using System;
using System.Collections.Generic;

namespace Neat {
    public class Brain {

        private readonly List<NodeGene> input;
        private readonly List<NodeGene> hidden;
        private readonly List<NodeGene> output;

        public Brain(Genome genome) {
            input = new List<NodeGene>(genome.neat.inputNodes.Length);
            output = new List<NodeGene>(genome.neat.outputNodes.Length);
            hidden = new List<NodeGene>(genome.nodes.size - (input.Capacity + output.Capacity));

            foreach (NodeGene node in genome.nodes) {
                switch (node.type) {
                    case NodeType.INPUT:
                        input.Add(node);
                        break;
                    case NodeType.HIDDEN:
                        hidden.Add(node);
                        break;
                    case NodeType.OUTPUT:
                        output.Add(node);
                        break;
                    default:
                        throw new Exception("Unknown type: " + node.type);
                }
            }
            
            hidden.Sort(new NodeCompare());
        }

        public float[] Calculate(float[] input) {
            if (input.Length != this.input.Count)
                throw new Exception("There are " + this.input.Count + " input nodes! You used " + input.Length);

            for (int i = 0; i < input.Length; i++)
                this.input[i].brainValue = input[i];

            foreach (NodeGene node in hidden)
                node.Calculate();
            
            float[] output = new float[this.output.Count];
            for (int i = 0; i < output.Length; i++) {
                output[i] = this.output[i].Calculate();
            }

            return output;
        }

        private class NodeCompare : IComparer<NodeGene> {
            public int Compare(NodeGene x, NodeGene y) {
                if (x == null || y == null)
                    throw new Exception("A null node");
                
                return x.x.CompareTo(y.x);
            }
        }
    }
}