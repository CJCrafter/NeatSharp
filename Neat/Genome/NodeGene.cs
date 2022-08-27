using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neat {
    public class NodeGene : Gene {

        // Universal for every node that shares an id
        public NodeType type { get; }
        public float x;
        public float y;

        // Changes per genome
        public List<ConnectionGene> incoming;
        public List<ConnectionGene> outgoing;
        public float brainValue;
        
        public NodeGene(NodeType type, int id) : base(id) {
            this.type = type;
            
            incoming = new List<ConnectionGene>();
            outgoing = new List<ConnectionGene>();
        }

        /**
         * Copy constructor is super important. Nodes are "shared" between
         * genomes, but don't want the reference to point to the same node.
         * This constructor is used to clone the node. Since base nodes don't
         * have connections, we don't need to copy them. 
         */
        public NodeGene(NodeGene node) : base(node.id) {
            type = node.type;
            x = node.x;
            y = node.y;

            incoming = new List<ConnectionGene>();
            outgoing = new List<ConnectionGene>();
        }

        protected bool Equals(NodeGene other) {
            return id == other.id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NodeGene) obj);
        }

        public float Calculate() {
            float sum = incoming.Where(connection => connection.enabled).Sum(connection => connection.weight * connection.@from.brainValue);
            brainValue = 1f / (1f + Mathf.Exp(-sum));
            return brainValue;
        }
    }
}