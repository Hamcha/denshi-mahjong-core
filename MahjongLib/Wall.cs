using System;
using System.Collections.Generic;
using System.Linq;

namespace MahjongLib
{
    public class Wall
    {
        public event Action<Tile> OnDoraRevealed;

        // Live wall
        private Queue<Tile> _liveWall;
        public List<Tile> LiveWall => _liveWall.ToList();

        public bool CanDraw => _liveWall.Count > 0;

        // Dead wall
        private List<Tile> _doraWall;
        private List<Tile> _kanWall;
        public int RevealedDoras { get; private set; }

        public bool CanKan => CanDraw && _kanWall.Count > 0;
        public Tile[] Doras => _doraWall.Take(RevealedDoras).ToArray();
        public Tile[] UraDoras => _doraWall.Skip(_doraWall.Count / 2).Take(RevealedDoras).ToArray();
        public bool CanRevealDora => RevealedDoras < (_doraWall.Count / 2);

        public Tile DrawTile()
        {
            try
            {
                return _liveWall.Dequeue();
            }
            catch (InvalidOperationException)
            {
                throw new LiveWallExhaustedException("No tiles left in wall");
            }
        }

        private readonly Game _game;

        public Wall(Game game)
        {
            _game = game;
        }

        public void RevealDora()
        {
            // Must be able to draw a kan
            if (!CanRevealDora)
            {
                throw new DeadWallExhaustedException("All doras have been revealed already");
            }
            RevealedDoras += 1;
            OnDoraRevealed?.Invoke(Doras.Last());
            _game.Logger.Log($"Dora tile revealed: {Doras.Last()} (the dora is {Doras.Last().Next})");
        }

        /// <summary>
        /// Draws a tile from the dead wall and takes one tile from the live wall as replacement.
        /// Can only be called if CanKan is true (ie. there are still tiles in the kan wall to draw from).
        /// </summary>
        /// <returns>Tile drawn</returns>
        /// <exception cref="DeadWallExhaustedException">Kan wall is empty</exception>
        public Tile DrawKan()
        {
            // Must be able to draw a kan
            if (!CanKan)
            {
                throw new DeadWallExhaustedException("Cannot draw a kan");
            }

            // Draw from the kan wall
            var tile = _kanWall[0];
            _kanWall.RemoveAt(0);

            // Pop tile from live wall to replace dead wall draw
            _liveWall = new Queue<Tile>(_liveWall.ToList().Take(_liveWall.Count - 1));

            return tile;
        }

        public void NewGame()
        {
            // Create and shuffle tiles
            var tiles = ShuffleTiles(PrepareTiles());

            // Create dora and kan wall from first N tiles
            const int kanCount = 4;
            const int doraCount = (1 + kanCount) * 2;
            _kanWall = tiles.Take(kanCount).ToList();
            tiles.RemoveRange(0, kanCount);
            _doraWall = tiles.Take(doraCount).ToList();
            tiles.RemoveRange(0, doraCount);

            // Create live wall from remaining tiles
            _liveWall = new Queue<Tile>(tiles);

            // Reset revealed doras
            RevealedDoras = 0;
        }

        /// <summary>
        /// Prepares a list of all tiles in the game.
        /// Tiles include 4 of each of the following:
        ///   - All tiles from 1 to 9 for each suit
        ///   - All honors
        /// </summary>
        /// <returns>List of tiles</returns>
        private List<Tile> PrepareTiles()
        {
            var numberTiles = from suit in new[] {Tile.Kind.Bamboo, Tile.Kind.Character, Tile.Kind.Circle}
                from value in Enumerable.Range(1, 9)
                select new Tile(suit, value);

            var windTiles = from wind in Enum.GetValues(typeof(Wind)).Cast<int>()
                select new Tile(Tile.Kind.Wind, wind);

            var dragonTiles = from dragon in Enum.GetValues(typeof(Dragon)).Cast<int>()
                select new Tile(Tile.Kind.Dragon, dragon);

            return numberTiles
                .Concat(windTiles)
                .Concat(dragonTiles)
                .SelectMany(tile => Enumerable.Repeat(tile, 4))
                .ToList();
        }

        /// <summary>
        /// Shuffles a list of elements randomly in linear time. List is shuffled in-place!
        /// </summary>
        /// <param name="input">List to shuffle</param>
        /// <typeparam name="T">Type of the elements</typeparam>
        /// <returns>Shuffled list</returns>
        private List<T> ShuffleTiles<T>(List<T> input)
        {
            // Create Random instance with new time seed
            Random rng = new Random();

            // O(n) shuffle by picking random indices and swapping around
            for (int index = input.Count - 1; index > 0; index -= 1)
            {
                // Pick random new index
                int targetIndex = rng.Next(0, index + 1);

                // Swap indices
                (input[index], input[targetIndex]) = (input[targetIndex], input[index]);
            }

            return input;
        }
    }

    public class DeadWallExhaustedException : Exception
    {
        public DeadWallExhaustedException(string message) : base(message)
        {
        }
    }

    public class LiveWallExhaustedException : Exception
    {
        public LiveWallExhaustedException(string message) : base(message)
        {
        }
    }
}