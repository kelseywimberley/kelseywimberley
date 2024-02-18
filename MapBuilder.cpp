// Snippet of my work from MapBuilder.cpp
void Map::SpawnWalls() {
	bse::BetterSellingEngine::GetInstance()->RegisterLayer("Wall Layer");
	// Speeds up collision detection if walls do not check collisions with each other
	bse::BetterSellingEngine::GetInstance()->SetCollisionInteraction("Wall Layer", "Wall Layer", false);

	auto& layer = tileIDs[TileData::Layer::Wall];
	int x = 0, z = 0;
	// y is used to keep track of the length of the wall
	std::list<glm::ivec3> verticalWalls, horizontalWalls, ddWalls, udWalls;
	bool newStart = true;
	auto gItr = verticalWalls.begin();

	// JSON file is organized by x rows and z columns
	for (auto& row : layer) {
		for (int cell : row) {
			if (cell == TileData::NoTile) {
				x++;
				continue;
			}

			TileData::WallType cellType = static_cast<TileData::WallType>(cell);
			switch (cellType)
			{
			case TileData::Horizontal:
				gItr = horizontalWalls.begin();
				newStart = true;
				// Look for the current wall with an increased length of one
				// For horizontal walls, z is the same and x increases
				while (gItr != horizontalWalls.end()) {
					if (gItr->z == z && x == (gItr->y + 1)) {
						gItr->y = x;
						newStart = false;
						break;
					}
					gItr++;
				}
				// If the current wall is not found, make a new one
				if (newStart) {
					horizontalWalls.push_back(glm::ivec3(x, x, z));
				}

				break;

			case TileData::Vertical:
				gItr = verticalWalls.begin();
				newStart = true;
				// Look for the current wall with an increased length of one
				// For vertical walls, x is the same and z increases
				while (gItr != verticalWalls.end()) {
					if (gItr->x == x && z == (gItr->y + 1)) {
						gItr->y = z;
						newStart = false;
						break;
					}
					gItr++;
				}
				// If the current wall is not found, make a new one
				if (newStart) {
					verticalWalls.push_back(glm::ivec3(x, z, z));
				}
				
				break;

			case TileData::DownDiagonal:
				gItr = ddWalls.begin();
				newStart = true;
				// Look for the current wall with an increased length of one
				// For horizontal walls, both x and z increase
				while (gItr != ddWalls.end()) {
					if (gItr->y == (x - 1) && gItr->z == (z - abs(gItr->x - gItr->y) - 1)) {
						gItr->y = x;
						newStart = false;
						break;
					}
					gItr++;
				}
				// If the current wall is not found, make a new one
				if (newStart) {
					ddWalls.push_back(glm::ivec3(x, x, z));
				}

				break;

			case TileData::UpDiagonal:
				gItr = udWalls.begin();
				newStart = true;
				// Look for the current wall with an increased length of one
				// For horizontal walls, x decreases and z increases
				while (gItr != udWalls.end()) {
					if (gItr->y == (x + 1) && gItr->z == (z - abs(gItr->x - gItr->y) - 1)) {
						gItr->y = x;
						newStart = false;
						break;
					}
					gItr++;
				}
				// If the current wall is not found, make a new one
				if (newStart) {
					udWalls.push_back(glm::ivec3(x, x, z));
				}

				break;

			case TileData::Intersection:
				// Repeat the earlier checks, but for all wall types

				// Horizontal
				gItr = horizontalWalls.begin();
				newStart = true;
				while (gItr != horizontalWalls.end()) {
					if (gItr->z == z && x == (gItr->y + 1)) {
						gItr->y = x;
						newStart = false;
						break;
					}
					gItr++;
				}
				if (newStart) {
					horizontalWalls.push_back(glm::ivec3(x, x, z));
				}

				// Vertical
				gItr = verticalWalls.begin();
				newStart = true;
				while (gItr != verticalWalls.end()) {
					if (gItr->x == x && z == (gItr->y + 1)) {
						gItr->y = z;
						newStart = false;
						break;
					}
					gItr++;
				}
				if (newStart) {
					verticalWalls.push_back(glm::ivec3(x, z, z));
				}

				// Down Diagonal
				gItr = ddWalls.begin();
				newStart = true;
				while (gItr != ddWalls.end()) {
					if (gItr->y == (x - 1) && gItr->z == (z - abs(gItr->x - gItr->y) - 1)) {
						gItr->y = x;
						newStart = false;
						break;
					}
					gItr++;
				}
				if (newStart) {
					ddWalls.push_back(glm::ivec3(x, x, z));
				}

				// Up Diagonal
				gItr = udWalls.begin();
				newStart = true;
				while (gItr != udWalls.end()) {
					if (gItr->y == (x + 1) && gItr->z == (z - abs(gItr->x - gItr->y) - 1)) {
						gItr->y = x;
						newStart = false;
						break;
					}
					gItr++;
				}
				if (newStart) {
					udWalls.push_back(glm::ivec3(x, x, z));
				}

				break;

			default:
				break;
			}
			x++;
		}
		z++;
		x = 0;
	}

	// Create walls for all the lists of walls
	// Direction 0 = Horizontal
	// Direction 1 = Vertical
	// Direction 2 = Down Diagonal
	// Direction 3 = Up Diagonal

	int i = 0;
	int size = horizontalWalls.size();
	for (i = 0; i < size; i++) {
		gItr = horizontalWalls.begin();
		if (gItr->x != gItr->y) {
			CreateWall(glm::ivec3(gItr->x, gItr->y, gItr->z), 0);
		}
		horizontalWalls.erase(gItr);
	}
	size = verticalWalls.size();
	for (i = 0; i < size; i++) {
		gItr = verticalWalls.begin();
		if (gItr->y != gItr->z) {
			CreateWall(glm::ivec3(gItr->x, gItr->y, gItr->z), 1);
		}
		verticalWalls.erase(gItr);
	}
	size = ddWalls.size();
	for (i = 0; i < size; i++) {
		gItr = ddWalls.begin();
		if (gItr->x != gItr->y && (gItr->x + 1) != gItr->y) {
			CreateWall(glm::ivec3(gItr->x, gItr->y, gItr->z), 2);
		}
		ddWalls.erase(gItr);
	}
	size = udWalls.size();
	for (i = 0; i < size; i++) {
		gItr = udWalls.begin();
		if (gItr->x != gItr->y && (gItr->x - 1) != gItr->y) {
			CreateWall(glm::ivec3(gItr->x, gItr->y, gItr->z), 3);
		}
		udWalls.erase(gItr);
	}
}

