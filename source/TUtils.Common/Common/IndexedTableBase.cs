using System;
using System.Collections.Generic;
using System.Linq;

namespace TUtils.Common.Common
{
	/// <summary>
	/// A set of rows, that provides performance optimzed Find-methods to it's values.
	/// 
	/// </summary>
	public class IndexedTableBase
	{
		#region types

		/// <summary>
		/// It's like a database index: represents an index of one or more columns of a row-set.
		/// Contains a mapping between keys and matching rows
		/// </summary>
		protected class Index
		{
			/// <summary>
			/// A performance optimized Dictionary (ComparableObjectArray -> bool).
			/// If it contains only one item, it won't create a dictionary internally.
			/// With FastSet you can put items of type ComparableObjectArray into a 
			/// bag, iterate through all items and remove a single item fast.
			/// </summary>
			private class FastSet
			{
				/// <summary>
				/// may be null, if dictionary isn't neccessary
				/// </summary>
				private Dictionary<ComparableObjectArray, bool> _dictionary;

				/// <summary>
				/// != null, if there is only one item in bag
				/// </summary>
				private ComparableObjectArray _singleValue;

				/// <summary>
				/// inserts an item into the bag
				/// </summary>
				/// <param name="value"></param>
				public void Set(ComparableObjectArray value)
				{
					if (_dictionary != null)
					{
						_dictionary[value] = true;
					}
					else
					{
						if (_singleValue != null)
						{
							// ReSharper disable once UseObjectOrCollectionInitializer
							_dictionary = new Dictionary<ComparableObjectArray, bool>();
							_dictionary[_singleValue] = true;
							_dictionary[value] = true;
							_singleValue = null;
						}
						else
						{
							_singleValue = value;
						}
					}
				}

				/// <summary>
				/// Tries to remove the passed item from bag.
				/// The item may not be present.
				/// </summary>
				/// <param name="value"></param>
				public void TryRemove(ComparableObjectArray value)
				{
					if (_dictionary != null)
					{
						if (_dictionary.ContainsKey(value))
							_dictionary.Remove(value);
					}
					else
					{
						if (_singleValue != null && _singleValue == value)
						{
							_singleValue = null;
						}
					}
				}

				/// <summary>
				/// Iterates through all items
				/// </summary>
				/// <returns></returns>
				public IEnumerable<ComparableObjectArray> GetValues()
				{
					if (_dictionary != null)
						return _dictionary.Keys.ToList();
					if (_singleValue != null)
						return new[] {_singleValue};
					return new ComparableObjectArray[0];
				}
			}

			/// <summary>
			/// The column numbers of the columns which are being indexed by this Index object.
			/// Zero-based !
			/// size: number of indexed columns
			/// </summary>
			private readonly int[] _indexedColPositions;

			/// <summary>
			/// Dictionary !
			/// (indexed row values) -> (rows matching the pattern)
			/// e.g.: Assume the table contains 4 columns of type int - 
			/// One entry of the dictionary could be:
			/// (null,2,12,null) -> {(11,2,12,65),(53,2,12,73),(25,2,12,53)}
			/// This means the key (null,2,12,null) references 3 rows with same key-pattern
			/// </summary>
			private readonly Dictionary<ComparableObjectArray, FastSet> _dictionary = new Dictionary<ComparableObjectArray, FastSet>();

			/// <summary>
			/// 
			/// </summary>
			/// <param name="indexedColPositions">
			/// The column numbers of the columns which are being indexed by this Index object.
			/// Zero-based !
			/// size: number of indexed columns
			/// Must be ordered !
			/// e.g.: (1,3,6)
			/// </param>
			public Index(int[] indexedColPositions)
			{
				_indexedColPositions = indexedColPositions;
			}

			/// <summary>
			/// Checks if given index-definition is the same as this index
			/// </summary>
			/// <param name="indexedColPositions"></param>
			/// <returns></returns>
			public bool IsEqual(int[] indexedColPositions)
			{
				if (_indexedColPositions.Length != indexedColPositions.Length)
					return false;

				// ReSharper disable once LoopCanBeConvertedToQuery
				for (int i = 0; i < indexedColPositions.Length; i++)
				{
					if (_indexedColPositions[i] != indexedColPositions[i])
						return false;
				}

				return true;
			}

			/// <summary>
			/// All rows of table. Since all rows will be indexed the index conatins all 
			/// rows of the table.
			/// Note ! A data table "IndexedTableBase" may contain more than one Index.
			/// Each Index contains all rows !
			/// </summary>
			/// <returns></returns>
			internal IEnumerable<ComparableObjectArray> GetAllValues()
			{
				var allValues = new List<ComparableObjectArray>();
				foreach (var collection in _dictionary.Values)
					allValues.AddRange(collection.GetValues());	

				return allValues;
			}

			/// <summary>
			/// Returns all rows which match the passed pattern
			/// </summary>
			/// <param name="key">
			/// e.g.: 
			/// (32,null,55): means that we want to have all rows with the first item set to 32 and the third item set to 55
			/// </param>
			/// <returns>may be null !</returns>
			internal IEnumerable<ComparableObjectArray> GetValuesOfKey(ComparableObjectArray key)
			{
				FastSet values;
				if (_dictionary.TryGetValue(key, out values))
					return values.GetValues();
				return null;
			}

