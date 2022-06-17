using System;
using System.Collections.Generic;
using System.Linq;
using DenshiMahjong.Utils;

namespace DenshiMahjong.Mahjong
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
            public Tile.WindDirection Source;
            public List<Tile> OwnTiles;
            public Tile CalledTile;
            public bool IsOpen => Type != CallType.Ankan;

            public Call(CallType Type, Tile.WindDirection Source, List<Tile> OwnTiles, Tile CalledTile)
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

        public Tile.WindDirection Wind;
        public int Index;
        public bool IsOpen => Calls.Count > 0 && Calls.Any(call => call.IsOpen);

        private readonly Wall _wall;

        public Player(Wall wall, int index)
        {
            this._wall = wall;
            Index = index;
        }

        public void DrawStartingHand()
        {
            Tiles = Enumerable.Range(0, 13).Select(_ => _wall.DrawTile()).ToList();
            OnNewHand?.Invoke();
        }

        public void DrawTile()
        {
            DrawnTile = _wall.DrawTile();
            OnTileDrawn?.Invoke(DrawnTile);
        }

        public void MakeCall(Call.CallType type, Tile.WindDirection source, Tile discardedTile, List<Tile> ownTiles)
        {
            var call = new Call(type, source, ownTiles, discardedTile);
            Calls.Add(call);
            OnCallMade?.Invoke(call);
        }

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

        public void DiscardDrawnTile()
        {
            var tile = DrawnTile;
            Discards.Add(tile);
            DrawnTile = null;
            OnTileDiscarded?.Invoke(tile, true);
        }

        public List<Tile> Sorted
        {
            get => Tiles.OrderBy(tile => tile).ToList();
        }
    }
}