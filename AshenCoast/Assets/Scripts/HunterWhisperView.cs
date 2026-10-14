using UnityEngine;
namespace AshenCoast
{
    public sealed class HunterWhisperView : MonoBehaviour
    {
        NightHunter hunter;
        CoastGame game;
        Font font;GUIStyle whisperStyle;Texture2D sparkle;
        readonly Vector3[] quoteMotes=new Vector3[144];
        readonly GUIContent[] glyphs=new GUIContent[NightHunter.Whisper.Length];
        readonly float[] widths=new float[NightHunter.Whisper.Length];
        float totalWidth;
        public void Initialize(NightHunter owner)
        {
            hunter=owner;game=owner.game;var random=new System.Random(17131);
            for(int i=0;i<quoteMotes.Length;i++)quoteMotes[i]=new Vector3((float)random.NextDouble(),(float)random.NextDouble(),(float)random.NextDouble()*6.28f);
        }
        void CacheGlyphs()
        {
            totalWidth=0;
            for(int i=0;i<glyphs.Length;i++){glyphs[i]=new GUIContent(NightHunter.Whisper[i].ToString());widths[i]=whisperStyle.CalcSize(glyphs[i]).x;totalWidth+=widths[i];}
        }
        void OnGUI()=>Draw();
        public void Draw()
        {
            if(game==null || !hunter.QuoteVisible || game.map.Open || Event.current.type!=EventType.Repaint)return;
            if(whisperStyle==null)
            {
                font=Font.CreateDynamicFontFromOSFont(new[]{"Cambria","Times New Roman","Georgia"},20);
                whisperStyle=new GUIStyle{font=font,fontSize=20,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};
                sparkle=new Texture2D(9,9,TextureFormat.RGBA32,false){filterMode=FilterMode.Point};
                for(int py=0;py<9;py++)for(int px=0;px<9;px++)
                {float d=Mathf.Abs(px-4)+Mathf.Abs(py-4);sparkle.SetPixel(px,py,new Color(1,1,1,Mathf.Clamp01(1-d/5)));}
                sparkle.Apply();
                CacheGlyphs();
            }
            GUI.depth=-5;var matrix=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(game.ui.UIScale*2,game.ui.UIScale*2,1));
            var viewport=game.cameraView.WorldToViewportPoint(hunter.QuoteAnchor);var p=new Vector2(viewport.x*game.ui.CanvasWidth*.5f,(1-viewport.y)*game.ui.CanvasHeight*.5f);
            float width=totalWidth;
            float x=p.x-width/2;
            float fade=Mathf.SmoothStep(0,1,Mathf.Clamp01(hunter.QuoteAge/.18f))*Mathf.Clamp01((NightHunter.QuoteDuration-hunter.QuoteAge)/.5f);
            for(int i=0;i<NightHunter.Whisper.Length;i++)
            {
                GUIContent letter=glyphs[i];float lw=widths[i];
                font.RequestCharactersInTexture(NightHunter.Whisper,20,FontStyle.Bold);if(font.material.mainTexture)font.material.mainTexture.filterMode=FilterMode.Point;
                var rect=new Rect(Mathf.Round(x),Mathf.Round(p.y-13+Mathf.Sin(i*2.7f)*.6f),lw+6,27);
                whisperStyle.normal.textColor=new Color(.016f,.007f,.020f,fade*.25f);
                for(int j=0;j<4;j++){var d=new Vector2(Mathf.Cos(j*Mathf.PI/2),Mathf.Sin(j*Mathf.PI/2))*3;GUI.Label(new Rect(rect.position+d,rect.size),letter,whisperStyle);}
                whisperStyle.normal.textColor=new Color(.025f,.005f,.045f,fade*.96f);
                for(int j=0;j<4;j++){var d=new Vector2(Mathf.Cos(j*Mathf.PI/2),Mathf.Sin(j*Mathf.PI/2));GUI.Label(new Rect(rect.position+d,rect.size),letter,whisperStyle);}
                whisperStyle.normal.textColor=NightHunter.WhisperInk(hunter.QuoteAge,i,fade);
                GUI.Label(rect,letter,whisperStyle);x+=lw;
            }
            for(int i=0;i<quoteMotes.Length;i++)
            {
                float age=hunter.QuoteAge-i*.025f;var mote=quoteMotes[i];float life=.75f+mote.y*.95f;
                if(age<0 || age>life)continue;
                float opacity=Mathf.Sin(age/life*Mathf.PI)*fade;
                var at=p+new Vector2((mote.x-.5f)*width+Mathf.Sin(mote.z+age*3)*age*15+(mote.x-.5f)*age*23,5-age*(28+mote.y*47));
                GUI.color=new Color(.025f,.005f,.036f,opacity*.30f);GUI.DrawTexture(new Rect(Mathf.Round(at.x)-1,Mathf.Round(at.y)-1,2,2),Texture2D.whiteTexture);
                GUI.color=NightHunter.WhisperInk(age*2,i,opacity);
                float size=2+mote.y*2;GUI.DrawTexture(new Rect(Mathf.Round(at.x),Mathf.Round(at.y),1+i%2,1),Texture2D.whiteTexture);
            }
            GUI.color=new Color(.070f,.018f,.086f,fade*.65f);
            for(int side=-1;side<=1;side+=2)
            {float ornament=p.x+side*(width/2+18);GUI.DrawTexture(new Rect(ornament-3,p.y-3,6,6),sparkle);GUI.DrawTexture(new Rect(ornament+side*7-9,p.y,18,1),Texture2D.whiteTexture);}
            GUI.color=Color.white;GUI.matrix=matrix;
        }
        void OnDestroy(){if(font)Destroy(font);if(sparkle)Destroy(sparkle);}
    }
}
