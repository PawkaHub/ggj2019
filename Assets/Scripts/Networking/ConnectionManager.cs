using System;
using System.Collections;
using System.Collections.Generic;

namespace Networking
{
    public enum ConnectionMode
    {
        SERVER,
        CLIENT
    }

    public class ConnectionManager
    {
        public ConnectionMode connectionMode { private set; get; }
        public int NumActivePlayers { private set; get; }

        public IEnumerable<NetworkPlayer> ActivePlayers
        {
            get { return ActiveConnection.ActivePlayers; }
        }

        public IConnection ActiveConnection;

        public event Action<NetworkPlayer> OnPlayerConnect;
        public event Action<NetworkPlayer> OnPlayerDisconnect;
        public event Action OnActivePlayersUpdated;
        
        public readonly string serverAddress = "localhost";

        public ConnectionManager(string serverAddress)
        {
            this.serverAddress = serverAddress;
        }

        public IEnumerator Initialize()
        {
            ActiveConnection = new ClientConnection(serverAddress);
            ActiveConnection.OnActivePlayersUpdated += ForwardOnActivePlayersUpdated;
            ActiveConnection.OnPlayerConnect += ForwardOnPlayerConnect;
            ActiveConnection.OnPlayerDisconnect += ForwardOnPlayerDisconnect;

            yield return ActiveConnection.Initialize();

            if (ActiveConnection.IsConnected)
            {
                connectionMode = ConnectionMode.CLIENT;
            }
            else
            {
                ActiveConnection.Shutdown();

                ActiveConnection = new ServerConnection();
                ActiveConnection.OnActivePlayersUpdated += ForwardOnActivePlayersUpdated;
                ActiveConnection.OnPlayerConnect += ForwardOnPlayerConnect;
                ActiveConnection.OnPlayerDisconnect += ForwardOnPlayerDisconnect;

                yield return ActiveConnection.Initialize();

                if (ActiveConnection.IsConnected)
                {
                    connectionMode = ConnectionMode.SERVER;
                }
            }
        }

        private void ForwardOnPlayerDisconnect(NetworkPlayer obj)
        {
            OnPlayerDisconnect?.Invoke(obj);
        }

        private void ForwardOnPlayerConnect(NetworkPlayer obj)
        {
            OnPlayerConnect?.Invoke(obj);
        }

        private void ForwardOnActivePlayersUpdated()
        {
            OnActivePlayersUpdated?.Invoke();
        }

        public void Update()
        {
            if (ActiveConnection != null)
            {
                ActiveConnection.Update();
            }
        }
    }
}