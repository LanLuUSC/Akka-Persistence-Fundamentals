using System;
using Akka.Actor;
using Akka.Persistence;
using GameConsole.ActorModel.Commands;
using GameConsole.ActorModel.Events;

namespace GameConsole.ActorModel.Actors
{
    class PlayerActorState
    {
        public string PlayerName { get; set; }
        public int Health { get; set; }

        public override string ToString()
        {
            return $"[PlayerActorState {PlayerName} {Health}]";
        }
    }

    class PlayerActor : ReceivePersistentActor
    {
        private PlayerActorState _state;
        private int _eventCount;

        public override string PersistenceId => $"player-{_state.PlayerName}";

        public PlayerActor(string playerName, int startingHealth)
        {
            _state = new PlayerActorState
            {
                PlayerName = playerName,
                Health = startingHealth
            };

            DisplayHelper.WriteLine($"{_state.PlayerName} created");

            Command<HitPlayer>(message => HitPlayer(message));
            Command<DisplayStatus>(message => DisplayPlayerStatus());
            Command<SimulateError>(message => SimulateError());

            Recover<PlayerHit>(message =>
            {
                DisplayHelper.WriteLine($"{_state.PlayerName} replaying PlayerHit event from journal");

                _state.Health -= message.DamageTaken;
            });
        }

        private void HitPlayer(HitPlayer command)
        {
            DisplayHelper.WriteLine($"{_state.PlayerName} received HitPlayer command");

            var @event = new PlayerHit(command.Damage);

            DisplayHelper.WriteLine($"{_state.PlayerName} persisting PlayerHit event");

            Persist(@event, playerHitEvent =>
            {
                DisplayHelper.WriteLine($"{_state.PlayerName} persisted PlayerHit event ok, updating actor state");

                _state.Health -= @event.DamageTaken;

                _eventCount++;

                if (_eventCount == 5)
                {
                    DisplayHelper.WriteLine($"{_state.PlayerName} saving snapshot");

                    SaveSnapshot(_state);

                    DisplayHelper.WriteLine($"{_state.PlayerName} resetting event count to 0");

                    _eventCount = 0;
                }
            });            
        }

        private void DisplayPlayerStatus()
        {
            DisplayHelper.WriteLine($"{_state.PlayerName} received DisplayStatus");

            Console.WriteLine($"{_state.PlayerName} has {_state.Health} health");
        }

        private void SimulateError()
        {
            DisplayHelper.WriteLine($"{_state.PlayerName} received SimulateError");

            throw new ApplicationException($"Simulated exception in player: {_state.PlayerName}");
        }
    }
}
