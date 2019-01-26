﻿using UnityEngine;
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
        disconnectPrompt.text = "Press \"esc\" to Disconnect";
    }

    public void Initialize(string ipAddress, Camera tvCamera, Camera uiCamera, Camera gameCamera, TVCanvasController tvCanvas)
    {
        this.ipAddress.text = ipAddress;
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
        if (initialized && Input.GetKeyDown(KeyCode.Escape))
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
}