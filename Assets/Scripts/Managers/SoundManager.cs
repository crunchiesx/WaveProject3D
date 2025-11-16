using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource _audioSourcePrefab;
    [SerializeField] private int _poolSize = 10;

    private List<AudioSource> _activeSources = new List<AudioSource>();
    private Queue<AudioSource> _audioPool = new Queue<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < _poolSize; i++)
        {
            var source = Instantiate(_audioSourcePrefab, transform);
            source.playOnAwake = false;
            source.spatialBlend = 1f; // make 3D
            _audioPool.Enqueue(source);
        }
    }

    public void PlayAudio(AudioClip clip, Transform followTransform = null, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source;
        if (_audioPool.Count > 0)
        {
            source = _audioPool.Dequeue();
        }
        else
        {
            source = Instantiate(_audioSourcePrefab, transform);
        }

        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.transform.SetParent(transform, false);

        if (followTransform != null)
        {
            source.transform.position = followTransform.position;
            _activeSources.Add(source);
        }

        source.Play();

        StartCoroutine(ReturnToPoolAfterPlaying(source, followTransform));
    }

    private IEnumerator ReturnToPoolAfterPlaying(AudioSource source, Transform follow)
    {
        while (source.isPlaying)
        {
            if (follow != null)
            {
                source.transform.position = follow.position;
            }
            yield return null;
        }

        source.clip = null;
        source.pitch = 1f;
        source.transform.SetParent(transform, false);
        _audioPool.Enqueue(source);
        _activeSources.Remove(source);
    }

    public void StopSoundsFollowing(Transform followTransform)
    {
        for (int i = _activeSources.Count - 1; i >= 0; i--)
        {
            if (_activeSources[i] != null && _activeSources[i].transform.position == followTransform.position)
            {
                _activeSources[i].Stop();
            }
        }
    }
}
