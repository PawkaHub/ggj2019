using System.Collections;
using UnityEngine;

public class LAZER : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject lazer;
    [SerializeField] private GameObject particles;

    private bool lerping;

    private void Start()
    {
        lineRenderer.positionCount = 2;
    }

    private void Update()
    {
        if (!lerping)
        {
            var vectorOnUnitSphere = Random.onUnitSphere;
            vectorOnUnitSphere.y = -Mathf.Abs(vectorOnUnitSphere.y);

            var ray = new Ray(lineRenderer.transform.position, vectorOnUnitSphere);
            RaycastHit hitInfo;
            Physics.Raycast(ray, out hitInfo);

            if (hitInfo.collider.gameObject != lazer)
            {
                StartCoroutine(SlerpyDerpy(hitInfo.point));
            }
        }
    }

    private IEnumerator SlerpyDerpy(Vector3 destinationPosition)
    {
        lerping = true;
        if (!particles.activeSelf)
        {
            particles.SetActive(true);
        }

        var startTime = Time.time;

        while (Vector3.Distance(lineRenderer.GetPosition(1), destinationPosition) > 0.05)
        {
            var currentPosition = lineRenderer.GetPosition(1);
            var timeSinceStarted = Time.time - startTime;

            var newPosition = Vector3.Lerp(
                currentPosition,
                destinationPosition,
                Time.deltaTime * timeSinceStarted);

            lazer.transform.LookAt(newPosition);
            lineRenderer.SetPosition(0, lineRenderer.transform.position);
            lineRenderer.SetPosition(1, newPosition);
            particles.transform.position = newPosition;
            particles.transform.LookAt(lineRenderer.transform.position);

            yield return null;
        }

        lineRenderer.SetPosition(1, destinationPosition);
        lerping = false;
    }
}
