using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public class ServerConnection : BaseConnection
    {
        WrappedNetworkServerSimple networkServerSimple;

        List<NetworkPlayer> activePlayers = new List<NetworkPlayer>();

        private bool _isConnected = false;
        public override bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        public override NetworkPlayer[] ActivePlayers => activePlayers.ToArray();

        public override event Action OnActivePlayersUpdated;
        public override event Action<NetworkPlayer> OnPlayerConnect;
        public override event Action<NetworkPlayer> OnPlayerDisconnect;

        public override IEnumerator Initialize()
        {
            networkServerSimple = new WrappedNetworkServerSimple();

            networkServerSimple.RegisterHandler(GameMsgType.UpdateCritterInput, HandeUpdateCritterInput);

            networkServerSimple.OnClientConnected += OnClientConnected;
            networkServerSimple.OnClientDisconnected += OnClientDisconnected;
            networkServerSimple.RegisterHandler(GameMsgType.Effects, HandleEffectReceived);

            networkServerSimple.Initialize();
            networkServerSimple.Listen(CONNECTION_PORT);
            _isConnected = true;

            Debug.Log("Server Initialized");

            CurrentPlayer.isServer = true;
            CurrentPlayer.ID = 0;
            CurrentPlayer.IsSelf = true;
            CurrentPlayer.ServerConnection = this;

            CurrentPlayer.PostCritterStatePacket += (p) =>
            {
                PostCritterStatePacket(p, CurrentPlayer);
            };

            activePlayers.Add(CurrentPlayer);

            OnPlayerConnect?.Invoke(CurrentPlayer);

            UpdateActivePlayers();

            yield break;
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
        private void PostCritterStatePacket(CritterStatePacket p, NetworkPlayer currentPlayer)
        {
            var message = new CritterStatePacketMessage()
            {
                ID = currentPlayer.ID,
                critterStatePacket = p
            };

            SendMessageToClients(GameMsgType.UpdateCritterState, message);
        }

        private void HandeUpdateCritterInput(NetworkMessage netMsg)
        {
            var inputPacket = netMsg.ReadMessage<CritterInputPacketMessage>();

            var player = activePlayers.Where(p => p.ID == inputPacket.ID).First();

            Debug.Log("RECIV HandeUpdateCritterInput player#" + player.ID + "  " + inputPacket);

            player.Player.SetInputPacket(inputPacket.critterInputPacket);

            SendUpdateCritterInput(player, inputPacket.critterInputPacket);
        }

        public void SendUpdateCritterInput(NetworkPlayer owner, CritterInputPacket critterInputPacket)
        {
            var message = new CritterInputPacketMessage()
            {
                ID = owner.ID,
                critterInputPacket = critterInputPacket
            };

            SendMessageToClients(GameMsgType.UpdateCritterInput, message, false);
        }

        private void SendMessageToClients(short msgType, MessageBase messageBase, bool reliable = true)
        {
            foreach (var activePlayer in activePlayers)
            {
                if (activePlayer.isServer)
                {
                    continue;
                }
                if (reliable)
                {
                    activePlayer.Connection.Send(msgType, messageBase);
                }
                else
                {
                    activePlayer.Connection.SendUnreliable(msgType, messageBase);
                }
            }
        }

        public void SendUpdatedScores()
        {
            foreach (var activePlayer in activePlayers)
            {
                var score = activePlayer.Player.Score;
                var message = new CritterScoreMessage()
                {
                    ID = activePlayer.ID,
                    DeathCount = score.Deaths,
                    KillCount = score.Kills,
                    DamageTaken = score.DamageTaken,
                    DamageDealt = score.DamageDealt,
                };

                foreach (var targetPlayer in activePlayers)
                {
                    if (targetPlayer.isServer)
                    {
                        continue;
                    }

                    targetPlayer.Connection.SendUnreliable(GameMsgType.UpdateCritterScores, message);
                }
            }
        }

        private void OnClientDisconnected(NetworkConnection obj)
        {
            Debug.Log("Server OnClientDisconnected" + obj.address + "connectionID " + obj.connectionId);

            var player = activePlayers.Where(p => p.Connection == obj).First();

            OnPlayerDisconnect?.Invoke(player);


            activePlayers.Remove(player);

            MessageBase message = new PlayerDisconnectMessage()
            {
                id = player.ID
            };
            SendMessageToClients(GameMsgType.PlayerDisconnect, message);


            UpdateActivePlayers();
        }

        private void UpdateActivePlayers()
        {
            var message = PlayersUpdateMessage.Create(activePlayers);

            SendMessageToClients(GameMsgType.UpdateActivePlayers, message);

            OnActivePlayersUpdated?.Invoke();
        }

        private void OnClientConnected(NetworkConnection obj)
        {
            Debug.Log("Server OnClientConnected" + obj.address + "connectionID " + obj.connectionId);

            var player = new NetworkPlayer();

            player.Connection = obj;
            player.ID = obj.connectionId;
            player.PostCritterStatePacket += (p) =>
            {
                PostCritterStatePacket(p, player);
            };
            player.ServerConnection = this;

            activePlayers.Add(player);

            OnPlayerConnect?.Invoke(player);


            UpdateActivePlayers();
        }


        public override void Shutdown()
        {
            OnPlayerDisconnect = null;
            OnPlayerConnect = null;
            OnActivePlayersUpdated = null;

            if (networkServerSimple == null)
            {
                return;
            }
            networkServerSimple.ClearHandlers();
            networkServerSimple.DisconnectAllConnections();
            networkServerSimple.Stop();
            networkServerSimple = null;

            _isConnected = false;
        }

        public override void Update()
        {
            base.Update();
            networkServerSimple.Update();
        }

        public override void SendMessage<T>(T msg)
        {
            GameMessageBase gameMsg = msg as GameMessageBase;
            var player = activePlayers.Find(p => p.ID == gameMsg.Id);

            if(player == CurrentPlayer)
            {
                if (gameMsg is PlayerEffectMessage)
                {
                    PlayerEffectMessage effectMsg = gameMsg as PlayerEffectMessage;
                    Game.GameManager.Instance.
                        GetPlayer(player.ID).
                        Effects.
                        ApplyEffect(effectMsg.Effect,
                            effectMsg.Point,
                            effectMsg.Normal);
                    return;
                }
            }

            player.Connection.Send(gameMsg.Type, msg);
        }
    }

}