using System.Collections;
using System.Linq;
using UnityEngine;

namespace AshenCoast
{
    public sealed partial class RuntimeChecks
    {
        IEnumerator FireMeditationChecks()
        {
            var g=game;var ritual=g.dragons;var s=g.survivor;var m=g.meditation;
            g.ui.CloseContainers();g.journey.SetArea(WorldArea.Coast,new Vector2(18,12));g.seconds=40;s.health=100;s.mana=100;
            g.combat.manual=true;g.checkCameraPosition=new Vector3(18,13,-20);g.checkCameraSize=6;g.ResetCamera(g.checkCameraPosition.Value);
            ritual.Clear();var boss=ritual.Spawn(new Vector2(12,12),g.coastRoot.transform);
            boss.BeginAttack(DragonAction.Breath,Vector2.right);boss.Tick(.58f);
            Check(g.arcane.flames.Any(f=>f.dragonFire),"dragon creates actual traveling fire tongues");
            Check(ritual.scorches.Count==0 && s.health==100,"breath cannot preburn or hurt a player before fire arrives");
            g.arcane.Tick(.03f);Check(ritual.scorches.Count==0,"very new flames do not blacken distant ground");
            g.arcane.Tick(.23f);
            Check(ritual.scorches.Count>0 && ritual.scorches.Max(p=>p.cell.x)<19,"scorching advances with visible fire instead of filling the entire cone");
            g.UpdateEnvironment(0);yield return Capture("v27-propagating-fire.png");
            var fire=g.arcane.EffectSprite("Contact-only fire check",g.art.flame[4],g.body.position+Vector2.right*2,0);
            fire.transform.localScale=Vector3.one*(1.4f/fire.sprite.bounds.size.y);g.art.PixelStyle(fire);
            float hp=s.health;ritual.clock+=1;ritual.HurtByFireContact(fire,12);
            Check(s.health==hp,"nearby fire without visible overlap deals no damage");
            bool touched=false;
            for(int x=-2;x<=2 && !touched;x++)for(int y=1;y<=5 && !touched;y++)
            {
                fire.transform.position=g.body.position+new Vector2(x*.15f,y*.20f);
                if(m.FireContact(fire)){ritual.HurtByFireContact(fire,12);touched=true;}
            }
            Check(touched && s.health==hp-12,"opaque fire contact applies damage to the rendered player");
            Destroy(fire.gameObject);ritual.Clear();foreach(var f in g.arcane.flames)Destroy(f.renderer.gameObject);g.arcane.flames.Clear();
            g.body.position=new Vector2(36,20);g.hero.transform.position=g.body.position;s.health=100;
            boss=ritual.Spawn(new Vector2(12,12),g.coastRoot.transform);boss.BeginAttack(DragonAction.Flight,Vector2.right);
            for(int i=0;i<100;i++)ritual.Tick(.02f);
            Check(ritual.falling.Any(f=>!f.landed) && ritual.GroundImpacts>0,"flight produces a moving jet and actual ground impacts");
            Check(ritual.falling.Where(f=>!f.landed).All(f=>(f.start-boss.Mouth).magnitude<.001f && (f.ground-boss.FlightImpact).magnitude<.001f),"all airborne flames keep up with the current beak, even on fast flybys");
            Check(Vector2.Dot(boss.FlightImpact-boss.Mouth,boss.FlightDirection)>3.5f,"airborne fire lands ahead of the mouth after allowing for height");
            g.checkCameraPosition=(Vector3)boss.Position+new Vector3(3,3,-20);g.ResetCamera(g.checkCameraPosition.Value);g.UpdateEnvironment(0);yield return Capture("v27-attached-forward-flight-fire.png");
            var airborne=ritual.falling.First(f=>!f.landed);boss.Warp(boss.Position+Vector2.right*4);ritual.Tick(.01f);
            Check((airborne.start-boss.Mouth).magnitude<.001f,"existing airborne fire follows a sudden bird movement instead of trailing");
            Check(g.soundscape.battle.clip && g.soundscape.battle.clip.channels==2 && g.soundscape.battle.clip.length>45,"original stereo violin battle music is included");
            g.body.position=boss.Position-Vector2.right*7;g.hero.transform.position=g.body.position;s.health=100;
            g.soundscape.Tick(2);Check(g.soundscape.battle.isPlaying && g.soundscape.battle.volume>.4f && g.soundscape.cello.volume==0 && g.soundscape.cosmos.volume==0,"battle violin replaces location music while the nearby boss is alive");
            boss.Hurt(2000);g.soundscape.Tick(2);Check(!g.soundscape.battle.isPlaying && g.soundscape.cello.volume>0,"location music returns after the boss dies");ritual.Clear();
            g.body.position=new Vector2(18,12);g.hero.transform.position=g.body.position;g.checkCameraPosition=new Vector3(18,13,-20);g.ResetCamera(g.checkCameraPosition.Value);g.checkMovement=Vector2.zero;s.health=19;
            Check(!m.Toggle() && !m.Active,"meditation cannot begin below twenty health");
            s.health=20;Check(m.Toggle() && m.Active && m.DaySpeed==1,"Q meditation can begin at exactly twenty health at normal day speed");
            float realClock=g.seconds;float arcaneClock=g.arcane.clock;float bossClock=ritual.clock;m.Tick(2,false);
            Check(m.DaySpeed>1 && m.DaySpeed<5,"day and night acceleration increases gradually");m.Tick(2,false);
            Check(Mathf.Abs(m.DaySpeed-5)<.001f && Mathf.Abs(Time.timeScale-5)<.001f,"meditation reaches five times world speed");
            Check(g.DaySeconds==g.seconds && m.Blend==1,"day, weather and combat share the accelerated world clock");
            g.numbers.Clear();g.UpdateEnvironment(0);yield return Capture("v27-meditation-seated-edge-blur.png");
            Check(!g.hero.enabled && m.VisibleTorso.enabled && m.VisibleTorso.bounds.max.y<g.hero.bounds.max.y-.35f,"hero visibly sits with lowered torso and crossed legs");
            Check(m.Toggle() && !m.Active && m.DaySpeed==1,"a second Q exits meditation immediately");m.Tick(.3f,false);yield return new WaitForEndOfFrame();Check(g.hero.enabled,"standing pose returns after meditation");
            s.health=100;m.Toggle();m.Tick(.3f,true);Check(!m.Active,"movement interrupts meditation");
            m.Toggle();g.HurtPlayer(81);Check(!m.Active && !m.Toggle(),"damage below twenty HP interrupts and prevents meditation");
            s.health=20;Check(m.Toggle(),"meditation becomes available again when health reaches twenty");m.Stop();m.Tick(.5f,false);
            g.checkMovement=null;g.checkCameraPosition=null;g.checkCameraSize=null;s.health=100;s.mana=100;g.combat.manual=false;g.numbers.Clear();
        }
    }
}
