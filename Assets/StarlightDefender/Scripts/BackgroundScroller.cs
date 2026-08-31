using System.Collections.Generic;
using UnityEngine;

namespace StarlightDefender
{
    public sealed class BackgroundScroller : MonoBehaviour
    {
        [SerializeField] private Sprite starSprite;
        [SerializeField] private int farStarCount = 42;
        [SerializeField] private int nearStarCount = 24;
        private readonly List<Star> stars = new();
        private Camera mainCamera;

        private sealed class Star
        {
            public Transform Transform;
            public float Speed;
        }

        public void Configure(Sprite sprite) => starSprite = sprite;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null || starSprite == null) return;
            mainCamera.backgroundColor = new Color(0.008f, 0.012f, 0.055f);
            CreateLayer(farStarCount, 0.45f, 0.04f, 0.1f, -20);
            CreateLayer(nearStarCount, 1.15f, 0.08f, 0.18f, -10);
        }

        private void CreateLayer(int count, float speed, float minScale, float maxScale, int order)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject star = new("Star");
                star.transform.SetParent(transform);
                SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = starSprite;
                renderer.sortingOrder = order;
                float scale = Random.Range(minScale, maxScale);
                star.transform.localScale = Vector3.one * scale;
                star.transform.position = ViewportPosition(Random.value, Random.value);
                stars.Add(new Star { Transform = star.transform, Speed = speed * Random.Range(0.75f, 1.25f) });
            }
        }

        private void Update()
        {
            if (mainCamera == null) return;
            float bottom = mainCamera.ViewportToWorldPoint(Vector3.zero).y - 0.3f;
            float top = mainCamera.ViewportToWorldPoint(Vector3.up).y + 0.3f;
            foreach (Star star in stars)
            {
                star.Transform.Translate(Vector3.down * (star.Speed * Time.deltaTime), Space.World);
                if (star.Transform.position.y >= bottom) continue;
                Vector3 position = ViewportPosition(Random.value, 1f);
                position.y = top;
                star.Transform.position = position;
            }
        }

        private Vector3 ViewportPosition(float x, float y)
        {
            Vector3 position = mainCamera.ViewportToWorldPoint(new Vector3(x, y, 10f));
            position.z = 1f;
            return position;
        }
    }
}
