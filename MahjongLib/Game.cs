using System;
using System.Collections.Generic;
using System.Linq;
using MahjongLib.Utils;

namespace MahjongLib
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
        public Wind CurrentWind { get; private set; }
        public int Turn { get; private set; }
        public int Repeat { get; private set; }
        public FSM<GameState> State { get; private set; }
        public Wall Wall { get; private set; }
        public List<Player> Players { get; private set; }
        public Wind CurrentTurn { get; private set; }
        public Player ActivePlayer => Players.Find(player => player.Wind == CurrentTurn);
        public GameState CurrentState => State.Current;

        public List<Wind> Winds { get; private set; }
        private Tile _lastDiscard;

        public readonly ILogger Logger;
        public readonly ITimestamp Time;

        public Game(ILogger logger = null, ITimestamp time = null)
        {
            Logger = logger ?? new DefaultLogger();
            Time = time ?? new DefaultTime();
            Wall = new Wall(this);
        }

        private void OnPlayerTileDiscarded(Player player, Tile tile, bool justDrawn)
        {
            Logger.Log($"{player.Wind} discarded {tile} ({(justDrawn ? "just drawn" : "from hand")})");
            _lastDiscard = tile;
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
                    Logger.Log($"== {CurrentTurn}'s turn ==");
                    StartPlayerTurn();
                    break;
                case GameState.WaitingForDiscard:
                    Logger.Log("Waiting for discard");
                    break;
                case GameState.WaitingForCall:
                    var calls = Players.Select(player => (player, player.ValidCalls(_lastDiscard, CurrentTurn)))
                        .Where(callTuple => callTuple.Item2.Count > 0).ToList();
                    if (calls.Count > 0)
                    {
                        Logger.Log("Waiting for calls from " + string.Join(", ",
                            calls.Select(callTuple =>
                                $"{callTuple.player.Wind} ({string.Join(" ,", callTuple.Item2)})")));
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

        private void NextTurn()
        {
            CurrentTurn = NextWind(CurrentTurn);
            State.Set(GameState.PlayerTurnBegin);
        }

        public void Setup(GameMode mode, Wind wind, int turn, int repeat)
        {
            Mode = mode;
            CurrentWind = wind;
            Turn = turn;
            Repeat = repeat;

            // Get only available winds
            Winds = Enum.GetValues(typeof(Wind))
                .Cast<Wind>()
                .Take(PlayersForMode(Mode))
                .ToList();

            Players = Enumerable.Range(0, Winds.Count).Select(index => new Player(this, index)).ToList();

            Players.ForEach(player =>
                player.OnTileDiscarded += (tile, drawn) => OnPlayerTileDiscarded(player, tile, drawn));

            // Assign wind to players depending on turn
            // This currently majorly sucks ass but I'm too tired to refactor it at the moment
            var currentWind =
                (Wind) (((int) wind + turn - 1) % Enum.GetValues(typeof(Wind)).Length);
            foreach (var player in Players)
            {
                player.Wind = currentWind;
                currentWind = NextWind(currentWind);
            }

            State = new FSM<GameState>(GameState.WaitingToStart, Time);
            State.OnStateChanged += OnStateChanged;
        }

        public void Start()
        {
            Wall.NewGame();
            Wall.RevealDora();
            State.Set(GameState.GameStart);
        }

        public void SetupBoard()
        {
            // Draw starting hand for each player
            Players.ForEach(player => player.DrawStartingHand());

            // Set current turn to first player (dealer, aka east)
            CurrentTurn = Wind.East;
            State.Set(GameState.PlayerTurnBegin);
        }

        public void StartPlayerTurn()
        {
            Players.Find(p => p.Wind == CurrentTurn).DrawTile();
            State.Set(GameState.WaitingForDiscard);
        }

        public Wind NextWind(Wind currentWind)
        {
            return Winds.SkipWhile(wind => currentWind != wind).Skip(1).FirstOrDefault();
        }

        public Wind PreviousWind(Wind currentWind)
        {
            try
            {
                return Winds.TakeWhile(wind => currentWind != wind).Last();
            }
            catch (InvalidOperationException)
            {
                return Winds.Last();
            }
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