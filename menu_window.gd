extends Window


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	hide()
	var usable = DisplayServer.screen_get_usable_rect()
	position = usable.position + usable.size - Constants.ICON_SIZE - Constants.MENU_SIZE
	size = Constants.MENU_SIZE


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_texture_button_pressed() -> void:
	pass # Replace with function body.


func _on_open_menu_button_pressed() -> void:
	if visible:
		hide()
	else:
		show()
