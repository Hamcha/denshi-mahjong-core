extends Node2D

class_name Tile

enum Kind {
	BAM,
	PIN,
	CRACK,
	DRAGON,
	WIND
}

export(String) var value setget set_value
export(Kind) var kind setget set_kind

func set_value(new_value):
	value = new_value
	$Value.text = str(new_value)

func set_kind(new_kind):
	kind = new_kind
	$Kind.text = str(Kind.keys()[new_kind]).substr(0, 3)

func _ready():
	set_value(value)
	set_kind(kind)
