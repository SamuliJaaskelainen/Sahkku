using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] soundEffects;

    public GameObject audioPrefab;

    List<GameObject> audioPoolObject = new List<GameObject>();

    public static AudioManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < 32; ++i)
        {
            GameObject audioObject = Instantiate(audioPrefab, transform) as GameObject;
            audioPoolObject.Add(audioObject);
        }
    }

    public void PlaySound(AudioClip audioClip, float volume = 1.0f, float pitch = 1.0f)
    {
        for (int i = 0; i < soundEffects.Length; ++i)
        {
            if (soundEffects[i].name == audioClip.name)
            {
                PlaySound(i, volume, pitch);
                break;
            }
        }
    }

    public void PlaySound(AudioClip[] audioClips, float volume = 1.0f, float pitch = 1.0f)
    {
        AudioClip audioClip = audioClips[Random.Range(0, audioClips.Length)];
        if (audioClip != null)
        {
            PlaySound(audioClip, volume, pitch);
        }
    }

    public void PlaySound(string clipName, float volume = 1.0f, float pitch = 1.0f)
    {
        for (int i = 0; i < soundEffects.Length; ++i)
        {
            if (soundEffects[i].name == clipName)
            {
                PlaySound(i, volume, pitch);
                break;
            }
        }
    }

    public void PlaySound(int clipIndex, float volume = 1.0f, float pitch = 1.0f)
    {
        if (clipIndex >= soundEffects.Length)
        {
            Debug.LogError("No sound at index: " + clipIndex);
            return;
        }

        if (audioPoolObject.Count > 0)
        {
            GameObject audioObject = audioPoolObject[0] as GameObject;
            AudioSource audioSource = audioObject.GetComponent<AudioSource>();

            if (audioSource != null)
            {
                audioPoolObject.RemoveAt(0);
                audioSource.transform.position = Vector3.zero;
                audioSource.clip = soundEffects[clipIndex];
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.Play();
                StartCoroutine("ReturnToPool", audioObject);
            }
            else
            {
                Debug.LogError("NULL audio source at index: " + clipIndex);
            }
        }
    }

    IEnumerator ReturnToPool(GameObject audioObject)
    {
        yield return new WaitForSeconds(audioObject.GetComponent<AudioSource>().clip.length);
        audioPoolObject.Add(audioObject);
    }
}
