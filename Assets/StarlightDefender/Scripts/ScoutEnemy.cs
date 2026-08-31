using UnityEngine;

namespace StarlightDefender
{
    public sealed class ScoutEnemy : EnemyBase
    {
        protected override void MoveEnemy() => transform.Translate(Vector3.down * (moveSpeed * Time.deltaTime), Space.World);
    }
}
