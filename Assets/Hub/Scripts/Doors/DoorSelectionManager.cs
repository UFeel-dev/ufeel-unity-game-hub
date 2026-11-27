using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GameHub.Common;
using UnityEngine;

namespace GameHub.Hub
{
    public class DoorSelectionManager : MonoBehaviour
    {
        private const int MaxDigits = 3;
        private const string CategorySeparator = " - ";
        private const string OtherCategory = "Other";
        private const string RootTitle = "Rooms";
        private const string BackLabel = "Back";
        private const string InProgressCaption = "In progress";
        private const string RoomSeparator = "  ·  ";

        [SerializeField] private CarouselRotator carouselRotator;
        [SerializeField] private Transform doorHoldersParent;
        [SerializeField] private Transform? stairsRoot;

        private readonly List<DoorCategory> categories = new();
        private readonly List<MenuEntry> entries = new();
        private readonly PointerInput mouse = new();
        private readonly GazeMenuInput gaze = new();

        private DoorPicker picker;
        private HubStairs? stairs;
        private CharacterController? rider;
        private DoorCategory? openCategory;
        private HubAtmosphere? atmosphere;
        private DoorChainController? loweredChain;
        private Coroutine? lowering;
        private string typed = string.Empty;
        private string? notice;
        private bool eyeControl;
        private DoorTrigger? labelledTrigger;
        private string? enterLabel;

        private static (string Category, string Room) SplitName(string doorName)
        {
            int separator = doorName.IndexOf(CategorySeparator, StringComparison.Ordinal);
            return separator < 0
                ? (OtherCategory, doorName)
                : (doorName[..separator], doorName[(separator + CategorySeparator.Length)..]);
        }

        private static bool IsInProgress(DoorIdentifier door)
        {
            return door.TryGetComponent(out DoorReferences references) && references.Trigger != null && references.Trigger.IsInProgress;
        }

        private static string? EnterLabel(DoorTrigger? trigger)
        {
            if (trigger == null || trigger.IsInProgress)
                return null;

            DoorIdentifier? door = trigger.GetComponentInParent<DoorIdentifier>();
            return door != null && !string.IsNullOrEmpty(door.DoorName) ? $"Enter {SplitName(door.DoorName).Room}" : "Enter";
        }

        private void Awake()
        {
            BuildCategories();
            int rows = categories.Count;
            foreach (DoorCategory category in categories)
                rows = Math.Max(rows, category.Doors.Count + 1);

            picker = new DoorPicker(UiKit.Shared, transform, rows);
            ShowRoot(0);
            TryGetComponent(out atmosphere);
            if (stairsRoot != null)
                stairs = new HubStairs(stairsRoot);
        }

        private void Update()
        {
            stairs?.Tick(Time.deltaTime, Rider());
            bool visible = SceneLoader.IsHubActive;
            picker.SetVisible(visible);
            if (!visible)
                return;

            float deltaTime = Time.unscaledDeltaTime;
            mouse.Tick();
            bool usingMenu = Cursor.lockState != CursorLockMode.Locked;
            picker.List.UseVerticalKeys = usingMenu;
            picker.Tick(deltaTime, usingMenu || eyeControl);
            picker.SetHints(usingMenu, openCategory != null, eyeControl, CurrentEnterLabel());
            picker.SetGaze(eyeControl ? gaze : null);

            if (!UiFocus.IsFree)
                return;

            if (LayoutInput.GetKeyDown(KeyCode.N))
                ToggleEyeControl();

            HandleDigits();
            if (eyeControl)
                HandleGaze(deltaTime);

            if (usingMenu)
                HandleMenu(deltaTime);
            else if (typed.Length > 0 && MenuKeys.Confirm())
                GoToTyped();
            else if (typed.Length > 0 && MenuKeys.Back())
                SetTyped(string.Empty);
        }

        private CharacterController? Rider()
        {
            if (rider == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
                if (player != null)
                    player.TryGetComponent(out rider);
            }

            return rider;
        }

        private string? CurrentEnterLabel()
        {
            if (DoorTrigger.InRange != labelledTrigger)
            {
                labelledTrigger = DoorTrigger.InRange;
                enterLabel = EnterLabel(labelledTrigger);
            }

            return enterLabel;
        }

        private void ToggleEyeControl()
        {
            eyeControl = !eyeControl;
            gaze.Reset();
        }

        private void HandleGaze(float deltaTime)
        {
            switch (gaze.Tick(deltaTime))
            {
                case GazeAction.Previous:
                    picker.List.Step(-1);
                    break;
                case GazeAction.Next:
                    picker.List.Step(1);
                    break;
                case GazeAction.Confirm:
                    ConfirmSelection();
                    break;
            }
        }

        private void HandleMenu(float deltaTime)
        {
            switch (picker.List.HandleInput(deltaTime, mouse))
            {
                case MenuAction.Moved:
                    atmosphere?.Audio.MenuMove();
                    break;
                case MenuAction.Confirmed when typed.Length > 0:
                    atmosphere?.Audio.MenuConfirm();
                    GoToTyped();
                    break;
                case MenuAction.Confirmed:
                case MenuAction.Increase when openCategory == null:
                    atmosphere?.Audio.MenuConfirm();
                    ConfirmSelection();
                    break;
                case MenuAction.Back when typed.Length > 0:
                    atmosphere?.Audio.MenuBack();
                    SetTyped(string.Empty);
                    break;
                case MenuAction.Back:
                case MenuAction.Decrease:
                    atmosphere?.Audio.MenuBack();
                    CloseCategory();
                    break;
            }
        }

