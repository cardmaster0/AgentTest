using System.Collections.Generic;
using UnityEngine;

namespace StarlightDefender
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        private readonly Dictionary<string, AudioClip> clips = new();
        private AudioSource source;

        private void Awake()
        {
            Instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = 0.45f;
            string[] names = { "PlayerShot", "EnemyExplosion", "PlayerHit", "PowerUp", "BossWarning", "BossExplosion" };
            foreach (string name in names)
            {
                AudioClip clip = Resources.Load<AudioClip>("StarlightDefender/Audio/" + name);
                if (clip != null) clips[name] = clip;
            }
        }

        public void Play(string clipName, float volume = 1f)
        {
            if (source != null && clips.TryGetValue(clipName, out AudioClip clip))
                source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }
}
