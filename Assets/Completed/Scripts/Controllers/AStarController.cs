using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AStarController : Controller {
	class Node {
		private int x;
		private int y;
		public float travelWeight;
		public float heuristicWeight;
		public Node parent;

		public Node (int x, int y) {
			this.x = x;
			this.y = y;
		}

		public Node (int x, int y, float travelWeight, Node parent) {
			this.x = x;
			this.y = y;
			this.travelWeight = travelWeight;
			this.parent = parent;
		}

		public bool Equals(Node that) {
			if (that == null) { return false; }
			else if (this.GetX () == that.GetX () && this.GetY () == that.GetY ()) { return true; }
			else { return false; }
		}

		public int GetX () { return x; }
		public int GetY () { return y; }
		public float GetTotalWeight () { return travelWeight + heuristicWeight; }
		public Vector2 GetVector () { return new Vector2((float)this.x, (float)this.y); }
		
		public void SetTravelWeight (float travelWeight) { this.travelWeight = travelWeight; }
		public void SetHeuristicWeight (Node goal) {
			heuristicWeight = -1 * (Mathf.Abs ((float)goal.GetX () - (float)this.GetX ()) + Mathf.Abs ((float)goal.GetY () - (float)this.GetY ())); 		
		}
		public void SetParent (Node parent) { this.parent = parent; }

		public List<Node> GetConnections (float[, ] weights, Node goal) {
			List<Node> connections = new List<Node> ();

			foreach (Vector2 pointAround in Utils.POINTS_AROUND) {
				Vector2 newPoint = GetVector () + pointAround;
				if (Utils.OnMap ((int)newPoint.x, (int)newPoint.y) && weights[(int)newPoint.x, (int)newPoint.y] != Utils.HUGE_PENALTY) {
					Node connection = new Node ((int)newPoint.x, (int)newPoint.y, this.travelWeight + weights[(int)newPoint.x, (int)newPoint.y], this);
					connection.SetHeuristicWeight (goal);
					connections.Add (connection);
				}
			}	

			return connections;
		}

		public bool IsBetter (Node that) {
			return this.travelWeight > that.travelWeight; 
		}
	}	

	override public void Move (out int xDir, out int yDir) {
		GameObject player = Utils.GetPlayerGameObject ();

		Vector2 playerPosition = (Vector2)player.transform.position;
		Vector2 exitPosition = Utils.GetExitPosition ();		
		
		Node start = new Node ((int)playerPosition.x, (int)playerPosition.y);
		Node goal = new Node ((int)exitPosition.x, (int)exitPosition.y);

		Debug.Log ("here1");
		List<Vector2> route = AStartPathfindingAlgorithm (start, goal);
		Debug.Log ("here2");
		xDir = 1;
		yDir = 0;
	}

	private List<Vector2> AStartPathfindingAlgorithm (Node start, Node goal) {
		List<Vector2> route = null;
		float[,] weights = Utils.GetMapWeights ();

		List<Node> open = new List<Node> ();
		List<Node> closed = new List<Node> ();

		start.SetHeuristicWeight (goal); 
		open.Add (start);

		while (open.Count > 0) {
			Node node = open.OrderBy(n => -n.GetTotalWeight ()).First ();
			
			if (node.Equals (goal)) {
				route = GetRoute (goal);
				return route;
			}
			else {
				List<Node> connections = node.GetConnections (weights, goal);

				foreach (Node connection in connections) {
					if (closed.Exists (n => n.Equals (connection))) {
						continue;
					}
					else if (open.Exists (n => n.Equals (connection))) {
						Node fromOpenList = open.Find (n => n.Equals (connection));
						if (connection.IsBetter (fromOpenList)) {
							open.Remove (fromOpenList);
							open.Add (connection);	
						}
					}
					else {
						open.Add (connection);
					}
				}
			}

			open.Remove (node);
		}

		return route;		
	}

	private List<Vector2> GetRoute (Node goal) {
		List<Vector2> route = new List<Vector2> ();
		Node current = goal;
		
		while (current.parent != null) {
			route.Add (current.GetVector ());
			current = current.parent;
		}

		return route;
	}
}
