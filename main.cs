using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ChallengeModes.Helpers;
using static ChallengeModes.ChallengeModes;
using static ChallengeModes.SharedState;
using System.IO;

namespace ChallengeModes
{
    public static class SharedState
    {
        public static Dictionary<string, float> timers = new Dictionary<string, float>
        {
            ["soundCannonTimer"] = -10f,
            ["constantSepsisTimer"] = 10f,
            ["randomExplodeTimer"] = 0f,
            ["narcolepsyTimer"] = 0f,
            ["leechTimer"] = 0f,
            ["randomActivationTimer"] = 0f,
        };

        public static float narcolepsyTarget = UnityEngine.Random.Range(60f, 300f);
        public static float randomActivationGoal = 10f;
        public static bool skipMindWipeStart = false;

        public class ModifierInfo
        {
            // depth, kills, layer, custom
            public string[] goals = { "depth" };

            public string name;
            public string description;
            public string author;
            public bool needsTimer = false;
        }

        public static Dictionary<string, ModifierInfo> modifierOptions = new Dictionary<string, ModifierInfo>
        {
            ["10NeuralBoosters"] = new ModifierInfo() { name = "10 Neural Boosters", description = "Boosts your stats 10 times" },
            ["1mSoundCannon"] = new ModifierInfo() { name = "1m Sound Cannon", description = "Spawns a sound cannon every minute" },
            ["Ablutophobia"] = new ModifierInfo() { name = "Ablutophobia", description = "Always 100% dirty" },
            ["Armless"] = new ModifierInfo() { name = "Armless", description = "No arms. Crazy" },
            ["Blind"] = new ModifierInfo() { name = "Blind", description = "No eyes. Also crazy" },
            ["Cancer"] = new ModifierInfo() { name = "Cancer", description = "Constant radiation growth" },
            ["ConstantDepression"] = new ModifierInfo() { name = "Constant Depression", description = "Mood is always below 0" },
            ["ConstantEarthquakes"] = new ModifierInfo() { name = "Constant Earthquakes", description = "Earthquakes unless 25 energy or less", author = "3_femtanyl_3" },
            ["ConstantSepsis"] = new ModifierInfo() { name = "Constant Sepsis", description = "You will always have at least one infection" },
            ["Deaf"] = new ModifierInfo() { name = "Deaf", description = "Zero volume. Have fun" },
            ["Dyslexia"] = new ModifierInfo() { name = "Dyslexia", description = "Text is always a little distorted" },
            ["Flimsy"] = new ModifierInfo() { name = "Flimsy", description = "0 strength, like a milky!" },
            ["GlassBones"] = new ModifierInfo() { name = "Glass Bones", description = "Stronger fall damage + shrapnel" },
            ["GlassCannon"] = new ModifierInfo() { name = "Glass Cannon", description = "17 strength but 0 resilience" },
            ["Hollowed"] = new ModifierInfo() { name = "Hollowed", description = "You start hollowed" },
            ["HostileTraders"] = new ModifierInfo() { name = "Hostile Traders", description = "All traders will kick you out" },
            ["ImmediateRadiationLine"] = new ModifierInfo() { name = "Immediate Radiation Line", description = "The radiation line moves immediately" },
            ["ManySalads"] = new ModifierInfo() { name = "Many Salads", description = "Spawns 20-100 salads every layer" },
            ["Narcolepsy"] = new ModifierInfo() { name = "Narcolepsy", description = "Randomly fall asleep (zzz)" },
            ["RandomActivation"] = new ModifierInfo() { name = "Random Activation", description = "Randomly toggles a modifier", author = "pablo.gonzalez.2009" },
            ["RandomExplode"] = new ModifierInfo() { name = "Random Explode", description = "1/480 chance to blow up every 10 seconds" },
            ["SuperSalad"] = new ModifierInfo() { name = "Super Salad", description = "A super strong salad spawns and tracks you down every layer", author = "3_femtanyl_3" },
            //["UntemperedGlass"] = new ModifierInfo() { name = "Untempered Glass", description = "All damage is 10 times stronger", author = "3_femtanyl_3" },
            ["ThousandsOfThem"] = new ModifierInfo() { name = "Thousands of Them", description = "Every few minutes, you get attacked by cave ticks", author = "3_femtanyl_3" },

            ["Bean"] = new ModifierInfo() { name = "Bean", description = "No jaw, no limbs, can't hear, and hollow", author = "milky.50262" },
            //["Chernobyl"] = new ModifierInfo() { name = "Chernobyl", description = "Start with radiation sickness", author = "milky.50262" },
            ["ShellShocked"] = new ModifierInfo() { name = "Shell Shocked", description = "100% trauma... What did you see", author = "milky.50262" },
            ["Halfed"] = new ModifierInfo() { name = "Halfed", description = "1 arm, 1 leg, 1 eye!", author = "milky.50262" },
            ["MyToes"] = new ModifierInfo() { name = "MY TOES", description = "Obese, broken feet, brain damage. Tough landing...", author = "milky.50262" },
            ["ThisExpieDeservesWorse"] = new ModifierInfo() { name = "This Expie Deserves Worse", description = "Unchipped, no jaw, brain damage, 60 trauma, broken hands, and no eating plants. Start with painkillers", author = "milky.50262" },
            ["NoStats"] = new ModifierInfo() { name = "No Stats", description = "0 intelligence, 0 resilience, 0 strength. Its a miracle you are alive", author = "milky.50262" },
            ["Scourging"] = new ModifierInfo() { name = "Scourging", description = "Destroy/kill as many creatures/traps/plants. Hands/traps only, 25 strength", author = "milky.50262" },
            ["TheyDrilledHowFar"] = new ModifierInfo() { name = "They Drilled HOW FAR?!", description = "Start at layer 5, kill as many salads as possible and live as long as possible. 15 STR/RES, 14 INT", author = "milky.50262", goals = new string[] { "depth", "kills" } },

            ["BloodBath"] = new ModifierInfo() { name = "Blood Bath", description = "Reach layer 5 without fixing external bleeding. Spawn with a plasma cutter and battery", author = "milky.50262", goals = new string[] { "custom", "layer" } },
            ["NoTracesLeftBehind"] = new ModifierInfo() { name = "No Traces Left Behind", description = "Cannot break blocks by hand or open crates or trade. Can only destroy plants or kill enemies or expie traders (only expie traders)", author = "milky.50262", goals = new string[] { "custom", "layer" } },
        };

        public static List<ModifierInfo> modifiers = new List<ModifierInfo>();

        public static TMP_FontAsset tmpFont;

