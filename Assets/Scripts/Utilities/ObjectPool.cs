// ObjectPool.cs — 투사체/이펙트 재사용 풀 (문서 06-5)
using System.Collections.Generic;
using UnityEngine;

namespace BroDungeon.Utilities
{
    /// <summary>MonoBehaviour 프리팹용 제네릭 풀.</summary>
    public class ObjectPool<T> where T : Component
    {
        readonly T _prefab;
        readonly Transform _parent;
        readonly Queue<T> _free = new Queue<T>();
        readonly HashSet<T> _active = new HashSet<T>();

        public ObjectPool(T prefab, int prewarm = 0, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarm; i++)
            {
                var o = Object.Instantiate(_prefab, _parent);
                o.gameObject.SetActive(false);
                _free.Enqueue(o);
            }
        }

        public T Get(Vector3 pos, Quaternion rot)
        {
            T o = _free.Count > 0 ? _free.Dequeue() : Object.Instantiate(_prefab, _parent);
            o.transform.SetPositionAndRotation(pos, rot);
            o.gameObject.SetActive(true);
            _active.Add(o);
            return o;
        }

        public void Release(T o)
        {
            if (!_active.Remove(o)) return;
            o.gameObject.SetActive(false);
            _free.Enqueue(o);
        }

        public void ReleaseAll()
        {
            foreach (var o in _active) { o.gameObject.SetActive(false); _free.Enqueue(o); }
            _active.Clear();
        }
    }
}
