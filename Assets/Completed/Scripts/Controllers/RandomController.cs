using UnityEngine;
using System.Collections;

public class RandomController : Controller {

    override public void Move (out int xDir, out int yDir) {
       xDir = 1;
       yDir = 0;
    }
}
