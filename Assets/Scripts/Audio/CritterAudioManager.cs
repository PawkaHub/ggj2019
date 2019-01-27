using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerAudioManager
{
    void PlayProjectileAudio();
    void PlayJumpSound();
}

[RequireComponent(typeof(AudioSource))]
public class CritterAudioManager : MonoBehaviour, IPlayerAudioManager
{
    [SerializeField] AudioClip[] attackClips;
    [SerializeField] AudioSource AudioSource;
    [SerializeField] AudioClip JumpSound;


    public void PlayProjectileAudio()
    {
        var index = Random.Range(0, attackClips.Length);
        this.AudioSource.clip = attackClips[index];
        AudioSource.Play();
    }

    // Start is called before the first frame update
    void Start()
    {
        AudioSource.clip = attackClips[0];
    }

    public void PlayJumpSound()
    {
        this.AudioSource.clip = JumpSound;
        AudioSource.Play();
    }
}
