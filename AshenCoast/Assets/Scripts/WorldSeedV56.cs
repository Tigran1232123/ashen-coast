using System;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public static class LanProtocolV49
    {
        public const int Build=58;
    }

    public static class SeededExteriorV56
    {
        public static void Move(Transform root,Vector2 delta)
        {
            if(delta==Vector2.zero)return;
            root.position+=(Vector3)delta;
            int order=CoastGame.Depth(delta.y)-CoastGame.Depth(0);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                // Ground and paving have deliberately fixed background layers.
                if(renderer.sortingOrder!=-20000 && renderer.sortingOrder!=-19900)
                    renderer.sortingOrder+=order;
            }
        }
    }

    public sealed partial class ProceduralMeadow
    {
        public bool AdoptSeedV56(int seed)
        {
            if(seed<1)throw new InvalidOperationException("Invalid world seed");
            if(seed==Seed)return false;
            foreach(var key in chunks.Keys.ToArray())Unload(key);
            picked.Clear();wanted.Clear();pending.Clear();retiring.Clear();
            lastCentre=new Vector2Int(int.MinValue,int.MinValue);lastRX=lastRY=-1;
            Seed=seed;WorldShape.Configure(seed);
            return true;
        }
    }

    public sealed partial class FortWorld
    {
        public void AdoptSeedV56()
        {SeededExteriorV56.Move(exteriorRootV56.transform,Position-(Vector2)exterior.transform.position);}
    }

    public sealed partial class CaveDungeon
    {
        public void AdoptSeedV56()
        {
            var point=CatacombLayout.Entrance(game.meadow.Seed);
            SeededExteriorV56.Move(entranceRootV56.transform,point-Position);Position=point;
        }
    }

    public sealed partial class VillageActorV46
    {
        public void MoveHomeV56(Vector2 delta)
        {
            home+=delta;TrainingPointV48+=delta;patrolGoal=routeGoal=Vector2.zero;
            ResetRouteV48();
        }
    }

    public sealed partial class AmazonSettlementV46
    {
        public static Vector2 CentreForWorldV56()
        {
            var point=WorldShape.NearestLand(new Vector2(-135,230),65);
            return Vector2.Distance(point,WorldShape.FortPosition)<90
                ?WorldShape.NearestLand(new Vector2(150,210),65):point;
        }
        public void AdoptSeedV56()
        {
            var point=CentreForWorldV56();var delta=point-Centre;Centre=point;
            WorldShape.AmazonPosition=point;
            if(delta==Vector2.zero)return;
            SeededExteriorV56.Move(root.transform,delta);
            foreach(var house in houses)house.entrance+=delta;
            foreach(var fire in fires)fire.point+=delta;
            for(int i=0;i<barriers.Count;i++){var b=barriers[i];b.position+=delta;barriers[i]=b;}
            for(int i=0;i<roadNodesV54.Count;i++)roadNodesV54[i]+=delta;
            foreach(var actor in people)actor.MoveHomeV56(delta);
            foreach(var tree in trees)tree.MoveHomeV56(delta);
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                if(renderer.sharedMaterial && renderer.sharedMaterial.shader.name=="AshenCoast/AmazonGroundV46")
                    renderer.sharedMaterial.SetVector("_Centre",Centre);
        }
    }

    public sealed partial class ExplorationMap
    {
        public void ResetWorldV56()
        {
            foreach(var region in regions)region?.Dispose();
            lastVisit=null;Initialize(game);Recenter();
        }
    }

    public sealed partial class CoopSessionV49
    {
        public void ReceiveWelcomeV56(LanWelcomeV49 welcome)
        {
            if(welcome==null || welcome.peer<1 || welcome.seed<1 ||
                !LanItemV49.Finite(welcome.x) || !LanItemV49.Finite(welcome.y) ||
                !LanItemV49.Finite(welcome.seconds) || !LanItemV49.Finite(welcome.networkTime) ||
                Mathf.Abs(welcome.x)>2000 || Mathf.Abs(welcome.y)>2000)
                throw new InvalidOperationException("Invalid world welcome");
            game.map.SetOpen(false);
            // The host owns generation. Replace dependent geometry before any queued snapshots.
            bool changed=game.meadow.AdoptSeedV56(welcome.seed);
            if(changed)
            {game.fort.AdoptSeedV56();game.cave.AdoptSeedV56();game.amazons.AdoptSeedV56();}
            game.amazons.ApplyLayoutSeedV54(welcome.villageSeed);game.ConfigureRiverWorldV58();
            Peer=welcome.peer;Online=true;Connecting=false;
            worldSeconds=welcome.seconds;receivedTime=Time.unscaledTime;
            networkBase=welcome.networkTime;networkReceived=Time.realtimeSinceStartup;
            Status="Подключено";MenuOpen=LocalPaused=false;game.paused=false;Time.timeScale=1;
            var spawn=new Vector2(welcome.x,welcome.y);
            game.journey.SetArea(WorldArea.Coast,spawn);game.body.position=spawn;
            game.body.linearVelocity=Vector2.zero;game.hero.transform.position=spawn;
            if(changed)game.map.ResetWorldV56();
            game.meadow.Refresh(spawn,true);Physics2D.SyncTransforms();
            players[Peer]=CapturePlayer();inventory.Joined();
        }
    }
}
