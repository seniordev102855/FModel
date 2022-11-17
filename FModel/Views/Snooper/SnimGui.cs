using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Objects.Core.Misc;
using FModel.Framework;
using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace FModel.Views.Snooper;

public class SnimGui
{
    public readonly ImGuiController Controller;
    private bool _viewportFocus;
    private bool _swapAwareness;

    private readonly Vector4 _accentColor = new (0.125f, 0.42f, 0.831f, 1.0f);
    private readonly Vector4 _alertColor = new (0.831f, 0.573f, 0.125f, 1.0f);
    private readonly Vector4 _errorColor = new (0.761f, 0.169f, 0.169f, 1.0f);

    private const ImGuiCond _firstUse = ImGuiCond.FirstUseEver; // switch to FirstUseEver once the docking branch will not be useful anymore...
    private const uint _dockspaceId = 1337;

    public SnimGui(int width, int height)
    {
        Controller = new ImGuiController(width, height);
        Theme();
    }

    public void Render(Snooper s)
    {
        Controller.SemiBold();
        DrawDockSpace(s.Size);
        DrawNavbar();

        SectionWindow("Material Inspector", s.Renderer, DrawMaterialInspector, false);

        Window("Timeline", () => {});
        Window("World", () => DrawWorld(s), false);
        Window("Sockets", () => DrawSockets(s));

        DrawOuliner(s);
        DrawDetails(s);
        Draw3DViewport(s);

        // ImGui.ShowDemoWindow();

        Controller.Render();
    }

