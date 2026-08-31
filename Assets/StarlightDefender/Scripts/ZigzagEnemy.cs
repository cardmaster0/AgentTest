using UnityEngine;

namespace StarlightDefender
{
    public sealed class ZigzagEnemy : EnemyBase
    {
        [SerializeField] private float amplitude = 1.35f;
        [SerializeField] private float frequency = 2.4f;
        private float originX;
        private float age;

        protected override void MoveEnemy()
        {
            age += Time.deltaTime;
            Vector3 position = transform.position;
            position.y -= moveSpeed * Time.deltaTime;
            position.x = originX + Mathf.Sin(age * frequency) * amplitude;
            transform.position = position;
        }

        public override void OnPoolSpawned()
        {
            base.OnPoolSpawned();
            originX = transform.position.x;
            age = 0f;
        }
    }
}
