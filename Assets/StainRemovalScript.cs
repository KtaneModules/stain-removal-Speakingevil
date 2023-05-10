using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class StainRemovalScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public List<KMSelectable> detergents;
    public List<KMSelectable> fabriccells;
    public KMSelectable resetbutton;
    public Renderer[] baserends;
    public Renderer[] detrends;
    public Renderer[] leds;
    public Renderer fabrend;
    public Renderer[] stainrends;
    public Material[] detmats;
    public Material[] io;
    public GameObject resetstain;
    public GameObject matstore;

    private readonly string[] brands = new string[9] { "Wave", "Hidro Clean", "Vamoose", "Satellite", "Swash", "Hammer", "Siv", "Green", "Ecozy"};
    private bool[] c = new bool[9];
    private bool[][,] detareas = new bool[9][,];
    private List<int> dets = new List<int> { };
    private int[][] cselect = new int[7][];
    private int[][,] fgrid = new int[2][,] { new int[8, 8], new int[8, 8] };
    private int[] uses = new int[5] { 3, 3, 3, 3, 3};
    private int dselect = -1;
    private bool bleached;
    private Color staincol;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

	private void Start ()
    {
        moduleID = ++moduleIDCounter;
        matstore.SetActive(false);
        leds[2].material = io[1];
        dets = Enumerable.Range(0, 9).ToArray().Shuffle().Take(5).ToList();
        for(int i = 0; i < 5; i++)
            detrends[i].material = detmats[dets[i]];
        c[0] = info.IsPortPresent(Port.Serial);
        c[1] = info.GetSerialNumberLetters().Any(x => "AEIOU".Contains(x.ToString()));
        c[2] = info.GetOnIndicators().Any();
        c[3] = dets.Any(x => x < 2);
        c[4] = info.GetModuleNames().GroupBy(x => x).Any(x => x.Count() > 1);
        c[5] = info.GetIndicators().Join().Any(x => "AEIOU".Contains(x.ToString()));
        c[6] = info.GetPortPlates().Any(x => x.Count() > 1);
        c[7] = info.GetBatteryCount() > info.GetBatteryHolderCount();
        c[8] = new int[] { 3, 4, 7, 8 }.Contains(dets[4]);
        int[] adets = dets.Concat(dets).Concat(dets).ToArray().Shuffle().Take(7).ToArray();
        for (int i = 0; i < 7; i++)
            cselect[i] = new int[3] { adets[i], Random.Range(0, 8), Random.Range(0, 8) };
        for (int i = 0; i < 5; i++)
            switch (dets[i])
            {
                case 0: detareas[0] = new bool[5, 5] { { true, c[1], c[0], c[1], true }, { !c[1], false, c[0], false, !c[1] }, { !c[0], !c[0], true, !c[0], !c[0] }, { !c[1], false, c[0], false, !c[1] }, { true, c[1], c[0], c[1], true } }; break;
                case 1: detareas[1] = new bool[5, 5] { { !c[1], c[2], true, c[2], !c[1] }, { c[2], c[1], !c[2], c[1], c[2] }, { true, !c[2], true, !c[2], true }, { c[2], c[1], !c[2], c[1], c[2] }, { !c[1], c[2], true, c[2], !c[1] } }; break;
                case 2: detareas[2] = new bool[5, 5] { { c[3], c[2], !c[2], c[2], c[3]}, { !c[2], c[3], true, c[3], !c[2]}, { c[3], true, true, true, c[3]}, { !c[2], !c[3], true, !c[3], !c[2]}, { !c[3], c[2], !c[2], c[2], !c[3]} };  break;
                case 3: detareas[3] = new bool[5, 5] { { !c[3], false, c[3], false, !c[3]}, { true, c[4], c[4], c[4], true}, { false, c[3], true, c[3], false}, { true, !c[4], !c[4], !c[4], true}, { !c[3], false, c[3], false, !c[3]} }; break;
                case 4: detareas[4] = new bool[5, 5] { { false, c[4], c[5], !c[4], false}, { c[4], c[4], c[5], !c[4], !c[4]}, { !c[5], !c[5], true, !c[5], !c[5]}, { !c[4], !c[4], c[5], c[4], c[4]}, { false, !c[4], c[5], c[4], false} }; break;
                case 5: detareas[5] = new bool[5, 5] { { c[5], c[5], c[5], c[5], c[5]}, { c[6], c[6], c[5], c[6], c[6]}, { false, true, true, true, false}, { !c[6], !c[6], !c[5], !c[6], !c[6]}, { !c[5], !c[5], !c[5], !c[5], !c[5]} }; break;
                case 6: detareas[6] = new bool[5, 5] { { !c[7], !c[6], true, !c[6], c[7]}, { c[6], !c[7], !c[6], c[7], c[6]}, { true, c[6], true, c[6], true}, { c[6], c[7], !c[6], !c[7], c[6]}, { c[7], !c[6], true, !c[6], !c[7]} }; break;
                case 7: detareas[7] = new bool[5, 5] { { c[8], c[7], !c[7], c[7], !c[8]}, { false, !c[7], c[7], !c[7], false}, { c[8], c[8], true, !c[8], !c[8]}, { false, c[7], !c[7], c[7], false}, { c[8], !c[7], c[7], !c[7], !c[8]} }; break;
                default: detareas[8] = new bool[5, 5] { { !c[8], c[0], false, !c[0], c[8]}, { !c[0], c[8], true, !c[8], c[0]}, { false, true, true, true, false}, { !c[0], c[8], true, !c[8], c[0]}, { !c[8], c[0], false, !c[0], c[8]} }; break;
            }
        for (int i = 0; i < 7; i++)
        {
            int[] dc = cselect[i];
            for (int j = -2; j < 3; j++)
            {
                if (dc[1] + j > 7)
                    break;
                if (dc[1] + j < 0)
                    continue;
                for (int k = -2; k < 3; k++)
                {
                    if (dc[2] + k > 7)
                        break;
                    if (dc[2] + k < 0)
                        continue;
                    if (detareas[dc[0]][j + 2, k + 2])
                        fgrid[0][dc[1] + j, dc[2] + k]++;
                }
            }
        }
        for (int i = 0; i < 64; i++)
        {
            int o = fgrid[0][i / 8, i % 8];
            fgrid[1][i / 8, i % 8] = o;
            if (o < 1)
                stainrends[i].enabled = false;
            else
                stainrends[i].material.color = new Color(0, 0, 0, o / 8f);
        }
        Debug.LogFormat("[Stain Removal #{0}] The number of treatments required for each square of the fabric is:\n[Stain Removal #{0}] {1}", moduleID, string.Join("\n[Stain Removal #" + moduleID +"] ", Enumerable.Range(0, 8).Select(x => string.Join("", Enumerable.Range(0, 8).Select(y => fgrid[0][x, y].ToString()).ToArray())).ToArray()));
        Debug.LogFormat("[Stain Removal #{0}] The detergent brand options are:\n[Stain Removal #{0}] {1}", moduleID, string.Join("\n[Stain Removal #" + moduleID + "] ", Enumerable.Range(0, 5).Select(x => brands[dets[x]] + ":\n[Stain Removal #" + moduleID + "] " + string.Join("\n[Stain Removal #" + moduleID + "] ", Enumerable.Range(0, 5).Select(y => string.Join("", Enumerable.Range(0, 5).Select(z => detareas[dets[x]][y, z] ? "\u25a1" : "\u25a0").ToArray())).ToArray())).ToArray()));
        string[] spots = new string[5];
        for(int i = 0; i < 7; i++)
        {
            int s = cselect[i][0];
            int d = dets.IndexOf(s);
            string coord = "ABCDEFGH"[cselect[i][2]] + (cselect[i][1] + 1).ToString();
            spots[d] += (spots[d] == null ? brands[s] + ": " : ", ") + coord;
        }
        for (int i = 0; i < 5; i++)
            if (spots[i] != null)
                spots[i] += ". ";
        Debug.LogFormat("[Stain Removal #{0}] Use the detergents at the following spots: {1}", moduleID, string.Join("", spots));
        Color col = new Color(Random.Range(0.2f, 0.7f), Random.Range(0.2f, 0.7f), Random.Range(0.2f, 0.7f));
        foreach (Renderer b in baserends)
            b.material.color = col;
        col += new Color(Random.Range(0.1f, 0.3f), Random.Range(0.1f, 0.3f), Random.Range(0.1f, 0.3f));
        fabrend.material.color = col;
        int sc = Random.Range(1, 8);
        col = new Color((col.r + ((sc / 4) / 2f)) % 1f, (col.g + (((sc / 2) % 2) / 2f)) % 1f, (col.b + ((sc % 2) / 2f)) % 1f);
        staincol = col;
        foreach (Renderer st in stainrends)
        {
            float a = st.material.color.a;
            st.material.color = new Color(col.r, col.g, col.b, a);
        }
        foreach(KMSelectable det in detergents)
        {
            int d = detergents.IndexOf(det);
            det.OnInteract = delegate ()
            {
                if (!moduleSolved && dselect != d)
                {
                    det.AddInteractionPunch(0.5f);
                    if (dselect >= 0)
                        leds[dselect].material = io[1];
                    Audio.PlaySoundAtTransform(uses[d] > 0 ? "Select" : "Empty", det.transform);
                    dselect = d;
                    leds[d].material = io[0];
                }
                return false;
            };
        }
        foreach(KMSelectable cell in fabriccells)
        {
            int fc = fabriccells.IndexOf(cell);
            cell.OnInteract = delegate ()
            {
                if (!moduleSolved && dselect >= 0)
                {
                    if (uses[dselect] < 1)
                        Audio.PlaySoundAtTransform("Empty", detergents[dselect].transform);
                    else
                    {
                        uses[dselect]--;
                        Audio.PlaySoundAtTransform("Wash", detergents[dselect].transform);
                        Debug.LogFormat("[Stain Removal #{0}] {1} used at {2}{3}.", moduleID, brands[dets[dselect]], "ABCDEFGH"[fc % 8], (fc / 8) + 1);                        
                        for (int j = 0; j < 5; j++)
                        {
                            int y = (fc / 8) + j - 2;
                            if (y > 7)
                                break;
                            if (y < 0)
                                continue;
                            for (int k = 0; k < 5; k++)
                            {
                                int x = (fc % 8) + k - 2;
                                if (x > 7)
                                    break;
                                if (x < 0)
                                    continue;
                                if (detareas[dets[dselect]][j, k])
                                {
                                    fgrid[1][y, x]--;
                                    int fg = fgrid[1][y, x];
                                    int fs = (y * 8) + x;
                                    if (fg < 0)
                                    {
                                        stainrends[fs].enabled = true;
                                        stainrends[fs].material.color = new Color(1, 1, 1, 1);
                                        if (!bleached)
                                        {
                                            bleached = true;
                                            module.HandleStrike();
                                        }
                                    }
                                    else if (fg == 0)
                                        stainrends[fs].enabled = false;
                                    else
                                    {
                                        col = stainrends[fs].material.color;
                                        stainrends[fs].material.color = new Color(col.r, col.g, col.b, fg / 8f);
                                    }
                                }
                            }
                        }
                        if (Enumerable.Range(0, 64).Select(x => fgrid[1][x / 8, x % 8]).All(x => x == 0))
                        {
                            moduleSolved = true;
                            module.HandlePass();
                            leds[dselect].material = io[1];
                            resetstain.SetActive(false);
                            Audio.PlaySoundAtTransform("Solve", transform);
                        }
                    }
                }
                return false;
            };
        }
        resetbutton.OnInteract = delegate ()
        {
            if (!moduleSolved)
            {
                bleached = false;
                uses = new int[5] { 3, 3, 3, 3, 3 };
                if(dselect >= 0)
                     leds[dselect].material = io[1];
                dselect = -1;
                Audio.PlaySoundAtTransform("Splat", transform);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, resetbutton.transform);
                for(int i = 0; i < 64; i++)
                {
                    int fg = fgrid[0][i / 8, i % 8];
                    fgrid[1][i / 8, i % 8] = fg;
                    if (fg < 1)
                        stainrends[i].enabled = false;
                    else
                    {
                        stainrends[i].enabled = true;
                        stainrends[i].material.color = staincol;
                        col = stainrends[i].material.color;
                        stainrends[i].material.color = new Color(col.r, col.g, col.b, fg / 8f);
                    }
                }
                Debug.LogFormat("[Stain Removal #{0}] Module reset.", moduleID);
            }
            return false;
        };
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} select <1-5> [Selects the detergent in the specified position.] | !{0} treat <a-h><1-8> [Selects the specified cell on the fabric.] | !{0} reset";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] commands = command.ToLowerInvariant().Split(' ');
        if(commands.Length > 2)
        {
            yield return "sendtochaterror!f Too many arguments.";
            yield break;
        }
        switch (commands[0])
        {
            case "select":
                if(commands.Length < 2)
                {
                    yield return "sendtochaterror!f No position given.";
                    yield break;
                }
                int d = 0;
                if (int.TryParse(commands[1], out d))
                {
                    if (d >= 1 && d <= 5)
                    {
                        yield return null;
                        detergents[d - 1].OnInteract();
                    }
                    else
                        yield return "sendtochaterror!f Positions must be in the range 1-5.";
                }
                else
                    yield return "sendtochaterror!f NaN position given.";
                yield break;
            case "treat":
                if (dselect < 0)
                    yield return "sendtochaterror!f No detergent selected.";
                else if (commands[1].Length != 2)
                    yield return "sendtochaterror!f \"" + commands[1] + "\" is an invalid coordinate.";
                else
                {
                    if ("abcdefgh".Contains(commands[1][0].ToString()) && "12345678".Contains(commands[1][1].ToString()))
                    {
                        int c = ((commands[1][0] - 'a') * 8) + (commands[1][1] - '1');
                        yield return null;
                        fabriccells[c].OnInteract();
                    }
                    else
                        yield return "sendtochaterror!f Invalid coordinate format.";
                }
                yield break;
            case "reset":
                yield return null;
                resetbutton.OnInteract();
                yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        if(dselect >= 0)
            resetbutton.OnInteract();
        for(int i = 0; i < 7; i++)
        {
            yield return new WaitForSeconds(0.1f);
            detergents[dets.IndexOf(cselect[i][0])].OnInteract();
            yield return new WaitForSeconds(0.1f);
            fabriccells[(cselect[i][1] * 8) + cselect[i][2]].OnInteract();
        }
    }
}
