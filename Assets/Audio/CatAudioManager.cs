using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OBSOLETE MOTHERFUCKER
/// </summary>

[RequireComponent(typeof(AudioSource))]
public class CatAudioManager : MonoBehaviour
{
    AudioSource AudioSource;
    [SerializeField] AudioClip[] attackClips;

    public void PlayProjectileAudio()
    {
        var index = Random.Range(0, attackClips.Length);
        this.AudioSource.clip = attackClips[index];
        AudioSource.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        AudioSource = GetComponent<AudioSource>();
        AudioSource.clip = attackClips[0];
    }
}
