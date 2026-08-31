using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightDefender
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private Vector2 viewportMargin = new(0.045f, 0.065f);
        private Rigidbody2D body;
        private Vector2 input;
        private Camera mainCamera;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || GameManager.Instance == null || GameManager.Instance.IsFinished)
            {
                input = Vector2.zero;
                return;
            }
            input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            input = Vector2.ClampMagnitude(input, 1f);
        }

        private void FixedUpdate()
        {
            if (mainCamera == null) return;
            Vector2 next = body.position + input * (moveSpeed * Time.fixedDeltaTime);
            body.MovePosition(ClampToCamera(next));
        }

        private Vector2 ClampToCamera(Vector2 position)
        {
            Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(viewportMargin.x, viewportMargin.y));
            Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f - viewportMargin.x, 1f - viewportMargin.y));
            position.x = Mathf.Clamp(position.x, min.x, max.x);
            position.y = Mathf.Clamp(position.y, min.y, max.y);
            return position;
        }

        public void ClampImmediatelyForAutomatedTest()
        {
            if (mainCamera != null) body.position = ClampToCamera(body.position);
        }
    }
}