        public static TimeSpan timer = new TimeSpan(0);
    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ChallengeModes : BaseUnityPlugin
    {
        public static ManualLogSource logger;
        public const string pluginGuid = "shushu.casualtiesunknown.challengemodes";
        public const string pluginName = "Challenge Modes";

        // Year.Month.Version.Bugfix
        public const string pluginVersion = "26.1.2.0";

        public static ChallengeModes Instance;

        public static int isOkayToPatch = 0;
        public static string activeVersion = "";
        public static bool repositionLoopStarted = false;

        public static List<RectTransform> topLeftButtons = new List<RectTransform>();
        public static float lastScanTime = Time.unscaledTime;
        public static bool cacheDirty = true;
        public static float scanCooldown = 0.25f;

        public static GameObject thornbackObj;
        public static SpiderHandler thornbackMe;
        public static BuildingEntity thornbackBuild;

        private static Harmony harmony;

        public void Awake()
        {
            Instance = this;
            logger = Logger;

            logger.LogInfo("Awake() ran - mod loaded!");

            harmony = new Harmony(pluginGuid);

            (string, byte[]) fileInfo = FileLoader.LoadFileBytes("origamimommy.ttf");
            string modDir = Path.Combine(Application.persistentDataPath, Assembly.GetExecutingAssembly().GetName().Name);
            Directory.CreateDirectory(modDir);
            string fontPath = Path.Combine(modDir, fileInfo.Item1);
            SHA256 sha256 = SHA256.Create();
            if (!File.Exists(fontPath))
                File.WriteAllBytes(fontPath, fileInfo.Item2);
            else
            {
                byte[] existingFileSha256 = sha256.ComputeHash(File.OpenRead(fontPath));
                byte[] embeddedFileSha256 = sha256.ComputeHash(fileInfo.Item2);
                if (!embeddedFileSha256.SequenceEqual(existingFileSha256))
                {
                    Log("Font hash isn't the same, overwriting");
                    File.WriteAllBytes(fontPath, fileInfo.Item2);
                }
            }
            Font unityFont = new Font(fontPath);
            tmpFont = TMP_FontAsset.CreateFontAsset(unityFont);

            StartCoroutine(CheckGameVersion());
        }

        public static void Log(string message)
        {
            logger.LogInfo(message);
        }

        public static IEnumerator CheckGameVersion()
        {
            harmony.Patch(AccessTools.Method(typeof(PreRunScript), "Awake"), prefix: new HarmonyMethod(typeof(ChallengeModes).GetMethod("VersionCheck")));

            while (true)
            {
                if (isOkayToPatch == 1)
                {
                    break;
                }
                if (isOkayToPatch == -1)
                {
                    harmony.Unpatch(AccessTools.Method(typeof(PreRunScript), "Awake"), HarmonyPatchType.Prefix);
                    logger.LogError($"Game version is not {activeVersion}, mod exiting...");
                    yield break;
                }
                yield return null;
            }

            harmony.Unpatch(AccessTools.Method(typeof(PreRunScript), "Awake"), HarmonyPatchType.Prefix);

            List<MethodInfo> patches = typeof(MyPatches).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList();
            foreach (MethodInfo patch in patches)
            {
                try
                {
                    string[] splitName = patch.Name.Replace("__", "$").Split('_');
                    for (int i = 0; i < splitName.Length; i++)
                        splitName[i] = splitName[i].Replace("$", "_");
                    if (splitName.Length < 3)
                        throw new Exception($"Patch method is named incorrectly\nPlease make sure the Patch method is named in the following pattern:\n\tTargetClass_TargetMethod_PatchType[_Version]");

                    if (splitName.Length >= 4)
                        if (splitName[3] != activeVersion)
                        {
                            Log($"{patch.Name} is not supported by version {activeVersion}");
                            continue;
                        }

                    string targetType = splitName[0];
                    MethodType targetMethodType;
                    if (splitName[1].Contains("get_"))
                        targetMethodType = MethodType.Getter;
                    else if (splitName[1].Contains("set_"))
                        targetMethodType = MethodType.Setter;
                    else
                        targetMethodType = MethodType.Normal;
                    string ogTargetMethod = splitName[1];
                    string targetMethod = splitName[1].Replace("get_", "").Replace("set_", "");
                    string patchType = splitName[2];

                    MethodInfo patchScript = typeof(MyPatches).GetMethod(patch.Name);

                    ParameterInfo[] parameters = patchScript.GetParameters();
                    Type[] scriptArgTypes = parameters.Where(p => p.ParameterType != AccessTools.TypeByName(targetType)).Select(p => p.ParameterType).ToArray();

                    MethodInfo ogScript = null;
                    switch (targetMethodType)
                    {
                        case MethodType.Enumerator:
                        case MethodType.Normal:
                            try
                            {
                                ogScript = AccessTools.Method(AccessTools.TypeByName(targetType), targetMethod);
                            }
                            catch (Exception)
                            {
                                ogScript = AccessTools.Method(AccessTools.TypeByName(targetType), targetMethod, scriptArgTypes);
                            }
                            break;

                        case MethodType.Getter:
                            ogScript = AccessTools.PropertyGetter(AccessTools.TypeByName(targetType), targetMethod);
                            break;

                        case MethodType.Setter:
                        case MethodType.Constructor:
                        case MethodType.StaticConstructor:
                        default:
                            throw new Exception($"Unknown patch method\nPatch method type \"{targetMethodType}\" currently has no handling");
                    }

                    List<string> validPatchTypes = new List<string>
                    {
                        "Prefix",
                        "Postfix",
                        "Transpiler"
                    };
                    if (ogScript == null || patchScript == null || !validPatchTypes.Contains(patchType))
                    {
                        throw new Exception("Patch method is named incorrectly\nPlease make sure the Patch method is named in the following pattern:\n\tTargetClass_TargetMethod_PatchType[_Version]");
                    }
                    HarmonyMethod harmonyMethod = new HarmonyMethod(patchScript)
                    {
                        methodType = targetMethodType
                    };

                    HarmonyMethod postfix = null;
                    HarmonyMethod prefix = null;
                    HarmonyMethod transpiler = null;
                    switch (patchType)
                    {
                        case "Prefix":
                            prefix = harmonyMethod;
                            break;

                        case "Postfix":
                            postfix = harmonyMethod;
                            break;

                        case "Transpiler":
                            transpiler = harmonyMethod;
                            break;
                    }
                    harmony.Patch(ogScript, prefix: prefix, postfix: postfix, transpiler: transpiler);
                    Log("Patched " + targetType + "." + targetMethod + " as a " + patchType);
                }
                catch (Exception exception)
                {
                    logger.LogError($"Failed to patch {patch.Name}");
                    logger.LogError(exception);
                }
            }

            foreach (KeyValuePair<string, ModifierInfo> modifier in modifierOptions)
            {
                int modActive = PlayerPrefs.GetInt("ChallengeModes_" + modifier.Key.Replace(" ", "_"), 0);
                if (modActive == 1)
                    modifiers.Add(modifier.Value);
            }

            // If you have any PreRunScript Awake/Start patches, uncomment next line
            SceneManager.LoadScene("PreGen");
        }

        public static void VersionCheck()
        {
            Dictionary<string, string[]> supportedVersions = new Dictionary<string, string[]>
            {
                ["Text (TMP) (18)"] = new string[] { "V5 Pre-testing 5", "v5p5" },
                ["Text (TMP) (17)"] = new string[] { "V5 Pre-testing 4", "v5p4" }
            };
            foreach (var supportedVersion in supportedVersions)
            {
                if (isOkayToPatch == 0)
                {
                    GameObject obj = GameObject.Find(supportedVersion.Key);
                    if (obj == null)
                        continue;
                    if (obj.GetComponent<TextMeshProUGUI>().text.Contains(supportedVersion.Value[0]))
                    {
                        activeVersion = supportedVersion.Value[1];
                        isOkayToPatch = 1;
                        break;
                    }
                }
            }
            if (isOkayToPatch == 0)
                isOkayToPatch = -1;
        }

        private class ModButtonInfo
        {
            public int ButtonType;
            public string OptionsMain;
            public string ButtonName;
            public string LabelText;
            public string ListName;
        }

        public static void CalculatePosition()
        {
            RectTransform dropdownToggleRect = GameObject.Find("dropdownToggleContainer").GetComponent<RectTransform>();
            dropdownToggleRect.anchorMin = new Vector2(0f, 1f);
            dropdownToggleRect.anchorMax = new Vector2(0f, 1f);
            dropdownToggleRect.pivot = new Vector2(0f, 1f);

            float maxRightEdge = 0f;
            Vector2 localPoint = Vector2.zero;
            foreach (RectTransform rt in topLeftButtons)
            {
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                float rightEdge = corners[2].x;
                maxRightEdge = Mathf.Max(maxRightEdge, rightEdge);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt,
                    new Vector2(maxRightEdge, 0f),
                    null,
                    out localPoint
                );
            }

            dropdownToggleRect.anchoredPosition = new Vector2(localPoint.x - 3f, 0f);
        }

