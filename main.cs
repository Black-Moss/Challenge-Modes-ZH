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
            ["10NeuralBoosters"] = new ModifierInfo() { name = "神经增强剂", description = "将你的属性提高10倍" },
            ["1mSoundCannon"] = new ModifierInfo() { name = "一分一个音波炮", description = "每分钟生成一个音波炮" },
            ["Ablutophobia"] = new ModifierInfo() { name = "反洁", description = "一直100%污垢" },
            ["Armless"] = new ModifierInfo() { name = "无臂", description = "你疯了吗" },
            ["Blind"] = new ModifierInfo() { name = "失明", description = "你和上面那个一样" },
            ["Cancer"] = new ModifierInfo() { name = "辐射源", description = "辐射持续增长" },
            ["ConstantDepression"] = new ModifierInfo() { name = "重度抑郁", description = "情绪一直为0" },
            ["ConstantEarthquakes"] = new ModifierInfo() { name = "重度地震", description = "持续地震除非25能量以下", author = "3_femtanyl_3" },
            ["ConstantSepsis"] = new ModifierInfo() { name = "持续败血症", description = "总会至少感染一种" },
            ["Deaf"] = new ModifierInfo() { name = "你尔多隆吗", description = "听不见，根本听不见" },
            ["Dyslexia"] = new ModifierInfo() { name = "阅读障碍", description = "啥啥啥，这写的是啥" },
            ["Flimsy"] = new ModifierInfo() { name = "软弱无能", description = "就像牛奶一样" },
            ["GlassBones"] = new ModifierInfo() { name = "脆骨症", description = "更强的坠落伤害与碎片" },
            ["GlassCannon"] = new ModifierInfo() { name = "玻璃炮", description = "17力量，但0韧性" },
            ["Hollowed"] = new ModifierInfo() { name = "空心灯", description = "感觉身体被清空" },
            ["HostileTraders"] = new ModifierInfo() { name = "不受欢迎", description = "所有商人都会把你赶出去" },
            ["ImmediateRadiationLine"] = new ModifierInfo() { name = "急速辐射线", description = "辐射线马上移动" },
            ["ManySalads"] = new ModifierInfo() { name = "一堆长老", description = "每层生成20-100只脊背兽长老" },
            ["Narcolepsy"] = new ModifierInfo() { name = "嗜睡", description = "任何时候都会睡...着..." },
            ["RandomActivation"] = new ModifierInfo() { name = "随机修改器", description = "随机启动修改器", author = "pablo.gonzalez.2009" },
            ["RandomExplode"] = new ModifierInfo() { name = "随机爆炸", description = "每10秒都有1/480概率爆炸" },
            ["SuperSalad"] = new ModifierInfo() { name = "超级长老", description = "一只超强的脊背兽长老会刷新到你所在的每一层", author = "3_femtanyl_3" },
            //["UntemperedGlass"] = new ModifierInfo() { name = "全身不耐受", description = "所有伤害强10倍", author = "3_femtanyl_3" },
            ["ThousandsOfThem"] = new ModifierInfo() { name = "成千上万", description = "每隔几分钟，你就会被洞穴蜱虫攻击", author = "3_femtanyl_3" },

            ["Bean"] = new ModifierInfo() { name = "人棍", description = "你没有下巴，没有四肢，也听不见任何东西，你被掏空了/洗脑了", author = "milky.50262" },
            //["Chernobyl"] = new ModifierInfo() { name = "切尔诺贝利", description = "先出现放射病。如果不采取措施，血胸会杀死你", author = "milky.50262" },
            ["ShellShocked"] = new ModifierInfo() { name = "精神创伤", description = "从100创伤开始", author = "milky.50262" },
            ["Halfed"] = new ModifierInfo() { name = "半条命", description = "你只有一条胳膊、一条腿和一只眼睛", author = "milky.50262" },
            ["MyToes"] = new ModifierInfo() { name = "我脚！", description = "双脚骨折、脑组织完整度受损80%、濒临窒息", author = "milky.50262" },
            ["ThisExpieDeservesWorse"] = new ModifierInfo() { name = "还能更糟", description = "无芯片、没有下巴、大脑受损、受到60伤害、手骨折、不吃植物。从止痛药开始", author = "milky.50262" },
            ["NoStats"] = new ModifierInfo() { name = "无属性", description = "你的所有属性（智力、抗性和力量）都是0，升级需要很长时间", author = "milky.50262" },
            ["Scourging"] = new ModifierInfo() { name = "大挖特挖", description = "摧毁/消灭尽可能多的生物/陷阱/植物。仅限手或陷阱，25力量", author = "milky.50262" },
            ["TheyDrilledHowFar"] = new ModifierInfo() { name = "它们钻了多深？", description = "从第5层开始，尽可能多地消灭脊背兽长老，并尽量存活更久。15力量/耐力、14智力", author = "milky.50262", goals = new string[] { "depth", "kills" } },

            ["BloodBath"] = new ModifierInfo() { name = "浴血奋战", description = "通过第5层才能阻止你的流血，自带慢点等离子切割器", author = "milky.50262", goals = new string[] { "custom", "layer" } },
            ["NoTracesLeftBehind"] = new ModifierInfo() { name = "不装饰你的梦", description = "你不能破坏方块（坠落、敌人、爆炸物等造成的方块破坏不算在内），你不能打开任何箱子，你只能通过破坏植物来获取食物，你不能进行交易。你只能击杀敌人和商人，而且只能击杀Experiment", author = "milky.50262", goals = new string[] { "custom", "layer" } },
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

            logger.LogInfo("Awake()运行——挑战模式运行中！");

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
                    Log("字体文件哈希不一样，正在覆盖");
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
                    logger.LogError($"游戏版本不是{activeVersion}，正在推出模组");
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
                        throw new Exception($"补丁方法命名不正确\n请确保补丁方法的命名符合以下模式：\n\t目标类_目标方法_补丁类型[_版本]");

                    if (splitName.Length >= 4)
                        if (splitName[3] != activeVersion)
                        {
                            Log($"{patch.Name}不支持{activeVersion}版本");
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
                            throw new Exception($"未知补丁方法\n补丁方法类型 \"{targetMethodType}\" 当前没有处理");
                    }

                    List<string> validPatchTypes = new List<string>
                    {
                        "Prefix",
                        "Postfix",
                        "Transpiler"
                    };
                    if (ogScript == null || patchScript == null || !validPatchTypes.Contains(patchType))
                    {
                        throw new Exception($"补丁方法命名不正确\n请确保补丁方法的命名符合以下模式：\n\t目标类_目标方法_补丁类型[_版本]");
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
                    logger.LogError($"{patch.Name} 补丁失败");
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
            dropdownToggleTMP.text = "困难模式修改器";
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

                    case "一分一个音波炮":
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

                    case "反洁":
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
                        case "空心灯":
                            skipMindWipeStart = true;
                            if (!body.GetComponent<MindwipeScript>())
                            {
                                body.gameObject.AddComponent<MindwipeScript>();
                                body.shock += 10f;
                                ChallengeModes.Instance.StartCoroutine(CustomMindwipeSequence(body));
                            }
                            break;

                        case "软弱无能":
                            body.skills.STR = 0;
                            body.skills.expSTR = 0;
                            body.skills.UpdateExpBoundaries();
                            break;

                        case "无臂":
                            body.LimbByName("UpArmF").Dismember();
                            body.LimbByName("UpArmB").Dismember();
                            body.LimbByName("UpTorso").skinHealth = 100f;
                            body.LimbByName("UpTorso").muscleHealth = 100f;
                            body.LimbByName("UpTorso").bleedAmount = 0f;
                            body.LimbByName("UpTorso").pain = 0f;
                            break;

                        case "失明":
                            body.RemoveEye();
                            body.RemoveEye();
                            body.limbs[0].pain = 0f;
                            body.limbs[0].bleedAmount = 0f;
                            break;

                        case "神经增强剂":
                            body.usedNeuralBooster = true;
                            for (int i = 0; i < 10; i++)
                            {
                                body.maxSpeed *= 1.25f;
                                body.moveForce *= 1.25f;
                                body.jumpSpeed *= 1.2f;
                                body.caffeinated += 600f;
                            }
                            break;

                        case "玻璃炮":
                            if (!modifiers.Contains(modifierOptions["Flimsy"]))
                            {
                                body.skills.STR = 17;
                                body.skills.expSTR = 2073;
                            }
                            body.skills.RES = 0;
                            body.skills.expRES = 0;
                            body.skills.UpdateExpBoundaries();
                            break;

                        case "人棍":
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

                        case "精神创伤":
                            body.traumaAmount = 100f;
                            break;

                        case "半条命":
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

                        case "我脚！":
                            body.weightOffset = 60f;
                            body.LimbByName("FootF").broken = true;
                            body.LimbByName("FootB").broken = true;
                            body.brainHealth = 80f;
                            break;

                        case "还能更糟":
                            WorldGeneration.world.unchippedMode = true;
                            body.disfigured = true;
                            body.brainHealth = 65f;
                            body.traumaAmount = 60f;
                            body.LimbByName("HandF").broken = true;
                            body.LimbByName("HandB").broken = true;
                            body.AutoPickUpItem(UnityEngine.Object.Instantiate(Resources.Load<GameObject>("painkillers"), body.transform.position, Quaternion.identity).GetComponent<Item>());
                            break;

                        case "无属性":
                            body.skills.STR = 0;
                            body.skills.RES = 0;
                            body.skills.INT = 0;
                            body.skills.UpdateExpBoundaries();
                            break;
                    }
                }
                switch (modifier.name)
                {
                    case "你尔多隆吗":
                        body.hearingLoss = int.MaxValue;
                        AudioListener.volume = 0f;
                        break;

                    case "一堆长老":
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
                            Log("脊背兽长老生成在：" + coords.x + " " + coords.y);
                            UnityEngine.Object.Instantiate(Resources.Load("thornbackelder"), coords, Quaternion.identity);
                        }
                        Log("脊背兽长老生成了" + coordArray.Count + " / " + randLoopCount);
                        break;

                    case "急速辐射线":
                        RadiationLine.line.active = true;
                        break;

                    case "超级长老":
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
            Log("补丁已完成，开始挑战");
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