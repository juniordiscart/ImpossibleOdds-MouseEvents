using System;
using System.Linq;
using ImpossibleOdds.MouseEvents;
using UnityEditor;

namespace ImpossibleOdds
{
	[InitializeOnLoad]
	internal class ScriptExecutionOrder
	{
		static ScriptExecutionOrder()
		{
			MonoScript[] allScripts = MonoImporter.GetAllRuntimeMonoScripts();

			// Find the mouse events monitor, and make sure it is put as the first executing script.
			MonoScript mouseEventMonitorScript = Array.Find(allScripts, (s) => s.GetClass() == typeof(MouseEventMonitor));
			int mouseEventMonitorSEO = MonoImporter.GetExecutionOrder(mouseEventMonitorScript);
			int minSEO = allScripts.Min(MonoImporter.GetExecutionOrder);

			if ((minSEO == 0) || (mouseEventMonitorSEO > minSEO))
			{
				MonoImporter.SetExecutionOrder(mouseEventMonitorScript, minSEO - 1);
			}
		}
	}
}