        public static void RebuildButtonCache()
        {
            topLeftButtons.Clear();

            foreach (Button button in GameObject.FindObjectsOfType<Button>())
            {
                RectTransform rt = button.GetComponent<RectTransform>();
                if (!rt || !rt.gameObject.activeInHierarchy || rt.gameObject.name == "dropdownToggle")
                    continue;

                Vector2 screenTopLeft = new Vector2(0, Screen.height);
                float maxDist = 500f;

                Vector2 rtScreenPos = RectTransformUtility.WorldToScreenPoint(null, rt.transform.position);
                float distX = rtScreenPos.x - screenTopLeft.x;
                float distY = screenTopLeft.y - rtScreenPos.y;
                if (distX >= 0 && distX <= maxDist && distY >= 0 && distY <= maxDist)
                    topLeftButtons.Add(rt);
            }
        }

        public static void ForceUpdateCanvases_Postfix()
        {
            cacheDirty = true;
        }

        public static IEnumerator UIRepositionLoop()
        {
            harmony.Patch(AccessTools.Method(typeof(Canvas), "ForceUpdateCanvases"), prefix: new HarmonyMethod(typeof(ChallengeModes).GetMethod("ForceUpdateCanvases_Postfix")));
            if (repositionLoopStarted)
                yield break;
            repositionLoopStarted = true;
            while (SceneManager.GetActiveScene().name == "PreGen")
            {
                if (cacheDirty && Time.unscaledTime - lastScanTime > scanCooldown)
                {
                    RebuildButtonCache();
                    lastScanTime = Time.unscaledTime;
                    cacheDirty = false;
                }
                else
                    CalculatePosition();
                yield return null;
            }
            repositionLoopStarted = false;
        }

        public static IEnumerator CreateModeUI()
        {
            yield return null;
            if (activeVersion != "v5p5")
            {
                modifierOptions.Remove("Dyslexia");
            }
            Canvas canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
            GameObject challengeModeDropdown = new GameObject("challengeModeDropdown");
            RectTransform dropdownRect = challengeModeDropdown.AddComponent<RectTransform>();
            dropdownRect.SetParent(canvas.transform, false);
            dropdownRect.anchorMax = new Vector2(0f, 1f);
            dropdownRect.anchorMin = new Vector2(0f, 1f);
            dropdownRect.pivot = new Vector2(0f, 1f);
            dropdownRect.anchoredPosition = new Vector2(19f, -35f);
            VerticalLayoutGroup challengeModeDropdownVLG = challengeModeDropdown.AddComponent<VerticalLayoutGroup>();
            challengeModeDropdownVLG.spacing = 15;
            challengeModeDropdownVLG.childAlignment = TextAnchor.UpperLeft;
            challengeModeDropdownVLG.childControlHeight = false;
            challengeModeDropdownVLG.childControlWidth = false;
            challengeModeDropdownVLG.childForceExpandHeight = false;
            challengeModeDropdownVLG.childForceExpandWidth = false;
            challengeModeDropdownVLG.padding.top += 31;
            ContentSizeFitter challengeModeDropdownFitter = challengeModeDropdown.AddComponent<ContentSizeFitter>();
            challengeModeDropdownFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            challengeModeDropdownFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject dropdownToggleContainer = new GameObject("dropdownToggleContainer");
            RectTransform dropdownToggleContainerRect = dropdownToggleContainer.AddComponent<RectTransform>();
            dropdownToggleContainerRect.SetParent(challengeModeDropdown.transform, false);
            dropdownToggleContainerRect.anchorMin = new Vector2(0f, 1f);
            dropdownToggleContainerRect.anchorMax = new Vector2(0f, 1f);
            dropdownToggleContainerRect.pivot = new Vector2(0f, 1f);
            LayoutElement dropdownToggleContainerLE = dropdownToggleContainer.AddComponent<LayoutElement>();
            dropdownToggleContainerLE.ignoreLayout = true;
            GameObject dropdownToggle = new GameObject("dropdownToggle");
            RectTransform dropdownToggleRect = dropdownToggle.AddComponent<RectTransform>();
            dropdownToggleRect.SetParent(dropdownToggleContainer.transform, false);
            dropdownToggleRect.anchorMin = new Vector2(0f, 1f);
            dropdownToggleRect.anchorMax = new Vector2(0f, 1f);
            dropdownToggleRect.pivot = new Vector2(0f, 1f);
            dropdownToggleRect.sizeDelta = new Vector2(173f, 31f);
            dropdownToggleRect.anchoredPosition = Vector2.zero;

            RebuildButtonCache();
            CalculatePosition();

            Image dropdownToggleImage = dropdownToggle.AddComponent<Image>();
            dropdownToggleImage.sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(f => f.name.Contains("uiBlockNano"));
            dropdownToggleImage.type = Image.Type.Sliced;
            Button dropdownToggleButton = dropdownToggle.AddComponent<Button>();
            dropdownToggleButton.transition = Selectable.Transition.ColorTint;
            dropdownToggleButton.colors = new ColorBlock()
            {
                normalColor = new Color(1f, 1f, 1f, 1f),
                highlightedColor = new Color(0.7453f, 0.7453f, 0.7453f, 1f),
                pressedColor = new Color(0.4906f, 0.4906f, 0.4906f, 1f),
                selectedColor = new Color(0.9608f, 0.9608f, 0.9608f, 1f),
                disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f),
                colorMultiplier = 1f,
                fadeDuration = 0f
            };
            GameObject dropdownToggleText = new GameObject("Label");
            dropdownToggleText.transform.SetParent(dropdownToggle.transform, false);
            TextMeshProUGUI dropdownToggleTMP = dropdownToggleText.AddComponent<TextMeshProUGUI>();
            dropdownToggleTMP.text = "Modifiers";
            dropdownToggleTMP.font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name.Contains("Retro GamingPix"));
            dropdownToggleTMP.fontSize = 15;
            ContentSizeFitter dropdownToggleTextFitter = dropdownToggleText.AddComponent<ContentSizeFitter>();
            dropdownToggleTextFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            dropdownToggleTextFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject scrollView = new GameObject("scrollView");
            RectTransform scrollViewRect = scrollView.AddComponent<RectTransform>();
            scrollViewRect.SetParent(challengeModeDropdown.transform, false);
            scrollViewRect.sizeDelta = new Vector2(0f, 650f);
            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 20f;

