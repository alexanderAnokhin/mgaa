using UnityEngine;
using System.Collections;

public class AStarController : Controller {
	
	override public void Move (out int xDir, out int yDir) {
		float[,] weights = Utils.GetMapWeights ();

		xDir = 1;
		yDir = 0;
	}
}
