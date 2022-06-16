using System.Collections.Generic;
using System.Linq;

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

        private Wall wall;
        private List<Tile> tiles;
        private List<Call> calls;
        private Tile drawnTile;

        public Tile.WindDirection wind;
        public bool IsOpen => calls.Count > 0 && calls.Any(call => call.IsOpen);

        public Player(Wall wall, Tile.WindDirection wind)
        {
            this.wall = wall;
            this.wind = wind;
        }

        public void DrawStartingHand()
        {
            tiles = Enumerable.Range(0, 13).Select(_ => wall.DrawTile()).ToList();
        }

        public void DrawTile()
        {
            drawnTile = wall.DrawTile();
        }

        public void MakeCall(Call.CallType type, Tile.WindDirection source, Tile discardedTile, List<Tile> ownTiles)
        {
            calls.Add(new Call(type, source, ownTiles, discardedTile));
        }

        public void DiscardForTurn(Tile tile)
        {
            tiles.Remove(tile);
            if (drawnTile != null)
            {
                tiles.Add(drawnTile);
                drawnTile = null;
            }
        }
    }
}