#include "EngineIncludes.h"
#include "UIEditor.h"
#include "../Managers/EventManager/Event.h"

void UIEditor::init() {
#ifdef NDEBUG
	return;
#endif

	anchorPoints.resize(9);
	anchorPoints[0] = glm::vec2(-1, -1);
	anchorPoints[1] = glm::vec2(0, -1);
	anchorPoints[2] = glm::vec2(1, -1);
	anchorPoints[3] = glm::vec2(-1, 0);
	anchorPoints[4] = glm::vec2(0, 0);
	anchorPoints[5] = glm::vec2(1, 0);
	anchorPoints[6] = glm::vec2(-1, 1);
	anchorPoints[7] = glm::vec2(0, 1);
	anchorPoints[8] = glm::vec2(1, 1);

	selectedID = -1;

	// All UI elements must have a Transform component and a GUI component
	gde::Signature rendererSignature;
	rendererSignature.set(iGO.getComponentType<gde::component::Transform>());
	rendererSignature.set(iGO.getComponentType<gde::component::GUI>());
	gde::GOIDItr itr;
	gde::GOIDItr end;
	iGO.getRelevantGOIDs(rendererSignature, itr, end);

	// Find all GameObject IDs in the currently selected JSON file
	gde::GOID id = -1;
	for (; itr != end; itr++) {
		id = *itr;
		uiGameObjects.insert(std::make_pair(id, iGO.getName(id)));
	}

	sourceFileNames = iResource.getSourceFiles();
	
	updateEditObjects();
}

void UIEditor::updateEditObjects() {
	auto& goidList = iResource.getGOIDsFromFile(sourceFileNames[currentSourceFile]);

	uiGameObjects.clear();
	for (auto& id : iResource.getGOIDsFromFile(sourceFileNames[currentSourceFile])) {
		uiGameObjects.insert(std::make_pair(id, iGO.getName(id)));
	}
}

void UIEditor::update(float dt) {
#ifdef NDEBUG
	return;
#endif

	if (inEditMode) {
		if (iInput.keyDown(GLFW_MOUSE_BUTTON_LEFT) == gde::Input::Enter) {
			std::map<gde::GOID, std::string>::iterator itrBegin = uiGameObjects.begin();
			std::map<gde::GOID, std::string>::iterator itrEnd = uiGameObjects.end();
			selectedID = -1;

			for (; itrBegin != itrEnd; itrBegin++) {
				// When edit mode is activated, click on a UI element to edit it
				if (iInput.isMouseOverUI(itrBegin->first, true)) {
					selectedID = itrBegin->first;
					selectedName = itrBegin->second;
					break;
				}
			}
		}
	}
}

// If the source file contains one of the key words, then the elements are defaulted to being hidden on startup
bool UIEditor::defaultHidden() {
	std::vector<std::string> keyWords{
		"pauseMenu",
		"inGameMenu",
		"collection"
	};

	for (std::string& str : keyWords) {
		if (sourceFileNames[currentSourceFile].find(str) != std::string::npos) return true;
	}

	return false;
}

