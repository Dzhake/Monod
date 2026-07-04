using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using ImGuiNET;

namespace Friflo.EcGui;

public sealed class QueryExplorer
{
	private readonly ExplorerHeader header;

	internal ExplorerMode mode;

	internal bool embedExpansions;

	internal int freezeColumns;

	internal List<Column> activeColumns;

	internal ArchetypeQuery activeQuery;

	internal string activeQueryName;

	internal bool queryChanged;

	private Entity selectedEntity;

	private int activeColumn;

	private SchemaType? activeColumnType;

	private MemberPath? activeColumnMember;

	internal bool setKeyboardFocus;

	internal bool columnFilter;

	private string? errorMessage;

	private string? errorException;

	private bool setFilterKeyboardFocus;

	private readonly DrawContext context = new DrawContext(syncTables: true);

	private readonly ColumnSelector columnSelector;

	private int nextDeletedWidgetId = -1;

	private ContextMenu contextMenu;

	private object clipboardData = new object();

	private Entity clipboardEntity;

	internal long allocations;

	private readonly ErrorPopup errorPopup = new ErrorPopup();

	private readonly EntityList entities = new EntityList();

	private readonly Dictionary<RawEntity, int> deletedEntities = new Dictionary<RawEntity, int>();

	internal static bool showImGuiDemoWindow;

	internal static bool showStyleEditor;

	public SchemaType? ActiveColumnType => activeColumnType;

	public MemberPath? ActiveColumnMember => activeColumnMember;

	public int EntityCount => entities.Count;

	public IReadOnlyList<Entity> Entities => entities;

	public Entity SelectedEntity => selectedEntity;

	public event Action<Entity>? OnSelectedEntityChange;

