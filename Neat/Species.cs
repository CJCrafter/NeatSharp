using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neat {
    public class Species {

        public List<Client> clients { get; }
        public Client @base { get; private set; }
        public float score { get; private set; }

        public Species(Client @base) {
            this.@base = @base ?? throw new Exception("null");
            this.@base.species = this;
            
            clients = new List<Client> {@base};
        }

        public Client Random() {
            if (clients.Count == 0)
                throw new Exception("Species is empty");
            
            return clients[UnityEngine.Random.Range(0, clients.Count)];
        }

        public bool Matches(Client client) {
            float distance = @base.genome.Distance(client.genome);
            float max = @base.neat.properties[Property.SPECIES_DISTANCE];
            return distance < max;
        }

        public bool Put(Client client, bool force = false) {
            if (force || Matches(client)) {
                client.species = this;
                clients.Add(client);
                return true;
            }

            return false;
        }

        public float Evaluate() {
            score = 0f;
            foreach (Client client in clients) {
                score += client.score;
            }

            score /= clients.Count;
            score = Mathf.Max(score, 0f);
            return score;
        }

        public void Kill() {
            foreach (Client client in clients) {
                client.species = null;
            }
            clients.Clear();
        }

        public void Kill(float percentage) {
            clients.Sort();
            int bound = (int) (percentage * clients.Count);

            IEnumerator<Client> iterator = clients.GetEnumerator();
            int count = 0;

            while (count++ < bound && iterator.MoveNext()) {
                iterator.Current.species = null;
                iterator.Dispose();
            }
        }

        public Genome Breed() {
            if (clients.Count == 0)
                return null;

            Client a = Random();
            Client b = Random();

            return a.genome * b.genome;
        }

        public void Reset() {
            @base = clients.Count == 0 ? @base : Random();
            Kill();
            clients.Clear();
            
            clients.Add(@base);
            score = 0f;
        }
    }
}