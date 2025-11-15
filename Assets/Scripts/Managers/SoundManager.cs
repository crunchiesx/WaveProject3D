using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource _audioSourcePrefab;
    [SerializeField] private int _poolSize = 10;

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
            _audioPool.Enqueue(source);
        }
    }

    public void PlayAudio(AudioClip clip, Transform parentTransform)
    {
        if (clip == null) return;

        AudioSource source;
        if (_audioPool.Count > 0)
        {
            source = _audioPool.Dequeue();
        }
        else
        {
            source = Instantiate(_audioSourcePrefab);
        }

        source.playOnAwake = false;
        source.transform.SetParent(parentTransform, false);
        source.pitch = Random.Range(0.9f, 1.1f);
        source.PlayOneShot(clip);

        StartCoroutine(ReturnToPool(source));
    }

    private IEnumerator ReturnToPool(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);

        source.transform.SetParent(transform, false);
        source.pitch = 1f;
        _audioPool.Enqueue(source);
    }
}
