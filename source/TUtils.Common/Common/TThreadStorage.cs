using System.Threading;

namespace TUtils
{
	/// <summary>
	/// Represents an object, which is bound to the current thread.
	/// </summary>
	/// <remarks>
	/// <example><code><![CDATA[
	/// 
	/// public class MyClassA
	/// {
	/// 	public int value;
	/// }
	/// 
	/// public class Test
	/// {
	/// 	private static void Do(string id)
	/// 	{
	/// 		MyClassA a1 = TThreadStorage<MyClassA>.GetData(id);
	/// 		Debug.WriteLine(string.Format(
	/// 			"thread={0} id={1} a1={2}",
	/// 			Thread.CurrentThread.ManagedThreadId,
	/// 			id,
	/// 			a1.value));
	/// 		a1.value++;
	/// 	}
	/// 
	/// 	private static void ThreadMethod()
	/// 	{
	/// 		Do("ID1");
	/// 		Do("ID1");
	/// 		Do("ID2");
	/// 		Do("ID2");
	/// 		
	/// 	}
	/// 
	/// 	public static void Test()
	/// 	{
	/// 		var thread = new System.Threading.Thread(ThreadMethod);
	/// 		thread.Start();
	/// 
	/// 		thread = new System.Threading.Thread(ThreadMethod);
	/// 		thread.Start();			
	/// 	}
	/// }
	/// 
	/// output:
	///		thread=1 id=ID1 a1=0
	///		thread=1 id=ID1 a1=1
	///		thread=1 id=ID2 a1=0
	///		thread=1 id=ID2 a1=1
	///		thread=2 id=ID1 a1=0
	///		thread=2 id=ID1 a1=1
	///		thread=2 id=ID2 a1=0
	///		thread=2 id=ID2 a1=1
	/// ]]></code></example>
	/// </remarks>
	/// <typeparam name="CLASSTYPE">
	/// Must be a class type with a default constructor.
	/// If you want to bind a single int or a struct to a thread you may also 
	/// try to use class 'BoxedObject'.
	/// <code><![CDATA[
	/// var a1 = TThreadStorage<BoxedObject<int>>.GetData(id);
	/// a1.Value++;
	/// ]]>
	/// </code>
	/// </typeparam>
	public class TThreadStorage<CLASSTYPE> where  CLASSTYPE: class, new()
	{
		public string Identifier { get; private set; }

		private CLASSTYPE cachedValue;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="identifier"></param>
		public TThreadStorage(string identifier)
		{
			Identifier = identifier;
		}

		/// <summary>
		/// Checks, if there is an object with the given name bound to the current thread.
		/// If not it creates one and binds it to the current thread.
		/// Returns the object.
		/// </summary>
		/// <remarks>
		/// Caches the object in this instance.
		/// </remarks>
		/// <returns></returns>
		public CLASSTYPE GetData()
		{
			if (cachedValue == null)
			{
				var threadDataSlot = Thread.GetNamedDataSlot(Identifier);
				object obj = Thread.GetData(threadDataSlot);
				if (obj == null)
				{
					obj = new CLASSTYPE();
					Thread.SetData(threadDataSlot, obj);
				}

				cachedValue = (CLASSTYPE)obj;
			}

			return cachedValue;
		}

		/// <summary>
		/// Checks, if there is an object with the given name bound to the current thread.
		/// If not it creates one and binds it to the current thread.
		/// Returns the object.
		/// </summary>
		/// <param name="identifier"></param>
		/// <returns></returns>
		public static CLASSTYPE GetData(string identifier)
		{
			var storage = new TThreadStorage<CLASSTYPE>(identifier);
			return storage.GetData();
		}
	}

	public class BoxedObject<INTRINSICTYPE>
	{
		public INTRINSICTYPE Value { get; set; }

		public BoxedObject()
		{
		}
	}

}
