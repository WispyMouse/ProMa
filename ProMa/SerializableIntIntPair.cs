using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProMa
{
	[Serializable]
	public class SerializableIntIntPair
	{
		public int Key { get; set; }
		public int Value { get; set; }

		public SerializableIntIntPair()
		{

		}

		public SerializableIntIntPair(int key, int value)
		{
			this.Key = key;
			this.Value = value;
		}
	}
}