bse::Ptr<bse::GameObject> Map::CreateWall(glm::ivec3 position, int direction) {
	if (direction < 0 || direction > 3) {
		return nullptr;
	}

	bse::Ptr<bse::GameObject> wall = pGof->MakeEmptyGameObject("Wall");
	wall->SetLayer("Wall Layer");

	bse::component::Transform* wallTransform = wall->GetComponent<bse::component::Transform>();
	// Default scales
	float scale = 0.01;
	float zScale = 0.8;
	float width = 1;
	
	// Horizontal
	if (direction == 0) {
		width = position.y - position.x + scale;
		wallTransform->SetPosition(glm::vec3((position.y + position.x) * 0.5f, 2.25, position.z));
		wallTransform->SetScale(glm::vec3(width, 5, zScale));
	}
	// Vertical
	if (direction == 1) {
		width = position.y - position.z + scale;
		wallTransform->SetPosition(glm::vec3(position.x, 2.25, (position.y + position.z) * 0.5f));
		wallTransform->SetRotation(glm::vec3(0, M_PI / 2, 0));
		wallTransform->SetScale(glm::vec3(width, 5, zScale));
	}
	// Down Diagonal
	if (direction == 2) {
		// Pythagorean theorem with width 1 and depth 1 -> Square root of 2
		width = (position.y - position.x) * glm::sqrt(2.0f);
		float midPoint = abs(position.x - position.y);
		wallTransform->SetPosition(glm::vec3((position.x + position.y) * 0.5f, 2.25,
			(position.z + (abs(position.x - position.y) * 0.5f))));
		wallTransform->SetRotation(glm::vec3(0, -M_PI / 4, 0));
		wallTransform->SetScale(glm::vec3(width, 5, zScale));
	}
	// Up Diagonal
	if (direction == 3) {
		// Pythagorean theorem with width 1 and depth 1 -> Square root of 2
		width = (position.x - position.y) * glm::sqrt(2.0f);
		wallTransform->SetPosition(glm::vec3((position.x + position.y) * 0.5f, 2.25,
			(position.z + (abs(position.x - position.y) * 0.5f))));
		wallTransform->SetRotation(glm::vec3(0, M_PI / 4, 0));
		wallTransform->SetScale(glm::vec3(width, 5, zScale));
	}

	// Looping the texture on the walls
	wall->AddComponent<bse::component::Sprite>();
	bse::component::Sprite* wallSprite = wall->GetComponent<bse::component::Sprite>();

	glm::vec3 firstPoint = glm::vec3(0.5f, 0.5f, 0.0f);
	glm::vec3 secondPoint = glm::vec3(0.5f, -0.5f, 0.0f);
	glm::vec3 thirdPoint = glm::vec3(-0.5f, -0.5f, 0.0f);
	glm::vec3 fourthPoint = glm::vec3(-0.5f, 0.5f, 0.0f);

	glm::vec2 firstTex = glm::vec2(width * 1.0f, 1.0f);
	glm::vec2 secondTex = glm::vec2(width * 1.0f, 0.0f);
	glm::vec2 thirdTex = glm::vec2(0.0f, 0.0f);
	glm::vec2 fourthTex = glm::vec2(0.0f, 1.0f);

	wallSprite->SetQuadGeometry(
		firstPoint, secondPoint, thirdPoint, fourthPoint,
		firstTex, secondTex, thirdTex, fourthTex);

	wallSprite->SetMaterial("./assets/thisIsAWall.png", false, true, true);

	// Set physics body as cube
	wall->AddComponent<bse::component::PhysicsBody>();
	bse::component::PhysicsBody* wallBody = wall->GetComponent<bse::component::PhysicsBody>();
	wallBody->SetOBC();
	wallBody->SetHasGravity(false);

	return wall;
}
