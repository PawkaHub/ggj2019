using UnityEngine;

public class CloudController : MonoBehaviour
{
    private Transform[] clouds;
    private float[] speeds;

    private void Start()
    {
        clouds = GetComponentsInChildren<Transform>();
        speeds = new float[clouds.Length];
        for (int i = 0; i < clouds.Length; i++)
        {
            speeds[i] = Random.Range(0.01f, 0.25f);
        }
    }

    private void Update()
    {
        for (int i = 0; i < clouds.Length; i++)
        {
            clouds[i].position += new Vector3(speeds[i], 0, 0);

            if (clouds[i].position.x > 360f)
            {
                clouds[i].position = new Vector3(-360f, clouds[i].position.y, clouds[i].position.z);
            }
        }
    }
}
