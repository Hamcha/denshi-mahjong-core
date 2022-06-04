extends Node2D

onready var tile_prefab = preload("res://Actors/Tile.tscn")

onready var wall: Wall = Wall.new()

func _ready():
	new_game()

func new_game():
	wall.new_game(4)
	var tile_row = 0
	for player in wall.players:
		var tile_col = 0
		for tile in player:
			var tile_instance = tile_prefab.instance()
			tile_instance.kind = tile[0]
			tile_instance.value = tile[1]
			add_child(tile_instance)
			tile_instance.position = Vector2(10 + tile_col * 40, 20 + tile_row * 60)
			tile_col += 1
		tile_row += 1
