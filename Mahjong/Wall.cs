using System;
using System.Collections.Generic;
using System.Linq;
using DenshiMahjong.Utils;

namespace DenshiMahjong.Mahjong
{
	public class Wall
	{
		public event Action<Tile> OnDoraRevealed;
		
		// Live wall
		private Queue<Tile> liveWall;

		public bool CanDraw => liveWall.Count > 0;

		// Dead wall
		private List<Tile> doraWall;
		private List<Tile> kanWall;
		private int revealedDoras;

		public bool CanKan => CanDraw && kanWall.Count > 0;
		public Tile[] Doras => doraWall.Take(revealedDoras).ToArray();
		public Tile[] UraDoras => doraWall.Skip(doraWall.Count / 2).Take(revealedDoras).ToArray();

		public Tile DrawTile() => liveWall.Dequeue();

		public void RevealDora()
		{
			revealedDoras += 1;
			OnDoraRevealed?.Invoke(Doras.Last());
			GameLog.Log($"Dora tile revealed: {Doras.Last()} (the dora is {Doras.Last().Next})");
		}

		public Tile DrawKan()
		{
			// Must be able to draw a kan
			if (!CanKan)
			{
				throw new Exception("Cannot draw a kan");
			}

			// Draw from the kan wall
			var tile = kanWall[0];
			kanWall.RemoveAt(0);

			// Pop tile from live wall to replace dead wall draw
			liveWall = new Queue<Tile>(liveWall.ToList().Take(liveWall.Count - 1));

			return tile;
		}

		public void NewGame()
		{
			// Create and shuffle tiles
			var tiles = ShuffleTiles(PrepareTiles());

			// Create dora and kan wall from first N tiles
			const int kanCount = 4;
			const int doraCount = (1 + kanCount) * 2;
			kanWall = tiles.Take(kanCount).ToList();
			tiles.RemoveRange(0, kanCount);
			doraWall = tiles.Take(doraCount).ToList();
			tiles.RemoveRange(0, doraCount);

			// Create live wall from remaining tiles
			liveWall = new Queue<Tile>(tiles);

			// Reset revealed doras
			revealedDoras = 0;
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
}