// The editor functionality is in an ImGui window
void UIEditor::customImguiWindow(bool& windowsActive) {
	if (sourceFileNames.size() == 0) return;

	ImGui::Begin("UI Editor", &windowsActive, ImGuiWindowFlags_NoFocusOnAppearing);

	if (ImGui::Checkbox("Editor On", &inEditMode)) {
		if (inEditMode) {
			iInput.setUIEditing(true);
			iDebug.getLogger().log(gde::Logger::Info, "UI Edit Mode On");
		}
		else {
			selectedID = -1;
			iInput.setUIEditing(false);
			iDebug.getLogger().log(gde::Logger::Info, "UI Edit Mode Off");
		}
	}

	// Change the JSON file currently being edited
	const char* combo_preview_value = sourceFileNames[currentSourceFile].c_str();
	if (ImGui::BeginCombo("Source File", combo_preview_value, 0))
	{
		for (int n = 0; n < sourceFileNames.size(); n++)
		{
			const bool is_selected = (currentSourceFile == n);
			if (ImGui::Selectable(sourceFileNames[n].c_str(), is_selected))
				currentSourceFile = n;

			// Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
			if (is_selected)
				ImGui::SetItemDefaultFocus();
		}
		ImGui::EndCombo();

		updateEditObjects();
		selectedID = -1;
	}

	// Save all GameObjects' settings to the JSON file
	if (ImGui::Button("Save UI Settings")) {
		iDebug.getLogger().log(gde::Logger::Info, "UI Saved");

		if (defaultHidden()) {
			for (const auto& [goid, _] : uiGameObjects) {
				iGUI->setRendering(goid, false);
			}
		}
		
		iResource.exportSourceFileChanges(sourceFileNames[currentSourceFile]);

		if (defaultHidden()) {
			for (const auto& [goid, _] : uiGameObjects) {
				iGUI->setRendering(goid, true);
			}
		}
	}

	// Only add editing options for position, scale, and anchor points
	// when a GameObject is selected
	if (inEditMode) {
		if (selectedID != -1) {
			for (int i = 0; i < 9; i++) {
				if (iGUI->getAnchor(selectedID) == anchorPoints[i]) {
					currentAnchorPoint = i;
				}
			}

			ImGui::SetNextItemOpen(true, ImGuiCond_Once);
			const char* title = selectedName.c_str();

			if (ImGui::TreeNode(title)) {
				glm::vec2 pos = iTransform->getPosition(selectedID);
				ImGui::DragFloat2("Position", &(pos[0]), 0.025f);
				iTransform->setPosition(selectedID, glm::vec3(pos, 0));

				// The developer has the option to lock or unlock the scale
				glm::vec2 scale = iGUI->getScale(selectedID);
				ImGui::DragFloat2("Scale", &(scale[0]), 0.005f); ImGui::SameLine();
				ImGui::Checkbox("Lock Scale", &scaleAspectLocked);
				scale = glm::max(scale, glm::vec2(0.01));
				if (scaleAspectLocked) {
					float aspectRatio = iGUI->getScale(selectedID).x / iGUI->getScale(selectedID).y;
					if (iGUI->getScale(selectedID).x != scale.x) {
						scale.y = scale.x / aspectRatio;
					}
					else if (iGUI->getScale(selectedID).y != scale.y) {
						scale.x = scale.y * aspectRatio;
					}
				}
				iGUI->setScale(selectedID, scale);

				ImGui::RadioButton("(-1,  1)", &currentAnchorPoint, 0); ImGui::SameLine();
				ImGui::RadioButton("(0,  1)", &currentAnchorPoint, 1); ImGui::SameLine();
				ImGui::RadioButton("(1,  1)", &currentAnchorPoint, 2);
				ImGui::RadioButton("(-1,  0)", &currentAnchorPoint, 3); ImGui::SameLine();
				ImGui::RadioButton("(0,  0)", &currentAnchorPoint, 4); ImGui::SameLine();
				ImGui::RadioButton("(1,  0)", &currentAnchorPoint, 5);
				ImGui::RadioButton("(-1, -1)", &currentAnchorPoint, 6); ImGui::SameLine();
				ImGui::RadioButton("(0, -1)", &currentAnchorPoint, 7); ImGui::SameLine();
				ImGui::RadioButton("(1, -1)", &currentAnchorPoint, 8);

				iGUI->setAnchor(selectedID, anchorPoints[currentAnchorPoint]);

				if (ImGui::Button("Delete Game Object")) {
					iGO.destroyGameObject(selectedID);
					iResource.rmvGOFromSourceFile(sourceFileNames[currentSourceFile], selectedID);
					updateEditObjects();
					selectedID = -1;
				}

				ImGui::TreePop();
			}
		}
	}

	ImGui::End();
}
