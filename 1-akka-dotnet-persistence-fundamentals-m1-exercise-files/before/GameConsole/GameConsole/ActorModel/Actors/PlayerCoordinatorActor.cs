using System;
using Akka.Actor;
using GameConsole.ActorModel.Messages;
using Akka.Persistence;
namespace GameConsole.ActorModel.Actors
{
    internal class PlayerCoordinatorActor : ReceivePersistentActor
    {
        private const int DefaultStartingHealth = 100;

        public override string PersistenceId
        {
            get
            {
                return "coordinator";
            }
        }

        public PlayerCoordinatorActor()
        {
            Command<CreatePlayerMessage>(message => Persist(message, createEvent => 
            {
                DisplayHelper.WriteLine($"PlayerCoordinatorActor received CreatePlayerMessage for {message.PlayerName}");

                Context.ActorOf(
                    Props.Create(() =>
                                new PlayerActor(message.PlayerName, DefaultStartingHealth)), message.PlayerName);
            }));

            Recover<CreatePlayerMessage>(message =>
            {
                DisplayHelper.WriteLine($"PlayerCoordinatorActor received CreatePlayerMessage for {message.PlayerName}");

                Context.ActorOf(
                    Props.Create(() =>
                                new PlayerActor(message.PlayerName, DefaultStartingHealth)), message.PlayerName);
            });
        }
    }
}