        private void ConfirmSelection()
        {
            if (entries.Count == 0)
                return;

            int index = picker.List.Selected;
            if (openCategory == null)
                ShowCategory(categories[index], 0);
            else if (index < openCategory.Doors.Count)
                GoTo(openCategory.Doors[index]);
            else
                CloseCategory();
        }

        private void BuildCategories()
        {
            List<DoorIdentifier> doors = new();
            foreach (Transform child in doorHoldersParent)
            {
                if (child.TryGetComponent(out DoorIdentifier identifier) && !string.IsNullOrEmpty(identifier.DoorName))
                    doors.Add(identifier);
            }

            doors.Sort((left, right) => left.DoorId.CompareTo(right.DoorId));
            foreach (DoorIdentifier door in doors)
            {
                string name = SplitName(door.DoorName).Category;
                DoorCategory? category = categories.Find(existing => existing.Name == name);
                if (category == null)
                {
                    category = new DoorCategory(name);
                    categories.Add(category);
                }

                category.Doors.Add(door);
            }
        }

        private void ShowRoot(int selected)
        {
            openCategory = null;
            entries.Clear();
            foreach (DoorCategory category in categories)
            {
                List<string> rooms = category.Doors.ConvertAll(door => SplitName(door.DoorName).Room);
                entries.Add(new MenuEntry(category.Name, string.Empty, string.Join(RoomSeparator, rooms)));
            }

            picker.Show(RootTitle, entries, selected);
        }

        private void ShowCategory(DoorCategory category, int selected)
        {
            openCategory = category;
            entries.Clear();
            foreach (DoorIdentifier door in category.Doors)
            {
                string caption = IsInProgress(door) ? InProgressCaption : string.Empty;
                entries.Add(new MenuEntry(SplitName(door.DoorName).Room, door.DoorId.ToString(CultureInfo.InvariantCulture), caption));
            }

            entries.Add(new MenuEntry(BackLabel));
            picker.Show(category.Name, entries, selected);
        }

        private void CloseCategory()
        {
            if (openCategory != null)
                ShowRoot(categories.IndexOf(openCategory));
        }

        private void HandleDigits()
        {
            for (int digit = 0; digit <= 9; digit++)
            {
                bool pressed = LayoutInput.GetKeyDown(KeyCode.Alpha0 + digit) || LayoutInput.GetKeyDown(KeyCode.Keypad0 + digit);
                if (pressed && typed.Length < MaxDigits)
                    SetTyped(typed + digit.ToString(CultureInfo.InvariantCulture));
            }

            if (typed.Length > 0 && LayoutInput.GetKeyDown(KeyCode.Backspace))
                SetTyped(typed[..^1]);
        }

        private void SetTyped(string value)
        {
            typed = value;
            notice = null;
            if (int.TryParse(typed, NumberStyles.None, CultureInfo.InvariantCulture, out int id))
                Reveal(id);

            RefreshFooter();
        }

        private void Reveal(int id)
        {
            foreach (DoorCategory category in categories)
            {
                int index = category.Doors.FindIndex(door => door.DoorId == id);
                if (index < 0)
                    continue;

                if (category == openCategory)
                    picker.List.Select(index);
                else
                    ShowCategory(category, index);
                return;
            }
        }

        private void GoToTyped()
        {
            int id = int.Parse(typed, NumberStyles.None, CultureInfo.InvariantCulture);
            typed = string.Empty;
            DoorIdentifier? door = FindDoor(id);
            if (door != null)
                GoTo(door);
            else
                notice = $"<color=#{Theme.Hex(Theme.Danger)}>No door {id}</color>";

            RefreshFooter();
        }

        private void RefreshFooter()
        {
            picker.SetFooter(typed.Length > 0 ? $"Door <b>{typed}</b>" : notice ?? string.Empty);
        }

        private DoorIdentifier? FindDoor(int id)
        {
            foreach (Transform child in doorHoldersParent)
            {
                if (child.TryGetComponent(out DoorIdentifier identifier) && identifier.DoorId == id)
                    return identifier;
            }

            return null;
        }

        private void GoTo(DoorIdentifier door)
        {
            if (lowering != null)
                StopCoroutine(lowering);

            lowering = StartCoroutine(Select(door, door.GetComponentInChildren<DoorChainController>()));
        }

        private IEnumerator Select(DoorIdentifier door, DoorChainController? chain)
        {
            if (loweredChain != null && loweredChain != chain)
            {
                loweredChain.Retract();
                loweredChain = null;
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.UnloadPendingRoom();

                if (stairs != null)
                {
                    stairs.Sink();
                    yield return new WaitUntil(() => stairs.IsHidden);
                }
            }

            carouselRotator.RotateToDoor(door.DoorId);
            yield return new WaitUntil(() => !carouselRotator.IsRotatingToTarget);
            lowering = null;
            if (chain == null)
                yield break;

            chain.Extend();
            loweredChain = chain;
            if (stairs != null)
                stairs.Rise();
        }
    }
}
