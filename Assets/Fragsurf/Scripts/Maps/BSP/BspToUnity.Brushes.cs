using SourceUtils.ValveBsp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.BSP
{
	public partial class BspToUnity
	{
		// i don't think this was right
		//private BrushContents GetBrushContents(int modelIndex, int solidNumber)
		//{
		//	var model = _bsp.Models[modelIndex];
		//	var node = _bsp.Nodes[model.HeadNode];

		//	foreach (var leafIndex in BspUtils.GetNodeLeafs(_bsp, node))
		//	{
		//		var leaf = _bsp.Leaves[leafIndex];
		//		var first = leaf.FirstLeafBrush;
		//		var last = leaf.FirstLeafBrush + leaf.NumLeafBrushes;
		//		var idx = first + solidNumber;
		//		if (idx < last)
		//		{
		//			return _bsp.Brushes[_bsp.LeafBrushes[idx]].Contents;
		//		}
		//	}

		//	return default;
		//}
		//private bool BrushHasFlag(int modelIndex, int solidNumber, BrushContents flag)
		//{
		//	return GetBrushContents(modelIndex, solidNumber).HasFlag(flag);
		//}
	}
}
