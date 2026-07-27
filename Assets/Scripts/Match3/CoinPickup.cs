using UnityEngine;
using UnityEngine.InputSystem;

namespace FishMatch.Match3
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CoinPickup : MonoBehaviour
    {
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float tapRadius = 0.4f;
        [SerializeField] private float lifetimeSeconds = 15f;

        private int goldValue;
        private Vector3 basePosition;
        private float bobPhase;
        private Camera targetCamera;

        public void Initialize(int value)
        {
            goldValue = value;
        }

        private void Start()
        {
            basePosition = transform.position;
            bobPhase = Random.Range(0f, Mathf.PI * 2f);
            targetCamera = Camera.main;
            Destroy(gameObject, lifetimeSeconds);
        }

        private void Update()
        {
            transform.position = basePosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobAmplitude);

            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                var pointerScreenPos = pointer.position.ReadValue();
                var screenPoint = new Vector3(pointerScreenPos.x, pointerScreenPos.y, -targetCamera.transform.position.z);
                var world = targetCamera.ScreenToWorldPoint(screenPoint);
                if (Vector2.Distance(world, transform.position) <= tapRadius)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            CurrencyManager.AddGold(goldValue);
            SaveManager.Save();
            Destroy(gameObject);
        }
    }
}
