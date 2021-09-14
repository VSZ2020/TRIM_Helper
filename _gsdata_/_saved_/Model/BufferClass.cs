using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRIM_Helper.Model
{
	public static class BufferClass
	{
		public static ComputationalTask BufferTask;
		public static TrimInputFile BufferInputFile;
		public static TRIMDatFile BufferDatFile;

		public static void CleanBuffer()
		{
			BufferInputFile = null;
			BufferDatFile = null;
		}
	}
}
