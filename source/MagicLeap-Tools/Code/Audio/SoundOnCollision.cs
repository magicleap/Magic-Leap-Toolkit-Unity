// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicLeapTools
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class SoundOnCollision : MonoBehaviour
    {
        //Public Variables:
        public AudioClip sound;
        public float randomPitchMin = .8f;
        public float randomPitchMax = 1.2f;
        public float volumeMin = .25f;
        public float volumeMax = 2f;
        public float volumeMultiplier = 1;

        //Private Variables:
        private AudioSource _audioSource;

        //Init:
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        //Event Handlers:
        private void OnCollisionEnter(Collision collision)
        {
            float pitch = _audioSource.pitch;
            _audioSource.pitch = Random.Range(randomPitchMin, randomPitchMax);
            float volume = Mathf.Clamp(collision.impulse.magnitude, volumeMin, volumeMax) * volumeMultiplier;
            _audioSource.PlayOneShot(sound, volume);
            _audioSource.pitch = pitch;
        }
    }
}