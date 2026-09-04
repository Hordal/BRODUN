// Projectile.cs — 마법/원거리 투사체 (오브젝트 풀 대상, 문서 06-5)
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Combat
{
    public class Projectile : MonoBehaviour
    {
        [HideInInspector] public ICombatant Owner;
        [HideInInspector] public AttackRequest Request;
        [HideInInspector] public LayerMask HitMask;

        public float speed = 12f;
        public float lifetime = 3f;
        public int pierce = 0;     // 관통 횟수 (연쇄/관통 속성)
        public int bounce = 0;     // 바운스 횟수

        Vector2 _dir;
        float _life;
        int _hitsLeft;
        System.Action<Projectile> _onRelease;

        public void Launch(Vector2 dir, System.Action<Projectile> onRelease)
        {
            _dir = dir.normalized; _life = lifetime; _hitsLeft = pierce + 1;
            _onRelease = onRelease;
        }

        void Update()
        {
            transform.Translate(_dir * speed * Time.deltaTime, Space.World);
            _life -= Time.deltaTime;
            if (_life <= 0f) _onRelease?.Invoke(this);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if ((HitMask.value & (1 << other.gameObject.layer)) == 0) return;
            var c = other.GetComponentInParent<ICombatant>();
            if (c == null || !c.IsAlive || c == Owner) return;

            var req = Request; req.Target = c;
            CombatManager.Instance.ProcessAttack(in req);

            _hitsLeft--;
            if (_hitsLeft <= 0)
            {
                if (bounce > 0) { bounce--; _hitsLeft = 1; _dir = -_dir; } // 단순 바운스
                else _onRelease?.Invoke(this);
            }
        }
    }
}
