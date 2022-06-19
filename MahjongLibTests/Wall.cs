using MahjongLib;
using NUnit.Framework;

namespace MahjongLibTests
{
    [TestFixture]
    public class WallTests
    {
        private Wall _wall;

        [SetUp]
        public void SetUp()
        {
            var game = new Game();
            _wall = new Wall(game);
            _wall.NewGame();
        }

        [Test]
        public void Can_only_Kan_four_times()
        {
            // Make sure CanKan stops at 4 times
            var kanCounter = 0;
            while (_wall.CanKan)
            {
                _wall.DrawKan();
                kanCounter++;
            }

            Assert.AreEqual(4, kanCounter);

            // Try to kan again, should not work
            Assert.Throws<DeadWallExhaustedException>(() => _wall.DrawKan());
        }

        [Test]
        public void Dead_wall_is_replenished()
        {
            // Check how big the live wall is
            var liveWallSize = _wall.LiveWall.Count;

            // Call a couple kans
            _wall.DrawKan();
            _wall.DrawKan();

            // Check how big the live wall is after kans
            var liveWallSizeAfterKans = _wall.LiveWall.Count;

            // Check that the live wall is exactly 2 shorter than it was
            Assert.AreEqual(liveWallSize - 2, liveWallSizeAfterKans);
        }

        [Test]
        public void Live_wall_eventually_exhausted()
        {
            // Draw as much as we can
            while (_wall.CanDraw)
            {
                _wall.DrawTile();
            }

            // Drawing more should throw the appropriate exception
            Assert.Throws<LiveWallExhaustedException>(() => _wall.DrawTile());
        }

        [Test]
        public void Doras_wventually_wxhausted()
        {
            // Draw as much as we can
            while (_wall.CanRevealDora)
            {
                _wall.RevealDora();
            }

            // Drawing more should throw the appropriate exception
            Assert.Throws<DeadWallExhaustedException>(() => _wall.RevealDora());
        }

        [Test]
        public void Dora_and_Kan_wall_match()
        {
            // Dora wall should be 2 tile bigger than kan wall.
            // However each dora revealed actually involves 2 tiles, so a single RevealDora should sync them up.
            _wall.RevealDora();

            while (_wall.CanKan && _wall.CanRevealDora)
            {
                _wall.DrawKan();
                _wall.RevealDora();
            }

            // Dora and kan wall should both be exhausted
            Assert.That(_wall.CanKan, Is.False);
            Assert.That(_wall.CanRevealDora, Is.False);
        }

        [Test]
        public void UraDoras_match_Doras()
        {
            while (_wall.CanRevealDora)
            {
                _wall.RevealDora();
                Assert.AreEqual(_wall.Doras.Length, _wall.UraDoras.Length);
            }
        }
    }
}