            GameObject viewport = new GameObject("viewport");
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.SetParent(scrollView.transform, false);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportRect.pivot = new Vector2(0f, 1f);
            viewport.AddComponent<RectMask2D>();

            scrollRect.viewport = viewportRect;

            GameObject scrollbar = new GameObject("scrollBar");
            RectTransform scrollbarRect = scrollbar.AddComponent<RectTransform>();
            scrollbarRect.SetParent(scrollView.transform, false);
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = Vector2.one;
            scrollbarRect.pivot = Vector2.one;
            scrollbarRect.sizeDelta = new Vector2(20f, 0f);
            scrollbarRect.anchoredPosition = Vector2.zero;
            Image scrollbarImage = scrollbar.AddComponent<Image>();
            scrollbarImage.color = Color.gray;
            Scrollbar scrollbarScroller = scrollbar.AddComponent<Scrollbar>();
            scrollbarScroller.direction = Scrollbar.Direction.BottomToTop;

            scrollRect.verticalScrollbar = scrollbarScroller;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.verticalScrollbarSpacing = -3;

            GameObject scrollHandle = new GameObject("scrollHandle");
            RectTransform scrollHandleRect = scrollHandle.AddComponent<RectTransform>();
            scrollHandleRect.SetParent(scrollbar.transform, false);
            scrollHandleRect.anchorMin = Vector2.zero;
            scrollHandleRect.anchorMax = Vector2.one;
            scrollHandleRect.sizeDelta = Vector2.zero;
            Image scrollHandleImage = scrollHandle.AddComponent<Image>();
            scrollHandleImage.color = Color.white;

            scrollbarScroller.handleRect = scrollHandleRect;

