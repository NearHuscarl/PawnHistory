using System;
using System.Collections.Generic;
using System.Linq;
using PawnHistory.Source.Helper;
using PawnHistory.Source.Ui;
using RimWorld;
using UnityEngine;
using Verse;
using static PawnHistory.Source.Ui.W;

namespace PawnHistory.Source.PawnTracker.Ui;

public sealed class AddRecordDialog : WidgetWindow
{
    private const float LabelWidth = 92f;
    private const float DescriptionMinHeight = 72f;
    private const float FooterHeight = 36f;
    
    private readonly AddRecordDialogState state;
    private readonly AutocompleteController<Thing> concernAutocomplete = new();
    private readonly Action onRecordCreated;

    public AddRecordDialog(Pawn pawn, Action onRecordCreated)
    {
        state = CreateState(pawn);
        this.onRecordCreated = onRecordCreated;

        optionalTitle = "NH_PH_AddRecord_Title".Translate();
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnAccept = false;
        closeOnCancel = true;
        closeOnClickedOutside = false;
        doCloseX = true;
    }

    public override Vector2 InitialSize => new(600f, 460f);

    protected override Widget Build(UiContext ctx)
    {
        var theme = ctx.Theme;

        return Column(
        [
            Expanded(ScrollView(
                Column([
                    LabeledField(
                        "NH_PH_AddRecord_FieldType".Translate(),
                        Button(state.SelectedDef.LabelCap, OpenDefMenu),
                        LabelWidth),

                    LabeledField(
                        "NH_PH_AddRecord_FieldDescription".Translate(),
                        TextField(
                            value: state.Description,
                            onChange: text => state.Description = text,
                            minHeight: DescriptionMinHeight,
                            multiline: true),
                        LabelWidth),

                    LabeledField(
                        "NH_PH_AddRecord_FieldDate".Translate(),
                        MenuSection(Label(state.Date), 6),
                        LabelWidth),

                    LabeledField(
                        "NH_PH_AddRecord_FieldConcerns".Translate(),
                        Autocomplete(
                            controller: concernAutocomplete,
                            findOptions: FindConcernSuggestions,
                            onSelected: AddConcern,
                            drawOption: (r, c) => ThingTile(c).Draw(ctx, r)),
                        LabelWidth),
                    Padding.Left(BuildConcernChipStrip(ctx), LabelWidth + theme.Gap),

                    LabeledField(
                        "NH_PH_AddRecord_FieldQuest".Translate(),
                        Button(state.SelectedQuest?.name ?? "NH_PH_AddRecord_None".Translate(), OpenQuestMenu),
                        LabelWidth),
                ]))),

            SizedBox(width: float.PositiveInfinity, height: FooterHeight, child: Row(
                [
                    Button("NH_PH_AddRecord_Cancel".Translate(), () => Close()),
                    Button("NH_PH_AddRecord_Create".Translate(), CreateRecord)
                ], crossAxis: StackCrossAxis.Center, mainAxis: StackMainAxis.End)
            )
        ]);
    }
    
    private IEnumerable<Thing> FindConcernSuggestions(string query)
    {
        return AddRecordDialogConcernSearchUtility.FindMatches(Find.CurrentMap, query, state.SelectedConcerns);
    }
    
    private void AddConcern(Thing concern)
    {
        if (concern != null)
            state.SelectedConcerns.Add(concern);
    }

    private void RemoveConcern(Thing concern)
    {
        state.SelectedConcerns.Remove(concern);
        concernAutocomplete.SetQuery(concernAutocomplete.Query, FindConcernSuggestions(concernAutocomplete.Query));
    }

    private Widget BuildConcernChipStrip(UiContext ctx)
    {
        if (state.SelectedConcerns.Count == 0)
            return MenuSection(Label("NH_PH_AddRecord_NoConcerns".Translate()), 4f);

        return MenuSection(
            Wrap(state.SelectedConcerns.Select(c => ActionChip(c, RemoveConcern))),
            ctx.Theme.GapXs);
    }

    private void OpenDefMenu()
    {
        var options = state.Defs
            .Select(def => new FloatMenuOption(def.LabelCap, () => state.SelectedDef = def))
            .ToList();
        Find.WindowStack.Add(new FloatMenu(options));
    }

    private void OpenQuestMenu()
    {
        var noneOption = new FloatMenuOption("NH_PH_AddRecord_None".Translate(), () => state.SelectedQuest = null);
        var questOptions = state.Quests.Select(quest => new FloatMenuOption(quest.name, () => state.SelectedQuest = quest));

        Find.WindowStack.Add(new FloatMenu([noneOption, ..questOptions]));
    }

    private static AddRecordDialogState CreateState(Pawn pawn)
    {
        var defs = AddRecordDialogUtility.LoadHistoryRecordDefs();
        var selectedDef = defs.First(def => def == HistoryRecordDefOf.Custom);
        var quests = AddRecordDialogUtility.LoadQuests();
        var date = DateHelper.GetShortDate(GenTicks.TicksAbs, pawn.GetTileId());

        return new AddRecordDialogState(pawn, defs, quests, selectedDef, date);
    }

    private void CreateRecord()
    {
        var desc = state.Description.Trim();
        if (desc.Length == 0)
        {
            Messages.Message("NH_PH_HistoryCard_EditRejectedEmpty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        var comp = CompHistoryManager.GetComp(state.Pawn);
        var record = new HistoryRecord(
            state.SelectedDef,
            state.Pawn,
            desc,
            state.SelectedConcerns.ToList(),
            quest: state.SelectedQuest)
        {
            pinned = true
        };

        comp.records.Add(record);
        onRecordCreated?.Invoke();
        Close();
    }

    private sealed class AddRecordDialogState(Pawn pawn, List<HistoryRecordDef> defs, List<Quest> quests, HistoryRecordDef selectedDef, string date)
    {
        public Pawn Pawn { get; } = pawn;
        public HashSet<Thing> SelectedConcerns { get; } = [];
        public List<HistoryRecordDef> Defs { get; } = defs;
        public List<Quest> Quests { get; } = quests;
        public HistoryRecordDef SelectedDef { get; set; } = selectedDef;
        public Quest SelectedQuest { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Date { get; } = date;
    }
}
