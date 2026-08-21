using System;
using UnityEngine;

namespace kelsgaming.site
{
    public class UnitActionSystemUI : MonoBehaviour
    {
        private static UnitActionSystemUI instance;
        public static UnitActionSystemUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<UnitActionSystemUI>();
                    if (instance == null)
                    {
                        GameObject uiGameObject = new GameObject("UnitActionSystemUI");
                        instance = uiGameObject.AddComponent<UnitActionSystemUI>();
                    }
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        private GUIStyle panelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle selectedButtonStyle;
        private GUIStyle headerStyle;
        private GUIStyle busyStyle;
        private bool stylesInitialized;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            // Panel Background Style
            panelStyle = new GUIStyle(GUI.skin.box);
            Texture2D panelTex = new Texture2D(1, 1);
            panelTex.SetPixel(0, 0, new Color(0.08f, 0.1f, 0.12f, 0.88f));
            panelTex.Apply();
            panelStyle.normal.background = panelTex;

            // Normal Button Style
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            buttonStyle.hover.textColor = Color.white;
            Texture2D btnTex = new Texture2D(1, 1);
            btnTex.SetPixel(0, 0, new Color(0.18f, 0.22f, 0.28f, 0.9f));
            btnTex.Apply();
            buttonStyle.normal.background = btnTex;

            // Selected/Active Button Style
            selectedButtonStyle = new GUIStyle(buttonStyle);
            selectedButtonStyle.normal.textColor = new Color(0.2f, 1f, 0.4f, 1f);
            Texture2D activeBtnTex = new Texture2D(1, 1);
            activeBtnTex.SetPixel(0, 0, new Color(0.12f, 0.35f, 0.2f, 0.95f));
            activeBtnTex.Apply();
            selectedButtonStyle.normal.background = activeBtnTex;

            // Header Style
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 15;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = new Color(0.3f, 0.9f, 1f, 1f);

            // Busy Style
            busyStyle = new GUIStyle(GUI.skin.label);
            busyStyle.fontSize = 13;
            busyStyle.fontStyle = FontStyle.Italic;
            busyStyle.alignment = TextAnchor.MiddleCenter;
            busyStyle.normal.textColor = new Color(1f, 0.8f, 0.2f, 1f);

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (UnitActionSystem.Instance == null) return;

            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            if (selectedUnit == null)
            {
                // No unit selected -> Show nothing
                return;
            }

            InitStyles();

            BaseAction[] baseActionArray = selectedUnit.GetBaseActionArray();
            if (baseActionArray == null || baseActionArray.Length == 0) return;

            BaseAction currentSelectedAction = UnitActionSystem.Instance.GetSelectedAction();
            bool isBusy = UnitActionSystem.Instance.IsBusy();

            float buttonWidth = 110f;
            float buttonHeight = 38f;
            float spacing = 10f;
            int buttonCount = baseActionArray.Length;

            float panelWidth = Mathf.Max(260f, buttonCount * (buttonWidth + spacing) + spacing);
            float panelHeight = 85f;
            float panelX = (Screen.width - panelWidth) * 0.5f;
            float panelY = Screen.height - panelHeight - 20f;

            // Draw Background HUD Box
            GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), GUIContent.none, panelStyle);

            // Draw Header
            string headerText = $"Unit: {selectedUnit.name}";
            GUI.Label(new Rect(panelX, panelY + 6f, panelWidth, 22f), headerText, headerStyle);

            if (isBusy)
            {
                string actionName = currentSelectedAction != null ? currentSelectedAction.GetActionName() : "Action";
                GUI.Label(new Rect(panelX, panelY + 34f, panelWidth, 30f), $"Performing {actionName}...", busyStyle);
                return;
            }

            // Draw Action Buttons
            float startX = panelX + (panelWidth - (buttonCount * buttonWidth + (buttonCount - 1) * spacing)) * 0.5f;
            float btnY = panelY + 34f;

            for (int i = 0; i < buttonCount; i++)
            {
                BaseAction baseAction = baseActionArray[i];
                string actionName = baseAction.GetActionName();
                int shortcutKey = i + 1;
                string buttonLabel = $"[{shortcutKey}] {actionName.ToUpper()}";

                bool isCurrent = baseAction == currentSelectedAction;
                GUIStyle styleToUse = isCurrent ? selectedButtonStyle : buttonStyle;

                Rect buttonRect = new Rect(startX + i * (buttonWidth + spacing), btnY, buttonWidth, buttonHeight);

                if (GUI.Button(buttonRect, buttonLabel, styleToUse))
                {
                    ExecuteActionButton(baseAction, selectedUnit);
                }
            }
        }

        private void ExecuteActionButton(BaseAction baseAction, Unit selectedUnit)
        {
            if (baseAction is MoveAction moveAction)
            {
                UnitActionSystem.Instance.SetSelectedAction(moveAction);
                Debug.Log($"[Action UI] Selected MOVE action. Use WASD to pick cell and press Enter.");
            }
            else if (baseAction is SpinAction spinAction)
            {
                UnitActionSystem.Instance.SetSelectedAction(spinAction);
                UnitActionSystem.Instance.SetBusy();
                spinAction.TakeAction(selectedUnit.GetGridPosition(), () =>
                {
                    if (UnitActionSystem.Instance != null)
                    {
                        UnitActionSystem.Instance.ClearBusy();
                    }
                });
                Debug.Log($"[Action UI] Triggered SPIN action on {selectedUnit.name}.");
            }
            else
            {
                UnitActionSystem.Instance.SetSelectedAction(baseAction);
            }
        }
    }
}
