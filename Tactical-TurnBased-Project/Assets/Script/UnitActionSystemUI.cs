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
        private GUIStyle subheaderStyle;
        private GUIStyle hintStyle;
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

            // Panel Box Style
            panelStyle = new GUIStyle(GUI.skin.box);
            Texture2D panelTex = new Texture2D(1, 1);
            panelTex.SetPixel(0, 0, new Color(0.06f, 0.08f, 0.11f, 0.94f));
            panelTex.Apply();
            panelStyle.normal.background = panelTex;

            // Normal Button Style
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.normal.textColor = new Color(0.85f, 0.88f, 0.92f, 1f);
            buttonStyle.hover.textColor = Color.white;
            Texture2D btnTex = new Texture2D(1, 1);
            btnTex.SetPixel(0, 0, new Color(0.14f, 0.18f, 0.24f, 0.95f));
            btnTex.Apply();
            buttonStyle.normal.background = btnTex;

            // Active / Focused Button Style (W/S selection)
            selectedButtonStyle = new GUIStyle(buttonStyle);
            selectedButtonStyle.normal.textColor = new Color(0.2f, 1f, 0.45f, 1f);
            Texture2D activeBtnTex = new Texture2D(1, 1);
            activeBtnTex.SetPixel(0, 0, new Color(0.10f, 0.38f, 0.22f, 0.98f));
            activeBtnTex.Apply();
            selectedButtonStyle.normal.background = activeBtnTex;

            // Header Style
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = new Color(0.3f, 0.9f, 1f, 1f);

            // Subheader Style
            subheaderStyle = new GUIStyle(GUI.skin.label);
            subheaderStyle.fontSize = 11;
            subheaderStyle.alignment = TextAnchor.MiddleCenter;
            subheaderStyle.normal.textColor = new Color(0.7f, 0.75f, 0.8f, 1f);

            // Hint Style (Target Selection mode)
            hintStyle = new GUIStyle(GUI.skin.box);
            hintStyle.fontSize = 13;
            hintStyle.fontStyle = FontStyle.Bold;
            hintStyle.alignment = TextAnchor.MiddleCenter;
            hintStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            Texture2D hintTex = new Texture2D(1, 1);
            hintTex.SetPixel(0, 0, new Color(0.08f, 0.12f, 0.18f, 0.85f));
            hintTex.Apply();
            hintStyle.normal.background = hintTex;

            // Busy Style
            busyStyle = new GUIStyle(GUI.skin.label);
            busyStyle.fontSize = 14;
            busyStyle.fontStyle = FontStyle.Italic;
            busyStyle.alignment = TextAnchor.MiddleCenter;
            busyStyle.normal.textColor = new Color(1f, 0.8f, 0.2f, 1f);

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (UnitActionSystem.Instance == null) return;

            InitStyles();

            UnitActionSystem.ActionFlowState flowState = UnitActionSystem.Instance.GetFlowState();

            // 1. Action Menu Selection State: Render Vertical Action Choice Menu
            if (flowState == UnitActionSystem.ActionFlowState.ActionMenuSelection)
            {
                DrawActionMenu();
                return;
            }

            // 2. Target Grid Selection State: Render subtle guide hint
            if (flowState == UnitActionSystem.ActionFlowState.TargetGridSelection)
            {
                DrawTargetSelectionHint();
                return;
            }

            // 3. Action Executing State: Render In-Progress indicator
            if (flowState == UnitActionSystem.ActionFlowState.ActionExecuting || UnitActionSystem.Instance.IsBusy())
            {
                DrawExecutingIndicator();
                return;
            }

            // 4. GridNavigation: UI is completely hidden (blank/default)
        }

        private void DrawActionMenu()
        {
            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            if (selectedUnit == null) return;

            BaseAction[] actions = selectedUnit.GetBaseActionArray();
            if (actions == null || actions.Length == 0) return;

            int focusedIndex = UnitActionSystem.Instance.GetSelectedMenuActionIndex();

            float panelWidth = 260f;
            float buttonHeight = 36f;
            float buttonSpacing = 8f;
            float headerHeight = 46f;
            float footerHeight = 26f;
            float panelHeight = headerHeight + actions.Length * (buttonHeight + buttonSpacing) + footerHeight;

            float panelX = (Screen.width - panelWidth) * 0.5f;
            float panelY = Screen.height - panelHeight - 30f;

            // Background Panel
            GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), GUIContent.none, panelStyle);

            // Header: Unit Name
            GUI.Label(new Rect(panelX, panelY + 6f, panelWidth, 22f), $"Unit: {selectedUnit.name}", headerStyle);
            GUI.Label(new Rect(panelX, panelY + 26f, panelWidth, 18f), "CHOOSE ACTION", subheaderStyle);

            // Action Choice Buttons (Vertical List)
            float startY = panelY + headerHeight;
            float btnWidth = panelWidth - 24f;
            float btnX = panelX + 12f;

            for (int i = 0; i < actions.Length; i++)
            {
                BaseAction action = actions[i];
                string actionName = action.GetActionName().ToUpper();
                bool isFocused = (i == focusedIndex);

                string label = isFocused ? $"▶  {i + 1}. {actionName}  ◀" : $"{i + 1}. {actionName}";
                GUIStyle style = isFocused ? selectedButtonStyle : buttonStyle;

                Rect btnRect = new Rect(btnX, startY + i * (buttonHeight + buttonSpacing), btnWidth, buttonHeight);

                if (GUI.Button(btnRect, label, style))
                {
                    UnitActionSystem.Instance.SetSelectedMenuActionIndex(i);
                    UnitActionSystem.Instance.ConfirmMenuSelection();
                }
            }

            // Footer Guide
            float footerY = startY + actions.Length * (buttonHeight + buttonSpacing);
            GUI.Label(new Rect(panelX, footerY + 2f, panelWidth, 20f), "W / S : Navigate   |   Enter : Select   |   Esc : Cancel", subheaderStyle);
        }

        private void DrawTargetSelectionHint()
        {
            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
            if (selectedUnit == null || selectedAction == null) return;

            float width = 420f;
            float height = 36f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - height - 20f;

            string hint = $"[ {selectedAction.GetActionName().ToUpper()} ] Use WASD to pick destination tile, then press Enter  (Esc: Cancel)";
            GUI.Box(new Rect(x, y, width, height), hint, hintStyle);
        }

        private void DrawExecutingIndicator()
        {
            float width = 260f;
            float height = 40f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - height - 20f;

            GUI.Box(new Rect(x, y, width, height), GUIContent.none, panelStyle);
            GUI.Label(new Rect(x, y + 8f, width, 24f), "Executing Action...", busyStyle);
        }
    }
}
