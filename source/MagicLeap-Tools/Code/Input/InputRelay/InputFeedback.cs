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
    [RequireComponent(typeof(InputReceiver))]
    public class InputFeedback : MonoBehaviour
    {
#if PLATFORM_LUMIN
        //Public Variables:
        public Renderer targetRenderer;
        public Color targetedColor = Color.gray;
        public Color selectedColor = Color.green;
        public Color draggedColor = Color.magenta;
        public AudioClip targetBeginSound;
        public AudioClip targetEndSound;
        public AudioClip selectedSound;
        public AudioClip dragStartSound;
        public AudioClip dragStopSound;
        public AudioClip collisionSound;

        //Private Variables:
        private InputReceiver _inputReceiver;
        private AudioSource _audioSource;
        private Color _idleColor;

        //Init:
        private void Reset()
        {
            //refs:
            targetRenderer = GetComponent<Renderer>();
        }

        private void Awake()
        {
            //refs:
            _audioSource = GetComponent<AudioSource>();
            _inputReceiver = GetComponent<InputReceiver>();

            //sets:
            if (targetRenderer != null)
            {
                _idleColor = targetRenderer.material.color;
            }
        }

        //Flow:
        private void OnEnable()
        {
            //hooks:
            _inputReceiver.OnTargetEnter.AddListener(HandleTargetEnter);
            _inputReceiver.OnTargetExit.AddListener(HandleTargetExit);
            _inputReceiver.OnSelected.AddListener(HandleSelect);
            _inputReceiver.OnDeselected.AddListener(HandleDeselect);
            _inputReceiver.OnDragBegin.AddListener(HandleDragBegin);
            _inputReceiver.OnDragEnd.AddListener(HandleDragEnd);

            //pointer collision events:
            PointerReceiver pointerReceiver = GetComponent<PointerReceiver>();
            if (pointerReceiver != null)
            {
                pointerReceiver.OnDraggedCollisionEnter.AddListener(HandleDraggedCollision);
            }
        }

        private void OnDisable()
        {
            //unhooks:
            _inputReceiver.OnTargetEnter.RemoveListener(HandleTargetEnter);
            _inputReceiver.OnTargetExit.RemoveListener(HandleTargetExit);
            _inputReceiver.OnSelected.RemoveListener(HandleSelect);
            _inputReceiver.OnDeselected.RemoveListener(HandleDeselect);
            _inputReceiver.OnDragBegin.RemoveListener(HandleDragBegin);
            _inputReceiver.OnDragEnd.RemoveListener(HandleDragEnd);

            //pointer collision events:
            PointerReceiver pointerReceiver = GetComponent<PointerReceiver>();
            if (pointerReceiver != null)
            {
                pointerReceiver.OnDraggedCollisionEnter.RemoveListener(HandleDraggedCollision);
            }
        }

        //Event Handlers
        private void HandleDraggedCollision(Collision collision, GameObject sender)
        {
            if (collisionSound != null)
            {
                float force = Mathf.Clamp(collision.impulse.magnitude, .1f, 1);
                _audioSource.PlayOneShot(collisionSound, force);
            }
        }

        private void HandleTargetEnter(GameObject sender)
        {
            if (_inputReceiver.TargetedBy.Count != 1)
            {
                return;
            }

            if (targetRenderer != null)
            {
                targetRenderer.material.color = targetedColor;
            }

            if (targetBeginSound != null)
            {
                _audioSource.PlayOneShot(targetBeginSound);
            }
        }

        private void HandleTargetExit(GameObject sender)
        {
            if (_inputReceiver.TargetedBy.Count != 0)
            {
                return;
            }

            if (targetRenderer != null)
            {
                targetRenderer.material.color = _idleColor;
            }

            if (targetEndSound != null)
            {
                _audioSource.PlayOneShot(targetEndSound);
            }
        }

        private void HandleSelect(GameObject sender)
        {
            if (_inputReceiver.SelectedBy.Count != 1)
            {
                return;
            }

            if (targetRenderer != null)
            {
                targetRenderer.material.color = selectedColor;
            }

            if (selectedSound != null)
            {
                _audioSource.PlayOneShot(selectedSound);
            }
        }

        private void HandleDeselect(GameObject sender)
        {
            if (_inputReceiver.SelectedBy.Count != 0)
            {
                return;
            }

            if (targetRenderer != null)
            {
                targetRenderer.material.color = targetedColor;
            }
        }

        private void HandleDragBegin(GameObject sender)
        {
            if (_inputReceiver.DraggedBy.Count != 1)
            {
                return;
            }

            if (targetRenderer != null)
            {
                targetRenderer.material.color = draggedColor;
            }

            if (dragStartSound != null)
            {
                _audioSource.PlayOneShot(dragStartSound);
            }
        }

        private void HandleDragEnd(GameObject sender)
        {
            if (_inputReceiver.DraggedBy.Count > 0)
            {
                return;
            }

            if (targetRenderer != null)
            {
                targetRenderer.material.color = selectedColor;
            }

            if (dragStopSound != null)
            {
                _audioSource.PlayOneShot(dragStopSound);
            }
        }
#endif
    }
}