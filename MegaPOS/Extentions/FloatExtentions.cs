using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Extentions
{
	public static class FloatExtentions
	{
		public static float Map(this float x, float in_min, float in_max, float out_min, float out_max)
		{
			return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
		}
	}
}
