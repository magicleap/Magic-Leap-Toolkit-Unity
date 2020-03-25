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
    public class PushButton : VirtualButton
    {
#if PLATFORM_LUMIN
        //Public Variables:
        [Tooltip("The backing or base visual that the plunger pushes against to visualize a complete press operation.")]
        public Transform frame;
        [Tooltip("The visual that moves to visualize the physical operation of this button.")]
        public Transform element;
        [Tooltip("Visualizes proximity of interaction with this button.")]
        public PercentageResponse hoverResponse;
        [Tooltip("A color to apply when this button is not in use.")]
        public Color idleColor = Color.white;
        [Tooltip("A color that blends with hoverColor as this button is pressed.")]
        public Color pressedColor = Color.green;
        public AudioClip pressedSound;
        public AudioClip releasedSound;
        private Collider _trackedCollider;
        private BoxCollider _trigger;

        //Private Variables:
        private AudioSource _audioSource;
        private string _leftIndexStatus = "leftIndexStatus";
        private string _leftIndexMCPStatus = "leftIndexeMCPStatus";
        private string _leftMiddleMCPStatus = "leftMiddleMCPStatus";
        private string _rightIndexStatus = "rightIndexStatus";
        private string _rightIndexMCPStatus = "rightIndexMCPStatus";
        private string _rightMiddleMCPStatus = "rightMiddleMCPStatus";
        private string _colliderStatus = "colliderStatus";

        //Init:
        private void Reset()
        {
            //try to hook up pieces just to be helpful:
            element = transform.Find("Element");
            frame = transform.Find("Frame");
        }
    
        protected virtual void Awake()
        {
            //register interactions:
            Register(_leftIndexStatus);
            Register(_leftIndexMCPStatus);
            Register(_leftMiddleMCPStatus);
            Register(_rightIndexStatus);
            Register(_rightIndexMCPStatus);
            Register(_rightMiddleMCPStatus);
            Register(_colliderStatus);

            //refs:
            _audioSource = GetComponent<AudioSource>();

            //trigger volume:
            _trigger = gameObject.AddComponent<BoxCollider>();
            _trigger.isTrigger = true;
            _trigger.size = Vector3.zero;

            //hooks:
            OnPressUpdated.AddListener(HandlePressUpdated);
            OnHoverUpdated.AddListener(HandleHoverUpdated);
            OnPressed.AddListener(HandlePressed);
            OnReleased.AddListener(HandleReleased);
            OnTouchEnd.AddListener(HandleTouchEnd);
            OnHoverEnd.AddListener(HandleHoverEnd);
            OnCanceled.AddListener(HandleCanceled);

            //sets:
            ResetElement();
            ResetFrame();
            ResetColors();
        }

        //Loops:
        private void Update()
        {
            //update trigger volume:
            _trigger.center = transform.InverseTransformPoint(Vector3.Lerp(transform.position, HoverPlaneLocation, .5f));
            Vector3 scaleFix = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            _trigger.size = Vector3.Scale(scaleFix, new Vector3(ScaledRadius, ScaledRadius, ScaledTouchDistance + hoverDistance));

            if (HandInput.Ready)
            {
                //update hand interactions:
                Set(_leftIndexStatus, HandInput.Left.Skeleton.Index.End);
                Set(_leftIndexMCPStatus, HandInput.Left.Skeleton.Index.Knuckle.positionFiltered);
                Set(_leftMiddleMCPStatus, HandInput.Left.Skeleton.Middle.Knuckle.positionFiltered);
                Set(_rightIndexStatus, HandInput.Right.Skeleton.Index.Tip.positionFiltered);
                Set(_rightIndexMCPStatus, HandInput.Right.Skeleton.Index.Knuckle.positionFiltered);
                Set(_rightMiddleMCPStatus, HandInput.Right.Skeleton.Middle.Knuckle.positionFiltered);
            }

            //respond to collider interactions:
            if (_trackedCollider != null)
            {
                Vector3 furthestPoint = _trackedCollider.ClosestPoint(transform.position - InputNormal);
                Vector3 centeredPoint = _trackedCollider.ClosestPoint(transform.position);
                furthestPoint = transform.InverseTransformPoint(furthestPoint);
                centeredPoint = transform.InverseTransformPoint(centeredPoint);
                centeredPoint.z = furthestPoint.z;
                centeredPoint = transform.TransformPoint(centeredPoint);
                Set(_colliderStatus, centeredPoint);
            }

            //make virtual button process interactions:
            Evaluate();
        }

        //Event Handlers:
        private void OnTriggerEnter(Collider other)
        {
            //due to the way ManagedHandCollider needs to turn the collider on and off for hands we can not
            //reliably use them for push buttons due to state jumping when they swap availability the fact
            //that they can be here one minute and gone the next is horrible for physics checks like this:
            if (other.GetComponent<HandCollider>() != null)
            {
                return;
            }
            _trackedCollider = other;   
        }

        private void HandleCanceled()
        {
            HandleTouchEnd();
        }

        protected virtual void HandlePressed()
        {
            if (pressedSound != null)
            {
                _audioSource.PlayOneShot(pressedSound);
            }
        }

        protected virtual void HandleReleased()
        {
            if (releasedSound != null)
            {
                _audioSource.PlayOneShot(releasedSound);
            }
        }

        private void HandleHoverUpdated(float percentage)
        {
            if (hoverResponse != null)
            {
                hoverResponse.Process(percentage);
            }
        }

        private void HandleHoverEnd()
        {
            HandleTouchEnd();
        }

        private void HandlePressUpdated(float percentage)
        {
            Vector3 targetPosition = Vector3.LerpUnclamped(TouchPlaneLocation, transform.position, percentage);
            Vector3 localTargetPosition = transform.InverseTransformPoint(targetPosition);
            localTargetPosition.x = element.localPosition.x;
            localTargetPosition.y = element.localPosition.y;
            element.localPosition = localTargetPosition;

            if (hoverResponse != null)
            {
                hoverResponse.transform.localPosition = localTargetPosition;
            }

            if (percentage > 1)
            {
                frame.position = targetPosition;
            }
            else
            {
                ResetFrame();
            }

            //colors:
            foreach (var item in frame.GetComponentsInChildren<Renderer>())
            {
                item.material.color = Color.Lerp(idleColor, pressedColor, Mathf.Clamp01(percentage));
            }

            foreach (var item in element.GetComponentsInChildren<Renderer>())
            {
                item.material.color = Color.Lerp(idleColor, pressedColor, Mathf.Clamp01(percentage));
            }
        }

        private void HandleTouchEnd()
        {
            ResetElement();
            ResetFrame();
            ResetColors();
            ResetHover();
        }

        //Private Methods:
        private void ResetHover()
        {
            if (hoverResponse != null)
            {
                hoverResponse.Process(0);
                hoverResponse.transform.localPosition = element.localPosition;
            }
        }

        private void ResetColors()
        {
            foreach (var item in frame.GetComponentsInChildren<Renderer>())
            {
                item.material.color = idleColor;
            }

            foreach (var item in element.GetComponentsInChildren<Renderer>())
            {
                item.material.color = idleColor;
            }
        }

        private void ResetElement()
        {
            Vector3 elementReset = TouchPlaneLocation;
            elementReset = transform.InverseTransformPoint(elementReset);
            elementReset.x = element.localPosition.x;
            elementReset.y = element.localPosition.y;
            element.localPosition = elementReset;
        }

        private void ResetFrame()
        {
            frame.position = transform.position;
        }
#endif
    }
}