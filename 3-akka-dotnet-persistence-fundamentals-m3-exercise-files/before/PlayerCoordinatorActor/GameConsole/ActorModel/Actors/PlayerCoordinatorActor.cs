using System;
using Akka.Actor;
using Akka.Persistence;
using GameConsole.ActorModel.Messages;

namespace GameConsole.ActorModel.Actors
{
    internal class PlayerCoordinatorActor : ReceivePersistentActor
    {
        private const int DefaultStartingHealth = 100;

        public override string PersistenceId { get; } = "player-coordinator";

        public PlayerCoordinatorActor()
        {
            Command<CreatePlayerMessage>(message =>
            {
                DisplayHelper.WriteLine($"PlayerCoordinatorActor received CreatePlayerMessage for {message.PlayerName}");

                Persist(message, createPlayerMessage =>
                {
                    DisplayHelper.WriteLine(
                        $"PlayerCoordinatorActor persisted a CreatePlayerMessage for {message.PlayerName}");

                    Context.ActorOf(
                        Props.Create(() =>
                                    new PlayerActor(message.PlayerName, DefaultStartingHealth)), message.PlayerName);
                });
            });

            Recover<CreatePlayerMessage>(createPlayerMessage =>
            {
                DisplayHelper.WriteLine($"PlayerCoordinatorActor replaying CreatePlayerMessage for {createPlayerMessage.PlayerName}");

                Context.ActorOf(
                        Props.Create(() =>
                                    new PlayerActor(createPlayerMessage.PlayerName, DefaultStartingHealth)), createPlayerMessage.PlayerName);

            });
        }
    }
}