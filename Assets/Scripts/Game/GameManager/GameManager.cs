using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Networking;
using System.Linq;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameManager>();
                }
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<GameManager>();
                }
                return instance;
            }
        }

        public bool Initialized { get; private set; }
        public event EventHandler OnInitialized;
        
        public IEnumerable<Player> Players { get { return ConnectionManager.ActivePlayers.Select(p=>p.Player); } }

        [SerializeField]
        private Player PlayerPrefab;

        public ConnectionManager ConnectionManager;
        public GameSpawner Spawner;

        [SerializeField] float mouseSensitivity = 4f;
        public CritterInputGrabber LocalInputGrabber;

        private Dictionary<Networking.NetworkPlayer, Player> NetworkToGameMap = new Dictionary<Networking.NetworkPlayer, Player>();

        public Coroutine Initialize(ConnectionManager connectionManager)
        {
            LocalInputGrabber = new CritterInputGrabber(mouseSensitivity);
            return StartCoroutine(InitializeAsync(connectionManager));
        }

        private IEnumerator InitializeAsync(ConnectionManager connectionManager)
        {
            this.ConnectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            Spawner = new GameSpawner(this);

            connectionManager.OnPlayerConnect += OnPlayerConnect;
            connectionManager.OnActivePlayersUpdated += OnActivePlayersUpdated;
            connectionManager.OnPlayerDisconnect += OnPlayerDisconnect;

            yield return connectionManager.Initialize();
            
            Initialized = true;
            OnInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void OnPlayerDisconnect(Networking.NetworkPlayer player)
        {
            var go = NetworkToGameMap[player];

            NetworkToGameMap.Remove(player);

            GameObject.Destroy(go);

            Debug.Log("OnPlayerDisconnect Player #" + player.ID + "(" + player.Name + ")");
        }


        private void OnPlayerConnect(Networking.NetworkPlayer netPlayer)
        {
            //var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            //go.name = "Player_" + player.Name;

            Player player = CreatePlayerObject(netPlayer);

            NetworkToGameMap.Add(netPlayer, player);

            Debug.Log("OnPlayerConnect Player #" + netPlayer.ID + "(" + netPlayer.Name + ")");
        }

        private Player CreatePlayerObject(Networking.NetworkPlayer netPlayer)
        {
            var player = GameObject.Instantiate<Player>(PlayerPrefab);

            player.Initialize(netPlayer, LocalInputGrabber, ConnectionManager.connectionMode == ConnectionMode.SERVER);

            Spawner.Spawn(player);

            return player;
        }

        public void OnActivePlayersUpdated()
		{
            Debug.Log("OnActivePlayersUpdated");
            foreach (Networking.NetworkPlayer player in ConnectionManager.ActivePlayers)
            {
                Debug.Log("OnActivePlayersUpdated Player #" + player.ID + "(" + player.Name + ")");
            }
        }

		public void Update()
		{
            if (Initialized)
            {
                ConnectionManager.Update();
            }
        }

        /*public void FixedUpdate()
        {
            if (Initialized && connectionManager.connectionMode == ConnectionMode.CLIENT)
            {
                var critterInputPacket = LocalInputGrabber.UpdateTick();

                connectionManager.
            }
        }*/
        
        public Player GetPlayer(int id)
        {
            foreach(var p in ConnectionManager.ActivePlayers)
            {
                if(p.ID == id)
                {
                    return p.Player;
                }
            }
            return null;
        }
    }
}