using System;
using Colossal.Collections;
using Colossal.Mathematics;
using Game.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace TerraformHardening
{
    partial class MySearchSystem : SystemBase
    {
        // static public SearchInSquare(Bounds2 square)
        // {
        //     World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Net.SearchSystem>();
        // }
        private Game.Net.SearchSystem m_SearchSystem;

        public MySearchSystem()
        {
            m_SearchSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Net.SearchSystem>();
        }

        public bool HasRoads(Bounds2 rect)
        {
            var tree = m_SearchSystem.GetNetSearchTree(true, out JobHandle netSearchTreeHandle);
            netSearchTreeHandle.Complete();

            SearchIterator iterator = default;
            iterator.m_SearchRect = rect;
            tree.Iterate(ref iterator);
            foreach (var item in iterator.m_EntityList)
            {
                Debug.Log(item);
            }
            iterator.Dispose();

            // tree.Select()

            return false;
        }


        protected override void OnUpdate()
        {

        }
    }


    // This looks more like normal iterator on top of NativeQuadTree, and it
    // doesn't seem to utilize the tree itself?
    struct SearchIterator : INativeQuadTreeIterator<Entity, QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, QuadTreeBoundsXZ>, IDisposable
    {
        public Bounds2 m_SearchRect;
        public NativeList<Entity> m_EntityList;

        public void Dispose()
        {
            m_EntityList.Dispose();
        }

        public bool Intersect(QuadTreeBoundsXZ bounds)
        {
            return MathUtils.Intersect(bounds.m_Bounds.xz, m_SearchRect);
        }

        public void Iterate(QuadTreeBoundsXZ bounds, Entity item)
        {
            if (MathUtils.Intersect(m_SearchRect, bounds.m_Bounds.xz))
            {
                m_EntityList.Add(item);
                Mod.log.Debug("Found entity: " + item.Index);
            }
        }
    }

    // Would Selector be better?
    struct Selector :
        INativeQuadTreeSelector<Entity, QuadTreeBoundsXZ, float>,
        IUnsafeQuadTreeSelector<Entity, QuadTreeBoundsXZ, float>
    {
        public bool Better(float priority1, float priority2)
        {
            throw new NotImplementedException();
        }

        public bool Check(QuadTreeBoundsXZ bounds, out float priority)
        {
            throw new NotImplementedException();
        }

        public bool Check(float priority)
        {
            throw new NotImplementedException();
        }

        public void Select(QuadTreeBoundsXZ bounds, Entity item)
        {
            throw new NotImplementedException();
        }
    }
}