    private void DrawWorld(Snooper s)
    {
        ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
        if (ImGui.CollapsingHeader("Editor"))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 3));
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(0, 1));
            if (ImGui.BeginTable("world_editor", 2))
            {
                Layout("Skybox");ImGui.PushID(1);
                ImGui.Checkbox("", ref s.Renderer.ShowSkybox);
                ImGui.PopID();Layout("Grid");ImGui.PushID(2);
                ImGui.Checkbox("", ref s.Renderer.ShowGrid);
                ImGui.PopID();Layout("Vertex Colors");ImGui.PushID(3);
                ImGui.Combo("vertex_colors", ref s.Renderer.VertexColor,
                    "Default\0Diffuse Only\0Colors\0Normals\0Tangent\0Texture Coordinates\0");
                ImGui.PopID();

                ImGui.EndTable();
            }
            ImGui.PopStyleVar(2);
        }

        ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
        if (ImGui.CollapsingHeader("Camera"))
        {
            s.Camera.ImGuiCamera();
        }

        ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
        if (ImGui.CollapsingHeader("Lights"))
        {

        }
    }

    private void DrawDockSpace(Vector2i size)
    {
        const ImGuiWindowFlags flags =
            ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        ImGui.SetNextWindowPos(new Vector2(0, 0));
        ImGui.SetNextWindowSize(new Vector2(size.X, size.Y));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("Oui oui", flags);
        ImGui.PopStyleVar();
        ImGui.DockSpace(_dockspaceId);
    }

    private void DrawNavbar()
    {
        if (!ImGui.BeginMainMenuBar()) return;

        if (ImGui.BeginMenu("Window"))
        {
            ImGui.MenuItem("Append", "R");
            ImGui.MenuItem("Close", "ESC");
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem("Undo", "CTRL+Z")) {}
            if (ImGui.MenuItem("Redo", "CTRL+Y", false, false)) {}  // Disabled item
            ImGui.Separator();
            if (ImGui.MenuItem("Cut", "CTRL+X")) {}
            if (ImGui.MenuItem("Copy", "CTRL+C")) {}
            if (ImGui.MenuItem("Paste", "CTRL+V")) {}
            ImGui.EndMenu();
        }

        const string text = "Press ESC to Exit...";
        ImGui.SetCursorPosX(ImGui.GetWindowViewport().WorkSize.X - ImGui.CalcTextSize(text).X - 5);
        ImGui.TextColored(new Vector4(0.36f, 0.42f, 0.47f, 1.00f), text); // ImGuiCol.TextDisabled

        ImGui.EndMainMenuBar();
    }

    private void DrawOuliner(Snooper s)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        Window("Outliner", () =>
        {
            if (ImGui.BeginTable("Items", 3, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterV, ImGui.GetContentRegionAvail()))
            {
                ImGui.TableSetupColumn("Instance", ImGuiTableColumnFlags.NoHeaderWidth | ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Channels", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Name");
                ImGui.TableHeadersRow();

                var i = 0;
                foreach ((FGuid guid, Model model) in s.Renderer.Cache.Models)
                {
                    ImGui.PushID(i);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    if (!model.Show)
                    {
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(new Vector4(1, 0, 0, .5f)));
                    }

                    ImGui.Text(model.TransformsCount.ToString("D"));
                    ImGui.TableNextColumn();
                    ImGui.Text(model.NumTexCoords.ToString("D"));
                    ImGui.TableNextColumn();
                    model.IsSelected = s.Renderer.Settings.SelectedModel == guid;
                    if (ImGui.Selectable(model.Name, model.IsSelected, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        s.Renderer.Settings.SelectModel(guid);
                    }
                    if (ImGui.BeginPopupContextItem())
                    {
                        s.Renderer.Settings.SelectModel(guid);
                        if (ImGui.MenuItem("Show", null, model.Show)) model.Show = !model.Show;
                        if (ImGui.MenuItem("Wireframe", null, model.Wireframe)) model.Wireframe = !model.Wireframe;
                        ImGui.Separator();
                        if (ImGui.Selectable("Save"))
                        {

                        }
                        ImGui.BeginDisabled(!model.HasSkeleton);
                        if (ImGui.Selectable("Animate"))
                        {
                            s.Renderer.Settings.AnimateMesh(true);
                            s.WindowShouldClose(true, false);
                        }
                        ImGui.EndDisabled();
                        if (ImGui.Selectable("Delete")) s.Renderer.Cache.Models.Remove(guid);
                        if (ImGui.Selectable("Deselect")) s.Renderer.Settings.SelectModel(Guid.Empty);
                        ImGui.Separator();
                        if (ImGui.Selectable("Copy Name to Clipboard")) ImGui.SetClipboardText(model.Name);
                        ImGui.EndPopup();
                    }
                    ImGui.PopID();
                    i++;
                }

                ImGui.EndTable();
            }
        });
        ImGui.PopStyleVar();
    }

    private void DrawSockets(Snooper s)
    {
        foreach (var model in s.Renderer.Cache.Models.Values)
        {
            if (!model.HasSkeleton || model.IsSelected) return;
            if (ImGui.TreeNode($"{model.Name} [{model.Skeleton.Sockets.Length}]"))
            {
                var i = 0;
                foreach (var socket in model.Skeleton.Sockets)
                {
                    ImGui.PushID(i);
                    ImGui.Text($"{socket.Name} attached to {socket.Bone}");
                    ImGui.Text($"P: {socket.Transform.Matrix.M41} | {socket.Transform.Matrix.M42} | {socket.Transform.Matrix.M43}");
                    // ImGui.Text($"R: {socket.Transform.Rotation}");
                    // ImGui.Text($"S: {socket.Transform.Scale}");
                    if (ImGui.Button("Attach"))
                    {
                        var guid = s.Renderer.Settings.SelectedModel;
                        if (s.Renderer.Cache.Models.TryGetValue(guid, out var selected))
                        {
                            selected.Transforms[selected.SelectedInstance] = socket.Transform;
                            selected.UpdateMatrix(selected.SelectedInstance);
                        }
                    }
                    ImGui.PopID();
                    i++;
                }
                ImGui.TreePop();
            }
        }
    }

    private void DrawDetails(Snooper s)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        MeshWindow("Details", s.Renderer, (icons, model) =>
        {
            ImGui.Text($"Entity: ({model.Type}) {model.Name}");
            ImGui.Text($"Guid: {s.Renderer.Settings.SelectedModel.ToString(EGuidFormats.UniqueObjectGuid)}");
            ImGui.Spacing();
            if (ImGui.Button("Go To"))
            {
                var instancePos = model.Transforms[model.SelectedInstance].Position;
                s.Camera.Position = new Vector3(instancePos.X, instancePos.Y, instancePos.Z);
            }
            ImGui.Spacing();
            if (ImGui.BeginTabBar("tabbar_details", ImGuiTabBarFlags.None))
            {
                if (ImGui.BeginTabItem("Sections") && ImGui.BeginTable("table_sections", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterV, ImGui.GetContentRegionAvail()))
                {
                    ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Material");
                    ImGui.TableHeadersRow();

                    var swap = false;
                    for (var i = 0; i < model.Sections.Length; i++)
                    {
                        var section = model.Sections[i];
                        var material = model.Materials[section.MaterialIndex];

                        ImGui.PushID(i);
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        if (!section.Show)
                        {
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(new Vector4(1, 0, 0, .5f)));
                        }

                        ImGui.Text(section.MaterialIndex.ToString("D"));
                        ImGui.TableNextColumn();
                        if (ImGui.Selectable(material.Name, s.Renderer.Settings.SelectedSection == i, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            s.Renderer.Settings.SelectSection(i);
                        }
                        if (ImGui.BeginPopupContextItem())
                        {
                            s.Renderer.Settings.SelectSection(i);
                            if (ImGui.MenuItem("Show", null, section.Show)) section.Show = !section.Show;
                            if (ImGui.Selectable("Swap"))
                            {
                                if (_swapAwareness)
                                {
                                    s.Renderer.Settings.SwapMaterial(true);
                                    s.WindowShouldClose(true, false);
                                }
                                else swap = true;
                            }
                            ImGui.Separator();
                            if (ImGui.Selectable("Copy Name to Clipboard")) ImGui.SetClipboardText(material.Name);
                            ImGui.EndPopup();
                        }
                        ImGui.PopID();
                    }
                    ImGui.EndTable();

                    var p_open = true;
                    if (swap) ImGui.OpenPopup("Swap?");
                    ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(.5f));
                    if (ImGui.BeginPopupModal("Swap?", ref p_open, ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.TextWrapped("You're about to swap a material.\nThe window will close for you to extract a material!\n\n");
                        ImGui.Separator();

                        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
                        ImGui.Checkbox("Got it! Don't show me again", ref _swapAwareness);
                        ImGui.PopStyleVar();

                        var size = new Vector2(120, 0);
                        if (ImGui.Button("OK", size))
                        {
                            ImGui.CloseCurrentPopup();
                            s.Renderer.Settings.SwapMaterial(true);
                            s.WindowShouldClose(true, false);
                        }

                        ImGui.SetItemDefaultFocus();
                        ImGui.SameLine();

                        if (ImGui.Button("Cancel", size))
                        {
                            ImGui.CloseCurrentPopup();
                        }

                        ImGui.EndPopup();
                    }

                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Transform"))
                {
                    const int width = 100;
                    var speed = s.Camera.Speed / 100f;

                    ImGui.PushID(0); ImGui.BeginDisabled(model.TransformsCount < 2);
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    ImGui.SliderInt("", ref model.SelectedInstance, 0, model.TransformsCount - 1, "Instance %i", ImGuiSliderFlags.AlwaysClamp);
                    ImGui.EndDisabled(); ImGui.PopID();

                    ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
                    if (ImGui.TreeNode("Location"))
                    {
                        ImGui.PushID(1);
                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("X", ref model.Transforms[model.SelectedInstance].Position.X, speed, 0f, 0f, "%.2f m");

                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("Y", ref model.Transforms[model.SelectedInstance].Position.Z, speed, 0f, 0f, "%.2f m");

                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("Z", ref model.Transforms[model.SelectedInstance].Position.Y, speed, 0f, 0f, "%.2f m");

                        ImGui.PopID();
                        ImGui.TreePop();
                    }

                    ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
                    if (ImGui.TreeNode("Rotation"))
                    {
                        ImGui.PushID(2);
                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("X", ref model.Transforms[model.SelectedInstance].Rotation.Roll, .5f, 0f, 0f, "%.1f°");

                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("Y", ref model.Transforms[model.SelectedInstance].Rotation.Pitch, .5f, 0f, 0f, "%.1f°");

                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("Z", ref model.Transforms[model.SelectedInstance].Rotation.Yaw, .5f, 0f, 0f, "%.1f°");

                        ImGui.PopID();
                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Scale"))
                    {
                        ImGui.PushID(3);
                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("X", ref model.Transforms[model.SelectedInstance].Scale.X, speed, 0f, 0f, "%.3f");

                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("Y", ref model.Transforms[model.SelectedInstance].Scale.Z, speed, 0f, 0f, "%.3f");

                        ImGui.SetNextItemWidth(width);
                        ImGui.DragFloat("Z", ref model.Transforms[model.SelectedInstance].Scale.Y, speed, 0f, 0f, "%.3f");

                        ImGui.PopID();
                        ImGui.TreePop();
                    }

                    model.UpdateMatrix(model.SelectedInstance);
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Morph Targets"))
                {
                    if (model.HasMorphTargets)
                    {
                        const float width = 10;
                        var region = ImGui.GetContentRegionAvail();
                        var box = new Vector2(region.X - width, region.Y / 1.5f);

                        if (ImGui.BeginListBox("", box))
                        {
                            for (int i = 0; i < model.Morphs.Length; i++)
                            {
                                ImGui.PushID(i);
                                if (ImGui.Selectable(model.Morphs[i].Name, s.Renderer.Settings.SelectedMorph == i))
                                {
                                    s.Renderer.Settings.SelectMorph(i, model);
                                }
                                ImGui.PopID();
                            }
                            ImGui.EndListBox();

                            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2f, 0f));
                            ImGui.SameLine(); ImGui.PushID(99);
                            ImGui.VSliderFloat("", box with { X = width }, ref model.MorphTime, 0.0f, 1.0f, "", ImGuiSliderFlags.AlwaysClamp);
                            ImGui.PopID(); ImGui.PopStyleVar();
                            ImGui.Spacing();
                            ImGui.Text($"Time: {model.MorphTime:P}%");
                        }
                    }
                    else ImGui.TextColored(_errorColor, "mesh has no morph targets");
                    ImGui.EndTabItem();
                }
            }
        });
        ImGui.PopStyleVar();
    }

    private void DrawMaterialInspector(Dictionary<string, Texture> icons, Model model, Section section)
    {
        var material = model.Materials[section.MaterialIndex];

        ImGui.Spacing();
        ImGui.Image(icons["material"].GetPointer(), new Vector2(24));
        ImGui.SameLine(); ImGui.AlignTextToFramePadding(); ImGui.Text(material.Name);
        ImGui.Spacing();

        ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
        if (ImGui.CollapsingHeader("Parameters"))
        {
            material.ImGuiParameters();
        }

        ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
        if (ImGui.CollapsingHeader("Textures"))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 3));
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(0, 1));
            if (ImGui.BeginTable("material_textures", 2))
            {
                Layout("Channel");ImGui.PushID(1); ImGui.BeginDisabled(model.NumTexCoords < 2);
                ImGui.DragInt("", ref material.SelectedChannel, 1, 0, model.NumTexCoords - 1, "UV %i", ImGuiSliderFlags.AlwaysClamp);
                ImGui.EndDisabled();ImGui.PopID();Layout("Type");ImGui.PushID(2);
                ImGui.Combo("texture_type", ref material.SelectedTexture,
                    "Diffuse\0Normals\0Specular\0Mask\0Emissive\0");
                ImGui.PopID();
                ImGui.EndTable();
            }
            ImGui.PopStyleVar(2);

            ImGui.Image(material.GetSelectedTexture() ?? icons["noimage"].GetPointer(), new Vector2(ImGui.GetContentRegionAvail().X), Vector2.Zero, Vector2.One, Vector4.One, new Vector4(1.0f, 1.0f, 1.0f, 0.25f));
            ImGui.Spacing();
        }

        ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
        if (ImGui.CollapsingHeader("Properties"))
        {
            ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
            if (ImGui.TreeNode("Scalars"))
            {
                material.ImGuiDictionaries("scalars", material.Parameters.Scalars, true);
                ImGui.TreePop();
            }
            ImGui.SetNextItemOpen(true, ImGuiCond.Appearing);
            if (ImGui.TreeNode("Colors"))
            {
                material.ImGuiDictionaries("colors", material.Parameters.Colors, true);
                ImGui.TreePop();
            }
            if (ImGui.TreeNode("Referenced Textures"))
            {
                material.ImGuiDictionaries("textures", material.Parameters.Textures);
                ImGui.TreePop();
            }
        }
    }

    private void Draw3DViewport(Snooper s)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        Window("3D Viewport", () =>
        {
            var largest = ImGui.GetContentRegionAvail();
            largest.X -= ImGui.GetScrollX();
            largest.Y -= ImGui.GetScrollY();

            var size = new Vector2(largest.X, largest.Y);
            s.Camera.AspectRatio = size.X / size.Y;
            ImGui.ImageButton(s.Framebuffer.GetPointer(), size, new Vector2(0, 1), new Vector2(1, 0), 0);

            if (ImGui.IsItemHovered())
            {
                // if left button down while mouse is hover viewport
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && !_viewportFocus)
                {
                    _viewportFocus = true;
                    s.CursorState = CursorState.Grabbed;
                }
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    var guid = s.Renderer.Picking.ReadPixel(ImGui.GetMousePos(), ImGui.GetCursorScreenPos(), size);
                    s.Renderer.Settings.SelectModel(guid);
                    ImGui.SetWindowFocus("Outliner");
                }
            }

            const float lookSensitivity = 0.1f;
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left, lookSensitivity) && _viewportFocus)
            {
                var io = ImGui.GetIO();
                var delta = io.MouseDelta * lookSensitivity;
                s.Camera.ModifyDirection(delta.X, delta.Y);
            }

            // if left button up and mouse was in viewport
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _viewportFocus)
            {
                _viewportFocus = false;
                s.CursorState = CursorState.Normal;
            }

            float framerate = ImGui.GetIO().Framerate;
            ImGui.SetCursorPos(size with { X = 7.5f });
            ImGui.Text($"FPS: {framerate:0} ({1000.0f / framerate:0.##} ms)");
        }, false);
        ImGui.PopStyleVar();
    }

    private void Window(string name, Action content, bool styled = true)
    {
        if (ImGui.Begin(name))
        {
            Controller.PopFont();
            if (styled) PushStyleCompact();
            content();
            if (styled) PopStyleCompact();
            ImGui.End();
        }
    }

    private void MeshWindow(string name, Renderer renderer, Action<Dictionary<string, Texture>, Model> content, bool styled = true)
    {
        Window(name, () =>
        {
            if (renderer.Cache.Models.TryGetValue(renderer.Settings.SelectedModel, out var model)) content(renderer.Cache.Icons, model);
            else NoMeshSelected();
        }, styled);
    }

    private void SectionWindow(string name, Renderer renderer, Action<Dictionary<string, Texture>, Model, Section> content, bool styled = true)
    {
        MeshWindow(name, renderer, (icons, model) =>
        {
            if (renderer.Settings.TryGetSection(model, out var section)) content(icons, model, section);
            else NoSectionSelected();
        }, styled);
    }

    private void PopStyleCompact() => ImGui.PopStyleVar(2);
    private void PushStyleCompact()
    {
        var style = ImGui.GetStyle();
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, style.FramePadding with { Y = style.FramePadding.Y * 0.6f });
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, style.ItemSpacing with { Y = style.ItemSpacing.Y * 0.6f });
    }

    private void NoMeshSelected() => CenteredTextColored(_errorColor, "No Mesh Selected");
    private void NoSectionSelected() => CenteredTextColored(_errorColor, "No Section Selected");
    private void CenteredTextColored(Vector4 color, string text)
    {
        var region = ImGui.GetContentRegionAvail();
        var size = ImGui.CalcTextSize(text);
        ImGui.SetCursorPos(new Vector2(
                ImGui.GetCursorPosX() + (region.X - size.X) / 2,
                ImGui.GetCursorPosY() + (region.Y - size.Y) / 2));
        Controller.Bold();
        ImGui.TextColored(color, text);
        Controller.PopFont();
    }

    public static void Layout(string name, bool tooltip = false)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Spacing();ImGui.SameLine();ImGui.Text(name);
        if (tooltip) TooltipCopy(name);
        ImGui.TableSetColumnIndex(1);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
    }

    public static void TooltipCopy(string name)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(name);
            ImGui.EndTooltip();
        }
        if (ImGui.IsItemClicked()) ImGui.SetClipboardText(name);
    }

    private void DrawSquareTexture(Texture texture, Vector2 size)
    {
        ImGui.Image(texture?.GetPointer() ?? IntPtr.Zero, size, Vector2.Zero, Vector2.One, Vector4.One, new Vector4(1, 1, 1, .5f));
        if (texture == null) return;

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text($"Type: ({texture.Format}) {texture.Type}:{texture.Name}");
            ImGui.Text($"Texture: {texture.Path}");
            ImGui.Text($"Imported: {texture.ImportedWidth}x{texture.ImportedHeight}");
            ImGui.Text($"Mip Used: {texture.Width}x{texture.Height}");
            ImGui.Spacing();
            ImGui.TextDisabled(texture.Label);
            ImGui.EndTooltip();
        }
        if (ImGui.IsItemClicked())
        {
            ImGui.SetClipboardText(Creator.Utils.FixPath(texture.Path));
            texture.Label = "(?) Path Copied to Clipboard";
        }
    }

    private void Theme()
    {
        var style = ImGui.GetStyle();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        io.ConfigWindowsMoveFromTitleBarOnly = true;
        io.ConfigDockingWithShift = true;

        style.WindowPadding = new Vector2(4f);
        style.FramePadding = new Vector2(3f);
        style.CellPadding = new Vector2(3f, 2f);
        style.ItemSpacing = new Vector2(6f, 3f);
        style.ItemInnerSpacing = new Vector2(3f);
        style.TouchExtraPadding = new Vector2(0f);
        style.IndentSpacing = 20f;
        style.ScrollbarSize = 10f;
        style.GrabMinSize = 8f;
        style.WindowBorderSize = 0f;
        style.ChildBorderSize = 0f;
        style.PopupBorderSize = 0f;
        style.FrameBorderSize = 0f;
        style.TabBorderSize = 0f;
        style.WindowRounding = 0f;
        style.ChildRounding = 0f;
        style.FrameRounding = 0f;
        style.PopupRounding = 0f;
        style.ScrollbarRounding = 0f;
        style.GrabRounding = 0f;
        style.LogSliderDeadzone = 0f;
        style.TabRounding = 0f;
        style.WindowTitleAlign = new Vector2(0.5f);
        style.WindowMenuButtonPosition = ImGuiDir.Right;
        style.ColorButtonPosition = ImGuiDir.Right;
        style.ButtonTextAlign = new Vector2(0.5f);
        style.SelectableTextAlign = new Vector2(0f);
        style.DisplaySafeAreaPadding = new Vector2(3f);

        style.Colors[(int) ImGuiCol.Text]                   = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        style.Colors[(int) ImGuiCol.TextDisabled]           = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        style.Colors[(int) ImGuiCol.WindowBg]               = new Vector4(0.11f, 0.11f, 0.12f, 1.00f);
        style.Colors[(int) ImGuiCol.ChildBg]                = new Vector4(0.15f, 0.15f, 0.19f, 1.00f);
        style.Colors[(int) ImGuiCol.PopupBg]                = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
        style.Colors[(int) ImGuiCol.Border]                 = new Vector4(0.25f, 0.26f, 0.33f, 1.00f);
        style.Colors[(int) ImGuiCol.BorderShadow]           = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int) ImGuiCol.FrameBg]                = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        style.Colors[(int) ImGuiCol.FrameBgHovered]         = new Vector4(0.69f, 0.69f, 1.00f, 0.20f);
        style.Colors[(int) ImGuiCol.FrameBgActive]          = new Vector4(0.69f, 0.69f, 1.00f, 0.39f);
        style.Colors[(int) ImGuiCol.TitleBg]                = new Vector4(0.09f, 0.09f, 0.09f, 1.00f);
        style.Colors[(int) ImGuiCol.TitleBgActive]          = new Vector4(0.09f, 0.09f, 0.09f, 1.00f);
        style.Colors[(int) ImGuiCol.TitleBgCollapsed]       = new Vector4(0.05f, 0.05f, 0.05f, 0.51f);
        style.Colors[(int) ImGuiCol.MenuBarBg]              = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        style.Colors[(int) ImGuiCol.ScrollbarBg]            = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
        style.Colors[(int) ImGuiCol.ScrollbarGrab]          = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
        style.Colors[(int) ImGuiCol.ScrollbarGrabHovered]   = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
        style.Colors[(int) ImGuiCol.ScrollbarGrabActive]    = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
        style.Colors[(int) ImGuiCol.CheckMark]              = new Vector4(0.13f, 0.42f, 0.83f, 1.00f);
        style.Colors[(int) ImGuiCol.SliderGrab]             = new Vector4(0.13f, 0.42f, 0.83f, 0.78f);
        style.Colors[(int) ImGuiCol.SliderGrabActive]       = new Vector4(0.13f, 0.42f, 0.83f, 1.00f);
        style.Colors[(int) ImGuiCol.Button]                 = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        style.Colors[(int) ImGuiCol.ButtonHovered]          = new Vector4(0.69f, 0.69f, 1.00f, 0.20f);
        style.Colors[(int) ImGuiCol.ButtonActive]           = new Vector4(0.69f, 0.69f, 1.00f, 0.39f);
        style.Colors[(int) ImGuiCol.Header]                 = new Vector4(0.05f, 0.26f, 0.56f, 1.00f);
        style.Colors[(int) ImGuiCol.HeaderHovered]          = new Vector4(0.05f, 0.26f, 0.56f, 0.39f);
        style.Colors[(int) ImGuiCol.HeaderActive]           = new Vector4(0.04f, 0.23f, 0.52f, 1.00f);
        style.Colors[(int) ImGuiCol.Separator]              = new Vector4(0.43f, 0.43f, 0.50f, 0.50f);
        style.Colors[(int) ImGuiCol.SeparatorHovered]       = new Vector4(0.10f, 0.40f, 0.75f, 0.78f);
        style.Colors[(int) ImGuiCol.SeparatorActive]        = new Vector4(0.10f, 0.40f, 0.75f, 1.00f);
        style.Colors[(int) ImGuiCol.ResizeGrip]             = new Vector4(0.13f, 0.42f, 0.83f, 0.39f);
        style.Colors[(int) ImGuiCol.ResizeGripHovered]      = new Vector4(0.12f, 0.41f, 0.81f, 0.78f);
        style.Colors[(int) ImGuiCol.ResizeGripActive]       = new Vector4(0.12f, 0.41f, 0.81f, 1.00f);
        style.Colors[(int) ImGuiCol.Tab]                    = new Vector4(0.15f, 0.15f, 0.19f, 1.00f);
        style.Colors[(int) ImGuiCol.TabHovered]             = new Vector4(0.35f, 0.35f, 0.41f, 0.80f);
        style.Colors[(int) ImGuiCol.TabActive]              = new Vector4(0.23f, 0.24f, 0.29f, 1.00f);
        style.Colors[(int) ImGuiCol.TabUnfocused]           = new Vector4(0.15f, 0.15f, 0.15f, 1.00f);
        style.Colors[(int) ImGuiCol.TabUnfocusedActive]     = new Vector4(0.15f, 0.15f, 0.15f, 1.00f);
        style.Colors[(int) ImGuiCol.DockingPreview]         = new Vector4(0.26f, 0.59f, 0.98f, 0.70f);
        style.Colors[(int) ImGuiCol.DockingEmptyBg]         = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
        style.Colors[(int) ImGuiCol.PlotLines]              = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
        style.Colors[(int) ImGuiCol.PlotLinesHovered]       = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
        style.Colors[(int) ImGuiCol.PlotHistogram]          = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        style.Colors[(int) ImGuiCol.PlotHistogramHovered]   = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
        style.Colors[(int) ImGuiCol.TableHeaderBg]          = new Vector4(0.09f, 0.09f, 0.09f, 1.00f);
        style.Colors[(int) ImGuiCol.TableBorderStrong]      = new Vector4(0.69f, 0.69f, 1.00f, 0.20f);
        style.Colors[(int) ImGuiCol.TableBorderLight]       = new Vector4(0.69f, 0.69f, 1.00f, 0.20f);
        style.Colors[(int) ImGuiCol.TableRowBg]             = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        style.Colors[(int) ImGuiCol.TableRowBgAlt]          = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        style.Colors[(int) ImGuiCol.TextSelectedBg]         = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
        style.Colors[(int) ImGuiCol.DragDropTarget]         = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        style.Colors[(int) ImGuiCol.NavHighlight]           = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        style.Colors[(int) ImGuiCol.NavWindowingHighlight]  = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        style.Colors[(int) ImGuiCol.NavWindowingDimBg]      = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        style.Colors[(int) ImGuiCol.ModalWindowDimBg]       = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
    }
}
