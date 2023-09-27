namespace Gal.Core
{
	/// <summary>
	/// 对象池
	/// </summary>
	/// <para>author gouanlin</para>
	public interface IPool
	{
		int maxCacheSize { get; set; }
		void Clear();
	}

	/// <summary>
	/// 对象池
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <para>author gouanlin</para>
	public interface IPool<T> : IPool
	{
		T Get();
		void Put(T item);
	}
}