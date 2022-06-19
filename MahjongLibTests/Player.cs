using MahjongLib;
using NUnit.Framework;

namespace MahjongLibTests
{
    public class PlayerTests
    {
        private Game _game;
        private Player _player;

        [SetUp]
        public void SetUp()
        {
            _game = new Game();
            _game.Setup(Game.GameMode.Riichi, Wind.East, 1, 0);
            _game.Start();
            _game.SetupBoard();
            _player = _game.ActivePlayer;
        }

        [Test]
        public void Sorted_must_match_tiles()
        {
            // Check that the two list matches by removing tiles
            var sorted = _player.Sorted;
            var tiles = _player.Tiles;
            foreach (var tile in sorted)
            {
                var found = tiles.FindIndex(t => t == tile);
                if (found == -1)
                {
                    Assert.Fail($"Tile {tile} not found in tiles");
                }
                tiles.RemoveAt(found);
            }
            Assert.That(tiles.Count, Is.EqualTo(0));
        }
    }
}