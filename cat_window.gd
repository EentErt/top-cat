extends Window

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var usable = DisplayServer.screen_get_usable_rect()
	position = usable.position
	size = usable.size
	$Cat.position.x = size.x / 2
	$Cat.position.y = size.y - 93


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	$Cat.position.x += $Cat.speed * $Cat.direction * delta
	


func _on_texture_button_pressed() -> void:
	$TransparentWindow.SetClickThrough(false)
	$Cat.squish = true
	var hand = load("res://assets/hand-large.png")
	Input.set_custom_mouse_cursor(hand)
	
	
func _input(event):
	if event.is_action_pressed("passthrough"):
		$TransparentWindow.SetClickThrough(true)
		$Cat.squish = false
		Input.set_custom_mouse_cursor(null)
