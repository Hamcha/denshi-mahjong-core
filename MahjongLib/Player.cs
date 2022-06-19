using System;
using System.Collections.Generic;
using System.Linq;

namespace MahjongLib
{
    public class Player
    {
        public struct Call
        {
            public enum CallType
            {
                Pon,
                Chii,
                Ankan,
                Chakan,
                Daiminkan
            }

            public CallType Type;
            public Wind Source;
            public List<Tile> OwnTiles;
            public Tile CalledTile;
            public bool IsOpen => Type != CallType.Ankan;

            public Call(CallType Type, Wind Source, List<Tile> OwnTiles, Tile CalledTile)
            {
                this.Type = Type;
                this.Source = Source;
                this.OwnTiles = OwnTiles;
                this.CalledTile = CalledTile;
            }
        }

        public event Action OnNewHand;
        public event Action<Tile, bool> OnTileDiscarded;
        public event Action<Tile> OnTileDrawn;
        public event Action<Call> OnCallMade;

        public List<Tile> Tiles { get; private set; }
        public List<Call> Calls { get; private set; } = new List<Call>();
        public Tile DrawnTile { get; private set; }
        public List<Tile> Discards { get; private set; } = new List<Tile>();

        public Wind Wind;
        public int Index;
        public bool IsOpen => Calls.Count > 0 && Calls.Any(call => call.IsOpen);

        private readonly Game _game;
        private Wall Wall => _game.Wall;

        public Player(Game game, int index)
        {
            this._game = game;
            Index = index;
        }

        /// <summary>
        /// Draws the starting hand (13 tiles) from the wall
        /// </summary>
        public void DrawStartingHand()
        {
            Tiles = Enumerable.Range(0, 13).Select(_ => Wall.DrawTile()).ToList();
            OnNewHand?.Invoke();
        }

        /// <summary>
        /// Draws a tile from the wall
        /// </summary>
        public void DrawTile()
        {
            DrawnTile = Wall.DrawTile();
            OnTileDrawn?.Invoke(DrawnTile);
        }

        /// <summary>
        /// Makes a call on a tile.
        /// </summary>
        /// <param name="type">Type of call</param>
        /// <param name="source">Which player discarded that call (or in case of a Shouminkan or Ankan, the player itself)</param>
        /// <param name="discardedTile">Which tile to call on</param>
        /// <param name="ownTiles">Tiles from hand to use for call</param>
        public void MakeCall(Call.CallType type, Wind source, Tile discardedTile, List<Tile> ownTiles)
        {
            var call = new Call(type, source, ownTiles, discardedTile);
            Calls.Add(call);
            //TODO: Remove tiles from call, other things
            OnCallMade?.Invoke(call);
        }

        /// <summary>
        /// Discards a tile from the player's hand and adds it to the discard pile.
        /// Additionally, if the players has drawn a tile, it is added to the hand.
        /// </summary>
        /// <param name="tile"></param>
        public void DiscardForTurn(Tile tile)
        {
            Discards.Add(tile);
            Tiles.Remove(tile);
            if (DrawnTile != null)
            {
                Tiles.Add(DrawnTile);
                DrawnTile = null;
            }

            OnTileDiscarded?.Invoke(tile, false);
        }

        /// <summary>
        /// Discards the tile that was just drawn.
        /// </summary>
        public void DiscardDrawnTile()
        {
            var tile = DrawnTile;
            Discards.Add(tile);
            DrawnTile = null;
            OnTileDiscarded?.Invoke(tile, true);
        }

        /// <summary>
        /// Returns a sorted list of all the tiles the player has in their hand.
        /// </summary>
        public List<Tile> Sorted
        {
            get => Tiles.OrderBy(tile => tile).ToList();
        }
        
        private int CountOf(Tile tile)
        {
            return Tiles.Count(t => t == tile);
        }

        /// <summary>
        /// Checks if the player can call Pon on the given tile.
        /// </summary>
        /// <param name="tile">Tile to check for</param>
        /// <returns>true if Pon can be called, false otherwise</returns>
        public bool CanPon(Tile tile)
        {
            // Can only Pon if you have at least two identical tiles in your hand
            return CountOf(tile) >= 2;
        }
        
        /// <summary>
        /// Checks if the player can call Daiminkan or Ankan on the given tile.
        /// </summary>
        /// <param name="tile">Tile to check for</param>
        /// <returns>true if Kan can be called, false otherwise</returns>
        public bool CanKan(Tile tile)
        {
            // Can only Kan if you have at least three identical tiles in your hand
            return CountOf(tile) >= 3;
        }

        /// <summary>
        /// Checks if the player can call Shouminkan on the given tile.
        /// </summary>
        /// <param name="tile">Tile to check for</param>
        /// <returns>true if Kan can be called, false otherwise</returns>
        public bool CanShouminkan(Tile tile)
        {
            // Can only upgrade to kan if you have at least a pon of the same tiles
            return Calls.Any(call => call.Type == Call.CallType.Pon && call.CalledTile == tile);
        }

        /// <summary>
        /// Checks if the player can call Chii on the given tile.
        /// </summary>
        /// <param name="tile">Tile to check for</param>
        /// <returns>true if Chii can be called, false otherwise</returns>
        public bool CanChii(Tile tile)
        {
            // Can only chii non-honor tiles
            if (tile.IsHonor)
            {
                return false;
            }

            // All possible combinations you can chii
            var chiiValid = new List<int[]>();

            // Check valid chii combinations depending on tile value
            if (!tile.IsTerminal)
            {
                chiiValid.Add(new[] {tile.value - 1, tile.value + 1});
            }

            if (tile.value < 8)
            {
                chiiValid.Add(new[] {tile.value + 1, tile.value + 2});
            }
            else if (tile.value > 2)
            {
                chiiValid.Add(new[] {tile.value - 1, tile.value - 2});
            }

            // Chii is ok if any possible sequence is satisfied by having all the matching tiles in that sequence
            return chiiValid.Any(seq => seq.All(value => Tiles.Any(t => t.kind == tile.kind && t.value == value)));
        }

        /// <summary>
        /// Returns the calls the player can make on the given tile.
        /// </summary>
        /// <param name="tile">Tile to check for</param>
        /// <param name="source">Player wind that discarded the tile</param>
        /// <returns>List of type of calls the player can perform on the tile</returns>
        public List<Call.CallType> ValidCalls(Tile tile, Wind source)
        {
            // Make list to store valid calls
            var validCalls = new List<Call.CallType>();

            // We can pon/kan regardless of wind!
            switch (CountOf(tile))
            {
                case 3:
                    validCalls.Add(Call.CallType.Daiminkan);
                    goto case 2;
                case 2:
                    validCalls.Add(Call.CallType.Pon);
                    break;
            }
            
            // We can only chii from the player to our left
            if (_game.PreviousWind(Wind) == source && CanChii(tile))
            {
                validCalls.Add(Call.CallType.Chii);
            }

            return validCalls;
        }
    }
}