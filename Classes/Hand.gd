class_name Hand

# Wall reference
var _wall

# Hand tiles
var tiles = [] # Hidden tiles
var open_tiles = [] # Called tiles (for open hands)

func _init(wall: Wall):
	_wall = wall

func draw_starting_hand():
	tiles = _wall.take(13)