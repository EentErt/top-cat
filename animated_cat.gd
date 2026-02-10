extends AnimatedSprite2D

var flip = false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func walk():
	play("walk")
	if !flip_h:
		$Head.offset = Vector2(0, -1)
	else:
		$Head.offset = Vector2(15, -1)
	
func sit():
	flip_h = false
	play("sit")
	$Head.offset = Vector2(7, -1)
	
func loaf():
	flip_h = false
	play("loaf")
	$Head.offset = Vector2(7, 13)
	
func nap():
	flip_h = false
	play("nap")
	$Head.offset = Vector2(7, 13)
	$Head.frame = 6
	
func squish():
	flip_h = false
	play("squish")
	frame = 17
	$Head.offset = Vector2(7, -1)
	$Head.look(get_global_mouse_position())
	var mouse = get_local_mouse_position()
	mouse.y += 15
	var center_point = Vector2(13.0, 30.0)
	if abs(mouse.x - center_point.x) < 15:
		frame = int(center_point.y - mouse.y)
		$Head.offset = Vector2(7, 16 - frame)
