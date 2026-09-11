using System;
using System.Collections.Generic;
using System.Reflection;
using IFix;
using IFix.Core;
using UnityEngine;
using COW;
using COW.GamePlay;

// ============================================================
//  ESP BOX + AIMBOT  (ILFix patch)
//  Patch method lives in GameSettingData_patch.cs
//  New classes below are marked [IFix.Interpret]
// ============================================================

namespace Haxx
{
    // ---------------- CONFIG ----------------
    [IFix.Interpret]
    public static class Cfg
    {
        public static bool ESPBox   = true;
        public static bool ESPLine  = true;
        public static bool ESPName  = true;
        public static bool ESPDist  = true;
        public static bool Aimbot   = true;
        public static float AimFov   = 160f;
        public static float AimDist  = 250f;
        public static float AimSmooth = 10f;

        public static void Load()
        {
            ESPBox    = PlayerPrefs.GetInt("esp_box", 1) == 1;
            ESPLine   = PlayerPrefs.GetInt("esp_line", 1) == 1;
            ESPName   = PlayerPrefs.GetInt("esp_name", 1) == 1;
            ESPDist   = PlayerPrefs.GetInt("esp_dist", 1) == 1;
            Aimbot    = PlayerPrefs.GetInt("aim_on", 1) == 1;
            AimFov    = PlayerPrefs.GetFloat("aim_fov", 160f);
            AimDist   = PlayerPrefs.GetFloat("aim_dist", 250f);
            AimSmooth = PlayerPrefs.GetFloat("aim_sm", 10f);
        }

        static void Save()
        {
            PlayerPrefs.SetInt("esp_box",  ESPBox  ? 1 : 0);
            PlayerPrefs.SetInt("esp_line", ESPLine ? 1 : 0);
            PlayerPrefs.SetInt("esp_name", ESPName ? 1 : 0);
            PlayerPrefs.SetInt("esp_dist", ESPDist ? 1 : 0);
            PlayerPrefs.SetInt("aim_on",   Aimbot  ? 1 : 0);
            PlayerPrefs.SetFloat("aim_fov",  AimFov);
            PlayerPrefs.SetFloat("aim_dist", AimDist);
            PlayerPrefs.SetFloat("aim_sm",   AimSmooth);
            PlayerPrefs.Save();
        }

        public static void Toggle(string key)
        {
            switch (key)
            {
                case "box":  ESPBox  = !ESPBox;  break;
                case "line": ESPLine = !ESPLine; break;
                case "name": ESPName = !ESPName; break;
                case "dist": ESPDist = !ESPDist; break;
                case "aim":  Aimbot  = !Aimbot;  break;
            }
            Save();
        }
    }

    // ---------------- DRIVER ----------------
    [IFix.Interpret]
    public class ESPDriver : MonoBehaviour
    {
        static ESPDriver inst;
        public static void Init()
        {
            if (inst != null) return;
            GameObject go = new GameObject("sys_drv");
            DontDestroyOnLoad(go);
            inst = go.AddComponent<ESPDriver>();
            Cfg.Load();
        }

        Material mat;
        readonly List<LineRenderer> lines = new List<LineRenderer>();
        readonly List<TextMesh> texts = new List<TextMesh>();
        readonly List<Player> players = new List<Player>();
        Player local;
        PropertyInfo lockProp;
        float scanT;
        bool menu;

        Material LineMat
        {
            get
            {
                if (mat == null)
                {
                    Shader sh = Shader.Find("Hidden/Internal-Colored");
                    mat = new Material(sh);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.SetInt("_ZTest", 8);
                    mat.renderQueue = 4000;
                }
                return mat;
            }
        }

        void Update()
        {
            try { Tick(); }
            catch (Exception) { }
        }

        void Tick()
        {
            scanT -= Time.deltaTime;
            if (scanT <= 0f) { scanT = 0.25f; Scan(); }

            local = GameFacade.CurrentLocalPlayer;
            DrawESP();
            if (Cfg.Aimbot) DoAim();
        }

        void Scan()
        {
            players.Clear();
            Player[] arr = FindObjectsOfType<Player>();
            for (int i = 0; i < arr.Length; i++)
            {
                Player p = arr[i];
                if (p == null || p.IsLocalPlayer || p.IsLocalTeammate) continue;
                players.Add(p);
            }
        }

