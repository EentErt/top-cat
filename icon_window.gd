extends Window

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var usable = DisplayServer.screen_get_usable_rect()
	position = usable.position + usable.size - Constants.ICON_SIZE
	pass

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
