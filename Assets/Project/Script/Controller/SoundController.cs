using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{

    AudioSource source;
    public List<AudioClip> audioClips = new List<AudioClip>();

    private void Awake()
    {
        source = gameObject.GetComponent<AudioSource>();
    }
    public void PlayAudio(int index,bool loop = false)
    {
        source.clip = audioClips[index];
        if (loop) source.loop = loop;
        source.Play();
    }
    public void PlayAudio(string name, bool loop = false)
    {
        
        foreach(AudioClip clip in audioClips)
        {
            if(clip.name == name)
            {
                source.clip = clip;
                if (loop) source.loop = loop;
                source.Play();
            }
        }
    }

    public void StopAudio()
    {
        source.Stop();
    }
}
