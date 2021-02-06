using SourceUtils.ValveBsp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.BSP
{
	public partial class BspToUnity
	{
		private bool BrushHasFlag(int modelIndex, int solidNumber, BrushContents flag)
		{
			var model = _bsp.Models[modelIndex];
			var node = _bsp.Nodes[model.HeadNode];

			foreach (var leafIndex in BspUtils.GetNodeLeafs(_bsp, node))
			{
				var leaf = _bsp.Leaves[leafIndex];
				var first = leaf.FirstLeafBrush;
				var last = leaf.FirstLeafBrush + leaf.NumLeafBrushes;
				var idx = first + solidNumber;
				if (idx < last)
				{
					return _bsp.Brushes[_bsp.LeafBrushes[idx]].Contents.HasFlag(flag);
				}
			}

			return false;
		}
	}
}
