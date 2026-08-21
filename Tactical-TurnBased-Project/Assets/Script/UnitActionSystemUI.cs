using System;
using System.Collections.Generic;
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
        private GUIStyle enemyPanelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle selectedButtonStyle;
        private GUIStyle disabledButtonStyle;
        private GUIStyle headerStyle;
        private GUIStyle enemyHeaderStyle;
        private GUIStyle subheaderStyle;
        private GUIStyle apBadgeStyle;
        private GUIStyle statsStyle;
        private GUIStyle speedAdvantageStyle;
        private GUIStyle hintStyle;
        private GUIStyle busyStyle;
        private GUIStyle queueHeaderStyle;
        private GUIStyle queueItemStyle;
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

            // Normal Panel Box Style
            panelStyle = new GUIStyle(GUI.skin.box);
            Texture2D panelTex = new Texture2D(1, 1);
            panelTex.SetPixel(0, 0, new Color(0.05f, 0.07f, 0.10f, 0.95f));
            panelTex.Apply();
            panelStyle.normal.background = panelTex;

            // Enemy Panel Box Style
            enemyPanelStyle = new GUIStyle(GUI.skin.box);
            Texture2D enemyPanelTex = new Texture2D(1, 1);
            enemyPanelTex.SetPixel(0, 0, new Color(0.18f, 0.04f, 0.05f, 0.95f));
            enemyPanelTex.Apply();
            enemyPanelStyle.normal.background = enemyPanelTex;

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

            // Disabled Button Style
            disabledButtonStyle = new GUIStyle(buttonStyle);
            disabledButtonStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

            // Header Style
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 15;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = new Color(0.3f, 0.9f, 1f, 1f);

            // Enemy Header Style
            enemyHeaderStyle = new GUIStyle(headerStyle);
            enemyHeaderStyle.normal.textColor = new Color(1f, 0.35f, 0.35f, 1f);

            // Subheader Style
            subheaderStyle = new GUIStyle(GUI.skin.label);
            subheaderStyle.fontSize = 11;
            subheaderStyle.alignment = TextAnchor.MiddleCenter;
            subheaderStyle.normal.textColor = new Color(0.7f, 0.75f, 0.8f, 1f);

            // Stats Style
            statsStyle = new GUIStyle(GUI.skin.label);
            statsStyle.fontSize = 12;
            statsStyle.fontStyle = FontStyle.Bold;
            statsStyle.alignment = TextAnchor.MiddleCenter;
            statsStyle.normal.textColor = new Color(0.9f, 0.95f, 1f, 1f);

            // AP Badge Style
            apBadgeStyle = new GUIStyle(GUI.skin.label);
            apBadgeStyle.fontSize = 12;
            apBadgeStyle.fontStyle = FontStyle.Bold;
            apBadgeStyle.alignment = TextAnchor.MiddleCenter;
            apBadgeStyle.normal.textColor = new Color(1f, 0.88f, 0.2f, 1f);

            // Speed Advantage Banner Style
            speedAdvantageStyle = new GUIStyle(GUI.skin.label);
            speedAdvantageStyle.fontSize = 12;
            speedAdvantageStyle.fontStyle = FontStyle.Bold;
            speedAdvantageStyle.alignment = TextAnchor.MiddleCenter;
            speedAdvantageStyle.normal.textColor = new Color(0.2f, 1f, 0.5f, 1f);

            // Hint Style (Target Selection mode)
            hintStyle = new GUIStyle(GUI.skin.box);
            hintStyle.fontSize = 13;
            hintStyle.fontStyle = FontStyle.Bold;
            hintStyle.alignment = TextAnchor.MiddleCenter;
            hintStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            Texture2D hintTex = new Texture2D(1, 1);
            hintTex.SetPixel(0, 0, new Color(0.08f, 0.12f, 0.18f, 0.90f));
            hintTex.Apply();
            hintStyle.normal.background = hintTex;

            // Busy Style
            busyStyle = new GUIStyle(GUI.skin.label);
            busyStyle.fontSize = 14;
            busyStyle.fontStyle = FontStyle.Italic;
            busyStyle.alignment = TextAnchor.MiddleCenter;
            busyStyle.normal.textColor = new Color(1f, 0.8f, 0.2f, 1f);

            // Queue Styles
            queueHeaderStyle = new GUIStyle(GUI.skin.label);
            queueHeaderStyle.fontSize = 12;
            queueHeaderStyle.fontStyle = FontStyle.Bold;
            queueHeaderStyle.normal.textColor = new Color(0.3f, 0.85f, 1f, 1f);

            queueItemStyle = new GUIStyle(GUI.skin.label);
            queueItemStyle.fontSize = 11;
            queueItemStyle.normal.textColor = Color.white;

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (UnitActionSystem.Instance == null || TurnSystem.Instance == null) return;

            InitStyles();

            DrawTopTurnBanner();
            DrawTurnOrderQueue();

            Unit activeUnit = TurnSystem.Instance.GetCurrentTurnUnit();

            // If active unit is Enemy, draw Enemy status instead of human menu
            if (activeUnit != null && activeUnit.IsEnemy())
            {
                DrawEnemyTurnIndicator(activeUnit);
                return;
            }

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

            // 4. Grid Navigation State: Render prompt hint to press Enter
            DrawNavigationPrompt();
        }

        private void DrawTopTurnBanner()
        {
            Unit activeUnit = TurnSystem.Instance.GetCurrentTurnUnit();
            if (activeUnit == null) return;

            float bannerWidth = 480f;
            float bannerHeight = 58f;
            float x = (Screen.width - bannerWidth) * 0.5f;
            float y = 12f;

            GUIStyle activePanelStyle = activeUnit.IsEnemy() ? enemyPanelStyle : panelStyle;
            GUIStyle activeHeaderStyle = activeUnit.IsEnemy() ? enemyHeaderStyle : headerStyle;

            GUI.Box(new Rect(x, y, bannerWidth, bannerHeight), GUIContent.none, activePanelStyle);

            string factionTag = activeUnit.IsEnemy() ? "[ENEMY TURN]" : "[PLAYER TURN]";
            string roundText = $"ROUND {TurnSystem.Instance.GetRoundNumber()}  |  {factionTag} {activeUnit.name}";
            string statsText = $"HP: {activeUnit.GetHealth()}/{activeUnit.GetMaxHealth()}  |  STR: {activeUnit.GetStrength()}  |  DEF: {activeUnit.GetDefense()}  |  SPD: {activeUnit.GetSpeed()}";
            string apText = $"Action Points: {activeUnit.GetActionPoints()} / {activeUnit.GetMaxActionPoints()}";

            if (TurnSystem.Instance.HasSpeedAdvantage())
            {
                apText += "  ⚡ DOUBLE MOVE (4 AP)";
            }

            GUI.Label(new Rect(x, y + 3f, bannerWidth, 18f), roundText, activeHeaderStyle);
            GUI.Label(new Rect(x, y + 20f, bannerWidth, 18f), statsText, statsStyle);
            GUI.Label(new Rect(x, y + 37f, bannerWidth, 18f), apText, apBadgeStyle);
        }

        private void DrawTurnOrderQueue()
        {
            List<Unit> sortedUnits = TurnSystem.Instance.GetAllUnitsSortedBySpeed();
            if (sortedUnits == null || sortedUnits.Count == 0) return;

            Unit activeUnit = TurnSystem.Instance.GetCurrentTurnUnit();

            float qWidth = 260f;
            float qHeight = 26f + sortedUnits.Count * 20f;
            float qX = Screen.width - qWidth - 14f;
            float qY = 12f;

            GUI.Box(new Rect(qX, qY, qWidth, qHeight), GUIContent.none, panelStyle);
            GUI.Label(new Rect(qX + 8f, qY + 4f, qWidth - 16f, 18f), "SPEED INITIATIVE QUEUE", queueHeaderStyle);

            for (int i = 0; i < sortedUnits.Count; i++)
            {
                Unit unit = sortedUnits[i];
                if (unit == null) continue;

                bool isCurrent = unit == activeUnit;
                string marker = isCurrent ? "▶ " : (unit.HasActedThisRound() ? "✓ " : "  ");
                string faction = unit.IsEnemy() ? "[E]" : "[P]";
                string hpInfo = $"HP:{unit.GetHealth()}";
                string status = unit.HasActedThisRound() ? "[Done]" : $"{unit.GetActionPoints()} AP";
                string itemText = $"{marker}{faction} {unit.name} ({hpInfo}, Spd:{unit.GetSpeed()}) - {status}";

                Color textColor = Color.white;
                if (isCurrent)
                {
                    textColor = unit.IsEnemy() ? new Color(1f, 0.4f, 0.4f, 1f) : new Color(0.2f, 1f, 0.45f, 1f);
                }
                else if (unit.HasActedThisRound())
                {
                    textColor = Color.gray;
                }
                else
                {
                    textColor = unit.IsEnemy() ? new Color(1f, 0.7f, 0.7f, 0.9f) : new Color(0.7f, 0.9f, 1f, 0.9f);
                }

                queueItemStyle.normal.textColor = textColor;
                GUI.Label(new Rect(qX + 8f, qY + 22f + i * 19f, qWidth - 16f, 18f), itemText, queueItemStyle);
            }
        }

        private void DrawEnemyTurnIndicator(Unit enemyUnit)
        {
            float width = 380f;
            float height = 40f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - height - 20f;

            GUI.Box(new Rect(x, y, width, height), GUIContent.none, enemyPanelStyle);
            GUI.Label(new Rect(x, y + 8f, width, 24f), $"Enemy {enemyUnit.name} is taking action...", busyStyle);
        }

        private void DrawActionMenu()
        {
            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            if (selectedUnit == null || selectedUnit.IsEnemy()) return;

            BaseAction[] actions = selectedUnit.GetBaseActionArray();
            if (actions == null || actions.Length == 0) return;

            int focusedIndex = UnitActionSystem.Instance.GetSelectedMenuActionIndex();

            float panelWidth = 300f;
            float buttonHeight = 38f;
            float buttonSpacing = 8f;
            float headerHeight = 78f;
            float footerHeight = 26f;
            float panelHeight = headerHeight + actions.Length * (buttonHeight + buttonSpacing) + footerHeight;

            float panelX = (Screen.width - panelWidth) * 0.5f;
            float panelY = Screen.height - panelHeight - 30f;

            // Background Panel
            GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), GUIContent.none, panelStyle);

            // Header: Unit Name, HP & Combat Stats
            GUI.Label(new Rect(panelX, panelY + 6f, panelWidth, 20f), $"Unit: {selectedUnit.name} (Speed: {selectedUnit.GetSpeed()})", headerStyle);
            string statsText = $"HP: {selectedUnit.GetHealth()}/{selectedUnit.GetMaxHealth()}  |  STR: {selectedUnit.GetStrength()}  |  DEF: {selectedUnit.GetDefense()}";
            GUI.Label(new Rect(panelX, panelY + 26f, panelWidth, 18f), statsText, statsStyle);

            string apText = $"Action Points: {selectedUnit.GetActionPoints()} / {selectedUnit.GetMaxActionPoints()}";
            GUI.Label(new Rect(panelX, panelY + 44f, panelWidth, 18f), apText, apBadgeStyle);

            if (TurnSystem.Instance.HasSpeedAdvantage())
            {
                GUI.Label(new Rect(panelX, panelY + 60f, panelWidth, 16f), "⚡ SPEED ADVANTAGE ACTIVE (4 AP)", speedAdvantageStyle);
            }

            // Action Choice Buttons (Vertical List)
            float startY = panelY + headerHeight;
            float btnWidth = panelWidth - 24f;
            float btnX = panelX + 12f;

            for (int i = 0; i < actions.Length; i++)
            {
                BaseAction action = actions[i];
                string actionName = action.GetActionName().ToUpper();
                int cost = action.GetActionPointsCost();
                bool canAfford = selectedUnit.CanSpendActionPointsToTakeAction(action);
                bool isFocused = (i == focusedIndex);

                string label = isFocused ? $"▶  {i + 1}. {actionName} ({cost} AP)  ◀" : $"{i + 1}. {actionName} ({cost} AP)";
                GUIStyle style = !canAfford ? disabledButtonStyle : (isFocused ? selectedButtonStyle : buttonStyle);

                Rect btnRect = new Rect(btnX, startY + i * (buttonHeight + buttonSpacing), btnWidth, buttonHeight);

                if (GUI.Button(btnRect, label, style))
                {
                    if (canAfford)
                    {
                        UnitActionSystem.Instance.SetSelectedMenuActionIndex(i);
                        UnitActionSystem.Instance.ConfirmMenuSelection();
                    }
                }
            }

            // Footer Guide
            float footerY = startY + actions.Length * (buttonHeight + buttonSpacing);
            GUI.Label(new Rect(panelX, footerY + 2f, panelWidth, 20f), "W / S : Navigate   |   Enter : Select   |   Esc : Explore", subheaderStyle);
        }

        private void DrawTargetSelectionHint()
        {
            Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
            BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
            if (selectedUnit == null || selectedAction == null) return;

            float width = 500f;
            float height = 38f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - height - 20f;

            string hint;
            if (selectedAction is SpinAction)
            {
                hint = $"[ SPIN ATTACK ] Pick adjacent cardinal tile (Up/Down/Left/Right) & press Enter (Esc: Cancel)";
            }
            else
            {
                hint = $"[ {selectedAction.GetActionName().ToUpper()} ] Use WASD to pick destination tile and press Enter (Esc: Cancel)";
            }

            GUI.Box(new Rect(x, y, width, height), hint, hintStyle);
        }

        private void DrawNavigationPrompt()
        {
            Unit activeUnit = TurnSystem.Instance.GetCurrentTurnUnit();
            if (activeUnit == null || activeUnit.IsEnemy()) return;

            float width = 360f;
            float height = 34f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height - height - 16f;

            string prompt = $"Press [ENTER] to Open Action Menu for {activeUnit.name}";
            GUI.Box(new Rect(x, y, width, height), prompt, hintStyle);
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