	internal void Draw()
	{
		long allocatedBytesForCurrentThread = GC.GetAllocatedBytesForCurrentThread();
		GlobalColors.UpdateStyles(forceUpdate: true);
		Entity entity = selectedEntity;
		ImGui.PushStyleColor(ImGuiCol.PopupBg, GlobalColors.popupBg);
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
		header.DrawExplorerMode();
		header.DrawQuerySelector();
		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 4f);
		ImGui.PopStyleVar();
		float num = ((errorMessage == null) ? 0f : (2f * ImGui.GetFrameHeightWithSpacing()));
		Vector2 size = new Vector2(0f, 0f - num);
		ImGui.BeginChild("table", size, ImGuiChildFlags.NavFlattened);
		DrawTable();
		ImGui.EndChild();
		if (errorMessage != null)
		{
			DrawError(num);
		}
		ImGui.PopStyleColor();
		errorPopup.DrawError();
		if (selectedEntity != entity)
		{
			this.OnSelectedEntityChange?.Invoke(selectedEntity);
		}
		allocations = GC.GetAllocatedBytesForCurrentThread() - allocatedBytesForCurrentThread;
	}

	private void DrawError(float errorHeight)
	{
		Vector2 size = new Vector2(ImGui.GetWindowWidth() - UI.Scl(150f), errorHeight - ImGui.GetStyle().ItemSpacing.Y);
		string input = ((errorException == null) ? errorMessage : (errorMessage + "\n" + errorException));
		ImGui.InputTextMultiline("##error", ref input, 0u, size, ImGuiInputTextFlags.ReadOnly);
		ImGui.SameLine();
		Vector2 cursorPos = ImGui.GetCursorPos();
		if (ImGui.Button("copy"))
		{
			ImGui.SetClipboardText(input);
		}
		ImGui.SetCursorPos(cursorPos + new Vector2(0f, ImGui.GetFrameHeightWithSpacing()));
		if (ImGui.Button("clear"))
		{
			SetError(null);
		}
	}

	private void SetError(string? message, Exception? exception = null)
	{
		errorMessage = message;
		if (exception == null)
		{
			errorException = null;
		}
		else
		{
			errorException = exception.ToString();
		}
	}

	internal void SetColumnFilter(bool enable)
	{
		columnFilter = enable;
		queryChanged = true;
	}

	private void SetFilters(Column column, FieldFilter[] filters, TermOperator termOperator, string? error)
	{
		bool filterOk = error == null;
		column.filterOk = filterOk;
		if (error == null)
		{
			column.SetFilters(filters, termOperator);
			queryChanged = true;
		}
		SetError(error);
	}

	private void DrawColumnFilter()
	{
		int num = -1;
		List<Column> list = activeColumns;
		ImGui.TableNextRow();
		for (int i = 0; i < list.Count; i++)
		{
			if (setFilterKeyboardFocus && i == activeColumn)
			{
				ImGui.SetKeyboardFocusHere();
				setFilterKeyboardFocus = false;
			}
			Column column = list[i];
			ImGui.TableSetColumnIndex(i);
			EcUtils.ID.PushID(i - 100);
			ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
			bool filterOk = column.filterOk;
			bool hasFilter = column.HasFilter;
			if (!filterOk)
			{
				ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.errorText);
			}
			ImGui.PushStyleColor(ImGuiCol.FrameBg, hasFilter ? GlobalColors.activeFilterBg : GlobalColors.filterBg);
			if (ImGui.InputText("##filter", ref column.filterText, 1000u))
			{
				TermOperator termOperator;
				string error;
				FieldFilter[] filters = TableFilter.CreateFieldFilters(column, out termOperator, out error);
				SetFilters(column, filters, termOperator, error);
			}
			if (ImGui.IsItemFocused())
			{
				num = i;
				if (ImGui.IsKeyPressed(ImGuiKey.Escape))
				{
					SetColumnFilter(enable: false);
				}
			}
			ImGui.PopStyleColor();
			if (!filterOk)
			{
				ImGui.PopStyleColor();
			}
			EcUtils.ID.PopID();
		}
		if (ImGui.IsWindowFocused() && num >= 0 && ImGui.IsKeyPressed(ImGuiKey.Delete))
		{
			SetFilters(list[num], Array.Empty<FieldFilter>(), TermOperator.And, null);
			list[num].filterText = "";
		}
	}

	private void UpdateEntities()
	{
		bool flag = queryChanged || mode == ExplorerMode.Live;
		queryChanged = false;
		if (flag)
		{
			activeQuery.Entities.ToEntityList(entities);
			if (columnFilter)
			{
				try
				{
					TableFilter.Filter(entities, activeColumns);
				}
				catch (Exception ex)
				{
					SetError("filter error: " + ex.GetType().Name + ": " + ex.Message, ex);
				}
			}
			deletedEntities.Clear();
			nextDeletedWidgetId = -1;
		}
		ImGuiTableSortSpecsPtr imGuiTableSortSpecsPtr = ImGui.TableGetSortSpecs();
		if ((flag || imGuiTableSortSpecsPtr.SpecsDirty) && imGuiTableSortSpecsPtr.SpecsCount > 0 && imGuiTableSortSpecsPtr.Specs.SortDirection != ImGuiSortDirection.None)
		{
			Column column = activeColumns[imGuiTableSortSpecsPtr.Specs.ColumnIndex];
			try
			{
				ColumnSorter.Sort(column, entities, imGuiTableSortSpecsPtr.Specs.SortDirection);
			}
			catch (Exception ex2)
			{
				SetError($"sort error - colum: {column.Name} - {ex2.GetType().Name}: {ex2.Message}", ex2);
			}
			imGuiTableSortSpecsPtr.SpecsDirty = false;
		}
	}

	private static void DrawContextMenuRect()
	{
		Vector2 vector = ImGui.GetWindowPos() + ImGui.GetCursorPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY()) - new Vector2(2f, 2f);
		Vector2 p_max = vector + new Vector2(ImGui.GetColumnWidth(), ImGui.GetFrameHeight()) + new Vector2(4f, 4f);
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector4 col = ImGui.GetStyle().Colors[0];
		windowDrawList.AddRect(vector, p_max, ImGui.GetColorU32(col), 0f, ImDrawFlags.None, 2f);
	}

	private unsafe static ImGuiListClipperPtr CreateListClipper()
	{
		return new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
	}

	private void DrawTable()
	{
		bool flag = ImGui.IsPopupOpen("ContextMenu");
		Entity entity = selectedEntity;
		List<Column> list = activeColumns;
		Entity entity2 = default(Entity);
		if (!ImGui.BeginTable(activeQueryName, list.Count, ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Sortable | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SortTristate))
		{
			return;
		}
		bool flag2 = ImGui.IsWindowFocused();
		foreach (Column item in list)
		{
			ImGuiTableColumnFlags imGuiTableColumnFlags = ImGuiTableColumnFlags.WidthFixed;
			if (!item.Sortable)
			{
				imGuiTableColumnFlags |= ImGuiTableColumnFlags.NoSort;
			}
			if (item is IdColumn)
			{
				imGuiTableColumnFlags |= ImGuiTableColumnFlags.NoReorder;
			}
			ImGui.TableSetupColumn(init_width_or_weight: UI.Scl(item.GetDefaultWidth(embedExpansions)), label: item.Name, flags: imGuiTableColumnFlags);
		}
		ImGui.TableSetupScrollFreeze(freezeColumns, (!columnFilter) ? 1 : 2);
		ImGui.TableHeadersRow();
		if (columnFilter)
		{
			DrawColumnFilter();
		}
		UpdateEntities();
		bool flag3 = false;
		if (ImGui.BeginPopupContextWindow())
		{
			ImGui.EndPopup();
			flag3 = true;
		}
		ImGuiListClipperPtr imGuiListClipperPtr = CreateListClipper();
		DrawCell drawCell = new DrawCell
		{
			context = context,
			explorer = this,
			drawValueFlags = ((!embedExpansions) ? DrawValueFlags.Value : DrawValueFlags.Expansion)
		};
		context.rect.size.Y = ImGui.GetFrameHeight();
		context.multiLine = false;
		imGuiListClipperPtr.Begin(entities.Count, ImGui.GetFrameHeight() + 2f * ImGui.GetStyle().CellPadding.Y);
		int row = -1;
		bool flag4 = false;
		int num = -1;
		while (imGuiListClipperPtr.Step())
		{
			int displayStart = imGuiListClipperPtr.DisplayStart;
			int displayEnd = imGuiListClipperPtr.DisplayEnd;
			for (int i = displayStart; i < displayEnd; i++)
			{
				Entity entity3 = entities[i];
				int value = entity3.Id * 100;
				if (mode == ExplorerMode.Edit && entity3.IsNull && !deletedEntities.TryGetValue(entity3.RawEntity, out value))
				{
					if (deletedEntities.Count > 10000)
					{
						deletedEntities.Clear();
						nextDeletedWidgetId = 0;
					}
					value = --nextDeletedWidgetId * 100;
					deletedEntities[entity3.RawEntity] = value;
				}
				ImGui.TableNextRow();
				bool flag5 = (drawCell.selected = entity3 == entity);
				bool flag6 = false;
				bool flag7 = false;
				int num2 = 0;
				context.entity = entity3;
				foreach (Column item2 in list)
				{
					value++;
					ImGui.TableSetColumnIndex(num2);
					EcUtils.ID.PushID(value);
					float columnWidth = ImGui.GetColumnWidth();
					ImGui.SetNextItemWidth(columnWidth);
					context.rect.size.X = columnWidth;
					if (flag && entity3 == contextMenu.entity && num2 == contextMenu.columnIndex)
					{
						contextMenu.rowIndex = i;
						DrawContextMenuRect();
					}
					ItemFlags itemFlags = ItemFlags.None;
					if (num2 == 0 || !entity3.IsNull)
					{
						itemFlags = item2.DrawCell(drawCell);
					}
					if ((itemFlags & ItemFlags.Present) == 0)
					{
						ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(0f, 0f - ImGui.GetStyle().FramePadding.Y));
						ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, default(Vector2));
						flag6 |= ImGui.Selectable("##field", flag5, ImGuiSelectableFlags.None, drawCell.Size);
						ImGui.PopStyleVar();
						itemFlags = TypeDrawer.Flags();
					}
					if ((itemFlags & ItemFlags.ContextMenu) != ItemFlags.None)
					{
						flag4 = true;
						contextMenu = new ContextMenu
						{
							explorer = this,
							column = item2,
							entity = entity3,
							rowIndex = i,
							columnIndex = num2
						};
					}
					if ((itemFlags & ItemFlags.Focused) != ItemFlags.None)
					{
						flag7 = true;
						num = num2;
					}
					EcUtils.ID.PopID();
					num2++;
				}
				if (!setKeyboardFocus)
				{
					if (flag6 && !flag5)
					{
						entity2 = entity3;
						row = i;
					}
					if (flag2 && flag7)
					{
						entity2 = entity3;
						row = i;
					}
				}
			}
		}
		ImGui.EndTable();
		selectedEntity = entity2;
		if (flag3 && !flag4)
		{
			int rowIndex = Math.Min(imGuiListClipperPtr.DisplayStart, entities.Count - 1);
			contextMenu = new ContextMenu
			{
				explorer = this,
				column = list[0],
				entity = default(Entity),
				rowIndex = rowIndex,
				columnIndex = 0
			};
			flag4 = true;
		}
		imGuiListClipperPtr.Destroy();
		if (num >= 0)
		{
			activeColumn = num;
		}
		activeColumnType = ((num > 0) ? list[num].SchemaType : null);
		activeColumnMember = ((num > 0) ? list[num].MemberPath : null);
		if (flag2 && !ImGui.IsAnyItemActive())
		{
			KeyboardCell(entity2, num, row);
		}
		KeyboardTable();
		if (flag4)
		{
			ImGui.OpenPopup("ContextMenu");
		}
		if (ImGui.BeginPopup("ContextMenu", ImGuiWindowFlags.None))
		{
			contextMenu.column.ContextMenu(contextMenu);
			ImGui.EndPopup();
		}
	}

	public QueryExplorer()
	{
		header = new ExplorerHeader(this);
		QueryEntry firstQueryEntry = header.FirstQueryEntry;
		activeQuery = firstQueryEntry.query;
		activeColumns = firstQueryEntry.columns;
		context.onEdit = delegate
		{
			mode = ExplorerMode.Edit;
		};
		context.onError = errorPopup.OnError;
		columnSelector = new ColumnSelector(this);
	}

	public void AddStore(string name, EntityStore store)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException("missing query name", "name");
		}
		if (store == null)
		{
			throw new ArgumentNullException("store");
		}
		header.AddStore(name, store);
	}

	public void AddQuery(string name, ArchetypeQuery query)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException("missing query name", "name");
		}
		if (query == null)
		{
			throw new ArgumentNullException("query");
		}
		header.AddQuery(name, query);
	}

	public void AddSystems(SystemRoot system)
	{
		if (system == null)
		{
			throw new ArgumentNullException("system");
		}
		header.AddSystemQueries(system);
	}

	public void AddComponentMemberColumn<TComponent>(string memberPath) where TComponent : struct, IComponent
	{
		MemberDrawer drawer = MemberDrawer.Create(MemberPath.Get(typeof(TComponent), memberPath));
		activeColumns.Add(new FieldColumn(drawer));
	}

	public void AddTagColumn<TTag>() where TTag : ITag
	{
		TagType tagType = EntityStore.GetEntitySchema().TagTypeByType[typeof(TTag)];
		activeColumns.Add(new TagColumn(tagType));
	}

	internal HistoryColumn AddHistoryColumn(MemberPath member)
	{
		HistoryColumn historyColumn = new HistoryColumn(FieldHistories.SubscribeHistory(activeQuery.Store, member), member);
		activeColumns.Add(historyColumn);
		return historyColumn;
	}

	internal void RemoveColumn(Column column)
	{
		List<Column> list = activeColumns;
		foreach (Column item in list)
		{
			if (item == column)
			{
				list.Remove(item);
				break;
			}
		}
	}

	internal void RemoveComponentFieldDrawer(MemberPath memberPath)
	{
		List<Column> list = new List<Column>();
		foreach (Column activeColumn in activeColumns)
		{
			if (activeColumn.MemberPath == memberPath)
			{
				list.Add(activeColumn);
			}
		}
		foreach (Column item in list)
		{
			RemoveColumn(item);
		}
	}

	internal void AddComponentFieldDrawer(MemberDrawer memberDrawer)
	{
		if (memberDrawer.typeDrawer is RelationsDrawer relationsDrawer)
		{
			ComponentType relationType = EntityStore.GetEntitySchema().ComponentTypeByType[relationsDrawer.RelationType];
			activeColumns.Add(new RelationColumn(memberDrawer, relationType));
		}
		else
		{
			activeColumns.Add(new FieldColumn(memberDrawer));
		}
	}

	internal void AddTagColumn(TagType tagType)
	{
		activeColumns.Add(new TagColumn(tagType));
	}

	internal void RemoveTagColumn(TagType tagType)
	{
		List<Column> list = new List<Column>();
		foreach (Column activeColumn in activeColumns)
		{
			if (activeColumn is TagColumn tagColumn && tagColumn.tagType == tagType)
			{
				list.Add(activeColumn);
			}
		}
		foreach (Column item in list)
		{
			RemoveColumn(item);
		}
	}

	internal int GetFilterCount()
	{
		int num = 0;
		foreach (Column activeColumn in activeColumns)
		{
			if (activeColumn.HasFilter)
			{
				num++;
			}
		}
		return num;
	}

	internal void OpenFieldSelector()
	{
		columnSelector.selection.Clear();
		foreach (Column activeColumn in activeColumns)
		{
			if (activeColumn is FieldColumn fieldColumn)
			{
				columnSelector.selection[fieldColumn.memberDrawer.member] = TriState.Checked;
			}
		}
		columnSelector.Start();
		ImGui.OpenPopup("add_fields");
	}

	internal void DrawFieldSelector()
	{
		ImGui.SetNextWindowSize(new Vector2(UI.Scl(700f), UI.Scl(1000f)));
		if (ImGui.BeginPopup("add_fields", ImGuiWindowFlags.None))
		{
			columnSelector.Draw();
			ImGui.EndPopup();
		}
	}

	private void KeyboardTable()
	{
		if (ImGui.IsKeyPressed(ImGuiKey.F5) || (ImGui.IsKeyPressed(ImGuiKey.R) && ImGui.IsKeyDown(ImGuiKey.ModCtrl)))
		{
			mode = ExplorerMode.Edit;
			queryChanged = true;
		}
		if (ImGui.IsKeyPressed(ImGuiKey.F6))
		{
			mode = ExplorerMode.Live;
		}
		if (ImGui.IsKeyPressed(ImGuiKey.F) && ImGui.IsKeyDown(ImGuiKey.ModCtrl))
		{
			SetColumnFilter(enable: true);
			setFilterKeyboardFocus = true;
		}
	}

	private void KeyboardCell(Entity entity, int column, int row)
	{
		column = ((column >= 0) ? column : 0);
		row = ((row >= 0) ? row : 0);
		if (ImGui.IsKeyPressed(ImGuiKey.Insert))
		{
			CellCommand cellCommand = (ImGui.IsKeyDown(ImGuiKey.ModCtrl) ? CellCommand.InsertEmpty : CellCommand.Insert);
			ExecuteCellCommand(cellCommand, entity, column, row);
			return;
		}
		if (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyPressed(ImGuiKey.V))
		{
			ExecuteCellCommand(CellCommand.Paste, entity, column, row);
		}
		if (ImGui.IsKeyPressed(ImGuiKey.Menu) || (ImGui.IsKeyDown(ImGuiKey.ModShift) && ImGui.IsKeyPressed(ImGuiKey.F10)))
		{
			Column column2 = activeColumns[column];
			contextMenu = new ContextMenu
			{
				explorer = this,
				column = column2,
				entity = entity,
				rowIndex = row,
				columnIndex = column
			};
			ImGui.OpenPopup("ContextMenu");
		}
		if (entity.Id == 0)
		{
			return;
		}
		if (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyPressed(ImGuiKey.D))
		{
			ExecuteCellCommand(CellCommand.Duplicate, entity, column, row);
			return;
		}
		if (ImGui.IsKeyPressed(ImGuiKey.Delete))
		{
			ExecuteCellCommand(CellCommand.Delete, entity, column, row);
			return;
		}
		if (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyPressed(ImGuiKey.X))
		{
			ExecuteCellCommand(CellCommand.Cut, entity, column, row);
		}
		if (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyPressed(ImGuiKey.C))
		{
			ExecuteCellCommand(CellCommand.Copy, entity, column, row);
		}
	}

	internal void ExecuteCellCommand(CellCommand cellCommand, Entity entity, int column, int row)
	{
		try
		{
			ExecuteCellCommandInternal(cellCommand, entity, column, row);
		}
		catch (Exception ex)
		{
			if (ex is TargetInvocationException ex2)
			{
				ex = ex2.InnerException ?? ex;
			}
			errorPopup.OnError($"{cellCommand} - failed", ex);
		}
	}

	private void ExecuteCellCommandInternal(CellCommand cellCommand, Entity entity, int column, int row)
	{
		switch (cellCommand)
		{
		case CellCommand.Duplicate:
			if (!entity.IsNull)
			{
				context.Edit();
				Entity entity2 = (selectedEntity = entity.Store.CreateEntity());
				entities.Insert(row, entity2);
				setKeyboardFocus = true;
				entity.CopyEntity(entity2);
			}
			break;
		case CellCommand.InsertEmpty:
			if (column == 0)
			{
				Entity newEntity = activeQuery.Store.CreateEntity();
				InsertEntity(newEntity, row);
			}
			break;
		case CellCommand.Insert:
			if (column == 0)
			{
				Entity entity3 = activeQuery.Store.CreateEntity();
				InsertEntity(entity3, row);
				{
					foreach (Column activeColumn in activeColumns)
					{
						if (activeColumn is FieldColumn fieldColumn)
						{
							EntityUtils.AddEntityComponent(entity3, fieldColumn.memberDrawer.member.componentType);
						}
						if (activeColumn is TagColumn tagColumn)
						{
							entity3.AddTags(new Tags(tagColumn.tagType));
						}
					}
					break;
				}
			}
			if (column > 0 && !entity.IsNull)
			{
				context.Edit();
				activeColumns[column].Insert(entity);
			}
			break;
		case CellCommand.Delete:
			if (column == 0)
			{
				DeleteEntity(entity, row);
			}
			else if (column > 0 && row >= 0 && !entity.IsNull)
			{
				context.Edit();
				activeColumns[column].Remove(entity);
			}
			break;
		case CellCommand.Cut:
			Cut(entity, column, row);
			break;
		case CellCommand.Copy:
			Copy(entity, column);
			break;
		case CellCommand.Paste:
			Paste(entity, column, row);
			break;
		}
	}

	private void Cut(Entity entity, int column, int row)
	{
		if (column == 0 && !entity.IsNull)
		{
			Entity entity2 = GetClipboardEntity();
			clipboardData = entity2;
			entity.CopyEntity(entity2);
			DeleteEntity(entity, row);
		}
		else
		{
			if (column <= 0 || entity.IsNull)
			{
				return;
			}
			Column column2 = activeColumns[column];
			if (column2 is FieldColumn fieldColumn)
			{
				ComponentType componentType = fieldColumn.ComponentType;
				if (entity.Archetype.ComponentTypes.HasAll(new ComponentTypes(componentType)))
				{
					context.Edit();
					IComponent entityComponent = EntityUtils.GetEntityComponent(entity, componentType);
					clipboardData = CreateDeepComponentCopy(componentType, entityComponent);
					column2.Remove(entity);
				}
			}
			else if (column2 is TagColumn tagColumn && entity.Tags.HasAll(new Tags(tagColumn.tagType)))
			{
				context.Edit();
				clipboardData = TypeUtils.CreateInstance(tagColumn.tagType.Type);
				column2.Remove(entity);
			}
		}
	}

	private void Copy(Entity entity, int column)
	{
		if (column == 0 && !entity.IsNull)
		{
			Entity entity2 = GetClipboardEntity();
			clipboardData = entity2;
			entity.CopyEntity(entity2);
		}
		else
		{
			if (column <= 0 || entity.IsNull)
			{
				return;
			}
			Column column2 = activeColumns[column];
			if (column2 is FieldColumn fieldColumn)
			{
				ComponentType componentType = fieldColumn.ComponentType;
				if (entity.Archetype.ComponentTypes.HasAll(new ComponentTypes(componentType)))
				{
					IComponent entityComponent = EntityUtils.GetEntityComponent(entity, componentType);
					clipboardData = CreateDeepComponentCopy(componentType, entityComponent);
				}
			}
			else if (column2 is TagColumn tagColumn && entity.Tags.HasAll(new Tags(tagColumn.tagType)))
			{
				clipboardData = TypeUtils.CreateInstance(tagColumn.tagType.Type);
			}
		}
	}

	private void Paste(Entity entity, int column, int row)
	{
		Type type = clipboardData.GetType();
		if (column == 0)
		{
			if (clipboardData is Entity entity2)
			{
				Entity entity3 = activeQuery.Store.CreateEntity();
				entity2.CopyEntity(entity3);
				InsertEntity(entity3, row);
			}
			else
			{
				SetError("Cannot paste " + type.Name + " as Entity");
			}
		}
		else
		{
			if (column <= 0 || entity.IsNull)
			{
				return;
			}
			Column column2 = activeColumns[column];
			if (column2 is FieldColumn fieldColumn)
			{
				ComponentType componentType = fieldColumn.ComponentType;
				if (componentType.Type == type)
				{
					context.Edit();
					IComponent value = CreateDeepComponentCopy(componentType, clipboardData);
					EntityUtils.AddEntityComponentValue(entity, componentType, value);
				}
				else
				{
					SetError("Cannot paste " + type.Name + " to " + componentType.Name);
				}
			}
			else if (column2 is TagColumn tagColumn)
			{
				if (tagColumn.tagType.Type == type)
				{
					entity.AddTags(new Tags(tagColumn.tagType));
				}
				else
				{
					SetError("Cannot paste " + type.Name + " to " + tagColumn.tagType.Type.Name);
				}
			}
		}
	}

	private void DeleteEntity(Entity entity, int row)
	{
		if (!entity.IsNull)
		{
			context.Edit();
			entity.DeleteEntity();
		}
		if (row + 1 < entities.Count)
		{
			selectedEntity = entities[row + 1];
		}
		else if (row - 1 >= 0)
		{
			selectedEntity = entities[row - 1];
		}
		else
		{
			selectedEntity = default(Entity);
		}
		setKeyboardFocus = true;
		entities.RemoveAt(row);
	}

	private void InsertEntity(Entity newEntity, int row)
	{
		context.Edit();
		selectedEntity = newEntity;
		if (entities.Count == 0)
		{
			entities.Add(newEntity);
			setKeyboardFocus = true;
		}
		else
		{
			entities.Insert(row, newEntity);
			setKeyboardFocus = true;
		}
	}

	private Entity GetClipboardEntity()
	{
		if (clipboardEntity.IsNull)
		{
			EntityStore entityStore = new EntityStore();
			clipboardEntity = entityStore.CreateEntity(1);
		}
		return clipboardEntity;
	}

	private static IComponent CreateDeepComponentCopy(ComponentType componentType, object component)
	{
		return (IComponent)typeof(EntityUtils).GetMethod("CopyComponent").MakeGenericMethod(componentType.Type).Invoke(null, new object[3]
		{
			component,
			default(Entity),
			default(Entity)
		});
	}
}
