using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Gal.Core
{
    /// <summary>
    /// 线程安全的对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <para>author gouanlin</para>
    public class SafePool<T> : IPool<T>
    {
        private readonly Func<T> m_ElementGenerate;
        private readonly Action<T> m_ElementAwake;
        private readonly Action<T> m_ElementSleep;

        internal readonly ConcurrentStack<T> m_Stack;

        private int m_MaxCacheSize;

        public int maxCacheSize {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_MaxCacheSize;
            set {
                Debug.Assert(value > 0, $"{nameof(maxCacheSize)} cannot be negative");

                m_MaxCacheSize = value;
                while (m_Stack.Count > m_MaxCacheSize) m_Stack.TryPop(out _);
            }
        }

        public SafePool(Func<T> elementGenerate, Action<T> elementSleep, Action<T> elementAwake, int maxCacheSize = 8) {
            Debug.Assert(elementGenerate != null, $"Parameter '{nameof(elementGenerate)}' cannot be null");
            Debug.Assert(elementSleep != null, $"Parameter '{nameof(elementSleep)}' cannot be null");
            Debug.Assert(maxCacheSize >= 0, $"Parameter '{nameof(maxCacheSize)}' cannot be negative");

            m_ElementGenerate = elementGenerate;
            m_ElementSleep = elementSleep;
            m_ElementAwake = elementAwake;
            m_MaxCacheSize = maxCacheSize;

            m_Stack = new();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get() {
            if (m_Stack.Count == 0 || !m_Stack.TryPop(out var t)) t = m_ElementGenerate();
            m_ElementAwake?.Invoke(t);
            return t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Put(T element) {
            m_ElementSleep(element);
            if (m_Stack.Count >= m_MaxCacheSize) return;
            m_Stack.Push(element);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => m_Stack.Clear();
    }
}