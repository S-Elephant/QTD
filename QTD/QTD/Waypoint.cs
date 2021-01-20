using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALib;

namespace QTD
{
    public class WayPoint
    {
        public int ID; // Use this to spawn enemies at the appropriate startpoints or something.
        public Vector2 Location;
        List<WayPoint> NextWaypoints = null;
        List<int> NextWaypointIDs = new List<int>();
        public float ShortestRouteToNextWPLength;
        public float ShortestRouteToFinishLength;

        public static List<WayPoint> StartPoints;
        public static int Spread;

        private bool m_IsFinish = false;
        public bool IsFinish
        {
            get { return m_IsFinish; }
            private set { m_IsFinish = value; }
        }

        private bool m_IsStart = false;
        public bool IsStart
        {
            get { return m_IsStart; }
            private set { m_IsStart = value; }
        }

        public WayPoint(int id, Vector2 location, bool isStart, bool isFinish, params int[] nextWayPointIDs)
        {
            ID = id;
            Location = location;
            IsStart = isStart;
            IsFinish = isFinish;
            NextWaypoints = new List<WayPoint>();
            if (nextWayPointIDs != null)
                NextWaypointIDs.AddRange(nextWayPointIDs);

            if (isStart)
                StartPoints.Add(this);
        }

        #warning This function will ofcourse not work properly when there are multiple endpoints and multiple routes that can lead to 2 or more endpoints from the same startpoint.
        public static void CalculateTotalRoutelength()
        {
            List<WayPoint> endPoints = Level.Instance.WayPoints.Where(w => w.IsFinish).ToList();
            foreach (WayPoint endPoint in endPoints)
                endPoint.RecursiveRouteCalculation(null);
        }

        public void RecursiveRouteCalculation(WayPoint previousWP)
        {
            if (previousWP != null)
                ShortestRouteToFinishLength = Vector2.Distance(Location, previousWP.Location) + previousWP.ShortestRouteToFinishLength;
            List<WayPoint> connectedWPs = Level.Instance.WayPoints.Where(w => w.NextWaypoints.Contains(this)).ToList();
            foreach (WayPoint wp in connectedWPs)
                wp.RecursiveRouteCalculation(this);
        }

        /// <summary>
        /// Converts ID's to actual waypoints and calculates the length of the routes.
        /// </summary>
        public void Initialize()
        {
            // Translate ID's
            foreach (int id in NextWaypointIDs)
            {
                WayPoint nextWP = Level.Instance.WayPoints.Find(w => w.ID == id);
                if (nextWP == null)
                    throw new NullReferenceException("Could not find the next wpID for id:" + ID + " and nextwpid: " + id+". Waypoint.Initialize();");
                NextWaypoints.Add(nextWP);
            }
            NextWaypointIDs.Clear();


            if (!IsFinish)
            {
                // Get lengths
                ShortestRouteToNextWPLength = float.MaxValue;
                foreach (WayPoint wp in NextWaypoints)
                {
                    float distance = Vector2.Distance(Location,wp.Location);
                    if (distance < ShortestRouteToNextWPLength)
                        ShortestRouteToNextWPLength = distance;
                }
            }
            else
            {
                ShortestRouteToFinishLength = ShortestRouteToNextWPLength = 0;
            }
        }

        public WayPoint GetNextWP()
        {
            if (IsFinish)
                throw new Exception("WP is finish");
            else
            {
                if (NextWaypoints.Count == 1)
                    return NextWaypoints[0];
                else
                    return NextWaypoints[Maths.RandomNr(0, NextWaypoints.Count - 1)];
            }
        }

        public LinePrimitive CreateLinesForMinimap(Vector2 offset, Vector2 ratio, Color LineColor)
        {
            LinePrimitive lp = new LinePrimitive(Engine.Instance.Graphics.GraphicsDevice, LineColor);
            foreach (WayPoint wp in NextWaypoints)
            {
                lp.AddVector(offset + Location * ratio);
                lp.AddVector(offset + wp.Location * ratio);
            }
            return lp;
        }

        public void DebugDraw()
        {
            Color drawColor = Color.Red;
            if (IsStart)
                drawColor = Color.Yellow;
            else if (IsFinish)
                drawColor = Color.Purple;
            Engine.Instance.SpriteBatch.Draw(Common.White1px50Trans, new Rectangle(Location.Xi() - 2, Location.Yi() - 2, 5, 5), drawColor);
            foreach (WayPoint wp in NextWaypoints)
            {
                LinePrimitive lp = new LinePrimitive(Engine.Instance.Graphics.GraphicsDevice, Color.Blue);
                lp.AddVector(Location);
                lp.AddVector(wp.Location);
                lp.Render(Engine.Instance.SpriteBatch);
            }
        }
    }
}