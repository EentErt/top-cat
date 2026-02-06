extends Node2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	$Cat.position.x += $Cat.speed * $Cat.direction * delta
	$Icon.position = DisplayServer.window_get_size() - Vector2i(45, -25)

func update_passthrough_region():
	var size = $Icon.get_size() * $Icon.scale
	var location = $Icon.position
	
	var region = [
		location,
		location + size.x,
		location + size,
		location + size.y
	]
	
	DisplayServer.window_set_mouse_passthrough(region)
	
	
