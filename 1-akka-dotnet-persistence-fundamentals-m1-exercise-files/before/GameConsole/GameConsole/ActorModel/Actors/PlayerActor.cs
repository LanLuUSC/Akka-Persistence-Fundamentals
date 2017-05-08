using System;
using Akka.Actor;
using Akka.Persistence;
using GameConsole.ActorModel.Messages;

namespace GameConsole.ActorModel.Actors
{
    class PlayerActor : ReceivePersistentActor
    {

        public override string PersistenceId
        {
            get
            {
                return _playerName;
            }
        }

        private readonly string _playerName;
        private int _health;

        private int _eventCount = 0;     

        public PlayerActor(string playerName, int startingHealth)
        {
            _playerName = playerName;
            _health = startingHealth;

            DisplayHelper.WriteLine($"{_playerName} created");

            Command<HitMessage>(message => Persist(message, hitEvent => HitPlayer(message)));
            Command<DisplayStatusMessage>(message => DisplayPlayerStatus());
            Command<CauseErrorMessage>(message => SimulateError());

            Recover<HitMessage>(message => _health -= message.Damage);
            Recover<SnapshotOffer>(offer => _health = (int) offer.Snapshot);
        }

        private void HitPlayer(HitMessage message)
        {
            DisplayHelper.WriteLine($"{_playerName} received HitMessage");

            _health -= message.Damage;

            ++_eventCount;

            if (_eventCount == 5)
            {
                SaveSnapshot(this._health);
                _eventCount = 0;
            }

            
        }

        private void DisplayPlayerStatus()
        {
            DisplayHelper.WriteLine($"{_playerName} received DisplayStatusMessage");

            Console.WriteLine($"{_playerName} has {_health} health");
        }

        private void SimulateError()
        {
            DisplayHelper.WriteLine($"{_playerName} received CauseErrorMessage");

            throw new ApplicationException($"Simulated exception in player: {_playerName}");
        }
    }
}
