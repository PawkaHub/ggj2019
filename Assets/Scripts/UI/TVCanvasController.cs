using Game;
using Networking;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TVCanvasController : MonoBehaviour
{
    private const string LocalHostIP = "localhost";

    [SerializeField] private Camera UICamera;
    [SerializeField] private GameObject Credits;
    [SerializeField] private GameObject ButtonList;
    [SerializeField] private Button CreateServer;
    [SerializeField] private Button Play;
    [SerializeField] private Button ConnectBack;
    [SerializeField] private Button CreditsBack;
    [SerializeField] private InputField IPAddressField;
    [SerializeField] private Button CreditsButton;
    [SerializeField] private Button Quit;
    [SerializeField] private GameCanvasController GameCanvas;
    [SerializeField] private AudioManager AudioManager;

    private Camera gameCamera;
    private Camera tvCamera;

    private CameraAnimator cameraAnimator;

    private string ipAddress;

    private string LastUsedIPAddress
    {
        get
        {
            return PlayerPrefs.GetString("LastUsedIPAddress", LocalHostIP);
        }
        set
        {
            PlayerPrefs.SetString("LastUsedIPAddress", value);
        }
    }

    public void Initialize(Camera gameCamera, Camera tvCamera)
    {
        this.gameCamera = gameCamera ?? throw new ArgumentNullException(nameof(gameCamera));
        this.tvCamera = tvCamera ?? throw new ArgumentNullException(nameof(tvCamera));

        cameraAnimator = new CameraAnimator(UICamera, gameCamera);
        cameraAnimator.OnAnimationFinished += HandoffCameraToGame;

        ResetState();
    }
    
    private void OnPlay()
    {
        if (string.IsNullOrWhiteSpace(IPAddressField.text))
        {
            IPAddressField.text = LastUsedIPAddress;
        }

        IPAddressField.gameObject.SetActive(true);
        Play.GetComponentInChildren<Text>().text = "Connect";
        Credits.gameObject.SetActive(false);
        Quit.gameObject.SetActive(false);
        ConnectBack.gameObject.SetActive(true);

        Play.onClick.RemoveAllListeners();
        Play.onClick.AddListener(OnConnect);
    }

    private void OnConnect()
    {
        LastUsedIPAddress = ipAddress = IPAddressField.text;
        PlayExitMenuAnimationAndConnect();
    }

    private void OnCreateServer()
    {
        ipAddress = LocalHostIP; //Get an ip address plz
        PlayExitMenuAnimationAndConnect();
    }

    private void OnCredits()
    {
        Credits.gameObject.SetActive(true);
        ButtonList.gameObject.SetActive(false);
        CreditsBack.onClick.AddListener(OnBack);
    }

    private void OnQuit()
    {
        Application.Quit();
    }

    private void OnBack()
    {
        ResetState();
    }

    private void PlayExitMenuAnimationAndConnect()
    {
        StartCoroutine(cameraAnimator.Animate());
        StartCoroutine(AudioManager.FadeOut());
    }

    private void HandoffCameraToGame() // we can make this a IEnumerator if connecting is async
    {
        tvCamera.gameObject.SetActive(true);

        GameCanvas.Initialize(ipAddress, tvCamera, UICamera, gameCamera, this);
        GameCanvas.gameObject.SetActive(true);

        var connectionManager = new ConnectionManager(ipAddress);
        var gameManager = GameObject.FindObjectOfType<GameManager>();
        gameManager.Initialize(connectionManager);
    }

    private void ResetState()
    {
        Play.GetComponentInChildren<Text>().text = "Play";

        CreateServer.gameObject.SetActive(true);
        Play.gameObject.SetActive(true);
        CreditsButton.gameObject.SetActive(true);
        Quit.gameObject.SetActive(true);
        ButtonList.gameObject.SetActive(true);

        ConnectBack.gameObject.SetActive(false);
        IPAddressField.gameObject.SetActive(false);
        Credits.gameObject.SetActive(false);

        CreateServer.onClick.RemoveAllListeners();
        Play.onClick.RemoveAllListeners();
        CreditsButton.onClick.RemoveAllListeners();
        Quit.onClick.RemoveAllListeners();
        ConnectBack.onClick.RemoveAllListeners();
        CreditsBack.onClick.RemoveAllListeners();

        CreateServer.onClick.AddListener(OnCreateServer);
        Play.onClick.AddListener(OnPlay);
        CreditsButton.onClick.AddListener(OnCredits);
        Quit.onClick.AddListener(OnQuit);
        ConnectBack.onClick.AddListener(OnBack);
    }
}
