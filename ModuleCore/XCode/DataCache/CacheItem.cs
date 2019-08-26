using System;
using System.Collections.Generic;
using System.Text;

namespace DNNGo.Modules.DNNGalleryPro
{
	internal class CacheItem<T>
	{
		private T _TValue;
		/// <summary>
		/// �����DataSet
		/// </summary>
		public T TValue
		{
			get { return _TValue; }
		}

		private List<String> _TableNames;
		/// <summary>
		/// �������ı��ı���
		/// </summary>
		public List<String> TableNames
		{
			get { return _TableNames; }
		}

		/// <summary>
		/// ����ʱ��
		/// </summary>
		public DateTime CacheTime = DateTime.Now;

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="tableNames"></param>
		/// <param name="tvalue"></param>
		public CacheItem(String[] tableNames, T tvalue)
		{
			if (tableNames != null && tableNames.Length > 0)
			{
				if (_TableNames == null)
					_TableNames = new List<string>();
				else
					_TableNames.Clear();

				for (Int32 i = 0; i < tableNames.Length; i++)
					if (!_TableNames.Contains(tableNames[i]))
						_TableNames.Add(tableNames[i]);
			}
			_TValue = tvalue;
		}

		/// <summary>
		/// �Ƿ�������ĳ����
		/// </summary>
		/// <param name="tableName">����</param>
		/// <returns></returns>
		public Boolean IsDependOn(String tableName)
		{
			// �ձ���������*����ʾȫ��ƥ��
			if (String.IsNullOrEmpty(tableName) || tableName == "*") return true;
			// ��������������ƥ��
			if (_TableNames.Contains(tableName)) return true;
			// ���Կ�������ʹ��*ʵ��ǰ׺ƥ����׺ƥ��
			return false;
		}
	}
}