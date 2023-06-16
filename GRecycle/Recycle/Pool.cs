using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Gal.Core
{
    /// <summary>
    /// 对象池(非线程安全)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <para>author gouanlin</para>
    public class Pool<T> : IPool<T>
    {
        private int m_MaxCacheSize;

        private readonly Func<T> m_ElementGenerate;
        private readonly Action<T> m_ElementAwake;
        private readonly Action<T> m_ElementSleep;

        private readonly Stack<T> m_Stack;

        public int maxCacheSize {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_MaxCacheSize;
            set {
                Debug.Assert(value > 0, $"{nameof(maxCacheSize)} cannot be negative");
 
                m_MaxCacheSize = value;
                while (m_Stack.Count > m_MaxCacheSize) m_Stack.Pop();
            }
        }

        public Pool(Func<T> elementGenerate, Action<T> elementSleep, Action<T> elementAwake, int maxCacheSize = 8) {
            Debug.Assert(maxCacheSize > 0, $"{nameof(maxCacheSize)} cannot be negative");
            Debug.Assert(elementGenerate != null, $"{nameof(elementGenerate)}");
            Debug.Assert(elementSleep != null, $"{nameof(elementSleep)}");

            m_ElementGenerate = elementGenerate;
            m_ElementSleep = elementSleep;
            m_ElementAwake = elementAwake;
            m_MaxCacheSize = maxCacheSize;

            m_Stack = new(maxCacheSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get() {
            var t = m_Stack.Count > 0 ? m_Stack.Pop() : m_ElementGenerate();
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