        void DrawESP()
        {
            Camera cam = Camera.main;
            int li = 0, ti = 0;

            if (cam == null) { HideAll(); return; }

            for (int i = 0; i < players.Count; i++)
            {
                Player p = players[i];
                if (p == null) continue;

                Transform root = p.RootTransform != null ? p.RootTransform : p.transform;
                Transform head = p.GetHeadTF();
                if (root == null || head == null) continue;

                Vector3 feet = root.position;
                Vector3 headPos = head.position;

                if (Vector3.Dot(cam.transform.forward, feet - cam.transform.position) <= 0f) continue;

                bool knocked = p.CurHP <= 0f;
                Color col = knocked ? Color.red : Color.white;

                float h = Mathf.Abs(headPos.y - feet.y) + 0.25f;
                float w = h * 0.45f;

                if (Cfg.ESPBox)
                {
                    LineRenderer lr = GetLine(li++);
                    lr.enabled = true;
                    lr.startColor = col; lr.endColor = col;
                    lr.widthMultiplier = 0.03f;
                    DrawBox(lr, feet, w, h);
                }

                if (Cfg.ESPName || Cfg.ESPDist)
                {
                    TextMesh tm = GetText(ti++);
                    tm.gameObject.SetActive(true);
                    tm.transform.position = headPos + Vector3.up * 0.35f;
                    tm.transform.rotation = Quaternion.LookRotation(tm.transform.position - cam.transform.position);
                    tm.color = col;
                    float d = Vector3.Distance(cam.transform.position, feet);
                    tm.text = Cfg.ESPName
                        ? (Cfg.ESPDist ? (int)d + "m" : "ENEMY")
                        : (int)d + "m";
                }
            }

            HideRest(li, ti);
        }

        static readonly Vector3[] c = new Vector3[8];
        void DrawBox(LineRenderer lr, Vector3 feet, float w, float h)
        {
            float hw = w * 0.5f;
            c[0] = feet + new Vector3(-hw, 0, -hw);
            c[1] = feet + new Vector3( hw, 0, -hw);
            c[2] = feet + new Vector3( hw, 0,  hw);
            c[3] = feet + new Vector3(-hw, 0,  hw);
            c[4] = c[0] + Vector3.up * h;
            c[5] = c[1] + Vector3.up * h;
            c[6] = c[2] + Vector3.up * h;
            c[7] = c[3] + Vector3.up * h;

            lr.positionCount = 24;
            int k = 0;
            lr.SetPosition(k++, c[0]); lr.SetPosition(k++, c[1]);
            lr.SetPosition(k++, c[1]); lr.SetPosition(k++, c[2]);
            lr.SetPosition(k++, c[2]); lr.SetPosition(k++, c[3]);
            lr.SetPosition(k++, c[3]); lr.SetPosition(k++, c[0]);
            lr.SetPosition(k++, c[4]); lr.SetPosition(k++, c[5]);
            lr.SetPosition(k++, c[5]); lr.SetPosition(k++, c[6]);
            lr.SetPosition(k++, c[6]); lr.SetPosition(k++, c[7]);
            lr.SetPosition(k++, c[7]); lr.SetPosition(k++, c[4]);
            lr.SetPosition(k++, c[0]); lr.SetPosition(k++, c[4]);
            lr.SetPosition(k++, c[1]); lr.SetPosition(k++, c[5]);
            lr.SetPosition(k++, c[2]); lr.SetPosition(k++, c[6]);
            lr.SetPosition(k++, c[3]); lr.SetPosition(k++, c[7]);
        }

        LineRenderer GetLine(int i)
        {
            while (lines.Count <= i)
            {
                GameObject go = new GameObject("esp_l" + lines.Count);
                go.transform.SetParent(transform);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.material = LineMat;
                lr.useWorldSpace = true;
                lr.alignment = LineAlignment.View;
                lr.numCornerVertices = 2;
                lr.numCapVertices = 2;
                lr.receiveShadows = false;
                lines.Add(lr);
            }
            return lines[i];
        }