            GameObject dropdownOptions = new GameObject("dropdownOptions");
            dropdownOptions.transform.SetParent(viewport.transform, false);
            RectTransform rect = dropdownOptions.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0f, 1f);
            Image dropdownOptionsImage = dropdownOptions.AddComponent<Image>();
            dropdownOptionsImage.color = new Color(0, 0, 0, 0.8f);
            dropdownOptionsImage.raycastTarget = true;
            UITooltip dropdownOptionsUITooltip = dropdownOptions.AddComponent<UITooltip>();
            dropdownOptionsUITooltip.skipLocale = true;
            dropdownOptionsUITooltip.tipName = "";
            dropdownOptionsUITooltip.tipDesc = "";
            var layout = dropdownOptions.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.spacing = 15;
            layout.padding = new RectOffset
            {
                top = 15,
                bottom = 15,
                left = 15,
                right = 15
            };
            ContentSizeFitter CSF = dropdownOptions.AddComponent<ContentSizeFitter>();
            CSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            CSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            foreach (KeyValuePair<string, ModifierInfo> modifier in modifierOptions)
            {
                string key = modifier.Key;
                ModifierInfo info = modifier.Value;
                GameObject modifierToggle = new GameObject(key);
                modifierToggle.transform.SetParent(dropdownOptions.transform, false);
                modifierToggle.layer = LayerMask.NameToLayer("UI");
                UITooltip modifierToggleUITooltip = modifierToggle.AddComponent<UITooltip>();
                modifierToggleUITooltip.skipLocale = true;
                modifierToggleUITooltip.tipName = info.name;
                modifierToggleUITooltip.tipDesc = info.description;
                Image modifierToggleRaycastTarget = modifierToggle.AddComponent<Image>();
                modifierToggleRaycastTarget.color = new Color(0, 0, 0, 0);
                modifierToggleRaycastTarget.raycastTarget = true;
                Toggle toggle = modifierToggle.AddComponent<Toggle>();
                HorizontalLayoutGroup layout2 = modifierToggle.AddComponent<HorizontalLayoutGroup>();
                layout2.spacing = 5;
                layout2.childForceExpandWidth = false;
                var bg = new GameObject("Background");
                bg.transform.SetParent(modifierToggle.transform, false);
                RectTransform bgRect = bg.AddComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(32.4001f, 32.4001f);
                layout2.childControlHeight = false;
                layout2.childControlWidth = false;
                Image bgImage = bg.AddComponent<Image>();
                bgImage.sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(f => f.name.Contains("uiBlockNano"));
                bgImage.type = Image.Type.Sliced;
                var check = new GameObject("Checkmark");
                check.transform.SetParent(bg.transform, false);
                RectTransform rect2 = check.AddComponent<RectTransform>();
                rect2.sizeDelta = new Vector2(20, 20);
                check.AddComponent<Image>();
                toggle.targetGraphic = bg.GetComponent<Image>();
                toggle.graphic = check.GetComponent<Image>();
                toggle.isOn = modifiers.Contains(info);
                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        if (!modifiers.Contains(info))
                        {
                            modifiers.Add(info);
                            PlayerPrefs.SetInt("ChallengeModes_" + key.Replace(" ", "_"), 1);
                        }
                    }
                    else if (modifiers.Contains(info))
                    {
                        modifiers.Remove(info);
                        PlayerPrefs.SetInt("ChallengeModes_" + key.Replace(" ", "_"), 0);
                    }
                    PlayerPrefs.Save();
                });
                var textObj = new GameObject("Label");
                textObj.transform.SetParent(modifierToggle.transform, false);
                textObj.AddComponent<RectTransform>().pivot = new Vector2(-1f, 0.5f);
                ContentSizeFitter textObjFitter = textObj.AddComponent<ContentSizeFitter>();
                textObjFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                textObjFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = info.name;
                text.enableWordWrapping = false;
                //textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
                text.font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name.Contains("Retro GamingPix"));
                text.fontSize = 23;
            }

            Dictionary<string, ModButtonInfo> modButtons = new Dictionary<string, ModButtonInfo>
            {
                ["QoLOptionsButton"] = new ModButtonInfo
                {
                    ButtonType = 0,
                    OptionsMain = "QoLOptionsButton",
                    ListName = "QoLOptionsList",
                    ButtonName = "QoLToggle/Label",
                    LabelText = "QoL Settings"
                },
                ["CustomOptionsToggle"] = new ModButtonInfo
                {
                    ButtonType = 1,
                    OptionsMain = "CustomOptionsButton",
                    ButtonName = "CustomOptionsToggle",
                    LabelText = "Show Options",
                    ListName = "dropdownOptions",
                }
            };
            dropdownToggleButton.onClick.AddListener(() =>
            {
                scrollView.SetActive(!scrollView.activeSelf);
                if (scrollView.activeSelf)
                {
                    foreach (string buttonName in modButtons.Keys)
                    {
                        ModButtonInfo modButtonData = modButtons[buttonName];
                        switch (modButtonData.ButtonType)
                        {
                            case 0:
                                GameObject mod0Main = GameObject.Find(modButtonData.OptionsMain);
                                if (mod0Main != null)
                                {
                                    Transform mod0List = mod0Main.transform.Find(modButtonData.ListName);
                                    if (mod0List != null && mod0List.gameObject.activeSelf)
                                    {
                                        mod0List.gameObject.SetActive(false);
                                        Transform mod0Label = mod0Main.transform.Find(modButtonData.ButtonName);
                                        if (mod0Label != null)
                                        {
                                            var tmp = mod0Label.GetComponent<TextMeshProUGUI>();
                                            if (tmp != null) tmp.text = modButtonData.LabelText;
                                        }
                                    }
                                }
                                break;

                            case 1:
                                GameObject mod1Main = GameObject.Find(modButtonData.OptionsMain);
                                if (mod1Main != null)
                                {
                                    Transform mod1List = mod1Main.transform.Find(modButtonData.ListName);
                                    if (mod1List != null && mod1List.gameObject.activeSelf)
                                    {
                                        mod1List.gameObject.SetActive(false);
                                        Transform mod1Button = mod1Main.transform.Find(modButtonData.ButtonName);
                                        Transform mod1Label = mod1Button?.transform.Find("Label");
                                        if (mod1Label != null)
                                        {
                                            var tmp1 = mod1Label.GetComponent<TextMeshProUGUI>();
                                            if (tmp1 != null) tmp1.text = modButtonData.LabelText;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            });
            scrollRect.content = rect;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownOptions.transform as RectTransform);
            float preferredWidth = LayoutUtility.GetPreferredWidth(rect);
            viewportRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
            scrollViewRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth + 20);
            scrollView.SetActive(false);
            Instance.StartCoroutine(UIRepositionLoop());
        }

        public static IEnumerator CustomMindwipeSequence(Body body)
        {
            if (body.mindWipe == null)
            {
            }
            float timer = 0f;
            while (timer < 3f)
            {
                body.Ragdoll();
                body.shock = 20f;
                body.hearingLoss += Time.deltaTime * 50f;
                PlayerCamera.main.bonusAbber += Time.deltaTime * 8f;
                timer += Time.deltaTime;
                yield return null;
            }
            body.hearingLoss = 80f;
            body.brainHealth += 50f;
            foreach (Recipe recipe in Recipes.recipes)
            {
                recipe.hasMadeBefore = false;
            }
            body.skills.INT = 0;
            body.skills.expINT = 0f;
            body.skills.UpdateExpBoundaries();
            MusicManager.main.StopSong();
            (UnityEngine.Object.Instantiate(Resources.Load("Special/MindwipeVignette"), PlayerCamera.main.mainCanvas.transform) as GameObject).transform.SetAsFirstSibling();
        }
    }

    public class MyPatches
    {
        private static Body body;
        private static TextMeshProUGUI counterTMP;
        private static RectTransform counterRT;

        private static string TimeSpanToString(TimeSpan timer)
        {
            return $"{(int)timer.TotalMinutes:D3}:{timer.Seconds:D2}.{timer.Milliseconds:D3}";
        }

        [HarmonyPatch(typeof(PreRunScript), "Start")]
        [HarmonyPrefix]
        public static void PreRunScript_Start_Prefix(PreRunScript __instance)
        {
            AudioListener.volume = 1f;
            Limb.dislocationHealSpeed = 0.07f;
            Limb.boneHealSpeed = 0.043f;
            Instance.StartCoroutine(CreateModeUI());
        }

        [HarmonyPatch(typeof(Body), "Start")]
        [HarmonyPrefix]
        public static void Body_Start_Prefix(Body __instance)
        {
            GameObject canvas = GameObject.Find("Canvas");
            GameObject counter = new GameObject("Counter");
            counterRT = counter.AddComponent<RectTransform>();
            counterRT.SetParent(canvas.transform);
            counterRT.anchorMin = Vector2.one;
            counterRT.anchorMax = Vector2.one;
            counterRT.offsetMin = Vector2.zero;
            counterRT.offsetMax = Vector2.zero;
            counterRT.pivot = new Vector2(0, 1);
            counterRT.anchoredPosition = new Vector2(-95, -15);
            GameObject counterT = new GameObject("CounterT");
            RectTransform counterTRT = counterT.AddComponent<RectTransform>();
            counterTRT.SetParent(counter.transform);
            counterTRT.anchorMin = Vector2.zero;
            counterTRT.anchorMax = Vector2.one;
            counterTRT.offsetMin = Vector2.zero;
            counterTRT.offsetMax = Vector2.zero;
            counterTMP = counterT.AddComponent<TextMeshProUGUI>();
            counterTMP.alignment = TextAlignmentOptions.Midline;
            counterTMP.text = "000:00.000";
            counterTMP.font = tmpFont;
            counterRT.sizeDelta = new Vector2(counterTMP.GetPreferredValues().x + 30, counterTMP.GetPreferredValues().y + 30);
            counterTMP.enableWordWrapping = false;
            counterTMP.margin = new Vector4(15, 15, 15, 15);
            Image counterBGI = counter.AddComponent<Image>();
            counterBGI.color = new Color(0, 0, 0, 0.8f);
        }

        private static void CalculateCounterOffset()
        {
            Vector2 pos = new Vector2(-95 - counterRT.rect.width, -15);
            if (PlayerCamera.main.woundView.activeSelf)
                pos.y = -246;
            counterRT.anchoredPosition = pos;
        }

        [HarmonyPatch(typeof(Body), "Update")]
        [HarmonyPostfix]
        public static void Body_Update_Postfix(Body __instance)
        {
            if (counterTMP != null)
            {
                timer = timer.Add(TimeSpan.FromSeconds(Time.deltaTime));
                counterTMP.text = TimeSpanToString(timer);
                CalculateCounterOffset();
            }
            string modifierToToggle = "";
            foreach (string key in timers.Keys.ToList())
            {
                timers[key] += Time.deltaTime;
            }
            foreach (ModifierInfo modifier in modifiers)
            {
                Body body = __instance;
                switch (modifier.name)
                {
                    case "Constant Depression":
                        if (body.totalHappiness > 0f)
                            body.happiness -= Time.deltaTime;
                        break;

                    case "Cancer":
                        if (body.radiationSickness < 80f)
                            body.radiationSickness += Time.deltaTime * (0.01389f + 0.033f);
                        break;

                    case "Random Explode":
                        if (timers["randomExplodeTimer"] > 10f)
                        {
                            timers["randomExplodeTimer"] = 0f;
                            int randInt = UnityEngine.Random.Range(0, 480);
                            if (randInt == 0)
                                WorldGeneration.CreateExplosion(new ExplosionParams { position = body.transform.position });
                        }
                        break;

                    case "Constant Sepsis":
                        if (timers["constantSepsisTimer"] > 10f)
                        {
                            timers["constantSepsisTimer"] = 0f;
                            if (body.septicShock < 10f)
                            {
                                Limb randomLimb = body.limbs[UnityEngine.Random.Range(0, body.limbs.Length)];
                                randomLimb.infected = true;
                                randomLimb.infectionAmount += 10;
                            }
                        }
                        break;

                    case "1m Sound Cannon":
                        if (!body.conscious)
                            timers["soundCannonTimer"] = 0f;
                        if (timers["soundCannonTimer"] > 360f)
                        {
                            timers["soundCannonTimer"] = 0f;
                            ChallengeModes.Instantiate<GameObject>((GameObject)Resources.Load("soundcannon"), body.transform.position, body.transform.rotation);
                        }
                        break;

                    case "Narcolepsy":
                        if (!body.conscious)
                            timers["narcolepsyTimer"] = 0f;
                        if (timers["narcolepsyTimer"] > narcolepsyTarget)
                        {
                            if (timers["narcolepsyTimer"] > narcolepsyTarget + 10f)
                            {
                                timers["narcolepsyTimer"] = 0f;
                                narcolepsyTarget = UnityEngine.Random.Range(60f, 300f);
                                body.energy = Mathf.Clamp(body.energy -= 15f, 0f, 60f);
                                body.sleeping = true;
                            }
                            body.consciousness = Mathf.Lerp(body.consciousness, 31f, 3);
                        }
                        break;

                    case "Ablutophobia":
                        body.dirtyness = 100f;
                        break;

                    case "Thousands of Them":
                        if (!body.conscious)
                            timers["leechTimer"] = 0f;
                        if (timers["leechTimer"] > 240)
                        {
                            ChallengeModes.Instantiate<GameObject>((GameObject)Resources.Load("caveticks"), body.transform.position, body.transform.rotation);
                            timers["leechTimer"] = 0f;
                        }
                        break;

                    case "Random Activation":
                        if (!body.conscious)
                            break;
                        if (timers["randomActivationTimer"] > randomActivationGoal)
                        {
                            modifierToToggle = modifierOptions.ElementAt(UnityEngine.Random.Range(0, modifierOptions.Count - 1)).Key;
                            timers["randomActivationTimer"] = 0f;
                            randomActivationGoal = UnityEngine.Random.Range(10f, 240f);
                        }
                        break;
                }
            }
            if (modifierToToggle != "")
            {
                if (modifierToToggle == "RandomActivation")
                    modifierToToggle = modifierOptions.ElementAt(modifierOptions.Count - 1).Key;
                if (modifiers.Contains(modifierOptions[modifierToToggle]))
                    modifiers.Remove(modifierOptions[modifierToToggle]);
                else
                    modifiers.Add(modifierOptions[modifierToToggle]);
            }
        }

        [HarmonyPatch(typeof(WorldGeneration), "Update")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> WorldGeneration_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if (!modifiers.Contains(modifierOptions["ConstantEarthquakes"]))
                return instructions;

            FieldInfo earthquakeIntensity = AccessTools.Field(typeof(WorldGeneration), "earthquakeIntensity");

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i + 1 > codes.Count - 1)
                    continue;
                if (codes[i].opcode == OpCodes.Ldfld && codes[i + 1].opcode == OpCodes.Ldc_R4 && codes[i].LoadsField(earthquakeIntensity))
                {
                    codes[i + 1].operand = 5;
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(MindwipeScript), "Start")]
        [HarmonyPrefix]
        public static bool MindwipeScript_Start_Prefix(MindwipeScript __instance)
        {
            __instance.active = true;
            if (skipMindWipeStart)
            {
                skipMindWipeStart = false;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(WorldGeneration), "ApplyLayerModifiers")]
        [HarmonyPostfix]
        public static void WorldGeneration_ApplyLayerModifiers_Postfix(WorldGeneration __instance)
        {
            body = PlayerCamera.main.body;
            if (body.hearingLoss >= 9999)
                body.hearingLoss = 0;
            foreach (ModifierInfo modifier in modifiers)
            {
                Log("Enabling " + modifier.name);
                if (!SaveSystem.loadedRun)
                {
                    switch (modifier.name)
                    {
                        case "Hollowed":
                            skipMindWipeStart = true;
                            if (!body.GetComponent<MindwipeScript>())
                            {
                                body.gameObject.AddComponent<MindwipeScript>();
                                body.shock += 10f;
                                ChallengeModes.Instance.StartCoroutine(CustomMindwipeSequence(body));
                            }
                            break;

                        case "Flimsy":
                            body.skills.STR = 0;
                            body.skills.expSTR = 0;
                            body.skills.UpdateExpBoundaries();
                            break;

                        case "Armless":
                            body.LimbByName("UpArmF").Dismember();
                            body.LimbByName("UpArmB").Dismember();
                            body.LimbByName("UpTorso").skinHealth = 100f;
                            body.LimbByName("UpTorso").muscleHealth = 100f;
                            body.LimbByName("UpTorso").bleedAmount = 0f;
                            body.LimbByName("UpTorso").pain = 0f;
                            break;

                        case "Blind":
                            body.RemoveEye();
                            body.RemoveEye();
                            body.limbs[0].pain = 0f;
                            body.limbs[0].bleedAmount = 0f;
                            break;

                        case "10 Neural Boosters":
                            body.usedNeuralBooster = true;
                            for (int i = 0; i < 10; i++)
                            {
                                body.maxSpeed *= 1.25f;
                                body.moveForce *= 1.25f;
                                body.jumpSpeed *= 1.2f;
                                body.caffeinated += 600f;
                            }
                            break;

                        case "Glass Cannon":
                            if (!modifiers.Contains(modifierOptions["Flimsy"]))
                            {
                                body.skills.STR = 17;
                                body.skills.expSTR = 2073;
                            }
                            body.skills.RES = 0;
                            body.skills.expRES = 0;
                            body.skills.UpdateExpBoundaries();
                            break;

                        case "Bean":
                            body.Disfigure();
                            body.hearingLoss = int.MaxValue;
                            body.LimbByName("UpArmF").Dismember();
                            body.LimbByName("UpArmB").Dismember();
                            body.LimbByName("ThighF").Dismember();
                            body.LimbByName("ThighB").Dismember();
                            body.LimbByName("HandF").dismembered = false;
                            foreach (Limb limb in body.limbs)
                            {
                                if (!limb.dismembered)
                                {
                                    limb.skinHealth = 100f;
                                    limb.muscleHealth = 100f;
                                    limb.bleedAmount = 0f;
                                    limb.pain = 0f;
                                }
                            }
                            skipMindWipeStart = true;
                            if (!body.GetComponent<MindwipeScript>())
                            {
                                body.gameObject.AddComponent<MindwipeScript>();
                                body.shock += 10f;
                                ChallengeModes.Instance.StartCoroutine(CustomMindwipeSequence(body));
                            }
                            break;

                        case "Shell Shocked":
                            body.traumaAmount = 100f;
                            break;

                        case "Halfed":
                            body.RemoveEye();
                            body.LimbByName("UpArmF").Dismember();
                            body.LimbByName("ThighF").Dismember();
                            foreach (Limb limb in body.limbs)
                            {
                                if (!limb.dismembered)
                                {
                                    limb.skinHealth = 100f;
                                    limb.muscleHealth = 100f;
                                    limb.bleedAmount = 0f;
                                    limb.pain = 0f;
                                }
                            }
                            break;

                        case "MY TOES":
                            body.weightOffset = 60f;
                            body.LimbByName("FootF").broken = true;
                            body.LimbByName("FootB").broken = true;
                            body.brainHealth = 80f;
                            break;

                        case "This Expie Deserves Worse":
                            WorldGeneration.world.unchippedMode = true;
                            body.disfigured = true;
                            body.brainHealth = 65f;
                            body.traumaAmount = 60f;
                            body.LimbByName("HandF").broken = true;
                            body.LimbByName("HandB").broken = true;
                            body.AutoPickUpItem(UnityEngine.Object.Instantiate(Resources.Load<GameObject>("painkillers"), body.transform.position, Quaternion.identity).GetComponent<Item>());
                            break;

                        case "No Stats":
                            body.skills.STR = 0;
                            body.skills.RES = 0;
                            body.skills.INT = 0;
                            body.skills.UpdateExpBoundaries();
                            break;
                    }
                }
                switch (modifier.name)
                {
                    case "Deaf":
                        body.hearingLoss = int.MaxValue;
                        AudioListener.volume = 0f;
                        break;

                    case "Many Salads":
                        int randLoopCount = UnityEngine.Random.Range(10, 100);
                        Log("Spawn goal: " + randLoopCount);
                        List<Vector2> coordArray = new List<Vector2>();
                        for (int i = 0; i < randLoopCount;)
                        {
                            float xCoord = UnityEngine.Random.Range(__instance.width / 2 * -1, __instance.width / 2);
                            float yCoord = UnityEngine.Random.Range(__instance.height / 2 * -1, __instance.height / 2);
                            Vector2 coords = new Vector2(xCoord, yCoord);
                            foreach (var prevCoord in coordArray)
                            {
                                if (!(Vector2.Distance(coords, prevCoord) > 200f))
                                    continue;
                            }
                            coordArray.Add(coords);
                            i++;
                            Log("Elder Thornback spawned at " + coords.x + " " + coords.y);
                            UnityEngine.Object.Instantiate(Resources.Load("thornbackelder"), coords, Quaternion.identity);
                        }
                        Log("Total Elder Thornbacks Spawned: " + coordArray.Count + " / " + randLoopCount);
                        break;

                    case "Immediate Radiation Line":
                        RadiationLine.line.active = true;
                        break;

                    case "Super Salad":
                        Vector2 spawnCoord = new Vector2(UnityEngine.Random.Range(__instance.width / 2 * -1, __instance.width / 2), UnityEngine.Random.Range(__instance.height / 2 * -1, __instance.height / 2));
                        thornbackObj = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("thornbackelder"), spawnCoord, Quaternion.identity);
                        thornbackMe = thornbackObj.GetComponent<SpiderHandler>();
                        thornbackBuild = thornbackObj.GetComponent<BuildingEntity>();
                        thornbackBuild.health = 999999f;
                        thornbackMe.burrowWallDamage = 999999f;
                        thornbackMe.retreatHealth = 0;
                        thornbackMe.seeDistance = 999999f;
                        break;
                }
            }
            Log("Finished applying game start challenges");
        }

        [HarmonyPatch(typeof(Body), "UseItem")]
        [HarmonyPrefix]
        public static bool Body_UseItem_Prefix(Item item)
        {
            if (modifiers.Contains(modifierOptions["ThisExpieDeservesWorse"]))
                return true;
            foreach (CraftingQuality itemCQ in item.gameObject.GetComponents<CraftingQuality>())
            {
                if (itemCQ.id == "produce")
                {
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(WorldGeneration), "Update")]
        [HarmonyPostfix]
        public static void WorldGeneration_Update_Postfix(WorldGeneration __instance)
        {
            if (!modifiers.Contains(modifierOptions["ConstantEarthquakes"]))
                return;
            if (body == null)
                return;
            if (body.energy > 25f)
                __instance.earthquakeDelay = 0f;
        }

        [HarmonyPatch(typeof(Limb), "Dislocate")]
        [HarmonyPrefix]
        public static void Limb_Dislocate_Prefix(Limb __instance)
        {
            if (modifiers.Contains(modifierOptions["GlassBones"]))
            {
                __instance.shrapnel = Mathf.Clamp(__instance.shrapnel + 3, 3, 5);
                Limb.dislocationHealSpeed = 0.035f;
            }
        }

        [HarmonyPatch(typeof(Limb), "Break Bone")]
        [HarmonyPrefix]
        public static void Limb_BreakBone_Prefix(Limb __instance)
        {
            if (modifiers.Contains(modifierOptions["GlassBones"]))
            {
                __instance.shrapnel = 5;
                Limb.boneHealSpeed = 0.0215f;
            }
        }

        [HarmonyPatch(typeof(Body), "Attack")]
        [HarmonyPrefix]
        public static void Body_Attack_Prefix(AttackInfo atk, int slot, Body __instance)
        {
            if (modifiers.Contains(modifierOptions["GlassBones"]))
            {
                if (atk.physicalSwing && UnityEngine.Random.Range(0, 2) == 0)
                {
                    foreach (Limb usedLimb in __instance.slots[slot].useLimbs)
                    {
                        if ((usedLimb.broken || usedLimb.dislocated) && UnityEngine.Random.Range(0, 4) == 0)
                        {
                            usedLimb.shrapnel = Mathf.Clamp(usedLimb.shrapnel + 3, 3, 5);
                            usedLimb.muscleHealth -= 6;
                            usedLimb.bleedAmount += 10;
                        }
                    }
                    if ((__instance.limbs[0].broken || __instance.limbs[0].dislocated) && UnityEngine.Random.Range(0, 4) == 0)
                    {
                        __instance.limbs[0].shrapnel = Mathf.Clamp(__instance.limbs[0].shrapnel + 3, 3, 5);
                        __instance.limbs[0].muscleHealth -= 6;
                        __instance.limbs[0].bleedAmount += 10;
                    }

                    if ((__instance.limbs[1].broken || __instance.limbs[1].dislocated) && UnityEngine.Random.Range(0, 4) == 0)
                    {
                        __instance.limbs[1].shrapnel = Mathf.Clamp(__instance.limbs[1].shrapnel + 3, 3, 5);
                        __instance.limbs[1].muscleHealth -= 6;
                        __instance.limbs[1].bleedAmount += 10;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Body), "Jump")]
        [HarmonyPrefix]
        public static void Body_Jump_Prefix_v5p5(Body __instance)
        {
            if (modifiers.Contains(modifierOptions["GlassBones"]))
            {
                foreach (Limb legLimb in __instance.legLimbs)
                {
                    if ((legLimb.dislocated || legLimb.broken) && (__instance.grounded || __instance.currentClimbable || (float)AccessTools.Field(typeof(Body), "timeSinceGrounded").GetValue(__instance) <= 0.11f || __instance.bodyAffect.wasWater) && __instance.standing && (float)AccessTools.Field(typeof(Body), "jumpCooldown").GetValue(__instance) <= 0f && !__instance.forceWalk && (bool)AccessTools.Field(typeof(Body), "movingAllowed").GetValue(__instance))
                    {
                        legLimb.shrapnel = Mathf.Clamp(legLimb.shrapnel + 3, 3, 5);
                        legLimb.muscleHealth -= 6;
                        legLimb.bleedAmount += 10;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Body), "Eat")]
        [HarmonyPrefix]
        public static void Body_Eat_Prefix(Body __instance)
        {
            if (modifiers.Contains(modifierOptions["GlassBones"]))
            {
                if ((__instance.limbs[0].dislocated || __instance.limbs[0].broken) && UnityEngine.Random.Range(0, 4) == 0)
                {
                    __instance.limbs[0].shrapnel = Mathf.Clamp(__instance.limbs[0].shrapnel + 1, 1, 5);
                    __instance.limbs[0].muscleHealth -= 3;
                    __instance.limbs[0].bleedAmount += 7;
                }
            }
        }

        [HarmonyPatch(typeof(DislocationMinigame), "CheckForHit")]
        [HarmonyPrefix]
        public static void DislocationMinigame_CheckForHit_Prefix(List<RaycastResult> uiCasts, DislocationMinigame __instance)
        {
            if (modifiers.Contains(modifierOptions["GlassBones"]) && Minigame.game.handClicking)
            {
                foreach (RaycastResult raycastResult in uiCasts)
                {
                    RectTransform bone = (RectTransform)(AccessTools.Field(typeof(DislocationMinigame), "bone")).GetValue(__instance);
                    if (raycastResult.gameObject == bone.gameObject)
                    {
                        if (!__instance.hasWrench && UnityEngine.Random.Range(0, 4) == 0)
                        {
                            Limb limb = (Limb)(AccessTools.Field(typeof(DislocationMinigame), "limb")).GetValue(__instance);
                            limb.shrapnel = Mathf.Clamp(limb.shrapnel + 2, 2, 5);
                            limb.muscleHealth -= 6;
                            limb.bleedAmount += 10;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Limb), "OnCollisionEnter2D")]
        [HarmonyPrefix]
        public static bool Limb_OnCollisionEnter2D_Prefix(Collision2D col, Limb __instance)
        {
            if (modifiers.Contains(modifierOptions["GlassBones"]))
            {
                float force = Mathf.Abs(col.relativeVelocity.magnitude * 1.3f * Mathf.Abs(Vector3.Dot(col.relativeVelocity.normalized, col.GetContact(0).normal)));
                __instance.ImpactDamage(force * 1.7f, __instance.body.lastTimeStepVelocity.normalized);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(TraderScript), "MeetPlayer")]
        [HarmonyPostfix]
        public static void TraderScript_MeetPlayer_Postfix(TraderScript __instance)
        {
            if (modifiers.Contains(modifierOptions["HostileTraders"]))
                __instance.reputation = 0f;
        }

        [HarmonyPatch(typeof(GlobalDark), "get_distortionAmount")]
        [HarmonyPrefix]
        public static bool GlobalDark_get__distortionAmount_Prefix_v5p5(ref float __result)
        {
            if (modifiers.Contains(modifierOptions["Dyslexia"]))
            {
                var pc = PlayerCamera.main;
                if (pc != null && pc.body != null)
                {
                    __result = 1f - PlayerCamera.main.body.brainHealth * 0.01f * 0.8f;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(ElderThornbackBehaviour), "Update")]
        [HarmonyPostfix]
        public static void ElderThornbackBehaviour_Update_Postfix(ElderThornbackBehaviour __instance)
        {
            if (thornbackObj == null || __instance.gameObject.transform != thornbackObj.transform)
                return;

            thornbackMe.target = body.transform.position;
            thornbackMe.biteCoolToSet = 0;
            thornbackMe.moveForce = 25000;
            thornbackMe.retreatMoveTime = 0;
        }
    }
}