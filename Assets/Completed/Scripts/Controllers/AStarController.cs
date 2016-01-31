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
        public void SetHeuristicWeight (Node goal, int heuristicType) {
			switch (heuristicType) {
			case 1:
				this.heuristicWeight = HeuristicOne (goal);
				break;
			case 2:
				this.heuristicWeight = HeuristicTwo (goal);
				break;
			case 3:
				this.heuristicWeight = HeuristicThree (goal);
				break;
			default:
				this.heuristicWeight = HeuristicOne (goal);
				break;
			}
        }

        public void SetParent (Node parent) { this.parent = parent; }

        public List<Node> GetConnections (float[, ] weights, Node goal, int heuristicType) {
            List<Node> connections = new List<Node> ();

            foreach (Vector2 pointAround in Utils.POINTS_AROUND) {
                Vector2 newPoint = GetVector () + pointAround;
                if (Utils.OnMap ((int)newPoint.x, (int)newPoint.y) && weights[(int)newPoint.x, (int)newPoint.y] != Utils.HUGE_PENALTY) {
                    Node connection = new Node ((int)newPoint.x, (int)newPoint.y, this.travelWeight + weights[(int)newPoint.x, (int)newPoint.y], this);
					connection.SetHeuristicWeight (goal, heuristicType);
                    connections.Add (connection);
                }
            }   

            return connections;
        }

		private float HeuristicOne (Node goal) {
			return -1 * (Mathf.Abs ((float)goal.GetX () - (float)this.GetX ()) + Mathf.Abs ((float)goal.GetY () - (float)this.GetY ()));         
		}

		private float HeuristicTwo (Node goal) {			
			float heuristicWeight = HeuristicOne (goal);
			
			GameObject[] foods = Utils.GetFoodGameObjects ();
			GameObject[] sodas = Utils.GetSodaGameObjects ();

			foreach (GameObject food in foods) {
				Vector2 foodPos = (Vector2)food.transform.position;
				heuristicWeight += Utils.FOOD_INCREASE -1 * (Mathf.Abs (foodPos.x - (float)this.GetX ()) + Mathf.Abs (foodPos.y - (float)this.GetY ()));         
			}

			foreach (GameObject soda in sodas) {
				Vector2 sodaPos = (Vector2)soda.transform.position;
				heuristicWeight += Utils.SODA_INCREASE -1 * (Mathf.Abs (sodaPos.x - (float)this.GetX ()) + Mathf.Abs (sodaPos.y - (float)this.GetY ()));         
			}			
			Debug.Log ("weight: " + heuristicWeight);
			return heuristicWeight;         
		}

		private float HeuristicThree (Node goal) {
			return 5 * (HeuristicTwo (goal) - HeuristicOne (goal)) + HeuristicOne (goal);
		}

        public bool IsBetter (Node that) {
            return this.travelWeight > that.travelWeight; 
        }
    }   
	
	private int heuristicType;
    private float shift;

	public AStarController (int heuristicType, float shift) {
		this.heuristicType = heuristicType;
        this.shift = shift;	
	}

    override public void Move (out int xDir, out int yDir) {
        Utils.PlotMatrix (shift);

        GameObject player = Utils.GetPlayerGameObject ();

        Vector2 playerPosition = (Vector2)player.transform.position;
        Vector2 exitPosition = Utils.GetExitPosition ();        
        
        Node start = new Node ((int)playerPosition.x, (int)playerPosition.y);
        Node goal = new Node ((int)exitPosition.x, (int)exitPosition.y);

		List<Vector2> route1 = AStartPathfindingAlgorithm (start, goal, 1);
		List<Vector2> route2 = AStartPathfindingAlgorithm (start, goal, 2);
        List<Vector2> route3 = AStartPathfindingAlgorithm (start, goal, 3);
        
		List<Vector2> route;

		switch (heuristicType) {
		case 1:
			route = route1;
			break;
		case 2:
			route = route2;
			break;
		case 3:
			route = route3;
			break;
		default:
			route = route1;
			break;
		}		
		
		if (route == null) {
            Debug.Log ("No route found!");
            xDir = 0;
            yDir = 0;
        }
        else {
            Vector2 move = route.Last () - playerPosition;
            xDir = (int)move.x;
            yDir = (int)move.y;
            Utils.PlotRoute (route1, 1);
			Utils.PlotRoute (route2, 2);
			Utils.PlotRoute (route3, 3);
        }       
    }

    private List<Vector2> AStartPathfindingAlgorithm (Node start, Node goal, int heuristicType) {
        List<Vector2> route = null;
        float[,] weights = Utils.GetMapWeights (shift);

        List<Node> open = new List<Node> ();
        List<Node> closed = new List<Node> ();

        start.SetHeuristicWeight (goal, heuristicType); 
        open.Add (start);

        while (open.Count > 0) {
            Node node = open.OrderBy(n => -n.GetTotalWeight ()).First ();
            
            if (node.Equals (goal)) {
                route = GetRoute (node);
                return route;
            }
            else {
				List<Node> connections = node.GetConnections (weights, goal, heuristicType);

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
            closed.Add (node);
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