			/// <summary>
			/// e.g.:
			/// value = (63,0,765,7) and _indexedColPositions= {1,2}
			/// ==> 
			/// returns: (null,0,765,null)
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			private ComparableObjectArray CreateKey(object[] value)
			{
				var key = new object[value.Length];
				foreach (var indexedColPosition in _indexedColPositions)
				{
					key[indexedColPosition] = value[indexedColPosition];
				}
				return new ComparableObjectArray(key);
			}

			/// <summary>
			/// Inserts a row into the Index
			/// </summary>
			/// <param name="value"></param>
			internal void InsertValue(ComparableObjectArray value)
			{
				var key = CreateKey(value.Array);
				var valueSet = GetValues(key);
				valueSet.Set(value);
			}


			private FastSet GetValues(ComparableObjectArray key)
			{
				FastSet values;
				if (!_dictionary.TryGetValue(key, out values))
				{
					values = new FastSet();
					_dictionary[key] = values;
				}
				return values;
			}

			/// <summary>
			/// Removes a key.
			/// </summary>
			/// <param name="key">
			/// !!!!! must fit to the pattern given by _indexedColPositions
			/// e.g.: if _indexedColPositions= [1,2]
			/// ==> key may be (null,0,765,null)
			/// but not (1,null,765,null)
			/// 
			/// </param>
			internal void RemoveKey(ComparableObjectArray key)
			{
				_dictionary.Remove(key);
			}

			/// <summary>
			/// Removes a single value / row from the Index.
			/// </summary>
			/// <param name="value"></param>
			internal void RemoveValue(ComparableObjectArray value)
			{
				var key = CreateKey(value.Array);
				var values = GetValues(key);
				values.TryRemove(value);
			}
		}

		#endregion

		#region private

		private int _countColumns;

		private readonly List<Index> _indexes = new List<Index>();

		/// <summary>
		/// The column numbers of the indexed columns
		/// e.g.: keys = {null,234,null,"jhs"} -> returns new int[] {1,3}
		/// e.g.: keys = {234,"jhs",null,null} -> returns new int[] {0,1}
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		private int[] GetKeyIndexes(object[] keys)
		{
			List<int> keyIndexes = new List<int>();
			for (int i = 0; i < keys.Length; i++)
			{
				if ( keys[i] != null)
					keyIndexes.Add(i);
			}
			return keyIndexes.ToArray();
		}

		private Index FindIndex(int[] keyIndexes)
		{
			return _indexes.FirstOrDefault(indx => indx.IsEqual(keyIndexes));
		}

		#endregion

		#region protected

		/// <summary>
		/// 
		/// </summary>
		/// <param name="countColumns">
		/// count of items in a row / count of columns
		/// </param>
		protected void Init(int countColumns)
		{
			_countColumns = countColumns;
		}

		protected void InsertValue(object[] value)
		{
			if (!_indexes.Any())
				EnsureIndex(GetKeyIndexes(value));

			var val = new ComparableObjectArray(value);

			foreach (var index in _indexes)
			{
				index.InsertValue(val);
			}			
		}

		protected void RemoveValue(object[] value)
		{
			var val = new ComparableObjectArray(value);

			foreach (var index in _indexes)
			{
				index.RemoveValue(val);
			}
		}

		protected void RemoveAllBy(object[] keys)
		{
			var key = new ComparableObjectArray(keys);
			var keyIndexes = GetKeyIndexes(keys);
			var rootIndex = EnsureIndex(keyIndexes);
			var values = rootIndex.GetValuesOfKey(key);
			rootIndex.RemoveKey(key);

			if (values != null)
			{
				foreach (var index in _indexes)
				{
					// ReSharper disable once PossibleMultipleEnumeration
					foreach (var value in values)
						index.RemoveValue(value);
				}
			}
		}

		protected IEnumerable<object[]> GetAllRows()
		{
			var firstIndex = _indexes.FirstOrDefault();
			if (firstIndex == null)
				return new List<object[]>();
			return firstIndex.GetAllValues().Select(val => val.Array);
		}

		protected IEnumerable<object[]> Find(object[] keys)
		{
			var keyIndexes = GetKeyIndexes(keys);
			var index = EnsureIndex(keyIndexes);
			var values = index.GetValuesOfKey(new ComparableObjectArray(keys));
			if (values == null)
				values = new ComparableObjectArray[0];
			return values.Select(val=>val.Array);
		}

		/// <summary>
		///  ensures that there is an index for the given combined column key
		/// </summary>
		/// <param name="columnsNumbers"></param>
		/// <returns></returns>
		protected Index EnsureIndex(params int[] columnsNumbers)
		{
			columnsNumbers = columnsNumbers.OrderBy(val => val).ToArray();

			var index = FindIndex(columnsNumbers);
			if (index != null)
				return index;

			if ( columnsNumbers.Any(idx => idx >= _countColumns))
				throw new ApplicationException("0 <= column number < _countColumns " + _countColumns);

			index = new Index(columnsNumbers);
			_indexes.Add(index);

			if (_indexes.Count > 1)
			{
				var firstIndex = _indexes.First();
				foreach (var value in firstIndex.GetAllValues())
				{
					index.InsertValue(value);
				}
			}

			return index;
		}

		#endregion
	}
}