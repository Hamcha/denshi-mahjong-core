using System;
using System.Collections.Generic;
using System.Linq;
using DenshiMahjong.Utils;

namespace DenshiMahjong.Mahjong
{
    public class Game
    {
        public enum GameState
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

        public GameMode Mode { get; private set; }
        public Tile.WindDirection CurrentWind { get; private set; }
        public int Turn { get; private set; }
        public int Repeat { get; private set; }
        public FSM<GameState> State { get; private set; }
        public Wall Wall { get; private set; } = new Wall();
        public List<Player> Players { get; private set; }
        public Tile.WindDirection CurrentTurn { get; private set; }
        public Player ActivePlayer => Players.Find(player => player.Wind == CurrentTurn);
        public GameState CurrentState => State.Current;

        private readonly List<Tile.WindDirection> _winds;

        public Game(GameMode mode, Tile.WindDirection wind, int turn, int repeat)
        {
            Mode = mode;
            CurrentWind = wind;
            Turn = turn;
            Repeat = repeat;

            // Get only available winds
            _winds = Enum.GetValues(typeof(Tile.WindDirection))
                .Cast<Tile.WindDirection>()
                .Take(PlayersForMode(Mode))
                .ToList();

            Players = Enumerable.Range(0, _winds.Count).Select(index => new Player(Wall, index)).ToList();

            Players.ForEach(player =>
                player.OnTileDiscarded += (tile, drawn) => OnPlayerTileDiscarded(player, tile, drawn));

            // Assign wind to players depending on turn
            // This currently majorly sucks ass but I'm too tired to refactor it at the moment
            var currentWind =
                (Tile.WindDirection) (((int) wind + turn - 1) % Enum.GetValues(typeof(Tile.WindDirection)).Length);
            foreach (var player in Players)
            {
                player.Wind = currentWind;
                currentWind = NextWind(currentWind);
            }

            State = new FSM<GameState>(GameState.WaitingToStart);
            State.OnStateChanged += OnStateChanged;
        }

        private void OnPlayerTileDiscarded(Player player, Tile tile, bool justDrawn)
        {
            GameLog.Log($"{player.Wind} discarded {tile} ({(justDrawn ? "just drawn" : "from hand")})");
            State.Set(GameState.WaitingForCall);
        }

        private void OnStateChanged(GameState newGameState)
        {
            switch (newGameState)
            {
                case GameState.GameStart:
                    SetupBoard();
                    break;
                case GameState.PlayerTurnBegin:
                    GameLog.Log($"== {CurrentTurn}'s turn ==");
                    StartPlayerTurn();
                    break;
                case GameState.WaitingForDiscard:
                    GameLog.Log("Waiting for discard");
                    break;
                case GameState.WaitingForCall:
                    var wait = CheckValidCalls();
                    if (wait)
                    {
                        GameLog.Log("Waiting for calls");
                    }
                    else
                    {
                        // No waits, skip to next turn
                        NextTurn();
                    }

                    break;
                case GameState.PlayerWon:
                    //TODO
                    break;
                case GameState.ExhaustiveDraw:
                    //TODO
                    break;
            }
        }

        private bool CheckValidCalls()
        {
            //TODO
            return false;
        }

        private void NextTurn()
        {
            CurrentTurn = NextWind(CurrentTurn);
            State.Set(GameState.PlayerTurnBegin);
        }

        public void Start()
        {
            Wall.NewGame();
            Wall.RevealDora();
            State.Set(GameState.GameStart);
        }

        public void RotateWinds()
        {
            // Rotate available winds
            Players.ForEach(player => player.Wind = NextWind(player.Wind));
        }

        public void SetupBoard()
        {
            // Draw starting hand for each player
            Players.ForEach(player => player.DrawStartingHand());

            // Set current turn to first player (dealer, aka east)
            CurrentTurn = Tile.WindDirection.East;
            State.Set(GameState.PlayerTurnBegin);
        }

        public void StartPlayerTurn()
        {
            Players.Find(p => p.Wind == CurrentTurn).DrawTile();
            State.Set(GameState.WaitingForDiscard);
        }

        private Tile.WindDirection NextWind(Tile.WindDirection currentWind)
        {
            return _winds.SkipWhile(wind => currentWind != wind).Skip(1).FirstOrDefault();
        }

        public static int PlayersForMode(GameMode mode)
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