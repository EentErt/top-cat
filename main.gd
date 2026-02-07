extends Node2D

var usable

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	usable = DisplayServer.screen_get_usable_rect()
	get_window().size = usable.size
	get_window().position = usable.position
	


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
	
