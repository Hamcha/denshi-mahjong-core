using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DenshiMahjong.Mahjong
{
    public class GameState : Node
    {
        public enum State
        {
            WaitingToStart,
            GameStart,
            PlayerTurnBegin,
            WaitingForDiscard,
            WaitingForCall,
            PlayerWon,
            ExhaustiveDraw,
        }

        public enum GameMode
        {
            Riichi, // 4 players
            Sanma, // 3 players
            Minefield // 2 players
        }

        GameMode mode;
        State currentState;
        Wall wall;
        List<Player> players;

        public GameState()
        {
            currentState = State.WaitingToStart;
        }

        public void StartGame(GameMode mode)
        {
            wall = new Wall();
            wall.NewGame();

            currentState = State.GameStart;
            players = Enum.GetValues(typeof(Tile.WindDirection))
                .Cast<Tile.WindDirection>()
                .Take(PlayersForMode(mode))
                .Select(wind => new Player(wall, wind))
                .ToList();
        }

        public void RotateWinds()
        {
            // Get only available winds
            var winds = Enum.GetValues(typeof(Tile.WindDirection))
                .Cast<Tile.WindDirection>()
                .Take(PlayersForMode(mode));
            // Rotate available winds
            players.ForEach(player =>
                player.wind = winds.SkipWhile(wind => wind != player.wind).Skip(1).FirstOrDefault());
        }

        public override void _Process(float delta)
        {
            switch (currentState)
            {
                case State.WaitingToStart:
                    // Do nothing
                    break;
                case State.GameStart:
                    // Do fancy game start anim
                    break;
            }
        }

        private static int PlayersForMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Riichi:
                    return 4;
                case GameMode.Sanma:
                    return 3;
                case GameMode.Minefield:
                    return 2;
                default:
                    throw new ArgumentException("Unknown game mode");
            }
        }
    }
}