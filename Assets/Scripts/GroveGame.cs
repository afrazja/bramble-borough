using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrambleBorough
{
    // Screen-space coordinates keep mouse and touch placement identical at every aspect ratio.
    public sealed class GroveState
    {
        public readonly Vector2[] Path = { new Vector2(-30,270), new Vector2(280,270), new Vector2(280,470), new Vector2(620,470), new Vector2(620,270), new Vector2(1040,270), new Vector2(1040,470), new Vector2(1320,470) };
        public readonly Vector2[] Pads = { new Vector2(160,365), new Vector2(385,345), new Vector2(470,560), new Vector2(535,355), new Vector2(720,365), new Vector2(875,365), new Vector2(950,560), new Vector2(1140,365) };
        public readonly int[] Towers = new int[8];
        public readonly float[] Cooldowns = new float[8];
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public readonly List<Shot> Shots = new List<Shot>();
        public int Coins = 180, Health = 10, Wave, Spawned, Kills;
        public bool Running, Won, Lost;
        float spawnClock;
        public sealed class Enemy { public Vector2 Position; public int Segment = 1; public float HP, MaxHP, Speed, Slow; public bool Heavy; }
        public sealed class Shot { public Vector2 From, To; public float Life = .16f; public int Kind; }
        public static int Cost(int kind) => kind == 1 ? 60 : 85;
        public bool Place(int pad, int kind)
        {
            if (pad < 0 || pad >= Pads.Length || kind < 1 || kind > 2 || Towers[pad] != 0 || Coins < Cost(kind) || Won || Lost) return false;
            Coins -= Cost(kind); Towers[pad] = kind; return true;
        }
        public bool StartWave()
        {
            if (Running || Won || Lost || Wave >= 3) return false;
            Wave++; Spawned = 0; spawnClock = 0; Running = true; return true;
        }
        public void Tick(float dt)
        {
            for (int i = Shots.Count - 1; i >= 0; i--) { Shots[i].Life -= dt; if (Shots[i].Life <= 0) Shots.RemoveAt(i); }
            if (Won || Lost || !Running) return;
            spawnClock -= dt;
            if (Spawned < 6 + Wave * 3 && spawnClock <= 0)
            {
                bool heavy = Wave >= 2 && Spawned % 4 == 3;
                float hp = (heavy ? 110 : 46) + Wave * 12;
                Enemies.Add(new Enemy { Position = Path[0], HP = hp, MaxHP = hp, Speed = heavy ? 45 : 58 + Wave * 5, Heavy = heavy });
                Spawned++; spawnClock += Wave == 3 ? .85f : 1.15f;
            }
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemy e = Enemies[i]; e.Slow = Mathf.Max(0, e.Slow - dt);
                float move = e.Speed * (e.Slow > 0 ? .55f : 1) * dt;
                while (move > 0 && e.Segment < Path.Length)
                {
                    float distance = Vector2.Distance(e.Position, Path[e.Segment]);
                    if (distance <= move) { e.Position = Path[e.Segment++]; move -= distance; }
                    else { e.Position = Vector2.MoveTowards(e.Position, Path[e.Segment], move); move = 0; }
                }
                if (e.Segment == Path.Length)
                {
                    Health = Mathf.Max(0, Health - (e.Heavy ? 2 : 1)); Enemies.RemoveAt(i);
                    if (Health == 0) { Lost = true; Running = false; return; }
                }
            }
            for (int p = 0; p < Pads.Length; p++)
            {
                Cooldowns[p] -= dt;
                if (Towers[p] == 0 || Cooldowns[p] > 0) continue;
                Enemy target = null;
                // Prioritize actual path progress even when slow effects change the enemy order.
                float progress = -1;
                foreach (Enemy e in Enemies)
                {
                    float score = e.Segment * 2000 - Vector2.Distance(e.Position, Path[e.Segment]);
                    if (Vector2.Distance(Pads[p], e.Position) <= (Towers[p] == 1 ? 205 : 180) && score > progress) { progress = score; target = e; }
                }
                if (target == null) continue;
                Shots.Add(new Shot { From = Pads[p], To = target.Position, Kind = Towers[p] });
                target.HP -= Towers[p] == 1 ? 23 : 11;
                if (Towers[p] == 2) target.Slow = 1.8f;
                Cooldowns[p] = Towers[p] == 1 ? .68f : .85f;
                if (target.HP <= 0) { Enemies.Remove(target); Coins += target.Heavy ? 20 : 12; Kills++; }
            }
            if (Spawned == 6 + Wave * 3 && Enemies.Count == 0)
            {
                Running = false; Coins += 40;
                if (Wave == 3) Won = true;
            }
        }
    }

    public sealed class GroveGame : MonoBehaviour
    {
        [NonSerialized] public GroveState State = new GroveState();
        Texture2D circle;
        GUIStyle label, button;
        int selected = 1;
        bool paused;
        string hint = "Choose a defender, tap a mossy pad, then send the first wave.";
        public static readonly Color Ink = new Color(.10f,.20f,.19f);
        void Awake() { Application.targetFrameRate = 60; }
        void Update() { if (!paused) State.Tick(Mathf.Min(Time.deltaTime,.05f)); }
        void OnDestroy() { if (circle != null) Destroy(circle); }
        void InitGUI()
        {
            if (circle != null) return;
            circle = new Texture2D(64,64,TextureFormat.RGBA32,false);
            for (int y=0;y<64;y++) for (int x=0;x<64;x++) circle.SetPixel(x,y,new Color(1,1,1, Mathf.Clamp01(32-Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32)))));
            circle.Apply();
            label = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            button = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };
        }
        void Box(Rect r, Color c) { GUI.color=c; GUI.DrawTexture(r,Texture2D.whiteTexture); GUI.color=Color.white; }
        void Oval(float x,float y,float w,float h,Color c) { GUI.color=c; GUI.DrawTexture(new Rect(x-w/2,y-h/2,w,h),circle); GUI.color=Color.white; }
        void Text(float x,float y,float w,float h,string s,int size,Color c) { label.fontSize=size; label.normal.textColor=c; GUI.Label(new Rect(x,y,w,h),s,label); }
        bool Button(Rect r,string s,Color c) { GUI.backgroundColor=c; bool hit=GUI.Button(r,s,button); GUI.backgroundColor=Color.white; return hit; }
        void Line(Vector2 a,Vector2 b,float width,Color c)
        {
            Matrix4x4 old=GUI.matrix;
            // Compose in logical canvas space; RotateAroundPivot uses screen-space pivots
            // and shifts vertical paths when the Web canvas is scaled or letterboxed.
            GUI.matrix=old*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg),Vector3.one);
            Box(new Rect(0,-width/2,Vector2.Distance(a,b),width),c); GUI.matrix=old;
            Oval(a.x,a.y,width,width,c); Oval(b.x,b.y,width,width,c);
        }
        void Face(float x,float y,float size,Color color,bool heavy=false)
        {
            Oval(x,y+size*.44f,size*1.1f,size*.28f,new Color(0,0,0,.16f));
            Oval(x,y,size+6,size+6,Ink); Oval(x,y-2,size,size,color);
            Oval(x-size*.18f,y-5,10,13,Color.white); Oval(x+size*.18f,y-5,10,13,Color.white);
            Oval(x-size*.16f,y-4,4,7,Ink); Oval(x+size*.20f,y-4,4,7,Ink);
            Oval(x,y+12,12,5,Ink);
            if (heavy) { Box(new Rect(x-size*.45f,y-size*.52f,size*.9f,13),Ink); Box(new Rect(x-size*.40f,y-size*.58f,size*.8f,10),new Color(.65f,.57f,.76f)); }
        }
        void OnGUI()
        {
            InitGUI();
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
            Box(new Rect(0,0,1280,720),new Color(.29f,.49f,.36f));
            for(int i=0;i<42;i++) { float x=(i*193+39)%1280, y=175+(i*137)%430; Oval(x,y,75,35,new Color(.34f,.54f,.38f)); }
            for(int i=1;i<State.Path.Length;i++) Line(State.Path[i-1],State.Path[i],76,new Color(.28f,.35f,.24f));
            for(int i=1;i<State.Path.Length;i++) Line(State.Path[i-1],State.Path[i],62,new Color(.79f,.68f,.44f));
            for(int i=0;i<30;i++) Oval((i*173+65)%1280,175+(i*83)%420,5,5,new Color(.96f,.81f,.48f));
            // The lantern grove is the endpoint, separate from buildable pads.
            Oval(1220,437,70,24,Ink); Box(new Rect(1194,375,52,60),new Color(.38f,.25f,.19f));
            Oval(1220,371,94,42,new Color(.95f,.59f,.27f)); Oval(1220,402,23,28,new Color(1,.88f,.47f));
            for(int i=0;i<State.Pads.Length;i++)
            {
                Vector2 p=State.Pads[i]; Oval(p.x,p.y+16,91,48,Ink); Oval(p.x,p.y+9,83,47,new Color(.58f,.69f,.39f));
                if(State.Towers[i]==0)
                {
                    Text(p.x-9,p.y-20,40,40,"+",30,new Color(.88f,.93f,.66f));
                    GUI.enabled=!paused&&!State.Won&&!State.Lost;
                    if(GUI.Button(new Rect(p.x-44,p.y-35,88,75),GUIContent.none,GUIStyle.none))
                        hint=State.Place(i,selected)? "Defender ready! Save coins for the next wave." : "Not enough amber. Defeat dustlings to earn more.";
                    GUI.enabled=true;
                }
                else
                {
                    bool frost=State.Towers[i]==2;
                    Face(p.x,p.y-11,49,frost?new Color(.50f,.81f,.85f):new Color(.95f,.65f,.32f));
                    Oval(p.x-18,p.y-40,28,15,frost?new Color(.69f,.91f,.96f):new Color(.39f,.66f,.36f));
                    Oval(p.x+6,p.y-43,19,24,frost?new Color(.69f,.91f,.96f):new Color(.39f,.66f,.36f));
                }
            }
            foreach(var e in State.Enemies)
            {
                float bob=Mathf.Sin(Time.unscaledTime*9+e.Position.x)*3;
                Face(e.Position.x,e.Position.y-13+bob,e.Heavy?49:38,e.Slow>0?new Color(.53f,.75f,.88f):new Color(.67f,.53f,.71f),e.Heavy);
                Box(new Rect(e.Position.x-23,e.Position.y-49,46,6),Ink); Box(new Rect(e.Position.x-22,e.Position.y-48,44*e.HP/e.MaxHP,4),new Color(.85f,.94f,.48f));
            }
            foreach(var shot in State.Shots) { Line(shot.From+Vector2.up*10,shot.To,4,shot.Kind==1?new Color(1,.87f,.39f):new Color(.68f,.95f,1)); }
            Box(new Rect(0,0,1280,130),Ink);
            Text(30,12,600,47,"BRAMBLE BOROUGH",34,new Color(1,.89f,.61f));
            Text(32,65,620,36,"01  /  Lantern Grove     •     Protect the little light",19,new Color(.69f,.83f,.73f));
            Text(720,23,200,45,"Amber  "+State.Coins,25,new Color(1,.82f,.35f));
            Text(934,23,150,45,"Lives  "+State.Health,25,new Color(1,.66f,.58f));
            Text(1100,23,170,45,"Wave "+State.Wave+" / 3",23,Color.white);
            Text(722,76,500,30,paused?"PAUSED":State.Running?"Dustlings incoming!":"Take your time. Build your defenses.",18,new Color(.69f,.83f,.73f));
            Box(new Rect(0,615,1280,105),Ink);
            if(Button(new Rect(22,632,255,57),(selected==1?"● ":"")+"Acorn Scout · 60",new Color(.95f,.64f,.30f))) selected=1;
            if(Button(new Rect(289,632,255,57),(selected==2?"● ":"")+"Dew Keeper · 85",new Color(.45f,.78f,.85f))) selected=2;
            GUI.enabled=!State.Running&&!State.Won&&!State.Lost&&!paused;
            if(Button(new Rect(562,632,240,57),"Send wave "+Mathf.Min(3,State.Wave+1),new Color(.61f,.80f,.39f))) State.StartWave();
            GUI.enabled=!State.Won&&!State.Lost;
            if(Button(new Rect(820,632,125,57),paused?"Resume":"Pause",Color.white)) paused=!paused;
            GUI.enabled=true;
            if(Button(new Rect(961,632,127,57),"Restart",Color.white)) ResetGame();
            Text(1106,631,164,60,selected==1?"SCOUT\nFast damage":"KEEPER\nSlows enemies",16,new Color(.76f,.85f,.76f));
            Text(27,580,1230,30,hint,18,new Color(.96f,.94f,.77f));
            if(State.Won||State.Lost)
            {
                Box(new Rect(0,130,1280,485),new Color(.04f,.11f,.10f,.88f));
                Text(330,240,670,65,State.Won?"THE GROVE GLOWS ON!":"THE LIGHT WENT OUT",37,new Color(1,.86f,.48f));
                Text(330,318,660,50,"Defeated "+State.Kills+" dustlings  •  "+State.Health+" lives remaining",23,Color.white);
                if(Button(new Rect(440,405,400,65),"Play again",new Color(.72f,.87f,.48f))) ResetGame();
            }
        }
        public void ResetGame() { State=new GroveState(); paused=false; selected=1; hint="Choose a defender, tap a mossy pad, then send the first wave."; }
    }
}
