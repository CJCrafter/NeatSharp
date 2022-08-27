using System.Diagnostics;
using NUnit.Framework;
using Neat;

namespace NeatTests;

public class NeatTest {

    private Neat.Neat neat;

    [SetUp]
    public void Setup() {
        neat = new Neat.Neat(2, 1);
        neat.Reset();
        neat.GenerateClients(100);
    }

    [Test]
    public void Test1() {

        float[] previous = new float[100];
        for (int i = 0; i < 500; i++) {
            for (var j = 0; j < neat.clients.Count; j++) {
                float[] result = neat.clients[j].brain.Calculate(new float[] { previous[j], 0f });
                previous[j] = result[0];

                neat.clients[j].score = result[0];
            }

            neat.Evolve();
        }

        neat.Debug();
        Assert.Pass();
    }
}