class_name Wall

# Live wall
var tiles = []

# Dead wall parts
var dora_wall = []
var kan_wall = []
var revealed_doras = 0

# Player hands
var players = []

const ERR_LIVE_WALL_EMPTY = "live wall is empty"
const ERR_ALL_KANS_DECLARED = "4 kans have been declared already"

#
# Setting up a new game
#

func new_game(player_count: int):
	# Prepare tiles
	prepare_tiles()

	# Shuffle
	tiles.shuffle()

	# Make dead wall
	kan_wall = take(4)
	dora_wall = take(10)
	reveal_dora()
	
	# Player hands
	for _p in range(player_count):
		players.push_back(take(13))

func prepare_tiles():
	for _num in range(4):
		# Add each bam/pin/crack
		for n in range(1, 10):
			tiles.push_back([Tile.Kind.BAM, n])
			tiles.push_back([Tile.Kind.CRACK, n])
			tiles.push_back([Tile.Kind.PIN, n])
		# Add each wind
		tiles.push_back([Tile.Kind.WIND, "E"])
		tiles.push_back([Tile.Kind.WIND, "N"])
		tiles.push_back([Tile.Kind.WIND, "S"])
		tiles.push_back([Tile.Kind.WIND, "W"])
		# Add each dragon
		tiles.push_back([Tile.Kind.DRAGON, "R"])
		tiles.push_back([Tile.Kind.DRAGON, "G"])
		tiles.push_back([Tile.Kind.DRAGON, "W"])

#
# Drawing from live wall
#

func can_draw():
	return tiles.size() > 0

func draw():
	return tiles.pop_front()

func take(n: int):
	var sliced = tiles.slice(0, n)
	tiles = tiles.slice(n, -1)
	return sliced

#
# Dead wall functions (Kan, Doras)
#

func can_kan():
	return tiles.size() > 0 and kan_wall.size() > 0

func draw_kan():
	# Can only call Kan if there are still tiles to draw
	if tiles.size() < 1:
		return ERR_LIVE_WALL_EMPTY

	# Max 4 kans per game
	if kan_wall.size() < 1:
		return ERR_ALL_KANS_DECLARED

	# Get tile from dead wall
	var tile = kan_wall.pop_front()

	# Pop tile from live wall
	dora_wall.push_back(tiles.pop_back())

	return tile

func reveal_dora():
	if revealed_doras < dora_wall.size() / 2:
		revealed_doras += 1
	return dora_wall[revealed_doras-1]

func doras():
	return dora_wall.slice(0, revealed_doras)

func ura_doras():
	return dora_wall.slice(5, revealed_doras)
