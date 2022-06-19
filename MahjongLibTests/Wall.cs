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
        public void Can_Only_Kan_Four_Times()
        {
            // Make sure CanKan stops at 4 times
            var kanCounter = 0;
            while (_wall.CanKan)
            {
                _wall.DrawKan();
                kanCounter++;
            }

            Assert.That(kanCounter, Is.EqualTo(4));
            
            // Try to kan again, should not work
            Assert.Throws<DeadWallExhaustedException>(() => _wall.DrawKan());
        }

        [Test]
        public void Dead_Wall_Is_Replenished()
        {
            // Check how big the live wall is
            var liveWallSize = _wall.LiveWall.Count;
            
            // Call a couple kans
            _wall.DrawKan();
            _wall.DrawKan();
            
            // Check how big the live wall is after kans
            var liveWallSizeAfterKans = _wall.LiveWall.Count;
            
            // Check that the live wall is exactly 2 shorter than it was
            Assert.That(liveWallSizeAfterKans, Is.EqualTo(liveWallSize - 2));
        }

        [Test]
        public void Live_Wall_Eventually_Exhausted()
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
        public void Doras_Eventually_Exhausted()
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
        public void Dora_And_Kan_Wall_Match()
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
        public void UraDoras_Match_Doras()
        {
            while (_wall.CanRevealDora)
            {
                _wall.RevealDora();
                Assert.That(_wall.Doras.Length, Is.EqualTo(_wall.UraDoras.Length));
            }
        }
    }
}