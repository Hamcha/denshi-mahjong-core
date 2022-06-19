using System;
using MahjongLib;
using NUnit.Framework;

namespace MahjongLibTests
{
    public class TileTests
    {
        [Test]
        public void Tiles_sort_correctly()
        {
            // Tiles of the same kind should be sorted by value
            Assert.That(new Tile(Tile.Kind.Bamboo, 9), Is.GreaterThan(new Tile(Tile.Kind.Bamboo, 8)));
            Assert.That(new Tile(Tile.Kind.Character, 6), Is.GreaterThan(new Tile(Tile.Kind.Character, 2)));
            Assert.That(new Tile(Tile.Kind.Circle, 1), Is.LessThan(new Tile(Tile.Kind.Circle, 4)));

            // Tiles of different kinds should be sorted by fixed kind order (Character < Bamboo < Circle < Wind < Dragon)
            Assert.That(new Tile(Tile.Kind.Character, 1), Is.LessThan(new Tile(Tile.Kind.Bamboo, 1)));
            Assert.That(new Tile(Tile.Kind.Bamboo, 1), Is.LessThan(new Tile(Tile.Kind.Circle, 1)));
            Assert.That(new Tile(Tile.Kind.Circle, 1), Is.LessThan(new Tile(Tile.Kind.Wind, 1)));
            Assert.That(new Tile(Tile.Kind.Wind, 1), Is.LessThan(new Tile(Tile.Kind.Dragon, 1)));
            Assert.That(new Tile(Tile.Kind.Dragon, 1), Is.GreaterThan(new Tile(Tile.Kind.Character, 2)));
        }

        [Test]
        public void Tiles_Is_checks_work()
        {
            Assert.That(new Tile(Tile.Kind.Bamboo, 1).IsHonor, Is.False);
            Assert.That(new Tile(Tile.Kind.Dragon, 1).IsHonor, Is.True);
            Assert.That(new Tile(Tile.Kind.Character, 1).IsTerminal, Is.True);
            Assert.That(new Tile(Tile.Kind.Bamboo, 2).IsTerminal, Is.False);
        }

        [Test]
        public void Error_conditions_work()
        {
            // CompareTo should only work with other tiles
            Assert.Throws<ArgumentException>(() => new Tile(Tile.Kind.Bamboo, 1).CompareTo(new object()));
            
            // When given an invalid kind, the string representation should be "UNK" (like "Unknown")
            Assert.AreEqual("UNK", new Tile((Tile.Kind) 999, 999).ToString());
        }
    }
}