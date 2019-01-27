using System;
using System.Linq;
using Networking;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameCanvasController : MonoBehaviour
{
    [SerializeField] private Text ipAddress;
    [SerializeField] private Text disconnectPrompt;

    private bool initialized;
    private Camera tvCamera;
    private Camera uiCamera;
    private Camera gameCamera;
    private TVCanvasController tvCanvas;

    private CameraAnimator cameraAnimator;

    private void Start()
    {
        disconnectPrompt.text = "Press \"Ctrl+ Q\" to Disconnect";
    }

    public void Initialize(Camera tvCamera, Camera uiCamera, Camera gameCamera, TVCanvasController tvCanvas)
    {
        this.ipAddress.enabled = false;
        this.tvCamera = tvCamera;
        this.uiCamera = uiCamera;
        this.gameCamera = gameCamera;
        this.tvCanvas = tvCanvas;

        cameraAnimator = new CameraAnimator(gameCamera, uiCamera);
        cameraAnimator.OnAnimationFinished += HandoffCameraToTV;

        initialized = true;
    }

    private void Update()
    {
        if (initialized && Input.GetKeyDown(KeyCode.Q) && Input.GetKeyDown(KeyCode.LeftControl))
        {
            Disconnect();
        }
    }

    private void Disconnect()
    {
        StartCoroutine(cameraAnimator.Animate());
    }

    private void HandoffCameraToTV()
    {
        tvCamera.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    internal void UpdateConnectionInfo(ConnectionManager connectionManager)
    {
        ipAddress.enabled = true;
        if (connectionManager.connectionMode == ConnectionMode.SERVER)
        {
            string hostName = System.Net.Dns.GetHostName();
            string localIP = System.Net.Dns.GetHostEntry(hostName).AddressList.First(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
            ipAddress.text = localIP;
        }
        else
        {
            ipAddress.text = "Connected";
        }
    }
}