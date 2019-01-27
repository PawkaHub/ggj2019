using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public class ClientConnection : BaseConnection
    {
        const int CONNECTION_TIMEOUT_SECONDS = 2;

        NetworkClient client;
        private readonly string serverAddress;

        public ClientConnection(string serverAddress)
        {
            this.serverAddress = serverAddress;
        }

        public override event Action OnActivePlayersUpdated;
        public override event Action<NetworkPlayer> OnPlayerConnect;
        public override event Action<NetworkPlayer> OnPlayerDisconnect;

        public override bool IsConnected => client.isConnected;

        List<NetworkPlayer> activePlayers = new List<NetworkPlayer>();

        public override NetworkPlayer[] ActivePlayers => activePlayers.ToArray();

        public override IEnumerator Initialize()
        {
            Debug.Log("Client Initializing...");

            client = new NetworkClient();

            client.RegisterHandler(GameMsgType.UpdateActivePlayers, HandleUpdateActivePlayers);
            client.RegisterHandler(GameMsgType.PlayerDisconnect, HandlePlayerDisconnect);
            client.RegisterHandler(GameMsgType.Effects, HandleEffectReceived);
            client.RegisterHandler(GameMsgType.UpdateCritterState, HandleUpdateCritterState);
            client.RegisterHandler(GameMsgType.UpdateCritterInput, HandeUpdateCritterInput);
            client.RegisterHandler(GameMsgType.UpdateCritterScores, HandleUpdateCritterScores);

            client.Connect(serverAddress, CONNECTION_PORT);

            var time = Time.time + CONNECTION_TIMEOUT_SECONDS;

            yield return new WaitUntil(() => client.isConnected || Time.time > time);

            if (client.isConnected)
            {
                Debug.Log("Client Connected!");
            }
            else
            {
                Debug.Log("Client Not Connected!");
            }
        }

        private void HandleUpdateCritterScores(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<CritterScoreMessage>();
            var player = activePlayers.Find(p => p.ID == message.ID);

            var score = player.Player.Score;
            score.UpdateScore(message);
        }

        private void HandeUpdateCritterInput(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<CritterInputPacketMessage>();
            var player = activePlayers.Find(p => p.ID == message.ID);

            //Debug.Log("SENDING CritterStatePacketMessage player#" + player.ID + " p" + message.critterStatePacket.position + " v" + message.critterStatePacket.velocity);

            player.Player.CritterController.InputPacketOveride = message.critterInputPacket;
        }

        public override void SendMessage<T>(T msg)
        {
            GameMessageBase gameMsg = msg as GameMessageBase;
            client.Send(gameMsg.Type, gameMsg);
        }

        private void HandleUpdateCritterState(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<CritterStatePacketMessage>();
            var player = activePlayers.Find(p => p.ID == message.ID);

            //Debug.Log("SENDING CritterStatePacketMessage player#" + player.ID + " p" + message.critterStatePacket.position + " v" + message.critterStatePacket.velocity);

            player.HandleCritterStatePacket(message.critterStatePacket);
        }

        private void HandleEffectReceived(NetworkMessage netMsg)
        {
            Debug.LogError("HandleEffectReceived");
            var message = netMsg.ReadMessage<PlayerEffectMessage>();
            var player = activePlayers.Find(p => p.ID == message.Id);
            player.Player.Effects.ApplyEffect(message.Effect,
                                        message.Point,
                                        message.Normal);
        }

        private void HandlePlayerDisconnect(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<PlayerDisconnectMessage>();
            var player = activePlayers.Find(p => p.ID == message.id);
            activePlayers.Remove(player);
            OnPlayerDisconnect(player);
        }

        private void HandleUpdateActivePlayers(NetworkMessage netMsg)
        {
            var playersUpdate = netMsg.ReadMessage<PlayersUpdateMessage>();

            Debug.Log("UPDATE ACTIVE PLAYERS:");
            foreach (var playerData in playersUpdate.Players)
            {
                var existingPlayer = activePlayers.Find(p => p.ID == playerData.ID);
                if (existingPlayer == null)
                {
                    existingPlayer = new NetworkPlayer()
                    {
                        ID = playerData.ID,
                        Name = playerData.Name,
                        CharacterType = playerData.CharacterType,
                        IsSelf = playerData.ID == client.connection.connectionId
                       
                    };
                    existingPlayer.Connection = (existingPlayer.IsSelf) ? client.connection : null;
                    activePlayers.Add(existingPlayer);
                    OnPlayerConnect?.Invoke(existingPlayer);
                }

                // todo, this is dumb
                existingPlayer.Name = playerData.Name;

                existingPlayer.Player.CritterController.transform.SetPositionAndRotation(playerData.Position, playerData.Rotation);

                Debug.Log("PLAYER #" + playerData.ID + " name" + playerData.Name);
            }


            OnActivePlayersUpdated?.Invoke();
        }

        public override void Shutdown()
        {
            OnPlayerDisconnect = null;
            OnPlayerConnect = null;
            OnActivePlayersUpdated = null;

            if (client == null)
            {
                return;
            }
            client.Shutdown();
            client = null;
        }


    }

}