        TextMesh GetText(int i)
        {
            while (texts.Count <= i)
            {
                GameObject go = new GameObject("esp_t" + texts.Count);
                go.transform.SetParent(transform);
                TextMesh tm = go.AddComponent<TextMesh>();
                tm.fontSize = 90;
                tm.characterSize = 0.08f;
                tm.anchor = TextAnchor.LowerCenter;
                texts.Add(tm);
            }
            return texts[i];
        }

        void HideRest(int li, int ti)
        {
            for (int i = li; i < lines.Count; i++) lines[i].enabled = false;
            for (int i = ti; i < texts.Count; i++) texts[i].gameObject.SetActive(false);
        }

        void HideAll() { HideRest(0, 0); }

        void DoAim()
        {
            Camera cam = Camera.main;
            if (cam == null || local == null) return;

            Player best = null;
            float bestAng = Cfg.AimFov * 0.5f;

            for (int i = 0; i < players.Count; i++)
            {
                Player p = players[i];
                if (p == null || p.CurHP <= 0f) continue;
                Transform head = p.GetHeadTF();
                if (head == null) continue;

                Vector3 dir = head.position - cam.transform.position;
                if (dir.magnitude > Cfg.AimDist) continue;

                float ang = Vector3.Angle(cam.transform.forward, dir);
                if (ang < bestAng) { bestAng = ang; best = p; }
            }

            if (best == null) return;

            TryLock(best);

            Transform ht = best.GetHeadTF();
            Quaternion want = Quaternion.LookRotation(ht.position - cam.transform.position);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, want, Time.deltaTime * Cfg.AimSmooth);
        }

        void TryLock(Player target)
        {
            if (lockProp == null)
            {
                foreach (PropertyInfo pi in typeof(Player).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    if (pi.Name == "LockedAimingCollider") { lockProp = pi; break; }
                if (lockProp == null)
                    foreach (PropertyInfo pi in typeof(GameFacade).GetProperties(BindingFlags.Public | BindingFlags.Static))
                        if (pi.Name == "LockedAimingCollider") { lockProp = pi; break; }
            }
            if (lockProp == null) return;
            try
            {
                object owner = lockProp.GetGetMethod(true) != null && lockProp.GetGetMethod().IsStatic ? null : local;
                lockProp.SetValue(owner, target.HeadCollider, null);
            }
            catch (Exception) { }
        }

        Rect mr = new Rect(20, 120, 260, 300);
        void OnGUI()
        {
            GUI.backgroundColor = new Color(0, 0, 0, 0.75f);
            if (GUI.Button(new Rect(20, 80, 120, 34), menu ? "CLOSE" : "MOD MENU"))
                menu = !menu;
            if (!menu) return;

            mr = GUI.Window(9999, mr, Win, "ESP + AIMBOT");
        }

        void Win(int id)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("ESP BOX : " + OnOff(Cfg.ESPBox)))  Cfg.Toggle("box");
            if (GUILayout.Button("ESP LINE: " + OnOff(Cfg.ESPLine))) Cfg.Toggle("line");
            if (GUILayout.Button("ESP NAME: " + OnOff(Cfg.ESPName))) Cfg.Toggle("name");
            if (GUILayout.Button("ESP DIST: " + OnOff(Cfg.ESPDist))) Cfg.Toggle("dist");
            if (GUILayout.Button("AIMBOT  : " + OnOff(Cfg.Aimbot)))  Cfg.Toggle("aim");

            GUILayout.Space(8);
            GUILayout.Label("FOV: " + (int)Cfg.AimFov);
            Cfg.AimFov = GUILayout.HorizontalSlider(Cfg.AimFov, 30f, 360f);
            GUILayout.Label("DIST: " + (int)Cfg.AimDist);
            Cfg.AimDist = GUILayout.HorizontalSlider(Cfg.AimDist, 50f, 500f);
            GUILayout.Label("SMOOTH: " + (int)Cfg.AimSmooth);
            Cfg.AimSmooth = GUILayout.HorizontalSlider(Cfg.AimSmooth, 1f, 30f);

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        static string OnOff(bool b) { return b ? "ON" : "OFF"; }
    }
}
