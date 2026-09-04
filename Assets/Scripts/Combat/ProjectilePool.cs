// ProjectilePool.cs — 투사체 풀 래퍼 (문서 06-5 오브젝트 풀)
using UnityEngine;
using BroDungeon.Data;
using BroDungeon.Utilities;

namespace BroDungeon.Combat
{
    public class ProjectilePool : MonoBehaviour
    {
        public Projectile prefab;
        public int prewarm = 32;
        ObjectPool<Projectile> _pool;

        // 런타임에 prefab을 나중에 주입하는 경우를 위해 지연 초기화(Awake 순서 문제 회피).
        void EnsurePool()
        {
            if (_pool != null || prefab == null) return;
            _pool = new ObjectPool<Projectile>(prefab, prewarm, transform);
        }

        public void Fire(ICombatant owner, AttackRequest req, Vector3 pos, Vector2 dir,
                         LayerMask hitMask, int pierce = 0, int bounce = 0)
        {
            EnsurePool();
            if (_pool == null) return; // prefab 미설정
            var p = _pool.Get(pos, Quaternion.identity);
            p.Owner = owner; p.Request = req; p.HitMask = hitMask;
            p.pierce = pierce; p.bounce = bounce;
            p.Launch(dir, Release);
        }

        void Release(Projectile p) => _pool.Release(p);
        public void ClearAll() => _pool?.ReleaseAll();
    }
}
