using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALib;
using System;
using System.Collections.Generic;

namespace QTD
{
    public class BroadPhase
    {
        public static BroadPhase Instance;

        /// <summary>
        /// Do not assign to from another class. It's not implemented as a member because of the cpu usage increase that it causes.
        /// </summary>
        public int GridSize;

        private Point m_GridCnt;
        public Point GridCnt
        {
            get { return m_GridCnt; }
            private set { m_GridCnt = value; }
        }
        public Block[,] Blocks;
        
        // Offset
        public readonly Point Location = new Point(0, 0);
        readonly Point LocationIdx = new Point(0, 0);

        public BroadPhase(int gridSize, Point mapSize)
        {
            GridSize = gridSize;
            GridCnt = new Point((int)Math.Ceiling(mapSize.X / (double)GridSize),
                                (int)Math.Ceiling(mapSize.Y / (double)GridSize));
            Blocks = new Block[GridCnt.X, GridCnt.Y];
        }

        public void Init()
        {
            for (int y = 0; y < GridCnt.Y; y++)
            {
                for (int x = 0; x < GridCnt.X; x++)
                {
                    Blocks[x, y] = new Block(new Point(x, y));
                }
            }
        }

        /* // outcommented because its not used.
        public Point LocationToGridIdx(Vector2 location)
        {
            return new Point(((int)location.X / GridSize) - LocationIdx.X, ((int)location.Y / GridSize) - LocationIdx.Y);
        }*/
        public IEntity GetFirstEntityInRange(IEntity caller, int range, Predicate<IEntity> filter)
        {
            Point idx = new Point(((int)caller.AABB.X / GridSize) - LocationIdx.X, ((int)caller.AABB.Y / GridSize) - LocationIdx.Y);
            int gridsToCheck = range / GridSize;
            Vector2 callerLoc = caller.AABB.Location.ToVector2();

            for (int y = idx.Y - gridsToCheck; y < idx.Y + gridsToCheck; y++)
            {
                for (int x = idx.Y - gridsToCheck; x < idx.Y + gridsToCheck; x++)
                {
                    if (x >= 0 && y >= 0 && x < GridCnt.X && y < GridCnt.Y)
                    {
                        foreach (IEntity entity in Blocks[x, y].Entities)
                        {
                            if(Maths.DistanceBetween(caller.AABB.Location,entity.AABB.Location) <= range)
                                return entity;
                        }
                    }
                }
            }

            return null;
        }

        public List<IEntity> GetAllEntitiesInRange(IEntity caller, int range, Predicate<IEntity> filter)
        {
            Point idx = new Point(((int)caller.FeetLoc.X / GridSize) - LocationIdx.X, ((int)caller.FeetLoc.Y / GridSize) - LocationIdx.Y);
            int gridsToCheck = (int)Math.Ceiling(range / (float)GridSize);
            Vector2 callerLoc = caller.AABB.Location.ToVector2();
            List<IEntity> result = new List<IEntity>();

            for (int y = idx.Y - gridsToCheck; y < idx.Y + gridsToCheck; y++)
            {
                for (int x = idx.X - gridsToCheck; x < idx.X + gridsToCheck; x++)
                {
                    if (x >= 0 && y >= 0 && x < GridCnt.X && y < GridCnt.Y)
                    {
                        foreach (IEntity entity in Blocks[x, y].Entities)
                        {
                            if (filter(entity) && Vector2.Distance(caller.FeetLoc, entity.FeetLoc) <= range && !result.Contains(entity))
                                result.Add( entity);
                        }
                    }
                }
            }

            return result;
        }

        public void RemoveEntity(IEntity entity)
        {
            Point start, end;
            start = new Point(((int)entity.AABB.X / GridSize) - LocationIdx.X, ((int)entity.AABB.Y / GridSize) - LocationIdx.Y);
            end = new Point((entity.AABB.Right / GridSize) - LocationIdx.X, (entity.AABB.Bottom / GridSize) - LocationIdx.Y);

            for (int y = start.Y; y <= end.Y; y++)
            {
                for (int x = start.X; x <= end.X; x++)
                    Blocks[x, y].Entities.Remove(entity);
            }
        }

        public void AddEntity(IEntity entity)
        {
            Point start, end;
            start = new Point(((int)entity.AABB.X / GridSize) - LocationIdx.X, ((int)entity.AABB.Y / GridSize) - LocationIdx.Y);
            end = new Point((entity.AABB.Right / GridSize) - LocationIdx.X, (entity.AABB.Bottom / GridSize) - LocationIdx.Y);

            for (int y = start.Y; y <= end.Y; y++)
            {
                for (int x = start.X; x <= end.X; x++)
                    Blocks[x, y].Entities.Add(entity);
            }
        }

        public void ClearEntities()
        {
            for (int y = 0; y < GridCnt.Y; y++)
            {
                for (int x = 0; x < GridCnt.X; x++)
                    Blocks[x, y].ClearExceptTowers();
            }
        }

        public void DebugDraw()
        {
            for (int y = 0; y < GridCnt.Y; y++)
            {
                for (int x = 0; x < GridCnt.X; x++)
                    Blocks[x, y].DebugDraw();
            }
        }
    }
}
