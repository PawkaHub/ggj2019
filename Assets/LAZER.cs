using UnityEngine;

public class LAZER : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject lazer;
    [SerializeField] private GameObject particles;
    [SerializeField] private float frequencyOfMovement = 5f;

    private float lastTime = 0;

    private void Start()
    {
        lineRenderer.positionCount = 2;
    }

    private void Update()
    {
        if (Time.time - lastTime > frequencyOfMovement)
        {
            var vectorOnUnitSphere = Random.onUnitSphere;
            vectorOnUnitSphere.y = -Mathf.Abs(vectorOnUnitSphere.y);

            lazer.transform.LookAt(transform.position + (vectorOnUnitSphere * 20));
            var ray = new Ray(lineRenderer.transform.position, vectorOnUnitSphere);
            RaycastHit hitInfo;
            Physics.Raycast(ray, out hitInfo);
            lineRenderer.SetPosition(0, lineRenderer.transform.position);
            lineRenderer.SetPosition(1, hitInfo.point);
            particles.transform.position = hitInfo.point;
            particles.transform.LookAt(lineRenderer.transform.position);

            if (!particles.activeSelf)
            {
                particles.SetActive(true);
            }

            lastTime = Time.time;
        }
    }
}
