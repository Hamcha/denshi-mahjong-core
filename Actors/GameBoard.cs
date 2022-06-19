using System.Collections.Generic;
using System.Linq;
using DenshiMahjong.Utils;
using Godot;
using MahjongLib;

namespace DenshiMahjong.Actors
{
	public class GameBoard : Node
	{
		// Logic
		public Game CurrentGame;

		// UI
		private Label _prevalentWindLabel;
		private Label _turnLabel;
		private List<HBoxContainer> _playerHands;
		private List<Label> _playerWindLabels;
		private List<Control> _playerDrawnTiles;
		private List<GridContainer> _playerDiscards;
		private PackedScene _tileNode = (PackedScene) GD.Load("res://Actors/Tile2D.tscn");

		private Color activeTurnColor = Colors.White;
		private Color idleTurnColor = Color.Color8(255, 255, 255, 160);

		// Editor / Debug
		[Export] public bool SingleClient = false;
		private GameLog _log = new GameLog();
		
		public override void _Ready()
		{
			_prevalentWindLabel = GetNode("Prevalent") as Label;
			_turnLabel = GetNode("Prevalent/Repeat") as Label;

			_playerHands = GetNode("SC/Players").GetChildren().Cast<HBoxContainer>().ToList();
			_playerWindLabels = GetNode("SC/Winds").GetChildren().Cast<Label>().ToList();
			_playerDrawnTiles = GetNode("SC/DrawnTiles").GetChildren().Cast<Control>().ToList();
			_playerDiscards = GetNode("Discards").GetChildren().Cast<GridContainer>().ToList();

			StartNextGame(Game.GameMode.Riichi, Wind.East, 1);
		}

		public void StartNextGame(Game.GameMode mode, Wind wind, int repeat)
		{
			_log.Log($"New game - {mode} ({Game.PlayersForMode(mode)} players) - {wind} {repeat}");
			CurrentGame = new Game(_log, new GodotTime());
			CurrentGame.Setup(mode, wind, 1, 0);
			CurrentGame.State.OnStateChanged += OnGameStateChanged;
			CurrentGame.Players.ForEach(player =>
			{
				player.OnNewHand += () => OnPlayerNewHand(player);
				player.OnTileDrawn += tile => OnTileDrawn(player, tile);
				player.OnTileDiscarded += (tile, drawn) => OnTileDiscarded(player, tile, drawn);
			});
			CurrentGame.Start();

			// Update UI elements
			_prevalentWindLabel.Text = CurrentGame.CurrentWind.ToString();
			_turnLabel.Text = CurrentGame.Turn.ToString();

			// In SC mode, update the player wind labels
			if (SingleClient)
			{
				CurrentGame.Players.ForEach(player =>
				{
					_playerWindLabels[player.Index].Text = player.Wind.ToString();
				});
			}
		}

		private void OnGameStateChanged(Game.GameState newGameState)
		{
			switch (newGameState)
			{
				case Game.GameState.PlayerTurnBegin:
					CurrentGame.Players.ForEach(player =>
					{
						_playerHands[player.Index].Modulate =
							player.Wind == CurrentGame.CurrentTurn ? activeTurnColor : idleTurnColor;
					});
					break;
			}
		}

		private void OnTileDiscarded(Player player, Tile tile, bool justDrawn)
		{
			var tileNode = _tileNode.Instance() as Tile2D;
			tileNode.Data = tile;
			tileNode.Modulate = justDrawn ? idleTurnColor : activeTurnColor;
			_playerDiscards[player.Index].AddChild(tileNode);
			if (SingleClient)
			{
				SCRefreshHand(player);
			}
		}

		private void OnPlayerNewHand(Player player)
		{
			_log.Log($"{player.Wind} player drew {player.Tiles.Count} tiles ({string.Join(", ", player.Sorted)})");

			// Populate hand in SC mode
			if (SingleClient)
			{
				SCRefreshHand(player);
			}
		}

		private void OnTileDrawn(Player player, Tile drawnTile)
		{
			_log.Log($"{player.Wind} player drew {drawnTile}");
			if (SingleClient)
			{
				SCRefreshHand(player);
			}
		}

		/// <summary>
		/// Refresh the hand in SC mode. This method is incredibly slow and expensive, but it's only used when debugging.
		/// </summary>
		/// <param name="player">Player to refresh the hand of</param>
		private void SCRefreshHand(Player player)
		{
			// Clear hand
			foreach (var child in _playerHands[player.Index].GetChildren())
			{
				(child as Node).QueueFree();
			}

			foreach (var child in _playerDrawnTiles[player.Index].GetChildren())
			{
				(child as Node).QueueFree();
			}

			// Create tiles for each tile drawn
			player.Sorted.ForEach(tile =>
			{
				var tileNode = _tileNode.Instance() as Tile2D;
				tileNode.Data = tile;
				tileNode.OnTileClicked += () =>
				{
					if (CurrentGame.ActivePlayer == player &&
						CurrentGame.CurrentState == Game.GameState.WaitingForDiscard)
					{
						player.DiscardForTurn(tile);
					}
				};
				_playerHands[player.Index].AddChild(tileNode);
			});

			// Check drawn tile
			if (player.DrawnTile != null)
			{
				var tileNode = _tileNode.Instance() as Tile2D;
				tileNode.Data = player.DrawnTile;
				tileNode.OnTileClicked += () =>
				{
					if (CurrentGame.ActivePlayer == player &&
						CurrentGame.CurrentState == Game.GameState.WaitingForDiscard)
					{
						player.DiscardDrawnTile();
					}
				};
				_playerDrawnTiles[player.Index].AddChild(tileNode);
			}
		}
	}
}
