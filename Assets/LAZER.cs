using Game;
using System.Collections;
using UnityEngine;

public class LAZER : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject lazer;
    [SerializeField] private ParticleSystem particles;

    private bool damaging = true;
    private bool lerping;
    private Damager damager;

    private void Start()
    {
        lineRenderer.positionCount = 2;
        damager = new Damager(gameObject);
    }

    private void Update()
    {
        if (!lerping)
        {
            damaging = !damaging;
            if (damaging)
            {
                lineRenderer.startColor = lineRenderer.endColor = particles.startColor = Color.red;
            }
            else
            {
                lineRenderer.startColor = lineRenderer.endColor = particles.startColor = Color.green;
            }

            var vectorOnUnitSphere = Random.onUnitSphere;
            vectorOnUnitSphere.y = -Mathf.Abs(vectorOnUnitSphere.y);

            var ray = new Ray(lineRenderer.transform.position, vectorOnUnitSphere);
            RaycastHit hitInfo;
            Physics.Raycast(ray, out hitInfo);

            if (hitInfo.collider != null && hitInfo.collider.gameObject != lazer)
            {
                StartCoroutine(SlerpyDerpy(hitInfo.point));
            }
        }
    }

    private IEnumerator SlerpyDerpy(Vector3 destinationPosition)
    {
        lerping = true;
        if (!particles.gameObject.activeSelf)
        {
            particles.gameObject.SetActive(true);
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

            newPosition = TryDamager(newPosition - lineRenderer.transform.position);

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

    private Vector3 TryDamager(Vector3 direction)
    {
        var ray = new Ray(lineRenderer.transform.position, direction);
        RaycastHit hitInfo;
        Physics.Raycast(ray, out hitInfo);

        if (hitInfo.collider != null)
        {
            var candidatePlayer = hitInfo.collider.gameObject.GetComponentInParent<Player>();
            if (candidatePlayer != null)
            {
                candidatePlayer.Health.Modify(damaging ? -20f : 20f, damager);
            }
        }

        return hitInfo.point;
    }
}
