using System;
using System.Linq;
using MahjongLib;
using NUnit.Framework;

namespace MahjongLibTests
{
    public class GameTests
    {
        private Game _game;

        [SetUp]
        public void SetUp()
        {
            _game = new Game();
        }

        [Test]
        public void Board_works_in_all_modes()
        {
            foreach (var mode in Enum.GetValues(typeof(Game.GameMode)).Cast<Game.GameMode>())
            {
                foreach (var wind in Enum.GetValues(typeof(Wind)).Cast<Wind>())
                {
                    for (var turn = 1; turn <= Game.PlayersForMode(mode); turn += 1)
                    {
                        var game = new Game();
                        var repeat = (int) mode;
                        game.Setup(mode, wind, turn, repeat);
                        
                        // Make sure the correct number of players was instanced
                        Assert.AreEqual(Game.PlayersForMode(mode), game.Players.Count);
                        
                        // Make sure the dealer is correct
                        Assert.AreEqual(Wind.East, game.Players[turn-1].Wind);
                        
                        // Bit meaningless but check the getters
                        Assert.AreEqual(mode, game.Mode);
                        Assert.AreEqual(wind, game.CurrentWind);
                        Assert.AreEqual(turn, game.Turn);
                        Assert.AreEqual(repeat, game.Repeat);
                    }
                }
            }
        }
    }
}