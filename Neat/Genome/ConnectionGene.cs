namespace Neat {
    public class ConnectionGene : Gene {

        public NodeGene from { get; }
        public NodeGene to { get; }
        public float weight { get; set; }
        public bool enabled { get; set; }
        public int replaceId { get; set; }

        public ConnectionGene(NodeGene from, NodeGene to) : base(0) {
            this.from = from;
            this.to = to;
            enabled = true;
        }

        public ConnectionGene(ConnectionGene other) : base(other.id) {
            from = other.from;
            to = other.to;
            weight = other.weight;
            enabled = other.enabled;
            replaceId = other.replaceId;
        } 

        protected bool Equals(ConnectionGene other) {
            return Equals(from, other.from) && Equals(to, other.to);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConnectionGene) obj);
        }

        public override int GetHashCode() {
            return from.id << Neat.MAX_NODE_BITS | to.id; 
        }
    }
}